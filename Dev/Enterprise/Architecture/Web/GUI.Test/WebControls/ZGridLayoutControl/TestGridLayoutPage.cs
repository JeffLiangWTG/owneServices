using System;
using System.Collections.Specialized;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class TestGridLayoutPage : GridLayoutPage
	{
		public TestGridLayoutPage() : base()
		{
		}

		public new void OnInit(EventArgs e)
		{
			AvailableColumnsListBox = new ZListBox();
			CurrentLayoutListBox = new ZListBox();

			SelectOneButton = new System.Web.UI.WebControls.Button();
			RemoveOneButton = new System.Web.UI.WebControls.Button();
			MoveUpButton = new System.Web.UI.WebControls.Button();
			MoveDownButton = new System.Web.UI.WebControls.Button();
			DefaultButton = new System.Web.UI.WebControls.Button();

			FormControl.Controls.Add(AvailableColumnsListBox);
			FormControl.Controls.Add(CurrentLayoutListBox);
			FormControl.Controls.Add(SelectOneButton);
			FormControl.Controls.Add(RemoveOneButton);
			FormControl.Controls.Add(MoveUpButton);
			FormControl.Controls.Add(MoveDownButton);
			FormControl.Controls.Add(DefaultButton);

			base.OnInit(e);
			EnsureChildControls();
		}

		public new void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
		}

		protected override HtmlForm GetForm(Control parent)
		{
			ZTestForm newForm = new ZTestForm();
			newForm.Method = "GET";
			newForm.Target = "Test.aspx";
			parent.Controls.Add(newForm);
			return newForm;
		}

		protected override Uri RequestUrl
		{
			get { return new Uri("http://www.test.com/WebTracker/"); }
		}

		protected override BrowserType GetBrowserType()
		{
			return BrowserType.IE;
		}

		protected override NameValueCollection RequestQueryString
		{
			get
			{
				if (fRequestQueryString == null)
				{
					fRequestQueryString = new NameValueCollection();
					fRequestQueryString.Add(ZIFramePage.OKFunctionQuery, "ZTextPopup_SetValueAndHidePopup");
					fRequestQueryString.Add(ZIFramePage.CancelFunctionQuery, "ZTextPopup_HidePopup");
					fRequestQueryString.Add(ZIFramePage.ControlIDQuery, "TestClientID");
					fRequestQueryString.Add(ZGridLayoutControl.GridModuleIDKey, WebModuleIDs.Dummy.ToString());
					fRequestQueryString.Add(ZGridLayoutControl.GridCurrentLayoutKey, "0");
				}
				return fRequestQueryString;
			}
		}
		NameValueCollection fRequestQueryString;
	}
}
