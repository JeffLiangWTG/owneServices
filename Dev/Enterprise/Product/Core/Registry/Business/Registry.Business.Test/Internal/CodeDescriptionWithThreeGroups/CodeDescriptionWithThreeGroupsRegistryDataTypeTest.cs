using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionWithThreeGroupsRegistryDataType))]
	sealed class CodeDescriptionWithThreeGroupsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CodeDescriptionWithThreeGroupsRegistryDataType>
	{
		protected override CodeDescriptionWithThreeGroupsRegistryDataType GetNewDataType()
		{
			return new CodeDescriptionWithThreeGroupsRegistryDataType(3);
		}

		protected override string ExpectedEditorName
		{
			get { return null; }
		}

		// Using custom EditorInfo.
		protected override bool HasEditor
		{
			get { return false; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var groups = new CodeDescriptionPairList();
			groups.AddPair("ABC", "Group 1");
			groups.AddPair("CDE", "Group 2");
			groups.AddPair("EFG", "Group 3");
			var groups2 = new CodeDescriptionPairList();
			groups2.AddPair("AB1", "Group 11");
			groups2.AddPair("CD1", "Group 21");
			groups2.AddPair("EF1", "Group 31");
			var groups3 = new CodeDescriptionPairList();
			groups3.AddPair("AB2", "Group 12");
			groups3.AddPair("CD2", "Group 22");
			groups3.AddPair("EF2", "Group 32");

			groups.DefaultCode = "ABC";
			groups2.DefaultCode = "CD1";
			groups3.DefaultCode = "EF2";

			var collection1 = new CodeDescriptionWithThreeGroupsCollection(groups, groups2, groups3, 17);
			var element = collection1.AddNew();
			element.Code = "AA1";
			element.Group = "CDE";
			element.Group2 = "CD1";
			element.Group3 = "CD2";
			element = collection1.AddNew();
			element.Code = "BB2";
			element.Group = "EFG";
			element.Group2 = "EF1";
			element.Group3 = "EF2";

			groups.DefaultCode = "CDE";
			groups2.DefaultCode = "EF1";
			groups3.DefaultCode = "CD2";

			var collection2 = new CodeDescriptionWithThreeGroupsCollection(groups, groups2, groups3, 17);
			element = collection2.AddNew();
			element.Code = "AA2";
			element.Group = "CDE";
			element.Group2 = "CD1";
			element.Group3 = "CD2";

			var string1 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfCodeDescriptionWithThreeGroups xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><CodeDescriptionWithThreeGroups><CodeMaxLength>17</CodeMaxLength><Code>AA1</Code><Description /><Group>CDE</Group><SystemDefined>False</SystemDefined><Group2>CD1</Group2><Group3>CD2</Group3></CodeDescriptionWithThreeGroups><CodeDescriptionWithThreeGroups><CodeMaxLength>17</CodeMaxLength><Code>BB2</Code><Description /><Group>EFG</Group><SystemDefined>False</SystemDefined><Group2>EF1</Group2><Group3>EF2</Group3></CodeDescriptionWithThreeGroups></ArrayOfCodeDescriptionWithThreeGroups>";
			var byteArray1 = System.Text.Encoding.Unicode.GetBytes(string1);

			var string2 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfCodeDescriptionWithThreeGroups xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><CodeDescriptionWithThreeGroups><CodeMaxLength>17</CodeMaxLength><Code>AA2</Code><Description /><Group>CDE</Group><SystemDefined>False</SystemDefined><Group2>CD1</Group2><Group3>CD2</Group3></CodeDescriptionWithThreeGroups></ArrayOfCodeDescriptionWithThreeGroups>";
			var byteArray2 = System.Text.Encoding.Unicode.GetBytes(string2);

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection1, byteArray1),
				new ValidSampleAndBinaryValueInDB(collection2, byteArray2)
			};
		}
	}
}
