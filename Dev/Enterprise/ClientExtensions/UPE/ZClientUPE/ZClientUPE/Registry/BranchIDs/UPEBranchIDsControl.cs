using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.UPE
{
	public partial class UPEBranchIDsControl : RegistryZUserControl
	{
		public UPEBranchIDsControl()
		{
			InitializeComponent();
			UPEBranchIDsGrid.DisableImportDataMenuItem = true;
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			UPEBranchIDsGrid.ReadOnly = readOnly;
		}

		internal ZGrid UPEBranchIDsGridForTest
		{
			get { return UPEBranchIDsGrid; }
		}
	}
}
