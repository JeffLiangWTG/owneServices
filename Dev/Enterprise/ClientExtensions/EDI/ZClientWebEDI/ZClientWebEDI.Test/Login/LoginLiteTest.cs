using System;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class LoginLiteTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestPageLoad()
		{
			var page1 = GetPageForTest();
			page1.DoPageLoad();
		}

		LoginLiteForTest GetPageForTest()
		{
			var page = new LoginLiteForTest();
			MethodInfo method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
			method.Invoke(page, new object[] { HttpContext.Current });
			page.InitialiseControls();
			return page;
		}

		public void TestTabIndex()
		{
			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			var str = resourceRetriever.GetString(@"LoginLite.aspx", System.Text.Encoding.UTF8);
			var lines = str.SplitByLine();
			void assertTabIndex(string id, string tabIndex)
			{
				AssertEquals($"{id}-{tabIndex}", true, lines.Any(x => x.Contains($" id=\"{id}\" ") && x.Contains($" TabIndex=\"{tabIndex}\"")));
			}

			assertTabIndex("LoginNameTextBox", "1");
			assertTabIndex("PasswordTextBox", "2");
			assertTabIndex("CompanyCodeTextBox", "3");
			assertTabIndex("SigninBtn", "4");
			assertTabIndex("RememberMeCheckBox", "5");
			AssertEquals(true, lines.Any(x => x.Contains("<a href=\"RetrieveLogin.aspx\" TabIndex=\"-1\">")));
		}

		class LoginLiteForTest : LoginLite
		{
			public void DoPageLoad()
			{
				try
				{
					base.OnLoad(EventArgs.Empty);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (ex is QueryStringException)
					{
						throw;
					}
				}
			}

			protected override ZGlobal GetNewTestGlobal()
			{
				var result = new GlobalForTest();
				result.OnCustomSessionStart();
				return result;
			}

			public void InitialiseControls()
			{
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
}
