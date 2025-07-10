using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public interface ITaxRecordLoader
	{
		AccTaxTransaction[] LoadSPRAPTaxRecords(ITaxRecordParent taxParent, bool getNotionalRecordsOnly = true, bool useLocalCacheOnly = false);
		AccTaxTransaction[] LoadSPRAPTaxRecordsLinkedToMatchTransaction(BusinessObjectFactory factory, ZGuid matchTransactionPK);
		AccTaxTransaction[] LoadMatchingBasisTaxRecords(ITaxRecordParent taxParent);
		bool HasRealisedSPRAPTaxRecordsInDB(ITaxRecordParent taxParent);
		AccTaxTransaction[] LoadAllReportableTaxRecords(ITaxRecordParent taxParent);
		AccTaxTransaction[] LoadNonSPRAPTaxRecords(ITaxRecordParent taxParent);
		(ZDate TaxExpenseDate, ZDecimal TaxExpenseAmount)[] GetTaxExpenses(BusinessObjectFactory factory, ZGuid linePK);
		AccTaxTransaction[] LoadAllTaxRecords(ITaxRecordParent taxParent);
		AccTaxRecordTransactionLinePivot[] LoadTaxRecordPivots(bool useLocalCacheOnly, params AccTaxTransaction[] taxRecords);
		List<(AccTaxTransaction accTaxTransaction, AccTaxRecordTransactionLinePivot accTaxRecordTransactionLinePivot)> LoadTaxRecordsAndPivotsLinkedToTaxableLines(BusinessObjectFactory factory, params ZGuid[] linePks);
		IReadOnlyCollection<IGLMovementDetails> LoadGLMovementDetails(BusinessObjectFactory factory, ZGuid taxParentPK);
		IReadOnlyCollection<IGLMovementDetails> LoadGLMovementDetailsWithGeneralLedgerData(BusinessObjectFactory factory, ZGuid taxParentPK);
	}

	class TaxRecordLoader : ITaxRecordLoader
	{
		AccTaxTransaction[] ITaxRecordLoader.LoadAllTaxRecords(ITaxRecordParent taxParent) => taxParent.Factory.Load<AccTaxTransaction>(new ZQuery(AccTaxTransactionSchema.ATT_AH, taxParent.PK) { FetchOnlyFromLocalCache = true });

		AccTaxRecordTransactionLinePivot[] ITaxRecordLoader.LoadTaxRecordPivots(bool useLocalCacheOnly, params AccTaxTransaction[] taxRecords)
		{
			var query = new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxRecords.Select(x => x.PK)) { FetchOnlyFromLocalCache = useLocalCacheOnly };
			return taxRecords.FirstOrDefault()?.Factory.Load<AccTaxRecordTransactionLinePivot>(query) ?? Array.Empty<AccTaxRecordTransactionLinePivot>();
		}

		AccTaxTransaction[] ITaxRecordLoader.LoadNonSPRAPTaxRecords(ITaxRecordParent taxParent)
		{
			var query = taxParent.Ledger == TaxConfigurationLedgers.AccountsPayable.Code ?
								GetActiveTaxRecordQuery(taxParent, TaxSuperTypeList.StandardPaymentRetention.Code, true) :
								GetActiveTaxRecordQuery(taxParent);
			return taxParent.Factory.Load<AccTaxTransaction>(query);
		}

		bool ITaxRecordLoader.HasRealisedSPRAPTaxRecordsInDB(ITaxRecordParent taxParent)
		{
			return taxParent.Factory.ExistsInDatabase(AccTaxTransaction.Schema.TableName, GetActiveSPRAPTaxRecordQuery(taxParent).AddToFilter(AccTaxTransactionSchema.ATT_RealisationDate, SQLComparisonOperator.NotEqual, null));
		}

		AccTaxTransaction[] ITaxRecordLoader.LoadSPRAPTaxRecords(ITaxRecordParent taxParent, bool getNotionalRecordsOnly, bool useLocalCacheOnly)
		{
			var additionalFilter = new ZQuery { FetchOnlyFromLocalCache = useLocalCacheOnly };
			if (getNotionalRecordsOnly)
			{
				additionalFilter.AddToFilter(AccTaxTransactionSchema.ATT_RealisationDate, null);
			}
			return LoadActiveSPRAPTaxRecords(taxParent.Factory, taxParent, additionalFilter);
		}

		AccTaxTransaction[] ITaxRecordLoader.LoadSPRAPTaxRecordsLinkedToMatchTransaction(BusinessObjectFactory factory, ZGuid matchTransactionPK)
		{
			return LoadActiveSPRAPTaxRecords(factory, null, new ZQuery(AccTaxTransactionSchema.ATT_AH_MatchTransaction, matchTransactionPK));
		}

		static AccTaxTransaction[] LoadActiveSPRAPTaxRecords(BusinessObjectFactory factory, ITaxRecordParent taxParent = null, ZQuery additionalFiler = null)
		{
			var query = GetActiveSPRAPTaxRecordQuery(taxParent).AddToFilter(additionalFiler);

			return factory.Load<AccTaxTransaction>(query);
		}

		AccTaxTransaction[] ITaxRecordLoader.LoadMatchingBasisTaxRecords(ITaxRecordParent taxParent)
		{
			var query = new ZQuery(AccTaxTransactionSchema.ATT_AH, taxParent.PK);
			query.AddToFilter(AccTaxTransactionSchema.ATT_Basis, TaxBasisList.Matching.Code);
			query.AddToFilter(AccTaxTransactionSchema.ATT_RealisationDate, null);

			return taxParent.Factory.Load<AccTaxTransaction>(query);
		}

		AccTaxTransaction[] ITaxRecordLoader.LoadAllReportableTaxRecords(ITaxRecordParent taxParent)
		{
			var query = new ZQuery();

			if (taxParent != null)
			{
				query.AddToFilter(AccTaxTransactionSchema.ATT_AH, taxParent.PK);
			}

			var allTaxRecords = taxParent.Factory.Load<AccTaxTransaction>(query).ToList();

			var cancelledNotionalSPRTaxRecords = allTaxRecords.Where(x => x.ATT_TaxSuperType == TaxSuperTypeList.StandardPaymentRetention.Code && x.ATT_RealisationDate.IsEmpty && x.ATT_IsCancelled);
			return allTaxRecords.Except(cancelledNotionalSPRTaxRecords).ToArray();
		}

		static ZQuery GetActiveSPRAPTaxRecordQuery(ITaxRecordParent taxParent)
		{
			return GetActiveTaxRecordQuery(taxParent, TaxSuperTypeList.StandardPaymentRetention.Code).AddToFilter(AccTaxTransactionSchema.ATT_IsCancelled, false).AddToFilter(AccTaxTransactionSchema.ATT_Ledger, TaxConfigurationLedgers.AccountsPayable.Code);
		}

		static ZQuery GetActiveTaxRecordQuery(ITaxRecordParent taxParent, string taxSuperType = "", bool excludeSuperType = false)
		{
			var query = new ZQuery();
			if (taxParent != null)
			{
				query.AddToFilter(AccTaxTransactionSchema.ATT_AH, taxParent.PK);
				if (!taxParent.IsPosted)
				{
					query.FetchOnlyFromLocalCache = true;
				}
			}

			if (!string.IsNullOrEmpty(taxSuperType))
			{
				if (excludeSuperType)
				{
					query.AddToFilter(AccTaxTransactionSchema.ATT_TaxSuperType, SQLComparisonOperator.NotEqual, taxSuperType);
				}
				else
				{
					query.AddToFilter(AccTaxTransactionSchema.ATT_TaxSuperType, taxSuperType);
				}
			}

			return query;
		}

		(ZDate TaxExpenseDate, ZDecimal TaxExpenseAmount)[] ITaxRecordLoader.GetTaxExpenses(BusinessObjectFactory factory, ZGuid linePK)
		{
			var query = new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_AL_TransactionLine, linePK);
			query.AddToFilter(AccTaxRecordTransactionLinePivotSchema.ATP_IsTaxExpense, true);
			var pivots = factory.Load<AccTaxRecordTransactionLinePivot>(query);

			query = new ZQuery(AccTaxTransactionSchema.PK, pivots.Select(p => p.ATP_ATT));
			var realisationDateBytaxRecordPK = factory.Load<AccTaxTransaction>(query).ToDictionary(x => x.PK, x => x.ATT_RealisationDate);

			return pivots.GroupBy(pivot => realisationDateBytaxRecordPK[pivot.ATP_ATT]).Select(group => (TaxExpenseDate: group.Key, TaxExpenseAmount: (ZDecimal)group.Sum(x => x.ATP_LocalTaxAmount))).ToArray();
		}

		List<(AccTaxTransaction accTaxTransaction, AccTaxRecordTransactionLinePivot accTaxRecordTransactionLinePivot)> ITaxRecordLoader.LoadTaxRecordsAndPivotsLinkedToTaxableLines(BusinessObjectFactory factory, params ZGuid[] linePks)
		{
			List<(AccTaxTransaction accTaxTransaction, AccTaxRecordTransactionLinePivot accTaxRecordTransactionLinePivot)> taxRecordList = new List<(AccTaxTransaction accTaxTransaction, AccTaxRecordTransactionLinePivot accTaxRecordTransactionLinePivot)>();

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccTaxTransaction));

			ZDBOnlySubQuery transactionLinePivot = new ZDBOnlySubQuery(typeof(AccTaxRecordTransactionLinePivot), AccTaxRecordTransactionLinePivotSchema.ATP_ATT);
			transactionLinePivot.AddToFilter(AccTaxRecordTransactionLinePivotSchema.ATP_AL_TransactionLine, linePks);

			query.AddSubQuery(transactionLinePivot, JoinCondition.And);
			query.AddToFilter(AccTaxTransactionSchema.ATT_IsCancelled, false);

			ZDBOnlyQuery pivotQuery = new ZDBOnlyQuery(typeof(AccTaxRecordTransactionLinePivot));
			pivotQuery.AddToFilter(AccTaxRecordTransactionLinePivotSchema.ATP_AL_TransactionLine, linePks);

			var accTaxTransactionArray = factory.Load<AccTaxTransaction>(query);

			if (!accTaxTransactionArray.Any())
			{
				return taxRecordList;
			}

			var accTaxRecordTransactionLinePivotArray = factory.Load<AccTaxRecordTransactionLinePivot>(pivotQuery);

			foreach (var accTaxTransaction in accTaxTransactionArray)
			{
				var accTaxRecordTransactionLinePivot = accTaxRecordTransactionLinePivotArray.First(item => item.ATP_ATT == accTaxTransaction.PK);
				taxRecordList.Add((accTaxTransaction, accTaxRecordTransactionLinePivot));
			}

			return taxRecordList;
		}

		IReadOnlyCollection<IGLMovementDetails> ITaxRecordLoader.LoadGLMovementDetails(BusinessObjectFactory factory, ZGuid taxParentPK)
		{
			var query = $@"
SELECT
	{GLMovementDetails.Schema.TaxConfiguration},
	{GLMovementDetails.Schema.GLAccount} = CASE Multiplier
					WHEN 1 THEN Debit.AG_AccountNum
					WHEN -1 THEN Credit.AG_AccountNum
				END,
	[{GLMovementDetails.Schema.GLAccountDesc}] =	CASE Multiplier
					WHEN 1 THEN Debit.AG_Description
					WHEN -1 THEN Credit.AG_Description
				END,
	{GLMovementDetails.Schema.PostPeriod},
	{GLMovementDetails.Schema.PostDate},
	{GLMovementDetails.Schema.Amount} = ATM_Amount * Multiplier,
	{GLMovementDetails.Schema.Basis},
	{GLMovementDetails.Schema.BranchCode},
	{GLMovementDetails.Schema.DepartmentCode},
	{GLMovementDetails.Schema.OSAmount} = ABS(ATT_OSTaxAmount) * Multiplier,
	{GLMovementDetails.Schema.CurrencyCode} = ATT_RX_NKOSTaxCurrency
FROM
	dbo.AccTaxGLMovement
	INNER JOIN dbo.AccTaxTransaction ON ATM_ATT_TaxTransaction = ATT_PK
	LEFT JOIN dbo.AccGLHeader Debit on ATM_AG_DebitAccount = Debit.AG_PK
	LEFT JOIN dbo.AccGLHeader Credit on ATM_AG_CreditAccount = Credit.AG_PK
	LEFT JOIN dbo.GlbBranch on ATT_GB = GB_PK
	LEFT JOIN dbo.GlbDepartment on ATT_GE_Department = GE_PK
	LEFT JOIN dbo.AccTaxConfiguration on ATT_ETC = ETC_PK
	CROSS JOIN (SELECT 1 AS Multiplier UNION ALL SELECT -1) Multipliers
WHERE
	ATT_AH = @TransactionHeader
ORDER BY
	{GLMovementDetails.Schema.PostPeriod}, {GLMovementDetails.Schema.PostDate}, {GLMovementDetails.Schema.Basis}, {GLMovementDetails.Schema.TaxConfiguration}, {GLMovementDetails.Schema.GLAccount}";

			return LoadGLMovementDetailsFromQuery(factory, taxParentPK, query);
		}

		public IReadOnlyCollection<IGLMovementDetails> LoadGLMovementDetailsWithGeneralLedgerData(BusinessObjectFactory factory, ZGuid taxParentPK)
		{
			var query = $@"
SELECT
	{GLMovementDetails.Schema.TaxConfiguration},
	{GLMovementDetails.Schema.GLAccount} = AG_AccountNum,
	[{GLMovementDetails.Schema.GLAccountDesc}] = AG_Description,
	{GLMovementDetails.Schema.PostPeriod} = GLD_PostPeriod,
	{GLMovementDetails.Schema.PostDate} = GLD_PostDate,
	{GLMovementDetails.Schema.Amount} = CASE
					WHEN GLD_LocalDebitAmount > 0 THEN GLD_LocalDebitAmount
					WHEN GLD_LocalCreditAmount > 0 THEN GLD_LocalCreditAmount * -1
				END,
	{GLMovementDetails.Schema.Basis},
	{GLMovementDetails.Schema.BranchCode},
	{GLMovementDetails.Schema.DepartmentCode},
	{GLMovementDetails.Schema.OSAmount} = CASE
					WHEN GLD_OSDebitAmount > 0 THEN GLD_OSDebitAmount
					WHEN GLD_OSCreditAmount > 0 THEN GLD_OSCreditAmount * -1
				END,
	{GLMovementDetails.Schema.CurrencyCode} = GLD_Currency
FROM 
	dbo.AccTaxTransaction ATT
	JOIN dbo.AccTaxGLMovement ATM ON ATM_ATT_TaxTransaction = ATT.ATT_PK
	JOIN dbo.AccGeneralLedgerData GLD ON ATM.ATM_PK = GLD.GLD_ATM_TaxGLMovement
	JOIN dbo.AccTaxConfiguration ETC ON ATT_ETC = ETC_PK
	LEFT JOIN dbo.AccGLHeader AG ON GLD.GLD_AG_GLAccount = AG.AG_PK
	LEFT JOIN dbo.GlbBranch ON GLD.GLD_GB_Branch = GB_PK
	LEFT JOIN dbo.GlbDepartment ON  GLD.GLD_GE_Department = GE_PK
WHERE
	ATT_AH = @TransactionHeader
ORDER BY
	{GLMovementDetails.Schema.PostPeriod}, {GLMovementDetails.Schema.PostDate}, {GLMovementDetails.Schema.Basis}, {GLMovementDetails.Schema.TaxConfiguration}, {GLMovementDetails.Schema.GLAccount}";

			return LoadGLMovementDetailsFromQuery(factory, taxParentPK, query);
		}

		IReadOnlyCollection<IGLMovementDetails> LoadGLMovementDetailsFromQuery(BusinessObjectFactory factory, ZGuid taxParentPK, string query)
		{
			var result = new List<IGLMovementDetails>();
			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load(query, new [] { ZSqlParameter.New("@TransactionHeader", taxParentPK, AccTransactionHeaderSchema.PK) });

			foreach (var item in collection)
			{
				IGLMovementDetails glMovementDetails = new GLMovementDetails();
				glMovementDetails.TaxConfiguration = item[GLMovementDetails.Schema.TaxConfiguration].ToString();
				glMovementDetails.GLAccount = item[GLMovementDetails.Schema.GLAccount].ToString();
				glMovementDetails.GLAccountDesc = item[GLMovementDetails.Schema.GLAccountDesc].ToString();
				glMovementDetails.PostPeriod = item[GLMovementDetails.Schema.PostPeriod].ToString();
				glMovementDetails.PostDate = new ZDate(item[GLMovementDetails.Schema.PostDate].ToString());
				glMovementDetails.Amount = ZDecimal.ParseSafe(item[GLMovementDetails.Schema.Amount].ToString(), ZDecimal.Zero);
				glMovementDetails.Basis = item[GLMovementDetails.Schema.Basis].ToString();
				glMovementDetails.BranchCode = item[GLMovementDetails.Schema.BranchCode].ToString();
				glMovementDetails.DepartmentCode = item[GLMovementDetails.Schema.DepartmentCode].ToString();
				glMovementDetails.OSAmount = ZDecimal.ParseSafe(item[GLMovementDetails.Schema.OSAmount].ToString(), ZDecimal.Zero);
				glMovementDetails.CurrencyCode = item[GLMovementDetails.Schema.CurrencyCode].ToString();

				result.Add(glMovementDetails);
			}

			return result;
		}
	}
}
