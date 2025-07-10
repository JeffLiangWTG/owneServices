using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage.Testing
{
	sealed class TS313And315MessageProviderTest : DataProviderTestCase<TS313And315MessageProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<System.ArgumentException>("Should throw ArgumentException with null header.", () => new TS313And315HeaderProviderTestForTest(null));
		}

		public void TestITS313And315Header()
		{
			Assert("Should implement ITS313And315Header", Provider is ITS313And315Header);
		}

		public void TestConsignment()
		{
			Assert("Should be ConsignmentProvider", Provider.Consignment is ConsignmentProvider);
		}

		public void TestSupervisingCustomsOffice()
		{
			AssertEquals("SupervisingCustomsOffice.ReferenceNumber", "AAA", Provider.SupervisingCustomsOffice);
		}

		public void TestCustomsOfficeLodgement()
		{
			AssertEquals("CustomsOfficeLodgement.ReferenceNumber", "BBB", Provider.CustomsOfficeLodgement);
		}

		public void TestRepresentative()
		{
			AssertNull("No representative added to header", Provider.Representative);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "E007");
			var customsContact = orgHeader.Contacts.AddNew();
			customsContact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
			customsContact.OC_Phone = "+35312345678";
			var orgAddress = orgHeader.MainAddress;
			orgAddress.CompanyName = "Rep1";
			orgAddress.OA_Address1 = "No1";
			orgAddress.OA_Address2 = "Sandy Cove";
			orgAddress.OA_City = "Dublin";
			orgAddress.OA_PostCode = "D12654";
			orgAddress.OA_RN_NKCountryCode = "IE";
			header.AMA_OA_Representative = orgAddress.PK;

			var representative = GetProvider().Representative;
			CombineAssertions(() =>
			{
				AssertType<RepresentativeProvider>("Representative added to header", representative);
				Assert("Should be IRepresentative", GetProvider().Representative is IRepresentative);
				AssertEquals("Name", "Rep1", representative.Name);
				AssertEquals("Status", "2", representative.Status);
				AssertEquals("Id", "IEE007", representative.Id);
				AssertEquals("StreetAndNumber", "No1", representative.Address.StreetAndNumber);
				AssertEquals("StreetAdditionalLine", "Sandy Cove", representative.Address.StreetAdditionalLine);
				AssertEquals("City", "Dublin", representative.Address.City);
				AssertEquals("Country", "IE", representative.Address.Country);
				AssertEquals("Postcode", "D12654", representative.Address.Postcode);
				AssertEquals("Communication Id", "+35312345678", representative.Communication.First().Id);
				AssertEquals("Communication Type", "TE", representative.Communication.First().Type);
			});
		}

		public void TestDeclarant()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "E022");
			var customsContact = orgHeader.Contacts.AddNew();
			customsContact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
			customsContact.OC_Email = "perry@test.com";
			var orgAddress = orgHeader.MainAddress;
			orgAddress.CompanyName = "Dec2";
			orgAddress.OA_Address1 = "Unit 12";
			orgAddress.OA_Address2 = "Beach Street";
			orgAddress.OA_City = "Dublin";
			orgAddress.OA_PostCode = "D12655";
			orgAddress.OA_RN_NKCountryCode = "IE";
			header.AMA_OA_Declarant = orgAddress.PK;

			var declarant = Provider.Declarant;
			AssertType<DeclarantProvider>(declarant);
			CombineAssertions(() =>
			{
				AssertEquals("Name", "Dec2", declarant.Name);
				AssertEquals("Id", "IEE022", declarant.Id);
				AssertEquals("StreetAndNumber", "Unit 12", declarant.Address.StreetAndNumber);
				AssertEquals("StreetAdditionalLine", "Beach Street", declarant.Address.StreetAdditionalLine);
				AssertEquals("City", "Dublin", declarant.Address.City);
				AssertEquals("Country", "IE", declarant.Address.Country);
				AssertEquals("Postcode", "D12655", declarant.Address.Postcode);
				AssertEquals("Communication Id", "perry@test.com", declarant.Communication.First().Id);
				AssertEquals("Communication Type", "EM", declarant.Communication.First().Type);
				AssertNull("Contact Details", declarant.ContactDetails);
			});
		}

		public void TestPresentationOffice()
		{
			AssertEquals("PresentationOffice.ReferenceNumber", "CCC", Provider.PresentationOffice);
		}

		public void TestFallbackProcedure()
		{
			Assert("Will be implemented in following work items", true);
		}

		protected override TS313And315MessageProvider GetProvider() => new TS313And315HeaderProviderTestForTest(header);

		TemporaryStorageHeader header;
		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			header.AMA_CustomsOffice = "AAA";
			header.CustomsOfficeOfLodgement = "BBB";
			header.PresentationCustomsOffice = "CCC";
		}
	}

	sealed class TS313And315HeaderProviderTestForTest : TS313And315MessageProvider
	{
		internal TS313And315HeaderProviderTestForTest(TemporaryStorageHeader header) : base(new TemporaryStorageMessageSendingObject(header))
		{
		}
	}
}
