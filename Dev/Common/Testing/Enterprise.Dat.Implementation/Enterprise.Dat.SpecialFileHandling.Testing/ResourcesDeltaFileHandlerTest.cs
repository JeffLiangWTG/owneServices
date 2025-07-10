using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CargoWise.IO;
using CargoWiseOne.ResourceStrings;
using Dat.Integration.VersionControl;
using Enterprise.Dat.SpecialFileHandling.Assets;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.Dat.SpecialFileHandling.Testing
{
	class ResourcesDeltaFileHandlerTest : TestCase
	{
		public void TestShouldUnshelveForDATCheckin()
		{
			var fileHandler = CreateHandler();
			Assert(!fileHandler.ShouldUnshelveForDATCheckin(new MockPendingChange(TfsChangeType.Edit, "$/Dev/Enterprise/Product/Core/ResourceStrings/DataFiles/XYZ/Resources.xml")));
			Assert(!fileHandler.ShouldUnshelveForDATCheckin(new MockPendingChange(TfsChangeType.Edit, "$/Dev/Enterprise/Product/Core/ResourceStrings/DataFiles/XYZ/ResourcesDelta.xml")));
			Assert(fileHandler.ShouldUnshelveForDATCheckin(new MockPendingChange(TfsChangeType.Add, "$/Dev/Enterprise/Product/Core/ResourceStrings/DataFiles/XYZ/Resources.xml")));
			Assert(fileHandler.ShouldUnshelveForDATCheckin(new MockPendingChange(TfsChangeType.Add, "$/Dev/Enterprise/Product/Core/ResourceStrings/DataFiles/XYZ/ResourcesDelta.xml")));
		}

		public void TestUpdateForDATCheckinOnDeltaFile()
		{
			using (var tempDir = new TempDirectory())
			using (var resourcesDeltaShelvedFile = TempFile.New())
			{
				var resourcesFile = Path.Combine(tempDir.DirectoryName, "Resources.xml");
				var resourcesDeltaSourceFile = Path.Combine(tempDir.DirectoryName, "ResourcesDelta.xml");
				var packageNameFile = Path.Combine(tempDir.DirectoryName, "PackageName.txt");

				using (var resourcesFileStream = File.Create(resourcesFile))
				{
					new ResourceStringXmSerializer(resourcesFileStream, "XYZ").Serialize(
					[
						new ResourceStringData("k1", "caption 1"),
						new ResourceStringData("k2", "caption 2"),
						new ResourceStringData("k3", "caption 3"),
					]);
				}

				using (var resourcesDeltaSourceFileStream = File.Create(resourcesDeltaSourceFile))
				{
					new ResourceStringXmSerializer(resourcesDeltaSourceFileStream, "XYZ").Serialize([]);
				}

				using (var resourcesDeltaShelvedFileStream = File.Create(resourcesDeltaShelvedFile.Filename))
				{
					new ResourceStringXmSerializer(resourcesDeltaShelvedFileStream, "XYZ").Serialize(
					[
						new ResourceStringData("k4", "caption 4"),
						new ResourceStringData("k2", string.Empty),
						new ResourceStringData("k3", "new caption 3"),
						new ResourceStringData("k0", "caption 0"),
					]);
				}

				var workspace = new Mock<IWorkspaceAccess>(MockBehavior.Strict);
				workspace.Setup(m => m.GetLocalItemForServerItem("$/Dev/Enterprise/Product/Core/ResourceStrings/DataFiles/XYZ/Resources.xml")).Returns(resourcesFile);
				workspace.Setup(m => m.PendEdit(resourcesFile)).Returns(1);

				var assetService = new DummyAssetService();
				var fileHandler = CreateHandler(assetService);
				var unshelvedFiles = fileHandler.UpdateForDATCheckin(workspace.Object, new MockPendingChange(TfsChangeType.Edit, "$/Dev/Enterprise/Product/Core/ResourceStrings/DataFiles/XYZ/ResourcesDelta.xml", resourcesDeltaShelvedFile.Filename));
				var upload = assetService.Uploads.Single();
				AssertContainsExactElementsInAnyOrder([resourcesFile, packageNameFile], unshelvedFiles);

				var expectedXml =
					"""
					<?xml version="1.0" encoding="utf-8"?>
					<EnterpriseResources Language="XYZ">
					  <Res>
					    <Key>k0</Key>
					    <Caption>caption 0</Caption>
					  </Res>
					  <Res>
					    <Key>k1</Key>
					    <Caption>caption 1</Caption>
					  </Res>
					  <Res>
					    <Key>k3</Key>
					    <Caption>new caption 3</Caption>
					  </Res>
					  <Res>
					    <Key>k4</Key>
					    <Caption>caption 4</Caption>
					  </Res>
					</EnterpriseResources>
					""";

				var expectedPackageName = "ResourceStrings/content/Translations/ResourceStrings-XYZ-####-##-f7105620ebc0dcd5443611ed0bbbf72ca8b691c4cfb33171085aa9557def0fa6";

				AssertEquals(expectedPackageName, HashOutDate(upload.AssetName));
				AssertEquals(resourcesFile, upload.FileName);
				AssertEquals(expectedXml, upload.Content);

				AssertMultilineASCIIEquals(
					"The resulting output should be deterministic.",
					expectedXml,
					File.ReadAllText(resourcesFile));

				AssertEquals(
					"PackageName.txt should contain the name of the uploaded package.",
					upload.AssetName,
					File.ReadAllText(packageNameFile));

				using (var resourcesFileStream = File.OpenRead(resourcesFile))
				{
					AssertContainsExactElementsInAnyOrder(
						[
							new ResourceStringData("k0", "caption 0"),
							new ResourceStringData("k1", "caption 1"),
							new ResourceStringData("k3", "new caption 3"),
							new ResourceStringData("k4", "caption 4"),
						],
						XmlResourceStringSource.ReadAll(resourcesFileStream));
				}
			}
		}

		public void TestUndefinedLanguageCode()
		{
			using (var tempDir = new TempDirectory())
			using (var resourcesDeltaShelvedFile = TempFile.New())
			{
				var resourcesFile = Path.Combine(tempDir.DirectoryName, "Resources.xml");
				var resourcesDeltaSourceFile = Path.Combine(tempDir.DirectoryName, "ResourcesDelta.xml");
				var packageNameFile = Path.Combine(tempDir.DirectoryName, "PackageName.txt");

				File.WriteAllText(
					resourcesFile,
					"""
					<?xml version="1.0" encoding="utf-8"?>
					<EnterpriseResources>
					</EnterpriseResources>
					""");

				File.WriteAllText(
					resourcesDeltaSourceFile,
					"""
					<EnterpriseResources>
					</EnterpriseResources>
					""");

				File.WriteAllText(
					resourcesDeltaShelvedFile.Filename,
					"""
					<EnterpriseResources>
					  <Res>
					    <Key>Foo</Key>
					    <Caption>Bar</Caption>
					  </Res>
					</EnterpriseResources>
					""");

				var workspace = new Mock<IWorkspaceAccess>(MockBehavior.Strict);
				workspace.Setup(m => m.GetLocalItemForServerItem("$/Dev/Enterprise/Product/Core/ResourceStrings/DataFiles/XYZ/Resources.xml")).Returns(resourcesFile);
				workspace.Setup(m => m.PendEdit(resourcesFile)).Returns(1);

				var assetService = new DummyAssetService();
				var fileHandler = CreateHandler(assetService);

				fileHandler.UpdateForDATCheckin(workspace.Object, new MockPendingChange(TfsChangeType.Edit, "$/Dev/Enterprise/Product/Core/ResourceStrings/DataFiles/XYZ/ResourcesDelta.xml", resourcesDeltaShelvedFile.Filename));
				var upload = assetService.Uploads.Single();

				var expectedXml =
					"""
					<?xml version="1.0" encoding="utf-8"?>
					<EnterpriseResources>
					  <Res>
					    <Key>Foo</Key>
					    <Caption>Bar</Caption>
					  </Res>
					</EnterpriseResources>
					""";

				AssertEquals(expectedXml, upload.Content);
			}
		}

		static ResourcesDeltaFileHandler CreateHandler(IAssetService assetService = null)
		{
			return new ResourcesDeltaFileHandler(assetService ?? NullAssetService.Instance);
		}

		static string HashOutDate(string assetName)
		{
			return Regex.Replace(assetName, "-[0-9]{4}-[0-9]{2}-", "-####-##-");
		}

		sealed class NullAssetService : IAssetService
		{
			public static NullAssetService Instance { get; } = new();

			NullAssetService()
			{
			}

			public Task UploadAsync(string assetName, string fileName)
			{
				return Task.CompletedTask;
			}
		}

		sealed class DummyAssetService : IAssetService
		{
			public Task UploadAsync(string assetName, string fileName)
			{
				uploads.Add(new UploadInfo(assetName, fileName, File.ReadAllText(fileName)));
				return Task.CompletedTask;
			}

			public IEnumerable<UploadInfo> Uploads => uploads;

			readonly List<UploadInfo> uploads = new();

			public sealed class UploadInfo(string assetName, string fileName, string content)
			{
				public string AssetName { get; } = assetName;
				public string FileName { get; } = fileName;
				public string Content { get; } = content;
			}
		}
	}
}
