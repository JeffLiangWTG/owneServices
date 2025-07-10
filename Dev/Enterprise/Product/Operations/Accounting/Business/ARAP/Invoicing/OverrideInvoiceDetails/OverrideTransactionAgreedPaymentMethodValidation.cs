using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	class OverrideTransactionAgreedPaymentMethodValidation : OverrideInvoiceDetailValidation
	{
		public OverrideTransactionAgreedPaymentMethodValidation(InvoicingBase parent)
			: base(parent)
		{
		}

		protected override void CheckAH_DueDate()
		{
			if (Parent.AH_DueDateInfo.HasChanges)
			{
				base.CheckAH_DueDate();

				MandatoryValidation.CheckEntered(Parent.AH_DueDateInfo);
				CheckAH_DueDateMustAfterInvoiceDate();
			}
		}
	}
}
