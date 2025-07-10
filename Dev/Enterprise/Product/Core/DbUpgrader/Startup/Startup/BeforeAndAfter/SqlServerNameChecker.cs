using CargoWise.Data;

namespace Enterprise.DbUpgrader.Startup
{
	public class SqlServerNameChecker
	{
		public string ErrorMessage
		{
			get { return "The SQL Server local server name does not match the machine name. The problem has been fixed, but it requires a restart of the SQL Server service."; }
		}

		public void CheckServerName(DbConnection connection)
		{
			try
			{
				connection.ExecuteNonQuery(sqlQuery);
			}
			catch
			{
				throw new ServerRequirementsNotMetException(ErrorMessage);
			}
		}

		const string sqlQuery = @"
DECLARE @OldServerName nvarchar(128);
DECLARE @NewServerName nvarchar(128);
DECLARE @SysServerName nvarchar(128);
DECLARE @Error nvarchar(max)

SELECT
	@OldServerName = @@SERVERNAME,
	@NewServerName = CONVERT(nvarchar(128), SERVERPROPERTY('ServerName'));

IF (@OldServerName != @NewServerName)
BEGIN
	SELECT @SysServerName = [name]
		FROM sys.servers
		WHERE server_id = 0;

	IF (@SysServerName != @NewServerName)
	BEGIN
		EXEC ('sp_dropserver ''' +  @SysServerName + '''');
		EXEC ('sp_addserver ''' + @NewServerName+''', ''local''');
	END;

	SET @Error =
		'This computer has been renamed after SQL Server was installed, ' +
		'hence the internal SQL Server name does not match the computer name. ' +
		'The problem has been rectified but to become effective, the SQL Server Service must be restarted. ' +
		'Please restart the service for [' + @NewServerName + '] and run this setup again.';
	RAISERROR (@Error, 16, 1);
END;
";
	}
}
