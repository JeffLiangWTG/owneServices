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
	[TestedType(typeof(InwardProcessingPreviousProceduresFromInventoryForEntryInstructionProvider))]
	sealed class InwardProcessingPreviousProceduresFromInventoryForEntryInstructionProviderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsApplicable()
		{
			CreateOutOfInwardProcessingRefCusProcedure(Common.EU.EUJobMessageTypeList.Codes.Export);
			var line = invoiceHeader.InvoiceLines.AddNew();
			line.JI_CEI = instruction.PK;
			line.JI_Procedure = "4051";

			var provider = new InwardProcessingPreviousProceduresFromInventoryForEntryInstructionProvider(instruction);
			AssertEquals("Is Import and has ipr lines", true, provider.IsApplicable);

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			provider = new InwardProcessingPreviousProceduresFromInventoryForEntryInstructionProvider(instruction);
			AssertEquals("Is not Import and has ipr lines", false, provider.IsApplicable);

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			line.JI_Procedure = ZString.Empty;
			provider = new InwardProcessingPreviousProceduresFromInventoryForEntryInstructionProvider(instruction);
			AssertEquals("Is Import and has no ipr lines", false, provider.IsApplicable);

			line.JI_Procedure = "4051";
			instruction.CEI_OA_Warehouse = ZGuid.Empty;
			provider = new InwardProcessingPreviousProceduresFromInventoryForEntryInstructionProvider(instruction);
			AssertEquals("Is Import and has ipr lines but no warehouse", false, provider.IsApplicable);
		}

		public void TestCreatePreviousProcedures()
		{
			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "EDIBRNWIS";
			var declarantAddress = declarant.MainAddress;
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER1");

			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			var previousDocuments = instruction.PreviousDocuments;
			var oldPreviousDocument = previousDocuments.AddNew();
			oldPreviousDocument.CSI_LineNo = 10;

			var line1 = CreateInwardProcessingInvoiceLine(invoiceHeader, instruction, "4051", "ATC5100001", 1, "33049900000");
			var line2 = CreateInwardProcessingInvoiceLine(invoiceHeader, instruction, "4051", "ATC5100002", 1, "33049900000");
			var line3 = CreateInwardProcessingInvoiceLine(invoiceHeader, instruction, "4051", "ATC510123456789012345", 2, "33049900000");
			var line4 = CreateInwardProcessingInvoiceLine(invoiceHeader, instruction, "", "ATH5100001", 1, "33049900000");

			var provider = new InwardProcessingPreviousProceduresFromInventoryForEntryInstructionProvider(instruction);

			provider.CreatePreviousProcedures();

			CombineAssertions(() =>
			{
				AssertEquals($"Count: {nameof(line1)} {nameof(line2)} {nameof(line3)}", 3, previousDocuments.Count);
				AssertEquals("oldPreviousDocument is deleted", true, oldPreviousDocument.IsDeleted);
				var previousDocumentsCast = previousDocuments.Cast<PreviousDocument>();

				PreviousDocument previousDocumentForLine3 = null;
				AssertNoExceptionThrown($"{nameof(line3)} with different JI_PreviousEntryLineNumber", () => previousDocumentForLine3 = previousDocumentsCast.Single(x => x.CSI_ReferenceNumber == "ATC510123456789012345"));
				AssertEquals("CSI_Procedure", PreviousProcedureList.Codes._ATAV, previousDocumentForLine3.CSI_Procedure);
				AssertEquals("AuthorizationNumber", "NUMBER1", previousDocumentForLine3.AuthorizationNumber);
				AssertEquals("CSI_LineNo", 2, previousDocumentForLine3.CSI_LineNo);
				AssertEquals("CSI_Description", "33049900000", previousDocumentForLine3.CSI_Description);
				AssertEquals("CSI_Status", "Y", previousDocumentForLine3.CSI_Status);

				AssertEquals($"IsOutOfInwardProcessing is false for {nameof(line4)}", 0, previousDocumentsCast.Count(x => x.CSI_ReferenceNumber == "ATH5100001"));
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
			CreateInwardProcessingInvoiceLine(invoiceHeader, instruction, "4051", "ATH5100001", 1, "33049900000");

			var provider = new InwardProcessingPreviousProceduresFromInventoryForEntryInstructionProvider(instruction);

			provider.CreatePreviousProcedures();
			AssertEquals("AuthNumberBefore", instruction.PreviousDocuments[0].AuthorizationNumber);
		}

		public void TestCreatePreviousProcedures_MultipleAuthorizationNumbers()
		{
			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "EDIBRNWIS";
			var declarantAddress = declarant.MainAddress;
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER1");
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER2");

			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;

			CreateInwardProcessingInvoiceLine(invoiceHeader, instruction, "4051", "ATH5100001", 1, "33049900000");

			var provider = new InwardProcessingPreviousProceduresFromInventoryForEntryInstructionProvider(instruction);

			provider.CreatePreviousProcedures();
			AssertEquals(ZString.Empty, instruction.PreviousDocuments[0].AuthorizationNumber);
		}

		public void TestCreatePreviousProcedures_CSI_Status()
		{
			var line = CreateInwardProcessingInvoiceLine(invoiceHeader, instruction, "4051", "", 1, "33049900000");
			var provider = new InwardProcessingPreviousProceduresFromInventoryForEntryInstructionProvider(instruction);

			var testCase = (ZString referenceNumber) =>
			{
				line.JI_PreviousEntryNumber = referenceNumber;
				provider.CreatePreviousProcedures();
				return (bool)instruction.PreviousDocuments[0].Status;
			};

			PreviousDocumentHelperTest.AssertIsValidAtlasReferenceForInwardProcessing_MRN(testCase);
			PreviousDocumentHelperTest.AssertIsValidAtlasReferenceForInwardProcessing(testCase, expectAutomaticalTruncationWhenPopulatingCSI_ReferenceNumber: true);
		}

		void CreateOutOfInwardProcessingRefCusProcedure(ZString shipmentType)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany);
			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = "40";
			procedure.ZZ6_PreviousProcedureCode = "51";
			procedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Germany;
			procedure.ZZ6_ShipmentType = shipmentType;
			procedure.ZZ6_OutOfInwardProcessing = WarehouseMoveStatus.Codes.Yes;
			procedure.ZZ6_Description = "Out of Inward Processing";
		}

		JobComInvoiceLine CreateInwardProcessingInvoiceLine(JobComInvoiceHeader invoiceHeader, CusEntryInstruction instruction, ZString procedure,
			ZString previousEntryNumber, ZShort previousEntryLineNumber, ZString tariff, JobComInvoiceLine invoiceLine = null)
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
			return invoiceLine;
		}

		protected override BusinessObject GetNewBusinessObject() =>
			new InwardProcessingPreviousProceduresFromInventoryForEntryInstructionProvider(instruction);

		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			CreateOutOfInwardProcessingRefCusProcedure(Common.EU.EUJobMessageTypeList.Codes.Import);

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			instruction = declaration.CustomsEntryInstructions.AddNew();

			var entryHeader = declaration.ActiveEntryHeaders.AddNew() as CusEntryHeader;
			entryHeader.CH_CEI_Instruction = instruction.PK;
			invoiceHeader = declaration.Invoices.AddNew();

			var fromWarehouseOrg = Factory.NewWithValidTestData<OrgHeader>();
			instruction.CEI_OA_Warehouse = fromWarehouseOrg.MainAddress.PK;
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		CusEntryInstruction instruction;
	}
}
