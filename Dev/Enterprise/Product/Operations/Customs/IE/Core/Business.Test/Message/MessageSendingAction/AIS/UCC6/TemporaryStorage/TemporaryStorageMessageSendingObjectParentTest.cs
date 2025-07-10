using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageMessageSendingObjectParent))]
	class TemporaryStorageMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			return new TemporaryStorageMessageSendingObjectParent(header);
		}

		public void TestSendingObjectsCollection()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var parent = new TemporaryStorageMessageSendingObjectParent(header);
			AssertEquals(1, parent.SendingObjectsCollection.Count);
		}

		public void TestMessageSendingObjectProperties()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var sendingObjectParent = new TemporaryStorageMessageSendingObjectParent(header);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(new ZString[]
				{
					TemporaryStorageMessageSendingObject.Schema.MessageType,
					TemporaryStorageMessageSendingObject.Schema.DeclarationType,
					TemporaryStorageMessageSendingObject.Schema.EntryStatus,
					TemporaryStorageMessageSendingObject.Schema.MessageStatus,
					TemporaryStorageMessageSendingObject.Schema.AlternativeDateOfAcceptance,
					TemporaryStorageMessageSendingObject.Schema.CustomsReference,
					TemporaryStorageMessageSendingObject.Schema.CustomsJustification,
				}, sendingObjectParent.MessageSendingObjectProperties.Select(x => x.PropertyName));

				var messageType = sendingObjectParent.MessageSendingObjectProperties.FirstOrDefault(x => x.PropertyName == TemporaryStorageMessageSendingObject.Schema.MessageType);
				Assert("Message type should be mandatory", messageType.IsMandatory);

				var declarationType = sendingObjectParent.MessageSendingObjectProperties.FirstOrDefault(x => x.PropertyName == TemporaryStorageMessageSendingObject.Schema.DeclarationType);
				Assert("Declaration type should not be mandatory", !declarationType.IsMandatory);

				var entryStatus = sendingObjectParent.MessageSendingObjectProperties.FirstOrDefault(x => x.PropertyName == TemporaryStorageMessageSendingObject.Schema.EntryStatus);
				Assert("Entry status should not be mandatory", !entryStatus.IsMandatory);

				var messageStatus = sendingObjectParent.MessageSendingObjectProperties.FirstOrDefault(x => x.PropertyName == TemporaryStorageMessageSendingObject.Schema.MessageStatus);
				Assert("Message status should not be mandatory", !messageStatus.IsMandatory);
			});
		}
	}
}
