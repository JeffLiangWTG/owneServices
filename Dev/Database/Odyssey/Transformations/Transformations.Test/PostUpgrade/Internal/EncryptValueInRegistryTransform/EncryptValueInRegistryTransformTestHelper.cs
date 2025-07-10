using System;
using System.Data;
using System.Text;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Testing
{
	static class EncryptValueInRegistryTransformTestHelper
	{
		internal static Guid InsertPasswordStmDataRecord(DbConnection connection, string registryName, string value)
		{
			var pk = Guid.NewGuid();

			var sql = $@"
INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Owner, SD_BinaryValue)
VALUES (@pk, '{registryName}', @owner, @binaryValue)";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				if (value != null)
				{
					cmd.AddParameter("@binaryValue", SqlDbType.VarBinary, Encoding.Unicode.GetBytes(value));
				}
				else
				{
					cmd.AddParameter("@binaryValue", SqlDbType.VarBinary, DBNull.Value);
				}
				cmd.AddParameter("@owner", SqlDbType.UniqueIdentifier, DBNull.Value);

				cmd.ExecuteNonQuery();
			}

			return pk;
		}
	}
}
