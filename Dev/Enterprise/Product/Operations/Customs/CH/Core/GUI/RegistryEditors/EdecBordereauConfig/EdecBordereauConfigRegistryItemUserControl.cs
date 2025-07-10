using Enterprise.Registry.GUI;

namespace Enterprise.Customs.CH.GUI;

public partial class EdecBordereauConfigRegistryItemUserControl : RegistryZUserControl
{
	public EdecBordereauConfigRegistryItemUserControl()
	{
		InitializeComponent();
	}

	protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
	{
		base.SetControlOrBusinessEntityReadOnly(readOnly);
		EnabledCheckBox.ReadOnly = readOnly;
		NumberOfDaysIntEdit.ReadOnly = readOnly;
	}
}
