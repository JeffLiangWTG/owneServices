using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Customs.KR.Messaging.Constants;
using EDIInterchange = Enterprise.Customs.KR.Business.EDIInterchange;
using EDIMessage = Enterprise.Customs.KR.Business.EDIMessage;

namespace Enterprise.Customs.KR.ServiceTasks.Testing
{
	[TestedType(typeof(InBoundServiceTask))]
	sealed class InBoundServiceTaskTest : ServiceTaskTestCase<InBoundServiceTask>
	{
		public void TestHostedServiceAttributeParameters()
		{
			HostedServiceAttribute hostedServiceAttribute = GetHostedServiceAttributes().Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "KRI", hostedServiceAttribute.Code);
				AssertEquals("Description", "KR Customs Message Processor", hostedServiceAttribute.Description);
				AssertEquals("Category", "KRC", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "1minute", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.KoreaSouth, hostedServiceAttribute.RequiresCompanyInCountry);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			});
		}

		[TestDate(2022, 08, 30)]
		public void TestRunTaskWithInterchanges()
		{
			var interchange1 = GetNewInterchange("첫번째 테스트 메시지", "0001");
			var interchange2 = GetNewInterchange("두번째 테스트 메시지", "0002");
			var unmatchingInterchange = GetUnmatchedInterchange();

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(interchange1.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "6N002");
			var password = Factory.NewWithValidTestData<GlbCompanyCredential>();
			password.GP_PasswordType = "KRB";
			password.GP_UserID = "3";
			password.GP_GC = interchange1.Company.PK;
			password.GP_Certificate = new byte[] { 48, 130, 8 };
			password.GP_IssueDate = new ZDateTime(2022, 1, 1);
			password.GP_ExpiryDate = new ZDateTime(2022, 12, 31);
			Factory.Save();

			var logs = InitialiseAndRunTaskSchedule(new InBoundServiceTask());
			interchange1.Reload();
			interchange2.Reload();
			unmatchingInterchange.Reload();

			CombineAssertions("Status", () =>
			{
				AssertEquals("Interchange1 Status", EDIInterchange.Status.Received, interchange1.EI_Status);
				AssertEquals("Interchange2 Status", EDIInterchange.Status.Received, interchange2.EI_Status);
				AssertEquals("Unmatched Interchange Status", EDIInterchange.Status.Cancelled, unmatchingInterchange.EI_Status);
				AssertEquals("Interchange1 Message Count", 1, interchange1.ContainedMessages.Count);
				AssertEquals("Interchange2 Message Count", 1, interchange2.ContainedMessages.Count);
				AssertEquals("Unmatching Interchange Message Count", 0, unmatchingInterchange.ContainedMessages.Count);
			});

			CombineAssertions("Logs", () =>
			{
				AssertEquals($"Log line1", true, logs[0].EndsWith("Interchange '0001' has been processed successfully."));
				AssertEquals($"Log line2", true, logs[1].EndsWith("Interchange '0002' has been processed successfully."));
			});

			CombineAssertions("Interchange1 Message", () =>
			{
				var createdMessage1 = interchange1.ContainedMessages[0];
				AssertEquals("EM_EI", interchange1.PK, createdMessage1.EM_EI);
				AssertEquals("EM_ApplicationCode", ApplicationCodeList.Codes.KRCustoms, createdMessage1.EM_ApplicationCode);
				AssertEquals("EM_MessageType", interchange1.EI_InterchangeType, createdMessage1.EM_MessageType);
				AssertNotEquals(ZString.Empty, createdMessage1.EM_MessageNum);
				AssertEquals("EM_MessageText", interchange1.EI_BodyText, createdMessage1.EM_MessageText);
				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Receive, createdMessage1.EM_ReceiveTransmit);
				AssertEquals("EM_Status", EDIMessage.Status.Queued, createdMessage1.EM_Status);
			});

			CombineAssertions("Interchange2 Message", () =>
			{
				var createdMessage2 = interchange2.ContainedMessages[0];
				AssertEquals("EM_EI", interchange2.PK, createdMessage2.EM_EI);
				AssertEquals("EM_ApplicationCode", ApplicationCodeList.Codes.KRCustoms, createdMessage2.EM_ApplicationCode);
				AssertEquals("EM_MessageType", interchange2.EI_InterchangeType, createdMessage2.EM_MessageType);
				AssertNotEquals(ZString.Empty, createdMessage2.EM_MessageNum);
				AssertEquals("EM_MessageText", interchange2.EI_BodyText, createdMessage2.EM_MessageText);
				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Receive, createdMessage2.EM_ReceiveTransmit);
				AssertEquals("EM_Status", EDIMessage.Status.Queued, createdMessage2.EM_Status);
			});
		}

		public void TestRunTaskWithInterchangesNotSetCompanyData()
		{
			var interchange1 = GetNewInterchange("첫번째 테스트 메시지", "0001");
			var interchange2 = GetNewInterchange("두번째 테스트 메시지", "0002");
			var unmatchingInterchange = GetUnmatchedInterchange();
			Factory.Save();

			var logs = InitialiseAndRunTaskSchedule(new InBoundServiceTask());
			interchange1.Reload();
			interchange2.Reload();
			unmatchingInterchange.Reload();

			CombineAssertions("Status", () =>
			{
				AssertEquals("Interchange1 Status", EDIInterchange.Status.Queued, interchange1.EI_Status);
				AssertEquals("Interchange2 Status", EDIInterchange.Status.Queued, interchange2.EI_Status);
				AssertEquals("Unmatched Interchange Status", EDIInterchange.Status.Cancelled, unmatchingInterchange.EI_Status);
				AssertEquals("Interchange1 Message Count", 0, interchange1.ContainedMessages.Count);
				AssertEquals("Interchange2 Message Count", 0, interchange2.ContainedMessages.Count);
				AssertEquals("Unmatching Interchange Message Count", 0, unmatchingInterchange.ContainedMessages.Count);
			});

			CombineAssertions("Logs", () =>
			{
				AssertEquals("Log Count", 0, logs.Count);
			});
		}

		[TestDate(2023, 05, 23)]
		public void TestRunTaskWithInterchangesMessageTypeSSR()
		{
			var interchange1 = GetNewInterchange("첫번째 테스트 메시지", "0001");
			interchange1.EI_InterchangeType = EDIInterchangeType.SSR;

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(interchange1.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "6N002");
			var password = Factory.NewWithValidTestData<GlbCompanyCredential>();
			password.GP_PasswordType = "KRB";
			password.GP_UserID = "3";
			password.GP_GC = interchange1.Company.PK;
			password.GP_Certificate = new byte[] { 48, 130, 8 };
			password.GP_IssueDate = new ZDateTime(2023, 1, 1);
			Factory.Save();
			InitialiseAndRunTaskSchedule(new InBoundServiceTask());
			AssertNotContains("Enterprise.Customs.KR.Business.MessageProcessorFactory did not update a received message's EM_Status away from QUE", ErrorReporter.LastMessageReported);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2022, 08, 30)]
		public void TestRunTaskWithIncomingMessages()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var entryHeader = GetTestEntry(jobDeclaration);
			var receivedMessage = GetIncomingMessage();

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(jobDeclaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "6N002");
			var password = Factory.NewWithValidTestData<GlbCompanyCredential>();
			password.GP_PasswordType = "KRB";
			password.GP_UserID = "3";
			password.GP_GC = jobDeclaration.RegistryCompanyPK;
			password.GP_Certificate = new byte[] { 48, 130, 8 };
			password.GP_IssueDate = new ZDateTime(2022, 1, 1);
			Factory.Save();

			var logs = InitialiseAndRunTaskSchedule(new InBoundServiceTask());

			receivedMessage.Reload();
			entryHeader.Reload();
			entryHeader.Messages.Reload(true);

			CombineAssertions("Logs", () =>
			{
				AssertEquals("Log Count", 3, logs.Count);
				AssertEquals($"Log line1", true, logs[0].EndsWith("Processing Message #123"));
				AssertEquals($"Log line2", true, logs[1].EndsWith("Saving..."));
				AssertEquals($"Log line3", true, logs[2].EndsWith("1 message processed"));
			});

			AssertEquals("Message status", EDIMessage.Status.Received, receivedMessage.EM_Status);
			AssertEquals("Messages count for header", 2, entryHeader.Messages.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRunTaskWithIncomingMessagesNotSetCompanyData()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var entryHeader = GetTestEntry(jobDeclaration);
			var receivedMessage = GetIncomingMessage();
			Factory.Save();

			var logs = InitialiseAndRunTaskSchedule(new InBoundServiceTask());

			receivedMessage.Reload();
			entryHeader.Reload();
			entryHeader.Messages.Reload(true);

			CombineAssertions("Logs", () =>
			{
				AssertEquals("Log Count", 0, logs.Count);
			});

			AssertEquals("Message status", EDIMessage.Status.Queued, receivedMessage.EM_Status);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"KR Customs Interchanges Inbound",
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIMessage.ApplicationCodes.KRCustoms),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"KR Customs Messages Inbound",
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.KRCustoms),
				};
			}
		}

		EDIInterchange GetNewInterchange(string body, string number)
		{
			var interchange1 = Factory.New<EDIInterchange>();
			interchange1.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange1.EI_Status = EDIInterchange.Status.Queued;
			interchange1.EI_ApplicationCode = EDIInterchange.ApplicationCodes.KRCustoms;
			interchange1.EI_InterchangeType = ElectronicDocumentTypeList.Codes._R20;
			interchange1.EI_InterchangeNum = number;
			interchange1.EI_From = "KR Customs";
			interchange1.EI_To = "TEST";
			interchange1.EI_BodyText = body;

			return interchange1;
		}

		EDIInterchange GetUnmatchedInterchange()
		{
			var unmatchingInterchange = Factory.New<EDIInterchange>();
			unmatchingInterchange.EI_Status = EDIInterchange.Status.Cancelled;
			unmatchingInterchange.EI_From = "KR Customs";
			unmatchingInterchange.EI_To = "TEST";

			return unmatchingInterchange;
		}

		CusEntryHeader GetTestEntry(JobDeclaration jobDeclaration)
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ORG";
			staff.GS_LoginName = "Origin";
			staff.GS_EmailAddress = "OriginalSender@wisetechglobal.com";

			var sentMessage = Factory.New<EDIMessage>();
			sentMessage.EM_MessageText = "Test Message Sent";
			sentMessage.EM_Status = EDIMessage.Status.Sent;
			sentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sentMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._830;
			sentMessage.EM_MessageNum = "123";
			sentMessage.EM_SystemCreateUser = "ORG";

			var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_Status = "AWO";
			entryHeader.Messages.Add(sentMessage);
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryNum = "4321013123456";
			entryNumber.CE_EntryType = "EXP";
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNumber.CE_ParentID = entryHeader.PK;
			entryNumber.CE_ParentTable = CusEntryHeader.Schema.TableName;

			return entryHeader;
		}

		EDIMessage GetIncomingMessage()
		{
			var receivedMessage = Factory.New<EDIMessage>();
			receivedMessage.EM_ApplicationCode = ApplicationCodeList.Codes.KRCustoms;
			receivedMessage.EM_Status = EDIMessage.Status.Queued;
			receivedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			receivedMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._R20;
			receivedMessage.EM_MessageNum = "123";

			var fileStream = File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\ServiceTasks.Test\TestFiles\GOVCBRR20_For830.xml"));
			receivedMessage.SetEM_MessageTextOrDataSource(new CargoWise.IO.Shim.SubStreamableStream(fileStream));

			return receivedMessage;
		}
	}
}
