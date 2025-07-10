using System.Windows.Forms;
using CargoWise.Common;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public sealed partial class AsycudaArrivalsUserControl : ManifestSpecificProviderUserControl
	{
		public AsycudaArrivalsUserControl() : base()
		{
			InitializeComponent();
		}

		protected override Control ControlToAddManifestSpecificUserControl => countrySpecificPanel;

		protected override void OnProviderIdentifierChanged(ApplicationGUIProvider provider)
		{
			base.OnProviderIdentifierChanged(provider);
			SetArrivalHeadersGridExtraColumnInfosVisibility(provider);
			SetArrivalLinesGridExtraColumnInfosVisibility(provider);
			SetArrivalLinesTabPageReadOnly(provider);
		}

		void SetArrivalHeadersGridExtraColumnInfosVisibility(ApplicationGUIProvider provider)
		{
			using (arrivalHeadersGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				provider?.GetArrivalHeadersGridColumnAvailability()?.ForEach(x => arrivalHeadersGrid.SetAvailability(x.Key, x.Value));
			}
		}

		void SetArrivalLinesGridExtraColumnInfosVisibility(ApplicationGUIProvider provider)
		{
			using (arrivalDetailsGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				provider?.GetArrivalLinesGridColumnAvailability()?.ForEach(x => arrivalDetailsGrid.SetAvailability(x.Key, x.Value));
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			SetTransfersTabPageVisibility();
		}

		void SetTransfersTabPageVisibility()
		{
			var featureProvider = Header?.FeatureProvider;
			if (featureProvider != null)
			{
				transfersTabPage.TabVisible = featureProvider.SupportArrivalTransfers;
			}
		}

		void SetArrivalLinesTabPageReadOnly(ApplicationGUIProvider provider)
		{
			if (provider != null)
			{
				arrivalDetailsGrid.ReadOnly = provider.ArrivalLinesReadOnly();
			}
		}
	}
}
