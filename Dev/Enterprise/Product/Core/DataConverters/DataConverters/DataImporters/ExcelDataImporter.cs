using System;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DataConverters
{
	public abstract class ExcelDataImporter : DataImporter
	{
		public ExcelDataImporter(ProgressLogger logger, ZString dataSourcePath, ZBool excludeExistingRecords) : base(logger, dataSourcePath, excludeExistingRecords)
		{
		}

		protected abstract string FirstLineExclusionString { get; }

		StringCollectionX GetCSVContents()
		{
			StringCollectionX result = null;
			var errorMessage = string.Empty;
			try
			{
				using (StreamReader reader = new StreamReader(DataSourcePath))
				{
					bool firstLine = true;
					string thisLine;
					result = new StringCollectionX();
					while ((thisLine = reader.ReadLine()) != null)
					{
						if (firstLine)
						{
							OCsvLine line = new OCsvLine(thisLine);
							ZString firstColumnFirstRow = line.FieldValues[0].ToUpper();
							if (!firstColumnFirstRow.Contains(FirstLineExclusionString) && !firstColumnFirstRow.Contains("CODE"))
							{
								result.Add(thisLine);
							}
							firstLine = false;
						}
						else
						{
							result.Add(thisLine);
						}
					}
				}
			}
			catch (ArgumentException e) { errorMessage = e.Message; }
			catch (IOException e) { errorMessage = e.Message; }
			// We do *not* want to catch OutOfMemoryExceptions. These are a critical failure and should bubble up.
			if (!string.IsNullOrEmpty(errorMessage))
			{
				result = null;
				Logger.DisplayFormatLogMessage("HEADER", errorMessage);
			}
			return result;
		}

		protected StringCollectionX Lines;

		protected override internal bool ReadData()
		{
			Logger.StartLog(DataTypeDescription, DataSourcePath);

			bool result = false;
			RecordCount = 0;
			CurrentRecord = 0;

			Lines = GetCSVContents();

			if (Lines != null && Lines.Count > 0)
			{
				RecordCount = Lines.Count;
				result = true;
			}
			else
			{
				Logger.Add(DataTypeDescription + " data could not be imported.");
			}

			return result;
		}

		protected OCsvLine GetNextCSVLine()
		{
			OCsvLine result = new OCsvLine(Lines[CurrentRecord]);
			CurrentRecord++;
			return result;
		}

		protected ZString GetZStringValueIfExists(string[] fieldValues, int index, int length)
		{
			return (index < fieldValues.Length) ? new ZString(fieldValues[index]).Trim().SubstringSafe(0, length) : ZString.Empty;
		}

		protected ZDecimal GetZDecimalValueIfExists(string[] fieldValues, int index)
		{
			ZString valueString = GetZStringValueIfExists(fieldValues, index, 20);
			ZDecimal result = new ZDecimal();
			ZDecimal.TryParse(valueString, out result);
			return result;
		}
	}
}
