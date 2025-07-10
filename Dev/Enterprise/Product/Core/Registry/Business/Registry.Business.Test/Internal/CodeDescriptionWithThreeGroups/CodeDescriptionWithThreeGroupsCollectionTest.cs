using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionWithThreeGroupsCollection))]
	sealed class CodeDescriptionWithThreeGroupsCollectionTest : CodeDescriptionWithThreeGroupsCollectionAbstractTest<CodeDescriptionWithThreeGroupsCollection>
	{
		protected override CodeDescriptionWithThreeGroupsCollection GetCollectionToTest()
		{
			var groupLookup = new CodeDescriptionPairList();
			groupLookup.AddPair("ABC", "Group A");
			groupLookup.AddPair("CDE", "Group C");
			groupLookup.AddPair("EFG", "Group E");
			groupLookup.AddPair("NA", "Not applicable");
			groupLookup.DefaultCode = "NA";
			var group2Lookup = new CodeDescriptionPairList();
			group2Lookup.AddPair("123", "Group 1");
			group2Lookup.AddPair("456", "Group 4");
			group2Lookup.AddPair("789", "Group 7");
			group2Lookup.AddPair("NA", "Not applicable");
			group2Lookup.DefaultCode = "NA";
			var group3Lookup = new CodeDescriptionPairList();
			group3Lookup.AddPair("Z1", "Group z");
			group3Lookup.AddPair("Y2", "Group y");
			group3Lookup.AddPair("X3", "Group x");
			group3Lookup.AddPair("NA", "Not applicable");
			group3Lookup.DefaultCode = "NA";

			return new CodeDescriptionWithThreeGroupsCollection(groupLookup, group2Lookup, group3Lookup, 17);
		}
	}
}
