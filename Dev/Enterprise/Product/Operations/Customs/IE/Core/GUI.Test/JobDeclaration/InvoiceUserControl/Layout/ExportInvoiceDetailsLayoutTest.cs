using System.Collections.Generic;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(ExportInvoiceDetailsLayout))]
	sealed class ExportInvoiceDetailsLayoutTest : LayoutsAbstractTest
	{
		public void TestVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var invoiceHeader = declaration.Invoices.AddNew();
			var layout = ((IPanelLayoutProvider)new ExportInvoiceDetailsLayout()).Layout;
			CombineAssertions(() =>
			{
				invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
				AssertEquals("Invisible", false, layout.IsVisible(CustomsBag.IncoTermPlaceTextBox, invoiceHeader));

				invoiceHeader.JZ_IncoTerm = "ABC";
				invoiceHeader.ZG_AgreedPlaceCode = "ABCDE";
				AssertEquals("Invisible", false, layout.IsVisible(CustomsBag.IncoTermPlaceTextBox, invoiceHeader));

				invoiceHeader.JZ_IncoTerm = "ABC";
				invoiceHeader.ZG_AgreedPlaceCode = "AB";
				AssertEquals("Visible", true, layout.IsVisible(CustomsBag.IncoTermPlaceTextBox, invoiceHeader));
			});
		}

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn => new[] { new[] {
			(CustomsBag.InvoiceNumberTextBox, ControlWidthClass.Auto),
			(CustomsBag.InvoiceDateEdit, ControlWidthClass.Auto),
			(CustomsBag.InvoiceAmountConvertToLocalCurrencyControl, ControlWidthClass.Auto),
			(CustomsBag.InvoiceCurrExRateCalcEdit, ControlWidthClass.Auto),
			(CustomsBag.IncoTermsUserControl, ControlWidthClass.Auto),
			(EUBag.AgreedPlaceCodeFindBox, ControlWidthClass.Auto),
			(CustomsBag.IncoTermPlaceTextBox, ControlWidthClass.Auto),
			(CustomsBag.AdditionalTermsTextBox, ControlWidthClass.Auto),
			(CustomsBag.ValuationCodeDropEdit, ControlWidthClass.Auto),
			(CustomsBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto),
			(CustomsBag.NetWeightCalcDropEdit, ControlWidthClass.Auto),
			(EUBag.TransportChargesMethodOfPaymentDropEdit, ControlWidthClass.Auto),
			(CustomsBag.NoOfPacksCalcDropEdit, ControlWidthClass.Auto),
			(CustomsBag.UCRTextBox, ControlWidthClass.Auto)
		} };

		CommercialInvoiceDetailsControlBag CustomsBag => CommercialInvoiceDetailsControlBag.Instance;

		InvoiceDetailsControlBag EUBag => InvoiceDetailsControlBag.Instance;

		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommercialInvoiceDetailsLayoutBuilder<JobComInvoiceHeader>();
	}
}
