using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common.DE;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Registry;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using CusEntryLine = Enterprise.Customs.DE.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.DE.Business.Testing.WarehouseIntegration
{
	public class BondedWarehousingHelperTest : TestCaseWithFactory
	{
		public void TestSetWarehouseTransactionStatusForImportMessageProcessing()
		{
			var entry = jobDeclaration.CustomsEntryHeaders.AddNew();
			BondedWarehousingHelper.SetWarehouseTransactionStatusForImportMessageProcessing(entry);

			AssertEquals("NotIntoWarehouseWarehousing", string.Empty, entry.CH_WarehouseTransactionStatus);

			var helper = WhsDataTestHelper.New(Factory);

			entry = helper.GetNewDeclarationWithInstruction(Factory, "IMP", "B0000", "ENT0001", 10000, true)
				.CustomsEntryHeaders[0];

			BondedWarehousingHelper.SetWarehouseTransactionStatusForImportMessageProcessing(entry);

			AssertEquals("IntoWarehouseWarehousing empty", WarehouseTransactionStatusList.Codes.InwardCreatedPending, entry.CH_WarehouseTransactionStatus);

			entry.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardUpdated;
			BondedWarehousingHelper.SetWarehouseTransactionStatusForImportMessageProcessing(entry);

			AssertEquals("IntoWarehouseWarehousing updated", WarehouseTransactionStatusList.Codes.InwardUpdatedPending, entry.CH_WarehouseTransactionStatus);
		}

		public void TestIsMarkedForBondedWarehousing()
		{
			CreateCustomsStatus(Factory, "RL4", true, true);
			CreateCustomsStatus(Factory, "RL5", true, false);

			var entry = jobDeclaration.ActiveEntryHeaders.AddNew();
			var entryLine = (CusEntryLine)entry.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.ZG_CustomsStatus = UniversalReferenceConstants.EntryStatus.RL5;

			const string isOutOfWarehouseWarehousingProcedureCode = "4071";

			var instruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Procedure = isOutOfWarehouseWarehousingProcedureCode;

			Factory.NewWithValidTestData<RefDataGrouping>().ZZZ_DataGrouping = Core.Constants.CountryCodes.Germany;
			WhsDataTestHelper.CreateOutwardCusProcedure(Factory);
			WhsDataTestHelper.CreateOutwardCusProcedureExport(Factory);
			WhsDataTestHelper.CreateWarehouseAdjustmentProcedure(Factory);

			var invoiceLine = jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Procedure = isOutOfWarehouseWarehousingProcedureCode;

			CombineAssertions(() =>
			{
				AssertEquals("ZG_CustomsStaus not 'RL4'", expected: true, helper.IsMarkedForBondedWarehousing(invoiceLine));

				entryLine.ZG_CustomsStatus = UniversalReferenceConstants.EntryStatus.RL4;
				AssertEquals("ZG_CustomsStaus is 'RL4'", expected: false, helper.IsMarkedForBondedWarehousing(invoiceLine));

				entryLine.ZG_CustomsStatus = ZString.Empty;
				jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				invoiceLine.JI_Procedure = isOutOfWarehouseWarehousingProcedureCode;

				AssertEquals("declaration is export", expected: true, helper.IsMarkedForBondedWarehousing(invoiceLine));

				entryLine.ZG_CustomsStatus = UniversalReferenceConstants.EntryStatus.RL4;
				jobDeclaration.JE_MessageType = DEJobMessageTypeList.Codes.WarehouseAdjustment;
				invoiceLine.JI_Procedure = "01";

				var actual = helper.IsMarkedForBondedWarehousing(invoiceLine);
				AssertEquals("declaration is warehouse adjustment", expected: false, actual);
			});
		}

		public void TestGetBondedWarehouseAttributeFromWhsInventoryWrapper()
		{
			var testHelper = new WhsDataTestHelper(Factory);
			var receive = testHelper.GetNewWhsReceive(testHelper.WhsWarehouse.PK, testHelper.Importer.PK);
			var receiveLine = testHelper.GetNewWhsReceiveLine(receive.PK, testHelper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-1", ZDateTime.Today.AddMonths(-1));
			var attribute = testHelper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", ZString.Empty, "ENT1234", 1);
			Factory.Save();

			var whsInventory = receiveLine.Inventory;
			var wrapper = new WhsInventoryWrapper(whsInventory, new ImportInventorySelectionHeader(jobDeclaration));
			AssertEquals(attribute.PK, BondedWarehousingHelper.GetBondedWarehouseAttributeFromWhsInventoryWrapper(wrapper).PK);
		}

		public void TestGetBondedWarehouseAttributeFromWhsDocketLine()
		{
			var testHelper = new WhsDataTestHelper(Factory);
			var receive = testHelper.GetNewWhsReceive(testHelper.WhsWarehouse.PK, testHelper.Importer.PK);
			var receiveLine = testHelper.GetNewWhsReceiveLine(receive.PK, testHelper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-1", ZDateTime.Today.AddMonths(-1));
			var attribute = testHelper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", ZString.Empty, "ENT1234", 1);
			Factory.Save();

			AssertEquals(attribute.PK, BondedWarehousingHelper.GetBondedWarehouseAttributeFromWhsDocketLine(receiveLine, Factory).PK);
		}

		public void TestGetOrgAddressFromBondedWarehouseAttributeAddInfo()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ORG";
			var orgAddress = orgHeader.MainAddress;
			orgAddress.OA_Code = "ADDRESS";

			CombineAssertions(() =>
			{
				AssertEquals("Valid source string", orgAddress.PK, BondedWarehousingHelper.GetOrgAddressFromBondedWarehouseAttributeAddInfo(Factory, "ORG;ADDRESS;bla;bla;bla").PK);
				AssertNull("Invalid source string", BondedWarehousingHelper.GetOrgAddressFromBondedWarehouseAttributeAddInfo(Factory, "ORG"));
			});
		}

		public void TestPublishShipmentForWHSOutward() => CombineAssertions(() =>
		{
			var helper = WhsDataTestHelper.New(Factory);

			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = ATLASVersionNumberList.Codes._101 } };

			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var inwardDecl = helper.GetNewDeclarationWithInstruction(Factory, "IMP", "B0000", "ENT0001", 10000, true);
				inwardDecl.InvoiceLines[0].JI_BondedWhsQuantity = 100m;

				inwardDecl.CustomsEntryHeaders[0].PublishShipmentForWHSInward(false);
				inwardDecl.CustomsEntryHeaders[0].PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);

				// ^^^ this gives us 100 in quantity

				Factory.Save();

				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("<PENDINGCUSTOMSRESPONSE>", 100m);

				var declaration = helper.GetNewDeclarationWithInstruction(Factory, DEJobMessageTypeList.Codes.Import, "B0001", "ENT0002", 1000, false, previousEntryNumber: "<PENDINGCUSTOMSRESPONSE>");
				declaration.CustomsEntryHeaders[0].MergedLines.DeleteAll();
				declaration.CustomsEntryHeaders.DeleteAll();
				declaration.InvoiceLines[0].JI_CEI = declaration.CustomsEntryInstructions[0].PK;
				declaration.InvoiceLines[0].JI_BondedWhsQuantity = 10m;
				declaration.InvoiceLines[0].JI_InvoiceQuantity = 10m;
				declaration.InvoiceLines[0].JI_BondedWhsUnitQty = "KG";
				declaration.InvoiceLines[0].JI_InvoiceUQ = "KG";
				declaration.InvoiceLines[0].JI_BondedWHSOrderLineNumber = 1;
				declaration.InvoiceLines[0].JI_PreviousEntryLineNumber = 0;

				Factory.Save();

				AssertEquals("order can be fulfilled", ZString.Empty, BondedWarehousingHelper.PublishShipmentForWHSOutward(declaration));
				var links = Factory.Load<IWhsDocketJobPivot>(new ZQuery(WhsDocketJobPivotSchema.WV_ParentId, declaration.PK));
				AssertEquals("docket link for this declaration created", 1, links.Length);
				AssertEquals(WarehouseTransactionStatusList.Codes.OutwardCreatedPending, declaration.WarehouseTransactionStatus);

				declaration.InvoiceLines[0].JI_BondedWhsQuantity = 110m;
				Factory.Save();

				var expectedError = "Error - Cannot Import Order\r\nOrder could not be created for Customs Job B0001 because there are errors:\r\nYou do not have enough stock to fulfill shortfalls on this order\r\n<PENDINGCUSTOMSRESPONSE> Product ~~1/~~1 DESC can not be ordered due to lack of stock. 110 was ordered, but 100 is available";
				AssertEquals("order cannot be fulfilled", expectedError, (string)BondedWarehousingHelper.PublishShipmentForWHSOutward(declaration));
				AssertEquals(WarehouseTransactionStatusList.Codes.OutwardCreatedPending, declaration.WarehouseTransactionStatus);

				declaration.InvoiceLines[0].JI_BondedWhsQuantity = 100m;
				Factory.Save();

				AssertEquals("order can be fulfilled", ZString.Empty, BondedWarehousingHelper.PublishShipmentForWHSOutward(declaration, true));
				AssertEquals(WarehouseTransactionStatusList.Codes.OutwardCreated, declaration.WarehouseTransactionStatus);
			}
		});

		protected override void SetUp()
		{
			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			helper = new BondedWarehousingHelper(jobDeclaration);
		}
		JobDeclaration jobDeclaration;
		BondedWarehousingHelper helper;

		public static void CreateCustomsStatus(BusinessObjectFactory factory, string statusCode, bool iUpdate, bool iCancel)
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CustomsStatus");
			var cusCodeList = factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus;
			cusCodeList.ZZD_Code = statusCode;
			cusCodeList.ZZD_Description = statusCode + " Desc";
			cusCodeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Germany;
			cusCodeList.ZZD_StartDate = ZDateTime.Today.AddMonths(-1);
			cusCodeList.ZZD_EndDate = ZDateTime.Today.AddMonths(1);
			if (iUpdate)
			{
				var attribute = factory.New<ZZRefCusCodeListAttributeCombined>();
				attribute.ZZE_ZZD_CodeList = cusCodeList.PK;
				attribute.ZZE_ZXE_NKName = RefCusCodeListAttributeTypes.Codes.IUpdateBondedWhs;
			}
			if (iCancel)
			{
				var attribute = factory.New<ZZRefCusCodeListAttributeCombined>();
				attribute.ZZE_ZZD_CodeList = cusCodeList.PK;
				attribute.ZZE_ZXE_NKName = RefCusCodeListAttributeTypes.Codes.ICancelBondedWhs;
			}
			factory.Save();
			cusCodeList.Attributes.Reload(true);
		}

		public static (IWhsOrder, ZGuid) CreateOrderWithPick(BusinessObjectFactory factory, bool createMultiplePickLines)
		{
			var helper = WhsDataTestHelper.New(factory);
			helper.WhsHelper.SetUpBondedWarehouse(helper.WhsWarehouse.PK, helper.WhsWarehouse.WarehouseAddress.PK);

			var receivePk = helper.WhsHelper.CreateWhsReceive(helper.Importer.PK, helper.WhsWarehouse.PK, "REF1", null);
			var view = helper.WhsHelper.CreateWhsReceiveInventoryLine(receivePk, helper.Part.PK, 1.0m, 1.0m, 1.0m, ZString.Empty, ZString.Empty, ZDateTimeOffset.Today, "ENTRY1", ZString.Empty);
			var receive2Pk = helper.WhsHelper.CreateWhsReceive(helper.Importer.PK, helper.WhsWarehouse.PK, "REF2", null);
			var view2 = helper.WhsHelper.CreateWhsReceiveInventoryLine(receive2Pk, helper.Part.PK, 1.0m, 1.0m, 1.0m, ZString.Empty, ZString.Empty, ZDateTimeOffset.Today, "ENTRY1", ZString.Empty);

			var row = helper.WhsWarehouse.Rows.Cast<IWhsRow>().FirstOrDefault(r => r.Locations.ToArray().Cast<IWhsLocation>().Any(l => l.WLV_PickingAreaType == "BON"));
			var location = row.Locations.ToArray().Cast<IWhsLocation>().First(x => x.WLV_PickingAreaType == "BON");
			view.WI_WL = location.PK;

			var newRow = helper.WhsHelper.CreateRowAndGenerateLocations(helper.WhsWarehouse, "BOND2");
			var newLocation = (IWhsLocation)newRow.Locations[0];
			view2.WI_WL = newLocation.PK;

			factory.Save();

			helper.WhsHelper.FinaliseDocketWithoutUserConfirmation(receivePk);
			factory.Save();
			helper.WhsHelper.FinaliseDocketWithoutUserConfirmation(receive2Pk);
			factory.Save();

			var order = (IWhsOrder)helper.WhsHelper.CreateWhsOrder(helper.Importer.PK, helper.WhsWarehouse.PK, helper.Importer.PK, "REF3");
			order.WD_DocketSubType = "CUS";

			var orderQuantity = createMultiplePickLines ? 2.0m : 1.0m;
			var docketLinePk = helper.WhsHelper.CreateWhsOrderLine(order.PK, helper.Part.PK, orderQuantity);
			var orderLine  = factory.Load<WhsOrderLine>(docketLinePk);
			orderLine.CustomsData.WB_EntryKey = "ABC";
			orderLine.CustomsData.WB_EntryLineNo = 0;
			factory.Save();

			helper.WhsHelper.CreateWhsPick(new[] { order.PK });

			return (order, docketLinePk);
		}
	}
}
