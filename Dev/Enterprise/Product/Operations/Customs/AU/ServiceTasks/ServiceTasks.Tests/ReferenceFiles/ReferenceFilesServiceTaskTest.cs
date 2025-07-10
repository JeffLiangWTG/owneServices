using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Integration.Customs.AU;

namespace Enterprise.Customs.AU.ServiceTasks.Testing
{
	[TestedType(typeof(ReferenceFilesServiceTask))]
	sealed class ReferenceFilesServiceTaskTest : ServiceTaskTestCase<ReferenceFilesServiceTask>
	{
		[TestDate]
		public void TestUpdateSuccessCommitsTransaction()
		{
			ClearRefDb();
			AssertRefRowsInDB("no existing entries", 0);
			SetupCertificates();

			var serviceTask = new ReferenceFilesServiceTaskTestObject();
			serviceTask.SimulateUpdate = true;
			serviceTask.SimulateUpdateError = false;
			var logger = InitialiseTaskSchedule(serviceTask);

			var testSourceLastModified = ZDateTime.MaxSmallDateTimeValue.AddDays(-1).ToDateTime();
			serviceTask.SetSourceLastModified(testSourceLastModified);
			RunTaskSchedule(serviceTask);

			AssertEquals("Log count", true, logger.Count > 0);
			string expectedLogEntry = "Information|AU Customs Reference files updated - Last Modified = " + testSourceLastModified.ToString(CultureInfo.CurrentCulture);
			AssertContains("Log entry", expectedLogEntry, logger.ToString().Trim());

			AssertRefRowsInDB("entry created", 1);
			Assert("Commit scheduled", !TestConnection.MustRollBack);
		}

