using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public sealed class ExportInvoiceDetailsLayoutProvider : IPanelLayoutProvider
{
	public PanelLayout Layout => ExportInvoice;

	public ExportInvoiceDetailsLayoutProvider()
	{
		ExportInvoice = CreateExportInvoiceDetailsLayouts();
	}

	PanelLayout CreateExportInvoiceDetailsLayouts()
	{
		var builder = new CommercialInvoiceDetailsLayoutBuilder<Business.JobComInvoiceHeader>();
		var commonBag = builder.CommonBag;
		var inBag = CommercialInvoiceDetailsControlBag.Instance;
		builder.AddControlBag(inBag);

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
		builder.Add(commonBag.ExporterAddressControl, ControlWidthClass.Auto);
		builder.Add(inBag.ExporterContractNumberTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.PaymentMethodDropEdit, ControlWidthClass.Auto);
		builder.Add(inBag.PaymentDaysCalcEdit, ControlWidthClass.Auto);
		builder.Add(inBag.AuthorizedEconomicOperatorGuidFindBox, ControlWidthClass.Auto);
		builder.Add(inBag.AuthorizedEconomicOperatorCountryTextBox, ControlWidthClass.Medium);
		builder.Add(inBag.AuthorizedEconomicOperatorCodeTextBox, ControlWidthClass.Auto);
		builder.Add(inBag.AuthorizedEconomicOperatorRoleTextBox, ControlWidthClass.Medium);
		builder.Add(inBag.IGSTPaymentStatusDropEdit, ControlWidthClass.Auto);

		return builder.Build();
	}

	PanelLayout ExportInvoice { get; }
}
