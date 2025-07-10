using System.Collections.Generic;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(ExportInvoiceLineDetailsLayout))]
sealed class ExportInvoiceLineDetailsLayoutTest : LayoutsAbstractTest
{
	public void TestBondedWarehouseRelatedFieldVisibility_EXP() => AssertBondedWarehouseRelatedFieldVisibility("EXP");

	public void TestCusNumberCodeFindBoxVisbility()
	{
		var invoiceLineDetailsControlBag = EU.GUI.InvoiceLineDetailsControlBag.Instance;
		declaration.JE_MessageType = "EXP";

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			AssertEquals("[PRE-CONDITION] Declaration.IsUcc6", false, declaration.IsUCC6);
			AssertEquals("When Declaration is not UCC6, CusNumberCodeFindBox.Visisble", false, Layout.IsVisible(invoiceLineDetailsControlBag.CusNumberCodeFindBox, invoiceLine));
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			AssertEquals("[PRE-CONDITION] Declaration.IsUcc6", true, declaration.IsUCC6);
			AssertEquals("When Declaration is UCC6, CusNumberCodeFindBox.Visisble", true, Layout.IsVisible(invoiceLineDetailsControlBag.CusNumberCodeFindBox, invoiceLine));
		}
	}

	public void TestCountryOfDestinationDropEditVisibility()
	{
		var controlBag = InvoiceLineDetailsControlBag.Instance;
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;

		var isCountryOfDestinationDropEditVisible = Layout.IsVisible(controlBag.CountryOfDestinationDropEdit, invoiceLine);
		AssertEquals("Non-UCC6, EXP", false, isCountryOfDestinationDropEditVisible);

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			isCountryOfDestinationDropEditVisible = Layout.IsVisible(controlBag.CountryOfDestinationDropEdit, invoiceLine);
			AssertEquals("UCC6, EXP", true, isCountryOfDestinationDropEditVisible);
		}
	}

	public void TestCountryOfDestinationDropEditCaption()
	{
		var controlBag = InvoiceLineDetailsControlBag.Instance;
		Layout.TryGetCaption(controlBag.CountryOfDestinationDropEdit, invoiceLine, out var captionData);
		AssertNotNull("CaptionData", captionData);

		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Country of Destination", captionData.Caption);
			AssertEquals("ShortCaption", "Dest. Country", captionData.ShortCaption);
			AssertEquals("Medium Caption", "Country of Destination", captionData.MediumCaption);
			AssertEquals("Full Description", "Country of Destination of the goods being moved.", captionData.FullDescription);
		});
	}

	public void TestCountryOfExportDropEditVisibility()
	{
		var controlBag = InvoiceLineDetailsControlBag.Instance;
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			AssertEquals("Non-UCC6, EXP", false, Layout.IsVisible(controlBag.CountryOfExportDropEdit, invoiceLine));
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			AssertEquals("UCC6, EXP", true, Layout.IsVisible(controlBag.CountryOfExportDropEdit, invoiceLine));
		}
	}

	public void TestAdditionalProcedureCodesUserControlVisibility()
	{
		var controlBag = EU.GUI.InvoiceLineDetailsControlBag.Instance;
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			AssertEquals("Non-UCC6, EXP", false, Layout.IsVisible(controlBag.AdditionalProcedureCodesUserControl, invoiceLine));
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			AssertEquals("UCC6, EXP", true, Layout.IsVisible(controlBag.AdditionalProcedureCodesUserControl, invoiceLine));
		}
	}

	public void TestInvoiceNumberDropEditVisibility()
	{
		var controlBag = InvoiceLineDetailsControlBag.Instance;
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			AssertEquals("UCC6, EXP", expected: true, Layout.IsVisible(controlBag.InvoiceNumberDropEdit, invoiceLine));
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			AssertEquals("Non-UCC6, EXP", expected: false, Layout.IsVisible(controlBag.InvoiceNumberDropEdit, invoiceLine));
		}
	}

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
		}
	}

	protected override int ControlBagCount => 3;

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
	}

	void AssertBondedWarehouseRelatedFieldVisibility(string messageType)
	{
		var helper = new ITUniversalReferenceTestDataHelper(Factory);
		helper.CreateRefCusProcedure71ForCurrentCountry();
		var procedure = helper.CreateRefCusProcedure("IT", "A", "18", "00", "", "Test", messageType);
		procedure.ZZ6_IntoWarehouse = "Y";
		declaration.JE_MessageType = messageType;
		var invoiceLineDetailsControlBag = CommonInvoiceLineDetailsControlBag.Instance;

		CombineAssertions($"For {messageType}", () =>
		{
			invoiceLine.JI_Procedure = "1800";
			AssertEquals("Visible when IntoWarehouse=Y and OutOfWarehouse=N and IntoVATWarehouse=N, BondedWhsQuantityCalcDropEdit", true, Layout.IsVisible(invoiceLineDetailsControlBag.BondedWhsQuantityCalcDropEdit, invoiceLine));
			AssertEquals("Not visible when OutOfWarehouse=N, MediumPreviousEntryNumberTextBox", false, Layout.IsVisible(invoiceLineDetailsControlBag.PreviousEntryNumberTextBox, invoiceLine));
			AssertEquals("Not visible when OutOfWarehouse=N, MediumPreviousEntryLineNumberCalcEdit", false, Layout.IsVisible(invoiceLineDetailsControlBag.PreviousEntryLineNumberCalcEdit, invoiceLine));

			procedure.ZZ6_IntoWarehouse = "N";
			AssertEquals("Not visible when IntoWarehouse=N and OutOfWarehouse=N and IntoVATWarehouse=N, BondedWhsQuantityCalcDropEdit", false, Layout.IsVisible(invoiceLineDetailsControlBag.BondedWhsQuantityCalcDropEdit, invoiceLine));

			procedure.ZZ6_OutOfWarehouse = "Y";
			AssertEquals("Visible when IntoWarehouse=N and OutOfWarehouse=Y and IntoVATWarehouse=N, BondedWhsQuantityCalcDropEdit", true, Layout.IsVisible(invoiceLineDetailsControlBag.BondedWhsQuantityCalcDropEdit, invoiceLine));
			AssertEquals("Visible when OutOfWarehouse=Y, MediumPreviousEntryNumberTextBox", true, Layout.IsVisible(invoiceLineDetailsControlBag.PreviousEntryNumberTextBox, invoiceLine));
			AssertEquals("Visible when OutOfWarehouse=Y, MediumPreviousEntryLineNumberCalcEdit", true, Layout.IsVisible(invoiceLineDetailsControlBag.PreviousEntryLineNumberCalcEdit, invoiceLine));

			procedure.ZZ6_OutOfWarehouse = "N";
			procedure.ZZ6_IntoVATWarehouse = "Y";
			AssertEquals("Visible when IntoWarehouse=N and OutOfWarehouse=N and IntoVATWarehouse=Y, BondedWhsQuantityCalcDropEdit", true, Layout.IsVisible(invoiceLineDetailsControlBag.BondedWhsQuantityCalcDropEdit, invoiceLine));

			invoiceLine.JI_Procedure = "";
			AssertEquals("Not visible when Procedure is empty, BondedWhsQuantityCalcDropEdit", false, Layout.IsVisible(invoiceLineDetailsControlBag.BondedWhsQuantityCalcDropEdit, invoiceLine));
			AssertEquals("Not visible when Procedure is empty, MediumPreviousEntryNumberTextBox", false, Layout.IsVisible(invoiceLineDetailsControlBag.PreviousEntryNumberTextBox, invoiceLine));
			AssertEquals("Not visible when Procedure is empty, MediumPreviousEntryLineNumberCalcEdit", false, Layout.IsVisible(invoiceLineDetailsControlBag.PreviousEntryLineNumberCalcEdit, invoiceLine));
		});
	}

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (CommonInvoiceLineDetailsControlBag.Instance.EntryInstructionGuidDropEdit, ControlWidthClass.Long);
			yield return (InvoiceLineDetailsControlBag.Instance.InvoiceNumberDropEdit, ControlWidthClass.Auto);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.FormattedProcedureCodeFindBox, ControlWidthClass.Long);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.AdditionalProcedureCodesUserControl, ControlWidthClass.Long);
			yield return (InvoiceLineDetailsControlBag.Instance.OriginCountryStateUserControl, ControlWidthClass.Auto);
			yield return (InvoiceLineDetailsControlBag.Instance.CountryOfExportDropEdit, ControlWidthClass.Auto);
			yield return (InvoiceLineDetailsControlBag.Instance.CountryOfDestinationDropEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.PartNoCodeFindBox, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.TariffFindBox, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.DescriptionLongTextControl, ControlWidthClass.Long);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.SupplementaryCode1DropEdit, ControlWidthClass.Medium);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.SupplementaryCode2DropEdit, ControlWidthClass.Medium);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.AdditionalSupplementaryCodesUserControl, ControlWidthClass.Long);
			yield return (InvoiceLineDetailsControlBag.Instance.PortTaxRateDropEdit, ControlWidthClass.Auto);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.CusNumberCodeFindBox, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CommodityCodeFindBox, ControlWidthClass.Auto);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (CommonInvoiceLineDetailsControlBag.Instance.LinePriceCurrencyCalcFindBox, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.WeightCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsQuantityCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.InvoiceQuantityCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.BondedWhsQuantityCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryNumberTextBox, ControlWidthClass.Medium);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryLineNumberCalcEdit, ControlWidthClass.Medium);
		}
	}

	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;

	PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new ExportInvoiceLineDetailsLayout()).Layout);

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new ExportInvoiceLineDetailsLayoutBuilder();

	PanelLayout layout;
}
