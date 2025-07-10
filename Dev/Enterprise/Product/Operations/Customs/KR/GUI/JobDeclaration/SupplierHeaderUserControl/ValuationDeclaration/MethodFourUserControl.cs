using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class MethodFourUserControl : ZUserControl
	{
		public MethodFourUserControl()
		{
			InitializeComponent();
			SalesOfHighestQuantityDynamicLayoutPanel.UpdateLayout(new SalesOfHighestQuantityLayout());
			DeductionCostDynamicLayoutPanel.UpdateLayout(new DeductionCostLayout());
		}

		public void ChangeBindingToMessageSendingObject()
		{
			SalesOfHighestQuantityDynamicLayoutPanel.UpdateLayout(new SalesOfHighestQuantitySendingObjectLayout());
			DeductionCostDynamicLayoutPanel.UpdateLayout(new DeductionCostSendingObjectLayout());
		}
	}
}
