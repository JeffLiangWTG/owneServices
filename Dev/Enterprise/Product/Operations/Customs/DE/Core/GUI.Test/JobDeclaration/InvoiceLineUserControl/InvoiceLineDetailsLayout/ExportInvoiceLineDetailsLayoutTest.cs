using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common.DE;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(ExportInvoiceLineDetailsLayout))]
	sealed class ExportInvoiceLineDetailsLayoutTest : LayoutsAbstractTest
	{
		public void TestVisibility()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_Procedure = isOutOfWarehouseWarehousingProcedureCode;
				Assert("Precondition", invoiceLine.IsOutOfWarehouseWarehousing);
				AssertEquals("BondedWhsQuantityCalcDropEdit IsOutOfWarehouseWarehousing = true", true, Layout.IsVisible(CommonInvoiceLineDetailsControlBag.Instance.BondedWhsQuantityCalcDropEdit, invoiceLine));
				AssertEquals("PreviousEntryNumberTextBox IsOutOfWarehouseWarehousing = true", true, Layout.IsVisible(CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryNumberTextBox, invoiceLine));
				AssertEquals("PreviousEntryLineNumberCalcEdit IsOutOfWarehouseWarehousing = true", true, Layout.IsVisible(CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryLineNumberCalcEdit, invoiceLine));
				AssertEquals("BondedWHSOrderNumberTextBox IsOutOfWarehouseWarehousing = true", true, Layout.IsVisible(InvoiceLineDetailsControlBag.Instance.BondedWHSOrderNumberTextBox, invoiceLine));
				AssertEquals("BondedWHSOrderLineNumberCalcEdit IsOutOfWarehouseWarehousing = true", true, Layout.IsVisible(InvoiceLineDetailsControlBag.Instance.BondedWHSOrderLineNumberCalcEdit, invoiceLine));

				procedure.ZZ6_OutOfWarehouse = "N";
				AssertEquals("BondedWhsQuantityCalcDropEdit IsOutOfWarehouseWarehousing = false", false, Layout.IsVisible(CommonInvoiceLineDetailsControlBag.Instance.BondedWhsQuantityCalcDropEdit, invoiceLine));
				AssertEquals("PreviousEntryNumberTextBox IsOutOfWarehouseWarehousing = false", false, Layout.IsVisible(CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryNumberTextBox, invoiceLine));
				AssertEquals("PreviousEntryLineNumberCalcEdit IsOutOfWarehouseWarehousing = false", false, Layout.IsVisible(CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryLineNumberCalcEdit, invoiceLine));
				AssertEquals("BondedWHSOrderNumberTextBox IsOutOfWarehouseWarehousing = false", false, Layout.IsVisible(InvoiceLineDetailsControlBag.Instance.BondedWHSOrderNumberTextBox, invoiceLine));
				AssertEquals("BondedWHSOrderLineNumberCalcEdit IsOutOfWarehouseWarehousing = false", false, Layout.IsVisible(InvoiceLineDetailsControlBag.Instance.BondedWHSOrderLineNumberCalcEdit, invoiceLine));

				invoiceLine.JI_BondedWhsQuantity = 5m;
				AssertEquals("BondedWhsQuantityCalcDropEdit IsOutOfWarehouseWarehousing = false, but one of the fields is not empty", true, Layout.IsVisible(CommonInvoiceLineDetailsControlBag.Instance.BondedWhsQuantityCalcDropEdit, invoiceLine));
				AssertEquals("PreviousEntryNumberTextBox IsOutOfWarehouseWarehousing = false, but one of the fields is not empty", true, Layout.IsVisible(CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryNumberTextBox, invoiceLine));
				AssertEquals("PreviousEntryLineNumberCalcEdit IsOutOfWarehouseWarehousing = false, but one of the fields is not empty", true, Layout.IsVisible(CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryLineNumberCalcEdit, invoiceLine));
				AssertEquals("BondedWHSOrderNumberTextBox IsOutOfWarehouseWarehousing = false, but one of the fields is not empty", true, Layout.IsVisible(InvoiceLineDetailsControlBag.Instance.BondedWHSOrderNumberTextBox, invoiceLine));
				AssertEquals("BondedWHSOrderLineNumberCalcEdit IsOutOfWarehouseWarehousing = false, but one of the fields is not empty", true, Layout.IsVisible(InvoiceLineDetailsControlBag.Instance.BondedWHSOrderLineNumberCalcEdit, invoiceLine));
			});
		}

		public void TestCaptions()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = DEJobMessageTypeList.Codes.Export;
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var euBag = EU.GUI.InvoiceLineDetailsControlBag.Instance;
			var deBag = InvoiceLineDetailsControlBag.Instance;
			AssertCaption(euBag.SupplementaryCode1DropEdit, invoiceLine, "Sup. Code 1");
			AssertCaption(euBag.SupplementaryCode2DropEdit, invoiceLine, "Sup. Code 2");
			AssertCaption(euBag.AdditionalSupplementaryCodesAndGDMUserControl, invoiceLine, "Add. Sup. Codes");
			AssertCaption(deBag.FixedMaxLengthInvoiceNumberDropEdit, invoiceLine, "Invoice Number");
		}

		protected override int ControlBagCount => 3;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
				yield return ThirdColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommonInvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (InvoiceLineDetailsControlBag.Instance.FixedMaxLengthEntryInstructionGuidDropEdit, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.FixedMaxLengthInvoiceNumberDropEdit, ControlWidthClass.Long);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.PartNoCodeFindBox, ControlWidthClass.Auto);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.UnformattedProcedureCodeFindBox, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.FixedMaxLengthWithDescriptionTariffFindBox, ControlWidthClass.Long);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.SupplementaryCode1DropEdit, ControlWidthClass.Medium);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.SupplementaryCode2DropEdit, ControlWidthClass.Medium);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.AdditionalSupplementaryCodesAndGDMUserControl, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.DescriptionLongTextControl, ControlWidthClass.Long);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CountryOfOriginDropEdit, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.OriginFederalStateDropEdit, ControlWidthClass.Medium);
				yield return (InvoiceLineDetailsControlBag.Instance.ExportCountryCodeFindBox, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (InvoiceLineDetailsControlBag.Instance.IsMainPackCheckBox, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.LinePriceCurrencyCalcFindBox, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CommodityCodeFindBox, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.WeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsQuantityCalcDropEdit, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Auto);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.CusNumberCodeFindBox, ControlWidthClass.Auto);
				yield return (InvoiceLineDetailsControlBag.Instance.DgSubstanceUserControl, ControlWidthClass.Auto);
				yield return (InvoiceLineDetailsControlBag.Instance.UsualReplacementCheckBox, ControlWidthClass.Auto);
				yield return (InvoiceLineDetailsControlBag.Instance.ReimportDateEdit, ControlWidthClass.Auto);				
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (CommonInvoiceLineDetailsControlBag.Instance.BondedWhsQuantityCalcDropEdit, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.SerialNumberTextBox, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryNumberTextBox, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryLineNumberCalcEdit, ControlWidthClass.Auto);
				yield return (InvoiceLineDetailsControlBag.Instance.BondedWHSOrderNumberTextBox, ControlWidthClass.Auto);
				yield return (InvoiceLineDetailsControlBag.Instance.BondedWHSOrderLineNumberCalcEdit, ControlWidthClass.Auto);
			}
		}

		PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new ExportInvoiceLineDetailsLayout()).Layout);

		PanelLayout layout;

		protected override void SetUp()
		{
			base.SetUp();
			procedure = Factory.NewWithValidTestData<RefCusProcedure>();
			isOutOfWarehouseWarehousingProcedureCode = "4071";
			Factory.NewWithValidTestData<RefDataGrouping>().ZZZ_DataGrouping = "DE";
			procedure.ZZ6_ZZZ_NKDataGrouping = "DE";
			procedure.ZZ6_ShipmentType = "EXP";
			procedure.ZZ6_ProcedureCode = isOutOfWarehouseWarehousingProcedureCode.Left(2);
			procedure.ZZ6_Concession = isOutOfWarehouseWarehousingProcedureCode.PadRight(7).Right(3);
			procedure.ZZ6_OutOfWarehouse = "Y";
			procedure.ZZ6_OutOfInwardProcessing = "N";
			procedure.ZZ6_PreviousProcedureCode = "71";
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
		}
		JobComInvoiceLine invoiceLine;
		ZString isOutOfWarehouseWarehousingProcedureCode;
		RefCusProcedure procedure;

		void AssertCaption(ControlReference reference, JobComInvoiceLine line, string expectedCaption)
		{
			Layout.TryGetCaption(reference, line, out var resourceStringData);
			AssertEquals(reference.Name, expectedCaption, resourceStringData.Caption);
		}
	}
}
