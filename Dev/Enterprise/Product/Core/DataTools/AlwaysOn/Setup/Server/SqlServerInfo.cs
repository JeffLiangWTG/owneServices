using System;
using System.Text.RegularExpressions;
using CargoWise.DataProtection.Administration.SqlServer;

namespace Enterprise.AlwaysOn.Setup
{
	public class SqlServerInfo
	{
		bool detailsLoaded;

		string serverAlias;
		string serverDomain;
		string serverMachineName;
		string serverFQDN;
		string serverInstanceName;
		readonly int serverPortNumber;

		public SqlServerInfo(string alias, int portNumber = default)
		{
			var nameAndInstance = alias.Split('\\');
			string serverName = GetLocalMachineNameForLocalhost(nameAndInstance[0]);
			string instanceName = (nameAndInstance.Length > 1) ? nameAndInstance[1] : "";

			serverMachineName = serverName;
			if (string.IsNullOrEmpty(instanceName))
			{
				serverInstanceName = DefaultSqlServerInstanceName;
			}
			else
			{
				serverInstanceName = instanceName;
			}
			serverAlias = nameAndInstance.Length < 2 ? serverName : $"{serverName}\\{instanceName}";

			serverPortNumber = portNumber;
		}

		public SqlServerInfo(string alias, string serverMachineName, string serverInstanceName, string serverDomainName, string serverFQDN, int portNumber) : this(alias, portNumber)
		{
			this.serverInstanceName = !string.IsNullOrEmpty(serverInstanceName) ? serverInstanceName : DefaultSqlServerInstanceName;
			this.serverMachineName = serverMachineName;
			this.serverDomain = serverDomainName;
			this.serverFQDN = serverFQDN;

			this.detailsLoaded = true;
		}

		public void LoadDetailsFromServer(ISqlExecutionContext sqlContext)
		{
			const string getNomeNameSql = @"
			DECLARE @fqdn varchar(255);
			DECLARE @domain varchar(255);
			EXEC xp_getnetname @fqdn output, 1;
			EXEC xp_getnetname @domain output, 2;
			SELECT
				SERVERPROPERTY('MachineName'),
				@@SERVERNAME,
				@@SERVICENAME,
				@fqdn ,
				@domain;
			";

			using (var reader = sqlContext.ExecuteReader(getNomeNameSql))
			{
				reader.Read();

				var rawMachineName = reader[0];
				var rawServerName = reader[1];
				var rawInstanceNameObj = reader[2];
				var rawFqdnObj = reader[3];
				var rawDomainObj = reader[4];

				if (rawMachineName == null || rawInstanceNameObj == null || rawFqdnObj == null || rawDomainObj == null)
				{
					throw new InvalidOperationException("MachineName column or servername column or fqdn column or domain column returned a null value");
				}

				this.serverMachineName = TrimEOS((string)rawMachineName);
				this.serverAlias = TrimEOS((string)rawServerName);
				this.serverDomain = TrimEOS((string)rawDomainObj);
				this.serverFQDN = TrimEOS((string)rawFqdnObj);
				this.serverInstanceName = TrimEOS((string)rawInstanceNameObj);
			}

			detailsLoaded = true;
		}

		static string TrimEOS(string rawString)
		{
			var eosIndex = rawString.IndexOf('\0');
			rawString = (eosIndex >= 0) ? rawString.Substring(0, eosIndex) : rawString;
			return rawString.Trim();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public override string ToString()
		{
			string result = (IsDefaultInstance) ? InstanceName + " (default instance)" : InstanceName;
			return result ?? string.Empty;
		}

		public string FullDataSource
		{
			get
			{
				if (PortNumber == default)
				{
					return ServerInstanceFQDN;
				}
				else
				{
					return $"{ServerInstanceFQDN},{PortNumber}";
				}
			}
		}

		public string AliasDataSource
		{
			get
			{
				if (PortNumber == default)
				{
					return ServerAlias;
				}
				else
				{
					return $"{ServerAlias},{PortNumber}";
				}
			}
		}

		public string ServerAlias => serverAlias;
		public int PortNumber => serverPortNumber;
		public string ServerDomain => ThrowIfNotLoaded(serverDomain);
		public string ServerFQDN => ThrowIfNotLoaded(serverFQDN);
		public string InstanceName => serverInstanceName;
		public string ServerMachineName => serverMachineName;
		public string ServerInstanceFQDN => IsDefaultInstance ? ServerFQDN : $"{ServerFQDN}\\{serverInstanceName}";
		public bool DetailsLoaded => detailsLoaded;

		string ThrowIfNotLoaded(string value)
		{
			return !detailsLoaded ? throw new InvalidOperationException("Not Loaded!") : value;
		}

		#region Implementation

		bool IsDefaultInstance
		{
			get
			{
				return InstanceName == DefaultSqlServerInstanceName;
			}
		}

		static string GetLocalMachineNameForLocalhost(string serverName)
		{
			return Regex.Replace(serverName, @"^(localhost|\(local\)|\.)", System.Environment.MachineName, RegexOptions.IgnoreCase);
		}

		public const string DefaultSqlServerInstanceName = "MSSQLSERVER";

		#endregion
	}
}
