using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonHolderOfTheTransitProcedureWithAddressWrapperTest : WrapperHelperTest<NCTS5CommonHolderOfTheTransitProcedureWithAddressWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertNull("NctsHeader null", GetWrapper(null));

				var nctsHeader = Factory.New<NctsHeader>();
				AssertNull("NctsHeader with no org address in Principal null", GetWrapper(nctsHeader));

				var address = Factory.New<OrgAddress>();
				nctsHeader.Principal.E2_OA_Address = address.PK;
				AssertNull("NctsHeader with org address in Pricipal but no OrgHeader null", GetWrapper(nctsHeader));

				var org = Factory.New<OrgHeader>();
				address.OA_OH = org.PK;
				AssertNotNull("NctsHeader withMovementHeader and org address in Pricipal and OrgHeader associated", GetWrapper(nctsHeader));
			});
		}

		public void TestAddress()
		{
			CombineAssertions("When TIR", () =>
			{
				nctsHeader.MovementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;
				wrapper = GetWrapper(nctsHeader);
				AssertNull("Expected null Address when no TIRHolderIdentificationNumber", wrapper.Address);

				var cusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "ABC123456", "ES");
				orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				wrapper = GetWrapper(nctsHeader);
				var address = wrapper.Address;
				AssertNotNull("Expected filled Address when TIRHolderIdentificationNumber is PAS and category is NAT", address);
				AssertSame("Cached Address", wrapper.Address, address);

				var cusCode2 = orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				wrapper = GetWrapper(nctsHeader);
				AssertNull("Expected null Address when TIRHolderIdentificationNumber is not PAS (NIF) and category is NAT", wrapper.Address);

				orgHeader.OH_Category = OrgConstants.Category.Business;
				wrapper = GetWrapper(nctsHeader);
				AssertNull("Expected null Address when when TIRHolderIdentificationNumber is not PAS (NIF) and category is not NAT", wrapper.Address);

				orgHeader.CustomsCodes.RemoveAndDeleteAll();
			});

			CombineAssertions("When not TIR", () =>
			{
				nctsHeader.MovementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2;
				wrapper = GetWrapper(nctsHeader);
				AssertNull("Expected null Address when no Id", wrapper.Address);

				var cusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "ABC123456", "ES");
				orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				wrapper = GetWrapper(nctsHeader);
				var address = wrapper.Address;
				AssertNotNull("Expected filled Address when Id is PAS and category is NAT", address);
				AssertSame("Cached Address", wrapper.Address, address);

				var cusCode2 = orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				wrapper = GetWrapper(nctsHeader);
				AssertNull("Expected null Address when Id is not PAS (NIF) and category is NAT", wrapper.Address);

				orgHeader.OH_Category = OrgConstants.Category.Business;
				wrapper = GetWrapper(nctsHeader);
				AssertNull("Expected null Address when when Id is not PAS (NIF) and category is not NAT", wrapper.Address);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			var orgAddress = Factory.New<OrgAddress>();
			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgAddress.OA_OH = orgHeader.PK;

			nctsHeader.Principal.E2_OA_Address = orgAddress.PK;

			wrapper = GetWrapper(nctsHeader);
		}
		NctsHeader nctsHeader;
		OrgHeader orgHeader;
		NCTS5CommonHolderOfTheTransitProcedureWithAddressWrapper wrapper;

		NCTS5CommonHolderOfTheTransitProcedureWithAddressWrapper GetWrapper(NctsHeader header) => NCTS5CommonHolderOfTheTransitProcedureWithAddressWrapper.New(header);

		protected override NCTS5CommonHolderOfTheTransitProcedureWithAddressWrapper GetProvider() => wrapper;
	}
}
