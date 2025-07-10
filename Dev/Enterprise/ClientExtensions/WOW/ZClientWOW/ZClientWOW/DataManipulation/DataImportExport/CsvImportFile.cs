using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.Wow
{
	public abstract class CsvImportFile
	{
		protected CsvImportFile(BusinessObjectFactoryProvider factoryProvider, StreamReader reader)
		{
			FactoryProvider = factoryProvider;
			CargoWise.Common.Argument.NotNull(reader, "reader");
			this.reader = reader;
		}
		protected readonly BusinessObjectFactoryProvider FactoryProvider;

		public StreamReader GetUpdatedMessageReader(string sequenceNoForEdiTrack)
		{
			ResetReader();

			VirtualMemoryStream virtualStream = new VirtualMemoryStream();
			StreamWriter writer = new StreamWriter(virtualStream);

			writer.WriteLine(GetSequenceNoForEdiTrack(sequenceNoForEdiTrack));
			string line = reader.ReadLine();  //Skip first line

			while (true)
			{
				line = reader.ReadLine();
				if (line == null)
				{
					break;
				}

				writer.WriteLine(line);
			}

			writer.Flush();
			virtualStream.Position = 0;

			StreamReader updatedReader = new StreamReader(virtualStream);
			return updatedReader;
		}

		public void VerifyAndUpdateBusinessDataAfterBatchDownload(INotifications notify)
		{
			NotificationBuffer verifyNotifyBuffer = new NotificationBuffer(notify);
			VerifyFileContentValid(notify);
			if (!verifyNotifyBuffer.HasErrors)
			{
				BatchDownloadRequiredData(notify);
				UpdateBusinessData(notify);
			}
		}

		public abstract void VerifyFileContentValid(INotifications notify);
		public abstract void BatchDownloadRequiredData(INotifications notify);
		public abstract bool ShouldSendToEdiTrack { get; }
		public abstract string GetSequenceNoForEdiTrack(ZString numberFountainID);
		public abstract CsvRecord TryNewRecord(string line);

		public virtual bool ShouldUpdateBusinessData
		{
			get { return true; }
		}

		public virtual void UpdateBusinessData(INotifications notify)
		{
			ResetReader();

			while (true)
			{
				List<string> lineBuffer = FillBuffer();
				if (lineBuffer.Count == 0)
				{
					break;
				}

				GenerateHints(lineBuffer);

				for (int i = 0; i < lineBuffer.Count; i++)
				{
					CsvRecord record = null;
					try
					{
						record = TryNewRecord(lineBuffer[i]);

						if (record == null)
						{
							string[] fieldValues = new OCsvLine(lineBuffer[i]).FieldValues;
							notify.Notify(new ErrorNotification(WowErrorType.UnknownRecordType, fieldValues[0]));
						}
						else if (record.SupportsUpdateBusinessData())
						{
							record.UpdateBusinessData(FactoryProvider, notify);
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						notify.AddWarning(String.Format((NoResString)"Warning: Line was skipped: {0}. Reason: {1}", lineBuffer[i], ex.Message));
					}
				}

				if (MaximumRecordsPerFactory.HasValue)
				{
					FactoryProvider.SaveCurrentAndCreateNew();
				}
			}
		}

		protected virtual void GenerateHints(IEnumerable<string> lines)
		{
		}

		protected virtual int? MaximumRecordsPerFactory
		{
			get { return null; }
		}

		#region Implementation

		protected StreamReader reader;

		protected string[] GetCsvLineAt(int lineIndex)
		{
			reader.DiscardBufferedData();
			reader.BaseStream.Position = 0;
			int lineNumber = 0;
			while (true)
			{
				string line = reader.ReadLine();
				if (line == null)
				{
					break;
				}

				if (lineNumber == lineIndex)
				{
					return new OCsvLine(line).FieldValues;
				}

				lineNumber++;
			}

			return null;
		}

		void ResetReader()
		{
			reader.DiscardBufferedData();
			reader.BaseStream.Position = 0;
		}

		List<string> FillBuffer()
		{
			List<string> lineBuffer = new List<string>();
			while (true)
			{
				string line = reader.ReadLine();
				if (line == null)
				{
					break;
				}

				lineBuffer.Add(line);

				if (MaximumRecordsPerFactory.HasValue)
				{
					if (lineBuffer.Count == MaximumRecordsPerFactory.Value)
					{
						break;
					}
				}
			}
			return lineBuffer;
		}
		#endregion

	}
}
