using System;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Database.Abstractions.Extensions
{
	[Immutable]
	public class DatabaseViewAndRoutineCreateScript : DatabaseObjectCreateScript
	{
		public DatabaseViewAndRoutineCreateScript(string objectName, string createScript, string dropScript, string objectType)
			: base(objectName, createScript, dropScript)
		{
			if (string.IsNullOrEmpty(objectType))
			{
				throw new ArgumentException("Value cannot be null or empty.", nameof(objectType));
			}

			ObjectType = objectType;
		}

		public string ObjectType { get; }
	}
}


