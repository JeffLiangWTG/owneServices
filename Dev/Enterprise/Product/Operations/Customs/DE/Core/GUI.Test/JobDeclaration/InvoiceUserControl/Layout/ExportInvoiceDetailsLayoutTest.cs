using System.Collections.Generic;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Declaration.Testing
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
				AssertEquals("Invisible", false, layout.IsVisible(EU.GUI.InvoiceDetailsControlBag.Instance.AgreedPlaceCodeFindBox, invoiceHeader));
				invoiceHeader.JZ_IncoTerm = "ABC";
				AssertEquals("Visible", true, layout.IsVisible(EU.GUI.InvoiceDetailsControlBag.Instance.AgreedPlaceCodeFindBox, invoiceHeader));
			});
		}

		protected override int ControlBagCount => 3;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
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
				yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.InvoiceNumberTextBox, ControlWidthClass.Auto);
				yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.InvoiceDateEdit, ControlWidthClass.Auto);
				yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.InvoiceAmountConvertToLocalCurrencyControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.InvoiceCurrExRateCalcEdit, ControlWidthClass.Auto);
				yield return (ExportInvoiceDetailsControlBag.Instance.FreeOfChargeCheckBox, ControlWidthClass.Auto);
				yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.IncoTermsUserControl, ControlWidthClass.Auto);
				yield return (EU.GUI.InvoiceDetailsControlBag.Instance.AgreedPlaceCodeFindBox, ControlWidthClass.Auto);
				yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.IncoTermPlaceTextBox, ControlWidthClass.Auto);
				yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.ValuationCodeDropEdit, ControlWidthClass.Auto);
				yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (EU.GUI.InvoiceDetailsControlBag.Instance.TransportChargesMethodOfPaymentDropEdit, ControlWidthClass.Auto);
				yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.NoOfPacksCalcDropEdit, ControlWidthClass.Auto);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommercialInvoiceDetailsLayoutBuilder<JobComInvoiceHeader>();
	}
}
