using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI;

public sealed class TransportDetailsLayout : IPanelLayoutProvider
{
	public PanelLayout Layout { get; } = CreateLayout();

	static PanelLayout CreateLayout()
	{
		var builder = new TransportDetailsLayoutBuilder();
		var commonBag = builder.CommonBag;
		var beBag = TransportDetailsUserControlBag.Instance;
		builder.AddControlBag(beBag);

		builder.AddColumn();
		builder.Add(commonBag.OverrideValuesCheckBox, ControlWidthClass.Long);
		builder.Add(commonBag.MasterBillTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.OceanBillTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.VesselCodeFindBox, ControlWidthClass.Auto);
		builder.Add(beBag.FlightAndNationalityUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.VoyageAndNationalityUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.TransportIDAndNationalityUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.PortOfLoadingUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.TransportDetailsPortOfLoadingWithIATAUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.PortOfFirstArrivalUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.PortOfDischargeUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.TransportInlandSeparatorUserControl, ControlWidthClass.Auto);

		builder.Add(commonBag.TransportInlandModeAndTypeOfIdUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.TransportInlandRoadUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.TransportInlandIDAndNationalityUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.TransportInlandSeaUserControl, ControlWidthClass.Auto);

		return builder.Build();
	}
}
