using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(InventorySelectionHeader))]
sealed class InventorySelectionHeaderForImportBondedDetailsTest : NonPersistentBusinessObjectTestCase
{
	public void TestImportInventories_InvoiceLine_JI_InvoiceQuantity()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: null,
			assertion: (invoiceLine) => AssertEquals("JI_InvoiceQuantity", 0m, invoiceLine.JI_InvoiceQuantity),
			packType: "NAR",
			quantityToDraw: 5m);
	}

	public void TestImportInventories_InvoiceLine_JI_InvoiceUQ()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: null,
			assertion: (invoiceLine) => AssertEquals("JI_InvoiceUQ", ZString.Empty, invoiceLine.JI_InvoiceUQ),
			packType: "NAR",
			quantityToDraw: 5m);
	}

	public void TestImportInventories_InvoiceLine_JI_BondedWhsQuantity_IntoWarehouseProcedure()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: (procedure) => procedure.ZZ6_IntoWarehouse = "Y",
			assertion: (invoiceLine) => AssertEquals("JI_BondedWhsQuantity", 5m, invoiceLine.JI_BondedWhsQuantity),
			quantityToDraw: 5m);
	}

	public void TestImportInventories_InvoiceLine_JI_BondedWhsQuantity_IntoInwardProcessingProcedure()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: (procedure) => procedure.ZZ6_IntoInwardProcessing = "Y",
			assertion: (invoiceLine) => AssertEquals("JI_BondedWhsQuantity", 5m, invoiceLine.JI_BondedWhsQuantity),
			quantityToDraw: 5m);
	}

	public void TestImportInventories_InvoiceLine_JI_BondedWhsQuantity_IntoOutwardProcessingProcedure()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: (procedure) => procedure.ZZ6_IntoOutwardProcessing = "Y",
			assertion: (invoiceLine) => AssertEquals("JI_BondedWhsQuantity", 5m, invoiceLine.JI_BondedWhsQuantity),
			quantityToDraw: 5m);
	}

	public void TestImportInventories_InvoiceLine_JI_BondedWhsQuantity_OutOfWarehouseProcedure()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: (procedure) => procedure.ZZ6_OutOfWarehouse = "Y",
			assertion: (invoiceLine) => AssertEquals("JI_BondedWhsQuantity", 5m, invoiceLine.JI_BondedWhsQuantity),
			quantityToDraw: 5m);
	}

	public void TestImportInventories_InvoiceLine_JI_BondedWhsQuantity_OutOfInwardProcessingProcedure()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: (procedure) => procedure.ZZ6_OutOfInwardProcessing = "Y",
			assertion: (invoiceLine) => AssertEquals("JI_BondedWhsQuantity", 5m, invoiceLine.JI_BondedWhsQuantity),
			quantityToDraw: 5m);
	}

	public void TestImportInventories_InvoiceLine_JI_BondedWhsQuantity_OutOfOutwardProcessingProcedure()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: (procedure) => procedure.ZZ6_OutofOutwardProcessing = "Y",
			assertion: (invoiceLine) => AssertEquals("JI_BondedWhsQuantity", 5m, invoiceLine.JI_BondedWhsQuantity),
			quantityToDraw: 5m);
	}

	public void TestImportInventories_InvoiceLine_JI_BondedWhsQuantity_NormalProcedure()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: null,
			assertion: (invoiceLine) => AssertEquals("JI_BondedWhsQuantity", 0m, invoiceLine.JI_BondedWhsQuantity),
			quantityToDraw: 5m);
	}

	public void TestImportInventories_InvoiceLine_JI_BondedWhsUnitQty_IntoWarehouseProcedure()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: (procedure) => procedure.ZZ6_IntoWarehouse = "Y",
			assertion: (invoiceLine) => AssertEquals("JI_BondedWhsUnitQty", "NAR", invoiceLine.JI_BondedWhsUnitQty),
			packType: "NAR");
	}

	public void TestImportInventories_InvoiceLine_JI_BondedWhsUnitQty_IntoInwardProcessingProcedure()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: (procedure) => procedure.ZZ6_IntoInwardProcessing = "Y",
			assertion: (invoiceLine) => AssertEquals("JI_BondedWhsUnitQty", "NAR", invoiceLine.JI_BondedWhsUnitQty),
			packType: "NAR");
	}

	public void TestImportInventories_InvoiceLine_JI_BondedWhsUnitQty_IntoOutwardProcessingProcedure()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: (procedure) => procedure.ZZ6_IntoOutwardProcessing = "Y",
			assertion: (invoiceLine) => AssertEquals("JI_BondedWhsUnitQty", "NAR", invoiceLine.JI_BondedWhsUnitQty),
			packType: "NAR");
	}

	public void TestImportInventories_InvoiceLine_JI_BondedWhsUnitQty_OutOfWarehouseProcedure()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: (procedure) => procedure.ZZ6_OutOfWarehouse = "Y",
			assertion: (invoiceLine) => AssertEquals("JI_BondedWhsUnitQty", "NAR", invoiceLine.JI_BondedWhsUnitQty),
			packType: "NAR");
	}

	public void TestImportInventories_InvoiceLine_JI_BondedWhsUnitQty_OutOfInwardProcessingProcedure()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: (procedure) => procedure.ZZ6_OutOfInwardProcessing = "Y",
			assertion: (invoiceLine) => AssertEquals("JI_BondedWhsUnitQty", "NAR", invoiceLine.JI_BondedWhsUnitQty),
			packType: "NAR");
	}

	public void TestImportInventories_InvoiceLine_JI_BondedWhsUnitQty_OutOfOutwardProcessingProcedure()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: (procedure) => procedure.ZZ6_OutofOutwardProcessing = "Y",
			assertion: (invoiceLine) => AssertEquals("JI_BondedWhsUnitQty", "NAR", invoiceLine.JI_BondedWhsUnitQty),
			packType: "NAR");
	}

	public void TestImportInventories_InvoiceLine_JI_BondedWhsUnitQty_NormalProcedure()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: null,
			assertion: (invoiceLine) => AssertEquals("JI_BondedWhsUnitQty", ZString.Empty, invoiceLine.JI_BondedWhsUnitQty),
			packType: "NAR");
	}

	public void TestImportInventories_InvoiceLine_JI_PreviousEntryNumber_IntoWarehouseProcedure()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: (procedure) => procedure.ZZ6_IntoWarehouse = "Y",
			assertion: (invoiceLine) => AssertEquals("JI_PreviousEntryNumber", ZString.Empty, invoiceLine.JI_PreviousEntryNumber));
	}

	public void TestImportInventories_InvoiceLine_JI_PreviousEntryNumber_IntoInwardProcessingProcedure()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: (procedure) => procedure.ZZ6_IntoInwardProcessing = "Y",
			assertion: (invoiceLine) => AssertEquals("JI_PreviousEntryNumber", ZString.Empty, invoiceLine.JI_PreviousEntryNumber));
	}

	public void TestImportInventories_InvoiceLine_JI_PreviousEntryNumber_IntoOutwardProcessingProcedure()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: (procedure) => procedure.ZZ6_IntoOutwardProcessing = "Y",
			assertion: (invoiceLine) => AssertEquals("JI_PreviousEntryNumber", ZString.Empty, invoiceLine.JI_PreviousEntryNumber));
	}

	public void TestImportInventories_InvoiceLine_JI_PreviousEntryNumber_OutOfWarehouseProcedure()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: (procedure) => procedure.ZZ6_OutOfWarehouse = "Y",
			assertion: (invoiceLine) => AssertEquals("JI_PreviousEntryNumber", "ENT1234", invoiceLine.JI_PreviousEntryNumber));
	}

	public void TestImportInventories_InvoiceLine_JI_PreviousEntryNumber_OutOfInwardProcessingProcedure()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: (procedure) => procedure.ZZ6_OutOfInwardProcessing = "Y",
			assertion: (invoiceLine) => AssertEquals("JI_PreviousEntryNumber", "ENT1234", invoiceLine.JI_PreviousEntryNumber));
	}

	public void TestImportInventories_InvoiceLine_JI_PreviousEntryNumber_OutOfOutwardProcessingProcedure()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: (procedure) => procedure.ZZ6_OutofOutwardProcessing = "Y",
			assertion: (invoiceLine) => AssertEquals("JI_PreviousEntryNumber", "ENT1234", invoiceLine.JI_PreviousEntryNumber));
	}

	public void TestImportInventories_InvoiceLine_JI_PreviousEntryNumber_NormalProcedure()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: null,
			assertion: (invoiceLine) => AssertEquals("JI_PreviousEntryNumber", ZString.Empty, invoiceLine.JI_PreviousEntryNumber));
	}

	public void TestImportInventories_InvoiceLine_JI_PreviousEntryLineNumber_IntoWarehouseProcedure()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: (procedure) => procedure.ZZ6_IntoWarehouse = "Y",
			assertion: (invoiceLine) => AssertEquals("JI_PreviousEntryLineNumber", (short)0, invoiceLine.JI_PreviousEntryLineNumber));
	}

	public void TestImportInventories_InvoiceLine_JI_PreviousEntryLineNumber_IntoInwardProcessingProcedure()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: (procedure) => procedure.ZZ6_IntoInwardProcessing = "Y",
			assertion: (invoiceLine) => AssertEquals("JI_PreviousEntryLineNumber", (short)0, invoiceLine.JI_PreviousEntryLineNumber));
	}

	public void TestImportInventories_InvoiceLine_JI_PreviousEntryLineNumber_IntoOutwardProcessingProcedure()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: (procedure) => procedure.ZZ6_IntoOutwardProcessing = "Y",
			assertion: (invoiceLine) => AssertEquals("JI_PreviousEntryLineNumber", (short)0, invoiceLine.JI_PreviousEntryLineNumber));
	}

	public void TestImportInventories_InvoiceLine_JI_PreviousEntryLineNumber_OutOfWarehouseProcedure()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: (procedure) => procedure.ZZ6_OutOfWarehouse = "Y",
			assertion: (invoiceLine) => AssertEquals("JI_PreviousEntryLineNumber", (short)1, invoiceLine.JI_PreviousEntryLineNumber));
	}

	public void TestImportInventories_InvoiceLine_JI_PreviousEntryLineNumber_OutOfInwardProcessingProcedure()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: (procedure) => procedure.ZZ6_OutOfInwardProcessing = "Y",
			assertion: (invoiceLine) => AssertEquals("JI_PreviousEntryLineNumber", (short)1, invoiceLine.JI_PreviousEntryLineNumber));
	}

	public void TestImportInventories_InvoiceLine_JI_PreviousEntryLineNumber_OutOfOutwardProcessingProcedure()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: (procedure) => procedure.ZZ6_OutofOutwardProcessing = "Y",
			assertion: (invoiceLine) => AssertEquals("JI_PreviousEntryLineNumber", (short)1, invoiceLine.JI_PreviousEntryLineNumber));
	}

	public void TestImportInventories_InvoiceLine_JI_PreviousEntryLineNumber_NormalProcedure()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: null,
			assertion: (invoiceLine) => AssertEquals("JI_PreviousEntryLineNumber", (short)0, invoiceLine.JI_PreviousEntryLineNumber));
	}

	void AssertUpdateOutwardLinesWithInventoryDetails(Action<RefCusProcedure> setUp, Action<JobComInvoiceLine> assertion, string packType = "NO", decimal quantityToDraw = 6)
	{
		var refDataHelper = new ITUniversalReferenceTestDataHelper(Factory);
		var procedure = refDataHelper.CreateRefCusProcedure("IT", "A", "40", "71", "", "Test", "IMP");

		declaration.JE_MessageType = "IMP";
		declaration.JE_OH_Importer = Helper.Importer.PK;
		declaration.JE_OA_Representative = ZGuid.Empty;
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;
		var invoice = declaration.Invoices.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Procedure = "40";

		var receive = Helper.GetNewWhsReceive(Helper.WhsWarehouse.PK, Helper.Importer.PK);
		var receiveLine = Helper.GetNewWhsReceiveLine(receive.PK, Helper.Part.PK, ZString.Empty, 1m, 100m, 100m, packType, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-1", ZDateTime.Today.AddMonths(-1));
		var receiveLineCustomsData = Helper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, packType, ZString.Empty, "ENT1234", 1);
		receiveLineCustomsData.WB_InwardProcedure = "7100";

		var inventory = receiveLine.Inventory;
		setUp?.Invoke(procedure);
		Factory.Save();

		var inventorySelectionHeader = new InventorySelectionHeader(declaration);
		var inventoryWrapper = new Customs.Business.WhsInventoryWrapper(inventory, inventorySelectionHeader);
		inventoryWrapper.QuantityToDraw = quantityToDraw;

		inventorySelectionHeader.ImportInventories();
		AssertEquals("InvoiceLines Count", 1, invoice.InvoiceLines.Count);
		var invoiceLine = invoice.InvoiceLines[0];
		CombineAssertions(() => assertion?.Invoke(invoiceLine));
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return new InventorySelectionHeader(declaration);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}

	WhsDataTestHelper Helper => helper ?? (helper = new WhsDataTestHelper(Factory));
	WhsDataTestHelper helper;

	JobDeclaration declaration;
}
