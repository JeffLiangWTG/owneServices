using System;
using System.Linq;
using CargoWise.Data;
using Enterprise.Client.EDI;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace ZClientEDI.Business.Billing
{
	public class ExternalServerChecker
	{
		public ExternalServerChecker(ILogger logger)
		{
			Logger = logger;
		}

		public void Check()
		{
			foreach (var connectionConfig in EDIDataRegistry.Instance.GetAllItems()
				.Where(x => x.Category.Contains(EDIDataRegistry.ExternalServersCategory))
				.OrderBy(x => x.Category)
				.GroupBy(x => x.Category).ToArray())
			{
				Log($"*** Checking... {connectionConfig.Key}");

				var serverName = connectionConfig.OfType<StringRegistryItem>().Single(x => x.Name.EndsWith("ServerName")).Value;
				var databaseName = connectionConfig.OfType<StringRegistryItem>().Single(x => x.Name.EndsWith("DBName") || x.Name.EndsWith("DatabaseName")).Value;
				var login = connectionConfig.OfType<ServerUsernamePasswordConfigurationRegistryItem>().Single().Value;

				CheckCore(serverName, databaseName, login.UserName, login.Password);
				Log("\r\n");
			}
		}

		public void Check(string connectionString)
		{
			try
			{
				var sqlConnString = new SqlConnectionStringBuilder(connectionString) { Encrypt = true };
				CheckCore(sqlConnString.DataSource, sqlConnString.InitialCatalog, sqlConnString.UserID, sqlConnString.Password);
			}
			catch (Exception ex)
			{
				Log(LogType.Error, ex.Message, ex);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Full name needed to identify SqlClient types")]
		void CheckCore(string dataSource, string initialCatalog, string userID, string password)
		{
			try
			{
				Log($"Connecting... Server:{dataSource}, Database:{initialCatalog}, UserID:{userID}, Password:{new string('*', password.Length)}");

				if (new[] { dataSource, initialCatalog, userID, password }.Any(x => string.IsNullOrWhiteSpace(x)))
				{
					Log(LogType.Error, "Database connection information is incomplete. Please ensure all required fields (data source, initial catalog, user ID, and password) are provided.");
					return;
				}

				using (var conn = Db.NewExtraConnection(dataSource, initialCatalog, userID, password))
				{
					var dbConnection = ((IDbConnectionInternals)conn).ADOConnection;
					if (dbConnection is System.Data.SqlClient.SqlConnection sqlConnectionSys)
					{
						sqlConnectionSys.InfoMessage += (_, e) => Log(e.Message);
					}
#if NET
					else if (dbConnection is Microsoft.Data.SqlClient.SqlConnection sqlConnectionMS)
					{
						sqlConnectionMS.InfoMessage += (_, e) => Log(e.Message);
					}
#endif

					conn.ExecuteNonQuery(PermissionQuery);
				}
			}
			catch (Exception ex)
			{
				Log(LogType.Error, ex.Message);
			}
		}

		const string PermissionQuery = @"DECLARE @TableName NVARCHAR(255)
DECLARE @SQL NVARCHAR(MAX)

DECLARE table_cursor CURSOR FOR
SELECT TABLE_NAME = QUOTENAME(TABLE_SCHEMA) + '.' + QUOTENAME(TABLE_NAME)
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_SCHEMA <> 'cdc' ORDER BY 1

OPEN table_cursor
FETCH NEXT FROM table_cursor INTO @TableName

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @SQL = 'SELECT TOP 1 PK=NEWID(), Col = 1 FROM ' + @TableName + ' WITH(NOLOCK);'
    
    BEGIN TRY
        EXEC sp_executesql @SQL
        PRINT 'Permission granted to query data from table: ' + @TableName
    END TRY
    BEGIN CATCH
        PRINT 'Permission denied to query data from table: ' + @TableName
    END CATCH

    FETCH NEXT FROM table_cursor INTO @TableName
END

CLOSE table_cursor
DEALLOCATE table_cursor";

		readonly ILogger Logger;
		void Log(string message) => Log(LogType.Information, message);
		void Log(LogType type, string message) => Logger?.Log(type, message);
		void Log(LogType type, string message, Exception ex) => Logger?.Log(type, message, ex);
	}
}
