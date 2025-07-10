using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.Testing;

[TestedType(typeof(G5TemporaryStorageMessageSendingObjectParent))]
public sealed class G5TemporaryStorageMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
{
	public void TestMessageSendingObjectProperties()
	{
		var sendingObjectParent = new G5TemporaryStorageMessageSendingObjectParent(sendingObject);
		var sendingObjectPropertyNames = sendingObjectParent.MessageSendingObjectProperties.Select(x => x.PropertyName).ToArray();
		AssertArrayEqualsByElements("MessageSendingObjectProperties", new ZString[] { "LRN", "EntryStatus", "MRN", "MessageSubType", "MessageStatus", "MessageType" }, sendingObjectPropertyNames);
	}

	public void TestSendingObjectsCollection()
	{
		var sendingObjectParent = new G5TemporaryStorageMessageSendingObjectParent(sendingObject);
		var sendingObjectsCollection = sendingObjectParent.SendingObjectsCollection;
		AssertType<EU.Business.CusTempStorage.TemporaryStorageMessageSendingObjectCollection<G5TemporaryStorageMessageSendingObject, TemporaryStorageHeader>>("SendingObjectsCollection Type", sendingObjectsCollection);
		AssertEquals("SendingObjectsCollection Count", 1, sendingObjectsCollection.Count);
		AssertType<G5TemporaryStorageMessageSendingObject>("SendingObject Type", sendingObjectsCollection[0]);
	}

	protected override BusinessObject GetNewBusinessObject() => new G5TemporaryStorageMessageSendingObjectParent(sendingObject);

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<TemporaryStorageHeader>();
		sendingObject = new G5MessageSendingObject(header, GlbStaff.CurrentUser);
	}

	G5MessageSendingObject sendingObject;
}
