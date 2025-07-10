using System.Data;
using CargoWise.Data;
using NUnit.Framework;

#pragma warning disable IDE0005 // Using directive is unnecessary.
using System; // DEV note: this is included in CWNetCoreTest.TestAdapter project, with implicit using. Using is still needed for Enterprise.Dat.Adapter.Test project
using System.Collections.Generic; //Not required for the .Net 8 build.
#pragma warning restore IDE0005 // Using directive is unnecessary.

namespace Enterprise.Dat.Implementation.Preconditions.Testing
{
#if NETFRAMEWORK
	[TargetFrameworks(TargetFramework.NetFramework | TargetFramework.NetCore)]
#endif
	sealed class SqlServerTransportProtocolTest : TestCase
	{
		public void TestCurrent()
		{
			TestCore();
		}

		[DatCapabilityRequirementLatestAvailableSqlServer]
		public void TestLatestAvailable()
		{
			TestCore();
		}

		void TestCore()
		{
			// Arrange
			using (var adminConnection = Db.NewAdminConnection())
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				// Act
				// Assert
				AssertContainsExactElementsInAnyOrder(
					"Shared memory transport protocol should be used",
					StringComparer.OrdinalIgnoreCase,
					new[] { $"Shared memory {connection.SPID}", $"Shared memory {adminConnection.SPID}" },
					GetNetworkTransports());

				IEnumerable<string> GetNetworkTransports()
				{
					var transports = new List<string>();
					adminConnection.ExecuteReader(
						@"
SELECT transport = CONCAT(CONVERT(nvarchar(max), net_transport), ' ', session_id)
FROM sys.dm_exec_connections
WHERE
	(
		session_id = @SessionId
		OR session_id = @@SPID
	)
	AND net_transport = 'Shared memory'
",
						(DbCommand cmd) => cmd.AddParameter("@SessionId", SqlDbType.Int, connection.SPID),
						(IDataRecord record) => transports.Add((string)record["transport"]));
					return transports;
				}
			}
		}
	}
}
