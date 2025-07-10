using Enterprise.Customs.DE.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.NCTS.GUI
{
	public sealed class DepartureDetailsLayout : IPanelLayoutProvider
	{
		public DepartureDetailsLayout()
		{
			Layout = CreateDepartureDetailsLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		PanelLayout CreateDepartureDetailsLayout()
		{
			var builder = new EU.NCTS.GUI.DepartureDetailsLayoutBuilder<NctsHeader>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.OverrideFreightDetailsCheckBox, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomerReferenceNumberTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.DeclarationTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.SimplifiedProcedureAndReducedDataSetUserControl, ControlWidthClass.Long);
			builder.Add(commonBag.DateLimitDateEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.SecurityDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.TirCarnetNumberTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.CountryOfDispatchDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.CountryOfDestinationDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.GrossWeightCalcDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.LocationOfGoodsUserControl, ControlWidthClass.Long);
			builder.Add(commonBag.CommercialReferenceNumberTextBox, ControlWidthClass.Long);

			builder.SetVisibility(commonBag.LocationOfGoodsUserControl, x => x.MovementHeader.IsSimplifiedNctsProcedure, x => x.MovementHeader.IsSimplifiedNctsProcedureInfo);

			return builder.Build();
		}
	}
}
