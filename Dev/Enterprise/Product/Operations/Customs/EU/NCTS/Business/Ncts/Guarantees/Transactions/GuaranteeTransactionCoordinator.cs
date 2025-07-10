using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class GuaranteeTransactionCoordinator
	{
		public GuaranteeTransactionCoordinator(NctsDepartureMovementHeader movementHeader)
		{
			this.movementHeader = movementHeader;
		}

		public ZString LocalReferenceNumber => movementHeader.BM_PaperlessInbondNum;

		public void CreatePendingTransactions(string appId)
		{
			var localReferenceNumber = LocalReferenceNumber;
			foreach (NctsGuarantee nctsGuarantee in GuaranteesThatShouldGenerateTransactions())
			{
				nctsGuarantee.CusGuarantee?.AddTransaction(localReferenceNumber,
														(NoResString)"NCTS write-off " + localReferenceNumber + " [" + movementHeader.Header.MovementReferenceNumber + "]",
														appId,
														ZString.Empty,
														nctsGuarantee.PW_BondAmount * -1,
														0,
														status: PermitTransactionStatusList.Codes.Pending);
			}
		}

		public void UpdatePendingTransactions(string appId)
		{
			var localReferenceNumber = LocalReferenceNumber;

			var guaranteesNoLongerInDeclaration = GetGuaranteesNoLongerInDeclaration();

			foreach (var guarantee in guaranteesNoLongerInDeclaration)
			{
				var bookedAmount = GetBookedAmountOnGuarantee(guarantee, localReferenceNumber);
				if (!bookedAmount.IsEmpty)
				{
					AddCounterBalancePendingTransaction(guarantee, bookedAmount);
				}
			}

			foreach (NctsGuarantee nctsGuarantee in GuaranteesThatShouldGenerateTransactions())
			{
				var bookedAmount = GetBookedAmountOnGuarantee(nctsGuarantee.CusGuarantee, localReferenceNumber);

				ZDecimal totalAmount = bookedAmount + nctsGuarantee.PW_BondAmount;
				if (!totalAmount.IsEmpty)
				{
					AddCounterBalancePendingTransaction(nctsGuarantee.CusGuarantee, totalAmount);
				}
			}

			void AddCounterBalancePendingTransaction(CusGuaranteeHeader guarantee, ZDecimal amount)
			{
				guarantee?.AddTransaction(localReferenceNumber,
					(NoResString)"NCTS write-off " + localReferenceNumber + " [" + movementHeader.Header.MovementReferenceNumber + "]",
					appId,
					ZString.Empty,
					amount * -1,
					0,
					status: PermitTransactionStatusList.Codes.Pending);
			}
		}

		public void ConfirmTransactions()
		{
			foreach (NctsGuarantee nctsGuarantee in GuaranteesThatShouldGenerateTransactions())
			{
				nctsGuarantee.CusGuarantee?.GetTransactions()
					.Where(t => t.CPL_Reference == LocalReferenceNumber && t.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Pending)
					.ForEach(t => t.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Confirmed);
			}
		}

		public void CounterBalanceConfirmedTransactions(string appId)
		{
			foreach (NctsGuarantee nctsGuarantee in GuaranteesThatShouldGenerateTransactions())
			{
				if (nctsGuarantee.CusGuarantee is CusGuaranteeHeader guarantee)
				{
					CounterBalanceConfirmedTransactions(appId, guarantee);
				}
			}
		}

		public void CounterBalanceTransactionsOfGuaranteesNoLongerInDeclaration(string appId)
		{
			foreach (var guaranteeHeader in GetGuaranteesNoLongerInDeclaration())
			{
				CounterBalanceConfirmedTransactions(appId, guaranteeHeader);
			}
		}

		public void CounterBalanceConfirmedTransactions(string appId, CusGuaranteeHeader guaranteeHeader)
		{
			var localReferenceNumber = LocalReferenceNumber;

			guaranteeHeader.GetTransactions()
				.Where(t => t.CPL_Reference == localReferenceNumber && t.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Pending)
				.ForEach(t => t.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Deleted);

			var confirmedTransactionsSum = guaranteeHeader.GetTransactions()
				.Where(t => t.CPL_Reference == localReferenceNumber && t.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Confirmed)
				.Sum(t => t.CPL_TranValue);

			if (confirmedTransactionsSum != 0)
			{
				guaranteeHeader.AddTransaction(localReferenceNumber,
					(NoResString)"NCTS write-off " + appId + " [" + movementHeader.Header.MovementReferenceNumber + "]",
					appId,
					ZString.Empty,
					confirmedTransactionsSum * -1,
					0,
					status: PermitTransactionStatusList.Codes.Confirmed);
			}
		}

		public void DeleteTransactions()
		{
			foreach (NctsGuarantee nctsGuarantee in GuaranteesThatShouldGenerateTransactions())
			{
				nctsGuarantee.CusGuarantee?.GetTransactions()
					.Where(t => t.CPL_Reference == LocalReferenceNumber)
					.ForEach(t => t.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Deleted);
			}
		}

		public ZDecimal GetBookedAmountOnGuarantee(CusGuaranteeHeader guarantee, string reference)
		{
			return guarantee?.GetTransactions()
				.Where(x => x.CPL_Reference == reference && (x.IsPending || x.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Confirmed))
				.Sum(x => x.CPL_TranValue) ?? ZDecimal.Zero;
		}

		public void ConfirmValidAndDeleteInvalidTransactions()
		{
			var localReferenceNumber = LocalReferenceNumber;

			foreach (var nctsGuarantee in GuaranteesThatShouldGenerateTransactions())
			{
				nctsGuarantee.CusGuarantee?.GetTransactions()
					.Where(t => t.CPL_Reference == localReferenceNumber && t.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Pending)
					.ForEach(t => t.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Confirmed);
			}

			foreach (var guaranteeHeader in GetGuaranteesNoLongerInDeclaration())
			{
				guaranteeHeader.GetTransactions()
					.Where(t => t.CPL_Reference == localReferenceNumber && t.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Pending)
					.ForEach(t => t.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Deleted);
			}
		}

		public void UpdateGuaranteeAmount(NctsGuarantee nctsGuarantee, string appId, Money incomingAmountMoney)
		{
			var guaranteeHeader = nctsGuarantee?.CusGuarantee;
			var localReferenceNumber = LocalReferenceNumber;
			if (guaranteeHeader != null)
			{
				var incomingAmount = incomingAmountMoney.Amount;
				var existingTransactionsAmount = guaranteeHeader.CusGuaranteeLineTransactions.Cast<BaseCusGuaranteeLineTransaction>()
					.Where(x => x.CPL_Reference == localReferenceNumber && x.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Confirmed).Sum(x => x.CPL_TranValue);
				var amountDifference = -(incomingAmount + existingTransactionsAmount);

				if (amountDifference != 0)
				{
					guaranteeHeader.AddTransaction(localReferenceNumber, Res.GetString("87CA63CE-33D3-47FC-B998-B5770F7E6E0C", "NCTS departure {0} adjustment", localReferenceNumber), appId, ZString.Empty, amountDifference, 0, PermitTransactionStatusList.Codes.Confirmed);
				}

				nctsGuarantee.PW_Override = true;
				nctsGuarantee.PW_BondAmount = incomingAmount;
			}
		}

		protected virtual bool ShouldCreateTransactionForBondType(string bondType) => true;

		IEnumerable<NctsGuarantee> GuaranteesThatShouldGenerateTransactions() => movementHeader.Guarantees.Where(g => ShouldCreateTransactionForBondType(g.PW_BondType)).ToArray();

		CusGuaranteeHeader[] GetGuaranteesNoLongerInDeclaration()
		{
			var guaranteeHeaderQuery = new ZDBOnlyQuery(typeof(CusGuaranteeHeader));
			guaranteeHeaderQuery.AddToFilter(CusPermitHeaderSchema.CPH_Number, SQLComparisonOperator.NotEqual, movementHeader.Guarantees.Select(x => x.PW_BondNumber));

			var transactionQuery = new ZDBOnlySubQuery(typeof(SharedCusPermitLineTransaction), CusPermitLineTransactionSchema.CPL_CPH_PermitHeader);
			transactionQuery.AddToFilter(CusPermitLineTransactionSchema.CPL_Reference, LocalReferenceNumber);
			guaranteeHeaderQuery.AddSubQuery(transactionQuery, JoinCondition.And);
			return movementHeader.Factory.Load<CusGuaranteeHeader>(guaranteeHeaderQuery);
		}

		readonly NctsDepartureMovementHeader movementHeader;
	}
}
