using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.H7.GUI;
using Enterprise.Customs.IE.H7.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.H7.GUI
{
	public class H7ApplicationGUIProvider : EU.H7.GUI.H7ApplicationGUIProvider
	{
		public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm, ASYCUDA.Business.AsycudaManifestHeader header) => new MenuBuilder(header, mainForm);

		public override Type ApplicationBusinessProviderType => typeof(H7ApplicationBusinessProvider);

		protected override IPanelLayoutProvider GetBillLayoutCore() => new IEH7BillLayouts();

		protected override IPanelLayoutProvider GetManifestLayoutCore() => new IEH7ManifestLayouts();

		protected override IReadOnlyDictionary<bool, string[]> GetBillsGridColumnAvailabilityCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			return new Dictionary<bool, string[]>
			{
				{
					false,
					new[]
					{
						AsycudaBill.Schema.ABL_FreightValue,
						AsycudaBill.Schema.ABL_RX_NKFreightValueCurrency,
						AsycudaBillSchema.Constants.ABL_BolType
					}
				}
			};
		}

		protected override IEnumerable<ZGridColumnInfo> GetBillsGridExtraColumnInfosCore()
		{
			foreach (var columnInfo in base.GetBillsGridExtraColumnInfosCore())
			{
				yield return columnInfo;
			}

			var shipmentTypeColumnStyleInfo = new ZDropEditColumnStyleInfo();
			shipmentTypeColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_ShipmentType;
			shipmentTypeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			yield return shipmentTypeColumnStyleInfo;
		}

		protected override IEnumerable<IAdditionalTabPage> GetHeaderAdditionalTabPageUserControlsCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			return Enumerable.Empty<IAdditionalTabPage>();
		}

		protected override IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControlCore()
		{
			yield return new EUH7PackUserControl();
			yield return new EUH7ItemUserControl();
			yield return new AdditionalDocumentsUserControl();
			yield return new RequestedDocumentsUserControl();
			yield return new RelatedDocumentsUserControlWithGrid(RelatedDocumentsUserControlWithGrid.ResStringSupportingDocuments, RelatedDocumentsUserControlWithGrid.SupportingDocumentsTabSequence);
			yield return new RelatedDocumentsUserControlWithGrid(RelatedDocumentsUserControlWithGrid.ResStringPreviousDocuments, RelatedDocumentsUserControlWithGrid.PreviousDocumentsTabSequence);
		}

		protected override IEnumerable<IAdditionalTabPage> GetItemAdditionalTabPageUserControlCore()
		{
			yield return new EUH7ItemPacksUserControl();
			yield return new AdditionalDocumentsUserControl();
			yield return new RelatedDocumentsUserControlWithGrid(RelatedDocumentsUserControlWithGrid.ResStringSupportingDocuments, RelatedDocumentsUserControlWithGrid.SupportingDocumentsTabSequence);
			yield return new RelatedDocumentsUserControlWithGrid(RelatedDocumentsUserControlWithGrid.ResStringPreviousDocuments, RelatedDocumentsUserControlWithGrid.PreviousDocumentsTabSequence);
		}

		protected override string[] GetPacksGridMandatoryColumnsCore()
		{
			return new string[]
			{
				AsycudaPack.Schema.APA_PackQty, AsycudaPack.Schema.APA_PackUQ,
				AsycudaPack.Schema.APA_MarksAndNumbers, AsycudaPack.Schema.APA_Weight,
				AsycudaPack.Schema.APA_WeightUQ, AsycudaPack.Schema.APA_LineNo,
			};
		}
	}
}
