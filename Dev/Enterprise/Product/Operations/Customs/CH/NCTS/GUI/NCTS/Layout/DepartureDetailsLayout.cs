using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

public class DepartureDetailsLayout : IPanelLayoutProvider
{
	public DepartureDetailsLayout()
	{
		Layout = CreateDepartureDetailsLayout();
	}

	PanelLayout Layout { get; }

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	PanelLayout CreateDepartureDetailsLayout()
	{
		var builder = new DepartureDetailsLayoutBuilder<Business.NctsHeader>();
		var commonBag = builder.CommonBag;

		builder.AddColumn();
		builder.Add(commonBag.OverrideFreightDetailsCheckBox, ControlWidthClass.Auto);
		builder.Add(commonBag.CustomerReferenceNumberTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.DeclarationTypeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.TimeLimitForTransitCalcEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.SecurityDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.TirCarnetNumberTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.CountryOfDispatchDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.CountryOfDestinationDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.GrossWeightCalcDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.LocationOfGoodsUserControl, ControlWidthClass.Long);
		builder.Add(commonBag.CommunicationLanguageDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.CommercialReferenceNumberTextBox, ControlWidthClass.Long);

		builder.SetVisibility(commonBag.DateLimitDateEdit, x => true);

		return builder.Build();
	}
}
