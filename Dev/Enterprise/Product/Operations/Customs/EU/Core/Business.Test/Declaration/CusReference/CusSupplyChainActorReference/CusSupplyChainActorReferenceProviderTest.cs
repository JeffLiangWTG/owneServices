using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class CusSupplyChainActorReferenceProviderTest : TestCaseWithFactory
	{
		public void TestGetNewValidation()
		{
			var cusSupplyChainActorReference = Factory.New<CusSupplyChainActorReference>();
			AssertType<CusSupplyChainActorReferenceValidation>(cusSupplyChainActorReference.Provider.GetNewValidation(cusSupplyChainActorReference));
		}

		public void TestGetByDataGroupingCode()
		{
			var provider = CusSupplyChainActorReferenceProvider.GetByDataGroupingCode(Core.Constants.CountryCodes.Afghanistan);
			CombineAssertions(() =>
			{
				AssertNotNull("Not Null", provider);
				AssertEquals("DataGroupingCode", Core.Constants.CountryCodes.Afghanistan, provider.DataGroupingCode);
			});
		}

		public void TestGetByDataGroupingCode_DataGroupingCodeIsEmpty()
		{
			var provider = CusSupplyChainActorReferenceProvider.GetByDataGroupingCode(ZString.Empty);
			CombineAssertions(() =>
			{
				AssertNotNull("Not Null", provider);
				AssertEquals("DataGroupingCode", ZString.Empty, provider.DataGroupingCode);
			});
		}

		public void TestOverwrittenReferenceColumnCaption()
		{
			AssertEquals(ZString.Empty, CusSupplyChainActorReferenceProvider.GetByDataGroupingCode(ZString.Empty).OverwrittenReferenceColumnCaption);
		}

		public void TestReferenceColumnCasingToUpper()
		{
			AssertEquals(ZBool.False, CusSupplyChainActorReferenceProvider.GetByDataGroupingCode(ZString.Empty).ReferenceColumnCasingToUpper);
		}

		public void TestGetReferenceFromOwner_NoValidCusCodes()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var cusSupplyChainActorReference = Factory.NewWithValidTestData<CusSupplyChainActorReference>();
			cusSupplyChainActorReference.OwnerOrgPK = orgHeader.PK;
			AssertEquals(ZString.Empty, cusSupplyChainActorReference.CFR_Reference);
		}

		public void TestGetReferenceFromOwner_Eori()
		{
			var cusSupplyChainActorReference = Factory.NewWithValidTestData<CusSupplyChainActorReference>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EOR1", Core.Constants.CountryCodes.Germany);
			cusSupplyChainActorReference.OwnerOrgPK = orgHeader.PK;
			AssertEquals("DEEOR1", cusSupplyChainActorReference.CFR_Reference);
		}

		public void TestGetReferenceFromOwner_TCU()
		{
			var cusSupplyChainActorReference = Factory.NewWithValidTestData<CusSupplyChainActorReference>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "TCU1", Core.Constants.CountryCodes.France);
			cusSupplyChainActorReference.OwnerOrgPK = orgHeader.PK;
			AssertEquals("FRTCU1", cusSupplyChainActorReference.CFR_Reference);
		}

		public void TestGetReferenceFromOwner_ExceedMaxLength()
		{
			var cusSupplyChainActorReference = Factory.NewWithValidTestData<CusSupplyChainActorReference>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, ZString.Replicate('A', 36), Core.Constants.CountryCodes.Germany);
			cusSupplyChainActorReference.OwnerOrgPK = orgHeader.PK;
			AssertEquals("DE".PadRight(35, 'A'), cusSupplyChainActorReference.CFR_Reference);
		}
	}
}
