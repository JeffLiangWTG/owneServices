using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(InventorySelectionHeader))]
sealed class InventorySelectionHeaderPreviousDocumentFillerTest : NonPersistentBusinessObjectTestCase
{
	#region Tests

	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new InventorySelectionHeaderPreviousDocumentFiller(null));
		AssertNoExceptionThrown(() => new InventorySelectionHeaderPreviousDocumentFiller(Factory.New<JobComInvoiceLine>()));
	}

	public void TestPreviousDocuments_NonUcc6_Export()
	{
		AssertUpdateOutwardLinesWithInventoryDetailsExport(
			setUp: (invoiceLine, _) =>
			{
				invoiceLine.JI_BondedWhsQuantity = 4;
				invoiceLine.JI_Weight = 6m;
				invoiceLine.JI_WeightUQ = "KG";
			},
			assertion: (invoiceLine) => AssertEquals("Previous Documents Count", 0, invoiceLine.PreviousDocuments.Count),
			packType: "KG");
	}

	public void TestPreviousDocuments_EntryWithoutDash_Ucc6_Export()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			AssertUpdateOutwardLinesWithInventoryDetailsExport(
				setUp: (invoiceLine, package) =>
				{
					invoiceLine.JI_BondedWhsQuantity = 4;
					invoiceLine.JI_Weight = 6m;
					invoiceLine.JI_WeightUQ = "KG";
					package.CW_PackQty = 5;
					package.CW_PackType = "CT";
				},
				assertion: (invoiceLine) =>
				{
					AssertEquals("Previous Documents Count", 1, invoiceLine.PreviousDocuments.Count);
					var previousDocument = invoiceLine.PreviousDocuments[0];
					AssertEquals($"When JI_PreviousEntryNumber is {invoiceLine.JI_PreviousEntryNumber}, CSI_Code", "NMRN", previousDocument.CSI_Code);
					AssertEquals("CSI_ReferenceNumber", "ENT1234", previousDocument.CSI_ReferenceNumber);
					AssertEquals("CSI_PackQty", 5, previousDocument.CSI_PackQty);
					AssertEquals("CSI_PackType", "CT", previousDocument.CSI_PackType);
					AssertEquals("CSI_Quantity", 6m, previousDocument.CSI_Quantity);
					AssertEquals("CSI_UnitOfQuantity", "KG", previousDocument.CSI_UnitOfQuantity);
					AssertEquals("CSI_ItemNumber", 1, previousDocument.CSI_ItemNumber);
				},
				packType: "KG");
		}
	}

	public void TestPreviousDocuments_EntryWithDash_Ucc6_Export()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			AssertUpdateOutwardLinesWithInventoryDetailsExport(
				setUp: (invoiceLine, package) =>
				{
					invoiceLine.JI_BondedWhsQuantity = 4;
					invoiceLine.JI_Weight = 6m;
					invoiceLine.JI_WeightUQ = "KG";
					package.CW_PackQty = 7;
					package.CW_PackType = "PT";
				},
				assertion: (invoiceLine) =>
				{
					AssertEquals("Previous Documents Count", 1, invoiceLine.PreviousDocuments.Count);
					var previousDocument = invoiceLine.PreviousDocuments[0];
					AssertEquals($"When JI_PreviousEntryNumber is {invoiceLine.JI_PreviousEntryNumber}, CSI_Code", "N337", previousDocument.CSI_Code);
					AssertEquals("CSI_ReferenceNumber", "ENT1234-12", previousDocument.CSI_ReferenceNumber);
					AssertEquals("CSI_PackQty", 7, previousDocument.CSI_PackQty);
					AssertEquals("CSI_PackType", "PT", previousDocument.CSI_PackType);
					AssertEquals("CSI_Quantity", 6m, previousDocument.CSI_Quantity);
					AssertEquals("CSI_UnitOfQuantity", "KG", previousDocument.CSI_UnitOfQuantity);
					AssertEquals("CSI_ItemNumber", 1, previousDocument.CSI_ItemNumber);
				},
				entryNo: "ENT1234-12",
				packType: "KG");
		}
	}

	public void TestPreviousDocuments_EntryWithoutDash_Import()
	{
		AssertUpdateOutwardLinesWithInventoryDetailsImport(
			setUp: (invoiceLine, package) =>
			{
				invoiceLine.JI_CustomsQuantity = 4m;
				invoiceLine.JI_CustomsUnitQty = "KGM";
				invoiceLine.JI_BondedWhsQuantity = 5m;
				invoiceLine.JI_Weight = 6m;
				invoiceLine.JI_WeightUQ = "KG";
				package.CW_PackQty = 9;
				package.CW_PackType = "CT";
			},
			assertion: (invoiceLine) =>
			{
				AssertEquals("Previous Documents Count", 1, invoiceLine.PreviousDocuments.Count);
				var previousDocument = invoiceLine.PreviousDocuments[0];
				AssertEquals("CSI_Procedure", "MR1", previousDocument.CSI_Procedure);
				AssertEquals("CSI_Code", "MRN", previousDocument.CSI_Code);
				AssertEquals("CSI_ReferenceNumber2", "ENT1234", previousDocument.CSI_ReferenceNumber2);
				AssertEquals("CSI_ItemNumber", 1, previousDocument.CSI_LineNo);
				AssertEquals("CSI_Quantity", 4m, previousDocument.CSI_Quantity);
				AssertEquals("CSI_UnitOfQuantity", "KG", previousDocument.CSI_UnitOfQuantity);
				AssertEquals("CSI_PackQty", 9, previousDocument.CSI_PackQty);
				AssertEquals("CSI_PackType", "CT", previousDocument.CSI_PackType);
				AssertEquals("CSI_Quantity3", 6m, previousDocument.CSI_Quantity3);
				AssertEquals("CSI_UnitOfQuantity3", "KG", previousDocument.CSI_UnitOfQuantity3);
			},
			packType: "LTR");
	}

	public void TestPreviousDocuments_EntryWithoutDash_Import_ReferenceNumber2Truncated()
	{
		AssertUpdateOutwardLinesWithInventoryDetailsImport(
			setUp: (invoiceLine, package) => { },
			assertion: (invoiceLine) =>
			{
				AssertEquals("Previous Documents Count", 1, invoiceLine.PreviousDocuments.Count);
				var previousDocument = invoiceLine.PreviousDocuments[0];
				AssertEquals("CSI_ReferenceNumber2", "012345678901234567", previousDocument.CSI_ReferenceNumber2);
			},
			entryNo: "012345678901234567890123456789012");
	}

	public void TestPreviousDocuments_EntryWithDash_Import()
	{
		AssertUpdateOutwardLinesWithInventoryDetailsImport(
			setUp: (invoiceLine, package) =>
			{
				invoiceLine.JI_CustomsQuantity = 4m;
				invoiceLine.JI_CustomsUnitQty = "KGM";
				invoiceLine.JI_BondedWhsQuantity = 5m;
				invoiceLine.JI_Weight = 6m;
				invoiceLine.JI_WeightUQ = "KG";
				package.CW_PackQty = 3;
				package.CW_PackType = "CT";
			},
			assertion: (invoiceLine) =>
			{
				AssertEquals("Previous Documents Count", 1, invoiceLine.PreviousDocuments.Count);
				var previousDocument = invoiceLine.PreviousDocuments[0];
				AssertEquals("CSI_Procedure", "7", previousDocument.CSI_Procedure);
				AssertEquals("CSI_Code", "ZZZ", previousDocument.CSI_Code);
				AssertEquals("CSI_ReferenceNumber", "95884", previousDocument.CSI_ReferenceNumber);
				AssertEquals("CSI_DateOfIssue", new ZDateTime(2021, 5, 3), previousDocument.CSI_DateOfIssue);
				AssertEquals("CSI_CustomsOffice", "IT279100", previousDocument.CSI_CustomsOffice);
				AssertEquals("CSI_LineNo", 1, previousDocument.CSI_LineNo);
				AssertEquals("CSI_Quantity", 4m, previousDocument.CSI_Quantity);
				AssertEquals("CSI_UnitOfQuantity", "KG", previousDocument.CSI_UnitOfQuantity);
				AssertEquals("CSI_PackQty", 3, previousDocument.CSI_PackQty);
				AssertEquals("CSI_PackType", "CT", previousDocument.CSI_PackType);
				AssertEquals("CSI_Quantity3", 6m, previousDocument.CSI_Quantity3);
				AssertEquals("CSI_UnitOfQuantity3", "KG", previousDocument.CSI_UnitOfQuantity3);
			},
			entryNo: "7-95884A-03052021-279100",
			packType: "LTR");
	}

	public void TestPreviousDocuments_EntryWithDash_Import_ProcedureTruncated()
	{
		AssertUpdateOutwardLinesWithInventoryDetailsImport(
			setUp: (invoiceLine, package) => { },
			assertion: (invoiceLine) =>
			{
				AssertEquals("Previous Documents Count", 1, invoiceLine.PreviousDocuments.Count);
				var previousDocument = invoiceLine.PreviousDocuments[0];
				AssertEquals("CSI_Procedure", "XXXX", previousDocument.CSI_Procedure);
			},
			entryNo: "XXXXX-95884A-03052021-279100");
	}

	public void TestPreviousDocuments_EntryWithDash_Import_CustomsOfficeTruncated()
	{
		AssertUpdateOutwardLinesWithInventoryDetailsImport(
			setUp: (invoiceLine, package) => { },
			assertion: (invoiceLine) =>
			{
				AssertEquals("Previous Documents Count", 1, invoiceLine.PreviousDocuments.Count);
				var previousDocument = invoiceLine.PreviousDocuments[0];
				AssertEquals("CSI_CustomsOffice", "ITXXXXXXXX", previousDocument.CSI_CustomsOffice);
			},
			entryNo: "7-95884A-03052021-XXXXXXXXXXX");
	}

	public void TestPreviousDocuments_EntryWithDash_ProcedureMissing_Import()
	{
		AssertUpdateOutwardLinesWithInventoryDetailsImport(
			setUp: (_, __) => { },
			assertion: (invoiceLine) =>
			{
				AssertEquals("Previous Documents Count", 1, invoiceLine.PreviousDocuments.Count);
				var previousDocument = invoiceLine.PreviousDocuments[0];

				AssertEquals("CSI_Procedure", ZString.Empty, previousDocument.CSI_Procedure);
				AssertEquals("CSI_ReferenceNumber", "95884", previousDocument.CSI_ReferenceNumber);
				AssertEquals("CSI_DateOfIssue", new ZDateTime(2021, 5, 3), previousDocument.CSI_DateOfIssue);
				AssertEquals("CSI_CustomsOffice", "IT279100", previousDocument.CSI_CustomsOffice);
			},
			entryNo: "-95884A-03052021-279100");
	}

	public void TestPreviousDocuments_EntryWithDash_ReferenceNumberMissing_Import()
	{
		AssertUpdateOutwardLinesWithInventoryDetailsImport(
			setUp: (_, __) => { },
			assertion: (invoiceLine) =>
			{
				AssertEquals("Previous Documents Count", 1, invoiceLine.PreviousDocuments.Count);
				var previousDocument = invoiceLine.PreviousDocuments[0];

				AssertEquals("CSI_Procedure", "7", previousDocument.CSI_Procedure);
				AssertEquals("CSI_ReferenceNumber", ZString.Empty, previousDocument.CSI_ReferenceNumber);
				AssertEquals("CSI_DateOfIssue", new ZDateTime(2021, 5, 3), previousDocument.CSI_DateOfIssue);
				AssertEquals("CSI_CustomsOffice", "IT279100", previousDocument.CSI_CustomsOffice);
			},
			entryNo: "7--03052021-279100");
	}

	public void TestPreviousDocuments_EntryWithDash_DateOfIssueMissing_Import()
	{
		AssertUpdateOutwardLinesWithInventoryDetailsImport(
			setUp: (_, __) => { },
			assertion: (invoiceLine) =>
			{
				AssertEquals("Previous Documents Count", 1, invoiceLine.PreviousDocuments.Count);
				var previousDocument = invoiceLine.PreviousDocuments[0];

				AssertEquals("CSI_Procedure", "7", previousDocument.CSI_Procedure);
				AssertEquals("CSI_ReferenceNumber", "95884", previousDocument.CSI_ReferenceNumber);
				AssertEquals("CSI_DateOfIssue", ZDateTime.Empty, previousDocument.CSI_DateOfIssue);
				AssertEquals("CSI_CustomsOffice", "IT279100", previousDocument.CSI_CustomsOffice);
			},
			entryNo: "7-95884A--279100");
	}

	public void TestPreviousDocuments_EntryWithDash_CustomsOfficeMissing_Import()
	{
		AssertUpdateOutwardLinesWithInventoryDetailsImport(
			setUp: (_, __) => { },
			assertion: (invoiceLine) =>
			{
				AssertEquals("Previous Documents Count", 1, invoiceLine.PreviousDocuments.Count);
				var previousDocument = invoiceLine.PreviousDocuments[0];

				AssertEquals("CSI_Procedure", "7", previousDocument.CSI_Procedure);
				AssertEquals("CSI_ReferenceNumber", "95884", previousDocument.CSI_ReferenceNumber);
				AssertEquals("CSI_DateOfIssue", new ZDateTime(2021, 5, 3), previousDocument.CSI_DateOfIssue);
				AssertEquals("CSI_CustomsOffice", ZString.Empty, previousDocument.CSI_CustomsOffice);
			},
			entryNo: "7-95884A-03052021-");
	}

	public void TestPreviousDocuments_EntryWithDash_AllNumbersMissing_Import()
	{
		AssertUpdateOutwardLinesWithInventoryDetailsImport(
			setUp: (_, __) => { },
			assertion: (invoiceLine) => AssertEquals("Previous Documents Count", 0, invoiceLine.PreviousDocuments.Count),
			entryNo: "---");
	}

	public void TestPreviousDocuments_InvalidEntryWithDash_Import()
	{
		AssertUpdateOutwardLinesWithInventoryDetailsImport(
			setUp: (_, __) => { },
			assertion: (invoiceLine) => AssertEquals("Previous Documents Count", 0, invoiceLine.PreviousDocuments.Count),
			entryNo: "7-ASWEWQ");
	}

	public void TestPreviousDocuments_EntryWithDash_InvalidDate_Import()
	{
		AssertUpdateOutwardLinesWithInventoryDetailsImport(
			setUp: (_, __) => { },
			assertion: (invoiceLine) =>
			{
				AssertEquals("Previous Documents Count", 1, invoiceLine.PreviousDocuments.Count);
				var previousDocument = invoiceLine.PreviousDocuments[0];

				AssertEquals("CSI_Procedure", "7", previousDocument.CSI_Procedure);
				AssertEquals("CSI_ReferenceNumber", "1113", previousDocument.CSI_ReferenceNumber);
				AssertEquals("CSI_DateOfIssue", ZDateTime.Empty, previousDocument.CSI_DateOfIssue);
				AssertEquals("CSI_CustomsOffice", "IT214235", previousDocument.CSI_CustomsOffice);
			},
			entryNo: "7-1113WQ-41402345-214235");
	}

	public void Test_AddPreviousDocumentIfRequired_ForInventoryWithQuantityToDraw_Ucc6_Export()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PreviousEntryNumber = "-";

			AssertEquals("PreviousDocuments should be empty", 0, invoiceLine.PreviousDocuments.Count);
			CreateInventorySelectionHeaderPreviousDocumentFiller(invoiceLine, null).AddPreviousDocumentIfRequired();
			AssertEquals("PreviousDocuments should have 1 element", 1, invoiceLine.PreviousDocuments.Count);

			AssertEquals($"Latest added PreviousDocument code should be {UniversalReferenceConstants.RefCusCodeListTypes.TemporaryStorageDeclaration}",
				UniversalReferenceConstants.RefCusCodeListTypes.TemporaryStorageDeclaration,
				invoiceLine.PreviousDocuments.Last().CSI_Code);

			var inventoryWrapper = CreateWhsInventoryWrapper(declaration);
			inventoryWrapper.QuantityToDraw = ZDecimal.Zero;
			CreateInventorySelectionHeaderPreviousDocumentFiller(invoiceLine, inventoryWrapper).AddPreviousDocumentIfRequired();
			AssertEquals("PreviousDocuments should have 2 elements", 2, invoiceLine.PreviousDocuments.Count);

			AssertEquals($"Latest added PreviousDocument code should be {UniversalReferenceConstants.RefCusCodeListTypes.TemporaryStorageDeclaration}",
				UniversalReferenceConstants.RefCusCodeListTypes.TemporaryStorageDeclaration,
				invoiceLine.PreviousDocuments.Last().CSI_Code);

			inventoryWrapper.QuantityToDraw = 10;
			CreateInventorySelectionHeaderPreviousDocumentFiller(invoiceLine, inventoryWrapper).AddPreviousDocumentIfRequired();
			AssertEquals("PreviousDocuments should have 3 elements", 3, invoiceLine.PreviousDocuments.Count);

			AssertEquals($"Latest added PreviousDocument code should be {UniversalReferenceConstants.RefCusCodeListTypes.DeclarationOrNotificationMrn}",
				UniversalReferenceConstants.RefCusCodeListTypes.DeclarationOrNotificationMrn,
				invoiceLine.PreviousDocuments.Last().CSI_Code);
		}
	}

	#endregion

	#region Assertion Routines

	void AssertUpdateOutwardLinesWithInventoryDetailsExport(Action<JobComInvoiceLine, Package> setUp, Action<JobComInvoiceLine> assertion, string entryNo = "ENT1234", short entryLineNo = 1, string packType = "NO")
	{
		declaration.JE_OH_Supplier = Helper.Supplier.PK;
		AssertUpdateOutwardLinesWithInventoryDetails(setUp, assertion, Helper.Supplier.PK, entryNo, entryLineNo, packType);
	}

	void AssertUpdateOutwardLinesWithInventoryDetailsImport(Action<JobComInvoiceLine, Package> setUp, Action<JobComInvoiceLine> assertion, string entryNo = "ENT1234", short entryLineNo = 1, string packType = "NO")
	{
		declaration.JE_MessageType = "IMP";
		declaration.JE_OH_Importer = Helper.Importer.PK;
		AssertUpdateOutwardLinesWithInventoryDetails(setUp, assertion, Helper.Importer.PK, entryNo, entryLineNo, packType);
	}

	void AssertUpdateOutwardLinesWithInventoryDetails(Action<JobComInvoiceLine, Package> setUp, Action<JobComInvoiceLine> assertion, ZGuid clientPk, string entryNo, short entryLineNo, string packType)
	{
		declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_PreviousEntryNumber = entryNo;
		invoiceLine.JI_PreviousEntryLineNumber = entryLineNo;
		var package = Factory.New<Package>();

		var receive = Helper.GetNewWhsReceive(Helper.WhsWarehouse.PK, clientPk);
		Helper.GetNewWhsReceiveLine(receive.PK, Helper.Part.PK, ZString.Empty, 1m, 100m, 100m, packType, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, $"{entryNo}-{entryLineNo}", ZDateTime.Today.AddMonths(-1));
		setUp?.Invoke(invoiceLine, package);
		Factory.Save();

		var pivot = invoiceLine.PackagesPivot.AddNew();
		pivot.CHC_CW = package.PK;
		var inventorySelectionHeader = new InventorySelectionHeader(declaration);
		var invoiceLines = new List<JobComInvoiceLine> { invoiceLine };
		inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
		CombineAssertions(() => assertion?.Invoke(invoiceLine));
	}

	#endregion

	#region Helper Methods

	WhsInventoryWrapper CreateWhsInventoryWrapper(JobDeclaration declaration)
	{
		var receive = Helper.GetNewWhsReceive(Helper.WhsWarehouse.PK, Helper.Importer.PK);
		var receiveLine = Helper.GetNewWhsReceiveLine(receive.PK, Helper.Part.PK, ZString.Empty,
			1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty,
			ZString.Empty, ZString.Empty, "ENT1234-1", ZDateTime.Today.AddMonths(-1));
		var inventory = receiveLine.Inventory;
		Factory.Save();

		var inventorySelectionHeader = new InventorySelectionHeader(declaration);
		var inventoryWrapper = new WhsInventoryWrapper(inventory, inventorySelectionHeader);
		return inventoryWrapper;
	}

	InventorySelectionHeaderPreviousDocumentFiller CreateInventorySelectionHeaderPreviousDocumentFiller(JobComInvoiceLine invoiceLine,
		WhsInventoryWrapper inventoryWrapper = null) => new InventorySelectionHeaderPreviousDocumentFiller(invoiceLine, inventoryWrapper);

	#endregion

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
