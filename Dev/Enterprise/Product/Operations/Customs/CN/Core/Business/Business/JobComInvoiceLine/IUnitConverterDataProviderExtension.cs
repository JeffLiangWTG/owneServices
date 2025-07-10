using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Business
{
	public static class IUnitConverterDataProviderExtension
	{
		public static ZString GetBestMatchingCustomsUnit(this IUnitConverterDataProvider provider, ZString commercialUnit)
		{
			Argument.NotNull(provider, nameof(provider));
			Argument.NotNull(commercialUnit, nameof(commercialUnit));

			var result = ZString.Empty;

			if (!commercialUnit.IsEmpty)
			{
				result = GetBestMatchingForPhysicalUnit(provider, commercialUnit);

				if (result.IsEmpty)
				{
					result = GetBestMatchingCustomsUnitFromProduct(provider, commercialUnit);
				}
				if (result.IsEmpty)
				{
					result = GetBestMatchingCustomsUnitFromRefPacks(provider, commercialUnit);
				}
			}

			return result;
		}

		static ZString GetBestMatchingForPhysicalUnit(IUnitConverterDataProvider provider, ZString commercialUnit)
		{
			var result = ZString.Empty;

			var physicalUnitMappings = provider.Factory.GetCachedValue("PhysicalUnitMappings", () => new Dictionary<ZString, ZString>());
			if (physicalUnitMappings.ContainsKey(commercialUnit))
			{
				result = physicalUnitMappings[commercialUnit];
			}
			else
			{
				result = GetDefaultCustomsPack(commercialUnit);
				if (!result.IsEmpty)
				{
					var customsUnit = GetBestMatchingSystemCustomsPack(provider.Factory, provider.CountryCode, commercialUnit);
					if (!customsUnit.IsEmpty)
					{
						result = customsUnit;
					}
					physicalUnitMappings.Add(commercialUnit, result);
				}
			}

			return result;
		}

		static ZString GetBestMatchingCustomsUnitFromConverters(IEnumerable<IUnitConverter> unitConverters, BusinessObjectFactory factory, ZString commercialUnit)
		{
			var allCustomsUQs = factory.GetCachedValue<RefCusPackListProvider>().GetCIPCustomsPackList(factory, Core.Constants.CountryCodes.China);

			var converters = unitConverters
				.Where(unit => unit.ParentUnit == commercialUnit && allCustomsUQs.ContainsCode(unit.ChildUnit)).Select(unit => new
				{
					Unit = unit.ChildUnit,
					unit.ConversionFactor
				})

				.Union(unitConverters.Where(unit => unit.ChildUnit == commercialUnit && allCustomsUQs.ContainsCode(unit.ParentUnit)).Select(unit => new
				{
					Unit = unit.ParentUnit,
					ConversionFactor = (ZDecimal)(1 / unit.ConversionFactor)
				}))

				.OrderByDescending(unit => unit.ConversionFactor);

			return converters.FirstOrDefault()?.Unit ?? ZString.Empty;
		}

		static ZString GetBestMatchingCustomsUnitFromProduct(IUnitConverterDataProvider provider, ZString commercialUnit)
		{
			var result = ZString.Empty;
			if (provider.ProductHasSpecificUnitConversions)
			{
				result = GetBestMatchingCustomsUnitFromConverters(provider.GetUnitConversionFactorsFromProductUnits(), provider.Factory, commercialUnit);
			}
			return result;
		}

		#region from dbo.RefPacks

		static ZString GetBestMatchingCustomsUnitFromRefPacks(IUnitConverterDataProvider provider, ZString commercialUnit)
		{
			var supplierPK = provider.SupplierFK;
			var countryCode = provider.CountryCode;
			var factory = provider.Factory;

			return factory.GetCachedValue("CNRefPacksUnit_" + "_" + supplierPK + "_" + countryCode + "_" + commercialUnit,
				() =>
				{
					var allPacks = factory.Load<CusRefPacks>(GetCusRefPacksQuery(countryCode, supplierPK, commercialUnit));
					var filterdPacks = CusRefPacksHelper.FilterRefPacks(allPacks, RPTypeList.Codes.CommercialInvoice);
					var orderedPacks = OrderByType(filterdPacks);

					return GetBestMatchingCustomsUnitFromConverters(orderedPacks, factory, commercialUnit);
				}
			);
		}

		static ZQuery GetCusRefPacksQuery(ZString countryCode, ZGuid supplierPK, ZString commercialUnit)
		{
			ZQuery result;
			if (supplierPK.IsValid)
			{
				result = new ZQuery(RefPacksSchema.RP_OH_Supplier, supplierPK);
				result.AddToFilter(GetNonSupplierRefPacksFilter(countryCode, supplierPK), JoinCondition.Or);
			}
			else
			{
				result = new ZQuery(RefPacksSchema.RP_OH_Supplier, DBNull.Value);
			}

			result.AddToFilter(RefPacksSchema.RP_CommercialPack, commercialUnit);
			result.AddToFilter(CusRefPacksHelper.GetRefPacksQuery(countryCode, RPTypeList.Codes.CommercialInvoice, ZString.Empty), JoinCondition.And);

			return result;
		}

		static ZQuery GetNonSupplierRefPacksFilter(ZString countryCode, ZGuid supplierPK)
		{
			var result = new ZDBOnlyQuery(typeof(CusRefPacks));

			var parameterCollection = new ZSqlParameterCollection();
			parameterCollection.Add("@Country", countryCode, RefPacksSchema.RP_CustomsCountry);
			parameterCollection.Add("@Supplier", supplierPK, RefPacksSchema.RP_OH_Supplier);

			var subQuery = new ZDBOnlySubQuery(typeof(CusRefPacks), RefPacksSchema.PK);
			subQuery.AddFilterAndZSQLParameterCollection(GenericRefPacksFilterScript, parameterCollection);
			result.AddSubQuery(subQuery, JoinCondition.And);

			return result;
		}

		static CusRefPacks[] OrderByType(List<CusRefPacks> filterRefPacks)
		{
			var result = filterRefPacks.Where(x => x.RP_Type == RPTypeList.Codes.CommercialInvoice).ToList();
			result.AddRange(filterRefPacks.Where(x => x.RP_Type == RPTypeList.Codes.AllAreas));
			return result.ToArray();
		}

		const string GenericRefPacksFilterScript = @"
	RP_CustomsCountry = @Country
	AND
	(
		RP_OH_Supplier is null 
		AND NOT EXISTS 
			(
				SELECT NULL FROM dbo.RefPacks Generic
				WHERE 
					Generic.RP_OH_Supplier = @Supplier
					AND Generic.RP_CustomsCountry = @Country
					AND Generic.RP_CustomsPack = RefPacks.RP_CustomsPack
					AND Generic.RP_CommercialPack = RefPacks.RP_CommercialPack
			)
	)";

		static ZString GetBestMatchingSystemCustomsPack(BusinessObjectFactory factory, ZString countryCode, ZString commercialPack)
		{
			var query = new ZQuery(RefPacksSchema.RP_CustomsCountry, countryCode);
			query.AddToFilter(JoinCondition.And, RefPacksSchema.RP_Type, RPTypeList.Codes.AllAreas);
			query.AddToFilter(JoinCondition.And, RefPacksSchema.RP_OH_Supplier, DBNull.Value);
			query.AddToFilter(JoinCondition.And, RefPacksSchema.RP_CommercialPack, commercialPack);

			var systemRefPacks = factory.Load<CusRefPacks>(query).OrderBy(x => x.ConversionFactor < 1 ? 1 / x.ConversionFactor : x.ConversionFactor / 1);

			return systemRefPacks.FirstOrDefault()?.RP_CustomsPack ?? ZString.Empty;
		}

		static ZString GetDefaultCustomsPack(ZString commercialUnit)
		{
			var result = ZString.Empty;

			if (Core.Constants.Weight.ContainsCode(commercialUnit))
			{
				result = CustomsUnitOfMeasurementListHelper.Codes.Kilograms;
			}
			else if (Core.Constants.Length.ContainsCode(commercialUnit))
			{
				result = CustomsUnitOfMeasurementListHelper.Codes.Metres;
			}
			else if (Core.Constants.Area.ContainsCode(commercialUnit))
			{
				result = CustomsUnitOfMeasurementListHelper.Codes.SquareMetres;
			}
			else if (Core.Constants.Volume.ContainsCode(commercialUnit))
			{
				result = CustomsUnitOfMeasurementListHelper.Codes.CubicMetres;
			}
			return result;
		}

		#endregion
	}
}
