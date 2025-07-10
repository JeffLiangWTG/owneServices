using System;
using Enterprise.Client.EDI;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class MyAccountLoginHelperPageTest : ZArchitecture.Web.GUI.WebControls.Testing.ZPageTestCase
	{
		public void TestRedirect()
		{
			Page.Request.QueryString.Add("a", "1");
			Page.Request.QueryString.Add("ReturnUrl", "/myaccount/abc.htm?a=1");
			Page.Request.QueryString.Add("b", "2");
			var helper = new MyAccountLoginHelperForTest(Page as BasePage);
			Assert(string.IsNullOrEmpty(Page.Response.RedirectLocation));
			helper.Redirect_Exposed();
			AssertContains("/myaccount/abc.htm?a=1", Page.Response.RedirectLocation);
		}

		public void TestRedirect_ContainsResizeInlineFramePage()
		{
			Page.Request.QueryString.Add("a", "1");
			Page.Request.QueryString.Add("ReturnUrl", "http://localhost/myaccount/resize-iframe.html?id=abc");
			Page.Request.QueryString.Add("b", "2");
			var helper = new MyAccountLoginHelperForTest(Page as BasePage);
			Assert(string.IsNullOrEmpty(Page.Response.RedirectLocation));
			helper.Redirect_Exposed();
			AssertContains(Page.AppInstance.DefaultPage, Page.Response.RedirectLocation);
		}

		public void TestRedirect_OpenRedirectAttack()
		{
			using (EDIDataRegistry.Instance.MyAccountHostingSiteRootUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://myaccount.com/"))
			{
				Page.Request.QueryString.Add("a", "1");
				Page.Request.QueryString.Add("ReturnUrl", "www.supersafe.com");
				Page.Request.QueryString.Add("b", "2");
				var helper = new MyAccountLoginHelperForTest(Page as BasePage);
				Assert(string.IsNullOrEmpty(Page.Response.RedirectLocation));
				helper.Redirect_Exposed();
				AssertEquals("https://myaccount.com/Home.aspx", Page.Response.RedirectLocation);
			}
		}

		class MyAccountLoginHelperForTest : MyAccountLoginHelper
		{
			public MyAccountLoginHelperForTest(BasePage page) : base(page)
			{
			}

			public void Redirect_Exposed() => base.Redirect();
		}

		protected override ZPage GetNewZPage() => new BasePageForTest()
		{ IsCreateNewAppInstanceIfNullForTest = true };
		class BasePageForTest : BasePage
		{
			protected override ZGlobal GetNewTestGlobal()
			{
				GlobalForTest result = new GlobalForTest();
				result.OnCustomSessionStart();
				return result;
			}
		}

		class GlobalForTest : Global
		{
			public void OnCustomSessionStart()
			{
				base.OnCustomSessionStart(this, EventArgs.Empty);
			}
		}
	}
}