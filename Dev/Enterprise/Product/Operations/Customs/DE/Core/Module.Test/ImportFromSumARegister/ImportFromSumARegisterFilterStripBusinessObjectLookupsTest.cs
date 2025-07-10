using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Module.Testing
{
	public class ImportFromSumARegisterFilterStripBusinessObjectLookupsTest : TestCaseWithFactory
	{
		ImportFromSumARegisterFilterStripBusinessObjectLookups lookups;

		protected override void SetUp()
		{
			base.SetUp();
			lookups = new ImportFromSumARegisterFilterStripBusinessObjectLookups(
				new ImportFromSumARegisterFilterStripBusinessObject());
		}

		public void TestOwnerReferenceTypeList_HasExpectedTypes()
		{
			CombineAssertions(() =>
			{
				var ownerReferenceTypes = lookups.OwnerReferenceTypeList;
				AssertEquals("Owner Reference Types are not as expected", "AWB, ULD, ZZZ", ownerReferenceTypes.CodesAsString);
				AssertSame("Owner Reference Types are not cached", ownerReferenceTypes, lookups.OwnerReferenceTypeList);
			});
		}
	}
}
