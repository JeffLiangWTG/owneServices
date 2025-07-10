using System;
using System.Collections.Specialized;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZTestFilterPage : ZFilterPage
	{
		public ZTestFilterPage() : base()
		{
		}

		public new void OnInit(EventArgs e)
		{
			base.OnInit(e);
			EnsureChildControls();
			SearchControl?.Initialise();

			Details = new ZTextBox();
			Details.BindTo = "Z0_Description";
			DetailsList = new ZDropDownList();
			StartsWithRadioButton = new ZRadioButton();
			ContainsRadioButton = new ZRadioButton();

			FormControl.Controls.Add(Details);
			FormControl.Controls.Add(DetailsList);
			FormControl.Controls.Add(StartsWithRadioButton);
			FormControl.Controls.Add(ContainsRadioButton);
			FormControl.Controls.Add(ButtonsContainer);
		}

		public new void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
		}

		public ZDataGrid FilterDataGrid
		{
			get { return SearchControl?.SearchResultsDataGrid; }
		}

		public HtmlGenericControl ResultsGridDiv
		{
			get { return SearchControl?.ResultsGridDiv; }
		}

		public void FindButton_Click(object sender, EventArgs e)
		{
			base.SearchControl?.FindButton_Click(sender, e);
		}

		protected override SearchControl GetNewSearchControl()
		{
			return new TestSearchControl() { Page = this };
		}

		protected override HtmlForm GetForm(Control parent)
		{
			var newForm = new ZTestForm();
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
				}
				return fRequestQueryString;
			}
		}
		NameValueCollection fRequestQueryString;

		public new TestSearchControl SearchControl
		{
			get { return base.SearchControl as TestSearchControl; }
		}
	}
}
