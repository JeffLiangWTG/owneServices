using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	public class CusTempSupplyChainActorReferenceProviderTest : TestCaseWithFactory
	{
		public void TestGetNewValidation()
		{
			var cusSupplyChainActorReference = Factory.New<CusSupplyChainActorReference>();
			AssertType<CusSupplyChainActorReferenceValidation>(cusSupplyChainActorReference.Provider.GetNewValidation(cusSupplyChainActorReference));//?????
		}

		public void TestGetReferenceFromOwner_NoValidCusCodes()
		{
			var supplyChainActor = Factory.NewWithValidTestData<CusSupplyChainActorReference>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			supplyChainActor.OwnerOrgPK = orgHeader.PK;
			AssertEquals(ZString.Empty, supplyChainActor.CFR_Reference);
		}

		public void TestGetReferenceFromOwner_CurrentCountry()
		{
			var supplyChainActor = Factory.NewWithValidTestData<CusSupplyChainActorReference>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EOR1", Core.Constants.CountryCodes.Latvia);
			supplyChainActor.OwnerOrgPK = orgHeader.PK;
			AssertEquals("LVEOR1", supplyChainActor.CFR_Reference);
		}

		public void TestGetReferenceFromOwner_OtherCountry()
		{
			var supplyChainActor = Factory.NewWithValidTestData<CusSupplyChainActorReference>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EOR1", Core.Constants.CountryCodes.Germany);
			supplyChainActor.OwnerOrgPK = orgHeader.PK;
			AssertEquals("DEEOR1", supplyChainActor.CFR_Reference);
		}
	}
}
