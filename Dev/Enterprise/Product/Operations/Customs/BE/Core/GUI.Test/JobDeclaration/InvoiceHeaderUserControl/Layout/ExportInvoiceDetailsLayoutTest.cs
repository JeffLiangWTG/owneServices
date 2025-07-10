using System.Collections.Generic;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.GUI.Testing;

[TestedType(typeof(ExportInvoiceDetailsLayout))]
sealed class ExportInvoiceDetailsLayoutTest : LayoutsAbstractTest
{
	public void TestIncoTermsCaption()
	{
		var layout = new ExportInvoiceDetailsLayout().Layout;
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		layout.TryGetCaptionData(Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.IncoTermsUserControl, invoice, out var captionData);
		AssertEquals("IncoTerms Caption", "[UCC 4/1] Incoterm", captionData["IncoTermsUserControl"].Caption);
	}

	public void TestExchangeRateCaption() => CombineAssertions(() =>
	{
		var layout = new ExportInvoiceDetailsLayout().Layout;
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		layout.TryGetCaptionData(Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.InvoiceCurrExRateCalcEdit, invoice, out var captionData);
		AssertEquals("ExchangeRate Caption", "Exchange Rate", captionData["InvoiceCurrExRateCalcEdit"].Caption);
		AssertEquals("ExchangeRate FullDescription", "[UCC 4/15] Exchange Rate", captionData["InvoiceCurrExRateCalcEdit"].FullDescription);
	});

	protected override int ControlBagCount => 2;

	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (CommercialInvoiceDetailsControlBag.Instance.InvoiceNumberTextBox, ControlWidthClass.Auto);
			yield return (CommercialInvoiceDetailsControlBag.Instance.InvoiceDateEdit, ControlWidthClass.Auto);
			yield return (CommercialInvoiceDetailsControlBag.Instance.InvoiceAmountConvertToLocalCurrencyControl, ControlWidthClass.Auto);
			yield return (CommercialInvoiceDetailsControlBag.Instance.InvoiceCurrExRateCalcEdit, ControlWidthClass.Auto);
			yield return (CommercialInvoiceDetailsControlBag.Instance.IncoTermsUserControl, ControlWidthClass.Auto);
			yield return (EU.GUI.InvoiceDetailsControlBag.Instance.AgreedPlaceCodeFindBox, ControlWidthClass.Auto);
			yield return (CommercialInvoiceDetailsControlBag.Instance.IncoTermPlaceTextBox, ControlWidthClass.Auto);
			yield return (EU.GUI.InvoiceDetailsControlBag.Instance.IncoTermsAgreedPlaceLongTextControl, ControlWidthClass.Auto);
			yield return (CommercialInvoiceDetailsControlBag.Instance.ValuationCodeDropEdit, ControlWidthClass.Auto);
			yield return (CommercialInvoiceDetailsControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommercialInvoiceDetailsControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Auto);
			yield return (EU.GUI.InvoiceDetailsControlBag.Instance.TransportChargesMethodOfPaymentDropEdit, ControlWidthClass.Auto);
			yield return (CommercialInvoiceDetailsControlBag.Instance.NoOfPacksCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommercialInvoiceDetailsControlBag.Instance.UCRTextBox, ControlWidthClass.Auto);
		}
	}
	protected override ICommonLayoutBuilder CommonLayoutBuilder => new InvoiceDetailsLayoutBuilder<JobComInvoiceHeader>();
}
