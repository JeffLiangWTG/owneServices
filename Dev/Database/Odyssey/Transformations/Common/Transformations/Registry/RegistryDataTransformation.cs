using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;
using WTG.Foundation.Cryptography;

namespace Enterprise.DbUpgrader.Transformation.Common
{
	/// <summary>
	/// Provides a base level class to perform transformations on the registry.
	/// </summary>
	public abstract class RegistryDataTransformation : DataTransformation
	{
		protected void ConvertPasswordToEncrypted(string oldPasswordRegistryItemName, string newPasswordRegistryItemName)
		{
			var passwordDataBytes = Helper.GetStmDataValue(oldPasswordRegistryItemName);
			if (passwordDataBytes == null)
			{
				return;
			}

			var randomGuid = Guid.NewGuid();

			var key = Encoding.ASCII.GetBytes("6052D90C81D64D5B8F5677AA055CAE23"); // The key used in TwoWayEncoder
			var iv = Encoding.ASCII.GetBytes(randomGuid.ToString().ToLowerInvariant().Replace("-", "").Substring(5, 16));

			var crypto = new AESCryptographicProvider(key, iv);
			var payload = crypto.Encrypt(passwordDataBytes);

			// this is intentional - the IV is a subset of the guid, but the whole guid gets written to the payload
			var output = randomGuid.ToByteArray().Concat(payload).ToArray();

			Helper.InsertStmDataRow(newPasswordRegistryItemName, "STR", output);
			Helper.DeleteStmDataRow(oldPasswordRegistryItemName);
		}

		public static void DeleteRegistryItemRows(params string[] registryItemNames)
		{
			if (registryItemNames.Length > 0)
			{
				using (var cmd = Db.Connection.Command(""))
				{
					var paramNames = new string[registryItemNames.Length];
					for (int i = 0; i < registryItemNames.Length; i++)
					{
						paramNames[i] = "@name" + i.ToString(CultureInfo.InvariantCulture);
						cmd.AddParameterBasedOnDbColumn(paramNames[i], registryItemNames[i], StmDataSchema.SD_Name);
					}

					cmd.CommandText = String.Format(@"DELETE dbo.StmData WHERE SD_Name in ({0});"
						, String.Join(", ", paramNames)
						);

					cmd.ExecuteNonQuery();
				}
			}
		}

		protected void CopyRows(string sourceRegistryItemName, string destinationRegistryItemName)
		{
			Helper.CopyRows(sourceRegistryItemName, destinationRegistryItemName);
		}

		protected internal DataTable GetDataTable(string registryItemName)
		{
			var resultTable = new DataTable();

			var sql = @"-- GetDataTable
SELECT
	SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_IsLogged, SD_BinaryValue, SD_GuidValue
FROM
	dbo.StmData
WHERE
	SD_Name = @SD_Name
;";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@SD_Name", registryItemName, StmDataSchema.SD_Name);
				using (var adapter = cmd.NewDataAdapter())
				{
					adapter.Fill(resultTable);
				}
			}

