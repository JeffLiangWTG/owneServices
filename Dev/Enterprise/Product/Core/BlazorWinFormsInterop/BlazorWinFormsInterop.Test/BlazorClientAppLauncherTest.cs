using System;
using System.Web;
using CargoWise.Application;
using CargoWise.Data;
using CargoWiseNext.Infrastructure.Authentication;
using CargoWiseNext.Infrastructure.Installations;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.BlazorWinFormsInterop.Test
{
	public class BlazorClientAppLauncherTest : TransactionedTestCase
	{
		public void TestLaunchSetsCorrectFileName()
		{
			var (token, programLauncher) = RunBlazorClientLauncherTest(new Uri(DataRegistry.Instance.BlazorUrl), "https://localhost:80");

			AssertNotNull("An auth token should have been generated", token);
			programLauncher.Verify(p => p.Launch($"{UrlHandlers.CargoWiseClient}:{DataRegistry.Instance.BlazorUrl}?{QueryParameters.ClientToken}={token}", ""));
		}

		public void TestLaunchSetsListenUrlAsScopeIfPassed()
		{
			var (token, _) = RunBlazorClientLauncherTest(new Uri(DataRegistry.Instance.BlazorUrl), "https://localhost:80");

			using (Db.DisposableActionForDbConnection())
			{
				ITokenizedAccessControl accessControl = new TokenizedAccessControl();
				accessControl.TryPeek(token, "BLC", out var tokenInfo);
				AssertEquals(tokenInfo.Scope, "https://localhost:80");
			}
		}

		public void TestLaunchSetsUserAsDefaultScope()
		{
			var (token, _) = RunBlazorClientLauncherTest(new Uri(DataRegistry.Instance.BlazorUrl), null);

			using (Db.DisposableActionForDbConnection())
			{
				ITokenizedAccessControl accessControl = new TokenizedAccessControl();
				accessControl.TryPeek(token, "BLC", out var tokenInfo);
				AssertEquals(tokenInfo.Scope, GlbStaff.CurrentUser.GS_LoginName);
			}
		}

		static (string, Mock<IProgramLauncher>) RunBlazorClientLauncherTest(Uri launchUrl, string listenUrl)
		{
			var token = default(string);
			var programLauncher = new Mock<IProgramLauncher>();
			programLauncher.Setup(p => p.Launch(It.Is<string>(s => s.StartsWith($"{UrlHandlers.CargoWiseClient}:{DataRegistry.Instance.BlazorUrl}?{QueryParameters.ClientToken}=")), It.Is<string>(s => s == "")))
				.Callback<string, string>((filename, args) =>
				{
					{
						var query = new Uri(filename.Replace($"{UrlHandlers.CargoWiseClient}:", "")).Query;
						var queryString = HttpUtility.ParseQueryString(query);
						token = queryString[QueryParameters.ClientToken];
					}
				});
			ObjectFactory.Substitute(programLauncher.Object);

			var urlhandlerProvider = new Mock<IUrlHandlerProvider>();
			urlhandlerProvider.Setup(u => u.GetUrlHandler()).Returns(UrlHandlers.CargoWiseClient);
			ObjectFactory.Substitute(urlhandlerProvider.Object);
			var blazorClientAppLauncher = new BlazorClientAppLauncher();

			blazorClientAppLauncher.Launch(launchUrl, listenUrl);

			return (token, programLauncher);
		}
	}
}
