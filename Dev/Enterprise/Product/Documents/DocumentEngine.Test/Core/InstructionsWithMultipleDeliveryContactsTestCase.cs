using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class InstructionsWithMultipleDeliveryContactsTestCase : TestCaseWithFactory
	{
		public void TestInstructionsWithMultipleDeliveryContactsDeliverToInstructionContactsNotReportContact()
		{
			var testMenuItem = Factory.New<DocumentCommand>();
			testMenuItem.SU_ContactType = ContactType.Consignor.Code;
			var docPack = new DocumentPack(testMenuItem);

			var instructionsContact1 = new DocDeliveryContact(Factory);
			instructionsContact1.Name = deliveryContactNames[0];
			instructionsContact1.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;

			var instructionsContact2 = new DocDeliveryContact(Factory);
			instructionsContact2.Name = deliveryContactNames[1];
			instructionsContact2.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;

			var instructions = new DeliveryInstructions(docPack);
			instructions.Recipients.RemoveAll();
			instructions.Destination = DeliveryInstructionDestination.None;
			instructions.Recipients.Add(instructionsContact1);
			instructions.Recipients.Add(instructionsContact2);

			using (var embeddedResourceRetriever = new EmbeddedResourceRetriever())
			{
				var bizo = Factory.New<DummyBusinessObject>();
				var wrapper = new DummyDocumentWrapper(bizo, Factory);
				var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
				var template = new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", Path.GetFullPath(tempFileName));

				using (var report = new Report(docPack, template, wrapper, "testReport", null, DocumentDirection.ARV, false))
				{
					docPack.OnAfterReportRun += new PrintTask.AfterReportRunEventHandler(AssertItemDeliveredHasCorrectContact);

					docPack.Add(report);
					docPack.Run(instructions);
				}
			}
		}

		readonly string[] deliveryContactNames = { "Sirrus", "Achenar" };
		int currentExpectedDeliveryNameIndex;

		void AssertItemDeliveredHasCorrectContact(object sender, IDeliverable deliverable)
		{
			var report = deliverable as Report;
			AssertEquals(report.DeliveryContact.Name, deliveryContactNames[currentExpectedDeliveryNameIndex++]);
		}
	}
}
