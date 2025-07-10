// This reads a number text files and put data into db

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ImportReferenceFileData : ReferenceFileBuilder, ICMRReferenceFileUpgrader, Integration.Customs.AU.IImportAndUpdateDataReferenceFileData
	{
		public ImportReferenceFileData()
		{
		}

		public ImportReferenceFileData(ILogger logger)
		{
			this.logger = logger;
		}
		readonly ILogger logger;

		void WriteToLog(LogType logtype, string message, Exception ex = null)
		{
			logger?.Log(logtype, message, ex);

			if (logtype == LogType.Error)
			{
				mostRecentLoggedError = $"{message} {ex?.Message ?? string.Empty}";
			}
		}

		string mostRecentLoggedError = string.Empty;

		public void ImportData(ZBlob fileToUnzip)
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				ImportData(fileToUnzip, connection);
			}
		}

		const int RetryAttempts = 3;
#pragma warning disable CW1050 // Use System.TimeSpan Type For A Duration
		protected virtual int RetryWaitDelayMilliseconds => 5000;
#pragma warning restore CW1050 // Use System.TimeSpan Type For A Duration

		protected void ImportData(ZBlob fileToUnzip, DbConnection connection)
		{
			IEnumerable<string> failedFiles = null;
			var attempts = RetryAttempts;

			while (attempts-- > 0)
			{
				failedFiles = UnZipAndProcessFiles(fileToUnzip, connection, failedFiles);
				if (!failedFiles.Any())
				{
					break;
				}
				System.Threading.Thread.Sleep(RetryWaitDelayMilliseconds);
			}

			if (failedFiles.Any())
			{
				WriteToLog(LogType.Debug, "Some files could not be processed.");
				throw new UpdateReferenceFilesException("Processing failed with errors. " + mostRecentLoggedError);
			}
		}

		IEnumerable<string> UnZipAndProcessFiles(ZBlob zippedFile, DbConnection connection, IEnumerable<string> justTheseFiles)
		{
			IEnumerable<string> failedFiles = null;

			try
			{
				FileSupporter.UnzipFiles(zippedFile);
				var tempPath = FileSupporter.UnzipFilePath;
				var fileNames = justTheseFiles?.Select(n => Path.Combine(tempPath, n)).ToArray() ?? FileSupporter.GetFilesInDirectory(".txt", tempPath);

				WriteToLog(LogType.Debug, ZString.Format("Processing {0} Unzipped Files in {1}.", fileNames.Length, tempPath));
				failedFiles = ImportDataFromFiles(fileNames, connection);
			}
			finally
			{
				try
				{
					FileSupporter.DeleteTempDirectory();
				}
				catch (Exception e)
				{
					WriteToLog(LogType.Error, "Error deleting TempDirectory: " + e.Message);
				}
			}

			return failedFiles;
		}

		protected virtual IEnumerable<string> ImportDataFromFiles(IEnumerable<string> unzippedFiles, DbConnection connection)
		{
			var failedFiles = new List<string>();

			foreach (ZString currentFile in unzippedFiles)
			{
				var fileName = Path.GetFileName(currentFile);
				var customsFileName = GetCustomsFileName(fileName);
				SetTableMappingRow(customsFileName);

				if (MappingDataRow != null)
				{
					if (customsFileName != CMRReferenceFileBuilderConstants.ExchangeRatesFileName)
					{
						try
						{
							using (var transactionManager = connection.BeginTransactionWithManager())
							{
								if (TableName == CMRCodeListsSchema.Constants.TableName)
								{
									ReadFileAndInsertRecordsForCodeLists(currentFile, connection);
								}
								else
								{
									ReadFileDropAndInsertNewRecords(currentFile, connection);
								}

								transactionManager.CommitTransaction();
							}
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							if (ex is SqlException)
							{
								throw;
							}

							failedFiles.Add(fileName);
							WriteToLog(LogType.Error, ZString.Format("Error while importing the CMR Reference File {0}.", customsFileName), ex);
						}
					}

					fTableFieldsForTable = null;
				}
			}

			return failedFiles;
		}

		public override Stream GetTableMappings()
		{
			MemoryStream stream = new MemoryStream(new EmbeddedResourceRetriever(GetType().Assembly).GetBytes("Enterprise.Customs.AU.Declaration.Business.Data.Import.CMRReferenceFileDataImporter.XMLTables.TableMappings.xml"));
			return stream;
		}

		public override Stream GetTableStructure()
		{
			MemoryStream stream = new MemoryStream(new EmbeddedResourceRetriever(GetType().Assembly).GetBytes("Enterprise.Customs.AU.Declaration.Business.Data.Import.CMRReferenceFileDataImporter.XMLTables.TableStructure.xml"));
			return stream;
		}

		#region Line To Insert DB With

		protected string LineToInsertDBWith
		{
			get { return fLineToInsertDBWith; }
			set { fLineToInsertDBWith = value; }
		}
		string fLineToInsertDBWith;

		#endregion

		#region TableFieldsForTable

		protected DataRow[] TableFieldsForTable
		{
			get
			{
				if (fTableFieldsForTable == null)
				{
					fTableFieldsForTable = GetFilteredAndSortedDataSet();
				}
				return fTableFieldsForTable;
			}
		}
		protected DataRow[] fTableFieldsForTable;

		#endregion

		#region Is File a Main File

		protected ZBool IsFileAMainFile(ZString currentFilePath)
		{
			ZString fileName = currentFilePath.Replace(FileSupporter.UnzipFilePath + "\\", "");
			return !fileName.Contains("EDCHNG");
		}

		#endregion

		#region Customs File Name

		ZString GetCustomsFileName(ZString fileName)
		{
			ZString result = fileName.Split('-')[0];

			if (result.StartsWith(CMRReferenceFileBuilderConstants.CodeListFileName))
			{
				result = CMRReferenceFileBuilderConstants.CodeListFileName;
			}

			return result;
		}

		#endregion

		void ReadFileDropAndInsertNewRecords(ZString currentFile, DbConnection connection)
		{
			var fileName = Path.GetFileName(currentFile);
			if (fileName.StartsWith("MSGADVCE", StringComparison.InvariantCultureIgnoreCase))
			{
				PreProcessAFile(currentFile, "MSGADVCE", "MSGWORK", 4);
			}
			else if (fileName.StartsWith("LGMNTQST", StringComparison.InvariantCultureIgnoreCase))
			{
				PreProcessAFile(currentFile, "LGMNTQST", "LGMNWORK", 5);
			}

			DeleteAllRecords(connection);

			var isAcceptableDelegate = GetIsAcceptableDelegate(currentFile);
			using (var reader = new StreamReader(currentFile))
			{
				while ((LineToInsertDBWith = reader.ReadLine()) != null)
				{
					InsertRecord(isAcceptableDelegate, connection);
				}
			}
		}

		delegate bool IsAcceptableDelegate(string columnName, object value);

		IsAcceptableDelegate GetIsAcceptableDelegate(string filePath)
		{
			IsAcceptableDelegate result = null;
			var filename = Path.GetFileName(filePath);
			if (filename.StartsWith("SEAIMPAR", StringComparison.InvariantCultureIgnoreCase))
			{
				result = IsWithdrawnIndicatorAcceptable;
			}
			else if (filename.StartsWith("RFNRSNTP", StringComparison.InvariantCultureIgnoreCase))
			{
				result = IsRefundReasonStartDateAcceptable;
			}

			return result;
		}

		bool IsWithdrawnIndicatorAcceptable(string columnName, object value) => columnName != "WithdrawnIndicator" || value.ToString() != "W";

		bool IsRefundReasonStartDateAcceptable(string columnName, object value)
		{
			if (columnName == "RefundReasonType")
			{
				currentRefundReasonTypeCode = (ZString)value;
			}
			else if (columnName == "RefundReasonStartDate")
			{
				ZDateTime startDate;
				ZDateTime.TryParseExact((ZString)value, out startDate, "yyyyMMdd");
				if (startDate.IsEmpty)
				{
					WriteToLog(LogType.Error, $"RefundReasonType {currentRefundReasonTypeCode} has an entry with an invalid Start Date."); // Log entry
					return false;
				}

				if (RefundReasonTypeCodeEndDates.ContainsKey(currentRefundReasonTypeCode))
				{
					var existingEndDate = RefundReasonTypeCodeEndDates[currentRefundReasonTypeCode];
					if (existingEndDate.IsEmpty || existingEndDate > startDate)
					{
						WriteToLog(LogType.Error, $"RefundReasonType {currentRefundReasonTypeCode} has an overlapping entry. Start Date of {startDate.ToISO8601ShortDateString()} is invalid."); // Log entry
						return false;
					}
				}
			}
			else if (columnName == "RefundReasonEndDate")
			{
				ZDateTime endDate;
				ZDateTime.TryParseExact((ZString)value, out endDate, "yyyyMMdd");

				if (RefundReasonTypeCodeEndDates.ContainsKey(currentRefundReasonTypeCode))
				{
					var existingEndDate = RefundReasonTypeCodeEndDates[currentRefundReasonTypeCode];
					if (existingEndDate.IsEmpty || existingEndDate > endDate)
					{
						WriteToLog(LogType.Error, $"RefundReasonType {currentRefundReasonTypeCode} has an overlapping entry. End Date of {endDate.ToISO8601ShortDateString()} is invalid."); // Log entry
						return false;
					}
				}
				RefundReasonTypeCodeEndDates[currentRefundReasonTypeCode] = endDate;
			}

			return true;
		}

		ZString currentRefundReasonTypeCode = "";

		Dictionary<ZString, ZDateTime> RefundReasonTypeCodeEndDates => refundReasonTypeCodeEndDates ?? (refundReasonTypeCodeEndDates = new Dictionary<ZString, ZDateTime>());
		Dictionary<ZString, ZDateTime> refundReasonTypeCodeEndDates;

		void PreProcessAFile(string currentFile, string fileName, string workName, int keyLength)
		{
			var workFileName = currentFile.Replace(fileName, workName);
			File.Delete(workFileName);
			int lastKey = 0;
			string lastLine = string.Empty;
			string currentLine = string.Empty;
			using (var writer = new StreamWriter(workFileName))
			using (var reader = new StreamReader(currentFile))
			{
				while ((currentLine = reader.ReadLine()) != null)
				{
					if (!string.IsNullOrEmpty(currentLine) && currentLine.Length > keyLength && int.TryParse(currentLine.Substring(0, keyLength), out int outKey) && outKey >= lastKey)
					{
						if (!string.IsNullOrEmpty(lastLine))
						{
							writer.WriteLine(lastLine);
						}

						lastLine = currentLine;
						lastKey = outKey;
					}
					else
					{
						if (!string.IsNullOrEmpty(currentLine))
						{
							lastLine += currentLine;
						}
					}
				}
				if (!string.IsNullOrEmpty(lastLine))
				{
					writer.WriteLine(lastLine);
				}
			}
			File.Copy(workFileName, currentFile, true);
			File.Delete(workFileName);
		}

		void ReadFileAndInsertRecordsForCodeLists(ZString currentFile, DbConnection connection)
		{
			if (!fCodesHaveBeenDroppedAlready)
			{
				DeleteAllRecords(connection);
				fCodesHaveBeenDroppedAlready = true;
			}

			using (var reader = new StreamReader(currentFile))
			{
				while ((LineToInsertDBWith = reader.ReadLine()) != null)
				{
					InsertRecord(null, connection);
				}
			}
		}

		void DeleteAllRecords(DbConnection connection)
		{
			connection.ExecuteScalar("DELETE " + TableName);
		}

		bool fCodesHaveBeenDroppedAlready;

		bool IsValidCharacter(char character)
		{
			foreach (char validCharacter in CMRReferenceFileBuilderConstants.CharactersToKeep)
			{
				if (character == validCharacter)
				{
					return true;
				}
			}

			return false;
		}

		void InsertRecord(IsAcceptableDelegate isAcceptable, DbConnection connection)
		{
			if (LineToInsertDBWith.Length > 0 && IsValidCharacter(LineToInsertDBWith[0]))
			{
				int errorCount = 0;
				string sqlCommand = string.Empty;
				try
				{
					ZString fieldOrder = ZString.Empty;
					ZString values = ZString.Empty;

					bool acceptValue = true;
					foreach (DataRow currentRow in TableFieldsForTable)
					{
						ZString dataItemValue = GetDataItemColumnValue(currentRow);

						if (dataItemValue != CMRReferenceFileBuilderConstants.ActionIndicator)
						{
							ZString value = GetColumnValueFromLine(currentRow);

							if (isAcceptable != null)
							{
								acceptValue &= isAcceptable(GetDataItemColumnValue(currentRow), value);
							}
							if (!value.IsEmpty)
							{
								fieldOrder += TablePrefix + dataItemValue + ", ";
								values += GetValueToInsert(value.Trim(), GetFormatColumnValue(currentRow)) + ", ";
							}
						}
					}

					if (acceptValue && !fieldOrder.IsEmpty && !values.IsEmpty)
					{
						fieldOrder += PKColumnName;
						values += "newid()";

						sqlCommand = "Insert into " + TableName + " (" + fieldOrder + ") " + "Values (" + values + ")";
						connection.ExecuteScalar(sqlCommand);
					}
				}
				catch (SqlException ex)
				{
					var dbError = new DbErrorMatch(ex);
					if (dbError.ExceptionType != DbErrorType.CannotInsertDuplicateUniqueIndexKey)
					{
						if (errorCount < 5)
						{
							errorCount++;
							ErrorReporter.ReportOnce(string.Format("Error while importing a row into CMR Reference File table {0}, the row has been ignored.", TableName), string.Format("SQL query is: {0}", sqlCommand), ex);
						}
						else
						{
							throw;
						}
					}
				}
			}
		}

		protected ZString GetColumnValueFromLine(DataRow fieldInformation)
		{
			int startPosition = int.Parse(GetStartPositionColumnValue(fieldInformation)) - 1;
			int length = GetLengthOfField(GetLengthColumnValue(fieldInformation));

			ZString result = new ZString(LineToInsertDBWith).SubstringSafe(startPosition, length);

			if (length == 12 && !result.IsEmpty && GetFormatColumnValue(fieldInformation) == "Dts16")
			{
				try
				{
					result = result.Substring(0, 4) + "/" + result.Substring(4, 2) + "/" + result.Substring(6, 2) + " " + result.Substring(8, 2) + ":" + result.Substring(10, 2);
					if (!((ZDateTime)Convert.ToDateTime(result)).IsValidSmallDateTime)
					{
						result = ZString.Empty;
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					result = ZString.Empty;
				}
			}
			return result.KeepChars(CMRReferenceFileBuilderConstants.CharactersToKeep).Trim();
		}

		ZInt GetLengthOfField(ZString length)
		{
			ZInt result = 0;

			if (length.Contains("."))
			{
				result = GetLengthWtihPrecisionAndScale(length) + 1;
			}
			else
			{
				result = int.Parse(length);
			}

			return result;
		}

		ZString GetValueToInsert(ZString valueToConvert, ZString dataType)
		{
			ZString result = "";

			switch (dataType)
			{
				case "Character":
					result = "'" + valueToConvert + "'";
					break;
				case "Bool":
					result = "'" + valueToConvert + "'";
					break;
				case "Dts20":
					result = "'" + valueToConvert + "'";
					break;
				case "Dcymd8":
					if (valueToConvert == "00010101")
					{
						result = "null";
					}
					else
					{
						result = "'" + valueToConvert + "'";
					}
					break;
				case "Numeric":
					result = valueToConvert;
					break;
				default:
					result = "'" + valueToConvert + "'";
					break;
			}

			return result;
		}

		protected void SetTableMappingRow(ZString customsFileName)
		{
			ZString filter = MappingsTable.Columns[CMRReferenceFileBuilderConstants.TablesMappings.CustomsFileNameColumn]
				+ " = '" + customsFileName + "'";

			DataRow[] filteredRows = MappingsTable.Select(filter);

			MappingDataRow = filteredRows.Length > 0 ? filteredRows[0] : null;
		}
	}
}
