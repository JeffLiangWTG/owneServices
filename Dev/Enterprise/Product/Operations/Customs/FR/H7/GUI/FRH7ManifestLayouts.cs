using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.EU.H7.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.H7.GUI
{
	public class FRH7ManifestLayouts : IPanelLayoutProvider
	{
		public FRH7ManifestLayouts()
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
			builder.Add(common.VoyageFlightTextBox, ControlWidthClass.Medium);
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
			builder.Add(headerControlBag.CustomsOfficeLabel, ControlWidthClass.LongNoCaption);
			builder.Add(headerControlBag.CustomsOfficeCodeFindBox, ControlWidthClass.Long);
			builder.Add(headerControlBag.PresentationOfficeCodeFindBox, ControlWidthClass.Long);
			builder.Add(headerControlBag.ConsolidatedStatusSeparatorUserControl, ControlWidthClass.Long);
			builder.Add(headerControlBag.ConsolidatedCustomsStatusDropEdit, ControlWidthClass.Long);

			builder.SetCaption(common.VoyageFlightTextBox, _ => Res.GetData("4c2d4a5e-9ce4-43ff-a806-bff12e187b59", "Flight/Journey/Voyage"));

			return builder.Build();
		}
	}
}
