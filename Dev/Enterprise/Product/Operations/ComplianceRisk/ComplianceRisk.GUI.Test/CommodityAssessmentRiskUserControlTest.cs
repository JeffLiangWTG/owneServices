using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	[TestedType(typeof(CommodityAssessmentRiskUserControl))]
	public class CommodityAssessmentRiskUserControlTest : ComplianceRiskHelperTest
	{
		public void TestUserControls()
		{
			using var userControl = new CommodityAssessmentRiskUserControl();
			var borderWiseLink = userControl.Controls.Find("CommodityBorderWiseLinkLabel", searchAllChildren: true).Single();
			AssertType<ZLinkLabel>(borderWiseLink);
			AssertEquals("CommodityBorderWiseLinkLabel is visible", true, borderWiseLink.Visible);
			AssertType<ZDropEdit>(userControl.Controls.Find("CommodityRiskStatusDropEdit", searchAllChildren: true).Single());
			AssertType<ZLabel>(userControl.Controls.Find("CommodityRiskStatusLabel", searchAllChildren: true).Single());
			AssertType<ZTextBox>(userControl.Controls.Find("AssessmentNotesTextBox", searchAllChildren: true).Single());
		}

		public void TestUserControls_BorderWiseAPIIntegration()
		{
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var userControl = new CommodityAssessmentRiskUserControl())
			{
				var borderWiseLink = userControl.Controls.Find("CommodityBorderWiseLinkLabel", searchAllChildren: true).Single();
				AssertType<ZLinkLabel>(borderWiseLink);
				AssertEquals("CommodityBorderWiseLinkLabel is visible", true, borderWiseLink.Visible);
			}
		}

		public void TestBorderWiseWebUrlLaunch()
		{
			var shipment = CreateNewShipment;
			var plugIn = new ComplianceRiskPlugInBusinessObject((IBusiness)shipment);

			var commodityDetail = plugIn.ComplianceRiskStatus.CommodityDetailCollection.AddNew();
			commodityDetail.CCD_HarmonizedCode = "123456";
			commodityDetail.BorderWiseCheckStatus = BorderWiseCheckStatus.Viewable;

			var checker = new DummyChecker();
			plugIn.CommodityRiskStatusChecker = checker;

			using var userControl = new CommodityAssessmentRiskUserControl();
			userControl.SetDataBinding(commodityDetail, string.Empty);
			var link = (ZLinkLabel)userControl.Controls.Find("CommodityBorderWiseLinkLabel", searchAllChildren: true).Single();
			link.OnLinkClicked_Exposed(new LinkLabelLinkClickedEventArgs(new LinkLabel.Link()));
			AssertEquals("Border Wise URL should launched:", true, checker.ViewBorderWisePortalCalled);
		}
	}

	public class DummyChecker : ISupportCheckCommodityRiskStatus
	{
		public Task CheckAllCommoditiesRiskStatus(bool needValidation = false)
		{
			return Task.CompletedTask;
		}

		public Task CheckCommoditiesRiskStatus(params ComplianceCommodityDetail[] complianceCommodityDetails)
		{
			return Task.CompletedTask;
		}

		public bool ViewBorderWisePortalCalled { get; set; }

		public Task ViewBorderWisePortal(ComplianceCommodityDetail commodityDetail)
		{
			ViewBorderWisePortalCalled = true;
			return Task.CompletedTask;
		}

		public Task GetSupportedCountriesAndAssignStatusIfNeeded()
		{
			return Task.CompletedTask;
		}
	}
}
