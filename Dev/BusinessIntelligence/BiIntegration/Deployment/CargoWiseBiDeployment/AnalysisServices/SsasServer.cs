using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Bi.Registration.Common;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Microsoft.AnalysisServices.AdomdClient;
using Microsoft.AnalysisServices.Tabular;
using SsasCore = Microsoft.AnalysisServices;
using SsasDatabase = Microsoft.AnalysisServices.Tabular.Database;

namespace CargoWise.Bi.Deployment.AnalysisServices
{
	public class SsasServer : IDisposable
	{
		#region SuppressResourceStringsCheckRegion

		#region Constructor

		ILogger logger;

		public ILogger Logger
		{
			get { return logger; }
			set { logger = value; }
		}
#if DEBUG
		public static SsasServer New(string ssasServerName, bool impersonatingUser = true)
		{
			return new SsasServer(ssasServerName, impersonatingUser);
		}
#endif

		public static SsasServer New(string ssasServerName)
		{
			return new SsasServer(ssasServerName);
		}

		protected SsasServer(string ssasServerName, bool impersonatingUser = true, ILogger logger = null)
		{
			if (string.IsNullOrWhiteSpace(ssasServerName))
			{
				throw new SsasException("Server name must have a value to open SSAS server connection");
			}
			else
			{
				try
				{
					Logger = logger;
					this.ssasServerName = ssasServerName;
					server = new Server();
					server.Connect(GetConnectionString());
					biReportUser = impersonatingUser ? new BiReportUser() : null;
				}
				catch (SsasCore.ConnectionException ex)
				{
					var message = string.Format(CultureInfo.InvariantCulture, "Could not connect to analysis server [{0}].\r\n{1}\r\n{2}", ssasServerName, ex.Message, ex.InnerException?.Message).Trim();
					if (IsWTGInternalSystem())
					{
						Logger?.Log(LogType.Warning, message);
					}
					else
					{
						throw new SsasException(message, ex);
					}
				}
			}
		}
		protected readonly Server server;
		readonly string ssasServerName;
		readonly BiReportUser biReportUser;

		IDisposable ImpersonateBiReportUser()
		{
			if (biReportUser != null)
			{
				return biReportUser.Impersonate();
			}
			else
			{
				return null;
			}
		}

		public string ID
		{
			get
			{
				return server?.ID;
			}
		}

		public string Name
		{
			get
			{
				return server?.Name;
			}
		}

