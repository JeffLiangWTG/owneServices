using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.Accounting.DataTransfer.XmlMapping;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public class TransactionBuilderConfig
	{
		public bool RunExtraValidation = true;
		public bool CrossLedgerImport;
		public bool SetBranch = true;
		public bool SetDepartment = true;
		public bool SetOrganisation = true;
		public bool SetLineDescription = true;
		public bool SetReceiptPaymentType = true;
		public bool AllowResetChargeCodeAndJobOnError = true;
		public bool UseConsolOrJobNumberToMatchJob = true;
		public bool UseForeignChargeAmountWhenPostingLocalCurrencyInvoices;
		public bool PreserveExchangeRateFromSourceTransaction;
		public bool UseChargeDescAsLineDesc;
	}

	public class TransactionHeaderBuilder
	{
		public TransactionHeaderBuilder(INotificationManager notifier, TransactionBuilderConfig config)
		{
			this.NotificationManager = notifier;
			this.Config = config;
		}

		public TransactionHeaderBuilder(NotificationManager notifier)
			: this(notifier, new TransactionBuilderConfig())
		{
		}

		public void SetValuesOnInvoiceBusinessObject(InvoicingBase invoice, Xsd.TxnHeader xmlInvoiceHeader, IValueObjectImportContext context, ZString errorContext)
		{
			using (invoice.GetValidationSuspender())
			{
				if (Config.SetOrganisation)
				{
					SetOrganisation(invoice, xmlInvoiceHeader, context, errorContext);
				}
				if (Config.SetBranch)
				{
					SetHeaderBranch(invoice, xmlInvoiceHeader, errorContext);
				}
				if (Config.SetDepartment)
				{
					SetHeaderDepartment(invoice, xmlInvoiceHeader, errorContext);
				}
				SetBasicFieldsForInvoice(invoice, xmlInvoiceHeader, context, errorContext);
				SetCurrencyDetails(invoice, xmlInvoiceHeader, context, errorContext);

				if (xmlInvoiceHeader.TxnOverrideAddressSpecified)
				{
					SetOverriddenAddress(invoice, xmlInvoiceHeader, context, errorContext);
				}

				if (xmlInvoiceHeader.TxnOverrideContactSpecified)
				{
					SetOverriddenContact(invoice, xmlInvoiceHeader, context, errorContext);
				}

				ExtractPdfsAndAttachToEDocs(invoice, xmlInvoiceHeader);

				if (invoice.AH_TransactionType == TransactionTypes.Invoice && ShouldPopulateCashReceiptOrPaymentDetails(xmlInvoiceHeader))
				{
					SetReceiptPaymentDetailsForCashInvoice((Invoice)invoice, xmlInvoiceHeader, context, errorContext);
				}

				using (invoice.Lines.SuspendListChanged())
				using (invoice.BeginImportingManyConsoleCosts())
				{
					for (int currentLine = 0; currentLine < xmlInvoiceHeader.TxnLines.Count; currentLine++)
					{
						string lineErrorContext = errorContext + Res.GetString("3f72a2eb-842b-4019-8a87-34363c077342", "Line {0}:", currentLine + 1) + " ";

						Xsd.TxnLine xmlInvoiceLine = xmlInvoiceHeader.TxnLines[currentLine];
						xmlInvoiceLine.OverrideSystemExchangeRate = xmlInvoiceHeader.OverrideSystemExchangeRate;

						if (IsApportionmentXmlInvoiceLine(invoice, xmlInvoiceLine))
						{
							ApportionmentBuilder builder = GetApportionmentBuilder(NotificationManager, Config);
							builder.AddApportionmentToInvoiceBusinessObject(invoice, xmlInvoiceLine, context, lineErrorContext);
						}
						else
						{
							TransactionLineBuilder builder = GetInvoiceLineBuilder(NotificationManager, Config);
							builder.AddTransactionLineToInvoiceBusinessObject(invoice, xmlInvoiceLine, context, lineErrorContext);
						}
					}
				}

				if (AccountingConfigurationRegistry.Instance.CombineInvoiceLinesByChargeCode.Value)
				{
					CombineInvoiceLinesByChargeCode(invoice);
				}

				if (Config.SetBranch && Env.CurrentUser.IsBatchProcessor && invoice.Lines.Count > 0)
				{
					if (xmlInvoiceHeader.Branch.IsEmpty)
					{
						if (invoice.LinesHaveSameBranch)
						{
							invoice.AH_GB = invoice.Lines[0].AL_GB;
							ZString branchCode = invoice.Branch != null ? invoice.Branch.GB_Code : ZString.Empty;
							NotificationManager.AddInfoNotification(Res.GetString("db24b82d-14b5-4411-84e3-141727786b62", "{0}: Header branch has been set to '{1}' because a header branch was not specified and all lines have the same branch.", errorContext, branchCode));
						}
						else
						{
							NotificationManager.AddErrorToNotifications(Res.GetString("bdd69374-8adf-4f9a-ad86-ee6b90e45501", "{0}: Header branch was not specified and could not be defaulted from lines because the lines cover multiple branches. Please specify a branch for the header.", errorContext));
						}
					}

					if (xmlInvoiceHeader.Department.IsEmpty)
					{
						if (invoice.LinesHaveSameDepartment)
						{
							invoice.AH_GE = invoice.Lines[0].AL_GE;
							ZString departmentCode = invoice.Department != null ? invoice.Department.GE_Code : ZString.Empty;
							NotificationManager.AddInfoNotification(Res.GetString("da99955d-9602-4cb8-b680-50909aeb3b6e", "{0}: Header department has been set to '{1}' because a header department was not specified and all lines have the same department", errorContext, departmentCode));
						}
						else
						{
							NotificationManager.AddErrorToNotifications(Res.GetString("99c23b5a-3ab7-4d69-9c18-b2d496cfc376", "{0}: Header department was not specified and could not be defaulted from lines because the lines cover multiple departments. Please specify a department for the header.", errorContext));
						}
					}
				}

				invoice.SetComplianceSubTypeIfIsNecessary();

				if (SystemDataRegistry.Instance.DuplicatePaymentReferenceValidationOnImportingInvoices.Value != Enterprise.Core.Constants.AllowDuplicatePaymentReferenceHandlerMethods.Codes.AllowDuplicates &&
					invoice.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsReceivable &&
					(invoice.AH_TransactionType == ZArchitecture.Core.TransactionTypes.Invoice ||
						invoice.AH_TransactionType == ZArchitecture.Core.TransactionTypes.CreditNote ||
							invoice.AH_TransactionType == ZArchitecture.Core.TransactionTypes.AdjustmentNote))
				{
					CheckForDuplicateTransactionNumbers(invoice, xmlInvoiceHeader.PaymentReference, SystemDataRegistry.Instance.DuplicatePaymentReferenceValidationOnImportingInvoices.Value);
				}

				if (AccountingConfigurationRegistry.Instance.DisallowPostingTransactionWithEmptyComplianceSubtype.Value && invoice.AH_ComplianceSubType.IsEmpty && invoice.GetMatchingComplianceSubType().IsEmpty)
				{
					NotificationManager.AddErrorToNotifications(string.Format(CultureInfo.CurrentCulture, "{0}: ", errorContext) + ComplianceSequenceNumberAllocationErrorMessages.EmptyComplianceSubTypeExceptionMessage);
				}
			}
			using (invoice.SuspendCreditLimitCheck())
			{
				invoice.RunPreSaveValidation();

				if (invoice.Factory.HasContext(BusinessContext.ShouldTraceExchangeRateError) && !invoice.HasErrors)
				{
					invoice.SetContext(BusinessContext.ShouldTraceExchangeRateError);
				}
			}
		}

		void SetOverriddenContact(InvoicingBase invoice, Xsd.TxnHeader xmlInvoiceHeader, IValueObjectImportContext context, ZString errorContext)
		{
			Xsd.ContactReference contactReference = new Xsd.ContactReference();
			contactReference.Organisation = xmlInvoiceHeader.DebtorOrCreditor;
			contactReference.Organisation.OrganisationDetails.Contacts.Clear();
			contactReference.Organisation.OrganisationDetails.Contacts.Add(xmlInvoiceHeader.TxnOverrideContact);
			contactReference.ContactSequenceRef = 1;

			ContactValueObjectHelper contactHelper = new ContactValueObjectHelper(errorContext);
			invoice.AH_OC_InvoiceContactOverride = contactHelper.FromContactReferenceGetContactPK(contactReference, context);

			if (invoice.AH_OC_InvoiceContactOverride.IsEmpty)
			{
				NotificationManager.AddWarningToNotifications(Res.GetString("bc6a87af-efe7-47e7-8885-ef33271da8dc", "Could not match Transaction override contact."));
			}
		}

		void SetOverriddenAddress(InvoicingBase invoice, Xsd.TxnHeader xmlInvoiceHeader, IValueObjectImportContext context, ZString errorContext)
		{
			Xsd.AddressReference addressReference = new Xsd.AddressReference();
			addressReference.Organisation = xmlInvoiceHeader.DebtorOrCreditor;
			addressReference.Organisation.OrganisationDetails.Addresses.Clear();
			addressReference.Organisation.OrganisationDetails.Addresses.Add(xmlInvoiceHeader.TxnOverrideAddress);
			addressReference.AddressSequenceRef = 1;

			AddressValueObjectHelper addressHelper = new AddressValueObjectHelper(errorContext);
			invoice.AH_OA_InvoiceAddressOverride = addressHelper.FromAddressReferenceGetAddressPK(addressReference, context);

			if (invoice.AH_OA_InvoiceAddressOverride.IsEmpty)
			{
				NotificationManager.AddWarningToNotifications(Res.GetString("c1daf438-9fd9-470c-91f6-ac12920e7922", "Could not match Transaction override address."));
			}
		}

		void SetReceiptPaymentDetailsForCashInvoice(Invoice invoice, Xsd.TxnHeader xmlInvoiceHeader, IValueObjectImportContext context, ZString errorContext)
		{
			bool originalValueForSubmittedFromInvoicingForm = invoice.SubmittedFromInvoicingForm;
			invoice.SubmittedFromInvoicingForm = true;
			invoice.IsInvoiceReceiptPayment = true;
			invoice.IsReceiptPaymentFromFileImport = true;
			invoice.SubmittedFromInvoicingForm = originalValueForSubmittedFromInvoicingForm;

			invoice.ReceiptPaymentAH_ReceiptType = xmlInvoiceHeader.ReceiptPaymentTypeSpecified ?
					TxnHeaderReceiptPaymentTypeXmlMapping.Instance.GetEnterpriseCode(xmlInvoiceHeader.ReceiptPaymentType, errorContext, NotificationManager.NotificationSubscriber) : "";

			AccBankAccount bank = BusinessObjectRetriever.GetBankFromBankCode(invoice.Factory, xmlInvoiceHeader.BankCode);
			invoice.ReceiptPaymentAH_AB = bank != null ? bank.PK : ZGuid.Empty;

			if (invoice.ReceiptPaymentAH_ReceiptType != ReceiptTypes.Cheque)
			{
				ZString chequeOrReference = !xmlInvoiceHeader.ChequeOrReference.IsEmpty ? xmlInvoiceHeader.ChequeOrReference : xmlInvoiceHeader.TxnNumber;
				context.SetPropertyInfoValueIfValueNotEmpty(invoice.ReceiptPaymentAH_ChequeOrReferenceInfo, chequeOrReference);
			}

			if (invoice.ReceiptPaymentAH_ReceiptType == ReceiptTypes.Cheque)
			{
				if (invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					context.SetPropertyInfoValueIfValueNotEmpty(invoice.ReceiptPaymentAH_ChequeOrReferenceInfo, xmlInvoiceHeader.ChequeOrReference);
					context.SetPropertyInfoValueIfValueNotEmpty(invoice.ReceiptPaymentAH_ChequeDrawerInfo, xmlInvoiceHeader.ChequeDrawer);
					context.SetPropertyInfoValueIfValueNotEmpty(invoice.ReceiptPaymentAH_DrawerBankInfo, xmlInvoiceHeader.DrawerBank);
					context.SetPropertyInfoValueIfValueNotEmpty(invoice.ReceiptPaymentAH_DrawerBranchInfo, xmlInvoiceHeader.DrawerBankBranch);
				}
				else if (invoice.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					APInvoice invoiceAsAPInvoice = invoice as APInvoice;

					AccChequeBook chequeBook = BusinessObjectRetriever.GetChequeBookFromChequeBookCode(invoiceAsAPInvoice.Factory, xmlInvoiceHeader.ChequeBook);
					invoiceAsAPInvoice.ReceiptPaymentAK_AB = chequeBook != null ? chequeBook.PK : ZGuid.Empty;

					int chequeDigits = invoiceAsAPInvoice.ReceiptPaymentBankAccount != null ? invoiceAsAPInvoice.ReceiptPaymentBankAccount.AB_ChequeNumDigits : 0;
					ZString chequeNumberForHotChequeSearch = xmlInvoiceHeader.ChequeOrReference.PadLeft(chequeDigits, '0');
					AccHotCheque hotCheque = invoiceAsAPInvoice.GetActiveHotCheques().ToArray<AccHotCheque>().FirstOrDefault(x => x.AQ_ChequeNumber == chequeNumberForHotChequeSearch);

					if (hotCheque != null)
					{
						invoiceAsAPInvoice.ImportSelectedHotCheque(hotCheque);
					}

					if (chequeBook != null && chequeBook.AK_AutoPrintCheque)
					{
						NotificationManager.AddErrorToNotifications(Res.GetString("2d8dc7b4-c1fb-4f35-92ac-121ad3b4f07f", "You cannot import an Invoice with Payment details when using a Cheque Book where cheques are auto-printed."));
					}
					else if (!invoice.IsChequeNumberAutoAllocated)
					{
						context.SetPropertyInfoValueIfValueNotEmpty(invoice.ReceiptPaymentAH_ChequeOrReferenceInfo, xmlInvoiceHeader.ChequeOrReference);
					}
				}
			}
		}

		void SetPaymentOverriddenAddressAndOverriddenContact(Payment payment, Xsd.TxnHeader xmlInvoiceHeader, IValueObjectImportContext context, ZString errorContext)
		{
			if (xmlInvoiceHeader.TxnOverrideAddress != null && !xmlInvoiceHeader.TxnOverrideAddress.AddressCode.IsEmpty)
			{
				var address = payment.Header == null ? null : payment.Header.Addresses.Cast<OrgAddress>().FirstOrDefault(x => x.OA_Code == xmlInvoiceHeader.TxnOverrideAddress.AddressCode && x.OA_IsActive);

				payment.AH_OA_InvoiceAddressOverride = address == null ? ZGuid.Empty : address.PK;

				if (payment.AH_OA_InvoiceAddressOverride.IsEmpty)
				{
					NotificationManager.AddErrorToNotifications(Res.GetString("c1daf438-9fd9-470c-91f6-ac12920e7922", "Could not match Transaction override address."));
				}
			}

			if (xmlInvoiceHeader.TxnOverrideContact != null && !xmlInvoiceHeader.TxnOverrideContact.Name.IsEmpty)
			{
				var contactReference = new Xsd.ContactReference();
				contactReference.Organisation = xmlInvoiceHeader.DebtorOrCreditor;
				contactReference.Organisation.OrganisationDetails.Contacts.Clear();
				contactReference.Organisation.OrganisationDetails.Contacts.Add(xmlInvoiceHeader.TxnOverrideContact);
				contactReference.ContactSequenceRef = 1;

				var contactHelper = new ContactValueObjectHelper(errorContext);
				payment.AH_OC_InvoiceContactOverride = contactHelper.FromContactReferenceGetContactPK(contactReference, context);
			}
		}

		protected virtual bool ShouldPopulateCashReceiptOrPaymentDetails(Xsd.TxnHeader xmlInvoiceHeader)
		{
			bool result = false;

			if (xmlInvoiceHeader.TxnType == Xsd.TxnType.INV)
			{
				result = xmlInvoiceHeader.ReceiptPaymentTypeSpecified
						|| !xmlInvoiceHeader.BankCode.IsEmpty
						|| !xmlInvoiceHeader.ChequeBook.IsEmpty
						|| !xmlInvoiceHeader.ChequeDrawer.IsEmpty
						|| !xmlInvoiceHeader.DrawerBank.IsEmpty
						|| !xmlInvoiceHeader.DrawerBankBranch.IsEmpty
						|| !xmlInvoiceHeader.ChequeOrReference.IsEmpty;
			}

			return result;
		}

		void CombineInvoiceLinesByChargeCode(InvoicingBase invoice)
		{
			List<InvoicingLineBase> combinedLines = new List<InvoicingLineBase>();
			List<InvoicingLineBase> linesToDelete = new List<InvoicingLineBase>();
			using (invoice.Lines.SuspendListChanged())
			{
				foreach (InvoicingLineBase line in invoice.Lines)
				{
					if (line.AL_AC.IsValid)
					{
						var combinedLine = from combined in combinedLines
										   where combined.AL_JH == line.AL_JH && combined.AL_AC == line.AL_AC && combined.AL_AG == line.AL_AG &&
											   combined.AL_AT == line.AL_AT && combined.AL_AW == line.AL_AW &&
											   combined.AL_GB == line.AL_GB && combined.AL_GE == line.AL_GE &&
											   combined.ImportedApportionmentID == line.ImportedApportionmentID &&
											   combined.TargetJobIDFromIntercompanyInvoiceImport == line.TargetJobIDFromIntercompanyInvoiceImport &&
											   combined.RelatedJobFromIntercompanyInvoiceImport.JobPk == line.RelatedJobFromIntercompanyInvoiceImport.JobPk
										   select combined;

						if (!combinedLine.Any())
						{
							combinedLines.Add(line);
						}
						else
						{
							InvoicingLineBase existingCombinedLine = combinedLine.First();

							ZDecimal taxAmount = existingCombinedLine.AL_OSTaxAmount + line.AL_OSTaxAmount;
							existingCombinedLine.AL_OSExTaxAmount += line.AL_OSExTaxAmount;
							existingCombinedLine.AL_OSTaxAmount = taxAmount;
							linesToDelete.Add(line);
						}
					}
				}

				foreach (InvoicingLineBase lineToDelete in linesToDelete)
				{
					invoice.Lines.RemoveAndDelete(lineToDelete);
				}
			}
		}

		protected virtual TransactionLineBuilder GetInvoiceLineBuilder(INotificationManager notificationManager, TransactionBuilderConfig config)
		{
			TransactionLineBuilder builder = new TransactionLineBuilder(notificationManager, config);
			return builder;
		}

		protected virtual ApportionmentBuilder GetApportionmentBuilder(INotificationManager notificationManager, TransactionBuilderConfig config)
		{
			ApportionmentBuilder builder = new ApportionmentBuilder(notificationManager, config);
			return builder;
		}

		public static bool IsTaxApplicable(AccTransactionHeader transactionHeader)
		{
			if (transactionHeader != null && transactionHeader.Header != null)
			{
				return GlbCompany.CurrentCompany.GC_IsGSTRegistered &&
				(
					transactionHeader.AH_Ledger == LedgerTypes.AccountsPayable && transactionHeader.Header.CompanyData.IsAPTaxApplicable ||
					transactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable && transactionHeader.Header.CompanyData.IsARTaxApplicable ||
					transactionHeader.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions && transactionHeader.Header.CompanyData.IsAPTaxApplicable ||
					transactionHeader.AH_Ledger == LedgerTypes.TransactionsPendingAllocation && transactionHeader.Header.CompanyData.IsAPTaxApplicable
				);
			}
			else
			{
				return GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			}
		}

		public void SetValuesOnDirectReceiptPaymentBusinessObject(DirectTransactionHeaderBase transactionHeader,
			Xsd.TxnHeader xmlInvoiceHeader, IValueObjectImportContext context, ZString errorContext)
		{
			SetInvoiceDate(transactionHeader, xmlInvoiceHeader);
			SetBankAccount(transactionHeader, xmlInvoiceHeader, errorContext);
			if (Config.SetReceiptPaymentType)
			{
				SetReceiptPaymentType(transactionHeader, xmlInvoiceHeader, errorContext);
			}
			SetChequeBook(transactionHeader, xmlInvoiceHeader, context, errorContext);
			SetChequeOrReference(transactionHeader, xmlInvoiceHeader, context, errorContext);
			SetExchangeRate(transactionHeader, xmlInvoiceHeader, errorContext);
			SetPostDate(transactionHeader, xmlInvoiceHeader);
			SetDescription(transactionHeader, xmlInvoiceHeader, context, errorContext);
			SetPayeeName(transactionHeader, xmlInvoiceHeader, context, errorContext);
			SetDrawerBank(transactionHeader, xmlInvoiceHeader, context, errorContext);
			SetDrawerBankBranch(transactionHeader, xmlInvoiceHeader, context, errorContext);

			for (int currentLine = 0; currentLine < xmlInvoiceHeader.TxnLines.Count; currentLine++)
			{
				string lineErrorContext = errorContext + Res.GetString("7b909e2e-5acd-4266-9c94-9b4b611857e1", "Line {0}:", currentLine + 1) + " ";
				Xsd.TxnLine xmlInvoiceLine = xmlInvoiceHeader.TxnLines[currentLine];

				TransactionLineBuilder builder = new TransactionLineBuilder(NotificationManager, Config);
				builder.AddTransactionLineToDirectReceiptPaymentBusinessObject(transactionHeader, xmlInvoiceLine, context, lineErrorContext);
			}
		}

		public void SetValuesOnJournalBusinessObject(Journal journal,
			Xsd.TxnHeader xmlInvoiceHeader, IValueObjectImportContext context, ZString errorContext)
		{
			if (!xmlInvoiceHeader.DebtorOrCreditorGUID.IsEmpty)
			{
				SetJournalOrganisation(journal, xmlInvoiceHeader, context, errorContext);
			}
			else
			{
				SetOrganisation(journal, xmlInvoiceHeader, context, errorContext);
			}
			SetDescription(journal, xmlInvoiceHeader, context, errorContext);
			SetInvoiceDate(journal, xmlInvoiceHeader);
			SetDueDate(journal, xmlInvoiceHeader);
			SetHeaderBranch(journal, xmlInvoiceHeader, errorContext);
			SetHeaderDepartment(journal, xmlInvoiceHeader, errorContext);
			SetCurrencyDetails(journal, xmlInvoiceHeader, context, errorContext);
			SetExchangeRate(journal, xmlInvoiceHeader, errorContext);
			SetOSTotal(journal, xmlInvoiceHeader);
			journal.OnLoaded();
			SetInvoiceAmount(journal, xmlInvoiceHeader);
			journal.AH_OutstandingAmount = journal.AH_InvoiceAmount;
			SetGLAccount(journal, errorContext);
		}

		void SetGLAccount(Journal journal, ZString errorContext)
		{
			var registryItem = journal.AH_Ledger == LedgerTypes.AccountsPayable ? AccountingConfigurationRegistry.Instance.APJournalAccount : AccountingConfigurationRegistry.Instance.ARJournalAccount;
			var glAccountPK = (Guid)registryItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			var glAccount = journal.Factory.Load<AccGLHeader>(glAccountPK);
			if (glAccount != null)
			{
				journal.AH_AG = glAccountPK;
			}
			else
			{
				NotificationManager.ReportNoBizObjsFoundError(Res.GetString("F6628EDB-36E1-4716-B829-265DF677FEDA", "GL Account"), registryItem.HumanReadableRegistryPath(), 0, errorContext);
			}
		}

		public void SetValuesOnClearingJournalBusinessObject(Journal journal,
			Xsd.TxnHeader xmlInvoiceHeader, IValueObjectImportContext context, ZString errorContext)
		{
			if (!xmlInvoiceHeader.DebtorOrCreditorGUID.IsEmpty)
			{
				SetJournalOrganisation(journal, xmlInvoiceHeader, context, errorContext);
			}
			else
			{
				SetOrganisation(journal, xmlInvoiceHeader, context, errorContext);
			}
			SetDescription(journal, xmlInvoiceHeader, context, errorContext);
			SetInvoiceDate(journal, xmlInvoiceHeader);
			SetDueDate(journal, xmlInvoiceHeader);
			SetHeaderBranch(journal, xmlInvoiceHeader, errorContext);
			SetHeaderDepartment(journal, xmlInvoiceHeader, errorContext);
			SetCurrencyDetails(journal, xmlInvoiceHeader, context, errorContext);
			if (xmlInvoiceHeader.LocalInvoiceAmtExclTax.Value != 0)
			{
				SetExchangeRate(journal, xmlInvoiceHeader, errorContext);
			}
			SetOSExTaxAmount(journal, xmlInvoiceHeader);
			journal.OnLoaded();
		}

		public void SetValuesOnReceiptBusinessObject(ReceiptPaymentBase transactionHeader, Xsd.TxnHeader xmlInvoiceHeader, IValueObjectImportContext context, ZString errorContext)
		{
			SetInvoiceDate(transactionHeader, xmlInvoiceHeader);
			if (Config.SetBranch)
			{
				SetHeaderBranch(transactionHeader, xmlInvoiceHeader, errorContext);
			}
			if (Config.SetDepartment)
			{
				SetHeaderDepartment(transactionHeader, xmlInvoiceHeader, errorContext);
			}
			SetPostDate(transactionHeader, xmlInvoiceHeader);
			if (Config.SetOrganisation)
			{
				SetOrganisation(transactionHeader, xmlInvoiceHeader, context, errorContext);
			}
			SetDescription(transactionHeader, xmlInvoiceHeader, context, errorContext);
			if (Config.SetReceiptPaymentType)
			{
				SetReceiptPaymentType(transactionHeader, xmlInvoiceHeader, errorContext);
			}
			SetBankAccount(transactionHeader, xmlInvoiceHeader, errorContext);
			SetChequeOrReference(transactionHeader, xmlInvoiceHeader, context, errorContext);
			SetCurrencyDetails(transactionHeader, xmlInvoiceHeader, context, errorContext);
			SetExchangeRate(transactionHeader, xmlInvoiceHeader, errorContext);
			SetOSExTaxAmount(transactionHeader, xmlInvoiceHeader);
			SetDrawerBank(transactionHeader, xmlInvoiceHeader, context, errorContext);
			SetDrawerBankBranch(transactionHeader, xmlInvoiceHeader, context, errorContext);
			SetPayeeName(transactionHeader, xmlInvoiceHeader, context, errorContext);
			SetPaymentReceiptBatchDate(transactionHeader, xmlInvoiceHeader);
		}

		public void SetValuesOnPaymentBusinessObject(Payment payment, Xsd.TxnHeader xmlInvoiceHeader, IValueObjectImportContext context, ZString errorContext)
		{
			SetInvoiceDate(payment, xmlInvoiceHeader);
			if (Config.SetBranch)
			{
				SetHeaderBranch(payment, xmlInvoiceHeader, errorContext);
			}
			if (Config.SetDepartment)
			{
				SetHeaderDepartment(payment, xmlInvoiceHeader, errorContext);
			}
			SetPostDate(payment, xmlInvoiceHeader);
			if (Config.SetOrganisation)
			{
				SetOrganisation(payment, xmlInvoiceHeader, context, errorContext);
			}
			SetDescription(payment, xmlInvoiceHeader, context, errorContext);
			if (Config.SetReceiptPaymentType)
			{
				SetReceiptPaymentType(payment, xmlInvoiceHeader, errorContext);
			}
			SetBankAccount(payment, xmlInvoiceHeader, errorContext);
			SetChequeOrReference(payment, xmlInvoiceHeader, context, errorContext);
			SetChequeBook(payment, xmlInvoiceHeader, context, errorContext);
			if (!payment.AH_ChequeOrReference.IsEmpty)
			{
				SetChequeOrReference(payment, xmlInvoiceHeader, context, errorContext);
			}
			SetCurrencyDetails(payment, xmlInvoiceHeader, context, errorContext);
			SetExchangeRate(payment, xmlInvoiceHeader, errorContext);
			SetOSExTaxAmount(payment, xmlInvoiceHeader);
			SetLocalExTaxAmount(payment, xmlInvoiceHeader);

			SetPaymentOverriddenAddressAndOverriddenContact(payment, xmlInvoiceHeader, context, errorContext);
		}

		public void SetValuesOnMiscTransactionBusinessObject(TransactionHeader transaction, Xsd.TxnHeader xmlInvoiceHeader, IValueObjectImportContext context)
		{
			var errorContext = string.Empty;

			if (Config.SetBranch)
			{
				SetHeaderBranch(transaction, xmlInvoiceHeader, errorContext);
			}
			if (Config.SetDepartment)
			{
				SetHeaderDepartment(transaction, xmlInvoiceHeader, errorContext);
			}
			if (Config.SetOrganisation)
			{
				SetOrganisation(transaction, xmlInvoiceHeader, context, errorContext);
			}
			SetInvoiceDate(transaction, xmlInvoiceHeader);
			SetPostDate(transaction, xmlInvoiceHeader);
			SetDueDate(transaction, xmlInvoiceHeader);
			SetDescription(transaction, xmlInvoiceHeader, context, errorContext);
			SetBankAccount(transaction, xmlInvoiceHeader, errorContext);
			SetCurrencyDetails(transaction, xmlInvoiceHeader, context, errorContext);
			SetExchangeRate(transaction, xmlInvoiceHeader, errorContext);
			SetOSTotal(transaction, xmlInvoiceHeader);
			SetInvoiceAmountForMiscTransaction(transaction, xmlInvoiceHeader);
			transaction.AH_OutstandingAmount = transaction.AH_InvoiceAmount;
		}

		public void SetValuesOnBankFeeJournalBusinessObject(Journal journal, Xsd.TxnHeader xmlInvoiceHeader, IValueObjectImportContext context)
		{
			var errorContext = string.Empty;

			if (!xmlInvoiceHeader.DebtorOrCreditorGUID.IsEmpty)
			{
				SetJournalOrganisation(journal, xmlInvoiceHeader, context, errorContext);
			}
			else
			{
				SetOrganisation(journal, xmlInvoiceHeader, context, errorContext);
			}
			SetDescription(journal, xmlInvoiceHeader, context, errorContext);
			SetInvoiceDate(journal, xmlInvoiceHeader);
			SetPostDate(journal, xmlInvoiceHeader);
			SetDueDate(journal, xmlInvoiceHeader);
			SetHeaderBranch(journal, xmlInvoiceHeader, errorContext);
			SetHeaderDepartment(journal, xmlInvoiceHeader, errorContext);
			SetCurrencyDetails(journal, xmlInvoiceHeader, context, errorContext);
			SetExchangeRate(journal, xmlInvoiceHeader, errorContext);
			SetInvoiceAmountForMiscTransaction(journal, xmlInvoiceHeader);

			if (xmlInvoiceHeader.OsInvoiceAmtInclTax.Value > 0)
			{
				journal.DebitCreditSign = DebitCreditDataEntry.CR;
			}
			else
			{
				journal.DebitCreditSign = DebitCreditDataEntry.DR;
			}

			journal.AH_InvoiceAmount = Math.Abs(Utilities.Round(journal.AH_InvoiceAmount, ((IMatching)journal).CurrencyDecimals));
			journal.AH_OSTotal = Math.Abs(Utilities.Round(xmlInvoiceHeader.OsInvoiceAmtInclTax.Value, ((IMatching)journal).CurrencyDecimals));
			journal.AH_OSExTaxAmount = journal.AH_OSTotal;
		}

		void SetPaymentReceiptBatchDate(ReceiptPaymentBase transactionHeader, Xsd.TxnHeader xmlInvoiceHeader)
		{
			if (xmlInvoiceHeader.PaymentReceiptBatchDate.IsValid && !xmlInvoiceHeader.PaymentReceiptBatchDate.IsEmpty)
			{
				((Receipt)transactionHeader).CompayReceiptBatchDate = xmlInvoiceHeader.PaymentReceiptBatchDate;
			}
		}

		void SetJournalOrganisation(TransactionHeader transactionHeader, Xsd.TxnHeader xmlInvoice, IValueObjectImportContext context, string errorContext)
		{
			OrganisationTypes orgType = xmlInvoice.Ledger == Xsd.TxnLedgerType.AP ? OrganisationTypes.Creditor : OrganisationTypes.Debtor;
			if (xmlInvoice.DebtorOrCreditorGUID.IsValid)
			{
				ZGuid organisationGUID = ZGuid.Empty;
				try
				{
					organisationGUID = new ZGuid(xmlInvoice.DebtorOrCreditorGUID);
				}
				catch (FormatException)
				{
					NotificationManager.AddErrorToNotifications(Res.GetString("4171fa7e-bf1b-4214-b317-c4cc7c42e01b", "Organization ID was not in correct format"));
				}

				OrgHeader organisation = transactionHeader.Factory.Load<OrgHeader>(organisationGUID);
				if (organisation != null)
				{
					transactionHeader.AH_OH = organisation.PK;
				}
			}

			if (Config.RunExtraValidation && !transactionHeader.AH_OH.IsValid)
			{
				ZString valueForDisplay = (xmlInvoice != null && xmlInvoice.DebtorOrCreditor != null) ? xmlInvoice.DebtorOrCreditor.EDICode : ZString.Empty;
				NotificationManager.ReportNoBizObjsFoundError(Res.GetString("022b0404-1eae-49c0-8b45-f683f66c41b1", "Organization"), valueForDisplay, 0, errorContext);
			}

			transactionHeader.Validation.ValidateAH_OH();
		}

		public void CheckInCacheForDuplicateTransactionNumbers(TransactionHeader transactionHeader)
		{
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, transactionHeader.PK);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, transactionHeader.AH_TransactionNum);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, transactionHeader.AH_TransactionType);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, transactionHeader.AH_Ledger);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_OH, transactionHeader.AH_OH);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, transactionHeader.AH_GC);
			filter.FetchOnlyFromLocalCache = true;
			TransactionHeader duplicatedHeader = transactionHeader.Factory.LoadTop1<TransactionHeader>(filter);
			if (duplicatedHeader != null)
			{
				NotificationManager.AddErrorToNotifications(Res.GetString("FCD176E6-A3E2-4016-B815-85E1D25E2459", "The transaction number '{0}' is already in use by another transaction in this file.", transactionHeader.AH_TransactionNum));
			}
		}

		public void CheckForDuplicateTransactionNumbers(TransactionHeader transactionHeader, string paymentReference, string disallowDuplicatedPaymentReferenceHandler)
		{
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, transactionHeader.PK);
			if (disallowDuplicatedPaymentReferenceHandler == Enterprise.Core.Constants.AllowDuplicatePaymentReferenceHandlerMethods.Codes.DisallowDuplicatesForLedgerAndTransactionType)
			{
				filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, transactionHeader.AH_TransactionType);
			}
			filter.AddToFilter(AccTransactionHeaderSchema.AH_ChequeOrReference, paymentReference);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, transactionHeader.AH_Ledger);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, transactionHeader.AH_GC);
			TransactionHeader duplicatedHeader = transactionHeader.Factory.LoadTop1<TransactionHeader>(filter);
			if (duplicatedHeader != null)
			{
				NotificationManager.AddErrorToNotifications(Res.GetString("3AFF5BF0-0686-4F83-8E9D-55959BECD54E", "The payment reference '{0}' is already in use by another transaction in this file or database.", paymentReference));
			}
		}

		#region RunValidationAndReportErrors

		public void RunValidationAndReportErrors(TransactionHeader transactionHeader)
		{
			if (transactionHeader is InvoicingBase)
			{
				using (((InvoicingBase)transactionHeader).SuspendCreditLimitCheck())
				{
					transactionHeader.Validation.ValidateAll();
				}
			}
			else
			{
				transactionHeader.Validation.ValidateAll();
			}

			if (transactionHeader.HasErrors)
			{
				ReportErrors(transactionHeader);
			}

			if (transactionHeader.HasWarnings)
			{
				ReportWarnings(transactionHeader);
			}
		}

		void ReportErrors(IBusiness businessEntity)
		{
			ZNotificationCollector notificationCollector = new ZNotificationCollector(businessEntity, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);

			foreach (var message in notificationCollector.GetErrors().Select(notification => notification.Message).Distinct())
			{
				NotificationManager.AddErrorToNotifications(message);
			}
		}

		void ReportWarnings(IBusiness businessEntity)
		{
			ZNotificationCollector notificationCollector = new ZNotificationCollector(businessEntity, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);

			foreach (var message in notificationCollector.GetWarnings().Select(notification => notification.Message).Distinct())
			{
				NotificationManager.AddWarningToNotifications(message);
			}
		}

		#endregion

		public void CheckTotalsOnTransactionHeaderWithLines(TransactionHeaderWithLines transactionHeader, Xsd.TxnHeader xmlInvoiceHeader, string errorContext)
		{
			ZDecimal bizObj_OSExTaxAmount = 0;
			ZDecimal bizObj_OSInclTaxAmount = 0;
			ZDecimal bizObj_OSTaxAmount = 0;
			ZDecimal bizObj_OSWHTAmount = 0;

			foreach (DependentTransactionLine transactionLine in transactionHeader.Lines)
			{
				if (transactionHeader.AH_RX_NKTransactionCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
				{
					bizObj_OSExTaxAmount += transactionLine.AL_LocalExTaxAmount;
					bizObj_OSTaxAmount += transactionLine.AL_LocalTaxAmount;
					bizObj_OSWHTAmount += transactionLine.AL_OSWHTAmount;
				}
				else
				{
					bizObj_OSExTaxAmount += transactionLine.AL_OSExTaxAmount;
					bizObj_OSTaxAmount += transactionLine.AL_OSTaxAmount;
					bizObj_OSWHTAmount += transactionLine.AL_OSWHTAmount;
				}
			}
			bizObj_OSInclTaxAmount = bizObj_OSExTaxAmount + bizObj_OSTaxAmount;

			ZDecimal xmlHdr_OSExTaxAmount = TxnHeaderMapper.GetDecimalFromXmlFinancialValue(transactionHeader.Factory, xmlInvoiceHeader.OsInvoiceAmtExclTax, transactionHeader.GetType());
			ZDecimal xmlHdr_OSInclTaxAmount = 0;
			if (xmlInvoiceHeader.OsInvoiceAmtInclTax.IsSpecified && !xmlInvoiceHeader.OsInvoiceAmtInclTax.Value.IsEmpty)
			{
				xmlHdr_OSInclTaxAmount = TxnHeaderMapper.GetDecimalFromXmlFinancialValue(transactionHeader.Factory, xmlInvoiceHeader.OsInvoiceAmtInclTax, transactionHeader.GetType());
			}
			ZDecimal xmlHdr_OSTaxAmount = TxnHeaderMapper.GetDecimalFromXmlFinancialValue(transactionHeader.Factory, xmlInvoiceHeader.OsTaxAmount, transactionHeader.GetType());
			ZDecimal xmlHdr_OSWHTAmount = TxnHeaderMapper.GetDecimalFromXmlFinancialValue(transactionHeader.Factory, xmlInvoiceHeader.OsWHTAmount, transactionHeader.GetType());

			ZDecimal xmlLines_OSExTaxAmount = 0;
			ZDecimal xmlLines_OSInclTaxAmount = 0;
			ZDecimal xmlLines_OSTaxAmount = 0;
			ZDecimal xmlLines_OSWHTAmount = 0;

			foreach (Xsd.TxnLine txnLine in xmlInvoiceHeader.TxnLines)
			{
				xmlLines_OSExTaxAmount += TxnHeaderMapper.GetDecimalFromXmlFinancialValue(transactionHeader.Factory, txnLine.OsInvoiceAmtExclTax, transactionHeader.GetType());
				xmlLines_OSTaxAmount += TxnHeaderMapper.GetDecimalFromXmlFinancialValue(transactionHeader.Factory, txnLine.OsTaxAmount, transactionHeader.GetType());
				xmlLines_OSWHTAmount += TxnHeaderMapper.GetDecimalFromXmlFinancialValue(transactionHeader.Factory, txnLine.OsWHTAmount, transactionHeader.GetType());
			}
			xmlLines_OSInclTaxAmount = xmlLines_OSExTaxAmount + xmlLines_OSTaxAmount;

			CheckXmlHeaderAndLineValues("OSExTaxAmount", xmlHdr_OSExTaxAmount, xmlLines_OSExTaxAmount, errorContext);
			if (xmlInvoiceHeader.OsInvoiceAmtInclTax.IsSpecified && !xmlInvoiceHeader.OsInvoiceAmtInclTax.Value.IsEmpty)
			{
				CheckXmlHeaderAndLineValues("OSInclTaxAmount", xmlHdr_OSInclTaxAmount, xmlLines_OSInclTaxAmount, errorContext);
			}
			CheckXmlHeaderAndLineValues("OSTaxAmount", xmlHdr_OSTaxAmount, xmlLines_OSTaxAmount, errorContext);
			CheckXmlHeaderAndLineValues("OSWHTAmount", xmlHdr_OSWHTAmount, xmlLines_OSWHTAmount, errorContext);

			CheckXmlAndBizObjValues("OSExTaxAmount", xmlLines_OSExTaxAmount, bizObj_OSExTaxAmount, errorContext);
			CheckXmlAndBizObjValues("OSInclTaxAmount", xmlLines_OSInclTaxAmount, bizObj_OSInclTaxAmount, errorContext);
			CheckXmlAndBizObjValues("OSTaxAmount", xmlLines_OSTaxAmount, bizObj_OSTaxAmount, errorContext);
			CheckXmlAndBizObjValues("OSWHTAmount", xmlLines_OSWHTAmount, bizObj_OSWHTAmount, errorContext);
		}

		void SetDrawerBankBranch(TransactionHeader transactionHeader, Xsd.TxnHeader xmlInvoiceHeader, IValueObjectImportContext context, ZString errorContext)
		{
			if (transactionHeader.AH_TransactionType == ZArchitecture.Core.TransactionTypes.DirectReceipt || transactionHeader.AH_TransactionType == ZArchitecture.Core.TransactionTypes.Receipt)
			{
				context.SetPropertyInfoValue(transactionHeader.AH_DrawerBranchInfo, xmlInvoiceHeader.DrawerBankBranch, xmlInvoiceHeader.DrawerBankBranchSpecified);
			}
		}

		void SetDrawerBank(TransactionHeader transactionHeader, Xsd.TxnHeader xmlInvoiceHeader, IValueObjectImportContext context, ZString errorContext)
		{
			if (transactionHeader.AH_TransactionType == ZArchitecture.Core.TransactionTypes.DirectReceipt || transactionHeader.AH_TransactionType == ZArchitecture.Core.TransactionTypes.Receipt)
			{
				context.SetPropertyInfoValue(transactionHeader.AH_DrawerBankInfo, xmlInvoiceHeader.DrawerBank, xmlInvoiceHeader.DrawerBankSpecified);
			}
		}

		void SetPayeeName(TransactionHeader transactionHeader, Xsd.TxnHeader xmlInvoiceHeader, IValueObjectImportContext context, ZString errorContext)
		{
			context.SetPropertyInfoValue(transactionHeader.AH_ChequeDrawerInfo, xmlInvoiceHeader.ChequeDrawer, xmlInvoiceHeader.ChequeDrawerSpecified);
		}

		bool IsApportionmentXmlInvoiceLine(InvoicingBase invoice, Xsd.TxnLine xmlInvoiceLine)
		{
			return (invoice is APInvoice || invoice is APCreditNote) && xmlInvoiceLine.ConsolOrJobTypeSpecified &&
				xmlInvoiceLine.ConsolOrJobType == Xsd.TxnLineConsolOrJobType.CSL;
		}

		void SetExchangeRate(TransactionHeader transactionHeader, Xsd.TxnHeader xmlInvoiceHeader, ZString errorContext)
		{
			ZDecimal exchangeRate = 1m;

			if (transactionHeader.AH_RX_NKTransactionCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
			{
				ZDecimal localAmount = xmlInvoiceHeader.LocalInvoiceAmtExclTax.Value;
				ZDecimal foreignAmount = xmlInvoiceHeader.OsInvoiceAmtExclTax.Value;
				ZInt decimals = transactionHeader.ExchangeRateDecimalPlaces;

				exchangeRate = Env.CurrentCompany.ExchangeRate.GetRate(localAmount, foreignAmount, decimals);
			}

			transactionHeader.AH_ExchangeRate = !exchangeRate.IsEmpty ? exchangeRate : new ZDecimal(1m);
		}

		public void SetReceiptPaymentType(TransactionHeader transactionHeader, Xsd.TxnHeader xmlInvoiceHeader, ZString errorContext)
		{
			transactionHeader.AH_ReceiptType = TxnHeaderReceiptPaymentTypeXmlMapping.Instance.GetEnterpriseCode(xmlInvoiceHeader.ReceiptPaymentType, errorContext, NotificationManager.NotificationSubscriber);
		}

		void SetChequeBook(Payment payment, Xsd.TxnHeader xmlInvoiceHeader, IValueObjectImportContext context, ZString errorContext)
		{
			if (payment.BankAccount != null && payment.AH_ReceiptType == ReceiptTypes.Cheque)
			{
				ZQuery chequeBookQuery = new ZQuery(AccChequeBookSchema.AK_Code, xmlInvoiceHeader.ChequeBook);
				chequeBookQuery.AddToFilter(AccChequeBookSchema.AK_AB, payment.BankAccount.PK);
				payment.ChequeBooks.Load(chequeBookQuery);
				if (payment.ChequeBooks.Count == 1)
				{
					payment.ChequeBook = payment.ChequeBooks[0].PK;
				}
			}
		}

		void SetChequeBook(DirectTransactionHeaderBase transactionHeader, Xsd.TxnHeader xmlInvoiceHeader, IValueObjectImportContext context, ZString errorContext)
		{
			if (transactionHeader.AH_TransactionType == ZArchitecture.Core.TransactionTypes.DirectPayment
				&& transactionHeader.BankAccount != null && transactionHeader.AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cheque
				&& ZDecimal.CanParseAsInteger(xmlInvoiceHeader.ChequeOrReference))
			{
				ZQuery chequeBookQuery = new ZQuery(AccChequeBookSchema.AK_AB, transactionHeader.BankAccount.PK);
				AccChequeBookCollection chequeBooks = new AccChequeBookCollection(context.Factory, chequeBookQuery);
				chequeBooks.Load();

				ZDecimal chequeNumber = ZDecimal.Parse(xmlInvoiceHeader.ChequeOrReference);

				foreach (AccChequeBook chequeBook in chequeBooks)
				{
					if (chequeBook.AK_StartNo <= chequeNumber && chequeBook.AK_LastNo >= chequeNumber)
					{
						transactionHeader.ChequeBookPK = chequeBook.PK;
					}
				}
			}
		}

		void SetOrganisation(TransactionHeader transactionHeader, Xsd.TxnHeader xmlInvoice, IValueObjectImportContext context, string errorContext)
		{
			OrganisationTypes orgType = xmlInvoice.Ledger == Xsd.TxnLedgerType.AP ? OrganisationTypes.Creditor : OrganisationTypes.Debtor;
			OrgHeader organisation = context.FindOrganisation(xmlInvoice.DebtorOrCreditor, transactionHeader, orgType);
			if (organisation != null && (orgType == OrganisationTypes.Creditor && organisation.OH_IsCreditor ||
										orgType == OrganisationTypes.Debtor && organisation.OH_IsDebtor))
			{
				transactionHeader.AH_OH = organisation.PK;
			}

			if (Config.RunExtraValidation && !transactionHeader.AH_OH.IsValid)
			{
				ZString valueForDisplay = (xmlInvoice != null && xmlInvoice.DebtorOrCreditor != null) ? xmlInvoice.DebtorOrCreditor.EDICode : ZString.Empty;
				NotificationManager.ReportNoBizObjsFoundError(Res.GetString("5D3252AC-3BA2-40a1-967A-824E0A924D66", "Organization"), valueForDisplay, 0, errorContext);
			}

			transactionHeader.Validation.ValidateAH_OH();
		}

		void SetHeaderBranch(TransactionHeader transactionHeader, Xsd.TxnHeader xmlInvoice, string errorContext)
		{
			// In Cross-Ledger import, Branch set after import takes precedence over imported Branch Code
			if (Config.CrossLedgerImport)
			{
				return;
			}

			if (!xmlInvoice.Branch.IsEmpty)
			{
				GlbBranch branch = BusinessObjectRetriever.GetBranchFromBranchCode(transactionHeader.Factory, xmlInvoice.Branch);

				if (branch != null)
				{
					transactionHeader.AH_GB = branch.PK;
				}
				else
				{
					NotificationManager.ReportNoBizObjsFoundError(Res.GetString("762c0569-0690-45c6-a868-1816737b7dfd", "Branch"), xmlInvoice.Branch, 0, errorContext);
				}
			}
			else if (!Env.CurrentUser.IsBatchProcessor)
			{
				transactionHeader.AH_GB = GlbBranch.CurrentBranch.PK;
			}
		}

		void SetHeaderDepartment(TransactionHeader transactionHeader, Xsd.TxnHeader xmlInvoice, string errorContext)
		{
			if (!xmlInvoice.Department.IsEmpty)
			{
				GlbDepartment department = BusinessObjectRetriever.GetDepartmentFromDepartmentCode(transactionHeader.Factory, xmlInvoice.Department);

				if (department != null)
				{
					transactionHeader.AH_GE = department.PK;
				}
				else
				{
					NotificationManager.ReportNoBizObjsFoundError(Res.GetString("819afaea-ac06-418b-bddd-2e51f1c83284", "Department"), xmlInvoice.Department, 0, errorContext);
				}
			}
			else if (!Env.CurrentUser.IsBatchProcessor)
			{
				transactionHeader.AH_GE = GlbDepartment.CurrentDepartment.PK;
			}
		}

		void SetBasicFieldsForInvoice(TransactionHeaderWithLines transactionHeader, Xsd.TxnHeader xmlInvoiceHeader, IValueObjectImportContext context, ZString errorContext)
		{
			context.SetPropertyInfoValue(transactionHeader.AH_TransactionCountInfo, xmlInvoiceHeader.TxnCount, xmlInvoiceHeader.TxnCountSpecified, errorContext + Res.GetString("cb8aaa50-3c8c-4a57-ad5a-0104b1a94464", "Transaction Count"));
			context.SetPropertyInfoValue(transactionHeader.AH_TransactionCategoryInfo, xmlInvoiceHeader.TxnCategory, xmlInvoiceHeader.TxnCategorySpecified, errorContext + Res.GetString("634517d8-49a4-4c14-96d2-d14c7ed52b01", "Transaction Category"));
			context.SetPropertyInfoValue(transactionHeader.AH_ConsolidatedInvoiceRefInfo, xmlInvoiceHeader.JobInvoiceNo, xmlInvoiceHeader.JobInvoiceNoSpecified, errorContext + Res.GetString("444ede01-823b-4bb5-968b-ea9011f3382f", "Consolidated Transaction Reference"));
			context.SetPropertyInfoValue(transactionHeader.AH_TransactionReferenceInfo, xmlInvoiceHeader.TxnReference, xmlInvoiceHeader.TxnReferenceSpecified, errorContext + Res.GetString("a10d9eb6-4cc1-4195-9de1-908106dccbda", "Transaction Reference"));

			SetChequeOrReference(transactionHeader, xmlInvoiceHeader, context, errorContext);
			SetDescription(transactionHeader, xmlInvoiceHeader, context, errorContext);
			SetInvoiceDate(transactionHeader, xmlInvoiceHeader);
			SetTransactionNum(transactionHeader, xmlInvoiceHeader, context, errorContext);
			SetIsDisbursement(transactionHeader, xmlInvoiceHeader, context, errorContext);
			SetPostDate(transactionHeader, xmlInvoiceHeader);
			SetDueDate(transactionHeader, xmlInvoiceHeader);
		}

		void SetIsDisbursement(TransactionHeaderWithLines transactionHeader, Xsd.TxnHeader xmlInvoiceHeader, IValueObjectImportContext context, ZString errorContext)
		{
			if (xmlInvoiceHeader.DisbursementFlagSpecified &&
				transactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable && transactionHeader is InvoicingBase)
			{
				((InvoicingBase)transactionHeader).IsDisbursementOrFinal = xmlInvoiceHeader.DisbursementFlag;
			}
		}

		void SetChequeOrReference(TransactionHeader transactionHeader, Xsd.TxnHeader xmlInvoiceHeader, IValueObjectImportContext context, ZString errorContext)
		{
			if (transactionHeader.AH_TransactionType == TransactionTypes.Invoice ||
				transactionHeader.AH_TransactionType == TransactionTypes.CreditNote ||
				transactionHeader.AH_TransactionType == TransactionTypes.AdjustmentNote)
			{
				if (!xmlInvoiceHeader.PaymentReference.IsEmpty)
				{
					context.SetPropertyInfoValue(transactionHeader.AH_ChequeOrReferenceInfo, xmlInvoiceHeader.PaymentReference, errorContext + Res.GetString("7A171A51-038C-40bf-89F5-2CDA29D226DE", "Payment Reference"));
				}
			}
			else
			{
				context.SetPropertyInfoValue(transactionHeader.AH_ChequeOrReferenceInfo, xmlInvoiceHeader.ChequeOrReference, xmlInvoiceHeader.ChequeOrReferenceSpecified, errorContext + Res.GetString("86107afb-4f11-4507-80ca-9afb8c0a2944", "Check or Reference"));

				if (transactionHeader.AH_TransactionType == TransactionTypes.DirectPayment && transactionHeader.BankAccount != null &&
					transactionHeader.BankAccount.HasChequeNumberBeenUsedOnAnotherTransactionAndNotYetSaved(transactionHeader.AH_ChequeOrReference, transactionHeader.PK))
				{
					NotificationManager.AddErrorToNotifications(Res.GetString("a7c452cb-e8e8-4eec-9e5c-20333deac6e0", "Header Check Or Reference: This check number is already in use."));
				}
			}
		}

		void SetTransactionNum(TransactionHeaderWithLines transactionHeader, Xsd.TxnHeader xmlInvoiceHeader, IValueObjectImportContext context, ZString errorContext)
		{
			if (transactionHeader.AH_Ledger == LedgerTypes.AccountsPayable || transactionHeader.AH_Ledger == "UA")
			{
				context.SetPropertyInfoValue(transactionHeader.AH_TransactionNumInfo, xmlInvoiceHeader.TxnNumber, xmlInvoiceHeader.TxnNumberSpecified, errorContext + Res.GetString("1baa66fd-992b-4d03-a76d-ef4cec3c1975", "Transaction Number"));
			}
		}

		void SetDueDate(TransactionHeader transactionHeader, Xsd.TxnHeader xmlInvoiceHeader)
		{
			if (xmlInvoiceHeader.DueDate.IsValid)
			{
				transactionHeader.AH_DueDate = xmlInvoiceHeader.DueDate;
			}
		}

		void SetPostDate(TransactionHeader transactionHeader, Xsd.TxnHeader xmlInvoiceHeader)
		{
			if (xmlInvoiceHeader.PostDate.IsValid)
			{
				transactionHeader.AH_PostDate = xmlInvoiceHeader.PostDate;
			}
		}

		void SetInvoiceDate(TransactionHeader transactionHeader, Xsd.TxnHeader xmlInvoiceHeader)
		{
			if (xmlInvoiceHeader.InvoiceDate.IsValid)
			{
				transactionHeader.AH_InvoiceDate = xmlInvoiceHeader.InvoiceDate;
			}
		}

		void SetDescription(TransactionHeader transactionHeader, Xsd.TxnHeader xmlInvoiceHeader, IValueObjectImportContext context, ZString errorContext)
		{
			if (!xmlInvoiceHeader.Description.IsEmpty)
			{
				context.SetPropertyInfoValue(transactionHeader.AH_DescInfo, xmlInvoiceHeader.Description, xmlInvoiceHeader.DescriptionSpecified, errorContext + Res.GetString("f8d2b2c0-fbd7-4656-bb20-0e0e211cc443", "Transaction Description"));
			}
		}

		void SetCurrencyDetails(TransactionHeader transactionHeader, Xsd.TxnHeader xmlInvoiceHeader, IValueObjectImportContext context, ZString errorContext)
		{
			ZString oldAH_RX_NKTransactionCurrency = transactionHeader.AH_RX_NKTransactionCurrency;
			if (xmlInvoiceHeader.OsInvoiceAmtInclTax.IsSpecified && !xmlInvoiceHeader.OsInvoiceAmtInclTax.CurrencyCode.IsEmpty)
			{
				context.SetPropertyInfoValue(transactionHeader.AH_RX_NKTransactionCurrencyInfo, xmlInvoiceHeader.OsInvoiceAmtInclTax.CurrencyCode, ForeignKeyType.CurrencyNK, errorContext + Res.GetString("3c416d5e-c914-4e7d-8d20-8ca53bdd7ea7", "Currency Code"));
			}
			else if (xmlInvoiceHeader.OsInvoiceAmtExclTax.IsSpecified && !xmlInvoiceHeader.OsInvoiceAmtExclTax.CurrencyCode.IsEmpty)
			{
				context.SetPropertyInfoValue(transactionHeader.AH_RX_NKTransactionCurrencyInfo, xmlInvoiceHeader.OsInvoiceAmtExclTax.CurrencyCode, ForeignKeyType.CurrencyNK, errorContext + Res.GetString("3c416d5e-c914-4e7d-8d20-8ca53bdd7ea7", "Currency Code"));
			}

			if (transactionHeader.TransactionCurrency != null)
			{
				var isLocalCurrency = transactionHeader.AH_RX_NKTransactionCurrency == transactionHeader.Company.GC_RX_NKLocalCurrency;
				if (!isLocalCurrency)
				{
					ExchangeRateValidLedgerEnum exRateLedger;
					ExchangeRateType rateType;
					if (transactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable)
					{
						exRateLedger = ExchangeRateValidLedgerEnum.AR;
						rateType = ExchangeRateType.Sell;
					}
					else
					{
						exRateLedger = ExchangeRateValidLedgerEnum.AP;
						rateType = ExchangeRateType.Buy;
					}

					if (Config.PreserveExchangeRateFromSourceTransaction)
					{
						transactionHeader.AH_ExchangeRate = Env.CurrentCompany.ExchangeRate.GetRate(xmlInvoiceHeader.LocalInvoiceAmtExclTax.Value, xmlInvoiceHeader.OsInvoiceAmtExclTax.Value, transactionHeader.TransactionCurrency.Decimals);
					}
					else
					{
						if (xmlInvoiceHeader.OverrideSystemExchangeRate)
						{
							transactionHeader.AH_ExchangeRate = Env.CurrentCompany.ExchangeRate.GetRate(xmlInvoiceHeader.LocalInvoiceAmtExclTax.Value, xmlInvoiceHeader.OsInvoiceAmtExclTax.Value, AccTransactionHeaderSchema.AH_ExchangeRate.Scale);
						}
						else
						{
							if (ExchangeRateCalculator.IsExRateOptionApplicable(exRateLedger, isLocalCurrency, transactionHeader.AH_GC))
							{
								transactionHeader.AH_ExchangeRate = ExchangeRateCalculator.GetOverrideExchangeRate(transactionHeader.AH_RX_NKTransactionCurrency, isLocalCurrency,
									transactionHeader.AH_GC, rateType, exRateLedger, transactionHeader.AH_InvoiceDate, transactionHeader.AH_PostDate, transactionHeader.InvoiceTaxDate);
							}
							else //When registry option is DEF, we use post date
							{
								transactionHeader.AH_ExchangeRate = ExchangeRateCalculator.GetRate(transactionHeader.AH_RX_NKTransactionCurrency, rateType, transactionHeader.AH_PostDate.ToDateTime());
							}
						}
					}
				}
				else
				{
					transactionHeader.AH_ExchangeRate = 1m;
				}
			}
			else
			{
				transactionHeader.AH_RX_NKTransactionCurrency = oldAH_RX_NKTransactionCurrency;
				NotificationManager.ReportNoBizObjsFoundWarning(transactionHeader, Res.GetString("9d76118a-9252-4643-909e-9ed3979013a7", "Currency"), xmlInvoiceHeader.OsInvoiceAmtInclTax.CurrencyCode, errorContext);
			}
		}

		void SetBankAccount(TransactionHeader transactionHeader, Xsd.TxnHeader xmlInvoice, string errorContext)
		{
			AccBankAccount bank = BusinessObjectRetriever.GetBankFromBankCode(transactionHeader.Factory, xmlInvoice.BankCode);

			if (bank != null)
			{
				transactionHeader.AH_AB = bank.PK;
			}
			else
			{
				NotificationManager.ReportNoBizObjsFoundWarning(bank, Res.GetString("e1fdd3be-8850-4fa5-a7c8-8918071ed162", "Bank"), xmlInvoice.BankCode, errorContext);
			}
		}

		void SetOSTotal(TransactionHeader transactionHeader, Xsd.TxnHeader xmlInvoiceHeader)
		{
			transactionHeader.AH_OSTotal = TxnHeaderMapper.GetDecimalFromXmlFinancialValue(transactionHeader.Factory, xmlInvoiceHeader.OsInvoiceAmtInclTax);
		}

		void SetInvoiceAmount(TransactionHeader transactionHeader, Xsd.TxnHeader xmlInvoiceHeader)
		{
			transactionHeader.AH_InvoiceAmount = TxnHeaderMapper.GetDecimalFromXmlFinancialValue(transactionHeader.Factory, xmlInvoiceHeader.LocalInvoiceAmtInclTax);
		}

		void SetInvoiceAmountForMiscTransaction(TransactionHeader transactionHeader, Xsd.TxnHeader xmlInvoiceHeader)
		{
			if (xmlInvoiceHeader.LocalInvoiceAmtInclTax.Value == 0)
			{
				transactionHeader.AH_InvoiceAmount = transactionHeader.Company.GetExchangeRate().ForeignToLocal(transactionHeader.AH_OSTotal, transactionHeader.AH_ExchangeRate);
			}
			else
			{
				SetInvoiceAmount(transactionHeader, xmlInvoiceHeader);
			}
		}

		void SetOSExTaxAmount(TransactionHeader transactionHeader, Xsd.TxnHeader xmlInvoiceHeader)
		{
			transactionHeader.AH_OSExTaxAmount = TxnHeaderMapper.GetDecimalFromXmlFinancialValue(transactionHeader.Factory, xmlInvoiceHeader.OsInvoiceAmtExclTax, transactionHeader.GetType());
		}

		void SetLocalExTaxAmount(TransactionHeader transactionHeader, Xsd.TxnHeader xmlInvoiceHeader)
		{
			transactionHeader.AH_LocalExTaxAmount = TxnHeaderMapper.GetDecimalFromXmlFinancialValue(transactionHeader.Factory, xmlInvoiceHeader.LocalInvoiceAmtInclTax);
		}

		void ExtractPdfsAndAttachToEDocs(InvoicingBase transactionHeader, Xsd.TxnHeader xmlInvoiceHeader)
		{
			if (xmlInvoiceHeader.Attachments.Count > 0)
			{
				bool atLeastOneAttachmentWasAdded = false;

				foreach (Xsd.TxnHeaderAttachment attachment in xmlInvoiceHeader.Attachments)
				{
					var documentType = attachment.DocumentType.IsEmpty ? Core.Constants.RefDocTypes.MiscellaneousDocument : attachment.DocumentType.ToString();

					if (attachment.Data != null && attachment.Data.Any())
					{
						transactionHeader.DocManagerInfo.AddFileOrDocument(attachment.Data, attachment.FileName, documentType);
						atLeastOneAttachmentWasAdded = true;
					}
					else if (!attachment.FileName.IsEmpty || !attachment.FilePath.IsEmpty)
					{
						string fileName = attachment.FileName.IsEmpty ? Path.GetFileName(attachment.FilePath) : attachment.FileName.ToString();
						string filePath = attachment.FilePath.IsEmpty ? attachment.FileName : attachment.FilePath;

						if (filePath.Length < 260)
						{
							if (File.Exists(filePath))
							{
								try
								{
									transactionHeader.DocManagerInfo.AddFileOrDocument(filePath, documentType, false, filenameOnly: fileName);
									atLeastOneAttachmentWasAdded = true;
								}
								catch (EmptyContentEDocsException)
								{
									NotificationManager.AddErrorToNotifications(Res.GetString("135E5BBF-7EBA-417A-BBB4-380A45FC01AA", "File '{0}' is empty.", filePath));
								}
							}
							else
							{
								NotificationManager.AddErrorToNotifications(Res.GetString("87A14AA9-5B0A-459e-B2BA-C35B375286CC", "Could not find file '{0}'.", filePath));
							}
						}
						else
						{
							NotificationManager.AddErrorToNotifications(Res.GetString("1DACCBB3-AC59-4da0-A490-EF83B6160DCA", "The specified path is too long. It must be less than 260 characters. Path: '{0}'.", filePath));
						}
					}
				}

				if (atLeastOneAttachmentWasAdded)
				{
					transactionHeader.DocManagerInfo.SetupEDocsFactoryToBeSavedWithMainFactory(false);
				}
			}
		}

		void CheckXmlHeaderAndLineValues(ZString description, ZDecimal xmlHeaderAmount, ZDecimal xmlLineAmount, ZString errorContext)
		{
			if (xmlHeaderAmount != xmlLineAmount)
			{
				ZString messageToDisplay = Res.GetString("b2380379-4916-4549-b19f-6accbd097684", @"{0}: Total of XML lines do not equal XML Header Amount.
     XML Header Amount: {1}
     XML Lines Total: {2}", description, xmlHeaderAmount, xmlLineAmount);
				NotificationManager.AddErrorToNotifications(errorContext + messageToDisplay);
			}
		}

		void CheckXmlAndBizObjValues(ZString description, ZDecimal xmlAmount, ZDecimal bizObjAmount, ZString errorContext)
		{
			if (xmlAmount != bizObjAmount)
			{
				ZString messageToDisplay = Res.GetString("18216b3b-f461-4aff-b874-95d8257aee5e", @"{0}: XML Total does not equal Transaction Total.
     XML Total: {1}
     Transaction Total: {2}", description, xmlAmount, bizObjAmount);
				NotificationManager.AddErrorToNotifications(errorContext + messageToDisplay);
			}
		}

		readonly INotificationManager NotificationManager;
		readonly TransactionBuilderConfig Config;
	}
}
