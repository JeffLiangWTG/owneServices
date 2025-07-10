using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.IE.ServiceTasks.Testing
{
	[TestedType(typeof(MessageSenderServiceTask))]
	public class MessageSenderServiceTaskTest : ServiceTaskTestCase<MessageSenderServiceTask>
	{
		public void TestServiceAttribute()
		{
			AssertSingleHostedServiceAttribute("IES", "IE Customs Message Sender", "IEC");
		}

		public void TestMinimumPeriod()
		{
			var hostedServiceAttribute = GetHostedServiceAttributes().Single();
			AssertEquals("MinimumPeriod", "60Seconds", hostedServiceAttribute.MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => new[]
		{
			new TaskNudgeInformationForTest(
				table: EDIMessageSchema.Constants.TableName,
				queueName: "IE Export Message Sender",
				EDIMessageSchema.Constants.EM_Status + "=PND",
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=TRX",
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=IEE",
				EDIMessageSchema.Constants.EM_ApplicationReference + "!="
			),
			new TaskNudgeInformationForTest(
				table: EDIMessageSchema.Constants.TableName,
				queueName: "IE Import Message Sender",
				EDIMessageSchema.Constants.EM_Status + "=PND",
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=TRX",
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=IEI",
				EDIMessageSchema.Constants.EM_ApplicationReference + "!="
			),
			new TaskNudgeInformationForTest(
				table: EDIMessageSchema.Constants.TableName,
				queueName: "IE Import Message UCC5 Sender",
				EDIMessageSchema.Constants.EM_Status + "=PND",
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=TRX",
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=IE5",
				EDIMessageSchema.Constants.EM_ApplicationReference + "!="
			),
			new TaskNudgeInformationForTest(
				table: EDIMessageSchema.Constants.TableName,
				queueName: "IE EMCS Message Sender",
				EDIMessageSchema.Constants.EM_Status + "=PND",
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=TRX",
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=IEM",
				EDIMessageSchema.Constants.EM_ApplicationReference + "!="
			),
			new TaskNudgeInformationForTest(
				table: EDIMessageSchema.Constants.TableName,
				queueName: "IE NCTS Message Sender",
				EDIMessageSchema.Constants.EM_Status + "=PND",
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=TRX",
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=IEN",
				EDIMessageSchema.Constants.EM_ApplicationReference + "!="
			),
			new TaskNudgeInformationForTest(
				table: EDIMessageSchema.Constants.TableName,
				queueName: "IE Customs and Excise Reports Message Sender",
				EDIMessageSchema.Constants.EM_Status + "=QUE",
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=TRX",
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=IER"
			),
			new TaskNudgeInformationForTest(
				table: EDIMessageSchema.Constants.TableName,
				queueName: "IE Customs PBN Message Sender",
				EDIMessageSchema.Constants.EM_Status + "=QUE",
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=TRX",
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=IEP"
			)
		};

		public void TestIEMessagesProcessed()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefSysConfigType("IESUBPCPBT", "TEST DESCRIPTION", "TEST LONG DESCRIPTION");
			helper.CreateRefSysConfig(
				"IESUBPCPBT",
				"https://www.ros.ie/customs/webservice/v1/rest/roro-control/pbn",
				ZDateTime.BrettsBirthday,
				ZDateTime.Empty
				);
			Factory.Save();
			helper.CreateRefSysConfigType("IEIEESUBMT", "IEE SUBMISSION DESCRIPTION", "IEE SUBMISSION LONG DESCRIPTION");
			helper.CreateRefSysConfigType("IEIEISUBMT", "IEI SUBMISSION DESCRIPTION", "IEE SUBMISSION LONG DESCRIPTION");
			helper.CreateRefSysConfigType("IEIE5SUBMT", "IE5 SUBMISSION DESCRIPTION", "IE5 SUBMISSION LONG DESCRIPTION");
			helper.CreateRefSysConfigType("IESUBM815T", "IEM SUBMISSION DESCRIPTION", "IEE SUBMISSION LONG DESCRIPTION");
			helper.CreateRefSysConfig("IEIEESUBMT", "https://www.ieetest.ie", ZDateTime.BrettsBirthday, ZDateTime.Empty);
			helper.CreateRefSysConfig("IEIEISUBMT", "https://www.ieitest.ie", ZDateTime.BrettsBirthday, ZDateTime.Empty);
			helper.CreateRefSysConfig("IEIE5SUBMT", "https://www.ie5test.ie", ZDateTime.BrettsBirthday, ZDateTime.Empty);
			helper.CreateRefSysConfig("IESUBM815T", "https://www.iemtest.ie", ZDateTime.BrettsBirthday, ZDateTime.Empty);
			var credential = Factory.NewWithValidTestData<EMCSGlbCompanyCredential>();
			Factory.Save();
			var registration = ObjectFactory.Get<IProductRegistration>();
			registration.KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;

			var msg1 = Factory.New<AESOutboundEDIMessage>();
			msg1.EM_ApplicationReference = "ABCDEF-12345678901234-XYZ";
			msg1.EM_ReceiveTransmit = "TRX";
			msg1.EM_Status = "PND";
			msg1.EM_ApplicationCode = "IEE";

			var msg2 = Factory.New<AISOutboundEDIMessage>();
			msg2.EM_ApplicationReference = "ABCDEF-12345678901234-ABC";
			msg2.EM_ReceiveTransmit = "TRX";
			msg2.EM_Status = "PND";
			msg2.EM_ApplicationCode = "IEI";

			var msg3 = Factory.New<EMCSOutboundMessageForTest>();
			msg3.EM_ApplicationReference = "ABCDEF-12345678901234-DEF";
			msg3.EM_ReceiveTransmit = "TRX";
			msg3.EM_Status = "PND";
			msg3.EM_ApplicationCode = "IEM";
			msg3.EM_MessageType = "815";
			msg3.EM_GP = credential.PK;

			var msg4 = Factory.New<AESOutboundEDIMessage>();
			msg4.EM_ApplicationReference = "HIJKLM-987654321234560-UVW";
			msg4.EM_ReceiveTransmit = "TRX";
			msg4.EM_Status = "PND";
			msg4.EM_ApplicationCode = "IEX";

			var msg5 = Factory.New<AISUCC5OutboundEDIMessage>();
			msg5.EM_ApplicationReference = "ABCDEF-12345678901234-555";
			msg5.EM_ReceiveTransmit = "TRX";
			msg5.EM_Status = "PND";
			msg5.EM_ApplicationCode = "IE5";

			var msg6 = Factory.New<CustomsAndExciseReportOutboundMessage>();
			msg6.EM_MessageType = "DSR";
			msg6.MessageDate = new ZDateTime(2024, 09, 06);

			var msg7 = Factory.New<PBNOutboundMessageForTest>();
			msg7.EM_ApplicationReference = "PBN-12345678901234-XYZ";
			msg7.EM_MessageType = "CPB";
			msg7.EM_ReceiveTransmit = "TRX";
			msg7.EM_Status = "QUE";
			msg7.EM_ApplicationCode = "IEP";
			Factory.Save();

			serviceTask.RunTask();

			msg1.Reload();
			AssertEquals("Msg1 (AES) EM_Status should be sent", EDIMessageStatusList.Codes.Sent, msg1.EM_Status);
			AssertEquals("Msg1 Interchange should be IEE", OutboundEDIMessage.ApplicationCodes.IECustomsExport, msg1.Interchange.EI_ApplicationCode);
			AssertContains("Msg1 Interchange should get correct endpoint", "https://www.ieetest.ie", msg1.Interchange.EI_HeaderText);
			msg2.Reload();
			AssertEquals("Msg2 (AIS) EM_Status should be sent", EDIMessageStatusList.Codes.Sent, msg2.EM_Status);
			AssertEquals("Msg2 Interchange should be IEI", OutboundEDIMessage.ApplicationCodes.IECustomsImport, msg2.Interchange.EI_ApplicationCode);
			AssertContains("Msg2 Interchange should get correct endpoint", "https://www.ieitest.ie", msg2.Interchange.EI_HeaderText);
			msg3.Reload();
			AssertEquals("Msg3 (EMCS) EM_Status should be sent", EDIMessageStatusList.Codes.Sent, msg3.EM_Status);
			AssertEquals("Msg3 Interchange should be IEM", OutboundEDIMessage.ApplicationCodes.IECustomsEMCS, msg3.Interchange.EI_ApplicationCode);
			AssertEquals("Msg3 Interchange.EI_GP should be equal to Message.EM_GP", credential.PK, msg3.Interchange.EI_GP);
			AssertContains("Msg3 Interchange should get correct endpoint", "https://www.iemtest.ie", msg3.Interchange.EI_HeaderText);
			msg4.Reload();
			AssertNotEquals("Msg4 (Incorrect Application Code) EM_Status should not be sent", EDIMessageStatusList.Codes.Sent, msg4.EM_Status);
			AssertNull("Msg4 (Incorrect Application Code) should have no interchange", msg4.Interchange);
			msg5.Reload();
			AssertEquals("Msg5 (AIS UCC5) EM_Status should be sent", EDIMessageStatusList.Codes.Sent, msg5.EM_Status);
			AssertEquals("Msg5 Interchange should be IE5", OutboundEDIMessage.ApplicationCodes.IECustomsUCC5Import, msg5.Interchange.EI_ApplicationCode);
			AssertContains("Msg5 Interchange should get correct endpoint", "https://www.ie5test.ie", msg5.Interchange.EI_HeaderText);
			msg6.Reload();
			AssertEquals("Msg6 (C&E) EM_Status should be sent", EDIMessageStatusList.Codes.Sent, msg6.EM_Status);
			AssertEquals("Msg6 Interchange should be IER", OutboundEDIMessage.ApplicationCodes.IECustomsAndExcise, msg6.Interchange.EI_ApplicationCode);
			AssertContains("Msg6 Interchange should get correct endpoint", "https://softwaretestnextversion.ros.ie/customs/webservice/v1/rest/transactions/daily/20240906/payer-summary-report", msg6.Interchange.EI_HeaderText);
			msg7.Reload();
			AssertEquals("Msg7 (PBN) EM_Status should be sent", EDIMessageStatusList.Codes.Sent, msg7.EM_Status);
			AssertEquals("Msg7 Interchange should be IEP", OutboundEDIMessage.ApplicationCodes.IECustomsPBN, msg7.Interchange.EI_ApplicationCode);
			AssertContains(
				"Msg7 Interchange should get correct endpoint",
				"{\"custom.IE.Endpoint\":\"https://www.ros.ie/customs/webservice/v1/rest/roro-control/pbn\",\"custom.IE.SigningOption\":\"Rest\"}",
				msg7.Interchange.EI_HeaderText
			);
		}

		public void TestIEEMCSMessageProcessedWhenCredentialNotFound()
		{
			var declaraion = Factory.New<EMCSJobDeclaration>();
			declaraion.JE_CustomsProfile = ZString.Empty;
			var msg = Factory.New<EMCSOutboundMessageForTest>();
			msg.EM_ApplicationReference = "ABCDEF-12345678901234-DEF";
			msg.EM_ReceiveTransmit = "TRX";
			msg.EM_Status = "PND";
			msg.EM_ApplicationCode = "IEM";
			msg.EM_MessageType = "815";
			msg.EM_LinkedObject = declaraion;
			msg.EM_GP = ZGuid.Empty;
			Factory.Save();
			serviceTask.RunTask();
			msg.Reload();
			AssertEquals("Msg EM_Status should be error", EDIMessageStatusList.Codes.Error, msg.EM_Status);
			AssertContains("Logger should log error information", $"Certificate Identifier of EMCS Declaration(PK:{declaraion.PK}) is blank.", logger.ToString());

			logger.ClearLog();
			declaraion.JE_CustomsProfile = "Invalid credential";
			msg.EM_Status = "PND";
			Factory.Save();
			serviceTask.RunTask();
			msg.Reload();
			AssertEquals("Msg EM_Status should be error", EDIMessageStatusList.Codes.Error, msg.EM_Status);
			AssertContains("Logger should log error information", $"No existing certificate matched for EMCS Declaration(PK:{declaraion.PK})'s Certificate Identifier.", logger.ToString());
		}

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestSettingEnviroment()
		{
			var msg1 = Factory.New<AESOutboundEDIMessage>();
			msg1.EM_ApplicationReference = "ABCDEF-12345678901234-XYZ";
			msg1.EM_ReceiveTransmit = "TRX";
			msg1.EM_Status = "PND";
			msg1.EM_ApplicationCode = "IEE";
			Factory.Save();

			var originUserContext = Env.CurrentUserContext;
			try
			{
				using (Env.Instance.TemporaryServiceTaskContext(ServiceTaskApplicationCodeList.Codes.IEMessageSender, canRunInAnyBranch: true))
				{
					serviceTask.RunTask();
				}
			}
			finally
			{
				Env.SetUserContext(originUserContext);
				AssertNullOrEmpty("Should not report \"Direct access to Env.CurrentBranch is not allowed\" message.", ErrorReporter.LastMessageReported);
			}
		}

		public void TestHostedServiceRequirement()
		{
			var methodInfo = typeof(MessageSenderServiceTask).GetMethod(nameof(MessageSenderServiceTask.IsRequired));
			Assert("HostedServiceRequirement is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));
		}

		internal void RunTask()
		{
			var task = new MessageSenderServiceTask();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);
		}

		public void SetUpTestClass()
		{
			SetUp();
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			serviceTask = new MessageSenderServiceTask();
			logger = new TestServiceLogger();
			serviceTask.ServiceLogger = logger;
		}
		TestServiceLogger logger;
		MessageSenderServiceTask serviceTask;

		sealed class EMCSOutboundMessageForTest : OutboundEDIMessage
		{
			public EMCSOutboundMessageForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override string GetMessageReferenceNumber() => EM_MessageNum;
		}

		sealed class PBNOutboundMessageForTest : OutboundEDIMessage
		{
			public PBNOutboundMessageForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override string GetMessageReferenceNumber() => EM_MessageNum;
		}
	}
}
