using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Client.EDI.ServiceTasks.TestDirectXt;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using ZClientEDI.Test.ServiceTasks.TestDirectXt;

namespace EDIServiceTask.Test
{
	[TestedType(typeof(TestDirectXtOutboundServiceTask))]
	public class EDIServiceTaskTest : ServiceTaskTestCase<TestDirectXtOutboundServiceTask>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestCanRunInAnyBranch()
		{
			var initialUserContext = Env.CurrentUserContext;

			try
			{
				var testInterchange = TestDirectXtTestUtils.CreateInterchange(Factory, EDIMessage.Status.Queued, true, EDIMessage.Direction.Transmit, EDIInterchange.TransportType.tXT);
				Factory.Save();

				var mockedMsgClientProvider = TestDirectXtTestUtils.GetMockedMsgClientProviderOutbound();

				var taskForTest = new TestDirectXtOutboundServiceTaskTestWrapper();
				taskForTest.ClientProvider = mockedMsgClientProvider;
				taskForTest.AttributeModifier = null;

				using (Env.Instance.TemporaryServiceTaskContext(TestDirectXtOutboundServiceTask.ServiceTaskCode, canRunInAnyBranch: true))
				{
					using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtTest.Code))
					{
						taskForTest.RunTask();
					}
				}
				testInterchange.Reload();
				AssertEquals(EDIMessage.Status.Sent, testInterchange.EI_Status);
			}
			finally
			{
				Env.SetUserContext(initialUserContext);
				AssertEquals("No Errors", false, ErrorReporter.LastMessageReported.Contains("Direct access to Env.CurrentBranch is not allowed from service tasks"));
			}
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("5minutes", GetHostedServiceAttributes().Single().MinimumPeriod);
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
