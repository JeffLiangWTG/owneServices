using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Customs.ES.Business.ESConstants;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business.MessageSending.Testing
{
	public class InboxListRequestSenderTest : TestCaseWithFactory
	{
		[TestDate(2020, 1, 9, 15, 13, 23, 456)]
		public void TestRemoveExpiredCusPollingTransactions()
		{
			var correctTransaction1 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now, "AAA");
			var correctTransaction2 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-5), "BBB");
			var correctTransaction3 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-3), "CCC");

			var expiredTransaction1 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-19), "DDD");
			var expiredTransaction2 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-8), "EEE");
			var expiredTransaction3 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-10), "FFF");

			var incorrectApplicationCodeTransaction = AddCusPollingTransaction("TRC", "OPN", ZDateTime.Now.AddDays(-10), "GGG", type: "T2O");
			var incorrectStatusTransaction = AddCusPollingTransaction("ESC", "CLS", ZDateTime.Now.AddDays(-10), "HHH");

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Before running the service task there are 7 ESC Polling Transactions", 7, GetCusPollingTransactions().Length);
				AssertEquals("Before running the service task there is 1 TRC Polling Transactions", 1, GetCusPollingTransactions("TRC").Length);

				var logger = new TestServiceLogger();
				new InboxListRequestSender(logger).RemoveExpiredCusPollingTransactions(Factory);

				AssertEquals("After running the service task there are only 4 ESC Polling Transactions, the OPN & expired ones have been deleted", 4, GetCusPollingTransactions().Length);
				AssertEquals("After running the service task there is still 1 TRC Polling Transactions", 1, GetCusPollingTransactions("TRC").Length);

				AssertTransactionStatus("correctTransaction1's status was not changed because it is not expired", "OPN", correctTransaction1);
				AssertTransactionStatus("correctTransaction2's status was not changed because it is not expired", "OPN", correctTransaction2);
				AssertTransactionStatus("correctTransaction3's status was not changed because it is not expired", "OPN", correctTransaction3);
				AssertTransactionStatus("incorrectApplicationCodeTransaction's status was not changed because it is expired but the ApplicationCode is not correct", "OPN", incorrectApplicationCodeTransaction);
				AssertTransactionStatus("incorrectStatusTransaction's status was not changed because it is already CLS", "CLS", incorrectStatusTransaction);

				AssertEquals("Log Count", 4, logger.Count);

				var expectedLogs = new StringBuilder();
				expectedLogs.AppendLine("Information|Removed 3 Polling Transactions");
				expectedLogs.AppendLine("Information|Polling Transaction removed with Type: RES, Transaction ID: DDD, System Create Time (universal time): 21-Dec-19 15:13");
				expectedLogs.AppendLine("Information|Polling Transaction removed with Type: RES, Transaction ID: FFF, System Create Time (universal time): 30-Dec-19 15:13");
				expectedLogs.AppendLine("Information|Polling Transaction removed with Type: RES, Transaction ID: EEE, System Create Time (universal time): 01-Jan-20 15:13");

				AssertEquals("Deleted transactions have been recorded in the log", expectedLogs.ToString(), logger.ToString());
			});
		}

		[TestDate(2020, 1, 9, 15, 13, 23, 456)]
		public void TestRemoveExpiredCusPollingTransactions_SetExpirationDays()
		{
			AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now, "AAA");
			AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-7), "BBB");
			AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-3), "CCC");
			AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-19), "DDD");
			AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-15), "EEE");
			AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-10), "FFF");

			Factory.Save();

			using (RegistryTemporarySetterHelper.SetInboxXTExpirationPeriodDays(10))
			{
				CombineAssertions(() =>
				{
					AssertEquals("Before running the service task there are 6 ESC Polling Transactions", 6, GetCusPollingTransactions().Length);

					var logger = new TestServiceLogger();
					new InboxListRequestSender(logger).RemoveExpiredCusPollingTransactions(Factory);

					AssertEquals("After running the service task there are only 4 ESC Polling Transactions, the OPN & expired ones have been deleted", 4, GetCusPollingTransactions().Length);

					AssertEquals("Log Count", 3, logger.Count);

					var expectedLogs = new StringBuilder();
					expectedLogs.AppendLine("Information|Removed 2 Polling Transactions");
					expectedLogs.AppendLine("Information|Polling Transaction removed with Type: RES, Transaction ID: DDD, System Create Time (universal time): 21-Dec-19 15:13");
					expectedLogs.AppendLine("Information|Polling Transaction removed with Type: RES, Transaction ID: EEE, System Create Time (universal time): 25-Dec-19 15:13");

					AssertEquals("Deleted transactions have been recorded in the log", expectedLogs.ToString(), logger.ToString());
				});
			}
		}

		public void TestCreateListPollingEDIMessages_NotxT()
		{
			using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(false))
			{
				var staffCode = "AZ";
				var staff = SetUpStaffWithCertificate(staffCode, "CERTNAME");
				var declaration = SetUpDeclarationWithDeclarant(staffCode, SetUpDeclarant());
				var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();

				var correctTransaction1 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now, "AAA", bo: entryHeader);

				Factory.Save();

				CombineAssertions(() =>
				{
					AssertNoExceptionThrown("No Exception when logger is null", () => new InboxListRequestSender(null).CreateListPollingEDIMessages(Factory));

					var logger = new TestServiceLogger();
					new InboxListRequestSender(logger).CreateListPollingEDIMessages(Factory);

					AssertTransactionStatus("correctTransaction1's status was not changed because registry item (sent inbox through xt) is false", "OPN", correctTransaction1);

					ZQuery messagesQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.ESCustomsMessage);
					messagesQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
					EDIMessage[] newRequestMessages = Factory.Load<EDIMessage>(messagesQuery);

					AssertEquals("No messages created", false, newRequestMessages.Any());

					AssertEquals("Log Count", 1, logger.Count);

					var expectedLogs = new StringBuilder();
					expectedLogs.AppendLine("Information|Created 0 EDI Messages");

					AssertEquals("Functionality has been recorded in the log", expectedLogs.ToString(), logger.ToString());
				});
			}
		}

		public void TestCreateListPollingEDIMessages_xT_SingleTypeCertificate()
		{
			using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(true))
			{
				var staffCode = "AZ";
				var certName = "CERTNAME";
				var staff = SetUpStaffWithCertificate(staffCode, certName);
				var declaration = SetUpDeclarationWithDeclarant(staffCode, SetUpDeclarant());
				var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();

				var correctTransaction1 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now, "AAA", bo: entryHeader);
				var correctTransaction2 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-5), "BBB", bo: entryHeader);
				var correctTransaction3 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-3), "CCC", bo: entryHeader);

				var incorrectApplicationCodeTransaction = AddCusPollingTransaction("TRC", "OPN", ZDateTime.Now.AddDays(-10), "DDD", type: "T2O", bo: entryHeader);
				var incorrectStatusTransaction = AddCusPollingTransaction("ESC", "CLS", ZDateTime.Now.AddDays(-10), "EEE", bo: entryHeader);

				Factory.Save();

				CombineAssertions(() =>
				{
					var logger = new TestServiceLogger();
					new InboxListRequestSender(logger).CreateListPollingEDIMessages(Factory);

					AssertTransactionStatus("correctTransaction1's status was changed to PND", "PND", correctTransaction1);
					AssertTransactionStatus("correctTransaction2's status was changed to PND", "PND", correctTransaction2);
					AssertTransactionStatus("correctTransaction3's status was changed to PND", "PND", correctTransaction3);
					AssertTransactionStatus("incorrectApplicationCodeTransaction's status was not changed because ApplicationCode is not correct", "OPN", incorrectApplicationCodeTransaction);
					AssertTransactionStatus("incorrectStatusTransaction's status was not changed because it is already CLS", "CLS", incorrectStatusTransaction);

					AssertNewInboxMessagesPerCertificateAndIsTest("For certName and not test", new ZString[] { InboxNotificationResponseTypes.AESExitResult }, certName);

					AssertEquals("Log Count", 2, logger.Count);

					var expectedLogs = new StringBuilder();
					expectedLogs.AppendLine("Information|Created 1 EDI Message");
					expectedLogs.AppendLine("Information|EDI message created with Message Type: LDI, Receive Transmit: TRX, Is Test Message: N for Polling Transaction.Type: RES and Certificate: CERTNAME");

					AssertEquals("Functionality has been recorded in the log", expectedLogs.ToString(), logger.ToString());
				});
			}
		}

		public void TestCreateListPollingEDIMessages_xT_MultipleTypeCertificate()
		{
			using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(true))
			{
				var staffCode1 = "AZ1";
				var certName1 = "CERTNAME1";
				var staff1 = SetUpStaffWithCertificate(staffCode1, certName1);
				var declarant = SetUpDeclarant();

				var declaration1 = SetUpDeclarationWithDeclarant(staffCode1, declarant);
				var entryHeader1 = (CusEntryHeader)declaration1.ActiveEntryHeaders.AddNew();
				var entryHeader2 = (CusEntryHeader)declaration1.ActiveEntryHeaders.AddNew();

				var correctTransaction1 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now, "AAA", bo: entryHeader1);
				var correctTransaction2 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-5), "BBB", bo: entryHeader1);
				var correctTransaction3 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now, "CCC", type: DeclarationMessageTypeList.Codes.InBoxNotificationForExport, bo: entryHeader1);
				var correctTransaction4 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-5), "DDD", type: DeclarationMessageTypeList.Codes.InBoxNotificationForExport, bo: entryHeader1);
				var correctTransaction5 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-3), "EEE", bo: entryHeader2);
				var correctTransaction6 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-5), "FFF", type: DeclarationMessageTypeList.Codes.InboxNotificationForDvdH2, bo: entryHeader2);

				var incorrectApplicationCodeTransaction = AddCusPollingTransaction("TRC", "OPN", ZDateTime.Now.AddDays(-10), "GGG", type: "T2O", bo: entryHeader1);
				var incorrectStatusTransaction = AddCusPollingTransaction("ESC", "CLS", ZDateTime.Now.AddDays(-10), "HHH", bo: entryHeader1);

				var staffCode2 = "AZ2";
				var certName2 = "CERTNAME2";
				var staff2 = SetUpStaffWithCertificate(staffCode2, certName2);
				var declaration2 = SetUpDeclarationWithDeclarant(staffCode2, declarant);
				var entryHeader3 = (CusEntryHeader)declaration2.ActiveEntryHeaders.AddNew();

				var correctTransaction7 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now, "AAA", bo: entryHeader3);
				var correctTransaction8 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-5), "BBB", bo: entryHeader3);

				var declaration3 = SetUpDeclarationWithDeclarant(staffCode1, declarant, true);
				var entryHeader4 = (CusEntryHeader)declaration3.ActiveEntryHeaders.AddNew();

				var correctTransaction9 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now, "AAA", bo: entryHeader4);
				var correctTransaction10 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-5), "BBB", bo: entryHeader4);

				var declaration4 = SetUpDeclarationWithDeclarant(staffCode1, declarant);
				var entryHeader5 = (CusEntryHeader)declaration4.ActiveEntryHeaders.AddNew();

				var correctTransaction11 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now, "AAA", bo: entryHeader5);

				Factory.Save();

				CombineAssertions(() =>
				{
					var logger = new TestServiceLogger();
					new InboxListRequestSender(logger).CreateListPollingEDIMessages(Factory);

					AssertTransactionStatus("correctTransaction1's status was changed to PND", "PND", correctTransaction1);
					AssertTransactionStatus("correctTransaction2's status was changed to PND", "PND", correctTransaction2);
					AssertTransactionStatus("correctTransaction3's status was changed to PND", "PND", correctTransaction3);
					AssertTransactionStatus("correctTransaction4's status was changed to PND", "PND", correctTransaction4);
					AssertTransactionStatus("correctTransaction5's status was changed to PND", "PND", correctTransaction5);
					AssertTransactionStatus("correctTransaction6's status was changed to PND", "PND", correctTransaction6);
					AssertTransactionStatus("incorrectApplicationCodeTransaction's status was not changed because ApplicationCode is not correct", "OPN", incorrectApplicationCodeTransaction);
					AssertTransactionStatus("incorrectStatusTransaction's status was not changed because it is already CLS", "CLS", incorrectStatusTransaction);
					AssertTransactionStatus("correctTransaction7's status was changed to PND", "PND", correctTransaction7);
					AssertTransactionStatus("correctTransaction8's status was changed to PND", "PND", correctTransaction8);
					AssertTransactionStatus("correctTransaction9's status was changed to PND", "PND", correctTransaction9);
					AssertTransactionStatus("correctTransaction10's status was changed to PND", "PND", correctTransaction10);
					AssertTransactionStatus("correctTransaction11's status was changed to PND", "PND", correctTransaction11);

					var expectedUrlsCert1 = new ZString[] { InboxNotificationResponseTypes.AESExitResult,
															InboxNotificationResponseTypes.Export,
															InboxNotificationResponseTypes.DVD };
					AssertNewInboxMessagesPerCertificateAndIsTest("For certName1 and not test", expectedUrlsCert1, certName1);

					AssertNewInboxMessagesPerCertificateAndIsTest("For certName2 and not test", new ZString[] { InboxNotificationResponseTypes.AESExitResult }, certName2);

					AssertNewInboxMessagesPerCertificateAndIsTest("For certName1 and test", new ZString[] { InboxNotificationResponseTypes.AESExitResult }, certName1, true);

					AssertEquals("Log Count", 6, logger.Count);

					var expectedLogs = new StringBuilder();
					expectedLogs.AppendLine("Information|Created 5 EDI Messages");
					expectedLogs.AppendLine("Information|EDI message created with Message Type: LDI, Receive Transmit: TRX, Is Test Message: N for Polling Transaction.Type: RES and Certificate: CERTNAME1");
					expectedLogs.AppendLine("Information|EDI message created with Message Type: LDI, Receive Transmit: TRX, Is Test Message: N for Polling Transaction.Type: NPE and Certificate: CERTNAME1");
					expectedLogs.AppendLine("Information|EDI message created with Message Type: LDI, Receive Transmit: TRX, Is Test Message: N for Polling Transaction.Type: NPD and Certificate: CERTNAME1");
					expectedLogs.AppendLine("Information|EDI message created with Message Type: LDI, Receive Transmit: TRX, Is Test Message: N for Polling Transaction.Type: RES and Certificate: CERTNAME2");
					expectedLogs.AppendLine("Information|EDI message created with Message Type: LDI, Receive Transmit: TRX, Is Test Message: Y for Polling Transaction.Type: RES and Certificate: CERTNAME1");

					AssertEquals("Functionality has been recorded in the log", expectedLogs.ToString(), logger.ToString());
				});
			}
		}

		public void TestCreateListPollingEDIMessages_xT_GivenTypes()
		{
			using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(true))
			{
				var staffCode1 = "AZ1";
				var certName1 = "CERTNAME1";
				var staff1 = SetUpStaffWithCertificate(staffCode1, certName1);
				var declarant = SetUpDeclarant();

				var declaration1 = SetUpDeclarationWithDeclarant(staffCode1, declarant);
				var entryHeader1 = (CusEntryHeader)declaration1.ActiveEntryHeaders.AddNew();
				var entryHeader2 = (CusEntryHeader)declaration1.ActiveEntryHeaders.AddNew();

				var correctTransaction1 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now, "AAA", bo: entryHeader1);
				var correctTransaction2 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-5), "BBB", bo: entryHeader1);
				var correctTransaction3 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now, "CCC", type: DeclarationMessageTypeList.Codes.InBoxNotificationForExport, bo: entryHeader1);
				var correctTransaction4 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-5), "DDD", type: DeclarationMessageTypeList.Codes.InBoxNotificationForExport, bo: entryHeader1);
				var correctTransaction5 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-3), "EEE", bo: entryHeader2);
				var correctTransaction6 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-5), "FFF", type: DeclarationMessageTypeList.Codes.InboxNotificationForDvdH2, bo: entryHeader2);

				var incorrectApplicationCodeTransaction = AddCusPollingTransaction("TRC", "OPN", ZDateTime.Now.AddDays(-10), "GGG", type: "T2O", bo: entryHeader1);
				var incorrectStatusTransaction = AddCusPollingTransaction("ESC", "CLS", ZDateTime.Now.AddDays(-10), "HHH", bo: entryHeader1);

				var staffCode2 = "AZ2";
				var certName2 = "CERTNAME2";
				var staff2 = SetUpStaffWithCertificate(staffCode2, certName2);
				var declaration2 = SetUpDeclarationWithDeclarant(staffCode2, declarant);
				var entryHeader3 = (CusEntryHeader)declaration2.ActiveEntryHeaders.AddNew();

				var correctTransaction7 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now, "AAA", bo: entryHeader3);
				var correctTransaction8 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-5), "BBB", bo: entryHeader3);

				var declaration3 = SetUpDeclarationWithDeclarant(staffCode1, declarant, true);
				var entryHeader4 = (CusEntryHeader)declaration3.ActiveEntryHeaders.AddNew();

				var correctTransaction9 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now, "AAA", bo: entryHeader4);
				var correctTransaction10 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-5), "BBB", bo: entryHeader4);

				var declaration4 = SetUpDeclarationWithDeclarant(staffCode1, declarant);
				var entryHeader5 = (CusEntryHeader)declaration4.ActiveEntryHeaders.AddNew();

				var correctTransaction11 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now, "AAA", bo: entryHeader5);

				Factory.Save();

				CombineAssertions(() =>
				{
					var logger = new TestServiceLogger();
					new InboxListRequestSender(logger).CreateListPollingEDIMessages(Factory, new ZString[] { DeclarationMessageTypeList.Codes.InBoxNotificationForExport, DeclarationMessageTypeList.Codes.InboxNotificationForDvdH2 });

					correctTransaction1.Reload();
					AssertEquals("correctTransaction1's status was not changed because type is not NPE or NPD", "OPN", correctTransaction1.CPT_Status);

					correctTransaction2.Reload();
					AssertEquals("correctTransaction2's status was not changed because type is not NPE or NPD", "OPN", correctTransaction2.CPT_Status);

					correctTransaction3.Reload();
					AssertEquals("correctTransaction3's status was changed to PND because type is NPE", "PND", correctTransaction3.CPT_Status);

					correctTransaction4.Reload();
					AssertEquals("correctTransaction4's status was changed to PND because type is NPE", "PND", correctTransaction4.CPT_Status);

					correctTransaction5.Reload();
					AssertEquals("correctTransaction5's status was not changed because type is not NPE or NPD", "OPN", correctTransaction5.CPT_Status);

					correctTransaction6.Reload();
					AssertEquals("correctTransaction6's status was changed to PND because type is NPD", "PND", correctTransaction6.CPT_Status);

					incorrectApplicationCodeTransaction.Reload();
					AssertEquals("incorrectApplicationCodeTransaction's status was not changed because ApplicationCode is not correct", "OPN", incorrectApplicationCodeTransaction.CPT_Status);

					incorrectStatusTransaction.Reload();
					AssertEquals("incorrectStatusTransaction's status was not changed because it is already CLS", "CLS", incorrectStatusTransaction.CPT_Status);

					correctTransaction7.Reload();
					AssertEquals("correctTransaction7's status was not changed because type is not NPE or NPD", "OPN", correctTransaction7.CPT_Status);

					correctTransaction8.Reload();
					AssertEquals("correctTransaction8's status was not changed because type is not NPE or NPD", "OPN", correctTransaction8.CPT_Status);

					correctTransaction9.Reload();
					AssertEquals("correctTransaction9's status was not changed because type is not NPE or NPD", "OPN", correctTransaction9.CPT_Status);

					correctTransaction10.Reload();
					AssertEquals("correctTransaction10's status wasnot changed because type is not NPE or NPD", "OPN", correctTransaction10.CPT_Status);

					correctTransaction11.Reload();
					AssertEquals("correctTransaction11's status was not changed because type is not NPE or NPD", "OPN", correctTransaction11.CPT_Status);

					var expectedUrlsCert1 = new ZString[] { InboxNotificationResponseTypes.Export,
															InboxNotificationResponseTypes.DVD, };
					AssertNewInboxMessagesPerCertificateAndIsTest("For certName1 and not test", expectedUrlsCert1, certName1);

					AssertEquals("Log Count", 3, logger.Count);

					var expectedLogs = new StringBuilder();
					expectedLogs.AppendLine("Information|Created 2 EDI Messages");
					expectedLogs.AppendLine("Information|EDI message created with Message Type: LDI, Receive Transmit: TRX, Is Test Message: N for Polling Transaction.Type: NPE and Certificate: CERTNAME1");
					expectedLogs.AppendLine("Information|EDI message created with Message Type: LDI, Receive Transmit: TRX, Is Test Message: N for Polling Transaction.Type: NPD and Certificate: CERTNAME1");

					AssertEquals("Functionality has been recorded in the log", expectedLogs.ToString(), logger.ToString());
				});
			}
		}

		void AssertTransactionStatus(ZString assertMessage, ZString expectedStatus, CusPollingTransaction transaction)
		{
			transaction.Reload();
			AssertEquals(assertMessage, expectedStatus, transaction.CPT_Status);
		}

		void AssertNewInboxMessagesPerCertificateAndIsTest(ZString assertMessage, ZString[] urls, ZString certName, bool isTest = false)
		{
			ZQuery messagesQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.ESCustomsMessage);
			messagesQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			messagesQuery.AddToFilter(EDIMessageSchema.EM_ApplicationReference, certName);
			messagesQuery.AddToFilter(EDIMessageSchema.EM_IsTestMessage, isTest);
			EDIMessage[] newRequestMessages = NewFactory().Load<EDIMessage>(messagesQuery);

			AssertEquals(assertMessage + " messages count is correct", urls.Length, newRequestMessages.Length);

			var newRequestMessagesText = new List<ZString>();
			foreach (EDIMessage message in newRequestMessages)
			{
				AssertEquals(assertMessage + " " + message.EM_MessageType + " message.EM_ApplicationCode", ApplicationCodeList.Codes.ESCustomsMessage, message.EM_ApplicationCode);
				AssertEquals(assertMessage + " " + message.EM_MessageType + " message.EM_MessageType", DeclarationMessageTypeList.Codes.InboxPendingList, message.EM_MessageType);
				AssertEquals(assertMessage + " " + message.EM_MessageType + " message.EM_MessageSubType", DeclarationMessageSubTypeList.Codes.OriginalDeclaration, message.EM_MessageSubType);
				AssertEquals(assertMessage + " " + message.EM_MessageType + " message.EM_IsTestMessage", isTest, message.EM_IsTestMessage);
				AssertEquals(assertMessage + " " + message.EM_MessageType + " message.EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals(assertMessage + " " + message.EM_MessageType + " message.EM_Status", EDIMessage.Status.Queued, message.EM_Status);
				AssertEquals(assertMessage + " " + message.EM_MessageType + " message.EM_ApplicationReference", certName, message.EM_ApplicationReference);
				AssertNull(assertMessage + " " + message.EM_MessageType + " message doesn't have interchange", message.Interchange);

				var url = urls.FirstOrDefault(x => message.EM_MessageText.Contains(x));
				AssertMultilineASCIIEquals(assertMessage + " " + message.EM_MessageType + " message.EM_MessageText", GetExpectedNewInboxMessageBodyText(url), message.EM_MessageText);
			}
		}

		ZString GetExpectedNewInboxMessageBodyText(ZString url) => ZString.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
