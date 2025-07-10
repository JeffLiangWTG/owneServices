using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaManifestMessageSendingObjectCollection))]
	public sealed class AsycudaManifestMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AsycudaManifestMessageSendingObjectCollection>
	{
		public void TestMessageSendingDefaultValues()
		{
			var sendingObject = (AsycudaManifestMessageSendingObject)GetNewElementToAddToTheCollection();
			Assert("By default sending is allowed", sendingObject.ShouldSend);
			AssertEquals("Message type should be MAN", "MAN", sendingObject.MessageType);
			AssertEquals("Message sub type should be 170", "170", sendingObject.MessageSubType);
		}

		protected override AsycudaManifestMessageSendingObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			return new AsycudaManifestMessageSendingObjectCollection(header);
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
