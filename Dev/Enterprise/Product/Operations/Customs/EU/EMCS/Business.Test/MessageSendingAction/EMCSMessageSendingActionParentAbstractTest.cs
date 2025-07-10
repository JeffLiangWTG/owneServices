using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestsSubclassesOf(typeof(EMCSMessageSendingActionParent<>))]
	abstract class EMCSMessageSendingActionParentAbstractTest<TSendingParent, TSendingAction> : NonPersistentBusinessObjectTestCase
		where TSendingParent : EMCSMessageSendingActionParent<TSendingAction>
		where TSendingAction : EMCSMessageSendingAction
	{
		public void TestSendingObjectCollection()
		{
			var testSendingParent = (TSendingParent)GetNewBusinessObject();
			AssertType("Should define correct Type of SendingObjectCollection.", ExpectedSendingObjectCollectionType, testSendingParent.SendingObjectsCollection);
		}

		protected abstract Type ExpectedSendingObjectCollectionType { get; }
	}
}
