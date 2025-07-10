using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public partial class AsycudaContainerUserControl : ManifestSpecificProviderUserControl
	{
		public AsycudaContainerUserControl()
		{
			InitializeComponent();
		}

		protected override ZUserControl GetManifestSpecificUserControl(ApplicationGUIProvider provider) => provider.GetContainerCountrySpecificUserControl();
		protected override Control ControlToAddManifestSpecificUserControl => countrySpecificPanel;
		protected override string ManifestSpecificUserControlDataMember => "Containers";

		void CountrySpecificPanel_VisibleChanged(object sender, System.EventArgs e)
		{
			containerDataSplitContainer.Panel2Collapsed = !countrySpecificPanel.Visible;
		}

		protected override void OnProviderIdentifierChanged(ApplicationGUIProvider provider)
		{
			base.OnProviderIdentifierChanged(provider);
			SetContainersGridExtraColumnInfosVisibility(provider);
		}

		void SetContainersGridExtraColumnInfosVisibility(ApplicationGUIProvider provider)
		{
			using (containersGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				if (containersGridExtraColumnInfos != null)
				{
					foreach (var columnInfo in containersGridExtraColumnInfos)
					{
						containersGrid.ColumnStyles.Remove(columnInfo);
					}
				}
				containersGridExtraColumnInfos = provider?.GetContainersGridExtraColumnInfos(Header).ToArray();
				if (containersGridExtraColumnInfos != null)
				{
					foreach (var columnInfo in containersGridExtraColumnInfos)
					{
						containersGrid.ColumnStyles.Add(columnInfo);
					}
				}
				var containersGridColumnVisibility = provider?.GetContainersGridColumnVisibility();
				if (containersGridColumnVisibility != null)
				{
					foreach (var containerGridColumnVisibilityData in containersGridColumnVisibility)
					{
						containersGrid.SetColumnVisible(containerGridColumnVisibilityData.Key, containerGridColumnVisibilityData.Value);
					}
				}
				var billsGridColumnsOrder = provider?.GetContainersGridColumnsOrder().ToArray();
				if (billsGridColumnsOrder != null)
				{
					containersGrid.ReOrderColumns(billsGridColumnsOrder);
				}
				var containersGridColumnAvailability = provider?.GetContainersGridColumnAvailability();
				if (containersGridColumnAvailability != null)
				{
					foreach (var containerGridColumnAvailabilityData in containersGridColumnAvailability)
					{
						containersGrid.SetAvailability(containerGridColumnAvailabilityData.Key, containerGridColumnAvailabilityData.Value);
					}
				}

				var containersGridColumnsWidth = provider?.GetContainersGridColumnsWidth();
				if (containersGridColumnsWidth != null)
				{
					foreach (var containersGridColumnsWidthData in containersGridColumnsWidth)
					{
						var columnStyle = containersGrid.GetColumnStyle(containersGridColumnsWidthData.Key);
						columnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(containersGridColumnsWidthData.Value);
					}
				}
			}
		}
		ZGridColumnInfo[] containersGridExtraColumnInfos;
	}
}
