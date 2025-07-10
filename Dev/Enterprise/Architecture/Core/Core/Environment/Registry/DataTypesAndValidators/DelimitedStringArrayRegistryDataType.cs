using System;
using System.Text;

namespace Enterprise.ZArchitecture.Environment
{
	public sealed class DelimitedStringArrayRegistryDataType : StringArrayRegistryDataType
	{
		public DelimitedStringArrayRegistryDataType()
			: base(RegistryDataTypes.Codes.DelimitedStringArray)
		{
		}

		protected override string[] DeserialiseCore(byte[] value)
		{
			var valueAsString = Encoding.Unicode.GetString(value);

			return string.IsNullOrEmpty(valueAsString)
				? Array.Empty<string>()
				: valueAsString.Split(new string[] { Delimiter }, StringSplitOptions.RemoveEmptyEntries);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded delimiter")]
		const string Delimiter = "◄◘►";

		protected override byte[] SerialiseCore(string[] value)
		{
			return value != null && value.Length > 0
				? Encoding.Unicode.GetBytes(string.Join(Delimiter, value))
				: Array.Empty<byte>();
		}
	}
}
