using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public class EUH7ManifestLayouts : IPanelLayoutProvider
	{
		public EUH7ManifestLayouts()
		{
			ManifestDetails = CreateManifestDetailsLayout();
		}

		public PanelLayout ManifestDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => ManifestDetails;

		PanelLayout CreateManifestDetailsLayout()
		{
			var builder = new ManifestLayoutBuilder<AsycudaManifestHeader>();
			var common = builder.CommonBag;
			var euH7ManifestControlBag = EUH7ManifestControlBag.Instance;

			builder.AddControlBag(euH7ManifestControlBag);

			builder.AddColumn();

			builder.Add(common.CountryTextBox, ControlWidthClass.Long);
			builder.Add(common.BranchGuidFindBox, ControlWidthClass.Long);
			builder.Add(common.TransportModeDropEdit, ControlWidthClass.Long);
			builder.Add(common.VesselCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.VoyageFlightTextBox, ControlWidthClass.Medium);
			builder.Add(common.LloydsNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.RadioCallSignTextBox, ControlWidthClass.Medium);
			builder.Add(common.ConveyanceCountryCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.VehicleRegistrationTextBox, ControlWidthClass.Medium);
			builder.Add(common.Trailer1RegNoTextBox, ControlWidthClass.Medium);
			builder.Add(common.Trailer1RegCountryCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.Trailer2RegNoTextBox, ControlWidthClass.Medium);
			builder.Add(common.Trailer2RegCountryCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.PortOfLoadingCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.EstDepartureDateEdit, ControlWidthClass.Auto);
			builder.Add(common.PortOfFirstArrivalCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.PortOfDischargeCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.EstArrivalDateEdit, ControlWidthClass.Auto);
			builder.Add(common.ActArrivalDateEdit, ControlWidthClass.Auto);

			builder.AddColumn();

			builder.Add(common.JobReferenceTextBox, ControlWidthClass.Medium);
			builder.Add(common.MasterBillTextBox, ControlWidthClass.Long);
			builder.Add(common.CustomsOfficeDropEdit, ControlWidthClass.Long);
			builder.Add(common.DeclarantAddressControl, ControlWidthClass.Long);
			builder.Add(common.RepresentativeAddressControl, ControlWidthClass.Long);
			builder.Add(euH7ManifestControlBag.ConsolidatedStatusSeparatorUserControl, ControlWidthClass.Long);
			builder.Add(euH7ManifestControlBag.ConsolidatedCustomsStatusDropEdit, ControlWidthClass.Long);

			builder.SetCaption(common.MasterBillTextBox, h => Res.GetData("24232963-66c5-48bc-a658-d69ebe9b06cb", "Bill No.", "Bill No.", "Bill Number", "The house bill number (e.g. BOL, MAWB) used to identify the shipment associated with the declaration."));

			return builder.Build();
		}
	}
}