		string GetConnectionString()
		{
			return string.Format(CultureInfo.InvariantCulture, "Data Source={0};Application Name=SSAS Cube Deployer;", ssasServerName);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void Dispose(bool disposing)
		{
			if (disposing)
			{
				CleanupBackUpFiles();
				server.Dispose();
			}
		}

		#endregion

		#region Query Execution

		public string ServerVersion
		{
			get
			{
				return server.Version;
			}
		}

		public string ServerMode
		{
			get
			{
				return server.ServerMode.ToString();
			}
		}

		public void ExecuteBatchCommand(string executeCommand)
		{
			try
			{
				using (biReportUser?.Impersonate())
				{
					Refresh();
					ExecuteBatchCommandCore(executeCommand);
				}
				Refresh();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ThrowExceptionIfExternal(ex);
			}
		}

		bool IsWTGInternalSystem()
		{
			bool isWtgInternal;

			try
			{
				var registration = ObjectFactory.Get<IProductRegistration>();
				isWtgInternal = registration.IsWiseTechGlobalInternalSystem();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				isWtgInternal = false;
			}

			return isWtgInternal;
		}

		public void ExecuteBatchCommandCore(string executeCommand)
		{
			if (!string.IsNullOrEmpty(executeCommand))
			{
				var executeErrors = server.Execute(executeCommand);
				if (executeErrors.Count > 0)
				{
					var errorList = new StringBuilder();
					foreach (SsasCore.XmlaResult error in executeErrors)
					{
						foreach (SsasCore.XmlaMessage message in error.Messages)
						{
							errorList.AppendLine(GetErrorMessage(message));
						}
					}

					var errors = errorList.ToString();
					if (!string.IsNullOrEmpty(errors))
					{
						var errorMsgRegexs = new Regex[] { new Regex("Either the '(?<userLogin>.+?)' user does not have permission to (create a new object in|alter the) '(?<serverName>.+?)'( object)*, or the object does not exist."), new Regex("Cannot execute the Refresh command") };

						foreach (var errorMsgRegex in errorMsgRegexs)
						{
							Match match = errorMsgRegex.Match(errors);
							if (match.Success)
							{
								var userLogin = !string.IsNullOrEmpty(match.Groups["userLogin"].Value) ? match.Groups["userLogin"].Value : WindowsIdentity.GetCurrent().Name;
								var serverName = !string.IsNullOrEmpty(match.Groups["serverName"].Value) ? match.Groups["serverName"].Value : ssasServerName;
								errors = string.Format(CultureInfo.InvariantCulture, "'{0}' does not have sufficient permission on SSAS server '{1}'. Connect to Analysis Services using SSMS and add the user as server administrator.", userLogin, serverName);
							}
						}

						throw new SsasException(string.Format(CultureInfo.InvariantCulture, "Execution of BIM query failed.\r\n{0}", errors.Trim()));
					}
				}
			}
		}

		string GetErrorMessage(SsasCore.XmlaMessage message)
		{
			StringBuilder errorMsg = new StringBuilder();

			if (!string.IsNullOrEmpty(message.Description))
			{
				var warningMessage = message as SsasCore.XmlaWarning;
				if (warningMessage != null)
				{
					errorMsg.AppendLine("Warning Code: " + warningMessage.WarningCode.ToString(CultureInfo.InvariantCulture));
				}

				var errorMessage = message as SsasCore.XmlaError;
				if (errorMessage != null)
				{
					errorMsg.AppendLine("Error Code: " + errorMessage.ErrorCode.ToString(CultureInfo.InvariantCulture));
				}

				errorMsg.AppendLine("Description: " + message.Description);
				errorMsg.AppendLine("Source: " + message.Source);
				if (!string.IsNullOrEmpty(message.HelpFile))
				{
					errorMsg.AppendLine("Help File: " + message.HelpFile);
				}

				errorMsg.Append(GetLocationInformation(message?.Location?.SourceObject));
			}

			return errorMsg.ToString().Trim();
		}

		string GetLocationInformation(SsasCore.XmlaLocationReference sourceObject)
		{
			if (sourceObject != null)
			{
				var cube = sourceObject.Cube;
				var tableName = sourceObject.TableName;
				var columnName = sourceObject.ColumnName;
				var attribute = sourceObject.Attribute;
				var dimension = sourceObject.Dimension;
				var hierarchy = sourceObject.Hierarchy;
				var measureGroup = sourceObject.MeasureGroup;
				var measureName = sourceObject.MeasureName;
				var memberName = sourceObject.MemberName;
				var partitionName = sourceObject.PartitionName;
				var role = sourceObject.Role;
				var roleName = sourceObject.RoleName;

				return string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}",
					!string.IsNullOrEmpty(cube) ? "Cube: " + cube + "\r\n" : "",
					!string.IsNullOrEmpty(tableName) ? "Table Name: " + tableName + "\r\n" : "",
					!string.IsNullOrEmpty(columnName) ? "Column Name: " + columnName + "\r\n" : "",
					!string.IsNullOrEmpty(attribute) ? "Attribute: " + attribute + "\r\n" : "",
					!string.IsNullOrEmpty(dimension) ? "Dimension: " + dimension + "\r\n" : "",
					!string.IsNullOrEmpty(hierarchy) ? "Hierarchy: " + hierarchy + "\r\n" : "",
					!string.IsNullOrEmpty(measureGroup) ? "Measure Group: " + measureGroup + "\r\n" : "",
					!string.IsNullOrEmpty(measureName) ? "Measure Name: " + measureName + "\r\n" : "",
					!string.IsNullOrEmpty(memberName) ? "Member Name: " + memberName + "\r\n" : "",
					!string.IsNullOrEmpty(partitionName) ? "Partition Name: " + partitionName + "\r\n" : "",
					!string.IsNullOrEmpty(role) ? "Role: " + role + "\r\n" : "",
					!string.IsNullOrEmpty(roleName) ? "Role Name: " + roleName + "\r\n" : "");
			}
			else
			{
				return null;
			}
		}

