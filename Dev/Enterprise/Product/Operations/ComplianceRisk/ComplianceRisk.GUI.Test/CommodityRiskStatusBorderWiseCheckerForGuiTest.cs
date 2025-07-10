using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Business.Test;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	class CommodityRiskStatusBorderWiseCheckerForGuiTest : ComplianceRiskHelperTest
	{
		public void TestSaveCommodityRiskStatusWhenBorderWiseAPIReturn()
		{
			AssertSaveCommodityRiskStatusWhenBorderWiseAPIReturn(jobHasChanges: false);
			AssertSaveCommodityRiskStatusWhenBorderWiseAPIReturn(jobHasChanges: true);

			void AssertSaveCommodityRiskStatusWhenBorderWiseAPIReturn(bool jobHasChanges)
			{
				var shipment = CreateNewShipment;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "NZAKL";

				using var form = new ComplianceRiskPluginParentFormForTest((IBusiness)shipment);
				form.Show();
				var pluginBizO = ((ComplianceRiskPlugIn)form.PlugIns.Instances[0]).GetBusinessObjectForPlugin;
				var commodity = pluginBizO.ComplianceRiskStatus.CommodityDetailCollection.AddNew();
				commodity.CCD_HarmonizedCode = "123456";

				Factory.Save();

				StmComplianceEventHelperTest.CreateAssessmentEvent((BusinessObject)shipment, ComplianceEventList.Codes.AssessmentInitialized);

				if (jobHasChanges)
				{
					shipment.JS_RL_NKDestination = "USORD";
				}

				using (BorderWiseApiHelper.SetResponse(CommodityRiskStatusBorderWiseCheckerTest.GetResponse("123456", false, false, Factory)))
				{
					var checker = new CommodityRiskStatusBorderWiseCheckerForGui(pluginBizO, CancellationToken.None);
					checker.CheckCommoditiesRiskStatus(commodity).Wait();

					CombineAssertions(() =>
					{
						AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, commodity.CCD_RiskStatus);
						AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, pluginBizO.ComplianceRiskStatus.COR_CommodityRisk);
						AssertEquals(jobHasChanges, pluginBizO.ComplianceRiskStatus.HasChanges);
						if (jobHasChanges)
						{
							AssertEquals("Not save the factory when job has changes", true, (shipment as BusinessObject).HasChanges);
						}
						else
						{
							AssertEquals("Save the factory when job has no changes", false, (shipment as BusinessObject).HasChanges);
						}
					});
				}

				form.Close();
			}
		}

		public void TestGetSupportedCountriesAndAssignStatusIfNeeded()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var plugIn = new ComplianceRiskPlugInBusinessObject((IBusiness)shipment);

			var commodity = plugIn.ComplianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity.CCD_HarmonizedCode = "123456";
			commodity.CCD_RN_NKOrigin = "AU";
			AssertEquals(ComplianceRiskStatusCodeList.Codes.NotChecked, commodity.CCD_RiskStatus);

			var checker = new CommodityRiskStatusBorderWiseCheckerForGui(plugIn, CancellationToken.None);

			using (BorderWiseApiHelper.SetSupportedCountriesResponse(new SupportedCountriesCheckResponseModel
			{
				CommodityLevel = new CommodityLevelModel
				{
					Export = new[] { "US" },
					Import = new[] { "US" },
					OriginOfGoods = new[] { "US" }
				}
			}))
			{
				checker.GetSupportedCountriesAndAssignStatusIfNeeded().Wait();
				AssertEquals(ComplianceRiskStatusCodeList.Codes.PossibleRisk, commodity.CCD_RiskStatus);
			}
		}

		public void TestCheckAllCommoditiesRiskStatus()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var plugIn = new ComplianceRiskPlugInBusinessObject((IBusiness)shipment);

			var commodity = plugIn.ComplianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity.CCD_HarmonizedCode = "123456";

			StmComplianceEventHelperTest.CreateAssessmentEvent((BusinessObject)shipment, ComplianceEventList.Codes.AssessmentInitialized);

			var checker = new CommodityRiskStatusBorderWiseCheckerForGui(plugIn, CancellationToken.None);

			using (BorderWiseApiHelper.SetResponse(CommodityRiskStatusBorderWiseCheckerTest.GetResponse("123456", false, false, Factory)))
			{
				checker.CheckAllCommoditiesRiskStatus(true).Wait();
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, commodity.CCD_RiskStatus);
			}
		}

		public void TestCheckCommoditiesRiskStatus()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var plugIn = new ComplianceRiskPlugInBusinessObject((IBusiness)shipment);

			var commodity = plugIn.ComplianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity.CCD_HarmonizedCode = "123456";

			StmComplianceEventHelperTest.CreateAssessmentEvent((BusinessObject)shipment, ComplianceEventList.Codes.AssessmentInitialized);

			var checker = new CommodityRiskStatusBorderWiseCheckerForGui(plugIn, CancellationToken.None);

			using (BorderWiseApiHelper.SetResponse(CommodityRiskStatusBorderWiseCheckerTest.GetResponse("123456", false, false, Factory)))
			{
				checker.CheckCommoditiesRiskStatus(commodity).Wait();
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, commodity.CCD_RiskStatus);
			}
		}

		public void TestViewBorderWisePortal()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var plugIn = new ComplianceRiskPlugInBusinessObject((IBusiness)shipment);

			var commodity = plugIn.ComplianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity.CCD_HarmonizedCode = "123456";
			commodity.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Released;

			ComplianceRiskStatusWorkflowInitializer.InitializeAssessmentWorkflow(plugIn.ComplianceRiskStatus);

			var checker = new CommodityRiskStatusBorderWiseCheckerForGui(plugIn, CancellationToken.None);

			var response = CommodityRiskStatusBorderWiseCheckerTest.GetResponse("123456", false, false, Factory);
			using (BorderWiseApiHelper.SetResponse(response))
			{
				checker.ViewBorderWisePortal(commodity).Wait();
				AssertEquals("Border Wise URL should launched:", GetBorderWiseLaunchedURL(response.RequestId, commodity.CCD_HarmonizedCode), WebUrlLauncher.LastUrlLaunched);

				var stmComplianceLogs = GetStmComplianceEventLog(commodity);

				AssertEquals("Legal Books Viewed Logs Created", 1, stmComplianceLogs.Length);
				AssertEquals(stmComplianceLogs[0].SCE_ParentID, commodity.ComplianceRiskStatus.COR_ParentID);
				AssertEquals(stmComplianceLogs[0].SCE_ParentTableCode, JobShipmentSchema.Constants.Prefix);
				AssertEquals(stmComplianceLogs[0].SCE_EventType, ComplianceEventList.EventType.BorderWiseIntegration);
				AssertEquals(stmComplianceLogs[0].SCE_EventSubType, ComplianceEventList.Codes.LegalBooksViewed);
				AssertEquals(stmComplianceLogs[0].SCE_EventReference, commodity.CCD_HarmonizedCode);
				AssertEquals(stmComplianceLogs[0].SCE_SystemCreateUser, Env.Instance.CurrentUser.Initials);

				checker.ViewBorderWisePortal(commodity).Wait();
				stmComplianceLogs = GetStmComplianceEventLog(commodity);
				AssertEquals("Legal Books Viewed Logs Created", 2, stmComplianceLogs.Length);
			}
		}

		public void TestViewBorderWisePortal_MessagePopUp_WhenLinkIsClickedWithoutSavingOrAssessmentNotInitialized()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var plugIn = new ComplianceRiskPlugInBusinessObject((IBusiness)shipment);

			var commodity = plugIn.ComplianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity.CCD_HarmonizedCode = "123456";
			commodity.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Released;

			var checker = new CommodityRiskStatusBorderWiseCheckerForGui(plugIn, CancellationToken.None);

			var response = CommodityRiskStatusBorderWiseCheckerTest.GetResponse("123456", false, false, Factory);
			using (BorderWiseApiHelper.SetResponse(response))
			{
				var borderWiseURL = GetBorderWiseLaunchedURL(response.RequestId, commodity.CCD_HarmonizedCode);

				checker.ViewBorderWisePortal(commodity).Wait();
				AssertNotEquals("Border Wise URL should not launched:", borderWiseURL, WebUrlLauncher.LastUrlLaunched);
				AssertEquals("Please save the form before opening Compliance Alerts link.", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();

				checker.ViewBorderWisePortal(commodity).Wait();
				AssertNotEquals("Border Wise URL should not launched:", borderWiseURL, WebUrlLauncher.LastUrlLaunched);
				AssertEquals("Please initiate the assessment first.", UnitTestUserNotification.Instance.LastMessage.Text);

				commodity.CCD_HarmonizedCode = "654321";
				borderWiseURL = GetBorderWiseLaunchedURL(response.RequestId, commodity.CCD_HarmonizedCode);
				checker.ViewBorderWisePortal(commodity).Wait();
				AssertNotEquals("Border Wise URL should not launched:", borderWiseURL, WebUrlLauncher.LastUrlLaunched);
				AssertEquals("Please save the form before opening Compliance Alerts link.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();

				Factory.Save();

				shipment.JS_RL_NKOrigin = "USORD";
				checker.ViewBorderWisePortal(commodity).Wait();
				AssertNotEquals("Border Wise URL should not launched:", borderWiseURL, WebUrlLauncher.LastUrlLaunched);
				AssertEquals("Please save the form before opening Compliance Alerts link.", UnitTestUserNotification.Instance.LastMessage.Text);

				var stmComplianceLogs = GetStmComplianceEventLog(commodity);
				AssertEquals(0, stmComplianceLogs.Length);
			}
		}

		public void TestCheckCommoditiesRiskStatus_ComplianceRiskSpinnerIndicatorVisibility()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			var visibilityChangeList = new List<bool>();

			var plugIn = new ComplianceRiskPlugInBusinessObject((IBusiness)shipment);
			plugIn.ComplianceRiskSpinnerIndicatorVisibility = visible =>
			{
				visibilityChangeList.Add(visible);
			};

			var commodity = plugIn.ComplianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity.CCD_HarmonizedCode = "123456";

			StmComplianceEventHelperTest.CreateAssessmentEvent((BusinessObject)shipment, ComplianceEventList.Codes.AssessmentInitialized);

			var checker = new CommodityRiskStatusBorderWiseCheckerForGui(plugIn, CancellationToken.None);

			using (BorderWiseApiHelper.SetResponse(CommodityRiskStatusBorderWiseCheckerTest.GetResponse("123456", false, false, Factory)))
			{
				checker.CheckCommoditiesRiskStatus(commodity).Wait();
				AssertContainsExactElementsInExactOrder(new[] { true, false }, visibilityChangeList);

				checker.CheckAllCommoditiesRiskStatus(true).Wait();
				AssertContainsExactElementsInExactOrder(new[] { true, false, true, false }, visibilityChangeList);
			}
		}

		bool rawEnableComplianceRisk;
		EnableComplianceWiseRegistryBusinessObject rawComplianceWiseRegistryBusinessObject;

		protected override void SetUp()
		{
			base.SetUp();
			rawEnableComplianceRisk = RawDataRegistry.Instance.EnableComplianceRisk.Value;
			rawComplianceWiseRegistryBusinessObject = FreightDataRegistry.Instance.FreightEnableComplianceWise.DefaultValue;

			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
			ComplianceWiseRegistryHelper.SetValue(true));
		}

		protected override void TearDown()
		{
			base.TearDown();
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawEnableComplianceRisk);
			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawComplianceWiseRegistryBusinessObject);
		}

		#region Implementation

		StmComplianceEvent[] GetStmComplianceEventLog(ComplianceCommodityDetail commodityDetail)
		{
			var query = new ZQuery(StmComplianceEventSchema.SCE_ParentID, commodityDetail.ComplianceRiskStatus.COR_ParentID);
			query.AddToFilter(StmComplianceEventSchema.SCE_SystemCreateUser, Env.Instance.CurrentUser.Initials);
			query.AddToFilter(StmComplianceEventSchema.SCE_EventReference, commodityDetail.CCD_HarmonizedCode);
			query.AddToFilter(StmComplianceEventSchema.SCE_EventType, ComplianceEventList.EventType.BorderWiseIntegration);
			query.AddToFilter(StmComplianceEventSchema.SCE_EventSubType, ComplianceEventList.Codes.LegalBooksViewed);

			return Factory.Load<StmComplianceEvent>(query);
		}

		string GetBorderWiseLaunchedURL(Guid guid, string focusedCommodity) => FormattableString.Invariant($"https://app.borderwise.com?requestId={guid}&focusedCommodity={focusedCommodity}");

		#endregion
	}
}
