using System;
using CargoWise.Data;
using CargoWise.Licensing;

namespace Enterprise.ProductRegistration.Client
{
	public interface IDatabaseUniqueKeyProvider
	{
		DatabaseUniqueKey UniqueKey { get; }
	}

	public class DatabaseUniqueKeyProvider : IDatabaseUniqueKeyProvider
	{
		public DatabaseUniqueKey UniqueKey
		{
			get
			{
				if (uniqueKey == null || connectionLoginTime != Db.Connection.LoginTime)
				{
					connectionLoginTime = Db.Connection.LoginTime;
					uniqueKey = ReadKeyFromTheDatabase();
				}
				return uniqueKey;
			}
		}

		DateTime connectionLoginTime;
		DatabaseUniqueKey uniqueKey;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		DatabaseUniqueKey ReadKeyFromTheDatabase()
		{
			DatabaseUniqueKey result;

			const string sqlText =
@"SELECT ServerName = @@SERVERNAME,
	create_date,
	group_database_id
from sys.databases db
where db.Name = db_name()";

			using (var reader = Db.Connection.Command(sqlText).ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
			{
				if (reader.Read())
				{
					var serverName = reader.GetString(0);
					var databaseCreated = reader.GetDateTime(1);
					var group = reader[2];

					result = new DatabaseUniqueKey(serverName,
						Db.DatabaseName,
						databaseCreated,
						Db.ServerName,
						group != DBNull.Value ? (Guid?)group : null);
				}
				else
				{
					result = new DatabaseUniqueKey();
				}
			}

			return result;
		}
	}
}
