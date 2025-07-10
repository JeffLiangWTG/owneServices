using NUnit.Framework;

namespace CargoWise.Data.Test.Utils
{
	class SqlLoginPasswordHashGeneratorTest : TestCase
	{
		public void TestGenerateFromDb()
		{
			// Arrange
			var plainTextPassword = "Pas!word123";

			// Act
			var passwordHash = SqlLoginPasswordHashGenerator.GenerateFromDb(Db.Connection, plainTextPassword);

			// Assert
			Assert(Db.Connection.ExecuteScalar<bool>("SELECT CAST(PWDCOMPARE(@password, @hash) AS bit)", cmd =>
			{
				cmd.AddParameter("@password", System.Data.SqlDbType.NVarChar, 128, plainTextPassword);
				cmd.AddParameter("@hash", System.Data.SqlDbType.VarBinary, 128, passwordHash);
			}));
		}
	}
}
