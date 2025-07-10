using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class ARTransactionToAPTransactionConverterBase
	{
		public ARTransactionToAPTransactionConverterBase(NotificationBuffer notificationBuffer)
		{
			Notify = notificationBuffer;
		}

		protected readonly NotificationBuffer Notify;
#if DEBUG
		internal Action<InvoicingBase> sourceTransactionInNewFactoryAction_ForTestOnly;
		internal string contextofCompanyForStoredARInvoice_ForTestOnly = GlbCompany.CurrentCompany.GC_Code;
#endif

		IValueObjectDataAdapter FinancialInvoiceDataAdapter
		{
			get
			{
				if (financialInvoiceDataAdapter == null)
				{
					financialInvoiceDataAdapter = (IValueObjectDataAdapter)ObjectFactory.Get(DataAdapterType);
					((IFinancialInvoiceDataAdapter)financialInvoiceDataAdapter).RunExtraValidation = false;
				}
				return financialInvoiceDataAdapter;
			}
		}

		IValueObjectDataAdapter financialInvoiceDataAdapter;

		internal (InvoicingBase convertedAPTransaction, ZString approvalEventReference) ConvertToAPTransactionFromARTransaction(InvoicingBase transaction, BusinessObjectFactory factoryForConvertedObject, bool releaseJobHeaderMutex, bool revalidateLines, bool generateInvoicePdf, bool isAutoImport)
		{
			var apInvoicingBase = ConvertToAPUnsafeFromARInvoiceOrCreditNote(transaction, factoryForConvertedObject);
			var consol = transaction.Consol;
			var isConsolInvoice = transaction.IsConsolInvoice;
			var approvalEventReference = (NoResString)"Intercompany Invoice Approved";
			var xmlInvoiceHeader = GetXMLInvoiceHeader(transaction);
			var context = GetContext(factoryForConvertedObject, transaction);
			var jobHeaders = new HashSet<Job>();
			var success = false;
			try
			{
				SetBusinessContexts(apInvoicingBase, isAutoImport);
				AddJobsFromARTransaction(jobHeaders, transaction, factoryForConvertedObject);
				SetOrgProxyOnAPTransaction(transaction, apInvoicingBase);
				AdjustXmlInvoiceLineValuesBeforeImport(consol, xmlInvoiceHeader, transaction);

				apInvoicingBase.IsConvertedFromARInvoice = true;
				FinancialInvoiceDataAdapter.ImportFromValueObject(apInvoicingBase, xmlInvoiceHeader, context);

				apInvoicingBase.SubmittedFromInvoicingForm = true;
				if (ShouldUseComplianceNumber(transaction))
				{
					apInvoicingBase.AH_TransactionNum = transaction.AH_TransactionReference;
					apInvoicingBase.AH_ChequeOrReference = transaction.AH_TransactionNum;
				}

				var invoiceCurrencyNK = apInvoicingBase.ExchangeRate.Currency;
				apInvoicingBase.AH_TransactionCategory = Core.Constants.TransactionCategory.Codes.Standard;
				apInvoicingBase.ExchangeRate.Currency = invoiceCurrencyNK;
				apInvoicingBase.AH_TransactionReference = ZString.Empty;

				transaction.AH_PostedInternal = true;
				transaction.HasChanges = true;

				var (invoicingPlugIn, rateConsumer) = GetInvoicingPluginAndRateConsumer(transaction, apInvoicingBase);
				apInvoicingBase.AH_PostDate = GetPostDate(invoicingPlugIn, apInvoicingBase.AH_InvoiceDate);
				apInvoicingBase.InvoicingValidation?.ValidateAH_InvoiceDate();
				if (isConsolInvoice || !apInvoicingBase.UseJobExchangeRate)
				{
					if (rateConsumer != null)
					{
						apInvoicingBase.AH_ExchangeRate = apInvoicingBase.GetExchangeRateFromInvoicingPlugIn(rateConsumer);
					}
				}

				SetAPCreditNoteOriginalTransactionReference(transaction, apInvoicingBase);
				apInvoicingBase.AH_DueDate = xmlInvoiceHeader.DueDate; //AH_DueDate must be set after call to SetAPCreditNoteOriginalTransactionReference()
				CreateConsolCosts(factoryForConvertedObject, revalidateLines, apInvoicingBase, consol, jobHeaders);

				foreach (InvoicingLineBase apLine in apInvoicingBase.Lines)
				{
					CheckLineTaxAmount(apLine);
				}
				success = true;
			}
			finally
			{
				apInvoicingBase.Factory.RemoveContext(BusinessContext.InterCompanyInvoiceImport);
				apInvoicingBase.Factory.RemoveContext(BusinessContext.IntercompanyInvoiceAutoImport);
				if (releaseJobHeaderMutex || !success)
				{
					foreach (Job jobHeader in jobHeaders)
					{
						jobHeader.Dispose();
					}

					apInvoicingBase.ReleaseAllMutexOnInvoice();
				}
			}

			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnAPInvoice(transaction, apInvoicingBase, isAutoImport);

			GenerateAndAttachInvoicePdfIfApplicable(transaction, apInvoicingBase, generateInvoicePdf);

			if (isAutoImport)
			{
				if (Notify?.HasErrors == true)
				{
					var errors = Notify.GetEventsByType(ErrorType.Error);
					foreach (var error in errors)
					{
						apInvoicingBase.AddRowError(error.Message);
					}
				}

				var complianceErrors = apInvoicingBase.AssignComplianceSubTypeAndCheckComplianceErrors();
				if (!complianceErrors.IsEmpty)
				{
					apInvoicingBase.AddRowError(complianceErrors);
				}
			}

			return (apInvoicingBase, approvalEventReference);
		}

		protected void CalculateTaxDateForInvoiceLinesBasedOnRegistryFCN(ZDate invoiceDate, IEnumerable<InvoicingLineBase> invoiceLines, IJobCostingPlugIn consol)
		{
			var costSupporter = consol?.CostSupporter;
			if (costSupporter != null)
			{
				var taxDateOption = AccountingUtils.GetTaxDateDefaultingOptionForCostSupporter(costSupporter, LedgerTypes.AccountsPayable, JobInvoicingConsumerTypes.ForwardingConsolCode);
				if (taxDateOption != null)
				{
					foreach (InvoicingLineBase line in invoiceLines)
					{
						if (line.AL_AT.IsValid)
						{
							if (taxDateOption.TaxDateOption == TaxDateDefaultingOption.Code.InvoiceDate)
							{
								line.AL_TaxDate = invoiceDate;
							}
							else if (taxDateOption.TaxDateOption == TaxDateDefaultingOption.Code.Today)
							{
								line.AL_TaxDate = ZDate.Today;
							}
						}
					}
				}
			}
		}

		#region Virtual Methods

		protected virtual string DataAdapterType => "IntercompanyTransactionFinancialInvoiceDataAdapter";

		protected virtual void SetBusinessContexts(InvoicingBase apInvoicingBase, bool isAutoImport)
		{
			apInvoicingBase.Factory.SetContext(BusinessContext.InterCompanyInvoiceImport);
			if (isAutoImport)
			{
				apInvoicingBase.Factory.SetContext(BusinessContext.IntercompanyInvoiceAutoImport);
			}
		}

		protected virtual void AdjustXmlInvoiceLineValuesBeforeImport(IJobCostingPlugIn consol, TxnHeader xmlInvoiceHeader, InvoicingBase transaction)
		{
		}

		protected virtual bool ShouldCreateConsolCostsForConsol(IJobCostingPlugIn consol) => false;

		protected virtual (IJobCostingPlugIn consol, IEnumerable<InvoicingLineBase> invoiceLines)[] GetConsolsAndRelatedLines(BusinessObjectFactory factory, IJobCostingPlugIn consol, InvoicingBase apInvoicingBase)
		{
			CalculateTaxDateForInvoiceLinesBasedOnRegistryFCN(apInvoicingBase.InvoiceDate.Date, apInvoicingBase.Lines.Cast<InvoicingLineBase>(), consol);
			return new[] { (consol, apInvoicingBase.Lines.Cast<InvoicingLineBase>()) };
		}

		#endregion

		#region Private Methods

		void CreateConsolCosts(BusinessObjectFactory factoryForConvertedObject, bool revalidateLines, InvoicingBase apInvoicingBase, IJobCostingPlugIn consol, HashSet<Job> jobHeaders)
		{
			if (ShouldCreateConsolCostsForConsol(consol))
			{
				InvoiceLinesToConsolCostConvertor.ConvertToConsolCostRelatedLines(factoryForConvertedObject, apInvoicingBase, true, GetConsolsAndRelatedLines(factoryForConvertedObject, consol, apInvoicingBase), new TaxOverriddenLinesInfoProvider(), jobHeaders);

				if (revalidateLines && !apInvoicingBase.IsValidationSuspended)
				{
					apInvoicingBase.Lines.RunPreSaveValidation();
				}
			}
		}

		InvoicingBase ConvertToAPUnsafeFromARInvoiceOrCreditNote(InvoicingBase transaction, BusinessObjectFactory factoryForConvertedObject)
		{
			InvoicingBase invoicingBase;
			if (transaction != null && transaction is ARInvoice)
			{
				invoicingBase = factoryForConvertedObject.New<APInvoice>();
				invoicingBase.ShowError = transaction.ShowError;
				return invoicingBase;
			}
			else if (transaction != null && transaction is ARCreditNote)
			{
				invoicingBase = factoryForConvertedObject.New<APCreditNote>();
				invoicingBase.ShowError = transaction.ShowError;
				return invoicingBase;
			}
			else
			{
				throw new NotSupportedException("Only AR Invoices and AR Credit Notes are supported.");
			}
		}

		TxnHeader GetXMLInvoiceHeader(InvoicingBase transactionToConvertToXMLTxnHeader)
		{
			var transaction = new BusinessObjectFactory() { NameForDebugging = "GetXMLInvoiceHeaderFactory" }.Load<InvoicingBase>(transactionToConvertToXMLTxnHeader.PK);
			CreateCopyWithJobDetailsIfNecessary();
			TxnHeader result = null;
			if (transaction != null)
			{
				using (new TemporaryUserContext() { BranchPK = transaction.AH_GB.ToGuid(), DepartmentPK = transaction.AH_GE.ToGuid() }.Set())
				{
					transaction.Factory.SetContext(BusinessContext.InterCompanyInvoiceExport);
					try
					{
						result = (TxnHeader)FinancialInvoiceDataAdapter.ExportToValueObject(transaction, new ValueObjectExportContext(Notify));
					}
					finally
					{
						transaction.Factory.RemoveContext(BusinessContext.InterCompanyInvoiceExport);
					}
				}
			}
			return result;

			void CreateCopyWithJobDetailsIfNecessary()
			{
				if (transaction.IsConsolInvoice && transaction.Lines.Cast<InvoicingLineBase>().Any(c => !c.AL_JH.IsValid))
				{
					ZQuery query = new ZQuery(JobConsolCostSchema.E6_AH_ARInvoice, transaction.PK);
					query.AddToFilter(JobConsolCostSchema.E6_IsForCollectInvoice, ZBool.True);
					query.AddToFilter(JobConsolCostSchema.E6_AH_APInvoice, SQLComparisonOperator.NotEqual, DBNull.Value);
					JobConsolCost[] jobConsolCosts = transaction.Factory.Load<JobConsolCost>(query);

					if (jobConsolCosts.Length > 0)
					{
#if DEBUG
						if (Globals.IsTest && sourceTransactionInNewFactoryAction_ForTestOnly != null)
						{
							sourceTransactionInNewFactoryAction_ForTestOnly(transaction);
						}
#endif
						/*
						When we are copying amount from AP invoice line to AR invoice line, we need to invert the signs, 
						because default sign of AP invoice line is negative and AR invoice line has InvertSigns as false
						so AR Invoice line gets negative value
						But when we are copying amount from AP credit note line to AR credit note line, we do not need to invert 
						since default sign of AP credit note is positive and AR credit note has InvertSigns as true so it inverts the amount as necessary
						*/
						var multiplier = transaction is ARInvoice ? -1 : 1;

						using (new TemporaryUserContext() { BranchPK = transaction.AH_GB.ToGuid(), DepartmentPK = transaction.AH_GE.ToGuid() }.Set())
						using (transaction.Lines.SuspendListChanged())
						{
							var listRemoveLines = transaction.Lines.Cast<InvoicingLineBase>().Where(c => !c.AL_JH.IsValid).ToArray();
							foreach (InvoicingLineBase removeLine in listRemoveLines)
							{
								transaction.Lines.RemoveAndDelete(removeLine);
							}

							var apInvoicePKs = jobConsolCosts.Select(x => x.E6_AH_APInvoice).Distinct().ToArray();
							var originalAPInvoices = transaction.Factory.Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.PK, apInvoicePKs));
							foreach (var originalAPInvoice in originalAPInvoices)
							{
								foreach (InvoicingLineBase invoicingLineBase in originalAPInvoice.Lines.Cast<InvoicingLineBase>().Where(c => c.AL_JH.IsValid))
								{
									var newARLine = (InvoicingLineBase)transaction.Lines.AddNew();
									newARLine.CopyValuesFrom(invoicingLineBase);
									newARLine.AL_ExchangeRate = transaction.AH_ExchangeRate;
									newARLine.AL_LocalExTaxAmount = invoicingLineBase.AL_LocalExTaxAmount * multiplier;
								}
							}
						}
					}
				}
			}
		}

		ValueObjectImportContext GetContext(BusinessObjectFactory factoryForConvertedObject, InvoicingBase transaction)
		{
			ValueObjectImportContext context = null;
			if (factoryForConvertedObject != null && transaction != null)
			{
				context = new ValueObjectImportContext(factoryForConvertedObject, Notify);
				((XmlInterchange)context.Interchange).InterchangeInfo.Source.EnterpriseCode = ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode;
				((XmlInterchange)context.Interchange).InterchangeInfo.Source.CompanyCode = transaction.Company.GC_Code;
			}
			return context;
		}

		void GenerateAndAttachInvoicePdfIfApplicable(InvoicingBase transaction, InvoicingBase apInvoicingBase, bool generateInvoicePdf)
		{
			if (apInvoicingBase != null && apInvoicingBase.AH_Ledger != LedgerTypes.AccountsPayable)
			{
				throw new ArgumentException("apInvoicingBase must be not null and AP", nameof(apInvoicingBase));
			}

			if (generateInvoicePdf && transaction.IsARInvoiceOrCreditNote && transaction.AH_GC != GlbCompany.CurrentCompany.PK && AccountingConfigurationRegistry.Instance.PayableAllowUserToStoreARDoc.Value)
			{
				using (new TemporaryUserContext() { BranchPK = transaction.AH_GB.ToGuid(), DepartmentPK = transaction.AH_GE.ToGuid() }.Set())
				{
					try
					{
						var printTask = new InvoicePrintTask(new InvoicePrintTask.Configuration(transaction) { ShouldForcePrintPostedTransaction = true, ShouldCreateeDocs = false });

						var invoiceFileName = string.Format((NoResString)"Invoice {0}", transaction.AH_TransactionNum);
						var containerListFileName = string.Format((NoResString)"Invoice {0} Container Details", transaction.AH_TransactionNum);
						var periodicInvoiceFileName = string.Format((NoResString)"Invoice {0} Periodic Details", transaction.AH_TransactionNum);

						var fileNamesAndTheirContents = printTask.GetFileNamesAndTheirContents(invoiceFileName, containerListFileName, periodicInvoiceFileName);

#if DEBUG
						if (Globals.IsTest && InvoicingBase.ShouldClearAttachedFileContent_ForTestOnly)
						{
							fileNamesAndTheirContents.Clear();
						}
#endif

						if (fileNamesAndTheirContents.Any())
						{
							foreach (var fileNameAndTheirContent in fileNamesAndTheirContents)
							{
								InvoicePrintTask.GenerateAndAttachInvoicePdf(apInvoicingBase, fileNameAndTheirContent.FileName, fileNameAndTheirContent.Content, fileNameAndTheirContent.DocumentType);
							}
						}
						else
						{
							throw new FileNotFoundException("File not found.");
						}

						apInvoicingBase.DocManagerInfo.SetupEDocsFactoryToBeSavedWithMainFactory(false);
					}
					catch (FileNotFoundException)
					{
						var caption = Res.GetString("C9490B5B-9566-4A61-ABA7-4973DB419087", "File not found");
						ShowErrorMessageForGenerateAndAttachInvoicePdf(apInvoicingBase, caption);
					}
					catch (UnableToFindInvoiceDocumentCommandException ex)
					{
						var caption = Res.GetString("1AFD64CB-0B77-44CD-B3F1-9A429847F010", "Invoice Document Command not found");
						ShowErrorMessageForGenerateAndAttachInvoicePdf(apInvoicingBase, caption, ex.Message);
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						ErrorReporter.ReportOnce("Error when generating Invoice PDF attachment from InvoicingBase", e);
					}

#if DEBUG
					if (Globals.IsTest)
					{
						contextofCompanyForStoredARInvoice_ForTestOnly = GlbCompany.CurrentCompany.GC_Code;
					}
#endif
				}
			}
		}

		void ShowErrorMessageForGenerateAndAttachInvoicePdf(InvoicingBase invoice, string caption, string additionalMessage = "")
		{
			var message = Res.GetString("86A7936B-742A-44CE-8A57-FA85206D3AFC", @"Could not generate AR Invoice to attach to eDocs.
{0}
Please check if the registry: '{1}' is overridden.
* If it is overridden, please check if the registry has a valid value and if the menu path is valid.
* If it is not overridden or the registry value is valid, please contact support.",
				additionalMessage,
				AccountingConfigurationRegistry.Instance.ARInvoiceMenuItem.Location());
			invoice.RaiseShowError(message, caption);
		}

		protected virtual void AddJobsFromARTransaction(HashSet<Job> jobHeaders, InvoicingBase transaction, BusinessObjectFactory factoryForConvertedObject)
		{
			if (jobHeaders != null && transaction != null && factoryForConvertedObject != null && transaction.IsConsolInvoice)
			{
				foreach (InvoicingLineBase line in transaction.Lines)
				{
					if (line.AL_JH.IsValid)
					{
						Job shipmentJob = null;
						var jobCreationErrorMessage = string.Empty;
						if (line.GenericJobObject != null && line.GenericJobObject.Consumer != null)
						{
							var loader = new Job.Loader(factoryForConvertedObject, line.GenericJobObject.Consumer);
							shipmentJob = loader.TryLoadOrCreateWithMutex();
							if (shipmentJob != null)
							{
								jobHeaders.Add(shipmentJob);
							}
							else
							{
								jobCreationErrorMessage = loader.GetJobCreationError();
							}
						}
						if (shipmentJob == null)
						{
							throw new JobCreationException("Unable to load or create JobHeader for transaction line.\r\n" + jobCreationErrorMessage);
						}
					}
				}
			}
		}

		void SetOrgProxyOnAPTransaction(InvoicingBase transaction, InvoicingBase apInvoicingBase)
		{
			if (transaction != null && transaction.AH_Ledger != LedgerTypes.AccountsReceivable)
			{
				throw new ArgumentException("transaction must be not null and AR", nameof(transaction));
			}
			if (apInvoicingBase != null && apInvoicingBase.AH_Ledger != LedgerTypes.AccountsPayable)
			{
				throw new ArgumentException("apInvoicingBase must be not null and AP", nameof(apInvoicingBase));
			}

			var useCompanyProxy = transaction.Branch.OrgProxy == null ||
				(!transaction.Branch.OrgProxy.OH_IsCreditor && transaction.Company.OrgProxy.OH_IsCreditor);
			apInvoicingBase.AH_OH = (useCompanyProxy ? transaction.Company.OrgProxy : transaction.Branch.OrgProxy).PK;
		}

		void SetAPCreditNoteOriginalTransactionReference(InvoicingBase arCrd, InvoicingBase apCrd)
		{
			if (apCrd is APCreditNote && arCrd is ARCreditNote)
			{
				var arInvoice = arCrd.Factory.Load<ARInvoice>(arCrd.OriginalTransactionReference);
				var apInvoiceQuery = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
				apInvoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);
				apInvoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
				apInvoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_OH, apCrd.AH_OH);

				if (arInvoice != null)
				{
					apInvoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_InvoiceDate, SQLComparisonOperator.EqualToDatePartOnly, arInvoice.AH_InvoiceDate);

					if (ShouldUseComplianceNumber(arInvoice))
					{
						apInvoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_ChequeOrReference, arInvoice.AH_TransactionNum);
						apInvoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, arInvoice.AH_TransactionReference);
					}
					else
					{
						apInvoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, arInvoice.AH_TransactionNum);
					}

					var apInvoiceCollection = apCrd.Factory.Load<APInvoice>(apInvoiceQuery);

					if (apInvoiceCollection.Length == 1)
					{
						var apInvoice = apInvoiceCollection[0];

						using (apCrd.GetSetGSTOnLinesForIntercompanyImportSuspender())
						{
							apCrd.OriginalTransactionReference = apInvoice.PK;
						}

						if (arCrd.ExchangeRate.Currency != apInvoice.ExchangeRate.Currency)
						{
							apCrd.ExchangeRate.Currency = arCrd.ExchangeRate.Currency;
						}
					}
					else
					{
						apCrd.AH_OriginalInvoiceDate = arCrd.AH_OriginalInvoiceDate.IsEmpty ? new ZDate(arInvoice.AH_InvoiceDate) : arCrd.AH_OriginalInvoiceDate;
						apCrd.AH_OriginalTransactionNum = ShouldUseComplianceNumber(arInvoice) ? arInvoice.AH_TransactionReference : arCrd.AH_OriginalTransactionNum;
					}
				}
				else if (!arCrd.AH_OriginalTransactionNum.IsEmpty && !arCrd.AH_OriginalInvoiceDate.IsEmpty)
				{
					apInvoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_InvoiceDate, SQLComparisonOperator.EqualToDatePartOnly, arCrd.AH_OriginalInvoiceDate);
					apInvoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, arCrd.AH_OriginalTransactionNum);

					var apInvoiceCollection = apCrd.Factory.Load<APInvoice>(apInvoiceQuery);

					if (apInvoiceCollection.Length == 1)
					{
						var apInvoice = apInvoiceCollection[0];
						using (apCrd.GetSetGSTOnLinesForIntercompanyImportSuspender())
						{
							apCrd.OriginalTransactionReference = apInvoice.PK;
						}

						if (arCrd.ExchangeRate.Currency != apInvoice.ExchangeRate.Currency)
						{
							apCrd.ExchangeRate.Currency = arCrd.ExchangeRate.Currency;
						}
					}
					else
					{
						apCrd.AH_OriginalInvoiceDate = new ZDate(arCrd.AH_OriginalInvoiceDate);
						apCrd.AH_OriginalTransactionNum = arCrd.AH_OriginalTransactionNum;
					}
				}
			}
		}

		bool ShouldUseComplianceNumber(InvoicingBase transaction)
		{
			return !transaction.AH_TransactionReference.IsEmpty && AccountingConfigurationRegistry.Instance.ImportSisterCompanyInvoiceComplianceNumberAsInvoiceNumber.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
		}

		(IJobInvoicingPlugIn invoicingPlugIn, IAccExchangeRateConfigurationRateConsumer rateConsumer) GetInvoicingPluginAndRateConsumer(InvoicingBase transaction, InvoicingBase apInvoicingBase)
		{
			if (transaction != null && transaction.AH_Ledger != LedgerTypes.AccountsReceivable)
			{
				throw new ArgumentException("transaction must be not null and AR", nameof(transaction));
			}
			if (apInvoicingBase != null && apInvoicingBase.AH_Ledger != LedgerTypes.AccountsPayable)
			{
				throw new ArgumentException("apInvoicingBase must be not null and AP", nameof(apInvoicingBase));
			}

			IJobInvoicingPlugIn invoicingPlugIn = null;
			IAccExchangeRateConfigurationRateConsumer rateConsumer = null;

			if (transaction.IsConsolInvoice)
			{
				invoicingPlugIn = transaction.Consol is IJobInvoicingPlugIn ? (IJobInvoicingPlugIn)transaction.Consol : null;
				rateConsumer = ExchangeRateConfigurationRateConsumerCreator.CreateExchangeRateConfigurationRateConsumerForConsolCost(transaction.Consol, null, apInvoicingBase.Company);
			}
			else if (!transaction.AH_JH.IsEmpty)  //Shipment AR Invoice
			{
				if (transaction.InvoicingJob != null && transaction.InvoicingJob.Parent != null)
				{
					invoicingPlugIn = transaction.Job.Parent as IJobInvoicingPlugIn;
					rateConsumer = ExchangeRateConfigurationRateConsumerCreator.CreateExchangeRateConfigurationRateConsumerForJob((Job)transaction.Job, apInvoicingBase.Company);
				}
			}

			return (invoicingPlugIn, rateConsumer);
		}

		ZDateTime GetPostDate(IJobInvoicingPlugIn invoicingPlugIn, ZDateTime invoiceDate)
		{
			if (invoicingPlugIn != null)
			{
				var helper = new PostDateConfigurationHelper(invoicingPlugIn);
				return helper.GetPostDate(ZDateTime.Today, invoiceDate);
			}
			else
			{
				return ZDateTime.Today;
			}
		}

		void CheckLineTaxAmount(InvoicingLineBase invoiceLine)
		{
			var taxErrorMessage = GetTaxAmountErrorMessageForInterCompanyInvoiceImport();
			if (!string.IsNullOrEmpty(taxErrorMessage))
			{
				invoiceLine.TaxAmountErrorDetailForInterCompanyInvoiceImport = (taxErrorMessage, invoiceLine.TaxRate.PK, invoiceLine.AL_OSTaxAmount, invoiceLine.AL_TaxRateCalc);
				(invoiceLine.Validation as InvoicingLineBaseValidation)?.ValidateAL_OSTaxAmount();
			}

			string GetTaxAmountErrorMessageForInterCompanyInvoiceImport()
			{
				var errorMessage = string.Empty;
				if (invoiceLine.TaxRateOverrideCalculator.IsUseTransactionContextAll)
				{
					if (invoiceLine.AL_OSTaxAmount != 0 && invoiceLine.TaxRate == null)
					{
						errorMessage = Res.GetString("D9239074-8270-4D49-9F39-D2CD51FA6B2C", "Line Tax Amount cannot be set if there is no Tax Code");
					}
					else if (invoiceLine.AL_OSTaxAmount != 0 && invoiceLine.AL_TaxRateCalc == 0)
					{
						errorMessage = Res.GetString("DF43ADE0-A93E-4F7F-A876-2E21B8808E7F", "Line Tax Amount cannot be set if Tax Rate is zero");
					}
					else if (invoiceLine.AL_OSTaxAmount == 0 && invoiceLine.TaxRate != null && invoiceLine.AL_TaxRateCalc != 0)
					{
						errorMessage = Res.GetString("4673EA27-A1AE-40E6-B1C8-9ABA98C26201", "Line Tax Amount cannot be zero if Tax Rate is not zero");
					}
					else if (invoiceLine.AL_OSTaxAmount != 0 && invoiceLine.TaxRate != null && invoiceLine.AL_TaxRateCalc != 0 && invoiceLine.IsOutsideExpectedTaxAmount())
					{
						errorMessage = Res.GetString("9B7BA8DD-45F9-436C-98AB-5525E8351E94", "Tax amount entered is outside the expected value for the selected tax rate");
					}
				}
				return errorMessage;
			}
		}
		#endregion
	}
}
