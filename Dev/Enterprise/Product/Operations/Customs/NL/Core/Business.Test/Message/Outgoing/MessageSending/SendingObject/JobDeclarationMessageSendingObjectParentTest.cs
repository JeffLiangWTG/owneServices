using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(JobDeclarationMessageSendingObjectParent))]
sealed class JobDeclarationMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
{
	public void TestMessageSendingObjectProperties()
	{
		var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
		var testItem = new JobDeclarationMessageSendingObjectParent(declaration).MessageSendingObjectProperties.ToList();

		AssertEquals(7, testItem.Count);
		Assert("MessageSendingObjectProperties should contain \"Update\"", testItem.Exists(item => item.PropertyName == "Update"));
		Assert("MessageSendingObjectProperties should contain \"MessageType\"", testItem.Exists(item => item.PropertyName == "MessageType"));
		Assert("MessageSendingObjectProperties should contain \"EntryType\"", testItem.Exists(item => item.PropertyName == "EntryType"));
		Assert("MessageSendingObjectProperties should contain \"SubStyle\"", testItem.Exists(item => item.PropertyName == "SubStyle"));
		Assert("MessageSendingObjectProperties should contain \"Description\"", testItem.Exists(item => item.PropertyName == "Description"));
		Assert("MessageSendingObjectProperties should contain \"Date\"", testItem.Exists(item => item.PropertyName == "Date"));
		Assert("MessageSendingObjectProperties should contain \"EntryStatus\"", testItem.Exists(item => item.PropertyName == "EntryStatus"));
	}

	public void TestGetSendingObjectsCollectionCore()
	{
		var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
		var testWrapper1 = new JobDeclarationMessageSendingObjectParent(declaration1);
		AssertEquals(0, testWrapper1.SendingObjectsCollection.Count);

		declaration1.CustomsEntryHeaders.AddNew();
		declaration1.CustomsEntryHeaders.AddNew();
		AssertEquals(0, testWrapper1.SendingObjectsCollection.Count);

		var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
		declaration2.CustomsEntryHeaders.AddNew();
		declaration2.CustomsEntryHeaders.AddNew();
		var testWrapper2 = new JobDeclarationMessageSendingObjectParent(declaration2);
		AssertEquals(2, testWrapper2.SendingObjectsCollection.Count);
	}

	protected override BusinessObject GetNewBusinessObject() => new JobDeclarationMessageSendingObjectParent(Factory.New<JobDeclaration>());
}
