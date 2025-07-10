using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessageSending.Testing
{
	[TestedType(typeof(DeltaGJobDeclarationMessageSendingObjectParent))]
	public class DeltaGJobDeclarationMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			return new DeltaGJobDeclarationMessageSendingObjectParent(declaration);
		}

		public void TestParentDeclaration()
		{
			var objectParent = (DeltaGJobDeclarationMessageSendingObjectParent)GetNewBusinessObject();
			AssertType<JobDeclaration>(objectParent.ParentDeclaration);
		}

		public void TestGetSendingObjectsCollection()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var testWrapper1 = new DeltaGJobDeclarationMessageSendingObjectParent(declaration1);
			AssertEquals(0, testWrapper1.SendingObjectsCollection.Count);

			declaration1.CustomsEntryHeaders.AddNew();
			declaration1.CustomsEntryHeaders.AddNew();
			AssertEquals(0, testWrapper1.SendingObjectsCollection.Count);

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.CustomsEntryHeaders.AddNew();
			declaration2.CustomsEntryHeaders.AddNew();
			var testWrapper2 = new DeltaGJobDeclarationMessageSendingObjectParent(declaration2);
			AssertEquals(2, testWrapper2.SendingObjectsCollection.Count);
		}
	}
}
