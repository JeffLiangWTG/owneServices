using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Printing
{
	public enum InvoicePrintContext
	{
		PostFromBilling,
		DontCare
	}

	public partial class InvoicePrintTask : IDisposable
	{
		#region Construction

		public class Configuration
		{
			public Configuration(ZGuid pk)
			{
				PK = pk;
			}

			public Configuration(params TransactionHeader[] invoices)
			{
				Invoices = Argument.NotNull(invoices, nameof(invoices));
			}

			public ZGuid PK { get; }
			public TransactionHeader[] Invoices { get; }
			public IJobHeaderParent JobParent { get; set; }
			public InvoicePrintContext Context { get; set; } = InvoicePrintContext.DontCare;
			public bool ShouldCreateeDocs { get; set; } = true;
			public ZString InvoicePrintingOptionCode { get; set; }
			internal bool ShouldForcePrintPostedTransaction { get; set; }
			public bool IsProFormaInvoice { get; set; }
			public bool IsLegacyDocument { get; set; }
			public BusinessObjectFactory Factory { get; set; }
			public INotifications CustomNotifications { get; set; }
			public ZString[] MenuNames { get; set; }
		}

		public InvoicePrintTask(Configuration config)
		{
			ZQuery filter = config.Invoices != null ? FilterForInvoices(config.Invoices) : FilterForPK(config.PK);

			if (config.Invoices != null)
			{
				for (int i = 0; i < config.Invoices.Length; i++)
				{
					Argument.NotNull(config.Invoices[i], FormattableString.Invariant($"{nameof(config.Invoices)}[{i}]"));
				}
			}

			this.isProFormaInvoice = config.IsProFormaInvoice;
			this.InvoicePrintingOptionCode = config.InvoicePrintingOptionCode;
			this.shouldCreateeDocs = config.ShouldCreateeDocs;
			this.isLegacyDocument = config.IsLegacyDocument;
			fMenuNames = config.MenuNames;
			printingContext = config.Context;
			this.jobParent = config.JobParent;
			this.shouldForcePrintPostedTransaction = config.ShouldForcePrintPostedTransaction;

			var transactions = new List<TransactionHeader>();

			if (!filter.IsEmpty)
			{
				invoicesGroupedByOrg = new Dictionary<string, List<InvoicingBase>>();
				this.factory = config.Factory ?? GetNewFactory();
				haveTransactionsBeenUpdatedAsPrinted = false;
				transactions.AddRange(Transactions(filter, config.Invoices?.Select(x => x.PK).ToArray()));
			}

			if (config.Invoices != null && config.Invoices.Any())
			{
				transactions.AddRange(config.Invoices.Where(x => !x.IsInDatabase));
			}

			transactionsToPrint = transactions.ToArray();
			AddInvoices(transactionsToPrint);
			CreateTask(config.Invoices, config.CustomNotifications);
		}

		protected virtual BusinessObjectFactory GetNewFactory()
		{
			return new BusinessObjectFactory();
		}

		readonly bool shouldCreateeDocs;
#if DEBUG
		public
#endif
		bool isProFormaInvoice;
		bool haveTransactionsBeenUpdatedAsPrinted;
		readonly IJobHeaderParent jobParent;
		readonly InvoicePrintContext printingContext;
		readonly TransactionHeader[] transactionsToPrint;
		readonly BusinessObjectFactory factory;
		protected ZString InvoicePrintingOptionCode;
		readonly bool isLegacyDocument;
		readonly bool shouldForcePrintPostedTransaction;

		internal readonly Dictionary<string, List<InvoicingBase>> invoicesGroupedByOrg;

		DocumentPrintSet Task
		{
			get => task_onlyAccessInProperty;
			set
			{
				if (task_onlyAccessInProperty != null)
				{
					ErrorReporter.ReportOnce("InvoicePrintingTaskCreatedOnlyOnce", "To improve performance of invoice printing, the task should be created only once.");
				}
				task_onlyAccessInProperty = value;
			}
		}
		DocumentPrintSet task_onlyAccessInProperty;

		#endregion

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Template Title")]
		static class TemplateTitle
		{
			public const string ExtraContainers = "Extra Containers";
			public const string ARInvoiceExtraContainers = "ARInvoice Extra Containers";
			public const string ExtraPeriodicDetail = "Extra Periodic Detail";
		}

		void CreateTask(TransactionHeader[] invoices, INotifications customNotifications)
		{
			var transaction = transactionsToPrint.FirstOrDefault() ?? invoices?.FirstOrDefault();

			if (transaction != null)
			{
				var invoice = transaction as InvoicingBase;
				if (invoice != null)
				{
					var menuPK = GetMenuPK(invoice);
					var manager = menuPK.IsValid
						? InvoicePrintCommandManager.New(invoice, menuPK)
						: InvoicePrintCommandManager.New(invoice, GetMenuNames(invoice)[0]);

					if (manager != null && invoicesGroupedByOrg != null)
					{
						if (UseNewPrintStreaming)
						{
							Task = new DocumentPrintSetWithStreaming(manager.Command, invoicesGroupedByOrg.Count, GetPacksIEnum(), DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.Value)
							{
								MostTopLevelBusinessObject = JobParent as BusinessObject
							};
							Task.DeliveryInstructionsDefaultPK = GetFixedGuidForDocumentMenu(invoicesGroupedByOrg.First().Value.Any() ? invoicesGroupedByOrg.First().Value[0] : null);
						}
						else
						{
							Task = new DocumentPrintSet(manager.Command, null)
							{
								MostTopLevelBusinessObject = JobParent as BusinessObject,
								CustomNotifications = customNotifications,
								DeliveryInstructionsDefaultPK = GetFixedGuidForDocumentMenu(invoice)
							};

							Task.Clear();
							createdDocumentPacks = new List<DocumentPack>(GetPacksIEnum());
							Task.AddRange(createdDocumentPacks);
						}
					}
				}
				else
				{
					throw new ReportException("Invoice print task can not work with non-invoice transaction. Document not printed.");
				}
			}
		}

		public const string MenuPkForAPInvoice = "94EBB7E3-45CC-46B8-A2A5-B12B68A1191C";
		internal const string MenuPkForProformaInvoice = "0FF26958-0C00-4F13-8823-E7327970871C";
		internal const string MenuPKForARInvoice = "F91EB783-869C-4A26-8181-689B03405CDF";

		ZGuid GetFixedGuidForDocumentMenu(TransactionHeader invoice)
		{
			ZGuid result = ZGuid.Empty;
			if (invoice is APInvoice || invoice is APCreditNote || invoice is APAdjustmentNote)
			{
				result = new ZGuid(MenuPkForAPInvoice);
			}
			else if (invoice is ARInvoice || invoice is ARCreditNote || invoice is ARAdjustmentNote)
			{
				var invoicingBase = ((InvoicingBase)invoice);
				if (invoicingBase.IsPrintingProformaInvoice || isProFormaInvoice)
				{
					result = new ZGuid(MenuPkForProformaInvoice);
				}
				else
				{
					result = new ZGuid(MenuPKForARInvoice);
				}
			}
			else
			{
				ErrorReporter.ReportOnce("GetFixedGuidForDocumentMenu", string.Format(CultureInfo.CurrentCulture, "Cannot get a document PK for the invoice number {0} with invoice type {1} due to its type not being supported.", invoice.AH_TransactionNum.ToString(), invoice.AH_TransactionType.ToString()));
			}
			return result;
		}

		List<DocumentPack> createdDocumentPacks;

		internal List<DocumentPack> GetInvoicePacksForComplianceInvoiceManagerOnly(InvoicingBase invoice)
		{
			// ConplianceInvoiceManager works with single Invoice. So it is safe to get all DocumentPacks if they were already created for non print streaming
			var result = createdDocumentPacks ?? new List<DocumentPack>();
			if (createdDocumentPacks == null)
			{
				AddInvoiceToPack(invoice, result);
			}
			return result;
		}

		IEnumerable<DocumentPack> GetPacksIEnum()
		{
			foreach (var invoicesGrouped in invoicesGroupedByOrg.Values)
			{
				var packs = new List<DocumentPack>();
				foreach (var invoice in invoicesGrouped)
				{
					AddInvoiceToPack(invoice, packs);
				}

				var firstPack = packs.FirstOrDefault();
				if (firstPack != null)
				{
					OrgHeader orgHeader = null;
					var invoice = invoicesGrouped.FirstOrDefault();
					if (invoice != null)
					{
						orgHeader = invoice.Header;
					}
					firstPack.BuildConsolidatedEmailSubject(orgHeader, invoicesGrouped.Count);
				}

				foreach (var pack in packs)
				{
					if (businessObjectForceToLogAgainst != null)
					{
						pack.ForceBusinessObjectToLogAgainst(businessObjectForceToLogAgainst);
					}
					yield return pack;
				}
			}
		}

		bool UseNewPrintStreaming
		{
			get
			{
				return AccountingMasterFilesRegistry.Instance.InvoiceUsePrintStreaming.Value && invoicesGroupedByOrg != null && invoicesGroupedByOrg.Count > PrintTask.MaxPreviewCount
#if DEBUG
 || (ZArchitecture.Environment.Globals.IsTest && TestAboveMaxPreviewCount)
#endif
;
			}
		}

		public void SetOneBusinessObjectToLogAgainstForAllPrintTasks(BusinessObject businessObjectToLogAgainst)
		{
			if (businessObjectToLogAgainst != null && Task != null)
			{
				if (Task is DocumentPrintSetWithStreaming)
				{
					businessObjectForceToLogAgainst = businessObjectToLogAgainst;
				}
				else
				{
					foreach (DocumentPack pack in Task.GetDocumentPacks())
					{
						pack.ForceBusinessObjectToLogAgainst(businessObjectToLogAgainst);
					}
				}
			}
		}
		BusinessObject businessObjectForceToLogAgainst;

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				haveTransactionsBeenUpdatedAsPrinted = true;
			}
		}

		protected static bool CheckAllInvoicesBelongingToSingleOrg(params TransactionHeader[] transactions)
		{
			return transactions.GroupBy(x => x.AH_OH).Count() == 1;
		}

		protected bool isAllInvoicesBelongingToSingleOrg;

		protected virtual void AddInvoices(params TransactionHeader[] transactions)
		{
			isAllInvoicesBelongingToSingleOrg = CheckAllInvoicesBelongingToSingleOrg(transactions);

			foreach (TransactionHeader transaction in transactions)
			{
				InvoicingBase invoice = transaction as InvoicingBase;
				if (invoice != null)
				{
					AddInvoice(invoice);
				}
			}
		}

		public void Run()
		{
			Run(true);
		}

		public void RunDraftInvoiceWithDeliveryOptions(AllowedDeliveryOptions deliveryOptions)
		{
			if (isProFormaInvoice)
			{
				if (TaskCount > 0)
				{
					foreach (var entry in invoicesGroupedByOrg)
					{
						foreach (var invoice in entry.Value)
						{
							if (invoice.IsDeleted)
							{
								throw new InvoiceIsDeletedException("Invoice");
							}
						}
					}
					RunTaskWithCorrectLanguage(deliveryOptions);
				}
			}
			else
			{
				throw new ApplicationException("RunDraftInvoiceWithAllDeliveryOptions should only be used for printing proforma invoice");
			}
		}

		public void Run(bool showDeliveryInstructions)
		{
			if (Task != null && TaskCount > 0)
			{
				if (showDeliveryInstructions)
				{
					var dest = Task.Run(Env.Security.None);

					if (dest != DeliveryInstructionDestination.None &&
						dest != DeliveryInstructionDestination.Preview &&
						dest != DeliveryInstructionDestination.UserCancelled)
					{
						UpdateTransactionsAsPrinted();
					}
				}
				else
				{
					RunTaskWithCorrectLanguage(AllowedDeliveryOptions.PreviewOnly);
				}
			}
		}

		void RunTaskWithCorrectLanguage(AllowedDeliveryOptions deliveryOptions)
		{
			var firstDocPack = GetFirstDocumentPack();
			var instructions = new DeliveryInstructions(firstDocPack);
			if (deliveryOptions == AllowedDeliveryOptions.PreviewOnly && firstDocPack != null)
			{
				instructions.Language = firstDocPack.Language;
			}
			instructions.IsDraft = true;
			Task.RunWithPartialInstructions(deliveryOptions, instructions, Env.Security.None);
		}

		public RunTaskResult RunSilentlyEmailOnly(bool isSaveInChunks = true)
		{
			if (Task == null || !Task.GetDocumentPacks().Any())
			{
				return RunTaskResult.NoDocumentPack;
			}

			var factoryStrategy = isSaveInChunks ? new FactoryStrategy.SaveInChunks() : (FactoryStrategy)new FactoryStrategy.PopulateButDoNotSave(factory);
			var instructions = new DeliveryInstructions(GetFirstDocumentPack(), factoryStrategy);

			var allRecipients = instructions.Recipients.ToArray<DocDeliveryContact>();

			if (allRecipients.Any(x => DeliveryMethodHelper.IsEmailOrEPrint(x.DeliveryMethod)))
			{
				instructions.Recipients.RemoveRange(
					allRecipients.Where(x =>
					{
						var isEmailOrEPrint = DeliveryMethodHelper.IsEmailOrEPrint(x.DeliveryMethod);
						return (isEmailOrEPrint && string.IsNullOrWhiteSpace(x.DeliveryAddress)) || !isEmailOrEPrint;
					}));

				instructions.Destination = DeliveryInstructionDestination.TakenFromContact;
				if (instructions.Recipients.Count > 0)
				{
					Task.Run(instructions);
					return RunTaskResult.Success;
				}
			}

			return RunTaskResult.NoRecipient;
		}

		public enum RunTaskResult
		{
			Success,
			NoRecipient,
			NoDocumentPack
		}

