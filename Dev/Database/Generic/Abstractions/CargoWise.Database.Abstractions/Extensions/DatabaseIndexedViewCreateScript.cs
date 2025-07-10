using System;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Database.Abstractions.Extensions
{
	[Immutable]
	public class DatabaseIndexedViewCreateScript : DatabaseViewAndRoutineCreateScript
	{
		public DatabaseIndexedViewCreateScript(string objectName, string viewCreateScript, string indexCreateScript, string dropScript, string objectType)
			: base(objectName, viewCreateScript, dropScript, objectType)
		{
			if (string.IsNullOrEmpty(indexCreateScript))
			{
				throw new ArgumentException("Value cannot be null or empty.", nameof(indexCreateScript));
			}

			IndexCreateScript = indexCreateScript;
		}

		public string IndexCreateScript { get; }
	}
}


