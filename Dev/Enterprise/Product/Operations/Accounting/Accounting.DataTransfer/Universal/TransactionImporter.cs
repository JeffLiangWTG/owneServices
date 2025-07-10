#define SafeTransitionFromIncorrectUsingOfDataContext

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.USSalesTax;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.Accounting.Integration;
using Enterprise.BatchProcessor;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.CodeMapping;
using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using DataTransferSubAccountHelper = Enterprise.Accounting.DataTransfer.Invoices.SubAccountHelper;
using DummyLogger = Enterprise.UniversalDataBuss.Integration.DummyLogger;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using UniversalTransaction = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.DataTransfer.Universal
{
	public class TransactionImporter : ITransactionImporter
	{
		public TransactionImporter()
		{
			InitializeMapping();
		}

		/// <summary>
		/// UMI messages are being processed in Parallel now.
		/// This means that if there are any changes to, or based upon existing jobs in ImportTransaction, these jobs need to be returned as "Keys" from this function.
		/// </summary>
		public IKeysResult GetKeysForBlockingParallelImport(IEDIMessage message, ITopLevelDataObject dataObject, IXmlImportLogger logger, IUniversalObjectFactory factory)
		{
			return new UniversalKeysResult(Enumerable.Empty<(string KeyValue, string KeySource)>());
		}

		public bool ImportTransaction(IEDIMessage message, ITopLevelDataObject dataObject, IXmlImportLogger logger, IUniversalObjectFactory factory)
		{
			var result = false;
			var factoryCasted = (UniversalObjectFactory)factory;
			var universalTransaction = dataObject as UniversalTransaction;

			if (universalTransaction == null)
			{
				logger.LogBoth(LogType.Error, Res.GetString("0dc65d97-48b7-49cc-80db-0ab41c8a6b72", "Data object type is not Universal Transaction type."));
				return false;
			}

			var isCrossLedgerImport = universalTransaction.HasRecipientRole(RecipientRoleType.IDB) && universalTransaction.Ledger.HasValue && universalTransaction.Ledger.Value == LedgerTypes.AccountsReceivable;

			if (universalTransaction.TransactionType == TransactionType.JNL && universalTransaction.Ledger.HasValue && (universalTransaction.Ledger.Value == LedgerTypes.AccountsReceivable || universalTransaction.Ledger.Value == LedgerTypes.AccountsPayable))
			{
				result = ImportARAPJournals(universalTransaction, message, logger, factoryCasted);
			}
			else if ((universalTransaction.Ledger.HasValue && universalTransaction.Ledger.Value == LedgerTypes.AccountsPayable) || isCrossLedgerImport)
			{
				if (IsShipmentDataTypeSupported(universalTransaction, logger))
				{
					result = ImportAPAndCrossLedgerTransactions(universalTransaction, message, logger, factoryCasted, isCrossLedgerImport);
				}
			}
			else if (universalTransaction.HasRecipientRole(RecipientRoleType.ORP) && universalTransaction.Ledger.HasValue && universalTransaction.Ledger.Value == LedgerTypes.AccountsReceivable)
			{
				result = ImportARTransactions(universalTransaction, message, logger, factoryCasted);
			}
			else
			{
				logger.LogBoth(LogType.Error, Res.GetString("af1d72c9-074c-4396-8d93-f5278919221d", "Transaction Ledger is not supported"));
			}

			return result;
		}

		#region Import AR/AP Journals

		bool ImportARAPJournals(UniversalTransaction universalTransaction, IEDIMessage message, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			var result = false;

			var transaction = universalTransaction.Ledger.Value == LedgerTypes.AccountsPayable ? factory.BOFactory.New<APJournal>() : (Journal)factory.BOFactory.New<ARJournal>();

			using (factory.BOFactory.AddDisposableService())
			{
				SetJournalValues(factory, transaction, universalTransaction, logger);

				using (((IBusinessObjectInternals)transaction).ResumeValidationForAllDescendantsTemporarily())
				{
					transaction.RunPreSaveValidation();
				}

				var transactionErrors = GetARAPJournalsErrors(transaction);
				if (!transactionErrors.IsEmpty)
				{
					throw new MessageProcessingBusinessFailureException(transactionErrors, false, string.Empty);
				}
				else if (!(logger?.HasErrors()).GetValueOrDefault())
				{
					factory.AssertAllowedSaveTypes(GetAllowableTypes());
					factory.RecordEndOfEveryRead(transaction);
					result = SaveTransaction(logger, factory, message.EM_MessageNum);
				}
			}

			return result;
		}

		void SetJournalValues(UniversalObjectFactory factory, Journal journal, UniversalTransaction universalTransaction, IXmlImportLogger logger)
		{
			SetOrganization(factory, journal, universalTransaction, logger);

			if (universalTransaction.TransactionDate.HasValue && universalTransaction.TransactionDate.Value.IsValid)
			{
				SetValue(journal, TransactionHeaderElement.TransactionDate, universalTransaction.TransactionDate);
			}
			else
			{
				SetValue(journal, TransactionHeaderElement.TransactionDate, (ZDateTime?)ZDateTime.Now);
			}

			SetValue(journal, TransactionHeaderElement.PostDate, universalTransaction.PostDate);

			ZInt numberOfDocumentsXmlValue;
			if (universalTransaction.NumberOfSupportingDocuments.TryGetValue(out numberOfDocumentsXmlValue) && numberOfDocumentsXmlValue > 0)
			{
				journal.AH_NumberOfSupportingDocuments = numberOfDocumentsXmlValue > byte.MaxValue ? (ZByte)byte.MaxValue : (ZByte)(byte)numberOfDocumentsXmlValue;
			}

			SetValue(journal, TransactionHeaderElement.AgreedPaymentMethod, universalTransaction.AgreedPaymentMethod);
			SetValue(journal, TransactionHeaderElement.DueDate, universalTransaction.DueDate);
			SetJournalAmountAndExchangeRate(journal, universalTransaction);
			SetValue(journal, TransactionHeaderElement.Description, universalTransaction.Description);
			SetValue(journal, TransactionHeaderElement.Branch, universalTransaction.Branch.GetValueSafe(x => x.Code));
			SetValue(journal, TransactionHeaderElement.Department, universalTransaction.Department.GetValueSafe(x => x.Code));

			SetGLAccount(journal, universalTransaction, logger);

			if (universalTransaction.AttachedDocumentCollection != null && universalTransaction.AttachedDocumentCollection.Count > 0)
			{
				var eDocsReader = new AttachedDocumentDataObjectReader();
				foreach (var attachedDocument in universalTransaction.AttachedDocumentCollection)
				{
					eDocsReader.TryAddAttachedDocument(attachedDocument, logger, journal, out IeDoc _);
				}
			}
		}

		static void SetOrganization(UniversalObjectFactory universalFactory, TransactionHeader transaction, UniversalTransaction universalTransaction, IXmlImportLogger logger)
		{
			var addressBO = GetOrgAddress(universalFactory, universalTransaction, logger);
			if (addressBO != null)
			{
				transaction.AH_OH = addressBO.OA_OH;
			}
		}

		void SetGLAccount(Journal journal, UniversalTransaction universalTransaction, IXmlImportLogger logger)
		{
			journal.AH_AG = Guid.Empty;
			SetValue(journal, TransactionHeaderElement.GLAccount, universalTransaction.PostingJournalCollection?.FirstOrDefault()?.GLAccount?.GetValueSafe(x => x.AccountCode));

			var gLHeader = journal.Factory.Load<AccGLHeader>(journal.AH_AG);

			if (gLHeader != null && !gLHeader.SubAccountTypes.IsNullOrEmpty() && universalTransaction.PostingJournalCollection?.FirstOrDefault()?.SubAccountCollection != null)
			{
				foreach (var subAccount in universalTransaction.PostingJournalCollection.FirstOrDefault().SubAccountCollection)
				{
					ImportSubAccount(journal, subAccount, logger);
				}
			}

			var mandatoryTypes = gLHeader?.SubAccountTypes.OfType<AccGLHeaderSubAccount>().Where(x => x.ASA_IsSubClassValidationRuleMandatory).Select(x => x.ASA_SubClass).ToList();

			if (mandatoryTypes?.Any() ?? false)
			{
				var missingTypes = journal.SubAccounts.OfType<JournalSubAccount>().Where(x => x.AHS_SubClassParentId.IsEmpty && mandatoryTypes.Contains(x.AHS_SubClassParentTableCode)).Select(x => x.AHS_Calc_SubClassParent).ToArray();

				if (missingTypes?.Length != 0)
				{
					LogMissingMandatoryTypeSubAccount(logger, missingTypes);
				}
			}
		}

		void ImportSubAccount(Journal journal, SubAccount subAccount, IXmlImportLogger logger)
		{
			var type = subAccount.Type?.Code.ToString();
			var code = subAccount.Code.ToString();
			if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(code))
			{
				LogInvalid(logger);
				return;
			}

			var journalSubAccount = journal.SubAccounts.OfType<JournalSubAccount>().FirstOrDefault(x => x.SubAccountTypeDisplayCode.Equals(type));
			if (journalSubAccount == null)
			{
				LogInvalid(logger);
				return;
			}

			var subClassParentId = DataTransferSubAccountHelper.GetSubAccountPKFromCode(journal.Factory, type, code);
			if (subClassParentId.IsEmpty)
			{
				LogInvalid(logger);
				return;
			}

			if (journalSubAccount.AHS_SubClassParentId.IsEmpty)
			{
				journalSubAccount.AHS_SubClassParentId = subClassParentId;
			}
			else if (journalSubAccount.AHS_SubClassParentId != subClassParentId)
			{
				LogDuplicate(logger);
			}
		}

		void LogMissingMandatoryTypeSubAccount(IXmlImportLogger logger, ZString[] types) => logger.Log(LogType.Error, Res.GetString("E0CB2328-56E8-43D7-AC85-07C4CA2CD6CA", "Sub Account: Please enter a Sub Account for '{0}' sub account type as it is mandatory.", string.Join("', '", types)));
		void LogInvalid(IXmlImportLogger logger) => logger.Log(LogType.Warning, Res.GetString("1e8bd977-3598-4bea-8d37-d40842ae9f64", "The system found invalid sub account info."));
		void LogDuplicate(IXmlImportLogger logger) => logger.Log(LogType.Warning, Res.GetString("81d98731-989a-4d1c-93c5-5025c808968e", "The system found duplicate sub account values for the same sub account type and the first valid value was imported."));

		ZString GetARAPJournalsErrors(Journal journal)
		{
			if (journal.HasErrors)
			{
				var transactionErrors = new StringBuilder();
				transactionErrors.AppendLine(Res.GetString("66F94FAD-E875-4EE2-9D85-103FCD8CF219", "Import failed because journal has validation errors:"));
				transactionErrors.AppendLine(string.Join("\r\n", new ZNotificationCollector(journal, true, true, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).Where(x => x.Type == NotificationType.Error).Select(y => y.Message)));

				return transactionErrors.ToString();
			}

			return ZString.Empty;
		}

		void SetJournalAmountAndExchangeRate(Journal journal, UniversalTransaction universalTransaction)
		{
			ZString osCurrencyCodeXmlValue;
			if (universalTransaction.OSCurrency.GetValueSafe(x => x.Code).TryGetValue(out osCurrencyCodeXmlValue) && !osCurrencyCodeXmlValue.IsEmpty)
			{
				journal.ExchangeRate.Currency = osCurrencyCodeXmlValue;
				ZDecimal exchangeRateXmlValue;
				if (universalTransaction.ExchangeRate.TryGetValue(out exchangeRateXmlValue) && exchangeRateXmlValue > 0)
				{
					journal.ExchangeRate.Rate = exchangeRateXmlValue;
				}
			}

			var localCurrency = journal.Company.LocalCurrency.RX_Code;
			ZString localCurrencyCodeXmlValue;

			var isLocalValuesApplicable = journal.AH_RX_NKTransactionCurrency != localCurrency &&
											universalTransaction.LocalCurrency.GetValueSafe(x => x.Code).TryGetValue(out localCurrencyCodeXmlValue) &&
											localCurrencyCodeXmlValue == localCurrency;

			journal.DebitCreditSign = universalTransaction.OSExGSTVATAmount > 0 ? DebitCreditDataEntry.CR : DebitCreditDataEntry.DR;
			journal.AH_OSExTaxAmount = Math.Abs(universalTransaction.OSExGSTVATAmount.GetValueOrDefault());
			if (isLocalValuesApplicable)
			{
				journal.AH_LocalExTaxAmount = Math.Abs(universalTransaction.LocalExVATAmount.GetValueOrDefault());
			}
		}

		#endregion

		bool IsShipmentDataTypeSupported(UniversalTransaction universalTransaction, IXmlImportLogger logger)
		{
			var isShipmentDataTypeSupported = true;

			if (universalTransaction.Ledger.Value != LedgerTypes.AccountsPayable || universalTransaction.ShipmentCollection == null || !universalTransaction.ShipmentCollection.Any())
			{
				return isShipmentDataTypeSupported;
			}

			var jobNumbers = universalTransaction.PostingJournalCollection?.Select(p => p.Job.GetValueSafe(x => x.Key).TryGetValue(out var jobNumber) && !jobNumber.IsEmpty ? jobNumber : ZString.Empty).Distinct().Where(p => p != ZString.Empty);

			if (jobNumbers == null || !jobNumbers.Any())
			{
				return isShipmentDataTypeSupported;
			}

			foreach (var jobNumber in jobNumbers)
			{
				var universalShipment = universalTransaction.ShipmentCollection.GetExactDataObject(jobNumber);
				if (universalShipment != null && (!TryGetShipmentDataContextAndManager(universalShipment.DataContext.DataSourceCollection, logger, out var contextManager, out var dataContextType) || contextManager == null))
				{
					isShipmentDataTypeSupported = false;
				}
			}

			return isShipmentDataTypeSupported;
		}

		static bool TryGetShipmentDataContextAndManager(IEnumerable<IDataSourceDataObject> dataSourceCollection, IXmlImportLogger logger, out IShipmentDataContextManager contextManager, out DataContextType dataContextType)
		{
			var type = dataSourceCollection.FirstOrDefault().Type.GetValueOrDefault();

			contextManager = null;

			if (Enum.TryParse(type, out dataContextType))
			{
				contextManager = dataContextType.GetUniversalDataContextManager() as IShipmentDataContextManager;

				if (contextManager == null)
				{
					logger.Log(LogType.Error, Res.GetString("7C01EADC-4642-41F6-9049-C6E43EF38A6D", "Data Type '{0}' import is not supported.", dataContextType));
				}

				return true;
			}
			else
			{
				logger.Log(LogType.Error, Res.GetString("4AE5E0CC-05B4-43C2-B6F9-58A3CB65F575", "Data Type '{0}' is not valid ", type));
				return false;
			}
		}

		void IsCashAdvancePresent(UniversalTransaction universalTransaction, IXmlImportLogger logger, string ledgerType)
		{
			if (universalTransaction != null && universalTransaction.PostingJournalCollection != null)
			{
				foreach (var universalLine in universalTransaction.PostingJournalCollection)
				{
					if (universalLine.CashAdvanceAmount.HasValue)
					{
						if (ledgerType == LedgerTypes.AccountsPayable)
						{
							logger.LogBoth(LogType.Warning, Res.GetString("35b82cc8-369a-459f-bc02-1a0756c28a68", "Advance Payment information is present in AP transaction"));
							break;
						}
						else if (ledgerType == LedgerTypes.AccountsReceivable)
						{
							logger.LogBoth(LogType.Warning, Res.GetString("3a351abd-aeb1-4820-8e64-ec1e5fb3110f", "Advance Payment information is present in AR transaction"));
							break;
						}
					}
				}
			}
		}

		bool ImportAPAndCrossLedgerTransactions(UniversalTransaction universalTransaction, IEDIMessage message, IXmlImportLogger logger, UniversalObjectFactory factory, bool isCrossLedgerImport)
		{
			var result = false;

			IsCashAdvancePresent(universalTransaction, logger, LedgerTypes.AccountsPayable);

			if (!AccountingMasterFilesRegistry.Instance.EnableTransactionPendingAllocationApproval.Value)
			{
				return false;
			}

			if (isCrossLedgerImport)
			{
				var isSentFromNettingSystem = universalTransaction.HasRecipientRole(RecipientRoleType.WNS);
				if (isSentFromNettingSystem)
				{
					if (universalTransaction.DataContext != null && universalTransaction.DataContext.IsFromSameSystem()) //Netting always sends xml in context of source system always generated by another or the same CW1 system. So we can treat DataContext as source system in this case.
					{
						logger.LogBoth(LogType.Warning, Res.GetString("170ab6ee-81cd-414a-b411-b9026e755abf", "Transaction Pending Allocation is not created since both the Issuer and Recipient is in the same Database and Recipient participates in Netting. Please use Intercompany Transaction Approval to approve any intercompany invoice."));
						return false;
					}
				}
				else if (message.Interchange != null && message.Interchange.EI_From == GlbCompany.CurrentCompany.LicenceKeyIdentifier)
				{
					logger.LogBoth(LogType.Error, Res.GetString("8E27344E-BD89-43DE-9A1E-48264901A553", "Transaction Pending Allocation is not created since both the Issuer and Recipient is in the same company."));
					return false;
				}
			}

			var multiplier = GetMultiplier(isCrossLedgerImport);
			var osExTaxAmount = multiplier * universalTransaction.OSExGSTVATAmount.GetValueOrDefault();
			var localExTaxAmount = multiplier * universalTransaction.LocalExVATAmount.GetValueOrDefault();
			if (AccountingMasterFilesUtils.ShouldPreventCreateCreditNote(LedgerTypes.AccountsPayable, GlbCompany.CurrentCompany.PK) && (osExTaxAmount < 0 || localExTaxAmount < 0))
			{
				var errorMessage = Res.GetString("020CA9AD-ED7E-4279-8C6A-73B519DDB8C5", "Transaction Pending Allocation is not created since it would create Transaction Pending Allocation with negative amounts but {0}", AccountingMasterFilesUtils.APCreditNoteDisallowedMessage);
				logger.LogBoth(LogType.Error, errorMessage);
				return false;
			}

			var transaction = factory.BOFactory.New<TransactionPendingAllocation>();
			SetTransactionPropertiesCachedInUniversalFactory(transaction, factory);
			using (factory.BOFactory.AddDisposableService())
			{
				using (factory.BOFactory.SetTempContext(OrganisationMatcherContexts.Payables))
				{
					SetTransactionValues(factory, transaction, universalTransaction, isCrossLedgerImport, logger, false);
				}

				SetPendingAllocateTransactionLines(transaction, universalTransaction, logger);

				if (!(logger?.HasErrors()).GetValueOrDefault())
				{
					ZString sourceXML;
					using (var reader = message.GetEM_MessageTextReader())
					{
						sourceXML = reader.ReadToEnd();
					}

					var sourceXMLToStoreInRequest = ReduceXMLSizeByRemovingElementsNotUsedInFuture(universalTransaction, sourceXML);

					var request = factory.BOFactory.New<TransactionPendingAllocationApprovalRequest>();
					request.Initialize(transaction, sourceXMLToStoreInRequest, isCrossLedgerImport);

					if (isCrossLedgerImport)
					{
						IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnUXMLImport(transaction,
							GetIntercompanyBranchDepartmentDeciderSourceWrapper(factory, transaction, universalTransaction, logger));
						LogRowWarnings(logger, transaction);
					}

					using (((IBusinessObjectInternals)transaction).ResumeValidationForAllDescendantsTemporarily())
					{
						transaction.RunPreSaveValidation();
					}
					if (transaction.HasErrors)
					{
						request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Error;
					}

					factory.AssertAllowedSaveTypes(GetAllowableTypes());
					factory.RecordEndOfEveryRead(transaction);
					result = SaveTransaction(logger, factory, message.EM_MessageNum);

					if (result && transaction is TransactionPendingAllocation pendingAllocation)
					{
						var loggerMsg = logger.ToString();
						if (!string.IsNullOrEmpty(loggerMsg))
						{
							var stmNote = pendingAllocation.CreateSystemNote(PredefinedNoteTypes.Instance.DataImportLogNote);
							stmNote.ST_NoteText = loggerMsg;

							factory.AssertAllowedSaveTypes(new[] { typeof(StmNote) });
							factory.RecordEndOfEveryRead(stmNote);
							factory.SaveAtEndOfImport(logger);
						}
					}
				}
			}

			return result;
		}

		bool ImportARTransactions(UniversalTransaction universalTransaction, IEDIMessage message, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			var result = false;

			if (universalTransaction.Job?.Key.HasValue ?? false)
			{
				logger.LogBoth(LogType.Error, Res.GetString("872c3655-2eb3-437a-be65-a3add51a11bf", "We cannot import AR transactions which have a Job."));
				return false;
			}

			IsCashAdvancePresent(universalTransaction, logger, LedgerTypes.AccountsReceivable);

			var transaction = universalTransaction.TransactionType.HasValue ? (universalTransaction.TransactionType.Value == TransactionType.INV ? factory.BOFactory.New<ARInvoice>() :
				universalTransaction.TransactionType.Value == TransactionType.CRD ? factory.BOFactory.New<ARCreditNote>() :
				universalTransaction.TransactionType.Value == TransactionType.ADJ ? (InvoicingBase)factory.BOFactory.New<ARAdjustmentNote>() : null) : null;

			if (transaction == null)
			{
				logger.LogBoth(LogType.Error, Res.GetString("91cf2c8a-4845-4010-80e0-43bbe0ba45b7", "You can import AR transaction only with type Invoice, Credit Note or Adjustment Note."));
				return false;
			}

			if (!string.IsNullOrEmpty(universalTransaction.CheckNumberOrPaymentRef) && CheckARTransactionDuplicated(transaction, universalTransaction.CheckNumberOrPaymentRef))
			{
				logger.LogBoth(LogType.Error, Res.GetString("DD9737A0-28BF-42D9-957F-79311F0DEC4C", "There is an existing AR Invoice or Credit Note with the same cheque number or payment reference."));
				return false;
			}

			SetTransactionPropertiesCachedInUniversalFactory(transaction, factory);
			using (factory.BOFactory.SetTempContext(OrganisationMatcherContexts.Receivables))
			{
				SetTransactionValues(factory, transaction, universalTransaction, false, logger, true);
			}

			ImportTransactionLinesCore(universalTransaction, transaction, false, true, logger);

			transaction.SetExchangeRateForInvoicePostingExchangeRateOption(AccountingConstants.InvoicePostingExchangeRateOption.EarliestOfInvoiceOrTaxDate.Code);

			var surchargeLineCreator = ObjectFactory.Get<ISurchargeLineCreator>();
			surchargeLineCreator.AddSurchargeLine(transaction);
			using (((IBusinessObjectInternals)transaction).ResumeValidationForAllDescendantsTemporarily())
			using (transaction.ValidateEmptyComplianceSequenceSuspender.GetSuspender())
			{
				transaction.RunPreSaveValidation();
			}

			var transactionErrors = GetTransactionErrors(transaction);
			if (!transactionErrors.IsEmpty)
			{
				//Architecture team told us to throw MessageProcessingBusinessFailureException when there is validation error, so that any bizos created are guaranteed not to be saved
				throw new MessageProcessingBusinessFailureException(transactionErrors, false, string.Empty);
			}
			else if (!(logger?.HasErrors()).GetValueOrDefault())
			{
				factory.AssertAllowedSaveTypes(GetAllowableTypes());
				factory.RecordEndOfEveryRead(transaction);
				result = SaveTransaction(logger, factory, message.EM_MessageNum);
			}

			return result;
		}

		ZString GetTransactionErrors(InvoicingBase transaction)
		{
			var complianceError = transaction.AssignComplianceSubTypeAndCheckComplianceErrors();
			if (transaction.HasErrors || !complianceError.IsEmpty)
			{
				var transactionErrors = new StringBuilder();
				transactionErrors.AppendLine(Res.GetString("37d1aea5-fcfa-4c11-9710-367bc7ddcc61", "Import failed because transaction has validation errors:"));

				if (transaction.HasErrors)
				{
					transactionErrors.AppendLine(string.Join("\r\n", new ZNotificationCollector(transaction, true, true, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).Where(x => x.Type == NotificationType.Error).Select(y => y.Message)));
				}

				if (!complianceError.IsEmpty)
				{
					transactionErrors.AppendLine(complianceError);
				}

				return transactionErrors.ToString();
			}

			return ZString.Empty;
		}

		bool CheckARTransactionDuplicated(InvoicingBase invoice, ZString? checkNumberOrPaymentRef)
		{
			var query = new ZQuery();
			query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, new ZString[] { TransactionTypes.Invoice, TransactionTypes.CreditNote });
			query.AddToFilter(AccTransactionHeaderSchema.AH_ChequeOrReference, checkNumberOrPaymentRef);
			query.AddToFilter(AccTransactionHeaderSchema.AH_GC, invoice.AH_GC);
			return invoice.Factory.Exists(typeof(AccTransactionHeader), query);
		}

		bool SaveTransaction(IXmlImportLogger logger, UniversalObjectFactory factory, ZString messageNum)
		{
			try
			{
				factory.SaveAtEndOfImport(logger);
				return true;
			}
			catch (ZSaveException e) when (!e.IndexNameIfUniqueIndexViolation.IsEmpty)
			{
				logger.LogBoth(LogType.Error, Res.GetString("0FB385A0-858E-4541-8CB1-9AE071400C29", "EDI Message {0} could not be imported due to duplicate Transaction Pending Allocation", messageNum));
				return false;
			}
		}

		void SetTransactionPropertiesCachedInUniversalFactory(InvoicingBase transaction, UniversalObjectFactory universalFactory)
		{
			transaction.USSalesTaxCalculator = universalFactory.GetCachedValue("UniversalImporter_IUSSalesTaxCalculator", () => ObjectFactory.Get<IUSSalesTaxCalculator>());
		}

		static IEnumerable<Type> GetAllowableTypes()
		{
			/*
			 *
			 *	Hello.
			 *	If new types are added here, it is necessary for you to modify GetKeysForBlockingParallelImport? (Probably)
			 *
			 */
			var transactionTypes = new HashSet<Type>()
			{
				typeof(TransactionPendingAllocation),
				typeof(TransactionPendingAllocationApprovalRequest),
				typeof(TransactionPendingAllocationApprovalDetails),
				typeof(AccTransactionHeader),
				GetBaseTypeOrClientOverrideType(typeof(ARInvoice)),
				typeof(ARCreditNote),
				typeof(ARAdjustmentNote),
				typeof(ARInvoiceLine),
				typeof(ARCreditNoteLine),
				typeof(ARAdjustmentNoteLine),
				typeof(TransactionLineSubAccount),
				typeof(AccTransactionLines),
				typeof(StmALog),
				typeof(ARJournal),
				typeof(APJournal),
				typeof(JournalSubAccount)
			};

			foreach (Type type in transactionTypes)
			{
				yield return type;
			}

			Type GetBaseTypeOrClientOverrideType(Type baseType)
			{
				var allowedType = baseType;
				var clientSpecificType = ClientHookLoader.Instance.ClientHook?.ClientTypeDeciders?[baseType]?.GetTypeForBinding();
				if (clientSpecificType != null)
				{
					allowedType = clientSpecificType;
				}
				return allowedType;
			}
		}

		public void ImportTransactionLines(ZString xml, IAccTransactionHeader transaction, bool isCrossLedgerImport)
		{
			var invoice = transaction as InvoicingBase;
			if (invoice != null)
			{
				var universalTransaction = ImportUniversalTransactionFromXmlCore(xml, true, invoice.Factory, isCrossLedgerImport)?.Item1;
				if (universalTransaction != null)
				{
					ImportTransactionLinesCore(universalTransaction, invoice, isCrossLedgerImport, invoice.AH_Ledger == LedgerTypes.AccountsReceivable, new DummyLogger());
				}
			}
		}

		public Tuple<ITopLevelDataObject, ICodeMappingManager> ImportUniversalTransactionFromXml(ZString xml, BusinessObjectFactory factory, bool isCrossLedgerImport)
		{
			var result = ImportUniversalTransactionFromXmlCore(xml, true, factory, isCrossLedgerImport);

			return result != null ? Tuple.Create((ITopLevelDataObject)result.Item1, result.Item2) : null;
		}

		public void TrySetMappedValueDirectlyFromSourceCode(ITopLevelDataObject universalTransaction)
		{
			TrySetMappedValueDirectlyFromSourceCode((UniversalTransaction)universalTransaction);
		}

		public void TrySetMappedValueDirectlyFromSourceCode(IDataObject universalLine, BusinessObject line)
		{
			TrySetMappedValueDirectlyFromSourceCode((PostingJournal)universalLine, (InvoicingLineBase)line);
		}

		public IJobHeader GetJob(BusinessObjectFactory factory, ITopLevelDataObject universalTransaction, IDataObject postingJournal)
		{
			using (InitializeCache())
			{
				var universalTransactionCasted = (UniversalTransaction)universalTransaction;
				var postingJournalCasted = (PostingJournal)postingJournal;

				var logger = new AccountingLogger(new LoggingInformation(), universalTransaction);

				return GetJob(factory, universalTransactionCasted, postingJournalCasted, false, logger)?.Item1;
			}
		}

		public IOrgHeader GetOrganization(ITopLevelDataObject universalTransaction, IUniversalObjectFactory universalFactory, IXmlImportLogger logger, bool isCrossLedgerImport)
		{
			var universalTransactionCasted = (UniversalTransaction)universalTransaction;
			var universalFactoryCasted = (UniversalObjectFactory)universalFactory;

			if (isCrossLedgerImport)
			{
				return GetSourceOrganizationForCrossLedgerImport(universalTransactionCasted, universalFactoryCasted, logger, false).sourceOrg;
			}
			else
			{
				var orgAddress = GetOrgAddress(universalFactoryCasted, universalTransactionCasted, logger);
				if (orgAddress != null)
				{
					return orgAddress.Header;
				}

				return null;
			}
		}

		public BusinessObject GetConsol(ITopLevelDataObject universalTransaction, ZString consolNumber, ZString? consolType)
		{
			BusinessObject importedConsol = null;
			ZString errorMessge;
			var logger = new AccountingLogger(new LoggingInformation(), universalTransaction);

			var universalConsol = FindConsol((UniversalTransaction)universalTransaction, consolNumber, consolType);
			if (universalConsol != null)
			{
				importedConsol = ImportShipment(universalConsol, LineUniversalObjectFactory, out errorMessge, logger);
			}

			return importedConsol;
		}

		static Tuple<UniversalTransaction, ICodeMappingManager> ImportUniversalTransactionFromXmlCore(ZString xml, bool useCodeMapping, BusinessObjectFactory factory, bool isCrossLedgerImport)
		{
			string namespaceUsed;

			return ImportUniversalTransactionFromXmlCore(xml, useCodeMapping, factory, isCrossLedgerImport, out namespaceUsed);
		}

		static Tuple<UniversalTransaction, ICodeMappingManager> ImportUniversalTransactionFromXmlCore(ZString xml, bool useCodeMapping, BusinessObjectFactory factory, bool isCrossLedgerImport, out string namespaceUsed)
		{
			namespaceUsed = null;
			if (xml.IsEmpty)
			{
				return null;
			}

			SubStreamableStream stream = null;

			//Use Try-Finally block to dispose the stream instead of nested using scopes because we get a Code warning (CA2202: Do not dispose objects multiple times) as the inner scope will dispose the outer stream object
			try
			{
				stream = new CargoWise.IO.Shim.SubStreamableStream();
				using (var writer = new StreamWriter(stream))
				{
					var streamToReadFrom = stream;
					stream = null;
					writer.Write(xml);
					writer.Flush();
					streamToReadFrom.Position = 0;

					UniversalTransaction universalTransaction;
					CodeMappingManager codeMapper = null;
					IXmlImportLogger logger = new DummyLogger();
					if (useCodeMapping)
					{
						codeMapper = new CodeMappingManager(logger);
					}
					var parseData = streamToReadFrom.ParseAndReturnMoreData<UniversalTransaction>(logger, codeMapper);
					universalTransaction = parseData.DataObject;
					namespaceUsed = parseData.NamespaceUsed;
					if (codeMapper != null)
					{
						codeMapper.UpdateMappedCodes(universalTransaction, factory);
					}

					if (isCrossLedgerImport)
					{
						universalTransaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.Instance);
						universalTransaction.OrganizationAddress.AddressType = nameof(DocAddressType.None);

						universalTransaction.OrganizationAddress.OrganizationCode = GetSourceOrganizationForCrossLedgerImport(universalTransaction, new UniversalObjectFactory(factory), logger, true).sourceOrgMappedCode;
					}

					return Tuple.Create(universalTransaction, (ICodeMappingManager)codeMapper);
				}
			}
			finally
			{
				if (stream != null)
				{
					stream.Dispose();
				}
			}
		}

		void ImportTransactionLinesCore(UniversalTransaction universalTransaction, InvoicingBase transaction, bool isCrossLedgerImport, bool isARImport, IXmlImportLogger xmlImportlogger)
		{
			if (universalTransaction.PostingJournalCollection == null || universalTransaction.PostingJournalCollection.Count == 0)
			{
				return;
			}

			using (InitializeCache())
			using (transaction.Lines.SuspendListChanged())
			{
				var jobsToDispose = new HashSet<Job>();
				try
				{
					var factory = transaction.Factory;
					var linesAdded = new Dictionary<PostingJournal, InvoicingLineBase>();
					int universalLineIndex = 0;

					var linesInfoProvider = new LinesInfoProvider();
					var logger = new AccountingLogger(new LoggingInformation(), universalTransaction);

					var nvoccLinesbyWayBillNo = new Dictionary<ZString, List<InvoicingLineBase>>();  // way Bill Number => List<InvoiceLine>
					var needToRunNVOCC = NeedToRunNVOCCMatchingLogic(isCrossLedgerImport, universalTransaction);
					var isTaxApplicable = IsTaxApplicable(transaction, universalTransaction, xmlImportlogger);
					TransactionImportJobChargeMappingProvider.Initialize(factory, universalTransaction);

					foreach (var universalLine in universalTransaction.PostingJournalCollection)
					{
						var line = (InvoicingLineBase)transaction.Lines.AddNew();
						linesAdded.Add(universalLine, line);
						line.IndexOfImportedUniversalTransactionLine = universalLineIndex++;

						if (!isARImport)
						{
							SetJob(factory, universalTransaction, universalLine, line, jobsToDispose, isCrossLedgerImport, logger);
						}

						SetInvoiceLineValues(universalLine, line, isCrossLedgerImport, isTaxApplicable, isARImport, linesInfoProvider, universalTransaction.PlaceOfSupply, xmlImportlogger);
						if (needToRunNVOCC)
						{
							PrepareNVOCCLinesByWayBillNum(universalLine, line, nvoccLinesbyWayBillNo);
						}

						TransactionImportJobChargeMappingProvider.MapTransactionLine(line.Factory, universalLine, line);
					}

					Dictionary<ZString, Tuple<ForwardingConsol, List<InvoicingLineBase>>> nvoccLinesbyConsol = null;
					if (needToRunNVOCC)
					{
						nvoccLinesbyConsol = PrepareNVOCCLinesByConsolNum(transaction, factory, nvoccLinesbyWayBillNo);
					}
					var consolCostData = GenerateConsolCostingData(factory, universalTransaction, transaction, linesAdded, isCrossLedgerImport, logger, nvoccLinesbyConsol);
					CreateConsolCostsOrGetJobCostingPlugIn(consolCostData, linesInfoProvider, jobsToDispose);
					jobsToDispose.Clear();
				}
				finally
				{
					jobsToDispose.ForEach(x => x.Dispose());
				}
			}
		}

		Dictionary<ZString, Tuple<ForwardingConsol, List<InvoicingLineBase>>> PrepareNVOCCLinesByConsolNum(InvoicingBase transaction, BusinessObjectFactory factory, Dictionary<ZString, List<InvoicingLineBase>> nvoccLinesbyWayBillNo)
		{
			var result = new Dictionary<ZString, Tuple<ForwardingConsol, List<InvoicingLineBase>>>();  // consol Number => (consol, List<InvoiceLine>)
			if (nvoccLinesbyWayBillNo.Any())
			{
				// Check to see if a Consol exists in DB that:
				//a) Is a "Co-Load" Consol;
				//b) Has "Co-Load With" equal to the Creditor Organisation
				//c) Has a Co-Load MBL equal to the XUT's Master Bill of Lading
				var creditorPK = transaction.AH_OH;
				var wayBillNumbers = nvoccLinesbyWayBillNo.Keys.ToList();

				var query = new ZDBOnlyQuery(typeof(ForwardingConsol));
				query.AddToFilter(JobConsolSchema.JK_AgentType, Constants.AgentType.CoLoad);
				query.AddToFilter(JobConsolSchema.JK_CoLoadMasterBill, wayBillNumbers);
				var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobConsolSchema.JK_OA_CreditorAddress);
				var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);
				orgHeaderSubQuery.AddToFilter(OrgHeaderSchema.PK, creditorPK);
				orgAddressSubQuery.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);
				query.AddSubQuery(orgAddressSubQuery, JoinCondition.And);
				var nvoccConsols = factory.Load<ForwardingConsol>(query);

				var orgCode = transaction.Header.OH_Code;
				//Find any way bill numbers which cannot match any consol's MBL
				var consolCoLoadMBLs = nvoccConsols.Select(x => x.JK_CoLoadMasterBill).Distinct();
				var failedToMatchMBLs = wayBillNumbers.Where(n => !consolCoLoadMBLs.Contains(n));
				if (failedToMatchMBLs.Any())
				{
					foreach (var masterBillNum in failedToMatchMBLs)
					{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						transaction.Logs.AddNew(Events.EditedARecord, string.Format("No Consol found with Co-Load MBL=[{0}] and Co-Load With=[{1}]", masterBillNum, orgCode));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					}
				}

				//Check if any consols have the same JK_CoLoadMasterBill, if so, these problematic consols should be excluded
				var duplicateCoLoadMasterBill = nvoccConsols.GroupBy(x => x.JK_CoLoadMasterBill).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
				if (duplicateCoLoadMasterBill.Any())
				{
					foreach (var masterBillNum in duplicateCoLoadMasterBill)
					{
						var conflictConsols = nvoccConsols.Where(x => x.JK_CoLoadMasterBill == masterBillNum);
						var consolNumbers = conflictConsols.Select(x => x.JK_UniqueConsignRef).OrderBy(s => s);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						transaction.Logs.AddNew(Events.EditedARecord,
							string.Format("Charges not matched to Consol. More than one Consol found with Co-Load MBL[{0}] and Co-Load With=[{1}]. (Duplicate Consols: {2})",
							masterBillNum,
							orgCode,
							string.Join(",", consolNumbers)));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					}
					nvoccConsols = nvoccConsols.Where(x => !duplicateCoLoadMasterBill.Contains(x.JK_CoLoadMasterBill)).ToArray();
				}

				if (nvoccConsols != null && nvoccConsols.Any())
				{
					foreach (var consol in nvoccConsols)
					{
						var consolNumber = consol.JK_UniqueConsignRef;
						var masterBillNumber = consol.JK_CoLoadMasterBill;
						var lines = nvoccLinesbyWayBillNo[masterBillNumber];
						var content = new Tuple<ForwardingConsol, List<InvoicingLineBase>>(consol, lines);
						result.Add(consolNumber, content);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						transaction.Logs.AddNew(Events.EditedARecord,
							string.Format("Matched to Consol [{0}] Co-load MBL=[{1}] and Co-Load With=[{2}]", consolNumber, masterBillNumber, orgCode));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					}
				}
			}
			return result;
		}

		bool NeedToRunNVOCCMatchingLogic(bool isCrossLedgerImport, UniversalTransaction universalTransaction)
		{
			//We only need to run NVOCC matching for
			//1. cross ledger import
			//2. for non consol invoice,
			//3. when Combined Reference and Party ID Matching is set to "Yes"
			//4. When the line's job cannot be matched to an existing operational job in the DB (checked per line in PrepareNVOCCLinesByWayBillNum)
			var result = false;
			if (isCrossLedgerImport && eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.Value)
			{
				ZString jobInvoiceNumber;
				if (universalTransaction.JobInvoiceNumber.TryGetValue(out jobInvoiceNumber))
				{
					if (jobInvoiceNumber.IsEmpty  // invoice number is empty when import periodic invoice xml
						|| !universalTransaction.Job.GetValueSafe(x => x.Key).GetValueOrDefault().IsEmpty)  //Job.Key is not empty when import shipment invoice xml
					{
						result = true;
					}
				}
			}
			return result;
		}

		void PrepareNVOCCLinesByWayBillNum(PostingJournal universalLine, InvoicingLineBase line, Dictionary<ZString, List<InvoicingLineBase>> nvoccLinesbyWayBillNo)
		{
			if (line.AL_JH.IsEmpty)
			{
				ZString jobNumber;
				if (universalLine.Job.GetValueSafe(x => x.Key).TryGetValue(out jobNumber) && !jobNumber.IsEmpty
					&& wayBillNumberMapForCLDShipment.ContainsKey(jobNumber))  //Verify the line relates to a CLD type Shipment (Co-Load Master)
				{
					var wayBillNumber = wayBillNumberMapForCLDShipment[jobNumber];
					if (nvoccLinesbyWayBillNo.ContainsKey(wayBillNumber))
					{
						var lines = nvoccLinesbyWayBillNo[wayBillNumber];
						lines.Add(line);
					}
					else
					{
						var lines = new List<InvoicingLineBase>();
						lines.Add(line);
						nvoccLinesbyWayBillNo[wayBillNumber] = lines;
					}
				}
			}
		}

		IntercompanyTransactionImportHelper.IntercompanyBranchDepartmentDeciderSourceWrapper GetIntercompanyBranchDepartmentDeciderSourceWrapper(UniversalObjectFactory factory, TransactionPendingAllocation transaction, UniversalTransaction universalTransaction, IXmlImportLogger logger)
		{
			var result = new IntercompanyTransactionImportHelper.IntercompanyBranchDepartmentDeciderSourceWrapper();

			result.AH_GE = transaction.AH_GE;

			if (universalTransaction.OrganizationAddress != null)
			{
				var addressBO = GetOrgAddress(factory, universalTransaction, logger);
				if (addressBO != null)
				{
					result.AH_OH = addressBO.OA_OH;
				}
			}

			using (InitializeCache())
			{
				var consolCostData = GenerateConsolCostingData(factory.BOFactory.GetCachedReadOnlyFactory(), universalTransaction, transaction, null, true, logger);
				var consol = consolCostData.LinesByConsols.FirstOrDefault()?.JobCostingPlugIn;
				if (consol != null)
				{
					result.IsConsolInvoice = true;
					result.Consol = consol;
				}
				else if (universalTransaction.PostingJournalCollection != null)
				{
					foreach (var universalLine in universalTransaction.PostingJournalCollection)
					{
						var job = GetJob(factory.BOFactory.GetCachedReadOnlyFactory(), universalTransaction, universalLine, true, logger).Item1;
						if (job != null)
						{
							result.IsJobRelated = true;
							result.JobBranch = job.Branch;
							result.JobDepartmentPK = job.JH_GE;

							if (result.JobBranch == null)
							{
								transaction.AddRowWarning(Res.GetString("B75DBE84-1145-4A79-A202-26D0C85BEB3C", "Job '{0}' doesn't have Branch. This may affect correct Branch setting for importing transactions.", job.JH_JobNum));
							}
							if (!result.JobDepartmentPK.IsValid)
							{
								transaction.AddRowWarning(Res.GetString("638649C7-894E-4FDD-987F-631DED99A20B", "Job '{0}' doesn't have Department. This may affect correct Department setting for importing transactions.", job.JH_JobNum));
							}

							break;
						}
					}
				}
			}

			return result;
		}

		void SetTransactionValues(UniversalObjectFactory factory, InvoicingBase transaction, UniversalTransaction universalTransaction, bool isCrossLedgerImport, IXmlImportLogger logger, bool isARImport)
		{
			SetOrganization(factory, transaction, universalTransaction, isCrossLedgerImport, logger);

			if (universalTransaction.TransactionDate.HasValue && universalTransaction.TransactionDate.Value.IsValid)
			{
				SetValue(transaction, TransactionHeaderElement.TransactionDate, universalTransaction.TransactionDate);
			}
			else
			{
				if (AccountingMasterFilesRegistry.Instance.InvoiceDateDefaultValue.Value == AccountingMasterFilesConstants.InvoiceDateDefaultValueCodes.Blank && !isARImport)
				{
					logger.LogBoth(LogType.Error, Res.GetString("26CA0753-25BD-45DA-8B17-456FD1106B79", "Transaction Date is invalid."));
					return;
				}

				SetValue(transaction, TransactionHeaderElement.TransactionDate, (ZDateTime?)ZDateTime.Now);
			}

			SetValue(transaction, TransactionHeaderElement.PostDate, universalTransaction.PostDate);
			SetValue(transaction, TransactionHeaderElement.Number, universalTransaction.Number);
			SetValue(transaction, TransactionHeaderElement.GovernmentAllocatedID, universalTransaction.GovernmentAllocatedID);
			SetValue(transaction, TransactionHeaderElement.ChequeOrReference, universalTransaction.CheckNumberOrPaymentRef);
			SetValue(transaction, TransactionHeaderElement.DocumentReceivedDate, universalTransaction.DocumentReceivedDate);

			ZInt numberOfDocumentsXmlValue;
			if (universalTransaction.NumberOfSupportingDocuments.TryGetValue(out numberOfDocumentsXmlValue) && numberOfDocumentsXmlValue > 0)
			{
				transaction.AH_NumberOfSupportingDocuments = numberOfDocumentsXmlValue > byte.MaxValue ? (ZByte)byte.MaxValue : (ZByte)(byte)numberOfDocumentsXmlValue;
			}

			SetValue(transaction, TransactionHeaderElement.DueDate, universalTransaction.DueDate);

			ZString osCurrencyCodeXmlValue;
			if (universalTransaction.OSCurrency.GetValueSafe(x => x.Code).TryGetValue(out osCurrencyCodeXmlValue) && !osCurrencyCodeXmlValue.IsEmpty)
			{
				transaction.ExchangeRate.Currency = osCurrencyCodeXmlValue;
				ZDecimal exchangeRateXmlValue;
				if (isARImport && universalTransaction.ExchangeRate.TryGetValue(out exchangeRateXmlValue) && exchangeRateXmlValue > 0)
				{
					transaction.ExchangeRate.Rate = exchangeRateXmlValue;
				}
			}

			if (!isARImport)
			{
				SetValue(transaction, TransactionHeaderElement.ComplianceSubType, universalTransaction.ComplianceSubType);
				SetTransactionAmounts(transaction, universalTransaction, isCrossLedgerImport, logger);
			}

			SetValue(transaction, TransactionHeaderElement.Description, universalTransaction.Description);
			if (!isCrossLedgerImport)
			{
				SetValue(transaction, TransactionHeaderElement.Branch, universalTransaction.Branch.GetValueSafe(x => x.Code));
			}
			SetValue(transaction, TransactionHeaderElement.Department, universalTransaction.Department.GetValueSafe(x => x.Code));

			if (universalTransaction.AttachedDocumentCollection != null && universalTransaction.AttachedDocumentCollection.Count > 0)
			{
				var eDocsReader = new AttachedDocumentDataObjectReader();
				foreach (var attachedDocument in universalTransaction.AttachedDocumentCollection)
				{
					eDocsReader.TryAddAttachedDocument(attachedDocument, logger, transaction, out IeDoc _);
				}
			}

			if (PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(transaction.Company) && universalTransaction.PlaceOfSupply != null)
			{
				SetValue(transaction, TransactionHeaderElement.PlaceOfSupply_Location, universalTransaction.PlaceOfSupply.Location?.Code);
				SetValue(transaction, TransactionHeaderElement.PlaceOfSupply_LocationType, universalTransaction.PlaceOfSupply.LocationType?.Code);
			}
		}

		static void SetTransactionAmounts(InvoicingBase transaction, UniversalTransaction universalTransaction, bool isCrossLedgerImport, IXmlImportLogger logger)
		{
			var localCurrency = transaction.Company.LocalCurrency.RX_Code;
			ZString localCurrencyCodeXmlValue;
			var isLocalValuesApplicable = transaction.AH_RX_NKTransactionCurrency != localCurrency &&
											universalTransaction.LocalCurrency.GetValueSafe(x => x.Code).TryGetValue(out localCurrencyCodeXmlValue) &&
											localCurrencyCodeXmlValue == localCurrency;
			int multiplier = GetMultiplier(isCrossLedgerImport);
			var osExTaxAmount = multiplier * universalTransaction.OSExGSTVATAmount.GetValueOrDefault();
			var localExTaxAmount = multiplier * universalTransaction.LocalExVATAmount.GetValueOrDefault();
			var osTaxAmount = multiplier * universalTransaction.OSGSTVATAmount.GetValueOrDefault();
			var localTaxAmount = multiplier * universalTransaction.LocalVATAmount.GetValueOrDefault();
			if (IsTaxApplicable(transaction, universalTransaction, logger))
			{
				transaction.AH_OSExTaxAmount = osExTaxAmount;
				if (isLocalValuesApplicable)
				{
					transaction.AH_LocalExTaxAmount = localExTaxAmount;
				}

				transaction.AH_OSTaxAmount = osTaxAmount;
				if (isLocalValuesApplicable)
				{
					transaction.AH_LocalTaxAmount = localTaxAmount;
				}
			}
			else
			{
				transaction.AH_OSExTaxAmount = osExTaxAmount + osTaxAmount;
				if (isLocalValuesApplicable)
				{
					transaction.AH_LocalExTaxAmount = localExTaxAmount + localTaxAmount;
				}
			}
		}

		static int GetMultiplier(bool isCrossLedgerImport)
		{
			return isCrossLedgerImport ? 1 : -1;
		}

		static void SetOrganization(UniversalObjectFactory universalFactory, InvoicingBase transaction, UniversalTransaction universalTransaction, bool isCrossLedgerImport, IXmlImportLogger logger)
		{
			if (isCrossLedgerImport)
			{
				var sourceOrg = GetSourceOrganizationForCrossLedgerImport(universalTransaction, universalFactory, logger, false).sourceOrg;
				if (sourceOrg != null)
				{
					transaction.AH_OH = sourceOrg.PK;
				}
			}
			else if (universalTransaction.OrganizationAddress != null)
			{
				var addressBO = GetOrgAddress(universalFactory, universalTransaction, logger);
				if (addressBO != null)
				{
					transaction.AH_OH = addressBO.OA_OH;
					transaction.AH_OA_InvoiceAddressOverride = addressBO.PK;
				}

				ZString contactXmlValue;
				if (universalTransaction.OrganizationAddress.Contact.TryGetValue(out contactXmlValue) && !contactXmlValue.IsEmpty)
				{
					var query = new ZQuery(OrgContactSchema.OC_ContactName, contactXmlValue);
					ZString phoneXmlValue;
					if (universalTransaction.OrganizationAddress.Phone.TryGetValue(out phoneXmlValue) && !phoneXmlValue.IsEmpty)
					{
						query.AddToFilter(OrgContactSchema.OC_Phone, phoneXmlValue);
					}
					ZString emailXmlValue;
					if (universalTransaction.OrganizationAddress.Email.TryGetValue(out emailXmlValue) && !emailXmlValue.IsEmpty)
					{
						query.AddToFilter(OrgContactSchema.OC_Email, emailXmlValue);
					}

					var contact = universalFactory.BOFactory.LoadTop1<OrgContact>(query);
					if (contact != null)
					{
						transaction.AH_OC_InvoiceContactOverride = contact.PK;
					}
				}
			}
		}

		void SetInvoiceLineValues(PostingJournal universalLine, InvoicingLineBase line, bool isCrossLedgerImport,
			bool isTaxApplicable, bool isARImport, LinesInfoProvider linesInfoProvider, PlaceOfSupply headerPlaceOfSupply, IXmlImportLogger logger)
		{
			var isChargeCodeSet = SetValue(line, TransactionLineElement.ChargeCode, (ZString?)universalLine.ChargeCode.GetValueSafe(x => x.Code));
			if (!isChargeCodeSet)
			{
				SetValue(line, TransactionLineElement.GLAccount, universalLine.GLAccount.GetValueSafe(x => x.AccountCode));
			}
			line.GenericCharge = line.AL_AC.IsEmpty ? line.AL_AG : line.AL_AC;

			SetValue(line, TransactionLineElement.Description, universalLine.Description);

			if (!isARImport)
			{
				SetValue(line, TransactionLineElement.IsFinalCharge, universalLine.IsFinalCharge);
			}

			if (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value && universalLine.GovernmentReportingChargeCode.HasValue)
			{
				SetValue(line, TransactionLineElement.GovtChargeCode, universalLine.GovernmentReportingChargeCode);
			}

			if (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value)
			{
				SetValue(line, TransactionLineElement.SupplyType, universalLine.SupplyType?.Code);
			}

			SetSequence(line, universalLine);

			if (!isCrossLedgerImport)
			{
				SetValue(line, TransactionLineElement.Branch, universalLine.Branch.GetValueSafe(x => x.Code));
				SetValue(line, TransactionLineElement.Department, universalLine.Department.GetValueSafe(x => x.Code));
			}

			SetExchangeRate(line, universalLine);
			SetTaxData(line, universalLine, isTaxApplicable, linesInfoProvider, isARImport);
			SetInvoiceLineAmounts(line, universalLine, isCrossLedgerImport, isTaxApplicable, isARImport, linesInfoProvider);

			if (GlbCompany.CurrentCompany.GC_IsWHTRegistered)
			{
				SetValue(line, TransactionLineElement.WithholdingTaxID, universalLine.WithholdingTaxID.GetValueSafe(x => x.TaxCode));
			}

			SetInvoiceLinePlaceOfSupply(line, headerPlaceOfSupply, universalLine.PlaceOfSupply);

			SetSubAccountCollection(line, universalLine, logger);
		}

		void SetPendingAllocateTransactionLines(InvoicingBase transaction, UniversalTransaction universalTransaction, IXmlImportLogger logger)
		{
			if (!universalTransaction.PostingJournalCollection.IsNullOrEmpty())
			{
				BusinessObjectFactory newFactory = new BusinessObjectFactory();

				foreach (var universalLine in universalTransaction.PostingJournalCollection)
				{
					InvoicingBase invoice = null;

					if (transaction.AH_TransactionType == TransactionTypes.InvoicePendingAllocation)
					{
						invoice = newFactory.New<APInvoice>();
					}
					else
					{
						invoice = newFactory.New<APCreditNote>();
					}
					var line = (InvoicingLineBase)invoice.Lines.AddNew();
					var isChargeCodeSet = SetValue(line, TransactionLineElement.ChargeCode, (ZString?)universalLine.ChargeCode.GetValueSafe(x => x.Code));
					if (!isChargeCodeSet)
					{
						SetValue(line, TransactionLineElement.GLAccount, universalLine.GLAccount.GetValueSafe(x => x.AccountCode));
					}
					line.GenericCharge = line.AL_AC.IsEmpty ? line.AL_AG : line.AL_AC;

					SetSubAccountCollection(line, universalLine, logger);

					foreach (AccTransactionLineSubAccount subAccount in line.SubAccounts)
					{
						subAccount.Validation.ValidateAL1_SubClassParentId();
						if (subAccount.HasErrors)
						{
							subAccount.NotificationsIncludingChildren.Where(x => x.Type == NotificationType.Error).ForEach(y => logger.Log(LogType.Error, y.Message));
						}
					}
				}
			}
		}

		void SetSubAccountCollection(InvoicingLineBase line, PostingJournal universalLine, IXmlImportLogger logger)
		{
			if (line.GLHeader != null && !line.GLHeader.SubAccountTypes.IsNullOrEmpty())
			{
				if (!universalLine.SubAccountCollection.IsNullOrEmpty())
				{
					foreach (SubAccount subAccount in universalLine.SubAccountCollection)
					{
						ImportSubAccount(line, subAccount, logger);
					}
				}
				//For XUT backward compatibility.
				else if (universalLine.SubAccount != null)
				{
					ImportSubAccount(line, universalLine.SubAccount, logger);
				}
			}
		}

		void ImportSubAccount(InvoicingLineBase line, SubAccount subAccount, IXmlImportLogger logger)
		{
			var type = subAccount.Type?.Code.ToString();
			var code = subAccount.Code.ToString();
			if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(code))
			{
				LogInvalid(logger);
				return;
			}

			var lineSubAccount = line.SubAccounts.OfType<TransactionLineSubAccount>().FirstOrDefault(x => x.SubAccountTypeDisplayCode.Equals(type));
			if (lineSubAccount == null)
			{
				LogInvalid(logger);
				return;
			}

			var subClassParentId = DataTransferSubAccountHelper.GetSubAccountPKFromCode(line.Factory, type, code);
			if (subClassParentId.IsEmpty)
			{
				LogInvalid(logger);
				return;
			}

			if (lineSubAccount.AL1_SubClassParentId.IsEmpty)
			{
				lineSubAccount.AL1_SubClassParentId = subClassParentId;
			}
			else if (lineSubAccount.AL1_SubClassParentId != subClassParentId)
			{
				LogDuplicate(logger);
			}
		}

		void SetInvoiceLinePlaceOfSupply(InvoicingLineBase line, PlaceOfSupply headerPlaceOfSupply, PlaceOfSupply linePlaceOfSupply)
		{
			if (PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(line.Company))
			{
				if (headerPlaceOfSupply != null
					&& (line.InvoiceBase?.NeedPlaceOfSupplyAtHeaderLevel ?? false))
				{
					SetValue(line, TransactionLineElement.PlaceOfSupply_Location, headerPlaceOfSupply.Location?.Code);
					SetValue(line, TransactionLineElement.PlaceOfSupply_LocationType, headerPlaceOfSupply.LocationType?.Code);
				}
				else if (linePlaceOfSupply != null)
				{
					SetValue(line, TransactionLineElement.PlaceOfSupply_Location, linePlaceOfSupply.Location?.Code);
					SetValue(line, TransactionLineElement.PlaceOfSupply_LocationType, linePlaceOfSupply.LocationType?.Code);
				}
			}
		}

		void SetExchangeRate(InvoicingLineBase line, PostingJournal universalLine)
		{
			ZString osCurrencyXmlValue;
			if (universalLine.OSCurrency.GetValueSafe(x => x.Code).TryGetValue(out osCurrencyXmlValue) && !osCurrencyXmlValue.IsEmpty)
			{
				line.ExchangeRate.Currency = osCurrencyXmlValue;
			}
		}

		void SetSequence(InvoicingLineBase line, PostingJournal universalLine)
		{
			ZInt sequenceXmlValue;
			if (universalLine.Sequence.TryGetValue(out sequenceXmlValue) && sequenceXmlValue > 0)
			{
				line.AL_Sequence = sequenceXmlValue > short.MaxValue ? (ZShort)short.MaxValue : (ZShort)(short)sequenceXmlValue;
			}
		}

		void SetTaxData(InvoicingLineBase line, PostingJournal universalLine, bool isTaxApplicable, LinesInfoProvider linesInfoProvider, bool isARImport)
		{
			if (isTaxApplicable)
			{
				if (SetValue(line, TransactionLineElement.VATTaxID, universalLine.VATTaxID.GetValueSafe(x => x.TaxCode)))
				{
					linesInfoProvider.SetTaxOverridden(line);
					SetValue(line, TransactionLineElement.TaxMessageID, universalLine.TaxMessageID.GetValueSafe(x => x.TaxMessageCode));
				}
				if (isARImport)
				{
					SetValue(line, TransactionLineElement.TaxDate, universalLine.TaxDate);
				}
				else
				{
					if (universalLine.TaxDate.HasValue)
					{
						SetValue(line, TransactionLineElement.TaxDate, universalLine.TaxDate);
					}
					else if (universalLine.CostSource == null || !universalLine.CostSource.GetValueSafe(x => x.Key).HasValue)
					{
						// if this line is not cost related, then workout the tax date based on registry setting for AP, i.e. based on the line's job type
						var operationalJob = line.InvoicingJob;
						if (operationalJob != null)
						{
							var taxDate = ZDate.Today;
							var taxDateOption = ((IPostingJob)line.Job).GetTaxDateDefaultingOptionForJob(line.Factory, LedgerTypes.AccountsPayable);
							if (taxDateOption != null)
							{
								var option = taxDateOption.TaxDateOption;
								if (option == TaxDateDefaultingOption.Code.InvoiceDate)
								{
									taxDate = line.InvoiceBase.AH_InvoiceDate.Date;
								}
								else if (option != TaxDateDefaultingOption.Code.Today)
								{
									var plugIn = line.InvoicingJob.GetInvoicingSupporter();
									if (plugIn != null)
									{
										var operationalDate = ZDate.Empty;
										var taxDateDescription = ZString.Empty;
										(operationalDate, taxDateDescription) = ((IPostingJob)operationalJob).GetTaxDateBasedOnRegistryDefaultingOption(plugIn, option, ZDate.Empty);
										taxDate = operationalDate;
									}
								}
							}
							SetValue(line, TransactionLineElement.TaxDate, (ZDate?)taxDate);
						}
						//else if no job recorded on the line, no need to do anything as it should already be defaulted to today's date for non-job related line.
					}
					else // else if this universal line is cost related and no tax date in it
					{
						// set tax date on invoice line to empty (by default it's today's date), so we can work it out later in InvoiceLinesToConsolCostConvertor's AddNewConsolCostToInvoiceAndSetValuesFromLine().
						line.AL_TaxDate = ZDate.Empty;
					}
				}
			}
			else
			{
				TransactionLineBuilder.ConvertToNotReportableTaxID(line);
			}

			if (line.SupportsInputTaxRecoverable && universalLine.RecoverableGSTVATPercentage.HasValue)
			{
				line.AL_Calc_InputGSTVATRecoverablePercentage = universalLine.RecoverableGSTVATPercentage.Value;
			}
		}

		void SetInvoiceLineAmounts(InvoicingLineBase line, PostingJournal universalLine, bool isCrossLedgerImport, bool isTaxApplicable, bool isARImport, LinesInfoProvider linesInfoProvider)
		{
			var localCurrency = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
			ZString localCurrencyCodeXmlValue;
			var isLocalValuesApplicable = line.AL_RX_NKTransactionCurrency != localCurrency &&
				universalLine.LocalCurrency.GetValueSafe(x => x.Code).TryGetValue(out localCurrencyCodeXmlValue) &&
				localCurrencyCodeXmlValue == localCurrency;

			if (isLocalValuesApplicable)
			{
				linesInfoProvider.SetLocalAmountApplicable(line);
			}

			int multiplier = line.InvoiceBase != null && line.InvoiceBase is APInvoice ? -1 : 1;

			if (isCrossLedgerImport)
			{
				multiplier *= -1;
			}

			var osExTaxAmount = multiplier * universalLine.OSAmount.GetValueOrDefault();
			var osTaxAmount = multiplier * universalLine.OSGSTVATAmount.GetValueOrDefault();
			var localExTaxAmount = multiplier * universalLine.LocalAmount.GetValueOrDefault();
			var localTaxAmount = multiplier * universalLine.LocalGSTVATAmount.GetValueOrDefault();
			var localExtraTaxAmount = multiplier * universalLine.LocalExtraVATAmount.GetValueOrDefault();

			if (isTaxApplicable)
			{
				SetLineExTaxAmounts(line, isLocalValuesApplicable, osExTaxAmount, localExTaxAmount);

				if (!isARImport)
				{
					line.AL_OSTaxAmount = osTaxAmount;
					if (line.TaxRate != null && line.TaxRate.IsLocalExtraTaxAmountValuePersistent)
					{
						line.AL_LocalExtraTaxAmount = localExtraTaxAmount;
					}
				}
			}
			else
			{
				SetLineExTaxAmounts(line, isLocalValuesApplicable, osExTaxAmount + osTaxAmount, localExTaxAmount + localTaxAmount);
			}
		}

		static void SetLineExTaxAmounts(InvoicingLineBase line, bool isLocalValuesApplicable, decimal osExTaxAmount, decimal localExTaxAmount)
		{
			line.AL_OSExTaxAmount = osExTaxAmount;
			if (isLocalValuesApplicable)
			{
				using (var osAmountRecalculationSuspender = new TransactionLine.OSAmountRecalculationSuspender(line))
				{
					line.AL_LocalExTaxAmount = localExTaxAmount;
				}
				line.CalculateHighPrecisionExchangeRate();
			}
		}

		ZString GetCodeValue(BusinessObject businessObj, Enum xmlFieldName, ZString value) => Helpers.GetCodeValue(businessObj, xmlFieldName, value, mapping);

		bool SetValue<T>(BusinessObject businessObj, Enum xmlFieldName, T? value)
			where T : struct, IZType
			=> Helpers.SetValue(businessObj, xmlFieldName, value, mapping, setWhenReadOnly: true, setWhenInvalid: false);

		void SetJob(BusinessObjectFactory factory, UniversalTransaction universalTransaction, PostingJournal universalLine, InvoicingLineBase line, HashSet<Job> jobsToDispose, bool isCrossLedgerImport, IXmlImportLogger logger)
		{
			var jobResult = GetOrCreateJob(factory, universalTransaction, universalLine, isCrossLedgerImport, true, jobsToDispose, logger);
			var job = jobResult.Item1;
			var errorMessages = jobResult.Item2;

			if (job != null)
			{
				line.AL_JH = job.PK;
			}
			else
			{
				line.UXml_JobErrorMessages = errorMessages;
			}
		}

		Tuple<Job, ZString> GetJob(BusinessObjectFactory factory, UniversalTransaction universalTransaction, PostingJournal universalLine, bool isCrossLedgerImport, IXmlImportLogger logger)
		{
			return GetOrCreateJob(factory, universalTransaction, universalLine, isCrossLedgerImport, false, null, logger);
		}

		Tuple<Job, ZString> GetOrCreateJob(BusinessObjectFactory factory, UniversalTransaction universalTransaction, PostingJournal universalLine, bool isCrossLedgerImport, bool createNewJobs, HashSet<Job> newJobsToDispose, IXmlImportLogger logger)
		{
			ZString jobNumber;
			if (universalLine.Job.GetValueSafe(x => x.Key).TryGetValue(out jobNumber) && !jobNumber.IsEmpty)
			{
				Job job = null;
				var shipmentCacheRecord = GetShipmentCacheRecord(universalTransaction, jobNumber, logger);
				var shipment = shipmentCacheRecord.Item1;
				var jobBranchCode = shipmentCacheRecord.Item2;
				var jobDepartmentCode = shipmentCacheRecord.Item3;
				var errorMessages = shipmentCacheRecord.Item4;
				if (shipment != null)
				{
					var shipmentInLineFactory = factory.Load(shipment.TablePrefix, shipment.PK);
					var parent = shipmentInLineFactory as IJobHeaderParent;
					if (parent != null)
					{
						var loader = new Job.Loader(parent);
						job = loader.Load(true);
						if (job == null && createNewJobs)
						{
							job = loader.TryCreateWithMutex();
							if (job == null)
							{
								errorMessages = AddErrorMessage(errorMessages, loader.GetJobCreationError());
							}
							else
							{
								newJobsToDispose.Add(job);
								if (!isCrossLedgerImport)
								{
									SetValue(job, JobElement.Branch, jobBranchCode);
									SetValue(job, JobElement.Department, jobDepartmentCode);
								}
								if (job.JH_GB.IsEmpty)
								{
									job.JH_GB = GlbBranch.CurrentBranch.PK;
								}

								if (job.JH_GE.IsEmpty)
								{
									job.JH_GE = GlbDepartment.CurrentDepartment.PK;
								}
							}
						}
					}
				}
				return Tuple.Create(job, errorMessages);
			}

			return Tuple.Create((Job)null, ZString.Empty);
		}

		Tuple<BusinessObject, ZString?, ZString?, ZString> GetShipmentCacheRecord(UniversalTransaction universalTransaction, ZString jobNumber, IXmlImportLogger logger)
		{
			Tuple<BusinessObject, ZString?, ZString?, ZString> shipmentCacheRecord;
			var importedShipmentKey = jobNumber;
			if (!importedShipments.TryGetValue(importedShipmentKey, out shipmentCacheRecord))
			{
				ZString importErrorMessages;
				var universalShipment = FindShipment(universalTransaction, jobNumber);
				if (universalShipment != null)
				{
					var universalShipmentType = universalShipment.DataContext.DataSourceCollection.FirstOrDefault().Type.GetValueOrDefault();

					universalShipment.DataContext.CodesMappedToTarget = universalTransaction.DataContext?.CodesMappedToTarget ?? false; //Required for Populating Additional References in Shipment matching process
					AddJobToWayBillNumberMapForCLDShipment(jobNumber, universalShipment);

					Func<BusinessObject> importShipment = () => ImportShipment(universalShipment, LineUniversalObjectFactory, out importErrorMessages, logger);
					var importedShipment = importShipment();
#if SafeTransitionFromIncorrectUsingOfDataContext
					if (importedShipment == null && !universalShipment.DataContext.CodesMappedToTarget)
					{
						universalShipment.DataContext.CodesMappedToTarget = true;
						importedShipment = importShipment();
						if (importedShipment != null)
						{
							ErrorReporter.ReportOnce("XUT_Import_GetShipment_CodesMappedToTarget", "A user may have their current import logic broken because a shipment is not found for an invoice. Minimum xml requirement was changed. They need to set destination data context to the message, so that CodesMappedToTarget will be set to true.");
						}
					}
#endif
					if (importedShipment == null)
					{
						importErrorMessages = AddErrorMessage(Res.GetString("569732E3-A1FD-4B2C-B6B5-BBDFDE157AFA", "Operational job {0} with {1} {2} '{3}' is not found.", universalShipmentType, "DataSource", "Key", jobNumber), importErrorMessages);
					}
					shipmentCacheRecord = Tuple.Create(importedShipment,
							universalShipment.JobCosting.GetValueSafe(x => x.Branch).GetValueSafe(x => x.Code),
							universalShipment.JobCosting.GetValueSafe(x => x.Department).GetValueSafe(x => x.Code),
							importErrorMessages);
				}
				else
				{
					importErrorMessages = AddErrorMessage(Res.GetString("29FA325B-D8D1-4121-AA05-06735DDD598D", "Can't find operational job with number '{0}'. XML should have operational job with {1} {2} element the same as line job number and correct {3}.", jobNumber, "DataSource", "Key", "Type"), importErrorMessages);
					shipmentCacheRecord = Tuple.Create<BusinessObject, ZString?, ZString?, ZString>(null, null, null, importErrorMessages);
				}
				importedShipments.Add(importedShipmentKey, shipmentCacheRecord);
			}

			return shipmentCacheRecord;
		}

		readonly Dictionary<ZString, ZString> wayBillNumberMapForCLDShipment = new Dictionary<ZString, ZString>();  // job number => way bill number
		void AddJobToWayBillNumberMapForCLDShipment(ZString jobNumber, UniversalShipment universalShipment)
		{
			if (universalShipment.ShipmentType != null
				&& universalShipment.ShipmentType.Code.GetValueOrDefault() == Constants.ShipmentTypes.CoLoadMaster
				&& !wayBillNumberMapForCLDShipment.ContainsKey(jobNumber)) //ShipmentCollection should contain shipment with distinct jobNumber, so this check might be redundant)
			{
				var wayBillNumber = universalShipment.WayBillNumber.GetValueOrDefault();
				if (!wayBillNumber.IsEmpty)
				{
					wayBillNumberMapForCLDShipment.Add(jobNumber, wayBillNumber);
				}
			}
		}

		TransactionAndLinesByConsols GenerateConsolCostingData(BusinessObjectFactory factory, UniversalTransaction universalTransaction, InvoicingBase transaction,
			Dictionary<PostingJournal, InvoicingLineBase> linesAdded, bool isCrossLedgerImport, IXmlImportLogger logger,
			Dictionary<ZString, Tuple<ForwardingConsol, List<InvoicingLineBase>>> nvoccLinesbyConsol = null)
		{
			IEnumerable<LinesForConsolCost> consolCostGroups = Array.Empty<LinesForConsolCost>();
			if (isCrossLedgerImport)
			{
				if (HasNVOCCConsolData())
				{
					List<LinesForConsolCost> nvoccConsolCostGroups = new List<LinesForConsolCost>();
					foreach (var x in nvoccLinesbyConsol)
					{
						var consolCostGroup = new LinesForConsolCost(x.Key, null, x.Value.Item2.ToArray());  //key is the consol Number, Value.Item2 has the invoice line list
						nvoccConsolCostGroups.Add(consolCostGroup);
					}
					consolCostGroups = nvoccConsolCostGroups;
				}
				else
				{
					ZString consolInvoiceNumber;
					if (universalTransaction.JobInvoiceNumber.TryGetValue(out consolInvoiceNumber) && universalTransaction.Job.GetValueSafe(x => x.Key).GetValueOrDefault().IsEmpty)
					{
						var consolNumber = InvoicingBase.GetConsolNumberFromConsolidatedInvoiceRef(consolInvoiceNumber);
						consolCostGroups = new[] { new LinesForConsolCost(consolNumber, null, transaction.Lines.ToArray<InvoicingLineBase>()) };
					}
				}
			}
			else
			{
				consolCostGroups = GenerateConsolCostGroupsFromLines(linesAdded);
			}

			var linesForJobCostingPlugIns = new List<LinesByConsols>();
			foreach (var consolCostGroup in consolCostGroups)
			{
				BusinessObject importedConsol = null;
				BusinessObject consolInRequiredFactory = null;
				ZString importErrorMessages;

				if (HasNVOCCConsolData())
				{
					consolInRequiredFactory = nvoccLinesbyConsol[consolCostGroup.ConsolID].Item1;
				}
				else
				{
					var universalConsol = FindConsol(universalTransaction, consolCostGroup.ConsolID, consolCostGroup.ConsolType);
					if (universalConsol != null)
					{
						importedConsol = ImportShipment(universalConsol, LineUniversalObjectFactory, out importErrorMessages, logger);
						var universalConsolType = universalConsol.DataContext.DataSourceCollection.FirstOrDefault().Type.GetValueOrDefault();
						if (importedConsol == null)
						{
							importErrorMessages = AddErrorMessage(Res.GetString("689172DC-C481-4EAA-AF8B-57580DF3F59E", "Consolidation {0} with {1} {2} '{3}' is not found.", universalConsolType, "DataSource", "Key", consolCostGroup.ConsolID), importErrorMessages);
						}
					}
					else
					{
						if (isCrossLedgerImport)
						{
							importErrorMessages = AddErrorMessage(Res.GetString("2EB6DF62-EFFB-42A7-8D85-639E311DBFA5", "Can't find consolidation with number '{0}'. XML should have consolidation with {1} {2} element the same as in invoice {4} and correct {3}.", consolCostGroup.ConsolID, "DataSource", "Key", "Type", "JobInvoiceNumber"), importErrorMessages);
						}
						else
						{
							importErrorMessages = AddErrorMessage(Res.GetString("136B6159-7182-45CA-877E-B9EBB9F01F98", "Can't find consolidation {0} with number '{1}'. XML should have consolidation {0} with {2} {3} and {4} elements the same as line {5}.", consolCostGroup.ConsolType, consolCostGroup.ConsolID, "DataSource", "Key", "Type", "CostSource"), importErrorMessages);
						}
					}

					consolInRequiredFactory = importedConsol != null ? factory.Load(importedConsol.TablePrefix, importedConsol.PK) : null;
				}

				var consolInRequiredFactoryAsPlugin = consolInRequiredFactory as IJobCostingPlugIn;
				if (consolInRequiredFactoryAsPlugin != null)
				{
					linesForJobCostingPlugIns.Add(new LinesByConsols(consolInRequiredFactoryAsPlugin, consolCostGroup.Lines));
				}
				else
				{
					if (consolInRequiredFactory != null)
					{
						importErrorMessages = AddErrorMessage(Res.GetString("9B9BAFC7-0073-465A-B9E9-1173088B3725", "Found {0} with number '{1}' is not a consolidation and can't be used for consol costing.", consolCostGroup.ConsolType, consolCostGroup.ConsolID), importErrorMessages);
					}

					consolCostGroup.Lines.ForEach(x => x.UXml_ConsolErrorMessages = importErrorMessages);
				}
			}

			return new TransactionAndLinesByConsols(factory, transaction, linesForJobCostingPlugIns, isCrossLedgerImport);

			bool HasNVOCCConsolData() => nvoccLinesbyConsol != null && nvoccLinesbyConsol.Any();
		}

		static void CreateConsolCostsOrGetJobCostingPlugIn(TransactionAndLinesByConsols transactionAndLinesByConsols, LinesInfoProvider linesInfoProvider, HashSet<Job> jobsToDispose)
		{
			var linesByConsolsToCreateConsolCostsByLineGroups = new List<(IJobCostingPlugIn consol, IEnumerable<InvoicingLineBase> invoiceLines)>();
			var linesByConsolsToCreateConsolCostsBySingleLine = new List<(IJobCostingPlugIn consol, InvoicingLineBase invoiceLine)>();
			foreach (var linesByConsol in transactionAndLinesByConsols.LinesByConsols)
			{
				linesByConsolsToCreateConsolCostsByLineGroups.Add(ValueTuple.Create(linesByConsol.JobCostingPlugIn, linesByConsol.Lines.Where(x => x.AL_JH.IsValid)));
				foreach (var line in linesByConsol.Lines.Where(x => x.AL_JH.IsEmpty))
				{
					linesByConsolsToCreateConsolCostsBySingleLine.Add(ValueTuple.Create(linesByConsol.JobCostingPlugIn, line));
				}
			}

			InvoiceLinesToConsolCostConvertor.ConvertToConsolCostRelatedLines(transactionAndLinesByConsols.Factory, transactionAndLinesByConsols.Transaction, transactionAndLinesByConsols.IsCrossLedgerImport, linesByConsolsToCreateConsolCostsByLineGroups.ToArray(), linesInfoProvider, jobsToDispose);
			InvoiceLinesToConsolCostConvertor.CreateSeparateConsolCostBasedOnEachLine(transactionAndLinesByConsols.Transaction, linesByConsolsToCreateConsolCostsBySingleLine.ToArray(), linesInfoProvider, jobsToDispose);
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		static IEnumerable<LinesForConsolCost> GenerateConsolCostGroupsFromLines(Dictionary<PostingJournal, InvoicingLineBase> linesAdded)
		{
			return from lineAdded in linesAdded
				   let xmlLine = lineAdded.Key
				   let invoiceLine = lineAdded.Value
				   let consolID = xmlLine.CostSource.GetValueSafe(x => x.Key).GetValueOrDefault()
				   let consolType = xmlLine.CostSource.GetValueSafe(x => x.Type).GetValueOrDefault()
				   let isSingleLineBasedConsolCost = xmlLine.Job == null
				   where !consolID.IsEmpty && !consolType.IsEmpty && (invoiceLine.AL_JH.IsValid || isSingleLineBasedConsolCost)
				   group invoiceLine by new { consolID, consolType } into g
				   select new LinesForConsolCost(g.Key.consolID, g.Key.consolType.ToString(), g.ToArray());
		}

		static OrgAddress GetOrgAddress(UniversalObjectFactory factoryCasted, UniversalTransaction universalTransaction, IXmlImportLogger logger) => TxnHeaderBuilder.GetOrgAddress(factoryCasted, universalTransaction.OrganizationAddress, logger);

		static (ZCodeMappedZString? sourceOrgMappedCode, IOrgHeader sourceOrg) GetSourceOrganizationForCrossLedgerImport(UniversalTransaction universalTransaction, UniversalObjectFactory factoryCasted, IXmlImportLogger logger, bool useCodeMapping)
		{
			ZCodeMappedZString? sourceOrgMappedCode = null;
			IOrgHeader sourceOrg = null;
			if (!universalTransaction.DataContext.GetValueSafe(x => x.DataProviderForCodeMapping).IsEmpty)
			{
				var mappedCode = new ZCodeMappedZString(universalTransaction.DataContext.DataProviderForCodeMapping);
				sourceOrg = universalTransaction.DataContext.GetSourceOrganisation(logger, factoryCasted.BOFactory);
				if (useCodeMapping && sourceOrg != null)
				{
					mappedCode.MappedValue = sourceOrg.OH_Code;
				}
				sourceOrgMappedCode = mappedCode;
			}
			if (sourceOrg == null)
			{
				sourceOrg = TxnHeaderBuilder.GetOrgAddress(factoryCasted, universalTransaction.BranchAddress, logger)?.Header;
#if SafeTransitionFromIncorrectUsingOfDataContext
				if (sourceOrg == null && !universalTransaction.DataContext.GetValueSafe(x => x.CompanyCodeToImportInto).IsEmpty)
				{
					sourceOrg = factoryCasted.BOFactory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, universalTransaction.DataContext.CompanyCodeToImportInto)?.OrgProxy;
					if (sourceOrg != null)
					{
						ReportBranchAddressMissing(logger);
					}
				}
#endif
			}

			return (sourceOrgMappedCode, sourceOrg);
		}

		void TrySetMappedValueDirectlyFromSourceCode(UniversalTransaction universalTransaction)
		{
			var logger = new DummyLogger();
			if (universalTransaction.OrganizationAddress != null)
			{
				var organizationCode = universalTransaction.OrganizationAddress.OrganizationCode.GetValueOrDefault();

				if (!organizationCode.SourceValue.IsEmpty && !organizationCode.IsMapped)
				{
					var addressBO = GetOrgAddress(LineUniversalObjectFactory, universalTransaction, logger);
					if (addressBO?.Header != null)
					{
						organizationCode.MappedValue = addressBO.Header.OH_Code;
						universalTransaction.OrganizationAddress.OrganizationCode = organizationCode;
					}
				}
			}
		}

		void TrySetMappedValueDirectlyFromSourceCode(PostingJournal universalLine, InvoicingLineBase line)
		{
			if (universalLine.ChargeCode != null && universalLine.ChargeCode.Code.HasValue)
			{
				var universalChargeCode = universalLine.ChargeCode.Code.Value;
				if (!universalChargeCode.SourceValue.IsEmpty && !universalChargeCode.IsMapped)
				{
					var mappedValue = GetCodeValue(line, TransactionLineElement.ChargeCode, universalChargeCode.SourceValue);
					if (!mappedValue.IsEmpty)
					{
						universalChargeCode.MappedValue = mappedValue;
						universalLine.ChargeCode.Code = universalChargeCode;
					}
				}
			}
		}

		static UniversalShipment FindShipment(UniversalTransaction universalTransaction, ZString jobNumber) => universalTransaction.ShipmentCollection.GetExactDataObject(jobNumber);

		static UniversalShipment FindConsol(UniversalTransaction universalTransaction, string consolID, string consolType) => universalTransaction.ShipmentCollection.GetExactDataObject(consolID, consolType);

		static ZString AddErrorMessage(ZString firstErrorMessage, ZString secondErrorMessage)
		{
			if (firstErrorMessage.IsEmpty)
			{
				return secondErrorMessage;
			}

			if (secondErrorMessage.IsEmpty)
			{
				return firstErrorMessage;
			}

			return firstErrorMessage + System.Environment.NewLine + secondErrorMessage;
		}

		UniversalObjectFactory LineUniversalObjectFactory => lineUniversalObjectFactory ?? (lineUniversalObjectFactory = new UniversalObjectFactory());
		UniversalObjectFactory lineUniversalObjectFactory;

		IDisposable InitializeCache() => new DisposableAction(
			() => importedShipments = new Dictionary<ZString, Tuple<BusinessObject, ZString?, ZString?, ZString>>(),
			() => importedShipments = null);
		Dictionary<ZString, Tuple<BusinessObject, ZString?, ZString?, ZString>> importedShipments;

		static BusinessObject ImportShipment(UniversalShipment dataObject, UniversalObjectFactory factory, out ZString errors, IXmlImportLogger logger)
		{
			var type = dataObject.DataContext.DataSourceCollection.FirstOrDefault().Type.GetValueOrDefault();

			BusinessObject shipment = null;
			DataContextType dataContextType;
			IShipmentDataContextManager contextManager;

			if (TryGetShipmentDataContextAndManager(dataObject.DataContext.DataSourceCollection, logger, out contextManager, out dataContextType))
			{
				if (contextManager != null && contextManager.ManagesShipments)
				{
					try
					{
						shipment = contextManager.FindExistingBusinessObjectForIncomingShipment(dataObject, logger, factory);
					}
					catch (DataObjectReadFailureException ex)
					{
						logger.Log(LogType.Error, ex.Message);
					}
				}
				else
				{
					if (contextManager == null)
					{
						ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Can't get DataContextManager for DataContextType {0}", dataContextType));
					}
					else if (contextManager.ManagesShipments)
					{
						ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "DataContextManager '{0}' for DataContextType '{1}' doesn't ManagesShipments", contextManager.GetType(), dataContextType));
					}
				}
			}

			errors = ZString.Empty;
			if (logger.HasErrors())
			{
				var errorMessages = from log in logger.Logs
									where log.Type == LogType.Error
									select log.Message;
				errors = new ZStringBuilder(errorMessages).ToStringWithNewLineBetweenAppends();
			}

			return shipment;
		}

		[SuppressMessage("CargoWiseOne", "CW1135: DoNotUseCountrySpecificBusinessRule", Justification = "Testing")]
		static bool IsTaxApplicable(AccTransactionHeader transaction, UniversalTransaction sourceUniversalTransaction, IXmlImportLogger logger)
		{
			var sourceCountyCode = sourceUniversalTransaction.BranchAddress?.Country?.Code ?? ZString.Empty;
#if SafeTransitionFromIncorrectUsingOfDataContext
			if (sourceCountyCode.IsEmpty)
			{
				sourceCountyCode = sourceUniversalTransaction.DataContext.GetValueSafe(x => x.CountryCodeToImportInto);
				if (!sourceCountyCode.IsEmpty)
				{
					ReportBranchAddressMissing(logger);
				}
			}
#endif

			return TransactionHeaderBuilder.IsTaxApplicable(transaction) && sourceCountyCode == transaction.Company.GC_RN_NKCountryCode;
		}

		static void ReportBranchAddressMissing(IXmlImportLogger logger)
		{
			var msg = Res.GetString("449a36cf-7deb-416b-a825-be056ba850b4", "Transaction Header Branch Address is missing or incomplete. You must at-least populate {0} and {1} when importing XML Universal Transactions.", "BranchAddress/AddressType", "BranchAddress/Country/Code");
			if (!(logger.Logs?.Any(x => x.Type == LogType.Warning && x.Message.Contains(msg)) ?? false))
			{
				logger.LogBoth(LogType.Warning, msg);
			}
		}

		static ZString ReduceXMLSizeByRemovingElementsNotUsedInFuture(UniversalTransaction universalTransaction, ZString sourceXML)
		{
			ZString resultXML = sourceXML;
			bool hasAttachmentsToRemove = universalTransaction.AttachedDocumentCollection != null && universalTransaction.AttachedDocumentCollection.Count > 0;
			if (hasAttachmentsToRemove)
			{
				string namespaceUsed;
				using (var transactionDataObject = ImportUniversalTransactionFromXmlCore(sourceXML, false, null, false, out namespaceUsed).Item1)
				{
					foreach (var document in transactionDataObject.AttachedDocumentCollection)
					{
						document.Dispose();
					}

					transactionDataObject.SetAttachedDocumentCollection(() => null);
					resultXML = SerializeDataObjectToXML(transactionDataObject, namespaceUsed);
				}
			}

			return resultXML;
		}

		static ZString SerializeDataObjectToXML(UniversalTransaction dataObject, string nameSpace)
		{
			ZString dataObjectXML;
			SubStreamableStream stream = null;

			//Use Try-Finally block to dispose the stream instead of nested using scopes because we get a Code warning (CA2202: Do not dispose objects multiple times) as the inner scope will dispose the outer stream object
			try
			{
				stream = new CargoWise.IO.Shim.SubStreamableStream();

				new XmlWriter().WriteXML(dataObject, stream, nameSpace);
				stream.Flush();
				stream.Position = 0;
				using (var reader = new StreamReader(stream))
				{
					stream = null;
					dataObjectXML = reader.ReadToEnd();
				}
			}
			finally
			{
				if (stream != null)
				{
					stream.Dispose();
				}
			}

			return dataObjectXML;
		}

		static void LogRowWarnings(IXmlImportLogger logger, TransactionPendingAllocation transaction)
		{
			foreach (var notification in transaction.RowWarnings)
			{
				logger.LogBoth(LogType.Warning, notification.Message);
			}
		}

		void InitializeMapping()
		{
			mapping = new Dictionary<Enum, SchemaColumn>
			{
				{ TransactionHeaderElement.TransactionDate, AccTransactionHeaderSchema.AH_InvoiceDate },
				{ TransactionHeaderElement.DocumentReceivedDate, AccTransactionHeaderSchema.AH_DocumentReceivedDate },
				{ TransactionHeaderElement.PostDate, AccTransactionHeaderSchema.AH_PostDate },
				{ TransactionHeaderElement.Number, AccTransactionHeaderSchema.AH_TransactionNum },
				{ TransactionHeaderElement.ComplianceSubType, AccTransactionHeaderSchema.AH_ComplianceSubType },
				{ TransactionHeaderElement.GovernmentAllocatedID, AccTransactionHeaderSchema.AH_GovernmentAllocatedID },
				{ TransactionHeaderElement.DueDate, AccTransactionHeaderSchema.AH_DueDate },
				{ TransactionHeaderElement.Description, AccTransactionHeaderSchema.AH_Desc },
				{ TransactionHeaderElement.Branch, AccTransactionHeaderSchema.AH_GB },
				{ TransactionHeaderElement.Department, AccTransactionHeaderSchema.AH_GE },
				{ TransactionHeaderElement.ChequeOrReference, AccTransactionHeaderSchema.AH_ChequeOrReference },
				{ TransactionHeaderElement.PlaceOfSupply_Location, AccTransactionHeaderSchema.AH_PlaceOfSupply },
				{ TransactionHeaderElement.PlaceOfSupply_LocationType, AccTransactionHeaderSchema.AH_PlaceOfSupplyType },
				{ TransactionHeaderElement.GLAccount, AccTransactionHeaderSchema.AH_AG },
				{ TransactionHeaderElement.AgreedPaymentMethod, AccTransactionHeaderSchema.AH_AgreedPaymentMethodOverride },
				{ TransactionLineElement.Branch, AccTransactionLinesSchema.AL_GB },
				{ TransactionLineElement.ChargeCode, AccTransactionLinesSchema.AL_AC },
				{ TransactionLineElement.Department, AccTransactionLinesSchema.AL_GE },
				{ TransactionLineElement.GLAccount, AccTransactionLinesSchema.AL_AG },
				{ TransactionLineElement.Description, AccTransactionLinesSchema.AL_Desc },
				{ TransactionLineElement.IsFinalCharge, AccTransactionLinesSchema.AL_IsFinalCharge },
				{ TransactionLineElement.VATTaxID, AccTransactionLinesSchema.AL_AT },
				{ TransactionLineElement.TaxMessageID, AccTransactionLinesSchema.AL_A9_VATClass },
				{ TransactionLineElement.WithholdingTaxID, AccTransactionLinesSchema.AL_AW },
				{ TransactionLineElement.GovtChargeCode, AccTransactionLinesSchema.AL_GovtChargeCode },
				{ TransactionLineElement.TaxDate, AccTransactionLinesSchema.AL_TaxDate },
				{ TransactionLineElement.PlaceOfSupply_Location, AccTransactionLinesSchema.AL_PlaceOfSupply },
				{ TransactionLineElement.PlaceOfSupply_LocationType, AccTransactionLinesSchema.AL_PlaceOfSupplyType },
				{ JobElement.Branch, JobHeaderSchema.JH_GB },
				{ JobElement.Department, JobHeaderSchema.JH_GE },
				{ TransactionLineElement.SupplyType, AccTransactionLinesSchema.AL_SupplyType },
			};
		}

		Dictionary<Enum, SchemaColumn> mapping;

		enum TransactionHeaderElement
		{
			TransactionDate,
			PostDate,
			Number,
			DueDate,
			Description,
			Branch,
			Department,
			ChequeOrReference,
			PlaceOfSupply_Location,
			PlaceOfSupply_LocationType,
			DocumentReceivedDate,
			GovernmentAllocatedID,
			ComplianceSubType,
			GLAccount,
			AgreedPaymentMethod
		}

		enum TransactionLineElement
		{
			Branch,
			Department,
			ChargeCode,
			GLAccount,
			Description,
			IsFinalCharge,
			VATTaxID,
			WithholdingTaxID,
			TaxMessageID,
			GovtChargeCode,
			TaxDate,
			PlaceOfSupply_Location,
			PlaceOfSupply_LocationType,
			SupplyType
		}

		enum JobElement
		{
			Branch,
			Department,
		}

		class LinesForConsolCost
		{
			public LinesForConsolCost(string consolID, string consolType, InvoicingLineBase[] lines)
			{
				ConsolID = consolID;
				ConsolType = consolType;
				Lines = lines;
			}

			public string ConsolID { get; }
			public string ConsolType { get; }
			public InvoicingLineBase[] Lines { get; }
		}

		class LinesByConsols
		{
			public LinesByConsols(IJobCostingPlugIn jobCostingPlugIn, InvoicingLineBase[] lines)
			{
				JobCostingPlugIn = jobCostingPlugIn;
				Lines = lines;
			}

			public IJobCostingPlugIn JobCostingPlugIn { get; }
			public InvoicingLineBase[] Lines { get; }
		}

		class TransactionAndLinesByConsols
		{
			public TransactionAndLinesByConsols(BusinessObjectFactory factory, InvoicingBase transaction, List<LinesByConsols> linesByConsols, bool isCrossLedgerImport)
			{
				Factory = factory;
				Transaction = transaction;
				LinesByConsols = linesByConsols;
				IsCrossLedgerImport = isCrossLedgerImport;
			}

			public BusinessObjectFactory Factory { get; }
			public InvoicingBase Transaction { get; }
			public List<LinesByConsols> LinesByConsols { get; }
			public bool IsCrossLedgerImport { get; }
		}
	}
}
