using Enterprise.Customs.IE.PBN.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.PBN.GUI
{
	public class PBNManifestLayout : IPanelLayoutProvider
	{
		PanelLayout ManifestDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => ManifestDetails;

		public PBNManifestLayout()
		{
			ManifestDetails = CreateManifestDetailsLayout();
		}

		PanelLayout CreateManifestDetailsLayout()
		{
			var builder = new PBNManifestLayoutBuilder<AsycudaManifestHeader>();
			var common = builder.CommonBag;
			var bag = PBNManifestControlBag.Instance;

			builder.AddControlBag(bag);

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
			builder.Add(bag.IsEmptyVehicleCheckBox, ControlWidthClass.Medium);
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
			builder.Add(common.CustomsOfficeDropEdit, ControlWidthClass.Long);

			builder.AddColumn();

			builder.Add(common.JobReferenceTextBox, ControlWidthClass.Medium);
			builder.Add(common.MessageStatusDropEdit, ControlWidthClass.Long);
			builder.Add(common.CustomsStatusDropEdit, ControlWidthClass.Long);
			builder.Add(common.CarrierAddressControl, ControlWidthClass.Long);
			builder.Add(bag.CarrierCodeDropEdit, ControlWidthClass.Long);
			builder.Add(bag.CustomsReferencesGroupBox, ControlWidthClass.LongNoCaption);
			builder.Add(bag.TransitReferencesGroupBox, ControlWidthClass.LongNoCaption);

			builder.AddColumn();
			builder.Add(bag.PersonsGroupBox, ControlWidthClass.LongNoCaption);

			return builder.Build();
		}
	}
}
