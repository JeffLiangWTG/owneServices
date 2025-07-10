using System;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Client.EDI.Billing.GUI.Test
{
	public class BillingBackgroundTaskCheckerTest : TestCase
	{
		public void TestCanProceed_ProductNotRegistered()
		{
			var mockRepo = new Mock<IProductRegistration>();
			mockRepo.Setup(m => m.LocalVerify())
				.Returns(ProductRegistrationVerifyResult.Unregistered);

			using (ObjectFactory.Substitute(mockRepo.Object))
			{
				var checker = new BillingBackgroundTaskChecker("CHU");
				AssertEquals(true, checker.CanProceed());
			}

			mockRepo.Verify(m => m.LocalVerify(), Times.Once);
			mockRepo.VerifyAll();
		}

		public void TestCanProceed_TaskRunning()
		{
			var mockProvider = new Mock<IServiceManagerQuerier>();
			mockProvider
				.Setup(m => m.CheckStateOfNamedServiceTask("CHU"))
				.Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

			using (ObjectFactory.Substitute(mockProvider.Object))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var checker = new BillingBackgroundTaskChecker("CHU");
				AssertEquals(true, checker.CanProceed());
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
			mockProvider.Verify(m => m.CheckStateOfNamedServiceTask("CHU"), Times.Once);
			mockProvider.VerifyAll();
		}

		public void TestCanProceed_TaskInactive()
		{
			var mockProvider = new Mock<IServiceManagerQuerier>();
			mockProvider
				.Setup(m => m.CheckStateOfNamedServiceTask("CHU"))
				.Returns(ServiceTaskStatus.ServiceTaskIsInactive);

			using (ObjectFactory.Substitute(mockProvider.Object))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var checker = new BillingBackgroundTaskChecker("CHU");
				AssertEquals(false, checker.CanProceed());
				AssertEquals(@"Question Service Task is inactive.
Billing data may not be generated.
Recommend re-activating service task.
Are you sure you wish to continue?", UnitTestUserNotification.Instance.LastMessage.ToString());

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				checker = new BillingBackgroundTaskChecker("CHU");
				AssertEquals(true, checker.CanProceed());
				AssertEquals(@"Question Service Task is inactive.
Billing data may not be generated.
Recommend re-activating service task.
Are you sure you wish to continue?", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
			mockProvider.Verify(m => m.CheckStateOfNamedServiceTask("CHU"), Times.Exactly(2));
			mockProvider.VerifyAll();
		}

		public void TestCanProceed_ServiceTaskStatusFaulty_NoTask() => AssertCanProceed_ServiceTaskStatusFaulty(ServiceTaskStatus.NoSuchTaskIsInstalledInThisDb);
		public void TestCanProceed_ServiceTaskStatusFaulty_NoMachine() => AssertCanProceed_ServiceTaskStatusFaulty(ServiceTaskStatus.NoMachineUponWhichToRunTheTask);
		public void TestCanProceed_ServiceTaskStatusFaulty_MachineNonResponsive() => AssertCanProceed_ServiceTaskStatusFaulty(ServiceTaskStatus.NoAvailableHosts);

		void AssertCanProceed_ServiceTaskStatusFaulty(ServiceTaskStatus status)
		{
			var mockProvider = new Mock<IServiceManagerQuerier>();
			mockProvider
				.Setup(m => m.CheckStateOfNamedServiceTask("CHU"))
				.Returns(status);

			using (ObjectFactory.Substitute(mockProvider.Object))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var checker = new BillingBackgroundTaskChecker("CHU");
				AssertEquals(false, checker.CanProceed());
				AssertEquals($@"Question Service Task status is {Enum.GetName(typeof(ServiceTaskStatus), status)}.
Billing data may not be generated.
Recommend resolving issue before proceeding.
Are you sure you wish to continue?", UnitTestUserNotification.Instance.LastMessage.ToString());

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				checker = new BillingBackgroundTaskChecker("CHU");
				AssertEquals(true, checker.CanProceed());
				AssertEquals($@"Question Service Task status is {Enum.GetName(typeof(ServiceTaskStatus), status)}.
Billing data may not be generated.
Recommend resolving issue before proceeding.
Are you sure you wish to continue?", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
			mockProvider.Verify(m => m.CheckStateOfNamedServiceTask("CHU"), Times.Exactly(2));
			mockProvider.VerifyAll();
		}
	}
}
