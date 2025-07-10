using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.Core;

namespace Enterprise.ZArchitecture.Core
{
	public class TempFileCleanup
	{
		public void CleanTempFolder()
		{
			try
			{
				string baseTempDirectoryDirectory = new DirectoryInfo(Temp.TempPathWithoutCreating).Parent.FullName;
				if (Directory.Exists(baseTempDirectoryDirectory))
				{
					DeleteContentsRecursively(new DirectoryInfo(baseTempDirectoryDirectory), 0);
				}
			}
			catch (IOException) { }
			catch (UnauthorizedAccessException) { }
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("TempFileCleanupTask.CleanTempFolder", "Error when cleaning up " + Constants.ProductName + " temp files", ex);
			}
		}

		void DeleteContentsRecursively(DirectoryInfo directory, int level)
		{
			int pid;
			try
			{
				if (level != 1 || (int.TryParse(directory.Name, out pid) && !ActiveProcessIDs.Any(id => id == pid)))
				{
					foreach (var subDirectory in directory.GetDirectories())
					{
						DeleteContentsRecursively(subDirectory, level + 1);
					}
					foreach (var file in directory.GetFiles())
					{
						try
						{
							if ((DateTime.UtcNow - file.LastWriteTimeUtc) >= TimeSpan.FromMinutes(2))
							{
								File.SetAttributes(file.FullName, FileAttributes.Normal);
								file.Delete();
							}
						}
						catch (IOException)
						{
						}
						catch (UnauthorizedAccessException)
						{
						}
					}
					if (level > 0)
					{
						try
						{
							if ((DateTime.UtcNow - directory.LastWriteTimeUtc) >= TimeSpan.FromMinutes(2))
							{
								directory.Delete();
							}
						}
						catch (IOException)
						{
						}
						catch (UnauthorizedAccessException)
						{
						}
					}
				}
			}
			catch (TargetInvocationException ex)
			{
				Win32Exception win32Ex = ex.InnerException as Win32Exception;
				if (win32Ex != null)
				{
					if (win32Ex.NativeErrorCode == 8)  // ERROR_NOT_ENOUGH_MEMORY
					{
						return;
					}
				}
				throw;
			}
		}

		int[] ActiveProcessIDs
		{
			get
			{
				return activeProcessIDs ?? (activeProcessIDs = ProcessLocator.Instance.GetProcessIDs());
			}
		}
		int[] activeProcessIDs;
	}
}

// Tested by Enterprise.Startup.Testing.TempFileCleanupTaskTest
