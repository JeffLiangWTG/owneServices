using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.InvoicePDFExport
{
	[Serializable]
	public class InvoicePDFExportLogSubscriber : LogSubscriber
	{
		#region Override

		public override string Name
		{
			get { return "InvoicePDFExport"; }  // log subscriber name
		}

		public override string[] TableNames
		{
			get { return new string[] { AccTransactionHeaderSchema.Constants.TableName }; }
		}

		public override string[] EventTypes
		{
			get { return new string[] { Events.AddedARecordToTheSystem.Code }; }
		}

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			ProcessQueuedLogs(queuedLogs);
		}

		#endregion

		#region Implementation

		internal void ProcessQueuedLogs(IQueuedLog[] queuedLogs)
		{
			try
			{
				foreach (var queuedLog in queuedLogs)
				{
					var invoice = LoadInvoice(queuedLog);
					if (TryGetDirectory(invoice.Company, out var directory))
					{
						// should export
						string filePath = Path.Combine(directory, GetFilename(invoice));
						ExportInvoicePdf(invoice, filePath);
					}
				}
			}
			finally
			{
				//we cannot clear logs immediately after creation of pdf file. We need to wait until the save process is complete which will be done by the NewsTransmitter
				//Hence we are relying on the subscriber's disposing function to clear logs.
				clearLogDisposableAction = new DisposableAction(() => ClearLogs());
			}
		}

		public void ClearLogs()
		{
			try
			{
				if (Directory.GetFiles(PKLogDirectoryPath).Length > 0)
				{
					DirectoryInfo logDir = new DirectoryInfo(PKLogDirectoryPath);
					foreach (FileInfo file in logDir.GetFiles())
					{
						file.Delete();
					}
					Directory.Delete(PKLogDirectoryPath);
				}
			}
			catch (UnauthorizedAccessException) { }
			catch (IOException) { }
		}

		static bool IsCompanyConfiguredForInvoicePDFExport(GlbCompany company) =>
			SystemDataRegistry.Instance.InvoicesInPDFFormatExportDirectory.Inner.HasActualValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty);

		static InvoicingBase LoadInvoice(IQueuedLog queuedLog)
		{
			if (queuedLog.SJ_ParentTableCode == AccTransactionHeaderSchema.Constants.Prefix)
			{
				var filter = new ZQuery(AccTransactionHeaderSchema.PK, queuedLog.SJ_ParentID);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, new ZString[] { TransactionTypes.Invoice, TransactionTypes.CreditNote });

				InvoicingBase invoice = queuedLog.Factory.LoadTop1<InvoicingBase>(filter);
				return invoice;
			}
			return null;
		}

		internal static bool ShouldExportInvoice(InvoicingBase invoice)
		{
			return invoice != null && (IsExportConsolInvoiceSetInRegistry(invoice.Company) && invoice.IsConsolInvoice || IsExportShipmentInvoiceSetInRegistry(invoice.Company) && IsFreightShipmentJob(invoice.Job));
		}

		static bool IsExportConsolInvoiceSetInRegistry(GlbCompany company)
		{
			return IsExportInvoiceTypeSetInRegistry(company, ExportInvoiceTypesInPDFFormat.ConsolInvoice);
		}

		static bool IsExportShipmentInvoiceSetInRegistry(GlbCompany company)
		{
			return IsExportInvoiceTypeSetInRegistry(company, ExportInvoiceTypesInPDFFormat.ShipmentInvoice);
		}

		static bool IsExportInvoiceTypeSetInRegistry(GlbCompany company, string exportInvoiceType)
		{
			foreach (ICodeDescriptionBool registryValue in SystemDataRegistry.Instance.InvoiceTypesToExportInPDFFormat.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty))
			{
				if (registryValue.Code == exportInvoiceType)
				{
					return registryValue.Bool;
				}
			}

			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service task logs are English-only.")]
		internal bool TryGetDirectory(GlbCompany company, out string directory)
		{
			bool hasActualValue = SystemDataRegistry.Instance.InvoicesInPDFFormatExportDirectory.Inner.HasActualValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty);

			bool result = hasActualValue;

			directory = SystemDataRegistry.Instance.InvoicesInPDFFormatExportDirectory.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty);

			if (hasActualValue && !Directory.Exists(directory))
			{
				string errorMessage = string.Format(CultureInfo.InvariantCulture, "No valid Export Directory is set for company '{0}'. Please check Registry: System -> Data Export -> Invoices in PDF Format.", GlbCompany.CurrentCompany.GC_Code);
				DefaultLogger.Log(LogType.Error, errorMessage);
				result = false;
			}

			return result;
		}

		internal string GetFilename(InvoicingBase invoice)
		{
			StringBuilder filename = new StringBuilder();
			filename.Append(GetFixedLengthString(invoice.Branch != null ? invoice.Branch.GB_Code : ZString.Empty, Constants.FilenameFieldLength.Branch));
			filename.Append(GetFixedLengthString(invoice.Department != null ? invoice.Department.GE_Code : ZString.Empty, Constants.FilenameFieldLength.Department));
			filename.Append(GetFixedLengthString(invoice.Header != null ? invoice.Header.OH_Code : ZString.Empty, Constants.FilenameFieldLength.DebtorCode));
			filename.Append(GetFixedLengthString(invoice.InvoiceNumber.ToString(), Constants.FilenameFieldLength.InvoiceNumber));
			filename.Append(GetFixedLengthString(invoice.Job != null ? invoice.Job.JH_JobNum : ZString.Empty, Constants.FilenameFieldLength.JobNumber));
			IMatching matching = invoice;
			filename.Append(GetFixedLengthString(matching != null ? matching.ShipmentMasterBill : ZString.Empty, Constants.FilenameFieldLength.MAWB));
			filename.Append(GetFixedLengthString(matching != null ? matching.ShipmentHouseBill : ZString.Empty, Constants.FilenameFieldLength.HouseBillNumber));
			filename.Append(GetFixedLengthString(invoice.AH_InvoiceDate.ToString("yyyyMMdd"), Constants.FilenameFieldLength.InvoiceDate));
			return filename + ".pdf";
		}

		void ExportInvoicePdf(InvoicingBase invoice, string filePath)
		{
			try
			{
				var factory = invoice.Factory.CreateNewFactory();
				factory.SetContext(BusinessContext.SuspendInvoiceCopies);

				using (ExcelInterface xlInterface = new ExcelInterface())
				{
					xlInterface.LoadExcelFile(new InvoicePrintTask(new InvoicePrintTask.Configuration(invoice) { Factory = factory, ShouldCreateeDocs = false }).RunToStreamExcelOnly());
					xlInterface.ExportToPdfAndScale(filePath, 100, null);
					invoice.Logs.AddNew(Events.DataExport, Name);
				}
			}
			catch (Exception ex) when (ex is ReprintingInvoiceException)
			{
				DefaultLogger.Log(LogType.Error, AccountingConstants.ReprintingInvoiceMessage);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				DefaultLogger.Log(LogType.Error, ex.ToString());
			}
		}

		string GetFixedLengthString(string input, int length)
		{
			if (string.IsNullOrEmpty(input))
			{
				return string.Empty.PadRight(length, ' ');
			}
			else
			{
				var safeInput = MakeFilenameSafe.MakeSafe(input, '_');
				return safeInput.Substring(0, Math.Min(length, safeInput.Length)).PadRight(length, ' ');
			}
		}

		static bool IsFreightShipmentJob(JobHeader job)
		{
			bool result = false;

			if (job != null && job.JH_ParentTableCode == JobShipmentSchema.Constants.Prefix)
			{
				CommonShipment shipment = job.Factory.Load<CommonShipment>(job.JH_ParentID);
				result = shipment != null && shipment.JS_IsForwardRegistered;
			}

			return result;
		}

		internal static string PKLogDirectoryPath
		{
			get { return Path.Combine(ServiceManagerHelper.GetLogFilesDirectory(Db.ServerName, Db.DatabaseName), Res.GetString("0cac5113-b45c-46ae-a45c-54dfc938f4f4", "Invoice PDF Export")); }
		}

		protected override ILogBatcher GetLogBatcher() => new InvoicePDFExportLogBatcher();

		protected override void Dispose(bool disposing)
		{
			clearLogDisposableAction?.Dispose();
			clearLogDisposableAction = null;
			base.Dispose(disposing);
		}

		[NonSerialized]
		DisposableAction clearLogDisposableAction;

		#endregion

		class InvoicePDFExportLogBatcher : LogBatcher<ZGuid>
		{
			protected override void AddGroupingFetchHints(IEnumerable<IQueuedLog> enumberable)
			{
			}

			protected override ZGuid GetGroupLogKey(IQueuedLog log) => log.SJ_ParentID;

			protected override LogsGroupContext SetContextForLogsGroup(ZGuid groupKey, IEnumerable<IQueuedLog> queuedLogs)
			{
				var invoice = LoadInvoice(queuedLogs.First());
				if (invoice != null && CanInvoiceBeExportedToPdf(invoice) && invoice.Company.FirstActiveBranch != null)
				{
					return new LogsGroupContext(skipGroup: false, DisposableEnvironment.ForBranch(invoice.Company.FirstActiveBranch.PK.ToGuid()));
				}
				else
				{
					return new LogsGroupContext(skipGroup: true);
				}
			}

			static bool CanInvoiceBeExportedToPdf(InvoicingBase invoice)
			{
				return invoice != null &&
					IsCompanyConfiguredForInvoicePDFExport(invoice.Company) &&
					ShouldExportInvoice(invoice);
			}
		}
	}
}
