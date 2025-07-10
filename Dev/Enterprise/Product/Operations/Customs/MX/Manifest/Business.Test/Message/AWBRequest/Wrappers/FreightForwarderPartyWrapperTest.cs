using System.Linq;
using CargoWise.Customs.MX.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	public class FreightForwarderPartyWrapperTest : TestCaseWithFactory
	{
		public void TestFreightForwarderPartyWrapper()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			PopulateCurrentCompany();

			IHouseWaybill wrapper = new HouseWaybillWrapper(header.Bills.AddNew(), MessageSubTypeCodes.Codes.Original);
			IParty freightForwarder = wrapper.MasterConsignment.IncludedHouseConsignment.FreightForwarderParty;
			IPostalStructuredAddress postalStructuredAddress = freightForwarder.PostalStructuredAddress;
			ITradeContact tradeContact = freightForwarder.DefinedTradeContact.ElementAt(0);

			CombineAssertions(() =>
			{
				AssertEquals("123456", freightForwarder.PrimaryID);
				AssertEquals("", freightForwarder.AdditionalID);
				AssertEquals("CompanyName", freightForwarder.Name);
				AssertEquals("", freightForwarder.AccountID);

				AssertEquals("H3A 2R4", postalStructuredAddress.PostcodeCode);
				AssertEquals("Address1 Address2", postalStructuredAddress.StreetName);
				AssertEquals("City", postalStructuredAddress.CityName);
				AssertEquals("MX", postalStructuredAddress.CountryID);
				AssertEquals("Mexico", postalStructuredAddress.CountryName);
				AssertEquals("", postalStructuredAddress.CityID);
				AssertEquals("", postalStructuredAddress.PostOfficeBox);

				AssertEquals("", tradeContact.PersonName);
				AssertEquals("Phone", tradeContact.DirectTelephoneCommunicationCompleteNumber);
				AssertEquals("Fax", tradeContact.FaxCommunicationCompleteNumber);
				AssertEquals("Email", tradeContact.EmailCommunicationID);
			});
		}

		void PopulateCurrentCompany()
		{
			GlbCompany.CurrentCompany.Address1 = "Address1";
			GlbCompany.CurrentCompany.Address2 = "Address2";
			GlbCompany.CurrentCompany.GC_City = "City";
			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "123456";
			GlbCompany.CurrentCompany.GC_Email = "Email";
			GlbCompany.CurrentCompany.GC_Fax = "Fax";
			GlbCompany.CurrentCompany.GC_Name = "CompanyName";
			GlbCompany.CurrentCompany.GC_Phone = "Phone";
			GlbCompany.CurrentCompany.GC_PostCode = "H3A 2R4";
		}
	}
}
