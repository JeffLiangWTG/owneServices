using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.BE.GUI.PlugIn;

namespace Enterprise.Customs.BE.GUI.Testing;

sealed class ImportInvoiceLineOrganizationsUserControlTest : TestCaseWithFactory
{
	public void TestConsignorAddressControl()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Visible", true, control.ConsignorAddressControl.Visible);
			AssertEquals("CaptionResourceString", ResourceStringData.Empty, control.ConsignorAddressControl.CaptionResourceString);
			AssertEquals("BindToOrgList", "FilteredInvoiceLines.Lookups+ExporterList", control.ConsignorAddressControl.BindToOrgList);
		});
	}

	public void TestConsigneeAddressControlNotVisible()
	{
		AssertEquals(false, control.ConsigneeAddressControl.Visible);
	}

	public void TestBuyerDocAddressControlLocation()
	{
		AssertEquals(ControlDpiScalingHelper.NewScaledPoint(65, 43, true), control.BuyerDocAddressControl.Location);
	}

	public void TestSellerDocAddressControlLocation()
	{
		AssertEquals(ControlDpiScalingHelper.NewScaledPoint(65, 68, true), control.SellerDocAddressControl.Location);
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new ImportInvoiceLineOrganizationsUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
	ImportInvoiceLineOrganizationsUserControl control;
}
