using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.xTMessaging.Shared.Test.TestUtils;

namespace Enterprise.xTMessaging.ServiceTasks.Test
{
	[TestedType(typeof(OutboundServiceTask))]
	class OutboundServiceTaskTest : ServiceTaskTestCase<OutboundServiceTask>
	{
		public void TestHostedServiceAttribute()
		{
			var attribute = typeof(OutboundServiceTask).Assembly.GetCustomAttributes(true).OfType<HostedServiceAttribute>().Single(x => x.Code == ServiceTaskCodeList.Codes.XTO);
			CombineAssertions(() =>
			{
				AssertEquals("Description", ServiceTaskCodeList.Descriptions.XTO, attribute.Description);
				AssertEquals("Category", "ESV", attribute.Category);
				AssertEquals("ConfigControlType", typeof(OutboundServiceTask), attribute.Type);
				AssertEquals("AllowsMultipleInstances", false, attribute.AllowsMultipleInstances);
				AssertEquals("IsMandatory", true, attribute.IsMandatory);
				AssertEquals("CanRunInAnyBranch", true, attribute.CanRunInAnyBranch);
				AssertEquals("DefaultScheduleRunEvery", "15minutes", attribute.DefaultScheduleRunEvery);
			});
		}

		public void TestRunTask()
		{
			var (interchangesShouldBeUpdated, interchangesShouldNotBeUpdated) = OutboundInterchangeProcessorTest.SetupDataForTesting(Factory);
			Factory.Save();

			var mockedMsgClientProvider = GetMoqMsgClientProvider();

			var taskForTest = new OutboundServiceTaskTestWrapper();
			taskForTest.ClientProvider = mockedMsgClientProvider;
			taskForTest.AttributeModifier = null;

			InsertRefSysConfig(Factory);

			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtTest.Code))
			{
				taskForTest.RunTask();
			}

			OutboundInterchangeProcessorTest.AssertEndToEndResult(interchangesShouldBeUpdated, interchangesShouldNotBeUpdated);
		}

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestCanRunInAnyBranch()
		{
			var initialUserContext = Env.CurrentUserContext;
			try
			{
				var testInterchange = CreateInterchangeForXT(Factory);
				Factory.Save();

				var mockedMsgClientProvider = GetMoqMsgClientProvider();

				var taskForTest = new OutboundServiceTaskTestWrapper();
				taskForTest.ClientProvider = mockedMsgClientProvider;
				taskForTest.AttributeModifier = null;

				InsertRefSysConfig(Factory);

				using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
				using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtTest.Code))
				{
					taskForTest.RunTask();
				}

				testInterchange.Reload();
				AssertEquals(EDIMessageStatusList.Codes.Sent, testInterchange.EI_Status);
			}
			finally
			{
				Env.SetUserContext(initialUserContext);
				AssertEquals("No Errors", false, ErrorReporter.LastMessageReported.Contains("Direct access to Env.CurrentBranch is not allowed from service tasks"));
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
						"xT Message Sending",
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIMessageStatusList.Codes.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Transmit,
						EDIInterchangeSchema.Constants.EI_TransportType + "=" + EDIInterchangeTransportTypeList.Codes.xT),
				};
			}
		}
	}
}
