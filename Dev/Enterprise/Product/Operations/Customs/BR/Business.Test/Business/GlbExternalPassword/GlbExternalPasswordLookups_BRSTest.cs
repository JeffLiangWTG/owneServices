using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class GlbExternalPasswordLookups_BRTest : BusinessObjectLookupsTestCase
	{
		public void TestEventIdCodeDescriptionPairList()
		{
			GlbExternalPassword_BRS parent = Factory.New<GlbExternalPassword_BRS>();
			AssertNotNull(parent.Lookups.EventIdCodeDescriptionPairList);
			AssertEquals(11, parent.Lookups.EventIdCodeDescriptionPairList.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { EventIdList.Codes.CctReleasedCargo, EventIdList.Codes.CctBlockedCargo, EventIdList.Codes.CctRedChannel, EventIdList.Codes.DuexHistoric, EventIdList.Codes.ProductCatalog,
				EventIdList.Codes.LpcoStatusChange, EventIdList.Codes.LpcoExigencyInclusion, EventIdList.Codes.LpcoExigencyCancelation, EventIdList.Codes.DuimpDiagnosis, EventIdList.Codes.DuimpRegister,
				EventIdList.Codes.DuimpStatus }, parent.Lookups.EventIdCodeDescriptionPairList.GetAllCodes());
		}
	}
}
