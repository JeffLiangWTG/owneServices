using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(CC141ADeclarationWrapper))]
	class CC141ADeclarationWrapperTest : DeclarationWrapperAbstractTest<CC141ADeclarationWrapper>
	{
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
			cusCode.OK_CustomsRegNo = "2222222222";
			header.Consignor.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("FR2222222222", wrapper.DeclarantTIN);
		}

		public void TestPrincipalTIN()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "3333333333";
			header.Principal.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("FR3333333333", wrapper.PrincipalTIN);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			wrapper = new CC141ADeclarationWrapper(header);
		}
		CC141ADeclarationWrapper wrapper;
		NctsHeader header;

		protected override CC141ADeclarationWrapper GetProvider() => wrapper;
	}
}
