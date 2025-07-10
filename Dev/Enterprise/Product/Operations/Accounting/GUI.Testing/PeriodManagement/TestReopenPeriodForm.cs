using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.PeriodManagement.Testing
{
	public class TestReopenPeriodForm : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbStaff testStaff = Factory.NewWithValidTestData<GlbStaff>();
			testStaff.GS_EmailAddress = "test@test.com";
			GlbGroup testGroup = Factory.NewWithValidTestData<GlbGroup>();
			testGroup.Staff.Add(testStaff);
			Factory.Save();
			AccountingConfigurationRegistry.Instance.PeriodReopenNotifyGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testGroup.PK.ToGuid());
		}

		[TestDate(2009, 07, 24)]
		public void TestReopenButton_Click()
		{
			AccPeriodManagement period = Factory.NewWithValidTestData<AccPeriodManagement>();
			period.AM_Period = 200901;
			period.AM_IsGeneralLedgerClosed = true;
			period.AM_IsSubLedgerClosed = true;
			period.AM_IsSubledgerClosedForAdjustments = true;
			using (ReopenPeriodForm form = new ReopenPeriodForm(period))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				form.ReopenSubLedgerPeriodCheckBox_ForTestOnly.Checked = true;
				form.ReopenGeneralLedgerPeriodCheckBox_ForTestOnly.Checked = true;
				form.ReopenForAdjustmentsCheckBox_ForTestOnly.Checked = true;

				form.ReopenButton_Click_ForTestOnly(null, null);
				AssertEquals("Last message", "Period 200901 is Reopened for: Sub ledger, General ledger, Adjustments.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!period.AM_IsGeneralLedgerClosed);
				Assert(!period.AM_IsSubLedgerClosed);
				Assert(!period.AM_IsSubledgerClosedForAdjustments);
				AssertEquals("1 Email should be created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		[TestDate(2009, 08, 12)]
		public void TestReopenPeriodsWorksForNonControllerUsers()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "imraan";
			staff.StaffPlainTextPassword = "password";
			Factory.Save();

			AccPeriodManagement period = Factory.NewWithValidTestData<AccPeriodManagement>();
			period.AM_Period = 200901;
			period.AM_IsGeneralLedgerClosed = true;
			period.AM_IsSubLedgerClosed = true;
			period.AM_IsSubledgerClosedForAdjustments = true;
			using (Env.SetTemporaryUserContext("imraan", GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (ReopenPeriodForm form = new ReopenPeriodForm(period))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				form.ReopenSubLedgerPeriodCheckBox_ForTestOnly.Checked = true;
				form.ReopenGeneralLedgerPeriodCheckBox_ForTestOnly.Checked = true;
				form.ReopenForAdjustmentsCheckBox_ForTestOnly.Checked = true;
				form.ReopenButton_Click_ForTestOnly(null, null);
				AssertEquals("Last message", "Period 200901 is Reopened for: Sub ledger, General ledger, Adjustments.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!period.AM_IsGeneralLedgerClosed);
				Assert(!period.AM_IsSubLedgerClosed);
				Assert(!period.AM_IsSubledgerClosedForAdjustments);
				AssertEquals("1 Email should be created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}
	}
}
