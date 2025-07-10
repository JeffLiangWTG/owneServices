using System;
using System.Collections.Specialized;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class TestZIFramePageWithFilterStripBizO : ZIFramePageWithFilterStripBizO
	{
		public TestZIFramePageWithFilterStripBizO(string queryStringKey)
		{
			this.queryStringKey = queryStringKey;

			testQueryString = new NameValueCollection();
			testQueryString.Add("OKFunction", "TestOKFunctionName");
			testQueryString.Add("CancelFunction", "TestCancelFunctionName");
			testQueryString.Add("ControlID", "TestControlID");
			testQueryString.Add("CallerPK", "40CE87D3-85BC-4BC9-B972-EB8452D566A8");
		}
		readonly string queryStringKey;
		readonly NameValueCollection testQueryString;

		public void OnInit() => base.OnInit(EventArgs.Empty);

		public override string QueryStringKey => queryStringKey;

		protected override string[] OKFunctionArguments => throw new NotImplementedException();

		protected override string[] CancelFunctionArguments => throw new NotImplementedException();

		protected override Uri RequestUrl => new Uri("http://www.test.com/WebTracker/");

		protected override BrowserType GetBrowserType() => BrowserType.IE;

		protected override NameValueCollection RequestQueryString => testQueryString;
	}
}
