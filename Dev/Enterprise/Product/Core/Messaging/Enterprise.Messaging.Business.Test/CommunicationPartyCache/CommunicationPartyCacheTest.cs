using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Messaging.Business.Testing
{
	class CommunicationPartyCacheTest : TestCaseWithFactory
	{
		public const string TestECAClientID = "Test Client ID";
		public const string TestECAAuthorizationEndpoint = "Test Endpoint";
		public const string TestECAUsername = "JarJarBinks";
		readonly string eAdaptorNextApplicationCode = eAdaptorNextApplicationDescriptor.ApplicationCode;

		public ICommunicationPartyConfigCache Cache => ObjectFactory.Get<ICommunicationPartyConfigCache>();

		public EDICommunicationParty CreateNewEDICommunicationParty(string direction, string authMode)
		{
			var ediCommunicationParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			ediCommunicationParty.ECP_Name = $"Test Party {Guid.NewGuid()}";
			ediCommunicationParty.ECP_ApplicationCode = eAdaptorNextApplicationCode;

			var ediCommunicationPartyConfig = direction == EDICommunicationPartyConfigDirectionsList.Codes.Inbound ?
				ediCommunicationParty.InboundConfig : ediCommunicationParty.OutboundConfig;

			ediCommunicationPartyConfig.Auth.ECA_ClientID = TestECAClientID;
			ediCommunicationPartyConfig.Auth.ECA_AuthorizationEndpoint = TestECAAuthorizationEndpoint;
			ediCommunicationPartyConfig.Auth.ECA_Username = TestECAUsername;
			ediCommunicationPartyConfig.Auth.ECA_AuthorizationMode = authMode;

			return ediCommunicationParty;
		}

		void UpdateDummyCommunicationPartyName(ZGuid communicationPartyConfigPk)
		{
			var dbReference = Factory.Load<EDICommunicationPartyConfig>(communicationPartyConfigPk);
			dbReference.Party.ECP_Name = "Updated Name";
			Factory.Save();
		}

		void DoWithEAdaptorNextEnabled(Action action)
		{
			var mockIFeatureData = new Mock<IFeatureData>();
			var mockIFeatureControlManager = new Mock<IFeatureControlManager>();
			mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.EAdaptorNextFeature, CancellationToken.None))
				.Returns(Task.FromResult(mockIFeatureData.Object));

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				action();
			}
		}

		public void TestCacheRetrievesOutboundCommunicationPartyConfig()
		{
			DoWithEAdaptorNextEnabled(() =>
			{
				var party = CreateNewEDICommunicationParty(EDICommunicationPartyConfigDirectionsList.Codes.Outbound, EDICommunicationAuthModesList.Codes.OAuthAuthentication);
				Factory.Save();
				AssertEquals(true, Cache.TryGetOutboundCommunicationPartyConfig(party.OutboundConfig.PK, out var storedCommunicationPartyConfig));
				AssertEquals(party.ECP_Name, storedCommunicationPartyConfig.Party.ECP_Name);
			});
		}

		public void TestCacheRetrievesInboundCommunicationPartyConfig()
		{
			DoWithEAdaptorNextEnabled(() =>
			{
				var party = CreateNewEDICommunicationParty(EDICommunicationPartyConfigDirectionsList.Codes.Inbound, EDICommunicationAuthModesList.Codes.OAuthAuthentication);
				Factory.Save();

				AssertEquals(true, Cache.TryGetInboundCommunicationPartyConfigByClientId(
					TestECAClientID,
					TestECAAuthorizationEndpoint,
					eAdaptorNextApplicationCode,
					out var storedCommunicationPartyConfig
				));
				AssertCommunicationPartyPropertiesAreEqual(party, storedCommunicationPartyConfig.Party);
			});
		}

		public void TestCacheRetrievesInboundCommunicationPartyConfigByUsername()
		{
			DoWithEAdaptorNextEnabled(() =>
			{
				var party = CreateNewEDICommunicationParty(EDICommunicationPartyConfigDirectionsList.Codes.Inbound, EDICommunicationAuthModesList.Codes.BasicAuthentication);
				Factory.Save();

				AssertEquals(true, Cache.TryGetInboundCommunicationPartyConfigByUsername(
					TestECAUsername,
					eAdaptorNextApplicationCode,
					out var storedCommunicationPartyConfig
				));
				AssertCommunicationPartyPropertiesAreEqual(party, storedCommunicationPartyConfig.Party);
			});
		}

		public void TestCacheReturnsNullWhenQueryRetrievesNothing()
		{
			var party = CreateNewEDICommunicationParty(EDICommunicationPartyConfigDirectionsList.Codes.Inbound, EDICommunicationAuthModesList.Codes.OAuthAuthentication);
			Factory.Save();

			AssertEquals(false, Cache.TryGetOutboundCommunicationPartyConfig(new ZGuid(), out var invalidOutboundCommunicationPartyConfig));
			AssertNull(invalidOutboundCommunicationPartyConfig);
			AssertEquals(false, Cache.TryGetInboundCommunicationPartyConfigByClientId(string.Empty, string.Empty, string.Empty, out var invalidInboundCommunicationPartyConfig));
			AssertNull(invalidInboundCommunicationPartyConfig);
		}

		public void TestCacheDoesNotRequery()
		{
			DoWithEAdaptorNextEnabled(() =>
			{
				var party = CreateNewEDICommunicationParty(EDICommunicationPartyConfigDirectionsList.Codes.Outbound, EDICommunicationAuthModesList.Codes.OAuthAuthentication);
				Factory.Save();
				var mockCache = new Mock<CommunicationPartyConfigCache>() { CallBase = true };
				mockCache.Setup(c => c.GetExpirationDate()).Returns(DateTime.MaxValue);
				mockCache.Object.TryGetOutboundCommunicationPartyConfig(party.OutboundConfig.PK, out var originalCommmunicationPartyConfig);
				UpdateDummyCommunicationPartyName(party.OutboundConfig.PK);
				mockCache.Object.TryGetOutboundCommunicationPartyConfig(party.OutboundConfig.PK, out var requeriedlCommmunicationPartyConfig);
				AssertEquals(originalCommmunicationPartyConfig.Party.ECP_Name, requeriedlCommmunicationPartyConfig.Party.ECP_Name);
			});
		}

		public void TestCacheDoesRequery()
		{
			DoWithEAdaptorNextEnabled(() =>
			{
				var party = CreateNewEDICommunicationParty(EDICommunicationPartyConfigDirectionsList.Codes.Outbound, EDICommunicationAuthModesList.Codes.OAuthAuthentication);
				Factory.Save();
				var mockCache = new Mock<CommunicationPartyConfigCache>() { CallBase = true };
				mockCache.Setup(c => c.GetExpirationDate()).Returns(DateTimeOffset.Now.AddDays(-1));
				mockCache.Object.TryGetOutboundCommunicationPartyConfig(party.OutboundConfig.PK, out var originalCommmunicationPartyConfig);
				UpdateDummyCommunicationPartyName(party.OutboundConfig.PK);
				mockCache.Object.TryGetOutboundCommunicationPartyConfig(party.OutboundConfig.PK, out var requeriedlCommmunicationPartyConfig);
				AssertNotEquals(originalCommmunicationPartyConfig.Party.ECP_Name, requeriedlCommmunicationPartyConfig.Party.ECP_Name);
			});
		}

		public void TestOutboundCacheReturnsRequestedConfig()
		{
			DoWithEAdaptorNextEnabled(() =>
			{
				var party1 = CreateNewEDICommunicationParty(EDICommunicationPartyConfigDirectionsList.Codes.Outbound, EDICommunicationAuthModesList.Codes.OAuthAuthentication);
				var party2 = CreateNewEDICommunicationParty(EDICommunicationPartyConfigDirectionsList.Codes.Outbound, EDICommunicationAuthModesList.Codes.OAuthAuthentication);

				Factory.Save();

				AssertEquals(true, Cache.TryGetOutboundCommunicationPartyConfig(party1.OutboundConfig.PK, out var originalCommmunicationPartyConfig1));
				AssertEquals(party1.OutboundConfig.PK, originalCommmunicationPartyConfig1.PK);

				AssertEquals(true, Cache.TryGetOutboundCommunicationPartyConfig(party2.OutboundConfig.PK, out var originalCommmunicationPartyConfig2));
				AssertEquals(party2.OutboundConfig.PK, originalCommmunicationPartyConfig2.PK);
			});
		}

		public void TestCacheRetrievesInActiveCommunicationPartyConfig()
		{
			var partyIn = CreateNewEDICommunicationParty(EDICommunicationPartyConfigDirectionsList.Codes.Inbound, EDICommunicationAuthModesList.Codes.OAuthAuthentication);
			var partyOut = CreateNewEDICommunicationParty(EDICommunicationPartyConfigDirectionsList.Codes.Outbound, EDICommunicationAuthModesList.Codes.OAuthAuthentication);
			partyIn.ECP_IsActive = false;
			partyOut.ECP_IsActive = false;
			Factory.Save();

			AssertEquals(expected: false, Cache.TryGetInboundCommunicationPartyConfigByUsername(
				TestECAUsername,
				eAdaptorNextApplicationCode,
				out var storedCommunicationPartyConfig
			));
			AssertNull(storedCommunicationPartyConfig);
			AssertEquals(expected: false, Cache.TryGetInboundCommunicationPartyConfigByClientId(
				TestECAClientID,
				TestECAAuthorizationEndpoint,
				eAdaptorNextApplicationCode,
				out var storedCommunicationPartyConfig2
			));
			AssertNull(storedCommunicationPartyConfig);
			AssertEquals(expected: false, Cache.TryGetOutboundCommunicationPartyConfig(partyOut.OutboundConfig.PK, out var storedCommunicationPartyConfigOut));
			AssertNull(storedCommunicationPartyConfigOut);
		}

		public void TestCacheRetrievesInActiveCommunicationPartyConfigInbound()
		{
			var party = CreateNewEDICommunicationParty(EDICommunicationPartyConfigDirectionsList.Codes.Inbound, EDICommunicationAuthModesList.Codes.OAuthAuthentication);
			party.InboundConfig.ECC_IsActive = false;
			Factory.Save();

			AssertEquals(expected: false, Cache.TryGetInboundCommunicationPartyConfigByUsername(
				TestECAUsername,
				eAdaptorNextApplicationCode,
				out var storedCommunicationPartyConfig
			));
			AssertNull(storedCommunicationPartyConfig);
			AssertEquals(expected: false, Cache.TryGetInboundCommunicationPartyConfigByClientId(
				TestECAClientID,
				TestECAAuthorizationEndpoint,
				eAdaptorNextApplicationCode,
				out var storedCommunicationPartyConfig2
			));
			AssertNull(storedCommunicationPartyConfig2);
		}

		public void TestCacheRetrievesInActiveCommunicationPartyConfigOutbound()
		{
			var party = CreateNewEDICommunicationParty(EDICommunicationPartyConfigDirectionsList.Codes.Outbound, EDICommunicationAuthModesList.Codes.OAuthAuthentication);
			party.OutboundConfig.ECC_IsActive = false;
			Factory.Save();

			AssertEquals(expected: false, Cache.TryGetOutboundCommunicationPartyConfig(party.OutboundConfig.PK, out var storedCommunicationPartyConfigOut));
			AssertNull(storedCommunicationPartyConfigOut);
		}

		public void TestTryGetInboundCommunicationPartyConfigByClientIdHandlesForwardingSlash()
		{
			DoWithEAdaptorNextEnabled(() =>
			{
				var party = CreateNewEDICommunicationParty(EDICommunicationPartyConfigDirectionsList.Codes.Inbound, EDICommunicationAuthModesList.Codes.OAuthAuthentication);
				Factory.Save();

				AssertEquals(true, Cache.TryGetInboundCommunicationPartyConfigByClientId(
					TestECAClientID,
					TestECAAuthorizationEndpoint,
					eAdaptorNextApplicationCode,
					out var storedCommunicationPartyConfig1
				));

				AssertEquals(true, Cache.TryGetInboundCommunicationPartyConfigByClientId(
					TestECAClientID,
					$"{TestECAAuthorizationEndpoint}/",
					eAdaptorNextApplicationCode,
					out var storedCommunicationPartyConfig2
				));

				party.InboundConfig.Auth.ECA_AuthorizationEndpoint += '/';
				Factory.Save();

				AssertEquals(true, Cache.TryGetInboundCommunicationPartyConfigByClientId(
					TestECAClientID,
					TestECAAuthorizationEndpoint,
					eAdaptorNextApplicationCode,
					out var storedCommunicationPartyConfig3
				));
			});
		}

		public void TestCacheWhenEAdaptorNextFeatureIsDisabled()
		{
			var party = CreateNewEDICommunicationParty(EDICommunicationPartyConfigDirectionsList.Codes.Outbound, EDICommunicationAuthModesList.Codes.OAuthAuthentication);
			CreateNewEDICommunicationParty(EDICommunicationPartyConfigDirectionsList.Codes.Inbound, EDICommunicationAuthModesList.Codes.OAuthAuthentication);
			CreateNewEDICommunicationParty(EDICommunicationPartyConfigDirectionsList.Codes.Inbound, EDICommunicationAuthModesList.Codes.BasicAuthentication);
			Factory.Save();

			Assert(!Cache.TryGetOutboundCommunicationPartyConfig(party.OutboundConfig.PK, out var storedCommunicationPartyConfig1));
			AssertNull(storedCommunicationPartyConfig1);

			Assert(!Cache.TryGetInboundCommunicationPartyConfigByClientId(
				TestECAClientID,
				TestECAAuthorizationEndpoint,
				eAdaptorNextApplicationCode,
				out var storedCommunicationPartyConfig2
			));
			AssertNull(storedCommunicationPartyConfig2);

			Assert(!Cache.TryGetInboundCommunicationPartyConfigByUsername(
				TestECAUsername,
				eAdaptorNextApplicationCode,
				out var storedCommunicationPartyConfig3
			));
			AssertNull(storedCommunicationPartyConfig3);
		}

		void AssertCommunicationPartyPropertiesAreEqual(IEDICommunicationParty expected, IEDICommunicationParty actual)
		{
			AssertEquals(expected.PK, actual.PK);
			AssertEquals(expected.ECP_Name, actual.ECP_Name);
			AssertEquals(expected.ECP_GS_SecurityProxy, actual.ECP_GS_SecurityProxy);
			AssertEquals(expected.ECP_ApplicationCode, actual.ECP_ApplicationCode);
			AssertEquals(expected.ECP_Summary, actual.ECP_Summary);
			AssertEquals(expected.ECP_IsActive, actual.ECP_IsActive);
		}
	}
}
