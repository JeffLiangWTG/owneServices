using CargoWise.Data.Providers.Common;
using Enterprise.Accounting.Web.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Web.Testing
{
	public class DataAccessTest : TestCase
	{
		public void TestEnabledMultiSubnetFailover()
		{
			// Arrange
			using (SqlFailoverSettingsTestHelper.SetMockWindowsRegistry(serversThatAreEnabled: new[] { WebConfigManager.EnterpriseDbServer }))
			{
				// Act
				var connection = DbAccess.NewConnection();

				// Assert
				AssertEquals(true, new SqlConnectionStringBuilder(connection.ConnectionString).MultiSubnetFailover);
			}
		}

		public void TestEnabledEncryptAndTrustServerCertificate()
		{
			// Arrange
			using (SqlFailoverSettingsTestHelper.SetMockWindowsRegistry(serversThatAreEnabled: new[] { WebConfigManager.EnterpriseDbServer }))
			{
				// Act
				var connection = DbAccess.NewConnection();

				// Assert
#pragma warning disable CW1065
				var builder = new SqlConnectionStringBuilder(connection.ConnectionString);
#pragma warning restore CW1065

				var serverName = builder.DataSource;
				AssertNotNullOrEmpty(serverName);

				var expectedEncrypt = SqlTlsSetting.ShouldEncryptSqlConnection(serverName);
				AssertEquals(expectedEncrypt, builder.Encrypt);
				AssertEquals(!expectedEncrypt, builder.TrustServerCertificate);
			}
		}
	}
}
