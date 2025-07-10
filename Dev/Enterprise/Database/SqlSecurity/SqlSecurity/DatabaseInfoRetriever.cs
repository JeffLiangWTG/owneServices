using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Security.ActiveDirectory;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using JC = CargoWise.EntityFramework.JoinCondition;

namespace Enterprise.SqlSecurity
{
	class DatabaseInfoRetriever : IDatabaseInfoRetriever
	{
		public DatabaseInfoRetriever(AdminConnection adminConnection, string mainDatabaseName, SqlSecurityLogger logger, CancellationToken cancellationToken)
			: this(adminConnection, mainDatabaseName, logger, cancellationToken, ignoreStaffMembers: false)
		{
		}

		public DatabaseInfoRetriever(AdminConnection adminConnection, string mainDatabaseName, SqlSecurityLogger logger, CancellationToken cancellationToken, bool ignoreStaffMembers)
		{
			this.adminConnection = adminConnection;
			this.mainDatabaseName = mainDatabaseName;
			this.logger = logger;
			this.cancellationToken = cancellationToken;
			this.ignoreStaffMembers = ignoreStaffMembers;
		}

		readonly bool ignoreStaffMembers;
		string singleRefDbName;
		bool isHostedInWiseCloud;
		bool isDatabaseSecurityModeOpen;
		readonly SqlSecurityLogger logger;
		readonly CancellationToken cancellationToken;

		IList<string> filterPatterns;
		IList<string> databases;
		IDictionary<string, bool> isDedicatedServerInstanceMap;

		readonly AdminConnection adminConnection;
		readonly string mainDatabaseName;
		AdminConnection dataWarehouseServerConnection;
		AdminConnection auditServerConnection;
		bool initialised;

		void RetrieveAllDatabases(AdminConnection auditServerConnection, AdminConnection edwServerConnection)
		{
			var auditDbName = $"{mainDatabaseName}{Db.AuditDatabaseSuffix}";
			var edwDbName = $"{mainDatabaseName}{Db.EdwDatabaseSuffix}";

			databases = adminConnection.GetDatabases(CargoWise.Data.DatabaseType.All).ToList();

			if (!databases.Contains(auditDbName, StringComparer.OrdinalIgnoreCase)
				&& auditServerConnection != null
				&& auditServerConnection.DatabaseExists(auditDbName))
			{
				databases.Add(auditDbName);
			}

			if (!databases.Contains(edwDbName, StringComparer.OrdinalIgnoreCase)
				&& edwServerConnection != null
				&& edwServerConnection.DatabaseExists(edwDbName))
			{
				databases.Add(edwDbName);
			}
		}

		void RetrieveDatabaseInformationIfNeeded()
		{
			if (!initialised)
			{
				var adIntegrationEnabled = false;
				filterPatterns = new List<string>();

				Helper.PerformActionWithElevatedPermissions(() =>
				{
					singleRefDbName = RefDbTableNameResolver.SingleRefDatabaseName;

					var enterpriseInfoRetriever = new EnterpriseInformationRetriever();
					isHostedInWiseCloud = EnvProxy.IsHostedWithCargowise;
					isDatabaseSecurityModeOpen = enterpriseInfoRetriever.IsDatabaseSecurityModeOpenAccordingToRegistrationKey();

					if (!ignoreStaffMembers)
					{
						var adRegistry = ObjectFactory.Get<IADRegistry>();
						adIntegrationEnabled = adRegistry.IsIntegrationEnabled;
						if (adIntegrationEnabled)
						{
							var userLoginPrefix = adRegistry.UserLoginPrefix;

							if (!string.IsNullOrEmpty(userLoginPrefix))
							{
								foreach (var domainCredential in adRegistry.DomainCredentialsCollection)
								{
									var directorySearcher = ObjectFactory.Get<IDirectorySearcherProvider>().GetDirectorySearcher(domainCredential, false);
									filterPatterns.Add($"{directorySearcher.DomainNetBiosName}\\{userLoginPrefix}");
								}
							}

							var query = new ZDBOnlyQuery(typeof(GlbStaff));

							query.AddToFilter(new ZQuery(GlbStaffSchema.GS_ActiveDirectoryObjectGuid, SQLComparisonOperator.NotEqual, ZGuid.Invalid), JC.And);
							query.AddToFilter(new ZQuery(GlbStaffSchema.GS_ActiveDirectoryObjectGuid, SQLComparisonOperator.NotEqual, ZGuid.Missing), JC.And);
							query.AddToFilter(new ZQuery(GlbStaffSchema.GS_ActiveDirectoryObjectGuid, SQLComparisonOperator.NotEqual, ZGuid.Empty), JC.And);
							query.AddToFilter(new ZQuery(GlbStaffSchema.GS_IsActive, SQLComparisonOperator.Equal, true), JC.And);

							adStaffLoginNames = new GlbStaffCollection(new BusinessObjectFactory(Db.Connection), query)
							.Select(s =>
							{
								string adLoginName = null;
								try
								{
									adLoginName = s.GetDownLevelLogonName();
								}
								catch (Exception ex) when (!ex.IsCriticalException() && !(ex is DatabaseUpgradeException))
								{
									logger.LogToIntegrationLoger(LogType.Warning, $"Failed to get full AD name for staff member with login '{s.GS_LoginName}'", ex);
								}

								return adLoginName;
							})
							.Where(name => !string.IsNullOrEmpty(name))
							.ToList();
						}
					}
				}, cancellationToken);

				auditServerConnection = GetBiConnection(DbRegistry.BiAuditServer.LoadValue(adminConnection));
				dataWarehouseServerConnection = GetBiConnection(DbRegistry.BiDataWarehouseServer.LoadValue(adminConnection), auditServerConnection);

				isDedicatedServerInstanceMap = new Dictionary<string, bool>();
				isDedicatedServerInstanceMap[adminConnection.ServerInstanceName] = DbEnv.Instance.DedicatedSqlServerInstanceDefinition.IsDedicated(adminConnection.ServerInstanceName);
				isDedicatedServerInstanceMap[auditServerConnection?.ServerInstanceName ?? adminConnection.ServerInstanceName] = DbEnv.Instance.DedicatedSqlServerInstanceDefinition.IsDedicated(auditServerConnection?.ServerInstanceName ?? adminConnection.ServerInstanceName);
				isDedicatedServerInstanceMap[dataWarehouseServerConnection?.ServerInstanceName ?? adminConnection.ServerInstanceName] = DbEnv.Instance.DedicatedSqlServerInstanceDefinition.IsDedicated(dataWarehouseServerConnection?.ServerInstanceName ?? adminConnection.ServerInstanceName);

				RetrieveAllDatabases(auditServerConnection, dataWarehouseServerConnection);

				if (!ignoreStaffMembers)
				{
					filterPatterns.Add($"{DbUserRepository.GetStaffDbLoginFullPrefix(mainDatabaseName)}");
				}

				initialised = true;
			}
		}

