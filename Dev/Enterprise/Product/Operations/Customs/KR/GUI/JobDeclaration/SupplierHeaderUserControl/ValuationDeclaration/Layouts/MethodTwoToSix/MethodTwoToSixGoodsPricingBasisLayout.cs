using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class MethodTwoToSixGoodsPricingBasisLayout : IPanelLayoutProvider
	{
		PanelLayout IPanelLayoutProvider.Layout => PanelLayout;

		PanelLayout PanelLayout { get; }

		public MethodTwoToSixGoodsPricingBasisLayout()
		{
			PanelLayout = CreatePanelLayout();
		}

		PanelLayout CreatePanelLayout()
		{
			var controlBag = MethodTwoToSixControlBag.InstanceForDeclaration;
			var layout = new PanelLayout();
			layout.RegisterControlBag(controlBag);

			var ruler1 = layout.CreateRuler(240);
			var ruler2 = layout.CreateRuler(220);

			layout.Include(0, ruler1, controlBag.PerformancePriceOfPaidTransactionCheckBox);
			layout.Include(0, ruler1, controlBag.PriceListCheckBox);
			layout.Include(0, ruler1, controlBag.ManufacturingCostCheckBox);
			layout.AddColumn();

			layout.Include(1, ruler2, controlBag.InvoiceCheckBox);
			layout.Include(1, ruler2, controlBag.GoodsPricingBasisOtherReasonTextBox);

			return layout;
		}
	}
}
