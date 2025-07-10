using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IdentityTenant.Business;
using Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Module.Testing
{
	[TestedType(typeof(TokenAuthenticationOnBoardingFilterBusinessObject))]
	public class TokenAuthenticationOnBoardingFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new TokenAuthenticationOnBoardingFilterBusinessObject();
		}

		#region TextFilters

		public void TestFilterByClaimMappingName()
		{
			var filterValue = "username";
			AddRow();
			var tokenAuthOnBoardingData2 = AddRow(claimMappingNameOverride: filterValue);
			AddRow(claimMappingNameOverride: "userId");
			var tokenAuthOnBoardingData4 = AddRow(claimMappingNameOverride: filterValue);

			var filteredCollection = Filter("Claim Mapping Name", filterValue);

			AssertContainsExactElementsInAnyOrder(new[] { tokenAuthOnBoardingData2, tokenAuthOnBoardingData4 }, filteredCollection);
		}

		public void TestFilterByConfigurationIdentifier()
		{
			var filterValue = "PRD";
			AddRow();
			var tokenAuthOnBoardingData2 = AddRow(configurationIdentifierOverride: filterValue);
			AddRow(configurationIdentifierOverride: "TST");
			var tokenAuthOnBoardingData4 = AddRow(configurationIdentifierOverride: filterValue);

			var filteredCollection = Filter("Configuration Identifier", filterValue);

			AssertContainsExactElementsInAnyOrder(new[] { tokenAuthOnBoardingData2, tokenAuthOnBoardingData4 }, filteredCollection);
		}

		public void TestFilterByUniqueIdentifier()
		{
			var filterValue = Guid.NewGuid().ToString();
			AddRow();
			var tokenAuthOnBoardingData2 = AddRow(systemUniqueIdentifierOverride: filterValue);
			AddRow(systemUniqueIdentifierOverride: Guid.NewGuid().ToString());
			var tokenAuthOnBoardingData4 = AddRow(systemUniqueIdentifierOverride: filterValue);

			var filteredCollection = Filter("System Unique Identifier", filterValue);
			AssertContainsExactElementsInAnyOrder(new[] { tokenAuthOnBoardingData2, tokenAuthOnBoardingData4 }, filteredCollection);
		}

		public void TestFilterByVerificationUsername()
		{
			var filterValue = Guid.NewGuid().ToString();
			AddRow();
			var tokenAuthOnBoardingData2 = AddRow(verificationUsernameOverride: filterValue);
			AddRow(verificationUsernameOverride: Guid.NewGuid().ToString());
			var tokenAuthOnBoardingData4 = AddRow(verificationUsernameOverride: filterValue);

			var filteredCollection = Filter("Verification Username", filterValue);
			AssertContainsExactElementsInAnyOrder(new[] { tokenAuthOnBoardingData2, tokenAuthOnBoardingData4 }, filteredCollection);
		}
		#endregion

		#region int filter
		public void TestFilterByRetry()
		{
			var filterValue = 2;
			AddRow();
			var tokenAuthOnBoardingData2 = AddRow(retryOverride: filterValue);
			AddRow(retryOverride: filterValue - 1);
			var tokenAuthOnBoardingData4 = AddRow(retryOverride: filterValue);
			AddRow(retryOverride: filterValue + 1);

			var filteredCollection = Filter("Retry", filterValue);

			AssertContainsExactElementsInAnyOrder(new[] { tokenAuthOnBoardingData2, tokenAuthOnBoardingData4 }, filteredCollection);
		}
		#endregion

		#region flag filter
		public void TestFilterByClaimMappingIdentifier()
		{
			const string filterName = "Claim Mapping Identifier";
			var codeDescriptionPairList = new OIDCClaimMappingIdentifiers();
			var filterValue = OIDCClaimMappingIdentifiers.Codes.EmailAddress;

			var tokenAuthOnBoardingData1 = AddRow();
			var tokenAuthOnBoardingData2 = AddRow(claimMappingIdentifierOverride: filterValue);
			var tokenAuthOnBoardingData3 = AddRow(claimMappingIdentifierOverride: OIDCClaimMappingIdentifiers.Codes.LoginName);
			var tokenAuthOnBoardingData4 = AddRow(claimMappingIdentifierOverride: filterValue);

			var filteredCollection = Filter(codeDescriptionPairList, filterName, filterValue);

			AssertContainsExactElementsInAnyOrder(new[] { tokenAuthOnBoardingData2, tokenAuthOnBoardingData4 }, filteredCollection);

			var filteredCollectionWithAll = Filter(codeDescriptionPairList, filterName, FilterStripBusinessObject.StatusAll);

			AssertContainsExactElementsInAnyOrder(new[] { tokenAuthOnBoardingData1, tokenAuthOnBoardingData2, tokenAuthOnBoardingData3, tokenAuthOnBoardingData4 }, filteredCollectionWithAll);
		}

		public void TestFilterByOidcServer()
		{
			const string filterName = "OIDC Server";
			var codeDescriptionPairList = new OIDCServerTypesList();
			var filterValue = OIDCServerTypesList.Codes.Okta;

			var tokenAuthOnBoardingData1 = AddRow();
			var tokenAuthOnBoardingData2 = AddRow(oidcServerOverride: filterValue);
			var tokenAuthOnBoardingData3 = AddRow(oidcServerOverride: OIDCServerTypesList.Codes.Generic);
			var tokenAuthOnBoardingData4 = AddRow(oidcServerOverride: filterValue);

			var filteredCollection = Filter(codeDescriptionPairList, filterName, filterValue);

			AssertContainsExactElementsInAnyOrder(new[] { tokenAuthOnBoardingData2, tokenAuthOnBoardingData4 }, filteredCollection);

			var filteredCollectionWithAll = Filter(codeDescriptionPairList, filterName, FilterStripBusinessObject.StatusAll);

			AssertContainsExactElementsInAnyOrder(new[] { tokenAuthOnBoardingData1, tokenAuthOnBoardingData2, tokenAuthOnBoardingData3, tokenAuthOnBoardingData4 }, filteredCollectionWithAll);
		}

		public void TestFilterByStatus()
		{
			const string filterName = "Onboarding Status";
			const string expectedCodes = "All, NEW, QUE, SPR, SMV, PPR, VER, CTC, COM, ERR, REV";
			var filterValue = "VER";

			var tokenAuthOnBoardingData1 = AddRow();
			var tokenAuthOnBoardingData2 = AddRow(statusOverride: filterValue);
			var tokenAuthOnBoardingData3 = AddRow(statusOverride: "REV");
			var tokenAuthOnBoardingData4 = AddRow(statusOverride: filterValue);

			var filteredCollection = Filter(filterName, expectedCodes, filterValue);

			AssertContainsExactElementsInAnyOrder(new[] { tokenAuthOnBoardingData2, tokenAuthOnBoardingData4 }, filteredCollection);

			var filteredCollectionWithAll = Filter(filterName, expectedCodes, FilterStripBusinessObject.StatusAll);

			AssertContainsExactElementsInAnyOrder(new[] { tokenAuthOnBoardingData1, tokenAuthOnBoardingData2, tokenAuthOnBoardingData3, tokenAuthOnBoardingData4 }, filteredCollectionWithAll);
		}

		public void TestFilterByWinzorOnly()
		{
			const string filterName = "Winzor Only";

			var tokenAuthOnBoardingData1 = AddRow(winzorOnlyOverride: true);
			var tokenAuthOnBoardingData2 = AddRow(winzorOnlyOverride: true);
			var tokenAuthOnBoardingData3 = AddRow(winzorOnlyOverride: false);
			var tokenAuthOnBoardingData4 = AddRow(winzorOnlyOverride: false);

			var filteredCollectionWinzorOnly = Filter(filterName, true);
			AssertContainsExactElementsInAnyOrder(new[] { tokenAuthOnBoardingData1, tokenAuthOnBoardingData2 }, filteredCollectionWinzorOnly);

			var filteredCollectionNonWinzorOnly = Filter(filterName, false);
			AssertContainsExactElementsInAnyOrder(new[] { tokenAuthOnBoardingData3, tokenAuthOnBoardingData4 }, filteredCollectionNonWinzorOnly);
		}
		#endregion

		#region incident filters
		public void TestFilterByIncidentNumber()
		{
			var filterValue = "WI00584471";
			AddRow();
			var tokenAuthOnBoardingData2 = AddRow();
			tokenAuthOnBoardingData2.Incident.IM_IncidentNumber = filterValue;
			var tokenAuthOnBoardingData3 = AddRow();
			tokenAuthOnBoardingData3.Incident.IM_IncidentNumber = "WI00584455";
			Factory.Save();

			var filteredCollection = FilterForeignTextValue("Incident", "Incident Number", filterValue);
			AssertContainsExactElementsInAnyOrder(new[] { tokenAuthOnBoardingData2 }, filteredCollection);
		}
		#endregion

		#region license filters
		public void TestFilterByEnterpriseCode()
		{
			var tokenAuthOnBoardingData1 = AddRow(licenseEnterpriseCodeOverride: "Edi");

			var tokenAuthOnBoardingData2 = AddRow(licenseEnterpriseCodeOverride: "CW1");
			Factory.Save();

			var filteredCollection1 = ModuleGuidFilter("Enterprise Code", tokenAuthOnBoardingData1.TOD_LE);
			var filteredCollection2 = ModuleGuidFilter("Enterprise Code", tokenAuthOnBoardingData2.TOD_LE);
			AssertContainsExactElementsInAnyOrder(new[] { tokenAuthOnBoardingData2 }, filteredCollection1);
			AssertContainsExactElementsInAnyOrder(new[] { tokenAuthOnBoardingData1 }, filteredCollection2);
		}
		#endregion

		#region tenant filters
		public void TestFilterByTenantId()
		{
			var tenantId = ZGuid.NewZGuid().ToString();
			var tenant1 = Factory.NewWithValidTestData<EdiIdentityTenant>();
			tenant1.IDT_TenantId = ZGuid.BrettsGuid.ToString();
			var tenant2 = Factory.NewWithValidTestData<EdiIdentityTenant>();
			tenant2.IDT_TenantId = tenantId;
			var tokenAuthOnBoardingData1 = AddRow();
			tokenAuthOnBoardingData1.TOD_IDT = tenant1.PK;
			var tokenAuthOnBoardingData2 = AddRow();
			tokenAuthOnBoardingData2.TOD_IDT = tenant1.PK;
			var tokenAuthOnBoardingData3 = AddRow();
			tokenAuthOnBoardingData3.TOD_IDT = tenant2.PK;
			Factory.Save();

			var filteredCollection = Filter("Tenant ID", ZGuid.BrettsGuid.ToString());

			AssertContainsExactElementsInAnyOrder(new[] { tokenAuthOnBoardingData1, tokenAuthOnBoardingData2 }, filteredCollection);
			filteredCollection = Filter("Tenant ID", tenantId);
			AssertContainsExactElementsInAnyOrder(new[] { tokenAuthOnBoardingData3 }, filteredCollection);
		}
		#endregion

		EdiTokenAuthOnBoardingDataCollection Filter(string filterName, int filterValue)
		{
			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = GetTypedFilter<ModuleNumberRangeFilter>(filterName, filterBizO);
			AssertEquals("Numbers and References", filter.Category.ToString());
			filter.IsActive = true;
			filter.PropertySearch = new ZString(ModuleNumberRangeFilter.SearchTexts.EqualTo.GetUnresolvedString());
			filter.EqualToDefaultProperty = filterValue;

			return new EdiTokenAuthOnBoardingDataCollection(Factory) { AdditionalFilter = filterBizO.Filter };
		}

		EdiTokenAuthOnBoardingDataCollection Filter(string filterName, string filterValue)
		{
			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = GetTypedFilter<ModuleTextFilter>(filterName, filterBizO);
			CombineAssertions(() =>
			{
				AssertEquals("Text Search", filter.Category.ToString());
				AssertNull(filter.List);
				Assert(filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.Exact));
			});
			filter.IsActive = true;
			filter.Property = filterValue;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;

			return new EdiTokenAuthOnBoardingDataCollection(Factory) { AdditionalFilter = filterBizO.Filter };
		}

		EdiTokenAuthOnBoardingDataCollection Filter(string filterName, bool filterValue)
		{
			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = GetTypedFilter<ModuleFlagsFilter>(filterName, filterBizO);
			AssertEquals("Status and Flags", filter.Category.ToString());
			filter.IsActive = true;
			filter.Property0 = filterValue;

			return new EdiTokenAuthOnBoardingDataCollection(Factory) { AdditionalFilter = filterBizO.Filter };
		}

		EdiTokenAuthOnBoardingDataCollection FilterForeignTextValue(string filterName, string property, string filterValue)
		{
			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = GetTypedFilter<ModuleGuidForeignCollectionFilter>(filterName, filterBizO);
			CombineAssertions(() =>
			{
				AssertEquals("Other", filter.Category.ToString());
				AssertEquals("any match, none match", filter.ComparisonOperator_List.CodesAsString);
				AssertEquals(0, filter.List.Count);
			});
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			filter.SelectedFilters.AddTextFilterStrip(property, filterValue);

			return new EdiTokenAuthOnBoardingDataCollection(Factory) { AdditionalFilter = filterBizO.Filter };
		}

		EdiTokenAuthOnBoardingDataCollection ModuleGuidFilter(string filterName, ZGuid filterValue)
		{
			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = GetTypedFilter<ModuleGuidFilter>(filterName, filterBizO);
			CombineAssertions(() =>
			{
				AssertEquals("Other", filter.Category.ToString());
				AssertEquals(2, filter.List.Count);
				Assert(filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.Exact));
			});
			filter.IsActive = true;
			filter.Property = filterValue;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;

			return new EdiTokenAuthOnBoardingDataCollection(Factory) { AdditionalFilter = filterBizO.Filter };
		}

		EdiTokenAuthOnBoardingDataCollection Filter(string filterName, string expectedCodes, string filterValue)
		{
			var filterBizO = CheckAndSetFlagFilter(filterName, expectedCodes, filterValue);

			return new EdiTokenAuthOnBoardingDataCollection(Factory) { AdditionalFilter = filterBizO.Filter };
		}

		EdiTokenAuthOnBoardingDataCollection Filter<T>(T codeDescriptionPairList, string filterName, string filterValue) where T : CodeDescriptionPairList
		{
			var expectedCodes = $"All, {codeDescriptionPairList.CodesAsString}";
			var filterBizO = CheckAndSetFlagFilter(filterName, expectedCodes, filterValue);

			return new EdiTokenAuthOnBoardingDataCollection(Factory) { AdditionalFilter = filterBizO.Filter };
		}

		FilterStripBusinessObject CheckAndSetFlagFilter(string filterName, string expectedCodes, string filterValue)
		{
			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = GetTypedFilter<ModuleTextFilter>(filterName, filterBizO);
			AssertType<CodeDescriptionPairList>(filter.List);
			CombineAssertions(() =>
			{
				var acceptedPairList = (CodeDescriptionPairList)filter.List;
				AssertCollectionContains(filterValue, acceptedPairList.GetAllCodes());
				AssertEquals(expectedCodes, acceptedPairList.CodesAsString);
				AssertEquals("Status and Flags", filter.Category.ToString());
				Assert(filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.Exact));
			});
			filter.IsActive = true;
			filter.Property = filterValue;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;

			return filterBizO;
		}

		static T GetTypedFilter<T>(string filterName, FilterStripBusinessObject filterBizO) where T : ModuleFilter
		{
			var moduleFilter = filterBizO[filterName];
			Assert($"{filterName} not found in filter list: {string.Join(", ", filterBizO.Select(f => f.Description))}", moduleFilter != null);
			AssertType<T>(moduleFilter);
			return (T)moduleFilter;
		}

		EdiTokenAuthOnBoardingData AddRow(
			string adminUserNameOverride = null,
			string authorityUrlOverride = null,
			string claimMappingIdentifierOverride = null,
			string claimMappingNameOverride = null,
			string configurationIdentifierOverride = null,
			string oidcServerOverride = null,
			int? retryOverride = null,
			string statusOverride = null,
			string systemUniqueIdentifierOverride = null,
			string verificationUsernameOverride = null,
			string licenseEnterpriseCodeOverride = null,
			bool? winzorOnlyOverride = null)
		{
			var result = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			result.TOD_ClaimMappingIdentifier = claimMappingIdentifierOverride ?? result.TOD_ClaimMappingIdentifier;
			result.TOD_ClaimMappingName = claimMappingNameOverride ?? result.TOD_ClaimMappingName;
			result.TOD_ConfigurationIdentifier = configurationIdentifierOverride ?? result.TOD_ConfigurationIdentifier;
			result.TOD_OIDCServer = oidcServerOverride ?? result.TOD_OIDCServer;
			result.TOD_Retry = retryOverride ?? result.TOD_Retry;
			result.TOD_Status = statusOverride ?? result.TOD_Status;
			result.TOD_SystemUniqueIdentifier = systemUniqueIdentifierOverride ?? result.TOD_SystemUniqueIdentifier;
			result.TOD_VerificationUsername = verificationUsernameOverride ?? result.TOD_VerificationUsername;
			result.TOD_WinzorOnly = winzorOnlyOverride ?? result.TOD_WinzorOnly;

			var licenseEnterprise = result.LicenceEnterprise;
			licenseEnterprise.LE_EnterpriseCode = licenseEnterpriseCodeOverride ?? licenseEnterprise.LE_EnterpriseCode;
			return result;
		}
	}
}
