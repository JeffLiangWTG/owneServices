using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business.Customs
{
	[ExcludeEntryChargeTypeListFromTest]
	public class ZZEntryChargeTypeList : EntryChargeTypeList
	{
		public ZZEntryChargeTypeList(ZString countryCode)
		{
			this.countryCode = Argument.NotNullOrEmpty(countryCode, nameof(countryCode));
			LoadData();
		}

		void LoadData()
		{
			var factory = new BusinessObjectFactory();
			var rateTypeQuery = FormattableString.Invariant($@"
				SELECT {RefCusRateTypeSchema.Constants.ZZR_RateType},
				{RefCusRateTypeSchema.Constants.ZZR_Description}
				FROM {RefCusRateTypeSchema.Constants.TableName} 
				{GetQueryIncludeParentDataGrouping(factory, RefCusRateTypeSchema.ZZR_ZZZ_NKDataGrouping, countryCode).GetAsWhereClause(true)}
				AND {RefCusRateTypeSchema.Constants.ZZR_IsPayable} = 1 
				");
			var rateCollection = new DynamicBusinessObjectCollection(factory);
			rateCollection.Load(rateTypeQuery);
			rateCollection.ForEach(x => AddIfNotExists((ZString)x[RefCusRateTypeSchema.Constants.ZZR_RateType], (ZString)x[RefCusRateTypeSchema.Constants.ZZR_Description], true, ZString.Empty));

			var taxOrFeeTypeQuery = System.FormattableString.Invariant($@"
			SELECT DISTINCT {RefCusTaxOrFeeTypeSchema.Constants.ZX0_TaxOrFeeType} AS {RefCusRateTypeSchema.Constants.ZZR_RateType},
				{RefCusTaxOrFeeTypeSchema.Constants.ZX0_Description} AS {RefCusRateTypeSchema.Constants.ZZR_Description}
				FROM {RefCusTaxOrFeeTypeSchema.Constants.TableName}
				INNER JOIN {RefCusTaxOrFeeSchema.Constants.TableName}
				ON {RefCusTaxOrFeeTypeSchema.Constants.ZX0_TaxOrFeeType} = {RefCusTaxOrFeeSchema.Constants.ZZF_ZX0_NKTaxOrFeeType}
				{GetQueryIncludeParentDataGrouping(factory, RefCusTaxOrFeeSchema.ZZF_ZZZ_NKDataGrouping, countryCode).GetAsWhereClause(true)} 
				AND {RefCusTaxOrFeeSchema.Constants.ZZF_Value} > 0
			");
			var taxOrFeeCollection = new DynamicBusinessObjectCollection(factory);
			taxOrFeeCollection.Load(taxOrFeeTypeQuery);
			if (taxOrFeeCollection.Any())
			{
				taxOrFeeCollection.ForEach(x => AddIfNotExists((ZString)x[RefCusRateTypeSchema.Constants.ZZR_RateType], (ZString)x[RefCusRateTypeSchema.Constants.ZZR_Description], true, ZString.Empty));
			}

			var isSelfManagedTariffCountry = ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().IsSelfManagedTariffCountry(countryCode);
			if (isSelfManagedTariffCountry)
			{
				var cusRateTypeList = ObjectFactory.Get<Integration.Customs.ICusRefRateTypePairListProvider>().GetCusRefRateTypeList();
				foreach (CodeDescriptionPair cusRateType in cusRateTypeList)
				{
					AddIfNotExists(cusRateType.Code, cusRateType.Description, true, ZString.Empty);
				}
			}

			AddIfNotExists(Core.Constants.Customs.CusEntryFeeTypes.VAT, Res.GetString("2451B35E-C898-4BE7-8D37-FD355C0F0158", "{0} (Value Added Tax)", ZArchitecture.Environment.Country.GetConsumptionTaxDescription(countryCode)), true, ZString.Empty);
		}

		static ZString GetParentDataGrouping(BusinessObjectFactory factory, ZString countryCode)
		{
			var dynamicBOs = new DynamicBusinessObjectCollection(factory);
			dynamicBOs.Load(
				FormattableString.Invariant(
					$@"SELECT TOP 1 {RefDataGroupingSchema.Constants.ZZZ_DataGrouping} FROM {RefDataGroupingSchema.Constants.TableName}
					WHERE {RefDataGroupingSchema.Constants.PK} IN 
					(
						SELECT {RefDataGroupingSchema.Constants.ZZZ_ZZZ_Grouping} 
						FROM {RefDataGroupingSchema.Constants.TableName} 
						WHERE {RefDataGroupingSchema.Constants.ZZZ_ZZZ_Grouping} IS NOT NULL 
						AND {RefDataGroupingSchema.Constants.ZZZ_DataGrouping} = @CountryCode 
					)"),
				new ZSqlParameter[] { ZSqlParameter.New("@CountryCode", countryCode, RefDataGroupingSchema.ZZZ_DataGrouping) });
			return dynamicBOs.Select(x => new ZString(x[RefDataGroupingSchema.Constants.ZZZ_DataGrouping])).FirstOrDefault();
		}

		static ZQuery GetQueryIncludeParentDataGrouping(BusinessObjectFactory factory, SchemaStringColumn foreignNkColumn, ZString countryCode)
		{
			var countryCodes = new List<ZString>
			{
				countryCode
			};

			var parentDataGrouping = factory.GetCachedValue("ParentDataGrouping_" + countryCode, () => GetParentDataGrouping(factory, countryCode));
			if (!parentDataGrouping.IsEmpty)
			{
				countryCodes.Add(parentDataGrouping);
			}
			return new ZQuery(foreignNkColumn, countryCodes);
		}

		readonly ZString countryCode;

		public override string DutyCode => Core.Constants.Customs.CusEntryFeeTypes.DutyAmount;

		public override string TaxCode => "";
	}
}
