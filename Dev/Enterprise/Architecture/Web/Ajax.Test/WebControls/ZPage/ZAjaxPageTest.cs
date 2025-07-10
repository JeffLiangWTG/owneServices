using System;
using System.Collections;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	class ZAjaxPageForTesting : ZAjaxPage
	{
		protected override HtmlForm GetForm(Control parent)
		{
			return new HtmlForm();
		}
		internal ZWebResource AjaxSearchControlResourceForTesting => AjaxSearchControlResource;
		internal void OnPreInitForTesting(EventArgs e) => OnPreInit(e);
		internal HtmlForm FormControlForTesting => FormControl;
	}

	sealed class ZAjaxPageTest : WebControlTest
	{
		#region Resources

		public void TestResources()
		{
			bool ajaxSearchControlResourceFound = false;

			foreach (ZWebResource resource in TestPage.Resources)
			{
				if (resource == TestPage.AjaxSearchControlResourceForTesting)
				{
					ajaxSearchControlResourceFound = true;
					continue;
				}
			}
			Assert("AjaxSearchControl control file should be included into Resources collection", ajaxSearchControlResourceFound);
		}

		#endregion

		#region Test AJAX

		public void TestAJAXEnabled()
		{
			Assert("AJAX should be enabled", TestPage.IsAJAXEnabled);
		}

		public void TestAJAXManager()
		{
			AssertNotNull("Precondition: Form control should exist", TestPage.FormControlForTesting);

			if (TestPage.IsAJAXEnabled)
			{
				TestPage.OnPreInitForTesting(EventArgs.Empty);
				AssertNotNull("AJAX Manager should exist", TestPage.AJAX);

				bool ajaxManagerIsAdded = false;
				foreach (Control control in TestPage.FormControlForTesting.Controls)
				{
					if (control is AJAXManager)
					{
						ajaxManagerIsAdded = true;
						break;
					}
				}
				Assert("AJAX Manager should be added on the form", ajaxManagerIsAdded);
			}
		}

		public void TestAjaxClientScriptManager()
		{
			Assert("ClientScript should be of ZAjaxClientScriptManager type", TestPage.ZClientScript is ZAjaxClientScriptManager);
		}

		public void TestUpdatePanelRedirect()
		{
			var clientScript = TestPage.ClientScriptInternal;
			var clientScriptBlocks = (ArrayList)clientScript.GetType().InvokeMember("_clientScriptBlocks", System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, clientScript, null);

			AssertNull("No script blocks", clientScriptBlocks);

			TestPage.UpdatePanelRedirect("test", "test.html");
			clientScriptBlocks = (ArrayList)clientScript.GetType().InvokeMember("_clientScriptBlocks", System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, clientScript, null);
			AssertEquals("One script in Script Blocks", 1, clientScriptBlocks.Count);
		}

		#endregion

		#region Implementation

		protected override Control GetNewControl()
		{
			return new ZAjaxPageForTesting();
		}

		ZAjaxPageForTesting TestPage
		{
			get
			{
				return Control as ZAjaxPageForTesting;
			}
		}

		#endregion
	}
}
