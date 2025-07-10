using CargoWise.Data;
using NUnit.Framework;

namespace CargoWise.ServiceManager.Next.Launcher.Test;

public class AccessTokenServiceTest : TransactionedTestCase
{
	AccessTokenService? accessTokenService;

	public void Test_RotateToken_CreatesWhenNeededTest()
	{
		var oldTokenString = "oneMinuteLeft";
		var expiration = TimeSpan.FromMinutes(1);
		AddSQLTestData(oldTokenString, expiration);

		var tokenCount = GetTokenCount();
		AssertEquals("token count", 1, tokenCount);

		var result = accessTokenService!.RotateToken(TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(2));

		tokenCount = GetTokenCount();
		var sqlTokenStringText = $"SELECT SAT_Token FROM dbo.StmAccessToken WHERE SAT_Token != '{oldTokenString}'";
		CombineAssertions(() =>
		{
			AssertEquals("result", true, result);
			AssertEquals("token count", 2, tokenCount);
		});
		var tokenString = Db.Connection.ExecuteScalar<string>(sqlTokenStringText);
		var regex = new System.Text.RegularExpressions.Regex("^[A-F0-9]{30}$");
		AssertMatch("token", regex, tokenString);
	}

	public void Test_RotateToken_TokenIsNotCreatedWhenOverlapIsNotExceededTest()
	{
		var oldTokenString = "threeMinutesLeft";
		var expiration = TimeSpan.FromMinutes(3);
		AddSQLTestData(oldTokenString, expiration);

		var tokenCount = GetTokenCount();
		AssertEquals("token count", 1, tokenCount);

		var result = accessTokenService!.RotateToken(TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(2));

		tokenCount = GetTokenCount();
		CombineAssertions(() =>
		{
			AssertEquals("result", false, result);
			AssertEquals("token count", 1, tokenCount);
		});
	}

	public void Test_CheckTokenValid_ReturnsTrueWhenTokenIsValidTest()
	{
		var oldTokenString = "threeMinutesLeft";
		var expiration = TimeSpan.FromMinutes(3);
		AddSQLTestData(oldTokenString, expiration);

		var tokenCount = GetTokenCount();
		AssertEquals("token count", 1, tokenCount);

		var result = accessTokenService!.CheckTokenValidity(oldTokenString);

		AssertEquals("result", true, result);
	}

	public void Test_CheckTokenValid_ReturnsFalseWhenTokenDoesntExistTest()
	{
		var result = accessTokenService!.CheckTokenValidity("noToken");

		AssertEquals("result", false, result);
	}

	public void Test_CheckTokenValid_ReturnsFalseWhenTokenIsInvalidTest()
	{
		var oldTokenString = "expired";
		var expiration = TimeSpan.FromMinutes(-1);
		AddSQLTestData(oldTokenString, expiration);

		var tokenCount = GetTokenCount();
		AssertEquals("token count", 1, tokenCount);

		var result = accessTokenService!.CheckTokenValidity(oldTokenString);

		AssertEquals("result", false, result);
	}

	protected override void SetUp()
	{
		base.SetUp();
		accessTokenService = new AccessTokenService();
		Db.Connection.ExecuteScalar("DELETE FROM dbo.StmAccessToken");
	}

	static int GetTokenCount()
	{
		var tokenCount = Db.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.StmAccessToken");
		return tokenCount;
	}

	void AddSQLTestData(string oldTokenString, TimeSpan expiration)
	{
		var sqlText = $"""
INSERT INTO dbo.StmAccessToken
(SAT_PK, SAT_Token, SAT_Type, SAT_ExpiresAt, SAT_RemainingUseCount, SAT_ParentId, SAT_ParentTableCode, SAT_SystemCreateTimeUtc, SAT_SystemLastEditTimeUtc, SAT_SystemCreateUser, SAT_SystemLastEditUser)
VALUES
(NEWID(), '{oldTokenString}', 'ADB', DATEADD(SECOND, {expiration.TotalSeconds}, SYSUTCDATETIME()), -1, NEWID(), 'SEC', SYSUTCDATETIME(), SYSUTCDATETIME(), 'XYZ', 'XYZ')
""";

		Db.Connection.ExecuteScalar(sqlText);
	}
}
