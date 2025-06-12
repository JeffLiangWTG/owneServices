using System;
using System.Diagnostics;
using CargoWise.eServices.TestHelpers.Database.Common;
using CargoWise.eServices.TestHelpers.Database.Deployment;
using NUnit.Framework;
using static CargoWise.eServices.Authentication.IntegrationTests.Deployments;
using static eServices.eHubDatabase.Tests.Common.Deployments;

namespace CargoWise.eHub.Gateway.IntegrationTests
{
	[SetUpFixture]
	public class DatabaseSetup
	{
		[OneTimeSetUp]
		public void DeployDatabases()
		{
			var stopwatch = Stopwatch.StartNew();
			new BillingContext(SqlServerHelper.GetAdminConnectionString(BillingDatabaseName)).Database.Initialize(true);
			try
			{
				Deployment.Deploy(
					Master,
					MasterSecondary,
					EdiProd,
					EdiProdCache,
					EHubTransactions,
					EHubArchiveOnlineSecondary,
					Authentication,
					EHubTransactionsProxy);
			}
			catch
			{
				Deployment.Recreate(
					Master,
					MasterSecondary,
					EdiProd,
					EdiProdCache,
					EHubTransactions,
					EHubArchiveOnlineSecondary,
					Authentication,
					EHubTransactionsProxy);
			}
			stopwatch.Stop();
			DatabaseSetupTiming.DeployTime = stopwatch.Elapsed;
			TestContext.Progress.WriteLine($"[DatabaseSetup] DeployDatabases took {stopwatch.Elapsed.TotalSeconds:F1} seconds");
		}

		[OneTimeTearDown]
		public void DropEHubTransactionsProxy()
		{
			var databaseNames = new[] { "eHubTransactionsProxy" };
			SqlServerHelper.DropSnapShot(databaseNames);
			SqlServerHelper.DropDatabases(databaseNames);
		}



		public const string BillingDatabaseName = "eHubGateway.Billing.IntegrationTesting";
	}
}

public static class DatabaseSetupTiming
{
	public static TimeSpan? DeployTime { get; set; }
}
