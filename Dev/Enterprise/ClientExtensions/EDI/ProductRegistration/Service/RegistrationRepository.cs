using System;
using System.Data;
using System.Threading.Tasks;
using CargoWise.Licensing;
using Enterprise.ProductRegistration.Common;

namespace CargoWise.ProductRegistration.Service
{
	public sealed class RegistrationRepository : IRegistrationRepository
	{
		public RegistrationRepository(ConnectionManager connectionManager)
		{
			this.connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
		}

		public RegistrationRepository(System.Data.Common.DbConnection connection, System.Data.Common.DbTransaction transaction)
		{
			connectionManager = new ConnectionManager(connection, transaction);
		}

		public ConnectionManager connectionManager;

		public IConnectionManager Connection { get { return connectionManager; } }

		public void Dispose()
		{
			if (connectionManager != null)
			{
				connectionManager.Dispose();
				connectionManager = null;
			}
		}

		public static class DatabaseSchema
		{
			public const string DatabaseNumber = "LD_DatabaseNumber";
			public const string DatabaseType = "LD_LicenceType";
			public const string HostedLocation = "LD_HostedLocation";
			public const string DatabaseSecurityMode = "LD_DBServerSecurityMode";
			public const string LicenceExpiry = "LD_LicenceExpiry";
			public const string ServerCode = "LD_ServerCode";
			public const string Status = "LD_Status";
			public const string IsInternalSystem = "IsInternalSystem";
			public const string BillingModel = "BillingModel";
			public const string HostDatabaseName = "LD_HostDBName";
			public const string HostServerName = "LD_HostServerName";
			public const string HostGroupId = "LD_HostGroupId";
			public const string HostDatabaseCreated = "LD_HostDBCreateDate";
			public const string HostConnectionServerName = "LD_HostConnectionServerName";
			public const string Product = "LD_Product";
		}

		public async Task<DbStatus> GetStatus(string productKey)
		{
			var enterpriseCode = productKey.Substring(0, 3);
			var serverCode = productKey.Substring(3, 3);
			DbStatus result = null;

			var cmd = connectionManager.CreateCmd();
			cmd.CommandText = "select * from dbo.ProductRegistrationGetStatus(@EnterpriseCode, @ServerCode)";
			cmd.AddParameterWithValue("@EnterpriseCode", enterpriseCode);
			cmd.AddParameterWithValue("@ServerCode", serverCode);

			using (var reader = await connectionManager.ExecuteReaderAsync(cmd))
			{
				if (reader.Read())
				{
					result = new DbStatus();
					result.DatabaseNumber = (int)reader[DatabaseSchema.DatabaseNumber];
					result.Status = reader[DatabaseSchema.Status].ToString();
					result.Product = reader[DatabaseSchema.Product].ToString();
				}
			}

			return result;
		}

		public async Task<DbRegisterResult> Register(RegisterRequest request, string passwordHash)
		{
			var cmd = connectionManager.CreateCmd();
			cmd.CommandText = "dbo.ProductRegistrationAdd";
			cmd.CommandType = CommandType.StoredProcedure;
			var enterpriseCode = request.ProductKey.Substring(0, 3);
			var serverCode = request.ProductKey.Substring(3, 3);
			cmd.AddParameterWithValue("@EnterpriseCode", enterpriseCode);
			cmd.AddParameterWithValue("@ServerCode", serverCode);
			cmd.AddParameterWithValue("@ServerName", request.UniqueKey.ServerName);
			cmd.AddParameterWithValue("@DatabaseName", request.UniqueKey.DatabaseName);
			cmd.AddParameterWithValue("@DatabaseCreateDate", request.UniqueKey.DatabaseCreated);
			cmd.AddParameterWithValue("@GroupId", ToDbValue(request.UniqueKey.GroupId));
			cmd.AddParameterWithValue("@Password", passwordHash);
			if (request.UniqueKey.ConnectionServerName != null)
			{
				cmd.AddParameterWithValue("@ConnectionServerName", request.UniqueKey.ConnectionServerName);
			}
			AddProductVersionParams(cmd, request.ProductVersion);

			var returnValue = new SqlParameter("@ReturnValue", SqlDbType.Int);
			returnValue.Direction = ParameterDirection.ReturnValue;
			cmd.Parameters.Add(returnValue);

			var result = new DbRegisterResult();

			using (var reader = await connectionManager.ExecuteReaderAsync(cmd))
			{
				if (reader.Read())
				{
					PopulateCommon(reader, result);
					result.DatabaseNumber = (int)reader[DatabaseSchema.DatabaseNumber];
					result.EnterpriseCode = enterpriseCode;
					result.ServerCode = serverCode;
				}
			}

			// Note, when you use a DataReader object, you must close it or read to the end of the data before you can view the output parameters.
			result.ReturnCode = returnValue.Value is int ? (int)returnValue.Value : (int)RegisterStatus.InternalError;

			return result;
		}

