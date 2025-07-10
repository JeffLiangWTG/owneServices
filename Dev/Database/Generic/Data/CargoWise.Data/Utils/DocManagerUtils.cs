using System;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Common.Data;

namespace CargoWise.Data
{
	public static class DocManagerUtils
	{
		public static void AlterDbWriteableStateForDocManager(this DbConnection connection, string dbName, bool writeable)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			CheckDbNameForDocManager(dbName);

			var alreadyWriteable = IsDbWriteableForDocManager(dbName);

			if (writeable && !alreadyWriteable)
			{
				var sqlDropTrigger = FormattableString.Invariant($@"
					EXEC [{dbName}]..sp_executesql N'
						DROP TRIGGER {DatabaseConstants.TriggerNameToBlockInsertUpdateDeleteForDocManager}
					'");
				using (var cmd = connection.Command(sqlDropTrigger))
				{
					cmd.ExecuteNonQuery();
				}
			}
			else if (!writeable && alreadyWriteable)
			{
				var sqlCreateTrigger = FormattableString.Invariant($@"
					EXEC [{dbName}]..sp_executesql N'
						CREATE TRIGGER [{DatabaseConstants.TriggerNameToBlockInsertUpdateDeleteForDocManager}]
						ON {dbName}.dbo.StorageDocs FOR INSERT, UPDATE, DELETE
						AS
						BEGIN
						IF (@@rowcount = 0) RETURN
						SET NOCOUNT ON
						RAISERROR(''{dbName} has been made to ReadOnly.'',16,1)
						ROLLBACK TRANSACTION
						RETURN
						END'");
				using (var cmd = connection.Command(sqlCreateTrigger))
				{
					cmd.ExecuteNonQuery();
				}
			}
		}

		public static bool IsDbWriteableForDocManager(string dbName)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			CheckDbNameForDocManager(dbName);
			var objectName = FormattableString.Invariant($"{dbName}..{DatabaseConstants.TriggerNameToBlockInsertUpdateDeleteForDocManager}");
			return !DataUtils.ObjectExists(Db.Connection, objectName);
		}

		static void CheckDbNameForDocManager(string dbName)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			if (!IsDocManagerDatabase(dbName))
			{
				throw new ArgumentException(FormattableString.Invariant($"{nameof(dbName)} should be a DocManager database, something like {Db.DatabaseName}{Db.SDDatabaseAffix}XXX, but it is {dbName}."));
			}
		}

		public static bool IsDocManagerDatabase(string dbName)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			return Regex.Match(dbName, FormattableString.Invariant($@"{Db.DatabaseName}{Db.SDDatabaseAffix}\d{{3}}$"), RegexOptions.IgnoreCase).Success;
		}
	}
}
