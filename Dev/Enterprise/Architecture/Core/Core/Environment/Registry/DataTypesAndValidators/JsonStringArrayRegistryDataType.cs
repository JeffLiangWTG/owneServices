using System;
using System.Text;
using Newtonsoft.Json;

namespace Enterprise.ZArchitecture.Environment
{
	public sealed class JsonStringArrayRegistryDataType : StringArrayRegistryDataType
	{
		public JsonStringArrayRegistryDataType()
			: base(RegistryDataTypes.Codes.JsonStringArray)
		{
		}

		protected override string[] DeserialiseCore(byte[] value)
		{
			var valueAsString = Encoding.Unicode.GetString(value);
			if (!string.IsNullOrEmpty(valueAsString))
			{
				return JsonConvert.DeserializeObject<string[]>(valueAsString);
			}

			return Array.Empty<string>();
		}

		protected override byte[] SerialiseCore(string[] value)
		{
			var result = Array.Empty<byte>();

			if (value != null && value.Length > 0)
			{
				var jarray = JsonConvert.SerializeObject(value, new JsonSerializerSettings() { Formatting = Formatting.None });
				result = Encoding.Unicode.GetBytes(jarray);
			}
			return result;
		}
	}
}
