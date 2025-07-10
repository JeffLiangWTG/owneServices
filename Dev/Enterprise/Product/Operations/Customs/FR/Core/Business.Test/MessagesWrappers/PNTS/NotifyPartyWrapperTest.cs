using System.Linq;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	class NotifyPartyWrapperTest : Customs.Business.Testing.DataProviderTestCase<NotifyPartyWrapper>
	{
		public void TestAddress()
		{
			AssertEquals("City should equal OA_City", "Insomnia", Provider.Address.City);
			AssertEquals("Country should equal OA_RN_NKCountryCode", "LS", Provider.Address.Country);
			AssertEquals("waiting to be done in the future", string.Empty, Provider.Address.Number);
			AssertEquals("waiting to be done in the future", string.Empty, Provider.Address.PoBox);
			AssertEquals("PostCode should equal OA_PostCode", "000000", Provider.Address.PostCode);
			AssertEquals("Street should equal OA_Address1", "Kings Street", Provider.Address.Street);
			AssertEquals("StreetAdditionalLine should equal OA_Address2", "No.001", Provider.Address.StreetAdditionalLine);
			AssertEquals("waiting to be done in the future", string.Empty, Provider.Address.SubDivision);
		}

		public void TestCommunication()
		{
			AssertEquals("Communication has 2 elements", 2, Provider.Communication.Count);
			AssertEquals("Identifier should equal OC_Email", "b@b.com", Provider.Communication.First().Identifier);
			AssertEquals("Type should equal EM", "EM", Provider.Communication.First().Type);
		}

		public void TestIdentificationNumber()
		{
			AssertEquals("IdentificationNumber should equal EORI from bill notify party address.", "FR12345678900001", Provider.IdentificationNumber);

			var bill = Factory.New<TemporaryStorageBill>();

			var header = Factory.New<OrgHeader>();
			var address = Factory.New<OrgAddress>();
			address.OA_City = "Insomnia";
			address.OA_RN_NKCountryCode = "LS";
			address.OA_PostCode = "000000";
			address.OA_Address1 = "Kings Street";
			address.OA_Address2 = "No.001";
			address.OA_OH = header.PK;
			var cusCode = header.CustomsCodes.AddNew();
			cusCode.OK_CodeType = GlbCompany.CurrentCompany.Country.LocalBusinessRegNoCodeType;
			cusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.Country.Code;
			header.LocalBusinessRegNoObject.OK_CustomsRegNo = "1";
			header.OH_FullName = "Name";
			bill.ABL_OA_NotifyParty = address.PK;
			bill.ABL_NotifyPartyRegNoType = "4";

			var wrapper = NotifyPartyWrapper.New(bill);

			AssertEquals("IdentificationNumber should equal EORI from bill notify party.", "FR1", wrapper.IdentificationNumber);
		}

		public void TestName()
		{
			AssertEquals("Name should equal bill.Consignee?.Header?.OH_FullName", "Name", Provider.Name);
		}

		public void TestTypeOfPerson()
		{
			AssertEquals("TypeOfPerson should equal bill.ABL_NotifyPartyRegNoType", (byte)4, Provider.TypeOfPerson);
		}

		protected override NotifyPartyWrapper GetProvider()
		{
			var bill = Factory.New<TemporaryStorageBill>();

			var header = Factory.New<OrgHeader>();
			var address = Factory.New<OrgAddress>();
			address.OA_OH = header.PK;
			address.OA_City = "Insomnia";
			address.OA_RN_NKCountryCode = "LS";
			address.OA_PostCode = "000000";
			address.OA_Address1 = "Kings Street";
			address.OA_Address2 = "No.001";
			var contact = header.Contacts.AddNew();
			contact.OC_Email = "a@a.com";
			var cusCode = header.CustomsCodes.AddNew();
			cusCode.OK_CodeType = GlbCompany.CurrentCompany.Country.LocalBusinessRegNoCodeType;
			cusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.Country.Code;
			address.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			address.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			header.OH_FullName = "Name";
			bill.ABL_OA_NotifyParty = address.PK;
			bill.ABL_NotifyPartyRegNoType = "4";

			var contact2 = header.Contacts.AddNew();
			contact2.OC_ContactName = "Test Contact 2";
			contact2.OC_Email = "b@b.com";
			contact2.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;

			var contact3 = header.Contacts.AddNew();
			contact3.OC_ContactName = "Test Contact 3";
			contact3.OC_Email = "c@c.com";
			contact3.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;

			return NotifyPartyWrapper.New(bill);
		}
	}
}
