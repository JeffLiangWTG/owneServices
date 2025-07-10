using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AppDomainWrappers.Net.Test
{
	public class HeartbeatTests : TestCase
	{
		const string HOST_LOCK_FILE_NAME = "host.lock";
		const string CLIENT_LOCK_FILE_NAME = "client.lock";
		TemporaryWorkspace tempDirectory;

		protected override void SetUp()
		{
			tempDirectory = new TemporaryWorkspace();
			base.SetUp();
		}

		protected override void TearDown()
		{
			tempDirectory.Dispose();
		}

		#region InitialiseMyLockFile
		public void TestInitialiseMyLockFile_NullFilePath_ThrowsArgumentNullException()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatServer(HOST_LOCK_FILE_NAME);

			// Act & Assert
			AssertExceptionThrown<ArgumentNullException>(() => heartbeat.InitialiseMyLockFile(null));
		}

		public void TestInitialiseMyLockFile_InvalidFilePathEmpty_ThrowsArgumentException()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatServer(HOST_LOCK_FILE_NAME);

			// Act & Assert
			AssertExceptionThrown<ArgumentException>(() => heartbeat.InitialiseMyLockFile(""));
		}

		public void TestInitialiseMyLockFile_InvalidFilePathWhiteSpace_ThrowsArgumentException()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatServer(HOST_LOCK_FILE_NAME);

			// Act & Assert
			AssertExceptionThrown<ArgumentException>(() => heartbeat.InitialiseMyLockFile("    "));
		}

		public void TestInitialiseMyLockFile_NonExistantDirectoryPath_ThrowsDirectoryNotFoundException()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatServer(HOST_LOCK_FILE_NAME);
			var directoryPath = @"C:\SomeDirectoryWhichDoesntExist\5ADC1850-E4CB-4D34-855D-CB89C257392A";

			// Act & Assert
			AssertExceptionThrown<DirectoryNotFoundException>(() => heartbeat.InitialiseMyLockFile(directoryPath));
		}

		public void TestInitialiseMyLockFile_UserDoesntHavePermissions_ThrowsUnauthorizedAccessException()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatServer(HOST_LOCK_FILE_NAME);
			var lockedDirectoryPath = Path.Combine(tempDirectory.RootDirectory, "locked");
			_ = Directory.CreateDirectory(lockedDirectoryPath);

			// Temporarily grant the required permissions for the test
			var directorySecurity = new DirectorySecurity();
			directorySecurity.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, AccessControlType.Deny));
			Directory.SetAccessControl(lockedDirectoryPath, directorySecurity);

			// Act & Assert
			AssertExceptionThrown<UnauthorizedAccessException>(() => heartbeat.InitialiseMyLockFile(lockedDirectoryPath));

			// Teardown - Reset permissions after the test
			directorySecurity = new DirectorySecurity();
			directorySecurity.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.ReadAndExecute, AccessControlType.Allow));
			Directory.SetAccessControl(lockedDirectoryPath, directorySecurity);
		}

		public void TestInitialiseMyLockFile_FileAlreadyExistsAndOverwriteIsFalse_ThrowsIOException()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatServer(HOST_LOCK_FILE_NAME);
			var filePath = Path.Combine(tempDirectory.RootDirectory, heartbeat.GetFileName());
			File.Create(filePath).Close();

			// Act & Assert
			AssertEquals(File.Exists(filePath), actual: true);    // File already exists.
			AssertExceptionThrown<IOException>(() => heartbeat.InitialiseMyLockFile(tempDirectory.RootDirectory));
		}

		public void TestInitialiseMyLockFile_FileAlreadyExistsAndOverwriteIsTrue_ReturnsCreatedFilesPath()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatServer(HOST_LOCK_FILE_NAME);
			var filePath = Path.Combine(tempDirectory.RootDirectory, heartbeat.GetFileName());
			File.Create(filePath).Close();

			// Act
			string actual = null;
			AssertNoExceptionThrown(() => actual = heartbeat.InitialiseMyLockFile(tempDirectory.RootDirectory, overwrite: true));

			// Assert
			AssertNotNull(actual);
			AssertEquals(actual, filePath);
			AssertEquals(File.Exists(actual), actual: true);
		}

		public void TestInitialiseMyLockFile_CorrectFilePath_ReturnsCreatedFilesPath()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatServer(HOST_LOCK_FILE_NAME);

			// Act
			string actual = null;
			AssertNoExceptionThrown(() => actual = heartbeat.InitialiseMyLockFile(tempDirectory.RootDirectory));

			// Assert
			AssertNotNull(actual);
			AssertEquals(File.Exists(actual), actual: true);
		}
		#endregion

		#region DoLockMyLockFile
		public void TestDoLockMyLockFile_AttemptLockWithoutInitialise_ThrowsUninitialisedLockFileException()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatServer(HOST_LOCK_FILE_NAME);

			// Act & Assert
			AssertExceptionThrown<UninitialisedLockFileException>(() => heartbeat.DoLockMyLockFile());
		}

		public void TestDoLockMyLockFile_LockFileSuccess_FileIsLockedAndInaccessible()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatServer(HOST_LOCK_FILE_NAME);
			var resetEventSlim = new ManualResetEventSlim(false);
			var filePath = heartbeat.InitialiseMyLockFile(tempDirectory.RootDirectory);
			CancellationTokenSource cancellationTokenSource = null;
			var isFileLocked = false;

			// Act
			try
			{
				cancellationTokenSource = heartbeat.DoLockMyLockFile();
				isFileLocked = HeartbeatServer.IsFileLocked(filePath);
			}
			finally
			{
				// Cleanup
				heartbeat.HostStateChanged += (s, e) =>
				{
					if (e.State == LockFileState.Disconnected)
					{
						resetEventSlim.Set();
					}
				};
				cancellationTokenSource?.Cancel();
				resetEventSlim.Wait(TimeSpan.FromSeconds(5));
			}

			// Assert
			AssertNotNull(filePath);
			AssertEquals(File.Exists(filePath), actual: true);
			AssertEquals(isFileLocked, actual: true);
			AssertNotNull(cancellationTokenSource);
		}

		public void TestDoLockMyLockFile_LockFileCanBeCancelled_FileIsLockedAndThenUnlocked()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatServer(HOST_LOCK_FILE_NAME);
			var resetEventSlim = new ManualResetEventSlim(false);
			var filePath = heartbeat.InitialiseMyLockFile(tempDirectory.RootDirectory);
			var cancellationTokenSource = heartbeat.DoLockMyLockFile();
			heartbeat.HostStateChanged += (s, e) =>
			{
				if (e.State == LockFileState.Disconnected)
				{
					resetEventSlim.Set();
				}
			};
			var isFileLockedBeforeCancellation = HeartbeatServer.IsFileLocked(filePath);
			bool isFileLockedAfterCancellation;

			// Act
			cancellationTokenSource.Cancel();
			resetEventSlim.Wait(TimeSpan.FromSeconds(5));
			isFileLockedAfterCancellation = HeartbeatServer.IsFileLocked(filePath);

			// Assert
			AssertNotNull(filePath);
			AssertEquals(File.Exists(filePath), actual: true);
			AssertEquals(isFileLockedBeforeCancellation, actual: true);
			AssertEquals(isFileLockedAfterCancellation, actual: false);
			AssertNotNull(cancellationTokenSource);
		}
		#endregion

		#region DoCancelMyLockFile
		public void TestDoCancelMyLockFile_UninitialisedLockFile_ThrowsUninitialisedLockFileException()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatServer(HOST_LOCK_FILE_NAME);

			// Act & Assert
			AssertExceptionThrown<UninitialisedLockFileException>(() => heartbeat.DoCancelMyLockFile());
		}

		public void TestDoCancelMyLockFile_AttemptToCancelLockOnLockFileWhichIsNotLocked_ReturnsFalse()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatServer(HOST_LOCK_FILE_NAME);
			_ = heartbeat.InitialiseMyLockFile(tempDirectory.RootDirectory);

			// Act
			var cancelResult = heartbeat.DoCancelMyLockFile();

			// Assert
			AssertEquals(cancelResult, actual: false);
		}

		public void TestDoCancelMyLockFile_CorrectlyCancelLockFileOperation_ReturnsTrue()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatServer(HOST_LOCK_FILE_NAME);
			var filePath = heartbeat.InitialiseMyLockFile(tempDirectory.RootDirectory);
			var cancellationTokenSource = heartbeat.DoLockMyLockFile();
			var heartBeatWasActiveBefore = HeartbeatServer.IsFileLocked(filePath);

			// Act
			var wasCancelSuccessful = heartbeat.DoCancelMyLockFile();
			var heartBeatWasActiveAfter = HeartbeatServer.IsFileLocked(filePath);

			// Assert
			AssertEquals(File.Exists(filePath), actual: true);
			AssertNotNull(cancellationTokenSource);
			AssertEquals(heartBeatWasActiveBefore, actual: true);
			AssertEquals(wasCancelSuccessful, actual: true);
			AssertEquals(heartBeatWasActiveAfter, actual: false);
		}
		#endregion

		#region DoMonitorLockFile
		public void TestDoMonitorLockFile_NullFilePath_ThrowsArgumentNullException()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatServer(HOST_LOCK_FILE_NAME);

			// Act & Assert
			AssertExceptionThrown<ArgumentNullException>(() => heartbeat.DoMonitorLockFile(null, waitForConnection: false));
		}

		public void TestDoMonitorLockFile_EmptyFilePathEmpty_ThrowsArgumentException()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatServer(HOST_LOCK_FILE_NAME);

			// Act & Assert
			AssertExceptionThrown<ArgumentException>(() => heartbeat.DoMonitorLockFile("", waitForConnection: false));
		}

		public void TestDoMonitorLockFile_EmptyFilePathWhiteSpace_ThrowsArgumentException()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatServer(HOST_LOCK_FILE_NAME);

			// Act & Assert
			AssertExceptionThrown<ArgumentException>(() => heartbeat.DoMonitorLockFile("    ", waitForConnection: false));
		}

		public void TestDoMonitorLockFile_FileDoesNotExistNoWait_ThrowsFileNotFoundException()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatServer(HOST_LOCK_FILE_NAME);

			// Act & Assert
			AssertExceptionThrown<FileNotFoundException>(
				() => heartbeat.DoMonitorLockFile(
					Path.Combine(
						tempDirectory.RootDirectory,
						"62AF2611-BF55-43B8-88B4-6669C33F1274",
						heartbeat.GetFileName()),
					waitForConnection: false));
		}

		public void TestDoMonitorLockFile_NonLockFileNoExtension_ThrowsArgumentOutOfRangeException()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatServer(HOST_LOCK_FILE_NAME);

			// Act & Assert
			AssertExceptionThrown<ArgumentOutOfRangeException>(
				() => heartbeat.DoMonitorLockFile(
					Path.Combine(
						tempDirectory.RootDirectory,
						"server"),
					waitForConnection: false));
		}

		public void TestDoMonitorLockFile_NonLockFileExeExtension_ThrowsArgumentOutOfRangeException()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatServer(HOST_LOCK_FILE_NAME);

			// Act & Assert
			AssertExceptionThrown<ArgumentOutOfRangeException>(
				() => heartbeat.DoMonitorLockFile(
					Path.Combine(
						tempDirectory.RootDirectory,
						"server.exe"),
					waitForConnection: false));
		}

		public void TestDoMonitorLockFile_NonLockFileTxtExtension_ThrowsArgumentOutOfRangeException()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatServer(HOST_LOCK_FILE_NAME);

			// Act & Assert
			AssertExceptionThrown<ArgumentOutOfRangeException>(
				() => heartbeat.DoMonitorLockFile(
					Path.Combine(
						tempDirectory.RootDirectory,
						"server.txt"),
					waitForConnection: false));
		}

		public void TestDoMonitorLockFile_NonLockFileDoubleExtension_ThrowsArgumentOutOfRangeException()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatServer(HOST_LOCK_FILE_NAME);

			// Act & Assert
			AssertExceptionThrown<ArgumentOutOfRangeException>(
				() => heartbeat.DoMonitorLockFile(
					Path.Combine(
						tempDirectory.RootDirectory,
						"server.lock.txt"),
					waitForConnection: false));
		}

		public void TestDoMonitorLockFile_FilePresentButNotLocked_SetsStateToDisconnectedImmediately()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatServer(HOST_LOCK_FILE_NAME);
			var fileName = CLIENT_LOCK_FILE_NAME;
			var resetSlim = new ManualResetEventSlim(false);
			File.Create(Path.Combine(tempDirectory.RootDirectory, fileName)).Close();
			var allStates = new HashSet<LockFileState>();
			heartbeat.ClientStateChanged += LockFileChangedEvent;

			// Act
			try
			{
				_ = heartbeat.DoMonitorLockFile(
						Path.Combine(tempDirectory.RootDirectory, fileName),
						waitForConnection: false);

				resetSlim.Wait(TimeSpan.FromSeconds(5));
			}
			finally
			{
				heartbeat.ClientStateChanged -= LockFileChangedEvent;
			}

			// Assert
			AssertCollectionContains(LockFileState.Disconnected, allStates);
			AssertCollectionNotContains(LockFileState.Failure, allStates);
			AssertEquals(LockFileState.Disconnected, allStates.ElementAt(1));

			void LockFileChangedEvent(object sender, LockFileStateChangedEventArgs e)
			{
				allStates.Add(e.State);

				if (e.State == LockFileState.Disconnected)
				{
					resetSlim.Set();
				}
			}
		}

		public void TestDoMonitorLockFile_MonitorClientStateWithNoWait_TransitionsFromConnectedToDisconnected()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatServer(HOST_LOCK_FILE_NAME);
			var filePath = Path.Combine(tempDirectory.RootDirectory, CLIENT_LOCK_FILE_NAME);
			File.Create(filePath).Close();
			var cancellationTokenSource = new CancellationTokenSource();
			var clientStateChanges = new HashSet<LockFileState>();
			heartbeat.ClientStateChanged += LockFileChangedEvent;
			CancellationTokenSource monitorCancellationTokenSource = null;

			LockFile(filePath, cancellationTokenSource, TimeSpan.FromSeconds(1));

			// Act
			try
			{
				monitorCancellationTokenSource = heartbeat.DoMonitorLockFile(filePath, waitForConnection: false);

				Thread.Sleep(TimeSpan.FromSeconds(1.5));    // Give monitor a moment to run.

				cancellationTokenSource.Cancel();

				WaitForCancellationCompletion(cancellationTokenSource, TimeSpan.FromSeconds(5));
			}
			finally
			{
				// Cleanup
				monitorCancellationTokenSource?.Cancel();
				WaitForCancellationCompletion(monitorCancellationTokenSource, TimeSpan.FromSeconds(5));
				heartbeat.ClientStateChanged -= LockFileChangedEvent;
			}

			// Assert
			AssertEquals(clientStateChanges.Count, 2);
			AssertEquals(clientStateChanges.ElementAt(0), LockFileState.Connected);
			AssertEquals(clientStateChanges.ElementAt(1), LockFileState.Disconnected);

			void LockFileChangedEvent(object sender, LockFileStateChangedEventArgs e) => clientStateChanges.Add(e.State);
		}

		public void TestDoMonitorLockFile_MonitorClientStateWaitForConnectionWaitTooLong_TransitionsFromWaitingToFailure()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatServer(HOST_LOCK_FILE_NAME);
			var resetEventSlim = new ManualResetEventSlim(false);
			var filePath = Path.Combine(tempDirectory.RootDirectory, CLIENT_LOCK_FILE_NAME);
			var cancellationTokenSource = new CancellationTokenSource();
			var clientStateChanges = new HashSet<LockFileState>();
			heartbeat.ClientStateChanged += LockFileChangedEvent;
			CancellationTokenSource monitorCancellationTokenSource = null;

			// Act
			try
			{
				monitorCancellationTokenSource = heartbeat.DoMonitorLockFile(filePath, waitForConnection: true);

				resetEventSlim.Wait(TimeSpan.FromSeconds(7));
			}
			finally
			{
				heartbeat.ClientStateChanged -= LockFileChangedEvent;
			}

			// Assert
			AssertEquals(2, clientStateChanges.Count);
			AssertEquals(clientStateChanges.ElementAt(0), LockFileState.Waiting);
			AssertEquals(clientStateChanges.ElementAt(1), LockFileState.Failure);

			void LockFileChangedEvent(object sender, LockFileStateChangedEventArgs e)
			{
				clientStateChanges.Add(e.State);
				if (e.State == LockFileState.Failure)
				{
					resetEventSlim.Set();
				}
			}
		}

		public void TestDoMonitorLockFile_MonitorClientStateWaitForConnectionCancelledEarly_TransitionsFromWaitingToDisconnected()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatServer(HOST_LOCK_FILE_NAME);
			var filePath = Path.Combine(tempDirectory.RootDirectory, CLIENT_LOCK_FILE_NAME);
			var cancellationTokenSource = new CancellationTokenSource();
			var clientStateChanges = new HashSet<LockFileState>();
			heartbeat.ClientStateChanged += LockFileChangedEvent;
			CancellationTokenSource monitorCancellationTokenSource = null;

			// Act
			try
			{
				monitorCancellationTokenSource = heartbeat.DoMonitorLockFile(filePath, waitForConnection: true);

				Thread.Sleep(TimeSpan.FromSeconds(1));

				monitorCancellationTokenSource.Cancel();

				Thread.Sleep(TimeSpan.FromSeconds(2));
			}
			finally
			{
				heartbeat.ClientStateChanged -= LockFileChangedEvent;
			}

			// Assert
			AssertEquals(clientStateChanges.Count, 2);
			AssertEquals(clientStateChanges.ElementAt(0), LockFileState.Waiting);
			AssertEquals(clientStateChanges.ElementAt(1), LockFileState.Disconnected);

			void LockFileChangedEvent(object sender, LockFileStateChangedEventArgs e) => clientStateChanges.Add(e.State);
		}

		public void TestDoMonitorLockFile_MonitorClientStateWithWaitForConnection_TransitionsFromWaitingToConnectedToDisconnected()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatServer(HOST_LOCK_FILE_NAME);
			var filePath = Path.Combine(tempDirectory.RootDirectory, CLIENT_LOCK_FILE_NAME);
			var cancellationTokenSource = new CancellationTokenSource();
			var clientStateChanges = new HashSet<LockFileState>();
			heartbeat.ClientStateChanged += LockFileChangedEvent;
			CancellationTokenSource monitorCancellationTokenSource = null;

			// Act
			try
			{
				monitorCancellationTokenSource = heartbeat.DoMonitorLockFile(filePath, waitForConnection: true);

				Thread.Sleep(TimeSpan.FromSeconds(1));    // Give monitor a moment to wait for the file.

				File.Create(filePath).Close();

				LockFile(filePath, cancellationTokenSource, TimeSpan.FromSeconds(1));

				Thread.Sleep(TimeSpan.FromSeconds(1));  // Give monitor a moment to run after finding the file.

				cancellationTokenSource.Cancel();   // Cancel client file lock.

				WaitForCancellationCompletion(cancellationTokenSource, TimeSpan.FromSeconds(5));

				Thread.Sleep(TimeSpan.FromSeconds(1.5));  // Give monitor a moment to cancel.
			}
			finally
			{
				// Cleanup
				monitorCancellationTokenSource?.Cancel();
				WaitForCancellationCompletion(monitorCancellationTokenSource, TimeSpan.FromSeconds(5));
				heartbeat.ClientStateChanged -= LockFileChangedEvent;
			}

			// Assert
			AssertEquals(clientStateChanges.Count, 3);
			AssertEquals(clientStateChanges.ElementAt(0), LockFileState.Waiting);
			AssertEquals(clientStateChanges.ElementAt(1), LockFileState.Connected);
			AssertEquals(clientStateChanges.ElementAt(2), LockFileState.Disconnected);

			void LockFileChangedEvent(object sender, LockFileStateChangedEventArgs e) => clientStateChanges.Add(e.State);
		}
		#endregion

		#region DoCancelLockFileMonitor
		public void TestDoCancelLockFileMonitor_AttemptToCancelMonitorWithoutLockingFileFirst_ThrowsMonitoredLockFileException()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatServer(HOST_LOCK_FILE_NAME);

			// Act & Assert
			AssertExceptionThrown<MonitoredLockFileException>(() => heartbeat.DoCancelLockFileMonitor());
		}

		public void TestDoCancelLockFileMonitor_AttemptToCancelLockOnLockFileWhichIsNotLockedAndNotWaiting_ReturnsFalse()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatServer(HOST_LOCK_FILE_NAME);
			var filePath = Path.Combine(tempDirectory.RootDirectory, CLIENT_LOCK_FILE_NAME);
			File.CreateText(filePath).Close();
			_ = heartbeat.DoMonitorLockFile(filePath, waitForConnection: false);

			// Act
			var cancelResult = heartbeat.DoCancelLockFileMonitor();

			// Assert
			AssertEquals(cancelResult, actual: false);
		}

		public void TestDoCancelLockFileMonitor_CorrectlyCancelMonitorOperation_ReturnsTrue()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatServer(HOST_LOCK_FILE_NAME);
			var filePath = Path.Combine(tempDirectory.RootDirectory, CLIENT_LOCK_FILE_NAME);
			File.CreateText(filePath).Close();
			var lockFileState = LockFileState.None;
			var lockFileStateBeforeCancel = LockFileState.None;
			var lockFileStateAfterCancel = LockFileState.None;
			heartbeat.ClientStateChanged += LockFileChangedEvent;
			var lockFileCancellationToken = new CancellationTokenSource();
			LockFile(filePath, lockFileCancellationToken, TimeSpan.FromSeconds(1));
			var cancellationTokenSource = heartbeat.DoMonitorLockFile(filePath, waitForConnection: false);
			var wasCancelOperationSuccessful = false;

			try
			{
				// Act
				wasCancelOperationSuccessful = heartbeat.DoCancelLockFileMonitor();
				lockFileStateAfterCancel = lockFileState;
			}
			finally
			{
				// Cleanup
				lockFileCancellationToken.Cancel();
				WaitForCancellationCompletion(lockFileCancellationToken, TimeSpan.FromSeconds(5));
				heartbeat.ClientStateChanged -= LockFileChangedEvent;
			}

			// Assert
			AssertEquals(File.Exists(filePath), actual: true);
			AssertNotNull(cancellationTokenSource);
			AssertEquals(lockFileStateBeforeCancel, LockFileState.Connected);
			AssertEquals(wasCancelOperationSuccessful, actual: true);
			AssertEquals(lockFileStateAfterCancel, LockFileState.Disconnected);

			// Changed Event
			void LockFileChangedEvent(object sender, LockFileStateChangedEventArgs e)
			{
				if (e.State == LockFileState.Connected && lockFileState == LockFileState.None)
				{
					lockFileStateBeforeCancel = LockFileState.Connected;
				}
				lockFileState = e.State;
			}
		}
		#endregion

		#region GetFileName
		public void TestGetFileName_WhenCalled_ReturnsConstFileName()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatServer(HOST_LOCK_FILE_NAME);

			// Act
			var fileName = heartbeat.GetFileName();

			// Assert
			AssertNotNull(fileName);
			AssertEquals(fileName, HOST_LOCK_FILE_NAME);
		}
		#endregion

		#region IsFileLocked
		public void TestIsFileLocked_NullFilePath_ThrowsArgumentNullException()
		{
			// Arrange, Act, & Assert
			AssertExceptionThrown<ArgumentNullException>(() => HeartbeatServer.IsFileLocked(null));
		}

		public void TestIsFileLocked_EmptyFilePath_ThrowsArgumentNullException()
		{
			// Arrange, Act, & Assert
			AssertExceptionThrown<ArgumentException>(() => HeartbeatServer.IsFileLocked(""));
		}

		public void TestIsFileLocked_WhiteSpaceFilePath_ThrowsArgumentNullException()
		{
			// Arrange, Act, & Assert
			AssertExceptionThrown<ArgumentException>(() => HeartbeatServer.IsFileLocked("    "));
		}

		public void TestIsFileLocked_FileDoesNotExist_ReturnsFalse()
		{
			// Arrange
			var filePath = Path.Combine(tempDirectory.RootDirectory, CLIENT_LOCK_FILE_NAME);

			// Act
			var isFileLockedActual = HeartbeatServer.IsFileLocked(filePath);

			// Assert
			AssertEquals(isFileLockedActual, actual: false);
		}

		public void TestIsFileLocked_FileExistsButNotLocked_ReturnsFalse()
		{
			// Arrange
			var filePath = Path.Combine(tempDirectory.RootDirectory, CLIENT_LOCK_FILE_NAME);
			File.Create(filePath).Close();

			// Act
			var isFileLockedActual = HeartbeatServer.IsFileLocked(filePath);

			// Assert
			AssertEquals(isFileLockedActual, actual: false);
		}

		public void TestIsFileLocked_FileExistsAndIsLocked_ReturnsTrue()
		{
			// Arrange
			var isFileLockedActual = false;
			var filePath = Path.Combine(tempDirectory.RootDirectory, CLIENT_LOCK_FILE_NAME);
			File.Create(filePath).Close();
			var cancellationTokenSource = new CancellationTokenSource();
			LockFile(filePath, cancellationTokenSource, TimeSpan.FromSeconds(1));

			// Act
			try
			{
				isFileLockedActual = HeartbeatServer.IsFileLocked(filePath);
			}
			finally
			{
				cancellationTokenSource.Cancel();
				WaitForCancellationCompletion(cancellationTokenSource, TimeSpan.FromSeconds(5));
			}

			// Assert
			AssertEquals(isFileLockedActual, actual: true);
		}
		#endregion

		#region Utility
		void LockFile(string filePath,
			CancellationTokenSource cancellationTokenSource,
			TimeSpan waitTimeBetweenCancellationChecks)
		{
			var fileLockedEvent = new ManualResetEventSlim(false);
			_ = Task.Factory.StartNew(() =>
			{
				using var fileStream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
				fileLockedEvent.Set();

				while (!cancellationTokenSource.Token.IsCancellationRequested)
				{
					Thread.Sleep(waitTimeBetweenCancellationChecks);
				}
			});
			fileLockedEvent.Wait();
		}

		void WaitForCancellationCompletion(CancellationTokenSource tokenSource, TimeSpan maxWait)
		{
			if (tokenSource.Token.WaitHandle.WaitOne(maxWait))
			{
				// The cancellation request is checked every second. This sleep helps avoid race conditions.
				Thread.Sleep(TimeSpan.FromSeconds(1.2));
			}
			else
			{
				Fail($"Timed-out after {maxWait.Seconds} seconds waiting for cancellation.");
				return;
			}
		}
		#endregion
	}
}
