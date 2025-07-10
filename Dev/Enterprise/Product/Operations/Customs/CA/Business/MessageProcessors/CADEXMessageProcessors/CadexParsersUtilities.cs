using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Cadex.RecordParsers
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	class CadexParsersUtilities
	{
		#region CACClassHeader

		internal static CACClassHeader LoadOrCreateClassHeader(BusinessObjectFactory factory, ZDateTime effectiveDate, ZString classificationNumber)
		{
			var query = new ZQuery(CACClassHeaderSchema.ZA_ClassificationNumber, classificationNumber);
			query.AddToFilter(CACClassHeaderSchema.ZA_EffectiveDate, SQLComparisonOperator.EqualToDatePartOnly, effectiveDate);
			var result = factory.LoadTop1<CACClassHeader>(query);
			if (result == null)
			{
				var latestClassHeader = LoadLatestClassHeader(factory, effectiveDate, classificationNumber);
				if (latestClassHeader is CACClassHeader && latestClassHeader.ZA_ExpiryDate >= effectiveDate)
				{
					latestClassHeader.ZA_ExpiryDate = effectiveDate.AddDays(-1);
				}

				result = factory.New<CACClassHeader>();
				using (result.GetValidationSuspender())
				{
					result.ZA_ClassificationNumber = classificationNumber;
					result.ZA_EffectiveDate = effectiveDate;
				}
			}
			return result;
		}

		internal static CACClassHeader LoadLatestClassHeader(BusinessObjectFactory factory, ZDateTime effectiveDate, ZString classificationNumber)
		{
			var query = new ZQuery(CACClassHeaderSchema.ZA_ClassificationNumber, classificationNumber);
			query.AddToFilter(CACClassHeaderSchema.ZA_EffectiveDate, SQLComparisonOperator.LessThan, effectiveDate);
			query.OrderBy = CACClassHeaderSchema.Constants.ZA_EffectiveDate + OrderByClause.Descending;
			var result = factory.LoadTop1<CACClassHeader>(query);
			return result;
		}

		#endregion

		#region CACRateHeader

		internal static CACRateHeader LoadOrCreateRateHeader(ICadexProcessor processor, CACClassHeader classHeader, ZDateTime effectiveDate, string type)
		{
			var result = LoadRateHeader(classHeader, effectiveDate, type);
			if (result == null)
			{
				result = CreateRateHeader(classHeader, effectiveDate, type);
			}
			else
			{
				var query = DeleteRatesFromLocalCacheReturningFilterToDeleteFromDb(result);
				if (result.IsInDatabase)
				{
					processor.AddDeleteFetchHint(CACRate.Schema.TableName, query);
				}
			}
			return result;
		}

		internal static CACRateHeader LoadOrCreateRateHeader(CACClassHeader classHeader, ZDateTime effectiveDate, string type)
		{
			var result = LoadRateHeader(classHeader, effectiveDate, type);
			if (result == null)
			{
				var latestRateHeader = LoadLatestRateHeader(classHeader, effectiveDate, type);
				if (latestRateHeader is CACRateHeader && latestRateHeader.ZB_ExpiryDate >= effectiveDate)
				{
					latestRateHeader.ZB_ExpiryDate = effectiveDate.AddDays(-1);
				}
				result = CreateRateHeader(classHeader, effectiveDate, type);
			}
			return result;
		}

		static CACRateHeader LoadLatestRateHeader(CACClassHeader classHeader, ZDateTime effectiveDate, string type)
		{
			var query = new ZQuery(CACRateHeaderSchema.ZB_EffectiveDate, SQLComparisonOperator.LessThan, effectiveDate);
			query.AddToFilter(CACRateHeaderSchema.ZB_ZA_ClassHeader, classHeader.PK);
			query.AddToFilter(CACRateHeaderSchema.ZB_RateType, type);
			query.OrderBy = CACRateHeaderSchema.Constants.ZB_EffectiveDate + OrderByClause.Descending;
			return classHeader.Factory.LoadTop1<CACRateHeader>(query);
		}

		static CACRateHeader LoadRateHeader(CACClassHeader master, ZDateTime effectiveDate, string type)
		{
			var query = new ZQuery(CACRateHeaderSchema.ZB_EffectiveDate, SQLComparisonOperator.EqualToDatePartOnly, effectiveDate);
			query.AddToFilter(CACRateHeaderSchema.ZB_ZA_ClassHeader, master.PK);
			query.AddToFilter(CACRateHeaderSchema.ZB_RateType, type);
			return master.Factory.LoadTop1<CACRateHeader>(query);
		}

		static CACRateHeader CreateRateHeader(CACClassHeader master, ZDateTime effectiveDate, string type)
		{
			var result = master.Factory.New<CACRateHeader>();
			using (result.GetValidationSuspender())
			{
				result.ZB_ZA_ClassHeader = master.PK;
				result.ZB_RateType = type;
				result.ZB_EffectiveDate = effectiveDate;
			}
			return result;
		}

		static ZQuery DeleteRatesFromLocalCacheReturningFilterToDeleteFromDb(BusinessObject master)
		{
			var query = new ZQuery(CACRateSchema.ZC_ParentID, master.PK);

			var tables = ((INeedDataSet)master.Factory).Data.Tables;
			var rateTable = tables[CACRate.Schema.TableName];
			if (rateTable != null)
			{
				var ratesToDeletePKs = from DataRow row in rateTable.Rows
									   where (Guid)row[AutoCACRate.Schema.ZC_ParentID] == master.PK.ToGuid()
									   select (Guid)row[AutoCACRate.Schema.PK];

				if (ratesToDeletePKs.Any())
				{
					var rateLinesTable = tables[CACRateLine.Schema.TableName];
					if (rateLinesTable != null)
					{
						var rateLinesToDeletePKs = from pk in ratesToDeletePKs
												   from DataRow row in rateLinesTable.Rows
												   where (Guid)row[AutoCACRateLine.Schema.ZR_ZC_Rate] == pk
												   select (Guid)row[AutoCACRateLine.Schema.PK];

						if (rateLinesToDeletePKs.Any())
						{
							DeleteObjectsFromPKs(master, rateLinesToDeletePKs);
						}
					}
					DeleteObjectsFromPKs(master, ratesToDeletePKs);
					query.AddToFilter(CACRateSchema.PK, SQLComparisonOperator.NotEqual, ratesToDeletePKs);
				}
			}
			return query;
		}

		static void DeleteObjectsFromPKs(BusinessObject master, IEnumerable<Guid> rateLinesToDeletePKs)
		{
			(from pk in rateLinesToDeletePKs
			 from obj in master.Factory.GetBizOsForPK(pk)
			 select obj).ToList().ForEach(obj => obj.Delete());
		}

		#endregion

		#region CACTariffHeader

		internal static CACTariffHeader LoadOrCreateTariffHeader(ICadexProcessor processor, ZDateTime effectiveDate, string tariffCode)
		{
			var result = LoadTariffHeader(processor.Factory, effectiveDate, tariffCode);
			if (result == null)
			{
				result = CreateTariffHeader(processor.Factory, effectiveDate, tariffCode);
			}
			else
			{
				var query = DeleteRatesFromLocalCacheReturningFilterToDeleteFromDb(result);
				if (result.IsInDatabase)
				{
					processor.AddDeleteFetchHint(CACRate.Schema.TableName, query);
				}
			}
			return result;
		}

		static CACTariffHeader LoadTariffHeader(BusinessObjectFactory factory, ZDateTime effectiveDate, ZString tariffCode)
		{
			var query = new ZQuery(CACTariffHeaderSchema.ZF_TariffCode, tariffCode);
			query.AddToFilter(CACTariffHeaderSchema.ZF_AuthEffectiveDate, SQLComparisonOperator.EqualToDatePartOnly, effectiveDate);
			return factory.LoadTop1<CACTariffHeader>(query);
		}

		static CACTariffHeader CreateTariffHeader(BusinessObjectFactory factory, ZDateTime effectiveDate, ZString tariffCode)
		{
			var result = factory.New<CACTariffHeader>();
			using (result.GetValidationSuspender())
			{
				result.ZF_TariffCode = tariffCode;
				result.ZF_AuthEffectiveDate = effectiveDate;
			}
			return result;
		}

		#endregion

		#region CACTaxRefNumHeader

		internal static CACTaxRefNumHeader LoadOrCreateTaxRefNumHeader(ICadexProcessor processor, CACClassHeader master, ZDateTime effectiveDate)
		{
			var result = LoadTaxRefNumHeader(master, effectiveDate);
			if (result == null)
			{
				result = CreateTaxRefNumHeader(master, effectiveDate);
			}
			else
			{
				DeleteRefNumbers(processor, result);
			}
			return result;
		}

		internal static void DeleteRefNumbers(ICadexProcessor processor, CACTaxRefNumHeader master)
		{
			var query = DeleteRefNumbersFromLocalCacheReturningFilterToDeleteFromDb(master);
			if (master.IsInDatabase)
			{
				processor.AddDeleteFetchHint(CACTaxRefNumber.Schema.TableName, query);
			}
		}

		internal static CACTaxRefNumHeader LoadTaxRefNumHeader(CACClassHeader master, ZDateTime effectiveDate)
		{
			var query = new ZQuery(CACTaxRefNumHeaderSchema.ZD_EffectiveDate, SQLComparisonOperator.EqualToDatePartOnly, effectiveDate);
			query.AddToFilter(CACTaxRefNumHeaderSchema.ZD_ZA_ClassNumber, master.PK);
			return master.Factory.LoadTop1<CACTaxRefNumHeader>(query);
		}

		internal static CACTaxRefNumHeader CreateTaxRefNumHeader(CACClassHeader master, ZDateTime effectiveDate)
		{
			var result = master.Factory.New<CACTaxRefNumHeader>();
			using (result.GetValidationSuspender())
			{
				result.ZD_ZA_ClassNumber = master.PK;
				result.ZD_EffectiveDate = effectiveDate;
				result.ZD_ExpiryDate = master.ZA_ExpiryDate;
				result.ZD_Inactive = master.ZA_InactiveInd;
			}
			return result;
		}

		static ZQuery DeleteRefNumbersFromLocalCacheReturningFilterToDeleteFromDb(CACTaxRefNumHeader master)
		{
			var query = new ZQuery(CACTaxRefNumberSchema.ZE_ZD_TaxRefNumHeader, master.PK);
			var table = ((INeedDataSet)master.Factory).Data.Tables[CACTaxRefNumber.Schema.TableName];
			if (table != null)
			{
				var numbersToDelete = from DataRow row in table.Rows
									  where (Guid)row[AutoCACTaxRefNumber.Schema.ZE_ZD_TaxRefNumHeader] == master.PK.ToGuid()
									  from obj in master.Factory.GetBizOsForPK((Guid)row[AutoCACTaxRefNumber.Schema.PK])
									  select obj;

				if (numbersToDelete.Any())
				{
					numbersToDelete.ToList().ForEach(number => number.Delete());
					query.AddToFilter(CACTaxRefNumberSchema.PK, SQLComparisonOperator.NotEqual, numbersToDelete.Select(n => n.PK));
				}
			}
			return query;
		}

		#endregion

		#region CACTaxRate

		internal static CACTaxRate LoadOrCreateTaxRate(BusinessObjectFactory factory, ZString refNumber, ZString taxType, ZDateTime effectiveDate)
		{
			var query = new ZQuery(CACTaxRateSchema.ZH_TaxRefNumber, refNumber);
			query.AddToFilter(CACTaxRateSchema.ZH_EffectiveDate, SQLComparisonOperator.EqualToDatePartOnly, effectiveDate);
			CACTaxRate.AddTaxTypeFilter(query, taxType);

			var result = factory.LoadTop1<CACTaxRate>(query);
			if (result == null)
			{
				result = factory.New<CACTaxRate>();
				using (result.GetValidationSuspender())
				{
					result.ZH_TaxType = taxType;
					result.ZH_TaxRefNumber = refNumber;
					result.ZH_EffectiveDate = effectiveDate;
				}
			}
			return result;
		}

		#endregion

		#region CACRate

		internal static CACRate LoadOrCreateRate(BusinessObjectFactory factory, CACRateHeader rateHeader, ZString preferenceCode)
		{
			var query = new ZQuery(CACRateSchema.ZC_ParentID, rateHeader.PK);
			if (!preferenceCode.IsEmpty)
			{
				query.AddToFilter(CACRateSchema.ZC_TreatmentCode, preferenceCode);
			}

			var result = factory.LoadTop1<CACRate>(query);
			if (result == null)
			{
				result = factory.New<CACRate>();
				using (result.GetValidationSuspender())
				{
					result.ZC_ParentID = rateHeader.PK;
					result.ZC_ParentTableCode = CACRateHeaderSchema.Constants.Prefix;
					if (!preferenceCode.IsEmpty)
					{
						result.ZC_TreatmentCode = preferenceCode;
					}
				}
			}
			return result;
		}

		#endregion
	}
}
