using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalCopy.GUI.FileLocator;
using Enterprise.UniversalCopy.GUI.ImportService;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace CargoWise.UniversalCopy.Test
{
	public class UniversalCopyXmlImportServiceTest : TestCaseWithFactory
	{
		public void TestImportFileWithValidData()
		{
			var copyTemplateTreeFroImport = new CopyTemplateTree(typeof(ITestModel));
			copyTemplateTreeFroImport.Filter = new EntityFilter { FilterTypeId = "type1", OrderBy = "PK", FilterData = "xyz" };
			using (var stream = new MemoryStream())
			{
				copyTemplateTreeFroImport.Serialize(stream);
				stream.Seek(0, SeekOrigin.Begin);
				fileLocatorMock.Setup(m => m.GetFileStream(out fileName)).Returns(stream);

				var copyTemplateTree = importService.Import(false);
				AssertNotNull("The imported object should not be null", copyTemplateTree);
				AssertEquals("The imported value should be same with original", "type1", copyTemplateTree.Filter.FilterTypeId);
				AssertEquals("The imported value should be same with original", "PK", copyTemplateTree.Filter.OrderBy);
				AssertEquals("The imported value should be same with original", "xyz", copyTemplateTree.Filter.FilterData);
			}
		}

		public void TestImportFileWithInvalidData()
		{
			string content = "Test Content";
			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream))
			{
				writer.Write(content);
				writer.Flush();

				fileLocatorMock.Setup(m => m.GetFileStream(out fileName)).Returns(stream);

				var copyTemplateTree = importService.Import(false);
				AssertNull("The imported object should be null", copyTemplateTree);
			}
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
			importServiceMock = mocks.Create<UniversalCopyXmlImportService>();
			importService = importServiceMock.Object;
			fileLocatorMock = mocks.Create<UniversalCopyReadFileLocator>();
			loggerMock = mocks.Create<IUserNotification>();
			importService.Logger = loggerMock.Object;
			importService.FileLocator = fileLocatorMock.Object;
			fileName = Path.Combine(EnvProxy.Instance.TempPath, Guid.NewGuid().ToString());
		}

		UniversalCopyXmlImportService importService;
		Mock<UniversalCopyXmlImportService> importServiceMock;
		Mock<UniversalCopyReadFileLocator> fileLocatorMock;
		MockRepository mocks;
		Mock<IUserNotification> loggerMock;
		string fileName;
	}
}
