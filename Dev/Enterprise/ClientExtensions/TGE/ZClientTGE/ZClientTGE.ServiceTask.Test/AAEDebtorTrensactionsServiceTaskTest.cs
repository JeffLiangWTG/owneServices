using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.TGE.PMS;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.TGE.ServiceTask.Testing
{
	[TestedType(typeof(PMSImportServiceTask))]
	class AAEDebtorTrensactionsServiceTaskTest : ServiceTaskTestCase<PMSImportServiceTask>
	{
		public void TestIsEnvironmentDataValid()
		{
			RunTaskSchedule(ServiceTask);
			Assert(ServiceTask.GetBuffer().AsString.Contains("================== Task started =================="));
			Assert(ServiceTask.GetBuffer().AsString.Contains("================== Task ended =================="));
			Assert(ServiceTask.GetBuffer().AsString.Contains("The PMS Import File Directory has not been set up or not exists."));
			Assert(ServiceTask.GetBuffer().AsString.Contains("The PMS Archive Directory has not been set up or not exists."));
			Assert(ServiceTask.GetBuffer().AsString.Contains("The Registry Allow Departure Depot Address Import has to be set to true for data import."));
			Assert(ServiceTask.GetBuffer().AsString.Contains("The PMS Notification Group has not been set up or invalid."));
			Assert(ServiceTask.GetBuffer().AsString.Contains("The PMS Organisation For Code Mapping has not been set up."));
			Assert(ServiceTask.GetBuffer().AsString.Contains("The Registry Update Consol Shipments During Automatic Import has to be set to false for data import."));
			Assert(ServiceTask.GetBuffer().AsString.Contains("The Registry Update Consol During Automatic Import has to be set to false for data import."));
			SetValidRegistry();
			ServiceTask.GetBuffer().Clear();
			RunTaskSchedule(ServiceTask);
			Assert(!ServiceTask.GetBuffer().AsString.Contains("The PMS Import File Directory has not been set up or not exists."));
			Assert(!ServiceTask.GetBuffer().AsString.Contains("The PMS Archive Directory has not been set up or not exists."));
			Assert(!ServiceTask.GetBuffer().AsString.Contains("The Registry Allow Departure Depot Address Import has to be set to true for data import."));
			Assert(!ServiceTask.GetBuffer().AsString.Contains("The PMS Notification Group has not been set up or invalid."));
			Assert(!ServiceTask.GetBuffer().AsString.Contains("The PMS Organisation For Code Mapping has not been set up."));
			Assert(!ServiceTask.GetBuffer().AsString.Contains("The Registry Update Consol Shipments During Automatic Import has to be set to false for data import."));
			Assert(!ServiceTask.GetBuffer().AsString.Contains("The Registry Update Consol During Automatic Import has to be set to false for data import."));
		}

		public void TestRunTask()
		{
			SetValidRegistry();
			CopyTestFileToDirectory(TestFile);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			RunTaskSchedule(ServiceTask);
			Assert(ServiceTask.GetBuffer().AsString.Contains("================== Task started =================="));
			Assert(ServiceTask.GetBuffer().AsString.Contains("================== Task ended =================="));
			Assert(ServiceTask.GetBuffer().AsString.Contains(string.Concat("Processing 1 file(s) from ", TGEDataRegistry.Instance.PMSFileImportDirectory)));
			AssertEquals("Should have 1 more Consols", 1, Factory.GetDatabaseCount(typeof(CommonConsol)));
			AssertEquals("Should have 1 more Shipments", 1, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
			AssertEquals("Number of files Imported is 1", 0, Directory.GetFiles(ImportDerictory).Length);
			AssertEquals("Number of files Imported is 1", 1, Directory.GetFiles(ArchiveDerictory).Length);
			AssertEquals("No Email Should be sent because there should not be any errors", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			Assert(ServiceTask.GetBuffer().AsString.Contains("Imported file expconblah.tsv"));
		}

		public void TestRunTaskWithNoFiles()
		{
			SetValidRegistry();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			RunTaskSchedule(ServiceTask);
			Assert(ServiceTask.GetBuffer().AsString.Contains("================== Task started =================="));
			Assert(ServiceTask.GetBuffer().AsString.Contains("================== Task ended =================="));
			Assert(ServiceTask.GetBuffer().AsString.Contains(string.Concat("Processing 0 file(s) from ", TGEDataRegistry.Instance.PMSFileImportDirectory)));
		}

		public void TestRunTaskSendingEmailWhenWarning()
		{
			SetValidRegistry();
			CopyTestFileToDirectory(TestFileWithWarning);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			RunTaskSchedule(ServiceTask);
			AssertEquals("Should be 1 more Consols", 1, Factory.GetDatabaseCount(typeof(CommonConsol)));
			AssertEquals("Number Of files failed to Import", 0, Directory.GetFiles(ImportDerictory).Length);
			AssertEquals("Number of files Imported is 1", 1, Directory.GetFiles(ArchiveDerictory).Length);
			Assert(ServiceTask.GetBuffer().AsString.Contains("Warning with File: expconblah.tsv. Please see mail error report for more Details."));
			AssertEquals("1 Email should be sent because there was an error.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertNotNull("Email should not be null", email);
			AssertEquals("Email Subject", "Error Processing Flat Text File(s)", email.Subject);
			AssertEquals(1, email.Attachments.Count);
		}

		public void TestRunTaskWithException()
		{
			SetValidRegistry();
			CopyTestFileToDirectory(TestFileWithException);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			ServiceTask.TGEImporterDelegate = new PMSImportServiceTask.GetImporterDelegate(() =>
			{
				return new TGEDataImporterWithException();
			});
			RunTaskSchedule(ServiceTask);
			Assert(ServiceTask.GetBuffer().AsString.Contains(" Failure processing Files: "));
			AssertEquals("1 Email should be sent because there was an error.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertNotNull("Email should not be null", email);
			AssertEquals("Email Subject", "Error Processing Flat Text File(s)", email.Subject);
			AssertEquals(1, email.Attachments.Count);
		}

		// No nudging: no queue table; service task polls external data source.
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
		EmbeddedResourceRetriever resourceRetriever;
		public void TestHasNoQueueProvider()
		{
			var queueProvider = GetHostedServiceQueueProviderInstanceByServiceTaskCode(PMSImportServiceTask.Code);
			AssertNull("No queue table for this service task; polls external data source.", queueProvider);
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("30seconds", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		#region Set Up
		void CopyTestFileToDirectory(string fileName)
		{
			string destinationFile = Path.Combine(ImportDerictory, "expconblah.tsv");
			File.WriteAllBytes(destinationFile, File.ReadAllBytes(fileName));
		}

		void SetValidRegistry()
		{
			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			GlbStaff staff = group.Staff.AddNew();
			staff.GS_Code = "P.T";
			staff.GS_EmailAddress = "blah@blah.com";
			Factory.Save();
			TGEDataRegistry.Instance.PMSFileImportDirectory = ImportDerictory;
			TGEDataRegistry.Instance.PMSFileArchiveDirectory = ArchiveDerictory;
			SystemDataRegistry.Instance.AllowDepartureDepotAddressImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			TGEDataRegistry.Instance.PMSNotificationGroup = group.PK.ToGuid();
			TGEDataRegistry.Instance.CodeMapPMSOrganisation = MappingOrg.PK;
			SystemDataRegistry.Instance.UpdateConsolShipmentsDuringAutomaticImportOther.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			SystemDataRegistry.Instance.UpdateConsolDuringAutomaticImportOther.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		class TGEDataImporterWithException : TGEDataImporter
		{
			protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, CargoWise.ComponentModel.INotifications notifications, out CargoWise.Integration.ITransactionParticipant[] additionalTransactionActions)
			{
				throw new ArgumentException("An ArgumentException occurred while importing data to factory core.");
			}
		}

		PMSImportServiceTask ServiceTask;
		OrgHeader MappingOrg;
		protected override void SetUpCore()
		{
			base.SetUpCore();
			ServiceTask = new PMSImportServiceTask();
			InitialiseTaskSchedule(ServiceTask);
			MappingOrg = Factory.Load<OrgHeader>(Env.CurrentCompany.OrganisationPK);
			OrgPatternMatchOverride orgMatch1 = MappingOrg.CreatePatternMatchOverrideForTest();
			orgMatch1.OO_ForeignCode = "1212121";
			orgMatch1.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			orgMatch1.OO_LocalGuid = MappingOrg.PK;
			OrgPatternMatchOverride orgMatch2 = MappingOrg.CreatePatternMatchOverrideForTest();
			orgMatch2.OO_ForeignCode = "1313131";
			orgMatch2.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			orgMatch2.OO_LocalGuid = MappingOrg.PK;
			Factory.Save();
			Directory.CreateDirectory(ImportDerictory);
			Directory.CreateDirectory(ArchiveDerictory);
			resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			TestFile = resourceRetriever.SaveResourceToFile("expconWithValidTestData.tsv");
			TestFileWithWarning = resourceRetriever.SaveResourceToFile("expconTestForMAWBHAWBExistWarning.tsv");
			TestFileWithException = resourceRetriever.SaveResourceToFile("expconTestDataWithException.tsv");
		}

		protected override void TearDownCore()
		{
			base.TearDownCore();
			TempDirectory.DeleteDirectory(ImportDerictory);
			TempDirectory.DeleteDirectory(ArchiveDerictory);
			resourceRetriever.Dispose();
		}

		readonly ZString ImportDerictory = Env.TempPath + @"TGE\";
		readonly ZString ArchiveDerictory = Env.TempPath + @"TGE_Archive\";
		ZString TestFile;
		ZString TestFileWithWarning;
		ZString TestFileWithException;
		#endregion
	}
}
