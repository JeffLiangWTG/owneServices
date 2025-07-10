#if NETFRAMEWORK
using System;
using System.Web;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	[HttpContextEnabledTest]
	sealed class ZGlobalBaseTest : TestCase
	{
		public void TestSiteUser()
		{
			AssertNull("Pre-condition", ZGlobal.SiteUser);

			ZGlobal.OnCustomSessionStart(this, EventArgs.Empty);
			AssertEquals(typeof(OrgContactWebUser), ZGlobal.SiteUser.GetType());
		}

		public void TestOnCustomSessionStart()
		{
			AssertNull("Pre-condition", HttpContext.Current.Session["SiteUser"]);

			ZGlobal.OnCustomSessionStart(this, EventArgs.Empty);
			AssertEquals(typeof(OrgContactWebUser), HttpContext.Current.Session["SiteUser"].GetType());
		}

		public void TestGetNewSiteUser()
		{
			AssertEquals(typeof(OrgContactWebUser), ZGlobal.GetNewSiteUser().GetType());
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			ZGlobal = new ZGlobalBaseForTest();
			HttpContext.Current.ApplicationInstance = ZGlobal;
		}

		protected override void TearDown()
		{
			ZGlobal.Dispose();

			base.TearDown();
		}

		ZGlobalBaseForTest ZGlobal;

		class ZGlobalBaseForTest : ZGlobalBase
		{
			public new void OnCustomSessionStart(object sender, EventArgs e)
			{
				base.OnCustomSessionStart(sender, e);
			}
		}

		#endregion
	}
}
#elif NET
using CargoWise.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	sealed class ZGlobalBaseTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			var mockAccessor = new Mock<IHttpContextAccessor>();
			var context = new DefaultHttpContext();
			context.Session = new DummySession();
			mockAccessor.Setup(a => a.HttpContext).Returns(context);
			WebEnv.HttpContextAccessor = mockAccessor.Object;
		}

		protected override void TearDown()
		{
			WebEnv.HttpContextAccessor = null;
		}

		public void TestSiteUser()
		{
			var builder = new WebHostBuilder()
				.ConfigureServices((services) =>
				{
					services.AddDistributedMemoryCache();
					services.AddHttpContextAccessor();
					services.AddRouting();
					services.AddSession();
				})
				.Configure(app =>
				{
					app.UseSession();
					app.UseRouting();
					app.Use(async (context, next) =>
					{
						using (Db.DisposableActionForDbConnection())
						{
							WebEnv.HttpContextAccessor.HttpContext.Session.SetObject("SiteUser", (WebUser)new OrgContactWebUser());
							await next();
						}
					});
					app.UseEndpoints(endpoints =>
					{
						endpoints.MapGet("/dummy", () => "Test");
					});
				});
			using var testServer = new TestServer(builder);
			using var client = testServer.CreateClient();
			AssertNull("Pre-condition", WebEnv.SiteUser);
			var response = client.GetAsync("/dummy").Result;
			AssertEquals(typeof(OrgContactWebUser), WebEnv.SiteUser.GetType());
		}
	}
}
#endif
