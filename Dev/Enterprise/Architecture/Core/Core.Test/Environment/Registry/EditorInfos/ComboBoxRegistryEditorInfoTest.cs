using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class ComboBoxRegistryEditorInfoTest : TestCase
	{
		public void TestConstructor1()
		{
			ComboBoxRegistryEditorInfo editorInfo = new ComboBoxRegistryEditorInfo(OLookUpEditType.PaymentType);
			AssertEquals("LookUpList.ContainsCode(\"" + "CCX" + "\")", true, editorInfo.LookUpList.ContainsCode("CCX"));
			AssertEquals("LookUpList.ContainsCode(\"" + "PPD" + "\")", true, editorInfo.LookUpList.ContainsCode("PPD"));
			AssertEquals("ShowComboDescription", true, editorInfo.ShowComboDescription);
		}

		public void TestConstructor2()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("ABC", "");
			list.AddPair("XYZ", "");

			var listProvider = new CodeDescriptionPairListProvider(() => list);

			ComboBoxRegistryEditorInfo editorInfo = new ComboBoxRegistryEditorInfo(listProvider);
			AssertEquals("LookUpList.Count", 2, editorInfo.LookUpList.Count);
			AssertEquals("LookUpList.ContainsCode(\"ABC\")", true, editorInfo.LookUpList.ContainsCode("ABC"));
			AssertEquals("LookUpList.ContainsCode(\"XYZ\")", true, editorInfo.LookUpList.ContainsCode("XYZ"));
			AssertEquals("ShowComboDescription", true, editorInfo.ShowComboDescription);
		}

		public void TestConstructor3()
		{
			ComboBoxRegistryEditorInfo editorInfo = new ComboBoxRegistryEditorInfo(OLookUpEditType.PaymentType, false);
			AssertEquals("LookUpList.ContainsCode(\"" + "CCX" + "\")", true, editorInfo.LookUpList.ContainsCode("CCX"));
			AssertEquals("LookUpList.ContainsCode(\"" + "PPD" + "\")", true, editorInfo.LookUpList.ContainsCode("PPD"));
			AssertEquals("ShowComboDescription", false, editorInfo.ShowComboDescription);

			editorInfo = new ComboBoxRegistryEditorInfo(OLookUpEditType.PaymentType, true);
			AssertEquals("ShowComboDescription", true, editorInfo.ShowComboDescription);
		}

		public void TestConstructor4()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("ABC", "");
			list.AddPair("XYZ", "");

			var listProvider = new CodeDescriptionPairListProvider(() => list);

			ComboBoxRegistryEditorInfo editorInfo = new ComboBoxRegistryEditorInfo(listProvider, false);
			AssertEquals("LookUpList.Count", 2, editorInfo.LookUpList.Count);
			AssertEquals("LookUpList.ContainsCode(\"ABC\")", true, editorInfo.LookUpList.ContainsCode("ABC"));
			AssertEquals("LookUpList.ContainsCode(\"XYZ\")", true, editorInfo.LookUpList.ContainsCode("XYZ"));
			AssertEquals("ShowComboDescription", false, editorInfo.ShowComboDescription);

			editorInfo = new ComboBoxRegistryEditorInfo(listProvider, true);
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

			var editorInfo = new ComboBoxRegistryEditorInfo(listProvider);

			Assert("LookUpList should not be loaded yet", !listIsCreated);

			var lookUpList = editorInfo.LookUpList;
			Assert("CodeDescriptionPairList is now loaded when it is called for", listIsCreated);
		}
	}
}
