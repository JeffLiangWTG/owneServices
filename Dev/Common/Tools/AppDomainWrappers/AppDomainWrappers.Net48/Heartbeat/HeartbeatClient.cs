using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AppDomainWrappers.Net48
{
	public class HeartbeatClient : IHeartbeat
	{
		const uint HOST_FILE_CHECK_INTERVAL = 3;
		const uint CLIENT_FILE_CHECK_INTERVAL = 1;
		const float WAIT_FACTOR = 2.1f;
		readonly string _fileName;

		string _hostFilePath = string.Empty;
		string _clientFilePath = string.Empty;
		CancellationTokenSource _hostCancellationTokenSource;
		CancellationTokenSource _clientCancellationTokenSource;

		public HeartbeatClient(string fileName)
		{
			_fileName = fileName;
		}

		public string InitialiseMyLockFile(string directoryPath, bool overwrite = false)
		{
			if (directoryPath == null)
			{
				throw new ArgumentNullException(nameof(directoryPath));
			}
			else if (string.IsNullOrWhiteSpace(directoryPath))
			{
				throw new ArgumentException("The provided path was empty.", nameof(directoryPath));
			}

			if (!Directory.Exists(directoryPath))
			{
				throw new DirectoryNotFoundException();
			}

			// File path to use.
			var filePath = Path.Combine(directoryPath, GetFileName());

			if (!overwrite && File.Exists(filePath))
			{
				throw new IOException($"Unable to create lock file because it already exists.{Environment.NewLine}File Path: '{filePath}'.");
			}

			try
			{
				// Create the lock file.
				File.Create(filePath).Close();
			}
			catch (IOException)
			{
				// Pass this along if any other IO exception occurs.
				throw;
			}

			if (!File.Exists(filePath))
			{
				throw new FileNotFoundException("Attempted to create lock file, but failed.", filePath);
			}

			_clientFilePath = filePath;

			return _clientFilePath;
		}

		public bool DoCancelMyLockFile()
		{
			if (string.IsNullOrWhiteSpace(_clientFilePath)
				|| !File.Exists(_clientFilePath))
			{
				throw new UninitialisedLockFileException(nameof(_clientFilePath));
			}

			if (_clientCancellationTokenSource == null
				|| !IsFileLocked(_clientFilePath))
			{
				return false;
			}

			if (_clientCancellationTokenSource.IsCancellationRequested)
			{
				return true;    // Don't send request again, but let user know that it has been cancelled.
			}

			_clientCancellationTokenSource.Cancel();

			return DoWaitForConnectionDisconnect(
				_clientCancellationTokenSource,
				TimeSpan.FromSeconds(CLIENT_FILE_CHECK_INTERVAL * WAIT_FACTOR),
				throwExceptionOnTimeout: false);
		}

		public string GetFileName()
		{
			return _fileName;
		}

		public CancellationTokenSource DoLockMyLockFile()
		{
			if (string.IsNullOrWhiteSpace(_clientFilePath) || !File.Exists(_clientFilePath))
			{
				throw new UninitialisedLockFileException(nameof(_clientFilePath));
			}

			var fileLockedEvent = new ManualResetEventSlim(false);
			_clientCancellationTokenSource = new CancellationTokenSource();

			// Start a separate thread to lock the file indefinitely
			_ = Task.Factory.StartNew(() =>
			{
				try
				{
					// Lock the file using File.Open with FileShare.None (prevents access via user or code).
					using var fileStream = File.Open(_clientFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

					SetState(ClientStateChanged, LockFileState.Connected, _clientCancellationTokenSource);

					// File is now locked
					fileLockedEvent.Set();

					// Periodically check for cancellation.
					while (!_clientCancellationTokenSource.Token.IsCancellationRequested)
					{
						Thread.Sleep(TimeSpan.FromSeconds(CLIENT_FILE_CHECK_INTERVAL));
					}

					SetState(ClientStateChanged, LockFileState.Disconnected, _clientCancellationTokenSource);
				}
				catch (IOException)
				{
					SetState(ClientStateChanged, LockFileState.Failure, _clientCancellationTokenSource);
				}
			});

			// Wait for the file to be locked before returning.
			fileLockedEvent.Wait();

			return _clientCancellationTokenSource;
		}

		public CancellationTokenSource DoMonitorLockFile(string lockFilePath, bool waitForConnection)
		{
			if (lockFilePath == null)
			{
				throw new ArgumentNullException(nameof(lockFilePath));
			}

			if (string.IsNullOrWhiteSpace(lockFilePath))
			{
				throw new ArgumentException(null, nameof(lockFilePath));
			}

			if (Path.GetExtension(lockFilePath) != ".lock")
			{
				throw new ArgumentOutOfRangeException(nameof(lockFilePath), "Provided file is not a lock file.");
			}

			if (!waitForConnection && !File.Exists(lockFilePath))
			{
				throw new FileNotFoundException("Unable to find provided filepath.", lockFilePath);
			}

			_hostFilePath = lockFilePath;
			_hostCancellationTokenSource = new CancellationTokenSource();
			var fileLockedEvent = new ManualResetEventSlim(false);

			_ = Task.Factory.StartNew(() =>
			{
				if (waitForConnection)
				{
					SetState(HostStateChanged, LockFileState.Waiting, _hostCancellationTokenSource);

					var initialisationMaxWaitTime = TimeSpan.FromSeconds(5);
					var currentWaitTime = TimeSpan.Zero;

					fileLockedEvent.Set();
					while (!File.Exists(lockFilePath))
					{
						Thread.Sleep(TimeSpan.FromSeconds(1));
						currentWaitTime = currentWaitTime.Add(TimeSpan.FromSeconds(1));

						if (_hostCancellationTokenSource.Token.IsCancellationRequested)
						{
							SetState(HostStateChanged, LockFileState.Disconnected, _hostCancellationTokenSource);
							return; // Can't monitor if cancelled.
						}
						else if (initialisationMaxWaitTime <= currentWaitTime)
						{
							SetState(HostStateChanged, LockFileState.Failure, _hostCancellationTokenSource);
							return; // Stop file monitoring.
						}
					}
				}
				else
				{
					fileLockedEvent.Set();
				}

				SetState(HostStateChanged, LockFileState.Connected, _hostCancellationTokenSource);

				// Start a separate thread to monitor the lock file indefinitely
				// Periodically check for cancellation.
				while (!_hostCancellationTokenSource.Token.IsCancellationRequested
					&& IsFileLocked(lockFilePath))
				{
					Thread.Sleep(TimeSpan.FromSeconds(HOST_FILE_CHECK_INTERVAL));
				}

				SetState(HostStateChanged, LockFileState.Disconnected, _hostCancellationTokenSource);
			});

			fileLockedEvent.Wait();
			return _hostCancellationTokenSource;
		}

		public bool DoCancelLockFileMonitor()
		{
			if (string.IsNullOrWhiteSpace(_hostFilePath)
				|| !File.Exists(_hostFilePath))
			{
				throw new MonitoredLockFileException($"Attempt to cancel the monitored lock file process before it has been started. Have you ran the {nameof(DoMonitorLockFile)} operation?");
			}

			if (_hostCancellationTokenSource == null
				|| !IsFileLocked(_hostFilePath))
			{
				return false;
			}

			if (_hostCancellationTokenSource.IsCancellationRequested)
			{
				return true;    // Don't send request again, but let user know that it has been cancelled.
			}

			_hostCancellationTokenSource.Cancel();

			return DoWaitForConnectionDisconnect(
				_hostCancellationTokenSource,
				TimeSpan.FromSeconds(HOST_FILE_CHECK_INTERVAL * WAIT_FACTOR),
				throwExceptionOnTimeout: false);
		}

		#region Events
		void SetState(EventHandler<LockFileStateChangedEventArgs> handler, LockFileState newState, CancellationTokenSource cancellationTokenSource)
		{
			handler?.Invoke(this, new LockFileStateChangedEventArgs
			{
				State = newState,
				CancellationTokenSource = cancellationTokenSource
			});
		}

		public event EventHandler<LockFileStateChangedEventArgs> HostStateChanged;
		public event EventHandler<LockFileStateChangedEventArgs> ClientStateChanged;
		#endregion

		#region Utility
		public static bool IsFileLocked(string filePath)
		{
			if (filePath == null)
			{
				throw new ArgumentNullException(nameof(filePath));
			}

			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException("Provided file path was empty.", nameof(filePath));
			}

			try
			{
				if (File.Exists(filePath))
				{
					using var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
					// if the file is successfully opened, it means the file is not locked.
				}
			}
			catch (IOException)
			{
				// If caught, then the file is currently locked, and thus the process is alive.
				return true;
			}

			return false;
		}

		bool DoWaitForConnectionDisconnect(CancellationTokenSource tokenSource,
			TimeSpan maxWait,
			bool throwExceptionOnTimeout = true)
		{
			if (tokenSource.Token.WaitHandle.WaitOne(maxWait))
			{
				Thread.Sleep(maxWait);  // Wait this long again, to ensure watch loop has ended.
				return true;
			}
			else
			{
				if (throwExceptionOnTimeout)
				{
					throw new TimeoutException($"Timed-out after {maxWait.Seconds} seconds waiting for cancellation.");
				}
				return false;
			}
		}
		#endregion
	}
}
