using System;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Business.Test;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	public class ComplianceRiskStatusFormSynchronizerTest : ComplianceRiskHelperTest
	{
		public void TestSynchronizeEnforceOnFormLoad_WhenOverallRiskNoChangesOccurred()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			((CommonShipment)shipment).ConsigneePK = (Factory.NewWithValidTestData<OrgHeader>()).PK;

			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.OverrideClear;
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.HighRisk;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Unknown;

			Factory.Save();

			using (var form = new ComplianceRiskPluginParentFormForTest(shipment))
			{
				form.Show();
				var pluginBizO = ((ComplianceRiskPlugIn)form.PlugIns.Instances[0]).GetBusinessObjectForPlugin;

				AssertEquals("Party Risk status Potential Risk", ComplianceRiskStatusCodeList.Descriptions.HighRisk.GetUnresolvedString(), pluginBizO.PartyRiskDescription);
				AssertEquals("Location Risk status Clear", ComplianceRiskStatusCodeList.Descriptions.Clear.GetUnresolvedString(), pluginBizO.LocationRiskDescription);
				AssertEquals("Commodity Risk status Unknown", ComplianceRiskStatusCodeList.Descriptions.Unknown.GetUnresolvedString(), pluginBizO.CommodityRiskDescription);
				AssertEquals("Overall Risk status Override Clear", ComplianceRiskStatusCodeList.Descriptions.OverrideClear.GetUnresolvedString(), pluginBizO.OverallRiskDescription);

				form.Dispose();
			}
		}

		public void TestSynchronizeEnforceOnFormLoad_WhenOverallRiskIsOutOfSync()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Incomplete;

			var tariffView = ComplianceRisk.Business.Test.ComplianceRiskTariffTestDataHelper.CreateNewOrLoadTariff(Factory, "357412");
			var commodity = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity.CCD_HarmonizedCode = tariffView.ZZ1_TariffCode;

			StmComplianceEventHelperTest.CreateAssessmentEvent((BusinessObject)shipment, ComplianceEventList.Codes.AssessmentInitialized);

			Factory.Save();

			using (var form = new ComplianceRiskPluginParentFormForTest(shipment))
			{
				form.Show();
				var pluginBizO = ((ComplianceRiskPlugIn)form.PlugIns.Instances[0]).GetBusinessObjectForPlugin;

				CombineAssertions("Pre-Condition: ", () =>
				{
					AssertEquals("Party Risk status Clear", ComplianceRiskStatusCodeList.Descriptions.Clear.GetUnresolvedString(), pluginBizO.PartyRiskDescription);
					AssertEquals("Location Risk status Clear", ComplianceRiskStatusCodeList.Descriptions.Clear.GetUnresolvedString(), pluginBizO.LocationRiskDescription);
					AssertEquals("Commodity Risk status Unknown", ComplianceRiskStatusCodeList.Descriptions.Unknown.GetUnresolvedString(), pluginBizO.CommodityRiskDescription);
				});

				AssertEquals("Expected Overall Risk status Potential Risk to Held", ComplianceRiskStatusCodeList.Descriptions.Held.GetUnresolvedString(), pluginBizO.OverallRiskDescription);

				form.Dispose();
			}
		}

		public void TestSynchronizeEnforceOnFormLoad_WhenPartyRiskIsOutOfSync()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			((CommonShipment)shipment).ConsigneePK = (Factory.NewWithValidTestData<OrgHeader>()).PK;

			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Incomplete;

			Factory.Save();

			using (var form = new ComplianceRiskPluginParentFormForTest(shipment))
			{
				form.Show();
				var pluginBizO = ((ComplianceRiskPlugIn)form.PlugIns.Instances[0]).GetBusinessObjectForPlugin;

				CombineAssertions("Pre-Condition: ", () =>
				{
					AssertEquals("Location Risk status Clear", ComplianceRiskStatusCodeList.Descriptions.Clear.GetUnresolvedString(), pluginBizO.LocationRiskDescription);
					AssertEquals("Commodity Risk status Unknown", ComplianceRiskStatusCodeList.Descriptions.Unknown.GetUnresolvedString(), pluginBizO.CommodityRiskDescription);
				});

				AssertEquals("Expected Party Risk status Clear to High Risk", ComplianceRiskStatusCodeList.Descriptions.HighRisk.GetUnresolvedString(), pluginBizO.PartyRiskDescription);
				AssertEquals("Expected Overall Risk status Clear to Held", ComplianceRiskStatusCodeList.Descriptions.Held.GetUnresolvedString(), pluginBizO.OverallRiskDescription);

				form.Dispose();
			}
		}

		public void TestSynchronizeEnforceOnFormLoad_WhenLocationRiskIsOutOfSync()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "IRTHR";

			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Incomplete;

			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Iran));
			country.RN_IsSanctioned = true;

			Factory.Save();

			using (var form = new ComplianceRiskPluginParentFormForTest(shipment))
			{
				form.Show();
				var pluginBizO = ((ComplianceRiskPlugIn)form.PlugIns.Instances[0]).GetBusinessObjectForPlugin;

				CombineAssertions(() =>
				{
					AssertEquals("Party Risk status Clear", ComplianceRiskStatusCodeList.Descriptions.Clear.GetUnresolvedString(), pluginBizO.PartyRiskDescription);
					AssertEquals("Commodities Risk status Unknown", ComplianceRiskStatusCodeList.Descriptions.Unknown.GetUnresolvedString(), pluginBizO.CommodityRiskDescription);
					AssertEquals("Location Risk status Clear to Blocked", ComplianceRiskStatusCodeList.Descriptions.Blocked.GetUnresolvedString(), pluginBizO.LocationRiskDescription);
					AssertEquals("Overall Risk status Clear to  Blocked", ComplianceRiskStatusCodeList.Descriptions.Blocked.GetUnresolvedString(), pluginBizO.OverallRiskDescription);
				});

				form.Dispose();
			}
		}

		public void TestInitializeComplianceAssessmentWhenMenuClick()
		{
			var shipment = CreateNewShipment;
			var complianceRisk = new ComplianceRiskPlugInBusinessObject(shipment as IBusiness);
			using (var form = new ComplianceWorkflowInitiationHelperTest.FormForTest(shipment as BusinessObject))
			{
				ComplianceRiskStatusFormSynchronizer.InitializeComplianceAssessmentWhenMenuClick(complianceRisk, form).Wait();
				AssertEquals("Please save form before Initializing Compliance Assessment", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();

				ComplianceRiskStatusFormSynchronizer.InitializeComplianceAssessmentWhenMenuClick(complianceRisk, form).Wait();
				AssertEquals("Compliance Assessment is not available on this Job.", UnitTestUserNotification.Instance.LastMessage.Text);

				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";

				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessages();
				ComplianceRiskStatusFormSynchronizer.InitializeComplianceAssessmentWhenMenuClick(complianceRisk, form).Wait();

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Incomplete, complianceRisk.ComplianceRiskStatus.COR_CommodityRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRisk.ComplianceRiskStatus.COR_OverallRisk);
				AssertEquals("ComplianceRiskTabPage", form.TabPageName);

				form.TabPageName = "DUMMY";
				ComplianceRiskStatusFormSynchronizer.InitializeComplianceAssessmentWhenMenuClick(complianceRisk, form).Wait();
				AssertEquals("Not Duplicate Initialize", "DUMMY", form.TabPageName);
			}
		}

		public void TestInitializeComplianceAssessmentWithConcurrencyError()
		{
			ComplianceAssessmentWithConcurrencyError(true);
		}

		public void TestDeclineComplianceAssessmentWithConcurrencyError()
		{
			ComplianceAssessmentWithConcurrencyError(false);
		}

		void ComplianceAssessmentWithConcurrencyError(bool isInitialize)
		{
			ErrorReporter.Instance.Clear();
			var shipment = CreateNewShipment;
			var complianceRisk = new ComplianceRiskPlugInBusinessObject(shipment as IBusiness);
			using (var form = new ComplianceWorkflowInitiationHelperTest.FormForTest(shipment as BusinessObject))
			{
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessages();
				BusinessObjectFactory.SetOnFactorySaveHookForTest(_ => throw new ZSaveConcurrencyException(new ZSaveConcurrencyException(new ZDataConcurrencyException(new InvalidOperationException("~ConcurrencyError~"), ((IBusinessObjectInternals)complianceRisk.ComplianceRiskStatus).Row, Db.Connection), Factory)));
				if (isInitialize)
				{
					ComplianceRiskStatusFormSynchronizer.InitializeComplianceAssessmentWhenMenuClick(complianceRisk, form).Wait();
					AssertEquals("Failed to initialize Compliance Assessment due to concurrency error. Please reload the form and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
				else
				{
					ComplianceRiskStatusFormSynchronizer.DeclineComplianceAssessmentWhenMenuClick(complianceRisk);
					AssertEquals("Failed to decline Compliance Assessment due to concurrency error. Please reload the form and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				AssertEquals(0, ErrorReporter.TotalErrorCount);
			}
		}

		public void TestInitializeComplianceAssessmentWhenMenuClick_SecurityCheckPointNotGranted_ErrorMessageShown()
		{
			var shipment = CreateNewShipment;
			var complianceRisk = new ComplianceRiskPlugInBusinessObject(shipment as IBusiness);
			var securityCore = new ComplianceRiskHelperTest().CreateNewSecurityCore;
			securityCore.ShipmentsComplianceAllowComplianceAssessment.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			using (var form = new ComplianceWorkflowInitiationHelperTest.FormForTest(shipment as BusinessObject))
			{
				UnitTestUserNotification.Instance.ClearMessages();
				form.Show();

				ComplianceRiskStatusFormSynchronizer.InitializeComplianceAssessmentWhenMenuClick(complianceRisk, form).Wait();
				AssertEquals(securityCore.ShipmentsComplianceAllowComplianceAssessment.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDeclineComplianceAssessmentWhenMenuClick_SecurityCheckPointNotGranted_ErrorMessageShown()
		{
			var shipment = CreateNewShipment;
			var complianceRisk = new ComplianceRiskPlugInBusinessObject(shipment as IBusiness);
			var securityCore = new ComplianceRiskHelperTest().CreateNewSecurityCore;
			securityCore.ShipmentsComplianceAllowComplianceAssessment.IsAllowed = false;
			securityCore.ShipmentsComplianceDeclineComplianceAssessment.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			using (var form = new ComplianceWorkflowInitiationHelperTest.FormForTest(shipment as BusinessObject))
			{
				UnitTestUserNotification.Instance.ClearMessages();
				form.Show();

				ComplianceRiskStatusFormSynchronizer.DeclineComplianceAssessmentWhenMenuClick(complianceRisk);
				AssertEquals(securityCore.ShipmentsComplianceDeclineComplianceAssessment.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDeclineComplianceAssessmentWhenMenuClick()
		{
			var shipment = CreateNewShipment;
			var complianceRisk = new ComplianceRiskPlugInBusinessObject(shipment as IBusiness);
			ComplianceRiskStatusFormSynchronizer.DeclineComplianceAssessmentWhenMenuClick(complianceRisk);
			AssertEquals("Please save form before Decline Compliance Assessment", UnitTestUserNotification.Instance.LastMessage.Text);

			Factory.Save();

			ComplianceRiskStatusFormSynchronizer.DeclineComplianceAssessmentWhenMenuClick(complianceRisk);
			AssertEquals("Compliance Assessment is not available on this Job.", UnitTestUserNotification.Instance.LastMessage.Text);

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessages();
			ComplianceRiskStatusFormSynchronizer.DeclineComplianceAssessmentWhenMenuClick(complianceRisk);

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRisk.ComplianceRiskStatus.COR_OverallRisk);

			var eventLog = complianceRisk.ComplianceRiskStatus.GetEventLogs().Single(u => u.SCE_EventType == AutoEvents.ComplianceRiskInteraction.Code);
			AssertNotNull(eventLog);
			AssertEquals("CRI", eventLog.SCE_EventType);
			AssertEquals("CAD", eventLog.SCE_EventSubType);
			AssertEquals("|MST=Compliance Assessment|NEW=CAD", eventLog.SCE_EventReference);
		}

		public void TestGetValidComplianceRiskProviderBizO_ViewProvider()
		{
			var mockViewProvider = new Mock<IViewComplianceRiskStatusProvider>();
			var provider = new Mock<IComplianceItemRiskStatusProvider>().Object;
			mockViewProvider.Setup(x => x.GetProviderBusinessObject()).Returns(provider);

			AssertEquals(provider, GetProviderDelegate().Invoke(mockViewProvider.As<IBusiness>().Object));
		}

		public void TestGetValidComplianceRiskProviderBizO_ViewProviderWithEmpty()
		{
			var mockViewProvider = new Mock<IViewComplianceRiskStatusProvider>();
			mockViewProvider.Setup(x => x.GetProviderBusinessObject()).Returns(default(IComplianceItemRiskStatusProvider));

			AssertNull(GetProviderDelegate().Invoke(mockViewProvider.As<IBusiness>().Object));
		}

		public void TestGetValidComplianceRiskProviderBizO()
		{
			var provider = new Mock<IComplianceItemRiskStatusProvider>().As<IBusiness>().Object;
			AssertEquals(provider, GetProviderDelegate().Invoke(provider));

			AssertNull(GetProviderDelegate().Invoke(new Mock<IBusiness>().Object));
		}

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

		TryGetValidComplianceRiskProviderBizO GetProviderDelegate() => (TryGetValidComplianceRiskProviderBizO)Delegate.CreateDelegate(
			typeof(TryGetValidComplianceRiskProviderBizO),
			typeof(ComplianceRiskStatusFormSynchronizer).GetMethod("TryGetValidComplianceRiskProviderBizO", BindingFlags.Static | BindingFlags.NonPublic));

		delegate IComplianceItemRiskStatusProvider TryGetValidComplianceRiskProviderBizO(IBusiness bizO);
	}
}
