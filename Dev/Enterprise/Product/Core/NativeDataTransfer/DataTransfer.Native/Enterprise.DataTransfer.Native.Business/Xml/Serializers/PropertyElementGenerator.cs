using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Business.Xsd.Type;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.DB;
using Microsoft.SqlServer.Types;

namespace Enterprise.DataTransfer.Native.Business.Xml.Serializers
{
	class PropertyElementGenerator : IXmlGenerator<Property, XElement>
	{
		#region IPropertyElementGenerator Members

		public XElement Generate(Property property)
		{
			return GenerateCore(property);
		}

		protected virtual XElement GenerateCore(Property property)
		{
			var xmlValue = GenerateValue(property.Definition, property.Value);
			return new XElement(property.Name, xmlValue);
		}

		/// <summary>
		/// Convert Value to Xml
		/// </summary>
		/// <param name="propertyDef"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Same as ZDateTime version")]
		[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		public string GenerateValue(IPropertyDef propertyDef, object value)
		{
			if (propertyDef.GetXsdDataType() is XsBoolean)
			{
				bool boolValue = value is bool ? (bool)value : value.ToString().Trim() == "Y";
				return boolValue ? "true" : "false";
			}

			if (value is DateTime)
			{
				return ((DateTime)value).ToString("s");
			}
			else if (value is DateTimeOffset)
			{
				return ((DateTimeOffset)value).ToString("O");
			}
			else if (value is SqlGeography geo)
			{
				return geo.AsTextZM().ToSqlString().ToString();
			}

			if (propertyDef.ColumnDef.DataType == DbDataType.VarBinary && value is byte[])
			{
				// Need to get plain bytes array as this row was loaded directly without any blob readers and may contain compressed data.
				return Convert.ToBase64String((byte[])ZCompressor.GetUncompressedVersion(value, propertyDef.ColumnDef.Name));
			}

			return value.ToString().TrimEnd();
		}

		#endregion
	}
}
