using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public static class JsonSchemaLoader
	{
		public static JSchema Load(string schemaResourceName)
		{
			var schemaText = string.Empty;
			var assembly = Assembly.GetExecutingAssembly();
			var resourceStream = assembly.GetManifestResourceStream(schemaResourceName);
			if (resourceStream != null)
			{
				using (var reader = new StreamReader(resourceStream))
				{
					schemaText = reader.ReadToEnd();
				}
				var schema = JSchema.Parse(schemaText);
				return schema;
			}
			return null;
		}

		public static Json.Schema.JsonSchema Load2(string schemaResourceName, Assembly sourceAssembly = null)
		{
			var schema = SchemaLookup.GetOrAdd(schemaResourceName, _ =>
			{
				var assembly = sourceAssembly ?? Assembly.GetExecutingAssembly();
				var schemaStream = assembly.GetManifestResourceStream(schemaResourceName)
								?? throw new ArgumentException($"Unknown JSON schema embedded resource '{schemaResourceName}' in assembly '{assembly.FullName}'.", schemaResourceName);
				var options = new System.Text.Json.JsonSerializerOptions() { ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip };
				return Json.Schema.JsonSchema.FromStream(schemaStream, options).GetAwaiter().GetResult();
			});
			return schema;
		}

		[ThreadSafe]
		static readonly ConcurrentDictionary<string, Json.Schema.JsonSchema> SchemaLookup = new ConcurrentDictionary<string, Json.Schema.JsonSchema>();
	}
}
