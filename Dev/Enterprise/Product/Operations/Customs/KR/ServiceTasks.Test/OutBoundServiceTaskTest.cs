using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using Constants = Enterprise.Customs.KR.Messaging.Constants;
using EDIMessage = Enterprise.Customs.KR.Business.EDIMessage;

namespace Enterprise.Customs.KR.ServiceTasks.Testing
{
	[TestedType(typeof(OutBoundServiceTask))]
	sealed class OutBoundServiceTaskTest : ServiceTaskTestCase<OutBoundServiceTask>
	{
		public void TestHostedServiceAttributeParameters()
		{
			HostedServiceAttribute hostedServiceAttribute = GetHostedServiceAttributes().Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "KRO", hostedServiceAttribute.Code);
				AssertEquals("Description", "KR Customs Message Sender", hostedServiceAttribute.Description);
				AssertEquals("Category", "KRC", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "1minute", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.KoreaSouth, hostedServiceAttribute.RequiresCompanyInCountry);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			});
		}

		[TestDate(2022, 11, 1)]
		public void TestRunTaskWithOutgoingMessages()
		{
			var message1 = TestTransmitMessage;
			var message2 = TestTransmitMessage;
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "6N002");
			SetupCompanyCredentials(Factory);
			SetupCustomsRegistryCertificate();
			Factory.Save();

			CombineAssertions("PreCondition", () =>
			{
				AssertNull("Message1 Message Interchange", message1.Interchange);
				AssertNull("Message2 Message Interchange", message2.Interchange);
			});

			var logs = InitialiseAndRunTaskSchedule(new OutBoundServiceTask());

			message1.Reload();
			message2.Reload();

			CombineAssertions("Status", () =>
			{
				AssertEquals("Message1 Status", EDIMessage.Status.Sent, message1.EM_Status);
				AssertEquals("Message2 Status", EDIMessage.Status.Sent, message2.EM_Status);
				AssertNotNull("Message1 Interchange", message1.Interchange);
				AssertNotNull("Message2 Interchange", message2.Interchange);
				AssertNotEquals("Must create one interchange for each message (no collation) => Message1.Interchange should not be the same as Message2.Interchange", message1.EM_EI, message2.EM_EI);
			});

			CombineAssertions("Logs", () =>
			{
				AssertEquals("Log Count", 1, logs.Count);
				AssertEquals($"Log line1", true, logs[0].EndsWith("2 message(s) have been processed."));
			});
		}

		public void TestRunTaskWithOutgoingMessagesNotSetCompanyData()
		{
			var message = TestTransmitMessage;
			Factory.Save();

			CombineAssertions("PreCondition", () =>
			{
				AssertNull("Message1 Message Interchange", message.Interchange);
			});

			var logs = InitialiseAndRunTaskSchedule(new OutBoundServiceTask());

			CombineAssertions("Logs", () =>
			{
				AssertEquals("Log Count", 0, logs.Count);
			});
		}

		public void TestNoCustomsPublicKey()
		{
			var message = TestTransmitMessage;
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "6N002");
			SetupCompanyCredentials(Factory);
			Factory.Save();

			var logs = InitialiseAndRunTaskSchedule(new OutBoundServiceTask());

			CombineAssertions("Logs", () =>
			{
				AssertEquals("Log Count", 1, logs.Count);
				AssertEquals($"Log line1", true, logs[0].EndsWith("KR Customs public key has never been set. Please check if the service task 'KRP' is up and running. This service task stops now."));
			});
		}

		[TestDate(2022, 11, 1)]
		public void TestCustomsPublicKeyNotUpdated()
		{
			var message = TestTransmitMessage;
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "6N002");
			SetupCompanyCredentials(Factory);
			SetupCustomsRegistryCertificate();
			Factory.Save();

			var logs = InitialiseAndRunTaskSchedule(new OutBoundServiceTask());

			CombineAssertions("Logs", () =>
			{
				AssertEquals("Log Count", 1, logs.Count);
				AssertEquals($"Log line2", true, logs[0].EndsWith("1 message(s) have been processed."));
			});
		}

		[TestDate(2023, 04, 30)]
		public void TestCusPollingTransactionToEDIMessage()
		{
			var message1 = Factory.New<EDIMessage>();
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_Status = EDIMessage.Status.Received;
			message1.EM_MessageNum = "01";
			message1.EM_MessageText = "첫번째 테스트 메시지";

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "6N002");
			SetupCompanyCredentials(Factory);
			SetupCustomsRegistryCertificate();

			var queryGUID = "2020040909570320200409-ELI-edb54e29-d2aa-49ea-849c-87f29bbf1f42";
			var cusPollingTransaction = Factory.New<CusPollingTransaction>();
			cusPollingTransaction.CPT_ApplicationCode = "KRC";
			cusPollingTransaction.CPT_Reference = ElectronicDocumentTypeList.Codes._830;
			cusPollingTransaction.CPT_Status = CusPollingTransactionStatusList.Codes.Opened;
			cusPollingTransaction.CPT_NumberOfAttempts = 1;
			cusPollingTransaction.CPT_TransactionID = queryGUID;
			cusPollingTransaction.CPT_ParentTableCode = message1.TablePrefix;
			cusPollingTransaction.CPT_ParentID = message1.PK;
			Factory.Save();

			var logs = InitialiseAndRunTaskSchedule(new OutBoundServiceTask());
			cusPollingTransaction.Reload();

			CombineAssertions("Logs", () =>
			{
				AssertEquals("Log Count", 1, logs.Count);
				AssertEquals($"Log line1", true, logs[0].EndsWith("DOC message(s) has been created successfully."));
			});

			var factory = new BusinessObjectFactory();
			var query = new ZQuery(EDIMessageSchema.EM_LinkTable, "cusPollingTransaction");
			query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, cusPollingTransaction.PK);
			var message = factory.Load<EDIMessage>(query);

			AssertEquals(CusPollingTransactionStatusList.Codes.Closed, cusPollingTransaction.CPT_Status);
			AssertEquals(1, message.Length);
			AssertEquals(queryGUID, MessageEncoding.UTF8WithoutBOM.GetString(message[0].EM_MessageData));
			AssertEquals(Constants.EDIInterchangeType.DOC, message[0].EM_MessageType);
			AssertEquals(EDIMessage.Status.Queued, message[0].EM_Status);
			AssertEquals(EDIMessage.Direction.Transmit, message[0].EM_ReceiveTransmit);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"KR Customs Messages Outbound",
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.KRCustoms),

					new TaskNudgeInformationForTest(
						CusPollingTransactionSchema.Constants.TableName,
						"KR CusPollingTransaction Outbound",
						CusPollingTransactionSchema.Constants.CPT_Status + "=" + CusPollingTransactionStatusList.Codes.Opened,
						CusPollingTransactionSchema.Constants.CPT_ApplicationCode + "=" + EDIMessage.ApplicationCodes.KRCustoms),
				};
			}
		}

		internal static void SetupCompanyCredentials(BusinessObjectFactory factory)
		{
			var password = factory.NewWithValidTestData<GlbCompanyCredential>();
			password.GP_PasswordType = "KRB";
			password.GP_UserID = "3";
			password.GP_GC = GlbCompany.CurrentCompany.PK;
			var messageData = new TestFileReader(typeof(OutBoundServiceTaskTest)).GetEmbeddedFileData(TestFilesPath, "12840.pfx");
			password.GP_Certificate = messageData;
			password.CurrentDecryptedCertificatePassphrase = "rk34879800!";
			password.GP_IssueDate = new ZDateTime(2022, 1, 1);
			password.GP_ExpiryDate = ZDateTime.MaxSmallDateTime;
		}

		internal static void SetupCustomsRegistryCertificate()
		{
			KRCustomsRegistry.Instance.CustomsCertificate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CertificateForTest);
		}

		internal static readonly byte[] CertificateForTest = MessageEncoding.UTF8WithoutBOM.GetBytes(PublicKeyUpdateServiceTaskTest.PublicKey);

		EDIMessage TestTransmitMessage
		{
			get
			{
				var message = Factory.New<EDIMessage>();
				message.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message.EM_Status = EDIMessage.Status.Queued;
				message.EM_MessageType = ElectronicDocumentTypeList.Codes._830;
				message.EM_MessageNum = "01";
				var stream = typeof(OutBoundServiceTaskTest).Assembly.GetManifestResourceStream($"{TestFilesPath}.GOVCBR830.xml");
				message.SetEM_MessageTextOrDataSource(stream);
				return message;
			}
		}

		[TestDate(2022, 11, 1)]
		public void TestRunOutgoingMessagesCreateInterChange()
		{
			var message1 = TestTransmitMessage;
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "6N002");

			SetupCompanyCredentials(Factory);
			SetupCustomsRegistryCertificate();
			Factory.Save();

			AssertNull("Message1 Message Interchange", message1.Interchange);

			InitialiseAndRunTaskSchedule(new OutBoundServiceTask());
			message1.Reload();

			AssertNotNullOrEmpty(MessageEncoding.UTF8WithoutBOM.GetString(message1.Interchange.EI_BodyData));

			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			AssertEquals(EDIInterchangeTransportTypeList.Codes.xT, message1.Interchange.EI_TransportType);
			AssertEquals(string.Empty, message1.Interchange.EI_FooterNText);
			AssertEquals(registrationKey.EnterpriseCode + registrationKey.ServerCode, message1.Interchange.EI_From);
		}

		const string TestFilesPath = "Enterprise.Customs.KR.ServiceTasks.Testing.TestFiles";
	}
}
