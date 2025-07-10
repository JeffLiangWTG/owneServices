using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.Business.Testing
{
	[TestedType(typeof(PBNMessageSendingObjectCollection))]
	sealed class PBNMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PBNMessageSendingObjectCollection>
	{
		public void TestCreateNonPersistentBusinessObject()
		{
			_ = AssertExceptionThrown<InvalidOperationException>("AddNew", "PBNMessageSendingObjectCollection should not support adding.", () => Collection.AddNew());
		}

		public void TestAllowNew()
		{
			AssertEquals(expected: false, Collection.AllowNew);
		}

		protected override PBNMessageSendingObjectCollection GetCollectionToTest() => new(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new PBNMessageSendingObject(Factory.New<AsycudaManifestHeader>());
	}
}
