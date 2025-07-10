using System;
using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Cognos.Testing
{
	[TestedType(typeof(CognosDataExporterBizO))]
	class CognosDataExporterBizOTest : NonPersistentBusinessObjectTestCase
	{
		public void TestExportSummaryInfoShouldBeSetToReadOnlyOnConstruction()
		{
			Assert("Should be set to read-only in the constructor", DataExporter.ExportSummaryInfo.ReadOnly);
		}

		[TestDate(2006, 12, 31)]
		public void TestExport_FailToDeliver()
		{
			AssertEquals("Pre-condition", 0, NotificationBuffer.Events.Length);
			try
			{
				SetupForAccPeriodTest();
				DataExporter.ExceptionToBeThrownWhenDeliveringFiles = new IOException("MEH MEH");
				Assert("Should return false when file cannot be delivered", !DataExporter.Export(NotificationBuffer));
				AssertEquals("User should be notified of the IO error", 1, NotificationBuffer.Events.Length);
				AssertEquals("User should be notified of the IO error", "MEH MEH", ((INotificationSubscriberNotification)NotificationBuffer.GetEventsByType(ErrorType.IOError)[0]).AdditionalInfo);
				NotificationBuffer.Clear();
				DataExporter.ExceptionToBeThrownWhenDeliveringFiles = new EmailSendFailedException("EMAIL MEH");
				Assert("Should return false when file cannot be delivered", !DataExporter.Export(NotificationBuffer));
				AssertEquals("User should be notified of the Email sending error", 1, NotificationBuffer.Events.Length);
				AssertEquals("User should be notified of the Email sending error", "EMAIL MEH", ((INotificationSubscriberNotification)NotificationBuffer.GetEventsByType(ErrorType.ErrorSendingEmail)[0]).AdditionalInfo);
			}
			finally
			{
				File.Delete(Path.Combine(Env.TempPath, "0612.csv"));
			}
		}

		public void TestExport_FailToExport()
		{
			AssertEquals("Pre-condition", 0, NotificationBuffer.Events.Length);
			DataExporter.DirectorForTest.ExceptionToBeThrownWhenExporting = new ApplicationException("UNHANDLED");
			Assert("Should return false when there is an exception", !DataExporter.Export(NotificationBuffer));
			AssertEquals("User should be notified when an error occurs", 1, NotificationBuffer.Events.Length);
			AssertEquals("User should be notified when an error occurs", "An error has occurred during export. Detail: UNHANDLED", ((INotificationSubscriberNotification)NotificationBuffer.GetEventsByType(ErrorType.Error)[0]).AdditionalInfo);
			AssertEquals(DataExporter.DirectorForTest.ExceptionToBeThrownWhenExporting, ErrorReporter.LastExceptionReported);
			ErrorReporter.Clear();
		}

		[TestDate(2006, 12, 31)]
		public void TestExport()
		{
			SetupForAccPeriodTest();
			((JASOrgHeader)GlbCompany.CurrentCompany.OrgProxy).NettingCode = "ITMIL";
			DataExporter.DeliveryMethod = JASDataExporterBizO.DirectoryDeliveryMethodCode;
			string exportDir = Path.Combine(Env.TempPath, "_CognosExport_");
			Directory.CreateDirectory(exportDir);
			try
			{
				DataExporter.ExportDirectory = exportDir;
				string expectedTargetFile = Path.Combine(exportDir, "0612ITMIL.csv");
				Assert("Export should be successful", DataExporter.Export(NotificationBuffer));
				Assert("File should be exported to the target directory", File.Exists(expectedTargetFile));
				AssertEquals("Check content", "31-Dec-2006 00:00", File.ReadAllText(expectedTargetFile));
			}
			finally
			{
				TempDirectory.DeleteDirectory(exportDir);
			}
		}

		public void TestExport_WithPeriodThatIsNotCurrentPeriod()
		{
			SetupForAccPeriodTest();
			((JASOrgHeader)GlbCompany.CurrentCompany.OrgProxy).NettingCode = "ITMIL";
			DataExporter.DeliveryMethod = JASDataExporterBizO.DirectoryDeliveryMethodCode;
			string exportDir = Path.Combine(Env.TempPath, "_CognosExport_");
			Directory.CreateDirectory(exportDir);
			DataExporter.EndingPeriod = 200402;
			try
			{
				DataExporter.ExportDirectory = exportDir;
				string expectedTargetFile = Path.Combine(exportDir, "0402ITMIL.csv");
				Assert("Export should be successful", DataExporter.Export(NotificationBuffer));
				Assert("File should be exported to the target directory", File.Exists(expectedTargetFile));
				AssertEquals("Check content", "29-Feb-2004 00:00", File.ReadAllText(expectedTargetFile));
			}
			finally
			{
				TempDirectory.DeleteDirectory(exportDir);
			}
		}

		public void TestGetNewCognosExportDirector()
		{
			CognosExportDirector director = DataExporter.GetNewCognosExportDirector_Original(NotificationBuffer);
			AssertEquals(typeof(CognosExportDirector), director.GetType());
			AssertEquals(NotificationBuffer, director.Notifications);
		}

		public void TestEmailSubject()
		{
			AssertEquals("Cognos Export file from CargoWise One", DataExporter.EmailSubject);
		}

		public void TestExportSummaryAndAppendExportSummary()
		{
			AssertEquals("Pre-condition", "", DataExporter.ExportSummary);
			DataExporter.AppendExportSummary("MEH1");
			AssertEquals("MEH1", DataExporter.ExportSummary);
			DataExporter.AppendExportSummary("MEH2");
			AssertEquals("MEH1\r\nMEH2", DataExporter.ExportSummary);
			DataExporter.AppendExportSummary("MEH3");
			AssertEquals("MEH1\r\nMEH2\r\nMEH3", DataExporter.ExportSummary);
		}

		public void TestRefreshBindingCalledWhenExportSummaryAppended()
		{
			DataExporter.ExportSummaryInfo.ValueChanged += new EventHandler(ExportSummaryInfo_ValueChanged);
			Assert("Pre-condition", !ExportSummaryInfoRefreshBindingCalled);
			DataExporter.AppendExportSummary("MEH");
			Assert("Refresh binding should be called on ExportSummaryInfo", ExportSummaryInfoRefreshBindingCalled);
		}

		public void TestValidation()
		{
			AssertEquals(typeof(CognosDataExporterBizOValidation), DataExporter.Validation.GetType());
		}

		[TestDate(2004, 2, 17)]
		public void TestEndingPeriod()
		{
			SetupForAccPeriodTest();
			AssertEquals("By default should be set to the current period", 200402, DataExporter.EndingPeriod);
			Assert("Pre-condition. Period should be valid", !DataExporter.EndingPeriodInfo.HasErrors());
			DataExporter.EndingPeriodInfo.ClearValue();
			Assert("End Period should be validated", DataExporter.EndingPeriodInfo.HasErrors());
		}

		public void TestPeriodCalculator()
		{
			AssertNotNull("Should be lazy created", DataExporter.PeriodCalculator);
			SetupForAccPeriodTest();
			AssertEquals("Should use current company", 200404, DataExporter.PeriodCalculator.GetLastPeriodForYear(2004));
		}

		void SetupForAccPeriodTest()
		{
			CreatePeriod(200401, new ZDateTime(2004, 1, 1), new ZDateTime(2004, 1, 31));
			CreatePeriod(200402, new ZDateTime(2004, 2, 1), new ZDateTime(2004, 2, 29));
			CreatePeriod(200403, new ZDateTime(2004, 3, 1), new ZDateTime(2004, 3, 31));
			CreatePeriod(200404, new ZDateTime(2004, 4, 1), new ZDateTime(2004, 4, 30));
			CreatePeriod(200612, new ZDateTime(2006, 12, 1), new ZDateTime(2006, 12, 31));
			Factory.Save();
		}

		void CreatePeriod(ZInt period, ZDateTime start, ZDateTime end)
		{
			AccPeriodManagement periodManagement = Factory.New<AccPeriodManagement>();
			periodManagement.AM_Period = period;
			periodManagement.AM_StartDate = start;
			periodManagement.AM_EndDate = end;
			periodManagement.AM_Year = (ZShort)start.Year;
			periodManagement.AM_GC_Company = GlbCompany.CurrentCompany.PK;
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CognosDataExporterBizO();
		}

		void ExportSummaryInfo_ValueChanged(object sender, EventArgs e)
		{
			ExportSummaryInfoRefreshBindingCalled = true;
		}

		CognosDataExporterBizOForTest DataExporter
		{
			get
			{
				if (fDataExporter == null)
				{
					fDataExporter = new CognosDataExporterBizOForTest(NotificationBuffer);
				}

				return fDataExporter;
			}
		}

		CognosNotificationBufferForTest NotificationBuffer
		{
			get
			{
				if (fNotificationBuffer == null)
				{
					fNotificationBuffer = new CognosNotificationBufferForTest();
				}

				return fNotificationBuffer;
			}
		}

		CognosDataExporterBizOForTest fDataExporter;
		CognosNotificationBufferForTest fNotificationBuffer;
		bool ExportSummaryInfoRefreshBindingCalled;
		#region class CognosDataExporterBizOForTest
		class CognosDataExporterBizOForTest : CognosDataExporterBizO
		{
			public CognosDataExporterBizOForTest(CognosNotificationBufferForTest notifications)
			{
				this.CognosNotifications = notifications;
			}

			protected override CognosExportDirector GetNewCognosExportDirector(ICognosNotificationSubscriber notificationSubscriber)
			{
				return DirectorForTest;
			}

			public CognosExportDirector GetNewCognosExportDirector_Original(ICognosNotificationSubscriber notificationSubscriber)
			{
				return base.GetNewCognosExportDirector(notificationSubscriber);
			}

			public new ZString EmailSubject
			{
				get
				{
					return base.EmailSubject;
				}
			}

			protected override void DeliverFiles(params string[] fullPathToSourceFiles)
			{
				if (ExceptionToBeThrownWhenDeliveringFiles != null)
				{
					throw ExceptionToBeThrownWhenDeliveringFiles;
				}

				base.DeliverFiles(fullPathToSourceFiles);
			}

			public CognosExportDirectorForTest DirectorForTest
			{
				get
				{
					if (fDirectorForTest == null)
					{
						fDirectorForTest = new CognosExportDirectorForTest(CognosNotifications);
					}

					return fDirectorForTest;
				}
			}

			public Exception ExceptionToBeThrownWhenDeliveringFiles;
			CognosExportDirectorForTest fDirectorForTest;
			readonly CognosNotificationBufferForTest CognosNotifications;
		}

		#endregion
		#region class CognosExportDirectorForTest
		class CognosExportDirectorForTest : CognosExportDirector
		{
			public CognosExportDirectorForTest(ICognosNotificationSubscriber notifications) : base(notifications)
			{
				AdvanceTheCurrentDateTimeForTestIfRequired();
			}

			public override bool Export(ZDateTime exportStartDateTime, ZString exportFilePath)
			{
				if (ExceptionToBeThrownWhenExporting != null)
				{
					throw ExceptionToBeThrownWhenExporting;
				}

				using (StreamWriter writer = new StreamWriter(exportFilePath))
				{
					writer.Write(exportStartDateTime.ToString("dd-MMM-yyyy HH:mm"));
				}

				return true;
			}

			void AdvanceTheCurrentDateTimeForTestIfRequired()
			{
				if (TestDateAttribute.IsActive)
				{
					TestDateAttribute.Date = TestDateAttribute.Date.AddHours(2);
				}
			}

			public Exception ExceptionToBeThrownWhenExporting;
		}

		#endregion
		ZGuid initialProxyOrgPK;
		protected override void SetUp()
		{
			initialProxyOrgPK = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			base.SetUp();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "1234567890123456789012345678901234567890";
			org.MainAddress.OA_Address1 = "2345678901234567890123456789012345678901";
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = org.PK;
			Factory.Save();
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = initialProxyOrgPK;
		}
		#endregion
	}
}
