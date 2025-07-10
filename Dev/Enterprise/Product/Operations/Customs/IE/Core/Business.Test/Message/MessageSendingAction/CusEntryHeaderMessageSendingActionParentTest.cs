using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestsSubclassesOf(typeof(CusEntryHeaderMessageSendingActionParent<>))]
	public abstract class CusEntryHeaderMessageSendingActionParentTest<TSendingParent, TSendingAction> : NonPersistentBusinessObjectTestCase
		where TSendingParent : CusEntryHeaderMessageSendingActionParent<TSendingAction>
		where TSendingAction : CusEntryHeaderMessageSendingAction
	{
		protected abstract Type ExpectedSendingObjectCollectionType { get; }

		public void TestSendingObjectCollection()
		{
			var testSendingParent = (TSendingParent)GetNewBusinessObject();
			AssertType("Should defined correct Type ofr SendingObjectCollection.", ExpectedSendingObjectCollectionType, testSendingParent.SendingObjectsCollection);
		}
	}
}
