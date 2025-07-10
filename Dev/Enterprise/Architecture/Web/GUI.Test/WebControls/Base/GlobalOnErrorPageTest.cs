using System;
using System.Web;
using CargoWise.Data;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.Security;
using Enterprise.ZArchitecture.Web.Utilities.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.Testing
{
	[HttpContextEnabledTest]
	public sealed class GlobalOnErrorPageTest : TransactionedTestCase, IHttpContextEnabledTestWithRequestPath
	{
		#region TestErrorPageOnSessionStartAbandonsSession

		public void TestErrorPageOnSessionStartAbandonsSession()
		{
			var global = new ZGlobalForTesting();
			TestGlobal.InitZGlobalWithHttpContext(global);

			using (WebDataRegistry.Instance.AllowInlineFrames.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				global.Session_Start_ForTesting(this, EventArgs.Empty);
				AssertEquals(false, (bool)HttpContext.Current.Session[ClickJackingProtectionModule.SessionKeyClickJackingProtectionDisabled]);
			}

			using (WebDataRegistry.Instance.AllowInlineFrames.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				global.Session_Start_ForTesting(this, EventArgs.Empty);
				AssertEquals(true, (bool)HttpContext.Current.Session[ClickJackingProtectionModule.SessionKeyClickJackingProtectionDisabled]);
			}
		}
		#endregion

		public void TestClickJackingProtectionEnabledOnSessionStart()
		{
			var global = new ZGlobalForTesting();

			global.Session_Start_ForTesting(this, EventArgs.Empty);

			AssertEquals("Session should have been abandoned", true, HttpContext.Current.Session.IsNewSession);
		}

		public void TestReportErrorOnErrorPageRendersErrorDirectly()
		{
			var global = new ZGlobalForTesting();
			TestGlobal.InitZGlobalWithHttpContext(global);
			WebInitialiser.Initialise();

			HttpContext.Current.AddError(new DatabaseUpgradeInProgressException());
			global.Application_Error_ForTesting(HttpContext.Current.ApplicationInstance, EventArgs.Empty);
			AssertEquals(false, HttpContext.Current.Response.IsRequestBeingRedirected);
		}

		#region IHttpContextEnabledTestWithRequestPath Members

		public string RequestPath
		{
			get { return @"Error.aspx"; }
		}

		public string QueryString
		{
			get { return ""; }
		}

		#endregion
	}
}