<ListaDecV4Ent tipoRespuesta=""{0}"" xmlns=""https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adht/band/ws/li/ListaDecV4Ent.xsd"">
  <declarante>
    <NifDeclarante>{1}</NifDeclarante>
    <NombreDeclarante>{2}</NombreDeclarante>
  </declarante>
</ListaDecV4Ent>
  </soapenv:Body>
</soapenv:Envelope>", url, DeclarantId, DeclarantName);

		JobDeclaration SetUpDeclarationWithDeclarant(ZString staffCode, OrgHeader declarant, bool isTest = false)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			declaration.JE_GS_NKCusAgent = staffCode;
			declaration.ZG_IsTrainingDeclaration = isTest;

			declaration.Declarant.OA_OH = declarant.PK;
			return declaration;
		}

		GlbStaff SetUpStaffWithCertificate(ZString staffCode, ZString certName)
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = staffCode;
			staff.GS_LoginName = staffCode + "test";
			var wrapper = ES.Business.GlbStaffWrapper.Get(staff);
			var cert = wrapper.ESBPasswordCollection.AddNew();
			cert.GP_Name = certName;
			cert.GP_MailBoxID = "Test";
			cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			return staff;
		}

		OrgHeader SetUpDeclarant()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_FullName = DeclarantName;
			declarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, DeclarantId);
			return declarant;
		}

		const string DeclarantId = "NIF22222222";
		const string DeclarantName = "Declarant Full Name";

		CusPollingTransaction AddCusPollingTransaction(ZString applicationCode, ZString status, ZDateTime createTime, ZString transactionID, string type = DeclarationMessageTypeList.Codes.ExportExitResultCommunication, BusinessObject bo = null)
		{
			var transaction = Factory.NewWithValidTestData<CusPollingTransaction>();
			transaction.CPT_ApplicationCode = applicationCode;
			transaction.CPT_Status = status;
			transaction.CPT_SystemCreateTimeUtc = createTime;
			transaction.CPT_TransactionID = transactionID;
			transaction.CPT_Type = type;
			transaction.CPT_NumberOfAttempts = 1;
			if (bo != null)
			{
				transaction.CPT_ParentID = bo.PK;
				transaction.CPT_ParentTableCode = bo.TablePrefix;
			}
			return transaction;
		}

		CusPollingTransaction[] GetCusPollingTransactions(string code = Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ESCustomsMessage)
		{
			var query = new ZQuery(CusPollingTransactionSchema.CPT_ApplicationCode, code);
			return NewFactory().Load<CusPollingTransaction>(query);
		}
	}
}
