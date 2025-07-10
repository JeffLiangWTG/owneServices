using System.Collections.Generic;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.ServiceTasks.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.ServiceTasks.Testing
{
	[TestedType(typeof(MessageProcessorService))]
	sealed class MessageProcessorServiceTest : BaseMultiCompanyCustomsMessagingServiceTest<MessageProcessorService>
	{
		public void TestMessageProcessorServiceOverride()
		{
			AssertEquals(typeof(AUCMessageProcessor), testServiceTask.ProcessTypeForTesting);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"AU Customs CMR messages inbound",
						EDIMessageSchema.Constants.EM_IsActive        + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.CMR,
						EDIMessageSchema.Constants.EM_MessageType     + "!=CRS",
						EDIMessageSchema.Constants.EM_MessageType     + "!=CTL",
						EDIMessageSchema.Constants.EM_MessageType     + "!=SEI",
						EDIMessageSchema.Constants.EM_MessageType     + "!=URR",
						EDIMessageSchema.Constants.EM_MessageType     + "!=URE"),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"AU Customs PRA messages inbound",
						EDIMessageSchema.Constants.EM_IsActive        + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.OneStop),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"AU Customs ExDocs messages inbound",
						EDIMessageSchema.Constants.EM_IsActive        + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.EXDOC),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"AU Customs NexDocs messages inbound",
						EDIMessageSchema.Constants.EM_IsActive        + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.NEXDOCS),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"AU Customs COLS messages inbound",
						EDIMessageSchema.Constants.EM_IsActive        + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.COLS
						)
				};
			}
		}
	}
}
