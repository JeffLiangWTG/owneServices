using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Business.Testing
{
	class TaxChangeAssessmentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryStatusList()
		{
			var taxChangeAssessment = Factory.New<TaxChangeAssessment>();
			AssertEquals("CodesAsString", "OPN, PRG, PRS, CAN", taxChangeAssessment.Lookups.EntryStatusList.CodesAsString);
		}
	}
}
