using Enterprise.Registry.GUI;

namespace Enterprise.ServiceManager.Tasks.FTP
{
	public partial class FtpConfigControl : RegistryZUserControl
	{
		public FtpConfigControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ProfilesGrid.ReadOnly = readOnly;
		}
	}
}
