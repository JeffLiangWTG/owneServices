using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class TransactionLineEmptyValidation : AccTransactionLinesValidation
	{
		public TransactionLineEmptyValidation(TransactionLine parent)
			: base(parent)
		{
		}

		protected override void CheckAL_TaxDate()
		{
			//Do nothing 
		}

		public override void ValidateAll()
		{
#if DEBUG
			using (Parent.SuspendValidationTesting())
			{
				Parent.ClearAllNotifications();
			}
#else
			Parent.ClearAllNotifications();
#endif

			base.ValidateAll();

			ValidateMultipleReversing();
		}

		void ValidateMultipleReversing()
		{
			var transactionLine = Parent as TransactionLine;
			if (transactionLine.MultipleReversingErrors.Count != 0)
			{
				foreach (var error in transactionLine.MultipleReversingErrors)
				{
					Parent.AddRowError(error);
				}
			}
		}

		protected override INotificationType NotificationTypeForBranchDepartmentCombination
		{
			get
			{
				var invoicingLine = Parent as InvoicingLineBase;
				if (invoicingLine != null)
				{
					var invoiceBase = invoicingLine.InvoiceBase;
					if (invoiceBase != null && invoiceBase.IsReversed)
					{
						return CargoWise.EntityFramework.NotificationType.Warning;
					}
				}

				return base.NotificationTypeForBranchDepartmentCombination;
			}
		}

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info) => info.Name != AccTransactionLinesSchema.AL_AH.Name && base.ShouldValidateFKToCancelledRecord(info);
	}
}
