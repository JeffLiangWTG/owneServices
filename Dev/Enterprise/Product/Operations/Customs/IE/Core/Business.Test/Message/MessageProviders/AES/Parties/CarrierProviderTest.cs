using Enterprise.MasterFiles.Business;
using static Enterprise.MasterFiles.Business.OrgConstants;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class CarrierProviderTest : Customs.Business.Testing.DataProviderTestCase<CarrierProvider>
	{
		public void TestNew()
		{
			CombineAssertions(() =>
			{
				AssertNull("JobDocAddress - Null", CarrierProvider.New((JobDocAddress)null, true));
				AssertNull("OrgHeader - Null", CarrierProvider.New((OrgHeader)null, true));
				AssertNotNull("Valid argument", Provider);
			});
		}

		public void TestCarrierId_JobDocAddress()
		{
			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_OA_Address = org.MainAddress.PK;
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.Greece);
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.BTW, "REG111");
			var deEORICustomsCode = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG222", Core.Constants.CountryCodes.Germany);
			AssertEquals("CarrierId", "DEREG222", CarrierProvider.New(docAddress).CarrierId);

			var ieEORICustomsCode = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG333", Core.Constants.CountryCodes.Ireland);
			var szEORICustomsCode = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG444", Core.Constants.CountryCodes.Swaziland);
			AssertEquals("CarrierId", "IEREG333", CarrierProvider.New(docAddress).CarrierId);

			deEORICustomsCode.Delete();
			ieEORICustomsCode.Delete();
			szEORICustomsCode.Delete();
			AssertEquals("Use TCU - CarrierId", "GR123456789", CarrierProvider.New(docAddress, true).CarrierId);
			AssertEquals("Do not use TCU - CarrierId", string.Empty, CarrierProvider.New(docAddress, false).CarrierId);
		}

		public void TestCarrierId()
		{
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.Greece);
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.BTW, "REG111");
			var deEORICustomsCode = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG222", Core.Constants.CountryCodes.Germany);
			AssertEquals("CarrierId", "DEREG222", CarrierProvider.New(org).CarrierId);

			var ieEORICustomsCode = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG333", Core.Constants.CountryCodes.Ireland);
			var szEORICustomsCode = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG444", Core.Constants.CountryCodes.Swaziland);
			AssertEquals("CarrierId", "IEREG333", CarrierProvider.New(org).CarrierId);

			deEORICustomsCode.Delete();
			ieEORICustomsCode.Delete();
			szEORICustomsCode.Delete();
			AssertEquals("Use TCU - CarrierId", "GR123456789", CarrierProvider.New(org, true).CarrierId);
			AssertEquals("Do not use TCU - CarrierId", string.Empty, CarrierProvider.New(org, false).CarrierId);
		}

		public void TestCarrierContact()
		{
			var orgContact = org.Contacts.AddNew();
			orgContact.OC_ContactName = "ContactName";
			orgContact.OC_Phone = "0889998888";
			orgContact.OC_Email = "test@test.com";
			var allocation = orgContact.Allocations.AddNew();
			allocation.PC_Type = ContactAllocationType.CUS;
			AssertEquals("CarrierContactName", "ContactName", GetProvider().CarrierContact.Name);

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("JobDocAddress - CarrierContactName", "ContactName", CarrierProvider.New(docAddress).CarrierContact.Name);
			allocation.PC_Type = ContactAllocationType.NZCustoms;
			AssertNull("Carrier contact not found", GetProvider().CarrierContact);
		}

		protected override CarrierProvider GetProvider() => CarrierProvider.New(org);

		protected override void SetUp()
		{
			base.SetUp();
			org = Factory.New<OrgHeader>();
		}
		protected OrgHeader org;
	}
}
