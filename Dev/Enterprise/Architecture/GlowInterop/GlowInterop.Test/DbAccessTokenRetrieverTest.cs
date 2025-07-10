using System;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Definitions.Authentication;
using CargoWise.SystemToSystemTrust;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GlowInterop.Test;

class DbAccessTokenRetrieverTest : TransactionedTestCase
{
	ITokenizedAccessControl accessControl;
	IDbAccessTokenRetriever dbAccessTokenRetriever;

	protected override void SetUp()
	{
		base.SetUp();

		accessControl = ObjectFactory.Get<ITokenizedAccessControl>();
		dbAccessTokenRetriever = ObjectFactory.Get<IDbAccessTokenRetriever>();
	}

	protected override void TearDown()
	{
		accessControl = null;
		dbAccessTokenRetriever = null;

		base.TearDown();
	}

	public void TestObjectFactoryRegistration()
	{
		CombineAssertions(() =>
		{
			AssertType<TokenizedAccessControl>(accessControl);
			AssertType<DbAccessTokenRetriever>(dbAccessTokenRetriever);
		});
	}

	public void TestGetDbAccessTokenAsync_WhenTokenExists_ReturnsAccessToken()
	{
		var info = new AccessTokenInfo("SomeScopeData", Guid.NewGuid(), "Z0");
		var token = accessControl.CreateLimitedToken(AccessTokenTypes.ApiDbToken, info, TimeSpan.FromMinutes(5), 1);

		var result = dbAccessTokenRetriever.GetDbAccessTokenAsync(CancellationToken.None).GetAwaiter().GetResult();

		AssertEquals(token, result);
	}

	public void TestGetDbAccessTokenAsync_WhenMultipleTokensExist_ReturnsAccessTokenWithLongestExpiry()
	{
		var info = new AccessTokenInfo("SomeScopeData", Guid.NewGuid(), "Z0");
		var tokens = new int?[] { 1, 10, -10, 5, null, }
			.Select<int?, TimeSpan?>(min => min.HasValue ? TimeSpan.FromMinutes(min.Value) : null)
			.Select(time => accessControl.CreateLimitedToken(AccessTokenTypes.ApiDbToken, info, time, 1))
			.ToArray();

		var result = dbAccessTokenRetriever.GetDbAccessTokenAsync(CancellationToken.None).GetAwaiter().GetResult();
		AssertEquals($"Unexpected result. Tokens: {string.Join(", ", tokens)}.", tokens[1], result);
	}

	public void TestGetDbAccessTokenAsync_WhenTokenDoesNotExist_ReturnsNull()
	{
		var result = dbAccessTokenRetriever.GetDbAccessTokenAsync(CancellationToken.None).GetAwaiter().GetResult();

		AssertNull(nameof(result), result);
	}

	public void TestGetDbAccessTokenAsync_WhenTokenExpires_ReturnsNull()
	{
		var info = new AccessTokenInfo("SomeScopeData", Guid.NewGuid(), "Z0");
		var token = accessControl.CreateLimitedToken(AccessTokenTypes.ApiDbToken, info, TimeSpan.FromMinutes(-5), 1);

		var result = dbAccessTokenRetriever.GetDbAccessTokenAsync(CancellationToken.None).GetAwaiter().GetResult();

		AssertNull(nameof(result), result);
	}
}
