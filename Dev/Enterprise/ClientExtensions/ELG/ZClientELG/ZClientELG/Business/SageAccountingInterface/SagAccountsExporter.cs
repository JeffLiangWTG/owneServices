using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ClientSharedComponents;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.ELG
{
	public abstract class SagAccountsExporter : AccountsExporterARAP
	{
		public SagAccountsExporter(int batchNumber, BusinessObjectFactory factory, NotificationBuffer notifications)
			: base(factory, notifications)
		{
			SetFilterProvider(batchNumber);
		}

		void SetFilterProvider(int batchNumber)
		{
			FilterProvider.IncludeAccrualsPosting = false;
			FilterProvider.IncludeAccrualsReversing = false;
			FilterProvider.IncludeWIPsPosting = false;
			FilterProvider.IncludeWIPsReversing = false;

			FilterProvider.IncludeAPCreditNotes = true;
			FilterProvider.IncludeAPInvoices = true;
			FilterProvider.IncludeAPAdjustmentNotes = false;

			FilterProvider.IncludeARCreditNotes = true;
			FilterProvider.IncludeARInvoices = true;
			FilterProvider.IncludeARAdjustmentNotes = false;

			FilterProvider.ExcludeNonJobRelatedTransactionsForAP = true;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAR = true;

			FilterProvider.CurrentBatchNo = batchNumber;
		}

		#region AccountingExporter Overrides
		protected override void BeforeDocumentBuild()
		{
			base.BeforeDocumentBuild();
			((SagAccountsConverter)Converter).HasHeadingBeenApplied = false;
		}

		protected override FlatFileFormat Format
		{
			get { return format ?? (format = new SagAccountsFlatFileFormat()); }
		}
		SagAccountsFlatFileFormat format;

		protected override void Convert(FlatFileConverter converter, IValueObject valueObject, FlatFileFormat format, TextWriter documentWriter)
		{
			try
			{
				base.Convert(converter, valueObject, format, documentWriter);
			}
			catch (ELGException ex)
			{
				if (ex.Type != ELGExceptionType.NoBranchDepartmentMappingsSetOrFound && ex.Type != ELGExceptionType.NoTransportModeAndChargeCodeMappingsSetOrFound
					&& ex.Type != ELGExceptionType.NoSageAccountCodeMappingsSetOrFound)
				{
					throw new Exception("Unknown issue.  See inner exception.", ex);
				}
			}
		}

		protected override void GetMessageToDisplayWhenExportIsFinished_Core(ZStringBuilder errorStringBuilder)
		{
			ReportDifferences(errorStringBuilder, "Invoices", NumberOfInvoicesInBatch, NumberOfInvoicesProcessed_Standard, NumberOfInvoicesProcessed_FlatFile);
			ReportDifferences(errorStringBuilder, "Credit Notes", NumberOfCreditNotesInBatch, NumberOfCreditNotesProcessed_Standard, NumberOfCreditNotesProcessed_FlatFile);
		}
		#endregion
	}
}
