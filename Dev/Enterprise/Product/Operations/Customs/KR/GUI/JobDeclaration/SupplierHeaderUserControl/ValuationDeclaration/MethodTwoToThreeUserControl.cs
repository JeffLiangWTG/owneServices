using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class MethodTwoToThreeUserControl : ZUserControl
	{
		public MethodTwoToThreeUserControl()
		{
			InitializeComponent();
			ReplacementDynamicLayoutPanel.UpdateLayout(new ReplacementLayout());
			AdditionalAdjustmentDynamicLayoutPanel.UpdateLayout(new AdditionalAdjustmentLayout());
			DeductionAdjustmentDynamicLayoutPanel.UpdateLayout(new DeductionAdjustmentLayout());
		}

		public void ChangeBindingToMessageSendingObject()
		{
			ReplacementDynamicLayoutPanel.UpdateLayout(new ReplacementSendingObjectLayout());
			AdditionalAdjustmentDynamicLayoutPanel.UpdateLayout(new AdditionalAdjustmentSendingObjectLayout());
			DeductionAdjustmentDynamicLayoutPanel.UpdateLayout(new DeductionAdjustmentSendingObjectLayout());
		}
	}
}
