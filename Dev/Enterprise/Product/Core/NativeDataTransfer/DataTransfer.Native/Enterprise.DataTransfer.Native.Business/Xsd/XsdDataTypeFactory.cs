using Enterprise.DataTransfer.Native.Business.Xsd.Type;
using Enterprise.DataTransfer.Native.DB;
using Enterprise.DataTransfer.Native.DB.Helpers;

namespace Enterprise.DataTransfer.Native.Business.Xsd
{
	public static class XsdDataTypeFactory
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "sql string")]
		public static XsdDataType Map(IColumnDef columnDefDef)
		{
			var dataType = columnDefDef.DataType;
			var errorMessage = "Sql Db DataType of [" + dataType + "] has not been mapped to an DataType.";

			if (dataType.Contains(DbDataType.Char))
			{
				if (XsdBooleanDataTypeHelper.XsdIsBool(columnDefDef))
				{
					return new XsBoolean();
				}
				else
				{
					return new XsString(columnDefDef.Length);
				}
			}

			if (dataType.Contains(DbDataType.Text) || dataType == DbDataType.Xml || dataType == DbDataType.Geography)
			{
				return new XsString(columnDefDef.Length);
			}

			if (dataType.Contains(DbDataType.Decimal) || dataType.Contains(DbDataType.Money))
			{
				return new XsDecimal();
			}
			if (dataType.Contains(DbDataType.Short))
			{
				return new XsShort();
			}
			if (dataType.Contains(DbDataType.Integer))
			{
				return new XsInteger();
			}
			if (dataType.Contains(DbDataType.DateTime) || dataType.Contains(DbDataType.DateTimeOffset))
			{
				return new XsDateTime();
			}
			if (dataType.Contains(DbDataType.Date))
			{
				return new XsDateTime();
			}
			if (dataType.Contains(DbDataType.Time))
			{
				return new XsTime();
			}

			if (dataType.Contains(DbDataType.UniqueIdentifier))
			{
				return new XsGuid();
			}
			if (dataType.Contains(DbDataType.Image))
			{
				return new XsBase64Binary();
			}
			if (dataType.Contains(DbDataType.Binary))
			{
				return new XsBase64Binary();
			}
			if (dataType.Contains(DbDataType.Bit))
			{
				return new XsBoolean();
			}
			throw new NativeXMLUserVisibleException(errorMessage);
		}
	}
}
