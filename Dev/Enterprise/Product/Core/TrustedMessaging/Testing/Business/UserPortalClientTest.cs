using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Integration;
using Enterprise.TrustedMessaging.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.TrustedMessaging.Testing
{
	public class UserPortalClientTest : TestCaseWithFactory
	{
		[TestDate(2021, 1, 1)]
		public void TestCreateUserAgreementInfo()
		{
			var reg = ObjectFactory.Get<IProductRegistration>();
			reg.ResetKeyToDefault();

			try
			{
				reg.KeyForTest.DatabaseNumberForTest = 100;

				var testDeveloperUser = (IGlbStaff)Factory.New(ObjectFactory.GetType<IGlbStaff>());
				testDeveloperUser.GS_Code = "TDU";
				testDeveloperUser.GS_LoginName = "tdu";
				testDeveloperUser.GS_FullName = "Test Developer User";
				testDeveloperUser.GS_EmailAddress = "tdu@cw1.com";

				Factory.Save();

				using (var userContext = Env.SetTemporaryUserContext(testDeveloperUser.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					var agreementInfo = new UserPortalClientForTest().CreateUserAgreementInfo_Exposed("MYA", true);
					AssertEquals("CW1", agreementInfo.Product);
					AssertEquals("100", agreementInfo.SystemId);
					AssertEquals("TDU", agreementInfo.UserId);
					AssertEquals("Test Developer User", agreementInfo.FullName);
					AssertEquals("tdu@cw1.com", agreementInfo.Email);
					AssertEquals("MYA", agreementInfo.UserAgreementType);
					AssertEquals(true, agreementInfo.ShouldSendAgreementCopy);
					AssertEquals(new ZDateTime(2021, 1, 1).AddDays(1), agreementInfo.InfoExpires);
				}
			}
			finally
			{
				reg.ResetKeyToDefault();
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestCreateUserAgreementInfo_NoCompany()
		{
			var reg = ObjectFactory.Get<IProductRegistration>();
			reg.ResetKeyToDefault();

			try
			{
				reg.KeyForTest.DatabaseNumberForTest = 100;

				var testDeveloperUser = (IGlbStaff)Factory.New(ObjectFactory.GetType<IGlbStaff>());
				testDeveloperUser.GS_Code = "TDU";
				testDeveloperUser.GS_LoginName = "tdu";
				testDeveloperUser.GS_FullName = "Test Developer User";
				testDeveloperUser.GS_EmailAddress = "tdu@cw1.com";

				Factory.Save();

				using (var userContext = Env.SetTemporaryUserContext(testDeveloperUser.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					StaticCurrentFetcher.Instance.CurrentBranch.GB_GC = ZGuid.NewZGuid();
					UserAgreementInfo agreementInfo = null;
					AssertNoExceptionThrown(delegate { agreementInfo = new UserPortalClientForTest().CreateUserAgreementInfo_Exposed("MYA", true); });
					AssertEquals("CW1", agreementInfo.Product);
					AssertEquals("100", agreementInfo.SystemId);
					AssertEquals("TDU", agreementInfo.UserId);
					AssertEquals("Test Developer User", agreementInfo.FullName);
					AssertEquals("tdu@cw1.com", agreementInfo.Email);
					AssertEquals("MYA", agreementInfo.UserAgreementType);
					AssertEquals(true, agreementInfo.ShouldSendAgreementCopy);
					AssertEquals(new ZDateTime(2021, 1, 1).AddDays(1), agreementInfo.InfoExpires);

					ErrorReporter.Instance.Clear();
				}
			}
			finally
			{
				reg.ResetKeyToDefault();
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestCreateUserAgreementInfo_NoCountry()
		{
			var reg = ObjectFactory.Get<IProductRegistration>();
			reg.ResetKeyToDefault();

			try
			{
				reg.KeyForTest.DatabaseNumberForTest = 100;

				var testDeveloperUser = (IGlbStaff)Factory.New(ObjectFactory.GetType<IGlbStaff>());
				testDeveloperUser.GS_Code = "TDU";
				testDeveloperUser.GS_LoginName = "tdu";
				testDeveloperUser.GS_FullName = "Test Developer User";
				testDeveloperUser.GS_EmailAddress = "tdu@cw1.com";

				Factory.Save();

				using (var userContext = Env.SetTemporaryUserContext(testDeveloperUser.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					StaticCurrentFetcher.Instance.CurrentBranch.Company.GC_RN_NKCountryCode = ":(";
					UserAgreementInfo agreementInfo = null;
					AssertNoExceptionThrown(delegate { agreementInfo = new UserPortalClientForTest().CreateUserAgreementInfo_Exposed("MYA", true); });
					AssertEquals("CW1", agreementInfo.Product);
					AssertEquals("100", agreementInfo.SystemId);
					AssertEquals("TDU", agreementInfo.UserId);
					AssertEquals("Test Developer User", agreementInfo.FullName);
					AssertEquals("tdu@cw1.com", agreementInfo.Email);
					AssertEquals("MYA", agreementInfo.UserAgreementType);
					AssertEquals(true, agreementInfo.ShouldSendAgreementCopy);
					AssertEquals(new ZDateTime(2021, 1, 1).AddDays(1), agreementInfo.InfoExpires);
				}
			}
			finally
			{
				reg.ResetKeyToDefault();
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestCreateEnterpriseAgreementInfo()
		{
			var reg = ObjectFactory.Get<IProductRegistration>();
			reg.ResetKeyToDefault();

			try
			{
				reg.KeyForTest.DatabaseNumberForTest = 100;

				Factory.Save();

				var agreementInfo = new UserPortalClientForTest().CreateEnterpriseAgreementInfo_Exposed("CWN", false);
				AssertEquals("CW1", agreementInfo.Product);
				AssertEquals("100", agreementInfo.SystemId);
				AssertEquals("CWN", agreementInfo.UserAgreementType);
				AssertEquals(false, agreementInfo.ShouldSendAgreementCopy);
				AssertEquals(new ZDateTime(2021, 1, 1).AddDays(1), agreementInfo.InfoExpires);
			}
			finally
			{
				reg.ResetKeyToDefault();
			}
		}

		public void TestCreateERequestInfo()
		{
			var reg = ObjectFactory.Get<IProductRegistration>();
			reg.ResetKeyToDefault();

			try
			{
				reg.KeyForTest.DatabaseNumberForTest = 100;

				var testDeveloperUser = (IGlbStaff)Factory.New(ObjectFactory.GetType<IGlbStaff>());
				testDeveloperUser.GS_Code = "TDU";
				testDeveloperUser.GS_LoginName = "tdu";
				testDeveloperUser.GS_FullName = "Test Developer User";
				testDeveloperUser.GS_EmailAddress = "tdu@cw1.com";

				Factory.Save();

				using (var userContext = Env.SetTemporaryUserContext(testDeveloperUser.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					var eRequestInfo = new UserPortalClientForTest().CreateERequestInfo_Exposed("langId", "INC_001", "Module#1", "SubModule#2", "Ref#3", "DDDABCSYD");
					AssertEquals("CW1", eRequestInfo.Product);
					AssertEquals("100", eRequestInfo.SystemId);
					AssertEquals("TDU", eRequestInfo.UserId);
					AssertEquals("Test Developer User", eRequestInfo.FullName);
					AssertEquals("tdu@cw1.com", eRequestInfo.Email);

					AssertEquals("langId", eRequestInfo.LandingPageId);
					AssertEquals("INC_001", eRequestInfo.IncidentNumber);
					AssertEquals("Module#1", eRequestInfo.Module);
					AssertEquals("SubModule#2", eRequestInfo.SubModule);
					AssertEquals("Ref#3", eRequestInfo.ReferenceId);
					AssertEquals("DDDABCSYD", eRequestInfo.LicenceCode);
				}
			}
			finally
			{
				reg.ResetKeyToDefault();
			}
		}

		public void TestCreateAutoLoginInfo()
		{
			var reg = ObjectFactory.Get<IProductRegistration>();
			reg.ResetKeyToDefault();

			try
			{
				reg.KeyForTest.DatabaseNumberForTest = 100;

				var testDeveloperUser = (IGlbStaff)Factory.New(ObjectFactory.GetType<IGlbStaff>());
				testDeveloperUser.GS_Code = "TDU";
				testDeveloperUser.GS_LoginName = "tdu";
				testDeveloperUser.GS_FullName = "Test Developer User";
				testDeveloperUser.GS_EmailAddress = "tdu@cw1.com";

				Factory.Save();

				using (var userContext = Env.SetTemporaryUserContext(testDeveloperUser.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					var autoLoginInfo = new UserPortalClientForTest().CreateAutoLoginInfo_Exposed(new Uri("http://www.cw1.com/"));
					AssertEquals("CW1", autoLoginInfo.Product);
					AssertEquals("100", autoLoginInfo.SystemId);
					AssertEquals("TDU", autoLoginInfo.UserId);
					AssertEquals("Test Developer User", autoLoginInfo.FullName);
					AssertEquals("tdu@cw1.com", autoLoginInfo.Email);
					AssertEquals(new Uri("http://www.cw1.com/"), autoLoginInfo.ReturnUrl);
				}
			}
			finally
			{
				reg.ResetKeyToDefault();
			}
		}

		public void TestCreateOAuthLoginInfo()
		{
			var reg = ObjectFactory.Get<IProductRegistration>();
			reg.ResetKeyToDefault();

			try
			{
				reg.KeyForTest.DatabaseNumberForTest = 100;

				var testDeveloperUser = (IGlbStaff)Factory.New(ObjectFactory.GetType<IGlbStaff>());
				testDeveloperUser.GS_Code = "TDU";
				testDeveloperUser.GS_LoginName = "tdu";
				testDeveloperUser.GS_FullName = "Test Developer User";
				testDeveloperUser.GS_EmailAddress = "tdu@cw1.com";

				Factory.Save();

				using (var userContext = Env.SetTemporaryUserContext(testDeveloperUser.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					var autoLoginInfo = new UserPortalClientForTest().CreateOAuthLoginInfo_Exposed(new Uri("http://www.cw1.com/"));
					AssertEquals("CW1", autoLoginInfo.Product);
					AssertEquals("100", autoLoginInfo.SystemId);
					AssertEquals("TDU", autoLoginInfo.UserId);
					AssertEquals("Test Developer User", autoLoginInfo.FullName);
					AssertEquals("tdu@cw1.com", autoLoginInfo.Email);
					AssertEquals(new Uri("http://www.cw1.com/"), autoLoginInfo.RedirectUriString);
				}
			}
			finally
			{
				reg.ResetKeyToDefault();
			}
		}
	}

	class UserPortalClientForTest : UserPortalClient
	{
		public UserAgreementInfo CreateUserAgreementInfo_Exposed(string userAgreementType, bool shouldSendAgreementCopy = false)
			=> CreateUserAgreementInfo(userAgreementType, shouldSendAgreementCopy);
		
		public EnterpriseAgreementInfo CreateEnterpriseAgreementInfo_Exposed(string userAgreementType, bool shouldSendAgreementCopy = false)
			=> CreateEnterpriseAgreementInfo(userAgreementType, shouldSendAgreementCopy);

		public ERequestInfo CreateERequestInfo_Exposed(string landingPageId, string incidentNumber, string module, string subModule, string referenceId, string licenceCode)
			=> CreateERequestInfo(landingPageId, incidentNumber, module, subModule, referenceId, licenceCode);

		public TrustedAutoLoginInfo CreateAutoLoginInfo_Exposed(Uri returnUrl)
			=> CreateAutoLoginInfo(returnUrl);

		public AuthenticationTokenInfo CreateOAuthLoginInfo_Exposed(Uri returnUrl)
			=> CreateOauthLoginInfo(returnUrl);
	}
}
