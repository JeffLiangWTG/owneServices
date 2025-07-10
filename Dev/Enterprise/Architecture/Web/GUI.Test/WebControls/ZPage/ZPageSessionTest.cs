using System;
using System.Linq;
using System.Reflection;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZPageSessionTest : ZPageTestCase
	{
		[TestDate(2021, 5, 18)]
		public void TestSessionTimeSensitiveData()
		{
			Page.SetSessionTimeSensitiveData("SessionKey1", "data1");
			Page.SetSessionTimeSensitiveData("SessionKey2", "data2");
			AssertEquals("data1", Page.GetSessionTimeSensitiveData("SessionKey1"));
			AssertEquals("data2", Page.GetSessionTimeSensitiveData("SessionKey2"));

			TestDateAttribute.AddMinutes(1);
			Page.OnUnloadInternal(new EventArgs());
			AssertEquals("data1", Page.GetSessionTimeSensitiveData("SessionKey1"));
			AssertEquals("data2", Page.GetSessionTimeSensitiveData("SessionKey2"));

			TestDateAttribute.AddMinutes(9);
			TestDateAttribute.AddSeconds(59);
			Page.OnUnloadInternal(new EventArgs());
			AssertEquals("data1", Page.GetSessionTimeSensitiveData("SessionKey1"));

			TestDateAttribute.AddSeconds(2);
			Page.OnUnloadInternal(new EventArgs());
			AssertEquals("data1", Page.GetSessionTimeSensitiveData("SessionKey1"));
			AssertEquals(null, Page.GetSessionTimeSensitiveData("SessionKey2"));

			TestDateAttribute.AddMinutes(10);
			TestDateAttribute.AddSeconds(1);
			Page.OnUnloadInternal(new EventArgs());
			AssertEquals(null, Page.GetSessionTimeSensitiveData("SessionKey1"));
			AssertEquals(null, Page.GetSessionTimeSensitiveData("SessionKey2"));
		}

		public void TestSessionObjectsHaveNoReferenceToThePage()
		{
			Page.OnPreLoadInternal(new EventArgs());
			Page.RenderScripts();
			Page.SaveViewStateInternal();
			Page.OnUnloadInternal(new EventArgs());

			var sessionObjectsWithPageReferences = Enumerable.Empty<string>();
			var session = Page.Session;
			if (session != null)
			{
				foreach (string key in session.Keys)
				{
					var sessionObject = session[key];
					var fields = sessionObject.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
					sessionObjectsWithPageReferences = sessionObjectsWithPageReferences.Concat(fields.Where(f => f.GetValue(sessionObject).Equals(Page)).Select(f => $"Session item of type {sessionObject.GetType()} has reference to the page in {f.Name} field."));

					var properties = sessionObject.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
					sessionObjectsWithPageReferences = sessionObjectsWithPageReferences.Concat(properties.Where(p => p.GetValue(sessionObject).Equals(Page)).Select(p => $"Session item of type {sessionObject.GetType()} has reference to the page in {p.Name} property."));
				}
			}

			AssertEquals(string.Empty, string.Join(System.Environment.NewLine, sessionObjectsWithPageReferences));
		}

		protected override ZPage GetNewZPage()
		{
			return new ZTestPage();
		}
	}
}
