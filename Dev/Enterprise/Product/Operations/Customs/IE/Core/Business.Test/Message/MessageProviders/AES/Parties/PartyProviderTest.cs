using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class PartyProviderTest : Customs.Business.Testing.DataProviderTestCase<PartyProvider>
	{
		public void TestNew()
		{
			CombineAssertions(() =>
			{
				AssertNull("Null - OrgAddress", PartyProvider.New((OrgAddress)null));
				AssertNull("Null - JobDocAddress", PartyProvider.New((JobDocAddress)null));
				AssertNotNull("Valid argument", Provider);
			});
		}

		public void TestUseIrishID()
		{
			var code1 = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG111", Core.Constants.CountryCodes.Germany);
			code1.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 10);
			var code2 = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG222", Core.Constants.CountryCodes.Ireland);
			code2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 11);
			var code3 = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG333", Core.Constants.CountryCodes.France);
			code3.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 09);
			var code4 = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG444", Core.Constants.CountryCodes.Swaziland);
			code4.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 12);
			AssertEquals("IEREG222", GetProvider().Id);
			code2.Delete();
			AssertEquals("FRREG333", GetProvider().Id);
			code3.Delete();
			AssertEquals("DEREG111", GetProvider().Id);
			code1.Delete();
			AssertEquals("SZREG444", GetProvider().Id);
		}

		public void TestId()
		{
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.BTW, "REG111");
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG222");
			AssertEquals("IEREG222", GetProvider().Id);
		}

		public void TestId_Consignor()
		{
			CombineAssertions(() =>
			{
				orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.BTW, "REG111");
				orgHeader.CustomsCodes.AddNew("CGT", "REG555");
				AssertEquals("CGTREG555", PartyProvider.New(orgHeader.MainAddress, PartyProvider.FallBackStyle.Consignor).Id);
				orgHeader.CustomsCodes.AddNew("ITX", "REG444");
				AssertEquals("ITXREG444", PartyProvider.New(orgHeader.MainAddress, PartyProvider.FallBackStyle.Consignor).Id);
				orgHeader.CustomsCodes.AddNew("PYE", "REG333");
				AssertEquals("PYEREG333", PartyProvider.New(orgHeader.MainAddress, PartyProvider.FallBackStyle.Consignor).Id);
				orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG222");
				AssertEquals("IEREG222", PartyProvider.New(orgHeader.MainAddress, PartyProvider.FallBackStyle.Consignor).Id);
			});
		}

		public void TestId_DocAddress()
		{
			var job = Factory.New<JobDeclaration>();
			job.JE_MessageType = JobMessageTypeList.Codes.Export;
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "VAT001");
			supplier.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EOR001");
			job.SupplierDocumentaryAddress.E2_OA_Address = supplier.MainAddress.PK;

			var provider = PartyProvider.New(job.SupplierDocumentaryAddress);
			AssertEquals("Get ID from dbo.OrgHeader EURI when created from dbo.JobDocAddress", "IEEOR001", provider.Id);
		}

		public void TestName()
		{
			orgHeader.OH_FullName = "TestHeader";
			AssertEquals("TestHeader", GetProvider().Name);
		}

		public void TestAddress()
		{
			orgHeader.MainAddress.OA_City = "TestCity";
			AssertEquals("Address", "TestCity", GetProvider().Address.City);
		}

		public void TestContact()
		{
			orgContact.OC_ContactName = "TestContact";
			AssertEquals("Contact", "TestContact", GetProvider().Contact.Name);
			var contact2 = orgHeader.Contacts.AddNew();
			contact2.OC_ContactName = "Test Contact 2";
			contact2.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
			AssertEquals("Contact", "Test Contact 2", GetProvider().Contact.Name);
		}

		protected PartyProvider GetProvider(OrgAddress address) => PartyProvider.New(address);
		protected override PartyProvider GetProvider() => GetProvider(orgHeader.MainAddress);

		protected override void SetUp()
		{
			base.SetUp();

			orgHeader = Factory.New<OrgHeader>();

			orgContact = Factory.New<OrgContact>();
			orgContact.OC_OH = orgHeader.PK;
			orgContact.OC_OA_OrgAddress = orgHeader.MainAddress.PK;
		}

		protected OrgHeader orgHeader;
		protected OrgContact orgContact;
	}
}
