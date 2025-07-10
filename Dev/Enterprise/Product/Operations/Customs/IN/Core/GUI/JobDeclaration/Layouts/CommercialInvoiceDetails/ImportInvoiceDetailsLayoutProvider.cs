using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public sealed class ImportInvoiceDetailsLayoutProvider : IPanelLayoutProvider
{
	public PanelLayout Layout => ImportInvoice;

	public ImportInvoiceDetailsLayoutProvider()
	{
		ImportInvoice = CreateImportInvoiceDetailsLayouts();
	}

	PanelLayout CreateImportInvoiceDetailsLayouts()
	{
		var builder = new CommercialInvoiceDetailsLayoutBuilder<Business.JobComInvoiceHeader>();
		var commonBag = builder.CommonBag;

		builder.AddColumn();
		builder.Add(commonBag.InvoiceNumberTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.GroupInvoiceDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.InvoiceAmountConvertToLocalCurrencyControl, ControlWidthClass.Auto);
		builder.Add(commonBag.InvoiceCurrExRateCalcEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.IncoTermDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.IncoTermPlaceTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.InvoiceCurrLandedCostExRateCalcEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.NoOfPacksCalcDropEdit, ControlWidthClass.Auto);

		return builder.Build();
	}

	PanelLayout ImportInvoice { get; }
}
