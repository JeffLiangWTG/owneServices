using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(ExportInvoiceDetailsLayout))]
sealed class ExportInvoiceDetailsLayoutTest : LayoutsAbstractTest
{
	public void TestCaptions()
	{
		var layout = GetLayout();

		AssertCaption(layout, invoice, customsBag.InvoiceAmountConvertToLocalCurrencyControl, "[22] Inv. Amount");
		AssertCaption(layout, invoice, customsBag.IncoTermPlaceTextBox, "[20.2] Place");
		AssertCaption(layout, invoice, euBag.AgreedPlaceCodeFindBox, "Incoterm Place Code");
		AssertCaption(layout, invoice, itBag.AgreedPlaceCodeDropEdit, "[20.3] Code");
		AssertCaption(layout, invoice, customsBag.AdditionalTermsTextBox, "Delivery Terms");
		AssertCaption(layout, invoice, customsBag.ValuationCodeDropEdit, "[24] Tran. Nature");
		AssertCaption(layout, invoice, euBag.TransportChargesMethodOfPaymentDropEdit, "Trans. Chrg. MoP");
		AssertCaption(layout, invoice, customsBag.InvoiceCurrExRateCalcEdit, "Exchange Rate");
	}

	public void TestAgreedPlacedCodeFindBoxVisibility()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetAgreedPlaceCodeSupport(declaration, false))
		{
			var layout = GetLayout();
			var isAgreedPlaceCodeBoxVisible = layout.IsVisible(euBag.AgreedPlaceCodeFindBox, invoice);
			AssertEquals("Visible", false, isAgreedPlaceCodeBoxVisible);
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetAgreedPlaceCodeSupport(declaration, true))
		{
			CombineAssertions("When not UCC6 and 'Agreed Place Code' is ON", () =>
			{
				invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CarriageAndInsurancePaidTo;
				var layout = GetLayout();
				var isAgreedPlaceCodeBoxVisible = layout.IsVisible(euBag.AgreedPlaceCodeFindBox, invoice);
				AssertEquals("and ‘INCO Terms’ is not ‘XXX’, Visible", true, isAgreedPlaceCodeBoxVisible);

				invoice.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
				isAgreedPlaceCodeBoxVisible = layout.IsVisible(euBag.AgreedPlaceCodeFindBox, invoice);
				AssertEquals("and ‘INCO Terms’ is ‘XXX’, Visible", false, isAgreedPlaceCodeBoxVisible);
			});
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			CombineAssertions("When not UCC6 and 'Agreed Place Code' is OFF", () =>
			{
				invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CarriageAndInsurancePaidTo;
				var layout = GetLayout();
				var isAgreedPlaceCodeBoxVisible = layout.IsVisible(euBag.AgreedPlaceCodeFindBox, invoice);
				AssertEquals("and ‘INCO Terms’ is not ‘XXX’, Visible", false, isAgreedPlaceCodeBoxVisible);

				invoice.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
				isAgreedPlaceCodeBoxVisible = layout.IsVisible(euBag.AgreedPlaceCodeFindBox, invoice);
				AssertEquals("and ‘INCO Terms’ is ‘XXX’, Visible", false, isAgreedPlaceCodeBoxVisible);
			});
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions("When UCC6", () =>
			{
				invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CarriageAndInsurancePaidTo;
				var layout = GetLayout();
				var isAgreedPlaceCodeBoxVisible = layout.IsVisible(euBag.AgreedPlaceCodeFindBox, invoice);
				AssertEquals("and ‘INCO Terms’ is not ‘XXX’, Visible", true, isAgreedPlaceCodeBoxVisible);

				invoice.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
				isAgreedPlaceCodeBoxVisible = layout.IsVisible(euBag.AgreedPlaceCodeFindBox, invoice);
				AssertEquals("and ‘INCO Terms’ is ‘XXX’, Visible", true, isAgreedPlaceCodeBoxVisible);
			});
		}
	}

	public void TestAgreedPlacedCodeDropBoxVisibility()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			var layout = GetLayout();
			AssertEquals("When job is not UCC6", true, layout.IsVisible(itBag.AgreedPlaceCodeDropEdit, invoice));
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			var layout = GetLayout();
			AssertEquals("When job is UCC6", false, layout.IsVisible(itBag.AgreedPlaceCodeDropEdit, invoice));
		}
	}

	public void TestAdditionalTermsTextBoxVisibility()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			var layout = GetLayout();
			invoice.JZ_IncoTerm = "XXX";
			AssertEquals("When job is not UCC6 and Incoterm is XXX, Visible", false, layout.IsVisible(customsBag.AdditionalTermsTextBox, invoice));
		}

		CombineAssertions("When job is UCC6", () =>
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				var layout = GetLayout();
				invoice.JZ_IncoTerm = "FOB";
				AssertEquals("When Incoterm is FOB, Visible", false, layout.IsVisible(customsBag.AdditionalTermsTextBox, invoice));

				invoice.JZ_IncoTerm = "XXX";
				AssertEquals("When Incoterm is XXX, Visible", true, layout.IsVisible(customsBag.AdditionalTermsTextBox, invoice));
			}
		});
	}

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn => new[]
	{
		new[]
		{
			(customsBag.InvoiceNumberTextBox, ControlWidthClass.Auto),
			(customsBag.InvoiceDateEdit, ControlWidthClass.Auto),
			(customsBag.InvoiceAmountConvertToLocalCurrencyControl, ControlWidthClass.Auto),
			(customsBag.InvoiceCurrExRateCalcEdit, ControlWidthClass.Auto),
			(customsBag.IncoTermsUserControl, ControlWidthClass.Auto),
			(euBag.AgreedPlaceCodeFindBox, ControlWidthClass.Auto),
			(itBag.AgreedPlaceCodeDropEdit, ControlWidthClass.Auto),
			(customsBag.IncoTermPlaceTextBox, ControlWidthClass.Auto),
			(customsBag.AdditionalTermsTextBox, ControlWidthClass.Auto),
			(customsBag.ValuationCodeDropEdit, ControlWidthClass.Auto),
			(euBag.TransportChargesMethodOfPaymentDropEdit, ControlWidthClass.Auto),
			(customsBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto),
			(customsBag.NetWeightCalcDropEdit, ControlWidthClass.Auto),
			(customsBag.NoOfPacksCalcDropEdit, ControlWidthClass.Auto),
		}
	};

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		invoice = declaration.Invoices.AddNew();
	}

	protected override int ControlBagCount => 3;

	void AssertCaption(PanelLayout layout, BusinessObject bo, ControlReference controlReference, string expectedCaption)
	{
		layout.TryGetCaption(controlReference, bo, out var resourceStringData);
		AssertNotNull($"ResourceData for {controlReference.Name}", resourceStringData);
		AssertEquals($"Caption for {controlReference.Name}", expectedCaption, resourceStringData.Caption);
	}

	IPanelLayoutProvider GetLayoutProvider() => new ExportInvoiceDetailsLayout();

	PanelLayout GetLayout() => GetLayoutProvider().Layout;

	CommercialInvoiceDetailsControlBag customsBag => CommercialInvoiceDetailsControlBag.Instance;

	EU.GUI.InvoiceDetailsControlBag euBag => EU.GUI.InvoiceDetailsControlBag.Instance;

	InvoiceDetailsControlBag itBag => InvoiceDetailsControlBag.Instance;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommercialInvoiceDetailsLayoutBuilder<JobComInvoiceHeader>();

	JobComInvoiceHeader invoice;

	JobDeclaration declaration;
}
