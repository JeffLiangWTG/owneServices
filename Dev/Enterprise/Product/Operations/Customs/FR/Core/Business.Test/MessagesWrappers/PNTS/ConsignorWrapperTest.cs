using System.Linq;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	public class ConsignorWrapperTest : Customs.Business.Testing.DataProviderTestCase<ConsignorWrapper>
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
			AssertEquals("Identifier should equal OC_Email", "a@a.com", Provider.Communication.First().Identifier);
			AssertEquals("Type should equal EM", "EM", Provider.Communication.First().Type);
		}

		public void TestIdentificationNumber()
		{
			AssertEquals("IdentificationNumber should equal EORI from bill consignor address.", "FR12345678900001", Provider.IdentificationNumber);

			var tempStorageHeader = Factory.New<TemporaryStorageHeader>();
			var bill = tempStorageHeader.Bills.AddNew();

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
			bill.ABL_OA_Shipper = address.PK;
			bill.ABL_ShipperRegNoType = "3";

			var wrapper = ConsignorWrapper.New(bill);

			AssertEquals("IdentificationNumber should equal EORI from  bill consignor.", "FR1", wrapper.IdentificationNumber);
		}

		public void TestName()
		{
			AssertEquals("Name should equal bill.Consignee?.Header?.OH_FullName", "Name", Provider.Name);
		}

		public void TestTypeOfPerson()
		{
			AssertEquals("TypeOfPerson should equal bill.ABL_ShipperRegNoType", (byte)3, Provider.TypeOfPerson);
		}

		protected override ConsignorWrapper GetProvider()
		{
			var tempStorageHeader = Factory.New<TemporaryStorageHeader>();
			var bill = tempStorageHeader.Bills.AddNew();

			var header = Factory.New<OrgHeader>();
			var address = Factory.New<OrgAddress>();
			address.OA_OH = header.PK;
			address.OA_City = "Insomnia";
			address.OA_RN_NKCountryCode = "LS";
			address.OA_PostCode = "000000";
			address.OA_Address1 = "Kings Street";
			address.OA_Address2 = "No.001";
			address.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			address.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			var contact = header.Contacts.AddNew();
			contact.OC_Email = "a@a.com";
			contact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
			var contact2 = header.Contacts.AddNew();
			contact2.OC_Email = "b@b.com";
			var contact3 = header.Contacts.AddNew();
			contact3.OC_Email = "c@c.com";
			contact3.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
			var cusCode = header.CustomsCodes.AddNew();
			cusCode.OK_CodeType = GlbCompany.CurrentCompany.Country.LocalBusinessRegNoCodeType;
			cusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.Country.Code;
			header.LocalBusinessRegNoObject.OK_CustomsRegNo = "1";
			header.OH_FullName = "Name";
			bill.ABL_OA_Shipper = address.PK;
			bill.ABL_ShipperRegNoType = "3";

			return ConsignorWrapper.New(bill);
		}
	}
}
