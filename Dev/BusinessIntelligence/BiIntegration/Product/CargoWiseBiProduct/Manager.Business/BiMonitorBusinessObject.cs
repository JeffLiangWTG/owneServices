using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace CargoWise.Bi.Product.Manager.Business
{
	public class BiMonitorBusinessObject : NonPersistentBusinessObject
	{
		public BiMonitorBusinessObject()
		{
			if (!IsDbSetup())
			{
				BiInformation = new BiInformation();
				CdcInformation = new CdcInformation();
				AuditInformation = new AuditInformation(null, null);
				EdwInformation = new EdwInformation(null, null);
				AnalysisServerInfo = new AnalysisServerInformation(null, null);
				PowerBiServerInfo = new PowerBiServerInformation(null);
			}
		}

		public bool IsDbSetup()
		{
			try
			{
				return !string.IsNullOrEmpty(Db.ServerName);
			}
			catch (InvalidOperationException)
			{
				return false;
			}
		}

		#region BI Servers Information

		public BiInformation BiInformation { get; private set; }

		public void RefreshBiInformation()
		{
			BiInformation = new BiInformation();
			BiInformation.RefreshInfo();
		}

		#endregion

		#region CDC Information

		public CdcInformation CdcInformation { get; private set; }

		public void RefreshCdcInformation()
		{
			CdcInformation = new CdcInformation();
			CdcInformation.RefreshInfo();
		}

		#endregion

		#region Audit Information

		public bool IsAuditUpdated()
		{
			return !string.IsNullOrEmpty(BiInformation.AuditDbInfo.ServerName) &&
				!string.IsNullOrEmpty(BiInformation.AuditDbInfo.DatabaseName) &&
				BiInformation.MainDbInfo.DatabaseVersion == BiInformation.AuditDbInfo.DatabaseVersion;
		}

		public AuditInformation AuditInformation { get; set; }

		public void RefreshAuditInformation()
		{
			if (IsAuditUpdated())
			{
				AuditInformation = new AuditInformation(BiInformation.AuditServer, BiInformation.AuditDbInfo.DatabaseName);
				AuditInformation.RefreshInfo();
			}
		}

		public int FilterCdcHistorySummary(string tableName, ZDateTime utcFromDate, ZDateTime utcToDate)
		{
			AuditInformation = new AuditInformation(BiInformation.AuditServer, BiInformation.AuditDbInfo.DatabaseName);
			return AuditInformation.FilterCdcHistorySummary(tableName, utcFromDate, utcToDate);
		}

		#endregion

		#region EDW Information

		public bool IsEdwUpdated()
		{
			return !string.IsNullOrEmpty(BiInformation.EdwDbInfo.ServerName) &&
				!string.IsNullOrEmpty(BiInformation.EdwDbInfo.DatabaseName) &&
				BiInformation.MainDbInfo.DatabaseVersion == BiInformation.EdwDbInfo.DatabaseVersion;
		}

		public EdwInformation EdwInformation { get; set; }

		public void RefreshEdwInformation()
		{
			if (IsEdwUpdated())
			{
				EdwInformation = new EdwInformation(BiInformation.DataWarehouseServer, BiInformation.EdwDbInfo.DatabaseName);
				EdwInformation.RefreshInfo();
			}
		}

		#endregion

		#region Analysis Server Information

		public bool IsAnalysisServerSet()
		{
			return !string.IsNullOrEmpty(BiInformation.AnalysisServer);
		}

		public AnalysisServerInformation AnalysisServerInfo { get; set; }

		public void RefreshAnalysisCubeInformation()
		{
			if (IsAnalysisServerSet())
			{
				AnalysisServerInfo = new AnalysisServerInformation(BiInformation.AnalysisServer, BiInformation.DataWarehouseServer);
				AnalysisServerInfo.RefreshInfo();
			}
		}

		public string AnalysisServerName
		{
			get
			{
				return AnalysisServerInfo?.ServerName;
			}
		}

		public string AnalysisServerVersion
		{
			get
			{
				return AnalysisServerInfo?.ServerVersion;
			}
		}

		public string AnalysisServerMode
		{
			get
			{
				return AnalysisServerInfo?.ServerMode;
			}
		}

		#endregion

		#region Power BI Information

		public PowerBiServerInformation PowerBiServerInfo { get; set; }

		public void RefreshPowerBiServerInformation()
		{
			if (!string.IsNullOrEmpty(PowerBiServer))
			{
				PowerBiServerInfo = new PowerBiServerInformation(PowerBiServer);
			}
		}

		public virtual string PowerBiServer
		{
			get
			{
				return SystemDataRegistry.Instance.BiPowerBiWebPortalUrl.Value;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		public string PowerBiWebPortalUrl
		{
			get
			{
				return PowerBiServerInfo?.PowerBiWebPortalUrl;
			}
		}

		public string PowerBiServerVersion
		{
			get
			{
				return PowerBiServerInfo?.PowerBiServerVersion;
			}
		}

		public string PowerBiReportsVersion
		{
			get
			{
				return PowerBiServerInfo?.PowerBiReportsVersion;
			}
		}

		public string PowerBiReportsStatus
		{
			get
			{
				return PowerBiServerInfo?.PowerBiReportsStatus;
			}
		}

		#endregion
	}
}
