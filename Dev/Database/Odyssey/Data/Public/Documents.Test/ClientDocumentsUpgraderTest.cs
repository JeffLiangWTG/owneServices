using System;
using System.IO;
using System.IO.Compression;
using CargoWise.Data;
using CargoWise.IO;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.Upgrades;
using Moq;
using NUnit.Framework;
using static System.FormattableString;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class ClientDocumentsUpgraderTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDoUpgrade()
		{
			CreateTestPackage(out var packagePk, out var packageVersion);
			var upgradeManager = new UpgradeManagerForTestWithOutputBuffer();
			var clientDocumentsUpgrader = new ClientDocumentsUpgrader(upgradeManager, Db.Connection, new VersionLabel(1, 0), new UpgradeInfo(packagePk, packageVersion));

			clientDocumentsUpgrader.FetchAndExtractDocuments();
			clientDocumentsUpgrader.RunUpgrade();

			AssertEquals(1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmTemplate WHERE SO_PK = 'b94ecef6-89fe-4324-914b-255328b6a2cf'"));

			var logs = upgradeManager.OutputTextCollection;
			AssertCollectionContains("Downloading client documents", logs);
			AssertCollectionContains("Extracting client documents", logs);
			AssertCollectionContains("Loading client documents", logs);
			AssertCollectionContains("Applying client documents upgrade", logs);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestClientDocumentsUpgraderWillThrowExceptionWhenExtractingDocumentsFail()
		{
			// Arrange
			CreateTestPackage(out var packagePk, out var packageVersion);
			var upgradeManager = new UpgradeManagerForTestWithOutputBuffer();

			var upgradeManagerMock = new Mock<UpgradeManager>(It.IsAny<string>(), It.IsAny<string>());
			upgradeManagerMock
				.Setup(x => x.DownloadUpgradePackageFile(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Progress>()))
				.Callback((Guid pk, string fileName, Progress progress) =>
				{
					throw new InvalidPackageException(Invariant($"Package {pk} not available to download"));
				});

			var clientDocumentsUpgrader_ForTest = new ClientDocumentsUpgrader_ForTest(upgradeManager, Db.Connection, new VersionLabel(1, 0),
				new UpgradeInfo(packagePk, packageVersion), upgradeManagerMock);

			// Act & Assert
			AssertExceptionThrown("Should throw the detailed exception when a error occured during downloading",
				typeof(InvalidPackageException),
				Invariant($"Package {packagePk} not available to download"),
				clientDocumentsUpgrader_ForTest.FetchAndExtractDocuments);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestClientDocumentsAreLoadedAfterFetchAndExtractAreCompleted()
		{
			// Arrange
			CreateTestPackage(out var packagePk, out var packageVersion);
			var upgradeManager = new UpgradeManagerForTestWithOutputBuffer();
			var clientDocumentsUpgrader = new ClientDocumentsUpgrader(upgradeManager, Db.Connection, new VersionLabel(1, 0), new UpgradeInfo(packagePk, packageVersion));

			// Act
			clientDocumentsUpgrader.FetchAndExtractDocuments();

			// Assert
			AssertNotNull(clientDocumentsUpgrader.ClientDocumentsUpgradeTask.ResourceFile.DataSet);
		}

		const string TEST_PACKAGE_CODE = "ABC";
		const string TEST_DOCUMENTS_FILE = "ABCDocuments.xml";

		internal static void CreateTestPackage(out Guid packagePk, out Version packageVersion)
		{
			AssertEquals("Precondition", 0, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmTemplate WHERE SO_PK = 'b94ecef6-89fe-4324-914b-255328b6a2cf'"));

			DbRegistry.ClientDocumentName.SaveValue(TEST_PACKAGE_CODE, Db.Connection);

			var upgradeManager = new ClientDocumentsUpgrader(null, Db.Connection, null, null).MakeUpgradeManager(Db.Connection);
			packageVersion = new Version(1, 2, 3, 4);
			using (var packageTempDir = new TempDirectory())
			{
				File.Copy(Path.Combine(TestFileConstants.DefaultTestDataFileBasePath, @"Shared.Test\TestFiles\TestClientDocuments.xml"), Path.Combine(packageTempDir, TEST_DOCUMENTS_FILE));

				var tempFile = Path.Combine(Temp.TempPath, Guid.NewGuid().ToString());
				try
				{
					ZipFile.CreateFromDirectory(packageTempDir.DirectoryName, tempFile);
					var package = upgradeManager.UploadUpgradePackage(tempFile, packageVersion, DateTime.UtcNow, "RDY", "", null);
					packagePk = package.PK;
				}
				finally
				{
					File.Delete(tempFile);
				}
			}
		}

		class ClientDocumentsUpgrader_ForTest : ClientDocumentsUpgrader
		{
			public ClientDocumentsUpgrader_ForTest(IUpgradeManager manager, DbConnection upgConnection, VersionLabel versionBeforeUpgrade, UpgradeInfo softwareUpgrade, Mock<UpgradeManager> upgradeManagerMock) : base(manager, upgConnection, versionBeforeUpgrade, softwareUpgrade)
			{
				this.upgradeManagerMock = upgradeManagerMock;
			}

			readonly Mock<UpgradeManager> upgradeManagerMock;

			protected internal override UpgradeManager MakeUpgradeManager(DbConnection connection)
			{
				return upgradeManagerMock != null ? upgradeManagerMock.Object : base.MakeUpgradeManager(connection);
			}
		}
	}
}
