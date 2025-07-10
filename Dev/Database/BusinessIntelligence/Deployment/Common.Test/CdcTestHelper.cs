using CargoWise.Data;

namespace Enterprise.ChangeDataCapture.Common.Testing
{
	public static class CdcTestHelper
	{
		public static void TruncateLog(AdminConnection conn)
		{
			if (NUnit.Framework.TestingState.IsRunningOnDAT)
			{
				var sqlText = string.Format(@"DECLARE @logfile nvarchar(50) =
				(SELECT TOP 1 [name] FROM sys.master_files WHERE database_id = db_id('{0}') AND type = 1) 
				DBCC SHRINKFILE(@logfile, 0, TRUNCATEONLY)", conn.CurrentDatabase);
				conn.ExecuteNonQuery(sqlText);
			}
		}
	}
}
