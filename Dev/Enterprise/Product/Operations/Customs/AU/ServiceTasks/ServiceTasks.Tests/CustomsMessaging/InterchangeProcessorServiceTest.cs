using System.Collections.Generic;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.ServiceTasks.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.ServiceTasks.Testing
{
	[TestedType(typeof(InterchangeProcessorService))]
	sealed class InterchangeProcessorServiceTest : BaseMultiCompanyCustomsMessagingServiceTest<InterchangeProcessorService>
	{
		public void TestInterchangeProcessorServiceOverride()
		{
			AssertEquals(typeof(AUCInboundInterchangeProcessor), testServiceTask.ProcessTypeForTesting);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"AU Customs CMR interchanges inbound",
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIMessage.ApplicationCodes.CMR),

					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"AU Customs ExDocs interchanges inbound",
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIMessage.ApplicationCodes.EXDOC),

					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"AU Customs PRA interchanges inbound",
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIMessage.ApplicationCodes.OneStop),
				};
			}
		}
	}
}
