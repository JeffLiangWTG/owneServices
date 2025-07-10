using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using Enterprise.Build.Database.Script.Public.Glow.TestHelpers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow.Test;

[TestedType(typeof(GetCurrentActiveAccessToken))]
sealed class GetCurrentActiveAccessTokenTest : DbCreateScriptTest
{
	public void TestCanGetActiveToken()
	{
		StmAccessTokenHelper.CreateStmAccessToken("123456789012345678901234567890", "ZZZ", DateTime.UtcNow.AddMonths(1), -1);

		var result = Execute("ZZZ");

		AssertEquals("Token", "123456789012345678901234567890", result);
	}

	public void TestCanGetActiveToken_WhenMultipleTokensExist()
	{
		StmAccessTokenHelper.CreateStmAccessToken("5days", "ZZZ", DateTime.UtcNow.AddDays(5), -1);
		StmAccessTokenHelper.CreateStmAccessToken("10days", "ZZZ", DateTime.UtcNow.AddDays(10), -1);
		StmAccessTokenHelper.CreateStmAccessToken("1day", "ZZZ", DateTime.UtcNow.AddDays(1), -1);
		StmAccessTokenHelper.CreateStmAccessToken("Expired", "ZZZ", DateTime.UtcNow.AddMinutes(-1), -1);

		var result = Execute("ZZZ");

		AssertEquals("Token", "10days", result);
	}

	public void TestCanGetActiveToken_WhenNoTokenExists()
	{
		var result = Execute("ZZZ");

		AssertEquals("Token", null, result);
	}

	public void TestCanGetActiveToken_WhenOnlyDifferentTypeTokenExists()
	{
		StmAccessTokenHelper.CreateStmAccessToken("10days", "XXX", DateTime.UtcNow.AddDays(10), -1);
		StmAccessTokenHelper.CreateStmAccessToken("5days", "YYY", DateTime.UtcNow.AddDays(5), -1);

		var result = Execute("ZZZ");

		AssertEquals("Token", null, result);
	}

	public void TestCanGetActiveToken_WhenTokenExpired()
	{
		StmAccessTokenHelper.CreateStmAccessToken("Expired", "ZZZ", DateTime.UtcNow.AddMinutes(-1), -1);

		var result = Execute("ZZZ");

		AssertEquals("Token", null, result);
	}

	static string Execute(string type)
	{
		using var command = Db.Connection.Command("GetCurrentActiveAccessToken");
		command.CommandType = CommandType.StoredProcedure;

		command.AddParameter("@Type", SqlDbType.VarChar, type);
			
		command.AddOutputParameter("@AccessToken", SqlDbType.VarChar, -1, 0, 0, null);

		command.ExecuteNonQuery();

		var parameterValue = command.GetParameterValue("@AccessToken");
		return parameterValue == DBNull.Value ? null : (string)parameterValue;
	}
}
