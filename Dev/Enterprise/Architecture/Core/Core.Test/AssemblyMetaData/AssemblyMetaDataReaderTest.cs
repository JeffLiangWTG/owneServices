using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.IO;
using Enterprise.ZArchitecture.Business;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class AssemblyMetaDataReaderTest
	{
		sealed class AssemblyMetaDataReaderImplTestBaseOnly : TestCase
		{
			public void TestIsSingleton()
			{
				var metaDataReader = ObjectFactory.Get<IAssemblyMetaDataReader>();
				AssertSame("IAssemblyMetaDataReader is configured as Singleton", metaDataReader, ObjectFactory.Get<IAssemblyMetaDataReader>());
			}

			public void TestIsRegistered()
			{
				AssertNotNull("IAssemblyMetaDataReader", ObjectFactory.Get<IAssemblyMetaDataReader>());
			}
		}

		abstract class AssemblyMetaDataReaderImplTestCase : TestCase
		{
			public void TestAssemblyMetaDataFile()
			{
				var assemblyMetaDataFiles = assemblyMetaDataReader.AssemblyMetaDataFiles;
				Assert("There should be at least 1 AssemblyMetaData.xml file", assemblyMetaDataFiles.Length > 0);
				Assert("AssemblyMetaData.xml is present in the path", assemblyMetaDataFiles.All(f => f.Contains("AssemblyMetaData.xml")));
			}

			public void TestGetAttributes()
			{
				var attributes = assemblyMetaDataReader.GetAttributes<ClusterKeyMetaDataAttribute>(false)?.ToArray();

				AssertNotNull("Attributes", attributes);
				AssertEquals("Attribute Count", 1, attributes.Length);
			}

			public void TestGetAttributes_RetrieveForAllClients()
			{
				var attributes = assemblyMetaDataReader.GetAttributes<ClusterKeyMetaDataAttribute>(true)?.ToArray();

				AssertNotNull("Attributes", attributes);
				AssertEquals("Attribute Count", 2, attributes.Length);
			}

			public void TestFilesExist()
			{
				Assert(assemblyMetaDataReader.FilesExist);

				if (Directory.Exists(tempRootPath))
				{
					Array.ForEach(Directory.GetFiles(tempRootPath), File.Delete);
				}

				assemblyMetaDataReader = GetAssemblyMetaDataReaderMock().Object;
				Assert("AssemblyMetaData.xml should not exist", !assemblyMetaDataReader.FilesExist);
			}

			protected override void SetUp()
			{
				base.SetUp();
				embeddedResourceRetriever = new EmbeddedResourceRetriever();
				tempRootPath = embeddedResourceRetriever.DirectoryPath;
				Directory.CreateDirectory(tempRootPath);

				foreach (var assemblyMetaDataFile in AssemblyMetaDataFilesForTest)
				{
					embeddedResourceRetriever.SaveResourceToFile(assemblyMetaDataFile.Value, assemblyMetaDataFile.Key);
				}

				assemblyMetaDataReaderMock = GetAssemblyMetaDataReaderMock();
				assemblyMetaDataReader = assemblyMetaDataReaderMock.Object;
			}

			Mock<AssemblyMetaDataReaderImpl> GetAssemblyMetaDataReaderMock()
			{
				var readerMock = new Mock<AssemblyMetaDataReaderImpl> { CallBase = true };
				readerMock.Protected().SetupGet<string>("RootFolder").Returns(tempRootPath);
				return readerMock;
			}

			protected override void TearDown()
			{
				embeddedResourceRetriever?.Dispose();
				embeddedResourceRetriever = null;
				tempRootPath = null;

				base.TearDown();
			}

			string tempRootPath;

			Mock<AssemblyMetaDataReaderImpl> assemblyMetaDataReaderMock;
			IAssemblyMetaDataReader assemblyMetaDataReader;
			EmbeddedResourceRetriever embeddedResourceRetriever;

			protected abstract Dictionary<string, string> AssemblyMetaDataFilesForTest { get; }
		}

		[TestedType(typeof(AssemblyMetaDataReaderImpl))]
		sealed class AssemblyMetaDataReaderImplSingleFileTest : AssemblyMetaDataReaderImplTestCase
		{
			protected override Dictionary<string, string> AssemblyMetaDataFilesForTest
			{
				get
				{
					var fileList = new Dictionary<string, string>
					{
						{ "AssemblyMetaData.xml", "Enterprise.ZArchitecture.Core.Test.AssemblyMetaData.TestFiles.AssemblyMetaDataForTest.xml" },
					};
					return fileList;
				}
			}
		}

		[TestedType(typeof(AssemblyMetaDataReaderImpl))]
		sealed class AssemblyMetaDataReaderImplSubModuleFilesTest : AssemblyMetaDataReaderImplTestCase
		{
			protected override Dictionary<string, string> AssemblyMetaDataFilesForTest
			{
				get
				{
					var fileList = new Dictionary<string, string>
					{
						{ "ModuleA.AssemblyMetaData.xml", "Enterprise.ZArchitecture.Core.Test.AssemblyMetaData.TestFiles.ModuleA.AssemblyMetaDataForTest.xml" },
						{ "ModuleB.AssemblyMetaData.xml", "Enterprise.ZArchitecture.Core.Test.AssemblyMetaData.TestFiles.ModuleB.AssemblyMetaDataForTest.xml" },
					};

					return fileList;
				}
			}
		}

		[TestedType(typeof(AssemblyMetaDataReaderImpl))]
		sealed class AssemblyMetaDataReaderImplSingleAndSubModuleFilesCombinedTest : AssemblyMetaDataReaderImplTestCase
		{
			protected override Dictionary<string, string> AssemblyMetaDataFilesForTest
			{
				get
				{
					var fileList = new Dictionary<string, string>
					{
						{ "AssemblyMetaData.xml", "Enterprise.ZArchitecture.Core.Test.AssemblyMetaData.TestFiles.AssemblyMetaDataForTest.xml" },
						{ "ModuleA.AssemblyMetaData.xml", "Enterprise.ZArchitecture.Core.Test.AssemblyMetaData.TestFiles.ModuleA.AssemblyMetaDataForTest.xml" },
						{ "ModuleB.AssemblyMetaData.xml", "Enterprise.ZArchitecture.Core.Test.AssemblyMetaData.TestFiles.ModuleB.AssemblyMetaDataForTest.xml" },
					};

					return fileList;
				}
			}
		}
	}

	[TestedType(typeof(AssemblyMetaDataReader))]
	sealed class AssemblyMetaDataReaderStaticTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestGetCustomAttributes()
		{
			var assemblyMetaDataReaderMock = new Mock<IAssemblyMetaDataReader>();
			using (ObjectFactory.Substitute(assemblyMetaDataReaderMock.Object))
			{
				AssemblyMetaDataReader.GetAttributes<ClusterKeyMetaDataAttribute>();
				assemblyMetaDataReaderMock.Verify(r => r.GetAttributes<ClusterKeyMetaDataAttribute>(It.Is<bool>(v => !v)), Times.Once);
			}
		}

		[ExpectNoExceptions]
		public void TestGetCustomAttributes_WithRequiredForAllClients()
		{
			var assemblyMetaDataReaderMock = new Mock<IAssemblyMetaDataReader>();
			using (ObjectFactory.Substitute(assemblyMetaDataReaderMock.Object))
			{
				AssemblyMetaDataReader.GetAttributes<ClusterKeyMetaDataAttribute>(true);
				assemblyMetaDataReaderMock.Verify(r => r.GetAttributes<ClusterKeyMetaDataAttribute>(It.Is<bool>(v => v)), Times.Once);
			}
		}

		public void TestAssemblyMetaDataFile_StaticClass()
		{
			var assemblyMetaDataReaderMock = new Mock<IAssemblyMetaDataReader>();
			assemblyMetaDataReaderMock.Setup(s => s.AssemblyMetaDataFiles).Returns(new[] { "File.xml" });
			using (ObjectFactory.Substitute(assemblyMetaDataReaderMock.Object))
			{
				AssertArrayEqualsByElements("AssemblyMetaDataFiles", new[] { "File.xml" }, AssemblyMetaDataReader.AssemblyMetaDataFiles);
			}

			assemblyMetaDataReaderMock.Setup(s => s.AssemblyMetaDataFiles).Returns(new[] { "A.File.xml", "B.File.xml" });
			using (ObjectFactory.Substitute(assemblyMetaDataReaderMock.Object))
			{
				AssertArrayEqualsByElements("AssemblyMetaDataFiles", new[] { "A.File.xml", "B.File.xml" }, AssemblyMetaDataReader.AssemblyMetaDataFiles);
			}
		}
	}
}
