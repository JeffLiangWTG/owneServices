using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
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
			var builder = new SecurityAtDepartureLayoutBuilder<Business.NctsHeader>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.PlaceOfLoadingUserControl, ControlWidthClass.Long);
			builder.Add(commonBag.PlaceOfUnloadingUserControl, ControlWidthClass.Long);
			builder.Add(commonBag.SpecificCircumstanceIndicatorDropEdit, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
