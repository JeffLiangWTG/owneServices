using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.EU.H7.GUI;
using Enterprise.Customs.IE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.H7.GUI
{
	public class IEH7ManifestLayouts : IPanelLayoutProvider
	{
		public IEH7ManifestLayouts()
		{
			ManifestDetails = CreateManifestDetailsLayout();
		}

		public PanelLayout ManifestDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => ManifestDetails;

		PanelLayout CreateManifestDetailsLayout()
		{
			var builder = new ManifestLayoutBuilder<AsycudaManifestHeader>();
			var common = builder.CommonBag;
			var manifestBag = EUH7ManifestControlBag.Instance;
			builder.AddControlBag(manifestBag);

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
			builder.Add(common.AgentTypeDropEdit, ControlWidthClass.Long);
			builder.Add(common.PaymentMethodDropEdit, ControlWidthClass.Long);
			builder.Add(common.GuaranteeTextBox, ControlWidthClass.Long);
			builder.Add(manifestBag.SubmitTypeDropEdit, ControlWidthClass.Long);
			builder.Add(manifestBag.CustomsOfficeLabel, ControlWidthClass.LongNoCaption);
			builder.Add(manifestBag.CustomsOfficeCodeFindBox, ControlWidthClass.Long);
			builder.Add(manifestBag.PresentationOfficeCodeFindBox, ControlWidthClass.Long);
			builder.Add(manifestBag.ConsolidatedStatusSeparatorUserControl, ControlWidthClass.Long);
			builder.Add(manifestBag.ConsolidatedCustomsStatusDropEdit, ControlWidthClass.Long);

			builder.SetVisibility(common.GuaranteeTextBox, d => d.AMA_PaymentMethod == PaymentMethodList.Codes.E, d => d.AMA_PaymentMethodInfo);

			return builder.Build();
		}
	}
}
