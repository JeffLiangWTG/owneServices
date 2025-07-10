using System.Collections.Generic;
using Enterprise.Customs.EU.GUI.CommercialInvoice;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.CommercialInvoice;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(ImportInvoiceDetailsLayout))]
	sealed class ImportInvoiceDetailsLayoutTest : LayoutsAbstractTest
	{
		public void TestVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var layout = (new ImportInvoiceDetailsLayout()).Layout;
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
				AssertEquals("Visible", true, layout.IsVisible(CommonBag.UCRTextBox, invoiceHeader));

				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
				AssertEquals("Visible", true, layout.IsVisible(CommonBag.UCRTextBox, invoiceHeader));

				declaration.JE_ApplicationCode = "@#@";
				AssertEquals("Invisible", false, layout.IsVisible(CommonBag.UCRTextBox, invoiceHeader));
			});
		}

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn => new[] { new[] {
			(BaseBag.InvoiceNumberBoundTextBox, ControlWidthClass.Auto),
			(BaseBag.InvoiceDateEdit, ControlWidthClass.Auto),
			(BaseBag.InvoiceAmountCalcFindBox, ControlWidthClass.Auto),
			(BaseBag.InvoiceCurrExRateCalcEdit, ControlWidthClass.Auto),
			(BaseBag.IncotermAndIncotermPlaceUserControl, ControlWidthClass.Auto),
			(EUBag.AgreedPlaceCodeFindBox, ControlWidthClass.Auto),
			(BaseBag.ValuationCodeDropEdit, ControlWidthClass.Auto),
			(BaseBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto),
			(BaseBag.NetWeightCalcDropEdit, ControlWidthClass.Auto),
			(BaseBag.InvoiceCurrLandedCostExRateCalcEdit, ControlWidthClass.Auto),
			(BaseBag.NoOfPacksCalcDropEdit, ControlWidthClass.Auto),
			(CommonBag.UCRTextBox, ControlWidthClass.Auto)
		} };

		CommercialInvoiceDetailsControlBag CommonBag => CommercialInvoiceDetailsControlBag.Instance;

		EUInvoiceHeaderDetailsControlBag EUBag => EUInvoiceHeaderDetailsControlBag.Instance;

		InvoiceHeaderDetailsControlBag BaseBag => InvoiceHeaderDetailsControlBag.Instance;

		protected override int ControlBagCount => 3;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommercialInvoiceDetailsLayoutBuilder<JobComInvoiceHeader>();
	}
}
