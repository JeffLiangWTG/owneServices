using System;
using System.IO;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWiseOne.WebInfrastructure.ErrorReporting;
using CargoWiseOne.WebInfrastructure.TestFramework;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

#pragma warning disable CS0436 // Type conflicts with imported type - due to InternalsVisibleTo
using InternalCommonAssemblyInfo = CommonAssemblyInfo;
#pragma warning restore CS0436 // Type conflicts with imported type

namespace CargoWiseOne.WebInfrastructure.Integration.Test
{
	[UseSnapshotProtection]
	class WebUpgradeManagerTest : TestCase
	{
		public void TestClientSpecificPackageIsInstalled()
		{
			var packageMaker = new PackageMaker(((IDbConnectionInternals)Db.Connection).ADOConnection);
			packageMaker.AddWebFile(PackageMaker.EnterpriseWebDeployZipName, "SharedFileList.txt", "bla bla bla");
			packageMaker.AddWebFile(PackageMaker.EnterpriseWebDeployZipName, "Tracking", "Tracking.aspx", "tracking");
			packageMaker.AddWebFile(PackageMaker.EnterpriseWebDeployZipName, "SomethingElse", "Foo.aspx", "foo");
			packageMaker.AddWebFile("ZClientWebEDI.zip", "SharedFileList.txt", "alb alb alb");
			var path = webUpgradeManager.InstallWebFilesForTest(packageMaker.UploadPackage(new Version(1, 0, 0, 0), "CUR"));
			AssertFileSameAsString(Path.Combine(path, "Tracking", "Tracking.aspx"), "tracking");
			AssertFileSameAsString(Path.Combine(path, "SomethingElse", "Foo.aspx"), "foo");
		}

		public void TestNetCoreAssembliesCopiedToBin()
		{
			var packageMaker = new PackageMaker(((IDbConnectionInternals)Db.Connection).ADOConnection);
			packageMaker.AddWebFile(PackageMaker.EnterpriseWebDeployZipName, "Rating", "Rating.exe", "Rating.Web");
			var path = webUpgradeManager.InstallWebFilesForTest(packageMaker.UploadPackage(new Version(1, 0, 0, 0), "CUR"));

			AssertFileSameAsString(Path.Combine(path, "Rating", "Rating.exe"), "Rating.Web");

			Assert(File.Exists(Path.Combine(path, "Rating", "bin", InternalCommonAssemblyInfo.CWNetCoreSubfolder, "NetCore.WebInfrastructure.dll")));
			Assert(File.Exists(Path.Combine(path, "Rating", "bin", InternalCommonAssemblyInfo.CWNetCoreSubfolder, "System.Data.SqlClient.dll")));
			Assert(File.Exists(Path.Combine(path, "Rating", "bin", InternalCommonAssemblyInfo.CWNetCoreSubfolder, "runtimes", "win", "lib", "net7.0", "System.Threading.AccessControl.dll")));
		}

		public void TestClientSpecificPackageIsInstalledOnExistingVersionFolder()
		{
			var packageMaker = new PackageMaker(((IDbConnectionInternals)Db.Connection).ADOConnection);
			packageMaker.AddWebFile(PackageMaker.EnterpriseWebDeployZipName, "SharedFileList.txt", "bla bla bla");
			packageMaker.AddWebFile(PackageMaker.EnterpriseWebDeployZipName, "Tracking", "Tracking.aspx", "tracking");
			packageMaker.AddWebFile(PackageMaker.EnterpriseWebDeployZipName, "SomethingElse", "Foo.aspx", "foo");
			var path = webUpgradeManager.InstallWebFilesForTest(packageMaker.UploadPackage(new Version(1, 0, 0, 0), ""));

			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
			packageMaker.BinFiles.Add("ZClientEDI.dll");
			packageMaker.AddWebFile("ZClientWebEDI.zip", "SharedFileList.txt", "alb alb alb");
			Db.Connection.ExecuteNonQuery("truncate table StmUpgrade");
			AssertEquals(path, webUpgradeManager.InstallWebFilesForTest(packageMaker.UploadPackage(new Version(1, 0, 0, 0), "")));
		}

