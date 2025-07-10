using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DataTransfer.IO;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.KNA.ServiceTasks.Testing
{
	[TestedType(typeof(ImportXmlServiceTask))]
	public class ImportXmlServiceTaskTest : ServiceTaskTestCase<ImportXmlServiceTask>
	{
		public void TestDirectoryDoesNotExist()
		{
			TempDirectory.DeleteDirectory(DirectoryPath);
			RunTaskSchedule(serviceTask);
			AssertDirectoryInvalid();
			serviceTask.Buffer.Clear();
			DirectoryPath = ZString.Empty;
			RunTaskSchedule(serviceTask);
			AssertDirectoryInvalid();
		}

		public void TestProcessedDirectoryDoesNotExist()
		{
			TempDirectory.DeleteDirectory(ProcessedPath);
			RunTaskSchedule(serviceTask);
			AssertDirectoryInvalid();
			serviceTask.Buffer.Clear();
			ProcessedPath = ZString.Empty;
			RunTaskSchedule(serviceTask);
			AssertDirectoryInvalid();
		}

		public void TestNoFilesInDirectory()
		{
			RunTaskSchedule(serviceTask);
			AssertEquals("Should be NO new consol in Database", NumberOfExistingConsols, Factory.GetDatabaseCount(typeof(CommonConsol)));
			AssertEquals("Should be NO new shipment in Database", NumberOfExistingShipments, Factory.GetDatabaseCount(typeof(CommonShipment)));
			AssertEquals("No Air Cargo Message should have be created", NumberOfExistingMAWB, Factory.GetDatabaseCount(typeof(CusMAWB)));
			AssertEquals("No Air Cargo Message should have be created", NumberOfExistingHAWB, Factory.GetDatabaseCount(typeof(CusHAWB)));
			Assert("should be no errors", !serviceTask.Buffer.HasErrors);
		}

		[TestDate(2006, 5, 1)]
		public void TestImportConsolAndShipmentAndSendAirCargoMessageWithUnmatched()
		{
			string threshold = Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.OrgMatchThreshold.Value;
			SystemDefinedOrganisation previousUnmatched = Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value;
			try
			{
				UnmatchedOrganisation unmatchedOrg = new UnmatchedOrganisation();
				unmatchedOrg.IsEnabled = true;
				OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, unmatchedOrg);
				OrganisationsDataRegistry.Instance.OrgMatchThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Registry.Business.OrgMatchThresholds.Codes.High);
				CopyFileAndSetAttributes(AirImportUnmatched, "AirImportUnmatched.xml");
				RunTaskSchedule(serviceTask);
				AssertEquals("Should be one new consol in Database", NumberOfExistingConsols + 1, Factory.GetDatabaseCount(typeof(CommonConsol)));
				AssertEquals("Should be one new shipment in Database", NumberOfExistingShipments + 1, Factory.GetDatabaseCount(typeof(CommonShipment)));
				AssertEquals("Should be one new CusMawb in Database", NumberOfExistingMAWB + 1, Factory.GetDatabaseCount(typeof(CusMAWB)));
				AssertEquals("Should be one new CusHawb in Database", NumberOfExistingHAWB + 1, Factory.GetDatabaseCount(typeof(CusHAWB)));
				Assert("should be no errors. Notifications are: \r\n" + serviceTask.Buffer.AsString, !serviceTask.Buffer.HasErrors);
				string savingNotification = "Data from AirImportUnmatched.xml has been imported.";
				Assert("Notification should contain " + savingNotification, serviceTask.Buffer.AsString.Contains(savingNotification));
				CusHAWB hAWB = Factory.LoadTop1<CusHAWB>(new ZQuery(Enterprise.ZArchitecture.Schema.CusHAWBSchema.CS_HAWB, "VIS65800764"));
				string expectedMessageText = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:99B:UN'
BGM+933:::AIRCR+S00001000/DAT1:1+9'
RFF+PQ:CC'
RFF+HWB:VIS65800764'
RFF+MWB:08165842663'
NAD+CN++INLINE SYSTEMS PTY. LTD.::8 PROSPERITY PARADE WARRIEWOOD, NSW: 2102 WARRIEWOOD, NSW 2102'
NAD+CZ++SCHOFLLY PIBEROPTIK GMBH::ROBERT-BOSCH-STR. 1-3 79211 DENZLIN:GEN DENZLINGEN 79211 AU'
NAD+VW+21003980130123::95'
TDT+20+ 006++6+QF::3'
LOC+8+AUSYD::6'
LOC+76+DEFRA::6'
LOC+12+AUSYD::6'
LOC+91+DEFRA::6'
DTM+178:20060203:102'
CNI+1'
RFF+UCN:S00001000'
MOA+96:NDV'
GID+1'
PAC+1'
FTX+AAA+++CAMERA AND LIGHT SOURE'
MEA+AAE+G+KG:28.00'
UNT+22+<<MSGNO PLACEHOLDER>>'";
				AssertMultilineASCIIEquals("Message not formatted correctly", expectedMessageText, hAWB.Messages[0].EM_MessageText.Replace("'", "'\r\n"));
				AssertFileHasBeenMoved("AirImportUnmatched.xml");
			}
			finally
			{
				OrganisationsDataRegistry.Instance.OrgMatchThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, threshold);
				OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, previousUnmatched);
			}
		}

		[TestDate(2006, 5, 1)]
		public void TestImportConsolAndShipmentAndSendAirCargoMessage()
		{
			CopyFileAndSetAttributes(AirImport, "AirImport.xml");
			RunTaskSchedule(serviceTask);
			AssertEquals("Should be one new consol in Database", NumberOfExistingConsols + 1, Factory.GetDatabaseCount(typeof(CommonConsol)));
			AssertEquals("Should be one new shipment in Database", NumberOfExistingShipments + 2, Factory.GetDatabaseCount(typeof(CommonShipment)));
			AssertEquals("Should be one new CusMawb in Database", NumberOfExistingMAWB + 1, Factory.GetDatabaseCount(typeof(CusMAWB)));
			AssertEquals("Should be one new CusHawb in Database", NumberOfExistingHAWB + 2, Factory.GetDatabaseCount(typeof(CusHAWB)));
			string savingNotification = "Data from AirImport.xml has been imported.";
			Assert("Notification should contain " + savingNotification, serviceTask.Buffer.AsString.Contains(savingNotification));
			AssertFileHasBeenMoved("AirImport.xml");
		}

		[ExpectNoExceptions()]
		public void TestImportConsolAndShipmentOverwritesExistingFile()
		{
			CopyFileAndSetAttributes(AirImport, "AirImport.xml");
			string pathToExistingFile = Path.Combine(ProcessedPath, "AirImport.xml");
			DateTime time1 = new FileInfo(pathToExistingFile).CreationTime;
			using (StreamWriter writer = new StreamWriter(pathToExistingFile))
			{
				writer.Write("Text");
			}

			long initialLength1 = new FileInfo(pathToExistingFile).Length;
			try
			{
				RunTaskSchedule(serviceTask);
				long initialLength2 = new FileInfo(pathToExistingFile).Length;
				AssertEquals(true, initialLength1 < initialLength2);
			}
			finally
			{
				DeleteIfExists(pathToExistingFile);
			}
		}

		public void TestImportConsolAndShipment_DoesNotSendAirCargo()
		{
			CopyFileAndSetAttributes(AirExport, "AirExport.xml");
			CopyFileAndSetAttributes(SeaImport, "SeaImport.xml");
			CopyFileAndSetAttributes(SeaExport, "SeaExport.xml");
			RunTaskSchedule(serviceTask);
			AssertEquals("Should be 3 new consols in Database", NumberOfExistingConsols + 3, Factory.GetDatabaseCount(typeof(CommonConsol)));
			AssertEquals("Should be 6 new shipments in Database", NumberOfExistingShipments + 6, Factory.GetDatabaseCount(typeof(CommonShipment)));
			AssertEquals("No Air Cargo Message should have be sent", NumberOfExistingMAWB, Factory.GetDatabaseCount(typeof(CusMAWB)));
			AssertEquals("No Air Cargo Message should have be sent", NumberOfExistingHAWB, Factory.GetDatabaseCount(typeof(CusHAWB)));
			string savingNotification = "Data from AirExport.xml has been imported.";
			Assert("Notification should contain " + savingNotification, serviceTask.Buffer.AsString.Contains(savingNotification));
			savingNotification = "Data from SeaImport.xml has been imported.";
			Assert("Notification should contain " + savingNotification, serviceTask.Buffer.AsString.Contains(savingNotification));
			savingNotification = "Data from SeaExport.xml has been imported.";
			Assert("Notification should contain " + savingNotification, serviceTask.Buffer.AsString.Contains(savingNotification));
			Assert("should be no errors", !serviceTask.Buffer.HasErrors);
			AssertFileHasBeenMoved("AirExport.xml");
			AssertFileHasBeenMoved("SeaImport.xml");
			AssertFileHasBeenMoved("SeaExport.xml");
		}

		[ExpectNoExceptions("Exceptions should be caught by the processor")]
		public void TestExceptionOccursButContinuesToExportFiles()
		{
			CopyFileAndSetAttributes(SeaImport, "SeaImport.xml");
			CopyFileAndSetAttributes(SeaExport, "SeaExport.xml");
			ImportXmlBatchProcessorThrowsException mockServiceTask = new ImportXmlBatchProcessorThrowsException();
			InitialiseTaskSchedule(mockServiceTask);
			RunTaskSchedule(mockServiceTask);
			Assert("Notification should have errors", mockServiceTask.Buffer.HasErrors);
			string errorNotification = string.Format("Occured in file: SeaExport.xml \r\n SeaExport.xml was imported 1st. \r\n {0}", mockServiceTask.MockException.StackTrace);
			Assert("Notification should contain " + errorNotification, mockServiceTask.Buffer.AsString.Contains(errorNotification));
			AssertEquals("Number of files attempted to imported should be 2", 2, mockServiceTask.NumberOfFilesImported);
			AssertEquals("Should be 1 new consol in the database", NumberOfExistingConsols + 1, Factory.GetDatabaseCount(typeof(CommonConsol)));
			AssertFileHasBeenMoved("SeaImport.xml");
			AssertFileHasBeenMoved("SeaExport.xml");
		}

		public void TestFilesThatAreInUseAreNotAccessed()
		{
			FileInformation fileAlreadyInUse = new FileInformation(Path.Combine(DirectoryPath, "SeaExport.xml"));
			FileInformation fileNotInUse = new FileInformation(Path.Combine(DirectoryPath, "SeaImport.xml"));
			CopyFileAndSetAttributes(SeaImport, fileNotInUse.Name);
			CopyFileAndSetAttributes(SeaExport, fileAlreadyInUse.Name);
			Stream stream = fileAlreadyInUse.Open();
			try
			{
				RunTaskSchedule(serviceTask);
				AssertEquals("Should be 1 consol in Database, only the SeaImport File should be imported", NumberOfExistingConsols + 1, Factory.GetDatabaseCount(typeof(CommonConsol)));
				Assert("SeaImport.xml file should be imported", serviceTask.Buffer.AsString.Contains("Data from SeaImport.xml has been imported"));
				Assert("SeaExport.xml file should NOT be imported", !serviceTask.Buffer.AsString.Contains("Data from SeaExport.xml has been imported"));
				AssertFileHasBeenMoved("SeaImport.xml");
			}
			finally
			{
				stream.Close();
				string[] files = Directory.GetFiles(DirectoryPath);
				AssertEquals("should still be 1 file in the DirectoryPath", 1, files.Length);
				AssertEquals("The that was not moved should be the SeaExport.xml file", fileAlreadyInUse.FullName, files[0]);
			}
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("1minute", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
#region Implementation
		void AssertDirectoryInvalid()
		{
			AssertEquals("Notification has errors", true, serviceTask.Buffer.HasErrors);
			AssertEquals("Notification has one error event", 1, serviceTask.Buffer.Events.Length);
			Assert("Error Type should be " + ErrorType.MissingDataDirectory, serviceTask.Buffer.ContainsNotificationType(ErrorType.MissingDataDirectory));
			AssertEquals("Notification Message", "Error: Could not find directory (Please specify an existing Import Directory and Processed Directory in Config -> System -> Registry -> Kuehne & Nagel Australia Client Extensions)", serviceTask.Buffer.Events[0].Message);
		}

		void AssertFileHasBeenMoved(string fileName)
		{
			Assert("File should have been moved from the Original Directory", !File.Exists(Path.Combine(DirectoryPath, fileName)));
			Assert("File should have been moved to the processed Directory", File.Exists(Path.Combine(ProcessedPath, fileName)));
		}

		void CopyFileAndSetAttributes(ZString sourceFile, ZString destinationFileName)
		{
			ZString destination = Path.Combine(DirectoryPath, destinationFileName);
			File.Copy(sourceFile, destination, true);
			File.SetAttributes(destination, FileAttributes.Normal);
		}

		string DirectoryPath;
		string ProcessedPath;
		protected string AirImport
		{
			get
			{
				return resourceRetriever.SaveResourceToFile("Test.AirImport.xml");
			}
		}
		protected string AirImportUnmatched
		{
			get
			{
				return resourceRetriever.SaveResourceToFile("Test.AirImportUnmatched.xml");
			}
		}
		protected string AirExport
		{
			get
			{
				return resourceRetriever.SaveResourceToFile("Test.AirExport.xml");
			}
		}
		protected string SeaImport
		{
			get
			{
				return resourceRetriever.SaveResourceToFile("Test.SeaImport.xml");
			}
		}
		protected string SeaExport
		{
			get
			{
				return resourceRetriever.SaveResourceToFile("Test.SeaExport.xml");
			}
		}
		int NumberOfExistingConsols;
		int NumberOfExistingShipments;
		int NumberOfExistingMAWB;
		int NumberOfExistingHAWB;
		ImportXmlServiceTask serviceTask;
		class ImportXmlBatchProcessorThrowsException : ImportXmlServiceTask
		{
			protected override void ImportXmlFile(FileInfo xmlFile, ForwardingConsolValueObjectDataAdapter adapter, MainFormConsolCollection collection, XmlValueObjectSerializer serializer)
			{
				NumberOfFilesImported++;
				if (xmlFile.Name == "SeaImport.xml")
				{
					base.ImportXmlFile(xmlFile, adapter, collection, serializer);
				}
				else
				{
					MockException = new Exception(xmlFile.Name + " was imported " + NumberOfFilesImported + "st.");
					throw MockException;
				}
			}

			public Exception MockException;
			public int NumberOfFilesImported;
		}

#endregion
#region Setup
		ZString currentRegNo;
		OrgHeader currentCompany;
		BusinessObjectFactory currentCompanyFactory;
		EmbeddedResourceRetriever resourceRetriever;
		protected override void SetUpCore()
		{
			base.SetUpCore();
			currentCompanyFactory = new BusinessObjectFactory();
			currentRegNo = GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number;
			currentCompany = currentCompanyFactory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
			currentCompany.PrimaryRegistrationNumber.Number = "21 003 980 130 123";
			currentCompanyFactory.Save();
			serviceTask = new ImportXmlServiceTask();
			InitialiseTaskSchedule(serviceTask, out _, out var scheduleGovernor);
			scheduleGovernor.SetBranchPk(Env.CurrentBranchPK);
			DirectoryPath = Path.Combine(Env.TempPath, "KNA");
			Directory.CreateDirectory(DirectoryPath);
			KNADataRegistry.Instance.DirectoryToImportXml = DirectoryPath;
			ProcessedPath = Path.Combine(Env.TempPath, "ProcessedKNA");
			Directory.CreateDirectory(ProcessedPath);
			KNADataRegistry.Instance.ProcessedDirectoryForXmlFiles = ProcessedPath;
			NumberOfExistingConsols = Factory.GetDatabaseCount(typeof(CommonConsol));
			NumberOfExistingShipments = Factory.GetDatabaseCount(typeof(CommonShipment));
			NumberOfExistingMAWB = Factory.GetDatabaseCount(typeof(CusMAWB));
			NumberOfExistingHAWB = Factory.GetDatabaseCount(typeof(CusHAWB));
			resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
		}

#endregion
#region teardown
		protected override void TearDownCore()
		{
			currentCompany.PrimaryRegistrationNumber.Number = currentRegNo;
			currentCompanyFactory.Save();
			if (!string.IsNullOrEmpty(DirectoryPath))
			{
				TempDirectory.DeleteDirectory(DirectoryPath);
			}

			if (!string.IsNullOrEmpty(ProcessedPath))
			{
				TempDirectory.DeleteDirectory(ProcessedPath);
			}

			resourceRetriever.Dispose();
			base.TearDownCore();
		}
#endregion
	}
}