			return resultTable;
		}

		public static DataTable GetDataTableFull(string registryItemName)
		{
			var resultTable = new DataTable();
			resultTable.Locale = CultureInfo.InvariantCulture;

			var sql = @"-- GetDataTable
SELECT
	SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_IsLogged, SD_BinaryValue, SD_GuidValue, SD_IsCancelled, SD_PreserveTestValue
FROM
	dbo.StmData
WHERE
	SD_Name = @SD_Name
;";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@SD_Name", registryItemName, StmDataSchema.SD_Name);
				using (var adapter = cmd.NewDataAdapter())
				{
					adapter.Fill(resultTable);
				}
			}

			return resultTable;
		}

		protected internal void UpdateDatabaseValue(Guid pk, byte[] value)
		{
			var sql = @"-- UpdateDatabaseValue
UPDATE dbo.StmData SET
	SD_BinaryValue = @SD_BinaryValue
WHERE
	SD_PK = @SD_PK
;";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@SD_BinaryValue", value ?? (object)DBNull.Value, StmDataSchema.SD_BinaryValue);
				cmd.AddParameterBasedOnDbColumn("@SD_PK", pk, StmDataSchema.PK);
				cmd.ExecuteNonQuery();
			}
		}

		protected internal void UpdateDatabaseValue(Guid pk, string type, byte[] value)
		{
			var sql = @"-- UpdateDatabaseValue
UPDATE dbo.StmData SET
	SD_Type = @SD_Type,
	SD_BinaryValue = @SD_BinaryValue
WHERE
	SD_PK = @SD_PK
;";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@SD_BinaryValue", value ?? (object)DBNull.Value, StmDataSchema.SD_BinaryValue);
				cmd.AddParameterBasedOnDbColumn("@SD_PK", pk, StmDataSchema.PK);
				cmd.AddParameterBasedOnDbColumn("@SD_Type", type, StmDataSchema.SD_Type);
				cmd.ExecuteNonQuery();
			}
		}

		protected internal void UpdateDatabaseValue(string registryItemName, byte[] value)
		{
			var sql = @"-- UpdateDatabaseValue
UPDATE dbo.StmData SET
	SD_BinaryValue = @SD_BinaryValue
WHERE
	SD_Name = @SD_Name
;";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@SD_BinaryValue", value ?? (object)DBNull.Value, StmDataSchema.SD_BinaryValue);
				cmd.AddParameterBasedOnDbColumn("@SD_Name", registryItemName, StmDataSchema.SD_Name);
				cmd.ExecuteNonQuery();
			}
		}

		protected void ResetDatabaseValueWithLogging(string registryItemName)
		{
			var sql = @"-- ResetDatabaseValueWithLogging
				DECLARE @Updated table(
					SD_PK uniqueidentifier,
					SD_BinaryValue varbinary(max)
				);

				UPDATE dbo.StmData SET SD_BinaryValue = null, SD_GuidValue = null, SD_SystemLastEditUser = '~BP', SD_SystemLastEditTimeUtc = getUtcDate()
				OUTPUT deleted.SD_PK, deleted.SD_BinaryValue INTO @Updated
				WHERE SD_Name=@SD_Name AND SD_BinaryValue IS NOT NULL;

				INSERT INTO dbo.StmALog (SL_Table, SL_Parent, SL_Reference, SL_SE_NKEvent, SL_EventTime, SL_GS_NKUser)
				SELECT 'StmData', updated.SD_PK, SUBSTRING(cast(updated.SD_BinaryValue as nvarchar(max)), 0, 1023), 'RST', getDate(), '~BP'
				FROM @Updated updated;
			";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@SD_Name", registryItemName, StmDataSchema.SD_Name);
				cmd.ExecuteNonQuery();
			}
		}

		protected internal Guid InsertDatabaseValue(string registryItemName, Guid ownerPK, byte[] value)
		{
			return InsertDatabaseValue(registryItemName, ownerPK, null, value);
		}

		protected internal Guid InsertDatabaseValue(string registryItemName, Guid? ownerPK, Guid? departmentPK, byte[] value)
		{
			return InsertDatabaseValue(registryItemName, ownerPK, departmentPK, value, string.Empty);
		}

		protected internal Guid InsertDatabaseValue(string registryItemName, Guid? ownerPK, Guid? departmentPK, byte[] value, string type)
		{
			var pk = Guid.NewGuid();

			var sql = @"-- InsertDatabaseValue
INSERT dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_BinaryValue, SD_Type) VALUES
	(@SD_PK, @SD_Name, @SD_Owner, @SD_DepartmentGuid, @SD_BinaryValue, @SD_Type)
