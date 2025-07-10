using Enterprise.Customs.GUI;

namespace Enterprise.Customs.CH.GUI;

public sealed partial class InAndOutwardProcessingUserControl : BaseCustomsEntryUserControl
{
	public InAndOutwardProcessingUserControl()
	{
		InitializeComponent();
		AddDynamicLayoutUserControl();
	}

	void AddDynamicLayoutUserControl()
	{
		DynamicInAndOutwardProcessingPanel.UpdateLayout(new InAndOutwardProcessingFieldsLayout());
	}

	protected override void ChangeControlsVisibility()
	{
		base.ChangeControlsVisibility();
		NotifyCustomsOfficesGroupBox.Visible = JobDeclaration?.IsImport ?? false;
	}
}
