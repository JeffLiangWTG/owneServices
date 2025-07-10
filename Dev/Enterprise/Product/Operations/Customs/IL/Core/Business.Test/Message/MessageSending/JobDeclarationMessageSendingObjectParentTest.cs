using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(JobDeclarationMessageSendingObjectParent))]
	sealed class JobDeclarationMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestParentDeclaration()
		{
			var objectParent = (JobDeclarationMessageSendingObjectParent)GetNewBusinessObject();
			AssertType<JobDeclaration>(objectParent.ParentDeclaration);
		}

		public void TestGetSendingObjectsCollection()
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

		public void TestMessageSendingObjectProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			var testItem = new JobDeclarationMessageSendingObjectParent(declaration).MessageSendingObjectProperties.ToList();

			AssertEquals(6, testItem.Count);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "MessageType", "DeclarationType", "EntryStatus", "MessageTypeDescription", "Procedure", "EntryInstructionDescription" }, testItem.Select(x => x.PropertyName));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			return new JobDeclarationMessageSendingObjectParent(declaration);
		}
	}
}
