using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace CargoWise.Main.Startup.Tools.JetBrains.Testing
{
	public abstract class ProfileModelTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLicense()
		{
			var license = GetLicense();

			AssertNotNullOrEmpty(license);
			AssertStartsWith("It is jetbrains license", "JETBRAINS USER AGREEMENT", license);
		}

		public void TestInit_PrerequisitesDontExist_UserDeclinesLicense_Abort()
		{
			if (Directory.Exists(PrerequisitesDestinationPath))
			{
				Directory.Delete(PrerequisitesDestinationPath, true);
			}

			var expectedLicense = GetLicense();

			var dialogServiceMock = new Mock<IDialogService>();
			dialogServiceMock.Setup(d => d.AcceptLicenseAgreement(expectedLicense)).Returns(false);

			var instance = GetInstance(dialogServiceMock.Object);
			instance.Init();

			dialogServiceMock.Verify(d => d.AcceptLicenseAgreement(expectedLicense), Times.Once);
			dialogServiceMock.Verify(d => d.Download(It.IsAny<string>()), Times.Never);
			dialogServiceMock.Verify(d => d.SelectFolder(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
			dialogServiceMock.Verify(d => d.ProfileMemory(It.IsAny<ProfileMemoryModel>()), Times.Never);
			dialogServiceMock.Verify(d => d.ProfilePerformance(It.IsAny<ProfilePerformanceModel>()), Times.Never);

			Assert(true);
		}

		public void TestInit_PrerequisitesDownloadFailedOrCancelled_Abort()
		{
			if (Directory.Exists(PrerequisitesDestinationPath))
			{
				Directory.Delete(PrerequisitesDestinationPath, true);
			}

			var expectedLicense = GetLicense();

			var dialogServiceMock = new Mock<IDialogService>();
			dialogServiceMock.Setup(d => d.AcceptLicenseAgreement(expectedLicense)).Returns(true);
			dialogServiceMock.Setup(d => d.Download(PrerequisitesUrl)).Returns(string.Empty);

			var instance = GetInstance(dialogServiceMock.Object);
			instance.Init();

			dialogServiceMock.Verify(d => d.AcceptLicenseAgreement(expectedLicense), Times.Once);
			dialogServiceMock.Verify(d => d.Download(PrerequisitesUrl), Times.Once);
			dialogServiceMock.Verify(d => d.SelectFolder(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
			dialogServiceMock.Verify(d => d.ProfileMemory(It.IsAny<ProfileMemoryModel>()), Times.Never);
			dialogServiceMock.Verify(d => d.ProfilePerformance(It.IsAny<ProfilePerformanceModel>()), Times.Never);

			Assert(true);
		}

		public void TestInit_PrerequisitesAlreadyExist_DontDownload()
		{
			if (!Directory.Exists(PrerequisitesDestinationPath))
			{
				Directory.CreateDirectory(PrerequisitesDestinationPath);
			}

			using (File.Create(Path.Combine(PrerequisitesDestinationPath, "dotTrace.exe")))
			using (File.Create(Path.Combine(PrerequisitesDestinationPath, "dotMemory.exe")))
			{
				var dialogServiceMock = new Mock<IDialogService>();
				var instance = GetInstance(dialogServiceMock.Object);
				instance.Init();
				instance.Cleanup();

				dialogServiceMock.Verify(d => d.AcceptLicenseAgreement(It.IsAny<string>()), Times.Never);
				dialogServiceMock.Verify(d => d.Download(PrerequisitesUrl), Times.Never);

				Assert(true);
			}
		}

		public void TestInit_ChecksumForPrerequisitesIsWrong_Abort()
		{
			if (Directory.Exists(PrerequisitesDestinationPath))
			{
				Directory.Delete(PrerequisitesDestinationPath, true);
			}

			var dialogServiceMock = new Mock<IDialogService>();
			dialogServiceMock.Setup(d => d.AcceptLicenseAgreement(It.IsAny<string>())).Returns(true);
			dialogServiceMock.Setup(d => d.Download(PrerequisitesUrl)).Returns(PrerequisitesZipFile);
			dialogServiceMock.Setup(d => d.SelectFolder(PrerequisitesUrl, PrerequisitesDestinationPath)).Returns(string.Empty);

			var instance = GetInstance(dialogServiceMock.Object, "someinvalidchecksum");
			instance.Init();

			dialogServiceMock.Verify(d => d.SelectFolder(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
			dialogServiceMock.Verify(d => d.ProfileMemory(It.IsAny<ProfileMemoryModel>()), Times.Never);
			dialogServiceMock.Verify(d => d.ProfilePerformance(It.IsAny<ProfilePerformanceModel>()), Times.Never);

			Assert(true);
		}

#if !WINZOR

		public void TestInit_UserDoesntSelectFolderForSnapshots_Abort()
		{
			var dialogServiceMock = new Mock<IDialogService>();
			dialogServiceMock.Setup(d => d.AcceptLicenseAgreement(It.IsAny<string>())).Returns(true);
			dialogServiceMock.Setup(d => d.Download(PrerequisitesUrl)).Returns(PrerequisitesZipFile);
			dialogServiceMock.Setup(d => d.SelectFolder(PrerequisitesUrl, PrerequisitesDestinationPath)).Returns(string.Empty);

			var instance = GetInstance(dialogServiceMock.Object, PrerequisitesZipFileCheckSum);
			instance.Init();

			dialogServiceMock.Verify(d => d.SelectFolder(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
			dialogServiceMock.Verify(d => d.ProfileMemory(It.IsAny<ProfileMemoryModel>()), Times.Never);
			dialogServiceMock.Verify(d => d.ProfilePerformance(It.IsAny<ProfilePerformanceModel>()), Times.Never);

			Assert(true);
		}

#endif

#if WINZOR

		public void TestInit_UserNotAskedToSelectFolderForSnapshots()
		{
			var dialogServiceMock = new Mock<IDialogService>();
			dialogServiceMock.Setup(d => d.AcceptLicenseAgreement(It.IsAny<string>())).Returns(true);
			dialogServiceMock.Setup(d => d.Download(PrerequisitesUrl)).Returns(PrerequisitesZipFile);
			dialogServiceMock.Setup(d => d.SelectFolder(PrerequisitesUrl, PrerequisitesDestinationPath)).Returns(string.Empty);

			var instance = GetInstance(dialogServiceMock.Object, PrerequisitesZipFileCheckSum);
			instance.Init();
			instance.Cleanup();

			dialogServiceMock.Verify(d => d.SelectFolder(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

			Assert(true);
		}

		public void TestStopProfiling_UserAskedToSelectFolderForSnapshots()
		{
			var downloadFileStreamMock = default(Stream);
			var dialogServiceMock = new Mock<IDialogService>();
			dialogServiceMock.Setup(d => d.AcceptLicenseAgreement(It.IsAny<string>())).Returns(true);
			dialogServiceMock.Setup(d => d.Download(PrerequisitesUrl)).Returns(PrerequisitesZipFile);
			dialogServiceMock.Setup(d => d.SelectSaveAs(ProfileDownloadFileName)).Returns(downloadFileStreamMock);

			var instance = GetInstance(dialogServiceMock.Object, PrerequisitesZipFileCheckSum);
			instance.Init();
			instance.StartProfiling();
			instance.StopProfiling();
			instance.Cleanup();

			dialogServiceMock.Verify(d => d.SelectSaveAs(It.IsAny<string>()), Times.Once);

			Assert(true);
		}

		public void TestStopProfiling_UserSelectsFolderForSnapshots_DataDownloaded()
		{
			var downloadFileStreamMock = new Mock<Stream>();
			downloadFileStreamMock.Setup(d => d.CanRead).Returns(false);
			downloadFileStreamMock.Setup(d => d.CanWrite).Returns(true);
			downloadFileStreamMock.Setup(d => d.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()));

			var dialogServiceMock = new Mock<IDialogService>();
			dialogServiceMock.Setup(d => d.AcceptLicenseAgreement(It.IsAny<string>())).Returns(true);
			dialogServiceMock.Setup(d => d.Download(PrerequisitesUrl)).Returns(PrerequisitesZipFile);
			dialogServiceMock.Setup(d => d.SelectSaveAs(ProfileDownloadFileName)).Returns(downloadFileStreamMock.Object);

			var instance = GetInstance(dialogServiceMock.Object, PrerequisitesZipFileCheckSum);
			instance.Init();
			instance.StartProfiling();
			instance.StopProfiling();
			instance.Cleanup();

			downloadFileStreamMock.Verify(d => d.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.AtLeastOnce());

			Assert(true);
		}

#endif

		protected abstract ProfileModel GetInstance(IDialogService dialogService, string prerequisitesCheckSum = null);
		protected abstract string PrerequisitesUrl { get; }
		protected abstract string PrerequisitesDestinationPath { get; }
		protected abstract string RunnerName { get; }
		protected string ProfileDownloadFileName { get; } = "Profile.zip";

		static string GetLicense()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var stream = assembly.GetManifestResourceStream("CargoWise.Main.Test.Startup.Tools.JetBrains.license.txt"))
			using (var reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}

		protected string PrerequisitesZipFile
		{
			get
			{
				if (string.IsNullOrEmpty(prerequisitesZipFile))
				{
					var runnerFilePath = Path.Combine(Env.TempPath, FormattableString.Invariant($"{RunnerName}.exe"));
					File.Create(runnerFilePath).Dispose();

					prerequisitesZipFile = Path.Combine(Env.TempPath, FormattableString.Invariant($"{RunnerName}.zip"));
					new ZipCreator().CreateZipFile(runnerFilePath, prerequisitesZipFile);

					File.Delete(runnerFilePath);
				}

				return prerequisitesZipFile;
			}
		}

		protected string PrerequisitesZipFileCheckSum
		{
			get
			{
				using (var sha = SHA512.Create())
				{
					using (var stream = new FileStream(PrerequisitesZipFile, FileMode.Open))
					{
						var hash = sha.ComputeHash(stream);
						return Convert.ToBase64String(hash);
					}
				}
			}
		}

		protected override void TearDown()
		{
			base.TearDown();

			if (!string.IsNullOrEmpty(prerequisitesZipFile))
			{
				File.Delete(prerequisitesZipFile);
			}
		}

		string prerequisitesZipFile;
	}
}
