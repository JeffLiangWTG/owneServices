using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Test
{
	sealed class CriticalityStageMappingRegistryEditorInfoTest : TestCase
	{
		public void TestConstructor()
		{
			var editorInfo = new CriticalityStageMappingRegistryEditorInfo(
				new MultilingualString[] { (NoResString)"1", (NoResString)"1", (NoResString)"1", (NoResString)"1", (NoResString)"1" },
				(NoResString)"B",
				null,
				new bool[] { true, true, true, true, true },
				new bool[] { false, false, false, false, false },
				false);

			AssertContainsExactElementsInAnyOrder(new bool[] { true, true, true, true, true }, editorInfo.AreBoolColumnsVisible);
			AssertContainsExactElementsInAnyOrder(new bool[] { false, false, false, false, false }, editorInfo.AreCustomizedColumnsVisible);
		}
	}
}