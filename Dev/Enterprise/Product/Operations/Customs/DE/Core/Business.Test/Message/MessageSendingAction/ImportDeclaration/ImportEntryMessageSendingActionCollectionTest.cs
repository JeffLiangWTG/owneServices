using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(ImportEntryMessageSendingActionCollection))]
	sealed class ImportEntryMessageSendingActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ImportEntryMessageSendingActionCollection>
	{
		public void TestGetSendingAction()
		{
			Declaration.CustomsEntryHeaders.AddNew();
			var coll = GetCollectionToTest();
			coll.PopulateElements();
			AssertEquals(typeof(ImportEntryMessageSendingAction), coll[0].GetType());
		}

		protected override Type GetExpectedCollectionType() => typeof(ImportEntryMessageSendingActionCollection);

		protected override ImportEntryMessageSendingActionCollection GetCollectionToTest() => new ImportEntryMessageSendingActionCollection(Declaration, null);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new ImportEntryMessageSendingAction(Declaration.CustomsEntryHeaders.AddNew(), null);

		JobDeclaration jobDeclaration;
		JobDeclaration Declaration => jobDeclaration ?? (jobDeclaration = Factory.New<JobDeclaration>());
	}
}
