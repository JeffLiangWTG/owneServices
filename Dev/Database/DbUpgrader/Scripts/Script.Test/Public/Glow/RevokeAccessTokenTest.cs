using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using Enterprise.Build.Database.Script.Public.Glow.TestHelpers;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow.Test
{
	[TestedType(typeof(RevokeAccessToken))]
	sealed class RevokeAccessTokenTest : DbCreateScriptTest
	{
		public void TestRevokeDeletesAccessToken()
		{
			const string Token = "abcdef";

			Assert("Sanity check - precondition: Token should exist", Exists(Token));
			Execute(Token);
			Assert("Token should still not exist :)", !Exists(Token));
		}

		public void TestRevokeDoesNotFailIfTokenDoesNotExist()
		{
			const string Token = "ImaginaryToken";

			Assert("Sanity check - precondition: Token should not exist", !Exists(Token));
			Execute(Token);
			Assert("Token should still not exist :)", !Exists(Token));
		}

		protected override void SetUp()
		{
			base.SetUp();

			ClearTable(StmAccessTokenSchema.Constants.TableName);
			StmAccessTokenHelper.CreateStmAccessToken("abcdef", "\\o/", null, 100);
		}

		static void ClearTable(string tableName)
		{
			var query = string.Format(CultureInfo.InvariantCulture, "DELETE FROM [{0}]", tableName);
			using (var command = Db.Connection.Command(query))
			{
				command.ExecuteNonQuery();
			}
		}

		static bool Exists(string token)
		{
			using (var command = Db.Connection.Command("IF EXISTS(SELECT * FROM dbo.StmAccessToken WHERE SAT_Token = @Token) SELECT 1 ELSE SELECT 0"))
			{
				command.AddParameter("@Token", SqlDbType.VarChar, token);

				return (int)command.ExecuteScalar() > 0;
			}
		}

		static void Execute(string token)
		{
			using (var command = Db.Connection.Command("RevokeAccessToken"))
			{
				command.AddParameter("@Token", SqlDbType.VarChar, token);

				command.ExecuteProcedureWithReturnValue();
			}
		}
	}
}

