using Enterprise.Customs.BR.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public sealed class ShipmentDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout ShipmentDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => ShipmentDetails;

		public ShipmentDetailsLayout()
		{
			ShipmentDetails = CreateShipmentDetailsLayout();
		}

		static PanelLayout CreateShipmentDetailsLayout()
		{
			var builder = new ShipmentDetailsLayoutBuilder<JobDeclaration>();
			var commonBag = builder.CommonBag;

			var brBag = ShipmentDetailsControlBag.Instance;
			builder.AddControlBag(brBag);

			builder.AddColumn();
			builder.Add(commonBag.HouseBillParcelPostTextBox, ControlWidthClass.Auto);
			builder.Add(brBag.UcrAndBillTypeUserControl, ControlWidthClass.Auto);
			builder.Add(brBag.CargoArrivalUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.GoodsOriginCodeFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.ShipmentDetailsOriginUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.ShipmentDetailsFinalDestinationUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.GoodsDescriptionTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.OwnersReferenceTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.WeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.VolumeCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.TotalNoOfPiecesCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.ContainerCountCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.TotalNoOfPacksCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.ShipmentDetailsIncoTermsUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.ShipmentDetailsScreeningUserControl, ControlWidthClass.Auto);

			builder.SetVisibility(commonBag.HouseBillParcelPostTextBox, h => !(h.IsImportLicense || h.IsLPCO), h => h.JE_MessageTypeInfo);
			builder.SetVisibility(brBag.UcrAndBillTypeUserControl, h => h.IsImportSiscomex, h => h.JE_MessageTypeInfo);
			builder.SetVisibility(commonBag.GoodsOriginCodeFindBox, h => h.IsCargoProvenanceAvailable, h => h.JE_MessageTypeInfo, h => h.JE_TransportModeInfo, h => h.JE_DispatchModalityInfo);
			builder.SetVisibility(commonBag.ShipmentDetailsOriginUserControl, h => !(h.IsImportLicense || h.IsLPCO), h => h.JE_MessageTypeInfo);
			builder.SetVisibility(commonBag.ShipmentDetailsFinalDestinationUserControl, h => !(h.IsImportLicense || h.IsLPCO), h => h.JE_MessageTypeInfo);
			builder.SetVisibility(commonBag.WeightCalcDropEdit, h => !(h.IsImportLicense || h.IsLPCO), h => h.JE_MessageTypeInfo);
			builder.SetVisibility(commonBag.VolumeCalcDropEdit, h => !(h.IsImportLicense || h.IsLPCO), h => h.JE_MessageTypeInfo);
			builder.SetVisibility(commonBag.TotalNoOfPiecesCalcEdit, h => !(h.IsImportLicense || h.IsLPCO), h => h.JE_MessageTypeInfo);
			builder.SetVisibility(commonBag.ContainerCountCalcEdit, h => !(h.IsImportLicense || h.IsLPCO), h => h.JE_MessageTypeInfo);
			builder.SetVisibility(commonBag.TotalNoOfPacksCalcDropEdit, h => !(h.IsImportLicense || h.IsLPCO), h => h.JE_MessageTypeInfo);
			builder.SetVisibility(commonBag.ShipmentDetailsIncoTermsUserControl, h => !(h.IsImportLicense || h.IsLPCO), h => h.JE_MessageTypeInfo);
			builder.SetVisibility(commonBag.ShipmentDetailsScreeningUserControl, h => !h.IsImportLicense, h => h.JE_MessageTypeInfo);
			builder.SetVisibility(commonBag.GoodsDescriptionTextBox, h => !h.IsLPCO, h => h.JE_MessageTypeInfo);
			builder.SetVisibility(commonBag.OwnersReferenceTextBox, h => !h.IsLPCO, h => h.JE_MessageTypeInfo);
			builder.SetVisibility(brBag.CargoArrivalUserControl, h => h.IsCargoArrivalDocumentApplicable, h => h.JE_MessageTypeInfo, h => h.JE_TransportModeInfo);

			builder.SetCaption(brBag.UcrAndBillTypeUserControl, h => h.UCRCaption, h => h.JE_TransportModeInfo);
			builder.SetCaption(commonBag.UCRTextBox, h => h.UCRCaption, h => h.JE_TransportModeInfo);

			return builder.Build();
		}
	}
}
