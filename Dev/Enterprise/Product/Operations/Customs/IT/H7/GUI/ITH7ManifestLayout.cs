using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.EU.H7.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.H7.GUI;

public class ITH7ManifestLayout : IPanelLayoutProvider
{
	public ITH7ManifestLayout()
	{
		ManifestDetails = CreateManifestDetailsLayout();
	}

	public PanelLayout ManifestDetails { get; }

	PanelLayout IPanelLayoutProvider.Layout => ManifestDetails;

	PanelLayout CreateManifestDetailsLayout()
	{
		var builder = new ManifestLayoutBuilder<AsycudaManifestHeader>();
		var common = builder.CommonBag;
		var headerControlBag = EUH7ManifestControlBag.Instance;
		builder.AddControlBag(headerControlBag);

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

		builder.Add(common.MasterBillTextBox, ControlWidthClass.Long);
		builder.Add(common.DeclarantAddressControl, ControlWidthClass.Long);
		builder.Add(common.RepresentativeAddressControl, ControlWidthClass.Long);
		builder.Add(common.AgentTypeDropEdit, ControlWidthClass.Long);
		builder.Add(common.PaymentMethodDropEdit, ControlWidthClass.Long);
		builder.Add(common.GuaranteeTextBox, ControlWidthClass.Long);
		builder.Add(headerControlBag.CustomsOfficeLabel, ControlWidthClass.LongNoCaption);
		builder.Add(headerControlBag.PresentationOfficeCodeFindBox, ControlWidthClass.Long);
		builder.Add(headerControlBag.ConsolidatedCustomsStatusDropEdit, ControlWidthClass.Long);

		builder.SetCaption(common.GuaranteeTextBox, _ => Res.GetData("d8a8886b-0294-479b-879b-ea2b84a2fb89", "Account Number"));

		return builder.Build();
	}
}
