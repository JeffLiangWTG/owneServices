using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.KR;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class AmendmentSessionalDataLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDutyPenaltyCause()
		{
			AssertEquals("01, 02, 0A, 03, 04, 0B, 05", sessionalData.Lookups.DutyPenaltyCauseList.CodesAsString);
		}

		public void TestTaxPenaltyCause()
		{
			AssertEquals("01, 02, 0A, 03, 04, 0B, 05", sessionalData.Lookups.TaxPenaltyCauseList.CodesAsString);
		}
		protected override void SetUp()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var amendmentSessionDataCollection = new AmendmentSessionalDataCollection(instruction);
			sessionalData = amendmentSessionDataCollection.AddNew();
		}
		AmendmentSessionalData sessionalData;
	}
}