;";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@SD_PK", pk, StmDataSchema.PK);
				cmd.AddParameterBasedOnDbColumn("@SD_Name", registryItemName, StmDataSchema.SD_Name);
				cmd.AddParameterBasedOnDbColumn("@SD_Owner", (object)ownerPK ?? DBNull.Value, StmDataSchema.SD_Owner);
				cmd.AddParameterBasedOnDbColumn("@SD_DepartmentGuid", (object)departmentPK ?? DBNull.Value, StmDataSchema.SD_DepartmentGuid);
				cmd.AddParameterBasedOnDbColumn("@SD_BinaryValue", value, StmDataSchema.SD_BinaryValue);
				cmd.AddParameterBasedOnDbColumn("@SD_Type", type, StmDataSchema.SD_Type);

				cmd.ExecuteNonQuery();
			}

			return pk;
		}

		protected internal Guid InsertDatabaseValue(string registryItemName, byte[] value)
		{
			var pk = Guid.NewGuid();

			var sql = @"-- InsertDatabaseValue
INSERT dbo.StmData (SD_PK, SD_Name, SD_BinaryValue) VALUES
	(@SD_PK, @SD_Name, @SD_BinaryValue)
;";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@SD_PK", pk, StmDataSchema.PK);
				cmd.AddParameterBasedOnDbColumn("@SD_Name", registryItemName, StmDataSchema.SD_Name);
				cmd.AddParameterBasedOnDbColumn("@SD_BinaryValue", value, StmDataSchema.SD_BinaryValue);

				cmd.ExecuteNonQuery();
			}

			return pk;
		}

		protected internal void UpdateOrCreateDatabaseValue(string registryItemName, byte[] value)
		{
			var sql = @"-- UpdateOrCreateDatabaseValue
MERGE
	dbo.StmData AS target
USING
	(VALUES(NEWID(), @SD_Name, @SD_BinaryValue)) AS source(SD_PK, SD_Name, SD_BinaryValue) ON 1=1
		AND source.SD_Name = target.SD_Name
WHEN MATCHED THEN
	UPDATE SET
		SD_BinaryValue = source.SD_BinaryValue
WHEN NOT MATCHED BY TARGET THEN
	INSERT (SD_PK, SD_Name, SD_BinaryValue) VALUES
		(source.SD_PK, source.SD_Name, source.SD_BinaryValue)
;";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@SD_Name", registryItemName, StmDataSchema.SD_Name);
				cmd.AddParameterBasedOnDbColumn("@SD_BinaryValue", value, StmDataSchema.SD_BinaryValue);

				cmd.ExecuteNonQuery();
			}
		}

		protected internal void UpdateOrCreateDatabaseValue(string registryItemName, Guid? ownerPK, Guid? departmentPK, string type, bool isLogged, byte[] value, Guid? guidValue, bool isCancelled, bool isPreserveTestValue)
		{
			var sql = @"-- UpdateOrCreateDatabaseValue
MERGE
	dbo.StmData AS target
USING
	(VALUES(NEWID(), @SD_Name, @SD_Owner, @SD_DepartmentGuid, @SD_Type, @SD_IsLogged, @SD_BinaryValue, @SD_GuidValue, @SD_IsCancelled, @SD_PreserveTestValue))
AS source(SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_IsLogged, SD_BinaryValue, SD_GuidValue, SD_IsCancelled, SD_PreserveTestValue) ON 1=1
		AND source.SD_Name = target.SD_Name AND (source.SD_Owner = target.SD_Owner OR (source.SD_Owner IS NULL AND target.SD_Owner IS NULL))
WHEN MATCHED THEN
	UPDATE SET
		SD_DepartmentGuid = source.SD_DepartmentGuid, SD_Type = source.SD_Type, SD_IsLogged = source.SD_IsLogged, SD_BinaryValue = source.SD_BinaryValue,
		SD_GuidValue = source.SD_GuidValue, SD_IsCancelled = source.SD_IsCancelled, SD_PreserveTestValue = source.SD_PreserveTestValue, SD_SystemLastEditTimeUtc = getUtcDate(), SD_SystemLastEditUser = '~BP'
WHEN NOT MATCHED BY TARGET THEN
	INSERT (SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_IsLogged, SD_BinaryValue, SD_GuidValue, SD_IsCancelled, SD_PreserveTestValue, SD_SystemLastEditTimeUtc, SD_SystemLastEditUser) VALUES
		(source.SD_PK, source.SD_Name, source.SD_Owner, source.SD_DepartmentGuid, source.SD_Type, source.SD_IsLogged, source.SD_BinaryValue, source.SD_GuidValue, source.SD_IsCancelled, source.SD_PreserveTestValue, getUtcDate(), '~BP')
;";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@SD_Name", registryItemName, StmDataSchema.SD_Name);
				cmd.AddParameterBasedOnDbColumn("@SD_Owner", (object)ownerPK ?? DBNull.Value, StmDataSchema.SD_Owner);
				cmd.AddParameterBasedOnDbColumn("@SD_DepartmentGuid", (object)departmentPK ?? DBNull.Value, StmDataSchema.SD_DepartmentGuid);
				cmd.AddParameterBasedOnDbColumn("@SD_Type", type, StmDataSchema.SD_Type);
				cmd.AddParameterBasedOnDbColumn("@SD_IsLogged", isLogged, StmDataSchema.SD_IsLogged);
				cmd.AddParameterBasedOnDbColumn("@SD_BinaryValue", value, StmDataSchema.SD_BinaryValue);
				cmd.AddParameterBasedOnDbColumn("@SD_GuidValue", (object)guidValue ?? DBNull.Value, StmDataSchema.SD_GuidValue);
				cmd.AddParameterBasedOnDbColumn("@SD_IsCancelled", isCancelled, StmDataSchema.SD_IsCancelled);
				cmd.AddParameterBasedOnDbColumn("@SD_PreserveTestValue", isPreserveTestValue, StmDataSchema.SD_PreserveTestValue);

				cmd.ExecuteNonQuery();
			}
		}

		protected internal void UpdateRegistryItemName(string oldName, string newName)
		{
			var sql = @"-- UpdateRegistryItemName
If EXISTS(Select null From dbo.StmData Where SD_Name = @newName)
	Delete From dbo.StmData Where SD_Name = @oldName
Else
	UPDATE dbo.StmData SET SD_Name = @newName WHERE SD_Name = @oldName
;";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@newName", newName, StmDataSchema.SD_Name);
				cmd.AddParameterBasedOnDbColumn("@oldName", oldName, StmDataSchema.SD_Name);

				cmd.ExecuteNonQuery();
			}
		}

		#region Registry Transformation Helper

		protected RegistryTransformationHelper Helper => new();

		#endregion // Registry Transformation Helper
	}
}
