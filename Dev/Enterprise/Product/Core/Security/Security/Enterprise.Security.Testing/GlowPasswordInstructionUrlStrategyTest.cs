using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.GlowInterop;
using Moq;
using Newtonsoft.Json;

namespace Enterprise.Security.Testing
{
	sealed class GlowPasswordInstructionUrlStrategyTest : TestCaseWithFactory
	{
		public void TestGenerateUrl_HasNavigateUrlShouldReturnNavigationUrl()
		{
			var result = strategy.GenerateUrl(sourceMock.Object, PasswordInstructionType.Reset, new PasswordResetInfo { NavigateUrl = new Uri("https://www.google.com") });

			AssertEquals("https://www.google.com/", result);
			accessControlMock.Verify(
				x => x.TryCreate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<AccessTokenInfo>()),
				Times.Never);
		}

		public void TestGenerateUrl_HasNoNavigateUrlNoExternalPortalsUri_ShouldReturnGlowPortalsUrlAndCreateToken_Set()
		{
			disposables.Add(GlowRegistry.Instance.GlowExternalUserPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""));

			AssertStrategyGenerateUrl("https://www.glow.com/Portals/GHC", PasswordInstructionType.Set);
		}

		public void TestGenerateUrl_HasNoNavigateUrlNoExternalPortalsUri_ShouldReturnGlowPortalsUrlAndCreateToken_Reset()
		{
			disposables.Add(GlowRegistry.Instance.GlowExternalUserPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""));

			AssertStrategyGenerateUrl("https://www.glow.com/Portals/GHC", PasswordInstructionType.Reset);
		}

		public void TestGenerateUrl_HasNoNavigateUrlWithExternalPortalsUri_ShouldReturnExternalPortalsUrlAndCreateToken_Set()
		{
			disposables.Add(GlowRegistry.Instance.GlowExternalUserPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://www.external.com/Portals/"));

			AssertStrategyGenerateUrl("https://www.external.com/Portals/GHC", PasswordInstructionType.Set);
		}

		public void TestGenerateUrl_HasNoNavigateUrlWithExternalPortalsUri_ShouldReturnExternalPortalsUrlAndCreateToken_Reset()
		{
			disposables.Add(GlowRegistry.Instance.GlowExternalUserPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://www.external.com/Portals/"));

			AssertStrategyGenerateUrl("https://www.external.com/Portals/GHC", PasswordInstructionType.Reset);
		}

		public void TestGenerateUrl_HasDefaultWebPortalNeoUnrestricted_ShouldReturnNeoPortalUrlAndCreateToken_Set()
		{
			using (GlowRegistryTestHelper.SetFeatureFlagNeo())
			{
				AssertStrategyGenerateUrl("https://www.glow.com/Portals/NEO", PasswordInstructionType.Set);
			}
		}

		public void TestGenerateUrl_HasDefaultWebPortalNeoUnrestricted_ShouldReturnNeoPortalUrlAndCreateToken_Reset()
		{
			using (GlowRegistryTestHelper.SetFeatureFlagNeo())
			{
				AssertStrategyGenerateUrl("https://www.glow.com/Portals/NEO", PasswordInstructionType.Reset);
			}
		}

		public void TestGenerateUrl_HasGhcWebPortalNeoUnrestricted_ShouldReturnGhcPortalUrlAndCreateToken_Set()
		{
			using (GlowRegistryTestHelper.SetFeatureFlagNeo())
			{
				disposables.Add(WebDataRegistry.Instance.DefaultCargoWiseWebPortal.SetTemporaryValue(Guid.Empty,
					Guid.Empty, Guid.Empty, DefaultCargoWiseWebPortalCodeList.Codes.GHC));

				AssertStrategyGenerateUrl("https://www.glow.com/Portals/GHC", PasswordInstructionType.Set);
			}
		}

		public void TestGenerateUrl_HasGhcWebPortalNeoUnrestricted_ShouldReturnGhcPortalUrlAndCreateToken_Reset()
		{
			using (GlowRegistryTestHelper.SetFeatureFlagNeo())
			{
				disposables.Add(WebDataRegistry.Instance.DefaultCargoWiseWebPortal.SetTemporaryValue(Guid.Empty,
					Guid.Empty, Guid.Empty, DefaultCargoWiseWebPortalCodeList.Codes.GHC));

				AssertStrategyGenerateUrl("https://www.glow.com/Portals/GHC", PasswordInstructionType.Reset);
			}
		}

		public void TestGenerateUrl_HasDefaultWebPortalNeoRestricted_ShouldReturnGhcPortalUrlAndCreateToken_Set()
		{
			AssertStrategyGenerateUrl("https://www.glow.com/Portals/GHC", PasswordInstructionType.Set);
		}

		public void TestGenerateUrl_HasDefaultWebPortalNeoRestricted_ShouldReturnGhcPortalUrlAndCreateToken_Reset()
		{
			AssertStrategyGenerateUrl("https://www.glow.com/Portals/GHC", PasswordInstructionType.Reset);
		}

		public void TestGenerateUrl_HasGhcWebPortalNeoRestricted_ShouldReturnGhcPortalUrlAndCreateToken_Set()
		{
			disposables.Add(WebDataRegistry.Instance.DefaultCargoWiseWebPortal.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DefaultCargoWiseWebPortalCodeList.Codes.GHC));

			AssertStrategyGenerateUrl("https://www.glow.com/Portals/GHC", PasswordInstructionType.Set);
		}

		public void TestGenerateUrl_HasGhcWebPortalNeoRestricted_ShouldReturnGhcPortalUrlAndCreateToken_Reset()
		{
			disposables.Add(WebDataRegistry.Instance.DefaultCargoWiseWebPortal.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DefaultCargoWiseWebPortalCodeList.Codes.GHC));

			AssertStrategyGenerateUrl("https://www.glow.com/Portals/GHC", PasswordInstructionType.Reset);
		}

		void AssertStrategyGenerateUrl(string expectedBaseUrl, PasswordInstructionType instructionType)
		{
			var url = strategy.GenerateUrl(sourceMock.Object, instructionType, null);

			AssertEquals($"{expectedBaseUrl}?token={createdToken}#/resetPassword", url);

			accessControlMock.Verify(
				x => x.TryCreate(It.IsAny<string>(), AccessTokenTypes.GlowPasswordReset, false, It.IsAny<DateTime>(), 1, It.IsAny<AccessTokenInfo>()),
				Times.Once);
			AssertEquals(userPk, tokenInfo.ParentId);
			AssertEquals("OC", tokenInfo.ParentTableCode);

			var scopeData = JsonConvert.DeserializeObject<PasswordResetScopeData>(tokenInfo.Scope);
			AssertEquals("FOOBAR/user@test.com", scopeData.UserName);
		}

		protected override void SetUp()
		{
			base.SetUp();

			strategy = new GlowPasswordInstructionUrlStrategy();

			accessControlMock = new Mock<ITokenizedAccessControl>(MockBehavior.Strict);
			accessControlMock.Setup(x => x.TryCreate(It.IsAny<string>(), AccessTokenTypes.GlowPasswordReset, false, It.IsAny<DateTime>(), 1, It.IsAny<AccessTokenInfo>()))
				.Returns((string token, string type, bool isPermanent, DateTime? expiresAtUtc, int useCount, AccessTokenInfo info) =>
				{
					createdToken = token;
					tokenInfo = info;
					return true;
				});
			disposables.Add(ObjectFactory.Substitute(accessControlMock.Object));

			sourceMock = new Mock<IPasswordInstructionEmailSource>(MockBehavior.Strict);
			sourceMock.Setup(x => x.OrgCode).Returns("FOOBAR");
			sourceMock.Setup(x => x.Email).Returns("user@test.com");
			sourceMock.Setup(x => x.PK).Returns(userPk);

			disposables.Add(GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://www.glow.com/Portals/"));
		}

		protected override void TearDown()
		{
			base.TearDown();

			foreach (var item in disposables)
			{
				item.Dispose();
			}
			disposables.Clear();
		}

		GlowPasswordInstructionUrlStrategy strategy;
		Mock<ITokenizedAccessControl> accessControlMock;
		Mock<IPasswordInstructionEmailSource> sourceMock;
		readonly List<IDisposable> disposables = new List<IDisposable>();
		readonly Guid userPk = Guid.NewGuid();

		string createdToken;
		AccessTokenInfo tokenInfo;
	}
}
