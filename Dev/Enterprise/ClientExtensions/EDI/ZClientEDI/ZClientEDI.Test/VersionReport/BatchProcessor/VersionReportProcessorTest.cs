using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.VersionReporting.BatchProcessor.Testing
{
	abstract class VersionReportProcessorTest : TestCaseWithFactory
	{
		public virtual void TestProcessValidXmlData()
		{
			SetupTestData();
			ServerVersionPK = PreviousBuild.PK;
			SetVersionNumber(PreviousBuild, "1.1.1885.10000");
			SetVersionNumber(NewBuild, "1.1.1990.10000");
			Factory.Save();
			string xmlData = GetXmlData(Organisation.PK);
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals("Server Version", NewBuild.PK, ServerVersionPK);
		}

		public virtual void TestProcessValidXmlDataWithMissingPreviousVersion()
		{
			SetupTestData();
			ServerVersionPK = ZGuid.Empty;
			SetVersionNumber(NewBuild, "1.1.1990.10000");
			Factory.Save();
			string xmlData = GetXmlData(Organisation.PK);
			AssertNotNullOrEmpty("XMLData exists", xmlData);
			VersionReportProcessorForTest.Process(xmlData);
		}

		public virtual void TestProcessValidXmlDataWithMissingNewVersion()
		{
			SetupTestData();
			Database.LD_HL_CurrentRunningVersion = PreviousBuild.PK;
			SetVersionNumber(PreviousBuild, "1.1.1885.10000");
			Factory.Save();
			string xmlData = GetXmlData(Organisation.PK);
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals("Server Version", ZGuid.Empty, ServerVersionPK);
		}

		public virtual void TestProcessValidXmlDataWithSameVersion()
		{
			SetupTestData();
			ServerVersionPK = PreviousBuild.PK;
			SetVersionNumber(PreviousBuild, "1.1.1990.10000");
			Factory.Save();
			string xmlData = GetXmlData(Organisation.PK);
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals("Server Version", PreviousBuild.PK, ServerVersionPK);
		}

		public virtual void TestProcessValidXmlDataWithMissingOrganisationPK()
		{
			SetupTestData();
			ServerVersionPK = PreviousBuild.PK;
			SetVersionNumber(PreviousBuild, "1.1.1885.10000");
			SetVersionNumber(NewBuild, "1.1.1990.10000");
			Factory.Save();
			string xmlData = GetXmlData(ZGuid.Empty);
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals("Server Version", NewBuild.PK, ServerVersionPK);
		}

		public virtual void TestProcessInvalidXmlData()
		{
			string xmlData = "Some dodgy virus.";
			VersionReportProcessor reportProcessor = VersionReportProcessorForTest;
			bool exceptionCaught = false;
			try
			{
				reportProcessor.Process(xmlData);
			}
			catch (InvalidOperationException e)
			{
				exceptionCaught = true;
				AssertEquals("xmlData is not valid.\r\n\r\nxmlData:\r\n\r\nSome dodgy virus.", e.Message);
			}

			IVersionReportProcessorForTest test = reportProcessor as IVersionReportProcessorForTest;
			Assert(exceptionCaught);
			AssertNotNull("Should support Test interface", test);
			AssertEquals("ProcessVersionReport should not have been called", false, test.ProcessVersionReportWasCalled());
		}

		public virtual void TestHandleConcurrencyException()
		{
			SetupTestData();
			ServerVersionPK = PreviousBuild.PK;
			SetVersionNumber(PreviousBuild, "1.1.1885.10000");
			SetVersionNumber(NewBuild, "1.1.1990.10000");
			Factory.Save();
			string xmlData = GetXmlData(Organisation.PK);
			DummyVersionReportProcessorWithConcurrencyException reportProcessor = new DummyVersionReportProcessorWithConcurrencyException();
			try
			{
				reportProcessor.Process(xmlData);
			}
			catch (Exception)
			{
			}

			AssertEquals("Should have run 3 times", 3, reportProcessor.ProcessVersionReportCalled);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			GlbGroup mailGroup = Factory.New<GlbGroup>();
			GlbStaff currentStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentStaff.GS_EmailAddress = "teststaff@edi.com.au";
			mailGroup.GG_Code = "TXB";
			mailGroup.Staff.Add(currentStaff);
			Factory.Save();
		}

		protected override void TearDown()
		{
			ReportProcessorHelper.ClearReportsFromTesting("DummyVersionReport");
			base.TearDown();
		}

		protected virtual void SetupTestData()
		{
			TestCaseHelper.ClearTable(LicenceConnectionSchema.Constants.TableName);
			TestCaseHelper.ClearTable(LicenceModulesSchema.Constants.TableName);
			TestCaseHelper.ClearTable(LicenceHeaderSchema.Constants.TableName);
			TestCaseHelper.ClearTable(LicenceDatabaseSchema.Constants.TableName);
			Organisation = Factory.New<EDIOrgHeader>();
			Organisation.CreateAndLoadLicenceForOrg();
			Enterprise = Organisation.LicEnterprise;
			Company = Organisation.LicCompany;
			Database = Company.LicDatabases.AddNew();
			Database.LD_DatabaseNumber = 98765;
			Header = Company.GetHeader(Database);
			PreviousBuild = Factory.NewWithValidTestData<ReleaseBuild>();
			NewBuild = Factory.NewWithValidTestData<ReleaseBuild>();
			NextBuild = Factory.NewWithValidTestData<ReleaseBuild>();
			Organisation.OH_RL_NKClosestPort = "AUSYD";
			Organisation.OH_FullName = "ABCXYZ";
			Organisation.OH_Code = "ABCXYZ";
			Organisation.MainAddress.OA_Address1 = "Address1";
			Enterprise.LE_EnterpriseCode = "ABC";
			Company.LC_CompanyCode = "XYZ";
			Company.LC_RX_NKCurrency = "AUD";
			Database.LD_ServerCode = "123";
			Database.LD_Product = ProductTypes.Codes.Enterprise;
			Factory.Save();
		}

		protected void SetVersionNumber(ReleaseBuild build, string versionNumber)
		{
			string[] versionNumberArray = versionNumber.Split('.');
			build.HL_MajorVersion = ZInt.Parse(versionNumberArray[0]);
			build.HL_MinorVersion = ZInt.Parse(versionNumberArray[1]);
			build.HL_Release = ZInt.Parse(versionNumberArray[2]);
			build.HL_Patch = ZInt.Parse(versionNumberArray[3]);
		}

		const string ScheduleStateUPG = "<HostedServiceSerializableSettings><ConfigString>TestScheduleStateUPG</ConfigString><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></HostedServiceSerializableSettings>";
		const string ScheduleStateMUG = "<HostedServiceSerializableSettings><ConfigString>TestScheduleStateMUG</ConfigString><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></HostedServiceSerializableSettings>";

		protected string GetXmlData(ZGuid organisationPK)
		{
			return GetXmlData(organisationPK, false);
		}

		protected string GetXmlData(ZGuid organisationPK, bool splitDBServerAndInstance, string outboundeAdapterUrl = null, bool tokenAuthenticationEnabled = true)
		{
			string dbInstance = splitDBServerAndInstance ? "DBS\\INST" : "DBS";
			List<String> additionalInfoList = new List<string>();
			additionalInfoList.Add("Host Name:                 SYD-WSCW-1");
			additionalInfoList.Add("OS Name:                   Microsoft Windows 7 Enterprise");
			additionalInfoList.Add("OS Version:                6.1.7600 N/A Build 7600");
			additionalInfoList.Add("System Manufacturer:       Gigabyte Technology Co., Ltd.");
			additionalInfoList.Add("BIOS Version:              Award Software International, Inc. F5, 18/06/2008");
			additionalInfoList.Add("System Locale:             en-us;English (United States)");
			additionalInfoList.Add("Total Physical Memory:     8,190 MB");
			additionalInfoList.Add("Time Zone:                 (UTC+10:00) Canberra, Melbourne, Sydney");
			additionalInfoList.Add("Processor(s):              1 Processor(s) Installed.");
			additionalInfoList.Add("                           [01]: Intel64 Family 6 Model 23 Stepping 10 GenuineIntel ~3000 Mhz");
			additionalInfoList.Add("System Model:              EP45-DS3P");
			var report = new EDIVersionReport(organisationPK, "ABC", "XYZ", "123", dbInstance, "DBN", "1.1.1990.10000", "", new ZDateTime(2004, 12, 31, 13, 56, 0), additionalInfoList, outboundeAdapterUrl: outboundeAdapterUrl ?? "https://antongorlin.com", nextRunTimeUtcUPG: new ZDateTime(2023, 6, 8, 15, 0, 0), nextRunTimeUtcMUG: new ZDateTime(2023, 6, 8, 16, 0, 0), scheduleStateUPG: ScheduleStateUPG, scheduleStateMUG: ScheduleStateMUG, tokenAuthenticationEnabled: tokenAuthenticationEnabled);
			return report.GenerateLegacyXmlForTest();
		}

		protected string GetXmlDataWithAdditionalInfoInChinese(ZGuid organisationPK)
		{
			List<String> additionalInfoList = new List<string>();
			additionalInfoList.Add("主机名:						SYD-WSCW-1");
			additionalInfoList.Add("OS 名称:						Microsoft Windows 7 Enterprise");
			additionalInfoList.Add("OS 版本:						6.1.7600 N/A Build 7600");
			additionalInfoList.Add("系统制造商:					Gigabyte Technology Co., Ltd.");
			additionalInfoList.Add("BIOS 版本:					GATEWA - 20060512");
			additionalInfoList.Add("系统区域设置:				zh-cn;中文(中国)");
			additionalInfoList.Add("物理内存总量:				8,190 MB");
			additionalInfoList.Add("时区:						(UTC+10:00) Canberra, Melbourne, Sydney");
			additionalInfoList.Add("处理器:						1 Processor(s) Installed.");
			additionalInfoList.Add("							[01]: Intel64 Family 6 Model 23 Stepping 10 GenuineIntel ~3000 Mhz");
			additionalInfoList.Add("系统型号:					EP45-DS3P");
			var report = new EDIVersionReport(organisationPK, "ABC", "XYZ", "123", "DBS", "DBN", "1.1.1990.10000", "", new ZDateTime(2004, 12, 31, 13, 56, 0), additionalInfoList);
			return report.GenerateLegacyXmlForTest();
		}

		protected string GetXmlDataWithVMWare(ZGuid organisationPK)
		{
			List<String> additionalInfoList = new List<string>();
			additionalInfoList.Add("Host Name:                 SYD-WSCW-1");
			additionalInfoList.Add("OS Name:                   Microsoftr Windows Serverr 2008 Enterprise");
			additionalInfoList.Add("OS Version:                6.0.6002 Service Pack 2 Build 6002");
			additionalInfoList.Add("System Manufacturer:       VMware, Inc.");
			additionalInfoList.Add("BIOS Version:              Phoenix Technologies LTD 6.00, 10/13/2009");
			additionalInfoList.Add("System Locale:             en-us;English (United States)");
			additionalInfoList.Add("Total Physical Memory:     24,575 MB");
			additionalInfoList.Add("Time Zone:                 (GMT+01:00) Brussels, Copenhagen, Madrid, Paris");
			additionalInfoList.Add("Processor(s):              1 Processor(s) Installed.");
			additionalInfoList.Add("                           [01]: Intel64 Family 6 Model 23 Stepping 10 GenuineIntel ~3000 Mhz");
			additionalInfoList.Add("System Model:              VMware Virtual Platform");
			var report = new EDIVersionReport(organisationPK, "ABC", "XYZ", "123", "DBS", "DBN", "1.1.1990.10000", "", new ZDateTime(2004, 12, 31, 13, 56, 0), additionalInfoList);
			return report.GenerateLegacyXmlForTest();
		}

		protected EDIOrgHeader Organisation;
		protected LicenceEnterprise Enterprise;
		protected LicenceCompany Company;
		protected LicenceDatabase Database;
		protected LicenceHeader Header;
		protected ReleaseBuild PreviousBuild;
		protected ReleaseBuild NewBuild;
		protected ReleaseBuild NextBuild;
		protected ZGuid ServerVersionPK
		{
			get
			{
				return GetServerVersionPK();
			}

			set
			{
				SetServerVersionPK(value);
			}
		}

		protected abstract ZGuid GetServerVersionPK();
		protected abstract void SetServerVersionPK(ZGuid versionPK);
		protected VersionReportProcessor VersionReportProcessorForTest
		{
			get
			{
				return GetVersionReportProcessorForTest();
			}
		}

		protected virtual VersionReportProcessor GetVersionReportProcessorForTest()
		{
			return new DummyVersionReportProcessorForTest();
		}

		#region VersionReportProcessor class For Test
		protected interface IVersionReportProcessorForTest
		{
			bool ProcessVersionReportWasCalled();
		}

		class DummyVersionReportProcessorForTest : VersionReportProcessor, IVersionReportProcessorForTest
		{
			public bool ProcessVersionReportWasCalled()
			{
				return ProcessVersionReportCalled;
			}

			protected bool ProcessVersionReportCalled;
			protected override void ProcessVersionReport(EDIVersionReport report)
			{
				ProcessVersionReportCalled = true;
			}

			protected override string GetReportId()
			{
				return "DummyVersionReport";
			}
		}

		class DummyVersionReportProcessorWithConcurrencyException : VersionReportProcessor
		{
			public int ProcessVersionReportCalled
			{
				get
				{
					return processVersionReportCalled;
				}
			}

			protected int processVersionReportCalled;
			protected override void ProcessVersionReport(EDIVersionReport report)
			{
				processVersionReportCalled++;
				throw new DBConcurrencyException();
			}

			protected override string GetReportId()
			{
				return "DummyVersionReport";
			}
		}
		#endregion
		#endregion
	}
}
