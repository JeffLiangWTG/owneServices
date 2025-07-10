using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public partial class AsycudaManifestHeaderAdditionalTabPageUserControl : AdditionalTabPageUserControlBase
	{
		public AsycudaManifestHeaderAdditionalTabPageUserControl()
		{
			InitializeComponent();
		}

		protected override ZUserControl GetManifestSpecificUserControl(ApplicationGUIProvider provider) => CurrentControl;
		protected override Control ControlToAddManifestSpecificUserControl => this;
	}
}
