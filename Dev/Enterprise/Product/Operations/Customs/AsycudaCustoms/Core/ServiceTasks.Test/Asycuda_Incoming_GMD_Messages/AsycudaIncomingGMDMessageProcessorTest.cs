using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.ServiceTasks.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.ServiceTasks.Testing
{
	[TestedType(typeof(AsycudaGMDCustomsMessagingServiceTask))]
	class AsycudaIncomingGMDMessageProcessorTest : GMDCustomsMessagingServiceTest<AsycudaGMDCustomsMessagingServiceTask>
	{
		public void TestProcessing_of_Incoming_Asycuda_GMD_Messages()
		{
			var helper = new AsycudaIncomingGmdMessageTestScenarioHelper();
			helper.CreateUserAccounts();

			var scenarios = new List<AsycudaIncomingGmdMessageTestScenario>
			{
				helper.CreateTestScenario_Empty_Body(), helper.CreateTestScenario_No_Declaration_Reference(),
				helper.CreateTestScenario_Declaration_Not_Found(),
				helper.CreateTestScenario_Success_with_CusAgent(),
				helper.CreateTestScenario_Success_without_CusAgent(),
				helper.CreateTestScenario_Success_No_Identation_Body()
			};

			helper.SaveAll();
			InitialiseAndRunTaskSchedule(new AsycudaGMDCustomsMessagingServiceTask());

			foreach (var testScenario in scenarios)
			{
				helper.CheckScenario(testScenario);
			}
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("1minute", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"Asycuda GMD interchanges inbound",
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIMessage.ApplicationCodes.GenericMessageDelivery,
						EDIInterchangeSchema.Constants.EI_InterchangeType + "=ZZA"),
				};
			}
		}

		protected override AsycudaGMDCustomsMessagingServiceTask CreateServiceTask() => new AsycudaGMDCustomsMessagingServiceTask();

		protected override GMDCustomsMessagingServiceTestHelperData SetupDataForTesting()
		{
			var helper = new AsycudaIncomingGmdMessageTestScenarioHelper();
			helper.CreateUserAccounts();
			var scenario = helper.CreateTestScenario_Success_with_CusAgent();
			helper.SaveAll();
			return new GMDCustomsMessagingServiceTestHelperData() { InterchangePK = scenario.InterchangePK };
		}
	}
}
