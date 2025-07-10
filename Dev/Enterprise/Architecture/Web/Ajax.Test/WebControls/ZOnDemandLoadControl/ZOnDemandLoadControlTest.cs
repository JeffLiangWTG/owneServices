using System.Collections.Specialized;
using System.Reflection;
using System.Web;
using System.Web.UI;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	[HttpContextEnabledTest]
	sealed class ZOnDemandLoadControlTest : WebControlTest
	{
		#region Test Cases

		public void TestIsCollapsedDefault()
		{
			var testControl = (ZOnDemandLoadControl)Control;
			Assert("is collapsed by default", testControl.IsCollapsed);
		}

		public void TestIsCollapsedIfHasValue()
		{
			var testControl = (ZOnDemandLoadControl)Control;
			testControl.IsCollapsed = true;
			AssertEquals(true, testControl.IsCollapsed);

			testControl.IsCollapsed = false;
			AssertEquals(false, testControl.IsCollapsed);
		}

		public void TestIsCollapsedWhenDisabled()
		{
			var testControl = (ZOnDemandLoadControl)Control;
			testControl.IsCollapsingDisabled = true;
			AssertEquals(false, testControl.IsCollapsed);
		}

		public void TestIsCollapsedState()
		{
			var testControl = (ZOnDemandLoadControl)Control;

			var isReadOnlyProperty = typeof(NameValueCollection).GetProperty("IsReadOnly", BindingFlags.NonPublic | BindingFlags.Instance);
			isReadOnlyProperty.SetValue(HttpContext.Current.Request.Params, false, null);

			HttpContext.Current.Request.Params["State"] = "true";
			AssertEquals(true, testControl.IsCollapsed);

			HttpContext.Current.Request.Params["State"] = "false";
			AssertEquals(false, testControl.IsCollapsed);

			HttpContext.Current.Request.Params["State"] = "True";
			AssertEquals(true, testControl.IsCollapsed);

			HttpContext.Current.Request.Params["State"] = "False";
			AssertEquals(false, testControl.IsCollapsed);

			HttpContext.Current.Request.Params["State"] = "InvalidBool";
			AssertEquals(true, testControl.IsCollapsed);
		}

		#endregion

		#region Implementation

		protected override Control GetNewControl()
		{
			return new ZOnDemandLoadControl();
		}

		protected override ZTestPage GetNewZTestPage()
		{
			return base.GetNewZTestPage();
		}

		#endregion
	}
}
