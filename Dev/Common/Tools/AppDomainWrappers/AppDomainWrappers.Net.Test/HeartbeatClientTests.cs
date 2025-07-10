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
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Justification = "Keeping test structure agnostic, so to make them similar to both Client and System tests.")]
	public class HeartbeatClientTests : TestCase
	{
		const string HOST_LOCK_FILE_NAME = "server.lock";
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

		#region GetFileName

		public void TestGetFileName_WhenCalled_ReturnsConstFileName()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatClient(CLIENT_LOCK_FILE_NAME);

			// Act
			var fileName = heartbeat.GetFileName();

			// Assert
			AssertNotNull(fileName);
			AssertEquals(fileName, CLIENT_LOCK_FILE_NAME);
		}
		#endregion

		#region InitialiseMyLockFile
		public void TestInitialiseMyLockFile_NullFilePath_ThrowsArgumentNullException()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatClient(CLIENT_LOCK_FILE_NAME);

			// Act & Assert
			AssertExceptionThrown<ArgumentNullException>(() => heartbeat.InitialiseMyLockFile(null));
		}

		public void TestInitialiseMyLockFile_InvalidFilePathEmpty_ThrowsArgumentException()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatClient(CLIENT_LOCK_FILE_NAME);

			// Act & Assert
			AssertExceptionThrown<ArgumentException>(() => heartbeat.InitialiseMyLockFile(""));
		}

		public void TestInitialiseMyLockFile_InvalidFilePathWhiteSpace_ThrowsArgumentException()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatClient(CLIENT_LOCK_FILE_NAME);

			// Act & Assert
			AssertExceptionThrown<ArgumentException>(() => heartbeat.InitialiseMyLockFile("    "));
		}

		public void TestInitialiseMyLockFile_NonExistantDirectoryPath_ThrowsDirectoryNotFoundException()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatClient(CLIENT_LOCK_FILE_NAME);
			var directoryPath = @"C:\SomeDirectoryWhichDoesntExist\5ADC1850-E4CB-4D34-855D-CB89C257392A";

			// Act & Assert
			AssertExceptionThrown<DirectoryNotFoundException>(() => heartbeat.InitialiseMyLockFile(directoryPath));
		}

		public void TestInitialiseMyLockFile_UserDoesntHavePermissions_ThrowsUnauthorizedAccessException()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatClient(CLIENT_LOCK_FILE_NAME);
			var lockedDirectoryPath = Path.Combine(tempDirectory.RootDirectory, "locked");
			_ = Directory.CreateDirectory(lockedDirectoryPath);

			// Temporarily apply the required permissions for the test
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
			IHeartbeat heartbeat = new HeartbeatClient(CLIENT_LOCK_FILE_NAME);
			var filePath = Path.Combine(tempDirectory.RootDirectory, heartbeat.GetFileName());
			File.Create(filePath).Close();

			// Act & Assert
			AssertEquals(File.Exists(filePath), actual: true);    // File already exists.
			AssertExceptionThrown<IOException>(() => heartbeat.InitialiseMyLockFile(tempDirectory.RootDirectory));
		}

		public void TestInitialiseMyLockFile_FileAlreadyExistsAndOverwriteIsTrue_ReturnsCreatedFilesPath()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatClient(CLIENT_LOCK_FILE_NAME);
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
			IHeartbeat heartbeat = new HeartbeatClient(CLIENT_LOCK_FILE_NAME);

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
			IHeartbeat heartbeat = new HeartbeatClient(CLIENT_LOCK_FILE_NAME);

			// Act & Assert
			AssertExceptionThrown<UninitialisedLockFileException>(() => heartbeat.DoLockMyLockFile());
		}

		public void TestDoLockMyLockFile_LockFileSuccess_FileIsLockedAndInaccessible()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatClient(CLIENT_LOCK_FILE_NAME);
			var resetEventSlim = new ManualResetEventSlim(false);
			var filePath = heartbeat.InitialiseMyLockFile(tempDirectory.RootDirectory);
			CancellationTokenSource cancellationTokenSource = null;
			var isFileLocked = false;
			heartbeat.HostStateChanged += (s, e) =>
			{
				if (e.State == LockFileState.Disconnected)
				{
					resetEventSlim.Set();
				}
			};

			// Act
			try
			{
				cancellationTokenSource = heartbeat.DoLockMyLockFile();
				isFileLocked = HeartbeatClient.IsFileLocked(filePath);
			}
			finally
			{
				// Cleanup
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
			IHeartbeat heartbeat = new HeartbeatClient(CLIENT_LOCK_FILE_NAME);
			var resetEventSlim = new ManualResetEventSlim(false);
			var filePath = heartbeat.InitialiseMyLockFile(tempDirectory.RootDirectory);
			var cancellationTokenSource = heartbeat.DoLockMyLockFile();
			var isFileLockedBeforeCancellation = HeartbeatClient.IsFileLocked(filePath);
			bool isFileLockedAfterCancellation;
			heartbeat.HostStateChanged += (s, e) =>
			{
				if (e.State == LockFileState.Disconnected)
				{
					resetEventSlim.Set();
				}
			};

			// Act
			cancellationTokenSource.Cancel();
			resetEventSlim.Wait(TimeSpan.FromSeconds(5));
			isFileLockedAfterCancellation = HeartbeatClient.IsFileLocked(filePath);

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
			IHeartbeat heartbeat = new HeartbeatClient(CLIENT_LOCK_FILE_NAME);

			// Act & Assert
			AssertExceptionThrown<UninitialisedLockFileException>(() => heartbeat.DoCancelMyLockFile());
		}

		public void TestDoCancelMyLockFile_AttemptToCancelLockOnLockFileWhichIsNotLocked_ReturnsFalse()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatClient(CLIENT_LOCK_FILE_NAME);
			_ = heartbeat.InitialiseMyLockFile(tempDirectory.RootDirectory);

			// Act
			var cancelResult = heartbeat.DoCancelMyLockFile();

			// Assert
			AssertEquals(cancelResult, actual: false);
		}

		public void TestDoCancelMyLockFile_CorrectlyCancelLockFileOperation_ReturnsTrue()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatClient(CLIENT_LOCK_FILE_NAME);
			var filePath = heartbeat.InitialiseMyLockFile(tempDirectory.RootDirectory);
			var cancellationTokenSource = heartbeat.DoLockMyLockFile();
			var heartBeatWasActiveBefore = HeartbeatClient.IsFileLocked(filePath);

			// Act
			var wasCancelSuccessful = heartbeat.DoCancelMyLockFile();
			var heartBeatWasActiveAfter = HeartbeatClient.IsFileLocked(filePath);

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
			IHeartbeat heartbeat = new HeartbeatClient(CLIENT_LOCK_FILE_NAME);

			// Act & Assert
			AssertExceptionThrown<ArgumentNullException>(() => heartbeat.DoMonitorLockFile(null, waitForConnection: false));
		}

		public void TestDoMonitorLockFile_EmptyFilePath_ThrowsArgumentException()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatClient(CLIENT_LOCK_FILE_NAME);

			// Act & Assert
			AssertExceptionThrown<ArgumentException>(() => heartbeat.DoMonitorLockFile("", waitForConnection: false));
		}

		public void TestDoMonitorLockFile_WhiteSpaceFilePath_ThrowsArgumentException()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatClient(CLIENT_LOCK_FILE_NAME);

			// Act & Assert
			AssertExceptionThrown<ArgumentException>(() => heartbeat.DoMonitorLockFile("    ", waitForConnection: false));
		}

		public void TestDoMonitorLockFile_FileDoesNotExistNoWait_ThrowsFileNotFoundException()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatClient(CLIENT_LOCK_FILE_NAME);

			// Act & Assert
			AssertExceptionThrown<FileNotFoundException>(
				() => heartbeat.DoMonitorLockFile(
					Path.Combine(
						tempDirectory.RootDirectory,
						"62AF2611-BF55-43B8-88B4-6669C33F1274",
						heartbeat.GetFileName()),
					waitForConnection: false));
		}

		public void TestDoMonitorLockFile_NonLockFile_ThrowsArgumentOutOfRangeException()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatClient(CLIENT_LOCK_FILE_NAME);

			// Act & Assert
			AssertExceptionThrown<ArgumentOutOfRangeException>(
				() => heartbeat.DoMonitorLockFile(
					Path.Combine(
						tempDirectory.RootDirectory,
						"client"),
					waitForConnection: false));
		}

		public void TestDoMonitorLockFile_NonLockFileAlternateCommonExtension_ThrowsArgumentOutOfRangeException()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatClient(CLIENT_LOCK_FILE_NAME);

			// Act & Assert
			AssertExceptionThrown<ArgumentOutOfRangeException>(
				() => heartbeat.DoMonitorLockFile(
					Path.Combine(
						tempDirectory.RootDirectory,
						"client.exe"),
					waitForConnection: false));
		}

		public void TestDoMonitorLockFile_NonLockFileAlternateExtension_ThrowsArgumentOutOfRangeException()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatClient(CLIENT_LOCK_FILE_NAME);

			// Act & Assert
			AssertExceptionThrown<ArgumentOutOfRangeException>(
				() => heartbeat.DoMonitorLockFile(
					Path.Combine(
						tempDirectory.RootDirectory,
						"client.txt"),
					waitForConnection: false));
		}

		public void TestDoMonitorLockFile_NonLockFileMultipleExtensions_ThrowsArgumentOutOfRangeException()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatClient(CLIENT_LOCK_FILE_NAME);

			// Act & Assert
			AssertExceptionThrown<ArgumentOutOfRangeException>(
				() => heartbeat.DoMonitorLockFile(
					Path.Combine(
						tempDirectory.RootDirectory,
						"client.lock.txt"),
					waitForConnection: false));
		}

		public void TestDoMonitorLockFile_FilePresentButNotLocked_SetsStateToDisconnectedImmediately()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatClient(CLIENT_LOCK_FILE_NAME);
			var resetEventSlim = new ManualResetEventSlim(false);
			var fileName = HOST_LOCK_FILE_NAME;
			File.Create(Path.Combine(tempDirectory.RootDirectory, fileName)).Close();
			var hostLockStates = new HashSet<LockFileState>();
			heartbeat.HostStateChanged += LockFileChangedEvent;

			// Act
			try
			{
				_ = heartbeat.DoMonitorLockFile(
						Path.Combine(tempDirectory.RootDirectory, fileName),
						waitForConnection: false);

				resetEventSlim.Wait(TimeSpan.FromSeconds(5));
			}
			finally
			{
				heartbeat.HostStateChanged -= LockFileChangedEvent;
			}

			// Assert
			AssertCollectionContains(LockFileState.Disconnected, hostLockStates);
			AssertEquals(LockFileState.Disconnected, hostLockStates.ElementAt(1));

			void LockFileChangedEvent(object sender, LockFileStateChangedEventArgs e)
			{
				hostLockStates.Add(e.State);

				if (e.State == LockFileState.Disconnected)
				{
					resetEventSlim.Set();
				}
			}
		}

		public void TestDoMonitorLockFile_MonitorClientStateWithNoWait_TransitionsFromConnectedToDisconnected()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatClient(CLIENT_LOCK_FILE_NAME);
			var filePath = Path.Combine(tempDirectory.RootDirectory, HOST_LOCK_FILE_NAME);
			File.Create(filePath).Close();
			var cancellationTokenSource = new CancellationTokenSource();
			var hostStateChanges = new HashSet<LockFileState>();
			heartbeat.HostStateChanged += LockFileChangedEvent;
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
				heartbeat.HostStateChanged -= LockFileChangedEvent;
			}

			// Assert
			AssertEquals(hostStateChanges.Count, 2);
			AssertEquals(hostStateChanges.ElementAt(0), LockFileState.Connected);
			AssertEquals(hostStateChanges.ElementAt(1), LockFileState.Disconnected);

			void LockFileChangedEvent(object sender, LockFileStateChangedEventArgs e) => hostStateChanges.Add(e.State);
		}

		public void TestDoMonitorLockFile_MonitorClientStateWaitForConnectionWaitTooLong_TransitionsFromWaitingToFailure()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatClient(CLIENT_LOCK_FILE_NAME);
			var resetEventSlim = new ManualResetEventSlim(false);
			var filePath = Path.Combine(tempDirectory.RootDirectory, HOST_LOCK_FILE_NAME);
			var cancellationTokenSource = new CancellationTokenSource();
			var hostStateChanges = new HashSet<LockFileState>();
			heartbeat.HostStateChanged += LockFileChangedEvent;
			CancellationTokenSource monitorCancellationTokenSource = null;

			// Act
			try
			{
				monitorCancellationTokenSource = heartbeat.DoMonitorLockFile(filePath, waitForConnection: true);

				resetEventSlim.Wait(TimeSpan.FromSeconds(7));
			}
			finally
			{
				heartbeat.HostStateChanged -= LockFileChangedEvent;
			}

			// Assert
			AssertEquals(2, hostStateChanges.Count);
			AssertEquals(hostStateChanges.ElementAt(0), LockFileState.Waiting);
			AssertEquals(hostStateChanges.ElementAt(1), LockFileState.Failure);

			void LockFileChangedEvent(object sender, LockFileStateChangedEventArgs e)
			{
				hostStateChanges.Add(e.State);

				if (e.State == LockFileState.Disconnected)
				{
					resetEventSlim.Set();
				}
			}
		}

		public void TestDoMonitorLockFile_MonitorClientStateWaitForConnectionCancelledEarly_TransitionsFromWaitingToDisconnected()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatClient(CLIENT_LOCK_FILE_NAME);
			var filePath = Path.Combine(tempDirectory.RootDirectory, HOST_LOCK_FILE_NAME);
			var cancellationTokenSource = new CancellationTokenSource();
			var hostStateChanges = new HashSet<LockFileState>();
			var resetEventSlim = new ManualResetEventSlim(false);
			heartbeat.HostStateChanged += LockFileChangedEvent;
			CancellationTokenSource monitorCancellationTokenSource = null;

			// Act
			try
			{
				monitorCancellationTokenSource = heartbeat.DoMonitorLockFile(filePath, waitForConnection: true);

				Thread.Sleep(TimeSpan.FromSeconds(1));

				monitorCancellationTokenSource.Cancel();

				resetEventSlim.Wait();
			}
			finally
			{
				heartbeat.HostStateChanged -= LockFileChangedEvent;
			}

			// Assert
			AssertEquals(hostStateChanges.Count, 2);
			AssertEquals(hostStateChanges.ElementAt(0), LockFileState.Waiting);
			AssertEquals(hostStateChanges.ElementAt(1), LockFileState.Disconnected);

			void LockFileChangedEvent(object sender, LockFileStateChangedEventArgs e)
			{
				hostStateChanges.Add(e.State);

				if ((e.State == LockFileState.Disconnected || e.State == LockFileState.Failure)
					&& !resetEventSlim.IsSet)
				{
					resetEventSlim.Set();
				}
			}
		}

		public void TestDoMonitorLockFile_MonitorClientStateWithWaitForConnection_TransitionsFromWaitingToConnectedToDisconnected()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatClient(CLIENT_LOCK_FILE_NAME);
			var filePath = Path.Combine(tempDirectory.RootDirectory, HOST_LOCK_FILE_NAME);
			var cancellationTokenSource = new CancellationTokenSource();
			var hostStateChanges = new HashSet<LockFileState>();
			heartbeat.HostStateChanged += LockFileChangedEvent;
			CancellationTokenSource monitorCancellationTokenSource = null;

			// Act
			try
			{
				monitorCancellationTokenSource = heartbeat.DoMonitorLockFile(filePath, waitForConnection: true);

				Thread.Sleep(TimeSpan.FromSeconds(1));    // Give monitor a moment to wait for the file.

				File.Create(filePath).Close();

				LockFile(filePath, cancellationTokenSource, TimeSpan.FromSeconds(1));

				Thread.Sleep(TimeSpan.FromSeconds(1));  // Give monitor a moment to run after finding the file.

				cancellationTokenSource.Cancel();   // Cancel host file lock.

				WaitForCancellationCompletion(cancellationTokenSource, TimeSpan.FromSeconds(5));

				Thread.Sleep(TimeSpan.FromSeconds(1.5));  // Give monitor a moment to cancel.
			}
			finally
			{
				// Cleanup
				monitorCancellationTokenSource?.Cancel();
				WaitForCancellationCompletion(monitorCancellationTokenSource, TimeSpan.FromSeconds(5));
				heartbeat.HostStateChanged -= LockFileChangedEvent;
			}

			// Assert
			AssertEquals(hostStateChanges.Count, 3);
			AssertEquals(hostStateChanges.ElementAt(0), LockFileState.Waiting);
			AssertEquals(hostStateChanges.ElementAt(1), LockFileState.Connected);
			AssertEquals(hostStateChanges.ElementAt(2), LockFileState.Disconnected);

			void LockFileChangedEvent(object sender, LockFileStateChangedEventArgs e) => hostStateChanges.Add(e.State);
		}
		#endregion

		#region DoCancelLockFileMonitor
		public void TestDoCancelLockFileMonitor_AttemptToCancelMonitorWithoutLockingFileFirst_ThrowsMonitoredLockFileException()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatClient(CLIENT_LOCK_FILE_NAME);

			// Act & Assert
			AssertExceptionThrown<MonitoredLockFileException>(() => heartbeat.DoCancelLockFileMonitor());
		}

		public void TestDoCancelLockFileMonitor_AttemptToCancelLockOnLockFileWhichIsNotLockedAndNotWaiting_ReturnsFalse()
		{
			// Arrange
			IHeartbeat heartbeat = new HeartbeatClient(CLIENT_LOCK_FILE_NAME);
			var filePath = Path.Combine(tempDirectory.RootDirectory, HOST_LOCK_FILE_NAME);
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
			IHeartbeat heartbeat = new HeartbeatClient(CLIENT_LOCK_FILE_NAME);
			var filePath = Path.Combine(tempDirectory.RootDirectory, HOST_LOCK_FILE_NAME);
			File.CreateText(filePath).Close();
			var lockFileState = LockFileState.None;
			var lockFileStateBeforeCancel = LockFileState.None;
			var lockFileStateAfterCancel = LockFileState.None;
			heartbeat.HostStateChanged += LockFileChangedEvent;
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
				heartbeat.HostStateChanged -= LockFileChangedEvent;
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

		#region IsFileLocked
		public void TestIsFileLocked_NullFilePath_ThrowsArgumentNullException()
		{
			// Arrange, Act, & Assert
			AssertExceptionThrown<ArgumentNullException>(() => HeartbeatClient.IsFileLocked(null));
		}

		public void TestIsFileLocked_EmptyFilePath_ThrowsArgumentNullException()
		{
			// Arrange, Act, & Assert
			AssertExceptionThrown<ArgumentException>(() => HeartbeatClient.IsFileLocked(""));
		}

		public void TestIsFileLocked_WhiteSpaceFilePath_ThrowsArgumentNullException()
		{
			// Arrange, Act, & Assert
			AssertExceptionThrown<ArgumentException>(() => HeartbeatClient.IsFileLocked("    "));
		}

		public void TestIsFileLocked_FileDoesNotExist_ReturnsFalse()
		{
			// Arrange
			var filePath = Path.Combine(tempDirectory.RootDirectory, CLIENT_LOCK_FILE_NAME);

			// Act
			var isFileLockedActual = HeartbeatClient.IsFileLocked(filePath);

			// Assert
			AssertEquals(isFileLockedActual, actual: false);
		}

		public void TestIsFileLocked_FileExistsButNotLocked_ReturnsFalse()
		{
			// Arrange
			var filePath = Path.Combine(tempDirectory.RootDirectory, CLIENT_LOCK_FILE_NAME);
			File.Create(filePath).Close();

			// Act
			var isFileLockedActual = HeartbeatClient.IsFileLocked(filePath);

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
				isFileLockedActual = HeartbeatClient.IsFileLocked(filePath);
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
