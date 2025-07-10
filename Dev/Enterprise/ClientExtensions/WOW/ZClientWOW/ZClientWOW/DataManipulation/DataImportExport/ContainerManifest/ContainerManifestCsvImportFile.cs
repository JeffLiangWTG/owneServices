using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.Wow
{
	internal class ContainerManifestCsvImportFile : CsvImportFile
	{
		internal ContainerManifestCsvImportFile(BusinessObjectFactoryProvider factoryProvider, StreamReader reader)
			: base(factoryProvider, reader)
		{
			rowColumnValue = new List<string[]>();
			ReaderHeaderLines(reader);
			PopulateProperties();
		}

		internal string ContainerNumber { get; private set; }

		internal string ContainerSeal { get; private set; }

		internal string PortOfLoading { get; private set; }

		public string DestinationPort { get; private set; }

		public string Vessel { get; private set; }

		public string Voyage { get; private set; }

		public ZDateTime ETD { get; private set; }

		public ZDateTime ETA { get; private set; }

		public override void VerifyFileContentValid(INotifications notify)
		{
			CheckExistsTextAt(0, 0, "document", notify);
			CheckExistsTextAt(1, 0, "manifest", notify);

			CheckExistsTextAt(0, 1, "importer", notify);
			CheckExistsTextAt(0, 2, "consolidator", notify);
			CheckExistsTextAt(0, 3, "container", notify);
			CheckExistsTextAt(0, 4, "shipping", notify);
			CheckExistsTextAt(0, 5, "totals", notify);

			int currentIndex = 1;

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

				if (lineNumber < headerLineNumber)
				{
					continue;
				}

				string[] nextLine = new OCsvLine(line).FieldValues;
				if (nextLine.Length > 0)
				{
					double nextIndex = -1;
					double.TryParse(nextLine[0], NumberStyles.Integer, null, out nextIndex);
					if ((int)nextIndex != currentIndex)
					{
						notify.Notify(new ErrorNotification(
							WowErrorType.InvalidFileFormat, "Expected container record index " + currentIndex.ToString()));
						break;
					}
					else
					{
						currentIndex++;
					}
				}

				lineNumber++;
			}
		}

		public override void BatchDownloadRequiredData(INotifications notify)
		{
		}

		public override CsvRecord TryNewRecord(string line)
		{
			CsvRecord result;
			double tmp;
			string[] fieldValues = new OCsvLine(line).FieldValues;
			if (Double.TryParse(fieldValues[0], NumberStyles.Integer, null, out tmp))
			{
				result = new ContainerManifestCsvLine(this, line);
			}
			else
			{
				result = new ContainerManifestDummyLine(this, line);
			}
			return result;
		}
		public override bool ShouldSendToEdiTrack
		{
			get { return false; }
		}

		public override string GetSequenceNoForEdiTrack(ZString numberFountainID)
		{
			throw new NotSupportedException();
		}

		#region Implementation

		const int headerLineNumber = 6;

		readonly List<string[]> rowColumnValue;

		void ReaderHeaderLines(StreamReader reader)
		{
			reader.DiscardBufferedData();
			reader.BaseStream.Position = 0;
			while (true)
			{
				string line = reader.ReadLine();
				if (line == null)
				{
					break;
				}

				rowColumnValue.Add(new OCsvLine(line).FieldValues);
				if (rowColumnValue.Count == headerLineNumber)
				{
					break;
				}
			}
		}

		void PopulateProperties()
		{
			ContainerNumber = rowColumnValue[3][1];
			ContainerSeal = rowColumnValue[3][2];
			PortOfLoading = rowColumnValue[4][4];
			DestinationPort = rowColumnValue[4][5];
			Vessel = rowColumnValue[4][2];
			Voyage = rowColumnValue[4][3];
			ETD = ParseDate(rowColumnValue[4][6]);
			ETA = ParseDate(rowColumnValue[4][7]);
		}

		protected static ZDateTime ParseDate(string dateAsString)
		{
			ZDateTime result = ZDateTime.Invalid;
			dateAsString = dateAsString.Trim();
			try
			{
				if (dateAsString.Length > 4)
				{
					if (char.IsNumber(dateAsString[0]) &&
						char.IsNumber(dateAsString[1]) &&
						char.IsNumber(dateAsString[2]) &&
						char.IsNumber(dateAsString[3]))
					{
						DateTime date = DateTime.ParseExact(dateAsString, "yyyy/MM/dd", System.Globalization.CultureInfo.InvariantCulture);
						result = new ZDateTime(date);
					}
					else if (
						char.IsLetter(dateAsString[0]) &&
						char.IsLetter(dateAsString[1]) &&
						char.IsLetter(dateAsString[2]) &&
						dateAsString[3] == '-')
					{
						result = DateTime.ParseExact(dateAsString, "MMM-dd-yyyy", System.Globalization.CultureInfo.InvariantCulture);
					}
					else
					{
						DateTime date = DateTime.ParseExact(dateAsString, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
						result = new ZDateTime(date);
					}
				}
				else
				{
					result = ZDateTime.Invalid;
				}
			}
			catch (FormatException)
			{
				result = ZDateTime.Invalid;
			}

			return result;
		}

		protected void CheckExistsTextAt(int column, int row, string expectedText, INotifications notify)
		{
			bool isValid = true;

			if (row > headerLineNumber)
			{
				isValid = false;
			}

			if (isValid && column >= rowColumnValue[row].Length)
			{
				isValid = false;
			}

			if (isValid)
			{
				string actualText = rowColumnValue[row][column];
				if (expectedText != actualText.ToLower())
				{
					isValid = false;
				}
			}

			if (!isValid)
			{
				notify.Notify(new ErrorNotification(WowErrorType.InvalidFileFormat, "Expected text '" + expectedText + "' at (" + column + "," + row + ")"));
			}
		}

		#endregion
	}
}
