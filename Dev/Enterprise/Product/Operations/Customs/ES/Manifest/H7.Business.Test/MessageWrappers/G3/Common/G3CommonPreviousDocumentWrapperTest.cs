using System;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	public class G3CommonPreviousDocumentWrapperTest : DataProviderTestCase<G3CommonPreviousDocumentWrapper>
	{
		public void TestHouseConsignmentPreviousDocument()
		{
			CombineAssertions(() =>
			{
				var document = CreatePreviousDocument();
				var wrapper = new G3CommonPreviousDocumentWrapper(document);
				AssertEquals("Expected filled Name", "335", wrapper.Name);
				AssertEquals("Expected filled Number", "123", wrapper.Number);
				AssertEquals("Expected filled Goods Item Id", "234", wrapper.GoodsItemId);

				document = null;
#if NET
				AssertExceptionThrown("Throws Exception if document is null", typeof(ArgumentNullException),
					"Value cannot be null. (Parameter 'document')", () => new G3CommonPreviousDocumentWrapper(document));
#else
				AssertExceptionThrown("Throws Exception if document is null", typeof(ArgumentNullException),
					"Value cannot be null.\r\nParameter name: document", () => new G3CommonPreviousDocumentWrapper(document));
#endif
			});
		}

		public void TestMasterConsignmentPreviousDocument()
		{
			CombineAssertions(() =>
			{
				var header = Factory.New<AsycudaManifestHeader>();
				header.AMA_MasterInformation = "MIF";
				header.EntryLineNumber = "12345";

				var wrapper = new G3CommonPreviousDocumentWrapper(header, true);
				AssertEquals("Expected filled Name", "337", wrapper.Name);
				AssertEquals("Expected filled Number", "MIF", wrapper.Number);
				AssertEquals("Expected filled Entry Line Number", "12345", wrapper.GoodsItemId);

				header = null;
#if NET
				AssertExceptionThrown("Throws Exception if header is null", typeof(ArgumentNullException),
					"Value cannot be null. (Parameter 'header')", () => new G3CommonPreviousDocumentWrapper(header, true));
#else
				AssertExceptionThrown("Throws Exception if header is null", typeof(ArgumentNullException),
					"Value cannot be null.\r\nParameter name: header", () => new G3CommonPreviousDocumentWrapper(header, true));
#endif
			});
		}

		public void TestRevokeMasterConsigmentPreviousDocument()
		{
			CombineAssertions(() =>
			{
				var header = Factory.New<AsycudaManifestHeader>();
				header.AMA_MasterInformation = "MIF";
				header.G3MRNToRevoke = "123";
				header.EntryLineNumber = "12345";

				var wrapper = new G3CommonPreviousDocumentWrapper(header, false);
				AssertEquals("Expected filled Name", "MRN", wrapper.Name);
				AssertEquals("Expected filled Number", "123", wrapper.Number);
				AssertEquals("Expected empty Entry Line Number", string.Empty, wrapper.GoodsItemId);
			});
		}

		public void TestGoodsItemId()
		{
			var document = CreatePreviousDocument();
			var wrapper = new G3CommonPreviousDocumentWrapper(document);
			AssertEquals("Expected filled GoodsItemId", "234", wrapper.GoodsItemId);
		}

		protected override G3CommonPreviousDocumentWrapper GetProvider()
		{
			var document = CreatePreviousDocument();
			return new G3CommonPreviousDocumentWrapper(document);
		}

		PreviousDocument CreatePreviousDocument()
		{
			var bill = Factory.New<AsycudaBill>();
			var document = bill.PreviousDocuments.AddNew();
			document.CSI_ReferenceNumber = "123";
			document.CSI_ReferenceNumber2 = "234";
			return document;
		}
	}
}
