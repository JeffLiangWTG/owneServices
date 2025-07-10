using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaManifestQueryMessageSendingObjectParent))]
	public sealed class AsycudaManifestQueryMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSendingObjectsCollection()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var parent = new AsycudaManifestQueryMessageSendingObjectParent(header);
			AssertEquals(1, parent.SendingObjectsCollection.Count);
		}

		public void TestMessageSendingObjectProperties()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var sendingObjectParent = new AsycudaManifestQueryMessageSendingObjectParent(header);

			AssertContainsExactElementsInAnyOrder(
				new ZString[]
				{
					AsycudaManifestQueryMessageSendingObject.Schema.MessageType,
					AsycudaManifestQueryMessageSendingObject.Schema.MessageSubType,
					AsycudaManifestQueryMessageSendingObject.Schema.MessageSubTypeDescription,
					AsycudaManifestQueryMessageSendingObject.Schema.ManifestNumber,
					AsycudaManifestQueryMessageSendingObject.Schema.ParentDealNumber
				},
				sendingObjectParent.MessageSendingObjectProperties.Select(x => x.PropertyName));

			var messageType = sendingObjectParent.MessageSendingObjectProperties.FirstOrDefault(x => x.PropertyName == AsycudaManifestQueryMessageSendingObject.Schema.MessageType);
			Assert(messageType.IsMandatory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			return new AsycudaManifestQueryMessageSendingObjectParent(header);
		}
	}
}