		[SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		public DataTable GetDataTableFromQuery(string ssasDatabaseName, string commandText, List<AdomdParameter> parameters = null)
		{
			using (biReportUser?.Impersonate())
			{
				return GetDataTableFromQueryCore(ssasDatabaseName, commandText, parameters);
			}
		}

		DataTable GetDataTableFromQueryCore(string ssasDatabaseName, string commandText, List<AdomdParameter> parameters = null)
		{
			using (var analysisServiceConnection = new AdomdConnection())
			{
				analysisServiceConnection.ConnectionString = string.Format(CultureInfo.InvariantCulture, "Provider=MSOLAP;Data Source={0};Catalog={1}", ssasServerName, ssasDatabaseName);
				analysisServiceConnection.Open();
				using (var command = new AdomdCommand(commandText, analysisServiceConnection))
				{
					if (parameters != null)
					{
						foreach (var parameter in parameters)
						{
							command.Parameters.Add(parameter);
						}
					}
					return GetDataTableFromCommand(command);
				}
			}
		}

		public static DataTable GetDataTableFromCommand(IDbCommand command)
		{
			DataTable result = new DataTable();
			result.Locale = CultureInfo.InvariantCulture;

			using (var reader = command.ExecuteReader())
			{
				var columns = new List<string>();

				for (int i = 0; i < reader.FieldCount; i++)
				{
					var columnName = reader.GetName(i);
					var dataType = reader.GetFieldType(i);

					columns.Add(columnName);
					result.Columns.Add(GetColumnNameString(columnName), dataType);
				}

				while (reader.Read())
				{
					DataRow row = result.NewRow();
					foreach (var column in columns)
					{
						string columnName = GetColumnNameString(column);
						if (reader[column] == null)
						{
							row[columnName] = DBNull.Value;
						}
						else
						{
							row[columnName] = reader[column];
						}
					}
					result.Rows.Add(row);
				}
			}
			return result;
		}

		static string GetColumnNameString(string column)
		{
			if (column[0] == '[' && column[column.Length - 1] == ']')
			{
				return column.Substring(1, column.Length - 2);
			}
			return column;
		}

		void Refresh()
		{
			try
			{
				server.Disconnect();
				server.Connect(GetConnectionString());
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ThrowExceptionIfExternal(ex);
			}
		}

		#endregion

		#region Database

		public List<string> GetSsasDatabaseList()
		{
			var result = new List<string>();
			using (ImpersonateBiReportUser())
			{
				foreach (SsasDatabase database in server.Databases)
				{
					result.Add(database.Name);
				}
			}
			return result.OrderBy(d => d).ToList();
		}

		protected SsasDatabase GetSsasDatabase(string databaseName)
		{
			using (ImpersonateBiReportUser())
			{
				Refresh();
				return server.Databases.FindByName(databaseName);
			}
		}

		public List<string> GetSsasTableList(string databaseName)
		{
			Refresh();
			List<string> result = null;
			if (DatabaseExists(databaseName))
			{
				var database = GetSsasDatabase(databaseName);
				result = database.Model.Tables.Select(t => t.Name).OrderBy(t => t).ToList();
			}
			return result;
		}

		public bool DatabaseExists(string databaseName)
		{
			return GetSsasDatabase(databaseName) != null;
		}

		public bool TableExists(string databaseName, string tableName)
		{
			var result = false;

			if (DatabaseExists(databaseName))
			{
				var db = GetSsasDatabase(databaseName);
				var table = db.Model.Tables.Find(tableName);
				result = table != null;
			}

			return result;
		}

		public bool DataSourceExists(string databaseName, string dataSourceName)
		{
			DataSource edwDataSource = null;

			var database = GetSsasDatabase(databaseName);
			if (database != null)
			{
				using (ImpersonateBiReportUser())
				{
					edwDataSource = database.Model.DataSources.FirstOrDefault(ds => ds.Name == dataSourceName);
				}
			}

			return (edwDataSource != null);
		}

		public string GetDatabaseVersion(string databaseName)
		{
			string version = null;

			var database = GetSsasDatabase(databaseName);
			if (database != null)
			{
				version = database.Description;
			}

			return version;
		}

		public void SetDatabaseVersion(string ssasDatabaseName, string modelVersion)
		{
			var alterCommand = PrepareSetVersionDescriptionCommand(ssasDatabaseName, modelVersion);
			ExecuteBatchCommand(alterCommand);
		}

		string PrepareSetVersionDescriptionCommand(string ssasDatabaseName, string modelVersion)
		{
			return string.Format(CultureInfo.InvariantCulture, @"
{{
	""alter"": {{
		""object"": {{
			""database"": ""{0}""
		}},
		""database"": {{
			""name"": ""{0}"",
			""description"": ""{1}""
		}}
	}}
}}",
				ssasDatabaseName,
				modelVersion
			);
		}

		public const string AdminRoleName = "Administrators";

		#region SetUserAsAdminQuery

		public const string SetUserAsAdminQuery = @"<Batch xmlns=""http://schemas.microsoft.com/analysisservices/2003/engine"" Transaction=""true"">
	<Alter AllowCreate=""true"" ObjectExpansion=""ObjectProperties"" xmlns=""http://schemas.microsoft.com/analysisservices/2003/engine"">
		<Object />
		<ObjectDefinition>
			<Server xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:ddl2=""http://schemas.microsoft.com/analysisservices/2003/engine/2"" xmlns:ddl2_2=""http://schemas.microsoft.com/analysisservices/2003/engine/2/2"" xmlns:ddl100_100=""http://schemas.microsoft.com/analysisservices/2008/engine/100/100"" xmlns:ddl200=""http://schemas.microsoft.com/analysisservices/2010/engine/200"" xmlns:ddl200_200=""http://schemas.microsoft.com/analysisservices/2010/engine/200/200"" xmlns:ddl300=""http://schemas.microsoft.com/analysisservices/2011/engine/300"" xmlns:ddl300_300=""http://schemas.microsoft.com/analysisservices/2011/engine/300/300"" xmlns:ddl400=""http://schemas.microsoft.com/analysisservices/2012/engine/400"" xmlns:ddl400_400=""http://schemas.microsoft.com/analysisservices/2012/engine/400/400"" xmlns:ddl500=""http://schemas.microsoft.com/analysisservices/2013/engine/500"" xmlns:ddl500_500=""http://schemas.microsoft.com/analysisservices/2013/engine/500/500"">
				<ID>{0}</ID>
				<Name>{1}</Name>
			</Server>
		</ObjectDefinition>
	</Alter>
	<Alter AllowCreate=""true"" ObjectExpansion=""ObjectProperties"" xmlns=""http://schemas.microsoft.com/analysisservices/2003/engine"">
		<Object>
			<RoleID>Administrators</RoleID>
		</Object>
		<ObjectDefinition>
			<Role xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:ddl2=""http://schemas.microsoft.com/analysisservices/2003/engine/2"" xmlns:ddl2_2=""http://schemas.microsoft.com/analysisservices/2003/engine/2/2"" xmlns:ddl100_100=""http://schemas.microsoft.com/analysisservices/2008/engine/100/100"" xmlns:ddl200=""http://schemas.microsoft.com/analysisservices/2010/engine/200"" xmlns:ddl200_200=""http://schemas.microsoft.com/analysisservices/2010/engine/200/200"" xmlns:ddl300=""http://schemas.microsoft.com/analysisservices/2011/engine/300"" xmlns:ddl300_300=""http://schemas.microsoft.com/analysisservices/2011/engine/300/300"" xmlns:ddl400=""http://schemas.microsoft.com/analysisservices/2012/engine/400"" xmlns:ddl400_400=""http://schemas.microsoft.com/analysisservices/2012/engine/400/400"" xmlns:ddl500=""http://schemas.microsoft.com/analysisservices/2013/engine/500"" xmlns:ddl500_500=""http://schemas.microsoft.com/analysisservices/2013/engine/500/500"">
				<ID>Administrators</ID>
				<Name>Administrators</Name>
				<Members>
					<Member><Name>{2}</Name></Member>
				</Members>
			</Role>
		</ObjectDefinition>
	</Alter>
</Batch>";

		#endregion

		public void SetUserAsAdmin(string username)
		{
			var adminMembers = GetAdminMembers();

			if (!adminMembers.Any(m => m.Equals(username, StringComparison.OrdinalIgnoreCase)))
			{
				adminMembers.Add(username);
			}

			bool stop = false;

			while (!stop)
			{
				try
				{
					var cmd = string.Format(CultureInfo.InvariantCulture,
						SetUserAsAdminQuery,
						server.ID,
						server.Name,
						string.Join("</Name></Member>\r\n\t\t\t\t\t<Member><Name>", adminMembers));
					ExecuteBatchCommand(cmd);
					stop = true;
				}
				catch (SsasException ex)
				{
					var errorRegex = new Regex($"The '{AdminRoleName}' role includes domain account that does not exist. The following domain user account is no longer valid for this role: '(?<userAccount>\\b.+?\\b)'", RegexOptions.IgnoreCase);
					if (errorRegex.IsMatch(ex.Message))
					{
						Match match = errorRegex.Match(ex.Message);
						var userAccount = match.Groups["userAccount"].Value;
						if (adminMembers.Contains(userAccount))
						{
							adminMembers.Remove(userAccount);
						}
						else
						{
							throw;
						}
					}
					else
					{
						throw;
					}
				}
			}
			Refresh();
		}

		public List<string> GetAdminMembers()
		{
			Refresh();
			var members = server.Roles[AdminRoleName].Members;
			var adminMembers = new List<string>();

			foreach (SsasCore.RoleMember member in members)
			{
				if (!adminMembers.Any(m => m.Equals(member.Name, StringComparison.OrdinalIgnoreCase)))
				{
					adminMembers.Add(member.Name);
				}
			}
			return adminMembers;
		}

		public void BackupModel(string databaseName)
		{
			var database = GetSsasDatabase(databaseName);

			if (database != null)
			{
				var ssasBackupFileName = string.Format(CultureInfo.InvariantCulture, "{0}_bkp.abf", databaseName);
				database.Backup(ssasBackupFileName, true);
				backupFileList.Add(ssasBackupFileName);
			}
			modifiedDbList.Add(databaseName);
		}

		public void BackupModel(string databaseName, string backupFilePath)
		{
			var database = GetSsasDatabase(databaseName);

			if (database != null)
			{
				database.Backup(backupFilePath, true);
			}
		}

		public void DropAndRestorePreviousModels()
		{
			DropModels();
			RestorePreviousModels();
		}

		public void DropModel(string databaseName)
		{
			var ssasDb = GetSsasDatabase(databaseName);
			if (ssasDb != null)
			{
				ssasDb.Drop();
			}
		}

		public void RestoreModel(string filePath, string cubeName, bool allowOverwrite)
		{
			try
			{
				server.Restore(filePath, cubeName, allowOverwrite);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ThrowExceptionIfExternal(ex);
			}
		}

		void ThrowExceptionIfExternal(Exception ex)
		{
			if (IsWTGInternalSystem())
			{
				Logger?.Log(LogType.Warning, ex.Message);
			}
			else
			{
				throw ex;
			}
		}

		void DropModels()
		{
			foreach (var databaseName in modifiedDbList.ToArray())
			{
				DropModel(databaseName);
				modifiedDbList.Remove(databaseName);
			}
		}

		void RestorePreviousModels()
		{
			foreach (var backupFile in backupFileList)
			{
				if (!string.IsNullOrEmpty(backupFile))
				{
					server.Restore(backupFile);
				}
			}
		}

		void CleanupBackUpFiles()
		{
			foreach (var backupFile in backupFileList)
			{
				if (!string.IsNullOrEmpty(backupFile) && File.Exists(backupFile))
				{
					File.Delete(backupFile);
				}
			}
		}

		readonly List<string> modifiedDbList = new List<string>();
		readonly List<string> backupFileList = new List<string>();

		public bool CheckUserHasReaderRole(string ssasDatabaseName, string userDomainAndName)
		{
			var result = false;

			var ssasDb = GetSsasDatabase(ssasDatabaseName);
			var role = ssasDb.Model.Roles.Find(ReaderRoleName);

			if (role != null && role.Members.Contains(userDomainAndName))
			{
				result = true;
			}

			return result;
		}

		public void TryAddOrUpdateDatabaseReaderRole(string ssasDatabaseName, string userDomainAndName)
		{
			var roleMember = new WindowsModelRoleMember();
			roleMember.MemberName = userDomainAndName;
			var ssasDb = GetSsasDatabase(ssasDatabaseName);
			var role = ssasDb.Model.Roles.Find(ReaderRoleName);

			if (role == null)
			{
				role = new ModelRole();
				role.Name = ReaderRoleName;
				ssasDb.Model.Roles.Add(role);
			}

			if (!role.Members.Contains(userDomainAndName))
			{
				role.Members.Add(roleMember);
			}

			role.ModelPermission = ModelPermission.Read;

			var companyBranchHelperFilterTable = ssasDb.Model.Tables.Find(CompanyBranchHelperRowFilteringTableName);
			if (companyBranchHelperFilterTable != null)
			{
				if (!role.TablePermissions.Contains(CompanyBranchHelperRowFilteringTableName))
				{
					var tablePermission = new TablePermission();
					tablePermission.Table = ssasDb.Model.Tables.Find(CompanyBranchHelperRowFilteringTableName);
					tablePermission.FilterExpression = CompanyBranchHelperRowFilteringExpression;
					role.TablePermissions.Add(tablePermission);
				}
			}

			var companyBranchFilterTable = ssasDb.Model.Tables.Find(CompanyBranchRowFilteringTableName);
			if (companyBranchFilterTable != null)
			{
				if (!role.TablePermissions.Contains(CompanyBranchRowFilteringTableName))
				{
					var tablePermission = new TablePermission();
					tablePermission.Table = ssasDb.Model.Tables.Find(CompanyBranchRowFilteringTableName);
					tablePermission.FilterExpression = CompanyBranchRowFilteringExpression;
					role.TablePermissions.Add(tablePermission);
				}
			}

			ssasDb.Update(SsasCore.UpdateOptions.ExpandFull);
		}

		public void DropDatabaseRole(string ssasDatabaseName, string roleName)
		{
			var ssasDb = GetSsasDatabase(ssasDatabaseName);
			var role = ssasDb.Model.Roles.Find(roleName);

			if (role != null)
			{
				ssasDb.Model.Roles.Remove(role);
			}

			ssasDb.Update(SsasCore.UpdateOptions.ExpandFull);
		}

		public const string ReaderRoleName = "Reader";
		public const string CompanyBranchRowFilteringTableName = "Company Branch";
		public const string CompanyBranchRowFilteringExpression = @"IF(
CUSTOMDATA() = """",
TRUE(),
[Company Code] = CUSTOMDATA()
)";

		public const string CompanyBranchHelperRowFilteringTableName = "Company Branch Helper";
		public const string CompanyBranchHelperRowFilteringExpression = @"IF(
CUSTOMDATA() = """",
TRUE(),
[Company Code] = CUSTOMDATA()
)";
		#endregion

