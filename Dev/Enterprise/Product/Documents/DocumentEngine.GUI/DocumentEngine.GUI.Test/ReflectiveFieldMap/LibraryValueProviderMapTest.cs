using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using Enterprise.DocumentEngine.ValueProviders;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.ReflectiveFieldMap.Testing
{
	[TestedType(typeof(LibraryValueProviderMap))]
	public class LibraryValueProviderMapTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMultipleMacroLibrary()
		{
			var valueProviderMap = new LibraryValueProviderMap();
			valueProviderMap.Libraries = ImmutableArray.Create(new IMacroLibrary[] { new TestMacroLibrary(), new TestMultipleMacrosLibrary() });
			AssertEquals("valueProviderMap should have three element in MacroValueProviderMapCollection", 3, valueProviderMap.Macros.Count);
			Assert("The Usage and Description should match", valueProviderMap.Macros.Any(mapMacro =>
						((MacroValueProviderMap)mapMacro).Usage == "Usage" && ((MacroValueProviderMap)mapMacro).Description == "Description"));
			Assert("The Usage and Description should match", valueProviderMap.Macros.Any(mapMacro =>
								((MacroValueProviderMap)mapMacro).Usage == "Usage2" && ((MacroValueProviderMap)mapMacro).Description == "Description2"));
			Assert("The Usage and Description should match", valueProviderMap.Macros.Any(mapMacro =>
								((MacroValueProviderMap)mapMacro).Usage == "Usage3" && ((MacroValueProviderMap)mapMacro).Description == "Description3"));
		}

		public class TestMacroLibrary : MacroLibrary
		{
			protected override IEnumerable<IHandler> MacroHandlers
			{
				get
				{
					yield return new Handler<Func<IMacroScope, bool>>("Keyword", "Description", (scope) => true, "Usage");
				}
			}
		}

		public class TestMultipleMacrosLibrary : MacroLibrary
		{
			protected override IEnumerable<IHandler> MacroHandlers
			{
				get
				{
					yield return new Handler<Func<IMacroScope, bool>>("Keyword2", "Description2", (scope) => true, "Usage2");
					yield return new Handler<Func<IMacroScope, bool>>("Keyword3", "Description3", (scope) => true, "Usage3");
				}
			}
		}
	}
}