		public void TestClientSpecificDllIsInstalledOnExistingVersionFolder()
		{
			var packageMaker = new PackageMaker(((IDbConnectionInternals)Db.Connection).ADOConnection);
			packageMaker.AddWebFile(PackageMaker.EnterpriseWebDeployZipName, "SharedFileList.txt", "bla bla bla");
			packageMaker.AddWebFile(PackageMaker.EnterpriseWebDeployZipName, "Tracking", "Tracking.aspx", "tracking");
			packageMaker.AddWebFile(PackageMaker.EnterpriseWebDeployZipName, "SomethingElse", "Foo.aspx", "foo");
			var path = webUpgradeManager.InstallWebFilesForTest(packageMaker.UploadPackage(new Version(1, 0, 0, 0), ""));

			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientDFD";
			packageMaker.BinFiles.Add("ZClientDFD.dll");
			Db.Connection.ExecuteNonQuery("truncate table StmUpgrade");
			AssertEquals(path, webUpgradeManager.InstallWebFilesForTest(packageMaker.UploadPackage(new Version(1, 0, 0, 0), "")));
			Assert(File.Exists(Path.Combine(path, "Tracking", "bin", "ZClientDFD.dll")));
		}

		public void TestClientSpecificWebDllIsInstalledOnExistingVersionFolder()
		{
			var packageMaker = new PackageMaker(((IDbConnectionInternals)Db.Connection).ADOConnection);
			packageMaker.AddWebFile(PackageMaker.EnterpriseWebDeployZipName, "SharedFileList.txt", "bla bla bla");
			packageMaker.AddWebFile(PackageMaker.EnterpriseWebDeployZipName, "Tracking", "Tracking.aspx", "tracking");
			packageMaker.AddWebFile(PackageMaker.EnterpriseWebDeployZipName, "SomethingElse", "Foo.aspx", "foo");
			var path = webUpgradeManager.InstallWebFilesForTest(packageMaker.UploadPackage(new Version(1, 0, 0, 0), ""));

			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
			packageMaker.BinFiles.Add("ZClientEDI.dll");
			packageMaker.BinFiles.Add("ZClientWebEDI.dll");
			Db.Connection.ExecuteNonQuery("truncate table StmUpgrade");
			AssertEquals(path, webUpgradeManager.InstallWebFilesForTest(packageMaker.UploadPackage(new Version(1, 0, 0, 0), "")));
			Assert(File.Exists(Path.Combine(path, "Tracking", "bin", "ZClientWebEDI.dll")));
		}

		public void TestClientSpecificXmlSerializersDllIsInstalledOnExistingVersionFolder()
		{
			var packageMaker = new PackageMaker(((IDbConnectionInternals)Db.Connection).ADOConnection);
			packageMaker.AddWebFile(PackageMaker.EnterpriseWebDeployZipName, "SharedFileList.txt", "bla bla bla");
			packageMaker.AddWebFile(PackageMaker.EnterpriseWebDeployZipName, "Tracking", "Tracking.aspx", "tracking");
			packageMaker.AddWebFile(PackageMaker.EnterpriseWebDeployZipName, "SomethingElse", "Foo.aspx", "foo");
			var path = webUpgradeManager.InstallWebFilesForTest(packageMaker.UploadPackage(new Version(1, 0, 0, 0), ""));

			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
			packageMaker.BinFiles.Add("ZClientEDI.dll");
			packageMaker.BinFiles.Add("ZClientEDI.Business.dll");
			packageMaker.BinFiles.Add("ZClientEDI.Business.XMLSerializers.dll");
			Db.Connection.ExecuteNonQuery("truncate table StmUpgrade");
			AssertEquals(path, webUpgradeManager.InstallWebFilesForTest(packageMaker.UploadPackage(new Version(1, 0, 0, 0), "")));
			Assert(File.Exists(Path.Combine(path, "Tracking", "bin", "ZClientEDI.Business.XMLSerializers.dll")));
		}

		protected override void TearDown()
		{
			base.TearDown();

			WebSiteTestManager.DeleteApplicationRootDirectory(testConfiguration.RootDirectoryPath);
			ClearCleanupLogFile();

			webUpgradeManager?.Dispose();
			sqlContext?.Dispose();
		}

		protected override void SetUp()
		{
			base.SetUp();
			ClearCleanupLogFile();
			Db.Connection.EnsureIsOpen();
			errorReporterMock = new Mock<IErrorReporter>();
			sqlContext = new WebUpgradeSqlContext(Db.ServerName, Db.DatabaseName, () => ((IDbConnectionInternals)Db.Connection).ADOConnection);
			webUpgradeManager = new WebUpgradeManagerForTest(testConfiguration, new Version(0, 0, 0, 1), sqlContext, errorReporterMock.Object, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
		}

		void ClearCleanupLogFile()
		{
			if (File.Exists(testConfiguration.CleanupLogFilePath))
			{
				File.Delete(testConfiguration.CleanupLogFilePath);
			}
		}

		WebUpgradeSqlContext sqlContext;
		readonly InstallationConfigurationForTest testConfiguration = new InstallationConfigurationForTest(Guid.NewGuid());
		WebUpgradeManagerForTest webUpgradeManager;
		Mock<IErrorReporter> errorReporterMock;
	}
}