		[TestDate]
		public void TestUpdateFailureRollsBackTransaction()
		{
			ClearRefDb();
			AssertRefRowsInDB("no existing entries", 0);
			SetupCertificates();

			var serviceTask = new ReferenceFilesServiceTaskTestObject();
			serviceTask.SimulateUpdate = true;
			serviceTask.SimulateUpdateError = true;
			var logger = InitialiseTaskSchedule(serviceTask);

			var testSourceLastModified = ZDateTime.MaxSmallDateTimeValue.AddDays(-1).ToDateTime();
			serviceTask.SetSourceLastModified(testSourceLastModified);
			RunTaskSchedule(serviceTask);

			AssertEquals("Log count", true, logger.Count > 0);
			string expectedLogEntry = "Error|Processing failed with errors.";
			AssertContains("Log entry", expectedLogEntry, logger.ToString().Trim());

			AssertRefRowsInDB("entry created", 1);
			Assert("Rollback scheduled", TestConnection.MustRollBack);

			var msg = @"Error while importing CMR Reference Files. The table has not been updated.";
			AssertStartsWith("Failure is reported", msg, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		[TestDate]
		public void TestUpdateLock()
		{
			SetupCertificates();
			var serviceTask = new ReferenceFilesServiceTaskTestObject();
			var logger = InitialiseTaskSchedule(serviceTask);
			DateTime testSourceLastModified = ZDateTime.MinSmallDateTimeValue.AddDays(1).ToDateTime();
			using (var otherConnection = Db.NewExtraConnectionToMainDb())
			{
				try
				{
					otherConnection.BeginTransaction();
					CmrAuReferenceFileUpdateMutex.AcquireUpdateLock(otherConnection, TimeSpan.Zero);
					serviceTask.SetSourceLastModified(testSourceLastModified);
					RunTaskSchedule(serviceTask);
					AssertEquals("Log count", true, logger.Count > 0);
					string expectedLogEntry = "Information|Full update currently being performed by another system.";
					AssertContains("Log entry", expectedLogEntry, logger.ToString().Trim());
				}
				finally
				{
					otherConnection.RollbackTransaction();
				}
			}

			logger.ClearLog();
			RunTaskSchedule(serviceTask);
			AssertEquals("Log count with successful lock", true, logger.Count > 0);
			var expectedLogEntryWithSuccessfulLock = string.Format(
				"Information|AU Customs Reference files updated - Last Modified = {0}",
				testSourceLastModified.ToString());
			AssertContains("Log entry with successful lock", expectedLogEntryWithSuccessfulLock, logger.ToString().Trim());
		}

		public void TestCmrModeDependsOnTheRefDbBeingExclusiveOrShared()
		{
			ReferenceFilesServiceTaskTestObject serviceTask;
			var logger = new TestServiceLogger();

			var factory = new BusinessObjectFactory();
			var webMediator = new CustomsWebMediatorForTesting(factory, logger, false);
			webMediator.ExceptionToThrow = new WebException("Cannot get web response");

			Env.Registry.CMRTestMode = false;
			serviceTask = new ReferenceFilesServiceTaskTestObject();
			AssertEquals("Logs contain: Warning|URL?", webMediator.UrlProduction, serviceTask.SourceUrl);
			serviceTask = new ReferenceFilesServiceTaskTestObject("CW-RefDb-Abc");
			AssertEquals("Logs contain: Warning|URL?", webMediator.UrlProduction, serviceTask.SourceUrl);

			Env.Registry.CMRTestMode = true;
			serviceTask = new ReferenceFilesServiceTaskTestObject();
			AssertEquals("Logs contain: Warning|URL?", webMediator.UrlTest, serviceTask.SourceUrl);
			serviceTask = new ReferenceFilesServiceTaskTestObject("CW-RefDb-Abc");
			AssertEquals("Logs contain: Warning|URL?", webMediator.UrlProduction, serviceTask.SourceUrl);
		}

		public void TestDownloadReferenceTestFileIfRegistrySettingIsTrue()
		{
			var logger = new TestServiceLogger();
			var factory = new BusinessObjectFactory();
			var webMediator = new CustomsWebMediatorForTesting(factory, logger, false);

			Env.Registry.CMRTestMode = false;
			using (AUCustomsDataRegistry.Instance.AlwaysUseIndustryTestCMRFiles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertSourceUrl("Download from the production location using a share DB in the production system with AlwaysUseIndustryTestCMRFiles set to false", webMediator.UrlProduction, DatabaseTypes.Codes.Production, "CW-RefDatabase");
				AssertSourceUrl("Download from the production location using a share DB in the test system with AlwaysUseIndustryTestCMRFiles set to false", webMediator.UrlProduction, DatabaseTypes.Codes.Test, "CW-RefDatabase");
				AssertSourceUrl("Download from the production location using a no share DB in the production system with AlwaysUseIndustryTestCMRFiles set to false", webMediator.UrlProduction, DatabaseTypes.Codes.Production, "Odyssey_RefDb_Cmr_AU");
				AssertSourceUrl("Download from the production location using a no share DB in the test system with AlwaysUseIndustryTestCMRFiles set to false", webMediator.UrlProduction, DatabaseTypes.Codes.Test, "Odyssey_RefDb_Cmr_AU");
			}

			using (AUCustomsDataRegistry.Instance.AlwaysUseIndustryTestCMRFiles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertSourceUrl("Download from the production location using a share DB in the production system with AlwaysUseIndustryTestCMRFiles set to true", webMediator.UrlProduction, DatabaseTypes.Codes.Production, "CW-RefDatabase");
				AssertSourceUrl("Download from the test location using a share DB in the test system with AlwaysUseIndustryTestCMRFiles set to true", webMediator.UrlTest, DatabaseTypes.Codes.Test, "CW-RefDatabase");
				AssertSourceUrl("Download from the production location using a no share DB in the production system with AlwaysUseIndustryTestCMRFiles set to true", webMediator.UrlProduction, DatabaseTypes.Codes.Production, "Odyssey_RefDb_Cmr_AU");
				AssertSourceUrl("Download from the test location using a no share DB in the test system with AlwaysUseIndustryTestCMRFiles set to true", webMediator.UrlTest, DatabaseTypes.Codes.Test, "Odyssey_RefDb_Cmr_AU");
			}

			Env.Registry.CMRTestMode = true;
			using (AUCustomsDataRegistry.Instance.AlwaysUseIndustryTestCMRFiles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertSourceUrl("Download from the production location using a share DB in the production system with AlwaysUseIndustryTestCMRFiles set to false", webMediator.UrlProduction, DatabaseTypes.Codes.Production, "CW-RefDatabase");
				AssertSourceUrl("Download from the production location using a share DB in the test system with AlwaysUseIndustryTestCMRFiles set to false", webMediator.UrlProduction, DatabaseTypes.Codes.Test, "CW-RefDatabase");
				AssertSourceUrl("Download from the test location using a no share DB in the production system with AlwaysUseIndustryTestCMRFiles set to false", webMediator.UrlTest, DatabaseTypes.Codes.Production, "Odyssey_RefDb_Cmr_AU");
				AssertSourceUrl("Download from the test location using a no share DB in the test system with AlwaysUseIndustryTestCMRFiles set to false", webMediator.UrlTest, DatabaseTypes.Codes.Test, "Odyssey_RefDb_Cmr_AU");
			}

			using (AUCustomsDataRegistry.Instance.AlwaysUseIndustryTestCMRFiles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertSourceUrl("Download from the production location using a share DB in the production system with AlwaysUseIndustryTestCMRFiles set to true", webMediator.UrlProduction, DatabaseTypes.Codes.Production, "CW-RefDatabase");
				AssertSourceUrl("Download from the test location using share a share DB in the test system with AlwaysUseIndustryTestCMRFiles set to true", webMediator.UrlTest, DatabaseTypes.Codes.Test, "CW-RefDatabase");
				AssertSourceUrl("Download from the test location using a no share DB in the production system with AlwaysUseIndustryTestCMRFiles set to true", webMediator.UrlTest, DatabaseTypes.Codes.Production, "Odyssey_RefDb_Cmr_AU");
				AssertSourceUrl("Download from the test location using share a no share DB in the test system with AlwaysUseIndustryTestCMRFiles set to true", webMediator.UrlTest, DatabaseTypes.Codes.Test, "Odyssey_RefDb_Cmr_AU");
			}
		}

		void AssertSourceUrl(string assertMessage, string expectedUrl, string licenseType, string dbName)
		{
			LicenceTypeChanger.SetSystemLicence(licenseType);
			var serviceTask = new ReferenceFilesServiceTaskTestObject(dbName);
			AssertEquals(assertMessage, expectedUrl, serviceTask.SourceUrl);
		}

		public void TestCompanyHasCustomsRegistrationNumber()
		{
			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.Australia))
			{
				branch.Company.GC_CustomsRegistrationNo = "";
				branch.Factory.Save();
			}

			var serviceTask = new ReferenceFilesServiceTaskTestObject();
			var logger = InitialiseTaskSchedule(serviceTask);
			RunTaskSchedule(serviceTask);
			AssertEquals("Log count", 1, logger.Count);
			string expectedLogEntry = "Error|There is currently no 'Customs>Australia>CMR>Company Certificate Key File' established for any AU Company. - CMR Reference File Downloader is not required.";
			AssertEquals("Log entry", expectedLogEntry, logger.ToString().Trim());
		}

