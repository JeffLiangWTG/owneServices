using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.UPE.Registry.GUI
{
	public partial class UPEGlbGroupsControl : RegistryZUserControl
	{
		public UPEGlbGroupsControl()
		{
			InitializeComponent();
			UPEGlbGroupsGrid.DisableImportDataMenuItem = true;
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			UPEGlbGroupsGrid.ReadOnly = readOnly;
		}

		internal ZGrid UPEGlbGroupsGridForTest
		{
			get { return UPEGlbGroupsGrid; }
		}
	}
}
