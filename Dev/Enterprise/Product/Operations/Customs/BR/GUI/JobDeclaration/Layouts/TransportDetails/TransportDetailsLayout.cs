using Enterprise.Customs.BR.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public sealed class TransportDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout TransportDetails { get; }

		public PanelLayout Layout => TransportDetails;

		public TransportDetailsLayout()
		{
			TransportDetails = CreateTransportDetailsLayout();
		}

		PanelLayout CreateTransportDetailsLayout()
		{
			var builder = new TransportDetailsLayoutBuilder<JobDeclaration>();
			var commonBag = builder.CommonBag;

			var brBag = TransportDetailsControlBag.Instance;
			builder.AddControlBag(brBag);

			builder.AddColumn();
			builder.Add(commonBag.OverrideValuesCheckBox, ControlWidthClass.Long);
			builder.Add(commonBag.MasterBillTextBox, ControlWidthClass.Medium);
			builder.Add(commonBag.OceanBillTextBox, ControlWidthClass.Auto);
			builder.Add(brBag.PlateTextBox, ControlWidthClass.Auto);
			builder.Add(brBag.CargoArrivalDocUtilizationDropEdit, ControlWidthClass.Auto);
			builder.Add(brBag.VesselAndCountryUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.VoyageNumberTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.FlightUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.VehicleRegistrationNumberTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.PortOfLoadingUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.PortOfDischargeUserControl, ControlWidthClass.Auto);

			builder.SetVisibility(commonBag.OceanBillTextBox, h => h.IsSea || h.IsRail || h.IsRoad, h => h.JE_TransportModeInfo);
			builder.SetVisibility(commonBag.VoyageNumberTextBox, h => h.IsTransportByWater, h => h.JE_TransportModeInfo);
			builder.SetVisibility(brBag.CargoArrivalDocUtilizationDropEdit, h => h.IsCargoArrivalDocumentApplicable, h => h.JE_MessageTypeInfo, h => h.JE_TransportModeInfo);
			builder.SetVisibility(brBag.VesselAndCountryUserControl, h => h.IsTransportByWater, h => h.JE_TransportModeInfo);
			builder.SetVisibility(brBag.PlateTextBox, h => h.IsImportSiscomex && h.IsRoad, h => h.JE_MessageTypeInfo, h => h.JE_TransportModeInfo);

			builder.SetCaption(commonBag.OceanBillTextBox, h => h.OceanBillCaption, h => h.JE_TransportModeInfo);

			return builder.Build();
		}
	}
}
