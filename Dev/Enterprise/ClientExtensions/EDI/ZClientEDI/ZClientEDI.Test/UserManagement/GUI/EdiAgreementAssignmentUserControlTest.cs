using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.EDI.UserManagement.GUI.Testing
{
	[TestedType(typeof(EdiAgreementAssignmentUserControl))]
	public class EdiAgreementAssignmentUserControlTest : TestCaseWithFactory
	{
		public void TestAgreementsGridReadOnly()
		{
			EDISecurityCheckpoints.UserAgreementAssignCorporateAgreements.IsAllowed = false;
			Assert(!EDISecurityCheckpoints.UserAgreementAssignCorporateAgreements.IsAllowed);
			var enterprise = GetTestData();
			using (var form = new UserControlTestForm(enterprise))
			{
				form.Show();
				var agreementTab = form.topTabControl.TabPages[0].Controls[0] as EdiAgreementAssignmentUserControl;

				var agreementGrid = agreementTab.Controls.Find("corporateAgreementsGrid", true)[0] as ZGrid;
				Assert(agreementGrid.ReadOnly);
			}

			EDISecurityCheckpoints.UserAgreementAssignCorporateAgreements.IsAllowed = true;
			Assert(EDISecurityCheckpoints.UserAgreementAssignCorporateAgreements.IsAllowed);
			using (var form = new UserControlTestForm(enterprise))
			{
				form.Show();
				var agreementTab = form.topTabControl.TabPages[0].Controls[0] as EdiAgreementAssignmentUserControl;

				var agreementGrid = agreementTab.Controls.Find("corporateAgreementsGrid", true)[0] as ZGrid;
				Assert(!agreementGrid.ReadOnly);
			}
		}

		public void TestAddAcceptanceButtonEvent_WithoutRight()
		{
			EDISecurityCheckpoints.UserAgreementAddAcceptanceLogs.IsAllowed = false;
			Assert(!EDISecurityCheckpoints.UserAgreementAddAcceptanceLogs.IsAllowed);
			var enterprise = GetTestData();
			using (var form = new UserControlTestForm(enterprise))
			{
				form.Show();
				var agreementTab = form.topTabControl.TabPages[0].Controls[0] as EdiAgreementAssignmentUserControl;

				var agreementGrid = agreementTab.Controls.Find("corporateAgreementsGrid", true)[0] as ZGrid;
				agreementGrid.Select(0);
				Assert(agreementGrid.SelectedElements[0] is EdiUserAgreementAssignment);

				var button = agreementTab.Controls.Find("addAcceptanceButton", true)[0] as ZButton;
				Assert("the button should not be enabled when missing the right", !button.Enabled);
			}
		}

		public void TestAddAcceptanceButtonEvent_WithRight()
		{
			EDISecurityCheckpoints.UserAgreementAddAcceptanceLogs.IsAllowed = true;
			Assert(EDISecurityCheckpoints.UserAgreementAddAcceptanceLogs.IsAllowed);
			var enterprise = GetTestData();
			using (var form = new UserControlTestForm(enterprise))
			{
				var eventMethod = typeof(ZGrid).GetMethod("OnSelectedRowsChangedInMouseDown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

				form.Show();
				var agreementTab = form.topTabControl.TabPages[0].Controls[0] as EdiAgreementAssignmentUserControl;
				var agreementGrid = agreementTab.Controls.Find("corporateAgreementsGrid", true)[0] as ZGrid;
				var button = agreementTab.Controls.Find("addAcceptanceButton", true)[0] as ZButton;
				var assignment = agreementGrid.ListManager.List[0] as EdiUserAgreementAssignment;

				assignment.EAE_SystemLastEditTimeUtc = ZDateTime.UtcNow.AddMinutes(1);
				Assert(assignment.HasChanges);
				agreementGrid.UnSelectAll();
				agreementGrid.Select(0);
				eventMethod.Invoke(agreementGrid, []);

				Assert(button.Enabled);
				button.PerformClick();
				AssertEquals("The selected assignment was changed, please save form and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAddAcceptanceButtonEvent_NullAgreement()
		{
			TestConnection.ExecuteNonQuery("DISABLE TRIGGER TG_INS_EdiUserAgreement ON EdiUserAgreement ; DISABLE TRIGGER TG_UPD_EdiUserAgreement ON EdiUserAgreement");

			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise.Organisation.OH_Code = "org001";

			var assignment = Factory.New<EdiUserAgreementAssignment>();
			assignment.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment.EAE_VariantCode = "";
			assignment.Parent = enterprise;

			Factory.Save();

			using (var form = new UserControlTestForm(enterprise))
			{
				var eventMethod = typeof(ZGrid).GetMethod("OnSelectedRowsChangedInMouseDown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

				form.Show();
				var agreementTab = form.topTabControl.TabPages[0].Controls[0] as EdiAgreementAssignmentUserControl;
				var agreementGrid = agreementTab.Controls.Find("corporateAgreementsGrid", true)[0] as ZGrid;
				var button = agreementTab.Controls.Find("addAcceptanceButton", true)[0] as ZButton;

				agreementGrid.UnSelectAll();
				agreementGrid.Select(0);
				eventMethod.Invoke(agreementGrid, []);

				var gridAssignment = agreementGrid.SelectedElements[0] as EdiUserAgreementAssignment;
				AssertNull(gridAssignment.CurrentAgreement);

				Assert(button.Enabled);
				button.PerformClick();
				AssertEquals("The selected assignment has no agreement, please create a agreement or select another agreement type.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		LicenceEnterprise GetTestData()
		{
			TestConnection.ExecuteNonQuery("DISABLE TRIGGER TG_INS_EdiUserAgreement ON EdiUserAgreement ; DISABLE TRIGGER TG_UPD_EdiUserAgreement ON EdiUserAgreement");

			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise.Organisation.OH_Code = "org001";

			var agreement = Factory.New<EdiUserAgreement>();
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement.ERA_VariantCode = "CA1";
			agreement.ERA_VariantDescription = "CA1";
			agreement.ERA_VersionNumber = 1;
			agreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(-2);

			var assignment = Factory.New<EdiUserAgreementAssignment>();
			assignment.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment.EAE_VariantCode = "CA1";
			assignment.Parent = enterprise;

			var acceptanceLog = Factory.New<EdiUserAgreementAcceptanceLog>();
			acceptanceLog.EUL_ERA = agreement.PK;
			acceptanceLog.EUL_AcceptanceTimeUtc = ZDateTime.UtcNow;
			acceptanceLog.EUL_LE = enterprise.PK;

			Factory.Save();

			return enterprise;
		}
	}

	class UserControlTestForm : ZChildForm
	{
		public UserControlTestForm(LicenceEnterprise enterprise) : base(enterprise)
		{
			PlugIns.Add(ClientControllerRegistration.EdiUserAgreementAssignment);
			topTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Controls.Add(topTabControl);
		}

		public ZTabControl topTabControl = new ZTabControl();
	}
}
