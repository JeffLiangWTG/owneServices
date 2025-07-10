using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Excel;
using Moq;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ExcelExporterGuiNotificationsTest : TestCaseWithDummy
	{
		public void TestShowExcelNotInstalledAndCurrentUserHasNoEmailError()
		{
			ExporterNotifications.ShowExcelNotInstalledAndCurrentUserHasNoEmailError("He-Man");
			AssertLastMessageIsHeManError();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		public void TestShowNoRecordsToExportMessage()
		{
			ExporterNotifications.ShowNoRecordsToExportError("He-Man");
			AssertLastMessageIsHeManError();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		public void TestShowMaxRecordsSupportedByExcelExceededError()
		{
			ExporterNotifications.ShowMaxEntriesSupportedByExcelExceededError("He-Man");
			AssertLastMessageIsHeManError();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		#region Testing the ProgressForm

		public void TestNotifyExportingLotsOfRecords()
		{
			ExporterNotifications.NotifyExportingLotsOfRecords(ExcelExporter); // hooks into Exporter events

			ProgressFormMock.Setup(o => o.SetStatusAndPercentComplete("Initializing Export...", 0)).Verifiable();
			ProgressFormMock.Setup(o => o.ShowModalTo(ExporterNotifications.ParentFormExposed)).Verifiable();
			ExcelExporter.FireExporterStarting();
			ProgressFormMock.Verify();

			ProgressFormMock.Setup(o => o.SetStatusAndPercentComplete("Exporting 1 of 2 records.", 50)).Verifiable();
			ExcelExporter.FireRecordExported();
			ProgressFormMock.Verify();

			ProgressFormMock.Setup(o => o.SetStatusAndPercentComplete("Displaying results in Excel...", 100)).Verifiable();
			ProgressFormMock.Setup(o => o.Hide()).Verifiable();
			ProgressFormMock.Setup(o => o.Dispose()).Verifiable();
			ExcelExporter.FireExportFinished();
			ProgressFormMock.Verify();

			AssertProgressFormCleanUp(ExcelExporter, ProgressFormMock);
		}

		public void TestNotifyExportingLotsOfRecordsWithCancel()
		{
			ExporterNotifications.NotifyExportingLotsOfRecords(ExcelExporter); // hooks into Exporter events
			ExcelExporter.FireExporterStarting(); // start the export
			AssertEquals("Precondition - ExcelExporter.CancelExportCalled should be false.", false, ExcelExporter.CancelExportCalled);

			ProgressFormMock.Raise(o => o.Cancelled += null, EventArgs.Empty);
			AssertEquals("Raising the cancel event on the ProgressBar should have cancelled the export.", true, ExcelExporter.CancelExportCalled);
			AssertProgressFormCleanUp(ExcelExporter, ProgressFormMock);
		}

		void AssertProgressFormCleanUp(ExcelExporterForTesting excelExporter, Mock<IProgressForm> progressFormMock)
		{
			AssertNull(ExporterNotifications.ProgressFormExposed);
			AssertNull(ExporterNotifications.ExporterExposed);
			AssertNull(ExporterNotifications.ParentFormExposed);

			excelExporter.FireExporterStarting();
			AssertReferencesAreRemoved();

			excelExporter.FireRecordExported();
			AssertReferencesAreRemoved();

			excelExporter.FireExportFinished();
			AssertReferencesAreRemoved();

			progressFormMock.VerifyAll();
		}

		void AssertReferencesAreRemoved()
		{
			AssertNull("Notification events should be unhooked and firing them should not instantiate the ProgressForm.", ExporterNotifications.ProgressFormExposed);
			AssertNull("Notification events should be unhooked and firing them should not set a reference the Exporter.", ExporterNotifications.ExporterExposed);
			AssertNull("Notification events should be unhooked and firing them should not set a reference the Parent Form.", ExporterNotifications.ParentFormExposed);
		}

		class ExcelExporterForTesting : ExcelExporter
		{
			public ExcelExporterForTesting(IBusinessObjectCollection collectionToExport, List<ExcelExportColumnBase> excelColumns, IExcelExporterNotifications notifications)
				: base(collectionToExport, excelColumns, notifications)
			{
			}

			public void FireExporterStarting()
			{
				base.OnExportStarting();
			}

			public void FireExportFinished()
			{
				base.OnExportFinished();
			}

			public void FireRecordExported()
			{
				base.OnRecordExported(1);
			}

			public override void CancelExport()
			{
				base.CancelExport();
				CancelExportCalled = true;
			}

			public bool CancelExportCalled;
		}

		internal class ExcelExporterGuiNotificationsForTest : ExcelExporterGuiNotifications
		{
			public ExcelExporterGuiNotificationsForTest(Form parentForm)
				: base(parentForm)
			{
			}

			public ExcelExporterGuiNotificationsForTest(Form parentForm, IProgressForm progressForm)
				: base(parentForm)
			{
				this.ProgressFormFromConstructor = progressForm;
			}

			public ExcelExporter ExporterExposed
			{
				get { return Exporter; }
			}

			public Form ParentFormExposed
			{
				get { return ParentForm; }
			}

			public IProgressForm ProgressFormExposed
			{
				get { return progressForm; }
			}

			protected override IProgressForm GetNewProgressForm()
			{
				return ProgressFormFromConstructor;
			}

			readonly IProgressForm ProgressFormFromConstructor;
		}

		#endregion

		#region Implementation

		void AssertLastMessageIsHeManError()
		{
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertEquals("He-Man", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var columns = new List<ExcelExportColumnBase>();

			Dummy.Collection.RemoveAll();
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			AssertEquals("Precondition - Collection should have 2 records.", 2, Dummy.Collection.Count);

			ParentForm = new ZForm();
			ProgressFormMock = new Mock<IProgressForm>();
			ExporterNotifications = new ExcelExporterGuiNotificationsForTest(ParentForm, ProgressFormMock.Object);
			ExcelExporter = new ExcelExporterForTesting(Dummy.Collection, columns, ExporterNotifications);
		}

		protected override void TearDown()
		{
			if (ParentForm != null)
			{
				ParentForm.Dispose();
			}
			base.TearDown();
		}

		Mock<IProgressForm> ProgressFormMock;
		ExcelExporterGuiNotificationsForTest ExporterNotifications;
		ExcelExporterForTesting ExcelExporter;
		ZForm ParentForm;

		#endregion
	}
}
