using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using CargoWise.Data;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformation.Common
{
	public class RegistryTransformationHelper
	{
		public int GetStmDataRowCount(string registryName)
		{
			var sql = "SELECT COUNT(*) FROM dbo.StmData WHERE SD_Name = @SD_Name;";
			return Db.Connection.ExecuteScalar<int>(sql, cmd => cmd.AddParameterBasedOnDbColumn("@SD_Name", registryName, StmDataSchema.SD_Name));
		}

		public void CopyRows(string sourceRegistryItemName, string destinationRegistryItemName)
		{
			var sql = @"-- CopyRows
INSERT dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_IsLogged, SD_BinaryValue, SD_GuidValue)
SELECT
	NEWID(), @target_name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_IsLogged, SD_BinaryValue, SD_GuidValue
FROM
	dbo.StmData AS source
WHERE 1=1
	AND SD_Name = @source_name
	AND NOT EXISTS
	(
		SELECT NULL
		FROM dbo.StmData AS target
		WHERE 1=1
			AND target.SD_Name = @target_name
			AND
			(1=2
				OR target.SD_Owner is NULL AND source.SD_Owner is NULL
				OR target.SD_Owner = source.SD_Owner
			)
			AND
			(1=2
				OR target.SD_DepartmentGuid is NULL AND source.SD_DepartmentGuid is NULL
				OR target.SD_DepartmentGuid = source.SD_DepartmentGuid
			)
	)
";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@source_name", sourceRegistryItemName, StmDataSchema.SD_Name);
				cmd.AddParameterBasedOnDbColumn("@target_name", destinationRegistryItemName, StmDataSchema.SD_Name);

				cmd.ExecuteNonQuery();
			}
		}

		#region Get StmData Value

		public byte[] GetStmDataValue(string name)
		{
			return GetStmDataValue(name, Guid.Empty);
		}

		public byte[] GetStmDataValue(string name, Guid owner)
		{
			return GetStmDataValue(name, owner, Guid.Empty);
		}

		public byte[] GetStmDataValue(string name, Guid owner, Guid department)
		{
			var sql = string.Format(@"-- GetStmDataValue
SELECT
	SD_BinaryValue
FROM
	dbo.StmData
WHERE 1=1
	AND SD_Name = @SD_Name
	AND {0}
	AND {1}
"
				, (owner == Guid.Empty) ? "SD_Owner is NULL" : "SD_Owner = @SD_Owner"
				, (department == Guid.Empty) ? "SD_DepartmentGuid is NULL" : "SD_DepartmentGuid = @SD_DepartmentGuid"
				);

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@SD_Name", name, StmDataSchema.SD_Name);

				if (owner != Guid.Empty)
				{
					cmd.AddParameterBasedOnDbColumn("@SD_Owner", owner, StmDataSchema.SD_Owner);
				}

				if (department != Guid.Empty)
				{
					cmd.AddParameterBasedOnDbColumn("@SD_DepartmentGuid", department, StmDataSchema.SD_DepartmentGuid);
				}

				var data = cmd.ExecuteScalar();
				return Convert.IsDBNull(data) ? null : (byte[])data;
			}
		}

		public byte[] GetStmDataValue(Guid primaryKey, DbConnection connection = null)
		{
			object binaryValue;
			var sqlText = @"SELECT SD_BinaryValue FROM dbo.StmData WHERE SD_PK = @PK";

			using (var cmd = (connection ?? Db.Connection).Command(sqlText))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, primaryKey);
				binaryValue = cmd.ExecuteScalar();
			}

			return (binaryValue == DBNull.Value) ? null : (byte[])binaryValue;
		}

		#endregion

		#region Delete dbo.StmData Row

		public void DeleteStmDataRow(string sD_Name)
		{
			DeleteStmDataRow(sD_Name, Guid.Empty);
		}

		public void DeleteStmDataRow(string sD_Name, Guid sD_Owner)
		{
			DeleteStmDataRow(sD_Name, sD_Owner, Guid.Empty);
		}

		public void DeleteStmDataRow(string sD_Name, Guid sD_Owner, Guid sD_DepartmentGuid)
		{
			var sql = String.Format(@"-- DeleteStmDataRow
DELETE dbo.StmData
WHERE 1=1
	AND SD_Name = @SD_Name
	AND {0}
	AND {1}
"
				, (sD_Owner == Guid.Empty) ? "SD_Owner is NULL" : "SD_Owner = @SD_Owner"
				, (sD_DepartmentGuid == Guid.Empty) ? "SD_DepartmentGuid is NULL" : "SD_DepartmentGuid = @SD_DepartmentGuid"
				);

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@SD_Name", sD_Name, StmDataSchema.SD_Name);

				if (sD_Owner != Guid.Empty)
				{
					cmd.AddParameterBasedOnDbColumn("@SD_Owner", sD_Owner, StmDataSchema.SD_Owner);
				}

				if (sD_DepartmentGuid != Guid.Empty)
				{
					cmd.AddParameterBasedOnDbColumn("@SD_DepartmentGuid", sD_DepartmentGuid, StmDataSchema.SD_DepartmentGuid);
				}

				cmd.ExecuteNonQuery();
			}
		}

		public void DeleteStmDataRow(Guid sD_PK)
		{
			var sql = "DELETE dbo.StmData WHERE SD_PK = @SD_PK;";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@SD_PK", sD_PK, StmDataSchema.PK);

				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region Insert dbo.StmData Row

		public Guid InsertStmDataRow(string sD_Name, string sD_Type, byte[] binaryValue)
		{
			return InsertStmDataRow(sD_Name, Guid.Empty, Guid.Empty, sD_Type, binaryValue);
		}

		public Guid InsertStmDataRow(string sD_Name, Guid sD_Owner, string sD_Type, byte[] binaryValue)
		{
			return InsertStmDataRow(sD_Name, sD_Owner, Guid.Empty, sD_Type, binaryValue);
		}

		public Guid InsertStmDataRow(string sD_Name, Guid sD_Owner, Guid sD_DepartmentGuid, string sD_Type, byte[] binaryValue)
		{
			return InsertStmDataRow(sD_Name, sD_Owner, sD_DepartmentGuid, sD_Type, StmDataSchema.SD_BinaryValue, binaryValue);
		}

		public Guid InsertStmDataRow(string sD_Name, Guid sD_Owner, Guid value)
		{
			return InsertStmDataRow(sD_Name, sD_Owner, Guid.Empty, "GID", StmDataSchema.SD_GuidValue, value);
		}

		public Guid InsertStmDataRow(string sD_Name, Guid sD_Owner, Guid sD_DepartmentGuid, bool value)
		{
			return InsertStmDataRowCore(sD_Name, sD_Owner, sD_DepartmentGuid, "BOL", value ? "True" : "False");
		}

		public Guid InsertStmDataRow(string sD_Name, Guid sD_Owner, Guid sD_DepartmentGuid, string value)
		{
			return InsertStmDataRowCore(sD_Name, sD_Owner, sD_DepartmentGuid, "STR", value);
		}

		Guid InsertStmDataRowCore(string sD_Name, Guid sD_Owner, Guid sD_DepartmentGuid, string type, string value)
		{
			return InsertStmDataRow(sD_Name, sD_Owner, sD_DepartmentGuid, type, Encoding.Unicode.GetBytes(value));
		}

		public Guid InsertStmDataRow(string sD_Name, Guid sD_Owner, Guid sD_DepartmentGuid, string sD_Type, SchemaColumn schemaColumn, object value)
		{
			var pk = Guid.NewGuid();
			var sql = String.Format(@"-- InsertStmDataRow
INSERT dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, {0}) VALUES
	(@SD_PK, @SD_Name, @SD_Owner, @SD_DepartmentGuid, @SD_Type, @Value)
;"
				, schemaColumn.Name
				);

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@SD_PK", pk, StmDataSchema.PK);
				cmd.AddParameterBasedOnDbColumn("@SD_Name", sD_Name, StmDataSchema.SD_Name);
				cmd.AddParameterBasedOnDbColumn("@SD_Owner", (sD_Owner == Guid.Empty) ? DBNull.Value : sD_Owner, StmDataSchema.SD_Owner);
				cmd.AddParameterBasedOnDbColumn("@SD_DepartmentGuid", (sD_DepartmentGuid == Guid.Empty) ? DBNull.Value : sD_DepartmentGuid, StmDataSchema.SD_DepartmentGuid);
				cmd.AddParameterBasedOnDbColumn("@SD_Type", sD_Type, StmDataSchema.SD_Type);
				cmd.AddParameterBasedOnDbColumn("@Value", value ?? DBNull.Value, schemaColumn);

				cmd.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		public List<Guid> GetCompanyPKs()
		{
			List<Guid> companyPKs = new List<Guid>();
			using (DbCommand cmd = Db.Connection.Command("SELECT GC_PK FROM dbo.GlbCompany"))
			{
				using (IDataReader reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						companyPKs.Add((Guid)reader["GC_PK"]);
					}
				}
			}
			return companyPKs;
		}

		public Guid? GetCompanyPKFromBranch(Guid branchPK)
		{
			var sqlCommand = "SELECT GB_GC FROM dbo.GlbBranch WHERE GB_PK = @pk";
			using (var cmd = Db.Connection.Command(sqlCommand))
			{
				cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, branchPK);

				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						return reader.GetGuid(0);
					}
					return null;
				}
			}
		}

		public List<Guid> GetBranchPKs(Guid companyPK)
		{
			List<Guid> branches = new List<Guid>();
			using (DbCommand cmd = Db.Connection.Command(string.Format("SELECT GB_PK FROM dbo.GlbBranch WHERE GB_GC = '{0}'", companyPK)))
			{
				using (IDataReader reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						branches.Add((Guid)reader["GB_PK"]);
					}
				}
			}
			return branches;
		}

		public List<string> GetBranchCodes(Guid companyPK)
		{
			List<string> branches = new List<string>();
			using (DbCommand cmd = Db.Connection.Command(string.Format("SELECT GB_Code FROM dbo.GlbBranch WHERE GB_GC = '{0}'", companyPK)))
			{
				using (IDataReader reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						branches.Add((string)reader["GB_Code"]);
					}
				}
			}
			return branches;
		}

		public List<string> GetDepartmentCodes()
		{
			List<string> departments = new List<string>();
			using (DbCommand cmd = Db.Connection.Command("SELECT GE_Code FROM dbo.GlbDepartment"))
			{
				using (IDataReader reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						departments.Add((string)reader["GE_Code"]);
					}
				}
			}
			return departments;
		}

		public Dictionary<Guid, byte[]> GetValuesByName(string registryItemNames)
		{
			var result = new Dictionary<Guid, byte[]>();
			var sqlText = "SELECT [SD_PK] AS [Key], [SD_BinaryValue] AS [Value] FROM [dbo].[StmData] WHERE ([SD_Name] = @Name)";

			using (var cmd = Db.Connection.Command(sqlText))
			{
				cmd.AddParameterBasedOnDbColumn("@name", registryItemNames, StmDataSchema.SD_Name);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var key = (Guid)reader["Key"];
						var value = (reader["Value"] == DBNull.Value) ? null : (byte[])reader["Value"];

						result.Add(key, value);
					}
				}
			}

			return result;
		}
		
		public RegistryLog AssertRegistryLogRecord(string registryName, string registryValue)
		{
			var sql = "SELECT TOP 1 SL_Table, SL_Reference, SL_EventTime, SL_GS_NKUser, SL_EventTimeUtc " +
					 "FROM [dbo].[StmALog] " +
					 "INNER JOIN [dbo].[StmData] ON SL_Parent = SD_PK " +
					 "WHERE [SD_Name] = @SD_Name " +
					 "ORDER BY SL_EventTime DESC";

			using var cmd = Db.Connection.Command(sql);
			cmd.AddParameter("@SD_Name", SqlDbType.VarChar, registryName);

			using var reader = cmd.ExecuteReader();
			if (reader.Read())
			{
				var registryLog = new RegistryLog
				{
					Table = (string)reader["SL_Table"],
					Reference = (string)reader["SL_Reference"],
					EventTime = (DateTime)reader["SL_EventTime"],
					User = (string)reader["SL_GS_NKUser"],
					EventTimeUtc = (DateTime)reader["SL_EventTimeUtc"]
				};

				return registryLog;
			}

			return null;
		}

		public class RegistryLog
		{
			public string Table { get; set; }
			public string Reference { get; set; }
			public DateTime EventTime { get; set; }
			public string User { get; set; }
			public DateTime EventTimeUtc { get; set; }
		}
	}
}
