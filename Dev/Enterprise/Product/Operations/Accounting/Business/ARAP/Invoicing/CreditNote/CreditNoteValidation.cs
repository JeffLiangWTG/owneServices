using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class CreditNoteValidation : InvoiceBaseValidation
	{
		public CreditNoteValidation(CreditNote parent)
			: base(parent) { Parent = parent; }

		protected new CreditNote Parent { get; }

		protected override void CheckAH_OHCore()
		{
			base.CheckAH_OHCore();
			ValidateOrgDependantLineItems();
		}

		protected override BooleanRegistryItem OriginalInvoiceDetailsMandatoryRegistryItem => AccountingConfigurationRegistry.Instance.OriginalInvoiceDetailsMandatoryOnARCreditNotes;
	}
}
