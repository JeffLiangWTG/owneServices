using System;
using System.Data;
using CargoWise.Common;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Data
{
	interface IDbRegistryItem<T>
	{
		string ItemName { get; }
		T LoadValue(DbConnection conn);
		void SaveValue(T value, DbConnection conn);
	}

	[Immutable]
	public abstract class BaseDbRegistryItem<T> : IDbRegistryItem<T>
	{
		protected BaseDbRegistryItem(bool preserveTestValue = false)
		{
			this.preserveTestValue = preserveTestValue;
		}

		protected abstract T GetValueFromBytes(byte[] binaryValue);
		protected abstract byte[] GetBytesFromValue(T value);
		protected abstract T DefaultValue { get; }
		protected abstract string TypeCode { get; }

		readonly bool preserveTestValue;

		#region IDbRegistryItem<T> Members

		public abstract string ItemName { get; }

		public T LoadValue(DbConnection conn)
		{
			CheckIsNotSystemDatabase(conn);

			byte[] binaryValue = LoadBinaryValueFromDatabase(conn);
			T result = (binaryValue == null) ? DefaultValue : GetValueFromBytes(binaryValue);
			return result;
		}

		public void SaveValue(T value, DbConnection conn)
		{
			CheckIsNotSystemDatabase(conn);

			byte[] binaryValue = GetBytesFromValue(value);
			SaveBinaryValueToDatabase(binaryValue, conn);
		}

		void CheckIsNotSystemDatabase(DbConnection conn)
		{
			var currentDb = conn.CurrentDatabase;

			if (
				IsCurrentDb(Db.SqlMasterDb)
				|| IsCurrentDb(Db.SqlMsdb)
				|| IsCurrentDb("tempdb")
				|| IsCurrentDb("model")
			)
			{
				throw new RegistryAccessFromSystemDatabaseException(currentDb);
			}

			bool IsCurrentDb(string systemDbName) => currentDb.Equals(systemDbName, StringComparison.OrdinalIgnoreCase);
		}

		#endregion

		#region Load From and Save To Database

		byte[] LoadBinaryValueFromDatabase(DbConnection conn)
		{
			Argument.NotNull(conn, nameof(conn)); // Suggested By ReviewBot

			byte[] result = null;

			string sqlText = @"
				SELECT SD_BinaryValue
				FROM dbo.StmData
				WHERE SD_Name = @Name
				AND SD_Owner is null
				AND SD_DepartmentGuid is null";

			using (DbCommand cmd = conn.Command(sqlText))
			{
				cmd.AddParameter("@Name", SqlDbType.VarChar, 300, ItemName);
				result = cmd.ExecuteScalar() as byte[];
			}

			return result;
		}

		void SaveBinaryValueToDatabase(byte[] binaryValue, DbConnection conn)
		{
			Argument.NotNull(conn, nameof(conn)); // Suggested By ReviewBot

			string sqlText;

			if (binaryValue == null)
			{
				sqlText = @"
					DELETE dbo.StmData
					WHERE SD_Name = @Name
					AND SD_Owner is null
					AND SD_DepartmentGuid is null";
			}
			else
			{
				sqlText = @"
					UPDATE dbo.StmData
						SET
							SD_Type = @Type,
							SD_BinaryValue = @BinaryValue,
							SD_PreserveTestValue = @PreserveTestValue,
							SD_SystemLastEditTimeUtc = GetUtcDate(),
							SD_SystemLastEditUser = @CurrentUser
						WHERE
							SD_Name = @Name

					IF (@@rowcount = 0)
					BEGIN
						INSERT dbo.StmData (SD_PK, SD_Name, SD_Type, SD_BinaryValue, SD_GuidValue, SD_IsCancelled, SD_IsLogged, SD_PreserveTestValue, SD_SystemCreateTimeUtc, SD_SystemCreateUser)
						VALUES (NEWID(), @Name, @Type, @BinaryValue, '00000000-0000-0000-0000-000000000000', 0, 0, @PreserveTestValue, GetUtcDate(), @CurrentUser)
					END";
			}

			using (DbCommand sqlCmd = conn.Command(sqlText))
			{
				sqlCmd.AddParameter("@Name", SqlDbType.VarChar, 300, ItemName);
				sqlCmd.AddParameter("@Type", SqlDbType.Char, TypeCode);
				sqlCmd.AddParameter("@CurrentUser", SqlDbType.VarChar, 3, Db.GetCurrentUserOrDefault(defaultUser: "~BP"));

				if (binaryValue != null)
				{
					sqlCmd.AddParameter("@BinaryValue", SqlDbType.VarBinary, binaryValue);
				}

				sqlCmd.AddParameter("@PreserveTestValue", SqlDbType.Bit, preserveTestValue);

				sqlCmd.ExecuteNonQuery();
			}
		}

		#endregion
	}
}
