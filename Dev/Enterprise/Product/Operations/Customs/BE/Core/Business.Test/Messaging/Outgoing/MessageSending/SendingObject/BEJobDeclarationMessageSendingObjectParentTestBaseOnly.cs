using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class BEJobDeclarationMessageSendingObjectParentTestBaseOnly : TestCaseWithFactory
{
	public void TestMessageSendingObjectProperties()
	{
		var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
		var testWrapper2 = new BEJobDeclarationMessageSendingObjectParentForTesting(declaration);
		var testItem = testWrapper2.MessageSendingObjectProperties.ToList();
		CombineAssertions(() =>
		{
			AssertEquals(5, testItem.Count);
			AssertEquals("Sub Style caption", "Sub Style", testItem.ElementAt(0).ResourceString.Caption);
			AssertEquals("Declaration Type caption", "Declaration Type", testItem.ElementAt(1).ResourceString.Caption);
			AssertEquals("Description caption", "Description", testItem.ElementAt(2).ResourceString.Caption);
			AssertEquals("Entry Status caption", "Entry Status", testItem.ElementAt(3).ResourceString.Caption);
			AssertEquals("Entry Type caption", "Entry Type", testItem.ElementAt(4).ResourceString.Caption);

			AssertEquals("Type (Time) width", 80, testItem.ElementAt(0).ColumnWidth);
			AssertEquals("Type (Procedure) width", 110, testItem.ElementAt(1).ColumnWidth);
			AssertEquals("Description width", 200, testItem.ElementAt(2).ColumnWidth);
			AssertEquals("Entry Status width", 80, testItem.ElementAt(3).ColumnWidth);
			AssertEquals("Entry Type width", 80, testItem.ElementAt(4).ColumnWidth);

			AssertEquals("All properties mandatory", true, testItem.All(x => x.IsMandatory));
		});
	}

	public void TestGetSendingObjectsCollectionCore()
	{
		var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
		var testWrapper1 = new BEJobDeclarationMessageSendingObjectParentForTesting(declaration1);
		AssertEquals(0, testWrapper1.SendingObjectsCollection.Count);

		declaration1.CustomsEntryHeaders.AddNew();
		declaration1.CustomsEntryHeaders.AddNew();
		AssertEquals(0, testWrapper1.SendingObjectsCollection.Count);

		var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
		declaration2.CustomsEntryHeaders.AddNew();
		declaration2.CustomsEntryHeaders.AddNew();
		var testWrapper2 = new BEJobDeclarationMessageSendingObjectParentForTesting(declaration2);
		AssertEquals(2, testWrapper2.SendingObjectsCollection.Count);
	}
}
