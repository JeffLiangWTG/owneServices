using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class TransportBorderLayout : IPanelLayoutProvider
	{
		public TransportBorderLayout()
		{
			Layout = CreateTransportDepartureLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		PanelLayout CreateTransportDepartureLayout()
		{
			var builder = new TransportBorderLayoutBuilder<NctsDepartureMovementHeader>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.BorderTransportModeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.BorderTransportTypeOfIdDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.BorderTransportIdAndNationalityUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.BorderConveyanceNumberTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.BorderOfficeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.AdditionalTransportBorderUserControl, ControlWidthClass.Long);

			builder.SetVisibility(commonBag.AdditionalTransportBorderUserControl, h => !IsInPhase5TransitionPeriod(h) && NctsHelper.ShowExportTransportModeDetails(h));
			builder.SetVisibility(commonBag.BorderTransportTypeOfIdDropEdit, NctsHelper.ShowExportTransportModeDetails, h => h.BM_ExportTransportModeInfo);
			builder.SetVisibility(commonBag.BorderTransportIdAndNationalityUserControl, NctsHelper.ShowExportTransportModeDetails, h => h.BM_ExportTransportModeInfo);
			builder.SetVisibility(commonBag.BorderConveyanceNumberTextBox, NctsHelper.ShowExportTransportModeDetails, h => h.BM_ExportTransportModeInfo);
			builder.SetVisibility(commonBag.BorderOfficeDropEdit, NctsHelper.ShowExportTransportModeDetails, h => h.BM_ExportTransportModeInfo);

			builder.AddControlBehaviour(commonBag.BorderTransportIdAndNationalityUserControl
				, new BorderTransportIdAndNationalityUserControlBehaviour()
				, h => h.BM_ActiveBorderIdentificationTypeInfo
				, h => h.BM_ExportTransportModeInfo);

			return builder.Build();
		}

		static bool IsInPhase5TransitionPeriod(NctsDepartureMovementHeader moveHeader) => moveHeader.Header?.IsInPhase5TransitionPeriod ?? false;
	}
}
