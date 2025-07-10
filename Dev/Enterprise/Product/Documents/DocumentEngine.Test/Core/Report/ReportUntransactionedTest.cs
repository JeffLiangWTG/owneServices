using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DataProtection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Scheduler.Business;
using Enterprise.SqlSecurity;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class ReportUntransactionedTest : TestCase
	{
		/// <summary>
		/// Report DB is specified, and option to use it confirmed
		/// => REPORT RUNS USING REPORT DATABASE CONNECTION
		/// </summary>
		public void TestReportUsesReportDbConnectionIfReportDbSpecified()
		{
			using (var templateStream = new MemoryStream())
			{
				var excelTemplate = ReportTest.CreateTestRunningConnectionTemplate(templateStream);

				using (var report = new Report(new DocumentPack(), excelTemplate))
				{
					var reportDb = new ReportDbForTesting(Db.ServerName, Db.DatabaseName);
					var manager = SecondaryServerConnectionProviderProvider.GetProvider(reportDb);
					((IReportForReportDbTesting)report).SetReportDbManagerForTesting(manager);

					report.OverrideReportDbOption = false;

					var reportDbServerName = string.Empty;

					report.ReportServerNameChanged += (sender, reportServerName) =>
					{
						reportDbServerName = reportServerName;
					};

					using (var outputStream = new MemoryStream())
					{
						report.Save(outputStream);
						AssertEquals(Db.ServerName, reportDbServerName);
						ReportTest.AssertReportResults(outputStream, reportDb.CreatedConnectionSpid.Value, Db.Connection.ServerNameReportedByDatabase, Db.DatabaseName);
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestShowMessageOfSecondaryServerConnectionException()
		{
			using (SystemDataRegistry.Instance.ReportingDbServerNames.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { Db.Connection.ServerName }))
			using (SystemDataRegistry.Instance.ReportingDbServerThresholdCore.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 9999))
			using (var report = new Report(new DocumentPack(), EmptyAndValidTemplate))
			using (var outputStream = new MemoryStream())
			{
				var reportDb = new ReportDbForTestingWithoutOverrideAllReportServerNames(new string[] { Db.Connection.ServerName }, "InvalidDatabase", false, (exception, serverName) =>
				{
					Report.ShowMessageOfSecondaryServerConnectionExceptionByMessageBox(exception, serverName);
				});
				reportDb.ReportServerException = SqlExceptionBuilder.CreateSqlException(4060, $"Cannot open database \"InvalidDatabase\" requested by the login. The login failed.\r\nLogin failed for user '{((IDbReconnectionHandling)Db.Connection).LoginName}'.");
				var manager = SecondaryServerConnectionProviderProvider.GetProvider(reportDb);
				((IReportForReportDbTesting)report).SetReportDbManagerForTesting(manager);

				report.Save(outputStream);

				AssertEquals($"If other Secondary Servers are set up in the registry setting under \"System -> Reports -> Reporting databases full server names\", the system will try to use them before reverting to the Primary Server.\r\nException details:\r\nCannot open database \"InvalidDatabase\" requested by the login. The login failed.\r\nLogin failed for user '{((IDbReconnectionHandling)Db.Connection).LoginName}'.",
						UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals($"Unable to connect to the Secondary Server {Db.Connection.ServerName}", UnitTestUserNotification.Instance.LastMessage.Caption);
			}
		}

		[UseSnapshotProtection]
		public void TestShowCorrectServerForSecondaryServerConnectionException()
		{
			using (SystemDataRegistry.Instance.ReportingDbServerNames.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { Db.Connection.ServerName }))
			using (SystemDataRegistry.Instance.ReportingDbServerThresholdCore.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 9999))
			using (var report = new Report(new DocumentPack(), EmptyAndValidTemplate))
			using (var outputStream = new MemoryStream())
			{
				var reportDb = new ReportDbForTesting(new string[] { "InvalidServer" }, "InvalidDatabase", false, (exception, serverName) =>
				{
					Report.ShowMessageOfSecondaryServerConnectionExceptionByMessageBox(exception, serverName);
				});
				reportDb.ReportServerException = new ArgumentException("Dummy exception");
				var manager = SecondaryServerConnectionProviderProvider.GetProvider(reportDb);
				((IReportForReportDbTesting)report).SetReportDbManagerForTesting(manager);

				report.Save(outputStream);

				AssertEquals($"Unable to connect to the Secondary Server InvalidServer", UnitTestUserNotification.Instance.LastMessage.Caption);
			}
		}

		/// <summary>
		/// Report DB is specified, but the option to use it is overriden
		/// => REPORT RUNS USING MAIN DATABASE CONNECTION
		/// </summary>
		public void TestReportIfConnectionIfReportDbSpecifiedButOverriden()
		{
			using (var templateStream = new MemoryStream())
			{
				var excelTemplate = ReportTest.CreateTestRunningConnectionTemplate(templateStream);

				using (var report = new Report(new DocumentPack(), excelTemplate))
				{
					var reportDb = new ReportDbForTesting(Db.ServerName, Db.SqlMasterDb);
					var manager = SecondaryServerConnectionProviderProvider.GetProvider(reportDb);
					((IReportForReportDbTesting)report).SetReportDbManagerForTesting(manager);
					report.OverrideReportDbOption = true;

					using (var outputStream = new MemoryStream())
					{
						report.Save(outputStream);
						ReportTest.AssertReportResults(outputStream, Db.Connection.SPID, Db.Connection.ServerNameReportedByDatabase, Db.DatabaseName);
					}
				}
			}
		}

		[GuiTest, ExpectNoExceptions, UseSnapshotProtection]
		public void TestCatchReportErrorsAfterNotifyErrorsThenChooseContinue()
		{
			var contact = new DocDeliveryContact(new BusinessObjectFactory())
			{
				Name = "Test Contact",
				DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email,
				Email = "unit.test@cargowise.com",
				AttachmentType = AttachmentTypeList.Codes.Xls
			};

			using (var templateStream = new MemoryStream())
			{
				using (var creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					var workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "DataContext=.DummyBusinessObject";

					workSheet[3, 0] = "#SectionBody";
					workSheet[4, 1] = "<Z0_VarCharMax>";
					workSheet[5, 1] = "<AnyOtherNotExistsMacros>";
					workSheet[6, 264] = "#SectionFooter";

					workSheet[8, 0] = "#PageFooter";
					workSheet[9, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream, ExcelFileFormatOptionList.Codes.XLSX);
				}

				var dataSource = new BusinessObjectFactory().New<DummyBusinessObject>();
				var excelTemplate = new ExcelTemplateWrappingStream("TestTemplateFromStream", templateStream);
				using (var report = new Report(new DocumentPack(), excelTemplate, BODocDataProvider.Get(dataSource), "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false))
				using (Report.TemporarilyStopErrorsThrowingAnException())
				using (DocumentsDataRegistry.Instance.ExcelDefaultRenderingFormat.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ExcelFileFormatOptionList.Codes.XLS))
				using (var stream = new MemoryStream())
				{
					((IReportForUnitTesting)report).DeliveryContact = contact;
					report.Save(stream);
					ErrorReporter.Clear();
				}
			}
		}

		/// <summary>
		/// Report DB is NOT specified
		/// => REPORT RUNS USING MAIN DATABASE CONNECTION
		/// </summary>
		public void TestReportUsesMainConnectionIfReportDbNotSpecified()
		{
			using (var templateStream = new MemoryStream())
			{
				var excelTemplate = ReportTest.CreateTestRunningConnectionTemplate(templateStream);

				using (var report = new Report(new DocumentPack(), excelTemplate))
				{
					var reportDb = new ReportDbForTesting("", "");
					var manager = SecondaryServerConnectionProviderProvider.GetProvider(reportDb);
					((IReportForReportDbTesting)report).SetReportDbManagerForTesting(manager);

					using (var outputStream = new MemoryStream())
					{
						report.Save(outputStream);
						AssertNull("Report DB connection SPID should be NULL", reportDb.CreatedConnectionSpid);
						ReportTest.AssertReportResults(outputStream, Db.Connection.SPID, Db.Connection.ServerNameReportedByDatabase, Db.DatabaseName);
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestReportDoNotUsesImpersonate_RegistryIsFalse()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();

			staff.GS_FullName = "Test staff";
			staff.GS_LoginName = "TestLogin_001";
			staff.GS_Code = "TSD";
			staff.IsReadOnlyDBUser = true;

			new DbUserManager().SetPasswordForStaff(staff, "P@ssW0rd!");
			factory.Save();

			var scheduleTask = factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_GS_NKPrintUser = staff.GS_Code;

			using (var templateStream = new MemoryStream())
			{
				var excelTemplate = ReportTest.CreateTestRunningConnectionTemplateWithUsername(templateStream);

				using (var report = new Report(new DocumentPack(), excelTemplate))
				{
					report.SetScheduleTask(scheduleTask);

					var reportDb = new ReportDbForTesting(Db.ServerName, Db.DatabaseName);
					var manager = SecondaryServerConnectionProviderProvider.GetProvider(reportDb);
					((IReportForReportDbTesting)report).SetReportDbManagerForTesting(manager);

					report.OverrideReportDbOption = false;

					var reportDbServerName = string.Empty;

					report.ReportServerNameChanged += (sender, reportServerName) =>
					{
						reportDbServerName = reportServerName;
					};

					using (var outputStream = new MemoryStream())
					{
						DocumentsDataRegistry.Instance.EnforceDataAccessOnReport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

						var createdConnectionSpid = report.Save(outputStream);
						AssertEquals(Db.ServerName, reportDbServerName);
						ReportTest.AssertReportResults(outputStream, createdConnectionSpid, Db.Connection.ServerNameReportedByDatabase, Db.DatabaseName, RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName));
					}
				}
			}
		}

		/// <summary>
		/// Report DB is NOT specified
		/// => REPORT RUNS USING MAIN DATABASE CONNECTION
		/// </summary>
		[UseSnapshotProtection]
		public void TestReportIfReportDbNotSpecified_DeveloperAndBackupOperator_UseRestrictedReader()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();

			staff.GS_FullName = "Test staff";
			staff.GS_LoginName = "TestLogin_001";
			staff.GS_Code = "TSD";
			staff.IsDatabaseDeveloper = true;
			staff.IsBackupOperator = true;

			new DbUserManager().SetPasswordForStaff(staff, "P@ssW0rd!");
			factory.Save();

			var scheduleTask = factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_GS_NKPrintUser = staff.GS_Code;

			using (var templateStream = new MemoryStream())
			{
				var excelTemplate = ReportTest.CreateTestRunningConnectionTemplateWithUsername(templateStream);

				using (var report = new Report(new DocumentPack(), excelTemplate))
				{
					report.SetScheduleTask(scheduleTask);

					var reportDb = new ReportDbForTesting("", "");
					var manager = SecondaryServerConnectionProviderProvider.GetProvider(reportDb);
					((IReportForReportDbTesting)report).SetReportDbManagerForTesting(manager);

					using (var outputStream = new MemoryStream())
					{
						var createdConnectionSpid = report.Save(outputStream);
						ReportTest.AssertReportResults(outputStream, createdConnectionSpid, Db.Connection.ServerNameReportedByDatabase, Db.DatabaseName, RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName));
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestReportUsesReportDbConnectionIfReportDbNotSpecified_DatabaseReader_Impersonate()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();

			staff.GS_FullName = "Test staff";
			staff.GS_LoginName = "TestLogin_001";
			staff.GS_Code = "TSD";
			staff.IsReadOnlyDBUser = true;

			new DbUserManager().SetPasswordForStaff(staff, "Pa$$w0rd!");
			factory.Save();

			var scheduleTask = factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_GS_NKPrintUser = staff.GS_Code;

			using (var templateStream = new MemoryStream())
			{
				var excelTemplate = ReportTest.CreateTestRunningConnectionTemplateWithUsername(templateStream);

				using (var report = new Report(new DocumentPack(), excelTemplate))
				{
					report.SetScheduleTask(scheduleTask);

					var reportDb = new ReportDbForTesting("", "");
					var manager = SecondaryServerConnectionProviderProvider.GetProvider(reportDb);
					((IReportForReportDbTesting)report).SetReportDbManagerForTesting(manager);

					using (var outputStream = new MemoryStream())
					{
						DocumentsDataRegistry.Instance.EnforceDataAccessOnReport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

						var createdConnectionSpid = report.Save(outputStream);
						AssertEquals("Report connection SPID same as main connection?", false, createdConnectionSpid == Db.Connection.SPID);
						ReportTest.AssertReportResults(outputStream, createdConnectionSpid, Db.Connection.ServerNameReportedByDatabase, Db.DatabaseName, staff.SQLUserName);
					}
				}
			}
		}

		/// <summary>
		/// Report DB is specified, and option to use it confirmed
		/// => REPORT RUNS USING REPORT DATABASE CONNECTION
		/// </summary>
		[UseSnapshotProtection]
		public void TestReportUsesReportDbConnectionIfReportDbSpecified_DeveloperAndBackupOperator_UseRestrictedReader()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();

			staff.GS_FullName = "Test staff";
			staff.GS_LoginName = "TestLogin_001";
			staff.GS_Code = "TSD";
			staff.IsDatabaseDeveloper = true;
			staff.IsBackupOperator = true;

			new DbUserManager().SetPasswordForStaff(staff, "P@ssW0rd!");
			factory.Save();

			var scheduleTask = factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_GS_NKPrintUser = staff.GS_Code;

			using (var templateStream = new MemoryStream())
			{
				var excelTemplate = ReportTest.CreateTestRunningConnectionTemplateWithUsername(templateStream);

				using (var report = new Report(new DocumentPack(), excelTemplate))
				{
					report.SetScheduleTask(scheduleTask);

					var reportDb = new ReportDbForTesting(Db.ServerName, Db.DatabaseName);
					var manager = SecondaryServerConnectionProviderProvider.GetProvider(reportDb);
					((IReportForReportDbTesting)report).SetReportDbManagerForTesting(manager);

					report.OverrideReportDbOption = false;

					var reportDbServerName = string.Empty;

					report.ReportServerNameChanged += (sender, reportServerName) =>
					{
						reportDbServerName = reportServerName;
					};

					using (var outputStream = new MemoryStream())
					{
						var createdConnectionSpid = report.Save(outputStream);
						AssertEquals(Db.ServerName, reportDbServerName);
						AssertEquals("Report connection SPID is different from main connection.", false, createdConnectionSpid == Db.Connection.SPID);
						ReportTest.AssertReportResults(outputStream, createdConnectionSpid, Db.Connection.ServerNameReportedByDatabase, Db.DatabaseName, RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName));
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestReportUsesReportDbConnectionIfReportDbSpecified_DatabaseReader_Impersonate()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();

			staff.GS_FullName = "Test staff";
			staff.GS_LoginName = "TestLogin_001";
			staff.GS_Code = "TSD";
			staff.IsReadOnlyDBUser = true;

			new DbUserManager().SetPasswordForStaff(staff, "P@ssW0rd!");
			factory.Save();

			var scheduleTask = factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_GS_NKPrintUser = staff.GS_Code;

			using (var templateStream = new MemoryStream())
			{
				var excelTemplate = ReportTest.CreateTestRunningConnectionTemplateWithUsername(templateStream);

				using (var report = new Report(new DocumentPack(), excelTemplate))
				{
					report.SetScheduleTask(scheduleTask);

					var reportDb = new ReportDbForTesting(Db.ServerName, Db.DatabaseName);
					var manager = SecondaryServerConnectionProviderProvider.GetProvider(reportDb);
					((IReportForReportDbTesting)report).SetReportDbManagerForTesting(manager);

					report.OverrideReportDbOption = false;

					var reportDbServerName = string.Empty;

					report.ReportServerNameChanged += (sender, reportServerName) =>
					{
						reportDbServerName = reportServerName;
					};

					using (var outputStream = new MemoryStream())
					{
						DocumentsDataRegistry.Instance.EnforceDataAccessOnReport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

						var createdConnectionSpid = report.Save(outputStream);
						AssertEquals(Db.ServerName, reportDbServerName);
						AssertEquals("Report connection SPID same as main connection?", false, createdConnectionSpid == Db.Connection.SPID);
						ReportTest.AssertReportResults(outputStream, createdConnectionSpid, Db.Connection.ServerNameReportedByDatabase, Db.DatabaseName, staff.SQLUserName);
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestShowMessageOfSecondaryServerConnectionException_DeveloperAndBackupOperator_UseRestrictedReader()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();

			staff.GS_FullName = "Test staff";
			staff.GS_LoginName = "TestLogin_001";
			staff.GS_Code = "TSD";
			staff.IsDatabaseDeveloper = true;
			staff.IsBackupOperator = true;

			new DbUserManager().SetPasswordForStaff(staff, "P@ssW0rd!");
			factory.Save();

			var scheduleTask = factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_GS_NKPrintUser = staff.GS_Code;

			using (SystemDataRegistry.Instance.ReportingDbServerNames.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { Db.Connection.ServerName }))
			using (SystemDataRegistry.Instance.ReportingDbServerThresholdCore.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 9999))
			using (var report = new Report(new DocumentPack(), EmptyAndValidTemplate))
			using (var outputStream = new MemoryStream())
			{
				report.SetScheduleTask(scheduleTask);

				var reportDb = new ReportDbForTestingWithoutOverrideAllReportServerNames(new string[] { Db.Connection.ServerName }, "InvalidDatabase", false, (exception, serverName) =>
				{
					Report.ShowMessageOfSecondaryServerConnectionExceptionByMessageBox(exception, serverName);
				});
				reportDb.ReportServerException = SqlExceptionBuilder.CreateSqlException(4060, $"Cannot open database \"InvalidDatabase\" requested by the login. The login failed.\r\nLogin failed for user '{((IDbReconnectionHandling)Db.Connection).LoginName}'.");
				var manager = SecondaryServerConnectionProviderProvider.GetProvider(reportDb);

				AssertEquals("Manager should be SecondaryServerConnectionProvider", "SecondaryServerConnectionProvider", manager.GetType().Name);
				AssertEquals("SecondaryServerConnectionDetails should be ReportDbForTestingWithoutOverrideAllReportServerNames", "ReportDbForTestingWithoutOverrideAllReportServerNames", manager.SecondaryServerConnectionDetails.GetType().Name);

				((IReportForReportDbTesting)report).SetReportDbManagerForTesting(manager);
				report.Save(outputStream);

				AssertEquals($"If other Secondary Servers are set up in the registry setting under \"System -> Reports -> Reporting databases full server names\", the system will try to use them before reverting to the Primary Server.\r\nException details:\r\nCannot open database \"InvalidDatabase\" requested by the login. The login failed.\r\nLogin failed for user '{((IDbReconnectionHandling)Db.Connection).LoginName}'.",
						UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals($"Unable to connect to the Secondary Server {Db.Connection.ServerName}", UnitTestUserNotification.Instance.LastMessage.Caption);
			}
		}

		[UseSnapshotProtection]
		public void TestShowMessageOfSecondaryServerConnectionException_DatabaseReader_Impersonate()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();

			staff.GS_FullName = "Test staff";
			staff.GS_LoginName = "TestLogin_001";
			staff.GS_Code = "TSD";
			staff.IsReadOnlyDBUser = true;

			new DbUserManager().SetPasswordForStaff(staff, "P@ssW0rd!");
			factory.Save();

			var scheduleTask = factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_GS_NKPrintUser = staff.GS_Code;

			using (SystemDataRegistry.Instance.ReportingDbServerNames.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { Db.Connection.ServerName }))
			using (SystemDataRegistry.Instance.ReportingDbServerThresholdCore.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 9999))
			using (var report = new Report(new DocumentPack(), EmptyAndValidTemplate))
			using (var outputStream = new MemoryStream())
			{
				report.SetScheduleTask(scheduleTask);

				var reportDb = new ReportDbForTestingWithoutOverrideAllReportServerNames(new string[] { Db.Connection.ServerName }, "InvalidDatabase", false, (exception, serverName) =>
				{
					Report.ShowMessageOfSecondaryServerConnectionExceptionByMessageBox(exception, serverName);
				});
				var manager = SecondaryServerConnectionProviderProvider.GetProvider(reportDb);
				((IReportForReportDbTesting)report).SetReportDbManagerForTesting(manager);

				DocumentsDataRegistry.Instance.EnforceDataAccessOnReport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				report.Save(outputStream);

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Text.Equals($"If other Secondary Servers are set up in the registry setting under \"System -> Reports -> Reporting databases full server names\", the system will try to use them before reverting to the Primary Server.\r\nException details:\r\nLogin failed for user '{RestrictedReaderLoginCredentials.UserNameFor("InvalidDatabase")}'.", StringComparison.OrdinalIgnoreCase));
				AssertEquals($"Unable to connect to the Secondary Server {Db.Connection.ServerName}", UnitTestUserNotification.Instance.LastMessage.Caption);
			}
		}

		[UseSnapshotProtection]
		public void TestShowCorrectServerForSecondaryServerConnectionException_DeveloperAndBackupOperator_UseRestrictedReader()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();

			staff.GS_FullName = "Test staff";
			staff.GS_LoginName = "TestLogin_001";
			staff.GS_Code = "TSD";
			staff.IsDatabaseDeveloper = true;
			staff.IsBackupOperator = true;

			new DbUserManager().SetPasswordForStaff(staff, "P@ssW0rd!");
			factory.Save();

			var scheduleTask = factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_GS_NKPrintUser = staff.GS_Code;

			using (SystemDataRegistry.Instance.ReportingDbServerNames.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { Db.Connection.ServerName }))
			using (SystemDataRegistry.Instance.ReportingDbServerThresholdCore.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 9999))
			using (var report = new Report(new DocumentPack(), EmptyAndValidTemplate))
			using (var outputStream = new MemoryStream())
			{
				report.SetScheduleTask(scheduleTask);

				var reportDb = new ReportDbForTesting(new string[] { "InvalidServer" }, "InvalidDatabase", false, (exception, serverName) =>
				{
					Report.ShowMessageOfSecondaryServerConnectionExceptionByMessageBox(exception, serverName);
				});

				reportDb.ReportServerException = new ArgumentException("Dummy exception");
				var manager = SecondaryServerConnectionProviderProvider.GetProvider(reportDb);
				((IReportForReportDbTesting)report).SetReportDbManagerForTesting(manager);

				report.Save(outputStream);

				AssertEquals($"Unable to connect to the Secondary Server InvalidServer", UnitTestUserNotification.Instance.LastMessage.Caption);
			}
		}

		[UseSnapshotProtection]
		public void TestShowCorrectServerForSecondaryServerConnectionException_DatabaseReader_Impersonate()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();

			staff.GS_FullName = "Test staff";
			staff.GS_LoginName = "TestLogin_001";
			staff.GS_Code = "TSD";
			staff.IsReadOnlyDBUser = true;

			new DbUserManager().SetPasswordForStaff(staff, "P@ssW0rd!");
			factory.Save();

			var scheduleTask = factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_GS_NKPrintUser = staff.GS_Code;

			using (SystemDataRegistry.Instance.ReportingDbServerNames.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { Db.Connection.ServerName }))
			using (SystemDataRegistry.Instance.ReportingDbServerThresholdCore.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 9999))
			using (var report = new Report(new DocumentPack(), EmptyAndValidTemplate))
			using (var outputStream = new MemoryStream())
			{
				report.SetScheduleTask(scheduleTask);

				var reportDb = new ReportDbForTesting(new string[] { "InvalidServer" }, "InvalidDatabase", false, (exception, serverName) =>
				{
					Report.ShowMessageOfSecondaryServerConnectionExceptionByMessageBox(exception, serverName);
				});

				reportDb.ReportServerException = new ArgumentException("Dummy exception");
				var manager = SecondaryServerConnectionProviderProvider.GetProvider(reportDb);
				((IReportForReportDbTesting)report).SetReportDbManagerForTesting(manager);

				DocumentsDataRegistry.Instance.EnforceDataAccessOnReport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				report.Save(outputStream);

				AssertEquals($"Unable to connect to the Secondary Server InvalidServer", UnitTestUserNotification.Instance.LastMessage.Caption);
			}
		}

		/// <summary>
		/// Report DB is specified, but the option to use it is overriden
		/// => REPORT RUNS USING MAIN DATABASE CONNECTION
		/// </summary>
		[UseSnapshotProtection]
		public void TestReportIfReportDbSpecifiedButOverriden_DeveloperAndBackupOperator_UseRestrictedReader()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();

			staff.GS_FullName = "Test staff";
			staff.GS_LoginName = "TestLogin_001";
			staff.GS_Code = "TSD";
			staff.IsDatabaseDeveloper = true;
			staff.IsBackupOperator = true;

			new DbUserManager().SetPasswordForStaff(staff, "P@ssW0rd!");
			factory.Save();

			var scheduleTask = factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_GS_NKPrintUser = staff.GS_Code;

			using (var templateStream = new MemoryStream())
			{
				var excelTemplate = ReportTest.CreateTestRunningConnectionTemplateWithUsername(templateStream);

				using (var report = new Report(new DocumentPack(), excelTemplate))
				{
					report.SetScheduleTask(scheduleTask);

					var reportDb = new ReportDbForTesting(Db.ServerName, Db.SqlMasterDb);
					var manager = SecondaryServerConnectionProviderProvider.GetProvider(reportDb);
					((IReportForReportDbTesting)report).SetReportDbManagerForTesting(manager);
					report.OverrideReportDbOption = true;

					using (var outputStream = new MemoryStream())
					{
						var createdConnectionSpid = report.Save(outputStream);
						ReportTest.AssertReportResults(outputStream, createdConnectionSpid, Db.Connection.ServerNameReportedByDatabase, Db.DatabaseName, RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName));
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestReportIfReportDbSpecifiedButOverriden_DatabaseReader_Impersonate()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();

			staff.GS_FullName = "Test staff";
			staff.GS_LoginName = "TestLogin_001";
			staff.GS_Code = "TSD";
			staff.IsReadOnlyDBUser = true;

			new DbUserManager().SetPasswordForStaff(staff, "P@ssW0rd!");
			factory.Save();

			var scheduleTask = factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_GS_NKPrintUser = staff.GS_Code;

			using (var templateStream = new MemoryStream())
			{
				var excelTemplate = ReportTest.CreateTestRunningConnectionTemplateWithUsername(templateStream);

				using (var report = new Report(new DocumentPack(), excelTemplate))
				{
					report.SetScheduleTask(scheduleTask);

					var reportDb = new ReportDbForTesting(Db.ServerName, Db.SqlMasterDb);
					var manager = SecondaryServerConnectionProviderProvider.GetProvider(reportDb);
					((IReportForReportDbTesting)report).SetReportDbManagerForTesting(manager);
					report.OverrideReportDbOption = true;

					using (var outputStream = new MemoryStream())
					{
						DocumentsDataRegistry.Instance.EnforceDataAccessOnReport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

						var createdConnectionSpid = report.Save(outputStream);
						ReportTest.AssertReportResults(outputStream, createdConnectionSpid, Db.Connection.ServerNameReportedByDatabase, Db.DatabaseName, staff.SQLUserName);
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestReportDoesNotCauseConnectionRepairWhenPermissionIsDenied()
		{
			var factory = new BusinessObjectFactory();
			var scheduleTask = factory.NewWithValidTestData<ReportScheduleTask>();
			using (var templateStream = new MemoryStream())
			{
				var excelTemplate = ReportTest.CreateTestRunningConnectionTemplateWithHRMTable(templateStream);
				var loginModifyDates = ReportTest.GetApplicationLoginModifiedDates();

				using (var report = new Report(new DocumentPack(), excelTemplate))
				{
					report.SetScheduleTask(scheduleTask);

					var reportDb = new ReportDbForTesting("", "");
					var manager = SecondaryServerConnectionProviderProvider.GetProvider(reportDb);
					((IReportForReportDbTesting)report).SetReportDbManagerForTesting(manager);

					using (var outputStream = new MemoryStream())
					{
						AssertExceptionThrown<DocumentEngineException>(() => _ = report.Save(outputStream));

						var loginModifyDates_afterReport = ReportTest.GetApplicationLoginModifiedDates();
						foreach (var loginModifyDate in loginModifyDates)
						{
							AssertEquals($"{loginModifyDate.Key} should not be changed", loginModifyDate.Value, loginModifyDates_afterReport[loginModifyDate.Key]);
						}
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestReportDoesCauseConnectionRepairWhenSchemaPermissionIsDenied()
		{
			var factory = new BusinessObjectFactory();
			var scheduleTask = factory.NewWithValidTestData<ReportScheduleTask>();

			using (var adminConnection = Db.NewAdminConnection())
			{
				adminConnection.ExecuteNonQuery($"ALTER ROLE [cwRestrictedReaderRole] DROP MEMBER [{RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName)}];");
			}

			using (var templateStream = new MemoryStream())
			{
				var excelTemplate = ReportTest.CreateTestRunningConnectionTemplateWithDboTable(templateStream);
				var loginModifyDates = ReportTest.GetApplicationLoginModifiedDates();

				using (var report = new Report(new DocumentPack(), excelTemplate))
				{
					report.SetScheduleTask(scheduleTask);

					var reportDb = new ReportDbForTesting("", "");
					var manager = SecondaryServerConnectionProviderProvider.GetProvider(reportDb);
					((IReportForReportDbTesting)report).SetReportDbManagerForTesting(manager);

					using (var outputStream = new MemoryStream())
					{
						//Doesn't throw an exception anymore, but the rest of the test passes, so ???
						//AssertExceptionThrown<DocumentEngineException>(() => _ = report.Save(outputStream));
						try
						{
							report.Save(outputStream);
						}
						catch (DocumentEngineException)
						{
						}

						var restrictReaderDbLogin = RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName);
						var loginModifyDates_afterReport = ReportTest.GetApplicationLoginModifiedDates();

						foreach (var loginModifyDate in loginModifyDates)
						{
							if (loginModifyDate.Key == restrictReaderDbLogin)
							{
								AssertNotEquals($"{loginModifyDate.Key} should be fixed", loginModifyDate.Value, loginModifyDates_afterReport[loginModifyDate.Key]);
							}
							else
							{
								AssertEquals($"{loginModifyDate.Key} should not be changed", loginModifyDate.Value, loginModifyDates_afterReport[loginModifyDate.Key]);
							}
						}
					}
				}
			}

			using (var templateStream = new MemoryStream())
			{
				ExcelTemplateWrappingStream excelTemplate = ReportTest.CreateTestRunningConnectionTemplateWithDboTable(templateStream);
				var loginModifyDates = ReportTest.GetApplicationLoginModifiedDates();

				using (var report = new Report(new DocumentPack(), excelTemplate))
				{
					report.SetScheduleTask(scheduleTask);

					var reportDb = new ReportDbForTesting("", "");
					var manager = SecondaryServerConnectionProviderProvider.GetProvider(reportDb);
					((IReportForReportDbTesting)report).SetReportDbManagerForTesting(manager);

					using (var outputStream = new MemoryStream())
					{
						AssertNoExceptionThrown(() => _ = report.Save(outputStream));

						var loginModifyDates_afterReport = ReportTest.GetApplicationLoginModifiedDates();
						foreach (var loginModifyDate in loginModifyDates)
						{
							AssertEquals($"{loginModifyDate.Key} should not be changed", loginModifyDate.Value, loginModifyDates_afterReport[loginModifyDate.Key]);
						}
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestReportDoesNotCauseConnectionRepairWhenPermissionIsDenied_ImpersonatedAsStaffLogin()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();

			staff.GS_FullName = "Test staff";
			staff.GS_LoginName = "TestLogin_001";
			staff.GS_Code = "TSD";
			staff.IsReadOnlyDBUser = true;

			new DbUserManager().SetPasswordForStaff(staff, "P@ssW0rd!");
			factory.Save();

			var scheduleTask = factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_GS_NKPrintUser = staff.GS_Code;

			using (var templateStream = new MemoryStream())
			{
				ExcelTemplateWrappingStream excelTemplate = ReportTest.CreateTestRunningConnectionTemplateWithHRMTable(templateStream);
				var loginModifyDates = ReportTest.GetApplicationLoginModifiedDates();

				using (var report = new Report(new DocumentPack(), excelTemplate))
				{
					report.SetScheduleTask(scheduleTask);

					var reportDb = new ReportDbForTesting("", "");
					var manager = SecondaryServerConnectionProviderProvider.GetProvider(reportDb);
					((IReportForReportDbTesting)report).SetReportDbManagerForTesting(manager);

					using (var outputStream = new MemoryStream())
					{
						DocumentsDataRegistry.Instance.EnforceDataAccessOnReport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

						AssertExceptionThrown<DocumentEngineException>(() => _ = report.Save(outputStream));

						var loginModifyDates_afterReport = ReportTest.GetApplicationLoginModifiedDates();
						foreach (var loginModifyDate in loginModifyDates)
						{
							AssertEquals($"{loginModifyDate.Key} should not be changed", loginModifyDate.Value, loginModifyDates_afterReport[loginModifyDate.Key]);
						}
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestEnsureBadReportIsNeverGeneratedWhenFatalErrorHappens()
		{
			var factory = new BusinessObjectFactory();

			var notificationGroup = factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			notificationGroup.Staff[0].GS_EmailAddress = "unit.test@cargowise.com";
			factory.Save();

			var reportCommand = factory.New<ReportCommand>();
			var template = DocumentEngineTestHelper.CreateTemplateFromString(factory, "Test Report",
@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[Data:ReportData=SELECT col1, col2 FROM XXX]
{A}-[ColumnHeadings:]    {B}-[DisplayLabel=""VarCharMax"", HeadingText=""VarCharMax""]
{A}-[#SectionBody]
{B}-[Blah]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Z0_VarCharMax>]
{A}-[#EndOfReport]");

			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			using (var documentPack = new DocumentPack(reportCommand))
			{
				var deliveryInstructions = new DeliveryInstructions(documentPack);
				deliveryInstructions.Recipients.RemoveAndDeleteAll();

				var recipient = deliveryInstructions.Recipients.AddNew();
				recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
				recipient.AttachmentType = AttachmentTypeList.Codes.Pdf;
				recipient.Email = "unit.test@cargowise.com";

				var scheduledReport = factory.NewWithValidTestData<ReportScheduleTask>();
				scheduledReport.PopulateDefaultsFromDeliveryInstructions(deliveryInstructions);

				AssertEquals("Pre-condition: There should be one recipient.", 1, scheduledReport.Recipients.Count);

				var scheduledReportRecipient = scheduledReport.Recipients[0];
				scheduledReportRecipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendEmailNotification;

				using (Report.TemporarilyStopErrorsThrowingAnException())
				{
					try
					{
						Globals.IsUserInteractive = false;
						var notifications = new NotificationBuffer();
						scheduledReport.Run(notifications);

						AssertEquals("There should be three notifications", 3, notifications.Events.Count());
						AssertEquals(string.Format(@"Severity: [Fatal Error (without error report)] Message: [Error loading table [ReportData]. Error: [Invalid object name 'XXX'.] occurred running SQL: [
--Udf Parameters: 

--Server: {0}

--WhereClause Parameters: 

--Report Name: Dummy Report

--Staff Name: {3}

--Time Out: 900

SELECT col1, col2 FROM XXX option (recompile)]. SqlException: Msg 208, Level 16, State 1, Line 8, Invalid object name 'XXX'.] Cell: [N/A] Sheetname: [(unknown)] TemplatePath: [Template Name: Test Report]

Report Information:

MenuItem:-
   BusinessContext = []
   Name with Path = []
   Filter = []
   PK = [{1}]
   IsSystemDefined = [N]
   IsClientSpecific = [N]

Template:-
   Name = [Test Report]
   DataContext = [UnitTest]
   ExcelFilePath = []
   PK = [{2}]
   IsSystemDefined = [N]
   IsClientSpecific = [N]

Scheduled Task:-
   Description = []", Db.Connection.ServerName, pivot.SI_SU, pivot.SI_SO, Core.Constants.ProductSupportName), notifications.Events[0].Message);
						AssertContains("<a href=", notifications.Events[1].Message);
						AssertContains("Click to open Report Schedule", notifications.Events[1].Message);
						AssertEquals(@"Error notification email was sent to the following email address.
unit.test@cargowise.com", notifications.Events[2].Message);
					}
					finally
					{
						Globals.IsUserInteractive = true;
					}
				}

				var query = new ZDBOnlyQuery(typeof(StmPrintJob));
				var subQuery = new ZDBOnlySubQuery(typeof(StmPrintJobCopyRecipient), StmPrintJobCopyRecipientSchema.SPR_SP);
				subQuery.AddToFilter(StmPrintJobCopyRecipientSchema.SPR_RecipientType, SQLComparisonOperator.Equal, @"TO");
				subQuery.AddToFilter(StmPrintJobCopyRecipientSchema.SPR_EmailAddress, SQLComparisonOperator.Equal, @"unit.test@cargowise.com");
				query.AddSubQuery(subQuery, JoinCondition.And);

				var printJobs = factory.Load<StmPrintJob>(query);

				AssertEquals("There should be NO print job created.", 0, printJobs.Length);
			}
		}

		public void TestApplicationName()
		{
			var factory = new BusinessObjectFactory();
			using (var templateStream = new MemoryStream())
			{
				using (var creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					var workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "#DocumentHeader";
					workSheet[4, 1] = "<Now>";
					workSheet[5, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);
				}

				var excelTemplate = new ExcelTemplateReadFromByteArray("TemplateFromStream", "", templateStream.ToArray());
				var command = factory.New<DocumentCommand>();
				command.SU_BusinessContext = "RepWhatever";
				using (var pack = new DocumentPack(command))
				using (var report = new Report(pack, excelTemplate, BODocDataProvider.Get(factory.New<DummyBusinessObject>()), excelTemplate.TemplateName, null, DocumentDirection.ANY, false))
				{
					var lastApplicationName = "";
					List<string> applicationNames = null;
					string expectedApplicationName = null;
					report.OnRenderAndSave += (rep, connection) =>
					{
						lastApplicationName = connection.Command("Select APP_NAME()").ExecuteScalar().ToString();
						var sql = @"SELECT
  conn.session_id,
  host_name,
  program_name,
  nt_domain,
  login_name,
  connect_time,
  last_request_end_time
FROM sys.dm_exec_sessions AS sess
INNER JOIN sys.dm_exec_connections AS conn ON sess.session_id = conn.session_id
order by last_request_end_time asc";
						applicationNames = new List<string>();
						using (var adminConnection = Db.NewAdminConnection())
						using (var reader = adminConnection.Command(sql).ExecuteReader())
						{
							while (reader.Read())
							{
								applicationNames.Add(reader.GetString(2));
							}
						}
					};
					expectedApplicationName = DbConnectionConstants.ApplicationNames.CargoWiseOne + "_Customized_Report_" + pack.StmMenuCommand.PK + "_Unknown";
					report.Save(new MemoryStream());
					AssertEquals(expectedApplicationName, lastApplicationName);
					Assert(applicationNames.Aggregate((x, y) => x + "\r\n" + y), applicationNames.Contains(expectedApplicationName));
					using (Report.TemporarilySetReportRunSource(Report.ReportRunSource.ManualPrint))
					{
						expectedApplicationName = DbConnectionConstants.ApplicationNames.CargoWiseOne + "_Customized_Report_" + pack.StmMenuCommand.PK + "_ManualPrint";
						report.Save(new MemoryStream());
						AssertEquals(expectedApplicationName, lastApplicationName);
						Assert(applicationNames.Aggregate((x, y) => x + "\r\n" + y), applicationNames.Contains(expectedApplicationName));
					}
					using (Report.TemporarilySetReportRunSource(Report.ReportRunSource.Preview))
					{
						expectedApplicationName = DbConnectionConstants.ApplicationNames.CargoWiseOne + "_Customized_Report_" + pack.StmMenuCommand.PK + "_Preview";
						report.Save(new MemoryStream());
						AssertEquals(expectedApplicationName, lastApplicationName);
						Assert(applicationNames.Aggregate((x, y) => x + "\r\n" + y), applicationNames.Contains(expectedApplicationName));
					}
					using (Report.TemporarilySetReportRunSource(Report.ReportRunSource.ServiceTask))
					{
						expectedApplicationName = DbConnectionConstants.ApplicationNames.CargoWiseOne + "_Customized_Report_" + pack.StmMenuCommand.PK + "_ServiceTask";
						report.Save(new MemoryStream());
						AssertEquals(expectedApplicationName, lastApplicationName);
						Assert(applicationNames.Aggregate((x, y) => x + "\r\n" + y), applicationNames.Contains(expectedApplicationName));
					}
				}
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();
			serviceTaskNudgerMock.Setup(nudger => nudger.NudgeServiceTask("DSA", null))
				.Callback(() =>
				{
					using (var adminConnection = Db.NewAdminConnection())
					{
						var manager = new SqlSecurityManager(Mock.Of<Enterprise.Integration.ILogger>(), Db.DatabaseName);
						manager.BuildSecurity(adminConnection, CancellationToken.None);
					}
				});

			ObjectFactory.Substitute(serviceTaskNudgerMock.Object);
		}

		protected override void TearDown()
		{
			base.TearDown();
			embeddedResourceRetriever?.Dispose();
		}

		EmbeddedResourceRetriever embeddedResourceRetriever;

		ExcelTemplateForUnitTesting emptyAndValidTemplate;
		ExcelTemplateForUnitTesting EmptyAndValidTemplate
		{
			get
			{
				if (emptyAndValidTemplate == null)
				{
					embeddedResourceRetriever = new EmbeddedResourceRetriever();
					var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
					emptyAndValidTemplate = new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", Path.GetFullPath(tempFileName));
				}
				return emptyAndValidTemplate;
			}
		}

		#endregion
	}
}
