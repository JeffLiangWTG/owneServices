using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.Testing
{
	[UseSnapshotProtection]
	sealed class ZGlobalConfigMultithreadedTest : TestCase
	{
		public void TestConfigIsCachedAndWorksDuringUpgrade()
		{
			var branck = GlbBranch.CurrentBranch;
			if (branck.GB_WebAddress.IsEmpty)
			{
				branck.GB_WebAddress = "http://www.wisetechglobal.com/";
				branck.Factory.Save();
			}

			Env.Registry.WebBranch = branck.PK.ToGuid();
			string branchCode = branck.GB_Code;
			string companyName = branck.CompanyName;
			{
				var config = new ZGlobalConfig();
				config.ConfigurationItemsForTesting = null;
				AssertEquals(branchCode, config.Branch);
				AssertEquals(companyName, config.CompanyName);
			}
			using (var adminConnection = Db.NewAdminConnection())
			{
				AssertEquals(DbLockoutState.AquiredLockout, adminConnection.AcquireLockout());
				DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName);
				try
				{
					var task = new Task(() =>
					{
						using (Db.DisposableActionForDbConnection())
						{
							AssertExceptionThrown(typeof(DatabaseUpgradeInProgressException), () => Db.Connection.EnsureIsOpen());
							var config = new ZGlobalConfig();
							config.ConfigurationItemsForTesting = null;
							AssertEquals(branchCode, config.Branch);
							AssertEquals(companyName, config.CompanyName);
						}
					});
					task.Start();
					task.Wait();
				}
				finally
				{
					AssertEquals(DbLockoutState.ResetLockout, adminConnection.ResetLockout());
				}
			}
		}
	}
}
