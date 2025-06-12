using System;
using System.Collections.Generic;
using CargoWise.eServices.TestHelpers.Database.Deployment;
using NUnit.Framework;

namespace CargoWise.eHub.Gateway.IntegrationTests
{
	[TestFixture]
	public abstract class GenericTestBase
	{
		[OneTimeSetUp]
		public void ClassInitialize()
		{
			try
			{
				TestDataController = new TestDataController(CommonTestDataLocation, TestDataSchemaLocation, this.GetType().Name, ConnectionStringPattern, TestClassLocationPattern);
				TestDataController.LoadTestData();
			}
			catch (Exception ex1)
			{
                try
                {
                    TestDataController?.RestoreDatabase();
                }
                catch (Exception ex2)
                {
                    throw new AggregateException("There was an exception in setup, then a second exception occured during teardown.", ex1, ex2);
                }
                throw ex1;
			}
		}

		[OneTimeTearDown]
		public void ClassCleanup()
		{
			TestDataController?.RestoreDatabase();
		}

		[SetUp]
		public void SetUp()
		{
			TestContext.WriteLine(Deployment.GetLog());

			rollbackList = new List<Action>();
			SetUpCore();
		}

		[TearDown]
		public void TearDown()
		{
			TearDownCore();
			foreach (var action in rollbackList)
			{
				action();
			}
		}

		public virtual void SetUpCore()
		{
		}

		public virtual void TearDownCore()
		{
		}

		protected void AddRollback(Action action)
		{
			rollbackList.Add(action);
		}

		private TestDataController TestDataController;
		protected virtual string CommonTestDataLocation { get { return null; } }
		protected virtual string TestDataSchemaLocation { get { return null; } }
		protected virtual string ConnectionStringPattern { get { return ".Properties.Settings."; } }
		protected virtual string TestClassLocationPattern { get { return ".*."; } }
		List<Action> rollbackList;
	}
}
