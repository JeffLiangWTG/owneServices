using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public partial class AsycudaTransferDetailsUserControl : ManifestSpecificProviderUserControl
	{
		public AsycudaTransferDetailsUserControl()
		{
			InitializeComponent();
		}

		protected override void OnProviderIdentifierChanged(ApplicationGUIProvider provider)
		{
			dynamicDetailsPanel.UpdateLayout(provider?.GetTransferDetailsLayout());
			base.OnProviderIdentifierChanged(provider);
			SetTransferBillsGridExtraColumnInfosVisibility(provider);
		}

		void SetTransferBillsGridExtraColumnInfosVisibility(ApplicationGUIProvider provider)
		{
			using (transferBillsGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				if (transferBillsGridExtraColumnInfos != null)
				{
					foreach (var columnInfo in transferBillsGridExtraColumnInfos)
					{
						transferBillsGrid.ColumnStyles.Remove(columnInfo);
					}
				}

				if (provider != null)
				{
					transferBillsGridExtraColumnInfos = provider.GetTransferBillsGridExtraColumnInfos().ToArray();
					foreach (var columnInfo in transferBillsGridExtraColumnInfos)
					{
						transferBillsGrid.ColumnStyles.Add(columnInfo);
					}

					transferBillsGrid.ReOrderColumns(provider.GetTransferBillsGridColumnsOrder().ToArray());
				}
			}
		}
		ZGridColumnInfo[] transferBillsGridExtraColumnInfos;

		protected override ZUserControl GetManifestSpecificUserControl(ApplicationGUIProvider provider) => provider.GetTransferBillCountrySpecificUserControl();
		protected override Control ControlToAddManifestSpecificUserControl => CountrySpecificPanel;
		protected override string ManifestSpecificUserControlDataMember => "TransferBills";
	}
}
