using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Threading;

namespace CargoWise.Common
{
	public delegate void BackgroundDataFileAction(Stream fileStream);

	public interface IBackgroundDataSaver : IDisposable
	{
		void Load(BackgroundDataFileAction loadAction);

		bool DeleteFileOnDispose { get; set; }
	}

	public sealed class BackgroundDataSaver : IBackgroundDataSaver
	{
		readonly string saveFilePath;
		readonly string interimFilePath;
		readonly Timer saveTimer;
		readonly BackgroundDataFileAction saveAction;
		readonly Action<Exception> backgroundExceptionHandler;
		int oneCheckAtATimeLock;

		public BackgroundDataSaver(string saveFilePath, TimeSpan saveFrequency, BackgroundDataFileAction saveAction, Action<Exception> backgroundExceptionHandler)
		{
			this.saveFilePath = Argument.NotNullOrEmpty(saveFilePath, nameof(saveFilePath));
			this.saveAction = saveAction ?? throw new ArgumentNullException(nameof(saveAction));
			this.backgroundExceptionHandler = backgroundExceptionHandler ?? throw new ArgumentNullException(nameof(backgroundExceptionHandler));

			var saveFolderPath = Path.GetDirectoryName(saveFilePath);
			var saveFileName = Path.GetFileNameWithoutExtension(saveFilePath);
			interimFilePath = Path.Combine(saveFolderPath, string.Format(CultureInfo.InvariantCulture, "{0}_Interim.xml", saveFileName)); // Name of internal temporary file
			saveTimer = new Timer(SaveInBackground, null, saveFrequency, saveFrequency);
		}

		public bool DeleteFileOnDispose { get; set; }

		public void Dispose()
		{
			using (var waitHandle = new ManualResetEvent(false))
			{
				saveTimer.Dispose(waitHandle);
				waitHandle.WaitOne();
			}

			if (DeleteFileOnDispose)
			{
				if (File.Exists(saveFilePath))
				{
					File.Delete(saveFilePath);
				}
			}
			else
			{
				Save();
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031", Justification = "Top level exception handler required to insure graceful exit")]
		void SaveInBackground(object stateObj)
		{
			if (Interlocked.Exchange(ref oneCheckAtATimeLock, 1) == 1)
			{
				return;
			}

			try
			{
				Save();
			}
			catch (Exception ex)
			{
				backgroundExceptionHandler(ex);
			}
			finally
			{
				Interlocked.Exchange(ref oneCheckAtATimeLock, 0);
			}
		}

		void Save()
		{
			if (File.Exists(interimFilePath))
			{
				File.Delete(interimFilePath);
			}

			Directory.CreateDirectory(Path.GetDirectoryName(interimFilePath));

			using (var fileStream = File.Create(interimFilePath))
			{
				saveAction(fileStream);
			}

			if (File.Exists(saveFilePath))
			{
				File.Delete(saveFilePath);
			}
			File.Move(interimFilePath, saveFilePath);
		}

		public void Load(BackgroundDataFileAction loadAction)
		{
			if (!File.Exists(saveFilePath))
			{
				return;
			}

			using (var fileStream = File.Open(saveFilePath, FileMode.Open))
			{
				loadAction(fileStream);
			}
		}
	}
}
