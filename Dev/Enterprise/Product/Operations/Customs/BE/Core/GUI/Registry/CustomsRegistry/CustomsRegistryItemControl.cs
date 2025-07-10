using Enterprise.Registry.GUI;

namespace Enterprise.Customs.BE.GUI.Registry;

public partial class CustomsRegistryItemControl : RegistryZUserControl
{
	public CustomsRegistryItemControl()
	{
		InitializeComponent();
	}

	protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
	{
		base.SetControlOrBusinessEntityReadOnly(readOnly);
		CustomsRegistryGrid.ReadOnly = readOnly;
	}
}
