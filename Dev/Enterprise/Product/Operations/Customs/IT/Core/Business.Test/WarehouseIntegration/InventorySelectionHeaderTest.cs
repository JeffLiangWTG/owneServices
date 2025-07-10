using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Warehouse.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(InventorySelectionHeader))]
sealed class InventorySelectionHeaderTest : NonPersistentBusinessObjectTestCase
{
	public void TestSelectionLines()
	{
		var declaration = Factory.New<JobDeclaration>();
		var inventorySelectionHeader = new InventorySelectionHeader(declaration);
		AssertType<InventorySelectionLineCollection>("SelectionLines Type", inventorySelectionHeader.SelectionLines);
	}

	public void TestUpdateOutwardLinesWithInventoryDetails_DoesNotFillProcedureIfEntryInstructionIsNotFound()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: (customsData, _) => customsData.WB_InwardProcedure = "",
			assertion: (invoiceLine) => AssertEquals("JI_Procedure", "", invoiceLine.JI_Procedure));
	}

	public void TestUpdateOutwardLinesWithInventoryDetails_DoesNotFillProcedureIfEntryInstructionProcedureIsEmpty()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: (customsData, entryInstruction) =>
			{
				entryInstruction.CEI_Procedure = "";
				customsData.WB_InwardProcedure = "7100";
			},
			assertion: (invoiceLine) => AssertEquals("JI_Procedure", "", invoiceLine.JI_Procedure));
	}

	public void TestUpdateOutwardLinesWithInventoryDetails_DoesNotFillProcedureIfEntryInstructionProcedureIsNot2CharsLength()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: (customsData, entryInstruction) =>
			{
				entryInstruction.CEI_Procedure = "4";
				customsData.WB_InwardProcedure = "7100";
			},
			assertion: (invoiceLine) => AssertEquals("JI_Procedure", "", invoiceLine.JI_Procedure));
	}

	public void TestUpdateOutwardLinesWithInventoryDetails_DoesNotFillProcedureIfInwardProcedureIsEmpty()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: (customsData, entryInstruction) =>
			{
				entryInstruction.CEI_Procedure = "40";
				customsData.WB_InwardProcedure = "";
			},
			assertion: (invoiceLine) => AssertEquals("JI_Procedure", "", invoiceLine.JI_Procedure));
	}

	public void TestUpdateOutwardLinesWithInventoryDetails_DoesNotFillProcedureIfInwardProcedureIsNot2CharsLength()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: (customsData, entryInstruction) =>
			{
				entryInstruction.CEI_Procedure = "40";
				customsData.WB_InwardProcedure = "7";
			},
			assertion: (invoiceLine) => AssertEquals("JI_Procedure", "", invoiceLine.JI_Procedure));
	}

	public void TestUpdateOutwardLinesWithInventoryDetails_FillProcedure()
	{
		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: (customsData, entryInstruction) =>
			{
				entryInstruction.CEI_Procedure = "40";
				customsData.WB_InwardProcedure = "7100";
			},
			assertion: (invoiceLine) => AssertEquals("JI_Procedure", "4071", invoiceLine.JI_Procedure));
	}

	public void TestUpdateOutwardLinesWithInventoryDetails_FillProcedureShouldNotClearBondedQtyAndUnit()
	{
		var refDataHelper = new ITUniversalReferenceTestDataHelper(Factory);
		refDataHelper.CreateRefCusProcedure("IT", "A", "40", "71", "", "Test", "IMP", intoWarehouse: true);

		AssertUpdateOutwardLinesWithInventoryDetails(
			setUp: (customsData, entryInstruction) =>
			{
				entryInstruction.CEI_Procedure = "40";
				customsData.WB_InwardProcedure = "7100";
			},
			assertion: (invoiceLine) =>
			{
				AssertEquals("JI_Procedure", "4071", invoiceLine.JI_Procedure);
				AssertEquals("JI_BondedWhsQuantity", 10m, invoiceLine.JI_BondedWhsQuantity);
				AssertEquals("JI_BondedWhsUnitQty", "NO", invoiceLine.JI_BondedWhsUnitQty);
			});
	}

	void AssertUpdateOutwardLinesWithInventoryDetails(Action<IWhsBondedWarehouseAttribute, CusEntryInstruction> setUp, Action<JobComInvoiceLine> assertion)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_OH_Importer = Helper.Importer.PK;
		declaration.JE_OA_Representative = ZGuid.Empty;
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;
		var invoice = declaration.Invoices.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		var receive = Helper.GetNewWhsReceive(Helper.WhsWarehouse.PK, Helper.Importer.PK);
		var receiveLine = Helper.GetNewWhsReceiveLine(receive.PK, Helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-1", ZDateTime.Today.AddMonths(-1));
		var receiveLineCustomsData = Helper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", ZString.Empty, "ENT1234", 1);
		var inventory = receiveLine.Inventory;
		setUp?.Invoke(receiveLineCustomsData, entryInstruction);
		Factory.Save();

		var inventorySelectionHeader = new InventorySelectionHeader(declaration);
		var inventoryWrapper = new Customs.Business.WhsInventoryWrapper(inventory, inventorySelectionHeader);
		inventoryWrapper.QuantityToDraw = 10;

		inventorySelectionHeader.ImportInventories();
		AssertEquals("Count", 1, invoice.InvoiceLines.Count);
		var invoiceLine = invoice.InvoiceLines[0];
		CombineAssertions(() => assertion?.Invoke(invoiceLine));
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		return new InventorySelectionHeader(declaration);
	}

	WhsDataTestHelper Helper => helper ?? (helper = new WhsDataTestHelper(Factory));
	WhsDataTestHelper helper;
}
