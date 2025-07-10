using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.PeriodManagement.Testing
{
	[TestedType(typeof(ReSetPeriodsForm))]
	public class ReSetPeriodsFormTest : ZFormBasherTest
	{
		#region 1. LogOut

		public void TestLogOutLabelHasCorrectText()
		{
			using (var testForm = GetFormToBash())
			{
				var label = testForm.GetControl<ZLabel>("LogOutLabel");
				AssertEquals("1. Logout All Users", label.CaptionResourceString.Caption);
			}
		}

		#endregion

		#region 2. BackUp

		public void TestBackUp_LabelAndButtonHaveCorrectText()
		{
			AssertLabelAndButtonHaveCorrectText(
				"BackUp",
				"2. Backup database",
				"Open Backup Dialog");
		}

		public void TestBackUpButton()
		{
			PerformButtonClick("BackUp");
		}

		#endregion

		#region 3. DeletePeriods

		public void TestDeletePeriods_LabelAndButtonHaveCorrectText()
		{
			AssertLabelAndButtonHaveCorrectText(
				"DeletePeriods",
				"3a. Delete All Existing Periods",
				"Delete Periods");
		}

		public void TestDeletePeriodsButton()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			PerformButtonClick("DeletePeriods");
			AssertContains("LastMessage", "All periods have been deleted", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestDeletePeriodsRange_LabelAndButtonHaveCorrectText()
		{
			AssertLabelAndButtonHaveCorrectText(
				"DeletePeriodsFrom",
				"3b. Delete Periods From Specified Date",
				"Delete Periods From");
		}

		public void TestDeletePeriodsRangeButton()
		{
			PerformButtonClick("DeletePeriodsFrom");

			AssertType<DeletePeriodsFromForm>(ZFormModaliser.LastFormShownDialogForTest);
			ZFormModaliser.LastFormShownDialogForTest.Close();
		}

		#endregion

		#region 4. CreatePeriods

		public void TestCreatePeriodsButton_LabelAndButtonHaveCorrectText()
		{
			AssertLabelAndButtonHaveCorrectText(
				"CreatePeriods",
				"4a. Create New Periods",
				"Create Periods");
		}

		[TestDate(2022, 05, 25)]
		public void TestCreatePeriodsButtonClick()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2021, 1, 1));
			Factory.Save();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			PerformButtonClick("CreatePeriods");
		}

		#endregion

		#region 4b. Extend Last Financial Year

		public void TestExtendLastFinancialYear_LabelAndButtonHaveCorrectText()
		{
			AssertLabelAndButtonHaveCorrectText(
				"ExtendLastFinancialYear",
				"4b. Extend Last Financial Year",
				"Extend Last Financial Year");
		}

		public void TestExtendLastFinancialYearButton()
		{
			PerformButtonClick("ExtendLastFinancialYear");

			AssertType<ExtendLastFinancialYearForm>(ZFormModaliser.LastFormShownDialogForTest);
			ZFormModaliser.LastFormShownDialogForTest.Close();
		}

		#endregion

		#region 5. updateTaxGLMovementRecords

		public void TestSetNewPeriodOnTransactions_LabelAndButtonHaveCorrectText()
		{
			AssertLabelAndButtonHaveCorrectText(
				"updateTaxGLMovementRecords",
				"5. Set New Period on Tax GL Movement",
				"Update Tax GL Movement Records");
		}

		public void TestSetNewPeriodOnTransactions_UserPromtText()
		{
			PerformButtonClick("updateTaxGLMovementRecords");
			AssertEquals("Update Records", UnitTestUserNotification.Instance.LastMessage.Caption);
			AssertEquals("Are you sure you want to update periods on Transactions in the company [Eagle Datamation International]?", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSetNewPeriodOnTransactions_GLMovements()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);
			PerformButtonClick("updateTaxGLMovementRecords");
			taxProcessorMock.Verify(x => x.RecalculatePeriodForAllGLMovementRecords(It.IsAny<ZGuid>()), Times.Never);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			PerformButtonClick("updateTaxGLMovementRecords");
			taxProcessorMock.Verify(x => x.RecalculatePeriodForAllGLMovementRecords(It.IsAny<ZGuid>()), Times.Once);
			taxProcessorMock.Verify(x => x.RecalculatePeriodForAllGLMovementRecords(GlbCompany.CurrentCompany.PK));

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), TestObjectCreator.NonCurrentCompany.ActiveBranches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				taxProcessorMock.Reset();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				PerformButtonClick("updateTaxGLMovementRecords");
				taxProcessorMock.Verify(x => x.RecalculatePeriodForAllGLMovementRecords(It.IsAny<ZGuid>()), Times.Once);
				taxProcessorMock.Verify(x => x.RecalculatePeriodForAllGLMovementRecords(TestObjectCreator.NonCurrentCompany.PK));
			}
		}

		#endregion

		#region 6. updateGeneralLedgerDataRecords 

		public void TestSetNewPeriodOnGLD_LabelAndButtonHaveCorrectText()
		{
			AssertLabelAndButtonHaveCorrectText(
				"updateGeneralLedgerRecords",
				"6. Set New Period on GLD",
				"Update General Ledger Data Records");
		}

		public void TestSetNewPeriodOnGLD_ButtonClickOK()
		{
			PerformButtonClick("updateGeneralLedgerRecords");

			AssertType<UpdateGldPeriodsDateRangeForm>(ZFormModaliser.LastFormShownDialogForTest);
			ZFormModaliser.LastFormShownDialogForTest.Close();
		}

		#endregion

		#region 7. reAggregate

		public void TestReAggregate_LabelAndButtonHaveCorrectText()
		{
			AssertLabelAndButtonHaveCorrectText(
				"reAggregate",
				"7. Re-Aggregate GL",
				"Re-Aggregate GL");
		}

		public void TestReAggregateButtonClick()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			PerformButtonClick("reAggregate");
		}

		#endregion

		#region Implementation

		PeriodManagementForm PeriodManagementForm;

		protected override Form GetFormToBashCore()
		{
			PeriodManagementForm = new PeriodManagementForm();
			var reSetPeriodsForm = new ReSetPeriodsForm(PeriodManagementForm);
			reSetPeriodsForm.Disposed += ReSetPeriodsForm_Disposed;
			return reSetPeriodsForm;
		}

		void ReSetPeriodsForm_Disposed(object sender, EventArgs e)
		{
			if (PeriodManagementForm != null)
			{
				PeriodManagementForm.Dispose();
			}
		}

		void AssertLabelAndButtonHaveCorrectText(string actionName, string labelText, string buttonText)
		{
			using (var testForm = GetFormToBash())
			{
				var label = testForm.GetControl<ZLabel>(actionName + "Label");
				AssertEquals("Label", labelText, label.CaptionResourceString.Caption);

				var button = testForm.GetControl<ZButton>(actionName + "Button");
				AssertEquals("Button", buttonText, button.CaptionResourceString.Caption);
			}
		}

		void PerformButtonClick(string actionName)
		{
			using (var testForm = GetFormToBash())
			{
				testForm.Show();
				var button = testForm.GetControl<ZButton>(actionName + "Button");
				button.PerformClick();
			}
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		#endregion
	}
}
