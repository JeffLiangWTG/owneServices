using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.KR;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class PenaltyExemptionSessionalDataLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPenaltyExemptionCodeList()
		{
			AssertEquals("N, Y", sessionalData.Lookups.YNCodeList.CodesAsString);
		}
		protected override void SetUp()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var amendmentSessionData = new AmendmentSessionalDataCollection(instruction).AddNew();
			amendmentSessionData.CSI_Code = "A";
			sessionalData = amendmentSessionData.PenaltyExemptionSessionalData;
		}
		PenaltyExemptionSessionalData sessionalData;
	}
}
