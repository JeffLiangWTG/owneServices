using System;
using CargoWise.Loader.Common;
using Enterprise.Upgrades.UpgradePackageServices;
using Moq;
using NUnit.Framework;

namespace Enterprise.Server.Setup.Testing
{
	class DownloadPackageTest : TestCase
	{
		public void TestShouldThrowErrorWhenThePackageURLIsEmpty()
		{
			// Arrange
			SetupConfiguration config = new SetupConfiguration();
			config.InstallationSettings.LicenseCode = "XXXZZZ";

			var downloadPackage = new DownloadPackage_ForTest(new Installation(config), config.InstallationSettings);

			var onlineUpgradeVersion = new Version(1, 2, 3, 4);

			var responses = new UpgradePackageUrlResponse
			{
				ResponseClass = UpgradePackageUrlResponse.ResponseClassType.Success,
				URL = "",
				ErrorMessage = $"The package {onlineUpgradeVersion} has been rolled out to CargoWise Cloud",
				VersionNumber = onlineUpgradeVersion.ToString()
			};

			var mockClient = new Mock<IUpgradePackageService>(MockBehavior.Strict);
			mockClient.Setup(a => a.GetPackageUrl(It.IsAny<UpgradePackageUrlRequest>()))
				.Returns(responses);

			var result = new InstallationResultCollection();

			downloadPackage.OverwrittenUpgradePackageServiceClient = mockClient.Object;

			// Act
			downloadPackage.Install(result);

			// Assert
			var expectedMessage = $"Skipped downloading of the package: {responses.ErrorMessage}";
			var actualMessage = result.GetErrorMessages();
			AssertEquals(expectedMessage, actualMessage);
		}

		class DownloadPackage_ForTest : DownloadPackage
		{
			public DownloadPackage_ForTest(Installation installation, InstallationSettings installationSettings)
				: base(installation, installationSettings)
			{
			}

			protected override IUpgradePackageService UpgradePackageServiceClient => OverwrittenUpgradePackageServiceClient ?? base.UpgradePackageServiceClient;

			internal IUpgradePackageService OverwrittenUpgradePackageServiceClient { get; set; }
		}
	}
}
