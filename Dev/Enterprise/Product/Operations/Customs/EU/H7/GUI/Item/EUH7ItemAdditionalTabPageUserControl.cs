using System.Windows.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public partial class EUH7ItemAdditionalTabPageUserControl : AdditionalTabPageUserControlBase
	{
		public EUH7ItemAdditionalTabPageUserControl()
		{
			InitializeComponent();
		}

		protected override ZUserControl GetManifestSpecificUserControl(ApplicationGUIProvider provider) => CurrentControl;
		protected override Control ControlToAddManifestSpecificUserControl => this;
		protected override string ManifestSpecificUserControlDataMember => "";
	}
}
