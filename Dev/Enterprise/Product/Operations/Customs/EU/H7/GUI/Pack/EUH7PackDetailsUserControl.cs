using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public partial class EUH7PackDetailsUserControl : ZUserControl
	{
		public EUH7PackDetailsUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			AsycudaManifestHeader manifestHeader = null;

			if (dataSource is AsycudaManifestHeader header)
			{
				manifestHeader = header;
			}
			else if (dataSource is AsycudaBill bill)
			{
				manifestHeader = bill.Header;
			}

			UpdatePackDetailsControlLayout(manifestHeader);
			base.SetDataBinding(dataSource, dataMember);
		}

		void UpdatePackDetailsControlLayout(AsycudaManifestHeader manifestHeader)
		{
			if (manifestHeader != null)
			{
				var provider = (H7ApplicationGUIProvider)ApplicationGUIProvider.GetApplicationGuiProvider(manifestHeader);

				if (provider != null)
				{
					var layout = provider.GetH7PackDetailsLayout();
					DynamicPackDetailsPanel.UpdateLayout(layout);
				}
			}
		}
	}
}
