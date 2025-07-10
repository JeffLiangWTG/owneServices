using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using CargoWise.Common;
using CargoWise.IO;
using CargoWise.Shared;
using CargoWise.Types;
using Dat.Integration;

namespace Enterprise.Dat.Implementation
{
	sealed class DistributedFileLock : IDisposable
	{
		public DistributedFileLock(string fileToLock, ITaskLogger taskLogger = null)
		{
			logger = taskLogger;
			this.fileToLock = Argument.NotNullOrEmpty(fileToLock, nameof(fileToLock));

			if (!PathValidation.IsValid(fileToLock))
			{
				throw new ArgumentException($"Invalid file path: {fileToLock}", nameof(fileToLock));
			}

			fileLockPath = $"{this.fileToLock}.lock";
			lockOwner = false;
			lockExpireTime = TimeSpan.FromDays(1);
		}

		public bool Locked
		{
			get
			{
				return File.Exists(fileLockPath);
			}
		}

		public bool LockOwner
		{
			get
			{
				lock (lockObject)
				{
					return lockOwner && Locked;
				}
			}
		}

		public bool IsStale
		{
			get
			{
				lock (lockObject)
				{
					if (File.Exists(fileLockPath))
					{
						return File.GetCreationTimeUtc(fileLockPath).Add(lockExpireTime) < ZDateTime.UtcNow;
					}

					return true;
				}
			}
		}

		public bool Acquire()
		{
			lock (lockObject)
			{
				if (isDisposed)
				{
					throw new ObjectDisposedException(nameof(DistributedFileLock));
				}

				if (lockOwner)
				{
					return false;
				}

				try
				{
					if (Locked)
					{
						if (!IsStale)
						{
							return false;
						}

						RemoveLockFile();
					}

					CreateLockFile();

					lockOwner = true;
					return true;
				}
				catch (Exception e)
				{
					logger?.RecordInfo("Error Message:" + "\n" + e);
					logger?.RecordInfo("Stack Trace:" + "\n" + e.StackTrace);
					return false;
				}
			}
		}

		void CreateLockFile()
		{
			var content = $"Machine Name: {System.Environment.MachineName}{System.Environment.NewLine}";
			content += $"Lock Time (UTC): {ZDateTime.UtcNow:O}{System.Environment.NewLine}";

			using (var fs = new FileStream(fileLockPath, FileMode.CreateNew, FileAccess.Write))
			{
				using (var writer = new StreamWriter(fs))
				{
					writer.AutoFlush = true;
					writer.WriteLine(content);
				}
			}
		}

		public bool Acquire(TimeSpan timeout)
		{
			lock (lockObject)
			{
				if (!Directory.Exists(new FileInfo(fileLockPath).DirectoryName))
				{
					return false;
				}

				var acquireStatus = false;
				var timer = Stopwatch.StartNew();

				while (!lockOwner && !acquireStatus && timer.Elapsed < timeout)
				{
					acquireStatus = Acquire();

					if (!acquireStatus)
					{
						Thread.Sleep(50);
					}
				}

				return acquireStatus;
			}
		}

		public bool Release()
		{
			lock (lockObject)
			{
				if (isDisposed)
				{
					throw new ObjectDisposedException(nameof(DistributedFileLock));
				}

				if (!lockOwner)
				{
					return false;
				}

				try
				{
					RemoveLockFile();

					lockOwner = false;
					return true;
				}
				catch (Exception e)
				{
					logger?.RecordInfo("Error Message:" + "\n" + e.ToString());
					logger?.RecordInfo("Stack Trace:" + "\n" + e.StackTrace);
				}

				return false;
			}
		}

		void RemoveLockFile()
		{
			FileIO.DeleteFile(fileLockPath);
		}

		public void Dispose()
		{
			lock (lockObject)
			{
				if (!isDisposed)
				{
					_ = Release();
					isDisposed = true;
				}
			}
		}

		readonly ITaskLogger logger;
		readonly string fileToLock;
		readonly string fileLockPath;
		bool lockOwner;
		readonly TimeSpan lockExpireTime;
		readonly object lockObject = new object();
		bool isDisposed;
	}
}
