using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	public class ComplianceRiskMenuViewPartyRiskTest : ComplianceRiskHelperTest
	{
		public void TestViewPartyRiskMenuItem_OnPerformClick()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_Code = "Consignor";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignor = true;
			consignee.OH_Code = "Consignee";

			var shipment = CreateNewShipment;
			(shipment as CommonShipment).ConsigneePK = consignee.PK;
			(shipment as CommonShipment).ConsignorPK = consignor.PK;

			var manager = new DeniedPartyScreeningPresentationManagerForTest();
			using (var form = new ComplianceRiskPluginParentFormForTest(shipment))
			using (ObjectFactory.Substitute<IDeniedPartyScreeningPresentationManager>(() => manager))
			{
				form.Show();
				form.TopLevelMenu.MenuItems.FindByText("View Party Risk", true).PerformClick();

				AssertNotNull(manager.ComplianceRiskAction);
				AssertContainsExactElementsInAnyOrder(new[] { shipment }, manager.SourceBizOs?.Select(u => u.SourceBizO));
				AssertEquals(true, manager.ScreeningParties.Any(u => u.ScreeningEntity == consignor));
				AssertEquals(true, manager.ScreeningParties.Any(u => u.ScreeningEntity == consignee));
			}
		}

		public void TestOnViewPartyRisk_DoNotShowMessageIfJobOverallRiskUnchanged()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			var complianceRiskStatus = CreateNewComplianceRiskStatus(shipment as BusinessObject);
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Unknown;
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Held;

			var manager = new DeniedPartyScreeningPresentationManagerForTest();
			using (var form = new ComplianceRiskPluginParentFormForTest(shipment))
			using (ObjectFactory.Substitute<IDeniedPartyScreeningPresentationManager>(() => manager))
			{
				UnitTestUserNotification.Instance.ClearMessages();
				form.Show();
				form.TopLevelMenu.MenuItems.FindByText("View Party Risk", true).PerformClick();

				manager.ComplianceRiskAction.ShowMessageIfNeeded();

				AssertEquals(ComplianceRiskStatusCodeList.Codes.Held, complianceRiskStatus.COR_OverallRisk);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOnViewPartyRisk_ShowMessageIfOverallRiskChanged()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			var complianceRiskStatus = CreateNewComplianceRiskStatus(shipment as BusinessObject);
			var collection = complianceRiskStatus.CommodityDetailCollection;
			var commodity = collection.AddNew();
			commodity.CCD_HarmonizedCode = "123456";
			commodity.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

			var manager = new DeniedPartyScreeningPresentationManagerForTest();
			using (var form = new ComplianceRiskPluginParentFormForTest(shipment))
			using (ObjectFactory.Substitute<IDeniedPartyScreeningPresentationManager>(() => manager))
			{
				UnitTestUserNotification.Instance.ClearMessages();
				form.Show();
				form.TopLevelMenu.MenuItems.FindByText("View Party Risk", true).PerformClick();
				complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Clear;
				manager.ComplianceRiskAction.ShowMessageIfNeeded();

				AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);
				AssertEquals("The job will be set to 'Clear'.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region Implementation

		class DeniedPartyScreeningPresentationManagerForTest : IDeniedPartyScreeningPresentationManager
		{
			public Task PerformScreening(object parentForm, List<IDpsSourceWithParties> sourceBizOs, IScreeningParty[] screeningParties,
				bool isScreeningEntity, bool forceAllComplianceLists, bool suppressDeveloperException = false,
				Func<bool> parentEntityHasChanges = null, IComplianceRiskAction complianceRiskAction = null)
			{
				SourceBizOs = sourceBizOs.Cast<DpsSourceWithParties>().ToList();
				ScreeningParties = screeningParties as ScreeningParty[];
				ComplianceRiskAction = complianceRiskAction as ComplianceRiskAction;
				return Task.CompletedTask;
			}

			public List<DpsSourceWithParties> SourceBizOs { get; set; }
			public ScreeningParty[] ScreeningParties { get; set; }
			public ComplianceRiskAction ComplianceRiskAction { get; set; }
		}

		#endregion

		#region Module Menu: View Party Risk

		public void TestViewPartyRisk_ModuleActionMenuExists()
		{
			AssertNotNull("View Party Risk menu should exists", ViewPartyRiskModuleActionMenuExists(true));
			AssertNull("View Party Risk should not exists", ViewPartyRiskModuleActionMenuExists(false));

			MenuItem ViewPartyRiskModuleActionMenuExists(bool enableComplianceRisk)
			{
				using (var moduleFilterGrid = new FilterGridModuleForTest(Array.Empty<BusinessObject>()))
				using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(enableComplianceRisk)))
				using (Env.Registry.RawRegistry.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableComplianceRisk))
				{
					var menuItems = new List<MenuItem>();
					new DeniedPartyScreeningActionsProvider(moduleFilterGrid, menuItems).AddJobsMenuItem();
					return menuItems.FindByText("View Party Risk");
				}
			}
		}

		public void TestViewPartyRisk_ModuleActionMenuClick_NoSelectedItem()
		{
			using (var moduleFilterGrid1 = new FilterGridModuleForTest(Array.Empty<BusinessObject>()))
			using (Env.Registry.RawRegistry.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var menuItems = new List<MenuItem>();
				new DeniedPartyScreeningActionsProvider(moduleFilterGrid1, menuItems).AddJobsMenuItem();
				menuItems.FindByText("View Party Risk").PerformClick();

				AssertEquals("Please select at least 1 row to Process.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestViewPartyRisk_ModuleActionMenuClick_PartyAndBizOCorrect()
		{
			var shipment1 = CreateNewShipmentWithParty("CNR", "CNR1");
			var shipment2 = CreateNewShipmentWithParty("ORG", "ORG1");
			Factory.Save();

			var dummyBizOs = new List<BusinessObject>
			{
				shipment1, shipment2
			};

			using (var moduleFilterGrid1 = new FilterGridModuleForTest(dummyBizOs.ToArray()))
			using (Env.Registry.RawRegistry.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var menuItems = new List<MenuItem>();
				var manager = new DeniedPartyScreeningPresentationManagerForTest();
				using (ObjectFactory.Substitute<IDeniedPartyScreeningPresentationManager>(() => manager))
				{
					var query = new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, dummyBizOs.Select(x => x.PK));
					var complianceRiskStatuses = Factory.Load<ComplianceRiskStatus>(query);
					AssertEquals("No ComplianceRiskStatus in database", 0, complianceRiskStatuses.Length);

					new DeniedPartyScreeningActionsProvider(moduleFilterGrid1, menuItems).AddJobsMenuItem();
					menuItems.FindByText("View Party Risk").PerformClick();
					complianceRiskStatuses = Factory.Load<ComplianceRiskStatus>(query);
					AssertEquals("Two new ComplianceRiskStatus have been created and saved into database.", 2, complianceRiskStatuses.Length);

					AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, manager.SourceBizOs?.Select(u => u.SourceBizO));
					AssertEquals(true, manager.ScreeningParties.Any(u => u.ScreeningEntity == shipment1.Consignor));
					AssertEquals(true, manager.ScreeningParties.Any(u => u.ScreeningEntity == shipment1.Consignee));
					AssertEquals(true, manager.ScreeningParties.Any(u => u.ScreeningEntity == shipment2.Consignor));
					AssertEquals(true, manager.ScreeningParties.Any(u => u.ScreeningEntity == shipment2.Consignee));

					AssertNotNull(manager.ComplianceRiskAction);

					var complianceRiskStatus = complianceRiskStatuses.Single(x => x.COR_ParentID == shipment1.PK);
					AssertEquals("Job Compliance Status is Held", ComplianceRiskStatusCodeList.Codes.Held, complianceRiskStatus.COR_OverallRisk);

					complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Clear;
					UnitTestUserNotification.Instance.ClearMessages();
					manager.ComplianceRiskAction.ShowMessageIfNeeded();
					var jobRef = CodePropertyAttribute.CodeFromBusinessObject(shipment1);
					AssertEquals($"{jobRef} will be set to Clear.", UnitTestUserNotification.Instance.LastMessage.Text.TrimEnd());

					manager.ComplianceRiskAction.SynchronizeAndSaveIfNeeded();
					AssertEquals(ComplianceRiskStatusCodeList.Codes.Held, complianceRiskStatus.COR_OverallRisk);
				}
			}

			ForwardingShipment CreateNewShipmentWithParty(string consigneeCode, string consignorCode)
			{
				var consignee = Factory.NewWithValidTestData<OrgHeader>();
				consignee.OH_IsConsignee = true;
				consignee.OH_Code = consigneeCode;

				var consignor = Factory.NewWithValidTestData<OrgHeader>();
				consignor.OH_IsConsignor = true;
				consignor.OH_Code = consignorCode;

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.ConsigneePK = consignee.PK;
				shipment.ConsignorPK = consignor.PK;

				return shipment;
			}
		}

		#endregion

		IDisposable setAllowComplianceCommodityRiskAssessmentToTrue;
		protected override void SetUp()
		{
			base.SetUp();
			setAllowComplianceCommodityRiskAssessmentToTrue = OrganisationsDataRegistry.Instance.AllowComplianceCommodityRiskAssessment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			setAllowComplianceCommodityRiskAssessmentToTrue.Dispose();
		}
	}
}
