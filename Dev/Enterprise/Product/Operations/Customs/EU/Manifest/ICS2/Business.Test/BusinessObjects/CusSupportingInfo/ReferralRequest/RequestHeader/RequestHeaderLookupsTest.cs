using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class RequestHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRequestTypeList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var requestHeader = header.RequestHeaders.AddNew();
			var list = requestHeader.Lookups.RequestTypeList;

			AssertSame("Accessing the list twice should get the exact same object as the list is cached", list, requestHeader.Lookups.RequestTypeList);
			AssertEquals(true, list.ContainsCode("AMD"));
			AssertEquals(true, list.ContainsCode("RFI"));
			AssertEquals(true, list.ContainsCode("RFS"));
		}

		public void TestHouseBilList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			var requestHeader = header.RequestHeaders.AddNew();
			var list = requestHeader.Lookups.HouseBillList.Cast<AsycudaBill>();

			AssertEquals(2, list.Count());
			Assert(list.Contains(bill1));
			Assert(list.Contains(bill2));
		}

		public void TestTransportDocumentTypeList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var requestHeader = header.RequestHeaders.AddNew();
			var list = requestHeader.Lookups.TransportDocumentTypeList;

			AssertSame("Accessing the list twice should get the exact same object as the list is cached", list, requestHeader.Lookups.TransportDocumentTypeList);
			CombineAssertions(() =>
			{
				AssertEquals(15, list.Count);

				Assert(list.ContainsCode("C624"));
				AssertEquals("Form 302", list.GetDescriptionFromCode("C624"));

				Assert(list.ContainsCode("C625"));
				AssertEquals("Rhine Manifest", list.GetDescriptionFromCode("C625"));

				Assert(list.ContainsCode("C664"));
				AssertEquals("CN22 declaration according to Article 237 of the Regulation (ECC) No 2454/93", list.GetDescriptionFromCode("C664"));

				Assert(list.ContainsCode("C665"));
				AssertEquals("CN23 declaration according to Article 237 of the Regulation (ECC) No 2454/93", list.GetDescriptionFromCode("C665"));

				Assert(list.ContainsCode("N703"));
				AssertEquals("House waybill", list.GetDescriptionFromCode("N703"));

				Assert(list.ContainsCode("N704"));
				AssertEquals("Master bill of lading", list.GetDescriptionFromCode("N704"));

				Assert(list.ContainsCode("N705"));
				AssertEquals("Bill of lading", list.GetDescriptionFromCode("N705"));

				Assert(list.ContainsCode("N714"));
				AssertEquals("House bill of lading", list.GetDescriptionFromCode("N714"));

				Assert(list.ContainsCode("N720"));
				AssertEquals("Consignment note CIM", list.GetDescriptionFromCode("N720"));

				Assert(list.ContainsCode("N722"));
				AssertEquals("Road list - SMGS", list.GetDescriptionFromCode("N722"));

				Assert(list.ContainsCode("N730"));
				AssertEquals("Road consignment note", list.GetDescriptionFromCode("N730"));

				Assert(list.ContainsCode("N740"));
				AssertEquals("Air waybill", list.GetDescriptionFromCode("N740"));

				Assert(list.ContainsCode("N741"));
				AssertEquals("Master air waybill", list.GetDescriptionFromCode("N741"));

				Assert(list.ContainsCode("N750"));
				AssertEquals("Movement by post including parcel post", list.GetDescriptionFromCode("N750"));

				Assert(list.ContainsCode("N760"));
				AssertEquals("Multimodal / combined transport document", list.GetDescriptionFromCode("N760"));
			});
		}
	}
}
