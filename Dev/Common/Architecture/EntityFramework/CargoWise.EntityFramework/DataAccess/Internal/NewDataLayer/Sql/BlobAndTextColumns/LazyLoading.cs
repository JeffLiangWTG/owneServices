using System.Data;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	public static class LazyLoading
	{
		public readonly static byte[] BinaryPlaceholder = new byte[] { 0, 0 };
		const string BinaryPlaceholderSqlConstant = "0x0000";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Placeholder const string")]
		public const string TextPlacehoder = "\0\0";
		const string TextPlacehoderSqlConstant = "char(0) + char(0)";
		const string XmlPlaceholderSqlConstant = "'<?placeholder LazyLoading=\"Yes\"?>'";
		const string XmlPlaceholderSqlConstantSeparateElement = "'<placeholder LazyLoading=\"Yes\"></placeholder>'";

		public static string SqlPlaceholder(SchemaColumn schemaColumn)
		{
			if (schemaColumn.SqlDbType == SqlDbType.Xml)
			{
				return XmlPlaceholderSqlConstant;
			}
			return schemaColumn.IsBinary ? BinaryPlaceholderSqlConstant : TextPlacehoderSqlConstant;
		}

		public static bool LoadRequired(object data)
		{
			if (data is string)
			{
				var stringData = (string)data;
				var stringDataLength = stringData == null ? 0 : stringData.Length;
				return stringData == TextPlacehoder || (stringDataLength == 33 && "'" + stringData + "'" == XmlPlaceholderSqlConstant) || (stringDataLength == 45 && "'" + stringData + "'" == XmlPlaceholderSqlConstantSeparateElement);
			}
			else if (data is byte[])
			{
				var bytes = (byte[])data;
				if (bytes.Length != BinaryPlaceholder.Length)
				{
					return false;
				}
				for (int i = 0; i < bytes.Length; i++)
				{
					if (bytes[i] != BinaryPlaceholder[i])
					{
						return false;
					}
				}
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
