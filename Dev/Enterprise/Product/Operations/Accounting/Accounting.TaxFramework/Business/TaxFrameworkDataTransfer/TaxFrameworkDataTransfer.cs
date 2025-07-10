using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public interface ITaxFrameworkDataTransfer
	{
		IReadOnlyCollection<IReadOnlyTaxRecordData> CreateTaxRecordDataFromTaxTransactions(ITaxRecordParent taxParent, AccTaxTransaction[] accTaxTransactions, AccTaxRecordTransactionLinePivot[] accTaxRecordTransactionLinePivots);
		void CreateTaxTransactionFromTaxRecordData(ITaxRecordParent taxParent, IReadOnlyCollection<IReadOnlyTaxRecordData> taxRecordsData);
	}

	class TaxFrameworkDataTransfer : ITaxFrameworkDataTransfer
	{
		public TaxFrameworkDataTransfer(ITaxRecordPivotProcessor taxRecordPivotPorcessor)
		{
			TaxRecordPivotProcessor = Argument.NotNull(taxRecordPivotPorcessor, nameof(taxRecordPivotPorcessor));
		}

		public IReadOnlyCollection<IReadOnlyTaxRecordData> CreateTaxRecordDataFromTaxTransactions(ITaxRecordParent taxParent, AccTaxTransaction[] accTaxTransactions, AccTaxRecordTransactionLinePivot[] accTaxRecordTransactionLinePivots)
		{
			if (!taxParent.IsTaxTransactionsCalculatedBeforePosting)
			{
				return null;
			}

			var taxTransactionLinesInfoDictionary = GetTaxTransactionLinePKs(accTaxRecordTransactionLinePivots);

			var taxRecordDataList = new List<IReadOnlyTaxRecordData>();

			for (int i = 0; i < accTaxTransactions.Length; i++)
			{
				var taxRecordData = new TaxRecordData();

				var accTaxTransaction = accTaxTransactions[i];

				taxRecordData.TaxMessagePK = accTaxTransaction.ATT_A9_TaxMessage;
				taxRecordData.AffectsSourceTransactionTotal = accTaxTransaction.ATT_AffectsSourceTransactionTotal;
				taxRecordData.LedgerControlGLAccountPK = accTaxTransaction.ATT_AG_LedgerControlAccount;
				taxRecordData.TaxControlGLAccountPK = accTaxTransaction.ATT_AG_TaxControlAccount;
				taxRecordData.TaxExpenseGLAccountPK = accTaxTransaction.ATT_AG_TaxExpenseAccount;
				taxRecordData.TaxPendingControlGLAccountPK = accTaxTransaction.ATT_AG_TaxPendingControlAccount;
				taxRecordData.TaxIDPK = accTaxTransaction.ATT_AT_TaxID;
				taxRecordData.TaxBasis = accTaxTransaction.ATT_Basis;
				taxRecordData.TaxConfigurationPK = accTaxTransaction.ATT_ETC;
				taxRecordData.LocalTaxAmount = accTaxTransaction.ATT_LocalTaxAmount;
				taxRecordData.LocalTaxBaseAmount = accTaxTransaction.ATT_LocalTaxBaseAmount;
				taxRecordData.OSTaxAmount = accTaxTransaction.ATT_OSTaxAmount;
				taxRecordData.OSTaxBaseAmount = accTaxTransaction.ATT_OSTaxBaseAmount;
				taxRecordData.BranchPK = accTaxTransaction.ATT_GB;
				taxRecordData.DepartmentPK = accTaxTransaction.ATT_GE_Department;
				taxRecordData.Ledger = accTaxTransaction.ATT_Ledger;
				taxRecordData.PostDate = accTaxTransaction.ATT_PostDate;
				taxRecordData.RateDenominator = accTaxTransaction.ATT_RateDenominator;
				taxRecordData.RateNumerator = accTaxTransaction.ATT_RateNumerator;
				taxRecordData.RealisationDate = accTaxTransaction.ATT_RealisationDate;
				taxRecordData.OSTaxCurrency = accTaxTransaction.ATT_RX_NKOSTaxCurrency;
				taxRecordData.TaxAuthorityServiceCode = accTaxTransaction.ATT_TaxAuthorityServiceCode;
				taxRecordData.TaxAuthorityServiceCodeDescription = accTaxTransaction.ATT_TaxAuthorityServiceCodeDescription;
				taxRecordData.TaxDate = accTaxTransaction.ATT_TaxDate;
				taxRecordData.TaxSuperType = accTaxTransaction.ATT_TaxSuperType;
				taxRecordData.TaxSystemCode = accTaxTransaction.ATT_TaxSystemCode;
				taxRecordData.TransactionLinePKs = taxTransactionLinesInfoDictionary[accTaxTransaction.PK];
				taxRecordData.SystemCalculatedValues = CreateTaxRecordDataSystemCalculatedValues(accTaxTransaction);

				taxRecordDataList.Add(taxRecordData);
			}

			return taxRecordDataList;
		}

		Dictionary<ZGuid, List<ZGuid>> GetTaxTransactionLinePKs(AccTaxRecordTransactionLinePivot[] accTaxRecordTransactionLinePivots)
		{
			var result = new Dictionary<ZGuid, List<ZGuid>>();

			foreach (var pivot in accTaxRecordTransactionLinePivots)
			{
				List<ZGuid> taxRecordLinePKs;

				if (!result.TryGetValue(pivot.ATP_ATT, out taxRecordLinePKs))
				{
					taxRecordLinePKs = new List<ZGuid>();
					result.Add(pivot.ATP_ATT, taxRecordLinePKs);
				}
				taxRecordLinePKs.Add(pivot.ATP_AL_TransactionLine);
			}

			return result;
		}

		TaxRecordDataSystemCalculatedValues CreateTaxRecordDataSystemCalculatedValues(AccTaxTransaction taxRecord)
		{
			TaxRecordDataSystemCalculatedValues taxRecordSystemCalculatedValues = null;

			var systemCalculatedValues = taxRecord.GetSystemCalculatedValuesIfAvailable();

			if (systemCalculatedValues != null)
			{
				taxRecordSystemCalculatedValues = new TaxRecordDataSystemCalculatedValues(
					systemCalculatedValues.OSTaxBaseAmount,
					systemCalculatedValues.OSTaxAmount,
					systemCalculatedValues.RateNumerator,
					systemCalculatedValues.RateDenominator,
					systemCalculatedValues.TaxDate,
					systemCalculatedValues.TaxAuthorityServiceCode,
					systemCalculatedValues.TaxAuthorityServiceCodeDescription);
			}

			return taxRecordSystemCalculatedValues;
		}

		public void CreateTaxTransactionFromTaxRecordData(ITaxRecordParent taxParent, IReadOnlyCollection<IReadOnlyTaxRecordData> taxRecordsData)
		{
			List<AccTaxTransaction> taxTransactions = null;

			if (taxRecordsData != null)
			{
				var linesByLinePK = taxParent.GetLines().ToDictionary(x => x.PK);

				taxTransactions = new List<AccTaxTransaction>();

				foreach (var taxRecord in taxRecordsData)
				{
					var taxTransaction = RestoreValuesFromTaxRecordDataToTaxTransaction(taxParent, taxRecord);
					taxTransactions.Add(taxTransaction);

					RestoreValuesFromTaxRecordDataToTaxRecordTransactionLinePivot(taxTransaction, taxRecord.TransactionLinePKs, linesByLinePK);
				}

				taxParent.IsTaxTransactionsCalculatedBeforePosting = true;
			}
		}

		AccTaxTransaction RestoreValuesFromTaxRecordDataToTaxTransaction(ITaxRecordParent taxParent, IReadOnlyTaxRecordData taxRecordData)
		{
			var taxTransaction = taxParent.Factory.New<AccTaxTransaction>();

			using (taxTransaction.GetValidationSuspender())
			using (taxParent.Factory.SetTempContext(BusinessContext.RestoringIncompleteTransaction))
			{
				taxTransaction.ATT_AH = taxParent.PK;
				taxTransaction.ATT_RX_NKOSTaxCurrency = taxRecordData.OSTaxCurrency;
				taxTransaction.ATT_ETC = taxRecordData.TaxConfigurationPK;
				taxTransaction.ATT_GB = taxRecordData.BranchPK;
				taxTransaction.ATT_GC = taxTransaction.Branch?.GB_GC ?? ZGuid.Empty;
				taxTransaction.ATT_GE_Department = taxRecordData.DepartmentPK;
				taxTransaction.ATT_OSTaxBaseAmount = taxRecordData.OSTaxBaseAmount;
				taxTransaction.ATT_LocalTaxBaseAmount = taxRecordData.LocalTaxBaseAmount;
				taxTransaction.ATT_OSTaxAmount = taxRecordData.OSTaxAmount;
				taxTransaction.ATT_LocalTaxAmount = taxRecordData.LocalTaxAmount;
				taxTransaction.ATT_Ledger = taxRecordData.Ledger;
				taxTransaction.ATT_PostDate = taxRecordData.PostDate;
				taxTransaction.ATT_RateDenominator = taxRecordData.RateDenominator;
				taxTransaction.ATT_RateNumerator = taxRecordData.RateNumerator;
				taxTransaction.ATT_A9_TaxMessage = taxRecordData.TaxMessagePK;
				taxTransaction.ATT_AffectsSourceTransactionTotal = taxRecordData.AffectsSourceTransactionTotal;
				taxTransaction.ATT_AG_LedgerControlAccount = taxRecordData.LedgerControlGLAccountPK;
				taxTransaction.ATT_AG_TaxControlAccount = taxRecordData.TaxControlGLAccountPK;
				taxTransaction.ATT_AG_TaxExpenseAccount = taxRecordData.TaxExpenseGLAccountPK;
				taxTransaction.ATT_AG_TaxPendingControlAccount = taxRecordData.TaxPendingControlGLAccountPK;
				taxTransaction.ATT_AT_TaxID = taxRecordData.TaxIDPK;
				taxTransaction.ATT_Basis = taxRecordData.TaxBasis;
				taxTransaction.ATT_RealisationDate = taxRecordData.RealisationDate;
				taxTransaction.ATT_TaxDate = taxRecordData.TaxDate;
				taxTransaction.ATT_TaxSuperType = taxRecordData.TaxSuperType;
				taxTransaction.ATT_TaxSystemCode = taxRecordData.TaxSystemCode;
				taxTransaction.ATT_TaxAuthorityServiceCode = taxRecordData.TaxAuthorityServiceCode;
				taxTransaction.ATT_TaxAuthorityServiceCodeDescription = taxRecordData.TaxAuthorityServiceCodeDescription;
				RestoreSystemCalculatedValuesFromTaxRecordData(taxTransaction, taxRecordData);
			}

			return taxTransaction;
		}

		void RestoreSystemCalculatedValuesFromTaxRecordData(AccTaxTransaction taxTransaction, IReadOnlyTaxRecordData taxRecordData)
		{
			if (taxRecordData.SystemCalculatedValues != null)
			{
				taxTransaction.SetTaxTransactionsSystemCalculatedValues(
					taxRecordData.SystemCalculatedValues.OSTaxBaseAmount,
					taxRecordData.SystemCalculatedValues.OSTaxAmount,
					taxRecordData.SystemCalculatedValues.RateNumerator,
					taxRecordData.SystemCalculatedValues.RateDenominator,
					taxRecordData.SystemCalculatedValues.TaxDate,
					taxRecordData.SystemCalculatedValues.TaxAuthorityServiceCode,
					taxRecordData.SystemCalculatedValues.TaxAuthorityServiceCodeDescription);
			}
		}

		void RestoreValuesFromTaxRecordDataToTaxRecordTransactionLinePivot(AccTaxTransaction taxTransaction, IReadOnlyCollection<ZGuid> transactionLinePKs, Dictionary<ZGuid, ITaxableTransactionLine> linesByLinePK)
		{
			if (transactionLinePKs != null)
			{
				foreach (var linePK in transactionLinePKs)
				{
					if (linesByLinePK.TryGetValue(linePK, out ITaxableTransactionLine line))
					{
						TaxRecordPivotProcessor.Create(taxTransaction, line);
					}
					else
					{
						taxTransaction.AddRowError(Res.GetString("D17DFDB1-D63A-4115-9E69-21FDEFF0CFEA", "Error when restoring tax transaction because the associated line was not found. Please try recalculating tax transactions."));
					}
				}
			}
		}

		ITaxRecordPivotProcessor TaxRecordPivotProcessor { get; }
	}
}
