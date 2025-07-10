using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(CC014ADeclarationWrapper))]
	class CC014ADeclarationWrapperTest : DeclarationWrapperAbstractTest<CC014ADeclarationWrapper>
	{
		public void TestTypeOfDeclaration()
		{
			header.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
			AssertEquals(NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration, wrapper.TypeOfDeclaration);
		}

		public void TestIsTIRDeclaration()
		{
			header.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
			AssertEquals(false, wrapper.IsTIRDeclaration);

			header.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
			AssertEquals(true, wrapper.IsTIRDeclaration);
		}

		public void TestDeclarantTIN_IsDepartureFallback()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "3333333333";
			header.Consignor.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("FR3333333333", wrapper.DeclarantTIN);
		}

		public void TestPrincipalTIN()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "4444444444";
			header.Principal.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("FR4444444444", wrapper.PrincipalTIN);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			wrapper = new CC014ADeclarationWrapper(header);
		}
		NctsHeader header;
		CC014ADeclarationWrapper wrapper;

		protected override CC014ADeclarationWrapper GetProvider() => wrapper;
	}
}
