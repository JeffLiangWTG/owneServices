using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Data;
using Microsoft.Extensions.Logging;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	class ServiceHostTerminator : IServiceHostTerminator
	{
		public ServiceHostTerminator(IHostRegistrySettings hostRegistry)
		{
			this.hostRegistry = hostRegistry ?? throw new ArgumentNullException(nameof(hostRegistry));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public void TerminateInactiveServiceTaskHosts(IHostLogger hostLogger)
		{
			var frequencyAndIdleTimeInMinutes = hostRegistry.ServiceTaskHostTerminatorFrequency;
			if (terminatorStopwatch == null || terminatorStopwatch.Elapsed.TotalMinutes >= frequencyAndIdleTimeInMinutes)
			{
				(terminatorStopwatch ?? (terminatorStopwatch = new Stopwatch())).Restart();

				var sql_kill = string.Format(CultureInfo.InvariantCulture,
					sql
					, DbConnectionConstants.ApplicationNames.ServiceHost.QuoteName('\'')
					);

				using (var connection = Db.NewAdminConnection())
				using (var cmd = connection.Command(sql_kill))
				{
					cmd.AddParameter("@minutes", SqlDbType.Int, frequencyAndIdleTimeInMinutes);

					var result = (string)cmd.ExecuteScalar();
					if (!string.IsNullOrEmpty(result))
					{
						hostLogger?.Log(LogLevel.Warning, "The following service host SPIDs were killed on the database: " + result.Substring(0, result.Length - 2));
					}
				}
			}
		}

		Stopwatch terminatorStopwatch;
		readonly IHostRegistrySettings hostRegistry;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1183:DATEADD with non-integer value", Justification = "Rule was implemented incorrectly")]
		const string sql = @"
DECLARE
	@sql      varchar(8000) = ''
	, @result varchar(8000) = '';

SELECT TOP(500)
	@sql    += 'KILL ' + CONVERT(varchar(5), session_id) + '; ',
	@result += CONVERT(varchar(5), session_id) + ', '
FROM
	sys.dm_exec_sessions
WHERE
	database_id = DB_ID()
	AND status = 'sleeping'
	AND program_name = {0}
	AND last_request_start_time < DATEADD(MINUTE, -@minutes, GETDATE())
	AND last_request_end_time   < DATEADD(MINUTE, -@minutes, GETDATE())
ORDER BY
	session_id;

EXEC(@sql);
SELECT @result;

";
	}
}

