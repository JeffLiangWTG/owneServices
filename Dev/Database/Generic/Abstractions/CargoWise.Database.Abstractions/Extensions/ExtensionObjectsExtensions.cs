using System.Collections.Immutable;

namespace CargoWise.Database.Abstractions.Extensions
{
	public static class ExtensionObjectsExtensions
	{
		public static ImmutableArray<DatabaseObjectCreateScript> GetAllScripts(this IExtensionObjects objects)
		{
			var totalNumberOfScripts = objects.TableCreationScripts.Length + objects.ViewAndRoutineCreationScripts.Length;
			var builder = ImmutableArray.CreateBuilder<DatabaseObjectCreateScript>(totalNumberOfScripts);

			builder.AddRange(objects.TableCreationScripts);
			builder.AddRange(objects.ViewAndRoutineCreationScripts);

			return builder.MoveToImmutable();
		}
	}
}


