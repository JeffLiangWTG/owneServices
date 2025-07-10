using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class ProvisionalPriceLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; }

		public ProvisionalPriceLayout()
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
			var ruler2 = layout.CreateRuler(710);

			layout.Include(0, captionRuler, controlBag.ProvisionalPricingDropEdit, ruler1, controlBag.ProvisionalAdditionRateCalcEdit, ruler2, controlBag.ProvisionalAdditionAmountCalcEdit);
			layout.Include(0, captionRuler, controlBag.EstimatedDateOfFinalPriceDateEdit, ruler1, controlBag.ContractExpirationDateEdit);

			return layout;
		}
	}
}
