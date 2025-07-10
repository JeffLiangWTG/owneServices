using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class ARLDailyNoticeMessageProcessorTest : TestCaseWithFactoryAndMessagingHelpers
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMatchExactTransactionNumber()
		{
			var messageText = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\DNUniversalTransactionBatch2.xml");
			messageText = messageText.Replace("11222783837955", "783837955");

			var ediMessage = ARLDailyReportDocumentWrapperTest.CreateDailyNoticeMessageAndSetUpTestData(new BusinessObjectFactory(), messageText: messageText);
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "22333");
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			var declaration = ediMessage.Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001111"));
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			declaration.TransactionNumber.AccountSecurityCode = "11122";
			declaration.TransactionNumber.SequentialNumber = "78383795";
			declaration.CustomsEntryHeaders.AddNew().CH_MessageType = MessageTypeList.Codes.B3CUSDEC;

			var declaration2 = ediMessage.Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001112"));
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.TransactionNumber.AccountSecurityCode = "11222";
			declaration2.TransactionNumber.SequentialNumber = "78383798";
			declaration2.CustomsEntryHeaders.AddNew().CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			declaration2.CA_K84AccountingDate = new ZDateTime(2014, 11, 17);
			ediMessage.Factory.Save();

			var logger = new ServiceTaskLogForTesting();
			ProcessMessage(ediMessage, logger);
			ediMessage.Factory.Save();

			var factory = new BusinessObjectFactory();
			declaration = factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001111"));
			AssertEquals(ZDateTime.Empty, declaration.CA_K84StatementDate);
			AssertNull(declaration.Logs.MostRecentLogByEventTime(Events.MessageStatusChange, "DN Received"));

			declaration2 = factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001112"));
			AssertEquals(new ZDateTime(2014, 11, 22), declaration2.CA_K84StatementDate);
			AssertNotNull(declaration2.Logs.MostRecentLogByEventTime(Events.MessageStatusChange, "DN Received"));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetKeys()
		{
			var expectedHtml = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\DNMessageInterpretation.html");
			expectedHtml = expectedHtml.Replace("\r\n", "");

			var ediMessage = ARLDailyReportDocumentWrapperTest.CreateDailyNoticeMessageAndSetUpTestData(new BusinessObjectFactory());
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "22333");
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			Factory.SaveForTesting();

			var logger = new ServiceTaskLogForTesting();
			var keys = GetKeys(ediMessage, logger);

			AssertEquals(false, keys.ShouldShortCircuit);
			AssertEquals(nameof(ARLDailyNoticeDocumentWrapper), keys.Keys.Single());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessAndPurgeLegacyData()
		{
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "22333");

			var factory = new BusinessObjectFactory();

			var ediMessage = ARLDailyReportDocumentWrapperTest.CreateDailyNoticeMessageAndSetUpTestData(new BusinessObjectFactory());
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;

			Factory.SaveForTesting();

			var importer3 = factory.NewWithValidTestData<OrgHeader>();
			importer3.OH_Code = "IMPORTER3";

			#region Legacy Data

			var statementHeaderForUpdate = factory.New<CusStatementHeader>();
			statementHeaderForUpdate.B2_IsMonthlyStatement = false;
			statementHeaderForUpdate.B2_StatementType = "I";
			statementHeaderForUpdate.B2_StatementNumber = "123546789RM0001141117-001";

			statementHeaderForUpdate.B2_ImporterCustomsID = "123546789RM0001";
			statementHeaderForUpdate.B2_OH_Importer = TransactionBatchExtension.GetOrgsFromBN("123546789RM0001", ZString.Empty, factory).FirstOrDefault()?.PK ?? ZGuid.Empty;
			statementHeaderForUpdate.B2_EntryFilerCode = "22333";
			statementHeaderForUpdate.B2_DueDate = new ZDateTime(2014, 01, 05);
			statementHeaderForUpdate.B2_PrintDate = new ZDateTime(2014, 01, 01);
			statementHeaderForUpdate.B2_StatementAmount = 57m;
			statementHeaderForUpdate.B2_PaidAmount = 3m;
			statementHeaderForUpdate.B2_RefundAmount = 12m;

			statementHeaderForUpdate.Notes.AddNew(true, "Message to Recipient (English)", "TEST EN");
			statementHeaderForUpdate.Notes.AddNew(true, "Message to Recipient (French)", "TEST FR");

			var groupForUpdate = statementHeaderForUpdate.LineGroupCollection.FindOrCreate("123546789RM0001");
			var groupForDelete = statementHeaderForUpdate.LineGroupCollection.FindOrCreate("123546789RM0003");

			var lineForUpdate = statementHeaderForUpdate.StatementLines.AddNew();
			lineForUpdate.B3_EntryNum = "11222783837955";
			lineForUpdate.B3_EntryType = "B3";

			lineForUpdate.B3_BrokerReference = "B00000001";
			lineForUpdate.B3_EntryProcessPort = "NJ";
			lineForUpdate.B3_AssociatedEntry = "TEST ENTRY";
			lineForUpdate.B3_ImporterCustomsID = "123546789RM0000";
			lineForUpdate.B3_EntryDate = new ZDate(2014, 11, 05);
			lineForUpdate.B3_ScheduledProcessDate = new ZDate(2014, 11, 12);
			lineForUpdate.B3_DueDate = new ZDate(2014, 11, 01);

			var lineForDelete = statementHeaderForUpdate.StatementLines.AddNew();
			lineForDelete.B3_EntryNum = "11222783837952";
			lineForDelete.B3_EntryType = "B2";

			lineForDelete.B3_BrokerReference = "B00000002";
			lineForDelete.B3_EntryProcessPort = "BJ";
			lineForDelete.B3_AssociatedEntry = "TEST ENTRY II";
			lineForDelete.B3_ImporterCustomsID = "123546789RM0003";
			lineForDelete.B3_EntryDate = new ZDate(2014, 11, 03);
			lineForDelete.B3_ScheduledProcessDate = new ZDate(2014, 11, 15);
			lineForDelete.B3_DueDate = new ZDate(2014, 11, 08);

			factory.Save();

			#endregion

			var logger = new ServiceTaskLogForTesting();
			ProcessMessage(ediMessage, logger);
			ediMessage.Factory.Save();

			factory = new BusinessObjectFactory();

			var statementHeader = AssertStatementHeader(factory, 10, "123546789RM0001", "I", "123546789RM0001141117-001", "11222", new ZDateTime(2014, 11, 17), new ZDateTime(2014, 11, 18), 163970.5100m, 0m, 0m);
			AssertEquals("Should find the existing data and update some data.", statementHeader.PK, statementHeaderForUpdate.PK);
			AssertNull("Should remove the note as the latest message does not contains any EN Note.", statementHeaderForUpdate.Notes.FindByDescription("Message to Recipient (English)").FirstOrDefault());
			AssertNull("Should remove the note as the latest message does not contains any FR Note.", statementHeaderForUpdate.Notes.FindByDescription("Message to Recipient (French)").FirstOrDefault());

			var lineGroup = AssertStatementLineGroup(statementHeader, "123546789RM0001");
			AssertEquals("Should find the existing data and update some data.", lineGroup.PK, groupForUpdate.PK);

			var line = AssertStatementLine(statementHeader, "11222783837955", "123546789RM0001", "B00001111", "B3", "0395", "", new ZDateTime(2014, 11, 19), new ZDateTime(2014, 12, 01), ZDateTime.Empty, new ZDecimal[] { 135987.23m, 0m, 0m, 0m, 0m, 0m, 0m });
			AssertEquals("Should find the existing data and update some data.", line.PK, lineForUpdate.PK);

			Assert("Should be deleted as it's not used.", groupForDelete.IsDeleted);
			Assert("Should be deleted as it's not used.", lineForDelete.IsDeleted);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLoadOtherChargeTypeLines()
		{
			#region Message Text

			string messageText = @"<UniversalTransactionBatch xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
<TransactionBatch><DataContext><DataTargetCollection><DataTarget><Type>CAAccountsReceivableLedger</Type><Key /></DataTarget></DataTargetCollection></DataContext>
<BatchType><Code>DN</Code><Description>Daily Notices</Description></BatchType>
<OrganizationAddressCollection><OrganizationAddress><AddressType>ImportBroker</AddressType><GovRegNum>849322766RM0001</GovRegNum><CompanyName>A.D. RUTHERFORD INTERNATIONAL</CompanyName></OrganizationAddress>
</OrganizationAddressCollection>
<TransactionCollection>
	<Transaction>
		<Category>SUM</Category><CreateTime>2021-04-18T00:00:00</CreateTime><PostDate>2021-04-18T00:00:00</PostDate><ComplianceSubType>I</ComplianceSubType><Number>0001</Number>
		<OrganizationAddress><AddressType>ImportBroker</AddressType><GovRegNum>849322766RM0001</GovRegNum><CompanyName>CABELA'S RETAIL CANADA INC.</CompanyName></OrganizationAddress>
		<PostingJournalCollection><PostingJournal><Description>TotalPaymentReceived</Description><LocalAmount>0</LocalAmount></PostingJournal><PostingJournal><Description>Refund</Description>
		<LocalAmount>0</LocalAmount></PostingJournal></PostingJournalCollection><ShipmentCollection><Shipment><AddInfoCollection><AddInfo><Key>ImporterSectionCounter</Key><Value>1</Value></AddInfo></AddInfoCollection>
		</Shipment>
		</ShipmentCollection>
	</Transaction>
	<Transaction><Category>SUM</Category><OrganizationAddress><AddressType>ImporterDocumentaryAddress</AddressType><GovRegNum>849322766RM0001</GovRegNum><CompanyName>A.D. RUTHERFORD INTERNATIONAL</CompanyName></OrganizationAddress>
		<PostingJournalCollection><PostingJournal><ChargeCode><Code>DTY</Code><Description>Duty</Description></ChargeCode><ChargeTotalAmount>1023.57</ChargeTotalAmount></PostingJournal>
		<PostingJournal><ChargeCode><Code>GST</Code><Description>GST</Description></ChargeCode><ChargeTotalAmount>794.1</ChargeTotalAmount></PostingJournal>
		<PostingJournal><Description>TotalPaymentReceived</Description><LocalAmount>0</LocalAmount></PostingJournal><PostingJournal><Description>Refund</Description>
		<LocalAmount>0</LocalAmount></PostingJournal></PostingJournalCollection><LocalTotal>1817.67</LocalTotal>
	</Transaction>
	<Transaction>
		<Category>OTH</Category><OrganizationAddress><AddressType>ImporterDocumentaryAddress</AddressType><GovRegNum>849322766RM0001</GovRegNum></OrganizationAddress>
		<ComplianceSubType>B2</ComplianceSubType><TransactionReference>10105002214884</TransactionReference><PostingJournalCollection><PostingJournal><Description /></PostingJournal></PostingJournalCollection>
		<LocalTotal>6.95</LocalTotal><TransactionDate>2021-04-19T00:00:00</TransactionDate><DueDate>2021-05-19T00:00:00</DueDate><OriginalReference><OriginalTransactionNumber>10105002006746</OriginalTransactionNumber>
		</OriginalReference>
	</Transaction>
	<Transaction>
		<Category>OTH</Category><OrganizationAddress><AddressType>ImporterDocumentaryAddress</AddressType><GovRegNum>849322766RM0001</GovRegNum></OrganizationAddress>
		<ComplianceSubType>B2</ComplianceSubType><TransactionReference>10105002214884</TransactionReference><PostingJournalCollection><PostingJournal><Description /></PostingJournal></PostingJournalCollection>
		<LocalTotal>-6.95</LocalTotal><TransactionDate>2021-04-19T00:00:00</TransactionDate><OriginalReference><OriginalTransactionNumber>10105002006746</OriginalTransactionNumber></OriginalReference>
	</Transaction>
</TransactionCollection></TransactionBatch></UniversalTransactionBatch>";

			#endregion

			var ediMessage = ARLDailyReportDocumentWrapperTest.CreateDailyNoticeMessageAndSetUpTestData(new BusinessObjectFactory(), messageText: messageText);
			var importer1 = ediMessage.Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "IMPORTER1");
			importer1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "849322766RM0001", Core.Constants.CountryCodes.Canada);
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "22333");
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			Factory.SaveForTesting();

			var logger = new ServiceTaskLogForTesting();
			ProcessMessage(ediMessage, logger);
			ediMessage.Factory.Save();

			var cusStatementHeader = Factory.LoadTop1<CusStatementHeader>(new ZQuery(CusStatementHeaderSchema.B2_ImporterCustomsID, "849322766RM0001"));
			AssertNotNull("Statement Header exists", cusStatementHeader);

			AssertEquals("Statement Lines", 1, cusStatementHeader.StatementLines.Count);
			var line1 = cusStatementHeader.StatementLines[0];
			AssertEquals("TotalFee", 0m, line1.B3_CustomsFeesTotal);
			AssertEquals("OtherTypeCharges", 2, line1.Charges.Count);
			AssertEquals("6.95", true, line1.Charges.OfType<CusStatementLineCharge>().Any(x => x.B4_ChargeAmount == 6.95m));
			AssertEquals("-6.95", true, line1.Charges.OfType<CusStatementLineCharge>().Any(x => x.B4_ChargeAmount == -6.95m));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoteForCommentsEnAndFR()
		{
			var messageText = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\DNUniversalTransactionBatch5.xml");

			var ediMessage = ARLDailyReportDocumentWrapperTest.CreateDailyNoticeMessageAndSetUpTestData(new BusinessObjectFactory(), messageText: messageText);
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "22333");
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;

			Factory.SaveForTesting();
			var logger = new ServiceTaskLogForTesting();
			ProcessMessage(ediMessage, logger);
			ediMessage.Factory.Save();

			var factory = new BusinessObjectFactory();
			var headers = factory.Load(typeof(CusStatementHeader), new ZQuery());
			AssertEquals(1, headers.Length);

			var header = headers[0] as CusStatementHeader;
			AssertEquals("Message English", header.Notes.FindByDescription("Message to Recipient (English)")[0].ST_NoteDataAsText);
			AssertEquals("Message French", header.Notes.FindByDescription("Message to Recipient (French)")[0].ST_NoteDataAsText);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcess()
		{
			var expectedHtml = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\DNMessageInterpretation.html");
			expectedHtml = expectedHtml.Replace("{VersionNumber}", $"VersionNumber={new EnterpriseInformationRetriever().VersionNumber}").Replace("\r\n", "");

			var ediMessage = ARLDailyReportDocumentWrapperTest.CreateDailyNoticeMessageAndSetUpTestData(new BusinessObjectFactory());
			var importer1 = ediMessage.Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "IMPORTER1");
			importer1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "123546789RM0001", Core.Constants.CountryCodes.Canada);
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "22333");
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			Factory.SaveForTesting();

			var logger = new ServiceTaskLogForTesting();
			ProcessMessage(ediMessage, logger);
			ediMessage.Factory.Save();

			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, ediMessage.EM_Status);
			AssertEquals(ARLMessageTypes.Codes.DailyNotice, ediMessage.EM_MessageSubType);
			AssertXMLEquals("EM_MessageInterpretation", expectedHtml, ediMessage.EM_MessageInterpretation.Replace("\r\n", ""));

			var factory = new BusinessObjectFactory();
			var arlMessage = factory.Load<ARLMessage>(ediMessage.PK);
			AssertEquals(ZString.Empty, arlMessage.XMLCustomsMessageType);
			AssertEquals(new ZDateTime(2014, 11, 18), arlMessage.K84StatementDate);
			AssertEquals(new ZDateTime(2014, 11, 17), arlMessage.K84AccountingDate);

			var headers = factory.Load(typeof(CusStatementHeader), new ZQuery());
			AssertEquals(1, headers.Length);

			var cusStatementHeader = AssertStatementHeader(factory, 10, "123546789RM0001", "I", "123546789RM0001141117-001", "11222", new ZDateTime(2014, 11, 17), new ZDateTime(2014, 11, 18), 163970.5100m, 0m, 0m);

			AssertStatementLineGroup(cusStatementHeader, "123546789RM0001");
			AssertStatementLineGroup(cusStatementHeader, "123546789RM0002");

			var line1 = AssertStatementLine(cusStatementHeader, "11222783837955", "123546789RM0001", "B00001111", "B3", "0395", "", new ZDateTime(2014, 11, 19), new ZDateTime(2014, 12, 01), ZDateTime.Empty, new ZDecimal[] { 135987.23m, 0m, 0m, 0m, 0m, 0m, 0m });
			var line2 = AssertStatementLine(cusStatementHeader, "11222783837988", "123546789RM0001", "B00001112", "B3", "0395", "", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, new ZDecimal[] { 0m, 0m, 0m, 0m, 25698.25m, 0m, 0m });
			var line3 = AssertStatementLine(cusStatementHeader, "11222783837957", "123546789RM0001", "B00001113", "LA", "0395", "", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, new ZDecimal[] { 0m, 0m, 0m, 0m, 0m, 0m, 100.00m });
			AssertStatementLine(cusStatementHeader, "11222783837973", "123546789RM0001", "B00001119", "B2", "", "11222783837974", new ZDateTime(2014, 11, 17), ZDateTime.Empty, new ZDateTime(2014, 11, 17), new ZDecimal[] { 0m, 0m, 0m, 0m, 0m, 300.00m, 0m });
			AssertStatementLine(cusStatementHeader, "11222783837971", "123546789RM0001", "B00001117", "B2", "", "11222783837972", new ZDateTime(2014, 11, 17), ZDateTime.Empty, new ZDateTime(2014, 11, 17), new ZDecimal[] { 0m, 0m, 0m, 0m, 0m, 321.36m, 0m });
			AssertStatementLine(cusStatementHeader, "11222783837958", "123546789RM0002", "B00001114", "B2", "0395", "11222356875434m", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, new ZDecimal[] { -140.23m, 0m, 0m, 0m, 0m, 0m, 0m });
			AssertStatementLine(cusStatementHeader, "11222783837958", "123546789RM0002", "B00001114", "K3", "0395", "", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, new ZDecimal[] { 0m, 0m, 0m, 150.20m, 0m, 0m, 0m });
			AssertStatementLine(cusStatementHeader, "11222783837960", "123546789RM0002", "B00001116", "K3", "0395", "", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, new ZDecimal[] { 0m, -1150.20m, 0m, 0m, 0m, 0m, 0m });
			AssertStatementLine(cusStatementHeader, "11222783837961", "123546789RM0002", "", "P1", "0395", "", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, new ZDecimal[] { 0m, 0m, 0m, 0m, 0m, 3300.26m, 0m });
			AssertStatementLine(cusStatementHeader, "", "123546789RM0002", "", "NS", "", "", new ZDateTime(2014, 11, 17), ZDateTime.Empty, ZDateTime.Empty, new ZDecimal[] { 0m, 0m, 0m, 0m, 0m, 25.00m, 0m });

			AssertPaymentParty(line1, "IMP", EntryChargeTypeList.Codes.TotalDutyAmount);
			AssertPaymentParty(line2, "IMP", EntryChargeTypeList.Codes.TotalGSTDirectAmount);
			AssertPaymentParty(line3, "BRK", EntryChargeTypeList.Codes.K84LateFilingPenalty);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessWithoutImporter()
		{
			using (CACustomsDataRegistry.Instance.AccountSecurityNo.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "00335"))
			{
				var ediMessage = ARLDailyReportDocumentWrapperTest.CreateDailyNoticeMessageAndSetUpTestData(new BusinessObjectFactory());
				ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;

				var importer1 = ediMessage.Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "IMPORTER1");
				importer1.CustomsCodes.RemoveAndDeleteAll();

				var importer2 = ediMessage.Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "IMPORTER2");
				importer2.CustomsCodes.RemoveAndDeleteAll();

				var declarations = ediMessage.Factory.Load<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_OH_Importer, new[] { importer1.PK, importer2.PK }));
				foreach (var declaration in declarations)
				{
					declaration.JE_OH_Importer = ZGuid.Empty;
				}

				ediMessage.Factory.Save();

				var logger = new ServiceTaskLogForTesting();
				ProcessMessage(ediMessage, logger);

				ediMessage.Factory.Save();

				var factory = new BusinessObjectFactory();

				var cusStatementHeader = AssertStatementHeader(factory, 10, "123546789RM0001", "I", "123546789RM0001141117-001", "00335", new ZDateTime(2014, 11, 17), new ZDateTime(2014, 11, 18), 163970.5100m, 0m, 0m);
				AssertStatementLineGroup(cusStatementHeader, "123546789RM0001");
				AssertStatementLineGroup(cusStatementHeader, "123546789RM0002");
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSettingDates()
		{
			var messageText = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\DNUniversalTransactionBatch2.xml");

			var ediMessage = ARLDailyReportDocumentWrapperTest.CreateDailyNoticeMessageAndSetUpTestData(new BusinessObjectFactory(), messageText: messageText);
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "22333");
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			var declaration = ediMessage.Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001111"));
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			declaration.TransactionNumber.AccountSecurityCode = "11222";
			declaration.TransactionNumber.SequentialNumber = "78383795";
			declaration.CustomsEntryHeaders.AddNew().CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			declaration.CA_K84StatementDate = new ZDateTime(2014, 11, 18);

			var declaration2 = ediMessage.Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001112"));
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.TransactionNumber.AccountSecurityCode = "11222";
			declaration2.TransactionNumber.SequentialNumber = "78383798";
			declaration2.CustomsEntryHeaders.AddNew().CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			declaration2.CA_K84AccountingDate = new ZDateTime(2014, 11, 17);

			var declaration3 = ediMessage.Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001113"));
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.TransactionNumber.AccountSecurityCode = "11222";
			declaration3.TransactionNumber.SequentialNumber = "78383799";
			declaration3.CustomsEntryHeaders.AddNew().CH_MessageType = MessageTypeList.Codes.B3CUSDEC;

			ediMessage.Factory.Save();

			var logger = new ServiceTaskLogForTesting();
			ProcessMessage(ediMessage, logger);
			ediMessage.Factory.Save();

			var factory = new BusinessObjectFactory();
			var arlMessage = factory.Load<ARLMessage>(ediMessage.PK);
			AssertEquals(ARLMessageTypes.Codes.DailyNotice, arlMessage.EM_MessageSubType);
			AssertEquals(new ZDateTime(2014, 11, 22), arlMessage.K84StatementDate);
			AssertEquals(new ZDateTime(2014, 11, 21), arlMessage.K84AccountingDate);

			declaration = factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001111"));
			AssertEquals(new ZDateTime(2014, 11, 18), declaration.CA_K84StatementDate);
			AssertEquals(new ZDateTime(2014, 11, 21), declaration.CA_K84AccountingDate);
			AssertEquals(new ZDateTime(2014, 11, 21), declaration.CA_ConfirmedDate);
			AssertEquals(new ZDateTime(2014, 11, 21), declaration.CA_B2AcceptedDate);

			declaration2 = factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001112"));
			AssertEquals(new ZDateTime(2014, 11, 22), declaration2.CA_K84StatementDate);
			AssertEquals(new ZDateTime(2014, 11, 17), declaration2.CA_K84AccountingDate);

			declaration3 = factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001113"));
			AssertEquals(new ZDateTime(2014, 11, 22), declaration3.CA_K84StatementDate);
			AssertEquals(new ZDateTime(2014, 12, 1), declaration3.CA_K84AccountingDate);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAccountingDateSetForLVSTransactions()
		{
			var messageText = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\DNUniversalTransactionBatch2.xml");

			var ediMessage = ARLDailyReportDocumentWrapperTest.CreateDailyNoticeMessageAndSetUpTestData(new BusinessObjectFactory(), messageText: messageText);
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "22333");
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;

			var declaration = ediMessage.Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001113"));
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.TransactionNumber.AccountSecurityCode = "11222";
			declaration.TransactionNumber.SequentialNumber = "78383799";
			declaration.CustomsEntryHeaders.AddNew().CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			ediMessage.Factory.Save();

			var logger = new ServiceTaskLogForTesting();
			ProcessMessage(ediMessage, logger);
			ediMessage.Factory.Save();

			var factory = new BusinessObjectFactory();

			declaration = factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001113"));
			AssertEquals(new ZDateTime(2014, 12, 1), declaration.CA_K84AccountingDate);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAccountingDateSetForLVSTransactionsWhenLVXCreatedLaterThanLVS()
		{
			var factory = new BusinessObjectFactory();
			var messageText = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\DNUniversalTransactionBatch2.xml");

			var ediMessage = ARLDailyReportDocumentWrapperTest.CreateDailyNoticeMessageAndSetUpTestData(factory, messageText: messageText);
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "22333");
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;

			ARLDailyReportDocumentWrapperTest.CreatedDeclaration("36fb43a1-7db2-438a-a65b-8882fd166757", factory, null, "B00001115", "11222783837959", ZDateTime.UtcNow.AddHours(1), JobMessageTypeList.Codes.LVSForConsolidation);
			var declaration = ediMessage.Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001115"));
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration.TransactionNumber.AccountSecurityCode = "11122";
			declaration.TransactionNumber.SequentialNumber = "78383799";
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			cusEntryHeader.CH_BGMReference = "11222783837999";

			var declaration2 = ediMessage.Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001113"));
			declaration2.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration2.TransactionNumber.AccountSecurityCode = "11222";
			declaration2.TransactionNumber.SequentialNumber = "78383799";
			declaration2.CustomsEntryHeaders.AddNew().CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			ediMessage.Factory.Save();

			var logger = new ServiceTaskLogForTesting();
			ProcessMessage(ediMessage, logger);
			ediMessage.Factory.Save();

			var newFactory = new BusinessObjectFactory();
			declaration = newFactory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001113"));
			AssertEquals(new ZDateTime(2014, 11, 22), declaration.CA_K84StatementDate);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcess_AccountingDate()
		{
			var expectedHtml = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\DNMessageInterpretation.html");
			expectedHtml = expectedHtml.Replace("{VersionNumber}", $"VersionNumber={new EnterpriseInformationRetriever().VersionNumber}").Replace("\r\n", "");

			var ediMessage = ARLDailyReportDocumentWrapperTest.CreateDailyNoticeMessageAndSetUpTestData(new BusinessObjectFactory());
			var importer1 = ediMessage.Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "IMPORTER1");
			importer1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "123546789RM0001", Core.Constants.CountryCodes.Canada);
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "22333");
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			Factory.SaveForTesting();

			var logger = new ServiceTaskLogForTesting();
			ProcessMessage(ediMessage, logger);
			ediMessage.Factory.Save();

			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, ediMessage.EM_Status);
			AssertXMLEquals("EM_MessageInterpretation", expectedHtml, ediMessage.EM_MessageInterpretation.Replace("\r\n", ""));

			var factory = new BusinessObjectFactory();
			var arlMessage = factory.Load<ARLMessage>(ediMessage.PK);
			AssertEquals(ARLMessageTypes.Codes.DailyNotice, arlMessage.EM_MessageSubType);
			AssertEquals(new ZDateTime(2014, 11, 18), arlMessage.K84StatementDate);
			AssertEquals(new ZDateTime(2014, 11, 17), arlMessage.K84AccountingDate);

			var headers = factory.Load(typeof(CusStatementHeader), new ZQuery());
			AssertEquals(1, headers.Length);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcess_Warning()
		{
			var messageText = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\DNUniversalTransactionBatch.xml");
			var ediMessage = ARLDailyReportDocumentWrapperTest.CreateTransactionBatchMessage("d6ad4b25-7c49-43e9-bb9b-da8f7249c769", new BusinessObjectFactory(), messageText);
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;

			var logger = new ServiceTaskLogForTesting();
			ProcessMessage(ediMessage, logger);
			ediMessage.Factory.Save();

			var expectedLog = "Warning - Cannot find an organization with BN 123546789\r\nWarning - Cannot find the Account Security Code for importer or in Registry";

			AssertEquals(EDIMessageStatusList.Codes.Warning, ediMessage.EM_Status);
			AssertEquals(expectedLog, logger.ToString());

			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "22333");
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			ediMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch;
			logger = new ServiceTaskLogForTesting();
			ProcessMessage(ediMessage, logger);
			ediMessage.Factory.Save();

			expectedLog = "Warning - Cannot find an organization with BN 123546789\r\nUsing the Account Security Code '22333' in Registry";
			AssertEquals(EDIMessageStatusList.Codes.Warning, ediMessage.EM_Status);
			AssertEquals(expectedLog, logger.ToString());

			var factory = new BusinessObjectFactory();
			var headers = factory.Load(typeof(CusStatementHeader), new ZQuery());
			AssertEquals(1, headers.Length);

			var cusStatementHeader = AssertStatementHeader(factory, 10, "123546789RM0001", "I", "123546789RM0001141117-001", "22333", new ZDateTime(2014, 11, 17), new ZDateTime(2014, 11, 18), 163970.5100m, 0m, 0m);

			AssertStatementLine(cusStatementHeader, "11222783837955", "123546789RM0001", "", "B3", "0395", "", new ZDateTime(2014, 11, 19), new ZDateTime(2014, 12, 01), ZDateTime.Empty, new ZDecimal[] { 135987.23m, 0m, 0m, 0m, 0m, 0m, 0m });
			AssertStatementLine(cusStatementHeader, "11222783837988", "123546789RM0001", "", "B3", "0395", "", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, new ZDecimal[] { 0m, 0m, 0m, 0m, 25698.25m, 0m, 0m });
			AssertStatementLine(cusStatementHeader, "11222783837957", "123546789RM0001", "", "LA", "0395", "", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, new ZDecimal[] { 0m, 0m, 0m, 0m, 0m, 0m, 100.00m });
			AssertStatementLine(cusStatementHeader, "11222783837973", "123546789RM0001", "", "B2", "", "11222783837974", new ZDateTime(2014, 11, 17), ZDateTime.Empty, new ZDateTime(2014, 11, 17), new ZDecimal[] { 0m, 0m, 0m, 0m, 0m, 300.00m, 0m });
			AssertStatementLine(cusStatementHeader, "11222783837971", "123546789RM0001", "", "B2", "", "11222783837972", new ZDateTime(2014, 11, 17), ZDateTime.Empty, new ZDateTime(2014, 11, 17), new ZDecimal[] { 0m, 0m, 0m, 0m, 0m, 321.36m, 0m });
			AssertStatementLine(cusStatementHeader, "11222783837958", "123546789RM0002", "", "B2", "0395", "11222356875434m", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, new ZDecimal[] { -140.23m, 0m, 0m, 0m, 0m, 0m, 0m });
			AssertStatementLine(cusStatementHeader, "11222783837958", "123546789RM0002", "", "K3", "0395", "", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, new ZDecimal[] { 0m, 0m, 0m, 150.20m, 0m, 0m, 0m });
			AssertStatementLine(cusStatementHeader, "11222783837960", "123546789RM0002", "", "K3", "0395", "", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, new ZDecimal[] { 0m, -1150.20m, 0m, 0m, 0m, 0m, 0m });
			AssertStatementLine(cusStatementHeader, "11222783837961", "123546789RM0002", "", "P1", "0395", "", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, new ZDecimal[] { 0m, 0m, 0m, 0m, 0m, 3300.26m, 0m });
			AssertStatementLine(cusStatementHeader, "", "123546789RM0002", "", "NS", "", "", new ZDateTime(2014, 11, 17), ZDateTime.Empty, ZDateTime.Empty, new ZDecimal[] { 0m, 0m, 0m, 0m, 0m, 25.00m, 0m });
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSendNotification()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "CA1";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "AIR";
			company.GC_OH_OrgProxy = org.PK;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "AAA";
			Factory.SaveForTesting();

			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, "JANETEST");
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "22333");

			var expectedHtml = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\DNMessageInterpretation.html");
			expectedHtml = expectedHtml.Replace("\r\n", "");

			var ediMessage = ARLDailyReportDocumentWrapperTest.CreateDailyNoticeMessageAndSetUpTestData(new BusinessObjectFactory(), branch: branch);
			ediMessage.EM_GB = branch.PK;
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			Factory.SaveForTesting();

			var newGroup1 = Factory.New<GlbGroup>();
			newGroup1.GG_Code = "NG1";
			var newStaff1 = newGroup1.Staff.AddNew();
			newStaff1.GS_Code = "NS1";
			newStaff1.GS_LoginName = "NS1";
			newStaff1.GS_EmailAddress = "ns1@cargowise.com";
			CACustomsDataRegistry.Instance.SendDeclarationMessageAcknowledgementsToGroup.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, newGroup1.PK.ToGuid());
			Factory.SaveForTesting();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var logger = new ServiceTaskLogForTesting();
				ProcessMessage(ediMessage, logger);

				var email = Env.OutgoingMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "Daily Notice for 17-Nov-14");
				AssertNotNull(email);
				AssertEquals("One recipient", 1, email.CCRecipients.Count);
				AssertEquals(User.PostMasterUserName, "ns1@cargowise.com", email.CCRecipients[0].Email);

				Env.OutgoingMailManager.EmailsCreated.Clear();
				var newGroup2 = Factory.New<GlbGroup>();
				newGroup2.GG_Code = "NG2";
				var newStaff2 = newGroup2.Staff.AddNew();
				newStaff2.GS_Code = "NS2";
				newStaff2.GS_LoginName = "NS2";
				newStaff2.GS_EmailAddress = "ns2@cargowise.com";
				CACustomsDataRegistry.Instance.SendK84ReportNotificationsToGroup.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, newGroup2.PK.ToGuid());
				Factory.SaveForTesting();

				ediMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch;
				ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
				ProcessMessage(ediMessage, logger);
				email = Env.OutgoingMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "Daily Notice for 17-Nov-14");
				AssertNotNull(email);
				AssertEquals("One recipient", 1, email.CCRecipients.Count);
				AssertEquals(User.PostMasterUserName, "ns2@cargowise.com", email.CCRecipients[0].Email);
				AssertContains("email.Body", @"<tr><td>Importer Code</td><td>IMPORTER1</td></tr><tr><td>Importer Name</td><td>GUY &amp; MIKE&#39;S IMPORTS INC.<br>(IMPORTER1 NAME)</td></tr><tr><td>Importer BN</td><td>123546789RM0001</td></tr>" +
					@"<tr><td>Importer Direct</td><td>Yes</td></tr><tr><td>GST Direct</td><td>No</td></tr>", email.Body);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				ediMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch;
				ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;

				ProcessMessage(ediMessage, logger);

				ProcessMessage(ediMessage, logger);
				email = Env.OutgoingMailManager.EmailsCreated.FirstOrDefault();
				AssertNotNull("Should not create any emails when the processor is in reprocessing mode.", email);

				Env.OutgoingMailManager.EmailsCreated.Clear();

				ediMessage.EM_MessageOwner = "CASOAReprocess";
				ediMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch;
				ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;

				ProcessMessage(ediMessage, logger);
				email = Env.OutgoingMailManager.EmailsCreated.FirstOrDefault();
				AssertNull("No email was created.", email);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcess_NoTransactions()
		{
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "22333");

			var ediMessage = ARLDailyReportDocumentWrapperTest.CreateDailyNoticeMessageAndSetUpTestData(new BusinessObjectFactory(), messageText: NoTransactionsMessageText);
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			Factory.SaveForTesting();

			var logger = new ServiceTaskLogForTesting();
			ProcessMessage(ediMessage, logger);
			ediMessage.Factory.Save();

			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, ediMessage.EM_Status);
			AssertContains("Importer Details", ediMessage.EM_MessageInterpretation);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessWithRMAccountNumber()
		{
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "22333");

			var ediMessage = ARLDailyReportDocumentWrapperTest.CreateDailyNoticeMessageAndSetUpTestData(new BusinessObjectFactory(), messageText: RMAccountNumberMessageText);
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			Factory.SaveForTesting();

			var logger = new ServiceTaskLogForTesting();
			ProcessMessage(ediMessage, logger);
			ediMessage.Factory.Save();

			var header = Factory.LoadTop1<CusStatementHeader>(new ZQuery(CusStatementHeaderSchema.B2_ImporterCustomsID, "123546789RM0001"));
			AssertNotNull("Should use a safe RM Account Number.", header);
		}

		CusStatementHeader AssertStatementHeader(BusinessObjectFactory factory, int linesCount, ZString importerCustomsId, ZString statementType, ZString statementNumber, ZString accountSecurityNumber, ZDateTime accountingDate, ZDateTime printDate, ZDecimal statementAmount, ZDecimal paidAmount, ZDecimal refundAmount)
		{
			var query = new ZQuery();
			query.AddToFilter(CusStatementHeaderSchema.B2_StatementType, statementType);
			query.AddToFilter(CusStatementHeaderSchema.B2_StatementNumber, statementNumber);
			query.AddToFilter(CusStatementHeaderSchema.B2_IsMonthlyStatement, false);

			var cusStatementHeader = factory.LoadTop1<CusStatementHeader>(query);
			AssertNotNull("Statement Header exists", cusStatementHeader);

			CombineAssertions(() =>
			{
				AssertEquals("StatementLines.Count", linesCount, cusStatementHeader.StatementLines.Count);

				AssertEquals("Statement Type", statementType, cusStatementHeader.B2_StatementType);
				AssertEquals("Statement Number", statementNumber, cusStatementHeader.B2_StatementNumber);
				AssertEquals("Importer Customs ID", importerCustomsId, cusStatementHeader.B2_ImporterCustomsID);
				AssertEquals("Account Security Number", accountSecurityNumber, cusStatementHeader.B2_EntryFilerCode);

				var expectedImporter = TransactionBatchExtension.GetOrgsFromBN(importerCustomsId, ZString.Empty, factory).FirstOrDefault()?.PK ?? ZGuid.Empty;
				AssertEquals("Importer", expectedImporter, cusStatementHeader.B2_OH_Importer);

				AssertEquals("Print Date", printDate, cusStatementHeader.B2_PrintDate);
				AssertEquals("Accounting Date", accountingDate, cusStatementHeader.B2_ProcessDate);

				AssertEquals("Statement Amount", statementAmount, cusStatementHeader.B2_StatementAmount);
				AssertEquals("Paid Amount", paidAmount, cusStatementHeader.B2_PaidAmount);
				AssertEquals("Refund Amount", refundAmount, cusStatementHeader.B2_RefundAmount);
			});

			return cusStatementHeader;
		}

		CusStatementLineGroup AssertStatementLineGroup(CusStatementHeader header, ZString importerCustomsId)
		{
			var group = header.LineGroupCollection.Find(c => c.B10_ImporterCustomsID == importerCustomsId).FirstOrDefault();
			AssertNotNull($"Should create a group which the customs ID is {importerCustomsId}.", group);

			return group;
		}

		CusStatementLine AssertStatementLine(CusStatementHeader header, ZString transactionNumber, ZString importerCustomsId, ZString jobReference, ZString entryType, ZString releaseOffice, ZString associatedEntry, ZDateTime entryDate, ZDateTime scheduledProcessDate, ZDateTime dueDate, ZDecimal[] charges)
		{
			var cusStatementLine = header.StatementLines.GetStatementLineFor(transactionNumber, entryType, string.Empty);

			AssertNotNull(transactionNumber + " line exists", cusStatementLine);

			CombineAssertions(() =>
			{
				AssertEquals("Job Number", jobReference, cusStatementLine.B3_BrokerReference);
				AssertEquals("Entry Type", entryType, cusStatementLine.B3_EntryType);
				AssertEquals("Transaction Number", transactionNumber, cusStatementLine.B3_EntryNum);
				AssertEquals("Release Office", releaseOffice, cusStatementLine.B3_EntryProcessPort);
				AssertEquals("Associated Entry", associatedEntry, cusStatementLine.B3_AssociatedEntry);
				AssertEquals("Importer Customs ID", importerCustomsId, cusStatementLine.B3_ImporterCustomsID);

				AssertEquals("Transaction Date", entryDate, cusStatementLine.B3_EntryDate);
				AssertEquals("Scheduled Process Date", scheduledProcessDate, cusStatementLine.B3_ScheduledProcessDate);
				AssertEquals("Due Date", dueDate, cusStatementLine.B3_DueDate);

				AssertEquals("Duty Amount", charges[0], cusStatementLine.Charges.GetAmountFor(EntryChargeTypeList.Codes.TotalDutyAmount));
				AssertEquals("SIMA Amount", charges[1], cusStatementLine.Charges.GetAmountFor(EntryChargeTypeList.Codes.TotalSIMAAmount));
				AssertEquals("Excist Tax Amount", charges[2], cusStatementLine.Charges.GetAmountFor(EntryChargeTypeList.Codes.TotalExciseTaxAmount));
				AssertEquals("GST Amount", charges[3], cusStatementLine.Charges.GetAmountFor(EntryChargeTypeList.Codes.TotalGSTAmount));
				AssertEquals("GST Direct Amount", charges[4], cusStatementLine.Charges.GetAmountFor(EntryChargeTypeList.Codes.TotalGSTDirectAmount));
				AssertEquals("Other Amount", charges[5], cusStatementLine.Charges.GetAmountFor(EntryChargeTypeList.Codes.Others));
				AssertEquals("K84 Late Filing Penalty", charges[6], cusStatementLine.Charges.GetAmountFor(EntryChargeTypeList.Codes.K84LateFilingPenalty));
			});

			return cusStatementLine;
		}

		void AssertPaymentParty(CusStatementLine cusStatementLine, ZString paymentParty, string chargeType)
		{
			AssertEquals(paymentParty, cusStatementLine.Charges.GetFirstCharge(chargeType).B4_PaymentParty);
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
			<Code>DN</Code>
			<Description>Daily Notices</Description>
		</BatchType>
		<OrganizationAddressCollection>
			<OrganizationAddress>
				<AddressType>ImportBroker</AddressType>
				<GovRegNum>123456789</GovRegNum>
				<CompanyName>GUY &amp; MIKE'S IMPORTS INC.</CompanyName>
			</OrganizationAddress>
		</OrganizationAddressCollection>
		<TransactionCollection>
			<Transaction>
				<Category>SUM</Category>
				<CreateTime>2014-11-18T00:00:00</CreateTime>
				<PostDate>2014-11-17T00:00:00</PostDate>
				<NumberOfSupportingDocuments>0001</NumberOfSupportingDocuments>
				<OrganizationAddress>
					<AddressType>ImportBroker</AddressType>
					<GovRegNum>123546789</GovRegNum>
					<CompanyName>GUY &amp; MIKE'S IMPORTS INC.12</CompanyName>
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
					<PostingJournal>
						<Description>TotalPaymentReceived</Description>
						<LocalAmount>0</LocalAmount>
					</PostingJournal>
					<PostingJournal>
						<Description>Refund</Description>
						<LocalAmount>0</LocalAmount>
					</PostingJournal>
				</PostingJournalCollection>
			</Transaction>
			<Transaction>
				<Category>SUM</Category>
				<OrganizationAddress>
					<AddressType>ImporterDocumentaryAddress</AddressType>
					<GovRegNum>123546789RM0001</GovRegNum>
					<CompanyName>GUY &amp; MIKE'S IMPORTS INC.</CompanyName>
				</OrganizationAddress>
				<PostingJournalCollection>
					<PostingJournal>
						<ChargeCode>
							<Code>DTY</Code>
							<Description>Duty</Description>
						</ChargeCode>
						<ChargeTotalAmount>135987.23</ChargeTotalAmount>
					</PostingJournal>
					<PostingJournal>
						<ChargeCode>
							<Code>GST</Code>
							<Description>GST</Description>
						</ChargeCode>
						<ChargeTotalAmount>25698.25</ChargeTotalAmount>
					</PostingJournal>
					<PostingJournal>
						<ChargeCode>
							<Code>OTH</Code>
							<Description>Others</Description>
						</ChargeCode>
						<ChargeTotalAmount>100</ChargeTotalAmount>
					</PostingJournal>
					<PostingJournal>
						<Description>TotalPaymentReceived</Description>
						<LocalAmount>-33300</LocalAmount>
					</PostingJournal>
					<PostingJournal>
						<Description>Refund</Description>
						<LocalAmount>0</LocalAmount>
					</PostingJournal>
				</PostingJournalCollection>
				<LocalTotal>161785.48</LocalTotal>
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
			<Code>DN</Code>
			<Description>Daily Notices</Description>
		</BatchType>
		<OrganizationAddressCollection>
			<OrganizationAddress>
				<AddressType>ImportBroker</AddressType>
				<GovRegNum>123456789</GovRegNum>
			</OrganizationAddress>
		</OrganizationAddressCollection>
		<TransactionCollection>
			<Transaction>
				<Category>SUM</Category>
				<Number>RM0001</Number>
				<CreateTime>2014-11-18T00:00:00</CreateTime>
				<PostDate>2014-11-17T00:00:00</PostDate>
				<OrganizationAddress>
					<AddressType>ImporterDocumentaryAddress</AddressType>
					<GovRegNum>123546789RM0001</GovRegNum>
					<CompanyName>GUY &amp; MIKE'S IMPORTS INC.</CompanyName>
				</OrganizationAddress>
				<PostingJournalCollection>
					<PostingJournal>
						<ChargeCode>
							<Code>DTY</Code>
							<Description>Duty</Description>
						</ChargeCode>
						<ChargeTotalAmount>135987.23</ChargeTotalAmount>
					</PostingJournal>
				</PostingJournalCollection>
				<LocalTotal>161785.48</LocalTotal>
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
