using System.Collections.Immutable;
using CargoWise.Macros;
using Enterprise.DocumentEngine.ValueProviders;

namespace Enterprise.DocumentEngine.GUI.ReflectiveFieldMap
{
	public class LibraryValueProviderMap : ValueProviderMap
	{
		public ImmutableArray<IMacroLibrary> Libraries { get; set; } = ImmutableArray<IMacroLibrary>.Empty;

		protected override void PopulateMacroMapElements(MacroValueProviderMapCollection macros)
		{
			foreach (var libs in Libraries)
			{
				foreach (var lib in libs)
				{
					foreach (var doc in lib.MacroDocumentation)
					{
						macros.Add(new MacroValueProviderMap(doc.Usage, doc.Description));
					}
				}
			}
		}
	}
}
