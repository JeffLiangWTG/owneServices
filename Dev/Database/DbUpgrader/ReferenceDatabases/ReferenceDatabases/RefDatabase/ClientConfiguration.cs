using CargoWise.Data;
using CargoWise.RefDbRepo.Client.Common;

namespace Enterprise.DbUpgrader.ReferenceDatabases
{
	public class ClientConfiguration : IClientConfiguration
	{
		public string ServerUri
		{
			get
			{
				return GetServiceUriFromStmData();
			}
		}

		public string ClientId => string.Empty;

		public string ClientPassword => string.Empty;

		public bool CompressData => false;

		public int ClientTimeout => 0;

		public string SystemType => string.Empty;

		string GetServiceUriFromStmData()
		{
			var sqlText = $@"SELECT CONVERT(NVARCHAR(MAX), SD_BinaryValue)
FROM dbo.StmData
WHERE SD_Name = '{RefDbRepoServiceUriName}'";
			var uri = Db.Connection.ExecuteScalar(sqlText)?.ToString();
			if (string.IsNullOrEmpty(uri))
			{
				uri = @"https://refdbrepo.wisegrid.net/";
			}
			return uri;
		}

		const string RefDbRepoServiceUriName = "RemoteDatabaseServiceUri";
	}
}
