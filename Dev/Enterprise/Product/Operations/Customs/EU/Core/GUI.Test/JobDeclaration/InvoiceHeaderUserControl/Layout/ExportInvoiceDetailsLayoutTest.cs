using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(ExportInvoiceDetailsLayout))]
	sealed class ExportInvoiceDetailsLayoutTest : LayoutsAbstractTest
	{
		public void TestVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			var layout = new ExportInvoiceDetailsLayout().Layout;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetAgreedPlaceCodeSupport(declaration, true))
			{
				invoice.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
				AssertEquals("AgreedPlaceCodeFindBox should be hidden when AgreedPlaceCodeSupportAndVisible false.", false, layout.IsVisible(InvoiceDetailsControlBag.Instance.AgreedPlaceCodeFindBox, invoice));

				invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CarriageAndInsurancePaidTo;
				AssertEquals("AgreedPlaceCodeFindBox should be visible when AgreedPlaceCodeSupportAndVisible true.", true, layout.IsVisible(InvoiceDetailsControlBag.Instance.AgreedPlaceCodeFindBox, invoice));
			}
		}

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				var customsBag = CommercialInvoiceDetailsControlBag.Instance;
				var euBag = InvoiceDetailsControlBag.Instance;

				yield return new List<(ControlReference, ControlWidthClass)>
				{
					(customsBag.InvoiceNumberTextBox, ControlWidthClass.Auto),
					(customsBag.InvoiceDateEdit, ControlWidthClass.Auto),
					(customsBag.GroupInvoiceDropEdit, ControlWidthClass.Auto),
					(customsBag.InvoiceAmountConvertToLocalCurrencyControl, ControlWidthClass.Auto),
					(customsBag.InvoiceCurrExRateCalcEdit, ControlWidthClass.Auto),
					(customsBag.IncoTermsUserControl, ControlWidthClass.Auto),
					(euBag.AgreedPlaceCodeFindBox, ControlWidthClass.Auto),
					(customsBag.IncoTermPlaceTextBox, ControlWidthClass.Auto),
					(euBag.IncoTermsAgreedPlaceLongTextControl, ControlWidthClass.Auto),
					(customsBag.ValuationCodeDropEdit, ControlWidthClass.Auto),
					(customsBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto),
					(customsBag.NetWeightCalcDropEdit, ControlWidthClass.Auto),
					(euBag.TransportChargesMethodOfPaymentDropEdit, ControlWidthClass.Auto),
					(customsBag.InvoiceCurrLandedCostExRateCalcEdit, ControlWidthClass.Auto),
					(customsBag.NoOfPacksCalcDropEdit, ControlWidthClass.Auto),
				};
			}
		}

		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new InvoiceDetailsLayoutBuilder<JobComInvoiceHeader>();
	}
}
