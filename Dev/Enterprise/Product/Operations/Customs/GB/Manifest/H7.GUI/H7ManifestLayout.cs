using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.H7.GUI;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.GB.H7.GUI
{
	public class H7ManifestLayout : IPanelLayoutProvider
	{
		public H7ManifestLayout()
		{
			ManifestDetails = CreateManifestDetailsLayout();
		}

		public PanelLayout ManifestDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => ManifestDetails;

		PanelLayout CreateManifestDetailsLayout()
		{
			var builder = new ManifestLayoutBuilder<AsycudaManifestHeader>();
			var common = builder.CommonBag;

			var h7ManifestControlBag = H7ManifestControlBag.Instance;
			var euH7ManifestControlBag = EUH7ManifestControlBag.Instance;

			builder.AddControlBag(h7ManifestControlBag);
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
			builder.Add(common.DeclarantAddressControl, ControlWidthClass.Long);
			builder.Add(h7ManifestControlBag.SupervisingOfficeAddressControl, ControlWidthClass.Long);
			builder.Add(common.RepresentativeAddressControl, ControlWidthClass.Long);
			builder.Add(common.AgentTypeDropEdit, ControlWidthClass.Long);
			builder.Add(common.CustomsProfileDropEdit, ControlWidthClass.Medium);
			builder.Add(h7ManifestControlBag.CSPDropEdit, ControlWidthClass.Long);
			builder.Add(euH7ManifestControlBag.ConsolidatedStatusSeparatorUserControl, ControlWidthClass.Long);
			builder.Add(euH7ManifestControlBag.ConsolidatedCustomsStatusDropEdit, ControlWidthClass.Long);

			builder.SetVisibility(common.VoyageFlightTextBox, h => h.AMA_TransportMode != TransportModes.Mail, h => h.AMA_TransportModeInfo);

			return builder.Build();
		}
	}
}