#if DEBUG

		public static bool AttachARInvoiceToEdocs_ForTestOnly(InvoicingBase invoice, INotifications notifications) => AttachARInvoiceToEdocs(invoice, notifications, CancellationToken.None);

#endif

		public static bool AttachARInvoiceToEdocs(InvoicingBase invoice, INotifications notifications, CancellationToken token)
		{
			var canReprint = invoice.CheckCanPrintPostedInvoicingBase();
			if (!canReprint.Result)
			{
				notifications.AddWarning(Res.GetString("{bc59018a-cf55-4ea9-8ce4-a1c9868ee29d}", @"Could not attach {0} {1} {2} to eDocs.
Reason: {3}."
					, invoice.AH_Ledger
					, invoice.AH_TransactionType
					, invoice.AH_TransactionNum
					, canReprint.ReasonForNotBeingAbleToPrint));
				return false;
			}

			using (var printTask = new InvoicePrintTask(new Configuration(invoice.PK) { ShouldCreateeDocs = false }))
			{
				var invoiceFileName = InvoiceFileNameProvider.GetInvoiceFileName(invoice);
				var containerListFileName = InvoiceFileNameProvider.GetContainerListFileName(invoice);
				var periodicInvoiceFileName = InvoiceFileNameProvider.GetPeriodicInvoiceFileName(invoice);

				foreach (var fileNameAndContent in printTask.GetFileNamesAndTheirContents(invoiceFileName, containerListFileName, periodicInvoiceFileName, notifications))
				{
					token.ThrowIfCancellationRequested();
					GenerateAndAttachInvoicePdf(invoice, fileNameAndContent.FileName, fileNameAndContent.Content, fileNameAndContent.DocumentType, fileNameAndContent.EmailSubjectLine, addDDA: true);
					notifications.Add(NotificationSubscriberType.Info, (string.Format(CultureInfo.InvariantCulture, (NoResString)"Successfully attached {0} to EDocs Tab.", fileNameAndContent.FileName)));
				}

				invoice.DocManagerInfo.SetupEDocsFactoryToBeSavedWithMainFactory(false);
			}
			return true;
		}

		public MemoryStream RunToStreamExcelOnly()
		{
			if (shouldCreateeDocs)
			{
				ErrorReporter.ReportOnce("shouldCreateeDocsIsTrueWhenRunToStreamExcelOnly", "shouldCreateeDocs is true when RunToStreamExcelOnly");
			}

			return RunToStreamNotExcelOnly();
		}

		public MemoryStream RunToStreamNotExcelOnly()
		{
			MemoryStream result;

			DeliveryInstructions instructions;
			DirectoryInfo tempOutputDir;
			GetInstructionsAndDirectoryInfo(out instructions, out tempOutputDir);

			try
			{
				Task.Run(instructions);
				result = RunToStreamCore(tempOutputDir);
			}
			finally
			{
				tempOutputDir.Delete(true);
			}

			return result;
		}

		void GetInstructionsAndDirectoryInfo(out DeliveryInstructions instructions, out DirectoryInfo tempOutputDir)
		{
			instructions = new DeliveryInstructions(GetFirstDocumentPack());
			instructions.Destination = DeliveryInstructionDestination.Disk;
			instructions.IsDraft = true;
			Task.PrintTaskUIProviderType = PrintTaskUIProviderTypes.Unattended;
			instructions.OutputDirectory = Env.GetTempFileName();
			File.Delete(instructions.OutputDirectory);
			tempOutputDir = Directory.CreateDirectory(instructions.OutputDirectory);
		}

		protected virtual MemoryStream RunToStreamCore(DirectoryInfo outputDir)
		{
			var files = outputDir.GetFiles();
			if (files.Length > 0)
			{
				return new MemoryStream(File.ReadAllBytes(files.First().FullName));
			}
			else
			{
				throw new FileNotFoundException("File not found.");
			}
		}

		public void RunTaskWithInstructions(DeliveryInstructions instructions)
		{
			if (TaskCount > 0)
			{
				Task.Run(instructions);
			}
		}

		void UpdateTransactionsAsPrinted()
		{
			if (!haveTransactionsBeenUpdatedAsPrinted && (!GlbCompany.CurrentCompany.Country.SupportComplianceSubType || GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.China))
			{
				var factoryForUpdateOfPrintedFlag = GetNewFactory();
				factoryForUpdateOfPrintedFlag.Saved += new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
				try
				{
					var completedSavedTransactions = transactionsToPrint.Where(x => (ZString)x.AH_LedgerInfo.OriginalValue != LedgerTypes.IncompleteTransactions).ToArray();
					var transactionsToUpdateInNewFactory = ReloadUnPrintedTransactionsInNewFactory(completedSavedTransactions, factoryForUpdateOfPrintedFlag);

					if (transactionsToUpdateInNewFactory.Any())
					{
						foreach (var transaction in transactionsToUpdateInNewFactory)
						{
							if (Task.IsDocValidForPrinting(transaction.PK))
							{
								transaction.SetConcurrencyPolicyOnProperties(ConcurrencyPolicy.Ignore);
								transaction.AH_InvoicePrinted = true;
							}
						}
						factoryForUpdateOfPrintedFlag.Save();
					}
				}
				finally
				{
					factoryForUpdateOfPrintedFlag.Saved -= new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
				}
			}
		}

		TransactionHeader[] ReloadUnPrintedTransactionsInNewFactory(TransactionHeader[] transactions, BusinessObjectFactory newFactory)
		{
			var transactionsNotFlaggedAsPrintedPKs = from transaction in transactions.Where(transaction => !transaction.AH_InvoicePrinted) select transaction.PK;
			var transactionsToUpdateInNewFactory = newFactory.Load<TransactionHeader>(new ZQuery(AccTransactionHeaderSchema.PK, transactionsNotFlaggedAsPrintedPKs.ToArray()));
			return transactionsToUpdateInNewFactory.Where(transaction => !transaction.AH_InvoicePrinted).ToArray();
		}

		public int TaskCount
		{
			get { return Task != null ? Task.Count : 0; }
		}

