using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class ValuationDeclarationDetailsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; }

		public ValuationDeclarationDetailsLayout()
		{
			Layout = CreateLayout();
		}

		static PanelLayout CreateLayout()
		{
			var controlBag = DetailsAndProvisionalPriceControlBag.InstanceForDeclaration;
			var layout = new PanelLayout();
			layout.RegisterControlBag(controlBag);

			var captionRuler = layout.CreateRuler(160);
			var ruler1 = layout.CreateRuler(600);

			layout.Include(0, captionRuler, controlBag.InvoiceNoTextBox, ruler1, controlBag.InvoiceDateEdit);
			layout.Include(0, captionRuler, controlBag.PurchaseOrderNoTextBox, ruler1, controlBag.PurchaseOrderDateEdit);
			layout.Include(0, captionRuler, controlBag.ContractNoTextBox, ruler1, controlBag.ContractDateEdit);
			layout.Include(0, captionRuler, controlBag.TotalCustomsValueCalcEdit);

			return layout;
		}
	}
}
