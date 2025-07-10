using System;
using System.Collections.Generic;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.CO.Manifest.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CO.Manifest.GUI
{
	public class ApplicationGUIProvider : ASYCUDA.GUI.ApplicationGUIProvider
	{
		public override Type ApplicationBusinessProviderType => typeof(ApplicationBusinessProvider);

		public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm, ASYCUDA.Business.AsycudaManifestHeader header) => new MenuBuilder(header, mainForm);

		protected override IPanelLayoutProvider GetBillLayoutCore() => new COBillLayouts();

		protected override IPanelLayoutProvider GetManifestLayoutCore() => new COManifestLayouts();

		protected override IPanelLayoutProvider GetBillPartiesLayoutCore() => new COBillPartiesLayouts();

		protected override IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControlCore()
		{
			yield return new AsycudaPackUserControl();
		}

		protected override string[] GetPacksGridColumnsOrderCore()
		{
			return new string[]
			{
				AsycudaPack.Schema.APA_LineNo,
				AsycudaPack.Schema.APA_PackQty,
				AsycudaPack.Schema.APA_PackUQ,
				AsycudaPack.Schema.APA_Weight,
				AsycudaPack.Schema.APA_WeightUQ,
				AsycudaPack.Schema.APA_Volume,
				AsycudaPack.Schema.APA_VolumeUQ,
				AsycudaPack.Schema.APA_GoodsDescription,
				AsycudaPack.Schema.ContainerPK,
				UNDGDataItemFormManager.ColumnNames.UNDGSubstanceManagerValue,
				UNDGDataItemFormManager.ColumnNames.UNDGClassManagerValue,
				AsycudaPack.Schema.IsHazardous,
				AsycudaPack.Schema.ContactPK,
			};
		}

		protected override IEnumerable<ZGridColumnInfo> GetBillsGridExtraColumnInfosCore()
		{
			var billIssueDateEditInfo = new ZDateEditColumnStyleInfo();
			billIssueDateEditInfo.ColumnName = "ABL_BillIssueDate";
			billIssueDateEditInfo.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			billIssueDateEditInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			yield return billIssueDateEditInfo;
		}

		protected override IEnumerable<ZGridColumnInfo> GetPacksGridExtraColumnInfosCore()
		{
			var contactNameCodeFindBoxColumnStyleInfo = new ZGuidFindBoxColumnStyleInfo();
			contactNameCodeFindBoxColumnStyleInfo.BindToList = "UNDGs+Contacts";
			contactNameCodeFindBoxColumnStyleInfo.CaptionResourceString = Enterprise.Customs.CO.Manifest.GUI.Res.GetData("A437F3C0-D8CB-480A-A14D-707C2D3EEDDF", "DG Contact");
			contactNameCodeFindBoxColumnStyleInfo.ColumnName = "ContactPK";
			contactNameCodeFindBoxColumnStyleInfo.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			contactNameCodeFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			yield return contactNameCodeFindBoxColumnStyleInfo;

			var isHazardousCheckboxColumnStyleInfo = new ZCheckBoxColumnStyleInfo();
			isHazardousCheckboxColumnStyleInfo.ColumnName = "IsHazardous";
			isHazardousCheckboxColumnStyleInfo.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			isHazardousCheckboxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			yield return isHazardousCheckboxColumnStyleInfo;
		}

		protected override IReadOnlyDictionary<bool, string[]> GetBillsGridColumnAvailabilityCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			return new Dictionary<bool, string[]>() { { false, new[] { AsycudaBill.Schema.ABL_BolType, } } };
		}
	}
}
