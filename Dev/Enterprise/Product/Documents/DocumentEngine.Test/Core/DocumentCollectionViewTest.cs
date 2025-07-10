using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DocumentEngine.DocumentMenu.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	[TestedType(typeof(DocumentCollectionView))]
	sealed class DocumentCollectionViewTest : BusinessObjectCollectionViewTestCase<DocumentCollectionView>
	{
		public void TestShouldSuspendRecipientsValidationWhenRebuild()
		{
			var docPack = new DocumentPack();
			var report = GetNewElementToAddToTheCollection();
			((IDeliverable)report).CanIncludeInPrint = false;
			docPack.Add(report);

			var recipients = new DocDeliveryContactCollection(Factory);
			recipients.Deliverables = docPack;
			var contact = new DocDeliveryContactForTest(Factory);
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			((DocDeliveryContactValidationForTest)contact.Validation).CallCountForValidateDeliveryMethod = 0;
			recipients.Add(contact);

			var documentsView = new DocumentCollectionView(docPack, recipients);
			documentsView.Rebuild();
			AssertEquals("Should only be called once", 1, ((DocDeliveryContactValidationForTest)contact.Validation).CallCountForValidateDeliveryMethod);
		}

		public void TestRebuild()
		{
			var docPack = new DocumentPack();
			var dumy = Factory.New<DocumentPrintSetTest.DummyBizoStorageDocs>();
			docPack.Add(dumy);
			var report = GetNewElementToAddToTheCollection();
			docPack.Add(report);

			AssertEquals(2, docPack.Count);

			var documentsView = new DocumentCollectionView(docPack, new DocDeliveryContactCollection(Factory));
			AssertEquals(1, documentsView.Count);
		}

		protected override DocumentCollectionView GetCollectionToTest() => new DocumentCollectionView(new ReportCollection(), new DocDeliveryContactCollection(Factory));

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			using (var embeddedResourceRetriever = new EmbeddedResourceRetriever())
			{
				var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
				var emptyAndValidTemplate = new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", Path.GetFullPath(tempFileName));
				return new Report(new DocumentPack(), emptyAndValidTemplate);
			}
		}
	}
}
