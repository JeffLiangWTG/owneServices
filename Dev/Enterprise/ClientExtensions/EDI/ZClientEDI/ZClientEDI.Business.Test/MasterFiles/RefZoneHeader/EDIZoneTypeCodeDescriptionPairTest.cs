using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	public class EDIZoneTypeCodeDescriptionPairTest : TestCase
	{
		public void TestTraining()
		{
			EDIZoneTypeCodePairList list = new EDIZoneTypeCodePairList();

			Assert("There should be Training code in list", list.ContainsCode(EDIRefZoneHeaderLookups.EDIZoneTypeCodes.Training));
			AssertEquals("Training Module Only", list.GetDescriptionFromCode(EDIRefZoneHeaderLookups.EDIZoneTypeCodes.Training));
		}
	}
}