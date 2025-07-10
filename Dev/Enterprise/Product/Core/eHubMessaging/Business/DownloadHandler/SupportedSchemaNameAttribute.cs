using System;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	sealed class SupportedSchemaNameAttribute : Attribute
	{
		public SupportedSchemaNameAttribute(string schemaName)
		{
			SchemaName = schemaName;
		}

		public readonly string SchemaName;
	}
}
