using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.TNT.Testing
{
	[TestedType(typeof(Exit2ImportManager))]
	sealed class Exit2ImportManagerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFileFullName()
		{
			string tempSampleX2Path = TestUtils.CopyResourceToFile(TNTTestUtils.SampleX2Name);
			Exit2ImportManager testManager = new Exit2ImportManager(Factory, tempSampleX2Path);
			AssertEquals("File full name", tempSampleX2Path, testManager.FileFullName);
		}

		public void TestExit2Mawbs()
		{
			string tempSampleX2Path = TestUtils.CopyResourceToFile(TNTTestUtils.SampleX2Name);
			Exit2ImportManager testManager = new Exit2ImportManager(Factory, tempSampleX2Path);
			AssertEquals("Exit2Mawbs should lazy instantiate and load automatically", 6, testManager.Exit2Mawbs.Count);
			AssertEquals("Exit2Mawbs factory should be same as manager's", testManager.Factory, testManager.Exit2Mawbs.Factory);
			QuantumMawbCollection collection = new QuantumMawbCollection(testManager.Factory, tempSampleX2Path);
			collection.LoadFromFile();
			AssertEquals("Collection from the same file should have the same info", collection[0].MasterBillNum, testManager.Exit2Mawbs[0].MasterBillNum);
		}

		public void TestIsAnyMawbLinkedToAnEnterpriseConsol()
		{
			string tempSampleX2Path = TestUtils.CopyResourceToFile(TNTTestUtils.SampleX2Name);
			Exit2ImportManager testManager = new Exit2ImportManager(Factory, tempSampleX2Path);
			Assert("No Mawbs linked to Enterprise Consols so far", !testManager.IsAnyMawbLinkedToAnEnterpriseConsol);
		}

		public void TestErrorText()
		{
			string tempSampleX2Path = TestUtils.CopyResourceToFile(TNTTestUtils.SampleX2Name);
			Exit2ImportManager testManager = new Exit2ImportManager(Factory, tempSampleX2Path);
			AssertEquals("ErrorTest is empty", "", testManager.ErrorText);
			testManager.Notify(new WarningNotification("Warning Test Message"));
			AssertEquals("ErrorTest is empty", "", testManager.ErrorText);
			testManager.Notify(new ErrorNotification(ErrorType.Error, "Error Test Message"));
			AssertEquals("ErrorTest should not be empty", "Error: Error Test Message" + System.Environment.NewLine, testManager.ErrorText);
		}

		public void TestGetExit2ProcessReportDoesNotDisplaySameInfoMultipleTimes()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "08135025185";
			consol.Transports.MostInterestingTransport.JW_ETD = new ZDateTime(2004, 8, 14);
			consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "AUSYD";
			consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "JPNRT";
			consol.JK_UniqueConsignRef = "C00001000";
			Factory.Save();
			string testFile = TestUtils.CopyResourceToFile(TNTTestUtils.SampleX2Name);
			Exit2ImportManager testManager = new Exit2ImportManager(Factory, testFile);
			foreach (QuantumMawb mawb in testManager.Exit2Mawbs)
			{
				if (mawb.MasterBillNum == consol.JK_MasterBillNum)
				{
					mawb.LinkedConsol = consol;
				}
			}

			testManager.ProcessAllMatches(false);
			string expectedReport = string.Format("Consol ID\t\t- MasterBill\t- Shipments Linked{0}{0}C00001000\t- 08135025185\t- 4", System.Environment.NewLine);
			AssertMultilineASCIIEquals("Process Report should not display the same information more than once", expectedReport, testManager.GetExit2ProcessReport());
		}

		public void TestQuantumFileProcessedDirectory()
		{
			TNTDataRegistry.Instance.QuantumFileProcessedDirectoryForManualImport = "TESTAU";
			Exit2ImportManager testManager = new Exit2ImportManager(Factory, TestUtils.TinyFileFullName);
			var participants = testManager.SaveFactories;
			AssertEquals("PRE: there are two participants", 2, participants.Count);
			SaveInTransactionActionProcessedFileMover filemover = (SaveInTransactionActionProcessedFileMover)participants[1];
			AssertEquals("MoveToDirectory should be the same as the one in AU Registry", "TESTAU", filemover.FileMover.MoveToDirectory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new Exit2ImportManager(Factory, TestUtils.CopyResourceToFile(TNTTestUtils.SampleX2Name));
		}

		TNTTestUtils TestUtils;

		protected override void SetUp()
		{
			base.SetUp();
			TestUtils = new TNTTestUtils();
		}

		protected override void TearDown()
		{
			TestUtils.DeleteTempDirectoryFiles();
			base.TearDown();
		}
	}
}
