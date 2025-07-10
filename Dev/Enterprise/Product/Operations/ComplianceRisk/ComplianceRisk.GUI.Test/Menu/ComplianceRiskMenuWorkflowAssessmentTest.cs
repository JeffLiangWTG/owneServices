using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	public class ComplianceRiskMenuWorkflowAssessmentTest : ComplianceRiskHelperTest
	{
		public void TestComplianceAssessmentMenuItemsSettings_ForConsolidationInternationalAndDomesticJob()
		{
			var consol = CreateNewConsolidation;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			AssertComplianceAssessmentMenuItems(consol, "International");

			consol = CreateNewConsolidation;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUMEL";
			AssertComplianceAssessmentMenuItems(consol, "Domestic");

			void AssertComplianceAssessmentMenuItems(object dataSource, string message)
			{
				Factory.Save();

				using (var form = new ComplianceRiskPluginParentFormForTest(dataSource))
				{
					form.Show();
					var menuItemInitializeAssessment = form.TopLevelMenu.MenuItems.FindByText("Initialize Compliance Assessment", true);
					var menuItemDeclineAssessment = form.TopLevelMenu.MenuItems.FindByText("Decline Compliance Assessment", true);

					CombineAssertions($"Compliance Assessment menu settings for {message} Job:", () =>
					{
						AssertNull("Initialize Compliance Assessment menu should not exist", menuItemInitializeAssessment);
						AssertNull("Decline Compliance Assessment menu should not exist", menuItemDeclineAssessment);
					});

					form.Dispose();
				}
			}
		}

		public void TestComplianceAssessmentMenuItemsSettings_ForShipmentInternationalAndDomesticJob()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			AssertComplianceAssessmentMenuItems(shipment, "International", true);

			shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUMEL";
			AssertComplianceAssessmentMenuItems(shipment, "Domestic", false);

			void AssertComplianceAssessmentMenuItems(object dataSource, string message, bool menuEnabled)
			{
				Factory.Save();

				using (var form = new ComplianceRiskPluginParentFormForTest(dataSource))
				{
					form.Show();
					var menuItemInitializeAssessment = form.TopLevelMenu.MenuItems.FindByText("Initialize Compliance Assessment", true);
					var menuItemDeclineAssessment = form.TopLevelMenu.MenuItems.FindByText("Decline Compliance Assessment", true);

					CombineAssertions($"Compliance Assessment menu settings for {message} Job:", () =>
					{
						AssertEquals("Initialize Compliance Assessment menu is visible", true, menuItemInitializeAssessment.Visible);
						AssertEquals("Initialize Compliance Assessment menu is enabled", menuEnabled, menuItemInitializeAssessment.Enabled);
						AssertEquals("Decline Compliance Assessment menu is visible", true, menuItemDeclineAssessment.Visible);
						AssertEquals("Decline Compliance Assessment menu is enabled", menuEnabled, menuItemDeclineAssessment.Enabled);
					});

					form.Dispose();
				}
			}
		}

		public void TestInitiateComplianceAssessmentMenuItem_OnClick()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			Factory.Save();

			using (var form = new ComplianceRiskPluginParentFormForTest(shipment))
			{
				form.Show();
				var menuItemInitializeAssessment = form.TopLevelMenu.MenuItems.FindByText("Initialize Compliance Assessment", true);
				menuItemInitializeAssessment.PerformClick();

				var menuItemDeclineAssessment = form.TopLevelMenu.MenuItems.FindByText("Decline Compliance Assessment", true);

				CombineAssertions("Initialize Compliance Assessment menu OnClick:", () =>
				{
					AssertEquals("Initialize Compliance Assessment menu is visible", true, menuItemInitializeAssessment.Visible);
					AssertEquals("Initialize Compliance Assessment menu is disabled", false, menuItemInitializeAssessment.Enabled);
					AssertEquals("Decline Compliance Assessment menu is visible", true, menuItemDeclineAssessment.Visible);
					AssertEquals("Decline Compliance Assessment menu is disabled", false, menuItemDeclineAssessment.Enabled);
				});
			}
		}

		public void TestDeclineComplianceAssessmentMenuItem_OnClick()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			Factory.Save();

			using (var form = new ComplianceRiskPluginParentFormForTest(shipment))
			{
				form.Show();
				var menuItemDeclineAssessment = form.TopLevelMenu.MenuItems.FindByText("Decline Compliance Assessment", true);
				menuItemDeclineAssessment.PerformClick();

				var menuItemInitializeAssessment = form.TopLevelMenu.MenuItems.FindByText("Initialize Compliance Assessment", true);

				CombineAssertions("Decline Compliance Assessment menu OnClick:", () =>
				{
					AssertEquals("Initialize Compliance Assessment menu is visible", true, menuItemInitializeAssessment.Visible);
					AssertEquals("Initialize Compliance Assessment menu is enabled", true, menuItemInitializeAssessment.Enabled);
					AssertEquals("Decline Compliance Assessment menu is visible", true, menuItemDeclineAssessment.Visible);
					AssertEquals("Decline Compliance Assessment menu is disabled", false, menuItemDeclineAssessment.Enabled);
				});
			}
		}
	}
}
