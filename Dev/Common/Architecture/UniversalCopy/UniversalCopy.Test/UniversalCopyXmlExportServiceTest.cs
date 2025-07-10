using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalCopy.GUI.ExportService;
using Enterprise.UniversalCopy.GUI.FileLocator;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace CargoWise.UniversalCopy.Test
{
	public class UniversalCopyXmlExportServiceTest : TestCaseWithFactory
	{
		public void TestExportUniversalCopy()
		{
			var universalCopyTemplate = UniversalCopyExportImportTestHelper.CrateUCTemplate(Factory, "Template1", true);
			var copyTemplateTreeNode = universalCopyTemplate.CopyTemplateTree.CopyTemplateNode;

			string content = "ConfigurationSource=\"SLT\" NominatedRecordPk=\"00000000-0000-0000-0000-000000000000\" Active=\"true\"><E><P N=\"Z0_Number\" Do=\"Copy\" /></E><ConfigurationName>Templ&amp;ate1</ConfigurationName></CopyTemplateTree>";

			fileLocatorMock.Object.CopyTemplateTree = copyTemplateTreeNode;
			using (var stream = File.OpenWrite(fileName))
			{
				fileLocatorMock.Setup(m => m.GetFileStream(out fileName)).Returns(stream);
				exportService.Export(copyTemplateTreeNode);
			}

			AssertEquals("File should be created", true, File.Exists(fileName));

			AssertContains("The file should contains the expected xml string", content, File.ReadAllText(fileName));
		}

		public void TestExportUniversalCopy_EmptyFileName()
		{
			var universalCopyTemplate = UniversalCopyExportImportTestHelper.CrateUCTemplate(Factory, "Template1", true);
			var copyTemplateTreeNode = universalCopyTemplate.CopyTemplateTree.CopyTemplateNode;

			fileLocatorMock.Object.CopyTemplateTree = copyTemplateTreeNode;

			fileLocatorMock.Setup(m => m.GetFileStream(out fileName)).Returns((Stream)null);

			exportService.Export(copyTemplateTreeNode);

			AssertEquals("File should not be created", false, File.Exists(fileName));
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			mocks = new MockRepository(MockBehavior.Loose);
			exportServiceMock = mocks.Create<UniversalCopyXmlExportService>();
			fileLocatorMock = mocks.Create<UniversalCopyWriteFileLocator>();
			loggerMock = mocks.Create<IUserNotification>();
			exportService = exportServiceMock.Object;
			exportService.FileLocator = fileLocatorMock.Object;
			exportService.Logger = loggerMock.Object;
			fileName = Path.Combine(EnvProxy.Instance.TempPath, Guid.NewGuid().ToString());
		}

		Mock<UniversalCopyXmlExportService> exportServiceMock;
		Mock<UniversalCopyWriteFileLocator> fileLocatorMock;
		Mock<IUserNotification> loggerMock;
		UniversalCopyXmlExportService exportService;
		MockRepository mocks;
		string fileName;
	}
}
