using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(ExportInventorySelectionHeader))]
	public sealed class ExportInventorySelectionHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestJI_Procedure()
		{
			// Arrange
			var invoiceLine = CreateInvoiceLine(declaration, PreviousEntryNumber);

			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Supplier.PK);
			var receiveLine = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, BondedEntryKey, ZDateTime.Today.AddMonths(-1));
			var receiveLineCustomsData = helper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", ZString.Empty, PreviousEntryNumber, 1);

			receiveLineCustomsData.WB_InwardProcedure = "1234";
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Procedure = "56";
			invoiceLine.JI_CEI = entryInstruction.PK;
			Factory.Save();

			//Act
			header.UpdateOutwardLinesWithInventoryDetails(new[] { invoiceLine });

			// Assert
			AssertEquals("3171", invoiceLine.JI_Procedure);
		}

		public void TestFillInventoryDetails_NoCharges()
		{
			AssertUpdateOutwardLinesWithInventoryDetails(
				mocker: (receiveLine, customsData, invoiceLine) =>
				{
					receiveLine.WE_TransactionQuantity = 10m;
					invoiceLine.JI_BondedWhsQuantity = 2m;

					AddWarehouseCustomsAttributeAddInfo(customsData, "CCT", "*Amount=240*ChargeType=ADD*Currency=CNY*IsDutiable=Y*IsGSTApplicable=Y*IsIncludedInITOT=Y*IsStatisticalValueApplicable=Y");
					AddWarehouseCustomsAttributeAddInfo(customsData, "CCT", "*Amount=320*ChargeType=DED*Currency=USD*IsDutiable=*IsGSTApplicable=*IsIncludedInITOT=*IsStatisticalValueApplicable=Y");
				},
				assertion: invoiceLine =>
				{
					AssertEquals("3171", invoiceLine.JI_Procedure);
					AssertEquals(0, invoiceLine.Charges.Count);
				});
		}

		public void TestJI_FormattedProcedure()
		{
			// Arrange
			var invoiceLine = CreateInvoiceLine(declaration, PreviousEntryNumber);

			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Supplier.PK);
			var receiveLine = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, BondedEntryKey, ZDateTime.Today.AddMonths(-1));
			var receiveLineCustomsData = helper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", ZString.Empty, PreviousEntryNumber, 1);

			receiveLineCustomsData.WB_InwardProcedure = "1234";
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Procedure = "56";
			invoiceLine.JI_CEI = entryInstruction.PK;
			Factory.Save();

			//Act
			header.UpdateOutwardLinesWithInventoryDetails(new[] { invoiceLine });

			// Assert
			AssertEquals("3171", invoiceLine.JI_Procedure);
		}

		public void TestInvoiceGrouping()
		{
			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK);

			var receiveLine1 = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-1", ZDateTime.Today.AddMonths(-1));
			var addInfo1 = "LinePrice=500.0000*LinePriceCurrency=USD*IncotermCode=FOB*IncotermPlace=Frankfurt*TransNature=21";
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine1.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo1, PreviousEntryNumber, 1);

			var receiveLine2 = helper.GetNewWhsReceiveLine(receive.PK, helper.Part2.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-2", ZDateTime.Today.AddMonths(-1));
			var addInfo2 = "LinePrice=1000.0000*LinePriceCurrency=USD*IncotermCode=FOB*IncotermPlace=Frankfurt*TransNature=21";
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine2.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo2, PreviousEntryNumber, 2);

			var part3 = helper.CreateProduct(helper.Importer.PK, "~~3");
			var receiveLine3 = helper.GetNewWhsReceiveLine(receive.PK, part3.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-3", ZDateTime.Today.AddMonths(-1));
			var addInfo3 = "LinePrice=3000.0000*LinePriceCurrency=EUR*IncotermCode=FOB*IncotermPlace=Frankfurt*TransNature=21";
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine3.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo3, PreviousEntryNumber, 3);

			var part4 = helper.CreateProduct(helper.Importer.PK, "~~4");
			var receiveLine4 = helper.GetNewWhsReceiveLine(receive.PK, part4.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-4", ZDateTime.Today.AddMonths(-1));
			var addInfo4 = "LinePrice=4000.0000*LinePriceCurrency=USD*IncotermCode=XXX*IncotermPlace=Frankfurt*TransNature=21";
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine4.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo4, PreviousEntryNumber, 4);

			var part5 = helper.CreateProduct(helper.Importer.PK, "~~5");
			var receiveLine5 = helper.GetNewWhsReceiveLine(receive.PK, part5.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-5", ZDateTime.Today.AddMonths(-1));
			var addInfo5 = "LinePrice=5000.0000*LinePriceCurrency=USD*IncotermCode=FOB*IncotermPlace=Mainz*TransNature=21";
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine5.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo5, PreviousEntryNumber, 5);

			var part6 = helper.CreateProduct(helper.Importer.PK, "~~6");
			var receiveLine6 = helper.GetNewWhsReceiveLine(receive.PK, part6.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-6", ZDateTime.Today.AddMonths(-1));
			var addInfo6 = "LinePrice=6000.0000*LinePriceCurrency=USD*IncotermCode=FOB*IncotermPlace=Frankfurt*TransNature=22";
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine6.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo6, PreviousEntryNumber, 6);
			Factory.Save();

			helper.WhsHelper.WhsReceiveAllocateLocationsMock(receive.PK);
			receive.FinaliseDocketWithoutUserConfirmation();
			receiveLine1.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			receiveLine2.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			receiveLine3.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			receiveLine4.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			receiveLine5.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			receiveLine6.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;

			Factory.Save();

			header.UpdateSelectionLinesDetails(new[] { receiveLine1.Inventory, receiveLine2.Inventory, receiveLine3.Inventory, receiveLine4.Inventory, receiveLine5.Inventory, receiveLine6.Inventory });
			header.SelectionLines[0].US_ProductQtyToDraw = 100m;
			header.SelectionLines[1].US_ProductQtyToDraw = 100m;
			header.SelectionLines[2].US_ProductQtyToDraw = 100m;
			header.SelectionLines[3].US_ProductQtyToDraw = 100m;
			header.SelectionLines[4].US_ProductQtyToDraw = 100m;
			header.SelectionLines[5].US_ProductQtyToDraw = 100m;
			header.ImportInventories();

			CombineAssertions(() =>
			{
				AssertEquals("Invoices: count", 2, declaration.Invoices.Count);

				var invoiceEUR = GetInvoice(declaration, "EUR");
				AssertNotNull("InvoiceEUR exists", invoiceEUR);
				AssertEquals("InvoiceEUR: InvoiceAmount", (ZDecimal)3000, invoiceEUR.JZ_InvoiceAmount);

				var invoiceUSD = GetInvoice(declaration, "USD");
				AssertNotNull("InvoiceUSD exists", invoiceUSD);
				AssertEquals("InvoiceUSD: InvoiceAmount", (ZDecimal)16500, invoiceUSD.JZ_InvoiceAmount);
			});
		}

		public void TestCreatePreviousProcedure_Export()
		{
			declaration.JE_DeclarationReference = "JobNumber";
			var invoiceLine = CreateInvoiceLine(declaration, PreviousEntryNumber);
			invoiceLine.JI_PartNo = helper.Part.OP_PartNum;
			invoiceLine.JI_BondedWhsQuantity = 50;
			invoiceLine.JI_BondedWhsUnitQty = "NO";
			var initialPreviousProcedure1 = invoiceLine.PreviousProcedures.AddNew();
			initialPreviousProcedure1.CSI_ReferenceNumber = "REF_INITIAL1";
			var initialPreviousProcedure2 = invoiceLine.PreviousProcedures.AddNew();
			initialPreviousProcedure2.CSI_ReferenceNumber = "REF_INITIAL2";

			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Supplier.PK);
			var receiveLine = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, BondedEntryKey, ZDateTime.Today.AddMonths(-1));
			var receiveLineCustomsData = helper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", ZString.Empty, PreviousEntryNumber, 1);
			receiveLineCustomsData.WB_InwardProcedure = "1234";
			receiveLineCustomsData.WB_Tariff = "67890";
			var pivotImport = helper.Pivot;
			pivotImport.CI_ChildType = Common.Shared.ClassificationTypeList.Codes.Import;
			pivotImport.CI_FormattedTariffNum = "123456";
			var pivotExport = helper.Pivot2;
			pivotExport.CI_ChildType = Common.Shared.ClassificationTypeList.Codes.Export;
			pivotExport.CI_FormattedTariffNum = "654321";
			Factory.Save();
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "EDIBRNWIS";
			declarant.MainAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "AUTH_NUMBER");
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			header.UpdateOutwardLinesWithInventoryDetails(new[] { invoiceLine });

			CombineAssertions(() =>
			{
				AssertCollectionNotContains("Delete any existing PreviousProcedures: InitialPreviousProcedure1", initialPreviousProcedure1, invoiceLine.PreviousProcedures);
				AssertCollectionNotContains("Delete any existing PreviousProcedures: InitialPreviousProcedure2", initialPreviousProcedure2, invoiceLine.PreviousProcedures);

				var createdPreviousProcedure = invoiceLine.PreviousProcedures.Cast<PreviousDocument>().Single();
				AssertEquals("CSI_ReferenceNumber", "ENT1234", createdPreviousProcedure.CSI_ReferenceNumber);
				AssertEquals("CSI_LineNo", 1, createdPreviousProcedure.CSI_LineNo);
				AssertEquals("CSI_Tariff", "123456", createdPreviousProcedure.CSI_Tariff);
				AssertEquals("Status", expected: false, createdPreviousProcedure.Status);
				AssertEquals("CSI_Quantity2", 50m, createdPreviousProcedure.CSI_Quantity2);
				AssertEquals("CSI_UnitOfQuantity2", "NO", createdPreviousProcedure.CSI_UnitOfQuantity2);
				AssertEquals("CSI_Procedure", "AT-ZL", createdPreviousProcedure.CSI_Procedure);
				AssertEquals("AuthorizationNumber", "AUTH_NUMBER", createdPreviousProcedure.AuthorizationNumber);
				AssertEquals("CSI_ReferenceNumber2", "JobNumber", createdPreviousProcedure.CSI_ReferenceNumber2);
			});
		}

		public void TestCreatePreviousProcedure_CSI_Status()
		{
			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Supplier.PK);
			var invoiceLine = CreateInvoiceLine(declaration, "ENT");
			var receiveLine = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, $"ENT-1", ZDateTime.Today.AddMonths(-1));
			var receiveLineCustomsData = helper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", ZString.Empty, "ENT", 1);
			receiveLineCustomsData.WB_InwardProcedure = "1234";

			var testcase = (ZString previousEntryNumber) =>
			{
				invoiceLine.JI_PreviousEntryNumber = previousEntryNumber;
				receiveLine.WE_BondedEntryKey = $"{previousEntryNumber}-1";
				receiveLineCustomsData.WB_EntryKey = previousEntryNumber;
				Factory.Save();

				header.UpdateOutwardLinesWithInventoryDetails([invoiceLine]);

				var createdPreviousProcedure = invoiceLine.PreviousProcedures.Cast<PreviousDocument>().Single();
				return (bool)createdPreviousProcedure.Status;
			};

			PreviousDocumentHelperTest.AssertIsValidAtlasReferenceForBondedWarehouse_MRN(testcase);
			PreviousDocumentHelperTest.AssertIsValidAtlasReferenceForBondedWarehouse(testcase);

			receiveLineCustomsData.WB_InwardProcedure = "5100";
			PreviousDocumentHelperTest.AssertIsValidAtlasReferenceForInwardProcessing_MRN(testcase);
			PreviousDocumentHelperTest.AssertIsValidAtlasReferenceForInwardProcessing(testcase);
		}

		public void TestCreatePreviousProcedure_CSI_Tariff_FallBackToBWHAttribute()
		{
			var invoiceLine = CreateInvoiceLine(declaration, PreviousEntryNumber);
			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Supplier.PK);
			var receiveLine = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, BondedEntryKey, ZDateTime.Today.AddMonths(-1));
			var receiveLineCustomsData = helper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", ZString.Empty, PreviousEntryNumber, 1);
			receiveLineCustomsData.WB_Tariff = "67890";
			Factory.Save();

			header.UpdateOutwardLinesWithInventoryDetails(new[] { invoiceLine });

			var createdPreviousProcedure = invoiceLine.PreviousProcedures.Cast<PreviousDocument>().Single();
			AssertEquals("67890", createdPreviousProcedure.CSI_Tariff);
		}

		public void TestCreatePreviousProcedure_AuthorizationNumber_MultipleAvailableNumbers()
		{
			var invoiceLine = CreateInvoiceLine(declaration, PreviousEntryNumber);
			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Supplier.PK);
			var receiveLine = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, BondedEntryKey, ZDateTime.Today.AddMonths(-1));
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", ZString.Empty, PreviousEntryNumber, 1);
			Factory.Save();
			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "EDIBRNWIS";
			declarant.MainAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "AUTH_NUMBER1");
			declarant.MainAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, "AUTH_NUMBER2");
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			header.UpdateOutwardLinesWithInventoryDetails(new[] { invoiceLine });
			CombineAssertions(() =>
			{
				AssertEquals("PreviousProcedure created", true, invoiceLine.PreviousProcedures.Any());
				AssertEquals("No AuthorizationNumber as there're multiple available", ZString.Empty, invoiceLine.PreviousProcedureMaster.AuthorizationNumber);
			});
		}

		public void TestInvoiceLinePreviousProcedure_WhenInventoryInwardProcedureIs51()
		{
			var invoiceLine = CreateInvoiceLine(declaration, PreviousEntryNumber);
			invoiceLine.JI_PartNo = helper.Part.OP_PartNum;
			invoiceLine.JI_BondedWhsQuantity = 50;
			invoiceLine.JI_BondedWhsUnitQty = "NO";

			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Supplier.PK);
			var receiveLine = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, BondedEntryKey, ZDateTime.Today.AddMonths(-1));
			var receiveLineCustomsData = helper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", ZString.Empty, PreviousEntryNumber, 1);
			receiveLineCustomsData.WB_InwardProcedure = "5100";
			receiveLineCustomsData.WB_Tariff = "1111223333";

			var pivotImport = helper.Pivot;
			pivotImport.CI_ChildType = Common.Shared.ClassificationTypeList.Codes.Import;
			pivotImport.CI_FormattedTariffNum = "2222334444";
			var pivotExport = helper.Pivot2;
			pivotExport.CI_ChildType = Common.Shared.ClassificationTypeList.Codes.Export;
			pivotExport.CI_FormattedTariffNum = "4444556666";

			var authorization = helper.Supplier.MainAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "AUTH_NUMBER");
			authorization.CPH_OH_PermitHolder = helper.Supplier.PK;

			Factory.Save();

			header.UpdateOutwardLinesWithInventoryDetails(new[] { invoiceLine });

			var createdPreviousProcedure = invoiceLine.PreviousProcedures.Cast<PreviousDocument>().Single();

			CombineAssertions(() =>
			{
				AssertEquals("CPC 3151", "3151", invoiceLine.JI_Procedure);

				AssertEquals("CSI_Procedure", "AT-AV", createdPreviousProcedure.CSI_Procedure);
				AssertEquals("AuthorizationNumber", "AUTH_NUMBER", createdPreviousProcedure.AuthorizationNumber);
				AssertEquals("CSI_ReferenceNumber", "ENT1234", createdPreviousProcedure.CSI_ReferenceNumber);
				AssertEquals("CSI_ReferenceNumber2", declaration.JE_DeclarationReference, createdPreviousProcedure.CSI_ReferenceNumber2);
				AssertEquals("CSI_LineNo", 1, createdPreviousProcedure.CSI_LineNo);
				AssertEquals("Status", expected: false, createdPreviousProcedure.Status);
				AssertEquals("CSI_Description", expected: "2222.33.44 44", createdPreviousProcedure.CSI_Description);
				AssertEquals("CSI_Tariff", "2222334444", createdPreviousProcedure.CSI_Tariff);
				AssertEquals("CSI_Quantity2", 50m, createdPreviousProcedure.CSI_Quantity2);
				AssertEquals("CSI_UnitOfQuantity2", "NO", createdPreviousProcedure.CSI_UnitOfQuantity2);
			});
		}

		public void TestInvoiceLinePreviousProcedureWithSerialNumber_WhenInventoryInwardProcedureIs51()
		{
			var invoiceLine = CreateInvoiceLine(declaration, PreviousEntryNumber);
			invoiceLine.JI_PartNo = helper.Part.OP_PartNum;
			invoiceLine.JI_BondedWhsQuantity = 50;
			invoiceLine.JI_BondedWhsUnitQty = "NO";

			const string serialNumber = "SN01";

			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Supplier.PK);
			var receiveLine = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, serialNumber, BondedEntryKey, ZDateTime.Today.AddMonths(-1));
			var receiveLineCustomsData = helper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", ZString.Empty, PreviousEntryNumber, 1);
			receiveLineCustomsData.WB_InwardProcedure = "5100";

			var authorization = helper.Supplier.MainAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "AUTH_NUMBER");
			authorization.CPH_OH_PermitHolder = helper.Supplier.PK;

			Factory.Save();

			header.UpdateOutwardLinesWithInventoryDetails(new[] { invoiceLine });

			var createdPreviousProcedure = invoiceLine.PreviousProcedures.Cast<PreviousDocument>().Single();

			CombineAssertions(() =>
			{
				AssertEquals("CPC 3151", "3151", invoiceLine.JI_Procedure);
				AssertEquals("CSI_Procedure", "AT-AV", createdPreviousProcedure.CSI_Procedure);
				AssertEquals("CSI_Description", expected: serialNumber, createdPreviousProcedure.CSI_Description);
			});
		}

		public void TestInvoiceLinePreviousProcedures_WhenAssembledInventoryInwardProcedureIs51()
		{
			const string inwardProcedure51 = "5100";

			const string frameEntryKey = "ATC510123456789012345";
			const string frameWithSerialEntryKey = "ATS510123456789012345";
			const string wheelEntryKey = "ATD510123456789012345";
			const string cranksetEntryKey = "ATE510123456789012345";
			const string handlebarEntryKey = "ATP510123456789012345";
			const string saddleEntryKey = "ATZ000123456789012345";

			const int frameEntryLineNo = 5;
			const string serialNumber = "SN01";

			using (CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				helper.WhsHelper.EnableWarehouseForBond(helper.WhsWarehouse, true);
				helper.WhsWarehouse.WW_IsVirtualWarehouse = true;

				var area = helper.WhsHelper.CreateWhsArea(helper.WhsWarehouse.PK, "IPR", "IPR");
				var row = helper.WhsHelper.CreateRowAndGenerateLocations(helper.WhsWarehouse, "I");
				var location = row.Locations[0] as IWhsLocation;
				location.WLV_WA_PutawayArea = area.PK;
				location.WLV_WA_PickingArea = area.PK;
				Factory.Save();

				var frame = helper.CreateProduct(helper.Importer.PK, "FRAME");
				var frameWithSerial = helper.CreateProduct(helper.Importer.PK, "FRAMEWITHSERIAL");
				var wheel = helper.CreateProduct(helper.Importer.PK, "WHEEL");
				var crankset = helper.CreateProduct(helper.Importer.PK, "CRANKSET");
				var handlebar = helper.CreateProduct(helper.Importer.PK, "HANDLEBAR");
				var saddle = helper.CreateProduct(helper.Importer.PK, "SADDLE");
				var bike = helper.CreateProduct(helper.Importer.PK, "BIKE");

				helper.WhsHelper.CreateProductBOM(bike.PK, frame.PK, 1m);
				helper.WhsHelper.CreateProductBOM(bike.PK, frameWithSerial.PK, 1m);
				helper.WhsHelper.CreateProductBOM(bike.PK, wheel.PK, 2m);
				helper.WhsHelper.CreateProductBOM(bike.PK, crankset.PK, 2m);
				helper.WhsHelper.CreateProductBOM(bike.PK, handlebar.PK, 1m);
				helper.WhsHelper.CreateProductBOM(bike.PK, saddle.PK, 1m);

				var frameReceive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK, "RCV001-FRAME", ZDateTimeOffset.Today.AddMonths(-1));
				frameReceive.WD_IsInwardsProcessingJob = true;
				var framesReceiveLine = helper.GetNewWhsReceiveLineWithBondedAttribute(frameReceive.PK, frame.PK, vfd: 50000m, qty: 500m, location.PK, frameEntryLineNo, frameEntryKey);
				framesReceiveLine.CustomsData.WB_EntryDate = ZDateTime.Today.AddMonths(-2);
				framesReceiveLine.CustomsData.WB_InwardProcedure = inwardProcedure51;
				framesReceiveLine.CustomsData.WB_Tariff = "1111223333";
				frameReceive.FinaliseDocketWithoutUserConfirmation();

				var frameReceiveWithSerial = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK, "RCV001-FRAMEWITHSERIAL", ZDateTimeOffset.Today.AddMonths(-1));
				frameReceiveWithSerial.WD_IsInwardsProcessingJob = true;
				var framesReceiveLineWithSerial = helper.GetNewWhsReceiveLineWithBondedAttribute(frameReceiveWithSerial.PK, frameWithSerial.PK, vfd: 1m, qty: 1m, location.PK, frameEntryLineNo, frameWithSerialEntryKey);
				framesReceiveLineWithSerial.CustomsData.WB_EntryDate = ZDateTime.Today.AddMonths(-2);
				framesReceiveLineWithSerial.CustomsData.WB_InwardProcedure = inwardProcedure51;
				framesReceiveLineWithSerial.CustomsData.WB_Tariff = "1111223333";
				var relation = frameWithSerial.RelatedOrganisations[0];
				relation.OU_UseSerialNumber = true;
				relation.Organisation.MiscServ.OM_IMUseSerialNumber = true;
				framesReceiveLineWithSerial.WE_SerialNumber = serialNumber;
				frameReceiveWithSerial.FinaliseDocketWithoutUserConfirmation();

				var wheelReceive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK, "RCV001-WHEEL", ZDateTimeOffset.Today.AddMonths(-1));
				wheelReceive.WD_IsInwardsProcessingJob = true;
				var wheelReceiveLine = helper.GetNewWhsReceiveLineWithBondedAttribute(wheelReceive.PK, wheel.PK, vfd: 10000m, qty: 10000m, location.PK, 1, wheelEntryKey);
				wheelReceiveLine.CustomsData.WB_EntryDate = ZDateTime.Today.AddMonths(-2);
				wheelReceiveLine.CustomsData.WB_InwardProcedure = inwardProcedure51;
				wheelReceiveLine.CustomsData.WB_Tariff = "2222334444";
				wheelReceive.FinaliseDocketWithoutUserConfirmation();

				var cranksetReceive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK, "RCV001-CRANKSET", ZDateTimeOffset.Today.AddMonths(-1));
				cranksetReceive.WD_IsInwardsProcessingJob = true;
				var cranksetReceiveLine = helper.GetNewWhsReceiveLineWithBondedAttribute(cranksetReceive.PK, crankset.PK, vfd: 10000m, qty: 10000m, location.PK, 1, cranksetEntryKey);
				cranksetReceiveLine.CustomsData.WB_EntryDate = ZDateTime.Today.AddMonths(-2);
				cranksetReceiveLine.CustomsData.WB_InwardProcedure = inwardProcedure51;
				cranksetReceiveLine.CustomsData.WB_Tariff = "3333445555";
				cranksetReceive.FinaliseDocketWithoutUserConfirmation();

				var handlebarReceive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK, "RCV001-HANDLEBAR", ZDateTimeOffset.Today.AddMonths(-1));
				handlebarReceive.WD_IsInwardsProcessingJob = true;
				var handlebarReceiveLine = helper.GetNewWhsReceiveLineWithBondedAttribute(handlebarReceive.PK, handlebar.PK, vfd: 50000m, qty: 500m, location.PK, 3, handlebarEntryKey);
				handlebarReceiveLine.CustomsData.WB_EntryDate = ZDateTime.Today.AddMonths(-2);
				handlebarReceiveLine.CustomsData.WB_InwardProcedure = inwardProcedure51;
				handlebarReceiveLine.CustomsData.WB_Tariff = "4444556666";
				handlebarReceive.FinaliseDocketWithoutUserConfirmation();

				var saddleReceive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK, "RCV001-SADDLE", ZDateTimeOffset.Today.AddMonths(-1));
				saddleReceive.WD_IsInwardsProcessingJob = true;
				var saddleReceiveLine = helper.GetNewWhsReceiveLineWithBondedAttribute(saddleReceive.PK, saddle.PK, vfd: 50000m, qty: 500m, location.PK, 4, saddleEntryKey);
				saddleReceiveLine.CustomsData.WB_EntryDate = ZDateTime.Today.AddMonths(-2);
				saddleReceiveLine.CustomsData.WB_InwardProcedure = inwardProcedure51;
				saddleReceiveLine.CustomsData.WB_Tariff = "555666777";
				saddleReceive.FinaliseDocketWithoutUserConfirmation();

				Factory.Save();
				CombineAssertions("Precondition: Receives are finalised", () =>
				{
					Assert("Precondition: Frame Receive is finalised", !frameReceive.WD_FinalisedDate.IsEmpty);
					Assert("Precondition: Frame With Serial Receive is finalised", !frameReceiveWithSerial.WD_FinalisedDate.IsEmpty);
					Assert("Precondition: Wheel Receive is finalised", !wheelReceive.WD_FinalisedDate.IsEmpty);
					Assert("Precondition: Crankset Receive is finalised", !cranksetReceive.WD_FinalisedDate.IsEmpty);
					Assert("Precondition: Handlebar Receive is finalised", !handlebarReceive.WD_FinalisedDate.IsEmpty);
					Assert("Precondition: Saddle Receive is finalised", !saddleReceive.WD_FinalisedDate.IsEmpty);
				});

				helper.CreateAndFinaliseWorkOrderWithLine(helper.Importer.PK, helper.WhsWarehouse.PK, bike.PK, 2m);

				var bikeInventory = Factory.LoadTop1<IWhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_OP, bike.PK));

				var declaration = CreateExportJobDeclaration();
				var inventorySelectionHeader = new ExportInventorySelectionHeader(declaration);

				var authorization = helper.Supplier.MainAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "AUTH_NUMBER");
				authorization.CPH_OH_PermitHolder = helper.Supplier.PK;

				Factory.Save();

				var wrapper = new WhsInventoryWrapper(bikeInventory, inventorySelectionHeader);
				wrapper.QuantityToDraw = 1;
				inventorySelectionHeader.SelectedLines.Add(wrapper);

				inventorySelectionHeader.ImportInventories();
				var invoiceLine = declaration.InvoiceLines.Cast<JobComInvoiceLine>().Single();

				var framePreviousProcedure = GetPreviousProcedureByReference(invoiceLine, frameEntryKey);
				var frameWithSerialPreviousProcedure = GetPreviousProcedureByReference(invoiceLine, frameWithSerialEntryKey);
				var wheelPreviousProcedure = GetPreviousProcedureByReference(invoiceLine, wheelEntryKey);
				var cranksetPreviousProcedure = GetPreviousProcedureByReference(invoiceLine, cranksetEntryKey);
				var handlebarPreviousProcedure = GetPreviousProcedureByReference(invoiceLine, handlebarEntryKey);
				var saddlePreviousProcedure = GetPreviousProcedureByReference(invoiceLine, saddleEntryKey);

				CombineAssertions("Previous procedures per assembled inventory component", () =>
				{
					AssertNotNull(framePreviousProcedure);
					AssertNotNull(frameWithSerialPreviousProcedure);
					AssertNotNull(wheelPreviousProcedure);
					AssertNotNull(cranksetPreviousProcedure);
					AssertNotNull(handlebarPreviousProcedure);
					AssertNotNull(saddlePreviousProcedure);
				});

				CombineAssertions("Previous procedures properties", () =>
				{
					AssertEquals("CPC 3151", "3151", invoiceLine.JI_Procedure);

					AssertEquals("CSI_Procedure", "AT-AV", framePreviousProcedure.CSI_Procedure);
					AssertEquals("AuthorizationNumber", "AUTH_NUMBER", framePreviousProcedure.AuthorizationNumber);
					AssertEquals("CSI_ReferenceNumber", frameEntryKey, framePreviousProcedure.CSI_ReferenceNumber);
					AssertEquals("CSI_ReferenceNumber2", declaration.JE_DeclarationReference, framePreviousProcedure.CSI_ReferenceNumber2);
					AssertEquals("CSI_LineNo", frameEntryLineNo, framePreviousProcedure.CSI_LineNo);
					AssertEquals("CSI_Description", "1111223333", framePreviousProcedure.CSI_Description);
					AssertEquals("CSI_Description", serialNumber, frameWithSerialPreviousProcedure.CSI_Description);
					AssertEquals("CSI_Tariff", "1111223333", framePreviousProcedure.CSI_Tariff);
					AssertEquals("CSI_Quantity2", 1m, framePreviousProcedure.CSI_Quantity2);
					AssertEquals("CSI_UnitOfQuantity2", "NAR", framePreviousProcedure.CSI_UnitOfQuantity2);

					AssertEquals("Entry via ATLAS (ATC51)", expected: true, framePreviousProcedure.Status);
					AssertEquals("Entry via ATLAS (ATD51)", expected: true, wheelPreviousProcedure.Status);
					AssertEquals("Entry via ATLAS (ATE51)", expected: true, cranksetPreviousProcedure.Status);
					AssertEquals("Entry via ATLAS (ATP51)", expected: true, handlebarPreviousProcedure.Status);
					AssertEquals("No entry via ATLAS (non-ATx51)", expected: false, saddlePreviousProcedure.Status);
				});
			}

			PreviousDocument GetPreviousProcedureByReference(JobComInvoiceLine invoiceLine, string reference) => invoiceLine.PreviousProcedures.Cast<PreviousDocument>().SingleOrDefault(x => x.CSI_ReferenceNumber == reference);
		}

		public void TestFillInvoiceLineWithInventoryDetails_WhenNonAssembledInventory()
		{
			const string serialNumber = "SN01";
			const string previousDescription = "PN";

			var invoiceLine = CreateInvoiceLine(declaration, PreviousEntryNumber);
			invoiceLine.JI_Description = previousDescription;

			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Supplier.PK);
			helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "UNT", ZString.Empty, ZString.Empty, ZString.Empty, serialNumber, BondedEntryKey, ZDateTime.Today.AddMonths(-1));

			Factory.Save();

			header.UpdateOutwardLinesWithInventoryDetails(new[] { invoiceLine });

			CombineAssertions(() =>
			{
				AssertEquals("Appended Serial Number", $"{previousDescription}, SN: {serialNumber}", invoiceLine.JI_Description);
				AssertEquals("Warehouse Qty.", "UNT", invoiceLine.JI_BondedWhsUnitQty);
			});
		}

		public void TestFillInvoiceLineWithInventoryDetails_WhenAssembledInventory()
		{
			const string inwardProcedure51 = "5100";
			const string frameEntryKey = "ATC51****";
			const string wheelEntryKey = "ATD51****";
			const string cranksetEntryKey = "ATE51****";
			const string frameSerialNumber = "SN01";
			const string wheelSerialNumber = "SN02";
			const string cranksetSerialNumber = "SN03";

			using (CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				helper.WhsHelper.EnableWarehouseForBond(helper.WhsWarehouse, true);
				helper.WhsWarehouse.WW_IsVirtualWarehouse = true;

				var area = helper.WhsHelper.CreateWhsArea(helper.WhsWarehouse.PK, "IPR", "IPR");
				var row = helper.WhsHelper.CreateRowAndGenerateLocations(helper.WhsWarehouse, "I");
				var location = row.Locations[0] as IWhsLocation;
				location.WLV_WA_PutawayArea = area.PK;
				location.WLV_WA_PickingArea = area.PK;
				Factory.Save();

				var frame = helper.CreateProduct(helper.Importer.PK, "FRAME");
				var wheel = helper.CreateProduct(helper.Importer.PK, "WHEEL");
				var crankset = helper.CreateProduct(helper.Importer.PK, "CRANKSET");
				var bike = helper.CreateProduct(helper.Importer.PK, "BIKE");

				helper.WhsHelper.CreateProductBOM(bike.PK, frame.PK, 1m);
				helper.WhsHelper.CreateProductBOM(bike.PK, wheel.PK, 1m);
				helper.WhsHelper.CreateProductBOM(bike.PK, crankset.PK, 1m);

				ProcessProductReceive(frame, frameSerialNumber, frameEntryKey, "1111223333", inwardProcedure51, location.PK, 1);
				ProcessProductReceive(wheel, wheelSerialNumber, wheelEntryKey, "2222334444", inwardProcedure51, location.PK, 2);
				ProcessProductReceive(crankset, cranksetSerialNumber, cranksetEntryKey, "3333445555", inwardProcedure51, location.PK, 3);

				Factory.Save();

				helper.CreateAndFinaliseWorkOrderWithLine(helper.Importer.PK, helper.WhsWarehouse.PK, bike.PK, 1m);

				var bikeInventory = Factory.LoadTop1<IWhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_OP, bike.PK));

				var declaration = CreateExportJobDeclaration();
				var inventorySelectionHeader = new ExportInventorySelectionHeader(declaration);

				var authorization = helper.Supplier.MainAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "AUTH_NUMBER");
				authorization.CPH_OH_PermitHolder = helper.Supplier.PK;

				Factory.Save();

				var wrapper = new WhsInventoryWrapper(bikeInventory, inventorySelectionHeader);
				wrapper.QuantityToDraw = 1;
				inventorySelectionHeader.SelectedLines.Add(wrapper);

				inventorySelectionHeader.ImportInventories();
				var invoiceLine = declaration.InvoiceLines.Cast<JobComInvoiceLine>().Single();

				CombineAssertions(() =>
				{
					AssertEquals("Warehouse Qty.", "NAR", invoiceLine.JI_BondedWhsUnitQty);
					AssertEquals("Appended Serial Number", ", SN: SN01, SN02, SN03", invoiceLine.JI_Description);
				});
			}
		}

		protected override BusinessObject GetNewBusinessObject() => new ExportInventorySelectionHeader(Factory.New<JobDeclaration>());

		protected override void SetUp()
		{
			base.SetUp();
			helper = new WhsDataTestHelper(Factory);
			declaration = CreateExportJobDeclaration();
			header = new ExportInventorySelectionHeader(declaration);
		}
		JobDeclaration declaration;
		ExportInventorySelectionHeader header;
		WhsDataTestHelper helper;
		const string PreviousEntryNumber = "ENT1234";
		const string BondedEntryKey = "ENT1234-1";

		void AssertUpdateOutwardLinesWithInventoryDetails(Action<IWhsReceiveLine, IWhsBondedWarehouseAttribute, JobComInvoiceLine> mocker, Action<JobComInvoiceLine> assertion)
		{
			var invoiceLine = CreateInvoiceLine(declaration, PreviousEntryNumber);
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Procedure = "56";
			invoiceLine.JI_CEI = entryInstruction.PK;
			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Supplier.PK);
			var receiveLine = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, BondedEntryKey, ZDateTime.Today.AddMonths(-1));
			var receiveLineCustomsData = helper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", ZString.Empty, PreviousEntryNumber, 1);
			helper.WhsHelper.WhsReceiveAllocateLocationsMock(receive.PK);

			var inventory = receiveLine.Inventory;
			mocker?.Invoke(receiveLine, receiveLineCustomsData, invoiceLine);
			Factory.Save();

			var invoiceLines = new List<JobComInvoiceLine> { invoiceLine };
			header.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			CombineAssertions(() => assertion?.Invoke(invoiceLine));
		}

		void AddWarehouseCustomsAttributeAddInfo(IWhsBondedWarehouseAttribute customsData, string type, string addInfoData)
		{
			var addInfo = Factory.New<WarehouseCustomsAttributeAddInfo>();
			addInfo.B7_ParentTableCode = "WB";
			addInfo.B7_ParentID = customsData.PK;
			addInfo.B7_Type = type;
			addInfo.B7_AddInfoData = addInfoData;
		}

		BaseJobComInvoiceHeader GetInvoice(JobDeclaration declaration, ZString currencyCode) => declaration.Invoices.Cast<JobComInvoiceHeader>().SingleOrDefault(x => x.JZ_RX_NKInvoice_Currency == currencyCode);

		JobDeclaration CreateExportJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_OH_Supplier = helper.Supplier.PK;
			declaration.WarehouseDocAddress.E2_OA_Address = helper.Warehouse.MainAddress.PK;
			return declaration;
		}

		JobComInvoiceLine CreateInvoiceLine(JobDeclaration declaration, ZString previousEntryNumber)
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PreviousEntryNumber = previousEntryNumber;
			invoiceLine.JI_PreviousEntryLineNumber = 1;
			return invoiceLine;
		}

		void ProcessProductReceive(OrgSupplierPart product, string serialNumber, string entryKey, string tariff, string inwardProcedure, ZGuid locationPK, ZShort lineNo)
		{
			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK, $"RCV001-{product.OP_Desc}", ZDateTimeOffset.Today.AddMonths(-1));
			receive.WD_IsInwardsProcessingJob = true;
			var receiveLine = helper.GetNewWhsReceiveLineWithBondedAttribute(receive.PK, product.PK, vfd: 1m, qty: 1m, locationPK, lineNo, entryKey);
			receiveLine.CustomsData.WB_EntryDate = ZDateTime.Today.AddMonths(-2);
			receiveLine.CustomsData.WB_InwardProcedure = inwardProcedure;
			receiveLine.CustomsData.WB_Tariff = tariff;
			var relation = product.RelatedOrganisations[0];
			relation.OU_UseSerialNumber = true;
			relation.Organisation.MiscServ.OM_IMUseSerialNumber = true;
			receiveLine.WE_SerialNumber = serialNumber;
			receive.FinaliseDocketWithoutUserConfirmation();
		}
	}
}
