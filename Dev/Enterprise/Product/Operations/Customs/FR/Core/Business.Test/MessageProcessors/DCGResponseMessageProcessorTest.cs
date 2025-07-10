using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.CusStatement;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class DCGResponseMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcessMessageWhenStatementAlreadyExists()
		{
			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustoms.xml");

			var message = SetupMessage(importMessageText);

			var existingStatementHeader = Factory.New<CusStatementHeader>();
			existingStatementHeader.B2_GC = message.Company.PK;
			existingStatementHeader.B2_StatementType = StatementPeriodicityList.Codes.Day;
			var statementChargesDetail = existingStatementHeader.ChargesDetail;
			statementChargesDetail.B3_BrokerReference = "DCG_REFERENCE";

			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new DCGResponseMessageProcessor(logger);
			processor.ProcessMessage(message);

			Factory.Save();

			AssertEquals("Processor should load and update the existing statement header instead of creating a new one.", StatementStatusList.Codes.Complete, existingStatementHeader.B2_Status);
			AssertEquals("Processor should load the and link the message to existing statement header instead of creating a new one.", existingStatementHeader, message.EM_LinkedObject);
		}

		public void TestProcessMessageDCG_ResponseWithErrorsWhenNoMatchingStatementExists()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			dec1.JE_DeclarationReference = "0738/19";
			var entry1 = dec1.CustomsEntryHeaders.AddNew();
			var entryNumber1 = CusEntryNumber.New(entry1, EU.Business.MessageTypeList.Codes.Import, Core.Constants.CountryCodes.France);
			entryNumber1.CE_EntryNum = "1906142283";

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_DeclarationReference = "0892/19";
			var entry2 = dec2.CustomsEntryHeaders.AddNew();
			var entryNumber2 = CusEntryNumber.New(entry2, EU.Business.MessageTypeList.Codes.Import, Core.Constants.CountryCodes.France);
			entryNumber2.CE_EntryNum = "1906135971";

			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_DeclarationReference = "0736/19";
			var entry3 = dec3.CustomsEntryHeaders.AddNew();
			var entryNumber3 = CusEntryNumber.New(entry3, EU.Business.MessageTypeList.Codes.Import, Core.Constants.CountryCodes.France);
			entryNumber3.CE_EntryNum = "1906117547";

			var dec4 = Factory.New<JobDeclaration>();
			dec4.JE_DeclarationReference = "0809/19";
			var entry4 = dec4.CustomsEntryHeaders.AddNew();
			var entryNumber4 = CusEntryNumber.New(entry4, EU.Business.MessageTypeList.Codes.Import, Core.Constants.CountryCodes.France);
			entryNumber4.CE_EntryNum = "1906100402";

			var dec5 = Factory.New<JobDeclaration>();
			dec5.JE_DeclarationReference = "0961/19";
			var entry5 = dec5.CustomsEntryHeaders.AddNew();
			var entryNumber5 = CusEntryNumber.New(entry5, EU.Business.MessageTypeList.Codes.Import, Core.Constants.CountryCodes.France);
			entryNumber5.CE_EntryNum = "1906096292";

			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustomsWithErrors.xml");

			var message = SetupMessage(importMessageText);

			var group = SetUpStaffAndGroup();

			Factory.Save();

			FRCustomsDataRegistry.Instance.DCGResponseNotificationGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Enterprise.Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, false));
			var logger = new LoggingInformation();
			var processor = new DCGResponseMessageProcessor(logger);

			processor.ProcessMessage(message);
			Factory.Save();

			var statementsHeaders = Factory.Load<CusStatementHeader>(new ZQuery());
			AssertEquals("No statement header should have been loaded or created.", 0, statementsHeaders.Length);

			var mails = Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals(1, mails.Count);
			AssertEquals("New DCG response received. Reference: DCG_REFERENCE Entry Number: 19014110", mails[0].Subject);
			AssertEquals(@"A DCG response has been received. Liquidation statement does not exist.

Following errors were reported by Customs:
- DELTA_FONCTL/Erreur 1
- DELTA_FONCTL/Erreur 2", mails[0].Body);

			AssertEquals("Message status should have been set to ProcessedOK.", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
		}

		public void TestProcessMessageDCG_ResponseWithAnomaliesWhenNoMatchingStatementExists()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			dec1.JE_DeclarationReference = "0738/19";
			var entry1 = dec1.CustomsEntryHeaders.AddNew();
			var entryNumber1 = CusEntryNumber.New(entry1, EU.Business.MessageTypeList.Codes.Import, Core.Constants.CountryCodes.France);
			entryNumber1.CE_EntryNum = "1906142283";

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_DeclarationReference = "0892/19";
			var entry2 = dec2.CustomsEntryHeaders.AddNew();
			var entryNumber2 = CusEntryNumber.New(entry2, EU.Business.MessageTypeList.Codes.Import, Core.Constants.CountryCodes.France);
			entryNumber2.CE_EntryNum = "1906135971";

			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_DeclarationReference = "0736/19";
			var entry3 = dec3.CustomsEntryHeaders.AddNew();
			var entryNumber3 = CusEntryNumber.New(entry3, EU.Business.MessageTypeList.Codes.Import, Core.Constants.CountryCodes.France);
			entryNumber3.CE_EntryNum = "1906117547";

			var dec4 = Factory.New<JobDeclaration>();
			dec4.JE_DeclarationReference = "0809/19";
			var entry4 = dec4.CustomsEntryHeaders.AddNew();
			var entryNumber4 = CusEntryNumber.New(entry4, EU.Business.MessageTypeList.Codes.Import, Core.Constants.CountryCodes.France);
			entryNumber4.CE_EntryNum = "1906100402";

			var dec5 = Factory.New<JobDeclaration>();
			dec5.JE_DeclarationReference = "0961/19";
			var entry5 = dec5.CustomsEntryHeaders.AddNew();
			var entryNumber5 = CusEntryNumber.New(entry5, EU.Business.MessageTypeList.Codes.Import, Core.Constants.CountryCodes.France);
			entryNumber5.CE_EntryNum = "1906096292";

			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustomsWithAnomalies.xml");

			var message = SetupMessage(importMessageText);

			var group = SetUpStaffAndGroup();

			Factory.Save();

			FRCustomsDataRegistry.Instance.DCGResponseNotificationGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Enterprise.Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, false));
			var logger = new LoggingInformation();
			var processor = new DCGResponseMessageProcessor(logger);

			processor.ProcessMessage(message);
			Factory.Save();

			var statementsHeaders = Factory.Load<CusStatementHeader>(new ZQuery());
			AssertEquals("No statement header should have been loaded or created.", 0, statementsHeaders.Length);

			var mails = Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals(1, mails.Count);
			AssertEquals("New DCG response received. Reference: DCG_REFERENCE Entry Number: 19014110", mails[0].Subject);
			AssertEquals(@"A DCG response has been received. Liquidation statement does not exist.

Following anomalies were reported by Customs:
- DELTA_FONCTL/Anomalie 1 (1906096292)
- DELTA_FONCTL/Anomalie 2 (1906100402)", mails[0].Body);

			AssertEquals("Message status should have been set to ProcessedOK.", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
		}

		public void TestProcessMessageDCG_ResponseWithAnomaliesWhenMatchingStatementExists()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var orgCusAccount = Factory.New<OrgCusAccount>();
			orgCusAccount.CZ_OH = importer.PK;
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGI;
			orgCusAccount.CZ_Account = "TESTACC";
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G2;
			orgCusAccount.CZ_ReportingPeriod = ReportingPeriodList.Codes.MON;

			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			dec1.JE_OH_Importer = importer.PK;
			dec1.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			dec1.JE_CustomsProfile = "TESTACC";
			dec1.JE_DeclarationReference = "0738/19";
			var entry1 = dec1.CustomsEntryHeaders.AddNew();
			var entryNumber1 = CusEntryNumber.New(entry1, EU.Business.MessageTypeList.Codes.Import, Core.Constants.CountryCodes.France);
			entryNumber1.CE_EntryNum = "1906142283";

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_DeclarationReference = "0892/19";
			var entry2 = dec2.CustomsEntryHeaders.AddNew();
			var entryNumber2 = CusEntryNumber.New(entry2, EU.Business.MessageTypeList.Codes.Import, Core.Constants.CountryCodes.France);
			entryNumber2.CE_EntryNum = "1906135971";

			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_DeclarationReference = "0736/19";
			var entry3 = dec3.CustomsEntryHeaders.AddNew();
			var entryNumber3 = CusEntryNumber.New(entry3, EU.Business.MessageTypeList.Codes.Import, Core.Constants.CountryCodes.France);
			entryNumber3.CE_EntryNum = "1906117547";

			var dec4 = Factory.New<JobDeclaration>();
			dec4.JE_DeclarationReference = "0809/19";
			var entry4 = dec4.CustomsEntryHeaders.AddNew();
			var entryNumber4 = CusEntryNumber.New(entry4, EU.Business.MessageTypeList.Codes.Import, Core.Constants.CountryCodes.France);
			entryNumber4.CE_EntryNum = "1906100402";

			var dec5 = Factory.New<JobDeclaration>();
			dec5.JE_DeclarationReference = "0961/19";
			var entry5 = dec5.CustomsEntryHeaders.AddNew();
			var entryNumber5 = CusEntryNumber.New(entry5, EU.Business.MessageTypeList.Codes.Import, Core.Constants.CountryCodes.France);
			entryNumber5.CE_EntryNum = "1906096292";

			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustomsWithAnomalies.xml");

			var message = SetupMessage(importMessageText);

			var group = SetUpStaffAndGroup();

			SetUpExistingStatement(message);

			Factory.Save();

			FRCustomsDataRegistry.Instance.DCGResponseNotificationGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Enterprise.Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, false));
			var logger = new LoggingInformation();
			var processor = new DCGResponseMessageProcessor(logger);

			processor.ProcessMessage(message);
			Factory.Save();

			var statementsHeaders = Factory.Load<CusStatementHeader>(new ZQuery());
			AssertEquals("A statement header should have been loaded.", 1, statementsHeaders.Length);

			var statementHeader = statementsHeaders[0];
			AssertEquals("Loaded statement header should belong to message company.", message.Company.PK, statementHeader.B2_GC);
			AssertEquals("Loaded statement header status should have been set to Incomplete because anomalies were reported by Customs.", StatementStatusList.Codes.Incomplete, statementHeader.B2_Status);
			AssertEquals("Loaded statement header entry number should match response refdec.", "19014110", statementHeader.EntryNumber);
			AssertEquals("Loaded statement header type (frequency) should remain unchanged when receiving response with errors.", StatementPeriodicityList.Codes.Day, statementHeader.B2_StatementType);
			AssertEquals("Loaded statement header branch designation should remain unchanged when receiving response with anomalies.", ZString.Empty, statementHeader.B2_BranchDesignation);
			AssertEquals("Loaded statement header statement type should remain unchanged when receiving response with anomalies.", StatementPeriodicityList.Codes.Day, statementHeader.B2_StatementType);
			AssertEquals("Loaded statement header period start date should remain unchanged when receiving response with anomalies.", ZString.Empty, statementHeader.B2_PeriodStartDate.ToString());
			AssertEquals("Loaded statement header period end date should remain unchanged when receiving response with anomalies.", ZString.Empty, statementHeader.B2_PeriodEndDate.ToString());

			var chargesDetail = statementsHeaders[0].ChargesDetail;
			AssertNotNull("Loaded statement header should feature a charges detail.", chargesDetail);
			AssertEquals("Loaded statement header charges detail should have its broker reference set to message DCGReference ('DCG_REFERENCE').", "DCG_REFERENCE", chargesDetail.B3_BrokerReference);

			var charges = chargesDetail.Charges;
			AssertEquals("No statement line charge should have been created.", 0, charges.Count);

			var statementEntries = statementHeader.Entries;
			AssertEquals("No statement entry should have been created.", 0, statementEntries.Count);
			AssertEquals("1st reported entry status should have remained unchanged.", ZString.Empty, entry1.CH_EntryStatus);
			AssertEquals("2nd reported entry status should have remained unchanged.", ZString.Empty, entry2.CH_EntryStatus);
			AssertEquals("3rd reported entry status should have remained unchanged.", ZString.Empty, entry3.CH_EntryStatus);
			AssertEquals("4th reported entry status should have remained unchanged.", ZString.Empty, entry4.CH_EntryStatus);
			AssertEquals("5th reported entry status should have remained unchanged.", ZString.Empty, entry5.CH_EntryStatus);

			var mails = Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals(1, mails.Count);
			AssertEquals("New DCG response received. Reference: DCG_REFERENCE Entry Number: 19014110", mails[0].Subject);
			AssertEquals(@"A DCG response has been received. Liquidation statement status changed to Incomplete.

Following anomalies were reported by Customs:
- DELTA_FONCTL/Anomalie 1 (1906096292)
- DELTA_FONCTL/Anomalie 2 (1906100402)", mails[0].Body);

			AssertEquals("Message status should have been set to ProcessedOK.", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
		}

		public void TestProcessMessageDCG_ResponseWithErrorsWhenMatchingStatementExists()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var orgCusAccount = Factory.New<OrgCusAccount>();
			orgCusAccount.CZ_OH = importer.PK;
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGI;
			orgCusAccount.CZ_Account = "TESTACC";
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G2;
			orgCusAccount.CZ_ReportingPeriod = ReportingPeriodList.Codes.MON;

			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			dec1.JE_OH_Importer = importer.PK;
			dec1.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			dec1.JE_CustomsProfile = "TESTACC";
			dec1.JE_DeclarationReference = "0738/19";
			var entry1 = dec1.CustomsEntryHeaders.AddNew();
			var entryNumber1 = CusEntryNumber.New(entry1, EU.Business.MessageTypeList.Codes.Import, Core.Constants.CountryCodes.France);
			entryNumber1.CE_EntryNum = "1906142283";

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_DeclarationReference = "0892/19";
			var entry2 = dec2.CustomsEntryHeaders.AddNew();
			var entryNumber2 = CusEntryNumber.New(entry2, EU.Business.MessageTypeList.Codes.Import, Core.Constants.CountryCodes.France);
			entryNumber2.CE_EntryNum = "1906135971";

			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_DeclarationReference = "0736/19";
			var entry3 = dec3.CustomsEntryHeaders.AddNew();
			var entryNumber3 = CusEntryNumber.New(entry3, EU.Business.MessageTypeList.Codes.Import, Core.Constants.CountryCodes.France);
			entryNumber3.CE_EntryNum = "1906117547";

			var dec4 = Factory.New<JobDeclaration>();
			dec4.JE_DeclarationReference = "0809/19";
			var entry4 = dec4.CustomsEntryHeaders.AddNew();
			var entryNumber4 = CusEntryNumber.New(entry4, EU.Business.MessageTypeList.Codes.Import, Core.Constants.CountryCodes.France);
			entryNumber4.CE_EntryNum = "1906100402";

			var dec5 = Factory.New<JobDeclaration>();
			dec5.JE_DeclarationReference = "0961/19";
			var entry5 = dec5.CustomsEntryHeaders.AddNew();
			var entryNumber5 = CusEntryNumber.New(entry5, EU.Business.MessageTypeList.Codes.Import, Core.Constants.CountryCodes.France);
			entryNumber5.CE_EntryNum = "1906096292";

			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustomsWithErrors.xml");

			var message = SetupMessage(importMessageText);

			var group = SetUpStaffAndGroup();

			SetUpExistingStatement(message);

			Factory.Save();

			FRCustomsDataRegistry.Instance.DCGResponseNotificationGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Enterprise.Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, false));
			var logger = new LoggingInformation();
			var processor = new DCGResponseMessageProcessor(logger);

			processor.ProcessMessage(message);
			Factory.Save();

			var statementsHeaders = Factory.Load<CusStatementHeader>(new ZQuery());
			AssertEquals("1 statement header should have been created.", 1, statementsHeaders.Length);

			var statementHeader = statementsHeaders[0];
			AssertEquals("Loaded statement header should belong to message company.", message.Company.PK, statementHeader.B2_GC);
			AssertEquals("Loaded statement header status should have been set to Incomplete because errors were reported by Customs.", StatementStatusList.Codes.Incomplete, statementHeader.B2_Status);
			AssertEquals("Loaded statement header entry number should match response refdec.", "19014110", statementHeader.EntryNumber);
			AssertEquals("Loaded statement header type should remain unchanged when receiving response with errors.", StatementPeriodicityList.Codes.Day, statementHeader.B2_StatementType);
			AssertEquals("Loaded statement header branch designation should remain unchanged when receiving response with errors.", ZString.Empty, statementHeader.B2_BranchDesignation);
			AssertEquals("Loaded statement header statement type should remain unchanged when receiving response with errors.", StatementPeriodicityList.Codes.Day, statementHeader.B2_StatementType);
			AssertEquals("Loaded statement header period start date should remain unchanged when receiving response with errors.", ZString.Empty, statementHeader.B2_PeriodStartDate.ToString());
			AssertEquals("Loaded statement header period end date should remain unchanged when receiving response with errors.", ZString.Empty, statementHeader.B2_PeriodEndDate.ToString());

			var chargesDetail = statementsHeaders[0].ChargesDetail;
			AssertNotNull("Created statement header should feature a charges detail.", chargesDetail);
			AssertEquals("Statement header charges detail should have its broker reference set to message DCGReference ('DCG_REFERENCE').", "DCG_REFERENCE", chargesDetail.B3_BrokerReference);

			var charges = chargesDetail.Charges;
			AssertEquals("No statement line charge should have been created.", 0, charges.Count);

			var statementEntries = statementHeader.Entries;
			AssertEquals("No statement entry should have been created.", 0, statementEntries.Count);
			AssertEquals("1st reported entry status should have remained unchanged.", ZString.Empty, entry1.CH_EntryStatus);
			AssertEquals("2nd reported entry status should have remained unchanged.", ZString.Empty, entry2.CH_EntryStatus);
			AssertEquals("3rd reported entry status should have remained unchanged.", ZString.Empty, entry3.CH_EntryStatus);
			AssertEquals("4th reported entry status should have remained unchanged.", ZString.Empty, entry4.CH_EntryStatus);
			AssertEquals("5th reported entry status should have remained unchanged.", ZString.Empty, entry5.CH_EntryStatus);

			var mails = Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals(1, mails.Count);
			AssertEquals("New DCG response received. Reference: DCG_REFERENCE Entry Number: 19014110", mails[0].Subject);
			AssertEquals(@"A DCG response has been received. Liquidation statement status changed to Incomplete.

Following errors were reported by Customs:
- DELTA_FONCTL/Erreur 1
- DELTA_FONCTL/Erreur 2", mails[0].Body);

			AssertEquals("Message status should have been set to ProcessedOK.", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
		}

		public void TestProcessMessageDCG_Response()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var orgCusAccount = Factory.New<OrgCusAccount>();
			orgCusAccount.CZ_OH = importer.PK;
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGI;
			orgCusAccount.CZ_Account = "TESTACC";
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G2;
			orgCusAccount.CZ_ReportingPeriod = ReportingPeriodList.Codes.MON;

			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			dec1.JE_OH_Importer = importer.PK;
			dec1.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			dec1.JE_CustomsProfile = "TESTACC";
			dec1.JE_DeclarationReference = "0738/19";
			var entry1 = dec1.CustomsEntryHeaders.AddNew();
			var entryNumber1 = CusEntryNumber.New(entry1, EU.Business.MessageTypeList.Codes.Import, Core.Constants.CountryCodes.France);
			entryNumber1.CE_EntryNum = "1906142283";

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_DeclarationReference = "0892/19";
			var entry2 = dec2.CustomsEntryHeaders.AddNew();
			var entryNumber2 = CusEntryNumber.New(entry2, EU.Business.MessageTypeList.Codes.Import, Core.Constants.CountryCodes.France);
			entryNumber2.CE_EntryNum = "1906135971";

			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_DeclarationReference = "0736/19";
			var entry3 = dec3.CustomsEntryHeaders.AddNew();
			var entryNumber3 = CusEntryNumber.New(entry3, EU.Business.MessageTypeList.Codes.Import, Core.Constants.CountryCodes.France);
			entryNumber3.CE_EntryNum = "1906117547";

			var dec4 = Factory.New<JobDeclaration>();
			dec4.JE_DeclarationReference = "0809/19";
			var entry4 = dec4.CustomsEntryHeaders.AddNew();
			var entryNumber4 = CusEntryNumber.New(entry4, EU.Business.MessageTypeList.Codes.Import, Core.Constants.CountryCodes.France);
			entryNumber4.CE_EntryNum = "1906100402";

			var dec5 = Factory.New<JobDeclaration>();
			dec5.JE_DeclarationReference = "0961/19";
			var entry5 = dec5.CustomsEntryHeaders.AddNew();
			var entryNumber5 = CusEntryNumber.New(entry5, EU.Business.MessageTypeList.Codes.Import, Core.Constants.CountryCodes.France);
			entryNumber5.CE_EntryNum = "1906096292";

			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustoms.xml");

			var message = SetupMessage(importMessageText);

			var group = SetUpStaffAndGroup();
			Factory.Save();

			FRCustomsDataRegistry.Instance.DCGResponseNotificationGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Enterprise.Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, false));
			var logger = new LoggingInformation();
			var processor = new DCGResponseMessageProcessor(logger);

			processor.ProcessMessage(message);
			Factory.Save();

			var statementsHeaders = Factory.Load<CusStatementHeader>(new ZQuery());
			AssertEquals("1 statement header should have been created.", 1, statementsHeaders.Length);

			var statementHeader = statementsHeaders[0];
			AssertEquals("Created statement header should belong to message company.", message.Company.PK, statementHeader.B2_GC);
			AssertEquals("Created statement header entry number should match response refdec.", "19014110", statementHeader.EntryNumber);
			AssertEquals("Created statement header type (frequency) should be inferred from authorisation.", StatementPeriodicityList.Codes.Month, statementHeader.B2_StatementType);
			AssertEquals("Created statement header status should have been set to Complete because no error or anomalies were reported by Customs.", StatementStatusList.Codes.Complete, statementHeader.B2_Status);
			AssertEquals("Created statement header branch designation should have been inferred from first entry", StatementEntryTypeImpExpList.Codes.Import, statementHeader.B2_BranchDesignation);
			AssertEquals("Created statement header period start date should have been inferred from message", "01-Jul-19", statementHeader.B2_PeriodStartDate.ToString());
			AssertEquals("Created statement header period end date should have been inferred from message", "31-Jul-19", statementHeader.B2_PeriodEndDate.ToString());

			var chargesDetail = statementsHeaders[0].ChargesDetail;
			AssertNotNull("Created statement header should feature a charges detail.", chargesDetail);
			AssertEquals("Statement header charges detail should have its broker reference set to message DCGReference ('DCG_REFERENCE').", "DCG_REFERENCE", chargesDetail.B3_BrokerReference);

			var charges = chargesDetail.Charges;
			AssertEquals("There should have been 3 statement line charge created.", 3, charges.Count);
			AssertEquals("1st charge type should match reported charge type (codtax).", UniversalReferenceConstants.RefCusRateCodes.A445, charges[0].B4_ChargeType);
			AssertEquals("1st charge amount should match reported charge type (montanttax).", 101079m, charges[0].B4_ChargeAmount);
			AssertEquals("1st charge group should match reported charge EU code (codtaxeeu).", UniversalReferenceConstants.RefCusRateCodes.B00, charges[0].B4_ChargeGroup);
			AssertEquals("1st charge method of payment should match reported method of payment (statutliquidation).", "1", charges[0].B4_MethodOfPayment);
			AssertEquals("2nd charge type should match reported charge type (codtax).", UniversalReferenceConstants.RefCusRateCodes.U165, charges[1].B4_ChargeType);
			AssertEquals("2nd charge amount should match reported charge type (montanttax).", 160254m, charges[1].B4_ChargeAmount);
			AssertEquals("2nd charge group should match reported charge EU code (codtaxeeu).", UniversalReferenceConstants.RefCusRateCodes.A00, charges[1].B4_ChargeGroup);
			AssertEquals("2nd charge method of payment should match reported method of payment (statutliquidation).", "1", charges[1].B4_MethodOfPayment);
			AssertEquals("3rd charge type should match reported charge type (codtax).", UniversalReferenceConstants.RefCusRateCodes.A445, charges[2].B4_ChargeType);
			AssertEquals("3rd charge amount should match reported charge type (montanttax).", 597310m, charges[2].B4_ChargeAmount);
			AssertEquals("3rd charge group should match reported charge EU code (codtaxeeu).", UniversalReferenceConstants.RefCusRateCodes.B00, charges[2].B4_ChargeGroup);
			AssertEquals("3rd charge method of payment should match reported method of payment (statutliquidation).", "3", charges[2].B4_MethodOfPayment);

			var statementEntries = statementHeader.Entries;
			AssertEquals("There should have been 5 statement entries created.", 5, statementEntries.Count);
			AssertEquals("Statement entry 1 EntryNum should match reported entry number (refdec).", "1906142283", statementEntries[0].B3_EntryNum);
			AssertEquals("Statement entry 1 Broker Reference should match reported declaration number (refdos).", "0738/19", statementEntries[0].B3_BrokerReference);
			AssertEquals("Statement entry 2 EntryNum should match reported entry number (refdec).", "1906135971", statementEntries[1].B3_EntryNum);
			AssertEquals("Statement entry 2 Broker Reference should match reported declaration number (refdos).", "0892/19", statementEntries[1].B3_BrokerReference);
			AssertEquals("Statement entry 3 EntryNum should match reported entry number (refdec).", "1906117547", statementEntries[2].B3_EntryNum);
			AssertEquals("Statement entry 3 Broker Reference should match reported declaration number (refdos).", "0736/19", statementEntries[2].B3_BrokerReference);
			AssertEquals("Statement entry 4 EntryNum should match reported entry number (refdec).", "1906100402", statementEntries[3].B3_EntryNum);
			AssertEquals("Statement entry 4 Broker Reference should match reported declaration number (refdos).", "0809/19", statementEntries[3].B3_BrokerReference);
			AssertEquals("Statement entry 5 EntryNum should match reported entry number (refdec).", "1906096292", statementEntries[4].B3_EntryNum);
			AssertEquals("Statement entry 5 Broker Reference should match reported declaration number (refdos).", "0961/19", statementEntries[4].B3_BrokerReference);
			AssertEquals("1st reported entry should have its status set to 140.", EntryStatusDescriptionCodeList.Codes.ES140, entry1.CH_EntryStatus);
			AssertEquals("2nd reported entry should have its status set to 140.", EntryStatusDescriptionCodeList.Codes.ES140, entry2.CH_EntryStatus);
			AssertEquals("3rd reported entry should have its status set to 140.", EntryStatusDescriptionCodeList.Codes.ES140, entry3.CH_EntryStatus);
			AssertEquals("4th reported entry should have its status set to 140.", EntryStatusDescriptionCodeList.Codes.ES140, entry4.CH_EntryStatus);
			AssertEquals("5th reported entry should have its status set to 140.", EntryStatusDescriptionCodeList.Codes.ES140, entry5.CH_EntryStatus);

			var storageMain = (statementsHeaders[0] as IDocManagerSupport).DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(statementsHeaders[0], Core.Constants.DocManagerCodes.CusStatementHeader);
			AssertEquals(1, storageMain.Files.Count);
			AssertEquals("Global Supplementary Statement - 19014110.pdf", storageMain.Files[0].FileName);

			var mails = Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals(1, mails.Count);
			AssertEquals("New DCG response received. Reference: DCG_REFERENCE Entry Number: 19014110", mails[0].Subject);
			AssertEquals(@"A DCG response has been received. Liquidation statement status changed to Complete.

Response lists following taxes:
- A445/B00 : 101079 EUR
- U165/A00 : 160254 EUR
- A445/B00 : 597310 EUR", mails[0].Body);

			AssertEquals("Message status should have been set to ProcessedOK.", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
		}

		public void TestProcessMessageDCG_ResponseWhenNoRelatedDeclarationExists()
		{
			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustoms.xml");

			var message = SetupMessage(importMessageText);

			var group = SetUpStaffAndGroup();
			Factory.Save();

			FRCustomsDataRegistry.Instance.DCGResponseNotificationGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Enterprise.Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, false));
			var logger = new LoggingInformation();
			var processor = new DCGResponseMessageProcessor(logger);

			processor.ProcessMessage(message);
			Factory.Save();

			var statementsHeaders = Factory.Load<CusStatementHeader>(new ZQuery());
			AssertEquals("1 statement header should have been created.", 1, statementsHeaders.Length);

			var statementHeader = statementsHeaders[0];
			AssertEquals("Created statement header should belong to message company.", message.Company.PK, statementHeader.B2_GC);
			AssertEquals("Created statement header entry number should match response refdec.", "19014110", statementHeader.EntryNumber);
			AssertEquals("Created statement header type (frequency) should have been defaulted to Unknown because no autorization was found.", StatementPeriodicityList.Codes.Unknown, statementHeader.B2_StatementType);
			AssertEquals("Created statement header status should have been set to Complete because no error or anomalies were reported by Customs.", StatementStatusList.Codes.Complete, statementHeader.B2_Status);
			AssertEquals("Created statement header branch designation should have been inferred from first entry", StatementEntryTypeImpExpList.Codes.Import, statementHeader.B2_BranchDesignation);
			AssertEquals("Created statement header period start date should have been inferred from message", "01-Jul-19", statementHeader.B2_PeriodStartDate.ToString());
			AssertEquals("Created statement header period end date should have been inferred from message", "31-Jul-19", statementHeader.B2_PeriodEndDate.ToString());

			var chargesDetail = statementsHeaders[0].ChargesDetail;
			AssertNotNull("Created statement header should feature a charges detail.", chargesDetail);
			AssertEquals("Statement header charges detail should have its broker reference set to message DCGReference ('DCG_REFERENCE').", "DCG_REFERENCE", chargesDetail.B3_BrokerReference);

			var charges = chargesDetail.Charges;
			AssertEquals("There should have been 3 statement line charge created.", 3, charges.Count);
			AssertEquals("1st charge type should match reported charge type (codtax).", UniversalReferenceConstants.RefCusRateCodes.A445, charges[0].B4_ChargeType);
			AssertEquals("1st charge amount should match reported charge type (montanttax).", 101079m, charges[0].B4_ChargeAmount);
			AssertEquals("1st charge group should match reported charge EU code (codtaxeeu).", UniversalReferenceConstants.RefCusRateCodes.B00, charges[0].B4_ChargeGroup);
			AssertEquals("1st charge method of payment should match reported method of payment (statutliquidation).", "1", charges[0].B4_MethodOfPayment);
			AssertEquals("2nd charge type should match reported charge type (codtax).", UniversalReferenceConstants.RefCusRateCodes.U165, charges[1].B4_ChargeType);
			AssertEquals("2nd charge amount should match reported charge type (montanttax).", 160254m, charges[1].B4_ChargeAmount);
			AssertEquals("2nd charge group should match reported charge EU code (codtaxeeu).", UniversalReferenceConstants.RefCusRateCodes.A00, charges[1].B4_ChargeGroup);
			AssertEquals("2nd charge method of payment should match reported method of payment (statutliquidation).", "1", charges[1].B4_MethodOfPayment);
			AssertEquals("3rd charge type should match reported charge type (codtax).", UniversalReferenceConstants.RefCusRateCodes.A445, charges[2].B4_ChargeType);
			AssertEquals("3rd charge amount should match reported charge type (montanttax).", 597310m, charges[2].B4_ChargeAmount);
			AssertEquals("3rd charge group should match reported charge EU code (codtaxeeu).", UniversalReferenceConstants.RefCusRateCodes.B00, charges[2].B4_ChargeGroup);
			AssertEquals("3rd charge method of payment should match reported method of payment (statutliquidation).", "3", charges[2].B4_MethodOfPayment);

			var statementEntries = statementHeader.Entries;
			AssertEquals("There should have been 5 statement entries created.", 5, statementEntries.Count);
			AssertEquals("Statement entry 1 EntryNum should match reported entry number (refdec).", "1906142283", statementEntries[0].B3_EntryNum);
			AssertEquals("Statement entry 1 Broker Reference should match reported declaration number (refdos).", "0738/19", statementEntries[0].B3_BrokerReference);
			AssertEquals("Statement entry 2 EntryNum should match reported entry number (refdec).", "1906135971", statementEntries[1].B3_EntryNum);
			AssertEquals("Statement entry 2 Broker Reference should match reported declaration number (refdos).", "0892/19", statementEntries[1].B3_BrokerReference);
			AssertEquals("Statement entry 3 EntryNum should match reported entry number (refdec).", "1906117547", statementEntries[2].B3_EntryNum);
			AssertEquals("Statement entry 3 Broker Reference should match reported declaration number (refdos).", "0736/19", statementEntries[2].B3_BrokerReference);
			AssertEquals("Statement entry 4 EntryNum should match reported entry number (refdec).", "1906100402", statementEntries[3].B3_EntryNum);
			AssertEquals("Statement entry 4 Broker Reference should match reported declaration number (refdos).", "0809/19", statementEntries[3].B3_BrokerReference);
			AssertEquals("Statement entry 5 EntryNum should match reported entry number (refdec).", "1906096292", statementEntries[4].B3_EntryNum);
			AssertEquals("Statement entry 5 Broker Reference should match reported declaration number (refdos).", "0961/19", statementEntries[4].B3_BrokerReference);

			var mails = Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals(1, mails.Count);
			AssertEquals("New DCG response received. Reference: DCG_REFERENCE Entry Number: 19014110", mails[0].Subject);
			AssertEquals(@"A DCG response has been received. Liquidation statement status changed to Complete.

Response lists following taxes:
- A445/B00 : 101079 EUR
- U165/A00 : 160254 EUR
- A445/B00 : 597310 EUR", mails[0].Body);

			AssertEquals("Message status should have been set to ProcessedOK.", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
		}

		GlbGroup SetUpStaffAndGroup()
		{
			var staff = Factory.New<GlbStaff>();
			staff.FillWithValidTestData();
			staff.GS_Code = "~BB";
			staff.GS_EmailAddress = "test@cargowise.com";

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var groupLink = Factory.NewWithValidTestData<GlbGroupLink>();
			groupLink.GK_GG = group.PK;
			groupLink.GK_GS = staff.PK;
			group.Staff.Load();
			return group;
		}

		DCGResponseFREDIMessage SetupMessage(string importMessageText)
		{
			var message = Factory.New<DCGResponseFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.DCG;
			message.EM_MessageSubType = MessageSubTypeList.Codes.DCG;
			message.EM_MessageText = importMessageText;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;

			var messageCompany = Factory.NewWithValidTestData<GlbCompany>();
			var messageBranch = Factory.NewWithValidTestData<GlbBranch>();
			messageBranch.GB_GC = messageCompany.PK;
			message.EM_GB = messageBranch.PK;

			return message;
		}

		CusStatementHeader SetUpExistingStatement(DCGResponseFREDIMessage message)
		{
			var existingStatementHeader = Factory.New<CusStatementHeader>();
			existingStatementHeader.B2_GC = message.Company.PK;
			existingStatementHeader.B2_StatementType = StatementPeriodicityList.Codes.Day;
			var statementChargesDetail = existingStatementHeader.ChargesDetail;
			statementChargesDetail.B3_BrokerReference = "DCG_REFERENCE";

			return existingStatementHeader;
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
