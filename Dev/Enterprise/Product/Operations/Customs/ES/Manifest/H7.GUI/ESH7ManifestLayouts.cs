using CargoWise.Application;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.EU.H7.GUI;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.Manifest.H7.GUI
{
	public class ESH7ManifestLayouts : IPanelLayoutProvider
	{
		public ESH7ManifestLayouts()
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
			var esH7ManifestControlBag = ESH7ManifestControlBag.Instance;

			builder.AddControlBag(euH7ManifestControlBag);
			builder.AddControlBag(esH7ManifestControlBag);

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
			builder.Add(common.RepresentativeAddressControl, ControlWidthClass.Long);
			builder.Add(euH7ManifestControlBag.PresenterAddressControl, ControlWidthClass.Long);
			builder.Add(common.AgentTypeDropEdit, ControlWidthClass.Long);
			builder.Add(common.MastersNameTextBox, ControlWidthClass.Long);
			builder.Add(esH7ManifestControlBag.EntryLineNumberTextBox, ControlWidthClass.Long);
			builder.Add(esH7ManifestControlBag.TransportDocumentReferenceTextBox, ControlWidthClass.Long);
			builder.Add(esH7ManifestControlBag.TransportDocumentTypeDropEdit, ControlWidthClass.Long);
			builder.Add(esH7ManifestControlBag.LocationOfGoodsUserControl, ControlWidthClass.Long);
			builder.Add(esH7ManifestControlBag.G3MRNToRevokeDropEdit, ControlWidthClass.Long);
			builder.Add(euH7ManifestControlBag.CustomsOfficeLabel, ControlWidthClass.LongNoCaption);
			builder.Add(euH7ManifestControlBag.CustomsOfficeCodeFindBox, ControlWidthClass.Long);
			builder.Add(euH7ManifestControlBag.PresentationOfficeCodeFindBox, ControlWidthClass.Long);
			builder.Add(esH7ManifestControlBag.CusAgentCodeFindBox, ControlWidthClass.Long);
			builder.Add(esH7ManifestControlBag.CertificateDropEdit, ControlWidthClass.Long);
			builder.Add(esH7ManifestControlBag.TrainingCheckBox, ControlWidthClass.Long);
			builder.Add(euH7ManifestControlBag.ConsolidatedStatusSeparatorUserControl, ControlWidthClass.Long);
			builder.Add(euH7ManifestControlBag.ConsolidatedCustomsStatusDropEdit, ControlWidthClass.Long);

			builder.SetVisibility(common.MastersNameTextBox, h => true, h => h.AMA_TransportModeInfo);
			builder.SetVisibility(esH7ManifestControlBag.TrainingCheckBox, d => ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalSystem());

			return builder.Build();
		}
	}
}
