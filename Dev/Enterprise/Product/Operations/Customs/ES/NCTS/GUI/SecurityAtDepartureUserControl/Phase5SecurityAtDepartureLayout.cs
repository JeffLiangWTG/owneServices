using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public sealed class Phase5SecurityAtDepartureLayout : IPanelLayoutProvider
	{
		public Phase5SecurityAtDepartureLayout()
		{
			Layout = CreateLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		PanelLayout CreateLayout()
		{
			var builder = new EU.NCTS.GUI.SecurityAtDepartureLayoutBuilder<Business.NctsHeader>();
			var commonBag = builder.CommonBag;
			var esBag = SecurityAtDepartureControlBag.Instance;
			builder.AddControlBag(esBag);

			builder.AddColumn();
			builder.Add(esBag.PlaceOfLoadingCodeFindBox, ControlWidthClass.Long);
			builder.Add(esBag.PlaceOfUnloadingCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.SpecificCircumstanceIndicatorDropEdit, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
