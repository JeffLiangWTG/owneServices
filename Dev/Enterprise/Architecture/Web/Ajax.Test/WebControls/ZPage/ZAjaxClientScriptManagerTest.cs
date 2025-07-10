using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZAjaxClientScriptManagerTest : TestCase
	{
		#region TestClientScriptIncludeRegistration

		public void TestClientScriptIncludeRegistration()
		{
			using (ZAjaxPage page = new ZAjaxPage())
			{
				ZAjaxClientScriptManagerForTest manager = new ZAjaxClientScriptManagerForTest(page);

				manager.SetIsAsyncPostBack(false);
				Assert("IsClientScriptIncludeRegistered: for synchronous postback the functionality from base ZClientScriptManager is used", !manager.IsClientScriptIncludeRegistered("Foo"));
				manager.RegisterClientScriptInclude("Foo", "somescript.js");
				Assert("RegisterClientScriptInclude: for synchronous postback the functionality from base ZClientScriptManager is used and a script was registered", manager.IsClientScriptIncludeRegistered("Foo"));

				manager.SetIsAsyncPostBack(true);
				manager.RegisterClientScriptInclude("Bar", "somescript.js");
				Assert("For asynchronous postback IsClientScriptIncludeRegistered always returns false because AJAX ScriptManager does not provide a method to check whether a script include is registered", !manager.IsClientScriptIncludeRegistered("Bar"));
			}
		}

		class ZAjaxClientScriptManagerForTest : ZAjaxClientScriptManager
		{
			public ZAjaxClientScriptManagerForTest(ZAjaxPage page)
				: base(page)
			{
			}

			protected override bool IsAsyncPostBack
			{
				get { return isAsyncPostBack; }
			}
			bool isAsyncPostBack;

			public void SetIsAsyncPostBack(bool value)
			{
				isAsyncPostBack = value;
			}
		}

		#endregion
	}
}
