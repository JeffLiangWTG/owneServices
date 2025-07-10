using Enterprise.Customs.AE.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.GUI;

public sealed class InvoiceLineDetailsLayout : IPanelLayoutProvider
{
	public PanelLayout Layout => CreateLayout();

	static PanelLayout CreateLayout()
	{
		var builder = new CommonInvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>();
		var commonBag = builder.CommonBag;
		var aeBag = AE.GUI.InvoiceLineDetailsControlBag.Instance;
		builder.AddControlBag(aeBag);

		builder.AddColumn();
		builder.Add(commonBag.PartNoCodeFindBox, ControlWidthClass.Medium);
		builder.Add(commonBag.ProcedureCodeFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.TariffFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.DescriptionLongTextControl, ControlWidthClass.Long);
		builder.Add(commonBag.CountryOfOriginCodeFindBox, ControlWidthClass.Long);
		builder.Add(aeBag.GoodsConditionDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.PrimaryPreferenceDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.CommodityCodeFindBox, ControlWidthClass.Auto);

		builder.AddColumn();
		builder.Add(commonBag.InvoiceQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.CustomsQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.CustomsFourthQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.LinePriceCurrencyCalcFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.TaxTypeDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.WeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.VolumeCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.BondedWHSOrderLineNumberCalcEdit, ControlWidthClass.Long);
		builder.Add(commonBag.BondedWHSOrderNumberTextBox, ControlWidthClass.Long);

		return builder.Build();
	}
}
