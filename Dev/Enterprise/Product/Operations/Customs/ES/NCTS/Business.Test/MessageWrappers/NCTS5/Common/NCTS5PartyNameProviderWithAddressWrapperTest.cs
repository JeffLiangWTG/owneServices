using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5PartyNameProviderWithAddressWrapperTest : WrapperHelperTest<NCTS5PartyNameProviderWithAddressWrapper>
	{
		public void TestGetNewNCTS5PartyNameProviderWithAddressWrapper()
		{
			CombineAssertions(() =>
			{
				AssertNull("JobDocAddress null", NCTS5PartyNameProviderWithAddressWrapper.New(null));

				var docAddress = Factory.New<JobDocAddress>();
				AssertNull("JobDocAddress with no org address null", NCTS5PartyNameProviderWithAddressWrapper.New(docAddress));

				var address = Factory.New<OrgAddress>();
				docAddress.E2_OA_Address = address.PK;
				AssertNull("JobDocAddress with org address but no OrgHeader null", NCTS5PartyNameProviderWithAddressWrapper.New(docAddress));

				var org = Factory.New<OrgHeader>();
				address.OA_OH = org.PK;
				AssertNotNull("OrgAddress not null", NCTS5PartyNameProviderWithAddressWrapper.New(address));
				AssertNotNull("JobDocAddress not null", NCTS5PartyNameProviderWithAddressWrapper.New(docAddress, true));
			});
		}

		public void TestId()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty ID when no id declared (EOR, NIF or PAS)", ZString.Empty, wrapper.Id);
				putOverrideToTrue();
				wrapper = GetWrapper(docAddress);
				AssertEquals("Expected empty ID when no id declared (EOR, NIF or PAS), but is override", ZString.Empty, wrapper.Id);

				OrgCusCode cusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "ES");
				docAddress.E2_AddressOverride = false;
				wrapper = GetWrapper(docAddress);
				AssertEquals("Expected filled ID when id declared EOR", "ES22222222", wrapper.Id);
				putOverrideToTrue();
				wrapper = GetWrapper(docAddress);
				AssertEquals("Expected empty ID when id declared EOR, but is override", ZString.Empty, wrapper.Id);

				cusCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
				docAddress.E2_AddressOverride = false;
				wrapper = GetWrapper(docAddress);
				AssertEquals("Expected filled ID when id declared NIF", "ES22222222", wrapper.Id);
				putOverrideToTrue();
				wrapper = GetWrapper(docAddress);
				AssertEquals("Expected empty ID when id declared NIF, but is override", ZString.Empty, wrapper.Id);

				cusCode.OK_CodeType = OrgCusCode.CodeTypes.PassportID;
				docAddress.E2_AddressOverride = false;
				wrapper = GetWrapper(docAddress);
				AssertEquals("Expected filled ID when id declared PAS", "22222222", wrapper.Id);
				putOverrideToTrue();
				wrapper = GetWrapper(docAddress);
				AssertEquals("Expected empty ID when id declared PAS, but is override", ZString.Empty, wrapper.Id);
			});
		}

		public void TestAddress()
		{
			CombineAssertions(() =>
			{
				var cusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "ABC123456", "ES");
				orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				wrapper = GetWrapper(docAddress);
				var address = wrapper.Address;
				AssertNotNull("Expected filled Address when Id is PAS and category is NAT", address);
				AssertSame("Cached Address", wrapper.Address, address);

				var cusCode2 = orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				wrapper = GetWrapper(docAddress);
				AssertNull("Expected null Address when Id is not PAS (NIF) and category is NAT", wrapper.Address);
				putOverrideToTrue();
				wrapper = GetWrapper(docAddress);
				AssertNotNull("Expected not null Address when Id is not PAS (NIF) and category is NAT, but is override", wrapper.Address);
				docAddress.E2_AddressOverride = false;

				orgHeader.OH_Category = OrgConstants.Category.Business;
				wrapper = GetWrapper(docAddress);
				AssertNull("Expected null Address when when Id is not PAS (NIF) and category is not NAT", wrapper.Address);
				putOverrideToTrue();
				wrapper = GetWrapper(docAddress);
				AssertNotNull("Expected not null Address when when Id is not PAS (NIF) and category is not NAT, but is override", wrapper.Address);

				orgHeader.CustomsCodes.RemoveAndDeleteAll();
				AssertNotNull("Expected filled Address when no id declared", wrapper.Address);
			});
		}

		public void TestName()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected filled Name when no id declared (EOR, NIF or PAS)", "Org Name", wrapper.Name);
				putOverrideToTrue();
				wrapper = GetWrapper(docAddress);
				AssertEquals("Expected filled Name when no id declared (EOR, NIF or PAS), but is override", "Name Override", wrapper.Name);

				docAddress.E2_AddressOverride = false;
				OrgCusCode cusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "ES");
				wrapper = GetWrapper(docAddress);
				AssertEquals("Expected empty Name when id declared EOR", ZString.Empty, wrapper.Name);
				putOverrideToTrue();
				wrapper = GetWrapper(docAddress);
				AssertEquals("Expected filled Name when id declared EOR, but is override", "Name Override", wrapper.Name);

				docAddress.E2_AddressOverride = false;
				cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber;
				wrapper = GetWrapper(docAddress);
				AssertEquals("Expected filled Name when id declared but not EOR nor NIF nor PAS", "Org Name", wrapper.Name);
				putOverrideToTrue();
				wrapper = GetWrapper(docAddress);
				AssertEquals("Expected filled Name when id declared but not EOR nor NIF nor PAS, but is override", "Name Override", wrapper.Name);

				docAddress.E2_AddressOverride = false;
				cusCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
				wrapper = GetWrapper(docAddress);
				AssertEquals("Expected empty Name when id declared NIF", ZString.Empty, wrapper.Name);
				putOverrideToTrue();
				wrapper = GetWrapper(docAddress);
				AssertEquals("Expected filled Name when id declared NIF, but is override", "Name Override", wrapper.Name);

				docAddress.E2_AddressOverride = false;
				cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.ImportOneStopShopVatRegistration;
				wrapper = GetWrapper(docAddress);
				AssertEquals("Expected filled Name when id declared but not EOR nor NIF nor PAS", "Org Name", wrapper.Name);
				putOverrideToTrue();
				wrapper = GetWrapper(docAddress);
				AssertEquals("Expected filled Name when id declared but not EOR nor NIF nor PAS, but is override", "Name Override", wrapper.Name);

				docAddress.E2_AddressOverride = false;
				cusCode.OK_CodeType = OrgCusCode.CodeTypes.PassportID;
				wrapper = GetWrapper(docAddress);
				AssertEquals("Expected empty Name when id declared PAS", ZString.Empty, wrapper.Name);
				putOverrideToTrue();
				wrapper = GetWrapper(docAddress);
				AssertEquals("Expected filled Name when id declared PAS, but is override", "Name Override", wrapper.Name);
			});
		}

		public void TestAddressFields()
		{
			CombineAssertions(() =>
			{
				var cusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "ABC123456", "ES");
				orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				wrapper = GetWrapper(docAddress);
				var address = wrapper.Address;
				AssertNotNull("Expected filled Address when Id is PAS and category is NAT", address);
				AssertSame("Cached Address", wrapper.Address, address);

				wrapper = GetWrapper(docAddress);
				address = wrapper.Address;
				AssertEquals("Expected Address when id declared PAS and not Override", "Address", address.StreetAndNumber);
				AssertEquals("Expected Post Code when id declared PAS and not Override", "PostCode", address.PostCode);
				AssertEquals("Expected City when id declared PAS and not Override", "City", address.City);

				putOverrideToTrue();
				wrapper = GetWrapper(docAddress);
				address = wrapper.Address;
				AssertEquals("Expected Address when id declared PAS and is Override", "Address Override 012345678901234567890123456789012", address.StreetAndNumber);
				AssertEquals("Expected Post Code when id declared PAS and is Override", "Post Over", address.PostCode);
				AssertEquals("Expected City when id declared PAS and is Override", "City Over", address.City);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			docAddress = Factory.New<JobDocAddress>();
			var orgAddress = Factory.New<OrgAddress>();
			docAddress.E2_OA_Address = orgAddress.PK;
			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "AAAA";
			orgHeader.OH_FullName = "Org Name";
			orgAddress.Address1 = "Address";
			orgAddress.Postcode = "PostCode";
			orgAddress.City = "City";

			orgAddress.OA_OH = orgHeader.PK;

			wrapper = GetWrapper(docAddress);
		}
		JobDocAddress docAddress;
		OrgHeader orgHeader;
		NCTS5PartyNameProviderWithAddressWrapper wrapper;

		void putOverrideToTrue()
		{
			docAddress.E2_AddressOverride = true;
			docAddress.E2_CompanyName = "Name Override";
			docAddress.E2_Address1AndE2_Address2 = "Address Override 012345678901234567890123456789012AAAAAAAAAA";
			docAddress.E2_Postcode = "Post Over";
			docAddress.E2_City = "City Over";
		}

		NCTS5PartyNameProviderWithAddressWrapper GetWrapper(JobDocAddress docAddress) => NCTS5PartyNameProviderWithAddressWrapper.New(docAddress);

		protected override NCTS5PartyNameProviderWithAddressWrapper GetProvider() => wrapper;
	}
}
