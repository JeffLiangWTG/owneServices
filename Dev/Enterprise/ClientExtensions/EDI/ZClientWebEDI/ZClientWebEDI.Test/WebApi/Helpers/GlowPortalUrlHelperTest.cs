using System;
using System.Collections.Specialized;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.CustomerService.Business;
using Enterprise.ZArchitecture.GlowInterop;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class TokenAccessControlMock : ITokenizedAccessControl
	{
		public bool TryConsume(string accessToken, string accessTokenType, out AccessTokenInfo info)
		{
			throw new NotImplementedException();
		}

		public bool TryPeek(string accessToken, string accessTokenType, out AccessTokenInfo info)
		{
			throw new NotImplementedException();
		}

		public bool TryCreate(string token, string type, bool isPermanent, DateTime? expiresAtUtc, int useCount, AccessTokenInfo info)
		{
			return true;
		}
	}
	class GlowPortalUrlHelperTest : TestCaseWithFactory
	{
		public void TestGetNewRequestUrl()
		{
			var contactPK = Guid.NewGuid();
			var userAccountPK = Guid.NewGuid();
			var licenseCode = "licenceCode";
			var product = "pTest";
			var module = "mTest";
			var subModule = "smTest";
			var criticality = "imp";
			var referenceId = "refTest";
			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://unit-testing/GlowPortal/");

			var gQDict = GetNewRequestUrl(contactPK, userAccountPK, licenseCode, product, module, subModule, criticality, referenceId);
			AssertNewRequestUrl(gQDict, licenseCode, product, module, subModule, criticality, referenceId);
		}

		public void TestGetNewRequestUrl_UpdateModule()
		{
			var contactPK = Guid.NewGuid();
			var userAccountPK = Guid.NewGuid();
			var licenseCode = "licenceCode";
			var product = "NAN";
			var module = "mTest";
			var subModule = "smTest";
			var criticality = "imp";
			var referenceId = "refTest";
			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://unit-testing/GlowPortal/");

			var gQDict = GetNewRequestUrl(contactPK, userAccountPK, licenseCode, product, module, subModule, criticality, referenceId);
			AssertNewRequestUrl(gQDict, licenseCode, product, module, subModule, criticality, referenceId, "Should not change module");

			var sourceModules = new SourceModuleCollection();
			sourceModules.AddNew(subModule, "Dummy 1", "Dummies >", ModuleListType.MenuSection, "DUM", true, true, "NAN");
			EDIDataRegistry.Instance.SourceModules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, sourceModules);

			gQDict = GetNewRequestUrl(contactPK, userAccountPK, licenseCode, product, module, subModule, criticality, referenceId);
			AssertNewRequestUrl(gQDict, licenseCode, product, module, subModule, criticality, referenceId, "Should not change module when menu type isn't DetectedMenuItem");

			sourceModules = new SourceModuleCollection();
			sourceModules.AddNew(subModule, "Dummy 1", "Dummies >", ModuleListType.MenuSection, "DUM", true, true, "NAN");
			sourceModules.AddNew(subModule, "Dummy 1", "Dummies >", ModuleListType.DetectedMenuItem, "ABC", true, true, "NAN");
			EDIDataRegistry.Instance.SourceModules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, sourceModules);

			gQDict = GetNewRequestUrl(contactPK, userAccountPK, licenseCode, product, module, subModule, criticality, referenceId);
			AssertNewRequestUrl(gQDict, licenseCode, product, "ABC", subModule, criticality, referenceId, "Should change module to the menu item with type DetectedMenuItem");
		}

		NameValueCollection GetNewRequestUrl(Guid contactPK, Guid userAccountPK, string licenseCode, string product, string module, string subModule, string criticality, string referenceId)
		{
			var glowPortalUrlHelper = new GlowPortalUrlHelper(new TokenAccessControlMock());
			var requestUrl = glowPortalUrlHelper.GetNewRequestUrl(contactPK, userAccountPK, licenseCode, product, module, subModule, criticality, referenceId);
			var qDict = System.Web.HttpUtility.ParseQueryString(requestUrl.Query);
			var qData = qDict[SecureQueryString.QueryStringKey];
			var securityQueryString = new SecureQueryString(qData);
			var glowUrl = new Uri(securityQueryString["glowUrl"]);
			return System.Web.HttpUtility.ParseQueryString(glowUrl.Query);
		}

		void AssertNewRequestUrl(NameValueCollection gQDict, string licenseCode, string product, string module, string subModule, string criticality, string referenceId, string moduleErrorMessage = "")
		{
			AssertNotNull(gQDict["sso_otp"]);
			AssertEquals(licenseCode, gQDict["LicenseCode"]);
			AssertEquals(product, gQDict["Product"]);
			AssertEquals(moduleErrorMessage, module, gQDict["Module"]);
			AssertEquals(subModule, gQDict["SubModule"]);
			AssertEquals(criticality, gQDict["Criticality"]);
			AssertEquals(referenceId, gQDict["ReferenceId"]);
		}
	}
}
