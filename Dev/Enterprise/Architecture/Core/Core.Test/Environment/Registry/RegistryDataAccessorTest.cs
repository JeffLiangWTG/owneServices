using System;
using System.Text;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Test.Environment.Registry
{
	public class RegistryDataAccessorTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestGetBinaryValue_WhenDatabaseUpgradedExceptionHasBeenThrown()
		{
			using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
			{
				var recordName = Guid.NewGuid().ToString();
				var testValue = Encoding.Unicode.GetBytes("TestValue");

				registryDataAccessor.SetBinaryValue(recordName, EnvProxy.Instance.CurrentUser.PK, Guid.Empty, testValue, Guid.Empty, "STR", false, false);
				AssertEquals("TestValue", Encoding.Unicode.GetString(registryDataAccessor.GetBinaryValue(recordName, EnvProxy.Instance.CurrentUser.PK, Guid.Empty)));

				using (Db.DisposableUpgrade_ForTest(acquireLockOut: false))
				{
					AssertExceptionThrown<DatabaseUpgradedException>(() => ((IDbReconnectionHandling)Db.Connection).CloseAndReopenConnection());

					AssertNull(registryDataAccessor.GetBinaryValue(recordName, EnvProxy.Instance.CurrentUser.PK, Guid.Empty));
				}
			}
		}
	}
}
