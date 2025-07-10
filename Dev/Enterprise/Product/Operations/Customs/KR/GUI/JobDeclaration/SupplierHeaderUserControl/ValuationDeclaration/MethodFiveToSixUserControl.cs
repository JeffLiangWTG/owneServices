using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class MethodFiveToSixUserControl : ZUserControl
	{
		public MethodFiveToSixUserControl()
		{
			InitializeComponent();
			AmountAgreedUponWithCustomsDynamicLayoutPanel.UpdateLayout(new AmountAgreedUponWithCustomsLayout());
			AdditionalCostDynamicLayoutPanel.UpdateLayout(new AdditionalCostLayout());
		}

		public void ChangeBindingToMessageSendingObject()
		{
			AmountAgreedUponWithCustomsDynamicLayoutPanel.UpdateLayout(new AmountAgreedUponWithCustomsSendingObjectLayout());
			AdditionalCostDynamicLayoutPanel.UpdateLayout(new AdditionalCostSendingObjectLayout());
		}
	}
}
