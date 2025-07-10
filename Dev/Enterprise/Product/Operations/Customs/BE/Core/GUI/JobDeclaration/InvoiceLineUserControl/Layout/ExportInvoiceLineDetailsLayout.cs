using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI;

public sealed class ExportInvoiceLineDetailsLayout : IPanelLayoutProvider
{
	PanelLayout Layout { get; } = CreateLayout();

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	static PanelLayout CreateLayout()
	{
		var builder = new CommonInvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>();
		var commonBag = builder.CommonBag;
		var euBag = EU.GUI.InvoiceLineDetailsControlBag.Instance;
		builder.AddControlBag(euBag);

		var beBag = BE.GUI.InvoiceLineDetailsControlBag.Instance;
		builder.AddControlBag(beBag);

		builder.AddColumn();
		builder.Add(commonBag.EntryInstructionGuidDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.PartNoCodeFindBox, ControlWidthClass.Auto);
		builder.Add(euBag.FormattedProcedureCodeFindBox, ControlWidthClass.Long);
		builder.Add(euBag.AdditionalProcedureCodesUserControl, ControlWidthClass.Long);
		builder.Add(commonBag.WithDescriptionTariffFindBox, ControlWidthClass.Long);
		builder.Add(euBag.CusNumberCodeFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.DescriptionLongTextControl, ControlWidthClass.Long);
		builder.Add(commonBag.CountryOfOriginDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.CountryOfExportCodeFindBox, ControlWidthClass.Long);
		builder.Add(euBag.DestinationUsingZZRefCusCodeListCodeFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.WithDescriptionPrimaryPreferenceDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.ConcessionOrderTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.ValuationCodeDropEdit, ControlWidthClass.Auto);

		builder.AddColumn();
		builder.Add(commonBag.InvoiceQuantityCalcDropEdit, ControlWidthClass.Medium);
		builder.Add(commonBag.CustomsQuantityCalcDropEdit, ControlWidthClass.Medium);
		builder.Add(commonBag.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Medium);
		builder.Add(commonBag.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Medium);
		builder.Add(commonBag.CustomsFourthQuantityCalcDropEdit, ControlWidthClass.Medium);
		builder.Add(commonBag.LinePriceCurrencyCalcFindBox, ControlWidthClass.Medium);
		builder.Add(commonBag.TaxTypeDropEdit, ControlWidthClass.Medium);
		builder.Add(euBag.SupplementaryCode1DropEdit, ControlWidthClass.Medium);
		builder.Add(euBag.SupplementaryCode2DropEdit, ControlWidthClass.Medium);
		builder.Add(euBag.AdditionalSupplementaryCodesAndGDMUserControl, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(commonBag.StateOrRegionOfOriginDropEdit, ControlWidthClass.Medium);
		builder.Add(commonBag.WeightCalcDropEdit, ControlWidthClass.Medium);
		builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Medium);
		builder.Add(commonBag.VolumeCalcDropEdit, ControlWidthClass.Medium);
		builder.Add(beBag.UCRReferenceTextBox, ControlWidthClass.Medium);
		builder.Add(commonBag.CommodityCodeFindBox, ControlWidthClass.Medium);
		builder.Add(commonBag.ValuationMarkupCalcEdit, ControlWidthClass.Auto);

		return builder.Build();
	}
}
