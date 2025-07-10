using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace CargoWise.Common.JSON.Extensions
{
	public static class JSONExtensions
	{
		static DataContractJsonSerializerSettings SerializerSettings
		{
			get
			{
				return new DataContractJsonSerializerSettings
				{
					UseSimpleDictionaryFormat = true
				};
			}
		}

		public static T FromJSON<T>(this Stream jsonStream)
		{
			if (jsonStream is null)
			{
				throw new ArgumentNullException(nameof(jsonStream));
			}

			var serializer = new DataContractJsonSerializer(typeof(T), SerializerSettings);
			var deserializedObject = serializer.ReadObject(jsonStream);
			var deserialized = (T)deserializedObject;
			return deserialized;
		}

		public static T FromJSON<T>(this string jsonString)
		{
			if (jsonString is null)
			{
				throw new ArgumentNullException(nameof(jsonString));
			}

			using (var jsonStream = GenerateStreamFromString(jsonString))
			{
				return jsonStream.FromJSON<T>();
			}
		}

		public static string ToJSON<T>(this T obj)
		{
			var serializer = new DataContractJsonSerializer(typeof(T), SerializerSettings);
			var ms = new MemoryStream();
			serializer.WriteObject(ms, obj);
			ms.Position = 0;
			return Encoding.UTF8.GetString(ms.ToArray());
		}

		static Stream GenerateStreamFromString(string s)
		{
			var stream = new MemoryStream(Encoding.UTF8.GetBytes(s)) { Position = 0 };
			return stream;
		}
	}
}
