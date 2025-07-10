using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.ClientSharedComponents.Registry
{
	[SuppressFormsLocalizedTest]
	public partial class ServiceLevelRegistryControl : RegistryZUserControl
	{
		public ServiceLevelRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ServiceLevelGrid.ReadOnly = readOnly;
		}
	}
}
