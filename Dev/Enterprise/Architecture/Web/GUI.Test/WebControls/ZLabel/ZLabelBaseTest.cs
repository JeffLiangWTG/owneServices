using System.Web.Security.AntiXss;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public abstract class ZLabelBaseTest : ZLabelBaseTestBase
	{
		public virtual void TestBindToStringProperty()
		{
			ZLabelTestO.BindTo = "Z0_Description";
			ZLabelTestO.Bind(TestBizO);
			AssertEquals(ZLabelTestO.Text, TestBizO.Z0_Description);
		}

		public void TestBindToDataProperty()
		{
			ZLabelTestO.BindTo = "Z0_Date";
			ZLabelTestO.Bind(TestBizO);
			AssertEquals(ZLabelTestO.Text, TestBizO.Z0_Date.ToString());
		}

		public virtual void TestBindToEmptyStringProperty()
		{
			TestBizO.Z0_Description = "";
			ZLabelTestO.BindTo = "Z0_Description";
			ZLabelTestO.Bind(TestBizO);
			AssertEquals("", ZLabelTestO.Text);
		}

		public virtual void TestReBindModifiedText()
		{
			ZLabelTestO.BindTo = "Z0_Description";
			ZLabelTestO.Bind(TestBizO);
			AssertEquals(TestBizO.Z0_Description, ZLabelTestO.Text);

			string testString = "Modified Description Text";
			Assert(!TestBizO.Z0_Description.Equals(testString));
			ZLabelTestO.Text = testString;
			ZLabelTestO.Bind(TestBizO);
			AssertEquals(TestBizO.Z0_Description, ZLabelTestO.Text);
		}

		public virtual void TestUnBind()
		{
			ZLabelTestO.BindTo = "Z0_Description";
			ZLabelTestO.Bind(TestBizO);
			AssertEquals(ZLabelTestO.Text, TestBizO.Z0_Description);

			ZLabelTestO.UnBind();
			AssertEquals("", ZLabelTestO.Text);
			AssertEquals(null, ZLabelTestO.BindTo);
		}

		public virtual void TestDoNotEncodeHtmlTextWhenDisabled()
		{
			TestBizO.Z0_Code = "<tst>";

			ZLabelTestO.EnableHtmlEncoding = false;
			ZLabelTestO.BindTo = "Z0_Code";
			ZLabelTestO.Bind(TestBizO);

			AssertEquals("<tst>", ZLabelTestO.Text);
		}

		public virtual void TestShouldEncodeTextToAvoidCrossSiteScripting()
		{
			ZLabelTestO.Text = "<svg/onload=alert(document.domain)><img/src=x/onerror=alert(document.domain)><script>alert(document.domain)</script>";
			AssertEquals(AntiXssEncoder.HtmlEncode("<svg/onload=alert(document.domain)><img/src=x/onerror=alert(document.domain)><script>alert(document.domain)</script>", false), ZLabelTestO.Text);
		}
	}
}
