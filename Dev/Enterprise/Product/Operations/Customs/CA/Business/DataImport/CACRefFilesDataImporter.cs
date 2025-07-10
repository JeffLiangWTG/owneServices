using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.DataImport
{
	public interface IDataImportModule
	{
		string Name { get; }
		IBusinessObjectCollection Collection { get; }
		Dictionary<SchemaColumn, string> Columns { get; }
	}

	[CodeAlive("To be used by CA Customs")]
	public class CACRefFilesDataImporter : DataImporter
	{
		public void ImportDataFromCsvFile(StreamReader reader, IDataImportModule module, char delimiter)
		{
			ProcessSafe(reader, Res.GetString("8df39ed0-5d9b-4f1c-a90c-6b588aef1761", "Processing {0}...", module.Name), () => ImportDataFromCsvFile(module, delimiter));
		}

		void ImportDataFromCsvFile(IDataImportModule module, char delimiter)
		{
			var collection = module.Collection;
			var collectionFilter = collection.CompleteFilter;
			var bizObjType = collection.TypeOfElements;
			var columns = module.Columns;
			var columnsCount = columns.Count;

			string row;
			while (Success && !Canceled && (row = StreamReader.ReadLine()) != null)
			{
				var csvLine = new OCsvLine(row, delimiter);
				LineNumber++;

				if (csvLine.FieldValues.Length == columnsCount)
				{
					BusinessObject bizObj = null;
					for (var columnNumber = 0; columnNumber < columnsCount; columnNumber++)
					{
						ZString value = csvLine.FieldValues[columnNumber];
						var column = columns.ElementAt(columnNumber);
						var schemaColumn = column.Key;

						if (schemaColumn.MaxLength <= 0 || value.Length <= schemaColumn.MaxLength)
						{
							bizObj = bizObj ?? LoadBusinessObjectByFirstColumn(bizObjType, collectionFilter, schemaColumn, value)
									 ?? CreateNewBusinessObject(bizObjType, collection);

							var propertyInfo = bizObj.FindPropertyInfo(schemaColumn.Name);
							CheckPropertyIsValid(propertyInfo, schemaColumn, bizObj);

							if (!propertyInfo.SetValueFromString(value) || propertyInfo.HasNotifications())
							{
								bizObj.CancelChanges();
								if (propertyInfo.HasNotifications())
								{
									FireValueIsInvalid(columnNumber, column.Value, value, propertyInfo.Notifications);
								}
								else
								{
									FireUnableToParseSourceValue(columnNumber, column.Value, value, propertyInfo.PropertyType.Name);
								}

								break;
							}
						}
						else
						{
							FireLengthExceedsMaxLength(columnNumber, column.Value, value, schemaColumn.MaxLength);
							break;
						}
					}
				}
				else
				{
					FireColumnsCountDoNotMatch(columns, columnsCount, csvLine.FieldValues.Length);
				}

				if (SaveRequired)
				{
					SaveFactory();
				}
				else
				{
					FireOnProgressPeriodically();
				}
			}
		}

		#region Load/Create BO

		BusinessObject LoadBusinessObjectByFirstColumn(Type bizObjType, ZQuery collectionFilter, SchemaColumn schemaColumn, ZString value)
		{
			var query = new ZQuery(schemaColumn, value);
			if (!collectionFilter.IsNoResultQuery)
			{
				query.AddToFilter(collectionFilter);
			}

			return FactoryProvider.Current.LoadTop1(bizObjType, query);
		}

		BusinessObject CreateNewBusinessObject(Type bizObjType, IBusinessObjectCollection collection)
		{
			var bizObj = FactoryProvider.Current.New(bizObjType);
			collection.SetupNewElementButDoNotAddIt(bizObj, false);
			return bizObj;
		}

		#endregion

		#region Errors

		static void CheckPropertyIsValid(ZPropertyInfo propertyInfo, SchemaColumn schemaColumn, BusinessObject bizObj)
		{
			if (propertyInfo == null)
			{
				throw new InvalidOperationException(string.Format("Invalid property '{0}' on '{1}'\r\n", schemaColumn.Name, bizObj.GetType().FullName));
			}
		}

		void FireColumnsCountDoNotMatch(Dictionary<SchemaColumn, string> columns, int columnsCountExpected, int columnsCountActual)
		{
			var columnsDescription = new ZStringBuilder(columns.Select(f => f.Value)).ToStringWithDelimiterBetweenAppends(", ");
			var message = LineNumber == 1
							? Res.GetString("66139eb5-8b59-4369-9513-60e6ea27f9d9", "The source file should be a .CSV file with {0} columns in order: {1}. But it contains {2} column(s).", columnsCountExpected, columnsDescription, columnsCountActual)
							: Res.GetString("0b236eac-0a76-4246-b2d2-c7f695a3825e", "The source line {0} should have {1} columns in order: {2}. But it contains {3} column(s). The line has been skipped.", LineNumber, columnsCountExpected, columnsDescription, columnsCountActual);
			Success = LineNumber > 1;
			InvalidLines++;
			FireOnShowNotification(message + "\r\n\r\n");
		}

		void FireLengthExceedsMaxLength(int columnNumber, string columnName, ZString value, int maxLength)
		{
			FireColumnError(columnNumber, value, Res.GetString("8ae35da0-0634-447f-a2ec-ae0b8550ad90", "Source text length ({0}) exceeds destination {1} field max length ({2})", value.Length, columnName, maxLength));
		}

		void FireValueIsInvalid(int column, string columnName, string value, IEnumerable<INotification> notifications)
		{
			var notificationsFormatted = new ZStringBuilder(notifications.Select(n => n.Message)).ToStringWithNewLineBetweenAppends();
			FireColumnError(column, value, Res.GetString("ad3aa193-b786-4bc6-906e-c914e388b8a1", "Source value is not valid for {0} field, please see notifications below", columnName), notificationsFormatted);
		}

		void FireUnableToParseSourceValue(int column, string columnName, string value, string expectedType)
		{
			FireColumnError(column, value, Res.GetString("0de37430-007e-48ec-a167-8a1758e0efff", "Unable to parse source value to {0} data type for {1} field", expectedType, columnName));
		}

		void FireColumnError(int column, string value, string message, string notifications = "")
		{
			InvalidLines++;
			var builder = new ZStringBuilder();
			builder.Append(Res.GetString("7f976d45-4cac-4c2c-a1cd-b8cfb9d02599", "Line {0}, Column {1}: {2}. The line has been skipped.", LineNumber, column + 1, message));
			builder.Append(Res.GetString("cb773b1c-db6f-480c-b463-21f52122ab52", "Value: {0}", value));
			if (!string.IsNullOrEmpty(notifications))
			{
				builder.Append(Res.GetString("8d73d721-7323-43ed-81f3-8458bd6fc876", "Notifications: {0}", notifications));
			}

			FireOnShowNotification(builder.ToStringWithNewLineBetweenAppends() + "\r\n\r\n");
		}

		#endregion
	}
}
