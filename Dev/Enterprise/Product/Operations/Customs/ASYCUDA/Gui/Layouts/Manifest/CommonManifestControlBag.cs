using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public class CommonManifestControlBag : ControlBag
	{
		public static CommonManifestControlBag Instance => commonManifestControlBag.Value;

		CommonManifestControlBag()
		{
			CountryTextBox = RegisterControl(nameof(CountryTextBox));
			JobReferenceTextBox = RegisterControl(nameof(JobReferenceTextBox));
			RegistrationDateEdit = RegisterControl(nameof(RegistrationDateEdit));
			RegistrationNumberTextBox = RegisterControl(nameof(RegistrationNumberTextBox));
			MessageStatusTextBox = RegisterControl(nameof(MessageStatusTextBox));
			MessageStatusDropEdit = RegisterControl(nameof(MessageStatusDropEdit));
			CustomsStatusDropEdit = RegisterControl(nameof(CustomsStatusDropEdit));
			RegistrationDateTimeDateEdit = RegisterControl(nameof(RegistrationDateTimeDateEdit));
			ConveyanceCountryCodeFindBox = RegisterControl(nameof(CommonManifestUserControl.ConveyanceCountryCodeFindBox));
			AgentTypeDropEdit = RegisterControl(nameof(CommonManifestUserControl.AgentTypeDropEdit));
			VoyageFlightTextBox = RegisterControl(nameof(CommonManifestUserControl.VoyageFlightTextBox));
			MastersNameTextBox = RegisterControl(nameof(CommonManifestUserControl.MastersNameTextBox));
			VehicleRegistrationTextBox = RegisterControl(nameof(CommonManifestUserControl.VehicleRegistrationTextBox));
			ManifestNumberFromMasterBillTextBox = RegisterControl(nameof(CommonManifestUserControl.ManifestNumberFromMasterBillTextBox));
			ManifestNumberTextBox = RegisterControl(nameof(CommonManifestUserControl.ManifestNumberTextBox));
			VesselCodeFindBox = RegisterControl(nameof(CommonManifestUserControl.VesselCodeFindBox));
			VesselNameTextBox = RegisterControl(nameof(CommonManifestUserControl.VesselNameTextBox));
			IssueDateDateEdit = RegisterControl(nameof(CommonManifestUserControl.IssueDateDateEdit));
			EstDepartureDateEdit = RegisterControl(nameof(CommonManifestUserControl.EstDepartureDateEdit));
			TransportModeDropEdit = RegisterControl(nameof(CommonManifestUserControl.TransportModeDropEdit));
			ContainerModeDropEdit = RegisterControl(nameof(CommonManifestUserControl.ContainerModeDropEdit));
			PortOfLoadingCodeFindBox = RegisterControl(nameof(CommonManifestUserControl.PortOfLoadingCodeFindBox));
			PortOfDischargeCodeFindBox = RegisterControl(nameof(CommonManifestUserControl.PortOfDischargeCodeFindBox));
			BuyersConsolidationCheckBox = RegisterControl(nameof(CommonManifestUserControl.BuyersConsolidationCheckBox));
			CarrierAddressControl = RegisterControl(nameof(CommonManifestUserControl.CarrierAddressControl));
			MasterBOLTextBox = RegisterControl(nameof(CommonManifestUserControl.MasterBOLTextBox));
			RadioCallSignTextBox = RegisterControl(nameof(CommonManifestUserControl.RadioCallSignTextBox));
			Trailer1RegNoTextBox = RegisterControl(nameof(CommonManifestUserControl.Trailer1RegNoTextBox));
			Trailer2RegNoTextBox = RegisterControl(nameof(CommonManifestUserControl.Trailer2RegNoTextBox));
			Trailer1RegCountryCodeFindBox = RegisterControl(nameof(CommonManifestUserControl.Trailer1RegCountryCodeFindBox));
			Trailer2RegCountryCodeFindBox = RegisterControl(nameof(CommonManifestUserControl.Trailer2RegCountryCodeFindBox));
			DischargeTerminalAddressControl = RegisterControl(nameof(CommonManifestUserControl.DischargeTerminalAddressControl));
			DeconsolidateAddressControl = RegisterControl(nameof(CommonManifestUserControl.DeconsolidateAddressControl));
			CustomsOfficeDropEdit = RegisterControl(nameof(CommonManifestUserControl.CustomsOfficeDropEdit));
			CustomsOfficeCodeFindBox = RegisterControl(nameof(CommonManifestUserControl.CustomsOfficeCodeFindBox));
			ManifestTypeDropEdit = RegisterControl(nameof(CommonManifestUserControl.ManifestTypeDropEdit));
			PortOfFirstArrivalCodeFindBox = RegisterControl(nameof(CommonManifestUserControl.PortOfFirstArrivalCodeFindBox));
			ShippingAgentAddressControl = RegisterControl(nameof(CommonManifestUserControl.ShippingAgentAddressControl));
			CarrierCodeTextBox = RegisterControl(nameof(CommonManifestUserControl.CarrierCodeTextBox));
			NatureDropEdit = RegisterControl(nameof(CommonManifestUserControl.NatureDropEdit));
			EstArrivalDateEdit = RegisterControl(nameof(CommonManifestUserControl.EstArrivalDateEdit));
			ShortEstArrivalDateEdit = RegisterControl(nameof(CommonManifestUserControl.ShortEstArrivalDateEdit));
			CustomsOriginPortCodeFindBox = RegisterControl(nameof(CommonManifestUserControl.CustomsOriginPortCodeFindBox));
			CustomsLoadPortCodeFindBox = RegisterControl(nameof(CommonManifestUserControl.CustomsLoadPortCodeFindBox));
			CustomsDischargePortCodeFindBox = RegisterControl(nameof(CommonManifestUserControl.CustomsDischargePortCodeFindBox));
			LloydsNumberTextBox = RegisterControl(nameof(CommonManifestUserControl.LloydsNumberTextBox));
			DateAtCustomsOfficeDateEdit = RegisterControl(nameof(CommonManifestUserControl.DateAtCustomsOfficeDateEdit));
			GoodsLocationCodeFindBox = RegisterControl(nameof(CommonManifestUserControl.GoodsLocationCodeFindBox));
			MasterBillTextBox = RegisterControl(nameof(CommonManifestUserControl.MasterBillTextBox));
			CarrierReferenceTextBox = RegisterControl(nameof(CommonManifestUserControl.CarrierReferenceTextBox));
			PaymentMethodDropEdit = RegisterControl(nameof(CommonManifestUserControl.PaymentMethodDropEdit));
			ETADateEdit = RegisterControl(nameof(CommonManifestUserControl.ETADateEdit));
			GuaranteeTextBox = RegisterControl(nameof(CommonManifestUserControl.GuaranteeTextBox));
			ManifestTypeShortCodeLengthDropEdit = RegisterControl(nameof(CommonManifestUserControl.ManifestTypeShortCodeLengthDropEdit));
			CustomsOfficeShortCodeLengthDropEdit = RegisterControl(nameof(CommonManifestUserControl.CustomsOfficeShortCodeLengthDropEdit));
			ActArrivalDateEdit = RegisterControl(nameof(CommonManifestUserControl.ActArrivalDateEdit));
			RecipientReferenceDropEdit = RegisterControl(nameof(CommonManifestUserControl.RecipientReferenceDropEdit));
			CustomsProfileDropEdit = RegisterControl(nameof(CommonManifestUserControl.CustomsProfileDropEdit));
			BranchGuidFindBox = RegisterControl(nameof(CommonManifestUserControl.BranchGuidFindBox));
			DeclarantAddressControl = RegisterControl(nameof(CommonManifestUserControl.DeclarantAddressControl));
			RepresentativeAddressControl = RegisterControl(nameof(CommonManifestUserControl.RepresentativeAddressControl));
			SpecialCargoCodeDropEdit = RegisterControl(nameof(CommonManifestUserControl.SpecialCargoCodeDropEdit));
			OriginCodeFindBox = RegisterControl(nameof(CommonManifestUserControl.OriginCodeFindBox));
			DestinationCodeFindBox = RegisterControl(nameof(CommonManifestUserControl.DestinationCodeFindBox));
			GoodsDescriptionTextBox = RegisterControl(nameof(CommonManifestUserControl.GoodsDescriptionTextBox));
			RegistrationYearEdit = RegisterControl(nameof(CommonManifestUserControl.RegistrationYearEdit));
		}

		public ControlReference CountryTextBox { get; }
		public ControlReference JobReferenceTextBox { get; }
		public ControlReference RegistrationDateEdit { get; }
		public ControlReference RegistrationNumberTextBox { get; }
		public ControlReference MessageStatusTextBox { get; }
		public ControlReference MessageStatusDropEdit { get; }
		public ControlReference CustomsStatusDropEdit { get; }
		public ControlReference RegistrationDateTimeDateEdit { get; }
		public ControlReference ConveyanceCountryCodeFindBox { get; }
		public ControlReference AgentTypeDropEdit { get; }
		public ControlReference VoyageFlightTextBox { get; }
		public ControlReference MastersNameTextBox { get; }
		public ControlReference VehicleRegistrationTextBox { get; }
		public ControlReference ManifestNumberFromMasterBillTextBox { get; }
		public ControlReference ManifestNumberTextBox { get; }
		public ControlReference VesselCodeFindBox { get; }
		public ControlReference VesselNameTextBox { get; }
		public ControlReference IssueDateDateEdit { get; }
		public ControlReference EstDepartureDateEdit { get; }
		public ControlReference TransportModeDropEdit { get; }
		public ControlReference ContainerModeDropEdit { get; }
		public ControlReference PortOfLoadingCodeFindBox { get; }
		public ControlReference PortOfDischargeCodeFindBox { get; }
		public ControlReference BuyersConsolidationCheckBox { get; }
		public ControlReference CarrierAddressControl { get; }
		public ControlReference MasterBOLTextBox { get; }
		public ControlReference RadioCallSignTextBox { get; }
		public ControlReference Trailer1RegNoTextBox { get; }
		public ControlReference Trailer2RegNoTextBox { get; }
		public ControlReference Trailer1RegCountryCodeFindBox { get; }
		public ControlReference Trailer2RegCountryCodeFindBox { get; }
		public ControlReference DischargeTerminalAddressControl { get; }
		public ControlReference DeconsolidateAddressControl { get; }
		public ControlReference CustomsOfficeDropEdit { get; }
		public ControlReference CustomsOfficeCodeFindBox { get; }
		public ControlReference ManifestTypeDropEdit { get; }
		public ControlReference PortOfFirstArrivalCodeFindBox { get; }
		public ControlReference ShippingAgentAddressControl { get; }
		public ControlReference CarrierCodeTextBox { get; }
		public ControlReference NatureDropEdit { get; }
		public ControlReference EstArrivalDateEdit { get; }
		public ControlReference ShortEstArrivalDateEdit { get; }
		public ControlReference CustomsOriginPortCodeFindBox { get; }
		public ControlReference CustomsLoadPortCodeFindBox { get; }
		public ControlReference CustomsDischargePortCodeFindBox { get; }
		public ControlReference LloydsNumberTextBox { get; }
		public ControlReference DateAtCustomsOfficeDateEdit { get; }
		public ControlReference GoodsLocationCodeFindBox { get; }
		public ControlReference MasterBillTextBox { get; }
		public ControlReference CarrierReferenceTextBox { get; }
		public ControlReference PaymentMethodDropEdit { get; }
		public ControlReference ETADateEdit { get; }
		public ControlReference GuaranteeTextBox { get; }
		public ControlReference ManifestTypeShortCodeLengthDropEdit { get; }
		public ControlReference CustomsOfficeShortCodeLengthDropEdit { get; }
		public ControlReference ActArrivalDateEdit { get; }
		public ControlReference RecipientReferenceDropEdit { get; }
		public ControlReference CustomsProfileDropEdit { get; }
		public ControlReference BranchGuidFindBox { get; }
		public ControlReference DeclarantAddressControl { get; }
		public ControlReference RepresentativeAddressControl { get; }
		public ControlReference SpecialCargoCodeDropEdit { get; }
		public ControlReference OriginCodeFindBox { get; }
		public ControlReference DestinationCodeFindBox { get; }
		public ControlReference GoodsDescriptionTextBox { get; }
		public ControlReference RegistrationYearEdit { get; }

		protected override Control CreateTemplate() => new CommonManifestUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<CommonManifestControlBag> commonManifestControlBag = new Lazy<CommonManifestControlBag>(() => new CommonManifestControlBag());
	}
}
