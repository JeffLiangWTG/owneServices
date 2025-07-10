using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	[TestedType(typeof(DeliverableCollectionView))]
	sealed class DeliverableCollectionViewTest : BusinessObjectCollectionViewTestCase<DeliverableCollectionView>
	{
		public void TestIncludedDocuments()
		{
			var deliveryContacts = new DocDeliveryContactCollection(Factory);
			var contact = deliveryContacts.AddNew();
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;

			var reports = new ReportCollection(Factory);
			var report1 = (Report)reports.AddNew();
			report1.PrintCopyType = PrintCopyType.EML;

			var report2 = (Report)reports.AddNew();
			report2.PrintCopyType = PrintCopyType.PRN;

			var view = new DeliverableCollectionView(reports, deliveryContacts);
			AssertEquals("report1 should be included", true, view[0].IncludedInPrint);
			AssertEquals("report2 should not be included", false, view[1].IncludedInPrint);
		}

		public void TestRebuildOccursOnceOnConstruction()
		{
			var reports = new ReportCollection();
			reports.AddNew();
			var view = new DeliverableCollectionViewForTesting(reports, new DocDeliveryContactCollection(Factory));

			AssertEquals("Ensure we dont RebuildOnConstruction() and then Rebuild()", 1, view.NumberOfCallsToRebuildCore);
		}

		public void TestAllowSort()
		{
			var reports = new ReportCollection();
			reports.AddNew();
			var view = new DeliverableCollectionViewForTesting(reports, new DocDeliveryContactCollection(Factory));
			AssertEquals("SupportsSorting", false, ((IBindingList)view).SupportsSorting);
		}

		public void TestHasIncludedDocuments()
		{
			var deliveryContacts = new DocDeliveryContactCollection(Factory);
			var contact = deliveryContacts.AddNew();
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;

			var reports = new ReportCollection(Factory);
			var report1 = (Report)reports.AddNew();
			report1.PrintCopyType = PrintCopyType.ALL;

			var report2 = (Report)reports.AddNew();
			report2.PrintCopyType = PrintCopyType.ALL;

			var view = new DeliverableCollectionView(reports, deliveryContacts);
			AssertEquals("View has 2 items", 2, view.Count);

			AssertEquals("View has documents to be included", true, view.HasIncludedDocuments);

			report1.IncludedInPrint = false;

			AssertEquals("View still has documents to be included", true, view.HasIncludedDocuments);

			report2.IncludedInPrint = false;

			AssertEquals("View has NO documents to be included", false, view.HasIncludedDocuments);
			AssertEquals("View still has two items in it", 2, view.Count);
		}

		public void TestIncludeInPrintReadOnly()
		{
			var deliveryContacts = new DocDeliveryContactCollection(Factory);

			var pack = new DocumentPack();
			var deliverable = new DummyDeliverable();
			pack.Add(deliverable);

			var contact1 = new DocDeliveryContact(Factory) { DeliveryMethod = Core.Constants.ContactNotifyModes.Fax };
			var contact2 = new DocDeliveryContact(Factory) { DeliveryMethod = Core.Constants.ContactNotifyModes.Print };

			deliveryContacts.Add(contact1);
			var view = new DeliverableCollectionView(pack, deliveryContacts);
			view.Rebuild();
			AssertEquals("View has 1 items", 1, view.Count);
			Assert("Deliverable should be read only", view[0].IncludedInPrint_ReadOnly);

			deliveryContacts.Add(contact2);
			view.Rebuild();
			Assert("Deliverable should not be read only", !view[0].IncludedInPrint_ReadOnly);
		}

		public void TestIsPrintable()
		{
			var docPack = new DocumentPack();
			var report1 = (Report)docPack.AddNew();
			report1.PrintCopyType = PrintCopyType.EML;

			var report2 = (Report)docPack.AddNew();
			report2.PrintCopyType = PrintCopyType.FAX;

			var report3 = (Report)docPack.AddNew();
			report3.PrintCopyType = PrintCopyType.PRN;

			var report4 = (Report)docPack.AddNew();
			report4.PrintCopyType = PrintCopyType.ALL;

			var instructions = new DeliveryInstructions(docPack);
			instructions.Recipients.RemoveAndDeleteAll();
			AssertEquals("Precondition: No contacts exist", 0, instructions.Recipients.Count);

			var contact1 = new DocDeliveryContact(Factory) { DeliveryMethod = Core.Constants.ContactNotifyModes.Email };
			var contact2 = new DocDeliveryContact(Factory) { DeliveryMethod = Core.Constants.ContactNotifyModes.Fax };
			var contact3 = new DocDeliveryContact(Factory) { DeliveryMethod = Core.Constants.ContactNotifyModes.Print };
			var contact4 = new DocDeliveryContact(Factory) { DeliveryMethod = Core.Constants.ContactNotifyModes.EPrint };

			AssertEquals("View has 4 items in it", 4, instructions.DeliverablesToBePrinted.Count);

			instructions.Recipients.RemoveAll();
			instructions.Recipients.Add(contact1);
			AssertRecipientsAndIncludedInPrintAndCanIncludeInPrint(instructions, 1, 4, true, false, false, true);

			instructions.Recipients.RemoveAll();
			instructions.Recipients.Add(contact2);
			AssertRecipientsAndIncludedInPrintAndCanIncludeInPrint(instructions, 1, 4, false, true, false, true);

			instructions.Recipients.RemoveAll();
			instructions.Recipients.Add(contact3);
			AssertRecipientsAndIncludedInPrintAndCanIncludeInPrint(instructions, 1, 4, false, false, true, true);

			instructions.Recipients.RemoveAll();
			instructions.Recipients.Add(contact4);
			AssertRecipientsAndIncludedInPrintAndCanIncludeInPrint(instructions, 1, 4, false, false, true, true);

			instructions.Recipients.RemoveAll();
			AssertRecipientsAndIncludedInPrintAndCanIncludeInPrint(instructions, 0, 4, true, true, true, true);
		}

		void AssertRecipientsAndIncludedInPrintAndCanIncludeInPrint(DeliveryInstructions instructions, int recipientsCount, int deliverablesToBePrintedCount,
			bool shouldIncludeEmailItemInPrint, bool shouldIncludeFaxItemInPrint, bool shouldIncludePrintItemInPrint, bool shouldIncludeAllItemInPrint)
		{
			Func<DeliveryInstructions, PrintCopyType, IDeliverable> getItem = (inst, printCopyType) => (IDeliverable)inst.DeliverablesToBePrinted.First(p => ((Report)p).PrintCopyType == printCopyType);

			Func<string, bool, string> getIncludedInPrintMessage = (printType, shouldInclude) => string.Format("{0} item will {1}print", printType, shouldInclude ? "" : "NOT ");
			Func<string, bool, string> getCanIncludeInPrintMessage = (printType, shouldInclude) => string.Format("{0} item can {1}be included in print", printType, shouldInclude ? "" : "NOT ");

			AssertEquals(string.Format("View should have {0} recipient", recipientsCount),
				recipientsCount, instructions.Recipients.Count);

			AssertEquals(string.Format("View should have {0} items in it", deliverablesToBePrintedCount),
				deliverablesToBePrintedCount, instructions.DeliverablesToBePrinted.Count);

			var emailItem = getItem(instructions, PrintCopyType.EML);
			var faxItem = getItem(instructions, PrintCopyType.FAX);
			var printItem = getItem(instructions, PrintCopyType.PRN);
			var allItem = getItem(instructions, PrintCopyType.ALL);

			AssertEquals(getIncludedInPrintMessage("Email", shouldIncludeEmailItemInPrint), shouldIncludeEmailItemInPrint, emailItem.IncludedInPrint);
			AssertEquals(getIncludedInPrintMessage("Fax", shouldIncludeFaxItemInPrint), shouldIncludeFaxItemInPrint, faxItem.IncludedInPrint);
			AssertEquals(getIncludedInPrintMessage("Print", shouldIncludePrintItemInPrint), shouldIncludePrintItemInPrint, printItem.IncludedInPrint);
			AssertEquals(getIncludedInPrintMessage("ALL", shouldIncludeAllItemInPrint), shouldIncludeAllItemInPrint, allItem.IncludedInPrint);

			AssertEquals(getCanIncludeInPrintMessage("Email", shouldIncludeEmailItemInPrint), shouldIncludeEmailItemInPrint, emailItem.CanIncludeInPrint);
			AssertEquals(getCanIncludeInPrintMessage("Fax", shouldIncludeFaxItemInPrint), shouldIncludeFaxItemInPrint, faxItem.CanIncludeInPrint);
			AssertEquals(getCanIncludeInPrintMessage("Print", shouldIncludePrintItemInPrint), shouldIncludePrintItemInPrint, printItem.CanIncludeInPrint);
			AssertEquals(getCanIncludeInPrintMessage("ALL", shouldIncludeAllItemInPrint), shouldIncludeAllItemInPrint, allItem.CanIncludeInPrint);
		}

		public void TestRecipients()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
	"Test", string.Empty,
	@"{A}-[#Config]
{A}-[#EndOfReport]");

			using (var printTask = new PrintTask())
			{
				var docPack = new DocumentPack();
				printTask.Add(docPack);

				var report1 = new Report(docPack, excelTemplate);
				report1.PrintCopyType = PrintCopyType.EML;
				docPack.Add(report1);
				var report2 = new Report(docPack, excelTemplate);
				report2.PrintCopyType = PrintCopyType.FAX;
				docPack.Add(report2);
				var report3 = new Report(docPack, excelTemplate);
				report3.PrintCopyType = PrintCopyType.PRN;
				docPack.Add(report3);
				var report4 = new Report(docPack, excelTemplate);
				report4.PrintCopyType = PrintCopyType.ALL;
				docPack.Add(report4);

				var instructions = new DeliveryInstructions(docPack);
				instructions.Recipients.RemoveAndDeleteAll();
				AssertEquals("Precondition: No contacts exist", 0, instructions.Recipients.Count);
				AssertEquals("Precondition: View has 4 items in it", 4, instructions.DeliverablesToBePrinted.Count);

				var contact1 = new DocDeliveryContact(Factory) { DeliveryMethod = Core.Constants.ContactNotifyModes.Print };
				instructions.Recipients.RemoveAll();
				instructions.Recipients.Add(contact1);
				AssertRecipientsAndIncludedInPrintAndCanIncludeInPrint(instructions, 1, 4, false, false, true, true);

				var mockPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
				using (new PrintTaskUIProviderFactory.OverriderForTesting(mockPrintTaskUIProvider.Object))
				{
					mockPrintTaskUIProvider.Setup(m => m.ShowDocDeliveryUI(It.IsAny<PrintTask>(), It.IsAny<DeliveryInstructions>(), It.IsAny<ISecurityCheckpoint>())).Returns(true);
					printTask.Run(Env.Security.None);
				}

				AssertRecipientsAndIncludedInPrintAndCanIncludeInPrint(instructions, 1, 4, false, false, true, true);
			}
		}

		public void TestHasPreviewableDocuments()
		{
			var docPack = new DocumentPack();
			var report1 = (Report)docPack.AddNew();
			report1.PrintCopyType = PrintCopyType.EML;
			var instructions = new DeliveryInstructions(docPack);

			var contact1 = new DocDeliveryContact(Factory) { DeliveryMethod = Core.Constants.ContactNotifyModes.Email };
			var contact2 = new DocDeliveryContact(Factory) { DeliveryMethod = Core.Constants.ContactNotifyModes.Print };
			var contact3 = new DocDeliveryContact(Factory) { DeliveryMethod = Core.Constants.ContactNotifyModes.EPrint };

			instructions.Recipients.Add(contact1);
			AssertEquals("previewable document", true, instructions.DeliverablesToBePrinted.HasIncludedPreviewableDocuments);

			instructions.Recipients.RemoveAll();
			instructions.Recipients.Add(contact2);
			AssertEquals("previewable document", false, instructions.DeliverablesToBePrinted.HasIncludedPreviewableDocuments);

			instructions.Recipients.RemoveAll();
			instructions.Recipients.Add(contact3);
			AssertEquals("previewable document", false, instructions.DeliverablesToBePrinted.HasIncludedPreviewableDocuments);
		}

		public override void TestAddNew()
		{
			// this is a view therefore nothing will call AddNew on it
			Assert(true);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			using (var embeddedResourceRetriever = new EmbeddedResourceRetriever())
			{
				var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
				var emptyAndValidTemplate = new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", Path.GetFullPath(tempFileName));
				return new Report(new DocumentPack(), emptyAndValidTemplate);
			}
		}

		protected override DeliverableCollectionView GetCollectionToTest() => new DeliverableCollectionView(new ReportCollection(), new DocDeliveryContactCollection(Factory));
	}
}
