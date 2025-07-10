using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.ServiceTasks.TestDirectXt;
using Enterprise.Messaging.Business;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace ZClientEDI.Test.ServiceTasks.TestDirectXt
{
	[TestedType(typeof(TestDirectXtOutboundServiceTask))]
	class TestDirectXtOutboundServiceTaskTest : ServiceTaskTestCase<TestDirectXtOutboundServiceTask>
	{
		public void TestHostedServiceAttribute()
		{
			var attribute = typeof(TestDirectXtOutboundServiceTask).Assembly.GetCustomAttributes(true).OfType<HostedServiceAttribute>().Single(x => x.Code == TestDirectXtOutboundServiceTask.ServiceTaskCode);
			CombineAssertions(() =>
			{
				AssertEquals("Description", TestDirectXtOutboundServiceTask.ServiceTaskDescription, attribute.Description);
				AssertEquals("Category", "ESV", attribute.Category);
				AssertEquals("ConfigControlType", typeof(TestDirectXtOutboundServiceTask), attribute.Type);
				AssertEquals("AllowsMultipleInstances", false, attribute.AllowsMultipleInstances);
				AssertEquals("IsMandatory", true, attribute.IsMandatory);
				AssertEquals("CanRunInAnyBranch", true, attribute.CanRunInAnyBranch);
			});
		}

		public void TestRunTask()
		{
			var (interchangesShouldBeUpdated, interchangesShouldNotBeUpdated) = TestDirectXtOutboundProcessorTests.SetupDataForTesting(Factory);
			Factory.Save();

			var mockedMsgClientProvider = TestDirectXtTestUtils.GetMockedMsgClientProviderOutbound();

			var taskForTest = new TestDirectXtOutboundServiceTaskTestWrapper();
			taskForTest.ClientProvider = mockedMsgClientProvider;
			taskForTest.AttributeModifier = null;
			taskForTest.IsTest = true;

			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtTest.Code))
			{
				taskForTest.RunTask();
			}

			TestDirectXtOutboundProcessorTests.AssertEndToEndResult(interchangesShouldBeUpdated, interchangesShouldNotBeUpdated);
		}

		public void TestTaskIsDisabledInTest()
		{
			var methodInfo = typeof(TestDirectXtOutboundServiceTask).GetMethod(nameof(TestDirectXtOutboundServiceTask.CheckIsClientProductionDatabase));
			Assert("[HostedServiceRequirement] is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));

			var taskCheck = TestDirectXtOutboundServiceTask.CheckIsClientProductionDatabase();
			AssertEquals("This is not a production system.", taskCheck);
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("5minutes", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestIsProduction()
		{
			AssertEquals(false, new TestDirectXtOutboundServiceTaskTestWrapper().IsProduction());
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"xT Test Message Sending",
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Transmit,
						EDIInterchangeSchema.Constants.EI_TransportType + "=" + EDIInterchange.TransportType.tXT),
				};
			}
		}
	}
}
