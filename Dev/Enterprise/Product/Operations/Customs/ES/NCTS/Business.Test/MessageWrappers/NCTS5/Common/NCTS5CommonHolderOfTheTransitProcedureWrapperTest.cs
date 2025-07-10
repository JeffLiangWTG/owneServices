using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonHolderOfTheTransitProcedureWrapperTest : WrapperHelperTest<NCTS5CommonHolderOfTheTransitProcedureWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertNull("NctsHeader null", NCTS5CommonHolderOfTheTransitProcedureWrapper.New(null));

				var nctsHeader = Factory.New<NctsHeader>();
				AssertNull("NctsHeader with no org address in Principal null", NCTS5CommonHolderOfTheTransitProcedureWrapper.New(nctsHeader));

				var address = Factory.New<OrgAddress>();
				nctsHeader.Principal.E2_OA_Address = address.PK;
				AssertNull("NctsHeader with org address in Pricipal but no OrgHeader null", NCTS5CommonHolderOfTheTransitProcedureWrapper.New(nctsHeader));

				var org = Factory.New<OrgHeader>();
				nctsHeader.Principal.OrganisationPK = org.PK;
				AssertNotNull("NctsHeader with org address in Pricipal and OrgHeader associated", NCTS5CommonHolderOfTheTransitProcedureWrapper.New(nctsHeader));
			});
		}

		public void TestTIRHolderIdentificationNumber()
		{
			CombineAssertions(() =>
			{
				nctsHeader.MovementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;
				wrapper = NCTS5CommonHolderOfTheTransitProcedureWrapper.New(nctsHeader);

				AssertEquals("Expected empty TIRHolderIdentificationNumber", ZString.Empty, wrapper.TIRHolderIdentificationNumber);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
				AssertEquals("Expected PAS TIRHolderIdentificationNumber with country code when no NIF or EORI declared", "GB333333333", wrapper.TIRHolderIdentificationNumber);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				AssertEquals("Expected NIF TIRHolderIdentificationNumber", "NIF22222222", wrapper.TIRHolderIdentificationNumber);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeader.OH_Category = OrgConstants.Category.Government;
				AssertEquals("Expected Country+NIF for non NAT Organizations", "ESNIF22222222", wrapper.TIRHolderIdentificationNumber);

				OrgCusCode eoriCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
				AssertEquals("Expected EORI TIRHolderIdentificationNumber with country code", "FR22222222", wrapper.TIRHolderIdentificationNumber);

				eoriCusCode.OK_CustomsRegNo = "ES22222222";
				eoriCusCode.OK_RN_NKCodeCountry = "ES";
				AssertEquals("Expected EORI TIRHolderIdentificationNumber with country code not repeated", "ES22222222", wrapper.TIRHolderIdentificationNumber);

				nctsHeader.MovementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2;
				wrapper = NCTS5CommonHolderOfTheTransitProcedureWrapper.New(nctsHeader);
				AssertEquals("Expected empty TIRHolderIdentificationNumber when BM_InBondEntryType != TIR", ZString.Empty, wrapper.TIRHolderIdentificationNumber);
			});
		}

		public void TestId()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty Id", ZString.Empty, wrapper.Id);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
				AssertEquals("Expected PAS Id with country code when no NIF or EORI declared", "GB333333333", wrapper.Id);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				AssertEquals("Expected NIF Id", "NIF22222222", wrapper.Id);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeader.OH_Category = OrgConstants.Category.Government;
				AssertEquals("Expected Country+NIF for non NAT Organizations", "ESNIF22222222", wrapper.Id);

				OrgCusCode eoriCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
				AssertEquals("Expected EORI Id with country code", "FR22222222", wrapper.Id);

				eoriCusCode.OK_CustomsRegNo = "ES22222222";
				eoriCusCode.OK_RN_NKCodeCountry = "ES";
				AssertEquals("Expected EORI Id with country code not repeated", "ES22222222", wrapper.Id);

				nctsHeader.MovementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;
				wrapper = NCTS5CommonHolderOfTheTransitProcedureWrapper.New(nctsHeader);
				AssertEquals("Expected empty Id when BM_InBondEntryType == TIR", ZString.Empty, wrapper.Id);
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

			wrapper = NCTS5CommonHolderOfTheTransitProcedureWrapper.New(nctsHeader);
		}
		NctsHeader nctsHeader;
		OrgHeader orgHeader;
		NCTS5CommonHolderOfTheTransitProcedureWrapper wrapper;

		protected override NCTS5CommonHolderOfTheTransitProcedureWrapper GetProvider() => wrapper;
	}
}