		#region Partition

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public PartitionCollection GetPartitionCollectionFromCube(string analysisDbName)
		{
			var result = new PartitionCollection();

			var database = GetSsasDatabase(analysisDbName);
			if (database != null)
			{
				foreach (var table in database.Model.Tables)
				{
					foreach (var partition in table.Partitions)
					{
						var partitionSource = partition.Source as QueryPartitionSource;
						if (partitionSource != null)
						{
							var partitionQuery = partitionSource.Query;
							DateTime? fromValue = null;
							DateTime? toValue = null;

							var fromValueRegex = new Regex(@"WHERE\s+(.*?\s*\>=\s*)'(?<FromValue>.*?)'", RegexOptions.IgnoreCase);
							Match match = fromValueRegex.Match(partitionQuery);
							if (match.Success)
							{
								try
								{
									fromValue = SqlFormatInfo.FromSqlDate(match.Groups["FromValue"].Value);
								}
								catch (Exception ex) when (!ex.IsCriticalException()) { }
							}

							var toValueRegex = new Regex(@"WHERE\s+(.*?\s*\<[^\=]\s*)'(?<ToValue>.*?)'", RegexOptions.IgnoreCase);
							match = toValueRegex.Match(partitionQuery);
							if (match.Success)
							{
								try
								{
									toValue = SqlFormatInfo.FromSqlDate(match.Groups["ToValue"].Value);
								}
								catch (Exception ex) when (!ex.IsCriticalException()) { }
							}

							result.Add(analysisDbName, table.Name, partition.Name, fromValue, toValue);
						}
					}
				}
			}

			return result;
		}

