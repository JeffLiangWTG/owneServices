using System.Collections.Generic;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(ImportInvoiceDetailsLayout))]
sealed class ImportInvoiceDetailsLayoutTest : LayoutsAbstractTest
{
	public void TestCaptions()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var layout = ((IPanelLayoutProvider)new ImportInvoiceDetailsLayout()).Layout;
		var customsBag = CommercialInvoiceDetailsControlBag.Instance;

		AssertCaption(customsBag.IncoTermsUserControl, "[20.1] INCO term");
		AssertCaption(customsBag.IncoTermPlaceTextBox, "[20.2] Place");

		void AssertCaption(ControlReference controlReference, string expectedCaption)
		{
			layout.TryGetCaption(controlReference, invoice, out var resourceStringData);
			AssertNotNull($"ResourceData for {controlReference.Name}", resourceStringData);
			AssertEquals($"Caption for {controlReference.Name}", expectedCaption, resourceStringData.Caption);
		}
	}

	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
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
			yield return (CommercialInvoiceDetailsControlBag.Instance.ValuationCodeDropEdit, ControlWidthClass.Auto);
			yield return (CommercialInvoiceDetailsControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommercialInvoiceDetailsControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommercialInvoiceDetailsControlBag.Instance.InvoiceCurrLandedCostExRateCalcEdit, ControlWidthClass.Auto);
			yield return (CommercialInvoiceDetailsControlBag.Instance.NoOfPacksCalcDropEdit, ControlWidthClass.Auto);
		}
	}

	protected override int ControlBagCount => 2;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommercialInvoiceDetailsLayoutBuilder<JobComInvoiceHeader>();
}
