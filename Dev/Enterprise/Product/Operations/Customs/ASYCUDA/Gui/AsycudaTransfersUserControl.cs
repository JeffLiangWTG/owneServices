using System.Windows.Forms;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public sealed partial class AsycudaTransfersUserControl : ManifestSpecificProviderUserControl
	{
		public AsycudaTransfersUserControl() : base()
		{
			InitializeComponent();
		}

		protected override Control ControlToAddManifestSpecificUserControl => transferDetailsSpecificPanel;
		protected override string ManifestSpecificUserControlDataMember => ".";
	}
}
