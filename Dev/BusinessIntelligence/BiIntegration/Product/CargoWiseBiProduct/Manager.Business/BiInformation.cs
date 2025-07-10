namespace CargoWise.Bi.Product.Manager.Business
{
	using System.Text;
	using CargoWise.Bi.Common;
	using CargoWise.Data;

	public class BiInformation
	{
		public void RefreshInfo()
		{
			using (Db.DisposableActionForDbConnection())
			{
				MainDbInfo = new DatabaseInformation(Db.ServerName, Db.DatabaseName, DbType.MainDb);
				AuditDbInfo = new DatabaseInformation(AuditServer, Db.AuditDatabaseName, DbType.BiDb);
				EdwDbInfo = new DatabaseInformation(DataWarehouseServer, Db.EdwDatabaseName, DbType.BiDb);
			}
		}

		public string GetErrorMessage()
		{
			var errorMessage = new StringBuilder();

			if (MainDbInfo != null && !string.IsNullOrEmpty(MainDbInfo.ErrorMessage))
			{
				errorMessage.AppendLine(MainDbInfo.ErrorMessage);
			}
			if (AuditDbInfo != null && !string.IsNullOrEmpty(AuditDbInfo.ErrorMessage))
			{
				errorMessage.AppendLine(AuditDbInfo.ErrorMessage);
			}
			if (EdwDbInfo != null && !string.IsNullOrEmpty(EdwDbInfo.ErrorMessage))
			{
				errorMessage.AppendLine(EdwDbInfo.ErrorMessage);
			}

			return errorMessage.ToString();
		}

		public DatabaseInformation MainDbInfo { get; private set; }
		public DatabaseInformation AuditDbInfo { get; private set; }
		public DatabaseInformation EdwDbInfo { get; private set; }

		public virtual string AuditServer
		{
			get
			{
				return auditServer ?? (auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(Db.Connection));
			}
		}
		string auditServer;

		public virtual string DataWarehouseServer
		{
			get
			{
				return dwServer ?? (dwServer = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(Db.Connection));
			}
		}
		string dwServer;

		public virtual string AnalysisServer
		{
			get
			{
				return analysisServer ?? (analysisServer = BiServers.LoadAnalysisServerUsingCacheIfPossible(Db.Connection));
			}
		}
		string analysisServer;
	}
}
