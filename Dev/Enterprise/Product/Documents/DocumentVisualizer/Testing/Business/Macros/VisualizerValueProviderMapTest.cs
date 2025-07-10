namespace Enterprise.DocumentVisualizer.Testing
{
	using System.Collections.Generic;
	using CargoWise.EntityFramework.Testing;
	using Enterprise.DocumentEngine.ValueProviders;
	using Enterprise.DocumentVisualizer.Business;
	using NUnit.Framework;

	[TestedType(typeof(VisualizerValueProviderMap))]
	public class VisualizerValueProviderMapTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFetchingMacros()
		{
			var valueProviderMap = new VisualizerValueProviderMap();
			var errors = new List<string>();

			foreach (MacroValueProviderMap mapMacro in valueProviderMap.Macros)
			{
				if (string.IsNullOrWhiteSpace(mapMacro.Usage))
				{
					errors.Add(mapMacro.Usage + " do not have an valid usage description");
				}
			}

			Assert("There are errors with the following macros:\r\n\r\n" + string.Join("\r\n", errors.ToArray()) + "\r\n\r\nTotal Errors: " + errors.Count, errors.Count == 0);
		}
	}
}
