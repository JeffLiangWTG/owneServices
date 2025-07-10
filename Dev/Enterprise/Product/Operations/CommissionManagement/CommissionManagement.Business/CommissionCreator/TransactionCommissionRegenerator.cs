using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.CommissionManagement.Business
{
	public class TransactionCommissionRegenerator : CommissionRegenerator
	{
		readonly ICommissionableTransaction Transaction;

		public TransactionCommissionRegenerator(ICommissionableTransaction transaction, BusinessObjectFactory factory, ILogger logger = null)
			: base(factory, logger)
		{
			Argument.NotNull(transaction, nameof(transaction));
			Argument.NotNull(transaction, nameof(factory));

			this.Transaction = transaction;
		}

		public override void RegenerateCommissions()
		{
			if (Transaction.AH_IsCancelled)
			{
				Logger?.Log(Integration.LogType.Information, string.Format(TransactionAlreadyCancelledMessage, Transaction.AH_TransactionNum));
				return;
			}

			ReverseCommissions();
			CreateCommissions();
		}

		protected override void CreateCommissions()
		{
			var commissionCreator = NonJobRelatedTransactionCommissionCreator.New(Transaction);
			commissionCreator.CreateCommissions(new CreateCommissionContext() { RegeneratingCommissions = true, OverrideFactory = Factory });
		}

		static string TransactionAlreadyCancelledMessage => Res.GetString("88C1B36B-F365-4132-B727-5E2DF712C4DD", "{0} - This transaction has been canceled, therefore commissions cannot be regenerated for it.");

		protected override void ReverseCommissions()
		{
			var nonReversedCommissionHeaders = new CommissionHelper().GetNonReversedCommissionHeaders(Factory, Transaction);

			if (nonReversedCommissionHeaders == (null, null))
			{
				return;
			}

			foreach (AccCommissionHeader commissionHeader in nonReversedCommissionHeaders.CommissionHeaders)
			{
				commissionHeader?.MarkAsOverriden();
			}
		}
	}
}
