using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.WarehouseIntegration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.DE.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.DE.Business.Testing.WarehouseIntegration
{
	[TestedType(typeof(BondedWarehousePreviousProceduresFromInventoryForEntryInstructionProvider))]
	sealed class BondedWarehousePreviousProceduresFromInventoryForEntryInstructionProviderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsApplicable()
		{
			CreateOutOfWarehouseRefCusProcedure(Common.EU.EUJobMessageTypeList.Codes.Export);
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var line = invoiceHeader.InvoiceLines.AddNew();
			line.JI_CEI = instruction.PK;
			line.JI_Procedure = "4071";

			var provider = new BondedWarehousePreviousProceduresFromInventoryForEntryInstructionProvider(instruction);
			AssertEquals("Is Import and has bwh lines", true, provider.IsApplicable);

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			provider = new BondedWarehousePreviousProceduresFromInventoryForEntryInstructionProvider(instruction);
			AssertEquals("Is not Import and has bwh lines", false, provider.IsApplicable);

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			line.JI_Procedure = ZString.Empty;
			provider = new BondedWarehousePreviousProceduresFromInventoryForEntryInstructionProvider(instruction);
			AssertEquals("Is Import and has no bwh lines", false, provider.IsApplicable);

			line.JI_Procedure = "4071";
			instruction.CEI_OA_Warehouse = ZGuid.Empty;
			provider = new BondedWarehousePreviousProceduresFromInventoryForEntryInstructionProvider(instruction);
			AssertEquals("Is Import and has bwh lines but no warehouse", false, provider.IsApplicable);
		}

		public void TestCreatePreviousProcedures()
		{
			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "EDIBRNWIS";
			var declarantAddress = declarant.MainAddress;
			declarantAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "NUMBER1");

			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;

			var previousDocuments = instruction.PreviousDocuments;
			var oldPreviousDocument = previousDocuments.AddNew();
			oldPreviousDocument.CSI_LineNo = 10;

			var line1 = CreateBondedWareHouseInvoiceLine(invoiceHeader, instruction, "4071", "ATH7100001", 1, "33049900000", 1.23M, "KGM");
			var line2 = CreateBondedWareHouseInvoiceLine(invoiceHeader, instruction, "4071", "ATH7100001", 1, "33049900000", 2.34M, "KGM");
			var line3 = CreateBondedWareHouseInvoiceLine(invoiceHeader, instruction, "4071", "ATH7100002", 1, "33049900000", 3.45M, "KGM");
			var line4 = CreateBondedWareHouseInvoiceLine(invoiceHeader, instruction, "4071", "ATH7101234567890123450", 2, "33049900000", 4.56M, "KGM"); //extended AtlasRegistrationNumber to 22 characters to force truncation to 21 characters when populating CSI_ReferenceNumber
			var line5 = CreateBondedWareHouseInvoiceLine(invoiceHeader, instruction, "", "ATH7100001", 1, "33049900000", 5.67M, "KGM");

			var provider = new BondedWarehousePreviousProceduresFromInventoryForEntryInstructionProvider(instruction);

			provider.CreatePreviousProcedures();

			CombineAssertions(() =>
			{
				AssertEquals("Count", 3, previousDocuments.Count);
				AssertEquals("oldPreviousDocument is deleted", true, oldPreviousDocument.IsDeleted);

				var previousDocumentsCast = previousDocuments.Cast<PreviousDocument>();
				AssertEquals($"Quantity of {nameof(line1)} and {nameof(line2)} are accumlated and only one doc was generated", 1, previousDocumentsCast.Count(x => x.CSI_Quantity2 == 3.57M));
				AssertEquals($"{nameof(line3)} with different JI_PreviousEntryNumber", 1, previousDocumentsCast.Count(x => x.CSI_Quantity2 == 3.45M));

				PreviousDocument previousDocumentForLine4 = null;
				AssertNoExceptionThrown($"{nameof(line4)} with different JI_PreviousEntryLineNumber", () => previousDocumentForLine4 = previousDocumentsCast.Single(x => x.CSI_Quantity2 == 4.56M));
				AssertEquals("CSI_Procedure", PreviousProcedureList.Codes._ATZL, previousDocumentForLine4.CSI_Procedure);
				AssertEquals("CSI_ReferenceNumber2", "LRN001", previousDocumentForLine4.CSI_ReferenceNumber2);
				AssertEquals("AuthorizationNumber", "NUMBER1", previousDocumentForLine4.AuthorizationNumber);
				AssertEquals("CSI_ReferenceNumber", "ATH710123456789012345", previousDocumentForLine4.CSI_ReferenceNumber);
				AssertEquals("CSI_LineNo", 2, previousDocumentForLine4.CSI_LineNo);
				AssertEquals("CSI_Tariff", "33049900000", previousDocumentForLine4.CSI_Tariff);
				AssertEquals("CSI_Status", "Y", previousDocumentForLine4.CSI_Status); //Ensure the actual (truncated) CSI_ReferenceNumber is used to determine Status not the source value from JI_PreviousEntryNumber
				AssertEquals("CSI_UnitOfQuantity2", "KGM", previousDocumentForLine4.CSI_UnitOfQuantity2);

				AssertEquals($"IsOutOfWarehouseWarehousing is false for {nameof(line5)}", 0, previousDocumentsCast.Count(x => x.CSI_Quantity2 == 5.67M));
			});
		}

		public void TestCreatePreviousProcedures_KeepExistingAuthorizationNumber()
		{
			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "EDIBRNWIS";
			var declarantAddress = declarant.MainAddress;
			declarantAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "NUMBER1");
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			var previousDocument = instruction.PreviousDocuments.AddNew();
			previousDocument.AuthorizationNumber = "AuthNumberBefore";

			CreateBondedWareHouseInvoiceLine(invoiceHeader, instruction, "4071", "ATH7100001", 1, "33049900000", 1.23M, "KGM");

			var provider = new BondedWarehousePreviousProceduresFromInventoryForEntryInstructionProvider(instruction);

			provider.CreatePreviousProcedures();
			AssertEquals("AuthNumberBefore", instruction.PreviousDocuments[0].AuthorizationNumber);
		}

		public void TestCreatePreviousProcedures_MultipleAuthorizationNumbers()
		{
			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "EDIBRNWIS";
			var declarantAddress = declarant.MainAddress;
			declarantAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "NUMBER1");
			declarantAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, "NUMBER2");

			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;

			CreateBondedWareHouseInvoiceLine(invoiceHeader, instruction, "4071", "ATH7100001", 1, "33049900000", 1.23M, "KGM");

			var provider = new BondedWarehousePreviousProceduresFromInventoryForEntryInstructionProvider(instruction);

			provider.CreatePreviousProcedures();
			AssertEquals(ZString.Empty, instruction.PreviousDocuments[0].AuthorizationNumber);
		}

		public void TestCreatePreviousProcedures_CSI_Status()
		{
			var line = CreateBondedWareHouseInvoiceLine(invoiceHeader, instruction, "4071", "", 1, "33049900000", 1.23M, "KGM");
			var provider = new BondedWarehousePreviousProceduresFromInventoryForEntryInstructionProvider(instruction);

			var testCase = (ZString referenceNumber) =>
			{
				line.JI_PreviousEntryNumber = referenceNumber;
				provider.CreatePreviousProcedures();
				return (bool)instruction.PreviousDocuments[0].Status;
			};

			PreviousDocumentHelperTest.AssertIsValidAtlasReferenceForBondedWarehouse(testCase, expectAutomaticalTruncationWhenPopulatingCSI_ReferenceNumber: true);
			PreviousDocumentHelperTest.AssertIsValidAtlasReferenceForBondedWarehouse_MRN(testCase);
		}

		void CreateOutOfWarehouseRefCusProcedure(ZString shipmentType)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany);
			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = "40";
			procedure.ZZ6_PreviousProcedureCode = "71";
			procedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Germany;
			procedure.ZZ6_ShipmentType = shipmentType;
			procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
			procedure.ZZ6_Description = "Out of Warehouse";
		}

		JobComInvoiceLine CreateBondedWareHouseInvoiceLine(JobComInvoiceHeader invoiceHeader, CusEntryInstruction instruction, ZString procedure,
			ZString previousEntryNumber, ZShort previousEntryLineNumber, ZString tariff, ZDecimal bondedWhsQuantity, ZString bondedWhsUnitQty, JobComInvoiceLine invoiceLine = null)
		{
			if (invoiceLine == null)
			{
				invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			}
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = procedure;
			invoiceLine.JI_PreviousEntryNumber = previousEntryNumber;
			invoiceLine.JI_PreviousEntryLineNumber = previousEntryLineNumber;
			invoiceLine.JI_Tariff = tariff;
			invoiceLine.JI_BondedWhsQuantity = bondedWhsQuantity;
			invoiceLine.JI_BondedWhsUnitQty = bondedWhsUnitQty;
			return invoiceLine;
		}

		protected override BusinessObject GetNewBusinessObject() =>
			new BondedWarehousePreviousProceduresFromInventoryForEntryInstructionProvider(instruction);

		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			CreateOutOfWarehouseRefCusProcedure(Common.EU.EUJobMessageTypeList.Codes.Import);

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			instruction = declaration.CustomsEntryInstructions.AddNew();

			var entryHeader = declaration.ActiveEntryHeaders.AddNew() as CusEntryHeader;
			entryHeader.CH_CEI_Instruction = instruction.PK;
			entryHeader.LocalReferenceNumber = "LRN001";

			invoiceHeader = declaration.Invoices.AddNew();

			var fromWarehouseOrg = Factory.NewWithValidTestData<OrgHeader>();
			instruction.CEI_OA_Warehouse = fromWarehouseOrg.MainAddress.PK;
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		CusEntryInstruction instruction;
	}
}
