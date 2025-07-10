using System;
using System.Text;
using System.Text.Json;

namespace Enterprise.ZArchitecture.Environment
{
	public abstract class RegistryDataTypeWithJsonSerializer<T> : RegistryDataType<T>
	{
		protected RegistryDataTypeWithJsonSerializer(string code, T defaultValue) : base(code, defaultValue)
		{
		}

		protected override T DeserialiseCore(byte[] value)
		{
			var json = Encoding.Unicode.GetString(value);
			return JsonSerializer.Deserialize<T>(json);
		}

		protected override byte[] SerialiseCore(T value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			var jsonString = JsonSerializer.Serialize(value);
			return Encoding.Unicode.GetBytes(jsonString);
		}
	}
}
