using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Export.Business;
using Enterprise.Rating.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.DataTransfer.Universal
{
	internal static class Helpers
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception Message.")]
		public static object GetValue(SchemaColumn column, ZString value, ZGuid companyPK, BusinessObjectFactory factory)
		{
			IZType result = null;

			if (column is SchemaGuidColumn)
			{
				ZString exceptionMessage = ZString.Empty;
				int index = column.Name.IndexOf("_", StringComparison.OrdinalIgnoreCase);

				if (index > -1 && (column.Name.Length == 5 || (column.Name.Length > 5 && column.Name[5] == '_')))
				{
					ZString suffix = column.Name.Substring(index + 1, 2);
					ZString codeName = suffix + "_Code";
					ITableSchema tableSchema = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchemaFromColumnNamePrefix(suffix);

					if (tableSchema != null)
					{
						SchemaColumn codeColumn = null;
						if (ObjectFactory.Get<IApplicationSchemaResolver>().SchemaColumnExists(codeName, tableSchema.TableName))
						{
							codeColumn = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(codeName, tableSchema.TableName);
						}

						SchemaColumn companyColumn = null;
						string companyColumnName = suffix + "_GC";
						if (ObjectFactory.Get<IApplicationSchemaResolver>().SchemaColumnExists(companyColumnName, tableSchema.TableName))
						{
							companyColumn = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(companyColumnName, tableSchema.TableName);
						}

						if (codeColumn != null)
						{
							Type businessObjectType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(suffix);
							var query = new ZQuery(codeColumn, value);
							if (companyColumn != null)
							{
								query.AddToFilter(companyColumn, companyPK);
							}
							BusinessObject[] businessObjects = factory.Load(businessObjectType, query);

							if (businessObjects.Length == 1)
							{
								result = businessObjects[0].PK;
							}
							else if (businessObjects.Length == 0)
							{
								exceptionMessage = ZString.Format("{0} '{1}' cannot be found in receiving system. (Unable to determine primary key from code: {2}, {3})", DataBoundResourceStrings.GetColumnDescriptiveName(column.TableName, column.Name), value, column.Name, value);
							}
							else
							{
								exceptionMessage = ZString.Format("More than one records in the database for {0}. (Unable to determine primary key from code: {1}, {2})", DataBoundResourceStrings.GetColumnDescriptiveName(column.TableName, column.Name), column.Name, value);
							}
						}
						else
						{
							exceptionMessage = ZString.Format("Schema must contain a code column in order to perform matching criteria. Schema: {0} Column: {1} Value: {2}", tableSchema, column.Name, value);
						}
					}
					else
					{
						exceptionMessage = ZString.Format("Could not find schema related to column while performing matching criteria. Column: {0} Value: {1}", column.Name, value);
					}
				}
				else
				{
					exceptionMessage = ZString.Format("Invalid column name for matching criteria. Column: {0} Value: {1}", column.Name, value);
				}

				if (!exceptionMessage.IsEmpty)
				{
					throw new MatchingCriteriaException(exceptionMessage);
				}
			}
			else if (column is SchemaDateTimeColumn)
			{
				DateTime date;
				if (DateTime.TryParse(value, out date))
				{
					result = (ZDateTime)date;
				}
				else
				{
					throw new Exception(string.Format("Unable to parse date: {0}", value));
				}
			}
			else if (column is SchemaIntColumn)
			{
				ZInt i;
				if (ZInt.TryParse(value, out i))
				{
					result = i;
				}
				else
				{
					throw new Exception(string.Format("Unable to parse int: {0}", value));
				}
			}
			else if (column is SchemaDecimalColumn)
			{
				ZDecimal dec;
				if (ZDecimal.TryParse(value, out dec))
				{
					result = dec;
				}
				else
				{
					throw new Exception(string.Format("Unable to parse decimal: {0}", value));
				}
			}
			else if (column is SchemaShortColumn)
			{
				ZShort sh;
				if (ZShort.TryParse(value, out sh))
				{
					result = sh;
				}
				else
				{
					throw new Exception(string.Format("Unable to parse short: {0}", value));
				}
			}
			else if (column is SchemaBoolColumn)
			{
				result = new ZBool(value);
			}
			else
			{
				result = value;
			}

			return result;
		}

		public static bool SetValue<T>(BusinessObject businessObj, Enum xmlFieldName, T? value, Dictionary<Enum, SchemaColumn> mapping, bool setWhenReadOnly = false, bool setWhenInvalid = true, Action<ZString> onPropertyReadOnlyAction = null, Action<IZType> customValueSetter = null)
			where T : struct, IZType
			=> value.HasValue && SetValue(businessObj, xmlFieldName, value.Value, mapping, setWhenReadOnly, setWhenInvalid, onPropertyReadOnlyAction, customValueSetter);

		public static bool SetValue(BusinessObject businessObj, Enum xmlFieldName, IZType value, Dictionary<Enum, SchemaColumn> mapping, bool setWhenReadOnly = false, bool setWhenInvalid = true, Action<ZString> onPropertyReadOnlyAction = null, Action<IZType> customValueSetter = null)
		{
			bool result = false;

			SchemaColumn columnName;
			ZPropertyInfo info;
			IZType valueToSet = GetValueForBizoProperty(businessObj, xmlFieldName, value, mapping, setWhenInvalid, out columnName, out info, false);

			if (valueToSet != null)
			{
				if (setWhenReadOnly || !info.ReadOnly)
				{
					if (customValueSetter != null)
					{
						customValueSetter(valueToSet);
					}
					else
					{
						businessObj[columnName] = valueToSet;
					}
					result = true;
				}
				else if (onPropertyReadOnlyAction != null)
				{
					onPropertyReadOnlyAction(info.HumanReadableName);
				}
			}

			return result;
		}

		public static ZString GetCodeValue(BusinessObject businessObj, Enum xmlFieldName, ZString value, Dictionary<Enum, SchemaColumn> mapping)
		{
			SchemaColumn columnName;
			ZPropertyInfo info;
			return (ZString)GetValueForBizoProperty(businessObj, xmlFieldName, value, mapping, true, out columnName, out info, true);
		}

		static IZType GetValueForBizoProperty(BusinessObject businessObj, Enum xmlFieldName, IZType value, Dictionary<Enum, SchemaColumn> mapping, bool setWhenInvalid, out SchemaColumn columnName, out ZPropertyInfo info, bool getCodeFromPrimaryKey)
		{
			columnName = mapping[xmlFieldName];
			info = businessObj.ZPropertyInfoHash[columnName.Name];
			IZType valueToSet = null;
			if (columnName is SchemaGuidColumn)
			{
				if (value is ZString && !value.IsEmpty)
				{
					var listProvider = MetaData.GetListDataSource(businessObj, info.PropertyDescriptor) as IFindBoxListProvider;
					ZGuid primaryKey = listProvider.PrimaryKeyFromCode((ZString)value);
					if (setWhenInvalid || primaryKey.IsValid)
					{
						valueToSet = getCodeFromPrimaryKey ? (ZString)listProvider.CodeFromPrimaryKey(primaryKey) : primaryKey;
					}
				}
			}
			else
			{
				valueToSet = value;
			}

			return valueToSet;
		}

		public static List<RatingBasis> PopulateUniversalPaymentBases(BusinessObjectCollection<JobPaymentBasis> inputBases)
		{
			var result = new List<RatingBasis>();

			foreach (JobPaymentBasis pb in inputBases)
			{
				var universalPaymentBasis = JobPaymentBasisHelper.PopulateUniversalPaymentBases(pb);

				result.Add(universalPaymentBasis);
			}

			return result;
		}
	}
}
