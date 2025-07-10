using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class IncidentDetailsLayoutWithGrid : IPanelLayoutWithGridProvider
	{
		public IncidentDetailsLayoutWithGrid()
		{
			Layout = CreateLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new IncidentDetailsLayoutBuilder<Business.EnRouteIncident>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.IncidentCodeDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.InformationTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.EndorsementDateEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.EndorsementAuthorityTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.EndorsementCountryCodeDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.EndorsementPlaceTextBox, ControlWidthClass.Auto);
			builder.AddColumn();
			builder.Add(commonBag.LocationOfGoodsUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.EventCountryCodeDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.TransportMeansGroupUserControl, ControlWidthClass.LongNoCaption);
			return builder.Build();
		}
		Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(Phase5IncidentsGridUserControl);
	}
}
