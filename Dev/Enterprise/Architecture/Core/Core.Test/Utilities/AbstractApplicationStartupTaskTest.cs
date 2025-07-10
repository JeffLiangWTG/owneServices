using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.ZArchitecture.Core.Test.Utilities
{
	public abstract class AbstractApplicationStartupTaskTest<T> : TestCaseWithFactory where T : AbstractApplicationStartupTask
	{
		protected virtual object[] GetConstructorParams() => System.Array.Empty<object>();
		public abstract int DefaultErrorExitCode { get; }

		public virtual void TestExecuteSetsUpExitCodeWhenError()
		{
			// Arrange
			var applicationStartupTaskMock = new Mock<T>(GetConstructorParams()) { CallBase = true };

			// Act
			// Assert
			if (DefaultErrorExitCode != ExitCodes.Success)
			{
				AssertNotEquals(ExitCodes.Success, applicationStartupTaskMock.Object.FailureExitCode);
			}

			AssertEquals(DefaultErrorExitCode, applicationStartupTaskMock.Object.FailureExitCode);
		}
	}
}
