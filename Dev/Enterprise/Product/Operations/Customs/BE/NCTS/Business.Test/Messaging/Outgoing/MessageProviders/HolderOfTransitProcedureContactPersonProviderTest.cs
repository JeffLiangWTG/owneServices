using Enterprise.Customs.BE.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	sealed class HolderOfTransitProcedureContactPersonProviderTest : Customs.Business.Testing.DataProviderTestCase<HolderOfTransitProcedureContactPersonProvider>
	{
		public void TestNewOrNull_AddressNull()
		{
			AssertNull(HolderOfTransitProcedureContactPersonProvider.NewOrNull(null));
		}

		public void TestNewOrNull_AllPropertiesEmpty()
		{
			AssertNull(HolderOfTransitProcedureContactPersonProvider.NewOrNull(Factory.New<JobDocAddress>()));
		}

		public void TestName()
		{
			AssertEquals("LiuHuaQiang", provider.Name);
		}

		public void TestPhoneNumber()
		{
			AssertEquals("110110", provider.PhoneNumber);
		}

		public void TestPhoneNumber_FromAddress()
		{
			principal.Contacts[0].OC_Phone = "";
			AssertEquals("120120", provider.PhoneNumber);
		}

		public void TestEMailAddress()
		{
			AssertEquals("EMailAddress", provider.EMailAddress);
		}

		public void TestEMailAddress_FromAddress()
		{
			principal.Contacts[0].OC_Email = "";
			AssertEquals("1099176692@qq.com", provider.EMailAddress);
		}

		public void TestPropertiesWithAdditionalAccessCode()
		{
			var additionalAccessCode = guaranteeHeader.AdditionalAccessCodes.AddNew();
			additionalAccessCode.CPR_ValueFrom = "BBBB";
			additionalAccessCode.CPR_Description = "NameAdditional";
			NCTSTestHelper.CreateContactForTest(principal, "NameAdditional", "PhoneNrAdditional", "EMailAddressAdditional", "");
			guarantee.PW_Password = "BBBB";
			provider = HolderOfTransitProcedureContactPersonProvider.NewOrNull(nctsHeader.Principal);

			CombineAssertions(() =>
			{
				AssertEquals("Name", "NameAdditional", provider.Name);
				AssertEquals("Phone from contact", "PhoneNrAdditional", provider.PhoneNumber);
				AssertEquals("EMail", "EMailAddressAdditional", provider.EMailAddress);
				principal.Contacts[1].OC_Phone = "";
				AssertEquals("Phone from address", "120120", provider.PhoneNumber);
			});
		}

		public void TestNoGuarantee()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var principle = nctsHeader.Principal;
			var principleOrg = NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PRC", principle, "1");
			principle.Address.OA_Email = "1099176691@qq.com";
			var contact1 = principleOrg.Contacts.AddNew();
			contact1.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
			contact1.OC_IsActive = true;
			contact1.OC_ContactName = "BaBa";
			contact1.OC_Phone = "57234997";
			var contact2 = principleOrg.Contacts.AddNew();
			contact2.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CNCUS;
			contact2.OC_IsActive = true;
			contact2.OC_ContactName = "YeYe";
			var provider = HolderOfTransitProcedureContactPersonProvider.NewOrNull(principle);
			AssertEquals("test name", "BaBa", provider.Name);
			AssertEquals("test Email", "1099176691@qq.com", provider.EMailAddress);
			AssertEquals("test phone", "57234997", provider.PhoneNumber);
		}

		public void TestName_CaseSensitive()
		{
			guaranteeHeader.MainAccessPersonName = "liuhuaqiang";
			provider = HolderOfTransitProcedureContactPersonProvider.NewOrNull(nctsHeader.Principal);
			AssertEquals("Name", "liuhuaqiang", provider.Name);

			var additionalAccessCode = guaranteeHeader.AdditionalAccessCodes.AddNew();
			additionalAccessCode.CPR_ValueFrom = "BBBB";
			additionalAccessCode.CPR_Description = "name_casesensitive";
			NCTSTestHelper.CreateContactForTest(principal, "Name_CaseSensitive", "PhoneNrAdditional", "EMailAddressAdditional", "");
			guarantee.PW_Password = "BBBB";
			provider = HolderOfTransitProcedureContactPersonProvider.NewOrNull(nctsHeader.Principal);

			CombineAssertions(() =>
			{
				AssertEquals("Name", "name_casesensitive", provider.Name);
				AssertEquals("Phone from contact", "PhoneNrAdditional", provider.PhoneNumber);
				AssertEquals("EMail", "EMailAddressAdditional", provider.EMailAddress);
				principal.Contacts[1].OC_Phone = "";
				AssertEquals("Phone from address", "120120", provider.PhoneNumber);
			});
		}
		
		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
			principal = NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PRC", nctsHeader.Principal, "1", phoneNumber: "120120", contactPhone: "110110",  contactName: "LiuHuaQiang", contactEmail: "EMailAddress");
			nctsHeader.Principal.Address.OA_Email = "1099176692@qq.com";
			guaranteeHeader = (CusGuaranteeHeader)NCTSTestHelper.SetupGuarantee(principal);
			guaranteeHeader.MainAccessCode = "AAAA";
			guaranteeHeader.MainAccessPersonName = "LiuHuaQiang";
			guaranteeHeader.CPH_SubType = "0";
			guarantee = (NctsGuarantee)NCTSTestHelper.CreateGuaranteeForTest(nctsHeader, "0", "1234", "REF", "AAAA", "LO");
			provider = HolderOfTransitProcedureContactPersonProvider.NewOrNull(nctsHeader.Principal);
		}

		HolderOfTransitProcedureContactPersonProvider provider;
		OrgHeader principal;
		CusGuaranteeHeader guaranteeHeader;
		NctsHeader nctsHeader;
		NctsGuarantee guarantee;

		protected override HolderOfTransitProcedureContactPersonProvider GetProvider() => provider;
	}
}
