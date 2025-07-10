using System.Collections.Generic;
using System.IO;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.STI.Navision.Testing
{
	[TestedType(typeof(NavisionServiceTask))]
	public class NavisionServiceTaskTest : ServiceTaskTestCase<NavisionServiceTask>
	{
		public void TestDebtorOrgsAreLoaded()
		{
			NavisionServiceTask serviceTask = new NavisionServiceTask();
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, ZBool.True);
			query.AddSubQuery(subQuery, JoinCondition.And);
			AssertEquals("Should only load Debtor Organisations", serviceTask.OrganisationCollectionForInitialExport.Count, Factory.GetDatabaseCount(typeof(OrgHeader), query));
		}

		[TestDate(2016, 8, 15, 15, 20, 31)]
		public void TestIsEnvironmentDataValid()
		{
			ZDateTime originalHWM = ZDateTime.UtcNow.AddHours(-1);
			STIDataRegistry.Instance.DataExportHighWaterMark = originalHWM;
			STIDataRegistry.Instance.OrganisationExportDirectory = ZString.Empty;
			STIDataRegistry.Instance.ShipmentExportDirectory = Env.TempPath;
			STIDataRegistry.Instance.InvoiceHeaderExportDirectory = Env.TempPath;
			STIDataRegistry.Instance.InvoiceLinesExportDirectory = Env.TempPath;
			string directoryErrorExpected = "Invalid Export Directories: OrganisationExportDirectory have not been specified in System -> Registry -> Strang International Client Extensions -> Navision";
			RunTaskSchedule(serviceTask);
			AssertContains("There should be an error with OrganisationExportDirectory", directoryErrorExpected, serviceTask.Buffer.AsString);
			AssertEquals("HighWaterMark should NOT be updated", originalHWM, STIDataRegistry.Instance.DataExportHighWaterMark);
			serviceTask.Buffer.Clear();
			STIDataRegistry.Instance.DisableOrganisationExport = true;
			RunTaskSchedule(serviceTask);
			AssertNotContains("There should NOT be an error with OrganisationExportDirectory", directoryErrorExpected, serviceTask.Buffer.AsString);
			AssertEquals("HighWaterMark should be updated", ZDateTime.UtcNow, STIDataRegistry.Instance.DataExportHighWaterMark);
			serviceTask.Buffer.Clear();
			STIDataRegistry.Instance.DisableOrganisationExport = false;
			STIDataRegistry.Instance.DataExportHighWaterMark = originalHWM;
			STIDataRegistry.Instance.OrganisationExportDirectory = Env.TempPath;
			STIDataRegistry.Instance.ShipmentExportDirectory = ZString.Empty;
			directoryErrorExpected = "Invalid Export Directories: ShipmentExportDirectory have not been specified in System -> Registry -> Strang International Client Extensions -> Navision";
			RunTaskSchedule(serviceTask);
			AssertContains("There should be an error with ShipmentExportDirectory", directoryErrorExpected, serviceTask.Buffer.AsString);
			AssertEquals("HighWaterMark should NOT be updated", originalHWM, STIDataRegistry.Instance.DataExportHighWaterMark);
			serviceTask.Buffer.Clear();
			STIDataRegistry.Instance.DisableShipmentExport = true;
			RunTaskSchedule(serviceTask);
			AssertNotContains("There should NOT be an error with ShipmentExportDirectory", directoryErrorExpected, serviceTask.Buffer.AsString);
			AssertEquals("HighWaterMark should be updated", ZDateTime.UtcNow, STIDataRegistry.Instance.DataExportHighWaterMark);
			serviceTask.Buffer.Clear();
			STIDataRegistry.Instance.DisableShipmentExport = false;
			STIDataRegistry.Instance.DataExportHighWaterMark = originalHWM;
			STIDataRegistry.Instance.ShipmentExportDirectory = Env.TempPath;
			STIDataRegistry.Instance.InvoiceHeaderExportDirectory = ZString.Empty;
			directoryErrorExpected = "Invalid Export Directories: InvoiceHeaderExportDirectory have not been specified in System -> Registry -> Strang International Client Extensions -> Navision";
			RunTaskSchedule(serviceTask);
			AssertContains("There should be an error with InvoiceHeaderExportDirectory", directoryErrorExpected, serviceTask.Buffer.AsString);
			AssertEquals("HighWaterMark should NOT be updated", originalHWM, STIDataRegistry.Instance.DataExportHighWaterMark);
			serviceTask.Buffer.Clear();
			STIDataRegistry.Instance.DisableInvoiceExport = true;
			RunTaskSchedule(serviceTask);
			AssertNotContains("There should NOT be an error with InvoiceHeaderExportDirectory", directoryErrorExpected, serviceTask.Buffer.AsString);
			AssertEquals("HighWaterMark should be updated", ZDateTime.UtcNow, STIDataRegistry.Instance.DataExportHighWaterMark);
			serviceTask.Buffer.Clear();
			STIDataRegistry.Instance.DisableInvoiceExport = false;
			STIDataRegistry.Instance.DataExportHighWaterMark = originalHWM;
			STIDataRegistry.Instance.InvoiceHeaderExportDirectory = Env.TempPath;
			STIDataRegistry.Instance.InvoiceLinesExportDirectory = ZString.Empty;
			directoryErrorExpected = "Invalid Export Directories: InvoiceLinesExportDirectory have not been specified in System -> Registry -> Strang International Client Extensions -> Navision";
			RunTaskSchedule(serviceTask);
			AssertContains("There should be an error with Export Directories", directoryErrorExpected, serviceTask.Buffer.AsString);
			AssertEquals("HighWaterMark should NOT be updated", originalHWM, STIDataRegistry.Instance.DataExportHighWaterMark);
			serviceTask.Buffer.Clear();
			STIDataRegistry.Instance.DisableInvoiceExport = true;
			RunTaskSchedule(serviceTask);
			AssertNotContains("There should NOT be an error with InvoiceLinesExportDirectory", directoryErrorExpected, serviceTask.Buffer.AsString);
			AssertEquals("HighWaterMark should be updated", ZDateTime.UtcNow, STIDataRegistry.Instance.DataExportHighWaterMark);
			serviceTask.Buffer.Clear();
			STIDataRegistry.Instance.DisableInvoiceExport = false;
			STIDataRegistry.Instance.DataExportHighWaterMark = originalHWM;
			STIDataRegistry.Instance.InvoiceLinesExportDirectory = Env.TempPath;
			RunTaskSchedule(serviceTask);
			AssertNotContains("There should NOT be an error with Export Directories", directoryErrorExpected, serviceTask.Buffer.AsString);
			AssertEquals("HighWaterMark should be updated", ZDateTime.UtcNow, STIDataRegistry.Instance.DataExportHighWaterMark);
		}

		[TestDate(2016, 8, 15, 15, 20, 31)]
		public void TestAllOrganisationsAreExportedTheFirstTime()
		{
			SetUpRegistry(true);
			string fileName = ZDateTime.UtcNow.ToLocalBranchTime().ToString(Constants.DateTimeFileNameFormat);
			string orgFile = Path.Combine(Env.TempPath, "ORG" + fileName + ".csv");
			RunTaskSchedule(serviceTask);
			Assert("File " + orgFile + " should exists", File.Exists(orgFile));
			Assert("ExportingOrganisationsForTheFirstTime should now be false", !STIDataRegistry.Instance.ExportingOrganisationsForTheFirstTime);
			string message = "This is the First time an export of Organisations has occured, therefore All Organisations will be exported.";
			message += " This may Take some time.";
			AssertContains("Message when exporting all orgs", message, serviceTask.Buffer.AsString);
			AssertContains("End of export Message", "Exporting All Organisations Complete", serviceTask.Buffer.AsString);
			using (StreamReader reader = new StreamReader(orgFile))
			{
				int numberOfLinesInTheOrgFile = 0;
				while (reader.ReadLine() != null)
				{
					numberOfLinesInTheOrgFile++;
				}

				AssertEquals("Number of lines in the file should be 1", 1, numberOfLinesInTheOrgFile);
			}

			DeleteIfExists(orgFile);
			serviceTask.Buffer.Clear();
			STIDataRegistry.Instance.DataExportHighWaterMark = ZDateTime.UtcNow.AddHours(-1);
			RunTaskSchedule(serviceTask);
			Assert("This is not the First time Orgs have been exported, hence no Orgs should be exported", !File.Exists(orgFile));
			AssertNotContains("There should not be messages about exporting all orgs", message, serviceTask.Buffer.AsString);
		}

		[TestDate(2016, 8, 15, 15, 20, 31)]
		public void TestAllOrganisationsAreExportedTheFirstTime_DisableExportOrg()
		{
			SetUpRegistry(true);
			STIDataRegistry.Instance.DisableOrganisationExport = true;
			string fileName = ZDateTime.UtcNow.ToLocalBranchTime().ToString(Constants.DateTimeFileNameFormat);
			string orgFile = Path.Combine(Env.TempPath, "ORG" + fileName + ".csv");
			RunTaskSchedule(serviceTask);
			Assert("NO orgs should be exported", !File.Exists(orgFile));
			string message = "This is the First time an export of Organisations has occured, therefore All Organisations will be exported.";
			message += " This may Take some time.";
			AssertNotContains("There should not be messages about exporting all orgs", message, serviceTask.Buffer.AsString);
		}

		[TestDate(2016, 8, 15, 15, 20, 31)]
		public void TestExportOrganisationsAndJobs()
		{
			CreateStmALogs();
			SetUpRegistry();
			string fileName = ZDateTime.UtcNow.ToLocalBranchTime().ToString(Constants.DateTimeFileNameFormat);
			string orgFile = Path.Combine(Env.TempPath, "ORG" + fileName + ".csv");
			string jobFile = Path.Combine(Env.TempPath, "JOB" + fileName + ".csv");
			RunTaskSchedule(serviceTask);
			Assert("File " + orgFile + " should exists", File.Exists(orgFile));
			Assert("File " + jobFile + " should exists", File.Exists(jobFile));
			AssertEquals("HighWaterMark should be updated", ZDateTime.UtcNow, STIDataRegistry.Instance.DataExportHighWaterMark);
		}

		[TestDate(2016, 8, 15, 15, 20, 31)]
		public void TestExportOrganisationsAndJobs_RunToNow()
		{
			var consol = CreateStmALogs();
			SetUpRegistry();
			STIDataRegistry.Instance.DataExportHighWaterMark = new ZDateTime(2016, 8, 15, 12, 50, 56);
			string fileName = ZDateTime.UtcNow.ToLocalBranchTime().ToString(Constants.DateTimeFileNameFormat);
			string orgFile = Path.Combine(Env.TempPath, "ORG" + fileName + ".csv");
			string jobFile = Path.Combine(Env.TempPath, "JOB" + fileName + ".csv");
			Assert("PRE: HWM plus BatchInterval is before the last edit time of the job", consol.JK_SystemLastEditTimeUtc > STIDataRegistry.Instance.DataExportHighWaterMark.AddHours(serviceTask.BatchIntervalInHours));
			RunTaskSchedule(serviceTask);
			Assert("File " + orgFile + " should exists", File.Exists(orgFile));
			Assert("File " + jobFile + " should exists", File.Exists(jobFile));
			AssertEquals("HighWaterMark should be updated to now", ZDateTime.UtcNow, STIDataRegistry.Instance.DataExportHighWaterMark);
			AssertContains("Batch interval time should be 2 hours", "Batch End Time: 145056", serviceTask.Buffer.AsString);
			AssertContains("Last batch end time should be the current time", "Batch End Time: 152031", serviceTask.Buffer.AsString);
		}

		[TestDate(2016, 8, 15, 15, 20, 31)]
		public void TestExportOrganisationsAndJobs_DisableExportOrg()
		{
			CreateStmALogs();
			SetUpRegistry();
			STIDataRegistry.Instance.DisableOrganisationExport = true;
			string fileName = ZDateTime.UtcNow.ToLocalBranchTime().ToString(Constants.DateTimeFileNameFormat);
			string orgFile = Path.Combine(Env.TempPath, "ORG" + fileName + ".csv");
			string jobFile = Path.Combine(Env.TempPath, "JOB" + fileName + ".csv");
			RunTaskSchedule(serviceTask);
			Assert("File " + orgFile + " should NOT exists", !File.Exists(orgFile));
			Assert("File " + jobFile + " should exists", File.Exists(jobFile));
			AssertEquals("HighWaterMark should be updated", ZDateTime.UtcNow, STIDataRegistry.Instance.DataExportHighWaterMark);
		}

		[TestDate(2016, 8, 15, 15, 20, 31)]
		public void TestExportOrganisationsAndJobs_DisableExportJob()
		{
			CreateStmALogs();
			SetUpRegistry();
			STIDataRegistry.Instance.DisableShipmentExport = true;
			string fileName = ZDateTime.UtcNow.ToLocalBranchTime().ToString(Constants.DateTimeFileNameFormat);
			string orgFile = Path.Combine(Env.TempPath, "ORG" + fileName + ".csv");
			string jobFile = Path.Combine(Env.TempPath, "JOB" + fileName + ".csv");
			RunTaskSchedule(serviceTask);
			Assert("File " + orgFile + " should exists", File.Exists(orgFile));
			Assert("File " + jobFile + " should NOT exists", !File.Exists(jobFile));
			AssertEquals("HighWaterMark should be updated", ZDateTime.UtcNow, STIDataRegistry.Instance.DataExportHighWaterMark);
		}

		[TestDate(2006, 2, 24, 12, 21, 19)]
		public void TestExportAccountingDebtorTransactions()
		{
			SetUpRegistry();
			string fileName = ZDateTime.UtcNow.ToLocalBranchTime().ToString(Constants.DateTimeFileNameFormat);
			string headerFile = Path.Combine(Env.TempPath, "HDR" + fileName + ".csv");
			string linesFile = Path.Combine(Env.TempPath, "LIN" + fileName + ".csv");
			serviceTask.ExportAccountingDebtorTransactions();
			Assert("File " + headerFile + " should NOT exist, no transactions in batch", !File.Exists(headerFile));
			Assert("File " + linesFile + " should NOT exist, not transactions in batch", !File.Exists(linesFile));
			AssertNotContains("Batch should NOT be exported", "Batch 1 of the Accounting Transactions was exported", serviceTask.Buffer.AsString);
			TransactionExportTestDataHelper helper = new TransactionExportTestDataHelper(Factory);
			Factory.Save();
			serviceTask.ExportAccountingDebtorTransactions();
			Assert("File " + headerFile + " should exist", File.Exists(headerFile));
			Assert("File " + linesFile + " should exist", File.Exists(linesFile));
			AssertContains("Batch should be exported", "Batch 1 of the Accounting Transactions was exported", serviceTask.Buffer.AsString);
		}

		[TestDate(2006, 2, 24, 12, 21, 19)]
		public void TestExportAccountingDebtorTransactions_DisableExportInvoice()
		{
			SetUpRegistry();
			STIDataRegistry.Instance.DisableInvoiceExport = true;
			string fileName = ZDateTime.UtcNow.ToLocalBranchTime().ToString(Constants.DateTimeFileNameFormat);
			string headerFile = Path.Combine(Env.TempPath, "HDR" + fileName + ".csv");
			string linesFile = Path.Combine(Env.TempPath, "LIN" + fileName + ".csv");
			TransactionExportTestDataHelper helper = new TransactionExportTestDataHelper(Factory);
			Factory.Save();
			RunTaskSchedule(serviceTask);
			Assert("File " + headerFile + " should NOT exist, no transactions in batch", !File.Exists(headerFile));
			Assert("File " + linesFile + " should NOT exist, not transactions in batch", !File.Exists(linesFile));
			AssertNotContains("Batch should NOT be exported", "Batch 1 of the Accounting Transactions was exported", serviceTask.Buffer.AsString);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
		#region Implementation
		NavisionServiceTaskTestClass serviceTask;
		protected override void SetUpCore()
		{
			base.SetUpCore();
			serviceTask = new NavisionServiceTaskTestClass();
			InitialiseTaskSchedule(serviceTask, out _, out var scheduleGovernor);
			scheduleGovernor.SetBranchPk(Env.CurrentBranchPK);
		}

		protected override void TearDownCore()
		{
			base.TearDownCore();
			if (!UseSnapshotProtectionAttribute.IsProtected)
			{
				TempDirectory.DeleteDirectory(Env.TempPath);
			}
		}

		void SetUpRegistry(bool exportingOrganisationsForTheFirstTime = false)
		{
			STIDataRegistry.Instance.DataExportHighWaterMark = ZDateTime.UtcNow.AddHours(-1);
			STIDataRegistry.Instance.OrganisationExportDirectory = Env.TempPath;
			STIDataRegistry.Instance.ShipmentExportDirectory = Env.TempPath;
			STIDataRegistry.Instance.InvoiceHeaderExportDirectory = Env.TempPath;
			STIDataRegistry.Instance.InvoiceLinesExportDirectory = Env.TempPath;
			STIDataRegistry.Instance.ExportingOrganisationsForTheFirstTime = exportingOrganisationsForTheFirstTime;
		}

		CommonConsol CreateStmALogs()
		{
			CommonConsol consol = NavisionTestHelper.ConsolForTesting(Factory);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			consol.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			OrgHeader org = NavisionTestHelper.OrgForTesting(Factory);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			org.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Factory.Save();
			return consol;
		}

		public class NavisionServiceTaskTestClass : NavisionServiceTask
		{
			protected internal override OrgHeaderCollection OrganisationCollectionForInitialExport
			{
				get
				{
					OrgHeader org = FactoryProvider.Current.LoadTop1<OrgHeader>(new ZQuery());
					OrgHeaderCollection organisations = new OrgHeaderCollection(FactoryProvider.Current);
					organisations.Add(org);
					return organisations;
				}
			}

			protected override void ExportByBatch(ZDateTime batchEndDateTime)
			{
				base.ExportByBatch(batchEndDateTime);
				Buffer.Notify(new InfoNotification("Batch End Time: " + batchEndDateTime.ToString("HHmmss")));
			}
		}
		#endregion
	}
}
