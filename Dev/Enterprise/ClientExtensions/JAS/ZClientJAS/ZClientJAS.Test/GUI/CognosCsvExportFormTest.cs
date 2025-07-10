using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.JAS.Business;
using Enterprise.Client.JAS.Business.Cognos;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.GUI.Cognos
{
	public class CognosCsvExportFormTest : TestCaseWithFactory
	{
		public void TestFormHeading()
		{
			AssertEquals("Cognos Data Export", ExportForm.FormHeading);
		}

		public void TestBusinessEntity()
		{
			AssertNotNull(ExportForm.BusinessEntity);
		}

		public void TestExportButtonClick_DataExporterHasErrors()
		{
			ExportForm.Show();
			ExportForm.ExportButton.PerformClick();
			Assert("Should show an error dialog", UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertEquals("Should show an error dialog", "There are errors that need to be fixed before Cognos data can be exported", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestExportButtonClick_ExportFails()
		{
			ExportForm.Show();
			DataExporter.DeliveryMethod = JASDataExporterBizO.DirectoryDeliveryMethodCode;
			DataExporter.ExportDirectory = Env.TempPath;
			DataExporter.EndingPeriod = 200601;
			DataExporter.ExportShouldFail = true;
			Assert("Pre-condition", ExportForm.ExportButton.Enabled);
			Assert("Pre-condition", ExportForm.CloseButton.Enabled);
			Assert("Pre-condition", ExportForm.EndPeriodCalcEdit.Enabled);
			Assert("Pre-condition", ExportForm.jasDataExporterUserControl1.Enabled);
			ExportForm.ExportButton.PerformClick();
			Assert("Should show an error dialog", UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertEquals("Should show an error dialog", "An error has occurred during export. Review the export log for details.", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("Should be disabled after export", !ExportForm.ExportButton.Enabled);
			Assert("Should be disabled after export", !ExportForm.jasDataExporterUserControl1.Enabled);
			Assert("Should be disabled after export", !ExportForm.EndPeriodCalcEdit.Enabled);
			Assert("Should be re-enabled", ExportForm.CloseButton.Enabled);
		}

		public void TestExportButtonClick_ExportSuccess()
		{
			ExportForm.Show();
			DataExporter.DeliveryMethod = JASDataExporterBizO.DirectoryDeliveryMethodCode;
			DataExporter.ExportDirectory = Env.TempPath;
			DataExporter.EndingPeriod = 200601;
			Assert("Pre-condition", ExportForm.ExportButton.Enabled);
			Assert("Pre-condition", ExportForm.CloseButton.Enabled);
			Assert("Pre-condition", ExportForm.EndPeriodCalcEdit.Enabled);
			Assert("Pre-condition", ExportForm.jasDataExporterUserControl1.Enabled);
			ExportForm.ExportButton.PerformClick();
			Assert("Should show an information dialog", UnitTestUserNotification.Instance.LastMessage.WasInformation);
			AssertEquals("Should show an information dialog", "Cognos data has been successfully exported.", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("Should be disabled after export", !ExportForm.ExportButton.Enabled);
			Assert("Should be disabled after export", !ExportForm.jasDataExporterUserControl1.Enabled);
			Assert("Should be disabled after export", !ExportForm.EndPeriodCalcEdit.Enabled);
			Assert("Should be re-enabled", ExportForm.CloseButton.Enabled);
		}

		public void TestCloseButtonClick()
		{
			ExportForm.Show();
			Assert(ExportForm.Visible);
			ExportForm.CloseButton.PerformClick();
			Assert(!ExportForm.Visible);
		}

		#region ICognosNotificationSubscriber
		public void TestAdvanceProgressBy()
		{
			AssertEquals("Pre-condition", 0, ExportForm.ExportProgressBar.Value);
			CognosNotificationSubscriber.AdvanceProgressBy(5);
			AssertEquals("Should be advanced by 5%", 5, ExportForm.ExportProgressBar.Value);
			CognosNotificationSubscriber.AdvanceProgressBy(15);
			AssertEquals("Should be advanced by 15%", 20, ExportForm.ExportProgressBar.Value);
		}

		public void TestCompleteProgress()
		{
			AssertEquals("Pre-condition", 0, ExportForm.ExportProgressBar.Value);
			ExportForm.ExportProgressBar.Value = 15;
			CognosNotificationSubscriber.CompleteProgress();
			AssertEquals("Should be 100%", 100, ExportForm.ExportProgressBar.Value);
		}

		public void TestHasErrors()
		{
			Assert("Pre-condition", !CognosNotificationSubscriber.HasErrors);
			CognosNotificationSubscriber.Notify(new InfoNotification("only info"));
			Assert("no errors notified", !CognosNotificationSubscriber.HasErrors);
			CognosNotificationSubscriber.Notify(new ErrorNotification(ErrorType.Error, "Error"));
			Assert("Should have error now", CognosNotificationSubscriber.HasErrors);
		}

		public void TestNotify()
		{
			ExportForm.Show();
			AssertEquals("Pre-condition", 0, ExportForm.SummaryTextBox.Text.Length);
			// set the focus somewhere else other than the SummaryTextBox
			ExportForm.ExportButton.Focus();
			CognosNotificationSubscriber.Notify(new InfoNotification("MEH MEH"));
			Assert("Focus should now be moved to the SummaryTextBox", ExportForm.SummaryTextBox.Focused);
			AssertEquals("Should be added to the SummaryTextBox", "MEH MEH", ExportForm.SummaryTextBox.Text);
		}

		ICognosNotificationSubscriber CognosNotificationSubscriber
		{
			get
			{
				return ExportForm;
			}
		}

		#endregion
		#region Test Classes
		class CognosDataExporterBizOForTest : CognosDataExporterBizO
		{
			public override bool Export(ICognosNotificationSubscriber notificationSubscriber)
			{
				if (Exporting != null)
				{
					Exporting(this, EventArgs.Empty);
				}

				return !ExportShouldFail;
			}

			public event EventHandler Exporting;
			public bool ExportShouldFail;
		}

		#endregion
		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			DataExporter = new CognosDataExporterBizOForTest();
			DataExporter.Exporting += new EventHandler(AssertControlsState);
			ExportForm = new CognosCsvExportForm(DataExporter);
			CreatePeriod(200601, new ZDateTime(2006, 1, 1), new ZDateTime(2006, 1, 31));
		}

		void CreatePeriod(ZInt period, ZDateTime start, ZDateTime end)
		{
			AccPeriodManagement periodManagement = Factory.New<AccPeriodManagement>();
			periodManagement.AM_Period = period;
			periodManagement.AM_StartDate = start;
			periodManagement.AM_EndDate = end;
			periodManagement.AM_Year = (ZShort)start.Year;
			periodManagement.AM_GC_Company = GlbCompany.CurrentCompany.PK;
			Factory.Save();
		}

		void AssertControlsState(object sender, EventArgs e)
		{
			Assert("Should be disabled while exporting", !ExportForm.ExportButton.Enabled);
			Assert("Should be disabled while exporting", !ExportForm.CloseButton.Enabled);
			Assert("Should be disabled while exporting", !ExportForm.jasDataExporterUserControl1.Enabled);
		}

		protected override void TearDown()
		{
			ExportForm.Dispose();
			base.TearDown();
		}

		[TestedType(typeof(CognosCsvExportForm))]
		class BasherTest : ZFormBasherTest
		{
			protected override Form GetFormToBashCore()
			{
				return new CognosCsvExportForm(new CognosDataExporterBizO());
			}
		}

		CognosCsvExportForm ExportForm;
		CognosDataExporterBizOForTest DataExporter;
		#endregion
	}
}
