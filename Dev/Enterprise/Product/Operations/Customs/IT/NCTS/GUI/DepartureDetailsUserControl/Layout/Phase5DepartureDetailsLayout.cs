using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

sealed class Phase5DepartureDetailsLayout : IPanelLayoutProvider
{
	public Phase5DepartureDetailsLayout()
	{
		Layout = CreateDepartureDetailsLayout();
	}

	PanelLayout Layout { get; }

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	PanelLayout CreateDepartureDetailsLayout()
	{
		var builder = new DepartureDetailsLayoutBuilder();
		var commonBag = builder.CommonBag;
		var itBag = DepartureDetailsControlBag.Instance;

		builder.AddControlBag(itBag);
		builder.AddColumn();
		builder.Add(commonBag.OverrideFreightDetailsCheckBox, ControlWidthClass.Auto);
		builder.Add(commonBag.CustomerReferenceNumberTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.DeclarationTypeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.AdditionalDeclarationTypeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.SimplifiedProcedureAndReducedDataSetUserControl, ControlWidthClass.Long);
		builder.Add(commonBag.DateLimitDateEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.SecurityDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.TirCarnetNumberTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.CountryOfDispatchDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.CountryOfDestinationDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.GrossWeightCalcDropEdit, ControlWidthClass.Long);
		builder.Add(itBag.LocationOfGoodsUserControl, ControlWidthClass.Long);
		builder.Add(commonBag.PresentationDateTimeOffsetEdit, ControlWidthClass.Long);
		builder.Add(commonBag.CommercialReferenceNumberTextBox, ControlWidthClass.Long);

		return builder.Build();
	}
}
