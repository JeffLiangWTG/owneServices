using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.CDS.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.GB.CDS.ServiceTasks.Testing
{
	[TestedType(typeof(CDSMessageSenderServiceTask))]
	class CDSMessageSenderServiceTaskTests : ServiceTaskTestCase<CDSMessageSenderServiceTask>
	{
		[TestDate(2015, 10, 30, 09, 36, 0)]
		public void TestProcess()
		{
			CDSMessageSenderTestHelper.TestProcess(() =>
			{
				InitialiseAndRunTaskSchedule(new CDSMessageSenderServiceTask());
			});
		}

		public void TestCDSDISQueryMessagesProcessed()
		{
			var msg1 = Factory.New<CDSDISQueryMessage>();
			msg1.EM_ApplicationReference = "ABCDEF.12345678901234.XYZ";
			var msg2 = Factory.New<CDSDISQueryMessage>();
			msg2.EM_ApplicationReference = "HIJKLM.98765432123456.UVW";
			Factory.Save();

			InitialiseAndRunTaskSchedule(new CDSMessageSenderServiceTask());

			msg1.Reload();
			AssertEquals("Msg1 should be sent", EDIMessageStatusList.Codes.Sent, msg1.EM_Status);
			msg2.Reload();
			AssertEquals("Msg2 should be sent", EDIMessageStatusList.Codes.Sent, msg2.EM_Status);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return
				[
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"UK Customs CDS messages outbound CDS",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.GbCustomsDeclarationServices),
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"UK Customs CDS messages outbound CDS DIS",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.GbCDSDISQuery),
				];
			}
		}

		public void TestFailedMessagesInInterchange()
		{
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() =>
				{
					var msg1 = Factory.New<CDSEDIMessage>();
					msg1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
					msg1.EM_IsActive = true;
					msg1.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCustomsDeclarationServices;
					msg1.EM_Status = EDIMessageStatusList.Codes.Queued;
					msg1.EM_MessageType = "NIL";
					Factory.Save();
					AssertEquals("Pre-Requisite: EM_MessageOwner is blank, meaning we can't determine credentials, meaning EI_from is blank, which will cause a failure to package it", string.Empty, msg1.EM_MessageOwner);
					var logger = InitialiseAndRunTaskSchedule(new CDSMessageSenderServiceTask());
					AssertEquals("Logger Error Message", $"Error|Failed to create interchange for CDS Message #{msg1.EM_MessageNum} : There is no customs interchange sender id (EM_MessageOwner).", logger[0]);
					AssertEquals("Logger Warning Message", "Warning|No Interchange has been created", logger[1]);
					var reloadedMessage = new BusinessObjectFactory().Load<CDSEDIMessage>(msg1.PK);
					AssertEquals("Message has no Interchange ", ZGuid.Empty, msg1.EM_EI);
				});
			});
		}
	}
}
