using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.CA;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Customs.CA.Business.Testing
{
	public class BondedWarehousingHelperTest : TestCaseWithFactory
	{
		public void TestHasBondedWarehouseEntryDetails()
		{
			var helper = new WhsDataTestHelper(Factory);
			var declaration = CreateWHSDeclaration(Factory, "B00000001", "00000001", B3EntryTypeList.Codes.Warehouse10, helper.Importer, helper.Warehouse);
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			invoiceLine.JI_CL = entryHeader.MergedLines.AddNew().PK;

			Assert("HasBondedWarehouseEntryDetails", !declaration.BondedWarehousingHelper.HasBondedWarehouseEntryDetails(invoiceLine, true, false, false));
			AssertContains(JobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresEntryDetails("Inventory Management"), declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;
			Assert("HasBondedWarehouseEntryDetails", !declaration.BondedWarehousingHelper.HasBondedWarehouseEntryDetails(invoiceLine, false, true, false));
			AssertContains(JobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresPTNAndPTLN, declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());

			declaration.DoMerge();

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Warehouse10;
			Assert("HasBondedWarehouseEntryDetails", declaration.BondedWarehousingHelper.HasBondedWarehouseEntryDetails(invoiceLine, true, false, false));
			AssertNotContains(JobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresEntryDetails("Inventory Management"), declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;
			invoiceLine.JI_PreviousEntryNumber = "123456789";
			invoiceLine.JI_PreviousEntryLineNumber = 1;
			Assert("HasBondedWarehouseEntryDetails", declaration.BondedWarehousingHelper.HasBondedWarehouseEntryDetails(invoiceLine, false, true, false));
			AssertNotContains(JobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresPTNAndPTLN, declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());
		}

		public void TestHasBondedWarehouseEntryDetailsForCAD()
		{
			var helper = new WhsDataTestHelper(Factory);
			var declaration = CreateWHSDeclaration(Factory, "B00000001", "00000001", CADEntryTypeList.Codes.Warehouse101, helper.Importer, helper.Warehouse);
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			invoiceLine.JI_CL = entryHeader.MergedLines.AddNew().PK;

			Assert("HasBondedWarehouseEntryDetails", !declaration.BondedWarehousingHelper.HasBondedWarehouseEntryDetails(invoiceLine, true, false, false));
			AssertContains(JobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresEntryDetails("Inventory Management"), declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse102;
			Assert("HasBondedWarehouseEntryDetails", !declaration.BondedWarehousingHelper.HasBondedWarehouseEntryDetails(invoiceLine, true, false, false));
			AssertContains(JobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresEntryDetails("Inventory Management"), declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
			Assert("HasBondedWarehouseEntryDetails", !declaration.BondedWarehousingHelper.HasBondedWarehouseEntryDetails(invoiceLine, false, true, false));
			AssertContains(JobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresPTNAndPTLN, declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());

			declaration.DoMerge();

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;
			Assert("HasBondedWarehouseEntryDetails", declaration.BondedWarehousingHelper.HasBondedWarehouseEntryDetails(invoiceLine, true, false, false));
			AssertNotContains(JobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresEntryDetails("Inventory Management"), declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse102;
			Assert("HasBondedWarehouseEntryDetails", declaration.BondedWarehousingHelper.HasBondedWarehouseEntryDetails(invoiceLine, true, false, false));
			AssertNotContains(JobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresEntryDetails("Inventory Management"), declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
			invoiceLine.JI_PreviousEntryNumber = "123456789";
			invoiceLine.JI_PreviousEntryLineNumber = 1;
			Assert("HasBondedWarehouseEntryDetails", declaration.BondedWarehousingHelper.HasBondedWarehouseEntryDetails(invoiceLine, false, true, false));
			AssertNotContains(JobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresPTNAndPTLN, declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());
		}

		public void TestIsMarkedForBondedWarehousing()
		{
			var helper = new WhsDataTestHelper(Factory);
			var declaration = CreateWHSDeclaration(Factory, "B00000001", "00000001", B3EntryTypeList.Codes.Warehouse10, helper.Importer, helper.Warehouse);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine2.CA_CalculationMethod = CalculationMethods.Codes.RepairsRemission;
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			var invoiceLine3 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine3.CA_IsAutoDummyHSCodeCasualImportLine = true;
			Assert("IsMarkedForBondedWarehousing", declaration.BondedWarehousingHelper.IsMarkedForBondedWarehousing(invoiceLine1));
			Assert("IsMarkedForBondedWarehousing", !declaration.BondedWarehousingHelper.IsMarkedForBondedWarehousing(invoiceLine2));
			Assert("IsMarkedForBondedWarehousing", !declaration.BondedWarehousingHelper.IsMarkedForBondedWarehousing(invoiceLine3));

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;
			Assert("IsMarkedForBondedWarehousing", declaration.BondedWarehousingHelper.IsMarkedForBondedWarehousing(invoiceLine1));
			Assert("IsMarkedForBondedWarehousing", !declaration.BondedWarehousingHelper.IsMarkedForBondedWarehousing(invoiceLine2));
			Assert("IsMarkedForBondedWarehousing", !declaration.BondedWarehousingHelper.IsMarkedForBondedWarehousing(invoiceLine3));
		}

		public void TestIsMarkedForBondedWarehousingForCAD()
		{
			var helper = new WhsDataTestHelper(Factory);
			var declaration = CreateWHSDeclaration(Factory, "B00000001", "00000001", CADEntryTypeList.Codes.Warehouse101, helper.Importer, helper.Warehouse);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine2.CA_CalculationMethod = CalculationMethods.Codes.RepairsRemission;
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			var invoiceLine3 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine3.CA_IsAutoDummyHSCodeCasualImportLine = true;
			Assert("IsMarkedForBondedWarehousing", declaration.BondedWarehousingHelper.IsMarkedForBondedWarehousing(invoiceLine1));
			Assert("IsMarkedForBondedWarehousing", !declaration.BondedWarehousingHelper.IsMarkedForBondedWarehousing(invoiceLine2));
			Assert("IsMarkedForBondedWarehousing", !declaration.BondedWarehousingHelper.IsMarkedForBondedWarehousing(invoiceLine3));

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse102;
			Assert("IsMarkedForBondedWarehousing", declaration.BondedWarehousingHelper.IsMarkedForBondedWarehousing(invoiceLine1));
			Assert("IsMarkedForBondedWarehousing", !declaration.BondedWarehousingHelper.IsMarkedForBondedWarehousing(invoiceLine2));
			Assert("IsMarkedForBondedWarehousing", !declaration.BondedWarehousingHelper.IsMarkedForBondedWarehousing(invoiceLine3));

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
			Assert("IsMarkedForBondedWarehousing", declaration.BondedWarehousingHelper.IsMarkedForBondedWarehousing(invoiceLine1));
			Assert("IsMarkedForBondedWarehousing", !declaration.BondedWarehousingHelper.IsMarkedForBondedWarehousing(invoiceLine2));
			Assert("IsMarkedForBondedWarehousing", !declaration.BondedWarehousingHelper.IsMarkedForBondedWarehousing(invoiceLine3));
		}

		public static JobDeclaration CreateWHSDeclaration(BusinessObjectFactory factory, ZString declarationReference, ZString sequentialNumber, ZString entryType, OrgHeader importer, OrgHeader warehouseOrg)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = entryType;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_DeclarationReference = declarationReference;
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = sequentialNumber;
			declaration.WarehouseDocAddress.E2_OA_Address = warehouseOrg.MainAddress.PK;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.JE_IsCancelled = false;

			return declaration;
		}

		public static void SetupInvoiceLinesForWHS(JobDeclaration declaration, ZString partNum, ZDecimal quantity)
		{
			var invoice = declaration.Invoices.FirstOrDefault() ?? declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = quantity;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = (JobComInvoiceLine)(invoice.InvoiceLines.FirstOrDefault() ?? invoice.JobComInvoiceLines.AddNew());
			invoiceLine.JI_PartNo = partNum;
			invoiceLine.JI_InvoiceQuantity = quantity;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = quantity * 5;
			invoiceLine.JI_LinePrice = quantity * 10;
		}

		public static void CreateWarehouse(BusinessObjectFactory factory, OrgHeader warehouseOrg, ZString warehouseCode)
		{
			var locationType = factory.New<IWhsLocationType>();
			locationType.WLT_Code = warehouseCode;
			locationType.WLT_Description = warehouseCode;

			var warehouse = factory.New<IWhsWarehouse>();
			warehouse.WW_WarehouseCode = warehouseCode;
			warehouse.WW_WarehouseName = warehouseOrg.MainAddress.OA_Address1;
			warehouse.WW_OA_WarehouseAddress = warehouseOrg.MainAddress.PK;
			warehouse.WW_IsBondedWarehouse = true;
			warehouse.WW_IsVirtualWarehouse = true;
			((IWhsArea)warehouse.Areas[0]).WA_AreaType = "BON";
			warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
			warehouse.WW_AutoPrintPackingSlip = false;
			warehouse.WW_WLT_DefaultLocationType = locationType.PK;

			var row = (IWhsRow)warehouse.Rows.AddNew();
			row.WR_Name = "BOND";
			row.WR_Columns = 1;
			row.WR_Levels = 1;
			row.WR_Trays = 1;
		}
	}
}
