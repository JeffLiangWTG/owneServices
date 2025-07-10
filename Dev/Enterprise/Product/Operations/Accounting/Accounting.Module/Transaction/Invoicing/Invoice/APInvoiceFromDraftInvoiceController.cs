using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class APInvoiceFromDraftInvoiceController : APInvoicingBaseFromDraftInvoiceController<APInvoice>
	{
		public override ControllerID ID => ControllerIDs.APInvoice;

		protected override ZString ExpectedTransactionType => TransactionTypes.Invoice;

		protected override IZForm GetEditFormCore(InvoicingBase newAP) => new InvoiceForm(newAP);

		protected override string EditFormCaption => Res.GetString("6A06BF0B-974B-470D-B431-88C342034FC5", "New AP Invoice");
	}
}
