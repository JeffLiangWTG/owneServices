using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class ProvisionalPricingReasonsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; }

		public ProvisionalPricingReasonsLayout()
		{
			Layout = CreateLayout();
		}

		static PanelLayout CreateLayout()
		{
			var controlBag = ProvisionalPricingReasonsControlBag.InstanceForDeclaration;
			var layout = new PanelLayout();
			layout.RegisterControlBag(controlBag);

			var ruler1 = layout.CreateRuler(250);
			var ruler2 = layout.CreateRuler(550);
			var ruler3 = layout.CreateRuler(850);
			var ruler4 = layout.CreateRuler(1150);

			layout.Include(ruler1, controlBag.ProvisionalPricingReason101CheckBox, ruler2, controlBag.ProvisionalPricingReason102CheckBox, ruler3, controlBag.ProvisionalPricingReason103CheckBox, ruler4, controlBag.ProvisionalPricingReason104CheckBox);
			layout.Include(ruler1, controlBag.ProvisionalPricingReason105CheckBox, ruler2, controlBag.ProvisionalPricingReason106CheckBox, ruler3, controlBag.ProvisionalPricingReason107CheckBox, ruler4, controlBag.ProvisionalPricingReason108CheckBox);
			layout.Include(ruler1, controlBag.ProvisionalPricingReason109CheckBox, ruler2, controlBag.ProvisionalPricingReason110CheckBox, ruler3, controlBag.ProvisionalPricingReason111CheckBox, ruler4, controlBag.ProvisionalPricingReason112CheckBox);
			layout.Include(ruler4, controlBag.ProvisionalPricingReason113CheckBox);
			layout.Include(ruler4, controlBag.ProvisionalPricingReason114CheckBox);
			layout.Include(ruler4, controlBag.ProvisionalPricingReason115CheckBox);
			layout.Include(ruler4, controlBag.ProvisionalPricingReason116CheckBox);
			layout.Include(ruler4, controlBag.ProvisionalPricingReason117CheckBox);
			layout.Include(ruler4, controlBag.ProvisionalPricingReason120CheckBox);
			layout.Include(ruler1, controlBag.OtherReasonTextBox);

			return layout;
		}
	}
}
