using System;
using System.IO;
using CargoWise.BuildTools;
using CargoWise.BuildTools.Testing;
using Enterprise.Builder.DataUpgradeSetup;
using Enterprise.Builder.DataUpgradeSetup.Testing;
using Enterprise.DbUpgrader.Data;
using Enterprise.DbUpgrader.Data.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Build.Testing
{
	public class DocumentsSetupControllerTest : TransactionedTestCase
	{
		public void TestFullCheckOut()
		{
			Exception expectedException = new SourceControlException("Haha");
			var mock = new Mock<ISourceControl>();
			mock.Setup(m => m.CheckOut(It.IsAny<string>(), It.IsAny<bool>())).Throws(expectedException);
			SourceControl.SetEnterpriseInstanceForTesting(mock.Object);

			DocumentsTestSetupController controller = new DocumentsTestSetupController();

			try
			{
				controller.FullCheckOut();
				Fail("No exception thrown");
			}
			catch (SourceControlException ex)
			{
				AssertEquals(expectedException, ex);
			}
		}

		#region Test Classes

		public class DocumentsTestSetupController : DocumentsSetupController
		{
			public DocumentsTestSetupController()
			{
				TaskSetupList[0] = new DataTaskSetup(new EmbeddedUpgradeTask(new DocumentsTestDataFile()), this);
			}

			public override string DataVersionFile
			{
				get { return DataUpgradeSetupControllerForTest.TestDataVersionFile; }
			}

			public static string SourceControlTestClientsDir
			{
				get { return Path.Combine(MockSourceControl.MockWorkspacePath, @"DataUpgradeSetup\Clients"); }
			}
		}

		public class DocumentsTestDataFile : DocumentsDataFile
		{
			public static string TestDataFilePath
			{
				get { return Path.Combine(MockSourceControl.MockWorkspacePath, @"DataUpgradeSetup\TestDataFile.xml"); }
			}

			public DocumentsTestDataFile()
				: base(TestDataFilePath)
			{
			}

			public override string FileResourceName
			{
				get { return "Enterprise.DbUpgrader.Data.Shared.Test.TestFiles.TestDataFile.xml"; }
			}

			protected override System.Reflection.Assembly ResourceAssembly
			{
				get { return typeof(TestFileConstants).Assembly; }
			}

			public override string DefaultDataFileBasePath
			{
				get
				{
					return "";
				}
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			MockSourceControl.Setup();
		}

		protected override void TearDown()
		{
			base.TearDown();
			MockSourceControl.TearDown();
		}

		#endregion
	}
}
