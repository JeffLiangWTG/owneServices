using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	sealed class IM432DeclarationTypeProviderTest : DataProviderTestCase<IM432DeclarationTypeProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("Manifest header missing", () => new IM432DeclarationTypeProvider(null));
		}

		public void TestLRN()
		{
			AssertEquals("LRN", "TestLRN", Provider.LRN);
		}

		public void TestCustomsOffices02()
		{
			var customsOffices02 = Provider.CustomsOffices02;
			CombineAssertions("CustomsOffices02", () =>
			{
				Assert("CustomsOffices02 is CustomsOffices02Provider", customsOffices02 is CustomsOffices02Provider);
				AssertSame("Is cached", customsOffices02, Provider.CustomsOffices02);
			});
		}

		public void TestParties()
		{
			var parties = Provider.Parties;
			CombineAssertions("Parties", () =>
			{
				Assert("Parties is IM432PartiesTypeProvider", parties is IM432PartiesTypeProvider);
				AssertSame("Is cached", parties, Provider.Parties);
			});
		}

		protected override IM432DeclarationTypeProvider GetProvider()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			var representativeHeader = Factory.New<OrgHeader>();
			header.AMA_OA_Representative = representativeHeader.MainAddress.PK;

			var bill = header.Bills.AddNew();
			bill.LocalReferenceNumber = "TestLRN";

			return new IM432DeclarationTypeProvider(bill);
		}
	}
}
