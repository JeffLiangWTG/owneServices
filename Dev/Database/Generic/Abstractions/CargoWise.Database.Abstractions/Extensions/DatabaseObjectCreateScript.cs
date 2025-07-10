using System;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Database.Abstractions.Extensions
{
	[Immutable]
	public class DatabaseObjectCreateScript
	{
		public DatabaseObjectCreateScript(string objectName, string createScript, string dropScript)
		{
			ObjectName = objectName;
			CreateScript = createScript ?? throw new ArgumentNullException(nameof(createScript), "The createScript must not be null");
			DropScript = dropScript ?? throw new ArgumentNullException(nameof(dropScript), "The dropScript must not be null");
		}

		public string ObjectName { get; }
		public string CreateScript { get; }
		public string DropScript { get; }

		public bool IsObjectNameSpecified => !string.IsNullOrEmpty(ObjectName);
	}
}


