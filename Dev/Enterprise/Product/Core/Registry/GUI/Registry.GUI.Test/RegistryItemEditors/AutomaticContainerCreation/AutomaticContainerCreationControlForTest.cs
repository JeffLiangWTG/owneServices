using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class AutomaticContainerCreationControlForTest : AutomaticContainerCreationControl
	{
		public new void SetControlOrBusinessEntityReadOnly(bool readOnly) => base.SetControlOrBusinessEntityReadOnly(readOnly);
		public new ZRadioButton alwaysCreateRadioButton => base.alwaysCreateRadioButton;
		public new ZRadioButton createUpToATDOrShippingInstructionRadioButton => base.createUpToATDOrShippingInstructionRadioButton;
		public new ZRadioButton neverCreateRadioButton => base.neverCreateRadioButton;
	}
}
