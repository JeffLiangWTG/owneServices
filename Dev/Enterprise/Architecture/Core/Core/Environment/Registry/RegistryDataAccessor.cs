using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Environment
{
	public class RegistryDataAccessor : IDisposable
	{
		protected RegistryDataAccessor(IDisposable disposable, DbConnection connection)
		{
			Argument.NotNull(connection, "connection");

			this.disposable = disposable;
			Connection = connection;
		}

		public static RegistryDataAccessor DisposableInstance
		{
			get
			{
				var disposable = Db.DisposableActionForDbConnection();
				var connection = Db.Connection;
				return new RegistryDataAccessor(disposable, connection);
			}
		}

		public static RegistryDataAccessor CreateDisposableInstance(DbConnection connection)
		{
			return new RegistryDataAccessor(Db.DisposableActionForDbConnection(), connection);
		}

		#region GetBinaryValue / SetBinaryValue

		public byte[] GetBinaryValue(string name, Guid owner, Guid department)
		{
			return GetBinaryValue(name, TimeSpan.Zero, owner, department);
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]

		//static 
		internal byte[] GetBinaryValue(string name, TimeSpan maximumCacheAge, Guid owner, Guid department)
		{
			if (disabled || Db.IsDatabaseUpgraded)
			{
				return null;
			}
			byte[] result;
			using (DbCommand command = Connection.Command($"{Db.DatabaseName.QuoteName()}.[dbo].{ProcedureNameGetValueNOD.QuoteName()}"))
			{
				command.CommandType = CommandType.StoredProcedure;
				try
				{
					command.AddParameterBasedOnDbColumn("@Name", name, StmDataSchema.SD_Name);
					if (owner != Guid.Empty)
					{
						command.AddParameter("@Owner", SqlDbType.UniqueIdentifier, owner);
					}

					if (department != Guid.Empty)
					{
						command.AddParameter("@Department", SqlDbType.UniqueIdentifier, department);
					}

					result = command.ExecuteScalar() as byte[];
				}
				catch (System.Data.Common.DbException e)
				{
					var resultTuple = HandleSqlExceptionGettingBinaryValue(command, e);
					if (resultTuple.Handled)
					{
						return resultTuple.BinaryValue;
					}
					throw;
				}
			}

			return result;
		}

		internal (bool Handled, byte[] BinaryValue) HandleSqlExceptionGettingBinaryValue(DbCommand command, System.Data.Common.DbException e)
		{
			var errorType = new DbErrorHandler(e, Connection).ExceptionType;

			if (errorType == DbErrorType.CouldNotFindStoredProcedure)
			{
				return (true, TryGetBinaryValueFromStmData(command));
			}

			return (false, null);
		}

		byte[] TryGetBinaryValueFromStmData(DbCommand command)
		{
			command.CommandType = CommandType.Text;
			command.CommandText = FormattableString.Invariant($"SELECT SD_BinaryValue FROM {Db.DatabaseName.QuoteName()}.[dbo].[StmData] WHERE SD_Name = @Name");
			return command.ExecuteScalar() as byte[];
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Part of SQLExpression, \" AND \" + StmDataSchema.Constants.SD_Owner + \" is null \"\tTrue\tSystem.Collections.Generic.List`1[CargoWise.Tools.SpellCheck.ISpellingError]\tPart of SQLExpression\tC:\\Dev\\Enterprise\\Architecture\\Core\\Core\\Environment\\Registry\\RegistryDataAccessor.cs")]
		internal void DeleteRecord(string name, Guid ownerPK, Guid departmentPK)
		{
			if (name.Length > NameMaxLength)
			{
				throw new ArgumentException("Name of the key cannot be longer than " + NameMaxLength, nameof(name));
			}

#if DEBUG
			if (Globals.IsTest &&
				!Globals.InTransactionedTestCase &&
				!CargoWise.Data.Testing.UseSnapshotProtectionAttribute.IsProtected)
			{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
				bool testIsRunningOnADummyDatabaseOrSecondAppDomain = Connection.CurrentDatabase != Db.DatabaseName || !AppDomain.CurrentDomain.IsDefaultAppDomain();
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.

				// Tests like DB Upgrader that make their own dummy database can do what they like.
				// Tests running on the real database must use a transactioned test case.
				if (!testIsRunningOnADummyDatabaseOrSecondAppDomain)
				{
					throw new Exception("Attempted to delete a registry item from a unit test that was not in a TransactionedTestCase.");
				}
			}
#endif

			var ownerWhereSql = (ownerPK == Guid.Empty) ? (" AND " + StmDataSchema.Constants.SD_Owner + " is null ") : (" AND " + StmDataSchema.Constants.SD_Owner + " = @SD_Owner ");
			var departmentWhereSql = (departmentPK == Guid.Empty) ? (" AND " + StmDataSchema.Constants.SD_DepartmentGuid + " is null ") : (" AND " + StmDataSchema.Constants.SD_DepartmentGuid + " = @SD_DepartmentGuid ");

			var deleteSql = string.Format(@" 
				DELETE FROM dbo.StmData
					WHERE SD_Name = @SD_Name {0}{1}",
					ownerWhereSql,
					departmentWhereSql);

			using (DbCommand command = Connection.Command(deleteSql))
			{
				command.AddParameterBasedOnDbColumn("@SD_Name", name, StmDataSchema.SD_Name);

				if (ownerPK != Guid.Empty)
				{
					command.AddParameterBasedOnDbColumn("@SD_Owner", ownerPK, StmDataSchema.SD_Owner);
				}
				if (departmentPK != Guid.Empty)
				{
					command.AddParameterBasedOnDbColumn("@SD_DepartmentGuid", departmentPK, StmDataSchema.SD_DepartmentGuid);
				}

				command.ExecuteNonQuery();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		internal void SetBinaryValue(string name, Guid ownerPK, Guid departmentPK, byte[] binaryValue, Guid guidValue, string dataType, bool isLogged, bool preserveTestValue)
		{
			if (name.Length > NameMaxLength)
			{
				throw new ArgumentException("Name of the key cannot be longer than " + NameMaxLength, nameof(name));
			}

#if DEBUG
			if (Globals.IsTest &&
				!Globals.InTransactionedTestCase &&
				!CargoWise.Data.Testing.UseSnapshotProtectionAttribute.IsProtected)
			{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
				bool testIsRunningOnADummyDatabaseOrSecondAppDomain = Connection.CurrentDatabase != Db.DatabaseName || !AppDomain.CurrentDomain.IsDefaultAppDomain();
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.

				// Tests like DB Upgrader that make their own dummy database can do what they like.
				// Tests running on the real database must use a transactioned test case.
				if (!testIsRunningOnADummyDatabaseOrSecondAppDomain)
				{
					throw new InvalidOperationException("Attempted to set a registry item from a unit test that was not in a TransactionedTestCase, UseSnapshotProtection, dummy database or second appdomain.");
				}
			}
#endif

			string ownerWhereSql;
			string ownerInsertSql = string.Empty;
			string ownerInsertValueSql = string.Empty;
			if (ownerPK == Guid.Empty)
			{
				ownerWhereSql = " AND " + StmDataSchema.Constants.SD_Owner + " is null ";
			}
			else
			{
				ownerWhereSql = " AND " + StmDataSchema.Constants.SD_Owner + " = @SD_Owner ";
				ownerInsertSql = ", " + StmDataSchema.Constants.SD_Owner + " ";
				ownerInsertValueSql = ", @SD_Owner ";
			}

			string departmentWhereSql;
			string departmentInsertSql = string.Empty;
			string departmentInsertValueSql = string.Empty;
			if (departmentPK == Guid.Empty)
			{
				departmentWhereSql = " AND " + StmDataSchema.Constants.SD_DepartmentGuid + " is null ";
			}
			else
			{
				departmentWhereSql = " AND " + StmDataSchema.Constants.SD_DepartmentGuid + " = @SD_DepartmentGuid ";
				departmentInsertSql = ", " + StmDataSchema.Constants.SD_DepartmentGuid + " ";
				departmentInsertValueSql = ", @SD_DepartmentGuid ";
			}

			var lastEditColumnsInsertSql = string.Empty;
			var lastEditColumnsInsertValueSql = string.Empty;
			var lastEditColumnsUpdateValueSql = string.Empty;

			lastEditColumnsInsertSql = ", " + StmDataSchema.Constants.SD_SystemLastEditTimeUtc + ", " + StmDataSchema.Constants.SD_SystemLastEditUser;
			lastEditColumnsInsertValueSql = ", @SD_SystemLastEditTimeUtc, @SD_SystemLastEditUser";
			lastEditColumnsUpdateValueSql = FormattableString.Invariant($", {StmDataSchema.Constants.SD_SystemLastEditTimeUtc} = @SD_SystemLastEditTimeUtc, {StmDataSchema.Constants.SD_SystemLastEditUser} = @SD_SystemLastEditUser");

			string updateSql;

			if (binaryValue == null)
			{
				updateSql = string.Format(CultureInfo.InvariantCulture, @"
UPDATE dbo.StmData
	SET SD_Type = @SD_Type, SD_BinaryValue = NULL, SD_GuidValue = @SD_GuidValue, SD_IsLogged = @SD_IsLogged {2}
	WHERE SD_Name = @SD_Name {0}{1}",
					ownerWhereSql,
					departmentWhereSql,
					lastEditColumnsUpdateValueSql);
			}
			else
			{
				var preserveTestValueInsertSql = ", " + StmDataSchema.Constants.SD_PreserveTestValue + " ";
				var preserveTestValueInsertValueSql = ", @SD_PreserveTestValue ";
				var preserveTestValueUpdateValueSql = FormattableString.Invariant($", {StmDataSchema.Constants.SD_PreserveTestValue} = @SD_PreserveTestValue ");

				var createColumnsInsertSql = string.Empty;
				var createColumnsInsertValueSql = string.Empty;
				createColumnsInsertSql = ", " + StmDataSchema.Constants.SD_SystemCreateTimeUtc + ", " + StmDataSchema.Constants.SD_SystemCreateUser;
				createColumnsInsertValueSql = ", @SD_SystemCreateTimeUtc, @SD_SystemCreateUser";

				updateSql = string.Format(CultureInfo.InvariantCulture, @"
UPDATE dbo.StmData
	SET SD_Type = @SD_Type, SD_BinaryValue = @SD_BinaryValue, SD_GuidValue = @SD_GuidValue, SD_IsLogged = @SD_IsLogged {8} {11}
	WHERE SD_Name = @SD_Name {0}{1}
IF (@@rowcount = 0)
BEGIN
	INSERT dbo.StmData (SD_PK, SD_Name {2}{3}, SD_Type, SD_BinaryValue, SD_GuidValue, SD_IsLogged {6} {9} {12})
		VALUES (NEWID(), @SD_Name {4}{5}, @SD_Type, @SD_BinaryValue, @SD_GuidValue, @SD_IsLogged {7} {10} {13})
END
",
					ownerWhereSql,
					departmentWhereSql,
					ownerInsertSql,
					departmentInsertSql,
					ownerInsertValueSql,
					departmentInsertValueSql,
					preserveTestValueInsertSql,
					preserveTestValueInsertValueSql,
					preserveTestValueUpdateValueSql,
					lastEditColumnsInsertSql,
					lastEditColumnsInsertValueSql,
					lastEditColumnsUpdateValueSql,
					createColumnsInsertSql,
					createColumnsInsertValueSql);
			}

			var createOrEditTime = DateTime.UtcNow;
			var userCode = EnvProxy.Instance.CurrentUser?.Initials ?? string.Empty;

			using (DbCommand command = Connection.Command(updateSql))
			{
				command.AddParameterBasedOnDbColumn("@SD_Name", name, StmDataSchema.SD_Name);
				command.AddParameterBasedOnDbColumn("@SD_Type", dataType, StmDataSchema.SD_Type);
				command.AddParameterBasedOnDbColumn("@SD_GuidValue", guidValue != Guid.Empty ? guidValue : DBNull.Value, StmDataSchema.SD_GuidValue);
				command.AddParameterBasedOnDbColumn("@SD_IsLogged", isLogged, StmDataSchema.SD_IsLogged);
				command.AddParameterBasedOnDbColumn("@SD_SystemCreateTimeUtc", createOrEditTime, StmDataSchema.SD_SystemCreateTimeUtc);
				command.AddParameterBasedOnDbColumn("@SD_SystemCreateUser", userCode, StmDataSchema.SD_SystemCreateUser);
				command.AddParameterBasedOnDbColumn("@SD_SystemLastEditTimeUtc", createOrEditTime, StmDataSchema.SD_SystemLastEditTimeUtc);
				command.AddParameterBasedOnDbColumn("@SD_SystemLastEditUser", userCode, StmDataSchema.SD_SystemLastEditUser);
				command.AddParameterBasedOnDbColumn("@SD_PreserveTestValue", preserveTestValue, StmDataSchema.SD_PreserveTestValue);

				if (binaryValue != null)
				{
					command.AddParameterBasedOnDbColumn("@SD_BinaryValue", binaryValue, StmDataSchema.SD_BinaryValue);
				}
				if (ownerPK != Guid.Empty)
				{
					command.AddParameterBasedOnDbColumn("@SD_Owner", ownerPK, StmDataSchema.SD_Owner);
				}
				if (departmentPK != Guid.Empty)
				{
					command.AddParameterBasedOnDbColumn("@SD_DepartmentGuid", departmentPK, StmDataSchema.SD_DepartmentGuid);
				}

				command.ExecuteNonQuery();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		internal bool HasActualValueAtThisLevelForAnyDepartment(string name, Guid ownerPK)
		{
			ArrayList departmentPKs = new ArrayList();

			using (var reader = Connection.Command("select GE_PK from dbo.GlbDepartment").ExecuteReader())
			{
				while (reader.Read())
				{
					departmentPKs.Add((Guid)reader[0]);
				}
			}

			return (from Guid departmentPK in departmentPKs select GetBinaryValue(name, ownerPK, departmentPK)).Any(actualValue => actualValue != null);
		}

		#endregion

		#region Fallback

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		internal byte[] GetBinaryValueFallBack(string name, TimeSpan maximumCacheAge, Guid currentCompanyPK, Guid currentBranchPK, Guid currentDepartmentPK)
		{
			using (DbCommand command = Connection.Command(ProcedureNameGetValueFallback, null))
			{
				command.AddParameterBasedOnDbColumn("@Name", name, StmDataSchema.SD_Name);
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, currentCompanyPK);
				command.AddParameter("@BranchPK", SqlDbType.UniqueIdentifier, currentBranchPK);
				command.AddParameter("@DepartmentPK", SqlDbType.UniqueIdentifier, currentDepartmentPK);
				command.CommandType = CommandType.StoredProcedure;

				return command.ExecuteScalar() as byte[];
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		internal int GetFallBackLevel(string name, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			string sql;

			if (companyPK == Guid.Empty && branchPK == Guid.Empty && departmentPK == Guid.Empty)
			{
				return 6;
			}

			if (companyPK == Guid.Empty && branchPK == Guid.Empty && departmentPK != Guid.Empty)
			{
				sql = @"
					IF EXISTS (SELECT SD_Name FROM dbo.StmData WHERE SD_Name = @Name AND SD_Owner is null AND SD_DepartmentGuid = @DepartmentPK AND SD_BinaryValue is not null)
					  SELECT 5
					ELSE IF EXISTS (SELECT SD_Name FROM dbo.StmData WHERE SD_Name = @Name AND SD_Owner is null AND SD_DepartmentGuid is null AND SD_BinaryValue is not null)
					  SELECT 6";
			}
			else if (companyPK != Guid.Empty && branchPK == Guid.Empty && departmentPK == Guid.Empty)
			{
				sql = @"
					IF EXISTS (SELECT SD_Name FROM dbo.StmData WHERE SD_Name = @Name AND SD_Owner = @CompanyPK AND SD_DepartmentGuid is null AND SD_BinaryValue is not null)
					  SELECT 4
					ELSE IF EXISTS (SELECT SD_Name FROM dbo.StmData WHERE SD_Name = @Name AND SD_Owner is null AND SD_DepartmentGuid is not null AND SD_BinaryValue is not null)
					  SELECT 5
					ELSE IF EXISTS (SELECT SD_Name FROM dbo.StmData WHERE SD_Name = @Name AND SD_Owner is null AND SD_DepartmentGuid is null AND SD_BinaryValue is not null)
					  SELECT 6";
			}
			else if (companyPK != Guid.Empty && branchPK == Guid.Empty && departmentPK != Guid.Empty)
			{
				sql = @"
					IF EXISTS (SELECT SD_Name FROM dbo.StmData WHERE SD_Name = @Name AND SD_Owner = @CompanyPK AND SD_DepartmentGuid = @DepartmentPK AND SD_BinaryValue is not null)
						SELECT 3
					ELSE IF EXISTS (SELECT SD_Name FROM dbo.StmData WHERE SD_Name = @Name AND SD_Owner = @CompanyPK AND SD_DepartmentGuid is null AND SD_BinaryValue is not null)
						SELECT 4
					ELSE IF EXISTS (SELECT SD_Name FROM dbo.StmData WHERE SD_Name = @Name AND SD_Owner is null AND SD_DepartmentGuid = @DepartmentPK AND SD_BinaryValue is not null)
						SELECT 5
					ELSE IF EXISTS (SELECT SD_Name FROM dbo.StmData WHERE SD_Name = @Name AND SD_Owner is null AND SD_DepartmentGuid is null AND SD_BinaryValue is not null)
						SELECT 6";
			}
			else if (companyPK != Guid.Empty && branchPK != Guid.Empty && departmentPK == Guid.Empty)
			{
				sql = @"
					IF EXISTS (SELECT SD_Name FROM dbo.StmData WHERE SD_Name = @Name AND SD_Owner = @BranchPK AND SD_DepartmentGuid is null AND SD_BinaryValue is not null)
						SELECT 2
					ELSE IF EXISTS (SELECT SD_Name FROM dbo.StmData WHERE SD_Name = @Name AND SD_Owner = @CompanyPK AND SD_DepartmentGuid is not null AND SD_BinaryValue is not null)
						SELECT 3
					ELSE IF EXISTS (SELECT SD_Name FROM dbo.StmData WHERE SD_Name = @Name AND SD_Owner = @CompanyPK AND SD_DepartmentGuid is null AND SD_BinaryValue is not null)
						SELECT 4
					ELSE IF EXISTS (SELECT SD_Name FROM dbo.StmData WHERE SD_Name = @Name AND SD_Owner is null AND SD_DepartmentGuid is not null AND SD_BinaryValue is not null)
						SELECT 5
					ELSE IF EXISTS (SELECT SD_Name FROM dbo.StmData WHERE SD_Name = @Name AND SD_Owner is null AND SD_DepartmentGuid is null AND SD_BinaryValue is not null)
						SELECT 6";
			}
			else if (companyPK != Guid.Empty && branchPK != Guid.Empty && departmentPK != Guid.Empty)
			{
				sql = @"
					IF EXISTS (SELECT SD_Name FROM dbo.StmData WHERE SD_Name = @Name AND SD_Owner = @BranchPK AND SD_DepartmentGuid = @DepartmentPK AND SD_BinaryValue is not null)
						SELECT 1
					ELSE IF EXISTS (SELECT SD_Name FROM dbo.StmData WHERE SD_Name = @Name AND SD_Owner = @BranchPK AND SD_DepartmentGuid is null AND SD_BinaryValue is not null)
						SELECT 2
					ELSE IF EXISTS (SELECT SD_Name FROM dbo.StmData WHERE SD_Name = @Name AND SD_Owner = @CompanyPK AND SD_DepartmentGuid = @DepartmentPK AND SD_BinaryValue is not null)
						SELECT 3
					ELSE IF EXISTS (SELECT SD_Name FROM dbo.StmData WHERE SD_Name = @Name AND SD_Owner = @CompanyPK AND SD_DepartmentGuid is null AND SD_BinaryValue is not null)
						SELECT 4
					ELSE IF EXISTS (SELECT SD_Name FROM dbo.StmData WHERE SD_Name = @Name AND SD_Owner is null AND SD_DepartmentGuid = @DepartmentPK AND SD_BinaryValue is not null)
						SELECT 5
					ELSE IF EXISTS (SELECT SD_Name FROM dbo.StmData WHERE SD_Name = @Name AND SD_Owner is null AND SD_DepartmentGuid is null AND SD_BinaryValue is not null)
						SELECT 6";
			}
			else
			{
				throw new ArgumentException("Invalid combination of Company/Branch/Department, if BranchPK is provided, the CompanyPK must also be provided");
			}

			using (DbCommand command = Connection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@Name", name, StmDataSchema.SD_Name);
				if (companyPK != Guid.Empty)
				{
					command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);
				}

				if (branchPK != Guid.Empty)
				{
					command.AddParameter("@BranchPK", SqlDbType.UniqueIdentifier, branchPK);
				}

				if (departmentPK != Guid.Empty)
				{
					command.AddParameter("@DepartmentPK", SqlDbType.UniqueIdentifier, departmentPK);
				}

				object result = command.ExecuteScalar();
				return result == null ? -1 : (int)result;
			}
		}

		#endregion

		#region Is PK Referenced by Registry

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public bool IsPKReferencedByRegistry(Guid pk)
		{
			bool result = false;

			if (pk != Guid.Empty)
			{
				const string query = @"SELECT COUNT(*) FROM dbo.StmData WHERE SD_GuidValue = @GuidValue";
				using (DbCommand command = Connection.Command(query))
				{
					command.AddParameter("@GuidValue", SqlDbType.UniqueIdentifier, pk);
					int count = (int)command.ExecuteScalar();
					result = (count > 0);
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public string[] GetNamesOfRegistryItemsReferencingPK(Guid pk)
		{
			List<string> result = new List<string>();

			if (pk != Guid.Empty)
			{
				const string query = @"SELECT SD_Name FROM dbo.StmData WHERE SD_GuidValue = @GuidValue";
				using (DbCommand command = Connection.Command(query))
				{
					command.AddParameter("@GuidValue", SqlDbType.UniqueIdentifier, pk);
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							result.Add((string)reader[0]);
						}
					}
				}
			}

			return result.ToArray();
		}

		#endregion

		#region Get Item Names in Group

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		internal string[] GetItemNamesInGroup(string groupPrefixToSearchFor, Guid ownerPK)
		{
			ArrayList namesArray = new ArrayList();

			string ownerPKString = ownerPK == Guid.Empty ? "SD_Owner is null" : "SD_Owner = @OwnerPK"; // SQL Statement

			string query = "SELECT SD_Name FROM dbo.StmData WHERE SD_Name like @GroupPrefix AND " + ownerPKString + " ORDER BY SD_Name";
			using (DbCommand command = Connection.Command(query))
			{
				if (ownerPK != Guid.Empty)
				{
					command.AddParameterBasedOnDbColumn("@OwnerPK", ownerPK, StmDataSchema.SD_Owner);
				}

				command.AddParameterBasedOnDbColumn("@GroupPrefix", groupPrefixToSearchFor + "%", StmDataSchema.SD_Name);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						namesArray.Add(reader.GetString(0).Replace(groupPrefixToSearchFor, ""));
					}
				}
			}

			return (string[])namesArray.ToArray(typeof(string));
		}

		#endregion

		#region Has Value

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		internal bool HasValue(string name)
		{
			const string query = @"SELECT COUNT(*)
				FROM dbo.StmData
				WHERE SD_Name = @Name
				AND SD_BinaryValue is not null
				AND
				(
					SD_Owner IS NULL
					OR SD_Owner = @CompanyPK
					OR SD_Owner = @BranchPK
				)
				AND
				(
					SD_DepartmentGuid IS NULL
					OR SD_DepartmentGuid = @DepartmentPK
				)";

			using (DbCommand command = Db.Connection.Command(query))
			{
				IEnvironment env = EnvProxy.Instance;
				command.AddParameterBasedOnDbColumn("@Name", name, StmDataSchema.SD_Name);
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, env.CurrentCompany.PK);
				command.AddParameter("@BranchPK", SqlDbType.UniqueIdentifier, env.CurrentBranch.PK);
				command.AddParameter("@DepartmentPK", SqlDbType.UniqueIdentifier, env.CurrentDepartment.PK);

				return (int)command.ExecuteScalar() > 0;
			}
		}

		#endregion

		#region Audit Details

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		internal object GetAuditColumnValue(string name, string columnName, Guid owner, Guid department)
		{
			var ownerPredicateSql = owner == Guid.Empty ? "SD_Owner is null" : "SD_Owner = @Owner"; // SQL Statement
			var departmentPredicateSql = department == Guid.Empty ? "SD_DepartmentGuid is null" : "SD_DepartmentGuid = @Department"; // SQL Statement

			var query = string.Format(CultureInfo.InvariantCulture,
				@"SELECT {0} FROM dbo.StmData WHERE SD_Name = @Name AND {1} AND {2}",
				columnName, ownerPredicateSql, departmentPredicateSql);

			using (DbCommand command = Connection.Command(query))
			{
				command.AddParameterBasedOnDbColumn("@Name", name, StmDataSchema.SD_Name);

				if (owner != Guid.Empty)
				{
					command.AddParameter("@Owner", SqlDbType.UniqueIdentifier, owner);
				}
				if (department != Guid.Empty)
				{
					command.AddParameter("@Department", SqlDbType.UniqueIdentifier, department);
				}

				return command.ExecuteScalar();
			}
		}

		#endregion

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				disposable?.Dispose();
			}
		}

		public static IDisposable Disable()
		{
			if (disabled)
			{
				throw new InvalidOperationException("Already Disabled");
			}
			disabled = true;
			return new DisposableAction(() => disabled = false);
		}
		static bool disabled;

		readonly IDisposable disposable;
		public readonly DbConnection Connection;

		const int NameMaxLength = 300;

		protected const string ProcedureNameIsPkReferenced = "DataRegIsPkReferenced";
		protected const string ProcedureNameGetValueFallback = "DataRegGetValueFallback";
		protected const string ProcedureNameGetValueNOD = "DataRegGetValueNOD";
	}
}
