using System.Collections.Immutable;

namespace CargoWise.Database.Abstractions.Extensions
{
	public interface IExtensionObjects
	{
		ImmutableArray<DatabaseObjectCreateScript> TableCreationScripts { get; }
		ImmutableArray<DatabaseViewAndRoutineCreateScript> ViewAndRoutineCreationScripts { get; }
	}
}


