using System;
using CargoWise.Data;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	internal static class CopyProductionToTestHelper
	{
		internal const string ExceptionIfVersionIsLessThanMinOrNULL = "The min major version for {2} database should be {0}. The current version is {1}. Please upgrade {2} database and retry.";
		internal const string ExceptionIfDbSchemaIsGreaterThanAppVersion = "Application version is older than schema version of {0} database. Please use a later version of the application.";
		internal const string SchemaVersion = "DATABASE_SCHEMA_VERSION";
		internal const string MinorSchemaVersion = "DATABASE_MINOR_SCHEMA_VERSION";
		internal const string TransformationVersion = "DatabaseMajorTransformationVersion";
		internal const string MinorTransformationVersion = "DatabaseMinorTransformationVersion";

		internal static string GetStmData(DbConnection connection, string dbName, string name)
		{
			try
			{
				string selectStmData = String.Format(@"
If Exists(Select null From sys.databases Where name = '{0}' AND state = 0)
				SELECT TOP 1 convert(nvarchar(max), SD_BinaryValue) as textvalue
				FROM [{0}].[dbo].[StmData]
				WHERE SD_Name = '{1}'",
					dbName, name);

				object o = connection.ExecuteScalar(selectStmData);
				string result = (o == null) ? null : o.ToString().Trim();

				return result;
			}
			catch (SqlException ex)
			{
				var type = new DbErrorMatch(ex).ExceptionType;
				if (type == DbErrorType.InvalidColumnName || type == DbErrorType.InvalidObjectName || type == DbErrorType.SynonymRefersToAnInvalidObject)
				{
					return null;
				}
				else
				{
					throw;
				}
			}
		}
	}
}
