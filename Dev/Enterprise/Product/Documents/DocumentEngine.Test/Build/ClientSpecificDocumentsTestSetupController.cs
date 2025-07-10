using System.IO;
using CargoWise.BuildTools.Testing;
using Enterprise.Builder.DataUpgradeSetup;
using Enterprise.Builder.DataUpgradeSetup.Testing;
using Enterprise.DbUpgrader.Data;

namespace Enterprise.DocumentEngine.Build.Testing
{
	public class ClientSpecificDocumentsTestSetupController : ClientSpecificDocumentsSetupController
	{
		public ClientSpecificDocumentsTestSetupController(string clientName)
			: base("TestClient")
		{
			TaskSetupList.Clear();
			TaskSetupList.Add(new DataTaskSetup(new EmbeddedUpgradeTask(new DocumentsSetupControllerTest.DocumentsTestDataFile()), this));
		}

		public override string DataVersionFile => DataUpgradeSetupControllerForTest.TestDataVersionFile;

		public static string SourceControlTestClientsDir => Path.Combine(MockSourceControl.MockWorkspacePath, @"DataUpgradeSetup\Clients");

		protected override string ClientDirectoryName => SourceControlTestClientsDir;

		protected override void VerifyDbSchemaVersionIsLatest()
		{
			// when testing, this will always throw DbSchemaVersionIsNotTheLatestException
		}
	}
}
