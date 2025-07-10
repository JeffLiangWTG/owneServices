using System.Collections.Generic;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.ServiceTasks.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.ServiceTasks.Testing
{
	[TestedType(typeof(CRSMessageProcessorService))]
	sealed class CRSMessageProcessorServiceTest : BaseMultiCompanyCustomsMessagingServiceTest<CRSMessageProcessorService>
	{
		public void TestMessageProcessorServiceOverride()
		{
			AssertEquals(typeof(AUCCRSMessageProcessor), testServiceTask.ProcessTypeForTesting);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"AU Customs CMR CRS messages inbound",
						EDIMessageSchema.Constants.EM_IsActive        + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.CMR,
						EDIMessageSchema.Constants.EM_MessageType     + "=CRS"),
				};
			}
		}
	}
}
