using static Enterprise.DocumentEngineCore.DocumentParsing.DocumentFieldAttributeFinder;

namespace Enterprise.DocumentEngineCore.DocumentParsing.Testing
{
	sealed class DocumentFieldAttributeFinderTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestFindProperties()
		{
			DocumentFieldAttributeFinder finder = new DocumentFieldAttributeFinder();

			if (DocumentFieldAttributeHash.Contains("ClassToParseForTest"))
			{
				DocumentFieldAttributeHash.Remove("ClassToParseForTest");
			}
			if (DocumentFieldAttributeHash.Contains("AnotherClassToParseForTest"))
			{
				DocumentFieldAttributeHash.Remove("AnotherClassToParseForTest");
			}

			Assert("Hashtable should NOT contain ClassToParseForTest", !DocumentFieldAttributeHash.Contains("ClassToParseForTest"));
			DocumentFieldDefinitionCollection list = finder.FindProperties(typeof(ClassToParseForTest));
			AssertEquals("List should have 2 elements", 3, list.Count);
			Assert("List should contain apple code", list.ContainsField("Apple"));
			AssertEquals("Apples should have description", "All about Apples", list.GetDescription("Apple"));
			Assert("List should contain orange code", list.ContainsField("Orange"));
			AssertEquals("Orange description should be Yum", "Yum", list.GetDescription("Orange"));
			Assert("List should contain inherited property", list.ContainsField("InheritedProperty"));
			Assert("List should NOT contain property without description attribute", !list.ContainsField("NoDesc"));

			Assert("Hashtable should contain ClassToParseForTest", DocumentFieldAttributeHash.Contains("ClassToParseForTest"));

			Assert("Hashtable should NOT contain AnotherClassToParseForTest", !DocumentFieldAttributeHash.Contains("AnotherClassToParseForTest"));
			list = finder.FindProperties(typeof(AnotherClassToParseForTest));
			AssertEquals("List should have 3 elements", 3, list.Count);
			Assert("Hashtable should contain AnotherClassToParseForTest", DocumentFieldAttributeHash.Contains("AnotherClassToParseForTest"));

			list = finder.FindProperties(typeof(ClassWithMethodToParseForTest));
			AssertEquals("List should have 4 elements", 4, list.Count);
			Assert("Hashtable should contain method", list.ContainsField("GetLemon"));
			var getLemonField = list.GetFieldDefinition("GetLemon");
			AssertEquals("({freshFirst})", getLemonField.AdditionalFieldInfo);
			AssertEquals("GetLemon({freshFirst})", getLemonField.FieldNameForDisplay);
		}
	}
}
