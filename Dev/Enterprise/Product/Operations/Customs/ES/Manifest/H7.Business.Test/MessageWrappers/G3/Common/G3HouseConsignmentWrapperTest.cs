using System;
using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.ES.Manifest.H7.Business.MessageWrappers.G3.Common;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	public class G3HouseConsignmentWrapperTest : DataProviderTestCase<G3HouseConsignmentWrapper>
	{
		public virtual void TestConstructor()
		{
			CombineAssertions(() =>
			{
#if NET
				AssertExceptionThrown("Throws Exception if bill is null", typeof(ArgumentNullException),
					"Value cannot be null. (Parameter 'bill')", () => new G3HouseConsignmentWrapper(null));
#else
				AssertExceptionThrown("Throws Exception if bill is null", typeof(ArgumentNullException),
					"Value cannot be null.\r\nParameter name: bill", () => new G3HouseConsignmentWrapper(null));
#endif
			});
		}

		public void TestPreviousDocument()
		{
			var previousDocument = wrapper.PreviousDocument;

			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Previous Document", previousDocument);
				AssertEquals("Expected filled Previous Document", 2, previousDocument.Count);
				AssertEquals("Expected filled Previous Document Name", "335", previousDocument.First().Name);
				AssertEquals("Expected filled Previous Document Number", "RN1-01", previousDocument.First().Number);
				AssertEquals("Expected filled Previous Document Goods Item Id", "RN2-01", previousDocument.First().GoodsItemId);

				AssertSame("Cached Previous Document", previousDocument, wrapper.PreviousDocument);
			});
		}

		public void TestTransportDocument()
		{
			var transportDocument = wrapper.TransportDocument;

			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Transport Document", transportDocument);
				AssertEquals("Expected filled Transport Document Name", "5025", transportDocument.Name);

				AssertSame("Cached Transport Document", transportDocument, wrapper.TransportDocument);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			bill = Factory.New<AsycudaBill>();
			var document = bill.PreviousDocuments.AddNew();
			document.CSI_ReferenceNumber = "RN1-01";
			document.CSI_ReferenceNumber2 = "RN2-01";
			bill.PreviousDocuments.AddNew();

			wrapper = CreateWrapper();
		}

		protected virtual G3HouseConsignmentWrapper CreateWrapper()
		{
			return new G3HouseConsignmentWrapper(bill);
		}

		protected override G3HouseConsignmentWrapper GetProvider()
		{
			return wrapper;
		}

		protected G3HouseConsignmentWrapper wrapper;
		protected AsycudaBill bill;
	}
}
