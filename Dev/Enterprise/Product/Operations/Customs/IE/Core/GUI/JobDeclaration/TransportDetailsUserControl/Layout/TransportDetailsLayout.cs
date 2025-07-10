using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public sealed class TransportDetailsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; } = CreateLayout();

		static PanelLayout CreateLayout()
		{
			var builder = new TransportDetailsLayoutBuilder();
			var commonBag = builder.CommonBag;
			var euBag = EU.GUI.TransportDetailsControlBag.Instance;
			builder.AddControlBag(euBag);

			builder.AddColumn();
			builder.Add(commonBag.OverrideValuesCheckBox, ControlWidthClass.Long);
			builder.Add(commonBag.MasterBillTextBox, ControlWidthClass.Medium);
			builder.Add(commonBag.OceanBillTextBox, ControlWidthClass.Auto);
			builder.Add(euBag.VesselUserControl, ControlWidthClass.Auto);
			builder.Add(euBag.TransportIDAndNationalityUserControl, ControlWidthClass.Auto);
			builder.Add(euBag.TransportIDAndNationalityRailUserControl, ControlWidthClass.Auto);
			builder.Add(euBag.TransportIDAndNationalityInlandWaterwayUserControl, ControlWidthClass.Auto);
			builder.Add(euBag.TransportIDAndNationalityInlandWaterwayENIUserControl, ControlWidthClass.Auto);
			builder.Add(euBag.FlightAndNationalityUserControl, ControlWidthClass.Auto);
			builder.Add(euBag.AircraftRegistrationNumberTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.VoyageAndNationalityUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.PortOfLoadingUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.PortOfFirstArrivalUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.PortOfDischargeUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.TransportInlandSeparatorUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.TransportInlandModeAndTypeOfIdUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.TransportInlandIDAndNationalityUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.TransportInlandRoadUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.TransportInlandSeaUserControl, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
