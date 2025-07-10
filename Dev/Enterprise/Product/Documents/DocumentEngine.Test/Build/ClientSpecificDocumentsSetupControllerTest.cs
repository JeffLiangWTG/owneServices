using System;
using System.IO;
using CargoWise.BuildTools;
using CargoWise.BuildTools.Testing;
using Enterprise.DbUpgrader.Data.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Build.Testing
{
	public class ClientSpecificDocumentsSetupControllerTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreatingNewClientXml()
		{
			//Delete the existing one because this test is all about creating one
			File.SetAttributes(TestFileName, FileAttributes.Normal);
			File.Delete(TestFileName);

			AssertEquals("File exists", false, File.Exists(TestFileName));

			var controller = new ClientSpecificDocumentsTestSetupController("TestClient");
			controller.Initialise();
			AssertEquals("File exists", true, File.Exists(TestFileName));
			AssertEquals("IsCheckedOut", false, SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(TestFileName));

			controller.FullCheckOut();

			AssertEquals("IsCheckedOut", true, SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(TestFileName));

			controller.FullUndoCheckOut();
			AssertEquals("IsCheckedOut", false, SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(TestFileName));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetClientList()
		{
			var clientNames = ClientSpecificDocumentsTestSetupController.GetClientList(BaseSourcePath);

			Assert("AEL should be found", Array.IndexOf(clientNames, "AEL") >= 0);
			Assert("WLG", Array.IndexOf(clientNames, "WLG") >= 0);
		}

		public static string TestFileName => ClientSpecificDocumentsTestSetupController.SourceControlTestClientsDir + @"\TestClient\Documents\TestClientDocuments.xml";

		protected override void SetUp()
		{
			base.SetUp();
			DocumentTablesCleaner.Clean();
			MockSourceControl.Setup();
		}

		protected override void TearDown()
		{
			base.TearDown();
			MockSourceControl.TearDown();
		}
	}
}
