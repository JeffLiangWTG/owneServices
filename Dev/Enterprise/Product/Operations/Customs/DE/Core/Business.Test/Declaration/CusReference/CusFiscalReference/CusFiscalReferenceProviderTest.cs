using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class CusFiscalReferenceProviderTest : TestCaseWithFactory
	{
		public void TestGetByDataGroupingCode()
		{
			var provider = EU.Business.Declaration.CusFiscalReferenceProvider.GetByDataGroupingCode(Core.Constants.CountryCodes.Germany);
			CombineAssertions(() =>
			{
				AssertType<CusFiscalReferenceProvider>("Type", provider);
				AssertEquals("DataGroupingCode", Core.Constants.CountryCodes.Germany, provider.DataGroupingCode);
			});
		}

		public void TestGetNewLookups()
		{
			AssertType<CusFiscalReferenceLookups>(cusFiscalReference.Provider.GetNewLookups(cusFiscalReference));
		}

		public void TestGetNewValidation()
		{
			AssertType<CusFiscalReferenceValidation>(cusFiscalReference.Provider.GetNewValidation(cusFiscalReference));
		}

		public void TestReferenceIsReadOnly_FR1()
		{
			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR1;
			AssertEquals("ReadOnly for FR1", true, cusFiscalReference.CFR_ReferenceInfo.ReadOnly);
		}

		public void TestReferenceIsReadOnly_FR2()
		{
			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR2;
			AssertEquals("ReadOnly for FR2", true, cusFiscalReference.CFR_ReferenceInfo.ReadOnly);
		}

		public void TestReferenceIsReadOnly_FR3()
		{
			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR3;
			AssertEquals("ReadOnly for FR3", true, cusFiscalReference.CFR_ReferenceInfo.ReadOnly);
		}

		public void TestReferenceIsReadOnly_FR5()
		{
			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR5;
			AssertEquals("ReadOnly for FR5", false, cusFiscalReference.CFR_ReferenceInfo.ReadOnly);
		}

		public void TestOwnerIsReadOnly_FR1()
		{
			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR1;
			AssertEquals("ReadOnly for FR1", false, cusFiscalReference.CFR_OA_OwnerInfo.ReadOnly);
		}

		public void TestOwnerIsReadOnly_FR2()
		{
			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR2;
			AssertEquals("ReadOnly for FR2", false, cusFiscalReference.CFR_OA_OwnerInfo.ReadOnly);
		}

		public void TestOwnerIsReadOnly_FR3()
		{
			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR3;
			AssertEquals("ReadOnly for FR3", false, cusFiscalReference.CFR_OA_OwnerInfo.ReadOnly);
		}

		public void TestOwnerIsReadOnly_FR5()
		{
			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR5;
			AssertEquals("Not ReadOnly for FR5", true, cusFiscalReference.CFR_OA_OwnerInfo.ReadOnly);
		}

		public void TestRecalculateOwnerIfNeeded_FR1()
		{
			declaration.JE_OA_DeclarantAddress = orgAddress.PK;
			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR1;
			AssertEquals(orgAddress.PK, cusFiscalReference.CFR_OA_Owner);
		}

		public void TestRecalculateOwnerIfNeeded_FR2()
		{
			cusFiscalReference.CFR_Code = ZString.Empty;
			declaration.AcquirerDocAddress.E2_OA_Address = orgAddress.PK;
			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR2;
			AssertEquals(orgAddress.PK, cusFiscalReference.CFR_OA_Owner);
		}

		public void TestRecalculateOwnerIfNeeded_FR3()
		{
			declaration.JE_OA_Representative = orgAddress.PK;
			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR3;
			AssertEquals(orgAddress.PK, cusFiscalReference.CFR_OA_Owner);
		}

		public void TestRecalculateReferenceIfNeeded_FR1()
		{
			declaration.JE_OA_DeclarantAddress = orgAddress.PK;
			organisation.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.UST, "VAT903", Core.Constants.CountryCodes.Germany);

			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR1;
			AssertEquals("DEVAT903", cusFiscalReference.CFR_Reference);
		}

		public void TestRecalculateReferenceIfNeeded_FR2()
		{
			cusFiscalReference.CFR_Code = ZString.Empty;
			declaration.AcquirerDocAddress.E2_OA_Address = orgAddress.PK;
			organisation.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.UST, "VAT903", Core.Constants.CountryCodes.Germany);

			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR2;
			AssertEquals("DEVAT903", cusFiscalReference.CFR_Reference);
		}

		public void TestRecalculateReferenceIfNeeded_FR3()
		{
			declaration.JE_OA_Representative = orgAddress.PK;
			organisation.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.UST, "VAT903", Core.Constants.CountryCodes.Germany);

			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR3;
			AssertEquals("DEVAT903", cusFiscalReference.CFR_Reference);
		}

		public void TestRecalculateReferenceIfNeeded_FR1_NoDeVatCode()
		{
			declaration.JE_OA_DeclarantAddress = orgAddress.PK;
			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR1;

			AssertEquals("No DE UST Code set", ZString.Empty, cusFiscalReference.CFR_Reference);
		}

		public void TestRecalculateReferenceIfNeeded_FR2_NoDeVatCode()
		{
			declaration.AcquirerDocAddress.E2_OA_Address = orgAddress.PK;
			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR2;

			AssertEquals("No DE UST Code set", ZString.Empty, cusFiscalReference.CFR_Reference);
		}

		public void TestRecalculateReferenceIfNeeded_FR3_NoDeVatCode()
		{
			declaration.JE_OA_Representative = orgAddress.PK;
			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR3;

			AssertEquals("No DE UST Code set", ZString.Empty, cusFiscalReference.CFR_Reference);
		}

		public void TestRecalculateReferenceIfNeeded_FR1_FrenchOrg_FrVatCode()
		{
			var (frenchOrg, frenchAddress) = CreateOrgAndAddressFromCountry(Core.Constants.CountryCodes.France);

			declaration.JE_OA_DeclarantAddress = frenchAddress.PK;
			frenchOrg.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "123456789", Core.Constants.CountryCodes.France);

			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR1;

			AssertEquals("French TVA Code not valid for FR1", ZString.Empty, cusFiscalReference.CFR_Reference);
		}

		public void TestRecalculateReferenceIfNeeded_FR2_FrenchOrg_FrVatCode()
		{
			var (frenchOrg, frenchAddress) = CreateOrgAndAddressFromCountry(Core.Constants.CountryCodes.France);

			declaration.AcquirerDocAddress.E2_OA_Address = frenchAddress.PK;
			frenchOrg.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "123456789", Core.Constants.CountryCodes.France);

			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR2;

			AssertEquals("French TVA Code valid for FR2", "FR123456789", cusFiscalReference.CFR_Reference);
		}

		public void TestRecalculateReferenceIfNeeded_FR3_FrenchOrg_FrVatCode()
		{
			var (frenchOrg, frenchAddress) = CreateOrgAndAddressFromCountry(Core.Constants.CountryCodes.France);

			declaration.JE_OA_Representative = frenchAddress.PK;
			frenchOrg.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "123456789", Core.Constants.CountryCodes.France);

			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR3;

			AssertEquals("French TVA Code not valid for FR3", ZString.Empty, cusFiscalReference.CFR_Reference);
		}

		public void TestRecalculateReferenceIfNeeded_FR1_SwissOrg_DeVatCode()
		{
			var (chOrg, chAddress) = CreateOrgAndAddressFromCountry(Core.Constants.CountryCodes.Switzerland);

			declaration.JE_OA_DeclarantAddress = chAddress.PK;
			chOrg.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.UST, "123456789", Core.Constants.CountryCodes.Germany);

			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR1;

			AssertEquals("DE UST valid for Swiss Org FR1", "DE123456789", cusFiscalReference.CFR_Reference);
		}

		public void TestRecalculateReferenceIfNeeded_FR2_SwissOrg_DeVatCode()
		{
			var (swissOrg, swissAddress) = CreateOrgAndAddressFromCountry(Core.Constants.CountryCodes.Switzerland);

			declaration.AcquirerDocAddress.E2_OA_Address = swissAddress.PK;
			swissOrg.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.UST, "123456789", Core.Constants.CountryCodes.Germany);

			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR2;

			AssertEquals("DE UST not valid for Swiss Org FR2", ZString.Empty, cusFiscalReference.CFR_Reference);
		}

		public void TestRecalculateReferenceIfNeeded_FR3_SwissOrg_DeVatCode()
		{
			var (swissOrg, swissAddress) = CreateOrgAndAddressFromCountry(Core.Constants.CountryCodes.Switzerland);

			declaration.JE_OA_Representative = swissAddress.PK;
			swissOrg.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.UST, "123456789", Core.Constants.CountryCodes.Germany);

			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR3;

			AssertEquals("DE UST valid for Swiss Org FR3", "DE123456789", cusFiscalReference.CFR_Reference);
		}

		(OrgHeader, OrgAddress) CreateOrgAndAddressFromCountry(string countryCode)
		{
			var org = Factory.New<OrgHeader>();
			var address = org.Addresses.AddNew();
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			return (org, address);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			cusFiscalReference = entryInstruction.FiscalReferences.AddNew();
			organisation = Factory.NewWithValidTestData<OrgHeader>();
			orgAddress = organisation.Addresses.AddNew();
		}
		JobDeclaration declaration;
		CusFiscalReference cusFiscalReference;
		OrgHeader organisation;
		OrgAddress orgAddress;
	}
}
