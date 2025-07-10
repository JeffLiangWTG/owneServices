using Enterprise.ComplianceRisk.Business.Test;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	public class LinkedModuleCommodityUserControlTest : ComplianceRiskHelperTest
	{
		public void TestLinkedModuleCommodityUserControl()
		{
			using (var form = new ZForm())
			using (var userControl = new LinkedModuleCommodityUserControl())
			{
				var dummyDetail = new DummyLinkedModuleComplianceCommodityDetail();
				dummyDetail.CommodityExists = true;
				userControl.SetDataBinding(dummyDetail, "");
				form.Controls.Add(userControl);
				form.Show();

				var helper = (DummyInteractionWithComplianceWiseCommoditiesHelper)dummyDetail.SupportInteractionWithCommodities.Helper;

				AssertEquals(false, userControl.AssessmentInitializeButton.ReadOnly);

				var assessmentInitializeButton = userControl.FindSingleOrDefault<ZButton>(c => c.Name == "AssessmentInitializeButton");
				AssertNotNull(assessmentInitializeButton);
				AssertEquals(true, assessmentInitializeButton.Enabled);

				AssertEquals(0, helper.InitializeCout);
				assessmentInitializeButton.PerformClick();
				AssertEquals(1, helper.InitializeCout);

				var commodityBorderWiseLinkLabel = userControl.FindSingleOrDefault<ZLinkLabel>(c => c.Name == "CommodityBorderWiseLinkLabel");
				AssertNotNull(commodityBorderWiseLinkLabel);

				AssertEquals(0, dummyDetail.ViewBorderWisePortalCount);
				commodityBorderWiseLinkLabel.OnLinkClicked_Exposed(null);
				AssertEquals(1, dummyDetail.ViewBorderWisePortalCount);
			}
		}

		public void TestOnCurrentDataItemChanged()
		{
			using (var form = new ZForm())
			using (var userControl = new LinkedModuleCommodityUserControl())
			{
				var dummyDetail = new DummyLinkedModuleComplianceCommodityDetail();
				userControl.SetDataBinding(dummyDetail, "");
				form.Controls.Add(userControl);
				form.Show();

				AssertEquals(false, dummyDetail.CommodityExists);
				AssertEquals(true, userControl.AssessmentInitializeButton.ReadOnly);

				dummyDetail.CommodityExists = true;
				dummyDetail.SupportInteractionWithCommodities.Helper.CpwSideCommodities.AssessmentStatusChanged?.Invoke();
				AssertEquals(false, userControl.AssessmentInitializeButton.ReadOnly);

				((DummyInteractionWithComplianceWiseCommoditiesHelper)dummyDetail.SupportInteractionWithCommodities
					.Helper).AssessmentInitializedForTest = true;
				dummyDetail.SupportInteractionWithCommodities.Helper.CpwSideCommodities.AssessmentStatusChanged?.Invoke();
				AssertEquals(true, userControl.AssessmentInitializeButton.ReadOnly);
			}
		}
	}
}
