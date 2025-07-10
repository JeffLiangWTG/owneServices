using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public partial class EUH7ItemUserControl : ZUserControl, IAdditionalTabPage
	{
		public EUH7ItemUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			AsycudaManifestHeader manifestHeader = null;

			if (dataSource is AsycudaManifestHeader heaader)
			{
				dataMember = (NoResString)"Bills";
				manifestHeader = heaader;
			}
			else if (dataSource is AsycudaBill bill)
			{
				dataMember = "";
				manifestHeader = bill.Header;
			}

			SetupItemsGridExtraColumnInfos(manifestHeader);

			base.SetDataBinding(dataSource, dataMember);
		}

		public new AsycudaBill CurrentDataItem => (AsycudaBill)base.CurrentDataItem;

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;

		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("593b87fe-6f9f-4ad4-9105-6414d3d18426", "Items");

		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new AdditionalTabPageVisibility(h => true, null);

		int IAdditionalTabPage.TabPageSequence => 20;

		void SetupItemsGridExtraColumnInfos(AsycudaManifestHeader header)
		{
			if (header != null)
			{
				var provider = (H7ApplicationGUIProvider)ApplicationGUIProvider.GetApplicationGuiProvider(header);

				if (provider != null)
				{
					using (ItemsGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
					{
						var billsGridColumnAvailability = provider?.GetItemsGridColumnAvailability(header);
						if (billsGridColumnAvailability != null)
						{
							foreach (var billsGridColumnAvailabilityData in billsGridColumnAvailability)
							{
								ItemsGrid.SetAvailability(billsGridColumnAvailabilityData.Key, billsGridColumnAvailabilityData.Value);
							}
						}

						provider.CustomizeItemsGrid(ItemsGrid, header);
					}
				}
			}
		}
	}
}
