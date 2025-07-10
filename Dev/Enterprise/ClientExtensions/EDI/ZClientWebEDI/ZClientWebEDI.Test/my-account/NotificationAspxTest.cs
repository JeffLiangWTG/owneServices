using System.Net;
using System.Web;
using CargoWise.Common;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[TestedType(typeof(Notification))]
	public class NotificationAspxTest : ZPageTestCase
	{
		[HttpContextEnabledTest]
		public void TestShowMessage()
		{
			var pageTitle = "Test";
			var message = "Hello World";
			var notificationType = Notification.NotificationType.Warning;

			Notification.ShowMessage(HttpContext.Current.Response, pageTitle, message, notificationType);

			var url = HttpContext.Current.Response.RedirectLocation;
			var queryStrings = url.Split('=')[1];
			var queryObject = new SecureQueryString(WebUtility.UrlDecode(queryStrings));

			AssertEquals(pageTitle, queryObject["Title"]);
			AssertEquals(message, queryObject["Message"]);
			AssertEquals(notificationType.ToString(), queryObject["NotificationType"]);
		}

		public void TestShowMessage_invalidToken()
		{
			HttpContext.Current.Request.QueryString.Add(SecureQueryString.QueryStringKey, "invalid");
			TestPage.InitForTest();

			AssertEquals("MyAccount", TestPage.Title);
			AssertEquals("Invalid Data", TestPage.Message);
			AssertEquals(Notification.NotificationType.Error, TestPage.Type);
		}

		PageForTest TestPage => Page as PageForTest;

		public static SecureQueryString LoadDataFromUr(string url)
		{
			var queryStrings = url.Split('=')[1];
			return new SecureQueryString(WebUtility.UrlDecode(queryStrings));
		}

		protected override ZPage GetNewZPage()
		{
			return new PageForTest();
		}

		class PageForTest : Notification
		{
			public void InitForTest() => OnInit(null);
		}
	}
}
