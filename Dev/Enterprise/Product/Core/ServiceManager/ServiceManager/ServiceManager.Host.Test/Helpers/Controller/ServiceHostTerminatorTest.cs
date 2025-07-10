using System.Globalization;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing
{
	class ServiceHostTerminatorTest : TransactionedTestCase
	{
		[ExpectNoExceptions]
		public void TestServiceTaskHostTerminator()
		{
			var originalNumberOfServiceTaskHosts = GetNumberOfServiceTaskHosts();

			var applicationNameSuffix = DbConnectionConstants.ApplicationNames.ServiceHost.Replace(DbConnectionConstants.ApplicationNames.CargoWiseOne, string.Empty);
			using (var connection = Db.NewExtraRestrictedReaderConnection(Db.ServerName, Db.DatabaseName, applicationNameSuffix))
			{
				connection.EnsureIsOpen();

				var hostRegistry = new Mock<IHostRegistrySettings>();
				var terminator = new ServiceHostTerminator(hostRegistry.Object);

				NUnit.Framework.Assert.That(GetNumberOfServiceTaskHosts(), Is.EqualTo(originalNumberOfServiceTaskHosts + 1), "Should have a new service task host.");

				hostRegistry.SetupGet(o => o.ServiceTaskHostTerminatorFrequency).Returns(10);
				terminator.TerminateInactiveServiceTaskHosts(null);
				NUnit.Framework.Assert.That(GetNumberOfServiceTaskHosts(), Is.EqualTo(originalNumberOfServiceTaskHosts + 1), "Shouldn't have terminated the new service task host yet.");

				hostRegistry.SetupGet(o => o.ServiceTaskHostTerminatorFrequency).Returns(0);
				terminator.TerminateInactiveServiceTaskHosts(null);
				var serviceHostsFound = FindServiceHostRecordsFromDatabase();
				NUnit.Framework.Assert.That(GetNumberOfServiceTaskHosts(), Is.EqualTo(originalNumberOfServiceTaskHosts), "Should have terminated the service task host. But it found: " + serviceHostsFound);
			}
		}

		int GetNumberOfServiceTaskHosts()
		{
			var sql = string.Format(CultureInfo.InvariantCulture,
				"SELECT COUNT(*) FROM sys.dm_exec_sessions WHERE program_name = {0};"
				, DbConnectionConstants.ApplicationNames.ServiceHost.QuoteName('\'') // 0
				);

			using (var connection = Db.NewAdminConnection())
			using (var command = connection.Command(sql))
			{
				return (int)command.ExecuteScalar();
			}
		}

		string FindServiceHostRecordsFromDatabase()
		{
			var result = new StringBuilder();
			var sql = string.Format(CultureInfo.InvariantCulture, @"
SELECT
	spid         = s.session_id,
	last_batch   = s.last_request_end_time,
	status       = s.status,
	hostname     = s.host_name,
	program_name = s.program_name,
	loginame     = s.login_name,
	get_date     = GETDATE(),
	dbid         = s.database_id,
	get_db_id    = DB_ID()
FROM
	sys.dm_exec_sessions AS s
WHERE
	s.program_name = {0};

"
				, DbConnectionConstants.ApplicationNames.ServiceHost.QuoteName('\'') // 0
				);

			using (var connection = Db.NewAdminConnection())
			{
				connection.ExecuteReader(sql, (reader) =>
				{
					result.Append("\n");
					result.AppendFormat(CultureInfo.InvariantCulture,
						"spid={0}, last_batch={1}, status={2}, hostname={3}, program_name={4}, loginame={5}, dbid={6}, get_date={7}, get_db_id={8}"
						, reader["spid"].ToString().Trim()         // 0
						, reader["last_batch"].ToString().Trim()   // 1
						, reader["status"].ToString().Trim()       // 2
						, reader["hostname"].ToString().Trim()     // 3
						, reader["program_name"].ToString().Trim() // 4
						, reader["loginame"].ToString().Trim()     // 5
						, reader["dbid"].ToString().Trim()         // 6
						, reader["get_date"].ToString().Trim()     // 7
						, reader["get_db_id"].ToString().Trim()    // 8
						);
				});
			}
			return result.ToString();
		}
	}
}
