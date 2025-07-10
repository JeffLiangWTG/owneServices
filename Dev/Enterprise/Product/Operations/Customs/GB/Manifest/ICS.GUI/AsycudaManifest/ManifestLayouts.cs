using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.Manifest.GUI;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.ICS.GUI
{
	public sealed class ManifestLayouts : IPanelLayoutProvider
	{
		PanelLayout ManifestDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => ManifestDetails;

		public ManifestLayouts()
		{
			ManifestDetails = CreateManifestDetailsLayout();
		}

		PanelLayout CreateManifestDetailsLayout()
		{
			var builder = new ManifestLayoutBuilder<AsycudaManifestHeaderBase>();
			var common = builder.CommonBag;
			var eu = EUManifestControlBag.Instance;
			builder.AddControlBag(eu);

			builder.AddColumn();

			builder.Add(common.CountryTextBox, ControlWidthClass.Medium);
			builder.Add(common.RegistrationDateEdit, ControlWidthClass.Medium);
			builder.Add(common.RegistrationNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.TransportModeDropEdit, ControlWidthClass.Long);
			builder.Add(common.ManifestTypeDropEdit, ControlWidthClass.Long);
			builder.Add(common.NatureDropEdit, ControlWidthClass.Long);
			builder.Add(common.AgentTypeDropEdit, ControlWidthClass.Long);
			builder.Add(common.ContainerModeDropEdit, ControlWidthClass.Long);
			builder.Add(common.BuyersConsolidationCheckBox, ControlWidthClass.Long);
			builder.Add(common.VesselCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.VoyageFlightTextBox, ControlWidthClass.Medium);
			builder.Add(common.LloydsNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.RadioCallSignTextBox, ControlWidthClass.Medium);
			builder.Add(common.ConveyanceCountryCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.MastersNameTextBox, ControlWidthClass.Long);
			builder.Add(common.VehicleRegistrationTextBox, ControlWidthClass.Medium);
			builder.Add(common.Trailer1RegNoTextBox, ControlWidthClass.Medium);
			builder.Add(common.Trailer1RegCountryCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.Trailer2RegNoTextBox, ControlWidthClass.Medium);
			builder.Add(common.Trailer2RegCountryCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.PortOfLoadingCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.EstDepartureDateEdit, ControlWidthClass.Medium);
			builder.Add(common.PortOfFirstArrivalCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.PortOfDischargeCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.EstArrivalDateEdit, ControlWidthClass.Auto);

			builder.AddColumn();

			builder.Add(common.JobReferenceTextBox, ControlWidthClass.Long);
			builder.Add(common.MessageStatusTextBox, ControlWidthClass.Medium);
			builder.Add(common.MessageStatusDropEdit, ControlWidthClass.Long);
			builder.Add(common.CustomsStatusDropEdit, ControlWidthClass.Long);
			builder.Add(common.ManifestNumberFromMasterBillTextBox, ControlWidthClass.Long);
			builder.Add(common.MasterBOLTextBox, ControlWidthClass.Medium);
			builder.Add(common.IssueDateDateEdit, ControlWidthClass.Auto);
			builder.Add(common.CarrierAddressControl, ControlWidthClass.Long);
			builder.Add(common.CarrierCodeTextBox, ControlWidthClass.Medium);
			builder.Add(common.ShippingAgentAddressControl, ControlWidthClass.Long);
			builder.Add(common.CustomsOfficeDropEdit, ControlWidthClass.Long);

			builder.Add(eu.SpecificCircumstanceIndicatorDropEdit, ControlWidthClass.Long);
			builder.Add(eu.MethodOfPaymentDropEdit, ControlWidthClass.Long);
			builder.Add(eu.ETAatFirstCustomsOfficeDateEdit, ControlWidthClass.Auto);
			builder.Add(eu.ATAatFirstCustomsOfficeDateEdit, ControlWidthClass.Auto);
			builder.Add(eu.EUCustomsOfficesUserControl, ControlWidthClass.LongNoCaption);

			return builder.Build();
		}
	}
}
