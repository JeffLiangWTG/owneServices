using System;
using System.Collections.Generic;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.BR.Manifest.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using AsycudaPack = Enterprise.Customs.BR.Manifest.Business.AsycudaPack;

namespace Enterprise.Customs.BR.Manifest.GUI
{
	public class ApplicationGUIProvider : ASYCUDA.GUI.ApplicationGUIProvider
	{
		public override Type ApplicationBusinessProviderType => typeof(ApplicationBusinessProvider);

		public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm, ASYCUDA.Business.AsycudaManifestHeader header) => new MenuBuilder(header, mainForm);

		protected override IPanelLayoutProvider GetManifestLayoutCore() => new BRManifestLayouts();

		protected override IPanelLayoutProvider GetBillLayoutCore() => new BRBillLayouts();

		protected override IPanelLayoutProvider GetBillPartiesLayoutCore() => new BRBillPartiesLayouts();

		protected override IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControlCore()
		{
			yield return new AsycudaPackUserControl();
		}

		protected override IEnumerable<ZGridColumnInfo> GetContainersGridExtraColumnInfosCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			var zTareWeightCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			zTareWeightCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zTareWeightCalcEditColumnStyleInfo1.ColumnName = Business.AsycudaContainer.Schema.TareWeight;
			zTareWeightCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			yield return zTareWeightCalcEditColumnStyleInfo1;
		}

		protected override IReadOnlyDictionary<bool, string[]> GetBillsGridColumnAvailabilityCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			return new Dictionary<bool, string[]>() { { false, new[] { Business.AsycudaBill.Schema.ABL_BolType, } } };
		}

		protected override ZBool ShowTaxesUserControlCore(ASYCUDA.Business.AsycudaManifestHeader header) => ((Business.AsycudaManifestHeader)header).IsMercante;

		protected override ResourceStringData GetDutiesTabPageCaptionCore() => Res.GetData("432C0AD8-1956-4E0A-940C-583645128AB1", "Charges");

		protected override string[] GetTaxesGridColumnsOrderCore()
		{
			return new string[]
			{
				AutoAsycudaTax.Schema.AET_ChargeType,
				Business.AsycudaTax.Schema.AET_TypeDescription,
				AutoAsycudaTax.Schema.AET_ChargeAmount,
				AutoAsycudaTax.Schema.AET_RX_NKCurrency,
				AutoAsycudaTax.Schema.AET_MethodOfPayment,
			};
		}

		protected override IEnumerable<ZGridColumnInfo> GetTaxesGridExtraColumnInfosCore()
		{
			var aet_descriptionTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			aet_descriptionTextBoxColumnStyleInfo.ColumnName = "AET_TypeDescription";
			aet_descriptionTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			yield return aet_descriptionTextBoxColumnStyleInfo;
		}

		protected override string[] GetPacksGridColumnsOrderCore()
		{
			return new string[]
			{
				AsycudaPack.Schema.APA_PackQty,
				AsycudaPack.Schema.APA_PackUQ,
				AsycudaPack.Schema.BulkType,
				AsycudaPack.Schema.APA_Weight,
				AsycudaPack.Schema.APA_WeightUQ,
				AsycudaPack.Schema.APA_Volume,
				AsycudaPack.Schema.APA_VolumeUQ,
				AsycudaPack.Schema.APA_GoodsDescription,
				AsycudaPack.Schema.ContainerPK,
				AsycudaPack.Schema.APA_CommodityCode,
				AsycudaPack.Schema.APA_MarksAndNumbers,
			};
		}

		protected override IEnumerable<ZGridColumnInfo> GetPacksGridExtraColumnInfosCore()
		{
			var bulkTypeDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			bulkTypeDropEditColumnStyleInfo.ColumnName = "BulkType";
			bulkTypeDropEditColumnStyleInfo.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			bulkTypeDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(117);
			yield return bulkTypeDropEditColumnStyleInfo;
		}
	}
}
