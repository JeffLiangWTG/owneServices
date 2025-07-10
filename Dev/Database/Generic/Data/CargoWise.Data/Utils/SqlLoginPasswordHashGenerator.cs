namespace CargoWise.Data
{
	public static class SqlLoginPasswordHashGenerator
	{
		public static byte[] GenerateFromDb(DbConnection connection, string password)
		{
			return connection.ExecuteScalar<byte[]>(
						"SELECT PWDENCRYPT(@password)",
						cmd => cmd.AddParameter("@password", System.Data.SqlDbType.NVarChar, 128, password));
		}
	}
}
