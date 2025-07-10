using CargoWise.IO;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Testing
{
	class FtpEngineWithTriggerTest : TestCase
	{
		public void TestPullRemoteFilesFromSourceLogs()
		{
			var logger = new Logger();
			var engine = new TestFtpEngineWithTrigger(logger);
			engine.PullRemoteFilesFromSource();
			AssertContains("received 1234 bytes", logger.ToString());
		}

		class TestFtpEngineWithTrigger : FtpEngineWithTrigger
		{
			public TestFtpEngineWithTrigger(ILogger serviceLogger) : base(serviceLogger, CreateMockedOptionsProvider())
			{
			}

			public new void PullRemoteFilesFromSource()
			{
				Initialise();
				base.PullRemoteFilesFromSource();
			}

			public override string[] TriggerFileExtensionsIncludingDots_Pull => new[] { ".m" };

			public override string TriggerFileExtensionIncludingDots_Push => string.Empty;

			protected override IFtpProcessor CreateFTPProcessor()
			{
				var mockProcessor = new Mock<IFtpProcessor>();
				mockProcessor.Setup(m => m.ListDirectory("/")).Returns(new[] { "filename.m", "filename.1" });
				mockProcessor.Setup(m => m.DownloadFile(It.IsAny<string>(), "/filename.1")).Returns(1234);
				mockProcessor.Setup(m => m.DeleteRemoteFile(It.IsAny<string>())).Returns(true);

				return mockProcessor.Object;
			}

			static IProviderOfTriggerFtpOptions CreateMockedOptionsProvider()
			{
				var mockProvider = new Mock<IProviderOfTriggerFtpOptions>();
				mockProvider.Setup(m => m.LocalEnterpriseSharedFolderName).Returns(".");
				mockProvider.Setup(m => m.UriString).Returns("ftp://localhost");
				mockProvider.Setup(m => m.Username).Returns(".");
				mockProvider.Setup(m => m.Password).Returns(".");

				return mockProvider.Object;
			}
		}
	}
}
