using CargoWise.Data;

namespace Enterprise.ZArchitecture.Core
{
	public class PrimaryServerConnectionProvider : IConnectionProvider
	{
		public IDbConnectionForReportingWrapper GetNewConnectionWrapper(string dbUserName = null, string applicationNameSuffix = null)
		{
			if (string.IsNullOrWhiteSpace(dbUserName))
			{
				if (applicationNameSuffix == null)
				{
					//will still execute as RestrictedReaderLogin: NativeSqlTableProvider.FillDataTable -> ReadOnlyDataAdapter.Fill -> ReadOnlyDataAdapter.FillSafe -> ExecuteAsReader.Execute
					return new DbConnectionForReportingWrapper(Db.Connection, shouldDisposeConnection: false);
				}
				else
				{
					return new DbConnectionForReportingWrapper(Db.NewExtraConnectionToMainDbWithReaderCredentials(applicationNameSuffix));
				}
			}
			else
			{
				return new DbConnectionForReportingWrapper(Db.NewExtraUnrestrictedWriterConnection(Db.ServerName, Db.DatabaseName, applicationNameSuffix))
					.ImpersonateDbUser(dbUserName);
			}
		}
	}
}
