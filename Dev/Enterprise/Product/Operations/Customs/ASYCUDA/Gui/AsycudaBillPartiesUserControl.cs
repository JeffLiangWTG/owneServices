using System.Windows.Forms;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public partial class AsycudaBillPartiesUserControl : ManifestSpecificProviderUserControl
	{
		public AsycudaBillPartiesUserControl()
		{
			InitializeComponent();
		}

		protected override void OnProviderIdentifierChanged(ApplicationGUIProvider provider)
		{
			var layout = provider?.GetBillPartiesLayout();
			DynamicBillPartiesPanel.UpdateLayout(layout);

			base.OnProviderIdentifierChanged(provider);
		}

		protected override Control ControlToAddManifestSpecificUserControl => BillPartiesSpecificPanel;

		protected override string ManifestSpecificUserControlDataMember => string.Empty;
	}
}
