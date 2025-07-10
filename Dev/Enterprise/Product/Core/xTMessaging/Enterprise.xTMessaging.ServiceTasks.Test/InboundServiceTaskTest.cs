using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using Enterprise.Integration.Licensing;
using Enterprise.xTMessaging.Business;
using Enterprise.xTMessaging.Shared.Test;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.xTMessaging.Shared.Test.TestUtils;

namespace Enterprise.xTMessaging.ServiceTasks.Test
{
	[TestedType(typeof(InboundServiceTask))]
	class InboundServiceTaskTest : ServiceTaskTestCase<InboundServiceTask>
	{
		public void TestHostedServiceAttribute()
		{
			var attribute = typeof(InboundServiceTask).Assembly.GetCustomAttributes(true).OfType<HostedServiceAttribute>().Single(x => x.Code == ServiceTaskCodeList.Codes.XTI);
			CombineAssertions(() =>
			{
				AssertEquals("Description", ServiceTaskCodeList.Descriptions.XTI, attribute.Description);
				AssertEquals("Category", "ESV", attribute.Category);
				AssertEquals("ConfigControlType", typeof(InboundServiceTask), attribute.Type);
				AssertEquals("AllowsMultipleInstances", false, attribute.AllowsMultipleInstances);
				AssertEquals("IsMandatory", true, attribute.IsMandatory);
				AssertEquals("CanRunInAnyBranch", true, attribute.CanRunInAnyBranch);
				AssertEquals("DefaultScheduleRunEvery", "1minute", attribute.DefaultScheduleRunEvery);
			});
		}

		public void TestRunTask()
		{
			(var mockedMsgClientProvider, _) = TestUtils.GetMockedMsgClientProviderWithIncomingMessages(new Dictionary<ulong, ITestIncomingMessage>());

			var taskForTest = new InboundServiceTaskTestWrapper();
			taskForTest.ClientProvider = mockedMsgClientProvider;

			InsertRefSysConfig(Factory);

			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtTest.Code))
			{
				AssertNoExceptionThrown(() => taskForTest.RunTask());
			}
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("1minute", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestHostedServiceNudgedIsApplied()
		{
			using (MockProductRegistration(DatabaseTypes.Codes.Production, isInternalSystem: false, hostedLocation: "NCW"))
			{
				AssertEquals(false, InboundServiceTask.CheckIsNudged());
			}

			using (MockProductRegistration(DatabaseTypes.Codes.Production, isInternalSystem: false))
			{
				using (DirectxTMessagingRegistry.Instance.EnableXTINudge.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertEquals(false, InboundServiceTask.CheckIsNudged());
				}

				using (DirectxTMessagingRegistry.Instance.EnableXTINudge.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertEquals(true, InboundServiceTask.CheckIsNudged());
				}
			}

			using (MockProductRegistration(DatabaseTypes.Codes.Production, isInternalSystem: true))
			{
				AssertEquals(false, InboundServiceTask.CheckIsNudged());
			}

			using (MockProductRegistration(DatabaseTypes.Codes.Test, isInternalSystem: false))
			{
				using (DirectxTMessagingRegistry.Instance.EnableXTINudge.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertEquals(false, InboundServiceTask.CheckIsNudged());
				}
			}

			using (MockProductRegistration(DatabaseTypes.Codes.Training, isInternalSystem: false))
			{
				using (DirectxTMessagingRegistry.Instance.EnableXTINudge.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertEquals(false, InboundServiceTask.CheckIsNudged());
				}
			}
		}

		public static IDisposable MockProductRegistration(string databaseType, bool isInternalSystem = false, string hostedLocation = "SYD")
		{
			var prodKeyMock = new Mock<IProductRegistrationKey>();
			var productRegistrationMock = new Mock<IProductRegistration>();
			productRegistrationMock.Setup(m => m.IsWiseTechGlobalInternalSystem()).Returns(isInternalSystem);
			productRegistrationMock.Setup(m => m.Key).Returns(prodKeyMock.Object);
			prodKeyMock.Setup(m => m.DatabaseType).Returns(databaseType);
			prodKeyMock.Setup(m => m.HostedLocation).Returns(hostedLocation);

			return ObjectFactory.Substitute(productRegistrationMock.Object);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
