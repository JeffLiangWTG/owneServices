using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class HBLPackLinesDisplayOrderRegistryEditorInfoTest : TestCase
	{
		public void TestConstructor()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("ABC", "");
			list.AddPair("XYZ", "");

			var listProvider = new CodeDescriptionPairListProvider(() => list);

			var editorInfo = new HBLPackLinesDisplayOrderRegistryEditorInfo(listProvider);
			AssertEquals("LookUpList.Count", 2, editorInfo.LookUpList.Count);
			AssertEquals("LookUpList.ContainsCode(\"ABC\")", true, editorInfo.LookUpList.ContainsCode("ABC"));
			AssertEquals("LookUpList.ContainsCode(\"XYZ\")", true, editorInfo.LookUpList.ContainsCode("XYZ"));
			AssertEquals("ShowComboDescription", true, editorInfo.ShowComboDescription);
		}

		public void TestLookUpListIsLazyLoaded()
		{
			var listIsCreated = false;
			var listProvider = new CodeDescriptionPairListProvider(() =>
			{
				listIsCreated = true;
				return new CodeDescriptionPairList();
			});

			var editorInfo = new HBLPackLinesDisplayOrderRegistryEditorInfo(listProvider);

			Assert("LookUpList should not be loaded yet", !listIsCreated);

			var lookUpList = editorInfo.LookUpList;
			Assert("CodeDescriptionPairList is now loaded when it is called for", listIsCreated);
		}
	}
}
