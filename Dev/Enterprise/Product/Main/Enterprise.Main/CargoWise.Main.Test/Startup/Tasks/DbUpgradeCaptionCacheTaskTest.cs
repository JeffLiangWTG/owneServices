using CargoWise.Main.Startup.Tasks;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace CargoWise.Main.Test.Startup.Tasks
{
	internal class DbUpgradeCaptionCacheTaskTest : TestCase
	{
		public void TestShouldExecute()
		{
			// Arrange
			// Act
			// Assert
			Assert(new DbUpgradeCaptionCacheTask().ShouldExecute());
		}

		[ExpectNoExceptions]
		public void TestExecute()
		{
			// Arrange
			var upgradeCaptionsMock = new Mock<IDbUpgradeCaptions>();
			var envMock = new Mock<BaseEnvironment>(Mock.Of<IUserContextManager>());
			envMock
				.Setup(x => x.DbUpgradeCaptions)
				.Returns(upgradeCaptionsMock.Object);

			var evnProviderMock = new Mock<EnvProvider>() { CallBase = true };
			evnProviderMock
				.Setup(x => x.Instance)
				.Returns(envMock.Object);

			using (envMock.Object)
			using (evnProviderMock.Object)
			using(Env.SetEnvProviderTemporarily(evnProviderMock.Object))
			{
				// Act
				new DbUpgradeCaptionCacheTask().Execute();

				// Assert
				upgradeCaptionsMock.Verify(x => x.Refresh(), Times.Once);
			}
		}
	}
}
