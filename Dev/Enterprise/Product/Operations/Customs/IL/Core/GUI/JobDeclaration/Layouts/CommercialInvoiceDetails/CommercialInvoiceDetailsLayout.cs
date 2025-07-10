using Enterprise.Customs.GUI;
using Enterprise.Customs.IL.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI
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

			var ilBag = InvoiceControlBag.Instance;
			builder.AddControlBag(ilBag);

			builder.AddColumn();

			builder.Add(ilBag.SequenceTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.InvoiceNumberTextBox, ControlWidthClass.Auto);
			builder.Add(ilBag.InvoiceDateDateEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.InvoiceAmountConvertToLocalCurrencyControl, ControlWidthClass.Auto);
			builder.Add(ilBag.IncoTermsWithCountryCodeUserControl, ControlWidthClass.Auto);
			builder.Add(ilBag.SupplierCodeFindBox, ControlWidthClass.Auto);
			builder.Add(ilBag.PreferenceAgreementDropEdit, ControlWidthClass.Auto);
			builder.Add(ilBag.PaymentTermsDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.NoOfPacksCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.InvoiceCurrLandedCostExRateCalcEdit, ControlWidthClass.Auto);

			return builder.Build();
		}

		PanelLayout CommercialInvoice { get; }
	}
}
