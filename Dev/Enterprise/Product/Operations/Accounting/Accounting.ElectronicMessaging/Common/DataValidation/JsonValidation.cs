using System;
using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Newtonsoft.Json;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Accounting.ElectronicMessaging.Common.DataValidation
{
	public class JsonValidation : IPayloadValidation
	{
		public JsonValidation(string schemaResourceName)
		{
			if (schemaResourceName.IsNullOrEmpty())
			{
				throw new ArgumentException("schemaResourceName empty or null is not supported.");
			}

			SchemaResourceName = schemaResourceName;
		}

		readonly string SchemaResourceName;

		public void Validate(Stream stream, INotifications errorNotifications, INotifications warningNotifications)
		{
			stream.Position = 0;
			var reader = new StreamReader(stream);
			var payload = reader.ReadToEnd();

			try
			{
				var schema = JsonSchemaLoader.Load(SchemaResourceName);
				schema.ValidateJSON(payload, errorNotifications);
			}
			catch (JsonReaderException)
			{
				errorNotifications.AddError(Res.GetString("D7BA080C-8DCA-43A1-B7D1-CEA48BF89ED2", "Error occurred while reading following text:\r\n{0}", payload));
				throw;
			}
		}
	}

	[CodeAlive("Production usage chunked into future workflow")]
	public class JsonValidation2 : IPayloadValidation
	{
		public JsonValidation2(string schemaResourceName)
		{
			if (schemaResourceName.IsNullOrEmpty())
			{
				throw new ArgumentException("schemaResourceName empty or null is not supported.");
			}

			SchemaResourceName = schemaResourceName;
		}

		readonly string SchemaResourceName;

		public void Validate(Stream stream, INotifications errorNotifications, INotifications warningNotifications)
		{
			stream.Position = 0;
			var reader = new StreamReader(stream);
			var payload = reader.ReadToEnd();

			Json.Schema.JsonSchema schema;
			try
			{
				schema = Json.Schema.JsonSchema.FromStream(
					typeof(JsonValidation).Assembly.GetManifestResourceStream(SchemaResourceName),
					new System.Text.Json.JsonSerializerOptions() { ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip }
				).GetAwaiter().GetResult();
			}
			catch (System.Text.Json.JsonException)
			{
				errorNotifications.AddError(Res.GetString("D7BA080C-8DCA-43A1-B7D1-CEA48BF89ED2", "Error occurred while reading following text:\r\n{0}", payload));
				throw;
			}

			JsonHelper.ValidateJSON(schema, payload, errorNotifications);
		}
	}
}
