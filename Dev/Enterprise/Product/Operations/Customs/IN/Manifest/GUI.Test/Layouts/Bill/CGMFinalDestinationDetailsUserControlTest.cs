using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IN.Manifest.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.GUI.Testing;

[TestedType(typeof(CGMFinalDestinationDetailsUserControl))]
sealed class CGMFinalDestinationDetailsUserControlTest : TestCaseWithFactory
{
	public void TestDataSourceType()
	{
		using (var control = new CGMFinalDestinationDetailsUserControl())
		{
			AssertEquals("DataSourceType", typeof(CGMAsycudaBill), control.DataSourceType);
		}
	}

	public void TestCustomsFinalDestinationPortControlVisible()
	{
		var header = Factory.New<CGMAsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		bill.ABL_LocationInformation = DestinationCodeList.Codes.CUS;
		using (var form = new ZForm())
		using (var control = new CGMFinalDestinationDetailsUserControl())
		{
			control.SetDataBinding(bill, "");
			form.Controls.Add(control);
			form.Show();
			var customsFinalDestinationPortCodeFindBox = control.CustomsFinalDestinationPortCodeFindBox;
			var customsFinalDestinationPortTextBox = control.CustomsFinalDestinationPortTextBox;
			control.ChangeVisibility(bill);
			Assert("customsFinalDestinationPortCodeFindBox visible when CUS", customsFinalDestinationPortCodeFindBox.Visible);
			Assert("customsFinalDestinationPortTextBox not visible when CUS", !customsFinalDestinationPortTextBox.Visible);
			bill.ABL_LocationInformation = DestinationCodeList.Codes.CFS;
			control.ChangeVisibility(bill);
			Assert("customsFinalDestinationPortCodeFindBox not visible when CFS", !customsFinalDestinationPortCodeFindBox.Visible);
			Assert("customsFinalDestinationPortTextBox visible when CFS", customsFinalDestinationPortTextBox.Visible);
		}
	}
}
