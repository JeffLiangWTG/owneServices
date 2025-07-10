
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IL.Business.CodeDescriptionPairLists;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaManifestQueryMessageSendingObjectCollection))]
	public sealed class AsycudaManifestQueryMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AsycudaManifestQueryMessageSendingObjectCollection>
	{
		public void TestMessageSendingDefaultValues()
		{
			var sendingObject = (AsycudaManifestQueryMessageSendingObject)GetNewElementToAddToTheCollection();
			Assert("By default sending is allowed", sendingObject.ShouldSend);
			AssertEquals("Message type should be MAN", "MAN", sendingObject.MessageType);
			AssertEquals("Message sub type should be 820", "820", sendingObject.MessageSubType);
			AssertEquals("ManifestNumber should return the value of AMA_ManifestNumber", "123", sendingObject.ManifestNumber);
			AssertEquals("ParentDealNumber should return the value of the first bill's ParentDealNumber", "456", sendingObject.ParentDealNumber);
		}

		protected override AsycudaManifestQueryMessageSendingObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestNumber = "123";
			var asycudaBill = header.Bills.AddNew();
			var asycudaTransportDocumentInfo = asycudaBill.TransportDocuments.AddNew();
			asycudaTransportDocumentInfo.CSI_Code = TransportDocsTypeList.Codes.IL2;
			asycudaTransportDocumentInfo.CSI_ReferenceNumber = "456";

			return new AsycudaManifestQueryMessageSendingObjectCollection(header);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var collection = GetCollectionToTest();
			return collection.AddNew();
		}

		public void TestAllowNew()
		{
			var collection = GetCollectionToTest();
			Assert(!collection.AllowNew);
		}
	}
}
