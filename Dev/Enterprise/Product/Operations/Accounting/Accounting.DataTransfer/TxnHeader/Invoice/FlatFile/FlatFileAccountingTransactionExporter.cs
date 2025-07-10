using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.Accounting.DataTransfer.Invoices.FlatFile
{
	public abstract class FlatFileAccountingTransactionExporter : AccountingTransactionsDataExporter
	{
		protected FlatFileAccountingTransactionExporter(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected abstract AccountingFlatFileConverter Converter { get; }
		protected abstract FlatFileFormat Format { get; }

		protected override void BeforeDocumentBuild()
		{
			DataConsumer.ResetDataCollector();
			DataConsumer.DataCollector.BOF = true;

			if (NumberOfTransactionsInBatch == 1)
			{
				DataConsumer.DataCollector.EOF = true;
			}
			else
			{
				DataConsumer.DataCollector.EOF = false;
			}
		}

		protected override void ExportObjectsToEndPoint(BusinessObject bizObj, IValueObjectDataAdapter dataAdapter, ZString status)
		{
			if (DataConsumer.DataCollector.UnitsProcessed + 1 == NumberOfTransactionsInBatch)
			{
				DataConsumer.DataCollector.EOF = true;
			}

			BusinessObject bizObjToSerialize = LoadCorrectTypeOfBusinessObject(dataAdapter, bizObj, status);

			IValueObject valueObject = CreateNewValueObject(dataAdapter);
			IncreaseStandardTransactionsProcessedCount(bizObjToSerialize);
			dataAdapter.ExportToValueObject(bizObjToSerialize, valueObject, new ValueObjectExportContext(Notify));
			Convert(Converter, valueObject, Format, DocumentWriter);
			DataConsumer.DataCollector.UnitsProcessed++;
		}

		protected virtual void Convert(FlatFileConverter converter, IValueObject valueObject, FlatFileFormat format, TextWriter documentWriter)
		{
			converter.ExportFlatFile(valueObject, format, documentWriter);
		}

		ITransferDataConsumer DataConsumer
		{
			get { return Converter; }
		}

		protected override void InitialiseDocumentWriter(Stream flatFileStream)
		{
			Document = flatFileStream;
			DocumentWriter = new StreamWriter(flatFileStream);
		}

		protected override void DeInitialiseDocumentWriter()
		{
			DocumentWriter.Flush();
			base.DeInitialiseDocumentWriter();
		}

		protected TextWriter DocumentWriter;

		#region Number of Transactions in Flat File Export

		public ZInt NumberOfInvoicesProcessed_FlatFile
		{
			get { return Converter != null ? Converter.NumberOfInvoicesProcessed : ZInt.Zero; }
		}

		public ZInt NumberOfCreditNotesProcessed_FlatFile
		{
			get { return Converter != null ? Converter.NumberOfCreditNotesProcessed : ZInt.Zero; }
		}

		public ZInt NumberOfAdjustmentNotesProcessed_FlatFile
		{
			get { return Converter != null ? Converter.NumberOfAdjustmentNotesProcessed : ZInt.Zero; }
		}

		public ZInt NumberOfWipPostingProcessed_FlatFile
		{
			get { return Converter != null ? Converter.NumberOfWipPostingProcessed : ZInt.Zero; }
		}

		public ZInt NumberOfWipReversingProcessed_FlatFile
		{
			get { return Converter != null ? Converter.NumberOfWipReversingProcessed : ZInt.Zero; }
		}

		public ZInt NumberOfAccrualPostingProcessed_FlatFile
		{
			get { return Converter != null ? Converter.NumberOfAccrualPostingProcessed : ZInt.Zero; }
		}

		public ZInt NumberOfAccrualReversingProcessed_FlatFile
		{
			get { return Converter != null ? Converter.NumberOfAccrualReversingProcessed : ZInt.Zero; }
		}

		protected override void ResetNumberOfTransactionsExported()
		{
			base.ResetNumberOfTransactionsExported();
			if (Converter != null)
			{
				Converter.ResetCounters();
			}
		}

		protected override void GetMessageToDisplayWhenExportIsFinished_Core(ZStringBuilder errorStringBuilder)
		{
			if (Converter.CheckThatAllTransactionsAreExported)
			{
				ReportDifferences(errorStringBuilder, Res.GetString("90aba865-15c9-41dd-bd18-894f0bcc1841", "Invoices"), NumberOfInvoicesInBatch, NumberOfInvoicesProcessed_Standard, NumberOfInvoicesProcessed_FlatFile);
				ReportDifferences(errorStringBuilder, Res.GetString("b226a827-7407-4aca-ae35-a59b933fd0ae", "Credit Notes"), NumberOfCreditNotesInBatch, NumberOfCreditNotesProcessed_Standard, NumberOfCreditNotesProcessed_FlatFile);
				ReportDifferences(errorStringBuilder, Res.GetString("cd5be494-a199-48e7-875c-6fc14677b781", "Adjustment Notes"), NumberOfAdjustmentNotesInBatch, NumberOfAdjustmentNotesProcessed_Standard, NumberOfAdjustmentNotesProcessed_FlatFile);
				ReportDifferences(errorStringBuilder, Res.GetString("fe3300a6-3f3c-44d4-8487-f09430814803", "WIP Posting"), NumberOfWipPostingsInBatch, NumberOfWipPostingProcessed_Standard, NumberOfWipPostingProcessed_FlatFile);
				ReportDifferences(errorStringBuilder, Res.GetString("44d83ab8-f22f-4a66-a3a2-821cd41ac9ef", "WIP Reversing"), NumberOfWipReversalsInBatch, NumberOfWipReversingProcessed_Standard, NumberOfWipReversingProcessed_FlatFile);
				ReportDifferences(errorStringBuilder, Res.GetString("9fece355-0bda-44ca-be93-6ed658456e1f", "Accrual Posting"), NumberOfAccrualPostingsInBatch, NumberOfAccrualPostingProcessed_Standard, NumberOfAccrualPostingProcessed_FlatFile);
				ReportDifferences(errorStringBuilder, Res.GetString("752ecb87-047b-4e27-8986-427a86ee683c", "Accrual Reversing"), NumberOfAccrualReversalsInBatch, NumberOfAccrualReversingProcessed_Standard, NumberOfAccrualReversingProcessed_FlatFile);
			}
		}

		protected void ReportDifferences(ZStringBuilder errorStringBuilder, ZString description,
			ZInt transactionsInBatch, ZInt transactionsInStandardExport, ZInt transactionsInFlatFileExport)
		{
			ZString result;
			ZBool exportIsSuccessful = IsExportSuccessful(transactionsInBatch, transactionsInStandardExport, transactionsInFlatFileExport);
			result = GetDifferences(exportIsSuccessful, description, transactionsInBatch, transactionsInStandardExport, transactionsInFlatFileExport);
			errorStringBuilder.Append(result);
		}

		protected bool IsExportSuccessful(ZInt transactionsInBatch, ZInt transactionsInStandardExport, ZInt transactionsInFlatFileExport)
		{
			bool baseResult = base.IsExportSuccessful(transactionsInBatch, transactionsInStandardExport);
			return (baseResult && transactionsInBatch == transactionsInFlatFileExport);
		}

		protected ZString GetDifferences(ZBool exportIsSuccessful, ZString description, ZInt transactionsInBatch, ZInt transactionsInStandardExport, ZInt transactionInFlatFileExport)
		{
			ZString result = GetDifferences(exportIsSuccessful, description, transactionsInBatch, transactionsInStandardExport);
			result += " " + Res.GetString("512898c3-ac45-4507-b4c0-2365cac1df0c", "- Exported to Flat File Format: {0}", transactionInFlatFileExport) + "\r\n";
			return result;
		}

		protected override ZBool ErrorHasOccuredCore
		{
			get
			{
				return !(IsExportSuccessful(NumberOfInvoicesInBatch, NumberOfInvoicesProcessed_Standard, NumberOfInvoicesProcessed_FlatFile) &&
				IsExportSuccessful(NumberOfCreditNotesInBatch, NumberOfCreditNotesProcessed_Standard, NumberOfCreditNotesProcessed_FlatFile) &&
				IsExportSuccessful(NumberOfAdjustmentNotesInBatch, NumberOfAdjustmentNotesProcessed_Standard, NumberOfAdjustmentNotesProcessed_FlatFile) &&
				IsExportSuccessful(NumberOfWipPostingsInBatch, NumberOfWipPostingProcessed_Standard, NumberOfWipPostingProcessed_FlatFile) &&
				IsExportSuccessful(NumberOfWipReversalsInBatch, NumberOfWipReversingProcessed_Standard, NumberOfWipReversingProcessed_FlatFile) &&
				IsExportSuccessful(NumberOfAccrualPostingsInBatch, NumberOfAccrualPostingProcessed_Standard, NumberOfAccrualPostingProcessed_FlatFile) &&
				IsExportSuccessful(NumberOfAccrualReversalsInBatch, NumberOfAccrualReversingProcessed_Standard, NumberOfAccrualReversingProcessed_FlatFile));
			}
		}

		#endregion
	}
}
