using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.IO;
using CargoWise.Shared;
using CargoWise.Types;
using Dat.Integration;

namespace Enterprise.Dat.Implementation
{
	public class CacheHelper
	{
		public CacheHelper(string cacheServerName, string cacheSubDirectory, ITaskLogger taskLogger = null)
		{
			rawCacheServerName = Argument.NotNullOrEmpty(cacheServerName, nameof(cacheServerName));
			this.cacheSubDirectory = Argument.NotNullOrEmpty(cacheSubDirectory, nameof(cacheSubDirectory));
			this.taskLogger = taskLogger;
		}

		public delegate bool VerifyDelegate(string cachedFilePath);

		public virtual VerifyDelegate Verify { get; set; }

		public virtual string GetFile(string networkFilePath, TimeSpan timeout = default)
		{
			_ = Argument.NotNullOrEmpty(networkFilePath, nameof(networkFilePath));

			if (timeout == default)
			{
				timeout = DefaultTimeout;
			}

			string cacheFilePath = null;
			try
			{
				var cts = new CancellationTokenSource(timeout);

				cacheFilePath = GetCacheFilePath(networkFilePath);

				using (var cacheFileLock = new DistributedFileLock(cacheFilePath, taskLogger))
				{
					if (!cacheFileLock.Acquire(timeout))
					{
						return null;
					}

					using (taskLogger?.RecordTask("Copying file to cache location"))
					{
						var copyTask = Task.Run(() => CopyFile(networkFilePath, cacheFilePath), cts.Token);

						copyTask.Wait(cts.Token);
					}

					if (Verify == null)
					{
						return cacheFilePath;
					}

					using (taskLogger?.RecordTask("Verifying cached file"))
					{
						if (Verify.Invoke(cacheFilePath))
						{
							return cacheFilePath;
						}

						TryDeleteFile(cacheFilePath);
						return null;
					}
				}
			}
			catch (Exception e)
			{
				taskLogger?.RecordInfo($"Cannot get cache file: {e.Message}");
				TryDeleteFile(cacheFilePath);
				return null;
			}
		}

		public virtual void CleanupOldCacheFiles()
		{
			using (taskLogger?.RecordTask("Cleaning old files"))
			{
				try
				{
					foreach (var file in Directory.GetFiles(CacheDirectory))
					{
						var fileName = Path.GetFileName(file);
						if (fileName.StartsWith("CW", StringComparison.OrdinalIgnoreCase)
							&& File.GetLastAccessTimeUtc(file) < ZDateTime.UtcNow.Add(TimeSpan.FromDays(-7)))
						{
							TryDeleteFile(file);
						}
					}
				}
				catch (Exception e)
				{
					taskLogger?.RecordInfo($"Cleanup failed: {e.Message}");
				}
			}
		}

		public virtual void TryDeleteFile(string filepath)
		{
			using (var cacheFileLock = new DistributedFileLock(filepath, taskLogger))
			{
				if (!cacheFileLock.Acquire())
				{
					return;
				}

				try
				{
					if (File.Exists(filepath))
					{
						FileIO.DeleteFile(filepath);
					}
				}
				catch (Exception e)
				{
					taskLogger?.RecordInfo($"Delete file failed: {e.Message}");
				}
			}
		}

		internal virtual string CacheServerName => cacheServerName ?? (cacheServerName = CleanupServerName(rawCacheServerName));

		internal virtual string CacheDirectory => cacheDirectory ?? (cacheDirectory = GetCacheDirectory());

		internal virtual string GetCacheFilePath(string networkFilePath)
		{
			const string filenamePrefix = "CW";
			var filename = Path.GetFileName(networkFilePath);
			if (!filename.StartsWith(filenamePrefix, StringComparison.OrdinalIgnoreCase))
			{
				filename = $"{filenamePrefix}{filename}";
			}

			return Path.Combine(CacheDirectory, filename);
		}

		internal virtual string CleanupServerName(string rawServerName)
		{
			return rawServerName.Trim('\\').Split('\\')[0];
		}

		protected virtual void CopyFile(string networkFilePath, string cacheFilePath)
		{
			var cacheFileExists = File.Exists(cacheFilePath);

			if (cacheFileExists)
			{
				if (File.GetLastWriteTime(networkFilePath) <= File.GetLastWriteTime(cacheFilePath))
				{
					taskLogger?.RecordInfo("Using existing cache");
					return;
				}

				var attributes = File.GetAttributes(cacheFilePath);
				if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
				{
					File.SetAttributes(cacheFilePath, attributes & ~FileAttributes.ReadOnly);
				}
			}

			var tempLocation = Temp.GetTempFileName();
			try
			{
				File.Copy(networkFilePath, tempLocation, overwrite: true);
				File.Delete(cacheFilePath);
				File.Move(tempLocation, cacheFilePath);

				var identity = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
				var accessRule =
					new FileSystemAccessRule(identity, FileSystemRights.FullControl, AccessControlType.Allow);
				var accessControl = File.GetAccessControl(cacheFilePath);
				accessControl.AddAccessRule(accessRule);
				File.SetAccessControl(cacheFilePath, accessControl);
			}
			catch (Exception e)
			{
				taskLogger?.RecordInfo($"CopyFile failed: {e.Message}");
			}
			finally
			{
				if (File.Exists(tempLocation))
				{
					File.Delete(tempLocation);
				}
			}
		}

		protected virtual TimeSpan DefaultTimeout => TimeSpan.FromMinutes(1);

		string GetCacheDirectory()
		{
			return $@"\\{CacheServerName}\{cacheSubDirectory}";
		}

		readonly string rawCacheServerName;
		string cacheServerName;
		string cacheDirectory;
		readonly string cacheSubDirectory;
		readonly ITaskLogger taskLogger;
	}
}
