using Enterprise.ZArchitecture;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class CreditReportsRegistryControlForTest : CreditReportsRegistryControl
	{
		public new ZGrid CreditReportRegistryGrid => base.CreditReportRegistryGrid;
		public new void SetControlOrBusinessEntityReadOnly(bool readOnly) => base.SetControlOrBusinessEntityReadOnly(readOnly);
	}
}
