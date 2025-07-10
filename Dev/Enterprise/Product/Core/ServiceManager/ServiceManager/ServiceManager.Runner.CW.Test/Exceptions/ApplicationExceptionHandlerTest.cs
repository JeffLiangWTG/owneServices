using System;
using System.IO;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Abstractions;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.ServiceManager.Runner;
using Enterprise.ZArchitecture.Environment;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Runner.Abstractions;

namespace CargoWise.ServiceManager.Runner.Test.Exceptions
{
	class ApplicationExceptionHandlerTest : TestCase
	{
		public void TestHandleFromInitialization_DatabaseUpgradeIsNotLogged()
		{
			// Arrange
			var handler = new ApplicationExceptionHandler(errorReporterProxyMock.Object);
			var mockLogger = new Mock<IRunnerLogger>();
			// Act
			handler.HandleFromInitialization(new DatabaseUpgradedException(), () => mockLogger.Object);
			// Assert
			AssertEquals(0, mockLogger.Invocations.Count);
			errorReporterProxyMock.Verify(x => x.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
			AssertEquals(true, handler.WasDatabaseUpgradeHandledFromInitialization);
		}

		public void TestCannotLoadObjectTypeException_PagingFileIsTooSmall()
		{
			// Arrange
			RegistryItemDictionary.Instance.PurgeAll();

			var handler = new ApplicationExceptionHandler(errorReporterProxyMock.Object);
			var mockLogger = new Mock<IRunnerLogger>();
			var testException = new CannotLoadObjectTypeException("a", new FileLoadException("a"));
			var fileLoadException = testException.InnerException;
			unchecked
			{
				typeof(FileLoadException).GetProperty("HResult", BindingFlags.Public | BindingFlags.Instance).GetSetMethod(true).Invoke(fileLoadException, new object[] { (int)0x800705AF });
			}

			// Act
			handler.HandleFromInitialization(testException, () => mockLogger.Object);

			// Assert
			AssertEquals(1, mockLogger.Invocations.Count);
			errorReporterProxyMock.Verify(x => x.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
		}

		public void TestHandleFromInitialization_SystemException()
		{
			// Arrange
			RegistryItemDictionary.Instance.PurgeAll();

			var handler = new ApplicationExceptionHandler(errorReporterProxyMock.Object);
			var mockLogger = new Mock<IRunnerLogger>();
			var testException = new SystemException("some problem");

			// Act
			handler.HandleFromInitialization(testException, () => mockLogger.Object);

			// Assert
			AssertEquals(0, mockLogger.Invocations.Count);
			errorReporterProxyMock.Verify(x => x.ReportOnce(It.IsAny<string>(), testException), Times.Once);

			AssertEquals(false, handler.WasDatabaseUpgradeHandledFromInitialization);
			AssertEquals(true, Db.IsThreadSchemaVersionCheckDisabled);
		}

		public void TestHandleFromTask_DatabaseUpgrade_SchemaCheckIsDisabled()
		{
			// Arrange
			RegistryItemDictionary.Instance.PurgeAll();

			var bumpedSchemaVersion = ObjectFactory.Get<IDatabaseAspectVersions>().SchemaVersion.Major + 10;
			var versionMock = new Mock<IDatabaseAspectVersions>();
			versionMock
				.Setup(x => x.SchemaVersion)
				.Returns(new VersionLabel(bumpedSchemaVersion, 0));

			using (ObjectFactory.Substitute(versionMock.Object))
			using (var disposable = new DisposableAction(
				() => { Db.ConnectionOverrideForTest = Db.NewExtraConnectionToMainDb(); },
				() =>
				{
					Db.ConnectionOverrideForTest.Dispose();
					Db.ConnectionOverrideForTest = null;
				}))
			{
				// Cause a DatabaseUpgradedException, so the real case can be handled
				Exception actualException = null;
				try
				{
					((IDbReconnectionHandling)Db.Connection).CloseAndReopenConnection();
				}
				catch (Exception ex)
				{
					actualException = ex;
				}

				AssertNotNull(nameof(actualException), actualException);

				var handler = new ApplicationExceptionHandler(errorReporterProxyMock.Object);
				var mockLogger = new Mock<IRunnerLogger>();

				handler.HandleFromTask(actualException, () => mockLogger.Object);

				// Assert
				mockLogger.Verify(x => x.Log(LogLevel.Error, "Unhandled exception in Task Runner", actualException), Times.Once);
				AssertEquals(true, Db.IsThreadSchemaVersionCheckDisabled);
				AssertEquals(1, mockLogger.Invocations.Count);
				errorReporterProxyMock.Verify(x => x.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
				AssertEquals(false, handler.WasDatabaseUpgradeHandledFromInitialization);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Db.EnableThreadSchemaVersionCheckPermanently_ForTest();
			errorReporterProxyMock = new Mock<IErrorReporterProxy>();
		}

		protected override void TearDown()
		{
			Db.EnableThreadSchemaVersionCheckPermanently_ForTest();
			base.TearDown();
		}

		Mock<IErrorReporterProxy> errorReporterProxyMock;
	}
}
