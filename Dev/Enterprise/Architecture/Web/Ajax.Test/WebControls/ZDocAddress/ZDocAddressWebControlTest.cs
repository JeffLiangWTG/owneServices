using System;
using System.Web.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.ServerServices;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZDocAddressWebControlTest : WebControlTest
	{
		#region Test Cases

		public void TestWebServiceMethods()
		{
			ZDocAddressWebControl testControl = new ZDocAddressWebControl();
			AssertNotNull(testControl.WebServiceMethods);
			AssertEquals(1, testControl.WebServiceMethods.Count);
			AssertEquals(typeof(PortFinderWebServiceMethod), testControl.WebServiceMethods[0].GetType());
		}

		[HttpContextEnabledTest]
		public void TestDeletedWebJobDocAddress_CreatesNew()
		{
			var testControl = new ZDocAddressWebControlForTest(true);
			testControl.OnInit();

			testControl.Bind(Factory.NewWithValidTestData<JobDocAddress>());

			testControl.WebJobDocAddressForTest.Delete();
			Assert("Precondition", testControl.WebJobDocAddressForTest.IsDeleted);

			testControl.Bind(Factory.NewWithValidTestData<JobDocAddress>());

			Assert("Should have created a new WebJobDocAddress", !testControl.WebJobDocAddressForTest.IsDeleted);
		}

		class ZDocAddressWebControlForTest : ZDocAddressWebControl
		{
			public ZDocAddressWebControlForTest(bool isPostBack = false)
			{
				Page = new ZPage { IsPostBack = isPostBack };
			}

			public void OnInit()
			{
				OnInit(EventArgs.Empty);
			}

			public WebJobDocAddress WebJobDocAddressForTest => WebJobDocAddress;
		}

		#endregion

		#region Implementation

		protected override Control GetNewControl()
		{
			return new ZDocAddressWebControl();
		}

		#endregion
	}
}
