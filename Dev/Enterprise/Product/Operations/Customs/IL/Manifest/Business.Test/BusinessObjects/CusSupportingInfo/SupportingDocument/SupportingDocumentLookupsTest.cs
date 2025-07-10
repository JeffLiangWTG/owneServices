using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.IL.Manifest.Business
{
	sealed class SupportingDocumentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEDocListAndCollection_ManifestWithConsol()
		{
			var factory = Factory;
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var consol = Factory.New<ForwardingConsol>();
			header.SetParent(consol);
			var supportingDocument = header.Bills.AddNew().SupportingDocuments.AddNew();

			var eDoc1 = header.DocManagerInfo.AddFileOrDocument(new byte[1], "Test1.pdf", "PDF");
			var eDoc2 = consol.DocManagerInfo.AddFileOrDocument(new byte[1], "Test2.txt", "TXT");

			var eDocList = supportingDocument.Lookups.EDocList;
			AssertEquals(1, supportingDocument.Lookups.EDocList.Count);
			AssertEquals(eDoc2.UniqueKey, ((AvailableEDocList)eDocList)[0].PK);
		}

		public void TestEDocListAndCollection_ManifestWithoutConsol()
		{
			var factory = Factory;
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var supportingDocument = header.Bills.AddNew().SupportingDocuments.AddNew();

			var eDoc1 = header.DocManagerInfo.AddFileOrDocument(new byte[1], "Test1.pdf", "PDF");
			var eDoc2 = header.DocManagerInfo.AddFileOrDocument(new byte[1], "Test2.txt", "TXT");

			var eDocList = supportingDocument.Lookups.EDocList;
			AssertEquals(2, supportingDocument.Lookups.EDocList.Count);
			AssertEquals(eDoc1.UniqueKey, ((AvailableEDocList)eDocList)[0].PK);
			AssertEquals(eDoc2.UniqueKey, ((AvailableEDocList)eDocList)[1].PK);
		}
	}
}
