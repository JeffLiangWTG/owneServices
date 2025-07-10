using System.Collections.Generic;
using System.Linq.Expressions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class Q04HeaderProviderTest : ICS2BaseMessageProviderTest<Q04HeaderProvider>
	{
		public void TestAddressedMemberStateCountry()
		{
			manifestHeader.AddressedMemberState = "DE";
			AssertEquals("AddressedMemberStateCountry", "DE", Provider.AddressedMemberStateCountry);
		}

		public void TestRepresentativeIdentificationNumber_Null()
		{
			AssertEquals("RepresentativeIdentificationNumber", string.Empty, Provider.RepresentativeIdentificationNumber);
		}

		public void TestRepresentativeIdentificationNumber()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456", Core.Constants.CountryCodes.France);
			var addressWithEORI = orgHeader.Addresses.AddNew();
			addressWithEORI.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			manifestHeader.AMA_OA_ShippingAgent = addressWithEORI.PK;
			AssertEquals("Representative", "FR123456", Provider.RepresentativeIdentificationNumber);
		}

		public void TestTransportDocument_Null()
		{
			AssertNull(Provider.TransportDocument);
		}

		public void TestTransportDocument_EmptyDocumentNumber()
		{
			manifestHeader.AMA_MasterBill = null;
			AssertNull(Provider.TransportDocument);
		}

		public void TestTransportDocument()
		{
			manifestHeader.AMA_MasterBill = "bill123";
			manifestHeader.MasterBill.TransportDocumentType = "N722";
			AssertEquals("TransportDocument DocumentNumber", "bill123", Provider.TransportDocument.Identifier);
			AssertEquals("TransportDocument Type", "N722", Provider.TransportDocument.Type);
		}

		public void TestDeclarantIdentificationNumber()
		{
			AssertEquals("Declarant IdentificationNumber", "DE654321", Provider.DeclarantIdentificationNumber);
		}

		public void TestCustomsOfficeOfFirstEntry_Null()
		{
			AssertEquals("CustomsOfficeOfFirstEntry", string.Empty, Provider.CustomsOfficeOfFirstEntry);
		}

		public void TestCustomsOfficeOfFirstEntry()
		{
			manifestHeader.AddressedMemberState = ZString.Empty;
			manifestHeader.AMA_CustomsOffice = "Office123";
			AssertEquals("CustomsOfficeOfFirstEntry", "Office123", Provider.CustomsOfficeOfFirstEntry);
		}

		public void TestCustomsOfficeOfFirstEntry_NotPopulatedIfAddressedMemberStateIsSet()
		{
			manifestHeader.AddressedMemberState = "DE";
			manifestHeader.AMA_CustomsOffice = "Office123";
			AssertEquals("CustomsOfficeOfFirstEntry should not be populated when AddressedMemberState is set", string.Empty, Provider.CustomsOfficeOfFirstEntry);
			AssertEquals("AddressedMemberState is set", "DE", Provider.AddressedMemberStateCountry);
		}

		protected override IEnumerable<Expression<System.Func<Q04HeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.TransportDocument;
		}
	}
}
