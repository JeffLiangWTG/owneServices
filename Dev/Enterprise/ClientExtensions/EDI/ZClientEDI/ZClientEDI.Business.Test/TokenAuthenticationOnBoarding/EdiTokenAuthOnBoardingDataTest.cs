using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IdentityTenant.Business;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business.Testing
{
	[TestedType(typeof(EdiTokenAuthOnBoardingData))]
	public class EdiTokenAuthOnBoardingDataTest : EnterpriseBusinessObjectTestCase
	{
		readonly List<string> SignificantFields = new List<string>
		{
			nameof(EdiTokenAuthOnBoardingData.TOD_ConfigurationIdentifier),
			nameof(EdiTokenAuthOnBoardingData.TOD_OIDCServer),
			nameof(EdiTokenAuthOnBoardingData.TOD_SystemUniqueIdentifier),
			nameof(EdiTokenAuthOnBoardingData.TOD_IDT)
	};

		public void TestDefaultValues()
		{
			var ediTokenAuthOnBoardingData = Factory.New<EdiTokenAuthOnBoardingData>();
			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, ediTokenAuthOnBoardingData.TOD_ClaimMappingIdentifier);
				AssertEquals(ZString.Empty, ediTokenAuthOnBoardingData.TOD_ClaimMappingName);
				AssertEquals("Azure", ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier);
				AssertEquals(ZGuid.Empty, ediTokenAuthOnBoardingData.TOD_IM);
				AssertEquals(ZGuid.Empty, ediTokenAuthOnBoardingData.TOD_LE);
				AssertEquals("AZU", ediTokenAuthOnBoardingData.TOD_OIDCServer);
				AssertEquals(0, ediTokenAuthOnBoardingData.TOD_Retry);
				AssertEquals("NEW", ediTokenAuthOnBoardingData.TOD_Status);
				AssertEquals(ZString.Empty, ediTokenAuthOnBoardingData.TOD_SystemUniqueIdentifier);
				AssertEquals(ZString.Empty, ediTokenAuthOnBoardingData.TOD_VerificationUsername);
				AssertEquals(ZString.Empty, ediTokenAuthOnBoardingData.TOD_VerificationUserPassword);
				AssertEquals(false, ediTokenAuthOnBoardingData.TOD_WinzorOnly);
			});
		}

		void TestConfigurationIdentifierNoOverride(EdiTokenAuthOnBoardingData ediTokenAuthOnBoardingData, string expectedValue)
		{
			AssertEquals(expectedValue, ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier);
			ediTokenAuthOnBoardingData.TOD_OIDCServer = "GEN";
			AssertEquals(expectedValue, ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier);
			ediTokenAuthOnBoardingData.TOD_OIDCServer = "AZU";
			AssertEquals(expectedValue, ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier);
			ediTokenAuthOnBoardingData.TOD_OIDCServer = "OKT";
			AssertEquals(expectedValue, ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier);
			ediTokenAuthOnBoardingData.TOD_OIDCServer = "AZU";
			AssertEquals(expectedValue, ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier);
		}

		public void TestConfigurationIdentifierFromOidcServerNoOverrideWhenDefault() // override logic is done in the form and not in the BizO
		{
			const string expectedValue = "Azure";
			var ediTokenAuthOnBoardingData = Factory.New<EdiTokenAuthOnBoardingData>();
			TestConfigurationIdentifierNoOverride(ediTokenAuthOnBoardingData, expectedValue);
		}

		public void TestConfigurationIdentifierFromOidcServerNoOverrideWhenSet() // override logic is done in the form and not in the BizO
		{
			const string someValue = nameof(someValue);
			var ediTokenAuthOnBoardingData = Factory.New<EdiTokenAuthOnBoardingData>();
			ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier = someValue;
			TestConfigurationIdentifierNoOverride(ediTokenAuthOnBoardingData, someValue);
		}

		public void TestConfigurationIdentifierFromOidcServerNoOverrideWhenBlank() // override logic is done in the form and not in the BizO
		{
			var ediTokenAuthOnBoardingData = Factory.New<EdiTokenAuthOnBoardingData>();
			string expectedValue = string.Empty;
			ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier = expectedValue;
			TestConfigurationIdentifierNoOverride(ediTokenAuthOnBoardingData, expectedValue);
		}

		public void TestSaveOnboardingDataWithDuplicateEnterpriseCode()
		{
			var licenceEnterprise1 = Factory.NewWithValidTestData<LicenceEnterprise>();
			var incidentWithLicense1 = Factory.NewWithValidTestData<SupportIncident>();
			var incidentWithLicense2 = Factory.NewWithValidTestData<SupportIncident>();
			var ediTokenAuthOnBoardingData1 = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			var ediTokenAuthOnBoardingData2 = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			ediTokenAuthOnBoardingData1.TOD_IM = incidentWithLicense1.PK;
			ediTokenAuthOnBoardingData1.TOD_LE = licenceEnterprise1.PK;
			Factory.Save();
			ediTokenAuthOnBoardingData2.TOD_IM = incidentWithLicense2.PK;
			ediTokenAuthOnBoardingData2.TOD_LE = licenceEnterprise1.PK;
			var errorMessage = AssertExceptionThrown<ZSaveException>("Factory save should fail because of the duplication check in trigger", () => Factory.Save()).Message;
			AssertContains($"Cannot insert duplicate key row in object 'dbo.EdiTokenAuthOnBoardingData' with unique index 'FK_UC__TOD_LE'. The duplicate key value is ({licenceEnterprise1.PK}).", errorMessage);
		}

		public void TestHumanReadable()
		{
			var ediTokenAuthOnBoardingData = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			// sanity check
			AssertNotNull(ediTokenAuthOnBoardingData.Incident);
			AssertNotNullOrEmpty(ediTokenAuthOnBoardingData.Incident.IM_IncidentNumber);
			// check with the randomly populated test data
			AssertEquals("Token Authentication Onboarding Data", ediTokenAuthOnBoardingData.HumanReadableName);
		}

		public void TestHumanReadableShortcutName()
		{
			var ediTokenAuthOnBoardingData = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			// sanity check
			AssertNotNull(ediTokenAuthOnBoardingData.Incident);
			AssertNotNullOrEmpty(ediTokenAuthOnBoardingData.Incident.IM_IncidentNumber);
			// check with the randomly populated test data
			CombineAssertions(() =>
			{
				var humanReadableShortcutName = ediTokenAuthOnBoardingData.HumanReadableShortcutName;
				AssertStartsWith($"{nameof(ediTokenAuthOnBoardingData.HumanReadableShortcutName)} should start with a readable name", "Token Authentication Onboarding Data - ", humanReadableShortcutName);
				AssertEndsWith("{nameof(ediTokenAuthOnBoardingData.HumanReadableShortcutName)} should end with space", " ", humanReadableShortcutName);
				AssertContains(ediTokenAuthOnBoardingData.Incident.IM_IncidentNumber, humanReadableShortcutName);
			});
			// check with fixed data to match the full string
			ediTokenAuthOnBoardingData.Incident.IM_IncidentNumber = "123456";
			AssertEquals("Token Authentication Onboarding Data - 123456 ", ediTokenAuthOnBoardingData.HumanReadableShortcutName);
		}

		public void TestVerificationResultFromVerificationResultDetails()
		{
			var ediTokenAuthOnBoardingData = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			AssertEquals("Not Verified", ediTokenAuthOnBoardingData.VerificationResult);
			ediTokenAuthOnBoardingData.VerificationResultDetails = "Something";
			AssertEquals("Failed", ediTokenAuthOnBoardingData.VerificationResult);
			ediTokenAuthOnBoardingData.VerificationResultDetails = null;
			AssertEquals("Success", ediTokenAuthOnBoardingData.VerificationResult);
		}

		public void TestVerificationResultChanges()
		{
			var verificationResultChanges = new List<EventArgs>();
			var verificationResultDetailsChanges = new List<EventArgs>();
			var ediTokenAuthOnBoardingData = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			ediTokenAuthOnBoardingData.VerificationResultInfo.ValueChanged += (sender, e) => { verificationResultChanges.Add(e); };
			ediTokenAuthOnBoardingData.VerificationResultDetailsInfo.ValueChanged += (sender, e) => { verificationResultDetailsChanges.Add(e); };
			ediTokenAuthOnBoardingData.VerificationResultDetails = "Something";
			ediTokenAuthOnBoardingData.VerificationResultDetails = null;
			AssertEquals(2, verificationResultDetailsChanges.Count);
			AssertEquals(2, verificationResultChanges.Count);
		}

		public void TestNewFieldsReadOnly()
		{
			var ediTokenAuthOnBoardingData = Factory.New<EdiTokenAuthOnBoardingData>();
			var readonlyProperties = RetrieveAllReadOnlyProperties(ediTokenAuthOnBoardingData);
			AssertCollectionContains(nameof(ediTokenAuthOnBoardingData.VerificationResult), readonlyProperties);
			AssertCollectionNotContains(nameof(ediTokenAuthOnBoardingData.VerificationResultDetails), readonlyProperties);
			AssertEquals(false, ediTokenAuthOnBoardingData.VerificationResultInfo.HasSetter);
			AssertEquals(true, ediTokenAuthOnBoardingData.VerificationResultDetailsInfo.HasSetter);
		}

		public void TestReadOnlyFieldsByStatus()
		{
			var ediTokenAuthOnBoardingData = Factory.New<EdiTokenAuthOnBoardingData>();
			var alwaysReadOnlyProperties = new[]
			{
				nameof(ediTokenAuthOnBoardingData.TOD_Status),
				nameof(ediTokenAuthOnBoardingData.VerificationResult),
				nameof(ediTokenAuthOnBoardingData.TOD_LE)
			};
			foreach (var status in new OnBoardingStatuses().GetAllCodes())
			{
				ediTokenAuthOnBoardingData.TOD_Status = status;
				var expectedProperties = status == OnBoardingStatuses.Codes.New || status == OnBoardingStatuses.Codes.Error
					? alwaysReadOnlyProperties
					: alwaysReadOnlyProperties.Concat(SignificantFields);

				var readonlyProperties = RetrieveAllReadOnlyProperties(ediTokenAuthOnBoardingData);
				AssertContainsExactElementsInAnyOrder($"Error for status {status}", expectedProperties, readonlyProperties);
			}
		}

		public void TestSignificantFieldsShouldResetVerificationResult()
		{
			var dataSource = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			Factory.Save();
			TestSignificantFieldsShouldResetVerificationResult(ediTokenAuthOnBoardingData: dataSource, action: o => o.TOD_ClaimMappingIdentifier = "123", expectReset: true);
			TestSignificantFieldsShouldResetVerificationResult(ediTokenAuthOnBoardingData: dataSource, action: o => o.TOD_ClaimMappingName = "123", expectReset: true);
			TestSignificantFieldsShouldResetVerificationResult(ediTokenAuthOnBoardingData: dataSource, action: o => o.TOD_ConfigurationIdentifier = "123", expectReset: true);
			TestSignificantFieldsShouldResetVerificationResult(ediTokenAuthOnBoardingData: dataSource, action: o => o.TOD_IM = ZGuid.BrettsGuid, expectReset: false);
			TestSignificantFieldsShouldResetVerificationResult(ediTokenAuthOnBoardingData: dataSource, action: o => o.TOD_LE = ZGuid.BrettsGuid, expectReset: false);
			TestSignificantFieldsShouldResetVerificationResult(ediTokenAuthOnBoardingData: dataSource, action: o => o.TOD_OIDCServer = "123", expectReset: true);
			TestSignificantFieldsShouldResetVerificationResult(ediTokenAuthOnBoardingData: dataSource, action: o => o.TOD_Retry = 123, expectReset: false);
			TestSignificantFieldsShouldResetVerificationResult(ediTokenAuthOnBoardingData: dataSource, action: o => o.TOD_Status = OnBoardingStatuses.Codes.Error, expectReset: false);
			TestSignificantFieldsShouldResetVerificationResult(ediTokenAuthOnBoardingData: dataSource, action: o => o.TOD_SystemUniqueIdentifier = "123", expectReset: true);
			TestSignificantFieldsShouldResetVerificationResult(ediTokenAuthOnBoardingData: dataSource, action: o => o.TOD_VerificationUsername = "123", expectReset: false);
			TestSignificantFieldsShouldResetVerificationResult(ediTokenAuthOnBoardingData: dataSource, action: o => o.TOD_VerificationUserPassword = "123", expectReset: false);
		}

		void TestSignificantFieldsShouldResetVerificationResult(EdiTokenAuthOnBoardingData ediTokenAuthOnBoardingData, Action<EdiTokenAuthOnBoardingData> action, bool expectReset)
		{
			TestSignificantFieldsShouldResetVerificationResult(ediTokenAuthOnBoardingData, action, previousResultSuccess: true, expectReset);
			TestSignificantFieldsShouldResetVerificationResult(ediTokenAuthOnBoardingData, action, previousResultSuccess: false, expectReset);
		}

		static void TestSignificantFieldsShouldResetVerificationResult(
			EdiTokenAuthOnBoardingData ediTokenAuthOnBoardingData, Action<EdiTokenAuthOnBoardingData> action, bool previousResultSuccess, bool expectReset)
		{
			ediTokenAuthOnBoardingData.Reload();
			ediTokenAuthOnBoardingData.VerificationResultDetails = previousResultSuccess ? null : "something";
			var previousResult = previousResultSuccess
				? EdiTokenAuthOnBoardingDataLookups.VerificationResult.Success
				: EdiTokenAuthOnBoardingDataLookups.VerificationResult.Failed;
			AssertEquals(previousResult, ediTokenAuthOnBoardingData.VerificationResult);
			action(ediTokenAuthOnBoardingData);
			var expectedValue = expectReset
				? EdiTokenAuthOnBoardingDataLookups.VerificationResult.NotVerified
				: previousResult;
			AssertEquals(expectedValue, ediTokenAuthOnBoardingData.VerificationResult);
		}

		IEnumerable<string> RetrieveAllReadOnlyProperties(EdiTokenAuthOnBoardingData ediTokenAuthOnBoardingData)
		{
			return ediTokenAuthOnBoardingData.ZPropertyInfoHash
				.OfType<ZPropertyInfo>()
				.Where(p => p.ReadOnly)
				.Select(p => p.Name);
		}

		public void TestLogStatusChangesOnSaving()
		{
			var ediTokenAuthOnBoardingData = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			ediTokenAuthOnBoardingData.TOD_Status = OnBoardingStatuses.Codes.Queued;
			// Assert no message before saving
			var expectedLogs = new List<string>();
			AssertContainsExactElementsInAnyOrder(expectedLogs, RetrieveLogs(ediTokenAuthOnBoardingData));
			Factory.Save();
			// We expect a new Added log without specific reference
			expectedLogs.Add("ADD - ");
			AssertContainsExactElementsInAnyOrder(expectedLogs, RetrieveLogs(ediTokenAuthOnBoardingData));
			// Update the object without changing the status
			ediTokenAuthOnBoardingData.TOD_Status = OnBoardingStatuses.Codes.Queued;
			ediTokenAuthOnBoardingData.TOD_SystemUniqueIdentifier = Guid.NewGuid().ToString();
			Factory.Save();
			// We expect a new Edit log without specific reference
			expectedLogs.Add("EDT - ");
			AssertContainsExactElementsInAnyOrder(expectedLogs, RetrieveLogs(ediTokenAuthOnBoardingData));
			// Update the object by changing the status
			ediTokenAuthOnBoardingData.TOD_Status = OnBoardingStatuses.Codes.StagingPullRequest;
			ediTokenAuthOnBoardingData.TOD_SystemUniqueIdentifier = Guid.NewGuid().ToString();
			Factory.Save();
			// We expect a new Edit log with a specific reference
			expectedLogs.Add("EDT - Changed Status from QUE to SPR.");
			AssertContainsExactElementsInAnyOrder(expectedLogs, RetrieveLogs(ediTokenAuthOnBoardingData));
			// Update the object with an invalid status
			ediTokenAuthOnBoardingData.TOD_Status = "123";
			// We expect this to fail and no new logs
			AssertExceptionThrown<ZSaveException>(() => Factory.Save());
			AssertContainsExactElementsInAnyOrder(expectedLogs, RetrieveLogs(ediTokenAuthOnBoardingData));
			// Update the object back with a valid status
			ediTokenAuthOnBoardingData.TOD_Status = OnBoardingStatuses.Codes.Error;
			Factory.Save();
			// We expect a new Edit log with a specific reference
			expectedLogs.Add("EDT - Changed Status from SPR to ERR.");
			AssertContainsExactElementsInAnyOrder(expectedLogs, RetrieveLogs(ediTokenAuthOnBoardingData));
			// Update the object with an invalid state and a different status
			var licensePk = ediTokenAuthOnBoardingData.TOD_LE;
			ediTokenAuthOnBoardingData.TOD_Status = OnBoardingStatuses.Codes.Verified;
			ediTokenAuthOnBoardingData.TOD_LE = ZGuid.Empty;
			// We expect this to fail and no new logs
			AssertExceptionThrown<ZSaveException>(() => Factory.Save());
			AssertContainsExactElementsInAnyOrder(expectedLogs, RetrieveLogs(ediTokenAuthOnBoardingData));
			// set the state to valid, without changing the status
			ediTokenAuthOnBoardingData.TOD_LE = licensePk;
			Factory.Save();
			// We expect a new Edit log with a specific reference
			expectedLogs.Add("EDT - Changed Status from ERR to VER.");
			AssertContainsExactElementsInAnyOrder(expectedLogs, RetrieveLogs(ediTokenAuthOnBoardingData));
			// Update the object with a new value then reverting it before saving
			ediTokenAuthOnBoardingData.TOD_Status = OnBoardingStatuses.Codes.StagingPullRequest;
			ediTokenAuthOnBoardingData.TOD_SystemUniqueIdentifier = Guid.NewGuid().ToString();
			ediTokenAuthOnBoardingData.TOD_Status = OnBoardingStatuses.Codes.Verified;
			Factory.Save();
			// We expect a new Edit log without specific reference
			expectedLogs.Add("EDT - ");
		}

		IEnumerable<string> RetrieveLogs(EdiTokenAuthOnBoardingData ediTokenAuthOnBoardingData)
		{
			return ediTokenAuthOnBoardingData.Logs.GetAllLogs().Select(x => $"{x.SL_SE_NKEvent} - {x.SL_Reference}");
		}

		public void TestTryGetOidcConfigReturnsConfig()
		{
			Assertion(OIDCServerTypesList.Codes.Azure, AzureB2CEnvironmentCodeDescriptionList.Codes.PRD);
			Assertion(OIDCServerTypesList.Codes.Azure, AzureB2CEnvironmentCodeDescriptionList.Codes.STG);
			Assertion(OIDCServerTypesList.Codes.Okta, AzureB2CEnvironmentCodeDescriptionList.Codes.PRD);
			Assertion(OIDCServerTypesList.Codes.Okta, AzureB2CEnvironmentCodeDescriptionList.Codes.STG);
			Assertion(OIDCServerTypesList.Codes.Generic, AzureB2CEnvironmentCodeDescriptionList.Codes.PRD);
			Assertion(OIDCServerTypesList.Codes.Generic, AzureB2CEnvironmentCodeDescriptionList.Codes.STG);
			Assertion(OIDCServerTypesList.Codes.OneLogin, AzureB2CEnvironmentCodeDescriptionList.Codes.PRD);
			Assertion(OIDCServerTypesList.Codes.OneLogin, AzureB2CEnvironmentCodeDescriptionList.Codes.STG);
		}

		void Assertion(string oidcServer, string code)
		{
			var tenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			tenant.IDT_TenantId = ZGuid.NewZGuid().ToString();
			tenant.IDT_AuthorityUrl = "https://www.example2.com";
			tenant.IDT_GraphClientId = ZGuid.NewZGuid().ToString();
			tenant.IDT_Name = "Test2";
			tenant.IDT_Onboarding = true;
			var ediTokenAuthOnBoardingData = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			ediTokenAuthOnBoardingData.TOD_ClaimMappingIdentifier = OIDCClaimMappingIdentifiers.Codes.LoginName;
			ediTokenAuthOnBoardingData.TOD_ClaimMappingName = "user_name";
			ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier = "Azure";
			ediTokenAuthOnBoardingData.TOD_OIDCServer = oidcServer;
			ediTokenAuthOnBoardingData.TOD_SystemUniqueIdentifier = Guid.NewGuid().ToString();
			ediTokenAuthOnBoardingData.TOD_Retry = 5;
			ediTokenAuthOnBoardingData.TOD_Status = "QUE";
			ediTokenAuthOnBoardingData.TOD_VerificationUsername = "user";
			ediTokenAuthOnBoardingData.TOD_VerificationUserPassword = "password";
			ediTokenAuthOnBoardingData.TOD_IDT = tenant.PK;
			using (EDIDataRegistry.Instance.AzureOpenIDConnectConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeCollection()))
			{
				ediTokenAuthOnBoardingData.Environment = code;
				bool result = ediTokenAuthOnBoardingData.TryGetOidcConfig(out var oidcConfig, out var domainHint);
				CombineAssertions(() =>
				{
					AssertEquals(true, result);
					AssertNotNull(oidcConfig);
					AssertNotNull(domainHint);
				});
				CombineAssertions(() =>
				{
					AssertEquals(1, oidcConfig.ClaimsMappings.Count);
					var claimsMappings = oidcConfig.ClaimsMappings.OfType<OIDCClaimsMapping>().ToList();
					AssertContainsExactElementsInAnyOrder(claimsMappings.Select(cm => cm.ClaimName), new[] { ediTokenAuthOnBoardingData.TOD_ClaimMappingName });
					AssertContainsExactElementsInAnyOrder(claimsMappings.Select(cm => cm.Identifier), new[] { ediTokenAuthOnBoardingData.TOD_ClaimMappingIdentifier });
					AssertEquals(true, oidcConfig.IsOIDCEnabled);
					AssertEquals(false, oidcConfig.IsVerified);
					AssertEquals("As to the federated OIDC config, type should always be Azure", OIDCServerTypesList.Codes.Azure, oidcConfig.OIDCServerTypeCode);
					AssertEquals(1, oidcConfig.Scopes.Count);
					if (code == AzureB2CEnvironmentCodeDescriptionList.Codes.PRD)
					{
						AssertEquals(tenant.IDT_OidcClientId, oidcConfig.ClientIdentifier);
						AssertEquals(tenant.IDT_AuthorityUrl, oidcConfig.AuthorityURL);
						AssertContainsExactElementsInAnyOrder(oidcConfig.Scopes.OfType<OIDCScope>().Select(s => s.ScopeName), new List<string> { tenant.IDT_OidcClientId });
					}
					else
					{
						AssertEquals(ClientId, oidcConfig.ClientIdentifier);
						AssertEquals(AuthorityUrl, oidcConfig.AuthorityURL);
						AssertContainsExactElementsInAnyOrder(oidcConfig.Scopes.OfType<OIDCScope>().Select(s => s.ScopeName), new List<string> { ClientId });
					}
					AssertEquals(ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier, domainHint);
				});
			}
		}

		public void TestTryGetOidcConfigCheckMandatoryFields()
		{
			SignificantFields.Add(nameof(EdiTokenAuthOnBoardingData.TOD_ClaimMappingIdentifier));
			SignificantFields.Add(nameof(EdiTokenAuthOnBoardingData.TOD_ClaimMappingName));
			var ediTokenAuthOnBoardingData = Factory.New<EdiTokenAuthOnBoardingData>();
			// we need to explicitly set to an invalid value as it is initialized by default
			ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier = null;
			ediTokenAuthOnBoardingData.TOD_OIDCServer = "123";
			ediTokenAuthOnBoardingData.TOD_Status = null;
			using (EDIDataRegistry.Instance.AzureOpenIDConnectConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeCollection()))
			{
				ediTokenAuthOnBoardingData.Environment = Code;
				bool result = ediTokenAuthOnBoardingData.TryGetOidcConfig(out var oidcConfig, out var domainHint);
				CombineAssertions(() =>
				{
					AssertEquals(false, result);
					AssertNull(oidcConfig);
					AssertNull(domainHint);
				});
				var checkedProperties = ediTokenAuthOnBoardingData.PropertiesWithNotifications.Select(p => p.Name);
				AssertContainsExactElementsInAnyOrder(SignificantFields, checkedProperties);
			}
		}

		public void TestLicenceEnterpriseCode()
		{
			var enterprise = Factory.New<LicenceEnterprise>();
			var ediTokenAuthOnBoardingData = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			ediTokenAuthOnBoardingData.TOD_LE = enterprise.PK;
			AssertNullOrEmpty(ediTokenAuthOnBoardingData.LicenceEnterpriseCode);
			enterprise.LE_EnterpriseCode = "TST";
			AssertEquals(ediTokenAuthOnBoardingData.LicenceEnterpriseCode, enterprise.LE_EnterpriseCode);
		}

		AzureOpenIDConnectConfigurationCollection InitializeCollection()
		{
			var collection = new AzureOpenIDConnectConfigurationCollection();
			var azureApplicationManagement = collection.AddNew();
			azureApplicationManagement.Code = Code;
			azureApplicationManagement.AuthorityUrl = AuthorityUrl;
			azureApplicationManagement.ClientID = ClientId;
			return collection;
		}

		const string Code = "STG";

		const string AuthorityUrl = "https://www.example.com";

		const string ClientId = "46546646-e627-46fb-afe4-e5ea9928740e";
	}
}
