using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(JobDeclarationMessageSendingObjectParent))]
	sealed class JobDeclarationMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestParentDeclaration()
		{
			var objectParent = (JobDeclarationMessageSendingObjectParent)GetNewBusinessObject();
			AssertType<JobDeclaration>(objectParent.ParentDeclaration);
		}

		public void TestObjectsToSend()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.MovementReferenceNumberSetter("mrn", ZDateTime.BrettsBirthday);
			entry2.CH_EntryStatus = GBCommonConstants.EntryStatusCodes.DeclarationAccepted;
			var objectParent = new JobDeclarationMessageSendingObjectParent(declaration);
			AssertEquals(1, objectParent.ObjectsToSend.Count());
			AssertEquals(entry1, objectParent.ObjectsToSend.Single().Header);
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

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			return new JobDeclarationMessageSendingObjectParent(declaration);
		}
	}
}