		public void TestCompanyHasKeyFile()
		{
			GlbBranch.Loader branchLoader = new GlbBranch.Loader(Factory);
			foreach (var branch in branchLoader.LoadAllBranchesInThisCountryActiveOnly(Core.Constants.CountryCodes.Australia))
			{
				branch.Company.GC_CustomsRegistrationNo = "";
				Env.Registry.RawRegistry.AUCCompanyCertificateData.SetValue(branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, null);
				branch.Factory.Save();
			}

			var serviceTask = new ReferenceFilesServiceTaskTestObject();
			var logger = InitialiseTaskSchedule(serviceTask);
			RunTaskSchedule(serviceTask);
			AssertEquals("Log count", true, logger.Count > 0);
			string expectedLogEntry = "Error|There is currently no 'Customs>Australia>CMR>Company Certificate Key File' established for any AU Company. - CMR Reference File Downloader is not required.";
			AssertEquals("Log entry expected", true, logger.ToString().Trim().Contains(expectedLogEntry));
		}

		[TestDate]
		public void TestMultipleAUCompaniesFindsOneWithKeyFile()
		{
			GlbBranch.Loader branchLoader = new GlbBranch.Loader(Factory);
			foreach (var branch in branchLoader.LoadAllBranchesInThisCountryActiveOnly(Core.Constants.CountryCodes.Australia))
			{
				branch.Company.GC_CustomsRegistrationNo = "";
				Env.Registry.RawRegistry.AUCCompanyCertificateData.SetValue(branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, null);
			}

			Factory.Save();
			var aUForwardingCompany = Factory.NewWithValidTestData<GlbCompany>();
			aUForwardingCompany.GC_Code = "AUF";
			aUForwardingCompany.GC_RN_NKCountryCode = "AU";

			var aUTransportCompany = Factory.NewWithValidTestData<GlbCompany>();
			aUTransportCompany.GC_Code = "AUT";
			aUTransportCompany.GC_RN_NKCountryCode = "AU";

			var aUBrokerageCompany = Factory.NewWithValidTestData<GlbCompany>();
			aUBrokerageCompany.GC_Code = "AUB";
			aUBrokerageCompany.GC_RN_NKCountryCode = "AU";

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = aUForwardingCompany.PK;
			branch1.GB_RN_NKCountryCode = "AU";
			branch1.GB_RL_NKHomePort = "AUSYD";

			var branch4 = Factory.NewWithValidTestData<GlbBranch>();
			branch4.GB_GC = aUBrokerageCompany.PK;
			branch4.GB_RN_NKCountryCode = "AU";
			branch4.GB_RL_NKHomePort = "AUSYD";

			Env.Registry.RawRegistry.AUCCompanyCertificateData.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, null);
			var serviceTask = new ReferenceFilesServiceTaskTestObject();
			var logger = InitialiseTaskSchedule(serviceTask);
			RunTaskSchedule(serviceTask);
			AssertEquals("Log count", true, logger.Count > 0);
			string expectedLogEntry = "Error|There is currently no 'Customs>Australia>CMR>Company Certificate Key File' established for any AU Company. - CMR Reference File Downloader is not required.";
			AssertEquals("Log entry expected - no AU company has registry certificate established", true, logger.ToString().Trim().Contains(expectedLogEntry));

