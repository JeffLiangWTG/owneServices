#if DEBUG
using System;
using System.Collections.Specialized;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.Testing
{
	public class ZTestPage : ZPage
	{
		public ZTestPage() : base()
		{
			SetHeadersAndStyle();
		}

		protected override void FrameworkInitialize()
		{
			if (OnTestingDbDisposableConnection != null)
			{
				OnTestingDbDisposableConnection();
			}

			base.FrameworkInitialize();
		}

		protected override void Render(HtmlTextWriter writer)
		{
			if (OnTestingDbDisposableConnection != null)
			{
				return;
			}

			base.Render(writer);
		}

		public Action OnTestingDbDisposableConnection;

		public new HtmlForm FormControl => base.FormControl;

		protected override HtmlForm GetForm(Control parent)
		{
			var newForm = new ZTestForm();
			newForm.Method = "GET";
			newForm.Target = "Test.aspx";
			parent.Controls.Add(newForm);

			return newForm;
		}

		public virtual BusinessObject TestDataSource { get; set; }

		protected override BusinessObject GetNewDataSource() => TestDataSource;

		public void OnLoad() => base.OnLoad(EventArgs.Empty);

		public void CallOnLoadComplete() => OnLoadComplete(EventArgs.Empty);

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			if (DataSource != null)
			{
				new ZWebControlBinder(DataSource).Bind(Controls);
			}
		}

		public void PrepareForRendering() => EnsureChildControls();

		protected override void CreateChildControls()
		{
			base.CreateChildControls();
			Controls.Add(new HtmlGenericControl(nameof(HtmlTextWriterTag.Head)));
		}

		public void OnPreRenderForTesting() => OnPreRender(EventArgs.Empty);

		public HtmlGenericControl NotificationArea => base.NotificationsArea;

		protected override ZGlobal GetNewTestGlobal() => new ZTestGlobal();

		protected override Uri RequestUrl => new Uri("http://www.test.com/WebTracker/");

		protected override BrowserType GetBrowserType() => BrowserType.IE;

		protected override NameValueCollection RequestQueryString => requestQueryString ?? (requestQueryString = new NameValueCollection());

		NameValueCollection requestQueryString;

		protected override bool ShowLoginStatus => false;

		public void SetSiteUser(WebUser user)
		{
			var testAppInstance = (ZTestGlobal)AppInstance;
			testAppInstance.SetSiteUser(user);
		}
	}
}

#endif