#if DEBUG
		public DocumentPack this[int index]
		{
			get
			{
				// Warning: if task is DocumentPrintSetWithStreaming, we really should not use the indexer as this will sacrifice the benefit of Streaming, through GetDocumentPacks().
				// currently the indexer is mostly used in unit tests.
				if (Task is DocumentPrintSetWithStreaming)
				{
					return Task.GetDocumentPacks().ElementAt(index);
				}
				else
				{
					return Task[index];
				}
			}
		}

		public DocumentPrintSet GetTask()
		{
			return Task;
		}
#endif

		public DocumentPack GetFirstDocumentPack()
		{
			return Task?.GetFirstDocumentPack();
		}

		public IJobHeaderParent JobParent
		{
			get { return jobParent; }
		}

		internal virtual ZGuid GetMenuPK(TransactionHeader header)
		{
			return ZGuid.Empty;
		}

		internal virtual ZString[] GetMenuNames(TransactionHeader header)
		{
			if (fMenuNames == null || fMenuNames.Length == 0 || fMenuNames.Any(fMenuName => string.IsNullOrEmpty(fMenuName)))
			{
				fMenuNames = new ZString[] { header.EnterpriseInvoiceMenuName };
			}
			return fMenuNames;
		}

		ZString[] fMenuNames;

		#region Implementation

		protected InvoicePrintContext PrintingContext
		{
			get { return printingContext; }
		}

		static ZQuery FilterForPK(ZGuid pK)
		{
			return new ZQuery(AccTransactionHeaderSchema.PK, SQLComparisonOperator.Equal, pK);
		}

		public class ParentJobLoader
		{
			public ParentJobLoader(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}
			readonly BusinessObjectFactory factory;

			public BusinessObject Load(InvoicingBase invoice)
			{
				if (invoice.Job != null && invoice.Job.JH_ParentID.IsValid && !invoice.Job.JH_ParentTableCode.IsEmpty)
				{
					var genericJob = invoice.Job.LoadGenericJob<GenericJob.GenericJob>();
					return genericJob?.JobType?.BizoType == null ? null : factory.Load(genericJob.JobType.BizoType, genericJob.PK);
				}

				if (!invoice.AH_ConsolidatedInvoiceRef.IsEmpty && invoice.AH_JH.IsEmpty) // Special Jobless Consol Invoice
				{
					ZString[] splitConsolidatedInvoiceRef = invoice.AH_ConsolidatedInvoiceRef.Split('/');
					if (splitConsolidatedInvoiceRef.Length > 0)
					{
						ZQuery query = new ZQuery();
						query.AddToFilter(JobConsolSchema.JK_IsForwarding, ZBool.True);
						query.AddToFilter(JobConsolSchema.JK_UniqueConsignRef, splitConsolidatedInvoiceRef[0].Trim());
						return factory.LoadTop1<ForwardingConsol>(query);
					}
				}

				var (isAmendingTransactionForPeriodicInvoice, isSingleJob, job) = invoice.CheckIsAmendingTransactionForPeriodicInvoice();

				if (isAmendingTransactionForPeriodicInvoice && isSingleJob && job != null)
				{
					var genericJob = job.LoadGenericJob<GenericJob.GenericJob>();
					return genericJob == null ? null : factory.Load(JobInvoicingConsumerTypes.New()[genericJob.VJ_JobType].BizoType, genericJob.PK);
				}

				return null;
			}
		}

		protected virtual void AddExtraDocumentsToPack(InvoicingBase invoice, DocumentPack pack)
		{
			DocumentCommand entryPrint = null;
			IDocumentSupportable entryPrintBizo = null;
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia && (bool)ObjectFactory.Get<Enterprise.Integration.Customs.AU.IAUCustomsRegistry>().PrintEntryWhenPrintingInvoice.Value && invoice.HasCustomsDisbursementCharge && invoice.Job != null)
			{
				entryPrint = GetEntryPrintCommand(invoice.Job);
				entryPrintBizo = GetEntryPrintDocumentBizo(invoice.Job);
				if (DataStateValidToRunEntryPrint(entryPrintBizo, entryPrint))
				{
					pack.AddReportsToPack(entryPrint, null, entryPrintBizo, null);
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exact Menu Name is need")]
		const string entryMenuName = "Entry Print (Landscape)";

		DocumentCommand GetEntryPrintCommand(JobHeader job)
		{
			if (job != null)
			{
				ZQuery entryPrintFilter = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, entryMenuName);
				if (job.JH_ParentTableCode == JobShipmentSchema.Constants.Prefix)
				{
					entryPrintFilter.AddToFilter(StmMenuItemSchema.SU_BusinessContext, nameof(BusinessContext.Shipment));
				}
				else if (job.JH_ParentTableCode == JobDeclarationSchema.Constants.Prefix)
				{
					entryPrintFilter.AddToFilter(StmMenuItemSchema.SU_BusinessContext, nameof(BusinessContext.Customs));
				}

				entryPrintFilter.AddToFilter(StmMenuItemSchema.SU_FilterList, SQLComparisonOperator.Contains, "AU");
				return job.Factory.LoadTop1<DocumentCommand>(entryPrintFilter);
			}
			return null;
		}

		IDocumentSupportable GetEntryPrintDocumentBizo(JobHeader job)
		{
			if (job != null)
			{
				if (job.JH_ParentTableCode == JobShipmentSchema.Constants.Prefix)
				{
					ForwardingShipment shipment = job.Factory.Load<ForwardingShipment>(job.JH_ParentID);
					if (shipment != null && shipment.DeclarationForDocuments != null)
					{
						return shipment;
					}
				}
				else if (job.JH_ParentTableCode == JobDeclarationSchema.Constants.Prefix)
				{
					return job.Factory.Load<BaseJobDeclaration>(job.JH_ParentID);
				}
			}
			return null;
		}

		bool DataStateValidToRunEntryPrint(IDocumentSupportable documentBizo, StmMenuItem menu)
		{
			if (documentBizo != null && menu != null)
			{
				return documentBizo.DocumentSupporter.GetDataStateBeforeRun(menu).IsValid;
			}
			return false;
		}

		protected void AddInvoice(InvoicingBase invoice)
		{
			if (!shouldForcePrintPostedTransaction)
			{
				if (!invoice.CheckCanPrintPostedInvoicingBase().Result)
				{
					throw new ReprintingInvoiceException(FormattableString.Invariant($@"Reprinting Invoice document is not allowed for some countries and transaction types. It should be prevented before it gets here.
Current company country: {GlbCompany.CurrentCompany.Country.Code}, Transaction country: {invoice.Company?.Country.Code}, Transaction: {invoice.AH_Ledger} {invoice.AH_TransactionType}, Is in database: {invoice.IsInDatabase}. "));
				}
			}

			if (invoice.IsAwaitingApprovalFromGovt)
			{
				invoice.CustomWatermarkText = AccountingConfigurationRegistry.Instance.PrintWatermarkForTransactionAwaitingApproval.Value;
			}

			var invoiceKey = GetInvoiceKey(invoice);
			List<InvoicingBase> invoicesForOrg;

			if (invoicesGroupedByOrg.TryGetValue(invoiceKey, out invoicesForOrg))
			{
				invoicesForOrg.Add(invoice);
			}
			else
			{
				invoicesGroupedByOrg[invoiceKey] = new List<InvoicingBase>(new InvoicingBase[] { invoice });
			}
		}

		string GetInvoiceKey(InvoicingBase invoice)
		{
			return (AccountingConfigurationRegistry.Instance.InvoicePrintingGroupByOrganization.Value
				|| (isAllInvoicesBelongingToSingleOrg && AccountingConfigurationRegistry.Instance.AlwaysGroupInvoicesPrintingForSingleDebtor.Value))
				? invoice.AH_OH.ToStringKey() + invoice.CustomWatermarkText
				: invoice.PK.ToStringKey();
		}

		protected virtual void AddInvoiceToPack(InvoicingBase invoice, List<DocumentPack> packs)
		{
			ZGuid menuPK = GetMenuPK(invoice);
			if (!menuPK.IsValid)
			{
				AddInvoiceToPackCore(invoice, packs, GetMenuNames(invoice));
			}
			else
			{
				AddInvoiceToPackCore(invoice, packs, menuPK);
			}
		}

		protected void AddInvoiceToPackCore(InvoicingBase invoice, List<DocumentPack> packs, params ZString[] nameOfMenus)
		{
			foreach (ZString nameOfMenu in nameOfMenus)
			{
				if (packs.Any())
				{
					var pack = packs.First();
					pack.AddReportsToPack(InvoicePrintCommandManager.New(invoice, nameOfMenu, isLegacyDocument).Command, null, invoice, null);
					AddExtraDocumentsToPack(invoice, pack);
				}
				else
				{
					packs.AddRange(CreatePacks(invoice, nameOfMenu));
				}
			}
		}

		protected void AddInvoiceToPackCore(InvoicingBase invoice, List<DocumentPack> packs, ZGuid menuPK)
		{
			if (packs.Any())
			{
				var pack = packs.First();
				pack.AddReportsToPack(InvoicePrintCommandManager.New(invoice, menuPK, isLegacyDocument).Command, null, invoice, null);
				AddExtraDocumentsToPack(invoice, pack);
			}
			else
			{
				packs.AddRange(CreatePacks(invoice, menuPK));
			}
		}

		List<DocumentPack> CreatePacks(InvoicingBase invoice, ZString menuName)
		{
			InvoicePrintCommandManager manager = InvoicePrintCommandManager.New(invoice, menuName, isLegacyDocument);
			return CreatePacksCore(invoice, manager);
		}

		List<DocumentPack> CreatePacks(InvoicingBase invoice, ZGuid menuPk)
		{
			InvoicePrintCommandManager manager = InvoicePrintCommandManager.New(invoice, menuPk, isLegacyDocument);
			return CreatePacksCore(invoice, manager);
		}

		protected virtual DocumentPack CreateNewDocumentPackForInvoice(DocumentCommand command, InvoicingBase invoice, bool shouldCreateDocs)
		{
			return new DocumentPack(command, invoice, null, null, shouldCreateDocs);
		}

		List<DocumentPack> CreatePacksCore(InvoicingBase invoice, InvoicePrintCommandManager manager)
		{
			List<DocumentPack> result = new List<DocumentPack>();

			invoice.IsPrintingProformaInvoice = isProFormaInvoice;

			DocumentPack pack = CreateNewDocumentPackForInvoice(manager.Command, invoice, shouldCreateeDocs);
			AddExtraDocumentsToPack(invoice, pack);
			result.Add(pack);

			var newTask = manager.GetNewPrintTaskWithChildCommandsLoaded(shouldCreateeDocs);
			pack.Parent = Task;

			foreach (DocumentPack additionalPack in newTask.GetDocumentPacks())
			{
				additionalPack.Parent = Task;
				if (pack.Organisation != null && additionalPack.Organisation != null && additionalPack.Organisation.PK == pack.Organisation.PK)
				{
					pack.AddRange(additionalPack);
				}
				else
				{
					result.Add(additionalPack);
				}
			}

			if (invoice.IsInDatabase)
			{
				pack.ForceBusinessObjectToLogAgainst(invoice);
			}
			else if (invoice.Job != null)
			{
				pack.ForceBusinessObjectToLogAgainst(invoice.Job);
			}
			return result;
		}

		static ZQuery FilterForInvoices(BusinessObject[] invoices)
		{
			Argument.NotNull(invoices, nameof(invoices));
			for (int i = 0; i < invoices.Length; i++)
			{
				Argument.NotNull(invoices[i], FormattableString.Invariant($"{nameof(invoices)}[{i}]"));
			}

			var result = new ZQuery();

			if (invoices.Any())
			{
				var pks = invoices.Select(x => x.PK).ToArray();
				result.AddToFilter(AccTransactionHeaderSchema.PK, pks);
			}

			return result;
		}

		TransactionHeader[] Transactions(ZQuery filter, ZGuid[] invoicesOrder)
		{
			var query = new ZDBOnlyQuery(typeof(TransactionHeader));
			query.AddToFilter(filter, JoinCondition.And);

			if (!shouldForcePrintPostedTransaction)
			{
				query.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			}

			var transactions = new TransactionHeaderCollection(factory, query, true);
			transactions.Load();

			var result = transactions.Cast<TransactionHeader>();

			if (jobParent != null)
			{
				foreach (var transaction in result)
				{
					var job = transaction.Job;
					if (job != null)
					{
						job.Parent = jobParent;
					}
				}
			}

			if (invoicesOrder != null)
			{
				result = result.OrderBy(x => Array.IndexOf(invoicesOrder, x.PK));
			}

			return result.ToArray();
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public List<(string FileName, byte[] Content, string DocumentType, string EmailSubjectLine)> GetFileNamesAndTheirContents(string invoiceFileName, string containerListFileName, string periodicInvoiceFileName, INotifications notifications = null)
		{
			var result = new List<(string FileName, byte[] Content, string DocumentType, string EmailSubjectLine)>();

			DeliveryInstructions instructions;
			DirectoryInfo tempOutputDir;
			GetInstructionsAndDirectoryInfo(out instructions, out tempOutputDir);

			try
			{
				Task.Run(instructions, notifications);
				foreach (var file in tempOutputDir.GetFiles())
				{
					var fileName = invoiceFileName;
					var documentType = Core.Constants.RefDocTypes.Invoice;

					var filePath = file.FullName;
					var deliveryInfo = instructions.GetDeliveryInfoByFilePath(filePath);
					var deliveryInfoName = deliveryInfo?.Name ?? string.Empty;
					var emailSubjectLine = deliveryInfo?.EmailSubjectLine ?? string.Empty;
					if (!string.IsNullOrEmpty(deliveryInfoName))
					{
						if (deliveryInfoName.StartsWith(TemplateTitle.ExtraContainers))
						{
							documentType = Core.Constants.RefDocTypes.ContainerList;
							fileName = containerListFileName;
						}
						else if (deliveryInfoName.StartsWith(TemplateTitle.ARInvoiceExtraContainers))
						{
							fileName = containerListFileName;
						}
						else if (deliveryInfoName.StartsWith(TemplateTitle.ExtraPeriodicDetail))
						{
							fileName = periodicInvoiceFileName;
						}
					}

					var content = File.ReadAllBytes(filePath);
					result.Add((fileName, content, documentType, emailSubjectLine));
				}
			}
			finally
			{
				tempOutputDir.Delete(true);
			}

			return result;
		}

		public static void GenerateAndAttachInvoicePdf(InvoicingBase invoice, string fileName, byte[] content, string documentType, string emailSubjectLine = "", bool addDDA = false)
		{
			var deliveryInfo = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			var pdfBytesAsArray = DocumentConverter.ConvertFromExcel(content, null, OutputFormatType.PDF, deliveryInfo.Watermark, false, 1.0m);
			var eDoc = invoice.DocManagerInfo.AddFileOrDocument(pdfBytesAsArray, string.Format(CultureInfo.InvariantCulture, (NoResString)"{0}.pdf", fileName), documentType);
			invoice.Factory.ServiceContainer.GetService<ResetEDocStatusService>()?.ResetEDocStatus(invoice);

			if (addDDA && eDoc != null)
			{
				var result = eDoc as StorageDocsBase;
				//Reference: DbBackendDocumentFactory.CreateAndAllocateDocument
				invoice.Logs.AddNew(Events.DocumentAllocated, StmALogEventSourceExtensions.GenerateEventReference(documentType, result.PK, new ZString(emailSubjectLine).Left(result.SC_DescInfo.MaxLength)));
			}

			if (eDoc.Description.IsEmpty && documentType == Core.Constants.RefDocTypes.ContainerList)
			{
				eDoc.Description = Res.GetString("0ad2ede5-c9bc-4c47-bdd3-f24e7d75054b", "Container List");
			}

			((StorageDocsBase)eDoc).SC_IsSystemGenerated = true;
		}

		#endregion

		#region Test
#if DEBUG

		[ThreadStatic]
		protected static bool TestAboveMaxPreviewCount;

#endif
		#endregion

		#region Implementation of IDisposable

		public void Dispose()
		{
			if (Task != null)
			{
				Task.Dispose();
			}
		}

		#endregion
	}

	public static class ExtensionMethods
	{
		public static void BuildConsolidatedEmailSubject(this DocumentPack pack, OrgHeader orgHeader, int numberOfDocumentNeedToBeConsolidated)
		{
			if (pack != null)
			{
				pack.NumberOfDocumentNeedToBeConsolidated = numberOfDocumentNeedToBeConsolidated;
				if (pack.NumberOfDocumentNeedToBeConsolidated > 1)
				{
					ZStringBuilder builder = new ZStringBuilder();
					builder.Append(GlbCompany.CurrentCompany.GC_Name);
					builder.Append(GlbBranch.CurrentBranch.GB_BranchName);
					builder.Append(Res.GetString("80e9c733-2854-4bbc-ad8d-b0427ef11bdc", "Multiple Invoices"));
					if (orgHeader != null && !orgHeader.OH_Code.IsEmpty)
					{
						builder.Append(orgHeader.OH_Code);
					}
					pack.EmailSubjectForConsolidateReports = Res.GetString("d9c67ec0-1c8d-4699-a3cf-598baae27771", "{0} - Total {1}", builder.ToStringWithDelimiterBetweenAppends(" - "), pack.NumberOfDocumentNeedToBeConsolidated);
				}
			}
		}
	}
}