			//Update brokerage branch with certificate
			aUBrokerageCompany.GC_CustomsRegistrationNo = "12345";
			var certificatesHelper = ObjectFactory.New<ICertificateManagerHelper>(Factory);
			certificatesHelper.SetupValidCompanyCertificatesForTest(out _, out _, branch4.Company.PK.ToGuid());
			branch4.Factory.Save();

			var serviceTask2 = new ReferenceFilesServiceTaskTestObject();
			var logger2 = InitialiseTaskSchedule(serviceTask2);
			serviceTask2.SetSourceLastModified(DateTime.UtcNow.AddDays(1));
			logger2.ClearLog();
			RunTaskSchedule(serviceTask2);
			AssertEquals("Log count", true, logger2.Count > 0);
			expectedLogEntry = "Information|AU Customs Reference files updated - Last Modified = ";
			AssertEquals("Log entry", true, logger2.ToString().Trim().Contains(expectedLogEntry));
		}

		[TestDate]
		public void TestDownloaderDeniedWithInvalidCertificate()
		{
			SetupCertificates();
			DateTime certificateValidFromDate = auCompanyCertificateStartDate;
			DateTime certificateValidToDate = auCompanyCertificateEndDate;

			TestDateAttribute.Date = certificateValidToDate.AddMonths(-2);
			AssertServiceTaskLogContainsEntry("Information|AU Customs Reference files updated - Last Modified = ");

			TestDateAttribute.Date = certificateValidToDate.AddDays(14);
			AssertServiceTaskLogContainsEntry("CMR Reference File Downloader is not required");

			TestDateAttribute.Date = certificateValidToDate.AddDays(-14);
			AssertServiceTaskLogContainsEntry("Information|AU Customs Reference files updated - Last Modified = ");

			TestDateAttribute.Date = certificateValidFromDate.AddYears(-1);
			AssertServiceTaskLogContainsEntry("Information|AU Customs Reference files updated - Last Modified = ");
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		void ClearRefDb()
		{
			var sqlCommand = "Delete from " + CMRCodeListsSchema.Constants.TableName;
			using (var command = TestConnection.Command(sqlCommand))
			{
				command.ExecuteScalar();
			}
		}

		void AssertRefRowsInDB(ZString message, int count)
		{
			var sqlCommand = "Select count(*) from " + CMRCodeListsSchema.Constants.TableName;
			using (var command = TestConnection.Command(sqlCommand))
			{
				int result = (int)command.ExecuteScalar();
				AssertEquals(message, count, result);
			}
		}

		void AssertServiceTaskLogContainsEntry(string expectedLogEntry)
		{
			var serviceTask = new ReferenceFilesServiceTaskTestObject();
			var logger = InitialiseTaskSchedule(serviceTask);
			serviceTask.SetSourceLastModified(DateTime.UtcNow);
			logger.ClearLog();
			RunTaskSchedule(serviceTask);

			var logEntry = logger.ToString().Trim();
			AssertContains(expectedLogEntry, logEntry);
		}

		void SetupCertificates()
		{
			var certificatesHelper = ObjectFactory.New<ICertificateManagerHelper>(Factory);
			certificatesHelper.SetupValidCompanyCertificatesForTest(out auCompanyCertificateStartDate, out auCompanyCertificateEndDate);
			certificatesHelper.CreateCustomsCertificates();
			Factory.Save();
		}

		DateTime auCompanyCertificateStartDate;
		DateTime auCompanyCertificateEndDate;

		sealed class ReferenceFilesServiceTaskTestObject : ReferenceFilesServiceTask
		{
			public ReferenceFilesServiceTaskTestObject(string cmrAuRefDbName_Override = null)
				: base()
			{
				this.cmrAuRefDbName_Override = cmrAuRefDbName_Override;
				InitialiseAttributes(new BusinessObjectFactory());
			}

			public void SetRefDbAsUpdated(DbConnection refDbCnx, DateTime sourceLastModified)
			{
				webSourceLastModifiedTime_Override = sourceLastModified;
				SetLastSuccessfulUpdateTime(refDbCnx, sourceLastModified);
			}

			public void SetSourceLastModified(DateTime sourceLastModified)
			{
				webSourceLastModifiedTime_Override = sourceLastModified;
				new CMRReferenceFileUpdateLog(new BusinessObjectFactory()).LogUpdateSuccess(sourceLastModified);
			}

			public string SourceUrl { get { return (webMediator == null) ? null : webMediator.Url; } }

			protected override bool GetCustomsLastModifiedTime(out DateTime lastModified)
			{
				if (webSourceLastModifiedTime_Override == null)
				{
					return base.GetCustomsLastModifiedTime(out lastModified);
				}
				else
				{
					lastModified = webSourceLastModifiedTime_Override.Value;
					return true;
				}
			}
			DateTime? webSourceLastModifiedTime_Override;

			protected override string GetCmrAuReferenceDatabaseName(IPhysicalRefDbLocation refdbLocator)
			{
				if (cmrAuRefDbName_Override == null)
				{
					return base.GetCmrAuReferenceDatabaseName(refdbLocator);
				}
				else
				{
					return cmrAuRefDbName_Override;
				}
			}
			readonly string cmrAuRefDbName_Override;

			public bool SimulateUpdate;
			public bool SimulateUpdateError;

			protected override void RunFullUpdate(DbConnection refDbCnx, DateTime sourceLastModified, CMRReferenceFileUpdateLog updateLog)
			{
				// NOT TO REPEATEDLY REQUEST DATA FROM CUSTOMS WEBSITE

				if (SimulateUpdate)
				{
					var sqlCommand = $@"Insert into {CMRCodeListsSchema.Constants.TableName} (CI_CodeType, CI_Code, CI_CreateTimestamp, CI_Version, CI_Name, CI_Startdate, CI_EndDate, CI_Description, CI_PK)
Values ('ADVICESTAT', 'F', '20000101000000000000', 1, 'FINALISED', '20050109', null, 'THE ADVICE HAS BEEN GIVEN BY CUSTOMS', newid())";

					using (var command = refDbCnx.Command(sqlCommand))
					{
						command.ExecuteScalar();
					}

					if (SimulateUpdateError)
					{
						throw new UpdateReferenceFilesException("Processing failed with errors.");
					}
				}
			}
		}

		sealed class CustomsWebMediatorForTesting : CustomsWebMediator
		{
			public CustomsWebMediatorForTesting(BusinessObjectFactory factory, ILogger serviceLogger, bool cmrTestMode)
				: base(factory, serviceLogger, cmrTestMode)
			{
			}

			protected override HttpWebResponse GetWebResponse(HttpWebRequest webRequest)
			{
				WebRequestForTesting = webRequest;
				if (ExceptionToThrow != null)
				{
					throw ExceptionToThrow;
				}
				return null;
			}
			public HttpWebRequest WebRequestForTesting;
			public WebException ExceptionToThrow;
		}
	}
}
