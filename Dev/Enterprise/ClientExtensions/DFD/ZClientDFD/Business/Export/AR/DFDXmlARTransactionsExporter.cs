using System;
using System.Collections;
using System.IO;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.Client.DFD.Registry;
using Enterprise.DataTransfer.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.DFD.Export
{
	public class DFDXmlARTransactionsExporter : XmlAccountingTransactionExporter
	{
		public DFDXmlARTransactionsExporter(BusinessObjectFactory factory)
			: base(factory)
		{
			TransactionsTypesToExportBusinessObject registryObject = DFDDataRegistry.Instance.ARTransactionsTypesToExport.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

			FilterProvider.IncludeARAdjustmentNotes = registryObject.ARAdjustmentNote;
			FilterProvider.IncludeARCreditNotes = registryObject.ARCreditNote;
			FilterProvider.IncludeARInvoices = registryObject.ARInvoice;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAR = !registryObject.ARNonJobRelated;
			FilterProvider.ExcludeJobRelatedTransactionsForAR = !registryObject.ARJobRelated;

			FilterProvider.CurrentBatchNo = 0;
		}

		const string exportDateTimeFormat = "yyyyMMdd-HH-mm";
		const string postedDateDateTimeFormat = "yyyyMMdd";

		protected override void ExportObjectsToEndPoint(BusinessObject bizObj, IValueObjectDataAdapter dataAdapter, ZString status)
		{
			string fileName;
			AccTransactionHeader transaction = bizObj as AccTransactionHeader;
			OrgHeader debtor = Factory.Load<OrgHeader>(transaction.AH_OH);

			if (transaction.AH_ConsolidatedInvoiceRef != ZString.Empty)
			{
				fileName = string.Format("{0}-{1}-{2}-{3}-{4}-{5}-{6}",
						debtor != null ? debtor.OH_Code : ZString.Empty,
						debtor != null ? debtor.OH_RL_NKClosestPort : ZString.Empty,
						transaction.AH_TransactionType,
						transaction.AH_TransactionNum,
						transaction.AH_ConsolidatedInvoiceRef,
						transaction.AH_PostDate.ToString(postedDateDateTimeFormat),
						ZDateTime.Now.ToString(exportDateTimeFormat));
			}
			else
			{
				fileName = string.Format("{0}-{1}-{2}-{3}-{4}-{5}",
						debtor != null ? debtor.OH_Code : ZString.Empty,
						debtor != null ? debtor.OH_RL_NKClosestPort : ZString.Empty,
						transaction.AH_TransactionType,
						transaction.AH_TransactionNum,
						transaction.AH_PostDate.ToString(postedDateDateTimeFormat),
						ZDateTime.Now.ToString(exportDateTimeFormat));
			}

			FileNames.Add(((FileStream)Document).Name, fileName);
			base.BeforeDocumentBuild();
			base.ExportObjectsToEndPoint(bizObj, dataAdapter, status);
			base.AfterDocumentBuild();
			XmlDocumentWriter.Flush();
			Document.Close();
			tempFileStream = File.Open(Env.GetTempFileName(Env.TempPath + "DFD\\"), FileMode.Open, FileAccess.ReadWrite);
			InitialiseDocumentWriter(tempFileStream);

			bizObj.GetLogs().AddNew(Events.DataExport, DFDConstants.Export.DEXEventReference + (Env.CurrentUser.IsBatchProcessor ? "via ZD3 on " : "") + ZDateTime.Now.ToShortDateString());
			bizObj.Factory.Save();
		}

		FileStream tempFileStream;

		[SuppressWeaklyTypedCollectionMessage]
		public Hashtable FileNames
		{
			get
			{
				if (fileNames == null)
				{
					fileNames = new Hashtable();
				}

				return fileNames;
			}
		}
		Hashtable fileNames;

		protected override void BeforeDocumentBuild()
		{
		}

		protected override void AfterDocumentBuild()
		{
		}

		protected override void DeInitialiseDocumentWriter()
		{
			base.DeInitialiseDocumentWriter();
			File.Delete(((FileStream)Document).Name);
		}

		protected override bool CreateNewBatch()
		{
			return true;
		}

		protected override bool AllowExportWhithoutBatchNumber
		{
			get { return true; }
		}

		public override FinancialInvoiceTransactionExportFilter InvoiceBatchFilter
		{
			get
			{
				if (fInvoiceBatchFilter == null)
				{
					fInvoiceBatchFilter = new DFDFinancialInvoiceTransactionExportFilter(Factory, FilterProvider);
				}
				return fInvoiceBatchFilter;
			}
		}

		DFDFinancialInvoiceTransactionExportFilter fInvoiceBatchFilter;

		protected override ZBool ErrorHasOccuredCore
		{
			get
			{
				return false;
			}
		}

		protected override ZString GetDifferences(ZBool exportIsSuccessful, ZString description,
			ZInt transactionsInBatch, ZInt transactionsInStandardExport)
		{
			ZString result = MessageWhenSuccessfulExport + description + " - " + transactionsInStandardExport + System.Environment.NewLine;
			return result;
		}

		public override bool IsHighWaterMarkEnabled
		{
			get { return false; }
		}
	}
}