		public void CreatePartition(string databaseName, string tableName, string partitionName, string partitionQuery)
		{
			var query = string.Format(CultureInfo.InvariantCulture,
@"{{
  ""createOrReplace"": {{
		""object"": {{
			""database"": ""{0}"",
			""table"": ""{1}"",
			""partition"": ""{2}""
		}},
		""partition"": {{
			""name"": ""{2}"",
		""source"": {{
				""query"": ""{3}"",
			""dataSource"": ""EDW""
			}}
		}}
	}}
}}", databaseName, tableName, partitionName, partitionQuery);

			ExecuteBatchCommand(query);
		}

		public void DeletePartition(string databaseName, string tableName, string partitionName)
		{
			var query = string.Format(CultureInfo.InvariantCulture,
@"{{
  ""delete"": {{
		""object"": {{
			""database"": ""{0}"",
			""table"": ""{1}"",
			""partition"": ""{2}""
		}}
	}}
}}", databaseName, tableName, partitionName);

			ExecuteBatchCommand(query);
		}

		public void MergePartitions(string databaseName, string tableName, string partitionName, IEnumerable<string> sourcePartitions, string dataSourceName, string partitionQuery)
		{
			var partitions = sourcePartitions.Distinct();
			if (partitions.Any())
			{
				var targetPartition = partitions.Last();
				if (partitions.Count() > 1)
				{
					var query = string.Format(CultureInfo.InvariantCulture,
		@"{{
  ""mergePartitions"": {{
		""target"": {{
			""database"": ""{0}"",
			""table"": ""{1}"",
			""partition"": ""{2}""
		}},
		""sources"": [
			""{3}""
		]
	}}
}}",
						databaseName,
						tableName,
						targetPartition,
						string.Join("\",\r\n\t\t\t\"", partitions.Except(new[] { targetPartition })));

					ExecuteBatchCommand(query);
				}
				RecreatePartition(databaseName, tableName, targetPartition, partitionName, dataSourceName, partitionQuery);
			}
		}

		public void RecreatePartition(string databaseName, string tableName, string oldPartitionName, string newPartitionName, string dataSourceName, string partitionQuery)
		{
			var query = string.Format(CultureInfo.InvariantCulture,
@"{{
  ""createOrReplace"": {{
		""object"": {{
			""database"": ""{0}"",
			""table"": ""{1}"",
			""partition"": ""{2}""
		}},
		""partition"": {{
			""name"": ""{3}"",
			""dataView"": ""full"",
			""source"": {{
				""query"": ""{4}"",
				""dataSource"": ""{5}""
			}}
		}}
	}}
}}",
				databaseName,
				tableName,
				oldPartitionName,
				newPartitionName,
				partitionQuery,
				dataSourceName);

			ExecuteBatchCommand(query);
		}

		#endregion

		#endregion
	}
}
