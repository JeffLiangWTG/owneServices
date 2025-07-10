using System;
using System.Collections;
using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.Wow
{
	public abstract class CsvImportFileFromMI : CsvImportFile
	{
		public CsvImportFileFromMI(BusinessObjectFactoryProvider factoryProvider, StreamReader reader)
			: base(factoryProvider, reader)
		{
			reader.DiscardBufferedData();
			reader.BaseStream.Position = 0;
			string line = reader.ReadLine();

			if (line != null)
			{
				this.Header = new MIHeaderCsvRecord(line);
			}
		}

		public readonly MIHeaderCsvRecord Header;

		public override void VerifyFileContentValid(INotifications notify)
		{
			reader.DiscardBufferedData();
			reader.BaseStream.Position = 0;
			string line1 = reader.ReadLine();
			string line2 = reader.ReadLine();

			if (line1 == null || line2 == null)
			{
				notify.Notify(new ErrorNotification(WowErrorType.MissingHeader, ""));
			}
			else
			{
				MIHeaderCsvRecord fileHeader = TryNewRecord(line1) as MIHeaderCsvRecord;

				int lineCount = 2;
				string line;

				while (true)
				{
					line = reader.ReadLine();
					if (line == null)
					{
						break;
					}

					line2 = line;
					lineCount++;
				}

				MITrailerCsvRecord fileTrailer = TryNewRecord(line2) as MITrailerCsvRecord;

				if (fileHeader == null)
				{
					notify.Notify(new ErrorNotification(WowErrorType.MissingHeader, ""));
				}

				if (fileTrailer == null)
				{
					notify.Notify(new ErrorNotification(WowErrorType.MissingTrailer, ""));
				}

				int actualRecordCount = lineCount - 2;
				if (fileTrailer != null && fileTrailer.NumberOfRecords != actualRecordCount)
				{
					notify.Notify(new ErrorNotification(WowErrorType.TotalRecordCountMismatch, ""));
				}
			}
		}

		#region Batch Loading Into Factory from Natural Keys

		/// <summary>
		/// Call this method before processing data for efficiency. It batch reads and caches all the business objects that may be
		/// read in from the factory
		/// </summary>
		public override void BatchDownloadRequiredData(INotifications notify)
		{
			// generate a hash table of Type -> ArrayList<NKColumnValuePair>
			Hashtable hash = new Hashtable();

			reader.DiscardBufferedData();
			reader.BaseStream.Position = 0;
			while (true)
			{
				string line = reader.ReadLine();
				if (line == null)
				{
					break;
				}

				CsvRecord record = null;

				try
				{
					record = TryNewRecord(line);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}

				if (record != null)
				{
					NKColumnValuePair[] nKs = record.GetBizTypeAndNKsForBatchDownload();
					foreach (NKColumnValuePair nK in nKs)
					{
						AddToNKHash(hash, nK);
					}
				}
			}

			// batch download the business objects
			foreach (DictionaryEntry entry in hash)
			{
				Type bizType = (Type)entry.Key;
				ArrayList nKs = (ArrayList)entry.Value;
				BatchDownloadBOs(FactoryProvider.Current, bizType, (NKColumnValuePair[])nKs.ToArray(typeof(NKColumnValuePair)));
			}
		}

		protected void AddToNKHash(Hashtable hash, NKColumnValuePair nK)
		{
			ArrayList existingNKs = (ArrayList)hash[nK.BizType];
			if (existingNKs == null)
			{
				existingNKs = new ArrayList();
				hash[nK.BizType] = existingNKs;
			}
			existingNKs.Add(nK);
		}

		/// <summary>
		/// Batch download a set of records (by natural key) from the database and cache them into the factory.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		protected void BatchDownloadBOs(BusinessObjectFactory factory, Type bizType, NKColumnValuePair[] nKs)
		{
			for (int i = 0; i < nKs.Length; i += 50)
			{
				ZQuery filter = new ZQuery();
				for (int j = i; j < Math.Min(i + 50, nKs.Length); j++)
				{
					filter.AddToFilter(JoinCondition.Or, nKs[j].NKColumn, SQLComparisonOperator.Equal, nKs[j].Value);
					if (bizType != nKs[j].BizType)
					{
						throw new Exception("Wrong business object type in NKs");
					}
				}
				factory.Load(bizType, filter);
			}
		}

		#endregion

		public override bool ShouldSendToEdiTrack
		{
			get { return false; }
		}

		public override string GetSequenceNoForEdiTrack(ZString numberFountainID)
		{
			Header.UpdateSequenceNoForEdiTrack(numberFountainID);
			return Header.ToCsvLineString();
		}

		public override CsvRecord TryNewRecord(string line)
		{
			return TryNewHeaderOrTrailerRecord(line);
		}

		#region Implementation

		protected CsvRecord TryNewHeaderOrTrailerRecord(string line)
		{
			CsvRecord result = null;
			string[] fieldValues = new OCsvLine(line).FieldValues;

			string recordTypeID = "";
			if (fieldValues.Length > 1)
			{
				recordTypeID = fieldValues[0];
			}

			if (recordTypeID == "0")
			{
				result = new MIHeaderCsvRecord(line);
			}
			else if (recordTypeID == "9")
			{
				result = new MITrailerCsvRecord(line);
			}

			return result;
		}

		#endregion
	}
}
