using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.China;
using Enterprise.Messaging.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.Testing
{
	public class EInvoicingEventMessageCNProcessorForEventTest : TestCaseWithFactory
	{
		public void TestProcess()
		{
			using (var memoryStream = (SubStreamableStream)new MemoryStream(Encoding.ASCII.GetBytes("test")))
			{
				var log = new XmlSessionTracker(new ServiceTaskLogForTesting());
				var ediMessage = EDIMessageTestFactory.New(Factory);
				var eventDataObject = new UniversalEvent();
				eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext();
				var invoice = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", ObjectCreator.CNY,
					1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
				invoice.AH_ComplianceSubType = "ETA";
				invoice.AH_TransactionReference = "Gov123";
				invoice.AH_ComplianceDocumentDate = new ZDate(2022, 4, 25);

				var processor = new EInvoicingEventMessageCNProcessorForEvent(log, ediMessage, eventDataObject, invoice);
				processor.Process();

				Assert(!HasAdjustedDDIReference(null));

				eventDataObject.EventType = AutoEvents.InterchangeAcknowledgedCode;

				processor.Process();

				Assert(!HasAdjustedDDIReference(null));

				invoice.Logs.AddNew(Events.DocumentImported);
				var documentType = new DocumentType()
				{
					Code = "MSC", Description = "Miscellaneous Document"
				};
				var attachedDocument1 = new AttachedDocument()
				{
					Type = documentType,
					FileName = "a.pdf",
					IsPublished = true,
					ImageData = memoryStream
				};
				eventDataObject.AttachedDocumentCollection = new List<AttachedDocument>();
				eventDataObject.AttachedDocumentCollection.Add(attachedDocument1);

				processor.Process();

				Assert(HasAdjustedDDIReference("a"));

				invoice.Logs.GetAllLogs().RemoveAndDeleteAll();
				invoice.Logs.AddNew(Events.DocumentImported);
				invoice.Logs.AddNew(Events.DocumentImported);

				var attachedDocument2 = new AttachedDocument()
				{
					Type = documentType,
					FileName = "b.pdf",
					IsPublished = true,
					ImageData = memoryStream
				};
				eventDataObject.AttachedDocumentCollection.Add(attachedDocument2);

				invoice.DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "b", "INV");
				var fileName = "VAT_" + invoice.AH_TransactionNum + "_b";

				processor.Process();

				Assert(HasAdjustedDDIReference("a"));
				Assert(HasAdjustedDDIReference(fileName));
				AssertEquals(1, invoice.DocManagerInfo.AllEDocs.Count);
				AssertEquals(fileName, invoice.DocManagerInfo.AllEDocs[0].FileName);

				bool HasAdjustedDDIReference(string expectedReference)
				{
					if (expectedReference.IsNullOrEmpty())
					{
						return invoice.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == AutoEvents.DocumentImportedCode);
					}
					else
					{
						return invoice.Logs.GetAllLogs().Cast<StmALog>().Any(x =>
							x.SL_SE_NKEvent == AutoEvents.DocumentImportedCode && x.SL_Reference == expectedReference);
					}
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			ObjectCreator = new TestObjectCreator(Factory);
		}

		TestObjectCreator ObjectCreator;
	}
}
