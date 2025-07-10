using System;
using System.Web.UI;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	class ZOrgAddressControlForTesting : ZOrgAddressControl
	{
		internal ZAutoCompleteTextBoxWithButton AddressTextBoxForTesting => AddressTextBox;
		internal void OnInitForTesting(EventArgs e) => OnInit(e);
	}

	sealed class ZOrgAddressControlTest : WebControlTest
	{
		#region Test Cases

		public void TestAddressModuleID()
		{
			Control.AddressModuleID = WebModuleIDs.OrgAddress;
			Control.OnInitForTesting(EventArgs.Empty);

			AssertEquals(WebModuleIDs.OrgAddress, Control.AddressTextBoxForTesting.ModuleID);

			Control.AddressModuleID = WebModuleIDs.OrgAddressReceivablesTracking;
			Control.OnInitForTesting(EventArgs.Empty);

			AssertEquals(WebModuleIDs.OrgAddressReceivablesTracking, Control.AddressTextBoxForTesting.ModuleID);
		}

		public void TestAddressTextBoxHelper()
		{
			Control.AddressModuleID = WebModuleIDs.OrgAddress;
			Control.OnInitForTesting(EventArgs.Empty);

			AssertType<OrgAddressAutoCompleteHelper>(Control.AddressTextBoxForTesting.Helper);

			Control.AddressModuleID = WebModuleIDs.OrgAddressReceivablesTracking;
			Control.OnInitForTesting(EventArgs.Empty);

			AssertType<OrgAddressReceivablesAutoCompleteHelper>(Control.AddressTextBoxForTesting.Helper);
		}

		#endregion

		#region Implementation

		protected override Control GetNewControl() => new ZOrgAddressControlForTesting();

		new ZOrgAddressControlForTesting Control => (ZOrgAddressControlForTesting)base.Control;

		#endregion
	}
}
