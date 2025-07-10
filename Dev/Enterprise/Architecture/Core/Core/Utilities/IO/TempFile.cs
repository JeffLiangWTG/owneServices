using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Core
{
	public class TempFile : Disposable
	{
		#region Construction

		public static Stream CreateWithDeleteOnClose()
		{
			return CreateWithDeleteOnClose(FileOptions.None);
		}

		public static Stream CreateWithDeleteOnClose(FileOptions fileOptions)
		{
			var fileName = EnvProxy.Instance.GetTempFileName();
			fileOptions |= FileOptions.DeleteOnClose;
			return new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, 0x1000, fileOptions);
		}

		public static TempFile New()
		{
			return new TempFile(GetNewTempFileName());
		}

		public static TempFile NewInDirectory(string directoryName)
		{
			return new TempFile(GetNewTempFileName(directoryName));
		}

		/// <summary>
		/// Copies the contents of FileToCopyFrom to the temp file, and sets file attributes to Normal.
		/// </summary>
		public static TempFile NewFromFile(string fileToCopyFrom, bool keepOriginalExtension)
		{
			TempFile tempFile = TempFile.NewWithExtension(Path.GetExtension(fileToCopyFrom));
			File.Copy(fileToCopyFrom, tempFile.Filename, true);
			File.SetAttributes(tempFile.Filename, FileAttributes.Normal);

			return tempFile;
		}

		/// <summary>
		/// Copies the contents of FileToCopyFrom to the temp file and sets file attributes to Normal.
		/// </summary>
		public static TempFile NewFromFile(string fileToCopyFrom)
		{
			return NewFromFile(fileToCopyFrom, false);
		}

		public static TempFile NewWithExtension(string extension)
		{
			return new TempFile(Temp.GetTempFileNameWithExtension(extension));
		}

		public static TempFile New(string directory, string extension)
		{
			return new TempFile(GetNewTempFileName(directory, extension));
		}

		protected TempFile(string filename)
		{
			Filename = filename;
		}

		protected static string GetNewTempFileName()
		{
			return EnvProxy.Instance.GetTempFileName();
		}

		protected static string GetNewTempFileName(string directoryName)
		{
			return EnvProxy.Instance.GetTempFileName(directoryName);
		}

		protected static string GetNewTempFileNameWithExtension(string extension)
		{
			return Temp.GetTempFileNameWithExtension(extension);
		}

		protected static string GetNewTempFileName(string directory, string extension)
		{
			return EnvProxy.Instance.GetTempFileName(directory, extension);
		}

		#endregion

		public readonly string Filename;

		public override string ToString()
		{
			return Filename;
		}

		~TempFile()
		{
			Dispose(false);
		}

		protected override void Dispose(bool isDisposing)
		{
			DeleteIfExists();
		}

		protected virtual void DeleteIfExists()
		{
			if (File.Exists(Filename))
			{
				Delete();
			}
		}

		public const int FileIsInUseByAnotherProcess = -2147024864;

		protected void Delete()
		{
			Delete(Filename);
		}

		public static void Delete(string filename)
		{
			string s;
			TryDelete(filename, out s);
		}

		public static void Delete(string filename, bool reportException)
		{
			string s;
			TryDelete(filename, out s, reportException);
		}

		public static bool TryDeleteHandleAllExceptions(string filename, out string message)
		{
			return TryDelete(filename, out message, false, true);
		}

		public static bool TryDeleteHandleAllExceptions(string filename)
		{
			string dontNeedNoMessage;
			return TryDelete(filename, out dontNeedNoMessage, false, true);
		}

		public static bool TryDelete(string filename, out string message)
		{
			return TryDelete(filename, out message, true);
		}

		public static bool TryDelete(string filename, out string message, bool reportException)
		{
			return TryDelete(filename, out message, reportException, false);
		}

#if DEBUG
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static bool ThrowUnauthorizedAccessExceptionOnDelete;
#endif
		public static bool TryDelete(string filename, out string message, bool reportException, bool handleAllExceptions)
		{
			bool result = false;
			int attemptCount = 25;
			message = string.Empty;

			if (!string.IsNullOrEmpty(filename))
			{
				do
				{
					try
					{
#if DEBUG
						if (Globals.IsTest && ThrowUnauthorizedAccessExceptionOnDelete)
						{
							throw new UnauthorizedAccessException();
						}
#endif
						File.SetAttributes(filename, FileAttributes.Normal);
						File.Delete(filename);
						attemptCount = 0;
						result = true;
					}
					catch (FileNotFoundException) // No file to delete
					{
						attemptCount = 0;
						result = true;
					}
					catch (IOException ex)
					{
						if (Marshal.GetHRForException(ex) == FileIsInUseByAnotherProcess)
						{
							attemptCount--;
							if (reportException && attemptCount == 0)
							{
								ReportDeleteError(filename, ex);
							}

							Thread.Sleep(100);
							message = ex.Message;
						}
						else if (!File.Exists(filename)) // No file to delete
						{
							attemptCount = 0;
							result = true;
						}
						else
						{
							if (!handleAllExceptions)
							{
								throw;
							}
							if (reportException)
							{
								ReportDeleteError(filename, ex);
							}

							attemptCount = 0;
							message = ex.Message;
						}
					}
					catch (UnauthorizedAccessException ex)
					{
						attemptCount--;

						if (!handleAllExceptions)
						{
							throw;
						}

						if (attemptCount == 0)
						{
							if (reportException)
							{
								ErrorReporter.ReportOnce("Unable to delete file: Filename=" + filename, ex);
							}
							else
							{
								result = true; // If do not need to report exception, just ignore it.
							}
						}

						Thread.Sleep(100);
						message = ex.Message;
					}
					catch (Exception ex) when (!ex.IsCriticalException() && handleAllExceptions)
					{
						if (reportException)
						{
							ErrorReporter.ReportOnce("Unable to delete file: Filename=" + filename, ex);
						}

						attemptCount = 0;
						message = ex.Message;
					}
				}
				while (attemptCount > 0);
			}
			return result;
		}

		[System.Diagnostics.Conditional("DEBUG")]
		static void ReportDeleteError(string filename, IOException ex)
		{
			try
			{
				System.Threading.Thread.Sleep(5000);
				File.Delete(filename);
			}
			catch (IOException)
			{
				ErrorReporter.ReportOnce("Unable to delete file: Filename=" + filename, ex);
			}
		}
	}

	/// <summary>
	/// Attempts to delete the temp file on another thread once per second for 10 seconds.
	/// If after 10 seconds the file could not be deleted, the file remains undeleted and the thread dies.
	/// </summary>
	public class TempFileWithDelayedDelete : TempFile
	{
		#region Construction

		public static new TempFileWithDelayedDelete New()
		{
			return new TempFileWithDelayedDelete(GetNewTempFileName());
		}

		public static new TempFileWithDelayedDelete NewInDirectory(string directoryName)
		{
			return new TempFileWithDelayedDelete(GetNewTempFileName(directoryName));
		}

		public static new TempFileWithDelayedDelete NewWithExtension(string extension)
		{
			return new TempFileWithDelayedDelete(GetNewTempFileNameWithExtension(extension));
		}

		public static new TempFileWithDelayedDelete New(string directory, string extension)
		{
			return new TempFileWithDelayedDelete(GetNewTempFileName(directory, extension));
		}

		public static TempFileWithDelayedDelete NewWithFilename(string filename)
		{
			return new TempFileWithDelayedDelete(filename);
		}

		/// <summary>
		/// Create a new TempFile that, on deletion, waits the specified delay before the physical file is deleted.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public static TempFileWithDelayedDelete NewWithFilename(string filename, int delayInMilliseconds)
		{
			TempFileWithDelayedDelete newTempFile = new TempFileWithDelayedDelete(filename);
			newTempFile.invokeDelay = delayInMilliseconds;
			return newTempFile;
		}

		protected TempFileWithDelayedDelete(string filename)
			: base(filename)
		{
#if DEBUG
			if (Globals.IsTest)
			{
				Enterprise.ZArchitecture.Core.Testing.TempFilesTestListener.Instance.FilesWithDelayedDelete.Add(filename);
			}
#endif
		}

		#endregion

		protected override void DeleteIfExists()
		{
			StartDeleteTimer();
		}

		void StartDeleteTimer()
		{
			const int MillisecondTickInterval = 1000;
			TimerCallback timerDelegate = new TimerCallback(DeleteOnAnotherThread);

			lock (lockObj) // timer can tick before assigning to timer field
			{
				timer = new Timer(timerDelegate, this, invokeDelay, MillisecondTickInterval);
			}
		}

		void DeleteOnAnotherThread(object stateInfo)
		{
			lock (lockObj)
			{
				try
				{
					#region Testing Only
#if DEBUG
					if (Globals.IsTest && ThrowNonIOExceptionOnDelete)
					{
						throw new Exception();
					}
#endif
					#endregion

					bool unableToDeleteWithin15Seconds = (attempsToDelete >= Timeout); // in case 2 threads increment at the same time (== Timeout + 1)
					if (unableToDeleteWithin15Seconds)
					{
						timer.Dispose();

						#region Testing Only
#if DEBUG
						if (Globals.IsTest)
						{
							TimerDisposedAfter15Seconds = true;
						}
#endif
						#endregion
					}
					else
					{
						attempsToDelete++;
						File.SetAttributes(Filename, FileAttributes.Normal);
						File.Delete(Filename);
						timer.Dispose(); // if no exception was thrown we have successfully deleted any temp file
					}
				}
				catch (FileNotFoundException)
				{
					timer.Dispose(); // No file to delete
				}
				catch (IOException) // try again later..
				{
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					timer.Dispose(); // something bad happened, just exit..
				}
			}
		}

		#region Testing Only
#if DEBUG
		internal bool TimerDisposedAfter15Seconds;
		internal bool ThrowNonIOExceptionOnDelete;

		internal class TimeoutOverride : Disposable
		{
			public TimeoutOverride()
			{
				Timeout = 3;
			}

			protected override void Dispose(bool isDisposing)
			{
				if (isDisposing)
				{
					Timeout = TimeoutDefault;
				}
			}
		}

		[CargoWise.Common.Testing.SuppressThreadStaticFieldMessage]
		static int Timeout = TimeoutDefault;
		const int TimeoutDefault = 15;
#else
		const int Timeout = 15;
#endif
		#endregion

		int invokeDelay = 1000;
		readonly object lockObj = new object();
		int attempsToDelete;
		Timer timer;
	}
}
