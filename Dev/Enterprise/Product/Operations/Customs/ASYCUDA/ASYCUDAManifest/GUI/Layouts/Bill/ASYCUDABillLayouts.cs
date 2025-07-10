using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.ASYCUDAManifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDAManifest.GUI
{
	public class ASYCUDABillLayouts : IPanelLayoutProvider
	{
		PanelLayout BillDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => BillDetails;

		public ASYCUDABillLayouts()
		{
			BillDetails = CreateBillDetailsLayout();
		}

		PanelLayout CreateBillDetailsLayout()
		{
			var builder = new BillLayoutBuilder<AsycudaBill>();
			var common = builder.CommonBag;
			var asycudaBag = ASYCUDABillControlBag.Instance;
			builder.AddControlBag(asycudaBag);

			builder.AddColumn();
			builder.Add(common.BillNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.OriginCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.FinalDestinationCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.GrossWeightCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.VolumeCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.ShipmentTypeDropEdit, ControlWidthClass.Long);
			builder.Add(common.GoodsLocationDropEditWithFixedWidth, ControlWidthClass.Long);
			builder.Add(common.LocationInformationTextBox, ControlWidthClass.Long);
			builder.Add(asycudaBag.BDEGMSeparatorUserControl, ControlWidthClass.Long);
			builder.Add(asycudaBag.SADOfficeCodeDropEdit, ControlWidthClass.Long);
			builder.Add(asycudaBag.SADRegistrationSerialTextBox, ControlWidthClass.Long);
			builder.Add(asycudaBag.SADRegistrationNumberTextBox, ControlWidthClass.Long);
			builder.Add(asycudaBag.SADRegistrationDateEdit, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(common.GoodsDescriptionTextBox, ControlWidthClass.Long);
			builder.Add(common.MarksAndNumbersTextBox, ControlWidthClass.Long);
			builder.Add(common.RemarksTextBox, ControlWidthClass.Long);
			builder.Add(common.ShipperAddressControl, ControlWidthClass.Long);
			builder.Add(common.ConsigneeAddressControl, ControlWidthClass.Long);
			builder.Add(common.NotifyPartyAddressControl, ControlWidthClass.Long);
			builder.Add(common.AgentAddressControl, ControlWidthClass.Long);
			builder.Add(common.CarrierReferenceTextBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(common.PrepaidCollectDropEdit, ControlWidthClass.Long);
			builder.Add(common.FreightValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.TransportValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.InsuranceValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.DiscountValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.OtherChargesValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.CustomsValueConvertToLocalCurrencyControl, ControlWidthClass.Long);

			builder.SetVisibility(asycudaBag.BDEGMSeparatorUserControl, ShowExportGeneralManifest, GetExportGeneralManifestDependencies());
			builder.SetVisibility(asycudaBag.SADOfficeCodeDropEdit, ShowExportGeneralManifest, GetExportGeneralManifestDependencies());
			builder.SetVisibility(asycudaBag.SADRegistrationSerialTextBox, ShowExportGeneralManifest, GetExportGeneralManifestDependencies());
			builder.SetVisibility(asycudaBag.SADRegistrationNumberTextBox, ShowExportGeneralManifest, GetExportGeneralManifestDependencies());
			builder.SetVisibility(asycudaBag.SADRegistrationDateEdit, ShowExportGeneralManifest, GetExportGeneralManifestDependencies());

			return builder.Build();
		}

		bool ShowExportGeneralManifest(AsycudaBill bill) => bill.Header?.ShowExportGeneralManifest ?? false;

		Func<AsycudaBill, ZPropertyInfo>[] GetExportGeneralManifestDependencies() =>
			new Func<AsycudaBill, ZPropertyInfo>[]
			{
				bill => bill.Header?.AMA_RN_NKCountryInfo, bill => bill.Header?.AMA_NatureInfo
			};
	}
}
