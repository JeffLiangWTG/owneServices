using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public sealed class TransportDetailsLayouts : IPanelLayoutProvider
{
	PanelLayout TransportDetails { get; }

	PanelLayout IPanelLayoutProvider.Layout => TransportDetails;

	public TransportDetailsLayouts()
	{
		TransportDetails = CreateTransportDetailsLayout();
	}

	PanelLayout CreateTransportDetailsLayout()
	{
		var builder = new TransportDetailsLayoutBuilder<BaseJobDeclaration>();
		var commonBag = builder.CommonBag;
		var inBag = TransportDetailsControlBag.Instance;
		builder.AddControlBag(inBag);

		builder.AddColumn();
		builder.Add(commonBag.CustomsOfficeCodeFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.OverrideValuesCheckBox, ControlWidthClass.Long);
		builder.Add(commonBag.MasterBillTextBox, ControlWidthClass.Medium);
		builder.Add(commonBag.OceanBillTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.VehicleRegistrationNumberTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.VesselCodeFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.FlightUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.VoyageNumberTextBox, ControlWidthClass.Auto);
		builder.Add(inBag.LoadingAndDestinationInformationSeparatorUserControl, ControlWidthClass.LongControl);
		builder.Add(commonBag.PortOfLoadingUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.CustomsLoadPortCodeFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.PortOfDischargeUserControl, ControlWidthClass.Auto);

		builder.SetVisibility(commonBag.CustomsLoadPortCodeFindBox, x => x.IsExport && (x.IsAir || x.IsSea));
		builder.SetVisibility(inBag.LoadingAndDestinationInformationSeparatorUserControl, x => x.IsExport);
		return builder.Build();
	}
}
