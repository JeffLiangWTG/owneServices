using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.TNT.Testing;
using NUnit.Framework;

namespace Enterprise.Client.TNT.NZ.Testing
{
	[TestedType(typeof(NZImportManager))]
	sealed class NZImportManagerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestQuantumMawbCollectionCore()
		{
			NZImportManager importManager = (NZImportManager)GetNewBusinessObject();
			AssertEquals("Should be a NZQuantumMawbCollection", typeof(NZQuantumMawbCollection), importManager.Exit2Mawbs.GetType());
			QuantumMawbCollection collection = importManager.Exit2Mawbs;
			AssertSame("Collection was not lazy loaded", collection, importManager.Exit2Mawbs);
		}

		public void TestQuantumFileProcessedDirectory()
		{
			TNTDataRegistry.Instance.QuantumFileProcessedDirectoryForManualImport = "TESTNZ";
			NZImportManager importManager = (NZImportManager)GetNewBusinessObject();
			var participants = importManager.SaveFactories;
			AssertEquals("PRE: there are two participants", 2, participants.Count);
			SaveInTransactionActionProcessedFileMover filemover = (SaveInTransactionActionProcessedFileMover)participants[1];
			AssertEquals("MoveToDirectory should be the same as the one in NZ Registry", "TESTNZ", filemover.FileMover.MoveToDirectory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NZImportManager(Factory, TestUtils.TinyFileFullName);
		}

		TNTTestUtils TestUtils;
		protected override void SetUp()
		{
			base.SetUp();
			TestUtils = new TNTTestUtils();
		}

		protected override void TearDown()
		{
			base.TearDown();
			TestUtils.Dispose();
		}
	}
}
