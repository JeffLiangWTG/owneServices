using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageMessageSendingObjectParent<TemporaryStorageMessageSendingObject, TemporaryStorageHeader>))]
	public class TemporaryStorageMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			return new TemporaryStorageMessageSendingObjectParent<TemporaryStorageMessageSendingObject, TemporaryStorageHeader>(header);
		}

		public void TestSendingObjectsCollection()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var parent = new TemporaryStorageMessageSendingObjectParent<TemporaryStorageMessageSendingObject, TemporaryStorageHeader>(header);
			AssertEquals(1, parent.SendingObjectsCollection.Count);
		}

		public void TestMessageSendingObjectProperties()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var sendingObjectParent = new TemporaryStorageMessageSendingObjectParent<TemporaryStorageMessageSendingObject, TemporaryStorageHeader>(header);
			AssertContainsExactElementsInAnyOrder(new ZString[] { TemporaryStorageMessageSendingObject.Schema.VOCReason, TemporaryStorageMessageSendingObject.Schema.MessageType, TemporaryStorageMessageSendingObject.Schema.Date }, sendingObjectParent.MessageSendingObjectProperties.Select(x => x.PropertyName));

			var messageType = sendingObjectParent.MessageSendingObjectProperties.FirstOrDefault(x => x.PropertyName == TemporaryStorageMessageSendingObject.Schema.MessageType);
			Assert(messageType.IsMandatory);

			var date = sendingObjectParent.MessageSendingObjectProperties.FirstOrDefault(x => x.PropertyName == TemporaryStorageMessageSendingObject.Schema.Date);
			Assert(!date.IsMandatory);

			var vOCReason = sendingObjectParent.MessageSendingObjectProperties.FirstOrDefault(x => x.PropertyName == TemporaryStorageMessageSendingObject.Schema.VOCReason);
			Assert(!vOCReason.IsMandatory);
		}

		public void TestGetAdditionalWarnings()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.IsENSReuse = true;
			var sendingObjectParent = new TemporaryStorageMessageSendingObjectParent<TemporaryStorageMessageSendingObject, TemporaryStorageHeader>(header);
			AssertContains("When ENS Re-use = True, only minimal data will be sent to customs including MRN and Bill numbers. Bill details and related items will not be sent.", sendingObjectParent.AdditionalWarnings);

			header.IsENSReuse = false;
			sendingObjectParent = new TemporaryStorageMessageSendingObjectParent<TemporaryStorageMessageSendingObject, TemporaryStorageHeader>(header);
			AssertNotContains("When ENS Re-use = True, only minimal data will be sent to customs including MRN and Bill numbers. Bill details and related items will not be sent.", sendingObjectParent.AdditionalWarnings);
		}
	}
}
