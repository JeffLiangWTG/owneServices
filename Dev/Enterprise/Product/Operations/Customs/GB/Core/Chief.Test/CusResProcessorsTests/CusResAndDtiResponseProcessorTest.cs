using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Moq;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.GB.Chief.CusRes.Testing
{
	public abstract class CusResAndDtiResponseProcessorTest : TestCaseWithFactory
	{
		protected void RunProcessor()
		{
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				new CusResResponseMessageProcessor(log).ExecuteBatch();
			}
		}

		protected override void SetUp()
		{
			branchEnvironment = DisposableEnvironment.ForBranch(Business.Testing.DeclarationTestHelper.CreateGbCompanyAndBranchAndSave(Factory).PK.ToGuid());
			log = new TestServiceLogger();
		}

		IDisposable branchEnvironment;
		protected TestServiceLogger log { get; private set; }

		protected override void TearDown()
		{
			base.TearDown();
			branchEnvironment.Dispose();
		}
	}
}
