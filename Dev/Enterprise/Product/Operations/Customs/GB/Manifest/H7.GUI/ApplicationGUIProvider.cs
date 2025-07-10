using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.H7.GUI;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.H7.GUI
{
	public class ApplicationGUIProvider : H7ApplicationGUIProvider
	{
		public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm, ASYCUDA.Business.AsycudaManifestHeader header) => new MenuBuilder(header, mainForm);

		public override Type ApplicationBusinessProviderType => typeof(ApplicationBusinessProvider);

		protected override IPanelLayoutProvider GetManifestLayoutCore() => new H7ManifestLayout();

		protected override IPanelLayoutProvider GetBillLayoutCore() => new H7BillLayout();

		protected override IPanelLayoutProvider GetBillPartiesLayoutCore() => new H7BillPartiesLayout();

		protected override IPanelLayoutProvider GetItemDetailsLayoutCore() => new H7ItemDetailsLayout();

		protected override IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControlCore()
		{
			yield return new H7PackTabUserControl();
			yield return new EUH7ItemUserControl();
			yield return new AdditionalInfoUserControl();
			yield return new SupportingDocumentsUserControl();
			yield return new H7PreviousDocumentsUserControl();
		}

		protected override IEnumerable<IAdditionalTabPage> GetItemAdditionalTabPageUserControlCore()
		{
			yield return new EUH7ItemPacksUserControl(); // Adds clarity but is unused in messaging
			yield return new AdditionalInfoUserControl();
			yield return new H7SupportingDocumentsUserControl();
			yield return new H7PreviousDocumentsUserControl();
		}

		protected override void CustomizeItemsGridCore(ZGrid itemsGrid, EU.H7.Business.AsycudaManifestHeader header)
		{
			base.CustomizeItemsGridCore(itemsGrid, header);
			itemsGrid.SetColumnVisible(true, [AsycudaPackedItem.Schema.API_NetWeight, AsycudaPackedItem.Schema.API_NetWeightUQ]);
			var tariffColumnStyle = itemsGrid.ColumnStyles.ToArray().FirstOrDefault(x => x is TariffColumnStyleInfo) as TariffColumnStyleInfo;
			if (tariffColumnStyle != null)
			{
				tariffColumnStyle.GetDataGrouping = () => header.PortOfDischarge?.IsInNorthernIreland == true
					? Universal.Constants.DataGrouping.EuropeanUnion
					: header.ApplicationBusinessProvider?.PackedItemTariffDataGrouping ?? ZString.Empty;
			}
		}

		protected override void CustomizeTariffFindBoxCore(TariffFindBox tariffFindBox, EU.H7.Business.AsycudaManifestHeader header)
		{
			base.CustomizeTariffFindBoxCore(tariffFindBox, header);
			if (tariffFindBox != null)
			{
				tariffFindBox.GetDataGrouping = () => header.PortOfDischarge?.IsInNorthernIreland == true
					? Universal.Constants.DataGrouping.EuropeanUnion
					: header.ApplicationBusinessProvider?.PackedItemTariffDataGrouping ?? ZString.Empty;
			}
		}

		protected override IEnumerable<IAdditionalTabPage> GetHeaderAdditionalTabPageUserControlsCore(ASYCUDA.Business.AsycudaManifestHeader header) => Enumerable.Empty<IAdditionalTabPage>();

		readonly HashSet<string> billsGridToBeExcludedColumns = new HashSet<string>
		{
			AsycudaBill.Schema.ABL_Procedure
		};

		protected override IEnumerable<ZGridColumnInfo> GetBillsGridExtraColumnInfosCore()
		{
			foreach (var columnInfo in base.GetBillsGridExtraColumnInfosCore())
			{
				if (!billsGridToBeExcludedColumns.Contains(columnInfo.ColumnName))
				{
					yield return columnInfo;
				}
			}

			var shipmentTypeColumnStyleInfo = new ZDropEditColumnStyleInfo();
			shipmentTypeColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_ShipmentType;
			shipmentTypeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			yield return shipmentTypeColumnStyleInfo;
		}
	}
}
