using Enterprise.Customs.GUI;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class ExportCommercialInvoiceDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout IPanelLayoutProvider.Layout => PanelLayout;

		PanelLayout PanelLayout { get; }

		public ExportCommercialInvoiceDetailsLayout()
		{
			PanelLayout = CreatePanelLayout();
		}

		PanelLayout CreatePanelLayout()
		{
			var builder = new CommercialInvoiceDetailsLayoutBuilder<JobComInvoiceHeader>();
			var commonBag = builder.CommonBag;

			var krBag = CommercialInvoiceDetailsControlBag.Instance;
			builder.AddControlBag(krBag);

			builder.AddColumn();
			builder.Add(commonBag.InvoiceNumberTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.InvoiceAmountConvertToLocalCurrencyControl, ControlWidthClass.Auto);
			builder.Add(commonBag.InvoiceCurrExRateCalcEdit, ControlWidthClass.Auto);
			builder.Add(krBag.IncoTermCodeDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(krBag.NoOfPacksCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(krBag.PaymentTermsCodeDropEdit, ControlWidthClass.Auto);
			builder.Add(krBag.LetterOfCreditNumberTextBox, ControlWidthClass.Auto);
			builder.Add(krBag.NoteTextLongTextControl, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
