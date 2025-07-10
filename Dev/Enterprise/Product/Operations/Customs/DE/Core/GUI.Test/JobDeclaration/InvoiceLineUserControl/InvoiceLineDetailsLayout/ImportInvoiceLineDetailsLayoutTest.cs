using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using CusEntryInstruction = Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(ImportInvoiceLineDetailsLayout))]
	sealed class ImportInvoiceLineDetailsLayoutTest : LayoutsAbstractTest
	{
		public void TestExportCountryCodeFindBox_Visibility()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No EntryInstruction", expected: false, Layout.IsVisible(InvoiceLineDetailsControlBag.Instance.ExportCountryCodeFindBox, invoiceLine));

				invoiceLine.JI_CEI = entryInstruction.PK;
				foreach (var type in new ImportDeclarationTypeList().GetAllCodes().Except(ImportDeclarationTypeList.Codes.LUZ))
				{
					entryInstruction.CEI_Style = type;
					AssertEquals($"CEI_Style={type}", expected: false, Layout.IsVisible(InvoiceLineDetailsControlBag.Instance.ExportCountryCodeFindBox, invoiceLine));
				}
				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;
				AssertEquals("CEI_Style=LUZ", expected: true, Layout.IsVisible(InvoiceLineDetailsControlBag.Instance.ExportCountryCodeFindBox, invoiceLine));
			});
		}

		public void TestDecisiveDateEdit_Visibility()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No EntryInstruction", expected: false, Layout.IsVisible(InvoiceLineDetailsControlBag.Instance.DecisiveDateEdit, invoiceLine));

				invoiceLine.JI_CEI = entryInstruction.PK;
				foreach (var type in new ImportDeclarationTypeList().GetAllCodes().Except(ImportDeclarationTypeList.Codes.LUZ))
				{
					entryInstruction.CEI_Style = type;
					AssertEquals($"{type}", expected: false, Layout.IsVisible(InvoiceLineDetailsControlBag.Instance.DecisiveDateEdit, invoiceLine));
				}
				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;
				AssertEquals("LUZ", expected: true, Layout.IsVisible(InvoiceLineDetailsControlBag.Instance.DecisiveDateEdit, invoiceLine));
			});
		}

		public void TestBondedWhsQuantityCalcDropEdit_Visibility()
		{
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				invoiceLine.JI_CEI = entryInstruction.PK;
				declaration.SetSupportsBondedWarehousingForTesting(true);
				CombineAssertions(() =>
				{
					AssertEquals("IsBondedWhsQuantityVisible: false", expected: false, Layout.IsVisible(CommonInvoiceLineDetailsControlBag.Instance.BondedWhsQuantityCalcDropEdit, invoiceLine));

					entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;
					AssertEquals("IsBondedWhsQuantityVisible: true", expected: true, Layout.IsVisible(CommonInvoiceLineDetailsControlBag.Instance.BondedWhsQuantityCalcDropEdit, invoiceLine));
					AssertContainsExactElementsInAnyOrder("Visibility Dependencies", expected: new[] { invoiceLine.JI_ProcedureInfo, invoiceLine.JI_CEIInfo }, Layout.GetVisibilityDependencies(CommonInvoiceLineDetailsControlBag.Instance.BondedWhsQuantityCalcDropEdit, invoiceLine));
				});
			}
		}

		public void TestPreviousEntryNumberTextBox_Visibility()
		{
			PrepareProcedure();
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				declaration.SetSupportsBondedWarehousingForTesting(true);
				CombineAssertions(() =>
				{
					invoiceLine.JI_Procedure = "7100";
					AssertEquals("IsPreviousEntryNumberVisible: false", expected: false, Layout.IsVisible(CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryNumberTextBox, invoiceLine));

					invoiceLine.JI_Procedure = IsOutOfWarehouseWarehousingProcedureCode;
					AssertEquals("IsPreviousEntryNumberVisible: true", expected: true, Layout.IsVisible(CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryNumberTextBox, invoiceLine));
					AssertContainsExactElementsInAnyOrder("Visibility Dependencies", expected: new[] { invoiceLine.JI_ProcedureInfo, invoiceLine.JI_CEIInfo }, Layout.GetVisibilityDependencies(CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryNumberTextBox, invoiceLine));
				});
			}
		}

		public void TestPreviousEntryLineNumberCalcEdit_Visibility()
		{
			PrepareProcedure();
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				declaration.SetSupportsBondedWarehousingForTesting(true);
				CombineAssertions(() =>
				{
					invoiceLine.JI_Procedure = "7100";
					AssertEquals("IsPreviousEntryNumberVisible: false", expected: false, Layout.IsVisible(CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryLineNumberCalcEdit, invoiceLine));

					invoiceLine.JI_Procedure = IsOutOfWarehouseWarehousingProcedureCode;
					AssertEquals("IsPreviousEntryNumberVisible: true", expected: true, Layout.IsVisible(CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryLineNumberCalcEdit, invoiceLine));
					AssertContainsExactElementsInAnyOrder("Visibility Dependencies", expected: new[] { invoiceLine.JI_ProcedureInfo, invoiceLine.JI_CEIInfo }, Layout.GetVisibilityDependencies(CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryLineNumberCalcEdit, invoiceLine));
				});
			}
		}

		public void TestBondedWHSOrderNumberTextBox_Visibility()
		{
			PrepareProcedure();
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				declaration.SetSupportsBondedWarehousingForTesting(true);
				CombineAssertions(() =>
				{
					invoiceLine.JI_Procedure = "7100";
					AssertEquals("IsPreviousEntryNumberVisible: false", expected: false, Layout.IsVisible(InvoiceLineDetailsControlBag.Instance.BondedWHSOrderNumberTextBox, invoiceLine));

					invoiceLine.JI_Procedure = IsOutOfWarehouseWarehousingProcedureCode;
					AssertEquals("IsPreviousEntryNumberVisible: true", expected: true, Layout.IsVisible(InvoiceLineDetailsControlBag.Instance.BondedWHSOrderNumberTextBox, invoiceLine));
				});
			}
		}

		public void TestBondedWHSOrderLineNumberCalcEdit_Visibility()
		{
			PrepareProcedure();
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				declaration.SetSupportsBondedWarehousingForTesting(true);
				CombineAssertions(() =>
				{
					invoiceLine.JI_Procedure = "7100";
					AssertEquals("IsPreviousEntryNumberVisible: false", expected: false, Layout.IsVisible(InvoiceLineDetailsControlBag.Instance.BondedWHSOrderLineNumberCalcEdit, invoiceLine));

					invoiceLine.JI_Procedure = IsOutOfWarehouseWarehousingProcedureCode;
					AssertEquals("IsPreviousEntryNumberVisible: true", expected: true, Layout.IsVisible(InvoiceLineDetailsControlBag.Instance.BondedWHSOrderLineNumberCalcEdit, invoiceLine));
				});
			}
		}

		public void TestExportCountryCodeFindBoxCaption()
		{
			Layout.TryGetCaption(InvoiceLineDetailsControlBag.Instance.ExportCountryCodeFindBox, invoiceLine, out var resourceStringData);
			AssertEquals("Origin", resourceStringData.Caption);
		}

		public void TestPreviousEntryNumberTextBoxCaption()
		{
			Layout.TryGetCaption(CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryNumberTextBox, invoiceLine, out var resourceStringData);
			AssertEquals("Previous Entry No.", resourceStringData.Caption);
		}

		public void TestPreviousEntryLineNumberCalcEditCaption()
		{
			Layout.TryGetCaption(CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryLineNumberCalcEdit, invoiceLine, out var resourceStringData);
			AssertEquals("Previous Entry Line", resourceStringData.Caption);
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

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonInvoiceLineDetailsControlBag.Instance.EntryInstructionGuidDropEdit, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.InvoiceNumberDropEdit, ControlWidthClass.Long);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.FormattedProcedureCodeFindBox, ControlWidthClass.Long);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.AdditionalProcedureCodesUserControl, ControlWidthClass.Long);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.WithDescriptionTariffFindBox, ControlWidthClass.Long);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.SupplementaryCode1DropEdit, ControlWidthClass.Medium);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.SupplementaryCode2DropEdit, ControlWidthClass.Medium);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.AdditionalSupplementaryCodesAndGDMUserControl, ControlWidthClass.Long);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.DescriptionLongTextControl, ControlWidthClass.Long);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CountryOfOriginDropEdit, ControlWidthClass.Long);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.CountryOfSupplyCodeFindBox, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.PreferenceCodeDropEdit, ControlWidthClass.Long);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.LinePriceCurrencyCalcFindBox, ControlWidthClass.Auto);
				yield return (InvoiceLineDetailsControlBag.Instance.NetPriceCurrencyCalcFindBox, ControlWidthClass.Auto);
				yield return (InvoiceLineDetailsControlBag.Instance.CessionManagementFlagDropEdit, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.PartNoCodeFindBox, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CommodityCodeFindBox, ControlWidthClass.Auto);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.QuotaDropEdit, ControlWidthClass.Auto);
				yield return (InvoiceLineDetailsControlBag.Instance.QuotaQtyCalcDropEdit, ControlWidthClass.Auto);
				yield return (InvoiceLineDetailsControlBag.Instance.SupplementaryInformationTextBox, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.ExportCountryCodeFindBox, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.DecisiveDateEdit, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (CommonInvoiceLineDetailsControlBag.Instance.WeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsQuantityCalcDropEdit, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsFourthQuantityCalcDropEdit, ControlWidthClass.Auto);
				yield return (InvoiceLineDetailsControlBag.Instance.TobaccoStampTextBox, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.BondedWhsQuantityCalcDropEdit, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.SerialNumberTextBox, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryNumberTextBox, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryLineNumberCalcEdit, ControlWidthClass.Auto);
				yield return (InvoiceLineDetailsControlBag.Instance.BondedWHSOrderNumberTextBox, ControlWidthClass.Auto);
				yield return (InvoiceLineDetailsControlBag.Instance.BondedWHSOrderLineNumberCalcEdit, ControlWidthClass.Auto);
			}
		}

		public PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new ImportInvoiceLineDetailsLayout()).Layout);

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommonInvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>();

		PanelLayout layout;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
		}
		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
		CusEntryInstruction entryInstruction;
		const string IsOutOfWarehouseWarehousingProcedureCode = "4071";

		void PrepareProcedure()
		{
			var procedure = Factory.NewWithValidTestData<RefCusProcedure>();
			Factory.NewWithValidTestData<RefDataGrouping>().ZZZ_DataGrouping = "DE";
			procedure.ZZ6_ZZZ_NKDataGrouping = "DE";
			procedure.ZZ6_ShipmentType = "IMP";
			procedure.ZZ6_ProcedureCode = ((ZString)IsOutOfWarehouseWarehousingProcedureCode).Left(2);
			procedure.ZZ6_Concession = ((ZString)IsOutOfWarehouseWarehousingProcedureCode).PadRight(7).Right(3);
			procedure.ZZ6_OutOfWarehouse = "Y";
			procedure.ZZ6_PreviousProcedureCode = "71";
			Factory.Save();
		}
	}
}
