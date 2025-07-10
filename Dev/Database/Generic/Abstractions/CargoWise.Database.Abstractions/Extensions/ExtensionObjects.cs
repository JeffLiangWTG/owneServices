using System.Collections.Immutable;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Database.Abstractions.Extensions
{
	[Immutable]
	public sealed class ExtensionObjects : IExtensionObjects
	{
		public ExtensionObjects(ImmutableArray<DatabaseObjectCreateScript> tableCreationScripts, ImmutableArray<DatabaseViewAndRoutineCreateScript> viewAndRoutineCreationScripts)
		{
			TableCreationScripts = tableCreationScripts;
			ViewAndRoutineCreationScripts = viewAndRoutineCreationScripts;
		}

		public ImmutableArray<DatabaseObjectCreateScript> TableCreationScripts { get; }
		public ImmutableArray<DatabaseViewAndRoutineCreateScript> ViewAndRoutineCreationScripts { get; }
	}
}


