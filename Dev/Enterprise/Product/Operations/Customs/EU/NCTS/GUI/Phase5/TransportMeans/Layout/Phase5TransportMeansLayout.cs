using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class Phase5TransportMeansLayout : IPanelLayoutProvider
	{
		public Phase5TransportMeansLayout()
		{
			Layout = CreatePhase5TransportMeansLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		PanelLayout CreatePhase5TransportMeansLayout()
		{
			var builder = new Phase5TransportMeansLayoutBuilder<CusInBondEvent>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.TransportAtDepartureTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.TransportAtDepartureIDTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.TransportAtDepartureNationalityCodeFindBox, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
