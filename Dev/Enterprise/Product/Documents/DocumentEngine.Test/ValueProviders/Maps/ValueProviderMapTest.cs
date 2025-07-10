using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.ValueProviders.Testing
{
	[TestedType(typeof(ValueProviderMap))]
	sealed class ValueProviderMapTest : NonPersistentBusinessObjectTestCase
	{
		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestFetchingMacros()
		{
			var valueProviderMap = new ValueProviderMap();
			var errors = new List<string>();

			var provider = new ValueProviderCollector();
			AssertEquals(provider.ValueProviders.Count + 4, valueProviderMap.Macros.Count);

			foreach (MacroValueProviderMap mapMacro in valueProviderMap.Macros)
			{
				if ((!mapMacro.Usage.Substring(0, 1).Equals("<")) || (!mapMacro.Usage.Substring(mapMacro.Usage.Length - 1).Equals(">")))
				{
					errors.Add(mapMacro.Usage + " is not a valid Macro");
				}
			}

			Assert("There are errors with the following macros:\r\n\r\n" + string.Join("\r\n", errors.ToArray()) + "\r\n\r\nTotal Errors: " + errors.Count, errors.Count == 0);
		}
	}
}
