using System;
using System.Collections.Generic;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class IM432GoodsShipmentProviderTest : DataProviderTestCase<IM432GoodsShipmentProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("Manifest header missing", () => new IM432GoodsShipmentProvider(null));
		}

		public void TestPreviousDocuments()
		{
			var previousDocuments = Provider.PreviousDocuments;
			CombineAssertions("Previous documents", () =>
			{
				AssertEquals("Should have 2 previous documents", 2, previousDocuments.Count);
				Assert("Should be IReadOnlyCollection<DocumentProvider>", previousDocuments is IReadOnlyCollection<DocumentProvider>);
			});
		}

		public void TestConsignment()
		{
			var consignment = Provider.Consignment;
			CombineAssertions("Consignment", () =>
			{
				Assert("Should be MConsignment02Provider", consignment is MConsignment02Provider);
				AssertSame("Is cached", consignment, Provider.Consignment);
			});
		}

		protected override IM432GoodsShipmentProvider GetProvider()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			var bill = header.Bills.AddNew();
			bill.PreviousDocuments.AddNew();
			bill.PreviousDocuments.AddNew();

			return new IM432GoodsShipmentProvider(bill);
		}
	}
}
