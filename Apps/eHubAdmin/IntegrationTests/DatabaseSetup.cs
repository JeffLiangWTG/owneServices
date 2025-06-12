using CargoWise.eServices.TestHelpers.Database.Common;
using CargoWise.eServices.TestHelpers.Database.Deployment;
using NUnit.Framework;
using static eServices.eHubDatabase.Tests.Common.Deployments;

namespace eServices.eHubAdmin.IntegrationTests
{
	[SetUpFixture]
	public class DatabaseSetup
	{
		[OneTimeSetUp]
		public void DeployDatabases()
		{
			try
			{
				Deployment.Deploy(
					Master,
					EHubTransactions,
					EHubArchiveOnlineSecondary,
					EHubArchiveOnlineView
				);
				SqlServerHelper.AddTestAccountToDatabase("eHubTransactions");
				SqlServerHelper.AddTestAccountToDatabase("eHubArchiveOnlineView");
				SqlServerHelper.AddTestAccountToDatabase("eHubArchiveOnline");
				SqlServerHelper.AddTestAccountToDatabase("ediProdCache");
			}
			catch
			{
				Deployment.Recreate(
					Master,
					EHubTransactions,
					EHubArchiveOnlineSecondary,
					EHubArchiveOnlineView);
				SqlServerHelper.AddTestAccountToDatabase("eHubTransactions");
				SqlServerHelper.AddTestAccountToDatabase("eHubArchiveOnlineView");
				SqlServerHelper.AddTestAccountToDatabase("eHubArchiveOnline");
				SqlServerHelper.AddTestAccountToDatabase("ediProdCache");
			}
		}
	}
}
