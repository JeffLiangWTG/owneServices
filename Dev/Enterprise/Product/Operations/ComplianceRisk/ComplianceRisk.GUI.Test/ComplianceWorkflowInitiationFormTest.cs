using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	[TestedType(typeof(ComplianceWorkflowInitiationForm))]
	public class ComplianceWorkflowInitiationFormTest : ZFormBasherTest
	{
		public void TestYesButtonClick_WhenAllowAssessmentSecurityIsGranted()
		{
			var testData = CreateNewTestDataAndSave();
			var pluginBizO = new ComplianceRiskPlugInBusinessObject((IBusiness)testData.Provider);
			var securityCore = new ComplianceRiskHelperTest().CreateNewSecurityCore;
			securityCore.ShipmentsComplianceAllowComplianceAssessment.IsAllowed = true;

			var commodityRiskStatusChecker = new Mock<ISupportCheckCommodityRiskStatus>();
			pluginBizO.CommodityRiskStatusChecker = commodityRiskStatusChecker.Object;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			using (var form = new ComplianceWorkflowInitiationFormForTest(testData.Provider, testData.ComplianceRiskStatus))
			{
				form.Show();
				form.YesButtonExposed.PerformClick();

				CombineAssertions("Security checkpoint allow assessment -> TRUE, should proceed initialized assessment", () =>
				{
					AssertEquals("DialogResult", DialogResult.Yes, form.DialogResult);
					AssertEquals("form.Visible", false, form.Visible);
					AssertEquals("ComplianceRiskStatus.IsAssessmentInitialized", true, testData.ComplianceRiskStatus.IsAssessmentInitialized);
					commodityRiskStatusChecker.Verify(checker => checker.CheckAllCommoditiesRiskStatus(It.IsAny<bool>()), Times.Once);
				});
			}
		}

		public void TestNoButtonClick_WhenEitherTheAllowOrDeclineAssessmentSecurityOptionsAreGranted_SecurityErrorMessageNotShown()
		{
			var testData = CreateNewTestDataAndSave();
			var securityCore = new ComplianceRiskHelperTest().CreateNewSecurityCore;
			securityCore.ShipmentsComplianceAllowComplianceAssessment.IsAllowed = false;
			securityCore.ShipmentsComplianceDeclineComplianceAssessment.IsAllowed = true;

			AssertNorErrorShown("Security checkpoint allow Assessment -> FALSE and decline assessment -> TRUE");

			securityCore.ShipmentsComplianceAllowComplianceAssessment.IsAllowed = true;
			securityCore.ShipmentsComplianceDeclineComplianceAssessment.IsAllowed = false;

			AssertNorErrorShown("Security checkpoint allow Assessment -> TRUE and decline assessment -> FALSe");

			void AssertNorErrorShown(string message)
			{
				using (Env.SetTemporarySecurityInstanceForTest(securityCore))
				using (var form = new ComplianceWorkflowInitiationFormForTest(testData.Provider, testData.ComplianceRiskStatus))
				{
					UnitTestUserNotification.Instance.ClearMessages();

					form.Show();
					form.NoButtonExposed.PerformClick();

					CombineAssertions(message + ", should not show security rights error message", () =>
					{
						AssertEquals("DialogResult", DialogResult.No, form.DialogResult);
						AssertEquals("form.Visible", false, form.Visible);
						AssertNullOrEmpty("UserNotificationLastMessage", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals("ComplianceRiskStatus.IsAssessmentDeclined", true, testData.ComplianceRiskStatus.IsAssessmentDeclined);
					});
				}
			}
		}

		public void TestCloseForm()
		{
			var testData = CreateNewTestDataAndSave();
			using var form = new ComplianceWorkflowInitiationFormForTest(testData.Provider, testData.ComplianceRiskStatus);
			form.Show();
			form.Close();
			AssertEquals("DialogResult", DialogResult.Cancel, form.DialogResult);
			AssertEquals("form.Visible", false, form.Visible);
			AssertEquals("Compliance Assessment Declined or Initialized", false, testData.ComplianceRiskStatus.IsAssessmentDeclined || testData.ComplianceRiskStatus.IsAssessmentInitialized);
		}

		public void TestViewComplianceRiskLabelExposed()
		{
			var testData = CreateNewTestDataAndSave();
			using var form = new ComplianceWorkflowInitiationFormForTest(testData.Provider, testData.ComplianceRiskStatus);
			form.Show();
			form.ViewComplianceRiskLabelExposed.OnLinkClicked_Exposed(null);
			AssertEquals("DialogResult", DialogResult.Abort, form.DialogResult);
			AssertEquals("form.Visible", false, form.Visible);
			AssertEquals("Compliance Assessment Declined or Initialized", false, testData.ComplianceRiskStatus.IsAssessmentDeclined || testData.ComplianceRiskStatus.IsAssessmentInitialized);
		}

		public void TestComplianceWorkflowInitiationRiskControls()
		{
			var testData = CreateNewTestDataAndSave();
			using var form = new ComplianceWorkflowInitiationFormForTest(testData.Provider, testData.ComplianceRiskStatus);
			form.Show();

			var riskControls = form.FindAll<ComplianceWorkflowInitiationRiskControl>();
			AssertEquals(5, riskControls.Count());
			AssertEquals(true, riskControls.Any(c => c.Name.Contains("Header")));
			AssertEquals(true, riskControls.Any(c => c.Name.Contains("Parties")));
			AssertEquals(true, riskControls.Any(c => c.Name.Contains("Locations")));
			AssertEquals(true, riskControls.Any(c => c.Name.Contains("Commodities")));
			AssertEquals(true, riskControls.Any(c => c.Name.Contains("Assessment")));
		}

		public void TestComplianceWorkflowInitiationFormContainsCorrectText()
		{
			var testData = CreateNewTestDataAndSave();
			using var form = new ComplianceWorkflowInitiationFormForTest(testData.Provider, testData.ComplianceRiskStatus);
			form.Show();

			var yesButton = form.Controls.Find("YesButton", searchAllChildren: true).SingleOrDefault();
			AssertEquals("Yes - Perform check", yesButton.Text);
			Assert("YesButton visible true", yesButton.Visible);

			var noButton = form.Controls.Find("NoButton", searchAllChildren: true).SingleOrDefault();
			AssertEquals("No - Proceed without", noButton.Text);
			Assert("NoButton visible true", noButton.Visible);

			var viewComplianceRiskLabelText = form.CaptionResourceString.Caption;
			AssertEquals("Review compliance risk", viewComplianceRiskLabelText);

			var ctaText = form.Controls.Find("PromptLabel", searchAllChildren: true).SingleOrDefault().Text;
			AssertEquals("Would you like to check the commodities on this job for compliance risk before proceeding?", ctaText);

			var okSecurityButton = form.Controls.Find("OKSecurityButton", searchAllChildren: true).SingleOrDefault();
			AssertEquals("OK", okSecurityButton.Text);
			Assert("OKSecurityButton visible false", !okSecurityButton.Visible);
		}

		public void TestOKSecurityButtonClick_WhenAllowAndDeclineAssessmentSecurityOptionsAreNotGranted_SecurityRightsPathShown()
		{
			var testData = CreateNewTestDataAndSave();
			var securityCore = new ComplianceRiskHelperTest().CreateNewSecurityCore;
			securityCore.ShipmentsComplianceAllowComplianceAssessment.IsAllowed = false;
			securityCore.ShipmentsComplianceDeclineComplianceAssessment.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			using (var form = new ComplianceWorkflowInitiationFormForTest(testData.Provider, testData.ComplianceRiskStatus))
			{
				form.Show();

				CombineAssertions("Security rights user controls:", () =>
				{
					var expectedPromptLabel = $@"The commodities on this job may need to be checked for compliance risk before proceeding.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

{securityCore.ShipmentsComplianceAllowComplianceAssessment.DisplayTextPathToSecurityRight}
{securityCore.ShipmentsComplianceDeclineComplianceAssessment.DisplayTextPathToSecurityRight}";

					AssertEquals(expectedPromptLabel, form.PromptLabelExposed.Text);
					AssertEquals(true, form.OKSecurityButtonExposed.Visible);
					AssertEquals(false, form.YesButtonExposed.Visible);
					AssertEquals(false, form.NoButtonExposed.Visible);
				});

				form.OKSecurityButtonExposed.PerformClick();

				CombineAssertions("Should create StmComplianceEvent log: Compliance Assessment Decision Required", () =>
				{
					var eventLog = testData.ComplianceRiskStatus.GetEventLogs().Single(u => u.SCE_EventType == AutoEvents.ComplianceRiskInteraction.Code);
					AssertNotNull(eventLog);
					AssertEquals("CRI", eventLog.SCE_EventType);
					AssertEquals("REQ", eventLog.SCE_EventSubType);
					AssertEquals("|MST=Compliance Assessment|NEW=REQ", eventLog.SCE_EventReference);
				});

				CombineAssertions("Should create StmALog log: Compliance Assessment Decision Required", () =>
				{
					var log = (testData.Shipment as CommonShipment).Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.ComplianceRiskInteraction.Code)).Single();
					AssertEquals("|MST=Compliance Assessment|NEW=REQ", log.SL_Reference);
				});
			}
		}

		public void TestYesButtonClick_WhenAllowAssessmentSecurityCheckpointNotGranted_SecurityErrorMessageShown()
		{
			var testData = CreateNewTestDataAndSave();
			var securityCore = new ComplianceRiskHelperTest().CreateNewSecurityCore;
			securityCore.ShipmentsComplianceAllowComplianceAssessment.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			using (var form = new ComplianceWorkflowInitiationFormForTest(testData.Provider, testData.ComplianceRiskStatus))
			{
				UnitTestUserNotification.Instance.ClearMessages();

				form.Show();
				form.YesButtonExposed.PerformClick();

				CombineAssertions("Security checkpoint allow assessment -> FALSE, should not initialize assessment", () =>
				{
					AssertEquals("form.Visible", true, form.Visible);
					AssertEquals("UserNotificationLastMessage", securityCore.ShipmentsComplianceAllowComplianceAssessment.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("ComplianceRiskStatus.IsAssessmentInitialized should be False", false, testData.ComplianceRiskStatus.IsAssessmentInitialized);
				});
			}
		}

		public void TestYesButtonClick_InitializeComplianceAssessmentWithConcurrencyError()
		{
			YesAndNoButtonClick_ComplianceAssessmentWithConcurrencyError(true);
		}

		public void TestNoButtonClick_DeclineComplianceAssessmentWithConcurrencyError()
		{
			YesAndNoButtonClick_ComplianceAssessmentWithConcurrencyError(false);
		}

		void YesAndNoButtonClick_ComplianceAssessmentWithConcurrencyError(bool isInitialize)
		{
			ErrorReporter.Instance.Clear();
			var testData = CreateNewTestDataAndSave();
			using (var form = new ComplianceWorkflowInitiationFormForTest(testData.Provider, testData.ComplianceRiskStatus))
			{
				UnitTestUserNotification.Instance.ClearMessages();
				BusinessObjectFactory.SetOnFactorySaveHookForTest(_ => throw new ZSaveConcurrencyException(new ZSaveConcurrencyException(new ZDataConcurrencyException(new InvalidOperationException("~ConcurrencyError~"), ((IBusinessObjectInternals)testData.ComplianceRiskStatus).Row, Db.Connection), Factory)));
				form.Show();
				if (isInitialize)
				{
					form.YesButtonExposed.PerformClick();
					AssertEquals("Failed to initialize Compliance Assessment due to concurrency error. Please reload the form and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
				else
				{
					form.NoButtonExposed.PerformClick();
					AssertEquals("Failed to decline Compliance Assessment due to concurrency error. Please reload the form and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
				
				AssertEquals(0, ErrorReporter.TotalErrorCount);
			}
		}

		(IForwardingShipment Shipment, IComplianceItemRiskStatusProvider Provider, ComplianceRiskStatus ComplianceRiskStatus) CreateNewTestDataAndSave()
		{
			var shipment = Factory.New<IForwardingShipment>();
			var provider = (IComplianceItemRiskStatusProvider)shipment;
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = (shipment as BusinessObject).TablePrefix;
			Factory.Save();

			return (shipment, provider, complianceRiskStatus);
		}

		protected override Form GetFormToBashCore()
		{
			var shipment = Factory.New<IForwardingShipment>();
			var provider = (IComplianceItemRiskStatusProvider)shipment;
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = (shipment as BusinessObject).TablePrefix;
			Factory.Save();

			return new ComplianceWorkflowInitiationForm(provider, complianceRiskStatus);
		}

		protected override bool ShouldTestFormIsFullyTranslatable => false;

		public class ComplianceWorkflowInitiationFormForTest : ComplianceWorkflowInitiationForm
		{
			public ComplianceWorkflowInitiationFormForTest(IComplianceItemRiskStatusProvider complianceRiskStatusProvider, ComplianceRiskStatus complianceRiskStatus) : base(complianceRiskStatusProvider, complianceRiskStatus)
			{
			}

			public ZButton YesButtonExposed => YesButton;
			public ZButton NoButtonExposed => NoButton;
			public ZButton OKSecurityButtonExposed => OKSecurityButton;
			public ZLabel PromptLabelExposed => PromptLabel;
			public ZLinkLabel ViewComplianceRiskLabelExposed => ViewComplianceRiskLabel;
		}
	}
}
