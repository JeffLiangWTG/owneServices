using System.Collections.Generic;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(ImportInvoiceLineDetailsLayout))]
sealed class ImportInvoiceLineDetailsLayoutTest : LayoutsAbstractTest
{
	public void TestBondedWarehouseRelatedFieldVisibility()
	{
		var helper = new ITUniversalReferenceTestDataHelper(Factory);
		helper.CreateRefCusProcedure71ForCurrentCountry();
		var procedure = helper.CreateRefCusProcedure("IT", "A", "18", "00", "", "Test", "IMP");
		procedure.ZZ6_IntoWarehouse = "Y";
		var invoiceLineDetailsControlBag = CommonInvoiceLineDetailsControlBag.Instance;

		CombineAssertions("For IMP", () =>
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
		declaration.JE_MessageType = "IMP";
		invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
	}

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (CommonInvoiceLineDetailsControlBag.Instance.EntryInstructionGuidDropEdit, ControlWidthClass.Long);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.FormattedProcedureCodeFindBox, ControlWidthClass.Long);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.AdditionalProcedureCodesUserControl, ControlWidthClass.Long);
			yield return (InvoiceLineDetailsControlBag.Instance.GoodsOriginDropEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.PartNoCodeFindBox, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.TariffFindBox, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.DescriptionLongTextControl, ControlWidthClass.Long);
			yield return (InvoiceLineDetailsControlBag.Instance.VatTypeAndDescriptionUserControl, ControlWidthClass.Auto);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.SupplementaryCode1DropEdit, ControlWidthClass.Medium);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.SupplementaryCode2DropEdit, ControlWidthClass.Medium);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.AdditionalSupplementaryCodesAndGDMUserControl, ControlWidthClass.Long);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.PreferenceCodeDropEdit, ControlWidthClass.Long);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.QuotaWithCheckLinkUserControl, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.ValuationCodeDropEdit, ControlWidthClass.Long);
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
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsFourthQuantityCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.InvoiceQuantityCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.BondedWhsQuantityCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryNumberTextBox, ControlWidthClass.Medium);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryLineNumberCalcEdit, ControlWidthClass.Medium);
		}
	}

	PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new ImportInvoiceLineDetailsLayout()).Layout);

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommonInvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>();

	PanelLayout layout;
	JobComInvoiceLine invoiceLine;
	JobDeclaration declaration;
}
