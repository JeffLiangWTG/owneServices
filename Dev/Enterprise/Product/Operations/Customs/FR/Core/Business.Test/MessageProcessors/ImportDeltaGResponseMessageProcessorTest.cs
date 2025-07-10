using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.EdiMessages.Testing;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using CusEntryHeader = Enterprise.Customs.FR.Business.Declaration.CusEntryHeader;
using CusEntryLine = Enterprise.Customs.FR.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.FR.Business.Testing
{
	sealed class ImportDeltaGResponseMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcessorBehaviourAgainstLiquidationWithSuspendVatFeeRefresh()
		{
			var (declaration, entryHeader, entryLine1, entryLine2, message) = CreateDeclaration();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.EntryInstruction.ZG_BypassCode = "A";

			AssertProcessorBehaviourAgainstLiquidation(entryHeader, entryLine1, entryLine2, message);
		}

		public void TestProcessorBehaviourAgainstLiquidationWithOutSuspendVatFeeRefresh()
		{
			var (_, entryHeader, entryLine1, entryLine2, message) = CreateDeclaration();
			AssertProcessorBehaviourAgainstLiquidation(entryHeader, entryLine1, entryLine2, message);
		}

		public void TestProcessorBehaviourAgainstLiquidation_ShouldKeepEntryLineNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "9000-B00177613";

			var entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 3;
			entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 2;
			entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 5;
			entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 4;

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			entryHeader.Messages.Add(outgoingMessage);

			Factory.Save();

			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportResponseMessageWithFiveLiquidations.xml");
			var message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			message.EM_MessageText = importMessageText;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;

			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			Factory.Save();

			AssertEquals(10m, entryHeader.AllEntryLines.FindByLineNumber(1).ConfirmedFees[0].CF_ChargeAmount);
			AssertEquals(20m, entryHeader.AllEntryLines.FindByLineNumber(2).ConfirmedFees[0].CF_ChargeAmount);
			AssertEquals(30m, entryHeader.AllEntryLines.FindByLineNumber(3).ConfirmedFees[0].CF_ChargeAmount);
			AssertEquals(40m, entryHeader.AllEntryLines.FindByLineNumber(4).ConfirmedFees[0].CF_ChargeAmount);
			AssertEquals(50m, entryHeader.AllEntryLines.FindByLineNumber(5).ConfirmedFees[0].CF_ChargeAmount);
		}

		public void TestProcessMessageDeltaC_Import_Valid()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "9000-B00177613";

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			entry.Messages.Add(outgoingMessage);

			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportValidResponseMessage.xml");

			var message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.IMC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			message.EM_MessageText = importMessageText;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;

			Factory.Save();

			var permitHeader = SetupGuarantee(entry.DeclarantOrganisation, PermitQtyValIndicatorList.Codes.BTH);
			SetupTransaction(permitHeader, entry.CH_BGMReference, outgoingMessage.EM_MessageNum);

			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());

			processor.ProcessMessage(message);
			Factory.Save();

			AssertEquals(entry.PK, message.EM_LinkedObject.PK);

			var entryNum = CusEntryNumber.Load(entry, CusEntryNumberTypes.France.Import, Core.Constants.CountryCodes.France);
			AssertNotNull("Import CusEntryNum", entryNum);
			AssertEquals("CE_EntryNum", "1901204207", entryNum.CE_EntryNum);
			AssertEquals("CE_IssueDate", new ZDateTime(2019, 2, 13, 4, 4, 0), entryNum.CE_IssueDate);
			AssertEquals("CusEntryHeader.EntryNumber", entry.EntryNumber, entryNum.CE_EntryNum);
			AssertEquals("Declaration EarliestCustomsEntryIssueDate", dec.EarliestCustomsEntryIssueDate, entryNum.CE_IssueDate);
			AssertEquals("CH_EntryStatus", EntryStatusDescriptionCodeList.Codes.ES060, entry.CH_EntryStatus);
			AssertEquals("JE_EntryStatusDescription", EntryStatusDescriptionCodeList.Descriptions.ES060, entry.Declaration.JE_EntryStatusDescription);
			entry.CH_EntryStatus = "XXX";
			AssertEquals("JE_EntryStatusDescription when CH_EntryStatus is an unknown code", "Unknown", entry.Declaration.JE_EntryStatusDescription);

			var permitQuery = new ZDBOnlySubQuery(typeof(BaseCusPermitHeader), CusPermitLineTransactionSchema.CPL_CPH_PermitHeader);
			permitQuery.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, Core.Constants.CountryCodes.France);

			var query = new ZDBOnlyQuery(typeof(BaseCusPermitLineTransaction));
			query.AddToFilter(CusPermitLineTransactionSchema.CPL_Reference, entry.CH_BGMReference);
			query.AddSubQuery(permitQuery, JoinCondition.And);

			var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(1, transactions.Length);
			AssertEquals(PermitTransactionStatusList.Codes.Pending, transactions[0].CPL_TransactionStatus);
		}

		public void TestProcessMessageDeltaC_Import_BAE()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);
			var procedure = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.France, "", "10", "71", "F61", "description", "IMP", intoWarehouse: false, outOfWarehouse: true);
			procedure.ZZ6_IsGuaranteeReleased = YesNoList.Codes.Yes;
			procedure.ZZ6_IsGuaranteeConsumed = YesNoList.Codes.Yes;
			Factory.Save();

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.OH_FullName = "The declarant";
			declarant.MainAddress.Address1 = "DeclarantAddress";

			var guarantee = GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "DGUA", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "DeclarantAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "REFA", Core.Constants.CountryCodes.France);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes.SEL;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			var cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			cusEntryInstruction.CEI_Style = "10";

			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = cusEntryInstruction.PK;
			invoiceLine.JI_Procedure = "1071F61";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "9000-B00177613";
			var entryLine = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			declaration.JE_CustomsGuaranteeNumber = "DGUA";
			AssertEquals("Prerequisite", guarantee, entryHeader.Declaration.CustomsGuarantee);

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = "FRC";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			entryHeader.Messages.Add(outgoingMessage);

			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");

			var message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.IMC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			message.EM_MessageText = importMessageText;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;

			Factory.Save();

			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());

			processor.ProcessMessage(message);
			Factory.Save();

			AssertEquals(entryHeader.PK, message.EM_LinkedObject.PK);

			var entryNum = CusEntryNumber.Load(entryHeader, CusEntryNumberTypes.France.Import, Core.Constants.CountryCodes.France);
			AssertNotNull("Import CusEntryNum", entryNum);
			AssertEquals("CE_EntryNum", "1901204207", entryNum.CE_EntryNum);
			AssertEquals("CE_IssueDate", new ZDateTime(2019, 2, 13, 4, 4, 0), entryNum.CE_IssueDate);
			AssertEquals("CusEntryHeader.EntryNumber", entryNum.CE_EntryNum, entryHeader.EntryNumber);
			AssertEquals("Declaration EarliestCustomsEntryIssueDate", declaration.EarliestCustomsEntryIssueDate, entryNum.CE_IssueDate);
			AssertEquals("CH_EntryStatus", EntryStatusDescriptionCodeList.Codes.ES100, entryHeader.CH_EntryStatus);
			AssertEquals("JE_EntryStatusDescription", EntryStatusDescriptionCodeList.Descriptions.ES100, entryHeader.Declaration.JE_EntryStatusDescription);

			var orgHeader = entryHeader.DeclarantOrganisation;
			orgHeader.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "11111", "", "", "7CD2C5CC");

			entryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			entryHeader.CH_CEI_Instruction = cusEntryInstruction.PK;
			declaration.JE_CustomsProfile = "11111";
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.JE_TotalNoOfPacks = 8;

			var package = Factory.NewWithValidTestData<BasePackage>();
			package.CW_PackQty = 8;
			declaration.Packages.Add(package);

			var packing1 = invoiceLine.PackagesForInvoiceLinesForBindingOnly[0];
			packing1.FillWithValidTestData();
			packing1.IsLinked = true;
			packing1.PackQty = 8;

			var previousDocument = declaration.PreviousDocuments.AddNew();
			previousDocument.CSI_Code = entryHeader.ComplementaryJobPreviousDocumentCodeType;
			previousDocument.CSI_ReferenceNumber = "TST1";

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			dec2.JE_TotalNoOfPacks = 10;
			var invoiceLine2 = dec2.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.JI_Procedure = "1071F61";

			var package2 = Factory.NewWithValidTestData<BasePackage>();
			package2.CW_PackQty = 8;
			dec2.Packages.Add(package2);

			var packing2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly[0];
			packing2.FillWithValidTestData();
			packing2.IsLinked = true;
			packing2.PackQty = 10;

			var entry2 = dec2.CustomsEntryHeaders.AddNew();
			entry2.EntryNumber = "TST1";
			entry2.CH_BGMReference = "9000-B00177613";
			entry2.CH_CEI_Instruction = cusEntryInstruction.PK;

			var entryLine2 = entry2.AllEntryLines.AddNew();

			invoiceLine2.JI_CL = entryLine2.PK;
			var lineFee = entryLine2.Fees.AddNew();
			lineFee.CF_ChargeType = entry2.TaxCode;
			lineFee.CF_ChargeAmount = 5m;

			guarantee.CusGuaranteeLineTransactions.DeleteAll();
			processor.ProcessMessage(message);
			Factory.Save();
			AssertEquals("A transaction line should have been created that matches the released packages.", 1, guarantee.CusGuaranteeLineTransactions.Count);
			AssertEquals("Transaction value should be calculated as (number of package released) * (original liability amount) / (original package number) ", 4m, guarantee.CusGuaranteeLineTransactions[0].CPL_TranValue);
		}

		public void TestProcessMessageDeltaC_Import_BAE_CusEntryNumberExisting()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);
			var procedure = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.France, "", "10", "71", "F61", "description", "IMP", intoWarehouse: false, outOfWarehouse: true);
			procedure.ZZ6_IsGuaranteeReleased = YesNoList.Codes.Yes;
			procedure.ZZ6_IsGuaranteeConsumed = YesNoList.Codes.Yes;
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			var cusEntryInstruction = dec.CustomsEntryInstructions.AddNew();
			cusEntryInstruction.CEI_Style = "10";

			var invoiceLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = cusEntryInstruction.PK;
			invoiceLine.JI_Procedure = "1071F61";

			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "9000-B00177613";
			var entryLine = entry.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = "FRC";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			entry.Messages.Add(outgoingMessage);

			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");

			var message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.IMC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			message.EM_MessageText = importMessageText;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;

			Factory.Save();
			var existingEntryNum = CusEntryNumber.New(entry, CusEntryNumberTypes.France.Import, Core.Constants.CountryCodes.France);
			existingEntryNum.CE_IssueDate = ZDateTime.BrettsBirthday;
			existingEntryNum.CE_EntryNum = "123";

			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			Factory.Save();

			var entryNum = CusEntryNumber.Load(entry, CusEntryNumberTypes.France.Import, Core.Constants.CountryCodes.France);
			CombineAssertions(() =>
			{
				AssertEquals("existingEntryNum CE_IssueDate", ZDateTime.BrettsBirthday, entryNum.CE_IssueDate);
				AssertEquals("existingEntryNum CE_EntryNum", "123", entryNum.CE_EntryNum);
			});
		}

		public void TestProcessMessageDeltaC_Import_BAE_UpdateCH_EntryReleaseDate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);
			var procedure = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.France, "", "10", "71", "F61", "description", "IMP", intoWarehouse: false, outOfWarehouse: true);
			procedure.ZZ6_IsGuaranteeReleased = YesNoList.Codes.Yes;
			procedure.ZZ6_IsGuaranteeConsumed = YesNoList.Codes.Yes;
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			var cusEntryInstruction = dec.CustomsEntryInstructions.AddNew();
			cusEntryInstruction.CEI_Style = "10";

			var invoiceLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = cusEntryInstruction.PK;
			invoiceLine.JI_Procedure = "1071F61";

			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "9000-B00177613";
			entry.CH_EntryReleaseDate = ZDateTime.Empty;
			var entryLine = entry.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = "FRC";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			entry.Messages.Add(outgoingMessage);

			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");

			var message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.IMC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			message.EM_MessageText = importMessageText;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;

			Factory.Save();

			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			Factory.Save();

			AssertEquals("CH_EntryReleaseDate update form issueDate when updatedToBAEForTheFirstTime is true", new ZDateTime(2019, 2, 13, 4, 4, 0), entry.CH_EntryReleaseDate);
		}

		public void TestProcessMessageDeltaC_Import_Erreur()
		{
			var dec = Factory.New<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "9000-B00177613";

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			entry.Messages.Add(outgoingMessage);

			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportErreurResponseMessage.xml");

			var message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.IMC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			message.EM_MessageText = importMessageText;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;

			Factory.Save();

			var permitHeader = SetupGuarantee(entry.DeclarantOrganisation, PermitQtyValIndicatorList.Codes.BTH);
			SetupTransaction(permitHeader, entry.CH_BGMReference, outgoingMessage.EM_MessageNum);

			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());

			processor.ProcessMessage(message);
			Factory.Save();

			AssertEquals(entry.PK, message.EM_LinkedObject.PK);
			AssertNull("No Import CusEntryNum", entry.CusEntryNumber);
			AssertEquals("CH_EntryStatus", EntryStatusDescriptionCodeList.Codes.ES040, entry.CH_EntryStatus);
			AssertEquals("JE_EntryStatusDescription", "ERROR", entry.Declaration.JE_EntryStatusDescription);

			var permitQuery = new ZDBOnlySubQuery(typeof(BaseCusPermitHeader), CusPermitLineTransactionSchema.CPL_CPH_PermitHeader);
			permitQuery.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, Core.Constants.CountryCodes.France);

			var query = new ZDBOnlyQuery(typeof(BaseCusPermitLineTransaction));
			query.AddToFilter(CusPermitLineTransactionSchema.CPL_Reference, entry.CH_BGMReference);
			query.AddSubQuery(permitQuery, JoinCondition.And);

			var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(1, transactions.Length);
			AssertEquals(PermitTransactionStatusList.Codes.Pending, transactions[0].CPL_TransactionStatus);
		}

		public void TestProcessMessageWhenAmendingAI2AmountWithNoCurrentTransactions()
		{
			var toDay = ZDate.Today;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("ZZ1", 10.0m, "FR", 0m, 0m, GuaranteeTypeList.Codes.AI2, toDay.AddDays(-1), toDay.AddDays(1), "Test AI2");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.ZG_VATDeferNumber = "12345";
			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_ValuationDateOverride = toDay;

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();

			invoiceLine.JI_CL = cusEntryLine.PK;

			var fee1 = cusEntryLine.Fees.AddNew();
			fee1.NationalFeeTypeCode = "ZZ1";
			fee1.CF_ChargeAmount = 20m;
			cusEntryHeader.CH_BGMReference = "9000-B00177613";
			cusEntryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES118;

			var permitHeader = SetupGuarantee(cusEntryHeader.DeclarantOrganisation, PermitQtyValIndicatorList.Codes.BTH);

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = "FRC";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			cusEntryHeader.Messages.Add(outgoingMessage);

			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessageWithAcceptedRectification.xml");

			var message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.IMC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			message.EM_MessageText = importMessageText;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;

			Factory.Save();

			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());

			processor.ProcessMessage(message);
			Factory.Save();

			AssertEquals("CH_EntryStatus", EntryStatusDescriptionCodeList.Codes.ES100, cusEntryHeader.CH_EntryStatus);

			var permitQuery = new ZDBOnlySubQuery(typeof(BaseCusPermitHeader), CusPermitLineTransactionSchema.CPL_CPH_PermitHeader);
			permitQuery.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, Core.Constants.CountryCodes.France);

			var query = new ZDBOnlyQuery(typeof(BaseCusPermitLineTransaction));
			query.AddToFilter(CusPermitLineTransactionSchema.CPL_Reference, cusEntryHeader.CH_BGMReference);
			query.AddSubQuery(permitQuery, JoinCondition.And);

			var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(1, transactions.Length);
			AssertEquals(-20.0m, transactions[0].CPL_TranValue);

			fee1.CF_ChargeAmount = 150m;

			var message2 = Factory.New<DeltaCImportFREDIMessage>();
			message2.EM_ApplicationCode = "FRC";
			message2.EM_MessageType = MessageTypeList.Codes.IMC;
			message2.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			message2.EM_MessageText = importMessageText;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();

			cusEntryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES118;
			processor.ProcessMessage(message2);
			Factory.Save();

			AssertEquals("CH_EntryStatus", EntryStatusDescriptionCodeList.Codes.ES100, cusEntryHeader.CH_EntryStatus);

			var transactions2 = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(2, transactions2.Length);
			AssertEquals(-20.0m, transactions2[0].CPL_TranValue);
			AssertEquals(-130.0m, transactions2[1].CPL_TranValue);
		}

		public void TestProcessMessageWithRefusedRectificationWhenAmendingAI2AmountWithIncreasingAI2Amount()
		{
			var toDay = ZDate.Today;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("ZZ1", 10.0m, "FR", 0m, 0m, GuaranteeTypeList.Codes.AI2, toDay.AddDays(-1), toDay.AddDays(1), "Test AI2");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.ZG_VATDeferNumber = "12345";
			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_ValuationDateOverride = toDay;

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();

			invoiceLine.JI_CL = cusEntryLine.PK;

			var fee1 = cusEntryLine.Fees.AddNew();
			fee1.NationalFeeTypeCode = "ZZ1";
			fee1.CF_ChargeAmount = 90m;
			cusEntryHeader.CH_BGMReference = "9000-B00177613";
			cusEntryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES118;

			var permitHeader = SetupGuarantee(cusEntryHeader.DeclarantOrganisation, PermitQtyValIndicatorList.Codes.BTH);
			SetupTransaction(permitHeader, cusEntryHeader.CH_BGMReference, "1", PermitTransactionStatusList.Codes.Confirmed);

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = "FRC";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			cusEntryHeader.Messages.Add(outgoingMessage);

			var importBAEMessageWithRefusedRectificationText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessageWithRefusedRectification.xml");

			var message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.IMC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			message.EM_MessageText = importBAEMessageWithRefusedRectificationText;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;

			Factory.Save();

			var permitQuery = new ZDBOnlySubQuery(typeof(BaseCusPermitHeader), CusPermitLineTransactionSchema.CPL_CPH_PermitHeader);
			permitQuery.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, Core.Constants.CountryCodes.France);
			var query = new ZDBOnlyQuery(typeof(BaseCusPermitLineTransaction));
			query.AddToFilter(CusPermitLineTransactionSchema.CPL_Reference, cusEntryHeader.CH_BGMReference);
			query.AddSubQuery(permitQuery, JoinCondition.And);
			var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(1, transactions.Length);
			AssertEquals(-100.0m, transactions[0].CPL_TranValue);

			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());

			processor.ProcessMessage(message);
			Factory.Save();

			AssertEquals("CH_EntryStatus", EntryStatusDescriptionCodeList.Codes.ES100, cusEntryHeader.CH_EntryStatus);

			var transactions1 = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals("There is no change to the transction because the BAE message received did not accept rectification", 1, transactions1.Length);
			AssertEquals(-100.0m, transactions1[0].CPL_TranValue);

			fee1.CF_ChargeAmount = 150m;

			var importBAEMessageWithoutRefusedRectificationText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessageWithAcceptedRectification.xml");

			var message2 = Factory.New<DeltaCImportFREDIMessage>();
			message2.EM_ApplicationCode = "FRC";
			message2.EM_MessageType = MessageTypeList.Codes.IMC;
			message2.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			message2.EM_MessageText = importBAEMessageWithoutRefusedRectificationText;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();

			cusEntryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES118;
			processor.ProcessMessage(message2);
			Factory.Save();

			AssertEquals("CH_EntryStatus", EntryStatusDescriptionCodeList.Codes.ES100, cusEntryHeader.CH_EntryStatus);

			var transactions2 = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals("A new transction is added because the BAE message received accepted rectification", 2, transactions2.Length);
			AssertEquals(-100.0m, transactions2[0].CPL_TranValue);
			AssertEquals("A new transction is added because the BAE message received accepted Rectification", -50.0m, transactions2[1].CPL_TranValue);
		}

		public void TestProcessMessageWhenAmendingAI2AmountWithIncreasingAI2Amount()
		{
			var toDay = ZDate.Today;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("ZZ1", 10.0m, "FR", 0m, 0m, GuaranteeTypeList.Codes.AI2, toDay.AddDays(-1), toDay.AddDays(1), "Test AI2");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.ZG_VATDeferNumber = "12345";
			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_ValuationDateOverride = toDay;

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();

			invoiceLine.JI_CL = cusEntryLine.PK;

			var fee1 = cusEntryLine.Fees.AddNew();
			fee1.NationalFeeTypeCode = "ZZ1";
			fee1.CF_ChargeAmount = 90m;
			cusEntryHeader.CH_BGMReference = "9000-B00177613";
			cusEntryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES118;

			var permitHeader = SetupGuarantee(cusEntryHeader.DeclarantOrganisation, PermitQtyValIndicatorList.Codes.BTH);
			SetupTransaction(permitHeader, cusEntryHeader.CH_BGMReference, "1", PermitTransactionStatusList.Codes.Confirmed);

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = "FRC";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			cusEntryHeader.Messages.Add(outgoingMessage);

			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessageWithAcceptedRectification.xml");

			var message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.IMC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			message.EM_MessageText = importMessageText;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;

			Factory.Save();

			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());

			processor.ProcessMessage(message);
			Factory.Save();

			AssertEquals("CH_EntryStatus", EntryStatusDescriptionCodeList.Codes.ES100, cusEntryHeader.CH_EntryStatus);

			var permitQuery = new ZDBOnlySubQuery(typeof(BaseCusPermitHeader), CusPermitLineTransactionSchema.CPL_CPH_PermitHeader);
			permitQuery.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, Core.Constants.CountryCodes.France);

			var query = new ZDBOnlyQuery(typeof(BaseCusPermitLineTransaction));
			query.AddToFilter(CusPermitLineTransactionSchema.CPL_Reference, cusEntryHeader.CH_BGMReference);
			query.AddSubQuery(permitQuery, JoinCondition.And);

			var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(2, transactions.Length);
			AssertEquals(-100.0m, transactions[0].CPL_TranValue);
			AssertEquals(10.0m, transactions[1].CPL_TranValue);

			fee1.CF_ChargeAmount = 150m;

			var message2 = Factory.New<DeltaCImportFREDIMessage>();
			message2.EM_ApplicationCode = "FRC";
			message2.EM_MessageType = MessageTypeList.Codes.IMC;
			message2.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			message2.EM_MessageText = importMessageText;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();

			cusEntryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES118;
			processor.ProcessMessage(message2);
			Factory.Save();

			AssertEquals("CH_EntryStatus", EntryStatusDescriptionCodeList.Codes.ES100, cusEntryHeader.CH_EntryStatus);

			var transactions2 = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(3, transactions2.Length);
			AssertEquals(-100.0m, transactions2[0].CPL_TranValue);
			AssertEquals(10.0m, transactions2[1].CPL_TranValue);
			AssertEquals(-60.0m, transactions2[2].CPL_TranValue);
		}

		public void TestProcessMessageWhenAmendingAI2AmountWithDecreasingAI2Amount()
		{
			var toDay = ZDate.Today;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("ZZ1", 10.0m, "FR", 0m, 0m, GuaranteeTypeList.Codes.AI2, toDay.AddDays(-1), toDay.AddDays(1), "Test AI2");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.ZG_VATDeferNumber = "12345";
			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_ValuationDateOverride = toDay;

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();

			invoiceLine.JI_CL = cusEntryLine.PK;

			var fee1 = cusEntryLine.Fees.AddNew();
			fee1.NationalFeeTypeCode = "ZZ1";
			fee1.CF_ChargeAmount = 90m;
			cusEntryHeader.CH_BGMReference = "9000-B00177613";
			cusEntryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES118;

			var permitHeader = SetupGuarantee(cusEntryHeader.DeclarantOrganisation, PermitQtyValIndicatorList.Codes.BTH);
			SetupTransaction(permitHeader, cusEntryHeader.CH_BGMReference, "1", PermitTransactionStatusList.Codes.Confirmed);

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = "FRC";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			cusEntryHeader.Messages.Add(outgoingMessage);

			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessageWithAcceptedRectification.xml");

			var message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.IMC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			message.EM_MessageText = importMessageText;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;

			Factory.Save();

			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());

			processor.ProcessMessage(message);
			Factory.Save();

			AssertEquals("CH_EntryStatus", EntryStatusDescriptionCodeList.Codes.ES100, cusEntryHeader.CH_EntryStatus);

			var permitQuery = new ZDBOnlySubQuery(typeof(BaseCusPermitHeader), CusPermitLineTransactionSchema.CPL_CPH_PermitHeader);
			permitQuery.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, Core.Constants.CountryCodes.France);

			var query = new ZDBOnlyQuery(typeof(BaseCusPermitLineTransaction));
			query.AddToFilter(CusPermitLineTransactionSchema.CPL_Reference, cusEntryHeader.CH_BGMReference);
			query.AddSubQuery(permitQuery, JoinCondition.And);

			var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(2, transactions.Length);
			AssertEquals(-100.0m, transactions[0].CPL_TranValue);
			AssertEquals(10.0m, transactions[1].CPL_TranValue);

			fee1.CF_ChargeAmount = 50m;

			var message2 = Factory.New<DeltaCImportFREDIMessage>();
			message2.EM_ApplicationCode = "FRC";
			message2.EM_MessageType = MessageTypeList.Codes.IMC;
			message2.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			message2.EM_MessageText = importMessageText;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();

			cusEntryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES118;
			processor.ProcessMessage(message2);
			Factory.Save();

			AssertEquals("CH_EntryStatus", EntryStatusDescriptionCodeList.Codes.ES100, cusEntryHeader.CH_EntryStatus);

			var transactions2 = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(3, transactions2.Length);
			AssertEquals(-100.0m, transactions2[0].CPL_TranValue);
			AssertEquals(10.0m, transactions2[1].CPL_TranValue);
			AssertEquals(40.0m, transactions2[2].CPL_TranValue);
		}

		#region AddEntryHeaderLogWhenUpdateEntryStatus
		public void TestAddEntryHeaderLogWhenUpdateEntryStatusValid()
		{
			AssertAddEntryHeaderLogofBAE("DeltaCImportBAEResponseMessage.xml", ZBool.True);
		}

		public void TestAddEntryHeaderLogWhenUpdateEntryStatusInvalid()
		{
			AssertAddEntryHeaderLogofBAE("DeltaCImportErreurResponseMessage.xml", ZBool.False);
		}

		void AssertAddEntryHeaderLogofBAE(string fileName, ZBool logCanbeAdded)
		{
			var dec = Factory.New<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "9000-B00177613";

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = "FRC";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			entry.Messages.Add(outgoingMessage);

			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles." + fileName);

			var message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.IMC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			message.EM_MessageText = importMessageText;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;

			Factory.Save();

			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());
			var query = new ZQuery(StmALogSchema.SL_Reference, "100");
			query.AddToFilter(StmALogSchema.SL_EventTime, new ZDateTime(2019, 2, 13, 4, 4, 0));
			AssertNull("not log", entry.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus, query));
			processor.ProcessMessage(message);
			if (logCanbeAdded)
			{
				AssertNotNull("A new log has been added", entry.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus, query));
			}
			else
			{
				AssertNull("not log yet", entry.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus, query));
			}
		}
		#endregion

		public void TestProcessMessageDeltaC_Import_EventReference()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "9000-B00177613";

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			entry.Messages.Add(outgoingMessage);

			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportANTResponseMessage.xml");

			var message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.IMC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			message.EM_MessageText = importMessageText;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;

			Factory.Save();

			var permitHeader = SetupGuarantee(entry.DeclarantOrganisation, PermitQtyValIndicatorList.Codes.BTH);
			SetupTransaction(permitHeader, entry.CH_BGMReference, outgoingMessage.EM_MessageNum);

			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());

			processor.ProcessMessage(message);
			Factory.Save();

			var log = entry.Logs.EarliestLogByEventTime(Events.CustomsEntryStatus, l => l.SL_Reference != ZString.Empty);
			AssertEquals("SL_Reference", "Embarquement Transmanche UK vers FR620001", log.SL_Reference);
			var log2 = entry.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus, l => l.SL_Reference != ZString.Empty);
			AssertEquals("SL_Reference", "050", log2.SL_Reference);
		}

		public void TestProcessMessageDeltaD_Import_EventReference()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "9000-B00177613";

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			entry.Messages.Add(outgoingMessage);

			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportANTResponseMessage.xml");

			var message = Factory.New<DeltaDImportFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.IMD;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMD;
			message.EM_MessageText = importMessageText;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;

			Factory.Save();

			var permitHeader = SetupGuarantee(entry.DeclarantOrganisation, PermitQtyValIndicatorList.Codes.BTH);
			SetupTransaction(permitHeader, entry.CH_BGMReference, outgoingMessage.EM_MessageNum);

			var processor = new ImportDeltaDResponseMessageProcessor(new LoggingInformation());

			processor.ProcessMessage(message);
			Factory.Save();

			var log = entry.Logs.EarliestLogByEventTime(Events.CustomsEntryStatus, l => l.SL_Reference != ZString.Empty);
			AssertEquals("SL_Reference", "Embarquement Transmanche UK vers FR620001", log.SL_Reference);
			var log2 = entry.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus, l => l.SL_Reference != ZString.Empty);
			AssertEquals("SL_Reference", "050", log2.SL_Reference);
		}

		public void TestEventIsCreatedWithStatus55DeltaC()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			var cusEntryInstruction = dec.CustomsEntryInstructions.AddNew();
			cusEntryInstruction.CEI_Style = "10P";

			var invoiceLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = cusEntryInstruction.PK;
			invoiceLine.JI_Procedure = "1071F61";

			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "9000-B00177613";
			var entryLine = entry.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			entry.Messages.Add(outgoingMessage);

			var message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.IMC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.France, "", "10", "71", "F61", "", "IMP", intoWarehouse: false, outOfWarehouse: true);

			var orgHeader = entry.DeclarantOrganisation;
			orgHeader.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "11111", "", "", "94E87565");
			orgHeader.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G2, "22222", "", "", "94E87565");

			entry.CH_CEI_Instruction = cusEntryInstruction.PK;
			dec.JE_CustomsProfile = "11111";
			dec.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			dec.JE_TotalNoOfPacks = 8;

			var previousDocument = dec.PreviousDocuments.AddNew();
			previousDocument.CSI_Code = entry.ComplementaryJobPreviousDocumentCodeType;
			previousDocument.CSI_ReferenceNumber = "TST1";

			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_OH_PermitHolder = entry.DeclarantOrganisation.PK;
			guaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			guaranteeHeader.CPH_Number = "12345";
			guaranteeHeader.CPH_StartDate = ZDate.Today.AddYears(-20);
			guaranteeHeader.CPH_EndDate = ZDate.Today.AddYears(20);
			guaranteeHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.QTY;
			guaranteeHeader.CPH_UnitOfMeasure = "KGM";
			guaranteeHeader.CPH_Type = "9001";
			guaranteeHeader.CPH_Type = GuaranteeTypeList.Codes.AI2;

			var rule = guaranteeHeader.CusGuaranteeRules.AddNew();
			rule.CPR_RuleCode = "ADD";
			rule.CPR_ValueFrom = dec.DeclarantOrgAddress.AddressCode;

			var openingBalanceTransaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			openingBalanceTransaction.CPL_TranQty = 1000;
			openingBalanceTransaction.CPL_TranValue = 10000;
			openingBalanceTransaction.CPL_Reference = "Opening Balance";
			openingBalanceTransaction.CPL_TransactionDate = ZDateTime.Today;
			openingBalanceTransaction.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			openingBalanceTransaction.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.VAL;
			openingBalanceTransaction.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Confirmed;

			var pendingTransaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			pendingTransaction.CPL_TranQty = 0;
			pendingTransaction.CPL_TranValue = -60;
			pendingTransaction.CPL_Reference = "9000-B00177613";
			pendingTransaction.CPL_TransactionDate = ZDateTime.Today;
			pendingTransaction.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;
			pendingTransaction.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.VAL;
			pendingTransaction.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Pending;

			Factory.Save();

			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportResponseMessageEAVWithLiquidation.xml");
			entry.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES040;
			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			AssertEquals(EntryStatusDescriptionCodeList.Codes.ES055, entry.CH_EntryStatus);
			Factory.Save();

			var log = entry.Logs.EarliestLogByEventTime(Events.CustomsEntryStatus, l => l.SL_Reference != ZString.Empty);
			AssertEquals("SL_Reference", EntryStatusDescriptionCodeList.Codes.ES055, log.SL_Reference);
		}

		public void TestProcessMessageDeltaC_Import_SendVAAMessage()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			dec.JE_GoodsOrigin = Core.Constants.CountryCodes.UnitedKingdom;
			dec.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "DGI001", ZString.Empty, ZString.Empty, "DD0E1C6E");
			dec.SetupImporter(importer);
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "9000-B00177613";
			entry.MergedLines.AddNew();

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			entry.Messages.Add(outgoingMessage);

			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportANTResponseMessage.xml");

			var message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.IMC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			message.EM_MessageText = importMessageText;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;

			Factory.Save();

			var permitHeader = SetupGuarantee(entry.DeclarantOrganisation, PermitQtyValIndicatorList.Codes.BTH);
			SetupTransaction(permitHeader, entry.CH_BGMReference, outgoingMessage.EM_MessageNum);
			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);

			var createdVAAMessage = entry.Messages.Cast<EDIMessage>().FirstOrDefault(x => x.EM_MessageSubType == EntryActionCodeList.Codes.VAA);
			AssertNotNull("A VAA message shound have been sent.", createdVAAMessage);
			Assert("Saving of factory should be delayed.", !createdVAAMessage.IsInDatabase);
		}

		public void TestProcessMessageDeltaD_Import_SendVAAMessage()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			dec.JE_GoodsOrigin = Core.Constants.CountryCodes.UnitedKingdom;
			dec.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G2, "DGI001", ZString.Empty, ZString.Empty, "8AD336F5");
			dec.SetupImporter(importer);
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "9000-B00177613";
			var entryInstruction = dec.CustomsEntryInstructions.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = dec.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_CL = entryLine.PK;
			entry.CH_CEI_Instruction = entryInstruction.PK;

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			entry.Messages.Add(outgoingMessage);

			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportANTResponseMessage.xml");

			var message = Factory.New<DeltaDImportFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.IMD;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMD;
			message.EM_MessageText = importMessageText;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;

			Factory.Save();

			var permitHeader = SetupGuarantee(entry.DeclarantOrganisation, PermitQtyValIndicatorList.Codes.BTH);
			SetupTransaction(permitHeader, entry.CH_BGMReference, outgoingMessage.EM_MessageNum);
			var processor = new ImportDeltaDResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);

			var createdVAAMessage = entry.Messages.Cast<EDIMessage>().FirstOrDefault(x => x.EM_MessageSubType == EntryActionCodeList.Codes.VAA);
			AssertNotNull("A VAA message shound have been sent.", createdVAAMessage);
			Assert("Saving of factory should be delayed.", !createdVAAMessage.IsInDatabase);
		}

		(JobDeclaration, CusEntryHeader, CusEntryLine, CusEntryLine, DeltaCImportFREDIMessage) CreateDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			var invoiceHeader = declaration.Invoices.AddNew();

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CustomsSecondUnitQty = "DTNG";
			invoiceLine1.JI_CustomsSecondQuantity = 400;
			invoiceLine1.JI_CustomsThirdUnitQty = "HLT";
			invoiceLine1.JI_CustomsSecondQuantity = 100;

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CustomsSecondUnitQty = "DTNG";
			invoiceLine2.JI_CustomsSecondQuantity = 100;
			invoiceLine2.JI_CustomsThirdUnitQty = "HLT";
			invoiceLine2.JI_CustomsSecondQuantity = 100;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "9000-B00177613";

			var entryLine1 = (CusEntryLine)entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.InvoiceLines.Add(invoiceLine1);
			entryLine1.Fees.AddNew();
			entryLine1.ConfirmedFees.AddNew();
			var entryLine2 = (CusEntryLine)entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;

			var entryHeaderCharge = entryHeader.Charges.AddNew();
			entryHeaderCharge.C1_ChargeType = "V905";
			entryHeaderCharge.C1_ChargeAmount = 100;
			entryHeaderCharge.C1_MethodOfPayment = "";

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			entryHeader.Messages.Add(outgoingMessage);

			Factory.Save();

			var guaranteeHeader = SetupGuarantee(entryHeader.DeclarantOrganisation, PermitQtyValIndicatorList.Codes.BTH);
			SetupTransaction(guaranteeHeader, entryHeader.CH_BGMReference, outgoingMessage.EM_MessageNum);

			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportResponseMessageWithLiquidation.xml");

			var message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.IMC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			message.EM_MessageText = importMessageText;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;

			Factory.Save();
			return (declaration, entryHeader, entryLine1, entryLine2, message);
		}

		CusGuaranteeHeader SetupGuarantee(OrgHeader org, ZString permitIndicator, bool useEndDate = true, string permitNumber = "12345", string valueFrom = "1")
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_OH_PermitHolder = org.PK;
			guaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			guaranteeHeader.CPH_Number = permitNumber;
			guaranteeHeader.CPH_StartDate = ZDate.Today.AddYears(-20);
			guaranteeHeader.CPH_EndDate = useEndDate ? ZDate.Today.AddYears(20) : ZDate.Empty;
			guaranteeHeader.CPH_QtyValIndicator = permitIndicator;
			guaranteeHeader.CPH_UnitOfMeasure = "KGM";
			guaranteeHeader.CPH_Type = "9001";
			guaranteeHeader.CPH_Type = GuaranteeTypeList.Codes.AI2;
			Factory.Save();

			var rule = guaranteeHeader.CusGuaranteeRules.AddNew();
			rule.CPR_RuleCode = "ADD";
			rule.CPR_ValueFrom = valueFrom;

			var openingBalanceTransaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();

			if (permitIndicator == PermitQtyValIndicatorList.Codes.BTH || permitIndicator == PermitQtyValIndicatorList.Codes.QTY)
			{
				openingBalanceTransaction.CPL_TranQty = 1000;
			}

			if (permitIndicator == PermitQtyValIndicatorList.Codes.BTH || permitIndicator == PermitQtyValIndicatorList.Codes.VAL)
			{
				openingBalanceTransaction.CPL_TranValue = 10000;
			}
			openingBalanceTransaction.CPL_Reference = "Opening Balance";
			openingBalanceTransaction.CPL_TransactionDate = ZDateTime.Today;
			openingBalanceTransaction.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			openingBalanceTransaction.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.VAL;
			openingBalanceTransaction.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Confirmed;
			openingBalanceTransaction.CPL_IsAggregated = false;

			Factory.Save();

			return guaranteeHeader;
		}

		void SetupTransaction(CusGuaranteeHeader guaranteeHeader, ZString reference, ZString messageNum, string status = PermitTransactionStatusList.Codes.Pending)
		{
			var guaranteeLineTransaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			guaranteeLineTransaction.CPL_TranQty = 0;
			guaranteeLineTransaction.CPL_TranValue = -100;
			guaranteeLineTransaction.CPL_Reference = reference;
			guaranteeLineTransaction.CPL_TransactionDate = ZDateTime.Today;
			guaranteeLineTransaction.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;
			guaranteeLineTransaction.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.CUM;
			guaranteeLineTransaction.CPL_AppId = messageNum;
			guaranteeLineTransaction.CPL_TransactionStatus = status;

			Factory.Save();
		}

		void AssertProcessorBehaviourAgainstLiquidation(CusEntryHeader entryHeader, CusEntryLine entryLine1, CusEntryLine entryLine2, DeltaCImportFREDIMessage message)
		{
			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("There should be 1 entry header charges", 1, entryHeader.Charges.Count);
				AssertEquals("There should be 4 entry header confirmed charges", 4, entryHeader.ConfirmedCharges.Count);
				var confirmedCharges = entryHeader.ConfirmedCharges.Cast<Declaration.CusEntryHeaderCharges>().ToArray();
				AssertConfirmedEntryHeaderFee("Charges1", confirmedCharges[0], UniversalReferenceConstants.RefCusRateCodes.P635, 500m, "1");
				AssertConfirmedEntryHeaderFee("Charges2", confirmedCharges[1], UniversalReferenceConstants.RefCusRateCodes.V905, 1000m, "2");
				AssertConfirmedEntryHeaderFee("Charges3", confirmedCharges[2], "P002", 100m, "3");
				AssertConfirmedEntryHeaderFee("Charges4", confirmedCharges[3], "P001", 50m, "3");

				AssertEquals("There should be 1 fees against the first entry line.", 1, entryLine1.Fees.Count);
				AssertEquals("There should be 5 ConfirmedFees against the first entry line.", 5, entryLine1.ConfirmedFees.Count);
				AssertConfirmedEntryLineFee("entryLine1_Fee1", entryLine1.ConfirmedFees[0], "A445", Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage, 26m, 20m, 5m, "6", "");
				AssertConfirmedEntryLineFee("entryLine1_Fee2", entryLine1.ConfirmedFees[1], "G065", "TNE1", 50m, 6.1m, 305m, "1", "");
				AssertConfirmedEntryLineFee("entryLine1_Fee3: Because the content of <typtax> is 0, and because it is not based on goods value, the method of calculation should reflect the supplementary units of all invoice lines bound to the entry line. In case of calculation based on supplementary units, base value should be the sum of supplementary quantities of all invoice lines of the entry line.",
					entryLine1.ConfirmedFees[2], "A325", "DTNG", 500m, 1.2m, 600m, "1", "");
				AssertConfirmedEntryLineFee("entryLine1_Fee4: Because the content of <typtax> is 1, the method of calculation should reflect the third units of all invoice lines bound to the entry line. In case of calculation based on third units, base value should be the sum of third quantities of all invoice lines of the entry line.",
					entryLine1.ConfirmedFees[3], "V906", "HLT", 200m, 3.1m, 620m, "1", "");
				AssertConfirmedEntryLineFee("entryLine1_Fee5", entryLine1.ConfirmedFees[4], UniversalReferenceConstants.RefCusRateCodes.U425, Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage, 0m, 0m, 212m, "2", "");

				AssertEquals("There should be 1 fee against the second entry line.", 1, entryLine2.ConfirmedFees.Count);
				AssertConfirmedEntryLineFee("entryLine2", entryLine2.ConfirmedFees[0], UniversalReferenceConstants.RefCusRateCodes.U165, Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage, 16323m, 2.7m, 441m, "1", "");
				AssertEquals(50m, entryLine1.CL_ConfirmedStatisticalValue);
				AssertEquals(61m, entryLine1.CL_ConfirmedCustomsValue);
				AssertEquals(71m, entryLine1.CL_ConfirmedValueForVAT);
				AssertEquals(0m, entryLine2.CL_ConfirmedStatisticalValue);
				AssertEquals(0m, entryLine2.CL_ConfirmedCustomsValue);
				AssertEquals(0m, entryLine2.CL_ConfirmedValueForVAT);
				AssertEquals(2183m, entryHeader.CH_TotalPaid);

				var permitQuery = new ZDBOnlySubQuery(typeof(BaseCusPermitHeader), CusPermitLineTransactionSchema.CPL_CPH_PermitHeader);
				permitQuery.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, Core.Constants.CountryCodes.France);

				var query = new ZDBOnlyQuery(typeof(BaseCusPermitLineTransaction));
				query.AddToFilter(CusPermitLineTransactionSchema.CPL_Reference, entryHeader.CH_BGMReference);
				query.AddSubQuery(permitQuery, JoinCondition.And);

				var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
				AssertEquals(1, transactions.Length);
				AssertEquals(PermitTransactionStatusList.Codes.Pending, transactions[0].CPL_TransactionStatus);
				AssertEquals(-150m, transactions[0].CPL_TranValue);
			});
		}

		void AssertConfirmedEntryHeaderFee(ZString message, Declaration.CusEntryHeaderCharges confirmedFee, ZString typeCode, ZDecimal chargeAmount, ZString methodOfPayment)
		{
			AssertEquals(message + ":Charge type should reflect the content of <codtax>", typeCode, confirmedFee.C1_ChargeType);
			AssertEquals(message + ":Charge amount should reflect the content of <mtttax>", chargeAmount, confirmedFee.C1_ChargeAmount);
			AssertEquals(message + ":Method of Payment should reflect the content of <statutLiquidation>", methodOfPayment, confirmedFee.C1_MethodOfPayment);
		}

		void AssertConfirmedEntryLineFee(ZString message, Declaration.CusEntryLineFee confirmedFee, ZString typeCode, ZString methodOfCalculation, ZDecimal baseValue, ZDecimal rate, ZDecimal chargeAmount, ZString methodOfPayment, ZString rateOverride)
		{
			AssertEquals(message + ":Charge type should be the EU code", FeeTypeCodeConverter.GetEUFeeTypeCode(typeCode), confirmedFee.CF_ChargeType);
			AssertEquals(message + ":Method Of Calculation should be as the taxation is based on the value", methodOfCalculation, confirmedFee.CF_MethodOfCalculation);
			AssertEquals(message + ":Base value should reflect the content of <asstax> when the taxation is based on value", baseValue, confirmedFee.CF_BaseValue);
			AssertEquals(message + ":Rate should reflect the content of <quotax>", rate, confirmedFee.CF_Rate);
			AssertEquals(message + ":Charge amount should reflect the content of <mtttax>", chargeAmount, confirmedFee.CF_ChargeAmount);
			AssertEquals(message + ":Method of Payment should reflect the content of <statutLiquidation>", methodOfPayment, confirmedFee.CF_MethodOfPayment);
			AssertEquals(message + ":NationalFeeTypeCode should reflect the content of <codtax>", typeCode, confirmedFee.NationalFeeTypeCode);
			AssertEquals(message + ":Action should be empty", rateOverride, confirmedFee.G4_RateOverride);
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
