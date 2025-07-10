using System;
using System.Collections;
using System.Collections.Specialized;

using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.DataImport.Level1FileFormat
{
	public class Level1RecordList : IEnumerable
	{
		public StringCollection RecordLines
		{
			get
			{
				if (fRecordLines == null)
				{
					fRecordLines = new StringCollection();
				}

				return fRecordLines;
			}
		}
		StringCollection fRecordLines;

		public int Count
		{
			get
			{
				int result = 0;

				foreach (Level1Record record in this)
				{
					if (record != null && !record.IsEmpty)
					{
						result++;
					}
				}

				return result;
			}
		}

		#region IEnumerable

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Level1RecordEnumerator(this);
		}

		#endregion

		#region Enumerator

		class Level1RecordEnumerator : IEnumerator
		{
			internal Level1RecordEnumerator(Level1RecordList level1RecordList)
			{
				this.Level1RecordList = level1RecordList;
				Reset();
			}
			readonly Level1RecordList Level1RecordList;
			int CurrentIndex;
			Level1Record CurrentLevel1Record;

			#region IEnumerator Members

			public void Reset()
			{
				CurrentIndex = 0;
				CurrentLevel1Record = null;
			}

			public object Current
			{
				get
				{
					return CurrentLevel1Record;
				}
			}

			public bool MoveNext()
			{
				bool result = false;

				CurrentLevel1Record = new Level1Record();

				if (Level1RecordList.RecordLines.Count > CurrentIndex)
				{
					string recordType = "";
					string nextRecordType = "";

					try
					{
						do
						{
							recordType = ((ZString)Level1RecordList.RecordLines[CurrentIndex]).SubstringSafe(RecordLine.Constants.RecordType.Position, RecordLine.Constants.RecordType.Length);

							if (recordType == RecordLine.Constants.RecordTypes._100000)
							{
								nextRecordType = "100001";
							}
							else
							{
								CurrentLevel1Record.AddRecordLine(Level1RecordList.RecordLines[CurrentIndex]);

								if (Level1RecordList.RecordLines.Count > (CurrentIndex + 1))
								{
									nextRecordType = ((ZString)Level1RecordList.RecordLines[CurrentIndex + 1]).SubstringSafe(RecordLine.Constants.RecordType.Position, RecordLine.Constants.RecordType.Length);
								}
								else
								{
									nextRecordType = "";
								}
							}
							CurrentIndex++;
							result = true;
						}
						while (!string.IsNullOrEmpty(nextRecordType) && Convert.ToInt64(nextRecordType) > Convert.ToInt64(recordType));
					}
					catch (ArgumentOutOfRangeException ex)
					{
						Level1FileReadException level1FileReadException = new Level1FileReadException("Level 1 File is Invalid:\n" + ex.Message, ex);
						level1FileReadException.LineNumber = CurrentIndex;

						if (CurrentIndex >= 2 && Level1RecordList.RecordLines.Count >= 2)
						{
							level1FileReadException.PreviousLineValue = Level1RecordList.RecordLines[CurrentIndex - 2];
							level1FileReadException.LineValue = Level1RecordList.RecordLines[CurrentIndex - 1];
						}

						throw level1FileReadException;
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						Level1FileReadException level1FileReadException = new Level1FileReadException("Level 1 File is Invalid:\n" + e.Message, e);
						level1FileReadException.LineNumber = CurrentIndex;

						if (CurrentIndex >= 1)
						{
							level1FileReadException.PreviousLineValue = Level1RecordList.RecordLines[CurrentIndex - 1];
						}
						level1FileReadException.LineValue = Level1RecordList.RecordLines[CurrentIndex];

						string errorMessage = level1FileReadException.Message;

						errorMessage += "RecordType:'" + recordType + "'\n";
						errorMessage += "NextRecordType:'" + nextRecordType + "'\n";
						if (Level1RecordList.RecordLines.Count > 0 && CurrentIndex >= 0 && Level1RecordList.RecordLines.Count > (CurrentIndex + 1))
						{
							errorMessage += "Current Level 1 Line:" + Level1RecordList.RecordLines[CurrentIndex];
							errorMessage += "Next Level 1 Line:" + Level1RecordList.RecordLines[CurrentIndex + 1];
						}

						throw level1FileReadException;
					}
				}

				return result;
			}

			#endregion
		}

		#endregion
	}
}
