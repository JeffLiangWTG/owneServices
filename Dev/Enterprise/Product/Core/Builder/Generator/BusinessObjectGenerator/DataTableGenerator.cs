using System;
using System.Data;
using System.Globalization;
using System.Xml;
using Microsoft.SqlServer.Types;

namespace Enterprise.Builder.Generator
{
	public class DataTableGenerator
	{
		public DataTableGenerator()
		{
		}

		public DataTable FromXMLFile(string fileName)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(fileName);
			return FromXmlDocument(doc);
		}
		public DataTable FromXMLString(string xml)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);
			return FromXmlDocument(doc);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
		public DataTable FromXmlDocument(XmlDocument doc)
		{
			XmlNode tableNode = doc.SelectNodes("//Table")[0];
			DataTable table = new DataTable(tableNode.Attributes["Name"].Value);
			table.Locale = CultureInfo.InvariantCulture;

			foreach (XmlNode field in doc.SelectNodes("//Table/Field"))
			{
				table.Columns.Add(CreateColumnFromXmlNode(field));
			}

			return table;
		}

		#region Create Column from XmlNode

		protected DataColumn CreateColumnFromXmlNode(XmlNode field)
		{
			string fieldName = field.Attributes["Name"].Value;
			string dataType = field.Attributes["DataType"].Value;
			Type baseType = Type.GetType("System." + dataType, true, true);
			DataColumn column;

			if (baseType == typeof(decimal))
			{
				XmlAttribute precision = field.Attributes["Precision"];
				XmlAttribute scale = field.Attributes["Scale"];

				if (precision == null || scale == null)
				{
					throw new ArgumentException("The decimal column " + fieldName + " generated from XML an schema must have both Precision=\"x\" and Scale=\"x\" attributes.");
				}

				column = GetDecimalColumn(fieldName, GetByte(precision), GetByte(scale));
			}
			else if (baseType == typeof(DateTimeOffset))
			{
				var scale = field.Attributes["Scale"] ?? throw new ArgumentException("The DateTimeOffset column " + fieldName + " generated from XML an schema must have Scale=\"x\" attribute.");

				XmlAttribute allowNullAttribute = field.Attributes["AllowNull"];
				bool allowDBNull = (allowNullAttribute != null) && GetBool(allowNullAttribute);

				column = GetDateTimeOffsetColumn(fieldName, allowDBNull, GetByte(scale));
			}
			else if (baseType == typeof(string))
			{
				var maxLength = field.Attributes["MaxLength"];
				var nAddInfoFieldAttribute = field.Attributes["IsNAddInfoField"];
				var isNAddInfoField = (nAddInfoFieldAttribute != null) && GetBool(nAddInfoFieldAttribute);
				column = GetStringColumn(fieldName, GetInt(maxLength), isNAddInfoField);
			}
			else if (baseType == typeof(Boolean))
			{
				column = GetBoolColumn(fieldName);
			}
			else if (baseType == typeof(Guid) || baseType == typeof(DateTime) || baseType == typeof(TimeSpan) || baseType == typeof(byte[]) || baseType == typeof(SqlGeography))
			{
				XmlAttribute allowNullAttribute = field.Attributes["AllowNull"];
				bool allowDBNull = (allowNullAttribute != null) && GetBool(allowNullAttribute);
				column = GetGuidOrDateTimeOrByteArrayOrGeographyColumn(fieldName, baseType, allowDBNull);
				if (baseType == typeof(DateTime))
				{
					var isDateOnlyAttribute = field.Attributes["IsDateOnly"];
					var isDateOnly = (isDateOnlyAttribute != null) && GetBool(isDateOnlyAttribute);
					if (isDateOnly)
					{
						column.ExtendedProperties.Add("IsDateOnly", "Y");
					}
				}
			}
			else if (baseType == typeof(int) || baseType == typeof(short) || baseType == typeof(byte) || baseType == typeof(long))
			{
				column = GetIntegralColumn(fieldName, baseType);
			}
			else
			{
				throw new Exception("Unhandled type: " + baseType.ToString());
			}

			return column;
		}

		protected DataColumn GetDecimalColumn(string fieldName, byte precision, byte scale)
		{
			DataColumn column = new DataColumn(fieldName);
			column.AllowDBNull = false;
			column.DataType = typeof(decimal);
			column.DefaultValue = 0;

			column.ExtendedProperties.Add("Precision", precision);
			column.ExtendedProperties.Add("Scale", scale);

			return column;
		}

		protected DataColumn GetDateTimeOffsetColumn(string fieldName, bool allowDBNull, byte scale)
		{
			DataColumn column = new DataColumn(fieldName);
			column.AllowDBNull = allowDBNull;
			column.DataType = typeof(DateTimeOffset);
			column.ExtendedProperties.Add("Scale", scale);
			return column;
		}

		protected DataColumn GetStringColumn(string fieldName, int maxLength, bool isNAddInfoField)
		{
			DataColumn column = new DataColumn(fieldName);
			column.AllowDBNull = false;
			column.DataType = typeof(string);
			column.DefaultValue = "";
			column.MaxLength = maxLength;
			if (isNAddInfoField)
			{
				column.ExtendedProperties.Add("IsNAddInfoField", "Y");
			}

			return column;
		}

		protected DataColumn GetBoolColumn(string fieldName)
		{
			DataColumn column = new DataColumn(fieldName);
			column.AllowDBNull = false;
			column.DataType = typeof(bool);
			column.DefaultValue = false;
			return column;
		}

		protected DataColumn GetGuidOrDateTimeOrByteArrayOrGeographyColumn(string fieldName, Type baseType, bool allowDBNull)
		{
			DataColumn column = new DataColumn(fieldName);
			column.AllowDBNull = allowDBNull;
			column.DataType = baseType;

			return column;
		}

		protected DataColumn GetIntegralColumn(string fieldName, Type baseType)
		{
			DataColumn column = new DataColumn(fieldName);
			column.AllowDBNull = false;
			column.DataType = baseType;
			column.DefaultValue = 0;

			return column;
		}

		#endregion

		#region Get Primitive type from XmlAttribute

		protected int GetInt(XmlAttribute attribute)
		{
			return (attribute != null) ? Int32.Parse(attribute.Value) : 0;
		}

		protected byte GetByte(XmlAttribute attribute)
		{
			return (attribute != null) ? Byte.Parse(attribute.Value) : (byte)0;
		}

		protected bool GetBool(XmlAttribute attribute)
		{
			return attribute == null
				|| attribute.Value.StartsWith("Y", StringComparison.OrdinalIgnoreCase)
				|| attribute.Value.StartsWith("T", StringComparison.OrdinalIgnoreCase);
		}

		#endregion
	}
}
