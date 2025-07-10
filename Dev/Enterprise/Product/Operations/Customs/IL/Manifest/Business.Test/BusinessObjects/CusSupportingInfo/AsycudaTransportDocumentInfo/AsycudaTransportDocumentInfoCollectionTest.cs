using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaTransportDocumentInfoCollection))]
	sealed class AsycudaTransportDocumentInfoCollectionTest : CusSupportingInfoCollectionTest<AsycudaTransportDocumentInfo>
	{
		protected override CusSupportingInfoCollection<AsycudaTransportDocumentInfo> GetCusSupportingInfoCollection()
		{
			var tempHeader = Factory.New<AsycudaManifestHeader>();
			var bill = tempHeader.Bills.AddNew();
			return new AsycudaTransportDocumentInfoCollection(bill);
		}

		public void TestDefaultValues()
		{
			var transportDocumentInfo = Collection.AddNew() as AsycudaTransportDocumentInfo;

			AssertNotNull("The created item should of type AsycudaTransportDocumentInfo", transportDocumentInfo);
			AssertEquals("The CSI_SubType of the created item must be TRA", "TRA", transportDocumentInfo.CSI_SubType);
		}

		public void TestEnsureTransportDocumentType()
		{
			var collection = (IAsycudaTransportDocumentInfoCollection)Collection;

			collection.EnsureTransportDocumentType("704", "1234");

			var item = Collection.Cast<AsycudaTransportDocumentInfo>().FirstOrDefault(s => s.CSI_Code == "704");

			AssertNotNull("The item with CSI_Code 704 should be created", item);
			AssertEquals("The item with CSI_Code 704 should have CSI_ReferenceNumber 1234", "1234", item.CSI_ReferenceNumber);

			collection.EnsureTransportDocumentType("704", ZString.Empty);

			item = Collection.Cast<AsycudaTransportDocumentInfo>().FirstOrDefault(s => s.CSI_Code == "704");
			AssertNull("The item with CSI_Code 704 should disappear", item);
		}
	}
}
