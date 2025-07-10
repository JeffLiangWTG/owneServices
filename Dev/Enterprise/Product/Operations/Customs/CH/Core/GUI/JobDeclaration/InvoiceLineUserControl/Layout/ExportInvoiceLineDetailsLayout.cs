using Enterprise.Customs.CH.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public sealed class ExportInvoiceLineDetailsLayout : IPanelLayoutProvider
{
	public PanelLayout Layout { get; } = CreateLayout();

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	static PanelLayout CreateLayout()
	{
		var builder = new CommonInvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>();
		var commonBag = builder.CommonBag;
		builder.AddControlBag(commonBag);
		var chBag = InvoiceLineDetailsControlBag.Instance;
		builder.AddControlBag(chBag);

		builder.AddColumn();
		builder.Add(commonBag.EntryInstructionGuidDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.ProcedureCodeFindBox, ControlWidthClass.Long);
		builder.Add(chBag.NonCommercialGoodsCheckBox, ControlWidthClass.Long);
		builder.Add(chBag.GoodsReturnedCheckBox, ControlWidthClass.Long);
		builder.Add(commonBag.FormattedWithDescriptionTariffFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.DescriptionLongTextControl, ControlWidthClass.Long);
		builder.Add(commonBag.CountryOfOriginCodeFindBox, ControlWidthClass.Long);
		builder.Add(chBag.UNDGCodesUserControl, ControlWidthClass.Long);
		builder.Add(chBag.CusCodeFindBox, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(commonBag.InvoiceQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.WeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.LinePriceCurrencyCalcFindBox, ControlWidthClass.Auto);
		builder.Add(chBag.RefundTypeDropEdit, ControlWidthClass.Long);
		builder.Add(chBag.RefundReferenceNumberTextBox, ControlWidthClass.Long);
		builder.Add(chBag.RefundGoodsItemNumberIntEdit, ControlWidthClass.Long);
		builder.Add(chBag.RefundReasonTextBox, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(commonBag.CustomsQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.VolumeCalcDropEdit, ControlWidthClass.Medium, commonBag.CustomsThirdQuantityCalcDropEdit);
		builder.Add(commonBag.CommodityCodeFindBox, ControlWidthClass.Medium);

		builder.SetVisibility(chBag.RefundReferenceNumberTextBox, l => l.IsReturnedGoodsWithRefundRequest, l => l.JI_RefundTypeInfo);
		builder.SetVisibility(chBag.RefundGoodsItemNumberIntEdit, l => l.IsReturnedGoodsWithRefundRequest, l => l.JI_RefundTypeInfo);
		builder.SetVisibility(chBag.RefundReasonTextBox, l => l.IsReturnedGoodsWithRefundRequest, l => l.JI_RefundTypeInfo);

		return builder.Build();
	}
}