		public async Task<DbRegisterResult> Verify(int databaseNumber, string passwordHash, VerifyRequest request)
		{
			var dbKey = request.UniqueKey;
			var cmd = connectionManager.CreateCmd();
			cmd.CommandText = "dbo.ProductRegistrationVerify";
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.AddParameterWithValue("@DatabaseNumber", databaseNumber);
			cmd.AddParameterWithValue("@ServerName", dbKey.ServerName);
			cmd.AddParameterWithValue("@DatabaseName", dbKey.DatabaseName);
			cmd.AddParameterWithValue("@DatabaseCreateDate", dbKey.DatabaseCreated);
			cmd.AddParameterWithValue("@GroupId", ToDbValue(dbKey.GroupId));
			cmd.AddParameterWithValue("@Password", passwordHash);
			if (dbKey.ConnectionServerName != null)
			{
				cmd.AddParameterWithValue("@ConnectionServerName", dbKey.ConnectionServerName);
			}
			AddProductVersionParams(cmd, request.ProductVersion);

			var returnValue = cmd.CreateParameter();
			returnValue.Direction = ParameterDirection.ReturnValue;
			cmd.Parameters.Add(returnValue);

			var result = new DbRegisterResult();

			using (var reader = await connectionManager.ExecuteReaderAsync(cmd))
			{
				if (reader.Read())
				{
					PopulateCommon(reader, result);
					result.DatabaseNumber = databaseNumber;
					result.EnterpriseCode = reader["LE_EnterpriseCode"].ToString();
					result.ServerCode = reader[DatabaseSchema.ServerCode].ToString();
					result.CustomExpiredMessage = reader["CustomExpiredMessage"].ToString();
					result.CustomExpiryWeekMessage = reader["CustomExpiryWeekMessage"].ToString();
					result.CustomExpiryMonthMessage = reader["CustomExpiryMonthMessage"].ToString();
				}
			}

			// Note, when you use a DataReader object, you must close it or read to the end of the data before you can view the output parameters.
			result.ReturnCode = returnValue.Value is int ? (int)returnValue.Value : (int)RegisterStatus.InternalError;

			return result;
		}

		static void AddProductVersionParams(System.Data.Common.DbCommand cmd, string productVersion)
		{
			if (productVersion != null)
			{
				var parts = productVersion.Split('.');
				int major = SafeParse(parts, 0);
				int minor = SafeParse(parts, 1);
				int release = SafeParse(parts, 2);
				int patch = SafeParse(parts, 3);
				if (major != -1 && minor != -1 && release != -1 && patch != -1)
				{
					cmd.AddParameterWithValue("@VersionMajor", major);
					cmd.AddParameterWithValue("@VersionMinor", minor);
					cmd.AddParameterWithValue("@VersionRelease", release);
					cmd.AddParameterWithValue("@VersionPatch", patch);
				}
			}
		}

		static int SafeParse(string[] parts, int partIndex)
		{
			int result = -1;
			if (parts != null && partIndex < parts.Length)
			{
				if (!int.TryParse(parts[partIndex], out result))
				{
					result = -1;
				}
			}

			return result;
		}

		static object ToDbValue(Guid? id)
		{
			return id ?? (object)DBNull.Value;
		}

		static DateTime? FromDbDateTime(object dbValue)
		{
			return Convert.IsDBNull(dbValue) ? null : (DateTime)dbValue;
		}

		static void PopulateCommon(System.Data.Common.DbDataReader reader, DbRegisterResult db)
		{
			var utcNow = reader["UtcNow"];
			var databaseType = reader[DatabaseSchema.DatabaseType];
			var hostedLocation = reader[DatabaseSchema.HostedLocation];
			var databaseSecurityMode = reader[DatabaseSchema.DatabaseSecurityMode];
			var manualLicenceExpiry = reader[DatabaseSchema.LicenceExpiry];
			var isInternalSystem = reader[DatabaseSchema.IsInternalSystem];
			var billingModel = reader[DatabaseSchema.BillingModel];

			db.UtcNow = (DateTime)utcNow;
			db.DatabaseType = databaseType.ToString();
			db.HostedLocation = hostedLocation.ToString();
			db.DatabaseSecurityMode = databaseSecurityMode.ToString();
			db.ManualLicenceExpiry = FromDbDateTime(manualLicenceExpiry);
			db.IsInternalSystem = (Boolean)isInternalSystem;
			db.BillingModel = billingModel.ToString();

			var info = new BillingTimeZoneInfo(db.UtcNow);
			db.CurrentBillingTimeZoneUtcOffset = info.CurrentBillingTimeZoneUtcOffset;
			db.NextBillingTimeZoneUtcOffset = info.NextBillingTimeZoneUtcOffset;
			db.NextUtcOffsetEffectiveTimeUtc = info.NextUtcOffsetEffectiveTimeUtc;
		}

		public async Task<int> Unregister(int databaseNumber, string passwordHash)
		{
			var cmd = connectionManager.CreateCmd();
			cmd.CommandText = "dbo.ProductRegistrationRemove";
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.AddParameterWithValue("@DatabaseNumber", databaseNumber);
			cmd.AddParameterWithValue("@Password", passwordHash);
			var returnValue = new SqlParameter();
			returnValue.Direction = ParameterDirection.ReturnValue;
			cmd.Parameters.Add(returnValue);
			var rows = await connectionManager.ExecuteNonQueryAsync(cmd);
			return returnValue.Value is int ? (int)returnValue.Value : (int)RegisterStatus.InternalError;
		}

		public async Task<DatabaseUniqueKey> GetDatabaseUniqueKey(int databaseNumber)
		{
			DatabaseUniqueKey result = null;

			var cmd = connectionManager.CreateCmd();
			cmd.CommandText = "select * from dbo.ProductRegistrationGetDatabaseUniqueKey(@DatabaseNumber)";
			cmd.AddParameterWithValue("@DatabaseNumber", databaseNumber);

			using (var reader = await connectionManager.ExecuteReaderAsync(cmd))
			{
				if (reader.Read())
				{
					result = new DatabaseUniqueKey();
					result.DatabaseName = (string)reader[DatabaseSchema.HostDatabaseName];
					result.ServerName = (string)reader[DatabaseSchema.HostServerName];
					result.GroupId = (Guid)reader[DatabaseSchema.HostGroupId];
					result.DatabaseCreated = (DateTime)reader[DatabaseSchema.HostDatabaseCreated];
					result.ConnectionServerName = (string)reader[DatabaseSchema.HostConnectionServerName];
				}
			}

			return result;
		}
	}
}
