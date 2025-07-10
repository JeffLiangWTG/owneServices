//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.DataImport
{
	using System;
	using System.ComponentModel;
	using System.IO;
	using CargoWise.Common;
	using CargoWise.EntityFramework;
	using CargoWise.Types;

	public abstract class DataImporter
	{
		protected DataImporter()
		{
			FactoryProvider = new BusinessObjectFactoryProvider();
		}

		protected void ProcessSafe(StreamReader reader, string onStartMessage, Action process)
		{
			FireOnImportStart(reader, onStartMessage);

			try
			{
				process();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				FireOnFatalError(ex);
				FireOnProgress();
			}

			FireOnImportFinished();
		}

		protected void SaveFactory()
		{
			FactoryProvider.SaveCurrentAndCreateNew();
			countOfSavings++;
			FireOnProgress();
		}

		protected bool SaveRequired
		{
			get { return LineNumber / RecordsPerFactory >= countOfSavings || LineNumber >= linesCount || Canceled; }
		}

		#region Events

		#region OnImportStart

		protected virtual void FireOnImportStart(StreamReader reader, string message)
		{
			StreamReader = reader;
			actualStart = ZDateTime.Now;
			LineNumber = 0;
			InvalidLines = 0;
			countOfSavings = 1;
			Canceled = false;
			linesCount = GetLinesCount();
			Success = true;
			FireOnProgress();
			FireNotificationDelegate(OnImportStart, string.Format("{0}, {1}\r\n\r\n", actualStart.ToLongTimeString(), message));
		}

		int GetLinesCount()
		{
			var count = 0;
			while (StreamReader.ReadLine() != null)
			{
				count++;
			}

			StreamReader.BaseStream.Position = 0;
			StreamReader.DiscardBufferedData();
			return count;
		}

		#endregion

		#region OnImportFinished

		void FireOnImportFinished()
		{
			if (Canceled)
			{
				FireOnImportFinished(Res.GetString("63bc8d0a-c9c2-476d-9fa3-589b8a9a8b6a", "Import has been canceled. Please complete this operation later."));
			}
			else if (Success)
			{
				FireOnImportFinished(Res.GetString("629db2ac-7731-42eb-ac5e-7135fa70dc4c", "Import completed successfully."));
			}
			else
			{
				FireOnImportFinished(Res.GetString("a620f605-184d-4c7c-a598-e01b0ddccdff", "Import completed with an error."));
			}
		}

		void FireOnImportFinished(string message)
		{
			FireNotificationDelegate(OnImportFinished, Res.GetString("d0f2edea-7c96-48dc-a41f-1a632ce33b82", "{0} Process duration: {1}", message, new ZString((ZDateTime.Now - actualStart).ToString()).Left(8)) + "\r\n\r\n");
		}

		public void Cancel()
		{
			Canceled = true;
		}

		#endregion

		#region OnShowNotification

		void FireOnFatalError(Exception ex)
		{
			Success = false;
			FireOnShowNotification(Res.GetString("3e1cd2ce-cdf5-4fb2-b422-5426a3d92953", "Fatal Error:\r\n {0}\r\n{1}", ex.Message, ex.StackTrace) + "\r\n");
		}

		protected void FireOnShowNotification(string message)
		{
			FireNotificationDelegate(OnShowNotification, message);
		}

		void FireNotificationDelegate(NotificationDelegate method, string message)
		{
			if (method != null)
			{
				if (SyncInvoke == null)
				{
					method(message);
				}
				else
				{
					if (SyncInvoke != null && !SyncInvoke.InvokeRequired)
					{
						method(message);
					}
					else
					{
						SyncInvoke.BeginInvoke(method, new[] { message });
					}
				}
			}
		}

		#endregion

		#region OnProgress

		protected void FireOnProgressPeriodically()
		{
			if (LineNumber % 237 == 0)
			{
				FireOnProgress();
			}
		}

		protected void FireOnProgress()
		{
			if (OnProgress != null && !Canceled)
			{
				if (SyncInvoke == null)
				{
					OnProgress(LineNumber, InvalidLines, linesCount);
				}
				else
				{
					if (SyncInvoke != null && !SyncInvoke.InvokeRequired)
					{
						OnProgress(LineNumber, InvalidLines, linesCount);
					}
					else
					{
						SyncInvoke.BeginInvoke(OnProgress, new object[] { Math.Min(LineNumber, linesCount), InvalidLines, linesCount });
					}
				}
			}
		}

		#endregion

		public event NotificationDelegate OnImportStart;
		public event NotificationDelegate OnImportFinished;
		public event NotificationDelegate OnShowNotification;
		public event ProgressDelegate OnProgress;

		#endregion

		public ISynchronizeInvoke SyncInvoke { get; set; }
		public bool Success { get; protected set; }

		protected StreamReader StreamReader { get; private set; }
		protected BusinessObjectFactoryProvider FactoryProvider { get; private set; }
		protected bool Canceled { get; private set; }
		protected int LineNumber { get; set; }
		protected int InvalidLines { get; set; }

		internal const int RecordsPerFactory = 1000;

		int linesCount;
		int countOfSavings;
		ZDateTime actualStart;
	}

	public delegate void NotificationDelegate(string message);
	public delegate void ProgressDelegate(int linesProcessed, int invalidLines, int linesCount);
}
