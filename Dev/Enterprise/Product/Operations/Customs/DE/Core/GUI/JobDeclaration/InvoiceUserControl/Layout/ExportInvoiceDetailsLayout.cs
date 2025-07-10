using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public sealed class ExportInvoiceDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new CommercialInvoiceDetailsLayoutBuilder<JobComInvoiceHeader>();
			var commonBag = builder.CommonBag;
			var euBag = EU.GUI.InvoiceDetailsControlBag.Instance;
			var deBag = ExportInvoiceDetailsControlBag.Instance;

			builder.AddControlBag(euBag);
			builder.AddControlBag(deBag);

			builder.AddColumn();
			builder.Add(commonBag.InvoiceNumberTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.InvoiceDateEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.InvoiceAmountConvertToLocalCurrencyControl, ControlWidthClass.Auto);
			builder.Add(commonBag.InvoiceCurrExRateCalcEdit, ControlWidthClass.Auto);
			builder.Add(deBag.FreeOfChargeCheckBox, ControlWidthClass.Auto);
			builder.Add(commonBag.IncoTermsUserControl, ControlWidthClass.Auto);
			builder.Add(euBag.AgreedPlaceCodeFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.IncoTermPlaceTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.ValuationCodeDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(euBag.TransportChargesMethodOfPaymentDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.NoOfPacksCalcDropEdit, ControlWidthClass.Auto);

			builder.SetVisibility(euBag.TransportChargesMethodOfPaymentDropEdit, invoice => !(invoice.JobDeclaration?.IsMiscellaneous ?? false), invoice => invoice.JobDeclaration?.JE_MessageTypeInfo);
			builder.SetVisibility(euBag.AgreedPlaceCodeFindBox, invoice => invoice.AgreedPlaceCodeSupportAndVisible, invoice => invoice.JZ_IncoTermInfo);
			return builder.Build();
		}
	}
}
