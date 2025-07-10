using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business
{
	public abstract class TransactionCommissionCreator : CommissionCreator
	{
		protected TransactionCommissionCreator(ICommissionableTransaction transaction, DataTable effectiveDateCacheTable = null)
			: base(transaction.Factory)
		{
			Argument.NotNull(transaction, "transaction");

			if (!transaction.AH_TransactionBelongsToGroup.IsEmpty && transaction.AH_IsCancelled)
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Passed in transaction (PK:{0}) which is a reversal. Use {1} to create commission for reversal transactions", transaction.PK, nameof(ReversalTransactionCommissionCreator)));
			}

			Transaction = transaction;
			MainTransaction = GetMainTransaction(transaction);
			this.effectiveDateCacheTable = effectiveDateCacheTable;
		}

		protected readonly ICommissionableTransaction Transaction;
		protected readonly ICommissionableTransaction MainTransaction;
		protected DataTable effectiveDateCacheTable;

		public static ICommissionableTransaction GetMainTransaction(ICommissionableTransaction transaction)
		{
			var factory = transaction.Factory;
			return (transaction.AH_TransactionBelongsToGroup.IsValid ? factory.Load<TransactionHeader>(transaction.AH_TransactionBelongsToGroup) as ICommissionableTransaction : null) ?? transaction;
		}

		protected Action<AccCommissionHeader, ICommissionAgreementAndRates> GetCreatePercentageCommissionLineGroupsDelegate(IEnumerable<TransactionLine> transactionLines, Dictionary<ZGuid, ZDate> commissionDateByChargeDictionary)
		{
			return (commissionHeader, agreementAndRates) =>
			{
				CreatePercentageCommissionLineGroups(commissionHeader, transactionLines, agreementAndRates, commissionDateByChargeDictionary);
			};
		}

		protected void CreatePercentageCommissionLineGroups(AccCommissionHeader commissionHeader, IEnumerable<TransactionLine> transactionLines, ICommissionAgreementAndRates agreementAndRates, Dictionary<ZGuid, ZDate> commissionDateByChargeDictionary)
		{
			var transactionLinesGroupedByChargeCodeAndCurrency =
				from transactionLine in transactionLines
				where transactionLine.AL_AC.IsValid
				group transactionLine by new { transactionLine.AL_AC, transactionLine.AL_RX_NKTransactionCurrency };

			if (!transactionLinesGroupedByChargeCodeAndCurrency.Any())
			{
				return;
			}

			var agreement = agreementAndRates.CommissionAgreement;
			var agreementIsForRevenueOnly = agreement.CA0_CommissionBasis == CommissionBasisType.Codes.REV;
			foreach (var grouping in transactionLinesGroupedByChargeCodeAndCurrency)
			{
				var commissionLineGroup = commissionHeader.LineGroups.AddNew();
				using (commissionLineGroup.GetValidationSuspender())
				{
					commissionLineGroup.CLG_AC = grouping.Key.AL_AC;
					var commissionableLines = grouping.Where(x => !agreementIsForRevenueOnly || x.AL_LineType == TransactionLineTypes.Revenue || x.AL_LineType == TransactionLineTypes.WIP);

					if (commissionDateByChargeDictionary.ContainsKey(grouping.Key.AL_AC))
					{
						commissionLineGroup.CLG_CommissionDate = commissionDateByChargeDictionary[grouping.Key.AL_AC];
					}

					commissionLineGroup.CLG_TransactionAmount = commissionableLines.Sum(x =>
						x.AL_LineAmount * (x.Company != null && x.Company.GC_IsReciprocal
							? 1 / x.AL_ExchangeRate
							: (decimal)x.AL_ExchangeRate));

					commissionLineGroup.CLG_RX_NKTransactionCurrency = grouping.Key.AL_RX_NKTransactionCurrency;

					commissionLineGroup.CLG_TotalCommissionableAmount = commissionableLines.Sum(x => x.AL_LineAmount);
					commissionLineGroup.CLG_RX_NKCommissionCurrency = Transaction.AH_Calc_LocalRXCode;
				}
			}
		}

		public static ZQuery GetIsCommissionableTransactionQuery()
		{
			var result = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, CommissionableLedgerTypes);
			result.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, CommissionableTransactionTypes);
			result.AddToFilter(ReversalTransactionCommissionCreator.GetIsNotReversalTransactionQuery());
			return result;
		}

		public static IEnumerable<string> CommissionableLedgerTypes
		{
			get
			{
				yield return LedgerTypes.AccountsReceivable;
				yield return LedgerTypes.AccountsPayable;
				yield return LedgerTypes.JobCosting;
			}
		}

		public static IEnumerable<string> CommissionableTransactionTypes
		{
			get
			{
				yield return TransactionTypes.Invoice;
				yield return TransactionTypes.CreditNote;
				yield return TransactionTypes.AdjustmentNote;
				yield return TransactionTypes.JobRevenueJournal;
			}
		}

		public void PopulateEffectiveDateForTriggerType(ZGuid customerPk)
		{
			if (effectiveDateCacheTable == null)
			{
				return;
			}

			var customerRows = effectiveDateCacheTable.Select($"CustomerPk = '{customerPk}'");
			if (customerRows != null && customerRows.Length > 0)
			{
				return;
			}

			var sqlText = @"SELECT MIN(AL_ReverseDate) MinimumDate
FROM dbo.AccTransactionHeader 
LEFT JOIN dbo.AccTransactionLines on AL_AH = AH_PK
WHERE AH_Ledger = 'AR' AND AH_TransactionType = 'INV' AND AH_OH = @CustomerPk
";

			PopulateEffectiveDateForOneTriggerType(effectiveDateCacheTable, sqlText, "ERR", customerPk);

			sqlText = @"SELECT MIN(AH_PostDate) MinimumDate 
FROM dbo.AccTransactionHeader
WHERE AH_Ledger = 'AR' AND AH_TransactionType = 'INV' AND AH_OH = @CustomerPk
";
			PopulateEffectiveDateForOneTriggerType(effectiveDateCacheTable, sqlText, "1AR", customerPk);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Need to use aggregate function in sql query")]
		void PopulateEffectiveDateForOneTriggerType(DataTable effectiveDateCacheTable, string sqlText, string triggerType, ZGuid customerPk)
		{
			var connection = ((IDbConnected)Factory).Connection;
			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@CustomerPk", SqlDbType.UniqueIdentifier, customerPk.ToGuid());
				using (var reader = cmd.ExecuteReader())
				{
					var dataRow = effectiveDateCacheTable.NewRow();
					dataRow[TvpTriggerTypeEffectiveDate.Columns.TriggerType] = triggerType;
					dataRow[TvpTriggerTypeEffectiveDate.Columns.CustomerPk] = customerPk.ToGuid();
					if (reader.Read() && reader["MinimumDate"] != DBNull.Value)
					{
						dataRow[TvpTriggerTypeEffectiveDate.Columns.EffectiveDate] = (DateTime)reader["MinimumDate"];
					}
					else
					{
						dataRow[TvpTriggerTypeEffectiveDate.Columns.EffectiveDate] = DBNull.Value;
					}
					effectiveDateCacheTable.Rows.Add(dataRow);
				}
			}
		}
	}
}
