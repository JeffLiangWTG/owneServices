using System;
using System.Globalization;
using CargoWise.Data;
using Microsoft.Extensions.Logging;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	class DSATaskRunner : IDSATaskRunner
	{
		readonly IRunnableServiceTask dsaTask;
		readonly ITaskQueue taskQueue;
		readonly IHostLogger hostLogger;
		protected DateTime serverStartTime = DateTime.MinValue;

		public DSATaskRunner(IRunnableServiceTask dsaTask, ITaskQueue taskQueue, IHostLogger hostLogger)
		{
			this.dsaTask = dsaTask ?? throw new ArgumentNullException(nameof(dsaTask));
			this.taskQueue = taskQueue ?? throw new ArgumentNullException(nameof(taskQueue));
			this.hostLogger = hostLogger ?? throw new ArgumentNullException(nameof(hostLogger));
		}

		void IDSATaskRunner.RunDsaTaskIfDbServerRestarts()
		{
			var previousServerStartTime = serverStartTime;
			serverStartTime = GetCurrentSqlServerStartTime();

			if (serverStartTime > previousServerStartTime)
			{
				var newRequest = new DirectTaskRunRequest(dsaTask, echoes: false);
				hostLogger.Log(LogLevel.Debug, newRequest.FormatRequestToLogMessage(LogMessageStage.RequestIsCreated));
				taskQueue.EnqueueTask(newRequest);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		DateTime GetCurrentSqlServerStartTime()
		{
			var result = DateTime.MinValue;
			using (var cmd = Db.Connection.Command("SELECT create_date FROM sys.databases WHERE name = 'tempdb'"))
			{
				result = Convert.ToDateTime(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
			}
			return result;
		}
	}
}

