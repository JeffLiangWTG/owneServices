using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class HouseConsignmentTransportDepartureLayout : IPanelLayoutProvider
	{
		public HouseConsignmentTransportDepartureLayout()
		{
			Layout = CreateHouseConsigmentTransportDepartureLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		PanelLayout CreateHouseConsigmentTransportDepartureLayout()
		{
			var builder = new HouseConsignmentTransportDepartureLayoutBuilder<NctsBill>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.InlandTransportModeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.TransportAtDepartureTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.TransportAtDepartureTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.VesselCodeFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.TransportAtDepartureTrailer1RegNoTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.TransportAtDepartureTrailer2RegNoTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.AdditionalWagonNumbersButton, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(commonBag.PlaceHolderLabel, ControlWidthClass.LongNoCaption, commonBag.InlandTransportModeDropEdit);
			builder.Add(commonBag.PlaceHolder2Label, ControlWidthClass.Auto);
			builder.Add(commonBag.TransportAtDepartureCountryCodeFindBox, ControlWidthClass.LongNoCaption, commonBag.TransportAtDepartureTextBox);
			builder.Add(commonBag.VesselCountryCodeFindBox, ControlWidthClass.LongNoCaption, commonBag.VesselCodeFindBox);
			builder.Add(commonBag.TransportAtDepartureTrailer1NationalityCodeFindBox, ControlWidthClass.LongNoCaption, commonBag.TransportAtDepartureTrailer1RegNoTextBox);
			builder.Add(commonBag.TransportAtDepartureTrailer2NationalityCodeFindBox, ControlWidthClass.LongNoCaption, commonBag.TransportAtDepartureTrailer2RegNoTextBox);

			return builder.Build();
		}
	}
}
