using System;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ChangeDataCapture.Service.Testing
{
	public class CaptureTaskForTest : CaptureTask
	{
		public CaptureTaskForTest(TestServiceLogger logger)
		{
			ServiceLogger = logger;
		}

		protected override void CaptureChangesCore(CancellationToken token)
		{
			var objectId = new Random().Next(1000000000, 2000000000).ToString();
			var enableTraceFlagException = SqlExceptionBuilder.CreateSqlError(3764, 1, 1, Db.Connection.ServerName, $"Cannot alter the procedure 'cdc.sp_batchinsert_{objectId}' because it is being used for Change Data Capture.", "", 1);
			throw SqlExceptionBuilder.CreateSqlException(enableTraceFlagException);
		}
	}
}
