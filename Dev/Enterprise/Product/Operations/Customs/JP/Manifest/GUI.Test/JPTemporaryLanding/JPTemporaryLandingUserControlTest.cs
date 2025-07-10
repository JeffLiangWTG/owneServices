using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Manifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.GUI.Testing
{
	[TestedType(typeof(JPTemporaryLandingUserControl))]
	sealed class JPTemporaryLandingUserControlTest : TestCaseWithFactory
	{
		public void TestAdditionalTabPage()
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
			var additionalTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_JPTemporaryLandingUserControl");
			billsAndPacksTabControl.SelectedTab = additionalTabPage;
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Temporary Landing", additionalTabPage.CaptionResourceString.Caption);
				AssertEquals("TabVisible", true, additionalTabPage.TabVisible);
			});

			var jpTemporaryLandingUserControl = additionalTabPage.FindSingle<JPTemporaryLandingUserControl>(c => c.Name == "JPTemporaryLandingUserControl");
			var otherLawsandRegulationsGrid = jpTemporaryLandingUserControl.FindSingle<ZGrid>(c => c.Name == "OtherLawsandRegulationsGrid");
			var actualList = otherLawsandRegulationsGrid.Columns.Cast<ZGridColumn>().Select(x => x.ColumnName);
			AssertContainsExactElementsInExactOrder(new[] { "CFR_Reference", "CodeDescription" }, actualList);
			AssertEquals("TabPageSequence", 0, ((IAdditionalTabPage)jpTemporaryLandingUserControl).TabPageSequence);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			AssertEquals("TabVisible", false, additionalTabPage.TabVisible);
		}
	}
}
