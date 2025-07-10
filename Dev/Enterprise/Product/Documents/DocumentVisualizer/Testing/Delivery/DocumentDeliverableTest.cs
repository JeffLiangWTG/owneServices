using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using FlexCel.XlsAdapter;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	[TestedType(typeof(DocumentDeliverable))]
	sealed class DocumentDeliverableTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var delivery = new DummyDocumentDelivery
			{
				Document = new DummyDocument(),
				PrintInstructions = new DummyPrintInstructions(),
				EDocsInstructions = new DummyEDocsInstructions(),
				LogParent = Factory.New<DummyWithLogs>()
			};

			return new DocumentDeliverable(Factory, delivery);
		}

		public void TestIDeliverable()
		{
			var stmPrintQueue = Factory.NewWithValidTestData<StmPrintQueue>();
			stmPrintQueue.SQ_DisplayName = "Test Printer";
			stmPrintQueue.SQ_ServerName = "Test";

			var documentDelivery = new DummyDocumentDelivery
			{
				Document = new DummyDocument(),
				PrintInstructions = new DummyPrintInstructions
				{
					Title = "Title",
					NumberOfCopies = 7
				},
				EDocsInstructions = new DummyEDocsInstructions(),
				LogParent = Factory.New<DummyWithLogs>(),
				DeliveryMode = "PRN"
			};

			var deliverable = new DocumentDeliverable(Factory, documentDelivery);
			var info = deliverable.GetDeliveryInfo(false, FileType.XLS);

			AssertEquals((short)7, info.Copies);

			deliverable.PrinterDetails.NumberOfCopies = 5;
			AssertEquals("should be readonly.", (short)7, info.Copies);

			AssertEquals(1, deliverable.PrinterDetails.Printers.Count);
			AssertEquals(stmPrintQueue.PK, deliverable.PrinterDetails.Printers[0].PK);

			deliverable.PrinterDetails.PrintQueuePK = CargoWise.Types.ZGuid.Invalid;
			AssertNull(deliverable.PrinterDetails.PrintQueue);
			Assert("should have error if it's invalid printer name.", deliverable.PrinterDetails.PrintQueuePKInfo.HasError("Enter a valid Printer."));

			deliverable.PrinterDetails.PrintQueuePK = stmPrintQueue.PK;
			AssertEquals(stmPrintQueue, deliverable.PrinterDetails.PrintQueue);
		}

		public void TestEDocsInstructions()
		{
			var dummyLogParent = Factory.New<DummyWithLogs>();
			var eDocsParent = Factory.New<Forwarding.IForwardingShipment>();

			var documentDelivery = new DummyDocumentDelivery
			{
				Document = new DummyDocument(),

				PrintInstructions = new DummyPrintInstructions(),
				EDocsInstructions = new DummyEDocsInstructions
				{
					SaveCopyToEDocs = true,
					Parent = eDocsParent
				},
				LogParent = dummyLogParent
			};

			var deliverable = new DocumentDeliverable(Factory, documentDelivery);

			AssertEquals("EDocsParent", eDocsParent, deliverable.EDocsParent);
			AssertEquals("SaveCopyToEDocs", true, deliverable.SaveCopyToEDocs);
		}

		public void TestGetDeliveryInfo()
		{
			var document = new DummyDocument();

			document.Rows.Add(new Row(1, 20));
			document.Columns.Add(new Column(1, 20));

			var cell = new DummyCell
			{
				LeftColumn = 1,
				RightColumn = 1,
				TopRow = 1,
				BottomRow = 1,
				Value = "hello"
			};

			document.AddCell(cell);

			var dummyLogParent = Factory.New<DummyWithLogs>();

			var documentDelivery = new DummyDocumentDelivery
			{
				Document = document,
				PrintInstructions = new DummyPrintInstructions
				{
					Title = "Title",
					DeliveryModes = new[]
					{
						nameof(PrintCopyType.ALL)
					}
				},
				EDocsInstructions = new DummyEDocsInstructions
				{
					SaveCopyToEDocs = true
				},
				LogParent = dummyLogParent
			};

			var deliverable = new DocumentDeliverable(Factory, documentDelivery);
			var info = deliverable.GetDeliveryInfo(false, FileType.XLS);

			AssertEquals(DeliveryInfo.DeliveryFormats.Document, info.DeliveryFormat);
			AssertEquals((short)1, info.Copies);
			AssertEquals(false, info.ShowDraftWatermark);

			info.FileContents.Seek(0, SeekOrigin.Begin);

			var xls = new XlsFile(info.FileContents, true);

			AssertEquals("sheets", 1, xls.SheetCount);
			AssertEquals("rows", 1, xls.RowCount);
			AssertEquals("columns", 1, xls.ColCount);
			AssertEquals("cell A1", "hello", xls.GetCellValue(1, 1));
		}

		public void TestGetDeliveryInfo_NumberOfCopies()
		{
			var documentDelivery = new DummyDocumentDelivery
			{
				Document = new DummyDocument(),
				PrintInstructions = new DummyPrintInstructions
				{
					Title = "Title",
					DeliveryModes = new[]
					{
						nameof(PrintCopyType.ALL)
					},
					NumberOfCopies = 7
				},
				EDocsInstructions = new DummyEDocsInstructions(),
				LogParent = Factory.New<DummyWithLogs>()
			};

			var deliverable = new DocumentDeliverable(Factory, documentDelivery);
			var info = deliverable.GetDeliveryInfo(false, FileType.XLS);

			AssertEquals(DeliveryInfo.DeliveryFormats.Document, info.DeliveryFormat);
			AssertEquals((short)7, info.Copies);
		}

		public void TestGetDeliveryInfo_AttachedFilename()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "HBL0000007";

			var documentDelivery = new DummyDocumentDelivery
			{
				Document = new DummyDocument
				{
					Data = dummy.MakeDynamic()
				},
				PrintInstructions = new DummyPrintInstructions
				{
					Title = "Title",
					DeliveryModes = new[]
					{
						nameof(PrintCopyType.ALL)
					},
					NumberOfCopies = 7,
					AttachmentFilename = "Document - <Z0_Description>"
				},
				EDocsInstructions = new DummyEDocsInstructions(),
				LogParent = Factory.New<DummyWithLogs>()
			};

			var deliverable = new DocumentDeliverable(Factory, documentDelivery);
			var info = deliverable.GetDeliveryInfo(false, FileType.XLS);

			AssertEquals(nameof(info.AttachedFilename), "Document - HBL0000007", info.AttachedFilename);
		}

		public void TestDocumentDeliverable_DocumentName()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "HBL0000007";

			var documentDelivery = new DummyDocumentDelivery
			{
				Document = new DummyDocument
				{
					Data = dummy.MakeDynamic()
				},
				PrintInstructions = new DummyPrintInstructions
				{
					Title = "Title",
					DeliveryModes = new[]
					{
						nameof(PrintCopyType.ALL)
					},
					NumberOfCopies = 7,
					DocumentName = "Document - O.o - <Z0_Description>",
					AttachmentFilename = "Document - <Z0_Description>"
				},
				EDocsInstructions = new DummyEDocsInstructions(),
				LogParent = Factory.New<DummyWithLogs>()
			};

			var deliverable = new DocumentDeliverable(Factory, documentDelivery);

			AssertEquals(nameof(deliverable.DocumentName), "Document - O.o - HBL0000007", deliverable.DocumentName);
		}

		public void TestGetDeliveryInfo_AttachedFilename_InvalidChairs()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = ":a?b*c";

			var documentDelivery = new DummyDocumentDelivery
			{
				Document = new DummyDocument
				{
					Data = dummy.MakeDynamic()
				},
				PrintInstructions = new DummyPrintInstructions
				{
					Title = "Title",
					DeliveryModes = new[]
					{
						nameof(PrintCopyType.ALL)
					},
					NumberOfCopies = 7,
					AttachmentFilename = "Document - <Z0_Description>"
				},
				EDocsInstructions = new DummyEDocsInstructions(),
				LogParent = Factory.New<DummyWithLogs>()
			};

			var deliverable = new DocumentDeliverable(Factory, documentDelivery);
			var info = deliverable.GetDeliveryInfo(false, FileType.XLS);

			AssertEquals(nameof(info.AttachedFilename), "Document - abc", info.AttachedFilename);
		}

		public void TestGetDeliveryInfo_AttachedFilename_LongName()
		{
			var dummy = Factory.New<DummyBusinessObject>();

			var documentDelivery = new DummyDocumentDelivery
			{
				Document = new DummyDocument
				{
					Data = dummy.MakeDynamic()
				},
				PrintInstructions = new DummyPrintInstructions
				{
					Title = "Title",
					DeliveryModes = new[]
					{
						nameof(PrintCopyType.ALL)
					},
					NumberOfCopies = 7,
					AttachmentFilename = new string('a', 1000)
				},
				EDocsInstructions = new DummyEDocsInstructions(),
				LogParent = Factory.New<DummyWithLogs>()
			};

			var deliverable = new DocumentDeliverable(Factory, documentDelivery);
			var info = deliverable.GetDeliveryInfo(false, FileType.XLS);

			AssertEquals(nameof(info.AttachedFilename), new string('a', 128), info.AttachedFilename);
		}

		public void TestGetDeliverableInfo_DeliveryInstructionsAreSpecified_ReturnDeliveryInfoWithWatermarkDependingOnInstructions()
		{
			var document = new DummyDocument();
			var instructions = new DeliveryInstructions
			{
				IsDraft = true
			};

			var dummyLogParent = Factory.New<DummyWithLogs>();

			var documentDelivery = new DummyDocumentDelivery
			{
				Document = document,
				PrintInstructions = new DummyPrintInstructions
				{
					Title = "Title",
					DeliveryModes = new[]
					{
						nameof(PrintCopyType.ALL)
					}
				},
				EDocsInstructions = new DummyEDocsInstructions(),
				LogParent = dummyLogParent
			};

			var deliverable = new DocumentDeliverable(Factory, documentDelivery);
			var info = deliverable.GetDeliveryInfo(true, FileType.XLS);
			AssertEquals(true, info.ShowDraftWatermark);

			instructions.IsDraft = false;
			info = deliverable.GetDeliveryInfo(false, FileType.XLS);
			AssertEquals(false, info.ShowDraftWatermark);
		}

		public void TestDocType()
		{
			var refDocType = Factory.NewWithValidTestData<RefDocType>();
			refDocType.RT_DocType = "TST";
			refDocType.RT_Desc = "Test";

			var dummyLogParent = Factory.New<DummyWithLogs>();

			var documentDelivery = new DummyDocumentDelivery
			{
				Document = new DummyDocument(),
				PrintInstructions = new DummyPrintInstructions(),
				EDocsInstructions = new DummyEDocsInstructions(),
				LogParent = dummyLogParent,
				DocumentType = "TST"
			};

			var deliverable = new DocumentDeliverable(Factory, documentDelivery);

			AssertEquals("DocumentTypeCode", "TST", deliverable.DocumentTypeCode);
			AssertEquals("DocumentTypeDescription", "Test", deliverable.DocumentTypeDescription);
		}
	}
}
