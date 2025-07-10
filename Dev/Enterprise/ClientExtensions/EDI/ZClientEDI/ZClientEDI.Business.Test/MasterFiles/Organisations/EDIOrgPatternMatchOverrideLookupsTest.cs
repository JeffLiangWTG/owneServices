using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	class EDIOrgPatternMatchOverrideLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookupsType()
		{
			var pattern = Factory.New<EDIOrgPatternMatchOverride>();
			AssertType(typeof(EDIOrgPatternMatchOverrideLookups), pattern.Lookups);
		}

		public void TestOO_Relationship_List()
		{
			var pattern = Factory.New<EDIOrgPatternMatchOverride>();
			var list = pattern.Lookups.OO_Relationship_List;
			AssertEquals("All base relationships are available", 18, list.Count);
			AssertEquals("eHub Client ID", list.GetDescriptionFromCode(EDIConstants.OrgPatternMatchOverrideRelationships.EHubClientID));
		}

		public void TestIsNotCode()
		{
			var pattern = Factory.New<EDIOrgPatternMatchOverride>();
			pattern.OO_Relationship = EDIConstants.OrgPatternMatchOverrideRelationships.EHubClientID;
			AssertEquals(false, pattern.IsNotCode);

			pattern.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Organisation;
			AssertEquals(true, pattern.IsNotCode);
		}
	}
}
