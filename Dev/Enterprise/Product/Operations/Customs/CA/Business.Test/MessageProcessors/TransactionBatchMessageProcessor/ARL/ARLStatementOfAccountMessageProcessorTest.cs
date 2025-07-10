using System;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class ARLStatementOfAccountMessageProcessorTest : TestCaseWithFactoryAndMessagingHelpers
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetKeys()
		{
			var expectedHtml = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\SoAMessageInterpretation.html");
			expectedHtml = expectedHtml.Replace("\r\n", "");

			var ediMessage = ARLStatementOfAccountDocumentWrapperTest.CreateStatementOfAccountMessageAndSetUpTestData(new BusinessObjectFactory());
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			Factory.SaveForTesting();

			var logger = new ServiceTaskLogForTesting();
			var keys = GetKeys(ediMessage, logger);

			AssertEquals(false, keys.ShouldShortCircuit);
			AssertEquals(nameof(ARLStatementOfAccountMessageProcessor), keys.Keys.Single());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessAndPurgeLegacyData()
		{
			var factory = new BusinessObjectFactory();

			var ediMessage = ARLStatementOfAccountDocumentWrapperTest.CreateStatementOfAccountMessageAndSetUpTestData(factory);
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			Factory.SaveForTesting();

			var importer1 = factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "IMPORTER1");

			var importer3 = factory.NewWithValidTestData<OrgHeader>();
			importer3.OH_Code = "IMPORTER3";

			#region Legacy Data

			var statementHeaderForUpdate = factory.New<CusStatementHeader>();
			statementHeaderForUpdate.B2_IsMonthlyStatement = true;
			statementHeaderForUpdate.B2_StatementType = "B";
			statementHeaderForUpdate.B2_StatementNumber = "123546789RM0001102014-001";

			statementHeaderForUpdate.B2_ImporterCustomsID = "123546789RM0001";
			statementHeaderForUpdate.B2_OH_Importer = TransactionBatchExtension.GetOrgsFromBN("123546789RM0001", ZString.Empty, factory, true).FirstOrDefault()?.PK ?? ZGuid.Empty;
			statementHeaderForUpdate.B2_EntryFilerCode = "40077";
			statementHeaderForUpdate.B2_DueDate = new ZDateTime(2014, 01, 05);
			statementHeaderForUpdate.B2_PrintDate = new ZDateTime(2014, 01, 25);
			statementHeaderForUpdate.B2_StatementAmount = 2678m;

			statementHeaderForUpdate.Notes.AddNew(true, "Message to Recipient (English)", "TEST EN");
			statementHeaderForUpdate.Notes.AddNew(true, "Message to Recipient (French)", "TEST FR");

			var groupForUpdate = statementHeaderForUpdate.LineGroupCollection.FindOrCreate("123546789RM0001");
			var groupForDelete = statementHeaderForUpdate.LineGroupCollection.FindOrCreate("123546789RM0003");

			var lineForUpdate = statementHeaderForUpdate.StatementLines.AddNew();
			lineForUpdate.B3_EntryNum = "84100";
			lineForUpdate.B3_EntryType = "NP";

			lineForUpdate.B3_Status = "XOY";
			lineForUpdate.B3_EntryStatus = "sO";
			lineForUpdate.B3_EIIndicator = "DW";
			lineForUpdate.B3_AssociatedEntry = "TEST ENTRY";
			lineForUpdate.B3_ImporterCustomsID = "123546789RM0001";
			lineForUpdate.B3_ScheduledProcessDate = new ZDate(2014, 10, 05);
			lineForUpdate.B3_DueDate = new ZDate(2014, 10, 12);
			lineForUpdate.B3_CreditNote = "TEST NOTE";
			lineForUpdate.B3_CreditNoteDate = new ZDate(2014, 10, 08);
			lineForUpdate.B3_CustomsFeesTotal = 35m;

			var lineForDelete = statementHeaderForUpdate.StatementLines.AddNew();
			lineForDelete.B3_EntryNum = "97824";
			lineForDelete.B3_EntryType = "AI";

			lineForDelete.B3_Status = "OUR";
			lineForDelete.B3_EntryStatus = "SS";
			lineForDelete.B3_EIIndicator = "EEW";
			lineForDelete.B3_AssociatedEntry = "TEST ENTRY";
			lineForDelete.B3_ImporterCustomsID = "123546789RM0001";
			lineForDelete.B3_ScheduledProcessDate = new ZDate(2014, 10, 05);
			lineForDelete.B3_DueDate = new ZDate(2014, 10, 12);
			lineForDelete.B3_CreditNote = "TEST NOTE";
			lineForDelete.B3_CreditNoteDate = new ZDate(2014, 10, 08);
			lineForDelete.B3_CustomsFeesTotal = 35m;

			factory.Save();

			#endregion

			var logger = new ServiceTaskLogForTesting();
			ProcessMessage(ediMessage, logger);
			ediMessage.Factory.Save();

			factory = new BusinessObjectFactory();

			var statementHeader = AssertStatementHeader(factory, 4, "123546789RM0001", "B", "123546789RM0001102014-001", "40077", new ZDateTime(2014, 1, 31), new ZDateTime(2014, 1, 25), 3134.59m);
			AssertEquals("Should find the existing data and update some data.", statementHeader.PK, statementHeaderForUpdate.PK);

			var financialDetailAmounts = new ZDecimal[] { 21.12m, 0m, 0m, 26000m, 0m, 240870.35m, 240620.35m, 0m, 55m, 98100.74m, 250m, 2283144.44m };
			var lineGroup = AssertStatementLineGroup(statementHeader, importer1.PK, "123546789RM0001", financialDetailAmounts);
			AssertEquals("Should find the existing data and update some data.", lineGroup.PK, groupForUpdate.PK);
			AssertNull("Should remove the note as the latest message does not contains any EN Note.", statementHeaderForUpdate.Notes.FindByDescription("Message to Recipient (English)").FirstOrDefault());
			AssertNull("Should remove the note as the latest message does not contains any FR Note.", statementHeaderForUpdate.Notes.FindByDescription("Message to Recipient (French)").FirstOrDefault());

			var line = AssertStatementLine(statementHeader, "84100", "123546789RM0001", "OTH", "SO", "NP", "", "", "", ZDateTime.Empty, ZDateTime.Empty, new ZDateTime(2014, 11, 20), 100m);
			AssertEquals("Should find the existing data and update some data.", line.PK, lineForUpdate.PK);

			Assert("Should be deleted as it's not used.", groupForDelete.IsDeleted);
			Assert("Should be deleted as it's not used.", lineForDelete.IsDeleted);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcess()
		{
			var header1 = Factory.New<CusStatementHeader>();
			header1.B2_StatementType = "B";
			header1.B2_ImporterCustomsID = "123546789RM0001";
			header1.B2_IsMonthlyStatement = false;
			header1.B2_ProcessDate = new ZDateTime(2014, 10, 28);
			header1.B2_B2_PeriodicStatement = ZGuid.Empty;

			var header2 = Factory.New<CusStatementHeader>();
			header2.B2_StatementType = "B";
			header2.B2_ImporterCustomsID = "123546789RM0001";
			header2.B2_IsMonthlyStatement = false;
			header2.B2_ProcessDate = new ZDateTime(2014, 10, 24);
			header2.B2_B2_PeriodicStatement = ZGuid.Empty;

			var header3 = Factory.New<CusStatementHeader>();
			header3.B2_StatementType = "B";
			header3.B2_ImporterCustomsID = "123546789RM0001";
			header3.B2_IsMonthlyStatement = false;
			header3.B2_ProcessDate = new ZDateTime(2014, 10, 28);
			header3.B2_B2_PeriodicStatement = header2.PK;

			var expectedHtml = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\SoAMessageInterpretation.html");
			expectedHtml = expectedHtml.Replace("{VersionNumber}", $"VersionNumber={new EnterpriseInformationRetriever().VersionNumber}").Replace("\r\n", "").Replace("\t", "");

			var ediMessage = ARLStatementOfAccountDocumentWrapperTest.CreateStatementOfAccountMessageAndSetUpTestData(new BusinessObjectFactory());
			var importer1 = ediMessage.Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "IMPORTER1");
			importer1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "123546789RM0001", Core.Constants.CountryCodes.Canada);
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;

			Factory.SaveForTesting();

			var logger = new ServiceTaskLogForTesting();
			ProcessMessage(ediMessage, logger);
			ediMessage.Factory.Save();

			AssertXMLEquals("EM_MessageInterpretation", expectedHtml, ediMessage.EM_MessageInterpretation.Replace("\r\n", ""));

			var factory = new BusinessObjectFactory();

			var arlMessage = factory.Load<ARLMessage>(ediMessage.PK);
			AssertEquals(ARLMessageTypes.Codes.StatementOfAccount, arlMessage.EM_MessageSubType);
			AssertEquals(new ZDateTime(2014, 1, 25), arlMessage.K84StatementDate);
			AssertEquals(new ZDateTime(2014, 1, 31), arlMessage.K84AccountingDate);

			var importer2 = factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "IMPORTER2");

			var cusStatementHeader = AssertStatementHeader(factory, 4, "123546789RM0001", "B", "123546789RM0001102014-001", "40077", new ZDateTime(2014, 1, 31), new ZDateTime(2014, 1, 25), 3134.59m);

			header1.Reload();
			header2.Reload();
			header3.Reload();

			AssertEquals("Should locate all valid daily notice statement headers.", cusStatementHeader.PK, header1.B2_B2_PeriodicStatement);
			AssertEquals("Should not locate this daily notice statement header as it's expired.", ZGuid.Empty, header2.B2_B2_PeriodicStatement);
			AssertEquals("Should not locate this daily notice statement header as it has a parent.", header2.PK, header3.B2_B2_PeriodicStatement);

			var financialDetailAmounts = new ZDecimal[] { 21.12m, 0m, 0m, 26000m, 0m, 240870.35m, 240620.35m, 0m, 55m, 98100.74m, 250m, 2283144.44m };
			AssertStatementLineGroup(cusStatementHeader, importer1.PK, "123546789RM0001", financialDetailAmounts);

			financialDetailAmounts = new ZDecimal[] { 0m, 0m, 0m, 100m, 0m, 0m, 50000m, 0m, 4845.34m, 88595.79m, -50000m, 264500.08m };
			AssertStatementLineGroup(cusStatementHeader, importer2.PK, "123546789RM0002", financialDetailAmounts);

			AssertStatementLine(cusStatementHeader, "84100", "123546789RM0001", "OTH", "SO", "NP", "", "", "", ZDateTime.Empty, ZDateTime.Empty, new ZDateTime(2014, 11, 20), 100m);
			AssertStatementLine(cusStatementHeader, "84100", "123546789RM0002", "OTH", "SD", "AI", "", "", "", ZDateTime.Empty, ZDateTime.Empty, new ZDateTime(2014, 11, 20), 100m);
			AssertStatementLine(cusStatementHeader, "40077000125300", "123546789RM0001", "OUR", "S", "B2", "", "40077000000020", "", ZDateTime.Empty, ZDateTime.Empty, new ZDateTime(2014, 11, 20), 58711.50m);
			AssertStatementLine(cusStatementHeader, "40078000125301", "123546789RM0002", "OUR", "SS", "B2", "", "40078000000021", "", ZDateTime.Empty, ZDateTime.Empty, new ZDateTime(2014, 11, 20), 58711.50m);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessWithoutImporter()
		{
			var ediMessage = ARLStatementOfAccountDocumentWrapperTest.CreateStatementOfAccountMessageAndSetUpTestData(new BusinessObjectFactory());
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;

			var importer1 = ediMessage.Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "IMPORTER1");
			importer1.CustomsCodes.RemoveAndDeleteAll();

			var importer2 = ediMessage.Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "IMPORTER2");
			importer2.CustomsCodes.RemoveAndDeleteAll();

			ediMessage.Factory.Save();

			var logger = new ServiceTaskLogForTesting();
			ProcessMessage(ediMessage, logger);

			ediMessage.Factory.Save();

			var factory = new BusinessObjectFactory();

			var cusStatementHeader = AssertStatementHeader(factory, 4, "123546789RM0001", "B", "123546789RM0001102014-001", "", new ZDateTime(2014, 1, 31), new ZDateTime(2014, 1, 25), 3134.59m);

			var financialDetailAmounts = new ZDecimal[] { 21.12m, 0m, 0m, 26000m, 0m, 240870.35m, 240620.35m, 0m, 55m, 98100.74m, 250m, 2283144.44m };
			AssertStatementLineGroup(cusStatementHeader, ZGuid.Empty, "123546789RM0001", financialDetailAmounts);

			financialDetailAmounts = new ZDecimal[] { 0m, 0m, 0m, 100m, 0m, 0m, 50000m, 0m, 4845.34m, 88595.79m, -50000m, 264500.08m };
			AssertStatementLineGroup(cusStatementHeader, ZGuid.Empty, "123546789RM0002", financialDetailAmounts);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSendNotification()
		{
			var ediMessage = ARLStatementOfAccountDocumentWrapperTest.CreateStatementOfAccountMessageAndSetUpTestData(new BusinessObjectFactory());
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			Factory.SaveForTesting();

			var newGroup1 = Factory.New<GlbGroup>();
			newGroup1.GG_Code = "NG1";
			var newStaff1 = newGroup1.Staff.AddNew();
			newStaff1.GS_Code = "NS1";
			newStaff1.GS_LoginName = "NS1";
			newStaff1.GS_EmailAddress = "ns1@cargowise.com";
			CACustomsDataRegistry.Instance.SendDeclarationMessageAcknowledgementsToGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newGroup1.PK.ToGuid());
			Factory.SaveForTesting();

			var logger = new ServiceTaskLogForTesting();
			ProcessMessage(ediMessage, logger);

			var email = Env.OutgoingMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "Statement Of Account for 25-Jan-14");
			AssertNotNull(email);
			AssertEquals("One recipient", 1, email.CCRecipients.Count);
			AssertEquals(User.PostMasterUserName, "ns1@cargowise.com", email.CCRecipients[0].Email);
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, ediMessage.EM_Status);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			var newGroup2 = Factory.New<GlbGroup>();
			newGroup2.GG_Code = "NG2";
			var newStaff2 = newGroup2.Staff.AddNew();
			newStaff2.GS_Code = "NS2";
			newStaff2.GS_LoginName = "NS2";
			newStaff2.GS_EmailAddress = "ns2@cargowise.com";
			CACustomsDataRegistry.Instance.SendK84ReportNotificationsToGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newGroup2.PK.ToGuid());
			Factory.SaveForTesting();

			ediMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch;
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			ProcessMessage(ediMessage, logger);
			email = Env.OutgoingMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "Statement Of Account for 25-Jan-14");
			AssertNotNull(email);
			AssertEquals("One recipient", 1, email.CCRecipients.Count);
			AssertEquals(User.PostMasterUserName, "ns2@cargowise.com", email.CCRecipients[0].Email);
			AssertContains("email.Body", @"<thead><tr class=""tableheadings""><th>Importer Code</th><th>Importer BN</th><th>Importer Name</th><th>Check Issue Date</th></tr></thead>"
									+ @"<tr align=""center""><td>IMPORTER1</td><td>123546789RM0001</td><td>PIERRE IMPORTERS Inc.<br>(IMPORTER1 NAME)</td><td>&nbsp;</td></tr>", email.Body);
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, ediMessage.EM_Status);

			ErrorReporter.Clear();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			ediMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch;
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;

			ProcessMessage(ediMessage, logger);

			ProcessMessage(ediMessage, logger);
			email = Env.OutgoingMailManager.EmailsCreated.FirstOrDefault();
			AssertNotNull("Should not create any emails when the processor is in reprocessing mode.", email);
			AssertEquals(EDIMessageStatusList.Codes.Rejected, ediMessage.EM_Status);

			ErrorReporter.Clear();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			ediMessage.EM_MessageOwner = "CASOAReprocess";
			ediMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch;
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;

			ProcessMessage(ediMessage, logger);
			email = Env.OutgoingMailManager.EmailsCreated.FirstOrDefault();
			AssertNull("No email was created.", email);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcess_NoTransactions()
		{
			var ediMessage = ARLStatementOfAccountDocumentWrapperTest.CreateStatementOfAccountMessageAndSetUpTestData(new BusinessObjectFactory(), messageText: NoTransactionsMessageText);
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			Factory.SaveForTesting();

			var logger = new ServiceTaskLogForTesting();
			ProcessMessage(ediMessage, logger);
			ediMessage.Factory.Save();
			AssertContains("Daily Summary Totals", ediMessage.EM_MessageInterpretation);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcess_TotalPayable()
		{
			var messageText = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\SoAUniversalTransactionBatch3.xml");

			var expectedHtml = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\SoAMessageInterpretation3.html");
			expectedHtml = expectedHtml.Replace("{VersionNumber}", $"VersionNumber={new EnterpriseInformationRetriever().VersionNumber}").Replace("\r\n", "").Replace("\t", "");

			var ediMessage = ARLStatementOfAccountDocumentWrapperTest.CreateStatementOfAccountMessageAndSetUpTestData(new BusinessObjectFactory(), messageText: messageText);
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			Factory.SaveForTesting();

			var logger = new ServiceTaskLogForTesting();
			ProcessMessage(ediMessage, logger);
			ediMessage.Factory.Save();

			AssertXMLEquals("EM_MessageInterpretation", expectedHtml, ediMessage.EM_MessageInterpretation.Replace("\r\n", ""));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessWithRMAccountNumber()
		{
			var ediMessage = ARLStatementOfAccountDocumentWrapperTest.CreateStatementOfAccountMessageAndSetUpTestData(new BusinessObjectFactory(), messageText: RMAccountNumberMessageText);
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			Factory.SaveForTesting();

			var logger = new ServiceTaskLogForTesting();
			ProcessMessage(ediMessage, logger);
			ediMessage.Factory.Save();

			var header = Factory.LoadTop1<CusStatementHeader>(new ZQuery(CusStatementHeaderSchema.B2_ImporterCustomsID, "123546789RM0001"));
			AssertNotNull("Should use a safe RM Account Number.", header);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMergingMultiplePartsSOAIntoOneRecord()
		{
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();

			var header1 = Factory.New<CusStatementHeader>();
			header1.B2_StatementType = "B";
			header1.B2_StatementNumber = "849322706052020";
			header1.B2_EntryFilerCode = "12345";
			header1.B2_IsMonthlyStatement = true;
			header1.B2_ProcessDate = new ZDateTime(2020, 06, 25);
			header1.B2_PrintDate = new ZDateTime(2020, 06, 25);
			header1.B2_B2_PeriodicStatement = ZGuid.Empty;
			header1.B2_GC = otherCompany.PK;

			var header2 = Factory.New<CusStatementHeader>();
			header2.B2_StatementType = "B";
			header2.B2_StatementNumber = "849322706052020";
			header2.B2_EntryFilerCode = "54321";
			header2.B2_IsMonthlyStatement = true;
			header2.B2_ProcessDate = new ZDateTime(2020, 06, 25);
			header2.B2_PrintDate = new ZDateTime(2020, 06, 25);
			header2.B2_B2_PeriodicStatement = ZGuid.Empty;
			header2.B2_GC = GlbCompany.CurrentCompany.PK;

			Factory.SaveForTesting();

			var messageText1 = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\SoAUniversalTransactionBatch_WI00333702.xml");
			var messageText2 = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\SoAUniversalTransactionBatch_WI00333702_2.xml");

			var factory = new BusinessObjectFactory();
			var ediMessage1 = ARLStatementOfAccountDocumentWrapperTest.CreateStatementOfAccountMessageAndSetUpTestData(factory, messageText: messageText1);
			ediMessage1.EM_Status = EDIMessageStatusList.Codes.Queued;

			var ediMessage2 = ARLDailyReportDocumentWrapperTest.CreateTransactionBatchMessage("398C44D0-A664-4DFF-AEB9-7914DF329699", factory, messageText2);
			factory.Save();

			using (CACustomsDataRegistry.Instance.ConsolidateSOA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var logger = new ServiceTaskLogForTesting();
				ProcessMessage(ediMessage1, logger);
				ProcessMessage(ediMessage2, logger);
				factory.Save();
			}

			var query = new ZQuery();
			query.AddToFilter(CusStatementHeaderSchema.B2_StatementType, "B");
			query.AddToFilter(CusStatementHeaderSchema.B2_IsMonthlyStatement, true);

			var cusStatementHeaders = Factory.Load<CusStatementHeader>(query);
			AssertEquals("Only one statement should be created, so 3 in total", 3, cusStatementHeaders.Length);

			ediMessage1.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch;
			ediMessage2.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch;

			using (CACustomsDataRegistry.Instance.ConsolidateSOA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var logger = new ServiceTaskLogForTesting();
				ProcessMessage(ediMessage1, logger);
				ProcessMessage(ediMessage2, logger);
				factory.Save();
			}

			cusStatementHeaders = Factory.Load<CusStatementHeader>(query);
			AssertEquals("Two new statements should be created, so 5 in total", 5, cusStatementHeaders.Length);
		}

		CusStatementHeader AssertStatementHeader(BusinessObjectFactory factory, int linesCount, ZString importerCustomsId, ZString statementType, ZString statementNumber, ZString accountSecurityNumber, ZDateTime dueDate, ZDateTime printDate, ZDecimal statementAmount)
		{
			var query = new ZQuery();
			query.AddToFilter(CusStatementHeaderSchema.B2_StatementType, statementType);
			query.AddToFilter(CusStatementHeaderSchema.B2_StatementNumber, statementNumber);
			query.AddToFilter(CusStatementHeaderSchema.B2_IsMonthlyStatement, true);

			var cusStatementHeader = factory.LoadTop1<CusStatementHeader>(query);
			AssertNotNull("Statement Header exists", cusStatementHeader);

			CombineAssertions(() =>
			{
				AssertEquals("StatementLines.Count", linesCount, cusStatementHeader.StatementLines.Count);

				AssertEquals("Statement Type", statementType, cusStatementHeader.B2_StatementType);
				AssertEquals("Statement Number", statementNumber, cusStatementHeader.B2_StatementNumber);
				AssertEquals("Importer Customs ID", importerCustomsId, cusStatementHeader.B2_ImporterCustomsID);
				AssertEquals("Account Security Number", accountSecurityNumber, cusStatementHeader.B2_EntryFilerCode);

				var expectedImporter = TransactionBatchExtension.GetOrgsFromBN(importerCustomsId, ZString.Empty, factory, true).FirstOrDefault()?.PK ?? ZGuid.Empty;
				AssertEquals("Importer", expectedImporter, cusStatementHeader.B2_OH_Importer);

				AssertEquals("Print Date", printDate, cusStatementHeader.B2_PrintDate);
				AssertEquals("Due Date", dueDate, cusStatementHeader.B2_DueDate);

				AssertEquals("Statement Amount", statementAmount, cusStatementHeader.B2_StatementAmount);
			});

			return cusStatementHeader;
		}

		CusStatementLineGroup AssertStatementLineGroup(CusStatementHeader header, ZGuid importerPk, ZString importerCustomsId, ZDecimal[] financialDetailAmounts)
		{
			var group = header.LineGroupCollection.Find(c => c.B10_OH_Importer == importerPk && c.B10_ImporterCustomsID == importerCustomsId).FirstOrDefault();
			AssertNotNull($"Should create a group which the importer pk is {importerPk} and the customs ID is {importerCustomsId}.", group);

			CombineAssertions(() =>
			{
				AssertEquals("ArrearsInterest", financialDetailAmounts[0], group.FinancialDetailCollection.Find(PostingJournalTypeList.Codes.ArrearsInterest)?.B11_Amount ?? 0m);
				AssertEquals("InterestAmount", financialDetailAmounts[1], group.FinancialDetailCollection.Find(PostingJournalTypeList.Codes.InterestAmount)?.B11_Amount ?? 0m);
				AssertEquals("InstalmentLastAmount", financialDetailAmounts[2], group.FinancialDetailCollection.Find(PostingJournalTypeList.Codes.InstalmentLastAmount)?.B11_Amount ?? 0m);
				AssertEquals("OtherCharges", financialDetailAmounts[3], group.FinancialDetailCollection.Find(PostingJournalTypeList.Codes.OtherCharges)?.B11_Amount ?? 0m);
				AssertEquals("Refund", financialDetailAmounts[4], group.FinancialDetailCollection.Find(PostingJournalTypeList.Codes.Refund)?.B11_Amount ?? 0m);
				AssertEquals("PreviousMonthlyStatementTotal", financialDetailAmounts[5], group.FinancialDetailCollection.Find(PostingJournalTypeList.Codes.PreviousMonthlyStatementTotal)?.B11_Amount ?? 0m);
				AssertEquals("PaymentReceivedSinceLastMonthlyStatement", financialDetailAmounts[6], group.FinancialDetailCollection.Find(PostingJournalTypeList.Codes.PaymentReceivedSinceLastMonthlyStatement)?.B11_Amount ?? 0m);
				AssertEquals("TotalCredits", financialDetailAmounts[7], group.FinancialDetailCollection.Find(PostingJournalTypeList.Codes.TotalCredits)?.B11_Amount ?? 0m);
				AssertEquals("TotalPayableForBroker", financialDetailAmounts[8], group.FinancialDetailCollection.Find(PostingJournalTypeList.Codes.TotalPayableForBroker)?.B11_Amount ?? 0m);
				AssertEquals("TransactionTotal", financialDetailAmounts[9], group.FinancialDetailCollection.Find(PostingJournalTypeList.Codes.TransactionTotal)?.B11_Amount ?? 0m);
				AssertEquals("UnpaidBalanceForward", financialDetailAmounts[10], group.FinancialDetailCollection.Find(PostingJournalTypeList.Codes.UnpaidBalanceForward)?.B11_Amount ?? 0m);
				AssertEquals("TotalPayableForImporter", financialDetailAmounts[11], group.FinancialDetailCollection.Find(PostingJournalTypeList.Codes.TotalPayableForImporter)?.B11_Amount ?? 0m);
			});

			return group;
		}

		CusStatementLine AssertStatementLine(CusStatementHeader header, ZString transactionNumber, ZString importerCustomsId, ZString status, ZString entryStatus, ZString entryType, ZString indicator, ZString associatedEntry, ZString creditNote, ZDateTime scheduledProcessDate, ZDateTime creditNoteDate, ZDateTime dueDate, ZDecimal customsFeesTotal)
		{
			var cusStatementLine = header.StatementLines.GetStatementLineFor(transactionNumber, entryType, entryStatus);

			AssertNotNull(transactionNumber + " line exists", cusStatementLine);

			CombineAssertions(() =>
			{
				AssertEquals("Status", status, cusStatementLine.B3_Status);
				AssertEquals("Entry Status", entryStatus, cusStatementLine.B3_EntryStatus);
				AssertEquals("Entry Type", entryType, cusStatementLine.B3_EntryType);
				AssertEquals("Transaction Number", transactionNumber, cusStatementLine.B3_EntryNum);
				AssertEquals("Indicator", indicator, cusStatementLine.B3_EIIndicator);
				AssertEquals("Associated Entry", associatedEntry, cusStatementLine.B3_AssociatedEntry);
				AssertEquals("Importer Customs ID", importerCustomsId, cusStatementLine.B3_ImporterCustomsID);

				AssertEquals("Scheduled Process Date", scheduledProcessDate, cusStatementLine.B3_ScheduledProcessDate);
				AssertEquals("Due Date", dueDate, cusStatementLine.B3_DueDate);

				AssertEquals("Credit Note", creditNote, cusStatementLine.B3_CreditNote);
				AssertEquals("Credit Note Date", creditNoteDate, cusStatementLine.B3_CreditNoteDate);

				AssertEquals("Customs Fees Total", customsFeesTotal, cusStatementLine.B3_CustomsFeesTotal);
			});

			return cusStatementLine;
		}

		const string NoTransactionsMessageText = @"<UniversalTransactionBatch xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<TransactionBatch>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>CAAccountsReceivableLedger</Type>
					<Key></Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<BatchType>
			<Code>SOA</Code>
			<Description>Statement of Account</Description>
		</BatchType>
		<DateFrom>2014-10-25T00:00:00</DateFrom>
		<DateTo>2014-11-24T00:00:00</DateTo>
		<OrganizationAddressCollection>
			<OrganizationAddress>
				<AddressType>ImportBroker</AddressType>
				<GovRegNum>123546789</GovRegNum>
				<CompanyName>PIERRE IMPORTERS Inc.</CompanyName>
			</OrganizationAddress>
		</OrganizationAddressCollection>
		<TransactionCollection>
			<Transaction>
				<Category>SUM</Category>
				<CreateTime>2014-01-25T00:00:00</CreateTime>
				<DueDate>2014-01-31T00:00:00</DueDate>
				<NumberOfSupportingDocuments>0001</NumberOfSupportingDocuments>
				<OrganizationAddress>
					<AddressType>ImportBroker</AddressType>
					<GovRegNum>123546789</GovRegNum>
					<CompanyName>PIERRE IMPORTERS Inc.</CompanyName>
					<RegistrationNumberCollection>
						<RegistrationNumber>
							<Type>
								<Code>ASC</Code>
							</Type>
							<CountryOfIssue>
								<Code>CA</Code>
							</CountryOfIssue>
							<Value>00000</Value>
						</RegistrationNumber>
					</RegistrationNumberCollection>
				</OrganizationAddress>
				<PostingJournalCollection>
					<PostingJournal></PostingJournal>
				</PostingJournalCollection>
			</Transaction>
			<Transaction>
				<Category>SUM</Category>
				<OrganizationAddress>
					<AddressType>ImporterDocumentaryAddress</AddressType>
					<GovRegNum>123546789RM0001</GovRegNum>
					<CompanyName>PIERRE IMPORTERS Inc.</CompanyName>
				</OrganizationAddress>
				<PostingJournalCollection>
					<PostingJournal>
						<ChargeCode>
							<Code>DTY</Code>
							<Description>Duty</Description>
						</ChargeCode>
						<ChargeTotalAmount>56000</ChargeTotalAmount>
					</PostingJournal>
					<PostingJournal>
						<ChargeCode>
							<Code>GST</Code>
							<Description>GST</Description>
						</ChargeCode>
						<ChargeTotalAmount>41900.74</ChargeTotalAmount>
					</PostingJournal>
					<PostingJournal>
						<ChargeCode>
							<Code>OTH</Code>
							<Description>Others</Description>
						</ChargeCode>
						<ChargeTotalAmount>200</ChargeTotalAmount>
					</PostingJournal>
					<PostingJournal>
						<Description>PreviousMonthlyStatementTotal</Description>
						<LocalAmount>240870.35</LocalAmount>
					</PostingJournal>
					<PostingJournal>
						<Description>PaymentReceivedSinceLastMonthlyStatement</Description>
						<LocalAmount>240620.35</LocalAmount>
					</PostingJournal>
					<PostingJournal>
						<Description>Refund</Description>
						<LocalAmount>0</LocalAmount>
					</PostingJournal>
					<PostingJournal>
						<Description>UnpaidBalanceForward</Description>
						<LocalAmount>250</LocalAmount>
					</PostingJournal>
					<PostingJournal>
						<Description>ArrearsInterest</Description>
						<LocalAmount>21.12</LocalAmount>
					</PostingJournal>
					<PostingJournal>
						<Description>TransactionTotal</Description>
						<LocalAmount>98100.74</LocalAmount>
					</PostingJournal>
					<PostingJournal>
						<Description>OtherCharges</Description>
						<LocalAmount>26000</LocalAmount>
					</PostingJournal>
				</PostingJournalCollection>
				<LocalTotal>354200.64</LocalTotal>
			</Transaction>
		</TransactionCollection>
	</TransactionBatch>
</UniversalTransactionBatch>";

		const string RMAccountNumberMessageText = @"<UniversalTransactionBatch xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<TransactionBatch>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>CAAccountsReceivableLedger</Type>
					<Key></Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<BatchType>
			<Code>SOA</Code>
			<Description>Statement of Account</Description>
		</BatchType>
		<DateFrom>2014-10-25T00:00:00</DateFrom>
		<DateTo>2014-11-24T00:00:00</DateTo>
		<OrganizationAddressCollection>
			<OrganizationAddress>
				<AddressType>ImportBroker</AddressType>
				<GovRegNum>123546789</GovRegNum>
				<CompanyName>PIERRE IMPORTERS Inc.</CompanyName>
			</OrganizationAddress>
		</OrganizationAddressCollection>
		<TransactionCollection>
			<Transaction>
				<Category>SUM</Category>
				<CreateTime>2014-01-25T00:00:00</CreateTime>
				<DueDate>2014-01-31T00:00:00</DueDate>
				<NumberOfSupportingDocuments>0001</NumberOfSupportingDocuments>
				<Number>RM0001</Number>
				<OrganizationAddress>
					<AddressType>ImportBroker</AddressType>
					<GovRegNum>123546789</GovRegNum>
					<CompanyName>PIERRE IMPORTERS Inc.</CompanyName>
					<RegistrationNumberCollection>
						<RegistrationNumber>
							<Type>
								<Code>ASC</Code>
							</Type>
							<CountryOfIssue>
								<Code>CA</Code>
							</CountryOfIssue>
							<Value>00000</Value>
						</RegistrationNumber>
					</RegistrationNumberCollection>
				</OrganizationAddress>
				<PostingJournalCollection>
					<PostingJournal></PostingJournal>
				</PostingJournalCollection>
			</Transaction>
		</TransactionCollection>
	</TransactionBatch>
</UniversalTransactionBatch>";

		IDisposable instanceDetailsDisposable;

		protected override void SetUp()
		{
			base.SetUp();
			instanceDetailsDisposable = InstanceDetails.SetUpCurrentForTest();
		}

		protected override void TearDown()
		{
			instanceDetailsDisposable?.Dispose();
			base.TearDown();
		}
	}
}
