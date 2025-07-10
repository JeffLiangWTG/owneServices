using System;
using System.Reflection;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	[HttpContextEnabledTest]
	public abstract class ZPageTestCase : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestPageLoadNoExceptions()
		{
			Type pageType = Page.GetType();
			FieldInfo[] fields = pageType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

			foreach (FieldInfo fieldInfo in fields)
			{
				if (fieldInfo.GetValue(Page) == null && fieldInfo.FieldType.IsSubclassOf(typeof(Control)))
				{
					Control newInstance = (Control)Activator.CreateInstance(fieldInfo.FieldType);
					fieldInfo.SetValue(Page, newInstance);
				}
			}

			MethodInfo methodInfo = typeof(ZPage).GetMethod("OnLoad", BindingFlags.NonPublic | BindingFlags.Instance);
			methodInfo.Invoke(Page, new object[] { EventArgs.Empty });
		}

		protected override void TearDown()
		{
			if (fPage != null)
			{
				Page.Dispose();
				TempDirectory.Dispose();
			}

			base.TearDown();
		}

		protected ZPage Page
		{
			get
			{
				if (fPage == null)
				{
					fPage = GetNewZPage();
					MethodInfo method = typeof(Page).GetMethod("SetIntrinsics",
							BindingFlags.NonPublic | BindingFlags.Instance,
							null,
							new Type[] { typeof(HttpContext) },
							null);
					method.Invoke(fPage, new object[] { HttpContext.Current });

					fPage.SetServerMappedPathForTest(TempDirectory.DirectoryName);
				}
				return fPage;
			}
		}

		protected HttpSessionStateContainer SessionStateContainer
		{
			get
			{
				if (fSessionStateContainer == null)
				{
					PropertyInfo containerProperty = typeof(HttpSessionState).GetProperty("Container", BindingFlags.NonPublic | BindingFlags.Instance);
					fSessionStateContainer = (HttpSessionStateContainer)containerProperty.GetValue(HttpContext.Current.Session, null);
				}
				return fSessionStateContainer;
			}
		}

		TempDirectory TempDirectory
		{
			get
			{
				if (fTempDirectory == null)
				{
					fTempDirectory = new TempDirectory();
				}

				return fTempDirectory;
			}
		}

		protected abstract ZPage GetNewZPage();

		ZPage fPage;
		HttpSessionStateContainer fSessionStateContainer;
		TempDirectory fTempDirectory;
	}
}
