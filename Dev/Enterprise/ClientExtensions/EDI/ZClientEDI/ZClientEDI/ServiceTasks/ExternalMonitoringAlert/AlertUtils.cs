using CargoWise.Data;

namespace Enterprise.Client.EDI.ServiceTasks.ExternalMonitoringAlert
{
	class AlertUtils
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1065:EncryptSqlConnection", Justification = "Baseline")]
		internal static DbConnection GetDbConnection(string connectionString)
		{
			var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
			return string.IsNullOrEmpty(connectionStringBuilder.UserID) ? Db.NewExtraConnectionIntegratedSecurityEnabled(connectionStringBuilder.DataSource, connectionStringBuilder.InitialCatalog)
				: Db.NewExtraConnection(connectionStringBuilder.DataSource, connectionStringBuilder.InitialCatalog, connectionStringBuilder.UserID, connectionStringBuilder.Password);
		}
	}
}
