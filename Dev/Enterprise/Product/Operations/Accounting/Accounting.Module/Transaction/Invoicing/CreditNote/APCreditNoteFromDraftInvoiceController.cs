using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class APCreditNoteFromDraftInvoiceController : APInvoicingBaseFromDraftInvoiceController<APCreditNote>
	{
		public override ControllerID ID => ControllerIDs.APCreditNote;

		protected override ZString ExpectedTransactionType => TransactionTypes.CreditNote;

		protected override IZForm GetEditFormCore(InvoicingBase newAP) => new CreditNoteForm(newAP);

		protected override string EditFormCaption => Res.GetString("ED3B1F94-A7D5-442B-98B9-07C902B13905", "New AP Credit Note");
	}
}
