using Enterprise.Customs.GUI;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public sealed class ImportInvoiceLineDetailsLayout : IPanelLayoutProvider
{
	PanelLayout IPanelLayoutProvider.Layout => CreateLayout();

	static PanelLayout CreateLayout()
	{
		var builder = new CommonInvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>();
		var commonBag = builder.CommonBag;
		var euBag = EU.GUI.InvoiceLineDetailsControlBag.Instance;
		builder.AddControlBag(euBag);
		var itBag = InvoiceLineDetailsControlBag.Instance;
		builder.AddControlBag(itBag);

		builder.AddColumn();
		builder.Add(commonBag.EntryInstructionGuidDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.FormattedProcedureCodeFindBox, ControlWidthClass.Long);
		builder.Add(euBag.AdditionalProcedureCodesUserControl, ControlWidthClass.Long);
		builder.Add(itBag.GoodsOriginDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.PartNoCodeFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.TariffFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.DescriptionLongTextControl, ControlWidthClass.Long);
		builder.Add(itBag.VatTypeAndDescriptionUserControl, ControlWidthClass.Auto);
		builder.Add(euBag.SupplementaryCode1DropEdit, ControlWidthClass.Medium);
		builder.Add(euBag.SupplementaryCode2DropEdit, ControlWidthClass.Medium);
		builder.Add(euBag.AdditionalSupplementaryCodesAndGDMUserControl, ControlWidthClass.Long);
		builder.Add(euBag.PreferenceCodeDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.QuotaWithCheckLinkUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.ValuationCodeDropEdit, ControlWidthClass.Long);
		builder.Add(itBag.PortTaxRateDropEdit, ControlWidthClass.Auto);
		builder.Add(euBag.CusNumberCodeFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.CommodityCodeFindBox, ControlWidthClass.Auto);

		builder.AddColumn();
		builder.Add(commonBag.LinePriceCurrencyCalcFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.WeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.CustomsQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.CustomsFourthQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.InvoiceQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.VolumeCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.BondedWhsQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.PreviousEntryNumberTextBox, ControlWidthClass.Medium);
		builder.Add(commonBag.PreviousEntryLineNumberCalcEdit, ControlWidthClass.Medium);

		return builder.Build();
	}
}
