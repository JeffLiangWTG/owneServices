namespace Enterprise.StlAnalysis.Load
{
	using System;

	class HostedClient
	{
		public HostedClient(int id, string enterpriseCode, string dbServerCode, string companyCode, string name, string offlineServerName, string offlineDatabaseName)
			: this(id, enterpriseCode, dbServerCode, companyCode, name)
		{
			this.offlineServerName = offlineServerName;
			this.offlineDatabaseName = offlineDatabaseName;
		}

		public HostedClient(int id, string enterpriseCode, string dbServerCode, string companyCode, string name)
		{
			this.clientId = id;
			this.enterpriseCode = enterpriseCode;
			this.dbServerCode = dbServerCode;
			this.companyCode = companyCode;
			this.organisationName = name;
		}

		public int ClientId { get { return clientId; } }
		readonly int clientId;

		public string EnterpriseCode { get { return enterpriseCode; } }
		readonly string enterpriseCode;

		public string DbServerCode { get { return dbServerCode; } }
		readonly string dbServerCode;

		public string SystemCode { get { return enterpriseCode + dbServerCode; } }

		public string CompanyCode { get { return companyCode; } }
		readonly string companyCode;

		public string OrganisationName { get { return organisationName; } }
		readonly string organisationName;

		public string OfflineServerName { get { return offlineServerName; } }
		readonly string offlineServerName;

		public string OfflineDatabaseName { get { return offlineDatabaseName; } }
		readonly string offlineDatabaseName;

		public SqlServerInfo DatabaseInfo
		{
			get
			{
				return databaseInfo ?? (databaseInfo = SqlServerInfo.NewClientSpecificSqlServerInfo(this));
			}
		}
		SqlServerInfo databaseInfo;

		public override string ToString()
		{
			return string.Format("{0} ({1}{2})", OrganisationName, SystemCode, String.IsNullOrWhiteSpace(CompanyCode) ? "" : "-" + CompanyCode);
		}
	}
}
