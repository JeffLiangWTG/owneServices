using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public partial class EUH7ItemDetailsUserControl : ZUserControl
	{
		public EUH7ItemDetailsUserControl()
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

			UpdatePackedItemDetailControlLayout(manifestHeader);
			base.SetDataBinding(dataSource, dataMember);
		}

		void UpdatePackedItemDetailControlLayout(AsycudaManifestHeader manifestHeader)
		{
			if (manifestHeader != null)
			{
				var provider = (H7ApplicationGUIProvider)ApplicationGUIProvider.GetApplicationGuiProvider(manifestHeader);

				if (provider != null)
				{
					var layout = provider.GetH7ItemDetailsLayout();
					DynamicPackedItemDetailsPanel.UpdateLayout(layout);

					CustomizeTariffFindBox(provider, manifestHeader);
				}
			}
		}

		void CustomizeTariffFindBox(H7ApplicationGUIProvider provider, AsycudaManifestHeader manifestHeader)
		{
			var tariffFindBox = DynamicPackedItemDetailsPanel.FindSingleOrDefault<TariffFindBox>("TariffFindBox");

			if (tariffFindBox != null)
			{
				provider.CustomizeTariffFindBox(tariffFindBox, manifestHeader);
			}
		}
	}
}
