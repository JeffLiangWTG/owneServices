using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	public class EUICS2ManifestLayouts : IPanelLayoutProvider
	{
		public PanelLayout ManifestDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => ManifestDetails;

		public EUICS2ManifestLayouts()
		{
			ManifestDetails = CreateManifestDetailsLayout();
		}

		PanelLayout CreateManifestDetailsLayout()
		{
			var builder = new ManifestLayoutBuilder<AsycudaManifestHeader>();
			var common = builder.CommonBag;
			var controlBag = EUICS2ManifestControlBag.Instance;
			builder.AddControlBag(controlBag);

			builder.AddColumn();

			builder.Add(controlBag.AddressedMemberStateDropEdit, ControlWidthClass.Medium);
			builder.Add(controlBag.BranchGuidFindBox, ControlWidthClass.Medium);
			builder.Add(common.RegistrationDateEdit, ControlWidthClass.Medium);
			builder.Add(controlBag.RegistrationNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.TransportModeDropEdit, ControlWidthClass.Long);
			builder.Add(common.ManifestTypeDropEdit, ControlWidthClass.Long);
			builder.Add(controlBag.SpecificCircumstanceIndicatorDropEdit, ControlWidthClass.Long);
			builder.Add(common.NatureDropEdit, ControlWidthClass.Long);
			builder.Add(common.AgentTypeDropEdit, ControlWidthClass.Long);
			builder.Add(common.ContainerModeDropEdit, ControlWidthClass.Long);
			builder.Add(common.BuyersConsolidationCheckBox, ControlWidthClass.Long);
			builder.Add(common.VesselCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.VoyageFlightTextBox, ControlWidthClass.Medium);
			builder.Add(controlBag.MOTIdentifierTypeDropEdit, ControlWidthClass.Long);
			builder.Add(controlBag.MOTIdentifierTextBox, ControlWidthClass.Long);
			builder.Add(common.LloydsNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.RadioCallSignTextBox, ControlWidthClass.Medium);
			builder.Add(common.ConveyanceCountryCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.MastersNameTextBox, ControlWidthClass.Long);
			builder.Add(controlBag.MeansOfTransportTypeDropEdit, ControlWidthClass.Long);
			builder.Add(common.VehicleRegistrationTextBox, ControlWidthClass.Medium);
			builder.Add(controlBag.VehicleRegistrationAndNationalityUserControl, ControlWidthClass.Long);
			builder.Add(common.Trailer1RegNoTextBox, ControlWidthClass.Medium);
			builder.Add(common.Trailer1RegCountryCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.Trailer2RegNoTextBox, ControlWidthClass.Medium);
			builder.Add(common.Trailer2RegCountryCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.PortOfLoadingCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.EstDepartureDateEdit, ControlWidthClass.Auto);
			builder.Add(controlBag.ActualDepartureDateEdit, ControlWidthClass.Auto);
			builder.Add(common.PortOfFirstArrivalCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.PortOfDischargeCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.EstArrivalDateEdit, ControlWidthClass.Auto);
			builder.Add(common.ActArrivalDateEdit, ControlWidthClass.Auto);

			builder.AddColumn();

			builder.Add(common.JobReferenceTextBox, ControlWidthClass.Medium);
			builder.Add(controlBag.LocalReferenceNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.MessageStatusTextBox, ControlWidthClass.Medium);
			builder.Add(common.MessageStatusDropEdit, ControlWidthClass.Long);
			builder.Add(common.CustomsStatusDropEdit, ControlWidthClass.Long);
			builder.Add(common.ManifestNumberFromMasterBillTextBox, ControlWidthClass.Long);
			builder.Add(common.MasterBOLTextBox, ControlWidthClass.Medium);
			builder.Add(common.IssueDateDateEdit, ControlWidthClass.Auto);
			builder.Add(common.PaymentMethodDropEdit, ControlWidthClass.Long);
			builder.Add(controlBag.TransportDocumentTypeDropEdit, ControlWidthClass.Long);
			builder.Add(common.DeclarantAddressControl, ControlWidthClass.Long);
			builder.Add(common.CarrierAddressControl, ControlWidthClass.Long);
			builder.Add(common.CarrierCodeTextBox, ControlWidthClass.Medium);
			builder.Add(common.ShippingAgentAddressControl, ControlWidthClass.Long);
			builder.Add(common.CustomsOfficeCodeFindBox, ControlWidthClass.Long);
			builder.Add(controlBag.ReEntryIndicatorCheckBox, ControlWidthClass.Long);
			builder.Add(controlBag.SplitConsignmentIndicatorCheckBox, ControlWidthClass.Long);
			builder.Add(controlBag.PreviousMRNTextBox, ControlWidthClass.Long);
			builder.Add(controlBag.OriginCodeFindBox, ControlWidthClass.Long);
			builder.Add(controlBag.FinalDestinationCodeFindBox, ControlWidthClass.Long);
			builder.Add(controlBag.ReceptacleUserControl, ControlWidthClass.Long);

			builder.SetVisibility(common.VesselCodeFindBox, header => header.IsInlandWaterway || header.IsSea);
			builder.SetVisibility(common.LloydsNumberTextBox, header => header.IsInlandWaterway || header.IsSea);
			builder.SetVisibility(common.ConveyanceCountryCodeFindBox, header => header.IsInlandWaterway || header.IsSea);
			builder.SetVisibility(common.MastersNameTextBox, header => header.IsInlandWaterway || header.IsSea);
			builder.SetVisibility(controlBag.BranchGuidFindBox, header => header.IsForwarderManifest);
			builder.SetVisibility(controlBag.MOTIdentifierTextBox, header => !header.IsForwarderManifest);
			builder.SetVisibility(controlBag.MOTIdentifierTypeDropEdit, header => !header.IsForwarderManifest);
			builder.SetVisibility(controlBag.ActualDepartureDateEdit, header => header.AMA_TransportMode == TransportModes.Road && (header.SpecificCircumstanceIndicator == EUICS2SpecificCircumstanceList.Codes.F50 || header.IsCarrierManifest), header => header.AMA_TransportModeInfo, header => header.SpecificCircumstanceIndicatorInfo);
			builder.SetVisibility(controlBag.MeansOfTransportTypeDropEdit, header => header.IsCarrierManifest && header.AMA_TransportMode == TransportTypeList.Codes.Rail || header.AMA_TransportMode
				.In(new ZString[]
				{
					TransportTypeList.Codes.Road,
					TransportTypeList.Codes.Sea,
					TransportTypeList.Codes.InlandWaterwayTransport,
				}));
			builder.SetVisibility(common.RadioCallSignTextBox, header => !header.IsForwarderManifest && header.AMA_TransportMode == TransportTypeList.Codes.Sea);
			builder.SetVisibility(common.VehicleRegistrationTextBox, header => !header.IsForwarderManifest && header.AMA_TransportMode != TransportTypeList.Codes.Road);
			builder.SetVisibility(controlBag.VehicleRegistrationAndNationalityUserControl, header => header.AMA_TransportMode == TransportTypeList.Codes.Road);
			builder.SetVisibility(common.Trailer1RegNoTextBox, header => !header.IsForwarderManifest && header.AMA_TransportMode != TransportTypeList.Codes.Road);
			builder.SetVisibility(common.Trailer1RegCountryCodeFindBox, header => !header.IsForwarderManifest && header.AMA_TransportMode != TransportTypeList.Codes.Road);
			builder.SetVisibility(common.Trailer2RegNoTextBox, header => !header.IsForwarderManifest && header.AMA_TransportMode != TransportTypeList.Codes.Road);
			builder.SetVisibility(common.Trailer2RegCountryCodeFindBox, header => !header.IsForwarderManifest && header.AMA_TransportMode != TransportTypeList.Codes.Road);
			builder.SetVisibility(common.PaymentMethodDropEdit, header => header.IsCarrierManifest && (header.IsRoad || header.IsRail) && (header.SpecificCircumstanceIndicator == EUICS2SpecificCircumstanceList.Codes.F50 || header.SpecificCircumstanceIndicator == EUICS2SpecificCircumstanceList.Codes.F51), header => header.SpecificCircumstanceIndicatorInfo);
			builder.SetVisibility(controlBag.ReceptacleUserControl, header => header.IsCarrierManifest);
			builder.SetVisibility(controlBag.SplitConsignmentIndicatorCheckBox, header => header.IsCarrierManifest && header.AMA_TransportMode
				.In(new ZString[]
				{
					TransportTypeList.Codes.Sea,
					TransportTypeList.Codes.InlandWaterwayTransport,
				}));

			builder.AddControlBehaviour<ZDateEdit>(
				controlReference: common.EstDepartureDateEdit,
				behaviourName: "SetEstDepartureDateEditFormatBehaviour",
				updateControlBehaviourAction: (c, _) => c.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Long,
				dependencies: [
					h => h.AMA_ApplicationCodeInfo,
					h => h.AMA_TransportModeInfo
				]
			);

			builder.SetVisibility(
				controlReference: controlBag.OriginCodeFindBox,
				isVisible: header => header.IsInlandWaterwayOrSeaCarrierManifestWithF11OrF12Indicator,
				dependencies: [
					header => header.AMA_TransportModeInfo,
					header => header.SpecificCircumstanceIndicatorInfo
				]
			);
			builder.SetVisibility(
				controlReference: controlBag.FinalDestinationCodeFindBox,
				isVisible: header => header.IsInlandWaterwayOrSeaCarrierManifestWithF11OrF12Indicator,
				dependencies: [
					header => header.AMA_TransportModeInfo,
					header => header.SpecificCircumstanceIndicatorInfo
				]
			);

			return builder.Build();
		}
	}
}
