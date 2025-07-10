using System;
using System.Data;
using System.Globalization;
using CargoWise.Bi.Common;
using CargoWise.Data;
using ServiceManager.Integration.Abstractions;

namespace CargoWise.Bi.Product.ServiceTask
{
	public abstract class EtlExecutionTaskQueue : IHostedServiceQueueProvider
	{
		QueueResult IHostedServiceQueueProvider.QueueResult
		{
			get
			{
				try
				{
					return GetQueueResult();
				}
				catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.InvalidObjectName)
				{
					return QueueResult.Zero;
				}
			}
		}

		protected string GetParamValue(string paramName)
		{
			if (!string.IsNullOrEmpty(BiServer))
			{
				using (var biConnection = Db.NewExtraConnectionWithMainDbCredentials(BiServer, BiDbName))
				{
					return BiMasterState.GetParameter(biConnection, paramName);
				}
			}
			else
			{
				return string.Empty;
			}
		}

		protected byte[] GetLsnHighWaterMark()
		{
			byte[] maxLsn = null;
			if (!string.IsNullOrEmpty(BiServer))
			{
				using (var biConnection = Db.NewExtraConnectionWithMainDbCredentials(BiServer, BiDbName))
				{
					maxLsn = BiMasterState.GetParameterAsByteArray(biConnection, LsnHwmParameter);
				}
			}
			return maxLsn;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		QueueResult GetQueueResult()
		{
			QueueResult result = QueueResult.Zero;
			var maxLsn = GetLsnHighWaterMark();
			if (maxLsn != null)
			{
				using (var cmd = Db.Connection.Command("dbo.usp_GetEtlTaskQueueSize"))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddParameter("@MaxLsn", SqlDbType.Binary, 10, 0, 0, maxLsn);
					cmd.AddOutputParameter("@QueueSize", SqlDbType.Int, 32, 0, 0, null);
					cmd.AddOutputParameter("@QueueAge", SqlDbType.Int, 32, 0, 0, null);
					cmd.ExecuteNonQuery();

					var queueSize = Convert.ToInt32(cmd.GetParameterValue("@QueueSize"), CultureInfo.InvariantCulture);
					var queueAge = Convert.ToInt32(cmd.GetParameterValue("@QueueAge"), CultureInfo.InvariantCulture);

					return new QueueResult(queueSize, TimeSpan.FromSeconds(queueAge));
				}
			}
			return result;
		}

		protected abstract string BiServer { get; }
		protected abstract string BiDbName { get; }
		protected abstract string LsnHwmParameter { get; }
	}
}
