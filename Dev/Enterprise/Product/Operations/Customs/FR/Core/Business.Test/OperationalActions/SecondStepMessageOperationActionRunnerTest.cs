using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.OperationalActions.Testing
{
	public class SecondStepMessageOperationActionRunnerTest : TestCaseWithFactory
	{
		public void TestSendSecondStepMessage()
		{
			var deltaG2Importer = Factory.NewWithValidTestData<OrgHeader>();
			deltaG2Importer.MainAddress.OA_PostCode = "123456";
			deltaG2Importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G2, "123", ZString.Empty, ZString.Empty, "A4D9F6E0");
			var deltaG1Importer = Factory.NewWithValidTestData<OrgHeader>();
			deltaG1Importer.MainAddress.OA_PostCode = "123456";
			deltaG1Importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "456", ZString.Empty, ZString.Empty, "962A194A");
			var deltaG2Supplier = Factory.NewWithValidTestData<OrgHeader>();
			deltaG2Supplier.MainAddress.OA_PostCode = "123456";
			deltaG2Supplier.SetupAccount(OrgCusAccountCodeList.Codes.DGE, OrgCusAccountDeltaGTypeList.Codes.G2, "789", ZString.Empty, ZString.Empty, "C7D6A1C5");

			var importDeclaration = SetupJobDeclarationForAutoSending(Common.EU.EUJobMessageTypeList.Codes.Import, deltaG2Importer.PK, ZGuid.Empty, EntryStatusDescriptionCodeList.Codes.ES100, "19212081311");
			var exportDeclaration = SetupJobDeclarationForAutoSending(Common.EU.EUJobMessageTypeList.Codes.Export, ZGuid.Empty, deltaG2Supplier.PK, EntryStatusDescriptionCodeList.Codes.ES100, "19212081312");
			var declarationWithUnsuitableDeltaMode = SetupJobDeclarationForAutoSending(Common.EU.EUJobMessageTypeList.Codes.Import, deltaG1Importer.PK, ZGuid.Empty, EntryStatusDescriptionCodeList.Codes.ES100, "19212081314");
			var declarationAlreadySent = SetupJobDeclarationForAutoSending(Common.EU.EUJobMessageTypeList.Codes.Import, deltaG2Importer.PK, ZGuid.Empty, EntryStatusDescriptionCodeList.Codes.ES130, "19212081315");

			Factory.Save();

			var log = new Services.OperationalActions.Support.Testing.DummyOperationalActionSectionLog();
			var secondStepMessageOperationalActionRunner = new SecondStepMessageOperationalActionRunner(log, new[] { importDeclaration, exportDeclaration, declarationWithUnsuitableDeltaMode, declarationAlreadySent });
			secondStepMessageOperationalActionRunner.SendSecondStepMessage();

			AssertEquals("Only DeltaG2 in status BAE should be sent to Customs.", @"INFO: 2 entries found
INFO: Automated Delta D2M messages have been sent for Customs Entries 19212081311,19212081312", string.Join("\r\n", log.messages));

			log = new Services.OperationalActions.Support.Testing.DummyOperationalActionSectionLog();
			secondStepMessageOperationalActionRunner = new SecondStepMessageOperationalActionRunner(log, new[] { declarationWithUnsuitableDeltaMode, declarationAlreadySent });
			secondStepMessageOperationalActionRunner.SendSecondStepMessage();

			AssertEquals("A specific message shouls show when no DeltaG2 BAE entry is found", @"INFO: No Delta G2 entries with entry status BAE were found", string.Join("\r\n", log.messages));
		}

		JobDeclaration SetupJobDeclarationForAutoSending(ZString messageType, ZGuid importerPK, ZGuid supplierPK, ZString entryStatus, ZString entryReference)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			declaration.JE_OH_Importer = importerPK;
			declaration.JE_OH_Supplier = supplierPK;
			declaration.JE_PaymentMethod = MethodOfPaymentList.Codes.M;

			var cei = declaration.CustomsEntryInstructions.AddNew();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_Tariff = "2203001010";
			invoiceLine.JI_Description = "Unit Test";
			invoiceLine.JI_LinePrice = 50;
			invoiceLine.JI_CEI = cei.PK;
			invoiceLine.JI_CustomsQuantity = 1m;

			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = entryStatus;
			entryHeader.CH_BGMReference = entryReference;
			entryHeader.CH_CEI_Instruction = cei.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CEI = cei.PK;
			return declaration;
		}
	}
}
