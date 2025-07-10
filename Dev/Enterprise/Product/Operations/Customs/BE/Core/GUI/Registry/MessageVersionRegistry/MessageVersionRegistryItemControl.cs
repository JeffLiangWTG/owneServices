using Enterprise.Registry.GUI;

namespace Enterprise.Customs.BE.GUI.Registry;

public partial class MessageVersionRegistryItemControl : RegistryZUserControl
{
	public MessageVersionRegistryItemControl()
	{
		InitializeComponent();
	}

	protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
	{
		base.SetControlOrBusinessEntityReadOnly(readOnly);
		MessageVersionGrid.ReadOnly = readOnly;
	}
}
