using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public class JobComInvoiceHeaderLayouts : IPanelLayoutProvider
	{
		PanelLayout IPanelLayoutProvider.Layout => PanelLayout;

		PanelLayout PanelLayout { get; }

		public JobComInvoiceHeaderLayouts()
		{
			PanelLayout = CreatePanelLayout();
		}

		PanelLayout CreatePanelLayout()
		{
			var builder = new JobComInvoiceHeaderLayoutBuilder();
			var invoiceDetailsControlBag = builder.CommonBag;
			var jpControlBag = builder.JPControlBag;
			builder.AddControlBag(jpControlBag);

			builder.AddColumn();
			builder.Add(invoiceDetailsControlBag.InvoiceNumberTextBox, ControlWidthClass.Auto);
			builder.Add(invoiceDetailsControlBag.GroupInvoiceDropEdit, ControlWidthClass.Auto);
			builder.Add(jpControlBag.InvoiceAmountConvertToLocalCurrencyControl, ControlWidthClass.Auto);
			builder.Add(invoiceDetailsControlBag.InvoiceCurrExRateCalcEdit, ControlWidthClass.Auto);
			builder.Add(jpControlBag.IncoTermsUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(jpControlBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(jpControlBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(invoiceDetailsControlBag.InvoiceCurrLandedCostExRateCalcEdit, ControlWidthClass.Auto);
			builder.Add(invoiceDetailsControlBag.NoOfPacksCalcDropEdit, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
