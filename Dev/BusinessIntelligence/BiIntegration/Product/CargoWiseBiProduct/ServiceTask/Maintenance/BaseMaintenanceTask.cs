using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Bi.Common;
using CargoWise.Data;
using Enterprise.Integration;
using ServiceManager.Integration.ServiceTasks.CW;

namespace CargoWise.Bi.Product.ServiceTask.Maintenance
{
	public abstract class BaseMaintenanceTask : ServiceProviderImpl, IBiNotificationSource
	{
		protected List<Exception> Exceptions
		{
			get
			{
				return exceptions ?? (exceptions = new List<Exception>());
			}
		}
		List<Exception> exceptions;

		public abstract string Code { get; }
		public abstract string Description { get; }

		protected abstract string DatabaseServerName { get; }
		protected abstract string DatabaseName { get; }
		protected abstract string NudgeTimeParamName { get; }
		protected abstract void RunMaintenanceTasks(DbConnection dbConnection);

		#region SuppressResourceStringsCheckRegion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		public override void RunTask(CancellationToken token)
		{
			Exceptions.Clear();

			using (var biConnection = Db.NewAdminConnection(DatabaseServerName, DatabaseName))
			{
				if (!string.IsNullOrEmpty(BiMasterState.GetParameter(biConnection, NudgeTimeParamName)))
				{
					RunMaintenanceTasks(biConnection);
					BiMasterState.DeleteParameter(biConnection, NudgeTimeParamName);
				}
				else
				{
					var lastIndexRebuildDate = BiMasterState.GetParameterDate(biConnection, BiConstants.LastIndexRebuildUtcDt);
					if (lastIndexRebuildDate == null)
					{
						ServiceLogger.Log(LogType.Debug, $"Index has not been rebuilt. Running {Description}.");
						BiMasterState.SetParameter(biConnection, NudgeTimeParamName, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
						RunMaintenanceTasks(biConnection);
					}
					else
					{
						ServiceLogger.Log(LogType.Debug, $"{NudgeTimeParamName} is not set. Skipping {Description}.");
					}
				}
			}

			if (Exceptions.Any())
			{
				var aggregateException = new AggregateException(Exceptions);
				ServiceLogger.Log(LogType.Error, aggregateException.Message, aggregateException);
				throw aggregateException;
			}
		}
		#endregion
	}
}
