using System;
using System.Web.UI;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZHyperlinkTest : WebControlTest
	{
		protected override Control GetNewControl()
		{
			return new ZHyperlink();
		}

		ZHyperlink TestHyperlink
		{
			get { return (ZHyperlink)Control; }
		}

		#region Static text and static Url

		public void TestStaticTextStaticUrl()
		{
			TestHyperlink.Text = "TestLink";
			TestHyperlink.NavigateUrl = "http://www.edi.com.au";
			TestHyperlink.Bind(TestBizO);
			AssertEquals("Text", "TestLink", TestHyperlink.Text);
			AssertEquals("Navigate Url", "http://www.edi.com.au", TestHyperlink.NavigateUrl);
		}

		#endregion

		#region Static text and dynamic Url

		public void TestStaticTextDynamicUrl()
		{
			TestHyperlink.Text = "TestLink";
			TestHyperlink.DataNavigateUrlFormatString = "http://www.edi.com.au?Ref={0}";
			TestHyperlink.DataNavigateUrlFields = new string[1] { "Z0_Description" };
			TestHyperlink.Bind(TestBizO);
			AssertEquals("Text", "TestLink", TestHyperlink.Text);
			AssertEquals("Navigate Url", String.Format("http://www.edi.com.au?Ref={0}", TestBizO.Z0_Description), TestHyperlink.NavigateUrl);
		}

		#endregion

		#region Dynamic text and static Url

		public void TestDynamicTextStaticUrlBindToOnly()
		{
			TestHyperlink.Text = "TestLink";
			TestHyperlink.BindTo = "Z0_Description";
			TestHyperlink.NavigateUrl = "http://www.edi.com.au";

			TestHyperlink.Bind(TestBizO);
			AssertEquals("Text", TestBizO.Z0_Description, TestHyperlink.Text);
			AssertEquals("Navigate Url", "http://www.edi.com.au", TestHyperlink.NavigateUrl);
		}

		public void TestDynamicTextStaticUrlNoDataTextFieldsSpecified()
		{
			TestHyperlink.Text = "TestLink";
			TestHyperlink.BindTo = "Z0_Description";
			TestHyperlink.NavigateUrl = "http://www.edi.com.au";
			TestHyperlink.DataTextFormatString = "Hello {0} Hello";

			TestHyperlink.Bind(TestBizO);
			AssertEquals("Text", "Hello " + TestBizO.Z0_Description + " Hello", TestHyperlink.Text);
			AssertEquals("Navigate Url", "http://www.edi.com.au", TestHyperlink.NavigateUrl);
		}

		public void TestDynamicTextStaticUrlWithDataTextFieldsSpecified()
		{
			TestHyperlink.Text = "TestLink";
			TestHyperlink.BindTo = "Z0_Description";
			TestHyperlink.NavigateUrl = "http://www.edi.com.au";
			TestHyperlink.DataTextFormatString = "Hello {0} Hello {1}";
			TestHyperlink.DataTextFields = new string[2] { "Z0_Description", "Z0_Code" };

			TestHyperlink.Bind(TestBizO);
			AssertEquals("Text", "Hello " + TestBizO.Z0_Description + " Hello " + TestBizO.Z0_Code, TestHyperlink.Text);
			AssertEquals("Navigate Url", "http://www.edi.com.au", TestHyperlink.NavigateUrl);
		}

		#endregion

		#region Dynamic text and dynamic Url

		public void TestDynamicTextDynamicUrlBindToOnly()
		{
			TestHyperlink.Text = "TestLink";
			TestHyperlink.BindTo = "Z0_Description";
			TestHyperlink.DataNavigateUrlFormatString = "http://www.edi.com.au?Ref={0}";

			TestHyperlink.Bind(TestBizO);
			AssertEquals("Text", TestBizO.Z0_Description, TestHyperlink.Text);
			AssertEquals("Navigate Url", String.Format("http://www.edi.com.au?Ref={0}", TestBizO.Z0_Description), TestHyperlink.NavigateUrl);
		}

		public void TestDynamicTextDynamicUrlBindToAndDataNavigateUrlFields()
		{
			TestHyperlink.Text = "TestLink";
			TestHyperlink.BindTo = "Z0_Description";
			TestHyperlink.DataNavigateUrlFormatString = "http://www.edi.com.au?Ref={0}&X={1}";
			TestHyperlink.DataNavigateUrlFields = new string[2] { "Z0_VarCharMax", "Z0_Code" };

			TestHyperlink.Bind(TestBizO);
			AssertEquals("Text", TestBizO.Z0_Description, TestHyperlink.Text);
			AssertEquals("Navigate Url", String.Format("http://www.edi.com.au?Ref={0}&X={1}", TestBizO.Z0_VarCharMax, TestBizO.Z0_Code), TestHyperlink.NavigateUrl);
		}

		public void TestDynamicTextDynamicUrlBindToAndLabelFormatting()
		{
			TestHyperlink.Text = "TestLink";
			TestHyperlink.BindTo = "Z0_Description";
			TestHyperlink.DataNavigateUrlFormatString = "http://www.edi.com.au?Ref={0}&X={1}";
			TestHyperlink.DataNavigateUrlFields = new string[2] { "Z0_VarCharMax", "Z0_Code" };
			TestHyperlink.DataTextFormatString = "Hello {0} Hello";

			TestHyperlink.Bind(TestBizO);
			AssertEquals("Text", "Hello " + TestBizO.Z0_Description + " Hello", TestHyperlink.Text);
			AssertEquals("Navigate Url", String.Format("http://www.edi.com.au?Ref={0}&X={1}", TestBizO.Z0_VarCharMax, TestBizO.Z0_Code), TestHyperlink.NavigateUrl);
		}

		public void TestDynamicTextDynamicUrlBothUrlAndLabelFormatted()
		{
			TestHyperlink.Text = "TestLink";
			TestHyperlink.BindTo = "Z0_Description";
			TestHyperlink.DataNavigateUrlFormatString = "http://www.edi.com.au?Ref={0}&X={1}";
			TestHyperlink.DataNavigateUrlFields = new string[2] { "Z0_VarCharMax", "Z0_Code" };
			TestHyperlink.DataTextFormatString = "Hello {0} Hello {1}";
			TestHyperlink.DataTextFields = new string[2] { "Z0_VarCharMax", "Z0_Code" };

			TestHyperlink.Bind(TestBizO);
			AssertEquals("Text", "Hello " + TestBizO.Z0_VarCharMax + " Hello " + TestBizO.Z0_Code, TestHyperlink.Text);
			AssertEquals("Navigate Url", String.Format("http://www.edi.com.au?Ref={0}&X={1}", TestBizO.Z0_VarCharMax, TestBizO.Z0_Code), TestHyperlink.NavigateUrl);
		}

		#endregion

		#region TestEncodeTextToAvoidCrossSiteScripting

		public void TestRenderEncodesTextToAvoidCrossSiteScripting()
		{
			TestBizO.Z0_Code = "<tst>";

			TestHyperlink.BindTo = "Z0_Code";
			TestHyperlink.Bind(TestBizO);

			AssertEquals("&lt;tst&gt;", TestHyperlink.Text);
		}

		public void TestDoNotEncodeHtmlTextWhenDisabled()
		{
			TestBizO.Z0_Code = "<tst>";

			TestHyperlink.EnableHtmlEncoding = false;
			TestHyperlink.BindTo = "Z0_Code";
			TestHyperlink.Bind(TestBizO);

			AssertEquals("<tst>", TestHyperlink.Text);
		}

		#endregion

		#region TestPrependingHttp

		public void TestPrependingHttpToNavigateUrlForInternalLinks()
		{
			AssertEquals("Precondition: IsExternalHyperLink should be false by default", false, TestHyperlink.IsExternalHyperLink);

			string url1 = "www.edi.com.au";
			string url2 = "blah.edi.com.au";
			string url3 = "http://www.edi.com.au";
			string url4 = "https://www.edi.com.au";
			string url5 = "ftp://edi.com.au";
			string url6 = "/tracking/blah.aspx";
			string url7 = "blah.aspx";
			string url8 = "DocumentRequestHandler.axd";

			AssertNavigateURL(url1, url1);
			AssertNavigateURL(url2, url2);
			AssertNavigateURL(url3, url3);
			AssertNavigateURL(url4, url4);
			AssertNavigateURL(url5, url5);
			AssertNavigateURL(url6, url6);
			AssertNavigateURL(url7, url7);
			AssertNavigateURL(url8, url8);
		}

		public void TestPrependingHttpToNavigateUrlForExternalLinks()
		{
			AssertEquals("Precondition: IsExternalHyperLink should be false by default", false, TestHyperlink.IsExternalHyperLink);
			TestHyperlink.IsExternalHyperLink = true;
			AssertEquals("Precondition: IsExternalHyperLink", true, TestHyperlink.IsExternalHyperLink);

			AssertNavigateURL("www.edi.com.au", "http://www.edi.com.au");
			AssertNavigateURL("blah.edi.com.au", "http://blah.edi.com.au");
			AssertNavigateURL("http://www.edi.com.au", "http://www.edi.com.au");
			AssertNavigateURL("https://www.edi.com.au", "https://www.edi.com.au");
			AssertNavigateURL("ftp://edi.com.au", "ftp://edi.com.au");
			AssertNavigateURL("/tracking/blah.aspx", "/tracking/blah.aspx");
		}

		void AssertNavigateURL(string originalURL, string expectedURL)
		{
			TestHyperlink.DataNavigateUrlFormatString = originalURL;
			TestHyperlink.Bind(TestBizO);
			AssertEquals("Tes Hyperlink's NavigateURL", expectedURL, TestHyperlink.NavigateUrl);
		}

		#endregion

		public void TestClientClickToOpenNewWindowWhenHasWindowStyleAndTarget()
		{
			TestHyperlink.DataNavigateUrlFormatString = "http://www.edi.com.au";
			Assert("WindowStyle was not set", string.IsNullOrEmpty(TestHyperlink.WindowStyle));
			Assert("Target was not set", string.IsNullOrEmpty(TestHyperlink.Target));

			TestHyperlink.Bind(TestBizO);
			AssertEquals("http://www.edi.com.au", TestHyperlink.NavigateUrl);
			AssertNull("onclick Attribute should not be set", TestHyperlink.Attributes["onclick"]);

			TestHyperlink.Target = "_blank";
			AssertEquals("Target", "_blank", TestHyperlink.Target);
			TestHyperlink.Bind(TestBizO);
			AssertEquals("http://www.edi.com.au", TestHyperlink.NavigateUrl);
			AssertNull("onclick Attribute should not be set", TestHyperlink.Attributes["onclick"]);

			TestHyperlink.WindowStyle = "status=1";
			AssertEquals("WindowStyle", "status=1", TestHyperlink.WindowStyle);
			TestHyperlink.Target = "";
			Assert("Target is empty", string.IsNullOrEmpty(TestHyperlink.Target));
			TestHyperlink.Bind(TestBizO);
			AssertEquals("http://www.edi.com.au", TestHyperlink.NavigateUrl);
			AssertNull("onclick Attribute should not be set", TestHyperlink.Attributes["onclick"]);

			TestHyperlink.Target = "_blank";
			AssertEquals("Target", "_blank", TestHyperlink.Target);
			AssertEquals("WindowStyle", "status=1", TestHyperlink.WindowStyle);
			TestHyperlink.Bind(TestBizO);
			AssertEquals("http://www.edi.com.au", TestHyperlink.NavigateUrl);
			AssertNotNull("onclick Attribute should be set", TestHyperlink.Attributes["onclick"]);
			AssertEquals("oclick", "javascript: window.open('http://www.edi.com.au','_blank','status=1'); return false;", TestHyperlink.Attributes["onclick"]);
		}

		#region ImageUrl

		public void TestStaticImageUrl()
		{
			TestHyperlink.Text = "TestLink";
			TestHyperlink.BindTo = "Z0_Description";
			TestHyperlink.ImageUrl = "TestImage.gif";
			TestHyperlink.DataTextFormatString = "Hello {0}";
			TestHyperlink.DataTextFields = new string[] { "Z0_VarCharMax" };

			TestHyperlink.Bind(TestBizO);
			AssertEquals("Text", "Hello " + TestBizO.Z0_VarCharMax, TestHyperlink.Text);
			AssertEquals("Image Url", "TestImage.gif", TestHyperlink.ImageUrl);
		}

		public void TestDynamicImageUrl()
		{
			TestHyperlink.Text = "TestLink";
			TestHyperlink.BindTo = "Z0_Description";
			TestHyperlink.ImageUrl = "TestImage.gif";
			TestHyperlink.DataImageUrlFormatString = "{0}.gif";
			TestHyperlink.DataImageUrlFields = new string[] { "Z0_VarCharMax" };
			TestHyperlink.DataTextFormatString = "Hello {0}";
			TestHyperlink.DataTextFields = new string[] { "Z0_VarCharMax" };

			TestHyperlink.Bind(TestBizO);
			AssertEquals("Text", "Hello " + TestBizO.Z0_VarCharMax, TestHyperlink.Text);
			AssertEquals("Image Url", String.Format("{0}.gif", TestBizO.Z0_VarCharMax), TestHyperlink.ImageUrl);
		}

		#endregion
	}
}
