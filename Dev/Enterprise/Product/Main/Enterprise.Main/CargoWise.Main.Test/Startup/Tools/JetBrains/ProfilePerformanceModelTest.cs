using System.IO;
using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using Moq;
using NUnit.Framework;

namespace CargoWise.Main.Startup.Tools.JetBrains.Testing
{
	[TestedType(typeof(ProfilePerformanceModel))]
	sealed class ProfilePerformanceModelTest : ProfileModelTest
	{
		protected override string RunnerName => "dotTrace";

		public void TestStart_PrerequisitesInstalledAndSnapshotsFolderSelected_StartProfiling()
		{
			if (Directory.Exists(PrerequisitesDestinationPath))
			{
				Directory.Delete(PrerequisitesDestinationPath, true);
			}

			var dialogServiceMock = new Mock<IDialogService>();
			dialogServiceMock.Setup(d => d.AcceptLicenseAgreement(It.IsAny<string>())).Returns(true);
			dialogServiceMock.Setup(d => d.Download(PrerequisitesUrl)).Returns(PrerequisitesZipFile);
			dialogServiceMock.Setup(d => d.SelectFolder(It.IsAny<string>(), It.IsAny<string>())).Returns("C:\\McLaren");

			var instance = GetInstance(dialogServiceMock.Object, PrerequisitesZipFileCheckSum);
			instance.Init();

			dialogServiceMock.Verify(d => d.AcceptLicenseAgreement(It.IsAny<string>()), Times.Once);
			dialogServiceMock.Verify(d => d.Download(PrerequisitesUrl), Times.Once);
			dialogServiceMock.Verify(d => d.SelectFolder(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
			dialogServiceMock.Verify(d => d.ProfilePerformance(It.Is<ProfilePerformanceModel>(m => m.UserSnapshotsPath == "C:\\McLaren")), Times.Once);

			Assert(true);
		}

		protected override ProfileModel GetInstance(IDialogService dialogService, string prerequisiteCheckSum = null)
		{
			return new ModelForTest(dialogService, prerequisiteCheckSum);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetInstance(new Mock<IDialogService>().Object);
		}

		protected override string PrerequisitesUrl => "https://myaccount-portal.cargowise.com/my-account/public/downloads/JetBrains.dotTrace.CommandLineTools.windows-x64.2022.2.2.zip";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI011:TempPathRule", Justification = "We don't create any folder/file in tests. We just want to test that the valid path is generated and passed by the business logic.")]
		protected override string PrerequisitesDestinationPath => Path.Combine(Path.Combine(Path.GetTempPath(), BrandingFactory.Instance.CompanyName), "JetBrains.dotTrace.CommandLineTools.windows-x64.2022.2.2");

		class ModelForTest : ProfilePerformanceModel
		{
			public ModelForTest(IDialogService dialogService, string prerequisiteCheckSum) : base(dialogService, ProfilingType.SAMPLING)
			{
				PrerequisiteCheckSum = prerequisiteCheckSum;
			}

			protected override string PrerequisiteCheckSum { get; }

			protected override bool DoStartProfiling() => true;

			protected override bool DoStopProfiling() => true;
		}
	}
}
