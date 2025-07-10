using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using CargoWise.Data;
using CargoWiseOne.WebInfrastructure;
using Enterprise.RemotePrinting.Server.RPSCore;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Server.Testing
{
	[HttpContextEnabledTest]
	class GlobalTest : TransactionedTestCase
	{
		public void TestConfigurationOK()
		{
			var global = new GlobalForTest();
			AssertEquals("ConfigurationOK should be true", true, global.ConfigurationOKForTest);
		}

		public void TestEnvironmentProvider()
		{
			var global = new GlobalForTest();
			using (var provider = global.WebEnvProvider_Exposed)
			{
				AssertType<WebPrintEnvironmentProvider>("EnvironmentProvider should be WebPrintEnvironmentProvider", provider);
			}
		}

		public void TestExceptionShouldBeReported()
		{
			var global = new GlobalForTest();
			Assert(!global.ExceptionShouldBeHandledForTest(new XmlException("Unexpected end of file has occurred.")));
			Assert(!global.ExceptionShouldBeHandledForTest(new XmlException("Unexpected end of file while parsing Name has occurred.")));
			Assert(!global.ExceptionShouldBeHandledForTest(new XmlException("Root element is missing.")));
			Assert(!global.ExceptionShouldBeHandledForTest(new XmlException("There is an unclosed literal string.")));

			Assert(!global.ExceptionShouldBeHandledForTest(RemotePrintingDbConnectionException.New(new DatabaseUpgradedException())));
		}

		public void TestExceptionShouldBeReported_IfWebUpgradeManagerIsUpgrateRunning()
		{
			var global = new GlobalForTest();
			var sqlContext = new WebUpgradeSqlContext(Db.ServerName, Db.DatabaseName, () => ((IDbConnectionInternals)Db.Connection).ADOConnection);
			var upgradeManager = new WebUpgradeManager(sqlContext);
			var webUpdaterMutexName = WebUpgradeManager.GetUpdaterMutexName(sqlContext.DatabaseName);
			using (var manualResetEventForRunning = new ManualResetEvent(false))
			using (var manualResetEventForWaitting = new ManualResetEvent(false))
			{
				Task task = null;
				try
				{
					task = Task.Run(() =>
					{
						using (var mutex = new UpgraderMutex(webUpdaterMutexName))
						{
							manualResetEventForWaitting.Set();
							manualResetEventForRunning.WaitOne();
						}
					});
					manualResetEventForWaitting.WaitOne();

					AssertEquals("It's running upgrade", true, WebUpgradeManager.IsUpgrateRunning());
					AssertEquals("ExceptionShouldBeHandled should be true", true, global.ExceptionShouldBeHandledForTest(new Exception("Jerry Test")));
				}
				finally
				{
					manualResetEventForRunning.Set();
					task.Wait();
				}
			}
		}

		public void TestApplication_Error_ReportError_ErrorPage()
		{
			var global = new GlobalForTest();
			typeof(HttpApplication).GetField("_context", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(global, HttpContext.Current);
			var error = new OutOfMemoryException("Error message for test");
			HttpContext.Current.AddError(error);

			global.Application_ErrorForTest(HttpContext.Current.Application, EventArgs.Empty);
			var url = HttpContext.Current.Response.RedirectLocation;
			AssertStartsWith("Should redirect to error page", "/Error.aspx", url);
		}

		public void TestApplication_Error_ReportError_SOAPAction()
		{
			var originalContext = HttpContext.Current;
			try
			{
				var global = new GlobalForTest();
				var request = new HttpRequest(string.Empty, "http://url.test", string.Empty);
				SetSOAPAction(request);
				var response = new HttpResponse(new StringWriter());
				var context = new HttpContext(request, response);
				HttpContext.Current = context;

				typeof(HttpApplication).GetField("_context", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(global, HttpContext.Current);

				var error = new OutOfMemoryException("Error message for test");
				HttpContext.Current.AddError(error);

				global.Application_ErrorForTest(HttpContext.Current.Application, EventArgs.Empty);

				AssertEquals(500, response.StatusCode);
				AssertEquals("text/plain;charset=utf-8", response.ContentType);
				AssertContains("Error message for test", response.StatusDescription);
			}
			finally
			{
				HttpContext.Current = originalContext;
			}
		}

		void SetSOAPAction(HttpRequest request)
		{
			var headers = request.Headers;
			var t = headers.GetType();
			var item = new System.Collections.ArrayList();

			t.InvokeMember("MakeReadWrite", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, headers, null);
			t.InvokeMember("InvalidateCachedArrays", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, headers, null);
			item.Add("SOAPAction");
			t.InvokeMember("BaseAdd", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, headers, new object[] { "SOAPAction", item });
			t.InvokeMember("MakeReadOnly", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, headers, null);
		}

		public class GlobalForTest : Global
		{
			public override string ApplicationRoot => "/";
			public void Application_ErrorForTest(object sender, EventArgs e) => Application_Error(sender, e);
			public bool ExceptionShouldBeHandledForTest(Exception unhandledException) => ExceptionShouldBeHandled(unhandledException);
			public bool ConfigurationOKForTest => ConfigurationOK;
		}
	}

	class ConfigTopLevelOnlyAssembliesTest : BaseWebConfigTopLevelOnlyAssembliesTest
	{
		protected override string DebugFilePath => GetSupplementaryContentPath("Enterprise", "Product", "Documents", "RemotePrinting", "Server", "Server", "Web.config");
		protected override string DebugCompilationAssembliesXPath => "system.web/compilation/assemblies";

		protected override IEnumerable<string> GetExpectedAddedAssembliesForCompilation()
		{
			yield return "System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=B77A5C561934E089";
			yield return "System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=B03F5F7F11D50A3A";
			yield return "System.Management, Version=2.0.0.0, Culture=neutral, PublicKeyToken=B03F5F7F11D50A3A";
			yield return "System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35";
			yield return "Microsoft.Owin";
			yield return "Microsoft.Owin.Host.SystemWeb";
			yield return "CargoWise.ComponentModel";
			yield return "Enterprise.ZArchitecture.Business";
			yield return "RemotePrinting.Server";
		}
	}
}
