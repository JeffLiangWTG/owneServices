using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.ServiceTasks.JobCostingReport;
using Moq;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	public class JCDServiceTaskStatusCheckerTest : TestCaseWithFactory
	{
		public void TestIsJCDServiceTaskComplete()
		{
			var checker = new JCDServiceTaskStatusChecker();
			AccountingConfigurationRegistry.Instance.JobCostingReportRelatedDBObjectVersion.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDDependentObjectList.LATEST_VERSION);
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

			Assert("Should return false if JCD service task is not completed", !checker.IsJCDServiceTaskComplete);
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var startAction = new JCDStartActionStrategy(TestConnection, new TestServiceLogger());
				startAction.Process();
				var processAction = new JCDDefaultActionStrategy(TestConnection, new TestServiceLogger());
				processAction.Process();
			}
			AssertEquals("Precondition: JCD state should have changed to CUP", JCDActionList.Codes.CompletedOldTransactionLinesHaveBeenProcessed, AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);

			Assert("Should return true if JCD service task is completed and JCD DB objects are up to date", checker.IsJCDServiceTaskComplete);

			AccountingConfigurationRegistry.Instance.JobCostingReportRelatedDBObjectVersion.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, -1);
			Assert("Should return false if JCD DB objects are not up to date", !checker.IsJCDServiceTaskComplete);
		}
	}
}
