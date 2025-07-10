using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public class DataVersionLogValueFormatter
	{
		public string GetStringFromPropertyValue(ZPropertyInfo property, DataRow row, bool useOriginalValue = false)
		{
			return GetStringFromPropertyValueCore(property, row, useOriginalValue);
		}

		public string SerialiseBinaryValueToString(string propertyName, ZBlob binaryValue)
		{
			return SerialiseBinaryValueToStringCore(propertyName, binaryValue);
		}

		protected virtual string SerialiseBinaryValueToStringCore(string propertyName, byte[] binaryValue) => string.Empty;

		public byte[] DeserialiseBinaryValueFromString(string propertyName, string serialisedBinValue)
		{
			return DeserialiseBinaryValueFromStringCore(propertyName, serialisedBinValue);
		}

		protected virtual byte[] DeserialiseBinaryValueFromStringCore(string propertyName, string serialisedBinValue) => null;

		public string GetFileExtensionFilter(string propertyName, string serialisedBinValue)
		{
			return GetFileExtensionFilterCore(propertyName, serialisedBinValue);
		}

		protected virtual string GetFileExtensionFilterCore(string propertyName, string serialisedBinValue) => string.Empty;

		public bool CanSaveBinaryValue(string propertyName)
		{
			return CanSaveBinaryValueCore(propertyName);
		}

		public string GetFileExtensionFilter(string propertyName, byte[] binaryValue)
		{
			return GetFileExtensionFilterCore(propertyName, binaryValue);
		}

		protected virtual string GetFileExtensionFilterCore(string propertyName, byte[] binaryValue) => string.Empty;

		protected virtual bool CanSaveBinaryValueCore(string propertyName) => false;

		protected virtual string GetStringFromPropertyValueCore(ZPropertyInfo property, DataRow row, bool useOriginalValue)
		{
			var propertyValue = useOriginalValue ? property.OriginalValue : ZDataType.ObjectToZType(property.PropertyType, row[property.Name]);

			if (propertyValue is ZDateTime dateTimeValue)
			{
				return dateTimeValue.ToLongTimeString();
			}

			if (propertyValue is ZBlob)
			{
				return propertyValue.IsEmpty ? string.Empty : Res.GetString("78DAD5ED-8CA1-4C02-BF89-EB3A33211808", "<changed>");
			}

			if (propertyValue is ZGuid zGuidValue)
			{
				return GetStringFromZGuidValue(property, zGuidValue);
			}

			return propertyValue.ToString();
		}

		protected virtual string GetStringFromZGuidValue(ZPropertyInfo property, ZGuid propertyValue)
		{
			if (!(property.PropertyType == typeof(ZGuid)))
			{
				throw new ArgumentException("Value must have ZGuid type", nameof(property));
			}

			if (propertyValue.IsEmpty)
			{
				return string.Empty;
			}

			var relatedObject = BusinessObject.GetRelatedBizO(property);
			if (relatedObject != null && relatedObject.PK != propertyValue)
			{
				relatedObject = relatedObject.Factory.Load(relatedObject.GetType(), propertyValue);
			}

			if (relatedObject != null)
			{
				try
				{
					var code = CodePropertyAttribute.CodeFromBusinessObject(relatedObject);
					if (!string.IsNullOrEmpty(code))
					{
						return code;
					}
				}
				catch (NoCodePropertyException)
				{ }
			}

			if (MetaData.GetListDataSource(property.BizObj, property.PropertyDescriptor) is IFindBoxListProvider listProvider)
			{
				try
				{
					return listProvider.CodeFromPrimaryKey(propertyValue);
				}
				catch (NoCodePropertyException)
				{
				}
			}

			return propertyValue.ToString();
		}

		protected internal virtual string GetDataColumnName(ZPropertyInfo property)
		{
			return property.Name;
		}
	}

	[Serializable]
	public class DataVersionLogValueFormatterException : ZException
	{
		public DataVersionLogValueFormatterException(string message, Exception inner)
			: base(message, inner)
		{
		}

#if NETFRAMEWORK
		protected DataVersionLogValueFormatterException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
