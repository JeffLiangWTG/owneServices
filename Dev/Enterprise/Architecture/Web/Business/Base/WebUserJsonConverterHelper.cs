#if NET
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Web.Business
{
	public class ZStringJsonConverter : JsonConverter<ZString>
	{
		public override ZString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			=> new ZString(reader.GetString());

		public override void Write(Utf8JsonWriter writer, ZString value, JsonSerializerOptions options)
			=> writer.WriteStringValue(value.ToString());
	}

	public class ZGuidJsonConverter : JsonConverter<ZGuid>
	{
		public override ZGuid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var str = reader.GetString();
			return string.IsNullOrEmpty(str) ? ZGuid.Empty : new ZGuid(str);
		}

		public override void Write(Utf8JsonWriter writer, ZGuid value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}

	public class ZBoolJsonConverter : JsonConverter<ZBool>
	{
		public override ZBool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var str = reader.GetString();
			return string.IsNullOrEmpty(str) ? ZBool.False : new ZBool(str);
		}

		public override void Write(Utf8JsonWriter writer, ZBool value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}

	public class MultilingualStringJsonConverter : JsonConverter<MultilingualString>
	{
		public override MultilingualString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var str = reader.GetString();
			return default(MultilingualString);
		}

		public override void Write(Utf8JsonWriter writer, MultilingualString value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}

	public class ZDateTimeJsonConverter : JsonConverter<ZDateTime>
	{
		public override ZDateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var str = reader.GetString();
			if (string.IsNullOrEmpty(str))
			{
				return ZDateTime.Empty;
			}

			if (ZDateTime.TryParseISO8601Date(str, out var result))
			{
				return result;
			}

			return ZDateTime.Invalid;
		}

		public override void Write(Utf8JsonWriter writer, ZDateTime value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}

	public class WebUserPolymorphicJsonConverter : JsonConverter<WebUser>
	{
		public override WebUser Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			using (var jsonDoc = JsonDocument.ParseValue(ref reader))
			{
				var root = jsonDoc.RootElement;

				if (!root.TryGetProperty("UserType", out var typeProp))
				{
					throw new JsonException("Missing UserType discriminator.");
				}

				string userType = typeProp.GetString();

				WebUser result = userType switch
				{
					"OrgContactWebUser" => JsonSerializer.Deserialize<OrgContactWebUser>(root.GetRawText(), options),
					_ => throw new NotSupportedException($"Unknown WebUser type: {userType}")
				};

				return result;
			}
		}

		public override void Write(Utf8JsonWriter writer, WebUser value, JsonSerializerOptions options)
		{
			var type = value.GetType();
			string userType = type.Name;
			var json = JsonSerializer.SerializeToElement(value, type, options);

			writer.WriteStartObject();
			writer.WriteString("UserType", userType);

			foreach (var prop in json.EnumerateObject())
			{
				prop.WriteTo(writer);
			}

			writer.WriteEndObject();
		}
	}

	public class OrgContactMinimalJsonConverter : JsonConverter<OrgContact>
	{
		public override OrgContact Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			ZGuid oc_oh = ZGuid.Empty;

			if (reader.TokenType != JsonTokenType.StartObject)
			{
				throw new JsonException();
			}

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
				{
					break;
				}

				if (reader.TokenType != JsonTokenType.PropertyName)
				{
					throw new JsonException();
				}

				string propName = reader.GetString();
				reader.Read();

				switch (propName)
				{
					case "OC_OH":
						oc_oh = new ZGuid(reader.GetString());
						break;
					default:
						reader.Skip();
						break;
				}
			}

			var factory = new WebFactory();
			var contact = factory.Factory.New<OrgContact>();
			contact.OC_OH = oc_oh;
			return contact;
		}

		public override void Write(Utf8JsonWriter writer, OrgContact value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			writer.WriteString("OC_OH", value.OC_OH.ToString());
			writer.WriteEndObject();
		}
	}

	public class OrgHeaderMinimalJsonConverter : JsonConverter<OrgHeader>
	{
		public override OrgHeader Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			ZString oh_category = ZString.Empty;
			ZBool oh_consignee = ZBool.False;

			if (reader.TokenType != JsonTokenType.StartObject)
			{
				throw new JsonException();
			}

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
				{
					break;
				}

				if (reader.TokenType != JsonTokenType.PropertyName)
				{
					throw new JsonException();
				}

				string propName = reader.GetString();
				reader.Read();

				switch (propName)
				{
					case "OH_Category":
						oh_category = new ZString(reader.GetString());
						break;
					case "OH_IsConsignee":
						oh_consignee = new ZBool(reader.GetString());
						break;
					default:
						reader.Skip();
						break;
				}
			}

			var factory = new WebFactory();
			var contact = factory.Factory.New<OrgHeader>();
			contact.OH_Category = oh_category;
			contact.OH_IsConsignee = oh_consignee;
			return contact;
		}

		public override void Write(Utf8JsonWriter writer, OrgHeader value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			writer.WriteString("OH_Category", value.OH_Category.ToString());
			writer.WriteString("OH_IsConsignee", value.OH_IsConsignee.ToString());
			writer.WriteEndObject();
		}
	}

	internal class WebContactMinimalJsonConverter : JsonConverter<WebContact>
	{
		public override WebContact Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			ZGuid oc_oh = ZGuid.Empty;
			ZString oc_contactname = ZString.Empty;

			if (reader.TokenType != JsonTokenType.StartObject)
			{
				throw new JsonException();
			}

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
				{
					break;
				}

				if (reader.TokenType != JsonTokenType.PropertyName)
				{
					throw new JsonException();
				}

				string propName = reader.GetString();
				reader.Read();

				switch (propName)
				{
					case "OC_OH":
						oc_oh = new ZGuid(reader.GetString());
						break;
					case "OC_ContactName":
						oc_contactname = new ZString(reader.GetString());
						break;
					default:
						reader.Skip();
						break;
				}
			}

			var factory = new WebFactory();
			var orgContact = factory.Factory.New<OrgContact>();
			orgContact.OC_OH = oc_oh;
			orgContact.OC_ContactName = oc_contactname;
			var contact = new WebContact(factory, orgContact);
			return contact;
		}

		public override void Write(Utf8JsonWriter writer, WebContact value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			writer.WriteString("OC_OH", value.OC_OH.ToString());
			writer.WriteString("OC_ContactName", value.OC_ContactName.ToString());
			writer.WriteString("IsActive", value.IsActive.ToString());
			writer.WriteEndObject();
		}
	}
}
#endif
