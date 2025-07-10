using Enterprise.Customs.GUI;
using Enterprise.Customs.MX.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.MX.GUI
{
	public sealed class CommercialInvoiceDetailsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout => CommercialInvoice;

		public CommercialInvoiceDetailsLayout()
		{
			CommercialInvoice = CreateCommercialInvoiceDetailsLayouts();
		}

		PanelLayout CreateCommercialInvoiceDetailsLayouts()
		{
			var builder = new CommercialInvoiceDetailsLayoutBuilder<JobComInvoiceHeader>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();

			builder.Add(commonBag.InvoiceNumberTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.InvoiceDateEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.InvoiceAmountConvertToLocalCurrencyControl, ControlWidthClass.Auto);
			builder.Add(commonBag.IncoTermsUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.IncoTermPlaceTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.ValuationCodeDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.InvoiceCurrLandedCostExRateCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.NoOfPacksCalcDropEdit, ControlWidthClass.Auto);

			return builder.Build();
		}

		PanelLayout CommercialInvoice { get; }
	}
}
