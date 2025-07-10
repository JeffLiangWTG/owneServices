using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class AmountAgreedUponWithCustomsSendingObjectLayout : IPanelLayoutProvider
	{
		PanelLayout IPanelLayoutProvider.Layout => PanelLayout;

		PanelLayout PanelLayout { get; }

		public AmountAgreedUponWithCustomsSendingObjectLayout()
		{
			PanelLayout = CreatePanelLayout();
		}

		PanelLayout CreatePanelLayout()
		{
			var layout = new PanelLayout();
			var bag = MethodFiveToSixControlBag.InstanceForSendingObject;
			layout.RegisterControlBag(bag);

			var ruler1 = layout.CreateRuler(170);
			layout.Include(ruler1, bag.AmountAgreedUponWithCustomsKRWCalcEdit);

			return layout;
		}
	}
}
