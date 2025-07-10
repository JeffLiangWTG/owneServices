using CargoWise.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(WebCallbackUrlHandler))]
	public class WebCallbackUrlHandlerTests : TestCase
	{
		const string TestCommand = "Command=Acc.WebCallback?UZEP&code=3e5e156ad94a487fae91e39091548f7a&state=good";

		public void TestQueryStringHandling()
		{
			QueryString queryString = null;
			var handler = new WebCallbackUrlHandler((QueryString qs) => { queryString = qs; });

			Assert(!handler.CanHandle(new QueryString(null)));
			Assert(!handler.CanHandle(new QueryString(string.Empty)));
			Assert(!handler.CanHandle(new QueryString("Command=Acc.WebCall")));
			Assert(handler.CanHandle(new QueryString("Command=Acc.WebCallback")));
			Assert(handler.CanHandle(new QueryString("Command=Acc.WebCallback?Something")));
			Assert(handler.CanHandle(new QueryString("Command=Acc.WebCallback&Param=Value")));

			Assert(handler.CanHandle(new QueryString(TestCommand)));
			Assert(handler.Handle(new QueryString(TestCommand)));

			AssertNotNull(queryString);
		}

		public void TestQueryStringExecution()
		{
			QueryString queryString = null;
			var testUrl = $"{UrlHandler.EdiUrlPrefix}{TestCommand}";
			var handler = new WebCallbackUrlHandler((QueryString qs) => { queryString = qs; });

			Assert(!EnterpriseUrlHandlerService.Instance.ExecuteUrl(testUrl, waitForAppToStart: false));
			AssertNull(queryString);

			EnterpriseUrlHandlerService.RegisterUrlHandler(handler);

			Assert(EnterpriseUrlHandlerService.Instance.ExecuteUrl(testUrl, waitForAppToStart: false));
			AssertNotNull(queryString);
			AssertEquals("3e5e156ad94a487fae91e39091548f7a", queryString["code"]);
		}
	}
}
