using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.JP.Manifest.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.UserControls.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.GUI.Testing
{
	[TestedType(typeof(JPManifestBillPartiesSpecificUserControl))]
	sealed class JPManifestBillPartiesSpecificUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using var control = new JPManifestBillPartiesSpecificUserControl();
			TestHelper.AssertControlExists(control, "ConsigneeRegNoPanel", "");
			TestHelper.AssertControlExists(control, "ConsigneeRegNumTextBox", "ABL_ConsigneeRegNo");
			TestHelper.AssertControlExists(control, "ConsigneeRegTypeDropEdit", "ABL_ConsigneeRegNoType");

			TestHelper.AssertControlExists(control, "ShipperRegNoPanel", "");
			TestHelper.AssertControlExists(control, "ShipperRegNumTextBox", "ABL_ShipperRegNo");
			TestHelper.AssertControlExists(control, "ShipperRegTypeDropEdit", "ABL_ShipperRegNoType");

			TestHelper.AssertControlExists(control, "NotifyPartyRegNoPanel", "");
			TestHelper.AssertControlExists(control, "NotifyPartyRegNumTextBox", "ABL_NotifyPartyRegNo");
			TestHelper.AssertControlExists(control, "NotifyPartyRegTypeDropEdit", "ABL_NotifyPartyRegNoType");
		}

		public void TestTariffFindBoxSetting()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			var bill = header.Bills.AddNew();

			using var form = new ManifestForm(header);
			form.Show();
			var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
			var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
			var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
			mainTabControl.SelectedTab = billsAndPacksTabPage;
			var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
			var billsTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsTabPage");
			billsAndPacksTabControl.SelectedTab = billsTabPage;
			var dynamicBilllDetailsPanel = billsTabPage.FindSingle<DynamicLayoutPanel>(c => c.Name == "dynamicBilllDetailsPanel");
			var tariffFindBox = dynamicBilllDetailsPanel.FindSingle<Universal.GUI.TariffFindBox>(c => c.Name == "TariffFindBox");
			AssertEquals("Import", Universal.Constants.TariffTypes.Import, tariffFindBox.GetTariffType.Invoke());

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			AssertEquals("Export", Universal.Constants.TariffTypes.Export, tariffFindBox.GetTariffType.Invoke());
		}

		public void TestBillPartiesTabpageVisibility()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			var bill = header.Bills.AddNew();

			using var form = new ManifestForm(header);
			form.Show();
			var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
			var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
			var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
			mainTabControl.SelectedTab = billsAndPacksTabPage;
			var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
			var billPartiesTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billPartiesTabPage");

			Assert("Bill Parties tabpage should be visible by default.", billPartiesTabPage.TabVisible);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			Assert("Bill Parties tabpage should be hidden when HDF01.", !billPartiesTabPage.TabVisible);
		}
	}
}
