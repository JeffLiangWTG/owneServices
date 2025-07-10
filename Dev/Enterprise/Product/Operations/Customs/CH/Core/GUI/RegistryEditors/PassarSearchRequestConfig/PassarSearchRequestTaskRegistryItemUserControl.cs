using Enterprise.Registry.GUI;

namespace Enterprise.Customs.CH.GUI;

public partial class PassarSearchRequestConfigRegistryItemUserControl : RegistryZUserControl
{
	public PassarSearchRequestConfigRegistryItemUserControl()
	{
		InitializeComponent();
	}

	protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
	{
		base.SetControlOrBusinessEntityReadOnly(readOnly);
		EnabledCheckBox.ReadOnly = readOnly;
		TimeLimitIntEdit.ReadOnly = readOnly;
	}
}
