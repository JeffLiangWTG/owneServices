using Enterprise.Customs.AE.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.GUI;

public sealed class TransportDetailsLayout : IPanelLayoutProvider
{
	PanelLayout Layout { get; } = CreateLayout();

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	static PanelLayout CreateLayout()
	{
		var builder = new TransportDetailsLayoutBuilder<JobDeclaration>();
		var commonBag = builder.CommonBag;
		var aeBag = TransportDetailsControlBag.Instance;
		builder.AddControlBag(aeBag);

		builder.AddColumn();

		builder.Add(commonBag.OverrideValuesCheckBox, ControlWidthClass.Long);
		builder.Add(commonBag.MasterBillTextBox, ControlWidthClass.Medium);
		builder.Add(commonBag.OceanBillTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.VehicleRegistrationNumberTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.VesselCodeFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.FlightUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.VoyageNumberTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.PortOfLoadingUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.PortOfDischargeUserControl, ControlWidthClass.Auto);
		builder.Add(aeBag.PlaceOfDischargeDropEdit, ControlWidthClass.Auto);

		return builder.Build();
	}
}
