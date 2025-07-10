using System.Collections.Specialized;
using System.Reflection;
using System.Web;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	[HttpContextEnabledTest]
	sealed class ZIFramePageWithFilterStripBizOTest : TestCaseWithFactory
	{
		public void TestOnInit_ValidFilterStrip()
		{
			var isReadOnlyProperty = typeof(NameValueCollection).GetProperty("IsReadOnly", BindingFlags.NonPublic | BindingFlags.Instance);
			isReadOnlyProperty.SetValue(HttpContext.Current.Request.Params, false, null);
			HttpContext.Current.Request.Params["filterStripPK"] = "C7115221-0EAF-4514-A3F6-8679C26FB1DF";
			HttpContext.Current.Session["C7115221-0EAF-4514-A3F6-8679C26FB1DF"] = new DummyFilterBusinessObject();

			using (var temp = new TempDirectory())
			{
				var testPage = new TestZIFramePageWithFilterStripBizO("filterStripPK");
				testPage.SetServerMappedPathForTest(temp.DirectoryName);

				testPage.OnInit();

				Assert(!HttpContext.Current.Response.IsRequestBeingRedirected);
			}
		}

		public void TestOnInit_InvalidQueryString()
		{
			var testPage = new TestZIFramePageWithFilterStripBizO("filterStripPkNotInSession");

			testPage.OnInit();

			Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
			AssertEquals($"{testPage.AppInstance.ErrorPage}?invalidQuery=true", HttpContext.Current.Response.RedirectLocation);
		}
	}
}
