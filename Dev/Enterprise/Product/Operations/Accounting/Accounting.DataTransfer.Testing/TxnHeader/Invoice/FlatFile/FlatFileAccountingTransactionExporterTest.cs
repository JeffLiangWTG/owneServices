using System;
using System.Collections.Specialized;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.DataTransfer.Testing;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.IO;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices.FlatFile.Testing
{
	[TestedType(typeof(TestFlatFileAccountingTransactionExporter))]
	public class FlatFileAccountingTransactionExporterTest : FlatFileAccountingTransactionExporterTestCase
	{
		protected override AccountingTransactionsDataExporter NewExporter
		{
			get { return new TestFlatFileAccountingTransactionExporter(Factory); }
		}

		protected override bool ShouldTestMessagesToDisplayWhenExportIsFinished()
		{
			return true;
		}

		protected override bool OrderOfLinesIsImportantWhenExporting
		{
			get { return true; }
		}
	}

	[TestedType(typeof(TestFlatFileAccountingTransactionExporter))]
	public abstract class FlatFileAccountingTransactionExporterTestCase : AccountingTransactionsDataExporterTestCase
	{
		public void TestExportObjectsToEndPoint_ExportsWipAccrualsAndTransactions()
		{
			const int BatchNo = 999;

			ObjectCreator.CreateGenExportBatchSequencePostLine(BatchNo, WIP.PK, 1);
			ObjectCreator.CreateGenExportBatchSequenceHeader(BatchNo, Invoice.PK, 2);

			Factory.Save();

			FlatFileAccountingTransactionExporterThatWritesTestData exporter = new FlatFileAccountingTransactionExporterThatWritesTestData(Factory);
			exporter.FilterProvider.CurrentBatchNo = BatchNo;
			StringCollection fileAsStringCollection = new StringCollection();

			using (ExportToMemoryStream memoryStreamExporter = new ExportToMemoryStream(exporter))
			{
				foreach (string line in memoryStreamExporter.Export())
				{
					fileAsStringCollection.Add(line);
				}

				AssertEquals(2, fileAsStringCollection.Count);
			}

			AssertCollectionContains(typeof(Xsd.WipOrAccrual).FullName, fileAsStringCollection);
			AssertCollectionContains(typeof(Xsd.TxnHeader).FullName, fileAsStringCollection);
		}

		public void TestOnlyOneBizObjectProcessedCollectorIsBothEOFAndBOF()
		{
			ExportedFile = new ExportToMemoryStream(Exporter);
			bool hasHeader = false;
			bool hasFooter = false;

			using (FileLineIterator file = ExportedFile.Export())
			{
				foreach (string line in file)
				{
					if (line.StartsWith("HEADER"))
					{
						hasHeader = true;
					}

					if (line.StartsWith("FOOTER"))
					{
						hasFooter = true;
					}
				}
			}

			Assert("Header did not get written to file", hasHeader);
			Assert("Footer did not get written to file", hasFooter);
		}

		public void TestFlushOccured()
		{
			bool lastLineWritten = false;
			ExportedFile = new ExportToMemoryStream(Exporter);

			using (FileLineIterator file = ExportedFile.Export())
			{
				foreach (string line in file)
				{
					if (line.StartsWith("FOOTER"))
					{
						lastLineWritten = true;
					}
				}
			}

			Assert("Header did not get written to file", lastLineWritten);
		}

		public void TestNumberOfTransactionsProcessedIsIncrementedCorrectly()
		{
			if (ShouldTestMessagesToDisplayWhenExportIsFinished())
			{
				FlatFileAccountingTransactionExporter exporter = (FlatFileAccountingTransactionExporter)NewExporter;

				InvoicingBase aRInvoice1 = CreateInvoiceAndSetBatchNumber(typeof(ARInvoice), "AR", "INV", 100);
				InvoicingBase aPInvoice1 = CreateInvoiceAndSetBatchNumber(typeof(APInvoice), "AP", "INV", 100);
				InvoicingBase aRCreditNote1 = CreateInvoiceAndSetBatchNumber(typeof(ARCreditNote), "AR", "CRD", 100);
				InvoicingBase aPCreditNote1 = CreateInvoiceAndSetBatchNumber(typeof(APCreditNote), "AP", "CRD", 100);
				InvoicingBase aRAdjustmentNote1 = CreateInvoiceAndSetBatchNumber(typeof(ARAdjustmentNote), "AR", "ADJ", 100);
				InvoicingBase aPAdjustmentNote1 = CreateInvoiceAndSetBatchNumber(typeof(APAdjustmentNote), "AP", "ADJ", 100);
				BaseWIPAccrual wip1 = CreateWipAccrualAndSetBatchNumber(typeof(WIP), ZArchitecture.Core.TransactionLineTypes.WIP, 100, 100);
				BaseWIPAccrual wip2 = CreateWipAccrualAndSetBatchNumber(typeof(WIP), ZArchitecture.Core.TransactionLineTypes.WIP, 100, 100);
				BaseWIPAccrual accrual1 = CreateWipAccrualAndSetBatchNumber(typeof(Accrual), ZArchitecture.Core.TransactionLineTypes.Accrual, 100, 100);
				BaseWIPAccrual accrual2 = CreateWipAccrualAndSetBatchNumber(typeof(Accrual), ZArchitecture.Core.TransactionLineTypes.Accrual, 100, 100);
				Factory.Save();

				exporter.FilterProvider.CurrentBatchNo = 100;
				exporter.FilterProvider.IncludeARInvoices = true;
				exporter.FilterProvider.IncludeAPInvoices = true;
				exporter.FilterProvider.IncludeARCreditNotes = true;
				exporter.FilterProvider.IncludeAPCreditNotes = true;
				exporter.FilterProvider.IncludeARAdjustmentNotes = true;
				exporter.FilterProvider.IncludeAPAdjustmentNotes = true;
				exporter.FilterProvider.IncludeWIPsPosting = true;
				exporter.FilterProvider.IncludeWIPsReversing = true;
				exporter.FilterProvider.IncludeAccrualsPosting = true;
				exporter.FilterProvider.IncludeAccrualsReversing = true;

				exporter.Export(new MemoryStream());

				AssertEquals("Number of Invoices in Batch", 2, exporter.NumberOfInvoicesInBatch);
				AssertEquals("Number of Credit Notes in Batch", 2, exporter.NumberOfCreditNotesInBatch);
				AssertEquals("Number of Adjustment Notes in Batch", 2, exporter.NumberOfAdjustmentNotesInBatch);
				AssertEquals("Number of Wip Posting in Batch", 2, exporter.NumberOfWipPostingsInBatch);
				AssertEquals("Number of Wip Reversing in Batch", 2, exporter.NumberOfWipReversalsInBatch);
				AssertEquals("Number of Accrual Posting in Batch", 2, exporter.NumberOfAccrualPostingsInBatch);
				AssertEquals("Number of Accrual Reversing in Batch", 2, exporter.NumberOfAccrualReversalsInBatch);

				AssertEquals("Number of Invoice Processed in Standard XML Format", 2, exporter.NumberOfInvoicesProcessed_Standard);
				AssertEquals("Number of Credit Notes Processed in Standard XML Format", 2, exporter.NumberOfCreditNotesProcessed_Standard);
				AssertEquals("Number of Adjustment Notes Processed in Standard XML Format", 2, exporter.NumberOfAdjustmentNotesProcessed_Standard);
				AssertEquals("Number of Wip Posting Processed in Standard XML Format", 2, exporter.NumberOfWipPostingProcessed_Standard);
				AssertEquals("Number of Wip Reversing Processed in Standard XML Format", 2, exporter.NumberOfWipReversingProcessed_Standard);
				AssertEquals("Number of Accrual Posting Processed in Standard XML Format", 2, exporter.NumberOfAccrualPostingProcessed_Standard);
				AssertEquals("Number of Accrual Reversing Processed in Standard XML Format", 2, exporter.NumberOfAccrualReversingProcessed_Standard);

				AssertEquals("Number of Invoice Processed in Flat File Format", 2, exporter.NumberOfInvoicesProcessed_FlatFile);
				AssertEquals("Number of Credit Notes Processed in Flat File Format", 2, exporter.NumberOfCreditNotesProcessed_FlatFile);
				AssertEquals("Number of Adjustment Notes Processed in Flat File Format", 2, exporter.NumberOfAdjustmentNotesProcessed_FlatFile);
				AssertEquals("Number of Wip Posting Processed in Flat File Format", 2, exporter.NumberOfWipPostingProcessed_FlatFile);
				AssertEquals("Number of Wip Reversing Processed in Flat File Format", 2, exporter.NumberOfWipReversingProcessed_FlatFile);
				AssertEquals("Number of Accrual Posting Processed in Flat File Format", 2, exporter.NumberOfAccrualPostingProcessed_FlatFile);
				AssertEquals("Number of Accrual Reversing Processed in Flat File Format", 2, exporter.NumberOfAccrualReversingProcessed_FlatFile);

				AssertEquals("There was an error with the export", false, exporter.ErrorHasOccured);
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual InvoicingBase CreateInvoiceAndSetBatchNumber(Type type, ZString ledger, ZString transactionType, int batchNumber)
		{
			InvoicingBase invoice = PopulateInvoice(type, 100, 10, 1.0M, ObjectCreator.AUD);
			invoice.AH_Ledger = ledger;
			invoice.AH_TransactionType = transactionType;
			ObjectCreator.CreateGenExportBatchSequenceHeader(batchNumber, invoice.PK, 1);
			return invoice;
		}

		protected virtual BaseWIPAccrual CreateWipAccrualAndSetBatchNumber(Type type, ZString lineType, int exportBatchNumber, int exportReverseBatchNumber)
		{
			BaseWIPAccrual wipAccrual = FillWIPAccrualBizObjWithTestData(type, ObjectCreator.CC1, 100);
			ObjectCreator.CreateGenExportBatchSequencePostLine(exportBatchNumber, wipAccrual.PK, 1);
			wipAccrual.AL_LineType = lineType;
			ObjectCreator.CreateGenExportBatchSequenceReverseLine(exportReverseBatchNumber, wipAccrual.PK, 1);
			wipAccrual.AL_PostDate = new ZDateTime(2005, 01, 01, 11, 0, 0);

			if (wipAccrual.AL_LineType == TransactionLineTypes.WIP)
			{
				BaseCharge charge = Factory.LoadTop1<BaseCharge>(new ZQuery(JobChargeSchema.JR_AL_ARLine, wipAccrual.PK));
				if (charge != null)
				{
					charge.ReverseWIP(new ZDateTime(2005, 01, 02, 11, 0, 0));
				}
			}
			if (wipAccrual.AL_LineType == TransactionLineTypes.Accrual)
			{
				BaseCharge charge = Factory.LoadTop1<BaseCharge>(new ZQuery(JobChargeSchema.JR_AL_APLine, wipAccrual.PK));
				if (charge != null)
				{
					charge.ReverseAccrual(new ZDateTime(2005, 01, 02, 11, 0, 0));
				}
			}
			return wipAccrual;
		}

		protected override ZString ExpectedMessageWhenErrorOccurs()
		{
			ZStringBuilder result = new ZStringBuilder();
			result.Append("BATCH ERROR: There were problems exporting Batch 1.");
			result.Append("BATCH ERROR: Please do not use the files exported and contact support." + System.Environment.NewLine);
			result.Append("ERROR: There were problems exporting the following transactions: Invoices");
			result.Append(" - Transactions in Batch: 0");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(" - Exported to Flat File Format: 1");
			result.Append(ZString.Empty);
			result.Append("ERROR: There were problems exporting the following transactions: Credit Notes");
			result.Append(" - Transactions in Batch: 0");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(" - Exported to Flat File Format: 1");
			result.Append(ZString.Empty);
			result.Append("ERROR: There were problems exporting the following transactions: Adjustment Notes");
			result.Append(" - Transactions in Batch: 0");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(" - Exported to Flat File Format: 1");
			result.Append(ZString.Empty);
			result.Append("ERROR: There were problems exporting the following transactions: WIP Posting");
			result.Append(" - Transactions in Batch: 0");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(" - Exported to Flat File Format: 1");
			result.Append(ZString.Empty);
			result.Append("ERROR: There were problems exporting the following transactions: WIP Reversing");
			result.Append(" - Transactions in Batch: 0");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(" - Exported to Flat File Format: 1");
			result.Append(ZString.Empty);
			result.Append("ERROR: There were problems exporting the following transactions: Accrual Posting");
			result.Append(" - Transactions in Batch: 0");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(" - Exported to Flat File Format: 1");
			result.Append(ZString.Empty);
			result.Append("ERROR: There were problems exporting the following transactions: Accrual Reversing");
			result.Append(" - Transactions in Batch: 0");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(" - Exported to Flat File Format: 1");
			result.Append(ZString.Empty);
			return result.ToStringWithNewLineBetweenAppends();
		}

		protected override ZString ExpectedMessageWhenThereIsNoError()
		{
			ZStringBuilder result = new ZStringBuilder();
			result.Append("Batch 1 was exported successfully.");
			result.Append("");
			result.Append("The following transactions were exported successfully: Invoices");
			result.Append(" - Transactions in Batch: 1");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(" - Exported to Flat File Format: 1");
			result.Append(ZString.Empty);
			result.Append("The following transactions were exported successfully: Credit Notes");
			result.Append(" - Transactions in Batch: 1");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(" - Exported to Flat File Format: 1");
			result.Append(ZString.Empty);
			result.Append("The following transactions were exported successfully: Adjustment Notes");
			result.Append(" - Transactions in Batch: 1");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(" - Exported to Flat File Format: 1");
			result.Append(ZString.Empty);
			result.Append("The following transactions were exported successfully: " + "WIP Posting");
			result.Append(" - Transactions in Batch: 1");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(" - Exported to Flat File Format: 1");
			result.Append(ZString.Empty);
			result.Append("The following transactions were exported successfully: " + "WIP Reversing");
			result.Append(" - Transactions in Batch: 1");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(" - Exported to Flat File Format: 1");
			result.Append(ZString.Empty);
			result.Append("The following transactions were exported successfully: Accrual Posting");
			result.Append(" - Transactions in Batch: 1");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(" - Exported to Flat File Format: 1");
			result.Append(ZString.Empty);
			result.Append("The following transactions were exported successfully: Accrual Reversing");
			result.Append(" - Transactions in Batch: 1");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(" - Exported to Flat File Format: 1");
			result.Append(ZString.Empty);
			return result.ToStringWithNewLineBetweenAppends();
		}

		protected override void AssertDocumentsAreTheSame(StreamReader firstDocument, StreamReader secondDocument)
		{
			if (OrderOfLinesIsImportantWhenExporting)
			{
				base.AssertDocumentsAreTheSame(firstDocument, secondDocument);
			}
			else
			{
				StringCollection firstDoc = GetStringCollection(firstDocument);
				StringCollection secondDoc = GetStringCollection(secondDocument);

				ZString message = "The second batch was exported differently to the first batch";
				message += "FIRST BATCH CONTENTS: " + System.Environment.NewLine + firstDoc.ToString();
				message += System.Environment.NewLine + System.Environment.NewLine;
				message += "SECOND BATCH CONTENTS: " + System.Environment.NewLine + secondDoc.ToString();

				foreach (string firstDocString in firstDoc)
				{
					Assert(message, secondDoc.Contains(firstDocString));
				}

				foreach (string secondDocString in secondDoc)
				{
					Assert(message, firstDoc.Contains(secondDocString));
				}
			}
		}

		StringCollection GetStringCollection(StreamReader document)
		{
			StringCollection collection = new StringCollection();
			string lineFromFile = null;
			while ((lineFromFile = document.ReadLine()) != null)
			{
				collection.Add(lineFromFile);
			}
			return collection;
		}

		protected abstract bool OrderOfLinesIsImportantWhenExporting
		{
			get;
		}

		#region NonPersistenBusinessObjectTestCase

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TestFlatFileAccountingTransactionExporter(Factory);
		}

		public void TestDataCollectorUnitsProcessed()
		{
			ExportedFile = new ExportToMemoryStream(Exporter);
			TestFlatFileAccountingTransactionExporter exporterInstance = (TestFlatFileAccountingTransactionExporter)ExportedFile.ExportAndReturnExporterInstance();
			ITransferDataConsumer consumer = exporterInstance.GetConverter();
			AssertEquals(exporterInstance.LastBatchNumberOfTransactions, consumer.DataCollector.UnitsProcessed);
		}

		#endregion

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();

			Exporter = new TestFlatFileAccountingTransactionExporter(Factory);

			Exporter.FilterProvider.IncludeAccrualsPosting = true;
			Exporter.FilterProvider.IncludeAccrualsReversing = true;
			Exporter.FilterProvider.IncludeAPAdjustmentNotes = true;
			Exporter.FilterProvider.IncludeAPCreditNotes = true;
			Exporter.FilterProvider.IncludeAPInvoices = true;
			Exporter.FilterProvider.IncludeARAdjustmentNotes = true;
			Exporter.FilterProvider.IncludeARCreditNotes = true;
			Exporter.FilterProvider.IncludeARInvoices = true;
			Exporter.FilterProvider.IncludeWIPsPosting = true;
			Exporter.FilterProvider.IncludeWIPsReversing = true;
		}

		protected override void TearDown()
		{
			base.TearDown();

			if (ExportedFile != null)
			{
				ExportedFile.Dispose();
			}
		}

		TestFlatFileAccountingTransactionExporter Exporter;
		ExportToMemoryStream ExportedFile;

		#region ExportToMemoryStream

		public class ExportToMemoryStream : IDisposable
		{
			public ExportToMemoryStream(FlatFileAccountingTransactionExporter exporter)
			{
				this.Exporter = exporter;
				LocalFile = TempFile.New();
			}

			public FileLineIterator Export()
			{
				using (FileStream str = new FileStream(LocalFile.Filename, FileMode.Append))
				{
					Exporter.Export(str);
				}
				return new FileLineIterator(LocalFile.Filename);
			}

			public FlatFileAccountingTransactionExporter ExportAndReturnExporterInstance()
			{
				Export().Close();
				return Exporter;
			}

			#region IDisposable Members

			public void Dispose()
			{
				LocalFile.Dispose();
			}

			#endregion

			readonly TempFile LocalFile;
			readonly FlatFileAccountingTransactionExporter Exporter;
		}

		#endregion

		#region FlatFileAccountingTransactionExporterThatWritesTestData

		public class FlatFileAccountingTransactionExporterThatWritesTestData : TestFlatFileAccountingTransactionExporter
		{
			public FlatFileAccountingTransactionExporterThatWritesTestData(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override AccountingFlatFileConverter Converter
			{
				get { return new ConverterThatWritesTestData(Notify, Factory); }
			}
		}

		public class ConverterThatWritesTestData : AccountingFlatFileConverter
		{
			public ConverterThatWritesTestData(INotifications notify, BusinessObjectFactory factory)
				: base(notify, factory)
			{
			}

			protected override FlatFileDataRowCollection MapExport(IValueObject valueObject)
			{
				FlatFileDataRowCollection result = new FlatFileDataRowCollection();
				FlatFileDataRow row = new FlatFileDataRow(1);

				row.SetField(0, valueObject.GetType().FullName);

				result.Add(row);

				return result;
			}

			protected override ZBool fCheckThatAllTransactionsAreExported
			{
				get { return ZBool.True; }
			}
		}

		#endregion

		#region TestFlatFileAccountingTransactionExporter

		public class TestFlatFileAccountingTransactionExporter : FlatFileAccountingTransactionExporter
		{
			public TestFlatFileAccountingTransactionExporter(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override FlatFileFormat Format
			{
				get { return new CsvFlatFileFormat(false); }
			}

			public FlatFileConverter GetConverter()
			{
				return Converter;
			}

			protected override AccountingFlatFileConverter Converter
			{
				get
				{
					if (fConverter == null)
					{
						fConverter = new MockTransactionsFlatFileConverter(new NotificationBuffer(), Factory);
					}
					return fConverter;
				}
			}

			AccountingFlatFileConverter fConverter;
		}

		class MockTransactionsFlatFileConverter : AccountingFlatFileConverter
		{
			public MockTransactionsFlatFileConverter(INotifications notify, BusinessObjectFactory factory)
				: base(notify, factory)
			{
			}

			protected override FlatFileDataRowCollection MapExport(IValueObject valueObject)
			{
				FlatFileDataRowCollection result = new FlatFileDataRowCollection();

				FlatFileDataRow row = new FlatFileDataRow(1);
				row.SetField(0, valueObject.GetType().FullName);
				result.Add(row);
				IncreaseNumberOfTransactionsExportedFlatFileCount(valueObject);

				return result;
			}

			void IncreaseNumberOfTransactionsExportedFlatFileCount(IValueObject valueObject)
			{
				if (valueObject.GetType() == typeof(Xsd.TxnHeader))
				{
					Xsd.TxnHeader header = (Xsd.TxnHeader)valueObject;

					if (header.TxnType == Xsd.TxnType.INV)
					{
						fNumberOfInvoicesProcessed++;
					}
					else if (header.TxnType == Xsd.TxnType.CRD)
					{
						fNumberOfCreditNotesProcessed++;
					}
					else if (header.TxnType == Xsd.TxnType.ADJ)
					{
						fNumberOfAdjustmentNotesProcessed++;
					}
				}
				else if (valueObject.GetType() == typeof(Xsd.WipOrAccrual))
				{
					Xsd.WipOrAccrual wipAccrual = (Xsd.WipOrAccrual)valueObject;

					if (wipAccrual.LineType == Xsd.WipOrAccrualLineType.REV)
					{
						if (wipAccrual.PostOrReverse == Xsd.WipOrAccrualPostOrReverse.P)
						{
							fNumberOfWipPostingProcessed++;
						}
						else if (wipAccrual.PostOrReverse == Xsd.WipOrAccrualPostOrReverse.R)
						{
							fNumberOfWipReversingProcessed++;
						}
					}
					else if (wipAccrual.LineType == Xsd.WipOrAccrualLineType.CST)
					{
						if (wipAccrual.PostOrReverse == Xsd.WipOrAccrualPostOrReverse.P)
						{
							fNumberOfAccrualPostingProcessed++;
						}
						else if (wipAccrual.PostOrReverse == Xsd.WipOrAccrualPostOrReverse.R)
						{
							fNumberOfAccrualReversingProcessed++;
						}
					}
				}
			}

			protected override void CreateHeader(FlatFileDataRowCollection document)
			{
				FlatFileDataRow row = new FlatFileDataRow(1);
				row.SetField(0, "HEADER");
				document.Add(row);
			}

			protected override void CreateFooter(FlatFileDataRowCollection document)
			{
				FlatFileDataRow row = new FlatFileDataRow(1);
				row.SetField(0, "FOOTER");
				document.Add(row);
			}

			protected override Type DataCollectorType
			{
				get { return typeof(TransferDataCollector); }
			}

			protected override ZBool fCheckThatAllTransactionsAreExported
			{
				get { return ZBool.True; }
			}
		}

		#endregion

		#endregion
	}
}
