using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Cognos.Testing
{
	class CognosExportDirectorTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals("Should be assigned in the constructor", Notifications, ExportDirector.Notifications);
		}

		[ExpectExceptionMessage(typeof(ArgumentException), "ExportStartDateTime and ExportFilePath cannot be empty")]
		public void TestExport_InvalidParams()
		{
			ExportDirector.Export(ZDateTime.Empty, ZString.Empty);
		}

		public void TestExport_PreExportCheckFails()
		{
			ExportDirector.PreExportCheckShouldFail = true;
			Assert("Should return false if Pre-Export check fails", !ExportDirector.Export(ZDateTime.Now, "ArbitraryString"));
			AssertEquals("Should not export if pre-export check fails", 0, Notifications.CurrentProgress);
			AssertEquals("Should not export if pre-export check fails", 0, Notifications.Events.Length);
		}

		[TestDate(2006, 4, 4, 4, 4, 4)]
		public void TestExport_ExportToFileFails()
		{
			ExportDirector.ExportShouldFail = true;
			Assert("Should return false if Export fails", !ExportDirector.Export(ZDateTime.Now, "ArbitraryString"));
			AssertEquals("Should not complete progress if export fails", 0, Notifications.CurrentProgress);
			AssertEquals(2, Notifications.Events.Length);
			AssertNotification(Notifications.Events[0], typeof(InfoNotification), "Start Exporting - 04/04/2006 04:04:04");
			AssertNotification(Notifications.Events[1], typeof(ErrorNotification), "MEH");
		}

		[TestDate(2006, 4, 4, 4, 4, 4)]
		public void TestExport_ExportSuccess()
		{
			Assert("Should return true if Export successful", ExportDirector.Export(ZDateTime.Now, "ArbitraryString"));
			AssertEquals("Should complete progress if export successful", 100, Notifications.CurrentProgress);
			AssertEquals(2, Notifications.Events.Length);
			AssertNotification(Notifications.Events[0], typeof(InfoNotification), "Start Exporting - 04/04/2006 04:04:04");
			AssertNotification(Notifications.Events[1], typeof(InfoNotification), "Finish Exporting - 04/04/2006 04:04:04");
		}

		public void TestGetNewCognosFileExporter()
		{
			ZDateTime expectedExportStartDateTime = new ZDateTime(2006, 5, 5);
			CognosNotificationBufferForTest notifications = new CognosNotificationBufferForTest();
			CognosFileExporter exporter = ExportDirector.GetNewCognosFileExporter_Original(expectedExportStartDateTime, notifications);
			AssertEquals(typeof(CognosFileExporter), exporter.GetType());
			AssertEquals(expectedExportStartDateTime, exporter.ExportStartDateTime);
			AssertEquals(notifications, exporter.Notifications);
		}

		public void TestGetNewCognosPreExportCheck()
		{
			AssertEquals(typeof(CognosPreExportCheck), ExportDirector.GetNewCognosPreExportCheck_Original().GetType());
		}

		#region Implementation
		CognosExportDirectorForTest ExportDirector
		{
			get
			{
				if (fExportDirector == null)
				{
					fExportDirector = new CognosExportDirectorForTest(Notifications);
				}

				return fExportDirector;
			}
		}

		CognosNotificationBufferForTest Notifications
		{
			get
			{
				if (fNotifications == null)
				{
					fNotifications = new CognosNotificationBufferForTest();
				}

				return fNotifications;
			}
		}

		void AssertNotification(INotification notification, Type expectedNotificationType, ZString expectedMessage)
		{
			AssertEquals("Expected Notification Type", expectedNotificationType, notification.GetType());
			AssertEquals("AdditionalInfo not as expected", expectedMessage, ((INotificationSubscriberNotification)notification).AdditionalInfo);
		}

		CognosExportDirectorForTest fExportDirector;
		CognosNotificationBufferForTest fNotifications;
		#endregion
		#region class CognosExportDirectorForTest
		class CognosExportDirectorForTest : CognosExportDirector
		{
			public CognosExportDirectorForTest(ICognosNotificationSubscriber notifications) : base(notifications)
			{
			}

			protected override CognosFileExporter GetNewCognosFileExporter(ZDateTime exportStartDateTime, ICognosNotificationSubscriber notifications)
			{
				return new CognosFileExporterForTest(ExportShouldFail, exportStartDateTime, notifications);
			}

			public CognosFileExporter GetNewCognosFileExporter_Original(ZDateTime exportStartDateTime, ICognosNotificationSubscriber notifications)
			{
				return base.GetNewCognosFileExporter(exportStartDateTime, notifications);
			}

			protected override CognosPreExportCheck GetNewCognosPreExportCheck()
			{
				return new CognosPreExportCheckForTest(PreExportCheckShouldFail, Notifications);
			}

			public CognosPreExportCheck GetNewCognosPreExportCheck_Original()
			{
				return base.GetNewCognosPreExportCheck();
			}

			public bool PreExportCheckShouldFail;
			public bool ExportShouldFail;
		}

		#endregion
		#region class CognosFileExporterForTest
		class CognosFileExporterForTest : CognosFileExporter
		{
			public CognosFileExporterForTest(bool shouldThrowIOExceptionDuringExport, ZDateTime exportStartDateTime, ICognosNotificationSubscriber notifications) : base(exportStartDateTime, notifications)
			{
				this.ShouldThrowIOExceptionDuringExport = shouldThrowIOExceptionDuringExport;
			}

			protected override void ExportToFileCore(ZString exportFilePath)
			{
				if (ShouldThrowIOExceptionDuringExport)
				{
					throw new IOException("MEH");
				}
			}

			readonly bool ShouldThrowIOExceptionDuringExport;
		}

		#endregion
		#region class CognosPreExportCheckForTest
		class CognosPreExportCheckForTest : CognosPreExportCheck
		{
			public CognosPreExportCheckForTest(bool ensureCanExportShouldFail, ICognosNotificationSubscriber notifications) : base(notifications)
			{
				this.EnsureCanExportShouldFail = ensureCanExportShouldFail;
			}

			public override bool EnsureCanExport()
			{
				return !EnsureCanExportShouldFail;
			}

			readonly bool EnsureCanExportShouldFail;
		}
		#endregion
	}
}
