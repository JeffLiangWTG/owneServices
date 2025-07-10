using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(ExportEntryMessageSendingActionCollection))]
	class ExportEntryMessageSendingActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ExportEntryMessageSendingActionCollection>
	{
		public void TestGetSendingAction()
		{
			Declaration.CustomsEntryHeaders.AddNew();
			var coll = GetCollectionToTest();
			coll.PopulateElements();
			AssertEquals(typeof(ExportEntryMessageSendingAction), coll[0].GetType());
		}
		protected override Type GetExpectedCollectionType()
		{
			return typeof(ExportEntryMessageSendingActionCollection);
		}

		protected override ExportEntryMessageSendingActionCollection GetCollectionToTest()
		{
			return new ExportEntryMessageSendingActionCollection(Declaration);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var entry = Declaration.CustomsEntryHeaders.AddNew();
			return new ExportEntryMessageSendingAction(entry);
		}
		JobDeclaration Declaration => fJobDeclaration ?? (fJobDeclaration = Factory.New<JobDeclaration>());
		JobDeclaration fJobDeclaration;
	}
}
