using System;
using CargoWise.Common;
using Enterprise.VisualBoards.Business.Test;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.BufferManagement.Business.Test
{
	public abstract class BMServiceTaskTestCase<T> : ServiceTaskTestCase<T> where T : ServiceProviderImpl
	{
		public void TestHostedServiceAttribute()
		{
			var attributes = typeof(T).Assembly.GetCustomAttributes(typeof(HostedServiceAttribute), false);
			var attribute = Array.Find((HostedServiceAttribute[])attributes, a => a.TypeName == typeof(T).FullName);
			AssertEquals("Minimum Period should be 15 minutes", "15minutes", attribute.MinimumPeriod);
			AssertEquals(CanRunInAnyBranch ?
				"Should be able to run in any branch since any uses of CurrentBranch and CurrentDepartment will cause inconsistent behaviour, so should be carefully analysed." :
				"Should not be able to run in any branch",
				CanRunInAnyBranch, attribute.CanRunInAnyBranch);
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();

			ReleaseLogFailureService = new ReleaseGateFailureLogService_ForTest();
			BMSTestCaseWithFactory.SetFactoryReleaseLogFailureService(Factory, ReleaseLogFailureService);
			disposables = new DisposableList(new[] { DummySecondaryServerConnectionProvider.TemporarilyEnableDummyProvider() });
		}

		protected override void TearDownCore()
		{
			base.TearDownCore();

			disposables.Dispose();
		}

		protected virtual bool CanRunInAnyBranch => true;
		protected ReleaseGateFailureLogService_ForTest ReleaseLogFailureService { get; private set; }
		DisposableList disposables;
	}
}
