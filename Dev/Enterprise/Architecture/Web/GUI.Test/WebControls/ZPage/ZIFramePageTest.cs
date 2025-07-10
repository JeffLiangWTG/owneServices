using System;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public abstract class ZIFramePageTest : ZPageTest
	{
		#region Script Tests

		public void TestRegisterClientScriptCaller()
		{
			string testFunc = "TestFunc";
			string[] testArgs = new string[] { "arg1", "arg2" };
			AssertEquals("PreCondition: Script should not be registered", false, TestIFramePage.ZClientScript.IsClientScriptBlockRegistered(TestIFramePage.GetType(), "ZIFramePage_ClientScriptCaller"));
			TestIFramePage.RegisterClientScriptCaller(testFunc, testArgs);

			AssertEquals("Script should be registered", true, TestIFramePage.ZClientScript.IsClientScriptBlockRegistered(TestIFramePage.GetType(), "ZIFramePage_ClientScriptCaller"));
		}

		public virtual void TestOKButtonClick()
		{
			AssertEquals("PreCondition: Script should not be registered", false, TestIFramePage.ZClientScript.IsClientScriptBlockRegistered(TestIFramePage.GetType(), "ZIFramePage_ClientScriptCaller"));

			NameValueCollection testQueryString = new NameValueCollection();
			testQueryString.Add("OKFunction", "TestOKFunctionName");
			testQueryString.Add("CancelFunction", "TestCancelFunctionName");
			testQueryString.Add("ControlID", "TestControlID");
			TestIFramePage.CheckForRequiredQueriesInternal(testQueryString);

			AssertEquals("OKFunction", "TestOKFunctionName", TestIFramePage.OKFunctionName);
			AssertEquals("CancelFunction", "TestCancelFunctionName", TestIFramePage.CancelFunctionName);
			AssertEquals("ControlID", "TestControlID", TestIFramePage.ParentControlID);

			TestIFramePage.OKButton_Click(this, EventArgs.Empty);
			AssertEquals("Script should be registered", true, TestIFramePage.ZClientScript.IsClientScriptBlockRegistered(TestIFramePage.GetType(), "ZIFramePage_ClientScriptCaller"));
		}

		public void TestQuerystringFunctionNameValuesAreEncoded()
		{
			var testQueryString = new NameValueCollection
				{
					{ "OKFunction", "TestOKFunctionName){}%3balert(\"ok\");%2f%2f" },
					{ "CancelFunction", "TestCancelFunctionName){}%3balert(\"cancel\");%2f%2f" },
					{ "ControlID", "TestControlID" }
				};

			TestIFramePage.CheckForRequiredQueriesInternal(testQueryString);

			AssertEquals("OKFunction", "TestOKFunctionName){}%3balert(&quot;ok&quot;);%2f%2f", TestIFramePage.OKFunctionName);
			AssertEquals("CancelFunction", "TestCancelFunctionName){}%3balert(&quot;cancel&quot;);%2f%2f", TestIFramePage.CancelFunctionName);
		}

		protected virtual string ExpectedScriptArgs
		{
			get { return fExpectedScriptArgs; }
			set { fExpectedScriptArgs = value; }
		}
		string fExpectedScriptArgs;

		#endregion Script Tests

		#region Control Tests

		public virtual void TestOKButton()
		{
			StringBuilder expectedBuilder = new StringBuilder();
			StringWriter expectedWriter = new StringWriter(expectedBuilder);
			ExpOKButton.RenderControl(new HtmlTextWriter(expectedWriter));

			StringBuilder controlBuilder = new StringBuilder();
			StringWriter controlWriter = new StringWriter(controlBuilder);

			typeof(Control).GetProperty("ControlState", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(TestIFramePage, 6, null);
			HtmlTextWriter htmlTextWriter = new HtmlTextWriter(controlWriter);
			typeof(Page).InvokeMember("_inOnFormRender", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField, null, TestIFramePage, new object[] { true });
			TestIFramePage.OKButton.RenderControl(htmlTextWriter);
			AssertEquals(expectedBuilder.ToString(), controlBuilder.ToString());
		}

		public virtual void TestCancelButton()
		{
			StringBuilder expectedBuilder = new StringBuilder();
			StringWriter expectedWriter = new StringWriter(expectedBuilder);
			ExpCancelButton.RenderControl(new HtmlTextWriter(expectedWriter));

			StringBuilder controlBuilder = new StringBuilder();
			StringWriter controlWriter = new StringWriter(controlBuilder);

			typeof(Control).GetProperty("ControlState", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(TestIFramePage, 6, null);
			HtmlTextWriter htmlTextWriter = new HtmlTextWriter(controlWriter);
			typeof(Page).InvokeMember("_inOnFormRender", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField, null, TestIFramePage, new object[] { true });
			TestIFramePage.CancelButton.RenderControl(htmlTextWriter);
			AssertEquals(expectedBuilder.ToString(), controlBuilder.ToString());
		}

		public virtual void TestButtonsContainer()
		{
			StringBuilder expectedBuilder = new StringBuilder();
			StringWriter expectedWriter = new StringWriter(expectedBuilder);
			ExpButtonsContainer.RenderControl(new HtmlTextWriter(expectedWriter));

			StringBuilder controlBuilder = new StringBuilder();
			StringWriter controlWriter = new StringWriter(controlBuilder);
			TestIFramePage.EnsureChildControlsInternal();
			typeof(Control).GetProperty("ControlState", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(TestIFramePage, 6, null);
			typeof(Page).InvokeMember("_inOnFormRender", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField, null, TestIFramePage, new object[] { true });
			TestIFramePage.ButtonsContainerInternal.RenderControl(new HtmlTextWriter(controlWriter));
			AssertMultilineASCIIEquals("Expected Output", expectedBuilder.ToString(), controlBuilder.ToString());
		}

		#endregion Control Tests

		#region QueryString Tests

		[HttpContextEnabledTest]
		public void TestCheckForRequiredQueries_OK()
		{
			var testQueryString = new NameValueCollection();
			AssertInvalidQueryStringRedirects("OKFunction missing", testQueryString);
		}

		[HttpContextEnabledTest]
		public void TestCheckForRequiredQueries_Cancel()
		{
			var testQueryString = new NameValueCollection
				{
					{ "OKFunction", "TestOKFunctionName" }
				};

			AssertInvalidQueryStringRedirects("CancelFunction missing", testQueryString);
		}

		[HttpContextEnabledTest]
		public void TestCheckForRequiredQueries_ControlID()
		{
			var testQueryString = new NameValueCollection
				{
					{ "OKFunction", "TestOKFunctionName" },
					{ "CancelFunction", "TestCancelFunctionName" }
				};

			AssertInvalidQueryStringRedirects("ControlID missing", testQueryString);
		}

		[HttpContextEnabledTest]
		public void TestCheckForRequiredQueries_CallerPKInvalid()
		{
			var testQueryString = new NameValueCollection
				{
					{ "OKFunction", "TestOKFunctionName" },
					{ "CancelFunction", "TestCancelFunctionName" },
					{ "ControlID", "TestControlID" },
					{ "CallerPK", "NotAZGuid" }
				};

			AssertInvalidQueryStringRedirects("CallerPK Not a ZGuid", testQueryString);
		}

		[HttpContextEnabledTest]
		public void TestCheckForRequiredQueries_Success()
		{
			var testQueryString = new NameValueCollection
				{
					{ "OKFunction", "TestOKFunctionName" },
					{ "CancelFunction", "TestCancelFunctionName" },
					{ "ControlID", "TestControlID" }
				};

			TestIFramePage.CheckForRequiredQueriesInternal(testQueryString);

			AssertEquals("OKFunction", "TestOKFunctionName", TestIFramePage.OKFunctionName);
			AssertEquals("CancelFunction", "TestCancelFunctionName", TestIFramePage.CancelFunctionName);
			AssertEquals("ControlID", "TestControlID", TestIFramePage.ParentControlID);
			Assert(!HttpContext.Current.Response.IsRequestBeingRedirected);
		}

		protected void AssertInvalidQueryStringRedirects(string message, NameValueCollection testQueryString)
		{
			TestIFramePage.CheckForRequiredQueriesInternal(testQueryString);

			Assert(message, HttpContext.Current.Response.IsRequestBeingRedirected);
			AssertEquals($"{TestIFramePage.AppInstance.ErrorPage}?invalidQuery=true", HttpContext.Current.Response.RedirectLocation);
		}

		#endregion QueryString Tests

		#region FunctionArgument Tests

		public virtual void TestOKFunctionArguments()
		{
			AssertNotNull(TestIFramePage.OKFunctionArgumentsInternal);
		}

		public virtual void TestCancelFunctionArguments()
		{
			AssertNotNull(TestIFramePage.CancelFunctionArgumentsInternal);
		}

		#endregion FunctionArgument Tests

		#region Overrides

		public override void TestGetNewDataSource()
		{
			BusinessObject dataSource = TestIFramePage.GetNewDataSourceInternal();
			AssertNull("ZIFramePage DataSource should be null. Override in derived pages", dataSource);
		}

		#endregion Overrides

		#region Implementation

		#region Expected Controls

		protected virtual Panel ExpButtonsContainer
		{
			get
			{
				if (fButtonsContainer == null)
				{
					fButtonsContainer = new Panel();
					fButtonsContainer.HorizontalAlign = HorizontalAlign.Right;
					fButtonsContainer.Style[HtmlTextWriterStyle.VerticalAlign] = nameof(VerticalAlign.Bottom);

					foreach (Control ctl in ContainerControls)
					{
						fButtonsContainer.Controls.Add(ctl);
					}
				}
				return fButtonsContainer;
			}
		}
		Panel fButtonsContainer;

		protected Button ExpOKButton
		{
			get
			{
				if (fOKButton == null)
				{
					fOKButton = new Button();
					fOKButton.ID = "OK";
					fOKButton.Text = GetExpOKButtonText();
					fOKButton.Style[HtmlTextWriterStyle.Width] = GetExpOKButtonWidth();
					fOKButton.Style[HtmlTextWriterStyle.MarginRight] = "8px";
					fOKButton.Style[HtmlTextWriterStyle.MarginBottom] = "6px";
					fOKButton.Visible = OKButtonIsVisible;
					fOKButton.Click += new EventHandler(TestIFramePage.OKButton_Click);
					fOKButton.Enabled = OKButtonIsEnabled;
				}
				return fOKButton;
			}
		}
		Button fOKButton;

		protected virtual string GetExpOKButtonText()
		{
			return "OK";
		}

		protected virtual string GetExpOKButtonWidth()
		{
			return "100px";
		}

		protected Button ExpCancelButton
		{
			get
			{
				if (fCancelButton == null)
				{
					fCancelButton = new Button();
					fCancelButton.ID = "Cancel";
					fCancelButton.Text = "Cancel";
					fCancelButton.Style[HtmlTextWriterStyle.Width] = "100px";
					fCancelButton.Style[HtmlTextWriterStyle.MarginRight] = "8px";
					fCancelButton.Style[HtmlTextWriterStyle.MarginBottom] = "6px";
					fCancelButton.Visible = CancelButtonIsVisible;
					fCancelButton.Click += new EventHandler(TestIFramePage.CancelButton_Click_Internal);
				}
				return fCancelButton;
			}
		}
		Button fCancelButton;

		protected virtual Control[] ContainerControls
		{
			get { return new Control[] { ExpOKButton, ExpCancelButton }; }
		}

		protected virtual bool OKButtonIsVisible
		{
			get { return false; }
		}

		protected virtual bool OKButtonIsEnabled => true;

		protected virtual bool CancelButtonIsVisible
		{
			get { return true; }
		}
		#endregion

		protected ZIFramePage TestIFramePage
		{
			get { return (ZIFramePage)Control; }
		}

		#endregion Implementation
	}
}
