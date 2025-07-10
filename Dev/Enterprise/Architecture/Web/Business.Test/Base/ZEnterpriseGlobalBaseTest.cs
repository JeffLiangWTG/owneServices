#if NETFRAMEWORK
using System.Web;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	[HttpContextEnabledTest]
	sealed class ZEnterpriseGlobalBaseTest : TestCase
	{
		public void TestSiteUser()
		{
			AssertNull("Pre-condition", HttpContext.Current.Session["SiteUser"]);

			var expectedSiteUser = new OrgContactWebUser();
			HttpContext.Current.Session.Add(ZEnterpriseGlobalBase.SiteUserSessionKey, expectedSiteUser);

			AssertEquals(expectedSiteUser.GetType(), Global.SiteUser.GetType());
		}

		public void TestGetNewSiteUser()
		{
			AssertNull("Base should return null for SiteUser by default", Global.GetNewSiteUser());
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			Global = new ZEnterpriseGlobalBaseForTest();
			HttpContext.Current.ApplicationInstance = Global;
		}

		protected override void TearDown()
		{
			Global.Dispose();

			base.TearDown();
		}

		ZEnterpriseGlobalBaseForTest Global;

		class ZEnterpriseGlobalBaseForTest : ZEnterpriseGlobalBase
		{
		}

		#endregion
	}
}
#endif