		public IEnumerable<string> AllDatabases
		{
			get
			{
				RetrieveDatabaseInformationIfNeeded();
				return databases;
			}
		}

		#region BI connections

		AdminConnection GetBiConnection(string biServer, AdminConnection otherBiServerConnection = null)
		{
			if (!string.IsNullOrEmpty(biServer))
			{
				if (biServer.Equals(otherBiServerConnection?.ServerName, StringComparison.OrdinalIgnoreCase))
				{
					return otherBiServerConnection;
				}
				else if (!biServer.Equals(adminConnection.ServerName, StringComparison.OrdinalIgnoreCase))
				{
					return Db.NewAdminConnection(biServer, Db.SqlMasterDb);
				}
			}

			return null;
		}

		public AdminConnection DataWarehouseServerConnection
		{
			get
			{
				RetrieveDatabaseInformationIfNeeded();
				return dataWarehouseServerConnection;
			}
		}

		public AdminConnection AuditServerConnection
		{
			get
			{
				RetrieveDatabaseInformationIfNeeded();
				return auditServerConnection;
			}
		}

		#endregion BI Connections

		public virtual bool IsHostedInWiseCloud
		{
			get
			{
				RetrieveDatabaseInformationIfNeeded();
				return isHostedInWiseCloud;
			}
		}

		public virtual bool IsDedicatedServerInstance(string serverInstanceName)
		{
			RetrieveDatabaseInformationIfNeeded();
			return isDedicatedServerInstanceMap[serverInstanceName];
		}

		public virtual bool IsDatabaseSecurityModeOpen
		{
			get
			{
				RetrieveDatabaseInformationIfNeeded();
				return isDatabaseSecurityModeOpen;
			}
		}

		public IEnumerable<string> StaffPrefixes
		{
			get
			{
				RetrieveDatabaseInformationIfNeeded();
				return filterPatterns;
			}
		}

		public string SingleRefDbName
		{
			get
			{
				RetrieveDatabaseInformationIfNeeded();
				return singleRefDbName;
			}
		}

		#region AdLogins

		IEnumerable<string> adStaffLoginNames;

		/// <summary>
		/// This method is to be removed, once AD logins name convention is established
		/// </summary>
		/// <param name="adminConnection"></param>
		/// <returns></returns>
		public IEnumerable<string> AllStaffADLoginNames
		{
			get
			{
				RetrieveDatabaseInformationIfNeeded();
				return adStaffLoginNames;
			}
		}

		#endregion AdLogins

		#region IDisposable

		public void Dispose()
		{
			dataWarehouseServerConnection?.Dispose();
			auditServerConnection?.Dispose();
		}

		#endregion IDisposable
	}
}
