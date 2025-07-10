using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Declaration
{
	public sealed class TransportDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new TransportDetailsLayoutBuilder<JobDeclaration>();
			var commonBag = builder.CommonBag;
			var euBag = builder.EUBag;

			builder.AddControlBag(euBag);

			builder.AddColumn();

			builder.Add(commonBag.OverrideValuesCheckBox, ControlWidthClass.Long);
			builder.Add(commonBag.MasterBillTextBox, ControlWidthClass.Medium);
			builder.Add(commonBag.OceanBillTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.VesselCodeFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.FlightAndNationalityUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.VoyageAndNationalityUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.TransportIDAndNationalityUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.PortOfLoadingUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.TransportDetailsPortOfLoadingWithIATAUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.PortOfFirstArrivalUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.PortOfDischargeUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.TransportInlandSeparatorUserControl, ControlWidthClass.Auto);
			builder.Add(euBag.InlandTransportDetailsUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.InlandModeOfTransportDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.TransportInlandRoadUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.TransportInlandAirUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.TransportInlandInlandWaterwaysUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.TransportInlandOwnPropulsionUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.TransportInlandRailUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.TransportInlandSeaUserControl, ControlWidthClass.Auto);
			builder.Add(euBag.AdditionalWagonNumbersUserControl, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
