using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions.Authentication;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GlowInterop.Test
{
	class GlowSingleSignOnTokenProviderTest : TestCase
	{
		public void TestCreateLimitedToken_WhenIsNotCaptiveSession()
		{
			var tokenOptions = new GlowSingleSignOnTokenOptions
			{
				BranchPK = new Guid("1a190c26-1956-4b0a-99a3-dcf9f488bca6"),
				DepartmentPK = new Guid("76b61847-8078-42e1-9f53-6ebd77835cf8"),
				IsCaptiveSession = false,
			};

			TestCreateLimitedTokenCore(tokenOptions, scope =>
			{
				var scopeJsonObject = JObject.Parse(scope);
				AssertEquals((Guid)scopeJsonObject["branch"], new Guid("1a190c26-1956-4b0a-99a3-dcf9f488bca6"));
				AssertEquals((Guid)scopeJsonObject["department"], new Guid("76b61847-8078-42e1-9f53-6ebd77835cf8"));
				AssertEquals(scopeJsonObject.Count, 2);
			});
		}

		public void TestCreateLimitedToken_WhenIsCaptiveSession()
		{
			var tokenOptions = new GlowSingleSignOnTokenOptions
			{
				BranchPK = new Guid("1a190c26-1956-4b0a-99a3-dcf9f488bca6"),
				DepartmentPK = new Guid("76b61847-8078-42e1-9f53-6ebd77835cf8"),
				IsCaptiveSession = true,
			};

			TestCreateLimitedTokenCore(tokenOptions, scope =>
			{
				var scopeJsonObject = JObject.Parse(scope);
				AssertEquals((Guid)scopeJsonObject["branch"], new Guid("1a190c26-1956-4b0a-99a3-dcf9f488bca6"));
				AssertEquals((Guid)scopeJsonObject["department"], new Guid("76b61847-8078-42e1-9f53-6ebd77835cf8"));
				Assert((bool)scopeJsonObject["captive"]);
				AssertEquals(scopeJsonObject.Count, 3);
			});
		}

		public void TestCreateLimitedToken_WhenHavingIPRestriction()
		{
			var tokenOptions = new GlowSingleSignOnTokenOptions
			{
				BranchPK = new Guid("1a190c26-1956-4b0a-99a3-dcf9f488bca6"),
				DepartmentPK = new Guid("76b61847-8078-42e1-9f53-6ebd77835cf8"),
				IPRestriction = "abc",
			};

			TestCreateLimitedTokenCore(tokenOptions, scope =>
			{
				var scopeJsonObject = JObject.Parse(scope);
				AssertEquals((string)scopeJsonObject["ip"], "abc");
				AssertEquals((Guid)scopeJsonObject["branch"], new Guid("1a190c26-1956-4b0a-99a3-dcf9f488bca6"));
				AssertEquals((Guid)scopeJsonObject["department"], new Guid("76b61847-8078-42e1-9f53-6ebd77835cf8"));
				AssertEquals(scopeJsonObject.Count, 3);
			});
		}

		public void TestCreateLimitedToken_WhenHavingEmptyIPRestriction()
		{
			var tokenOptions = new GlowSingleSignOnTokenOptions
			{
				BranchPK = new Guid("1a190c26-1956-4b0a-99a3-dcf9f488bca6"),
				DepartmentPK = new Guid("76b61847-8078-42e1-9f53-6ebd77835cf8"),
				IPRestriction = "",
			};

			TestCreateLimitedTokenCore(tokenOptions, scope =>
			{
				var scopeJsonObject = JObject.Parse(scope);
				AssertEquals((Guid)scopeJsonObject["branch"], new Guid("1a190c26-1956-4b0a-99a3-dcf9f488bca6"));
				AssertEquals((Guid)scopeJsonObject["department"], new Guid("76b61847-8078-42e1-9f53-6ebd77835cf8"));
				AssertEquals(scopeJsonObject.Count, 2);
			});
		}

		public void TestCreateLimitedToken_WhenLoggedInWithSupportToken()
		{
			var tokenOptions = new GlowSingleSignOnTokenOptions
			{
				BranchPK = new Guid("1a190c26-1956-4b0a-99a3-dcf9f488bca6"),
				DepartmentPK = new Guid("76b61847-8078-42e1-9f53-6ebd77835cf8"),
				LoggedInWithSupportToken = true,
				SupportUserCode = "ETL",
				SupportUserName = "Elliott",
			};

			TestCreateLimitedTokenCore(tokenOptions, scope =>
			{
				var scopeJsonObject = JObject.Parse(scope);
				AssertEquals((Guid)scopeJsonObject["branch"], new Guid("1a190c26-1956-4b0a-99a3-dcf9f488bca6"));
				AssertEquals((Guid)scopeJsonObject["department"], new Guid("76b61847-8078-42e1-9f53-6ebd77835cf8"));
				AssertEquals((string)scopeJsonObject["externalUser"]["userCode"], "ETL");
				AssertEquals((string)scopeJsonObject["externalUser"]["userName"], "Elliott");
				AssertEquals(scopeJsonObject.Count, 3);
				AssertEquals(scopeJsonObject["externalUser"].Count(), 2);
			});
		}

		void TestCreateLimitedTokenCore(GlowSingleSignOnTokenOptions tokenOptions, Action<string> scopeAssert)
		{
			var expectedUserPK = new Guid("0dbb385f-9375-448f-94dc-0705ac106d50");
			var accessControlMock = new Mock<ITokenizedAccessControl>();

			Action<string, string, bool, DateTime?, int, AccessTokenInfo> callback = (token, type, isPermanent, expiresAtUtc, useCount, info) =>
			{
				AssertNotNullOrEmpty(token);
				AssertEquals(AccessTokenTypes.LocalIdentity, type);
				Assert(!isPermanent);
				AssertEquals(1, useCount);
				scopeAssert.Invoke(info.Scope);
				AssertEquals(expectedUserPK, info.ParentId);
				AssertEquals(GlbStaffSchema.Constants.Prefix, info.ParentTableCode);
			};
			accessControlMock.Setup(c => c.TryCreate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<AccessTokenInfo>())).Callback(callback).Returns(true);

			ObjectFactory.Substitute(accessControlMock.Object);
			new GlowSingleSignOnTokenProvider().CreateLimitedToken(expectedUserPK, GlbStaffSchema.Constants.Prefix, tokenOptions);

			accessControlMock.Verify(c => c.TryCreate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<AccessTokenInfo>()), Times.Once);
		}

		public void TestCreateLimitedTokenForContact()
		{
			var acessControlMock = new Mock<ITokenizedAccessControl>();
			var contactPK = Guid.NewGuid();
			Action<string, string, bool, DateTime?, int, AccessTokenInfo> callback = (token, type, isPermanent, expiresAtUtc, useCount, info) =>
			{
				AssertNotNullOrEmpty(token);
				AssertEquals(type, AccessTokenTypes.LocalIdentity);
				Assert(!isPermanent);
				AssertEquals(useCount, 1);

				var scopeJsonObject = JObject.Parse(info.Scope);
				AssertEquals((Guid)scopeJsonObject["branch"], Env.CurrentBranchPK);
				AssertEquals((Guid)scopeJsonObject["department"], Env.CurrentDepartmentPK);
				AssertEquals((string)scopeJsonObject["externalUser"]["userCode"], "abc");
				AssertEquals((string)scopeJsonObject["externalUser"]["userName"], "Developer");
				AssertEquals(scopeJsonObject.Count, 3);
				AssertEquals(scopeJsonObject["externalUser"].Count(), 2);

				AssertEquals(info.ParentId, contactPK);
				AssertEquals(info.ParentTableCode, OrgContactSchema.Constants.Prefix);
			};

			acessControlMock.Setup(c => c.TryCreate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<AccessTokenInfo>())).Callback(callback).Returns(true);
			ObjectFactory.Substitute(acessControlMock.Object);

			new GlowSingleSignOnTokenProvider().CreateLimitedTokenForContact(contactPK);

			acessControlMock.Verify(c => c.TryCreate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<AccessTokenInfo>()), Times.Once);
		}
	}
}
