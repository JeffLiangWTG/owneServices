using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestsSubclassesOf(typeof(BEJobDeclarationMessageSendingObjectParent<,>))]
public abstract class BEJobDeclarationMessageSendingObjectParentTest<TSendingParent, TSendingObjectCollection, TSendingAction> : NonPersistentBusinessObjectTestCase
	where TSendingParent : BEJobDeclarationMessageSendingObjectParent<TSendingObjectCollection, TSendingAction>
	where TSendingObjectCollection : BEJobDeclarationMessageSendingObjectCollection<TSendingAction>
	where TSendingAction : BEJobDeclarationMessageSendingObject
{
	protected abstract Type ExpectedSendingObjectCollectionType { get; }

	public void TestSendingObjectCollection()
	{
		Declaration.CustomsEntryHeaders.AddNew();
		Declaration.CustomsEntryHeaders.AddNew();

		var parent = (TSendingParent)GetNewBusinessObject();
		var testSendingParent = (TSendingParent)GetNewBusinessObject();

		CombineAssertions(() =>
		{
			AssertType("Should defined correct Type for SendingObjectCollection.", ExpectedSendingObjectCollectionType, testSendingParent.SendingObjectsCollection);
			AssertEquals("Populated", 2, parent.SendingObjectsCollection.Count);

			parent.SendingObjectsCollection[0].ShouldSend = true;
			parent.SendingObjectsCollection[1].ShouldSend = true;
			AssertEquals("No row errors", false, parent.HasRowErrors);
		});
	}

	protected override BusinessObject GetNewBusinessObject() => (TSendingParent)Activator.CreateInstance(typeof(TSendingParent), Declaration, Declaration.ActiveEntryHeaders);

	JobDeclaration Declaration => fJobDeclaration ?? (fJobDeclaration = Factory.New<JobDeclaration>());
	JobDeclaration fJobDeclaration;
}
