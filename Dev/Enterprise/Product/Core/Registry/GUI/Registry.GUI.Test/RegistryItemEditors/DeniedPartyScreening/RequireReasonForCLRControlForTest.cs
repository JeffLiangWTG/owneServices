using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class RequireReasonForCLRControlForTest : RequireReasonForCLRControl
	{
		public new ZGrid RequireReasonForCLRGrid => base.RequireReasonForCLRGrid;
		public new void SetControlOrBusinessEntityReadOnly(bool readOnly) => base.SetControlOrBusinessEntityReadOnly(readOnly);

		public new ZRadioButton YesRadioButton => base.YesRadioButton;
		public new ZRadioButton NoRadioButton => base.NoRadioButton;
	}
}
