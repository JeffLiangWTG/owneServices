using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.BR.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.Manifest.GUI
{
	public sealed class BRManifestLayouts : IPanelLayoutProvider
	{
		PanelLayout ManifestDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => ManifestDetails;

		public BRManifestLayouts()
		{
			ManifestDetails = CreateManifestDetailsLayout();
		}

		PanelLayout CreateManifestDetailsLayout()
		{
			var builder = new ManifestLayoutBuilder<AsycudaManifestHeader>();
			var common = builder.CommonBag;
			var br = BRManifestControlBag.Instance;
			builder.AddControlBag(br);

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
			builder.Add(common.VehicleRegistrationTextBox, ControlWidthClass.Medium);
			builder.Add(common.Trailer1RegNoTextBox, ControlWidthClass.Medium);
			builder.Add(common.Trailer2RegNoTextBox, ControlWidthClass.Medium);
			builder.Add(common.PortOfLoadingCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.EstDepartureDateEdit, ControlWidthClass.Auto);
			builder.Add(common.PortOfFirstArrivalCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.PortOfDischargeCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.EstArrivalDateEdit, ControlWidthClass.Auto);
			builder.Add(common.CustomsOfficeDropEdit, ControlWidthClass.Long);

			builder.AddColumn();

			builder.Add(common.JobReferenceTextBox, ControlWidthClass.Medium);
			builder.Add(common.MessageStatusTextBox, ControlWidthClass.Medium);
			builder.Add(common.MessageStatusDropEdit, ControlWidthClass.Long);
			builder.Add(common.CustomsStatusDropEdit, ControlWidthClass.Long);
			builder.Add(common.ManifestNumberFromMasterBillTextBox, ControlWidthClass.Long);
			builder.Add(common.MasterBOLTextBox, ControlWidthClass.Medium);
			builder.Add(br.CustomsOwnNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.IssueDateDateEdit, ControlWidthClass.Auto);
			builder.Add(common.CarrierAddressControl, ControlWidthClass.Long);
			builder.Add(common.CarrierCodeTextBox, ControlWidthClass.Medium);
			builder.Add(common.ShippingAgentAddressControl, ControlWidthClass.Long);
			builder.Add(common.DeconsolidateAddressControl, ControlWidthClass.Long);
			builder.Add(common.DischargeTerminalAddressControl, ControlWidthClass.Long);

			builder.SetVisibility(common.DischargeTerminalAddressControl, b => b.AMA_ManifestType == BRManifestTypes.Codes.MUCR, b => b.AMA_ManifestTypeInfo);

			builder.SetCaption(br.CustomsOwnNumberTextBox, b => b.AMA_ManifestType == BRManifestTypes.Codes.MUCR
				? Res.GetData("1AACBAAB-79BA-4E9E-841B-932F87DB9F45", "Master UCR")
				: Res.GetData("86B392F7-7ABA-4685-A25D-BC1D31C830AC", "CE Merchant"), b => b.AMA_ManifestTypeInfo);

			return builder.Build();
		}
	}
}
