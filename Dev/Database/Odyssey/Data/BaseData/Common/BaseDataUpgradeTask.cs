namespace Enterprise.DbUpgrader.Data.BaseData.Common
{
	using System;
	using CargoWise.Data;
	using CargoWise.Schema;

	public abstract class BaseDataUpgradeTask : EmbeddedUpgradeTask
	{
		public BaseDataUpgradeTask(BaseDataFile resourceFile)
			: base(resourceFile)
		{
		}

		/// <summary>
		/// Base Data Upgrades should NOT delete existing records
		/// </summary>
		protected override sealed void DoDelete(System.Data.DataRow targetRow, ref int targetIndex)
		{
			// Walk to next row on the target DataSet
			targetIndex++;
		}

		public static bool SameNkExists(SchemaStringColumn nkColumn, string nkValue)
		{
			string sqlText = String.Format("SELECT TOP 1 {0} FROM {1} WHERE {0} = '{2}'", nkColumn.Name, nkColumn.TableName, nkValue);
			return IsRecordInDatabase(sqlText);
		}

		public static bool SamePivotNkExists(SchemaGuidColumn nkGuidColumn1, Guid nkValue1, SchemaGuidColumn nkGuidColumn2, Guid nkValue2)
		{
			string sqlText = String.Format(
				"SELECT TOP 1 {0} FROM {1} WHERE {0} = '{2}' AND {3} = '{4}'",
				nkGuidColumn1.Name, nkGuidColumn1.TableName, nkValue1.ToString(), nkGuidColumn2.Name, nkValue2.ToString());
			return IsRecordInDatabase(sqlText);
		}

		public static bool FkIsNullOrReferencedPkExists(SchemaPKColumn pkColumn, object fkValue)
		{
			bool result = true;

			if (fkValue is Guid)
			{
				string sqlText = String.Format("SELECT TOP 1 {0} FROM {1} WHERE {0} = '{2}'", pkColumn.Name, pkColumn.TableName, fkValue.ToString());
				result = IsRecordInDatabase(sqlText);
			}

			return result;
		}

		public static bool FkReferencedPkExists(SchemaPKColumn pkColumn, object fkValue)
		{
			bool result = false;

			if (fkValue is Guid)
			{
				string sqlText = String.Format("SELECT TOP 1 {0} FROM {1} WHERE {0} = '{2}'", pkColumn.Name, pkColumn.TableName, fkValue.ToString());
				result = IsRecordInDatabase(sqlText);
			}

			return result;
		}

		public static bool IsRecordInDatabase(string oneColumnTopOneSqlQuery)
		{
			object objResult = Db.Connection.ExecuteScalar(oneColumnTopOneSqlQuery);
			return (objResult != null && objResult != DBNull.Value);
		}
	}
}

#region Test
#if DEBUG

namespace Enterprise.DbUpgrader.Data.BaseData.Common
{
	using System;
	using CargoWise.Data;
	using CargoWise.Schema;

	public static class BaseDataUpgradeTaskTestHelper
	{
		public static bool IsPkInDatabase(DbConnection connection, SchemaGuidColumn pkColumn, Guid pkValue)
		{
			string sqlText = String.Format("SELECT {0} FROM {1} WHERE {0} = '{2}'", pkColumn.Name, pkColumn.TableName, pkValue.ToString());
			return IsRecordInDatabase(connection, sqlText);
		}

		public static bool IsRecordInDatabase(DbConnection connection, string sqlText)
		{
			object objResult = connection.ExecuteScalar(sqlText);
			return (objResult != null && objResult != DBNull.Value);
		}

		public static string GetValueFieldFromPk(DbConnection connection, SchemaPKColumn pkColumn, Guid pkValue, SchemaStringColumn valueColumn)
		{
			string sqlText = String.Format("SELECT {0} FROM {1} WHERE {2} = '{3}'", valueColumn.Name, pkColumn.TableName, pkColumn.Name, pkValue.ToString());
			object objResult = connection.ExecuteScalar(sqlText);
			return (objResult == null || objResult == DBNull.Value) ? null : objResult.ToString();
		}

		public static bool GetValueFieldFromPk(DbConnection connection, SchemaPKColumn pkColumn, Guid pkValue, SchemaBoolColumn valueColumn)
		{
			string sqlText = String.Format("SELECT {0} FROM {1} WHERE {2} = '{3}'", valueColumn.Name, pkColumn.TableName, pkColumn.Name, pkValue.ToString());
			object objResult = connection.ExecuteScalar(sqlText);
			return !(objResult == DBNull.Value) && (bool)objResult;
		}

		public static Guid GetValueFieldFromPk(DbConnection connection, SchemaPKColumn pkColumn, Guid pkValue, SchemaGuidColumn valueColumn)
		{
			string sqlText = String.Format("SELECT {0} FROM {1} WHERE {2} = '{3}'", valueColumn.Name, pkColumn.TableName, pkColumn.Name, pkValue.ToString());
			object objResult = connection.ExecuteScalar(sqlText);
			return (objResult == null || objResult == DBNull.Value) ? Guid.Empty : (Guid)objResult;
		}
	}
}

#endif
#endregion
