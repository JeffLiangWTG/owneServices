using System;
using System.Data;
using System.IO;
using System.Text;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;
using ThirdParty.BouncyCastle.OpenSsl;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Registry
{
	public class UpdateMyAccountSSOJWTTokenExchangePrivateKeyRegistryDataTransformation : RegistryDataTransformation
	{
		public override string UserDescription => @"Convert the encoding format of MyAccountSSOJWTTokenExchangePrivateKey from Unicode to UTF-8.";

		protected override void OfflinePostUpgradeTransform()
		{
			string tokenExchangePrivateKey = "MyAccountSSOJWTTokenExchangePrivateKey";
			using (var dataTable = GetDataTable(tokenExchangePrivateKey))
			{
				foreach (DataRow dataRow in dataTable.Rows)
				{
					var pk = dataRow.Field<Guid>(StmDataSchema.Constants.PK);
					var originBinaryValue = dataRow.Field<byte[]>(StmDataSchema.Constants.SD_BinaryValue);
					if (originBinaryValue != null)
					{
						var encodedUnicodeValue = Encoding.Unicode.GetString(originBinaryValue);
						using (var valueReader = new StringReader(encodedUnicodeValue))
						{
							var pemObj = new PemReader(valueReader).ReadPemObject();
							if (pemObj == null)
							{
								continue;
							}

							var utf8Bytes = Encoding.UTF8.GetBytes(encodedUnicodeValue);
							UpdateDatabaseValue(pk, utf8Bytes);
						}
					}
				}
			}
		}
	}
}
