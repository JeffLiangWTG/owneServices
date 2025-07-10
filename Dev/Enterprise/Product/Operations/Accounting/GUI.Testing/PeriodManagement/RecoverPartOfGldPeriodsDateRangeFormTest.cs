using System;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.GeneralLedgerData.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Accounting.GUI.PeriodManagement.Testing
{
	[TestedType(typeof(RecoverPartOfGldPeriodsDateRangeForm))]
	public class RecoverPartOfGldPeriodsDateRangeFormTest : GldDateRangeFormTest<RecoverPartOfGldPeriodsDateRangeSetting>
	{
		public void TestFormGUI()
		{
			using (var testForm = (RecoverPartOfGldPeriodsDateRangeForm)GetFormToBash())
			{
				testForm.Show();
				AssertEquals("Regenerate GLD (This Company Only)", testForm.Text);

				var okButton = testForm.GetControl<ZButton>("OKButton");
				AssertEquals("Continue", okButton.Text);
			}
		}

		public override void TestOKButtonClick()
		{
			var mockServiceTaskNudger = new Mock<IServiceTaskNudger>();
			var mockDataRecover = CreateMockGeneralLedgerDataQueue();

			using (ObjectFactory.Substitute(mockServiceTaskNudger.Object))
			using (ObjectFactory.Substitute(mockDataRecover.Object))
			using (var testForm = GetFormToBash())
			{
				testForm.Show();
				var okButton = testForm.GetControl<ZButton>("OKButton");

				mockDataRecover.Verify(x => x.RemoveNarrowGLD(It.IsAny<DbConnection>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())
					, Times.Exactly(0)
					, "PreCondition");
				mockDataRecover.Verify(x => x.QueueTransaction(It.IsAny<DbConnection>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<bool>())
					, Times.Exactly(0)
					, "PreCondition");

				var currentCompnay = GlbCompany.CurrentCompany;
				using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					AssertOKButtonClick(currentCompnay, new DateTime(2022, 07, 10), new DateTime(2022, 07, 12), okButton, isContinue: false);
					AssertGeneralLedgerDataQueue(mockDataRecover
						, passingCompany: currentCompnay
						, passingStartDate: new DateTime(2022, 07, 10)
						, passingEndDate: new DateTime(2022, 07, 13)
						, expectedTotalTimesRemoveNarrowGLD: 0
						, expectedTotalTimesQueueTransaction: 0
						, expectedTimesRemoveNarrowGLD: 0
						, expectedTimesQueueTransaction: 0
					);
					mockServiceTaskNudger.Verify(x => x.NudgeServiceTask(It.IsAny<string>(), It.IsAny<TimeSpan?>()), Times.Exactly(0));

					AssertOKButtonClick(currentCompnay, new DateTime(2022, 07, 10), new DateTime(2022, 07, 12), okButton, isContinue: true);
					AssertGeneralLedgerDataQueue(mockDataRecover
						, passingCompany: currentCompnay
						, passingStartDate: new DateTime(2022, 07, 10)
						, passingEndDate: new DateTime(2022, 07, 13)
						, expectedTotalTimesRemoveNarrowGLD: 1
						, expectedTotalTimesQueueTransaction: 1
						, expectedTimesRemoveNarrowGLD: 1
						, expectedTimesQueueTransaction: 1
					);
					mockServiceTaskNudger.Verify(x => x.NudgeServiceTask(It.IsAny<string>(), It.IsAny<TimeSpan?>()), Times.Exactly(1));
					mockServiceTaskNudger.Verify(x => x.NudgeServiceTask("GLP", null), Times.Exactly(1));
				}

				var anotherCompnay = TestObjectCreator.NonCurrentCompany;
				using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser, TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK)))
				{
					AssertOKButtonClick(anotherCompnay, new DateTime(2022, 07, 09), new DateTime(2022, 07, 18), okButton, isContinue: false);
					AssertGeneralLedgerDataQueue(mockDataRecover
						, passingCompany: anotherCompnay
						, passingStartDate: new DateTime(2022, 07, 09)
						, passingEndDate: new DateTime(2022, 07, 19)
						, expectedTotalTimesRemoveNarrowGLD: 1
						, expectedTotalTimesQueueTransaction: 1
						, expectedTimesRemoveNarrowGLD: 0
						, expectedTimesQueueTransaction: 0
					);
					mockServiceTaskNudger.Verify(x => x.NudgeServiceTask(It.IsAny<string>(), It.IsAny<TimeSpan?>()), Times.Exactly(1));

					AssertOKButtonClick(anotherCompnay, new DateTime(2022, 07, 09), new DateTime(2022, 07, 18), okButton, isContinue: true);
					AssertGeneralLedgerDataQueue(mockDataRecover
						, passingCompany: anotherCompnay
						, passingStartDate: new DateTime(2022, 07, 09)
						, passingEndDate: new DateTime(2022, 07, 19)
						, expectedTotalTimesRemoveNarrowGLD: 2
						, expectedTotalTimesQueueTransaction: 2
						, expectedTimesRemoveNarrowGLD: 1
						, expectedTimesQueueTransaction: 1
					);
					mockServiceTaskNudger.Verify(x => x.NudgeServiceTask(It.IsAny<string>(), It.IsAny<TimeSpan?>()), Times.Exactly(2));
					mockServiceTaskNudger.Verify(x => x.NudgeServiceTask("GLP", null), Times.Exactly(2));
				}
			}

			void AssertOKButtonClick(GlbCompany currentCompany, DateTime startDate, DateTime endDate, ZButton button, bool isContinue)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				RecoverPartOfGldPeriodsDateRangeSetting.StartDate = startDate;
				RecoverPartOfGldPeriodsDateRangeSetting.EndDate = endDate;
				AssertNoErrors("PreCondition, setting has no error.", RecoverPartOfGldPeriodsDateRangeSetting);

				UnitTestUserNotification.Instance.AddAnswer(isContinue ? ZDialogResult.Yes : ZDialogResult.No);
				button.PerformClick();
				if (isContinue)
				{
					AssertEquals($"Are you sure you want to Delete and Regenerate all General Ledger Data in specified date range for Current Company [{currentCompany.CompanyName}]?", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
					AssertEquals($"General Ledger Data (GLD) for Period has been been deleted for {recoverPartOfGldPeriodsDateRangeSetting.StartDate.Date.ToISO8601ShortDateString()} to {recoverPartOfGldPeriodsDateRangeSetting.EndDate.Date.ToISO8601ShortDateString()}, it will be regenerated after the next Service Task cycle."
						, UnitTestUserNotification.Instance.LastMessage.Text);
				}
				else
				{
					AssertEquals($"Are you sure you want to Delete and Regenerate all General Ledger Data in specified date range for Current Company [{currentCompany.CompanyName}]?", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				AssertEquals("MustRollBack", false, Db.Connection.MustRollBack);
			}
		}

		public void TestRecoverPartOfGLDWhenErrorOnRemoveNarrowGLD()
		{
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);

			var mockDataRecover = CreateMockGeneralLedgerDataQueue();
			mockDataRecover.Setup(x => x.RemoveNarrowGLD(It.IsAny<DbConnection>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
				.Callback<DbConnection, Guid, DateTime, DateTime>(
					(connection, companyPK, startDate, endDate) => throw new Exception("Dummy Error on RemoveNarrowGLD")
				);

			using (ObjectFactory.Substitute(mockDataRecover.Object))
			using (var testForm = GetFormToBash())
			{
				testForm.Show();

				RecoverPartOfGldPeriodsDateRangeSetting.StartDate = new DateTime(2022, 07, 10);
				RecoverPartOfGldPeriodsDateRangeSetting.EndDate = new DateTime(2022, 07, 12);
				AssertNoErrors("PreCondition, setting has no error.", RecoverPartOfGldPeriodsDateRangeSetting);

				var okButton = testForm.GetControl<ZButton>("OKButton");
				var dummyExp = AssertExceptionThrown<Exception>(() => okButton.PerformClick());
				AssertEquals("Dummy Error on RemoveNarrowGLD", dummyExp.Message);
				AssertGeneralLedgerDataQueue(mockDataRecover
					, passingCompany: GlbCompany.CurrentCompany
					, passingStartDate: new DateTime(2022, 07, 10)
					, passingEndDate: new DateTime(2022, 07, 13)
					, expectedTotalTimesRemoveNarrowGLD: 1
					, expectedTotalTimesQueueTransaction: 0
					, expectedTimesRemoveNarrowGLD: 1
					, expectedTimesQueueTransaction: 0
				);
				AssertEquals("MustRollBack", true, Db.Connection.MustRollBack);
			}
		}

		public void TestRecoverPartOfGLDWhenDataSourceHasError()
		{
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);

			var mockDataRecover = CreateMockGeneralLedgerDataQueue();
			using (ObjectFactory.Substitute(mockDataRecover.Object))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			using (var testForm = GetGldDateRangeForm())
			{
				RecoverPartOfGldPeriodsDateRangeSetting.StartDate = new DateTime(2022, 07, 10);
				RecoverPartOfGldPeriodsDateRangeSetting.EndDate = new DateTime(2022, 07, 12);
				AssertNoErrors("PreCondition, setting has no error.", RecoverPartOfGldPeriodsDateRangeSetting);

				testForm.Show();
				var okButton = testForm.GetControl<ZButton>("OKButton");

				RecoverPartOfGldPeriodsDateRangeSetting.StartDateInfo.AddError("DummyError");
				okButton.PerformClick();
				AssertGeneralLedgerDataQueue(mockDataRecover
					, passingCompany: GlbCompany.CurrentCompany
					, passingStartDate: new DateTime(2022, 07, 10)
					, passingEndDate: new DateTime(2022, 07, 13)
					, expectedTotalTimesRemoveNarrowGLD: 0
					, expectedTotalTimesQueueTransaction: 0
					, expectedTimesRemoveNarrowGLD: 0
					, expectedTimesQueueTransaction: 0
				);

				RecoverPartOfGldPeriodsDateRangeSetting.StartDateInfo.ClearAllNotifications();
				okButton.PerformClick();
				AssertGeneralLedgerDataQueue(mockDataRecover
					, passingCompany: GlbCompany.CurrentCompany
					, passingStartDate: new DateTime(2022, 07, 10)
					, passingEndDate: new DateTime(2022, 07, 13)
					, expectedTotalTimesRemoveNarrowGLD: 1
					, expectedTotalTimesQueueTransaction: 1
					, expectedTimesRemoveNarrowGLD: 1
					, expectedTimesQueueTransaction: 1
				);
			}
		}

		Mock<IGeneralLedgerDataQueue> CreateMockGeneralLedgerDataQueue()
		{
			var mockDataRecover = new Mock<IGeneralLedgerDataQueue>();
			mockDataRecover.Setup(x => x.RemoveNarrowGLD(It.IsAny<DbConnection>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
				.Callback<DbConnection, Guid, DateTime, DateTime>(
					(connection, companyPK, startDate, endDate) => Assert("IsInTransaction", connection.IsInTransaction)
				);

			mockDataRecover.Setup(x => x.QueueTransaction(It.IsAny<DbConnection>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<bool>()))
				.Callback<DbConnection, Guid, DateTime, DateTime, DateTime, bool>(
					(connection, companyPK, startDate, endDate, endSystemCreateTime, checkDuplicated) => Assert("IsInTransaction", connection.IsInTransaction)
				);
			return mockDataRecover;
		}

		void AssertGeneralLedgerDataQueue(Mock<IGeneralLedgerDataQueue> mockDataRecover
			, GlbCompany passingCompany, DateTime passingStartDate, DateTime passingEndDate
			, int expectedTotalTimesRemoveNarrowGLD, int expectedTotalTimesQueueTransaction
			, int expectedTimesRemoveNarrowGLD, int expectedTimesQueueTransaction)
		{
			mockDataRecover.Verify(x => x.RemoveNarrowGLD(It.IsAny<DbConnection>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())
				, Times.Exactly(expectedTotalTimesRemoveNarrowGLD));
			mockDataRecover.Verify(x => x.QueueTransaction(It.IsAny<DbConnection>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<bool>())
				, Times.Exactly(expectedTotalTimesQueueTransaction));

			mockDataRecover.Verify(
				x => x.RemoveNarrowGLD(Db.Connection, passingCompany.PK.ToGuid(), passingStartDate, passingEndDate)
				, Times.Exactly(expectedTimesRemoveNarrowGLD)
			);
			mockDataRecover.Verify(
				x => x.QueueTransaction(Db.Connection, passingCompany.PK.ToGuid(), passingStartDate, passingEndDate, DateTime.MaxValue, true)
				, Times.Exactly(expectedTimesQueueTransaction)
			);
		}

		[TestDate(2022, 12, 28)]
		public void TestHandleGLDComplianceReport_WhenRegenerateGLD()
		{
			var generalLedgerData = Factory.NewWithValidTestData<AccGeneralLedgerData>();
			generalLedgerData.GLD_Type = "PST";
			generalLedgerData.GLD_GLAccountType = "ARC";
			generalLedgerData.GLD_PostDate = ZDateTime.Today;
			generalLedgerData.GLD_PostPeriod = 1;
			generalLedgerData.GLD_GC_Company = GlbCompany.CurrentCompany.PK;
			generalLedgerData.GLD_GB_Branch = GlbBranch.CurrentBranch.PK;
			generalLedgerData.GLD_GE_Department = GlbDepartment.CurrentDepartment.PK;

			var complianceReportFIN = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReportFIN.ACR_ReportType = "TST";
			complianceReportFIN.ACR_DateFrom = ZDate.Today.AddDays(-2);
			complianceReportFIN.ACR_DateTo = ZDate.Today.AddDays(2);
			complianceReportFIN.ACR_Status = AccComplianceReport.Status.ReportFinalised;

			var complianceReportGEN = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReportGEN.ACR_ReportType = "ASD";
			complianceReportGEN.ACR_DateFrom = ZDate.Today.AddDays(-2);
			complianceReportGEN.ACR_DateTo = ZDate.Today.AddDays(2);
			complianceReportGEN.ACR_Status = AccComplianceReport.Status.ReportGenerated;

			Factory.Save();

			var sql = $@"INSERT INTO AccComplianceReportTransactionPivot (ACL_PK, ACL_ParentID, ACL_ParentTableCode, ACL_GC_Company, ACL_ACR_Report, ACL_ReportSequence) VALUES
(NEWID(), '{generalLedgerData.PK}', 'GLD', '{GlbCompany.CurrentCompany.PK}', '{complianceReportFIN.PK}', 1)";

			TestConnection.ExecuteNonQuery(sql);

			using (var testForm = GetFormToBash())
			{
				testForm.Show();
				var okButton = testForm.GetControl<ZButton>("OKButton");

				using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					RecoverPartOfGldPeriodsDateRangeSetting.StartDate = ZDateTime.Now;
					RecoverPartOfGldPeriodsDateRangeSetting.EndDate = ZDateTime.Now.AddDays(2);
					UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
					testForm.GetControl<ZButton>("OKButton").PerformClick();

					var msg = @"You are going to change the status of Compliance Report that uses the GLD data from ""FIN - Report Finalised"" to ""INV - Report Invalidated by Transactions Updated""";
					Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(msg));
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator.CreateTestPeriodsForEntireYear(2022);
			using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser, TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK)))
			{
				TestObjectCreator.CreateTestPeriodsForEntireYear(2022);
			}
		}

		protected override GldDateRangeForm<RecoverPartOfGldPeriodsDateRangeSetting> GetGldDateRangeForm()
			=> new RecoverPartOfGldPeriodsDateRangeForm(RecoverPartOfGldPeriodsDateRangeSetting);

		RecoverPartOfGldPeriodsDateRangeSetting RecoverPartOfGldPeriodsDateRangeSetting => recoverPartOfGldPeriodsDateRangeSetting ?? (recoverPartOfGldPeriodsDateRangeSetting = new RecoverPartOfGldPeriodsDateRangeSetting());
		RecoverPartOfGldPeriodsDateRangeSetting recoverPartOfGldPeriodsDateRangeSetting;
	}
}
