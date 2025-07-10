using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.DocumentMenu.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DocumentUsageDetailsCollectorTest : TestCaseWithFactory
	{
		public void TestGetMenuTitle()
		{
			var documentUsageReporter = new DocumentUsageReporter();
			var documentUsageDetailsCollector = Factory.ServiceContainer.AddService(new DocumentUsageDetailsCollector(documentUsageReporter));
			documentUsageDetailsCollector.GetMenuTitle("TestMenuTitle");
			AssertEquals("MenuTitle", "TestMenuTitle", documentUsageReporter.MenuTitle);
		}
		public void TestGetIsPreview()
		{
			var documentUsageReporter = new DocumentUsageReporter();
			var documentUsageDetailsCollector = Factory.ServiceContainer.AddService(new DocumentUsageDetailsCollector(documentUsageReporter));
			documentUsageDetailsCollector.GetIsPreview(true);
			AssertEquals("IsPreview", true, documentUsageReporter.IsPreview);
			documentUsageDetailsCollector.GetIsPreview(false);
			AssertEquals("IsPreview", false, documentUsageReporter.IsPreview);
		}

		public void TestGetTriggerModule()
		{
			var documentUsageReporter = new DocumentUsageReporter();
			var documentUsageDetailsCollector = Factory.ServiceContainer.AddService(new DocumentUsageDetailsCollector(documentUsageReporter));
			var dummyBizo = Factory.NewWithValidTestData<DocumentCommandTest.DocDummyBusinessObject>();
			var menu = Factory.NewWithValidTestData<DocumentCommand>();
			menu.Parent = dummyBizo;
			var printTask = new PrintTask(menu);

			documentUsageDetailsCollector.GetTriggeredFrom(printTask);
			AssertEquals("TriggerModule", "DocDummyBusinessObject", documentUsageReporter.TriggeredFrom);

			documentUsageReporter.TriggeredFrom = null;
			printTask.MostTopLevelBusinessObject = Factory.NewWithValidTestData<DummyBusinessObject>();
			documentUsageDetailsCollector.GetTriggeredFrom(printTask);
			AssertEquals("TriggerModule", "DummyBusinessObject", documentUsageReporter.TriggeredFrom);
		}

		public void TestGetContactDetails()
		{
			var documentUsageReporter = new DocumentUsageReporter();
			var documentUsageDetailsCollector = Factory.ServiceContainer.AddService(new DocumentUsageDetailsCollector(documentUsageReporter));
			var contact1 = new DocDeliveryContact(Factory);
			contact1.DeliveryMethod = "EML";
			contact1.AttachmentType = "PDF";
			var contact2 = new DocDeliveryContact(Factory);
			contact2.DeliveryMethod = "PRN";
			contact2.AttachmentType = "";

			documentUsageDetailsCollector.GetContactDetails(contact1);
			documentUsageDetailsCollector.GetContactDetails(contact2);

			AssertEquals("Contact1 DeliveryMethod", "EML", documentUsageReporter.ContactDetails[0].DeliveryMethod);
			AssertEquals("Contact1 AttachmentType", "PDF", documentUsageReporter.ContactDetails[0].AttachmentType);

			AssertEquals("Contact2 DeliveryMethod", "PRN", documentUsageReporter.ContactDetails[1].DeliveryMethod);
			AssertEquals("Contact2 AttachmentType", "", documentUsageReporter.ContactDetails[1].AttachmentType);
		}

		public void TestGetIsUserSignatureUsed()
		{
			var documentUsageReporter = new DocumentUsageReporter();
			var documentUsageDetailsCollector = Factory.ServiceContainer.AddService(new DocumentUsageDetailsCollector(documentUsageReporter));
			documentUsageDetailsCollector.GetIsUserSignatureUsed(true);
			AssertEquals("IsUserSignatureUsed", true, documentUsageReporter.IsUserSignatureUsed);
			documentUsageDetailsCollector.GetIsUserSignatureUsed(false);
			AssertEquals("IsUserSignatureUsed", false, documentUsageReporter.IsUserSignatureUsed);
		}
	}
}
