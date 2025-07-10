using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Enterprise.ZArchitecture.Environment
{
	public class TableRowCountDataType
	{
		public TableRowCountDataType()
		{
			Dict = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
			UtcTime = DateTime.UtcNow;
		}

		public Dictionary<string, long> Dict;
		public DateTime UtcTime;
	}

	public class TableRowCountRegistryDataType : RegistryDataType<TableRowCountDataType>
	{
		public TableRowCountRegistryDataType()
			: base(RegistryDataTypes.Codes.Binary, new TableRowCountDataType())
		{ }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "string internal used only")]
		public const string TimeFormat = "yyyy-MM-dd HH:mm:ss";

		protected override TableRowCountDataType DeserialiseCore(byte[] value)
		{
			var str = Encoding.UTF8.GetString(value);
			var str1 = str.Substring(0, TimeFormat.Length);
			var str2 = str.Substring(TimeFormat.Length);

			var result = new TableRowCountDataType();
			result.UtcTime = DateTime.ParseExact(str1, TimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);

			if (str2.Length > 0)
			{
				result.Dict = str2.Split(';').Select(s => s.Split('=')).ToDictionary(a => a[0].Trim(), a => Convert.ToInt64(a[1].Trim(), CultureInfo.InvariantCulture));
			}

			return result;
		}

		protected override byte[] SerialiseCore(TableRowCountDataType value)
		{
			var str1 = value.UtcTime.ToString(TimeFormat, CultureInfo.InvariantCulture);
			var str2 = "";

			if (value.Dict.Count > 0)
			{
				str2 = string.Join(";", value.Dict.Select(x => x.Key + "=" + x.Value.ToString(CultureInfo.InvariantCulture)).ToArray());
			}

			return Encoding.UTF8.GetBytes(str1 + str2);
		}

		protected override TableRowCountDataType CloneValue(TableRowCountDataType value)
		{
			var result = new TableRowCountDataType();
			result.UtcTime = value.UtcTime;
			result.Dict = new Dictionary<string, long>();

			foreach (var item in value.Dict)
			{
				result.Dict.Add(item.Key, item.Value);
			}

			return result;
		}

		protected override bool HasDefaultEditorInfoCore
		{
			get { return false; }
		}
	}
}
