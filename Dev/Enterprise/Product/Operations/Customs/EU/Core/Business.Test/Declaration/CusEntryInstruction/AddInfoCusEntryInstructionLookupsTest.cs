using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class AddInfoCusEntryInstructionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestYesNoList()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			var lookups = instruction.AddInfoLookups;
			var list = lookups.YesNoList;
			Assert("Precondition: should be of type AddInfoCusEntryInstructionLookups", lookups is AddInfoCusEntryInstructionLookups);
			AssertSame("Cached", list, lookups.YesNoList);
			AssertEquals("ElementsAsString", @"N - No
Y - Yes", list.ElementsAsString);
		}
	}
}
