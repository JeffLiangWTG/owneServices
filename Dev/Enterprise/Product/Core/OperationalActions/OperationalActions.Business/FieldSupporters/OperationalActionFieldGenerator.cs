using System;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class OperationalActionFieldGenerator
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "diagnostic information")]
		public OperationalActionFieldSupporter CreateField(PropertyInfo[] path)
		{
			if (path == null)
			{
				throw new ArgumentNullException(nameof(path));
			}

			if (path.Length == 0)
			{
				throw new ArgumentException("path array is empty", nameof(path));
			}

			string field = ReflectionHelper.PathToFieldText(path);
			PropertyInfo info = path[path.Length - 1];

			ActionFieldAttribute att;
			OperationalActionFieldSupporter result;

			if ((att = ActionFieldAttribute.Get(info)) == null)
			{
				result = ReflectionHelper.IsUnsafePropertyName(info.Name) ? null : CreateAutoField(field, null, info);
			}
			else if (att.FieldType == ActionFieldType.Hidden)
			{
				result = null;
			}
			else
			{
				result = CreateExplicitField(field, att, info);

				if (result == null)
				{
					const string messageFormat =
						"A Field was explicitly flagged for addition yet we are unable to create an approperate field supporter for it.\r\n" +
						"field: {0}\r\n" +
						"root: {1}\r\n" +
						"";

					const string keyFormat = "OperationalActionFieldGenerator.CreateField: {0}";

					ErrorReporter.ReportOnce(
						string.Format(keyFormat, info.Name),
						string.Format(messageFormat, field, path[0].ReflectedType));
				}
			}

			return result;
		}

		public OperationalActionFieldSupporter CreateField(string field, string workflowType)
		{
			if (string.IsNullOrEmpty(field))
			{
				return null;
			}
			var customFields = CustomFieldHelper.GetCustomFields(workflowType);
			var property = customFields.FirstOrDefault(f => field.Equals(f.Info?.GetCaption(), StringComparison.OrdinalIgnoreCase));

			if (property != null)
			{
				var customBusinessObject = new CustomBusinessObject(null, CustomPropertyCollectionBuilder.GetCustomProperties(customFields));

				var propertyInfo = customBusinessObject.FindPropertyInfo(property.Identifier);

				if (propertyInfo.PropertyType == typeof(ZBool))
				{
					return new OperationalActionBooleanFieldSupporter(property.Identifier, propertyInfo.ReadOnly);
				}

				if (propertyInfo.PropertyType == typeof(ZDateTime))
				{
					var dateTimeFormatMetaData = property.Info.GetMetaData(MetaDataTypes.DateTimeFormat);

					if (dateTimeFormatMetaData != null)
					{
						return new OperationalActionDateTimeFieldSupporter(property.Identifier, propertyInfo.ReadOnly, (ZDateTimePickerFormat)dateTimeFormatMetaData.Value, propertyInfo.PropertyDescriptor.Attributes.OfType<ZDateTimeDurationValueAttribute>().Any());
					}

					return new OperationalActionDateFieldSupporter(property.Identifier, propertyInfo.ReadOnly);
				}

				if (propertyInfo.PropertyType == typeof(ZTime))
				{
					return new OperationalActionDateFieldSupporter(property.Identifier, propertyInfo.ReadOnly);
				}

				if (propertyInfo.PropertyType == typeof(ZDecimal))
				{
					return new OperationalActionNumericFieldSupporter(property.Identifier, propertyInfo.ReadOnly, decimal.MinValue, decimal.MaxValue, 9, 2) { IgnoreDecimalPrecisionCheck = true };
				}

				if (propertyInfo.PropertyType == typeof(ZInt))
				{
					return new OperationalActionNumericFieldSupporter(property.Identifier, propertyInfo.ReadOnly, int.MinValue, int.MaxValue, 10, 0);
				}

				if (propertyInfo.PropertyType == typeof(ZString))
				{
					var listDataSourceMetaData = property.Info.GetMetaData(MetaDataTypes.ListDataSource);

					return listDataSourceMetaData == null
						? new OperationalActionTextFieldSupporter(property.Identifier, propertyInfo.ReadOnly, Math.Min(GenCustomAddOnValue.Schema.XV_DataMaxLength, propertyInfo.MaxLength))
						: new OperationalActionCodeFieldSupporter(property.Identifier, propertyInfo.ReadOnly, (CodeDescriptionPairList)listDataSourceMetaData.Value);
				}
			}

			return null;
		}

		OperationalActionFieldSupporter CreateExplicitField(string field, ActionFieldAttribute att, PropertyInfo info)
		{
			switch (att.FieldType)
			{
				case ActionFieldType.Auto:
					return CreateAutoField(field, att, info);
				case ActionFieldType.Code:
					return CreateCodeField(field, att, info);
				case ActionFieldType.DateTime:
					return CreateDateTimeField(field, att, info);
				case ActionFieldType.Time:
					return CreateTimeField(field, att, info);
				case ActionFieldType.NKModule:
					return CreateNKModuleField(field, att, info, GetModuleCode(info.Name));
				case ActionFieldType.PKModule:
					return CreatePKModuleField(field, att, info, GetModuleCode(info.Name));
				case ActionFieldType.Text:
					return CreateTextField(field, att, info);
				case ActionFieldType.Address:
					return CreateAddressField(field, att, info);
				case ActionFieldType.Boolean:
					return CreateBooleanField(field, att, info);
				case ActionFieldType.Numeric:
					return CreateNumericField(field, att, info);
				case ActionFieldType.DateTimeOffset:
					return CreateDateTimeOffsetField(field, att, info);
				case ActionFieldType.Geography:
					return CreateGeographyField(field, att, info);
				default:
					return null;
			}
		}
		OperationalActionFieldSupporter CreateAutoField(string field, ActionFieldAttribute att, PropertyInfo info)
		{
			OperationalActionFieldSupporter result;
			string moduleCode;

			if (info.PropertyType == typeof(ZString))
			{
				moduleCode = GetModuleCode(info.Name);
				if (string.IsNullOrEmpty(moduleCode))
				{
					result = CreateNKModuleField(field, att, info, "") ?? CreateCodeField(field, att, info) ?? CreateTextField(field, att, info);
				}
				else if (moduleCode == OrgAddressSchema.Constants.Prefix)
				{
					result = null;
				}
				else
				{
					result = CreateNKModuleField(field, att, info, moduleCode);
				}
			}
			else if (info.PropertyType == typeof(ZDateTime))
			{
				result = CreateDateTimeField(field, att, info);
			}
			else if (info.PropertyType == typeof(ZDate))
			{
				result = CreateDateField(field, att, info);
			}
			else if (info.PropertyType == typeof(ZDateTimeOffset))
			{
				result = CreateDateTimeOffsetField(field, att, info);
			}
			else if (info.PropertyType == typeof(ZTime))
			{
				result = CreateTimeField(field, att, info);
			}
			else if (info.PropertyType == typeof(ZGeography))
			{
				result = CreateGeographyField(field, att, info);
			}
			else if (info.PropertyType == typeof(ZGuid))
			{
				moduleCode = GetModuleCode(info.Name);
				if (string.IsNullOrEmpty(moduleCode))
				{
					result = CreatePKModuleField(field, att, info, "");
				}
				else if (moduleCode == OrgAddressSchema.Constants.Prefix)
				{
					result = CreateAddressField(field, att, info);
				}
				else
				{
					result = CreatePKModuleField(field, att, info, moduleCode);
				}
			}
			else if (typeof(INumericZType).IsAssignableFrom(info.PropertyType))
			{
				result = CreateNumericField(field, att, info);
			}
			else if (info.PropertyType == typeof(ZBool))
			{
				result = CreateBooleanField(field, att, info);
			}
			else
			{
				result = null;
			}

			return result;
		}

		OperationalActionFieldSupporter CreateTextField(string field, ActionFieldAttribute att, PropertyInfo info)
		{
			int maxLength = (att != null && att.MaxLength > 0 ? att.MaxLength : QueryMaxLength(info));
			return (maxLength > 0) ? new OperationalActionTextFieldSupporter(field, GetReadOnly(att, info), maxLength) : null;
		}
		OperationalActionFieldSupporter CreateCodeField(string field, ActionFieldAttribute att, PropertyInfo info)
		{
			if (att == null)
			{
				return null;
			}
			else if (att.CollectionType != null && typeof(ReadOnlyCodeDescriptionPairList).IsAssignableFrom(att.CollectionType))
			{
				ReadOnlyCodeDescriptionPairList list = (ReadOnlyCodeDescriptionPairList)Activator.CreateInstance(att.CollectionType);
				return new OperationalActionCodeFieldSupporter(field, GetReadOnly(att, info), list);
			}
			else if (att.LookUpEditType != ActionFieldAttribute.NoDropEdit)
			{
				return new OperationalActionCodeFieldSupporter(field, GetReadOnly(att, info), new CodeDescriptionPairList(att.LookUpEditType));
			}
			else
			{
				return null;
			}
		}
		OperationalActionFieldSupporter CreateDateTimeField(string field, ActionFieldAttribute att, PropertyInfo info)
		{
			ZDateTimePickerFormat GetFormat()
			{
				if (att == null)
				{
					return ZDateTimePickerFormat.Long;
				}
				else
				{
					return att.DateTimeFormat;
				}
			}
			return new OperationalActionDateTimeFieldSupporter(field, GetReadOnly(att, info), GetFormat(), info.GetCustomAttributes().OfType<ZDateTimeDurationValueAttribute>().Any());
		}
		OperationalActionFieldSupporter CreateDateField(string field, ActionFieldAttribute att, PropertyInfo info)
		{
			return new OperationalActionDateFieldSupporter(field, GetReadOnly(att, info));
		}
		OperationalActionFieldSupporter CreateTimeField(string field, ActionFieldAttribute att, PropertyInfo info)
		{
			return new OperationalActionTimeFieldSupporter(field, GetReadOnly(att, info));
		}
		OperationalActionFieldSupporter CreateDateTimeOffsetField(string field, ActionFieldAttribute att, PropertyInfo info)
		{
			return new OperationalActionDateTimeOffsetFieldSupporter(field, GetReadOnly(att, info));
		}
		OperationalActionFieldSupporter CreateGeographyField(string field, ActionFieldAttribute att, PropertyInfo info)
		{
			return new OperationalActionGeographyFieldSupporter(field, GetReadOnly(att, info));
		}
		OperationalActionFieldSupporter CreateNKModuleField(string field, ActionFieldAttribute att, PropertyInfo info, string code)
		{
			Type collectionType;
			int maxLength = 0;

			if (att == null)
			{
				collectionType = CollectionTypeDecider.GuessCollectionTypeFromTablePrefix(code);
			}
			else if (att.CollectionType == null || typeof(IBusinessObjectCollection).IsAssignableFrom(att.CollectionType))
			{
				collectionType = att.CollectionType ?? CollectionTypeDecider.GuessCollectionTypeFromTablePrefix(code);
				maxLength = att.MaxLength;
			}
			else
			{
				collectionType = null;
			}

			OperationalActionFieldSupporter result = null;

			if (collectionType != null)
			{
				if (typeof(IHaveCodeDescriptionPairList).IsAssignableFrom(collectionType))
				{
					//Some collections like RefPackTypeCollection are augmented with a code/description list of built-in values, and we want to use them.
					//Reason why we're deciding to do this here is because we only get the collection type down here. Would have to rewrite a lot of logic to decide earlier.
					result = new OperationalActionCodeFieldSupporter(field, GetReadOnly(att, info), ((IHaveCodeDescriptionPairList)providerFactory.Get(collectionType)(new BusinessObjectFactory())).CodeDescriptionPairList);
				}
				else
				{
					if (maxLength <= 0)
					{
						maxLength = QueryMaxLength(info);
						if (maxLength <= 0)
						{
							Type elementType = BusinessObjectCollection.GetElementTypeFromCollectionType(collectionType);
							maxLength = QueryMaxLength(elementType);
						}
					}

					if (maxLength > 0)
					{
						result = new OperationalActionNKModuleFieldSupporter(field, GetReadOnly(att, info), maxLength, providerFactory.Get(collectionType));
					}
				}
			}

			return result;
		}
		OperationalActionFieldSupporter CreatePKModuleField(string field, ActionFieldAttribute att, PropertyInfo info, string code)
		{
			Type collectionType;

			if (att == null || att.CollectionType == null)
			{
				collectionType = CollectionTypeDecider.GuessCollectionTypeFromTablePrefix(code);
			}
			else if (typeof(IBusinessObjectCollection).IsAssignableFrom(att.CollectionType))
			{
				collectionType = att.CollectionType;
			}
			else
			{
				collectionType = null;
			}

			if (collectionType != null)
			{
				return new OperationalActionPKModuleFieldSupporter(field, GetReadOnly(att, info), providerFactory.Get(collectionType));
			}
			else
			{
				return null;
			}
		}
		OperationalActionFieldSupporter CreateAddressField(string field, ActionFieldAttribute att, PropertyInfo info)
		{
			Type collectionType = typeof(OrganisationsFindBoxCollection);
			AddressType defaultAddressType = AddressType.OFC;

			if (att != null)
			{
				collectionType = att.CollectionType ?? collectionType;
				defaultAddressType = att.DefaultAddressType;
			}

			return new OperationalActionAddressFieldSupporter(field, GetReadOnly(att, info), defaultAddressType, providerFactory.Get(collectionType));
		}
		OperationalActionFieldSupporter CreateBooleanField(string field, ActionFieldAttribute att, PropertyInfo info)
		{
			return new OperationalActionBooleanFieldSupporter(field, GetReadOnly(att, info));
		}
		OperationalActionFieldSupporter CreateNumericField(string field, ActionFieldAttribute att, PropertyInfo info)
		{
			int precision;
			int scale;
			decimal minValue;
			decimal maxValue;

			if (info.PropertyType == typeof(ZInt))
			{
				precision = 10;
				scale = 0;
				minValue = int.MinValue;
				maxValue = int.MaxValue;
			}
			else if (info.PropertyType == typeof(ZShort))
			{
				precision = 5;
				scale = 0;
				minValue = short.MinValue;
				maxValue = short.MaxValue;
			}
			else if (info.PropertyType == typeof(ZLong))
			{
				precision = 20;
				scale = 0;
				minValue = long.MinValue;
				maxValue = long.MaxValue;
			}
			else if (info.PropertyType == typeof(ZByte))
			{
				precision = 3;
				scale = 0;
				minValue = byte.MinValue;
				maxValue = byte.MaxValue;
			}
			else if (info.PropertyType == typeof(ZDecimal))
			{
				SchemaDecimalColumn column = QuerySchemaColumn(info) as SchemaDecimalColumn;
				precision = column == null ? 9 : column.Precision;
				scale = column == null ? 3 : column.Scale;
				minValue = decimal.MinValue;
				maxValue = decimal.MaxValue;
			}
			else
			{
				throw new ArgumentOutOfRangeException(nameof(info), info.PropertyType, "invalid property type");
			}

			if (att != null)
			{
				if (att.MaxLength > 0)
				{
					precision = att.MaxLength;
				}

				if (att.Scale >= 0)
				{
					scale = att.Scale;
				}

				if (att.MinValue > (float)minValue)
				{
					minValue = (decimal)att.MinValue;
				}

				if (att.MaxValue < (float)maxValue)
				{
					maxValue = (decimal)att.MaxValue;
				}
			}

			return new OperationalActionNumericFieldSupporter(field, GetReadOnly(att, info), minValue, maxValue, precision, scale);
		}

		#region Implementation

		static string GetModuleCode(string propertyName)
		{
			return Schema.GetForeignKeyTargetPrefixFromColumnName(propertyName);
		}

		static int QueryMaxLength(PropertyInfo info)
		{
			SchemaColumn schema = QuerySchemaColumn(info);
			return schema == null ? 0 : schema.MaxLength;
		}
		static int QueryMaxLength(Type type)
		{
			if (BusinessObjectFactory.HasTableName(type))
			{
				string tableName = BusinessObjectFactory.GetTableNameFromType(type);
				string columnName = CodePropertyAttribute.CodePropertyNameFromType(type);
				SchemaColumn schema = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(columnName, tableName);
				return schema == null ? 0 : schema.MaxLength;
			}
			else
			{
				return 0;
			}
		}

		static SchemaColumn QuerySchemaColumn(PropertyInfo info)
		{
			if (BusinessObjectFactory.HasTableName(info.ReflectedType))
			{
				string tableName = BusinessObjectFactory.GetTableNameFromType(info.ReflectedType);
				return ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(info.Name, tableName);
			}
			else
			{
				return null;
			}
		}

		bool GetReadOnly(ActionFieldAttribute att, PropertyInfo info)
		{
			return !info.CanWrite || (att != null && att.ReadOnly);
		}

		#endregion

		readonly BusinessObjectCollectionProviderFactory providerFactory = new BusinessObjectCollectionProviderFactory();
	}
}
