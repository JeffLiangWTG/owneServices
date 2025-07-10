using CargoWise.Types;
using Enterprise.Client.EDI.AutoDeploy;
using Enterprise.Client.EDI.AutoDeploy.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.VersionReporting.BatchProcessor.Testing
{
	class DeliveredVersionReportProcessorTest : VersionReportProcessorTest
	{
		public void TestLogging()
		{
			base.SetupTestData();
			SetVersionNumber(NewBuild, "1.4.3600.1");
			Factory.Save();
			TestServiceLogger logger = new TestServiceLogger();
			DeliveredVersionReportProcessor processor = new DeliveredVersionReportProcessor("server@cargowise.com", logger);
			DeliveredVersionReportProcessor processorWithoutLogger = new DeliveredVersionReportProcessor();
			var report = new EDIVersionReport(ZGuid.Empty, "ZZZ", "XXX", "PRD", "", "", "1.4.3000.1", "", new ZDateTime(2009, 11, 11));
			processor.ProcessWithoutSavingReport(report.GenerateLegacyXmlForTest());
			processorWithoutLogger.ProcessWithoutSavingReport(report.GenerateLegacyXmlForTest());
			AssertEquals("logger.Count", 1, logger.Count);
			AssertEquals("log msg", "Warning|LicenceDatabase [ZZZ/XXX/PRD] from sender server@cargowise.com is unknown", logger[0]);
			logger.ClearLog();
			report = new EDIVersionReport(ZGuid.Empty, Enterprise.LE_EnterpriseCode, Company.LC_CompanyCode, Database.LD_ServerCode, "", "", NewBuild.VersionNumber.ToString(), "", new ZDateTime(2009, 11, 11));
			processor.ProcessWithoutSavingReport(report.GenerateLegacyXmlForTest());
			processorWithoutLogger.ProcessWithoutSavingReport(report.GenerateLegacyXmlForTest());
			AssertEquals("logger.Count", 1, logger.Count);
			AssertEquals("log msg", "Information|LicenceDatabase [ABC/XYZ/123] received update v1.4.3600.1", logger[0]);
			logger.ClearLog();
			report = new EDIVersionReport(ZGuid.Empty, Enterprise.LE_EnterpriseCode, Company.LC_CompanyCode, Database.LD_ServerCode, "", "", "1.9.9874.7000", "", new ZDateTime(2009, 11, 11));
			processor.ProcessWithoutSavingReport(report.GenerateLegacyXmlForTest());
			processorWithoutLogger.ProcessWithoutSavingReport(report.GenerateLegacyXmlForTest());
			AssertEquals("logger.Count", 1, logger.Count);
			AssertEquals("log msg", "Warning|LicenceDatabase [ABC/XYZ/123] received unknown build v1.9.9874.7000", logger[0]);
		}

		public virtual void TestProcessScheduledUpgrade()
		{
			TestProcessValidXmlData();
			ScheduledUpgrade.Reload();
			AssertEquals("Status of upgrade changed to Received", UpgradesToClientStatus.Codes.Received, ScheduledUpgrade.L1_CurrentStatus);
		}

		public void TestUpdateExeVersion_CargoWiseNext()
		{
			base.SetupTestData();
			SetVersionNumber(NewBuild, "24.12.10.44");
			NewBuild.HL_Product = ProductTypes.Codes.CargoWiseNext;
			Factory.Save();
			var logger = new TestServiceLogger();
			var processor = new DeliveredVersionReportProcessor("server@cargowise.com", logger);
			var report = new EDIVersionReport(ZGuid.Empty, Enterprise.LE_EnterpriseCode, Company.LC_CompanyCode, Database.LD_ServerCode, "", "", NewBuild.VersionNumber.ToString(), "", new ZDateTime(2024, 12, 11));
			processor.ProcessWithoutSavingReport(report.GenerateLegacyXmlForTest());
			AssertEquals("logger.Count", 1, logger.Count);
			AssertEquals("log msg", "Information|LicenceDatabase [ABC/XYZ/123] received update v24.12.10.44", logger[0]);
			logger.ClearLog();
		}

		public void TestUpdateExeVersion_CargoWise()
		{
			base.SetupTestData();
			SetVersionNumber(NewBuild, "24.12.10.44");
			NewBuild.HL_Product = ProductTypes.Codes.CargoWise;
			Factory.Save();
			var logger = new TestServiceLogger();
			var processor = new DeliveredVersionReportProcessor("server@cargowise.com", logger);
			var report = new EDIVersionReport(ZGuid.Empty, Enterprise.LE_EnterpriseCode, Company.LC_CompanyCode, Database.LD_ServerCode, "", "", NewBuild.VersionNumber.ToString(), "", new ZDateTime(2024, 12, 11));
			processor.ProcessWithoutSavingReport(report.GenerateLegacyXmlForTest());
			AssertEquals("The CurrentSentVersion should be updated.", NewBuild.PK, Database.LD_HL_CurrentSentVersion);
			AssertEquals("logger.Count", 1, logger.Count);
			AssertEquals("log msg", "Information|LicenceDatabase [ABC/XYZ/123] received update v24.12.10.44", logger[0]);
			logger.ClearLog();
		}

		#region Implementation
		protected override void SetupTestData()
		{
			TestCaseHelper.ClearTable(UpgradesToClientSchema.Constants.TableName);
			base.SetupTestData();
			ScheduledUpgrade = Factory.New<UpgradesToClient>();
			ScheduledUpgrade.L1_HL = NewBuild.PK;
			ScheduledUpgrade.L1_LD = Database.PK;
			ScheduledUpgrade.L1_CurrentStatus = UpgradesToClientStatus.Codes.Processed;
		}

		protected override ZGuid GetServerVersionPK()
		{
			return Header.Database.LD_HL_CurrentSentVersion;
		}

		protected override void SetServerVersionPK(ZGuid versionPK)
		{
			Header.Database.LD_HL_CurrentSentVersion = versionPK;
		}

		protected override VersionReportProcessor GetVersionReportProcessorForTest()
		{
			return new DeliveredVersionReportProcessorForTest();
		}

		protected override void TearDown()
		{
			ReportProcessorHelper.ClearReportsFromTesting("DeliveredVersionReport");
			base.TearDown();
		}

		protected UpgradesToClient ScheduledUpgrade;
		#region VersionReportProcessor class For Test
		class DeliveredVersionReportProcessorForTest : DeliveredVersionReportProcessor, IVersionReportProcessorForTest
		{
			public bool ProcessVersionReportWasCalled()
			{
				return ProcessVersionReportCalled;
			}

			protected bool ProcessVersionReportCalled;
			protected override void ProcessVersionReport(EDIVersionReport report)
			{
				ProcessVersionReportCalled = true;
				base.ProcessVersionReport(report);
			}
		}
		#endregion
		#endregion
	}
}
