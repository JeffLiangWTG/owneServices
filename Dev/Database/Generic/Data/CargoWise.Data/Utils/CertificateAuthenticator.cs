using System;
using CargoWise.DataLink;

namespace CargoWise.Data
{
	class CertificateAuthenticator
	{
		public bool Verify()
		{
			var result = false;
			var query = string.Format(@"IF (CERTENCODED(CERT_ID('WtgServer')) = {0}) SELECT 1 ELSE SELECT 0", CertificateKey); // Query to get WtgServer location
			using (((ICurrentDbControl)Connection).UseDatabase(Db.SqlMasterDb))
			{
				using (var cmd = Connection.Command(query))
				{
					result = Convert.ToBoolean(cmd.ExecuteScalar());
				}
			}

			return result;
		}

		public AdminConnection Connection
		{
			get
			{
				if (adminConnection == null)
				{
					adminConnection = Db.NewAdminConnection();
				}
				return adminConnection;
			}
		}
		AdminConnection adminConnection;

		protected virtual string CertificateKey
		{
			get
			{
				using (var token = DataLinkManager.Instance.NewToken())
				{
					return token.GetProperty(token[DataLinkEnum.WSK]);
				}
			}
		}
	}
}
