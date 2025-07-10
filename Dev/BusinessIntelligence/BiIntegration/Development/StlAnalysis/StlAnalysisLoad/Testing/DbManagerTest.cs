using System;
using CargoWise.Data.Providers.Common;
using NUnit.Framework;

namespace Enterprise.StlAnalysis.Load.Testing
{
	public class DbManagerTest : TestCase
	{
		public void TestEnabledMultiSubnetFailover()
		{
			// Arrange
			var serverName = "server1";
			var dbName = "database1";

			var builder = new SqlConnectionStringBuilder();

			using (SqlFailoverSettingsTestHelper.SetMockWindowsRegistry(serversThatAreEnabled: Array.Empty<string>() ))
			{
				// Act
				DbManager.ConfigureConnectionString(builder, serverName, dbName, null, null, false);

				// Assert
				AssertEquals(false, builder.MultiSubnetFailover);
			}

			using (SqlFailoverSettingsTestHelper.SetMockWindowsRegistry(serversThatAreEnabled: new[] { serverName }))
			{
				// Act
				DbManager.ConfigureConnectionString(builder, serverName, dbName, null, null, false);

				// Assert
				AssertEquals(true, builder.MultiSubnetFailover);
			}
		}
	}
}
