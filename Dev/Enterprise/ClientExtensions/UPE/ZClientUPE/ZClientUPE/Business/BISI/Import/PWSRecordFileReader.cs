using System;
using System.Collections;
using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.UPE.Business.BISI
{
	public class PWSRecordFileReader : IDisposable, IEnumerable
	{
		public PWSRecordFileReader(string fileName, INotifications notificationSubscriber)
		{
			this.NotificationSubscriber = notificationSubscriber;
			try
			{
				Reader = new StreamReader(fileName);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorNotification error = new ErrorNotification(ErrorType.IOError, "Cannot read PWS File. " + ex.Message);
				notificationSubscriber.Notify(error);
			}
		}

		public bool IsValid
		{
			get { return Reader != null; }
		}

		#region IDisposable Members

		public void Dispose()
		{
			if (Reader != null)
			{
				Reader.Close();
			}
		}

		#endregion

		#region IEnumerable Members

		public PWSRecordEnumerator GetEnumerator()
		{
			return new PWSRecordEnumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Enumerators

		#region Decorating Record Enumerator

		public class PWSRecordEnumerator : IEnumerator
		{
			public PWSRecordEnumerator(PWSRecordFileReader pWSReader)
			{
				this.PWSReader = pWSReader;
			}

			#region IEnumerator Members

			public void Reset()
			{
				InnerEnumerator.Reset();
				fCurrent = null;
			}

			public PWSRecordBase Current
			{
				get { return fCurrent; }
			}

			object IEnumerator.Current
			{
				get { return Current; }
			}

			public bool MoveNext()
			{
				bool result = InnerEnumerator.MoveNext();
				if (result)
				{
					if (InnerEnumerator.Current is PWSDetailRecord)
					{
						fCurrent = PWSTotalDataAreaRecord.New(ReadToEndOfTotalDataArea(), PWSReader.NotificationSubscriber);
						if (fCurrent == null)
						{
							StoreFirstInvalidLineIndex();
							result = MoveNext();
						}
						else
						{
							NotifyPreviousInvalidStructure();
						}
					}
					else
					{
						fCurrent = InnerEnumerator.Current;
					}
				}
				return result;
			}

			PWSRecordBase fCurrent;

			#endregion

			#region Implementation

			RawLineEnumerator InnerEnumerator
			{
				get
				{
					if (fInnerEnumerator == null)
					{
						fInnerEnumerator = new RawLineEnumerator(PWSReader);
					}
					return fInnerEnumerator;
				}
			}

			PWSDetailRecord[] ReadToEndOfTotalDataArea()
			{
				PWSDetailRecord currentDetailRecord;
				int currentSequence = 0;
				ZString currentPackageNumber = "";
				ArrayList recordList = new ArrayList();

				do
				{
					currentDetailRecord = InnerEnumerator.Current as PWSDetailRecord;
					if (currentDetailRecord == null || !currentDetailRecord.CanConstructTotalDataArea(++currentSequence, currentPackageNumber))
					{
						recordList.Clear();
						break;
					}

					recordList.Add(currentDetailRecord);
					currentPackageNumber = currentDetailRecord.PackageNumber;
				}
				while (!currentDetailRecord.IsLastRecord && InnerEnumerator.MoveNext());

				return (PWSDetailRecord[])recordList.ToArray(typeof(PWSDetailRecord));
			}

			void StoreFirstInvalidLineIndex()
			{
				if (InvalidStructureLineIndex == -1)
				{
					InvalidStructureLineIndex = InnerEnumerator.CurrentRawLineIndex;
				}
			}

			void NotifyPreviousInvalidStructure()
			{
				if (InvalidStructureLineIndex > -1)
				{
					PWSTotalDataAreaRecord currentTotalDataAreaRecord = Current as PWSTotalDataAreaRecord;
					int lastInvalidIndex = InnerEnumerator.CurrentRawLineIndex - currentTotalDataAreaRecord.NumberOfDetailRecords;
					string message = string.Format("Line {0} to {1}. Invalid Total Data Area Structure", InvalidStructureLineIndex, lastInvalidIndex);
					PWSReader.NotificationSubscriber.Notify(new WarningNotification(message));
					InvalidStructureLineIndex = -1;
				}
			}

			int InvalidStructureLineIndex = -1;
			RawLineEnumerator fInnerEnumerator;
			readonly PWSRecordFileReader PWSReader;

			#endregion
		}

		#endregion

		#region Raw Line Enumerator

		class RawLineEnumerator : IEnumerator, INotifications
		{
			public RawLineEnumerator(PWSRecordFileReader pWSReader)
			{
				this.PWSReader = pWSReader;
			}

			#region IEnumerator Members

			public void Reset()
			{
				BaseStream.Position = 0;
				fCurrent = null;
			}

			public PWSRecordBase Current
			{
				get { return fCurrent; }
			}

			public int CurrentRawLineIndex
			{
				get { return fCurrentRawLineIndex; }
			}

			object IEnumerator.Current
			{
				get { return Current; }
			}

			public bool MoveNext()
			{
				bool result = false;

				if (StreamReader != null)
				{
					string line = StreamReader.ReadLine();
					fCurrentRawLineIndex++;
					if (line != null)
					{
						if (string.IsNullOrEmpty(line))
						{
							result = MoveNext();
						}
						else
						{
							fCurrent = GetNewPWSRecord(line);
							result = true;
						}
					}
				}

				return result;
			}

			PWSRecordBase fCurrent;
			int fCurrentRawLineIndex;

			#endregion

			#region Implementation

			Stream BaseStream
			{
				get { return StreamReader.BaseStream; }
			}

			StreamReader StreamReader
			{
				get { return PWSReader.Reader; }
			}

			PWSRecordBase GetNewPWSRecord(ZString line)
			{
				PWSRecordBase result;

				string recordCode = line.Left(3);
				switch (recordCode)
				{
					case "000":
						result = new PWSHeaderRecord(line, this);
						break;

					case "020":
						result = new PWSDetailRecord(line, this);
						break;

					case "900":
						result = new PWSTrailerRecord(line, this);
						break;

					default:
						result = new PWSUnknownRecord(line, this);
						break;
				}

				return result;
			}

			readonly PWSRecordFileReader PWSReader;

			#endregion

			#region INotifications Members

			void INotifications.Add(INotification notification)
			{
				string message = string.Format("Line {0}. {1}", CurrentRawLineIndex, ((INotificationSubscriberNotification)notification).AdditionalInfo);
				PWSReader.NotificationSubscriber.Notify(new WarningNotification(message));
			}

			#endregion
		}

		#endregion

		#endregion

		protected readonly StreamReader Reader;
		readonly INotifications NotificationSubscriber;
	}
}
