using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public static class IZTypeExtensions
	{
		internal static bool IsValidForTargetColumn(this ZDateTime value, SchemaDateTimeColumn column)
		{
			return value.IsValid
				&& ((column.SqlDbType == System.Data.SqlDbType.SmallDateTime && value.IsValidSmallDateTime)
				|| (column.SqlDbType == System.Data.SqlDbType.DateTime && value.IsValidSqlDateTime)
				|| column.SqlDbType == System.Data.SqlDbType.Date); // SQL Date doesn't have a limitation
		}
		internal static bool IsValidForTargetColumn(this ZTime value, SchemaTimeColumn column)
		{
			return value.IsValid
				&& column.SqlDbType == System.Data.SqlDbType.Time;
		}

		public static ZString GetValidMaxLengthValue(this ZString value, SchemaStringColumn column, IXmlImportLogger logger, int maxLengthOverride = 0, string columnNameOverride = null)
		{
			var valueLength = value.Length;
			int maxLength = column.MaxLength;
			if (maxLengthOverride > 0 && maxLengthOverride < maxLength)
			{
				maxLength = maxLengthOverride;
			}
			string columnName = column.Name;
			if (columnNameOverride != null)
			{
				columnName = columnNameOverride;
			}
			if (valueLength > maxLength)
			{
				logger.Log(LogType.Warning, Res.GetString("46200ffe-babe-48c0-a997-dd92ac1ffbba", "Attempted to insert {0} characters into Field [{1}] which has a maximum length of {2} characters. Field was truncated.", valueLength, columnName, maxLength));
				value = value.Substring(0, maxLength);
			}
			return value;
		}
	}
}
