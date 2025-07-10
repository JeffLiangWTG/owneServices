using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class GuaranteeReferenceForQueryWrapperTest : Customs.Business.Testing.DataProviderTestCase<GuaranteeReferenceForQueryWrapper>
	{
		public void TestGrn()
		{
			AssertEquals("Grn should be mapped to application specific reference of guarantee when able.", "ABC", GetProviderForGRNTest().Grn);
		}

		public void TestGrnFallbacksToPWBondNumber()
		{
			AssertEquals("Grn should be mapped to guarantee PW_BondNumber when no application specific reference of guarantee is available.", "GRN1", Provider.Grn);
		}

		public void TestGuaranteeQuery()
		{
			AssertEquals("GuaranteeQuery should be using GuaranteeQueryWrapper.", "Identifier1", Provider.GuaranteeQuery.QueryIdentifier);
		}

		public void TestOwner()
		{
			AssertEquals("Owner should be using OrganizationWrapper.", "BN CORP, FR123456789", $"{Provider.Owner.Name}, {Provider.Owner.IdentificationNumber}");
			AssertNull("Owner should be null when RequesterRole equals 1.", GetProviderForOwnerTest().Owner);
		}

		public void TestAccessCode()
		{
			AssertEquals("AccessCode should be using AccessCodeWrapper.", "1234", Provider.AccessCode.AccessCode);
		}

		protected override GuaranteeReferenceForQueryWrapper GetProvider()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			var principalOrgHeader = Factory.New<OrgHeader>();
			principalOrgHeader.OH_FullName = "BN CORP";
			principalOrgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			nctsHeader.Principal.OrganisationPK = principalOrgHeader.PK;

			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee.PW_BondType = "1";
			guarantee.PW_Password = "1234";
			guarantee.PW_BondNumber = "GRN1";

			var sendingObject = new TP5MessageSendingObject(nctsHeader);
			sendingObject.QueryIdentifier = "Identifier1";
			sendingObject.RequesterRole = "2";

			return GuaranteeReferenceForQueryWrapper.New(sendingObject, nctsHeader.MovementHeader.Guarantees[0]);
		}

		GuaranteeReferenceForQueryWrapper GetProviderForGRNTest()
		{
			EU.NCTS.Business.Testing.NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DEC001";

			var declarantAddress = declarant.Addresses.AddNew();
			declarantAddress.AddressCode = "DeclarantAddress";
			declarantAddress.Address1 = "Declarant Address";

			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.Principal.E2_OA_Address = declarantAddress.PK;
			nctsHeader.Declarant.E2_OA_Address = declarantAddress.PK;

			var sendingObject = new TP5MessageSendingObject(nctsHeader);

			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_OH_PermitHolder = declarant.PK;
			guaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			guaranteeHeader.CPH_Number = "GRN1";
			guaranteeHeader.CPH_StartDate = ZDate.Today.AddYears(-20);
			guaranteeHeader.CPH_Type = GuaranteeTypeList.Codes.COD;

			var additionalReference = guaranteeHeader.AdditionalGuaranteeReferences.AddNew();
			additionalReference.CY_Data = "ABC";
			additionalReference.CY_Code = OrgCusAccountDeltaTTypeList.Codes.TR;

			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee.PW_BondNumber = guaranteeHeader.CPH_Number;
			Factory.Save();
			return GuaranteeReferenceForQueryWrapper.New(sendingObject, nctsHeader.MovementHeader.Guarantees[0]);
		}

		GuaranteeReferenceForQueryWrapper GetProviderForOwnerTest()
		{
			var principalOrgHeader = Factory.New<OrgHeader>();
			principalOrgHeader.OH_FullName = "BN CORP";
			principalOrgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.Principal.OrganisationPK = principalOrgHeader.PK;
			nctsHeader.MovementHeader.Guarantees.AddNew();

			var sendingObject = new TP5MessageSendingObject(nctsHeader);
			sendingObject.RequesterRole = "1";

			return GuaranteeReferenceForQueryWrapper.New(sendingObject, nctsHeader.MovementHeader.Guarantees[0]);
		}
	}
}
