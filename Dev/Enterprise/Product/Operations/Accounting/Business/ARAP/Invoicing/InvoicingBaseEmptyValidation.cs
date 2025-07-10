using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	class InvoicingBaseEmptyValidation : TransactionHeaderEmptyValidation
	{
		public InvoicingBaseEmptyValidation(InvoicingBase parent) : base(parent)
		{
		}

		protected new InvoicingBase Parent => (InvoicingBase)base.Parent;

		protected override void CheckAH_OA_InvoiceAddressOverride()
		{
			Parent.OrganisationAddressWithContact.RefreshBinding(); //without this line on posted transactions ValidateAll doesn't clear previous validation warnings in invoice address user control
		}
	}
}