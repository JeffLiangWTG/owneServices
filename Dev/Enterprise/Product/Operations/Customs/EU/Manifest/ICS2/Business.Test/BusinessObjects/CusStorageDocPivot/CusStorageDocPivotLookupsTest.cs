using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class CusStorageDocPivotLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAvailableEDocs()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var requestHeader = manifestHeader.RequestHeaders.AddNew();

			var path = "Enterprise.Customs.EU.Manifest.ICS2.Business.Test.Message.MessageProcessors.Outgoing.TestFiles";
			var reader = new Customs.Business.Testing.TestFileReader(typeof(CusStorageDocPivotLookupsTest));

			var eDoc1 = manifestHeader.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var eDoc2 = manifestHeader.DocManagerInfo.AddFileOrDocument(new byte[1], "InvoiceShipment.pdf", "CIV");
			var eDoc3 = manifestHeader.DocManagerInfo.AddFileOrDocument(reader.GetEmbeddedFileData(path, "TestImage.jpg"), "Invoice1.jpg", "CIV", overwriteExistingFileIfNotImageFile: true);
			var eDoc4 = manifestHeader.DocManagerInfo.AddFileOrDocument(reader.GetEmbeddedFileData(path, "TestImage.jpeg"), "Invoice2.jpeg", "CIV", overwriteExistingFileIfNotImageFile: true);
			var eDoc5 = manifestHeader.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice3.xml", "CIV");

			var pivot = manifestHeader.EDocPivotCollection.AddNew();
			var expectedKeys = new[] { eDoc1.UniqueKey, eDoc2.UniqueKey, eDoc3.UniqueKey, eDoc4.UniqueKey, eDoc5.UniqueKey };
			AssertContainsExactElementsInAnyOrder("Should load all documents.", expectedKeys, pivot.Lookups.AvailableEDocs.AvailableList.Select(c => c.UniqueKey));

			pivot = requestHeader.Attachments.AddNew();
			expectedKeys = new[] { eDoc1.UniqueKey, eDoc2.UniqueKey, eDoc3.UniqueKey, eDoc4.UniqueKey };
			AssertContainsExactElementsInAnyOrder("Should contain these documents which type is PDF, JPG or JPEG.", expectedKeys, pivot.Lookups.AvailableEDocs.AvailableList.Select(c => c.UniqueKey));
		}
	}
}
