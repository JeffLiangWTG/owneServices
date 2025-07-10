using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaManifestMessageSendingObjectParent))]
	public sealed class AsycudaManifestMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSendingObjectsCollection()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var parent = new AsycudaManifestMessageSendingObjectParent(header);
			AssertEquals(1, parent.SendingObjectsCollection.Count);
		}

		public void TestMessageSendingObjectProperties()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var sendingObjectParent = new AsycudaManifestMessageSendingObjectParent(header);

			AssertContainsExactElementsInAnyOrder(
				new ZString[]
				{
					AsycudaManifestMessageSendingObject.Schema.MessageType,
					AsycudaManifestMessageSendingObject.Schema.MessageSubType,
					AsycudaManifestMessageSendingObject.Schema.MessageSubTypeDescription
				},
				sendingObjectParent.MessageSendingObjectProperties.Select(x => x.PropertyName));

			var messageType = sendingObjectParent.MessageSendingObjectProperties.FirstOrDefault(x => x.PropertyName == AsycudaManifestMessageSendingObject.Schema.MessageType);
			Assert(messageType.IsMandatory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			return new AsycudaManifestMessageSendingObjectParent(header);
		}
	}
}
