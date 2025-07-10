using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class DpsMatchingConfigurationControlForTest : DpsMatchingConfigurationControl
	{
		public new void SetControlOrBusinessEntityReadOnly(bool readOnly) => base.SetControlOrBusinessEntityReadOnly(readOnly);
		public new ZRadioButton StrictRadioButton => base.StrictRadioButton;
		public new ZRadioButton BalancedRadioButton => base.BalancedRadioButton;
		public new ZRadioButton ComprehensiveRadioButton => base.ComprehensiveRadioButton;
	}
}
