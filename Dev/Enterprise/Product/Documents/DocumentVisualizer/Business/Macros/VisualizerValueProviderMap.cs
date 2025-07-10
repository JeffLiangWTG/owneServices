namespace Enterprise.DocumentVisualizer.Business
{
	using CargoWise.Macros;
	using Enterprise.DocumentEngine.ValueProviders;

	public class VisualizerValueProviderMap : ValueProviderMap
	{
		protected override void PopulateMacroMapElements(MacroValueProviderMapCollection collection)
		{
			var libs = new StandardLibrary();
			foreach (var lib in libs)
			{
				foreach (var doc in lib.MacroDocumentation)
				{
					collection.Add(new MacroValueProviderMap(doc.Usage, doc.Description));
				}
			}
		}
	}
}