using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI;

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
		var builder = new DepartureDetailsLayoutBuilder<NctsHeader>();
		var esBag = DepartureDetailsControlBag.Instance;
		builder.AddControlBag(esBag);
		var euBag = builder.CommonBag;
		builder.AddControlBag(euBag);

		builder.AddColumn();
		builder.Add(euBag.OverrideFreightDetailsCheckBox, ControlWidthClass.Auto);
		builder.Add(euBag.CustomerReferenceNumberTextBox, ControlWidthClass.Long);
		builder.Add(euBag.DeclarationTypeDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.AdditionalDeclarationTypeDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.SimplifiedProcedureAndReducedDataSetUserControl, ControlWidthClass.Long);
		builder.Add(euBag.DateLimitDateEdit, ControlWidthClass.Auto);
		builder.Add(euBag.SecurityDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.TirCarnetNumberTextBox, ControlWidthClass.Long);
		builder.Add(euBag.CountryOfDispatchDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.CountryOfDestinationDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.GrossWeightCalcDropEdit, ControlWidthClass.Long);
		builder.Add(esBag.DepartureGoodsLocationZCodeFindBox, ControlWidthClass.Long);
		builder.Add(esBag.TNNDocumentTypeDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.CommercialReferenceNumberTextBox, ControlWidthClass.Long);

		builder.SetVisibility(esBag.TNNDocumentTypeDropEdit, h => h.IsPhaseStatusTNN);

		return builder.Build();
	}
}
