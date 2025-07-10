using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Core
{
	public class ConcurrencyExceptionHandler : IConcurrencyExceptionHandler
	{
		public ConcurrencyExceptionHandler(Exception e)
		{
			if (e is IConcurrencyException || e is DBConcurrencyException)
			{
				Exception = e;
			}
			else
			{
				throw new Exception("Unknown exception type. Cannot handle.", e);
			}
		}

		public void NotifyUserAndDevelopersIfNotAlreadyProcessed()
		{
			if (!IsExceptionProcessed)
			{
				NotifyUser();

				if (!Exception.NotifyUserWithoutErrorReport())
				{
					try
					{
						if (ObjectFactory.Get<IEntityFrameworkSettings>().ReportConcurrencyErrors)
						{
							NotifyDevelopers();
						}
					}
					catch (Exception e) when (!e.IsCriticalException()) // just in case
					{
						Globals.Message.ShowDeveloperException("Error creating Concurrency exception report", e);
					}
				}
			}
		}

		protected bool IsExceptionProcessed
		{
			get
			{
				return Exception.Source == GetType().Name;
			}
		}

		protected void NotifyUser()
		{
			MarkExceptionAsProcessed();

			Globals.Message.ShowError(UserFriendlyMessage);
		}

		public void NotifyDevelopers()
		{
			MarkExceptionAsProcessed();

			Globals.Message.ShowDeveloperException(Info, Exception);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception Handling Text")]
		public string Info
		{
			get
			{
				StringWriter strWriter = new StringWriter();
				XmlTextWriter xtw = new XmlTextWriter(strWriter);
				xtw.WriteString("\r\n--- Save Aborted Due to Concurrency Check ---"
					+ "\r\n" +
					BasicInfoAboutRow
					+ "\r\n");

				if (Row != null)
				{
					WriteColumnInformation(xtw);
				}

				return strWriter.GetStringBuilder().ToString();
			}
		}

		StringBuilder fUserFriendlyMessageBuilder = new StringBuilder("");
		public string UserFriendlyMessage
		{
			get
			{
				if (fUserFriendlyMessageBuilder == null || string.IsNullOrEmpty(fUserFriendlyMessageBuilder.ToString()))
				{
					var rowToUseForUserNameAndTime = RowOnDatabase ?? Row;
					var userNameAndTime = rowToUseForUserNameAndTime != null ? new EventManager().GetUserNameAndTimeOfLastEditOrDeleteOfARecord(rowToUseForUserNameAndTime) : null;

					fUserFriendlyMessageBuilder = new StringBuilder(
						Res.GetString("ca8afce3-a7fe-4f1c-abed-81fdf4aecc3f", "While you were editing your data, another user ({0}) modified it.\r\nYour changes cannot be saved because they may conflict with the other user's changes.\r\nPlease close and open this form to try again.", string.IsNullOrEmpty(userNameAndTime) ? Res.GetString("f498cd9c-49e5-4b6d-8ade-269a46b4b7a4", "Unknown") : userNameAndTime));

					if (!Globals.IsUserInteractive)
					{
						fUserFriendlyMessageBuilder.AppendLine(Exception.ToString());
					}
				}
				return fUserFriendlyMessageBuilder.ToString();
			}
		}

		#region Row to string information

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception Handling Text")]
		public string BasicInfoAboutRow
		{
			get
			{
				string info = "Row is null. Nothing known.";

				if (Row != null)
				{
					info = "\r\nROW INFORMATION";
					info += "\r\nTable      = " + Row.Table.TableName;
					info += "\r\nPK         = " + DataUtils.GetPk(Row);
					info += "\r\nRowState   = " + Row.RowState.ToString();
				}

				return info;
			}
		}

		#endregion

		#region Row to table format

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception Handling Text")]
		void WriteColumnInformation(XmlTextWriter xtw)
		{
			var rowInDb = RowOnDatabase;

			xtw.WriteString("\r\nCOLUMN INFORMATION\r\n");

			if (rowInDb != null && Row.HasVersion(DataRowVersion.Original))
			{
				if (DataUtils.IsDataInRowAccessible(Row))
				{
					GenerateConcurrencyTableValues(rowInDb);
					WriteConcurrencyTable(xtw);
				}
				else
				{
					xtw.WriteString("Row has been deleted.");
				}
			}
			else if (rowInDb == null)
			{
				if (!Row.HasVersion(DataRowVersion.Original))
				{
					xtw.WriteString("Row has no original version. Row is new.");
				}
				else
				{
					xtw.WriteString("Row in database has been deleted.");
				}
			}
		}

		public Dictionary<string, object> ColumnsDBChanged => columnsDBChanged;
		readonly Dictionary<string, object> columnsDBChanged = new Dictionary<string, object>();

		readonly Dictionary<string, RowValues> columns = new Dictionary<string, RowValues>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Localising diagnostic information is counterproductive.")]
		void GenerateConcurrencyTableValues(DataRow rowOnDb)
		{
			foreach (DataColumn col in Row.Table.Columns)
			{
				var originalRow = Row[col, DataRowVersion.Original];
				var valueFromDB = rowOnDb[col.ColumnName] is string ? ((string)rowOnDb[col.ColumnName]).TrimEnd(' ') : rowOnDb[col.ColumnName];
				var isGeographyColumn = ZGeography.IsGeographyValue(originalRow);
				if (!isGeographyColumn && (!originalRow.Equals(Row[col]) || !originalRow.Equals(valueFromDB))
					|| isGeographyColumn && (!ZGeography.Equals(originalRow, Row[col]) || !ZGeography.Equals(originalRow, valueFromDB)))
				{
					var hasDBChanged = HasDBChanged(valueFromDB, col);
					var rowValues = new RowValues(
						originalRow.ToString(),
						!isGeographyColumn ? ParseIfBool(valueFromDB.ToString()) : ZGeography.AsText(valueFromDB),
						Row[col].ToString(),
						hasDBChanged ? "DB Changed" : "None",
						ConcurrencyInfo.Get(Row, col).Strategy()
						);
					columns.Add(col.ColumnName, rowValues);

					if (hasDBChanged)
					{
						columnsDBChanged.Add(col.ColumnName, valueFromDB);
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Localising diagnostic information is counterproductive.")]
		string ParseIfBool(string colValue)
		{
			if (colValue == "Y" || colValue == "1")
			{
				return "True";
			}
			else if (colValue == "N" || colValue == "0")
			{
				return "False";
			}
			else
			{
				return colValue;
			}
		}

		bool HasDBChanged(object valueFromDB, DataColumn col)
		{
			var hasChanged = false;
			var valueFromApp = Row[col.ColumnName, DataRowVersion.Original];
			var isByteArray = valueFromDB is byte[] && valueFromApp is byte[];
			var isGeography = ZGeography.IsGeographyValue(valueFromDB);

			if (isGeography)
			{
				hasChanged = !ZGeography.Equals(valueFromDB, valueFromApp);
			}
			else if (valueFromApp is bool && valueFromDB is string)
			{
				hasChanged = (bool)valueFromApp != ((string)valueFromDB == "Y");
			}
			else if (!isByteArray && !valueFromApp.Equals(valueFromDB))
			{
				hasChanged = true;
			}

			return hasChanged;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hand crafted SQL")]
#if DEBUG
		internal
#endif
		DataRow RowOnDatabase
		{
			get
			{
				if (!isRowOnDatabaseLoaded)
				{
					isRowOnDatabaseLoaded = true;
					var tableName = Row?.Table.TableName;
					if (!string.IsNullOrEmpty(tableName))
					{
						var sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM {0} WHERE {1} = @pk",
																	tableName,
																	DataUtils.GetPkNameFromTable(Row.Table));

						var parameters = new List<IStructuralEquatable>();
						parameters.Add(("@pk", SqlDbType.UniqueIdentifier, 0, (object)DataUtils.GetPk(Row)));

						if (tableName == StmALogSchema.Constants.TableName)
						{
							sqlText += string.Format(" AND {0} = @parentPK", StmALogSchema.Constants.SL_Parent);
							parameters.Add(("@parentPK", SqlDbType.UniqueIdentifier, 0, Row[StmALogSchema.Constants.SL_Parent]));
						}

						DataTable tableOnDb = ZArchitecture.Core.Utilities.GetDataTableFromQuery(sqlText, parameters.ToArray());
						if (tableOnDb.Rows.Count == 1)
						{
							rowOnDatabase = tableOnDb.Rows[0];
							tableOnDb.TableName = tableName;
						}
					}
				}

				return rowOnDatabase;
			}
		}

		bool isRowOnDatabaseLoaded;
		DataRow rowOnDatabase;

		void WriteConcurrencyTable(XmlTextWriter xtw)
		{
			try
			{
				xtw.WriteStartElement("ConcurrencyTableRows"); // Localising diagnostic information is counterproductive.

				foreach (KeyValuePair<string, RowValues> col in columns)
				{
					xtw.WriteStartElement("ConcurrencyTableRow");
					RowValues rowValues = col.Value;
					xtw.WriteElementString("Column", col.Key);
					xtw.WriteElementString("Original", rowValues.Original);
					xtw.WriteElementString("DB", rowValues.DB);
					xtw.WriteElementString("Changed", rowValues.Changed);
					xtw.WriteElementString("Conflict", rowValues.Conflict);
					xtw.WriteElementString("Concurrency", rowValues.Concurrency);
					xtw.WriteEndElement();
				}
			}
			finally
			{
				xtw.WriteEndElement();
				columns.Clear();
			}
		}

		#endregion

		DataRow Row
		{
			get
			{
				return Exception is DBConcurrencyException ? ((DBConcurrencyException)Exception).Row : ((IConcurrencyException)Exception).Row;
			}
		}

		readonly Exception Exception;

		void MarkExceptionAsProcessed()
		{
			Exception.Source = GetType().Name;
		}
	}
}

class RowValues
{
	public RowValues(string original, string db, string changed, string conflict, string concurrency)
	{
		Original = original;
		DB = db;
		Changed = changed;
		Conflict = conflict;
		Concurrency = concurrency;
	}

	public string Original { get; private set; }
	public string DB { get; private set; }
	public string Changed { get; private set; }
	public string Conflict { get; private set; }
	public string Concurrency { get; private set; }
}
