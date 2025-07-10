using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using GlbCompanyWrapper = Enterprise.Customs.IE.Business.GlbCompanyWrapper;

namespace Enterprise.Customs.IE.ServiceTasks.Testing
{
	[TestedType(typeof(MessageRetrievingServiceTask))]
	class MessageRetrievingServiceTaskTest : ServiceTaskTestCase<MessageRetrievingServiceTask>
	{
		public void TestMinimumPeriod()
		{
			var hostedServiceAttribute = GetHostedServiceAttributes().Single();
			AssertEquals("MinimumPeriod", "60Seconds", hostedServiceAttribute.MinimumPeriod);
		}

		public void TestMailboxRequestIsCreated()
		{
			var helper = new WebServiceEndPointProviderTestHelper(Factory);
			var url = helper.SetupMailboxCollectURL();
			var ieData = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland);
			var wrapper = GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(ieData.company);
			var certificationData = wrapper.GlbExternalPassword;
			SetupOutgoingInterchange(ieData.branch.PK, certificationData.PK);
			Factory.Save();
			var task = new MessageRetrievingServiceTask();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);
			CombineAssertions(() =>
			{
				var query = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.IECustomsCommon);
				query.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, CommonInterchangeTypeList.Codes.MailboxRequest);
				query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
				query.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT);
				query.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Queued);
				query.AddToFilter(EDIInterchangeSchema.EI_GP, certificationData.PK);
				query.AddToFilter(EDIInterchangeSchema.EI_GB, ieData.branch.PK);
				var requestInterchange = Factory.Load<EDIInterchange>(query).Single();
				AssertEquals("requestInterchange.EI_HeaderText", $@"{{""custom.IE.Endpoint"":""{url}""}}", requestInterchange.EI_HeaderText);
				AssertNotEquals("requestInterchange.EI_SessionGUID", ZGuid.Empty, requestInterchange.EI_SessionGUID);
				AssertMultilineASCIIEquals("requestInterchange.EI_BodyText", $@"<crq:MailboxCollectRequest xmlns:crq=""http://www.ros.ie/schemas/customs/collectrequest/v1"" />", InterchangeProcessorTestHelper.FormatXml(requestInterchange.EI_BodyText));
			});
		}

		public void TestMailboxRequestIsCreatedForIECompanyWithMessagesInLast90Days()
		{
			var helper = new WebServiceEndPointProviderTestHelper(Factory);
			helper.SetupMailboxCollectURL();
			var ieData1 = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland);
			var wrapper1 = GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(ieData1.company);
			var ieCertificationData1 = wrapper1.GlbExternalPassword;
			var ieData2 = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "I2");
			var wrapper2 = GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(ieData2.company);
			var ieCertificationData2 = wrapper2.GlbExternalPassword;
			var auData = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Australia);
			var auCertificationData = Factory.New<Business.GlbCompanyCredential>();
			auCertificationData.GP_GC = auData.company.PK;
			var ieInterchange1 = SetupOutgoingInterchange(ieData1.branch.PK, ieCertificationData1.PK);
			ieInterchange1.EI_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-91);
			SetupOutgoingInterchange(ieData2.branch.PK, ieCertificationData2.PK);
			var auInterchange = SetupOutgoingInterchange(auData.branch.PK, auCertificationData.PK);
			auInterchange.EI_GP = auCertificationData.PK;
			Factory.Save();
			var task = new MessageRetrievingServiceTask();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);
			CombineAssertions(() =>
			{
				var query = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.IECustomsCommon);
				query.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, CommonInterchangeTypeList.Codes.MailboxRequest);
				query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
				query.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT);
				query.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Queued);
				var requestInterchanges = Factory.Load<EDIInterchange>(query);
				AssertEquals(1, requestInterchanges.Length);
				var requestInterchange = requestInterchanges[0];
				AssertEquals("requestInterchange.EI_GP", ieCertificationData2.PK, requestInterchange.EI_GP);
				AssertEquals("requestInterchange.EI_GB", ieData2.branch.PK, requestInterchange.EI_GB);
			});
		}

		public void TestOnlyIERTasksProcessed()
		{
			var interchange1 = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, EDIInterchange.ApplicationCodes.IECustomsCommon, CommonInterchangeTypeList.Codes.MailboxAcknowledge, string.Empty);
			var interchange2 = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, EDIInterchange.ApplicationCodes.IECustomsCommon, CommonInterchangeTypeList.Codes.MailboxRequest, string.Empty);
			var interchange3 = InterchangeProcessorTestHelper.CreateMessageAcknowledgementResponse(Factory, "09276E86-1C63-4DFA-B915-JJ82EBC90967");
			var interchange4 = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, EDIInterchange.ApplicationCodes.IECustomsCommon, CommonInterchangeTypeList.Codes.TransactionID, string.Empty);
			Factory.Save();
			var task = new MessageRetrievingServiceTask();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);
			CombineAssertions(() =>
			{
				var anotherFactory = new BusinessObjectFactory();
				interchange1 = anotherFactory.Load<EDIInterchange>(interchange1.PK);
				interchange2 = anotherFactory.Load<EDIInterchange>(interchange2.PK);
				interchange3 = anotherFactory.Load<EDIInterchange>(interchange2.PK);
				interchange4 = anotherFactory.Load<EDIInterchange>(interchange4.PK);
				AssertNotEquals("interchange1.EI_Status - MBA", EDIInterchange.Status.Queued, interchange1.EI_Status);
				AssertNotEquals("interchange2.EI_Status - MBA", EDIInterchange.Status.Queued, interchange2.EI_Status);
				AssertNotEquals("interchange3.EI_Status - ACK", EDIInterchange.Status.Queued, interchange3.EI_Status);
				AssertEquals("interchange3.EI_Status - TID - should be ignored", EDIInterchange.Status.Queued, interchange4.EI_Status);
			});
		}

		public void TestProcessAllEIEDIInterchange()
		{
			var ieData = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland);

			var interchange1 = AISInterchangeProcessorTestHelper.CreateStandardIM917Interchange(Factory, "07109E86-1C63-4DFA-B915-FA10EBC91633", "A19F4EB6-CB9E-4DD8-8A34-7C55CBB54C44");
			interchange1.EI_GB = ieData.branch.PK;
			Factory.Save();
			var task = new MessageRetrievingServiceTask();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);
			CombineAssertions(() =>
			{
				var anotherFactory = new BusinessObjectFactory();
				interchange1 = anotherFactory.Load<EDIInterchange>(interchange1.PK);
				AssertEquals("interchange1.EI_Status", EDIInterchange.Status.Received, interchange1.EI_Status);
			});
		}

		public void TestServiceAttribute()
		{
			AssertSingleHostedServiceAttribute("IER", "IE Customs Message Retriever", "IEC");
		}

		public void TestHostedServiceRequirement()
		{
			var methodInfo = typeof(MessageRetrievingServiceTask).GetMethod(nameof(MessageRetrievingServiceTask.IsRequired));
			Assert("HostedServiceRequirement is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));
		}

		internal void RunTask()
		{
			var task = new MessageRetrievingServiceTask();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);
		}

		EDIInterchange SetupOutgoingInterchange(ZGuid branchPK, ZGuid certificationPK)
		{
			const string messageText = "<GREETING>HELLO</GREETING>";
			var outgoingInterchange = InterchangeCreator.CreateOutgoingInterchange(Factory, EDIInterchange.ApplicationCodes.IECustomsExport, AESOutgoingMessageTypeList.Codes.ArrivalAtExit, branchPK, messageText, "https://www.where.com", credentialPK: certificationPK);
			outgoingInterchange.EI_Status = EDIInterchange.Status.Sent;
			return outgoingInterchange;
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => new[]
		{
			new TaskNudgeInformationForTest(
				table: EDIInterchangeSchema.Constants.TableName,
				queueName: "IE Customs Mailbox Request",
				EDIInterchangeSchema.Constants.EI_Status + "=QUE",
				EDIInterchangeSchema.Constants.EI_ApplicationCode + "=IEC",
				EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
				EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=RCV",
				EDIInterchangeSchema.Constants.EI_TransportType + "=XTT",
				EDIInterchangeSchema.Constants.EI_InterchangeType + "=MBR"
			),
			new TaskNudgeInformationForTest(
				table: EDIInterchangeSchema.Constants.TableName,
				queueName: "IE Customs Mailbox Acknowledge",
				EDIInterchangeSchema.Constants.EI_Status + "=QUE",
				EDIInterchangeSchema.Constants.EI_ApplicationCode + "=IEC",
				EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
				EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=RCV",
				EDIInterchangeSchema.Constants.EI_TransportType + "=XTT",
				EDIInterchangeSchema.Constants.EI_InterchangeType + "=MBA"
			),
			new TaskNudgeInformationForTest(
				table: EDIInterchangeSchema.Constants.TableName,
				queueName: "IE Customs Export Message Acknowledgement",
				EDIInterchangeSchema.Constants.EI_Status + "=QUE",
				EDIInterchangeSchema.Constants.EI_ApplicationCode + "=IEE",
				EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
				EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=RCV",
				EDIInterchangeSchema.Constants.EI_TransportType + "=XTT"
			),
			new TaskNudgeInformationForTest(
				table: EDIInterchangeSchema.Constants.TableName,
				queueName: "IE Customs Import Message Acknowledgement",
				EDIInterchangeSchema.Constants.EI_Status + "=QUE",
				EDIInterchangeSchema.Constants.EI_ApplicationCode + "=IEI",
				EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
				EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=RCV",
				EDIInterchangeSchema.Constants.EI_TransportType + "=XTT"
			),
			new TaskNudgeInformationForTest(
				table: EDIInterchangeSchema.Constants.TableName,
				queueName: "IE Customs Import UCC5 Message Acknowledgement",
				EDIInterchangeSchema.Constants.EI_Status + "=QUE",
				EDIInterchangeSchema.Constants.EI_ApplicationCode + "=IE5",
				EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
				EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=RCV",
				EDIInterchangeSchema.Constants.EI_TransportType + "=XTT"
			),
			new TaskNudgeInformationForTest(
				table: EDIInterchangeSchema.Constants.TableName,
				queueName: "IE Customs EMCS Message",
				EDIInterchangeSchema.Constants.EI_Status + "=QUE",
				EDIInterchangeSchema.Constants.EI_ApplicationCode + "=IEM",
				EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
				EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=RCV",
				EDIInterchangeSchema.Constants.EI_TransportType + "=XTT",
				EDIInterchangeSchema.Constants.EI_InterchangeType + "!=TID"
			),
			new TaskNudgeInformationForTest(
				table: EDIInterchangeSchema.Constants.TableName,
				queueName: "IE Customs NCTS Message Acknowledgement",
				EDIInterchangeSchema.Constants.EI_Status + "=QUE",
				EDIInterchangeSchema.Constants.EI_ApplicationCode + "=IEN",
				EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
				EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=RCV",
				EDIInterchangeSchema.Constants.EI_TransportType + "=XTT"
			),
			new TaskNudgeInformationForTest(
				table: EDIInterchangeSchema.Constants.TableName,
				queueName: "IE Customs And Excise Report Message",
				EDIInterchangeSchema.Constants.EI_Status + "=QUE",
				EDIInterchangeSchema.Constants.EI_ApplicationCode + "=IER",
				EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
				EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=RCV",
				EDIInterchangeSchema.Constants.EI_TransportType + "=XTT"
			),
			new TaskNudgeInformationForTest(
				table: EDIInterchangeSchema.Constants.TableName,
				queueName: "IE Customs PBN Message",
				EDIInterchangeSchema.Constants.EI_Status + "=QUE",
				EDIInterchangeSchema.Constants.EI_ApplicationCode + "=IEP",
				EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
				EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=RCV",
				EDIInterchangeSchema.Constants.EI_TransportType + "=XTT"
			),
		};

		public void SetUpTestClass()
		{
			SetUp();
		}
	}
}
