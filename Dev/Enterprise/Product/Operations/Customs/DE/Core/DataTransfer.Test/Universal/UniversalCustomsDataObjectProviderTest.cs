using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.DataTransfer.Universal.Testing
{
	sealed class UniversalCustomsDataObjectProviderTest : TestCaseWithFactory
	{
		public void TestTableSpecificCusReferenceTypeList()
		{
			var result = (CodeDescriptionPairList)new UniversalCustomsDataObjectProvider().TableSpecificCusReferenceTypeList(CusEntryInstructionSchema.Constants.Prefix, string.Empty);
			AssertEquals(CusReferenceTypeList.Codes.FiscalReference, result.CodesAsString);
			result = (CodeDescriptionPairList)new UniversalCustomsDataObjectProvider().TableSpecificCusReferenceTypeList(CusInBondCargoDescSchema.Constants.Prefix, string.Empty);
			AssertEquals(Customs.Business.CusReferenceTypeList.Codes.SupplyChainActor, result.CodesAsString);
		}

		public void TestBondedWarehouseDetailsIncluded_InwardProcessing()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("IWDPROC", Core.Constants.CountryCodes.Germany, ZDateTime.Today, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				var procedure = CreateCusProcedureForTest(Core.Constants.CountryCodes.Germany);

				var helper = new WhsDataTestHelper(Factory);

				helper.Importer.CompanyData.OB_CusInventoryForInwardProcessing = true;
				Factory.Save();

				((IExternalFetchHintSupporter)Factory).SetupCreator();

				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = "IMP";
				declaration.JE_OH_Importer = helper.Importer.PK;
				var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = procedure.ZZ6_ProcedureCode;
				entryInstruction.CEI_OA_Warehouse = helper.Warehouse.MainAddress.PK;
				entryInstruction.CEI_OA_Warehouse2 = helper.Warehouse.MainAddress.PK;
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.CH_MessageType = "IMP";
				entry.CH_CEI_Instruction = entryInstruction.PK;
				var entryLine = entry.MergedLines.AddNew();
				entryLine.CL_LineNumber = 1;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();

				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_CEI = entryInstruction.PK;
				invoiceLine.JI_PartNo = helper.Part.OP_PartNum;

				invoiceLine.JI_Procedure = procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode;
				invoiceLine.JI_BondedWhsQuantity = 10m;
				invoiceLine.JI_BondedWhsUnitQty = "NO";
				invoiceLine.JI_BondedWarehouseRemarks = "Test Remarks";
				invoiceLine.JI_BondedWHSOrderNumber = "Order Number";
				invoiceLine.JI_BondedWHSOrderLineNumber = 4;

				var recipientRoleType = RecipientRoleType.BWI;
				var writer = new WarehouseEntryDataObjectWriter(new DataWritingManager(new ActionInfo(recipientRoleType, entry)));
				var shipmentData = writer.GetDataObject(entry);

				var invoiceLineData = shipmentData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
				CombineAssertions(() =>
				{
					AssertEquals("invoiceLineData.BondedWarehouseQuantity", 10m, invoiceLineData.BondedWarehouseQuantity);
					AssertEquals("invoiceLineData.BondedWarehouseQuantityUnit.Code", "NO", invoiceLineData.BondedWarehouseQuantityUnit.Code);
					AssertEquals("invoiceLineData.BondedWarehouseRemarks", "Test Remarks", invoiceLineData.BondedWarehouseRemarks);
					AssertEquals("invoiceLineData.BondedWHSOrderNumber", "Order Number", invoiceLineData.BondedWHSOrderNumber);
					AssertEquals("invoiceLineData.BondedWHSOrderLineNumber", (ZShort)4, invoiceLineData.BondedWHSOrderLineNumber);
				});
			}
		}

		public void TestBondedWarehouseDetailsIncluded_OutOfInwardProcessing()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("IWDPROC", Core.Constants.CountryCodes.Germany, ZDateTime.Today, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				var procedure = CreateoOutOfInwardProcessingCusProcedureForTest(Core.Constants.CountryCodes.Germany);

				var helper = new WhsDataTestHelper(Factory);

				helper.Importer.CompanyData.OB_CusInventoryForInwardProcessing = true;
				Factory.Save();

				((IExternalFetchHintSupporter)Factory).SetupCreator();

				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = "IMP";
				declaration.JE_OH_Importer = helper.Importer.PK;
				var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = procedure.ZZ6_ProcedureCode;
				entryInstruction.CEI_OA_Warehouse = helper.Warehouse.MainAddress.PK;
				entryInstruction.CEI_OA_Warehouse2 = helper.Warehouse.MainAddress.PK;
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.CH_MessageType = "IMP";
				entry.CH_CEI_Instruction = entryInstruction.PK;
				var entryLine = entry.MergedLines.AddNew();
				entryLine.CL_LineNumber = 1;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();

				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_CEI = entryInstruction.PK;
				invoiceLine.JI_PartNo = helper.Part.OP_PartNum;

				invoiceLine.JI_Procedure = procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode;
				invoiceLine.JI_BondedWhsQuantity = 10m;
				invoiceLine.JI_BondedWhsUnitQty = "NO";
				invoiceLine.JI_BondedWarehouseRemarks = "Test Remarks";
				invoiceLine.JI_BondedWHSOrderNumber = "Order Number";
				invoiceLine.JI_BondedWHSOrderLineNumber = 4;

				var recipientRoleType = RecipientRoleType.BWI;
				var writer = new WarehouseEntryDataObjectWriter(new DataWritingManager(new ActionInfo(recipientRoleType, entry)));
				var shipmentData = writer.GetDataObject(entry);

				var invoiceLineData = shipmentData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
				CombineAssertions(() =>
				{
					AssertEquals("invoiceLineData.BondedWarehouseQuantity", 10m, invoiceLineData.BondedWarehouseQuantity);
					AssertEquals("invoiceLineData.BondedWarehouseQuantityUnit.Code", "NO", invoiceLineData.BondedWarehouseQuantityUnit.Code);
					AssertEquals("invoiceLineData.BondedWarehouseRemarks", "Test Remarks", invoiceLineData.BondedWarehouseRemarks);
					AssertEquals("invoiceLineData.BondedWHSOrderNumber", "Order Number", invoiceLineData.BondedWHSOrderNumber);
					AssertEquals("invoiceLineData.BondedWHSOrderLineNumber", (ZShort)4, invoiceLineData.BondedWHSOrderLineNumber);
				});
			}
		}

		public void TestBondedWarehouseDetailsIncluded_WarehouseAdjustment()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				var procedure = CreateWarehouseAdjustmentCusProcedureForTest(Core.Constants.CountryCodes.Germany);

				var helper = new WhsDataTestHelper(Factory);

				helper.Importer.CompanyData.OB_CusInventoryForInwardProcessing = true;
				Factory.Save();

				((IExternalFetchHintSupporter)Factory).SetupCreator();

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = "WAD";
				declaration.JE_OH_Importer = helper.Importer.PK;
				var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = procedure.ZZ6_ProcedureCode;
				entryInstruction.CEI_OA_Warehouse = helper.Warehouse.MainAddress.PK;
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.CH_MessageType = "WAD";
				entry.CH_CEI_Instruction = entryInstruction.PK;
				var entryLine = entry.MergedLines.AddNew();
				entryLine.CL_LineNumber = 1;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();

				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_CEI = entryInstruction.PK;
				invoiceLine.JI_PartNo = helper.Part.OP_PartNum;

				invoiceLine.JI_Procedure = procedure.ZZ6_ProcedureCode;
				invoiceLine.JI_BondedWhsQuantity = 10m;
				invoiceLine.JI_BondedWhsUnitQty = "NO";
				invoiceLine.JI_PreviousEntryNumber = "PREVOIUSMRN";
				invoiceLine.JI_PreviousEntryLineNumber = 4;

				var recipientRoleType = RecipientRoleType.BWI;
				var writer = new WarehouseEntryDataObjectWriter(new DataWritingManager(new ActionInfo(recipientRoleType, entry)));
				var shipmentData = writer.GetDataObject(entry);

				var invoiceLineData = shipmentData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
				CombineAssertions(() =>
				{
					AssertEquals("invoiceLineData.PreviousEntryNumber", "PREVOIUSMRN", invoiceLineData.PreviousEntryNumber);
					AssertEquals("invoiceLineData.PreviousEntryLineNumber", (ZShort)4, invoiceLineData.PreviousEntryLineNumber);
				});
			}
		}

		RefCusProcedure CreateCusProcedureForTest(ZString dataGrouping)
		{
			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);
			refDataHelper.CreateNewOrGetExistingDataGrouping(dataGrouping);
			var procedure = refDataHelper.CreateRefCusProcedure(dataGrouping, ZString.Empty, "IP", "01", ZString.Empty, "IP DESC", "IMP", intoWarehouse: false, group: "IP");
			procedure.ZZ6_IntoInwardProcessing = WarehouseMoveStatus.Codes.Yes;
			Factory.Save();
			return procedure;
		}

		RefCusProcedure CreateoOutOfInwardProcessingCusProcedureForTest(ZString dataGrouping)
		{
			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);
			refDataHelper.CreateNewOrGetExistingDataGrouping(dataGrouping);
			var procedure = refDataHelper.CreateRefCusProcedure(dataGrouping, ZString.Empty, "IP", "01", ZString.Empty, "IP DESC", "IMP", intoWarehouse: false, group: "IP");
			procedure.ZZ6_OutOfInwardProcessing = WarehouseMoveStatus.Codes.Yes;
			Factory.Save();
			return procedure;
		}

		RefCusProcedure CreateWarehouseAdjustmentCusProcedureForTest(ZString dataGrouping)
		{
			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);
			refDataHelper.CreateNewOrGetExistingDataGrouping(dataGrouping);
			var procedure = refDataHelper.CreateRefCusProcedure(dataGrouping, ZString.Empty, "01", ZString.Empty, ZString.Empty, "WAD DESC", "WAD", intoWarehouse: false, group: "SER");
			procedure.ZZ6_OutofOutwardProcessing = WarehouseMoveStatus.Codes.Yes;
			Factory.Save();
			return procedure;
		}
	}
}
