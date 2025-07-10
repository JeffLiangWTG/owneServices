using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Barcode.Business;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocTransactionHeader : DocBaseWrapperWithJobHeader, IGenericTransactionHeaderPlugIn
	{
		protected const string TransitNumberSymbol = "A";
		protected const string OnUSSymbol = "C";
		protected const string DashSymbol = "D";
		protected const string Spacer = " ";

		#region Construction

		protected DocTransactionHeader(TransactionHeader transactionHeader, BusinessObjectFactory factoryToWrap)
			: base(transactionHeader, factoryToWrap)
		{
		}

		public static DocTransactionHeader New(TransactionHeader transactionHeader, BusinessObjectFactory factoryToWrap)
		{
			DocTransactionHeader result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(transactionHeader, factoryToWrap);
			}
			else if (transactionHeader != null)
			{
				result = new DocTransactionHeader(transactionHeader, factoryToWrap);
			}
			return result;
		}

		public static DocTransactionHeader New(BusinessObjectFactory factory, ZGuid pK)
		{
			return New(factory.Load<TransactionHeader>(pK), factory);
		}

		#endregion

		#region IGenericTransactionHeaderPlugIn members

		GenericTransactionHeaderSupporter IGenericTransactionHeaderPlugIn.HeaderSupporter
		{
			get { return fGenericTransactionSupporter ?? (fGenericTransactionSupporter = new DocTransactionHeaderGenericTransactionSupporter(this)); }
		}
		DocTransactionHeaderGenericTransactionSupporter fGenericTransactionSupporter;

		#endregion

		class DocTransactionHeaderGenericTransactionSupporter : GenericTransactionHeaderSupporter
		{
			public DocTransactionHeaderGenericTransactionSupporter(DocTransactionHeader parent)
			{
				this.Parent = parent;
			}
			protected readonly DocTransactionHeader Parent;

			protected internal override DocAccountingVoucher GetVoucher()
			{
				return Parent.Voucher;
			}

			protected internal override ZString GetReferenceNumber()
			{
				return Parent.ReferenceNumber;
			}

			protected internal override ZString GetStatementDescription()
			{
				return Parent.StatementDescription;
			}

			protected internal override ZString GetDisbursement()
			{
				return Parent.Disbursement;
			}

			protected internal override ZDateTime GetTransactionDueDate()
			{
				return Parent.TransactionDueDate;
			}

			protected internal override ZDecimal GetInvoiceAmountWithGST()
			{
				return Parent.InvoiceAmountWithGST;
			}

			protected internal override ZDecimal GetBalance()
			{
				return Parent.Balance;
			}

			protected internal override DocOrganisation GetOrganisation()
			{
				return Parent.Organisation;
			}

			protected internal override DocARBatchInvoiceLineCollection GetInvoices()
			{
				return Parent.GetInvoicesCore();
			}

			protected internal override ZString GetTransactionType()
			{
				return Parent.TransactionType;
			}

			protected internal override ZBool GetIsCancelled()
			{
				return Parent.IsCancelled;
			}

			protected internal override ZDateTime GetInvoiceDate()
			{
				return Parent.InvoiceDate;
			}

			protected internal override ZDate GetComplianceDocDate()
			{
				return Parent.ComplianceDocDate;
			}

			protected internal override ZString GetBankAccountCode()
			{
				return Parent.BankAccount == null ? ZString.Empty : Parent.BankAccount.Code;
			}

			protected internal override ZString GetOrganisationECRCode()
			{
				return Parent.Organisation == null ? ZString.Empty : Parent.Organisation.ECRCode;
			}

			protected internal override ZString GetTransactionARPaymentMethod()
			{
				return Parent.TransactionARPaymentMethod;
			}

			protected internal override ZString GetOrganisationARTermsPaymentMethod()
			{
				return Parent.OrganisationARTermsPaymentMethod;
			}

			protected internal override ZString GetOrganisationARAgreedPaymentMethod()
			{
				return Parent.GetOrganisationARAgreedPaymentMethodCore();
			}

			protected internal override ZString GetOrganisationAPAgreedPaymentMethod()
			{
				return Parent.GetOrganisationAPAgreedPaymentMethodCore();
			}

			protected internal override ZString GetOrganisationAPAccountName()
			{
				ZString result = ZString.Empty;
				if (Parent.Organisation != null && Parent.Organisation.CompanyData != null)
				{
					Parent.Organisation.CompanyData.OB_TransactionCurrency = GetCurrencyCode();
					result = Parent.Organisation.CompanyData.OB_APAccountName;
				}
				return result;
			}

			protected internal override ZString GetOrganisationAPAccountNumber()
			{
				ZString result = ZString.Empty;
				if (Parent.Organisation != null && Parent.Organisation.CompanyData != null)
				{
					Parent.Organisation.CompanyData.OB_TransactionCurrency = GetCurrencyCode();
					result = Parent.Organisation.CompanyData.OB_APAccountNumber;
				}
				return result;
			}

			protected internal override ZString GetOrganisationAPBankDetail()
			{
				ZString result = ZString.Empty;
				if (Parent.Organisation != null && Parent.Organisation.CompanyData != null)
				{
					Parent.Organisation.CompanyData.OB_TransactionCurrency = GetCurrencyCode();
					result = Parent.Organisation.CompanyData.OB_APBankName;
					if (!Parent.Organisation.CompanyData.OB_APBankBSB.IsEmpty)
					{
						result = result + "/" + Parent.Organisation.CompanyData.OB_APBankBSB;
					}
				}
				return result;
			}

			protected internal override ZString GetOrganisationCode()
			{
				return Parent.Organisation == null ? ZString.Empty : Parent.Organisation.Code;
			}

			protected internal override ZString GetOrganisationName()
			{
				return Parent.Organisation == null ? ZString.Empty : Parent.Organisation.Name;
			}

			protected internal override ZString GetOrganisationPostalAddress()
			{
				return Parent.Organisation == null ? ZString.Empty : Parent.Organisation.PostalAddress;
			}

			protected internal override ZString GetReceiptTypeDescription()
			{
				return Parent.ReceiptTypeDescription;
			}

			protected internal override ZString GetReceiptType()
			{
				return Parent.ReceiptType;
			}

			protected internal override ZString GetChequeOrReference()
			{
				return Parent.ChequeOrReference;
			}

			protected internal override ZString GetTransactionReference()
			{
				return Parent.TransactionReference;
			}

			protected internal override ZString GetTransactionNumber()
			{
				return Parent.TransactionNumber;
			}

			protected internal override ZString GetTransactionNumberPrefixed()
			{
				return Parent.TransactionNumberPrefixed;
			}

			protected internal override ZString GetRemittanceAdviceContact()
			{
				return Parent.GetRemittanceAdviceContactCore();
			}

			protected internal override ZDecimal GetOSTotalForRemittanceAdvice()
			{
				return Parent.GetOSTotalForRemittanceAdviceCore();
			}

			protected internal override ZDecimal GetSummaryTotalForRemittanceAdvice()
			{
				return Parent.GetSummaryTotalForRemittanceAdviceCore();
			}

			protected internal override ZString GetPaymentCurrencyCode()
			{
				return Parent.GetPaymentCurrencyCodeCore();
			}

			protected internal override ZString GetAccountsPayableSuppliersReference()
			{
				return Parent.AccountsPayableSuppliersReference;
			}

			protected internal override ZBool GetShowOriginalAmount()
			{
				return Parent.GetShowOriginalAmountCore();
			}

			protected internal override ZString GetCurrencyCode()
			{
				return Parent.Currency == null ? ZString.Empty : Parent.Currency.Code;
			}

			protected internal override ZString GetCurrentCompanyCurrencyCode()
			{
				return Parent.CurrentCompany == null || Parent.CurrentCompany.Currency == null ? ZString.Empty : Parent.CurrentCompany.Currency.Code;
			}

			protected internal override ZDecimal GetCurrentCompanyReciprocal()
			{
				return (ZDecimal)Parent.CurrentCompany.ExchangeRateDecimalPlaces;
			}

			protected internal override ZDecimal GetMatchLinkAmount()
			{
				return Parent.MatchLink == null ? ZDecimal.Zero : Parent.MatchLink.Amount;
			}

			protected internal override ZDecimal GetExchangeRate()
			{
				return Parent.ExchangeRate;
			}

			protected internal override ZBool GetShowOriginalAmountForPAY()
			{
				return Parent.GetShowOriginalAmountForPAYCore();
			}

			protected internal override ZBool GetShowOriginalAmountForDPY()
			{
				return Parent.GetShowOriginalAmountForDPYCore();
			}

			protected internal override ZDateTime GetCreatedDate()
			{
				return Parent.CreatedDate;
			}

			protected internal override ZString GetCreatingUser()
			{
				return Parent.CreatingUser;
			}

			protected internal override DocTransactionLineCollection GetGLLines()
			{
				return Parent.GetLinesCore();
			}

			protected internal override ZBool HasForeignCurrencyLines => Parent.HasForeignCurrencyLines;
			protected internal override DocGenericTransactionLineCollection GetPaidLines()
			{
				return Parent.GetPaidLinesCore();
			}

			protected internal override DocGenericTransactionLineCollection GetReceiptLines()
			{
				return Parent.GetReceiptLinesCore();
			}

			protected internal override DocGenericTransactionLineCollection GetFlattenedInvoices()
			{
				return Parent.GetFlattenedInvoicesCore();
			}

			protected internal override DocGenericTransactionLineCollection GetFlattenedPayments()
			{
				return Parent.GetFlattenedPaymentsCore();
			}

			protected internal override DocGenericTransactionLineCollection GetFlattenedReceiptMatches()
			{
				return Parent.GetFlattenedReceiptMatchesCore();
			}

			protected internal override DocGenericTransactionLineCollection GetFilteredFlattenedReceiptMatches()
			{
				return Parent.GetFilteredFlattenedReceiptMatchesCore();
			}

			protected internal override DocGenericTransactionLineCollection GetCostConfirmationSummaryLines()
			{
				return Parent.GetCostConfirmationSummaryLinesCore();
			}

			protected internal override DocGenericTransactionLineCollection GetOrganisationCustomAttributes()
			{
				return Parent.GetOrganisationCustomAttributesCore();
			}

			protected internal override DocGenericTransactionLineCollection GetLinesForInvoice()
			{
				return Parent.GetLinesForInvoiceCore();
			}

			protected internal override ZString GetCostConfirmationRollupSetting()
			{
				return Parent.CostConfirmationRollupSetting;
			}

			protected internal override ZString GetDesc()
			{
				return Parent.Desc;
			}

			protected internal override ZDecimal GetMatchLinkInvertedOSAmount()
			{
				return Parent.MatchLink == null ? ZDecimal.Zero : Parent.MatchLink.InvertedOSAmount;
			}

			protected internal override ZString GetBarcode()
			{
				return Parent.Barcode;
			}

			protected internal override ZDecimal GetTotalPaymentForPaymentVoucher()
			{
				return Parent.GetTotalPaymentForPaymentVoucherCore();
			}

			protected internal override ZDecimal GetTotalPaidAsForPaymentVoucher()
			{
				return Parent.GetTotalPaidAsForPaymentVoucherCore();
			}

			protected internal override ZDecimal GetExchangeRateForPaymentVoucher()
			{
				return Parent.GetExchangeRateForPaymentVoucherCore();
			}

			protected internal override ZBool GetShowNewAuthorisationFooter()
			{
				return Parent.GetShowNewAuthorisationFooterCore();
			}

			protected internal override ZString GetPreparedBy()
			{
				return Parent.GetPreparedByCore();
			}

			protected internal override ZString GetApprovalStatus()
			{
				return Parent.GetApprovalStatusCore();
			}

			protected internal override ZString GetFirstAuthorisationDescription()
			{
				return Parent.GetFirstAuthorisationDescriptionCore();
			}

			protected internal override ZString GetSecondAuthorisationDescription()
			{
				return Parent.GetSecondAuthorisationDescriptionCore();
			}

			protected internal override ZString GetThirdAuthorisationDescription()
			{
				return Parent.GetThirdAuthorisationDescriptionCore();
			}

			protected internal override ZString GetFirstAuthorisation()
			{
				return Parent.GetFirstAuthorisationCore();
			}

			protected internal override ZString GetSecondAuthorisation()
			{
				return Parent.GetSecondAuthorisationCore();
			}

			protected internal override ZString GetThirdAuthorisation()
			{
				return Parent.GetThirdAuthorisationCore();
			}

			protected internal override ZDecimal GetPaymentVoucherOSTotal()
			{
				return Parent.GetPaymentVoucherOSTotalCore();
			}

			protected internal override ZString GetChequeDrawer()
			{
				return Parent.ChequeDrawer;
			}

			protected internal override ZString GetDrawerBank()
			{
				return Parent.DrawerBank;
			}

			protected internal override ZString GetDrawerBranch()
			{
				return Parent.DrawerBranch;
			}

			protected internal override ZDecimal GetTotalOSAmount()
			{
				return Parent.TotalOSAmount;
			}

			protected internal override ZString GetConsolidatedInvoiceRef()
			{
				return Parent.ConsolidatedInvoiceRef;
			}

			protected internal override ZString GetApprovalRequestID()
			{
				return Parent.GetApprovalRequestID();
			}

			protected internal override ZDateTime GetDepositBatchBatchDate()
			{
				return Parent.DocDepositBatch == null ? ZDateTime.Empty : Parent.DocDepositBatch.BatchDate;
			}

			protected internal override ZString GetReceiptBatchNo()
			{
				return Parent.ReceiptBatchNo;
			}

			protected internal override ZString GetLedger()
			{
				return Parent.Ledger;
			}

			protected internal override ZBool GetPrintLocalValues()
			{
				return Parent.PrintLocalValues;
			}

			protected internal override ZDateTime GetFullyPaidDate()
			{
				return Parent.FullyPaidDate;
			}

			protected internal override ZDecimal GetTotalAllocatedAmount()
			{
				return Parent.TotalAllocatedAmount;
			}

			protected internal override ZDecimal GetOSOutstandingAmount()
			{
				return Parent.OSOutstandingAmount;
			}

			protected internal override ZDecimal GetTotalLocalInvoiceAmount()
			{
				return Parent.TotalLocalInvoiceAmount;
			}

			protected internal override Image GetInvoiceLogo()
			{
				return Parent.GetInvoiceLogoCore();
			}

			protected internal override ZString GetCostConfirmationDocumentTitle()
			{
				return Parent.GetCostConfirmationDocumentTitleCore();
			}

			protected internal override ZString GetAccountCode()
			{
				return Parent.GetAccountCodeCore();
			}

			protected internal override ZString GetSupplierTaxIDNumber()
			{
				return Parent.GetSupplierTaxIDNumberCore();
			}

			protected internal override ZString GetSupplierTaxIDHeading()
			{
				return Parent.GetSupplierTaxIDHeadingCore();
			}

			protected internal override ZString GetCostConfirmationHeadingText()
			{
				return Parent.GetCostConfirmationHeadingTextCore();
			}

			protected internal override ZString GetAccountName()
			{
				return Parent.GetAccountNameCore();
			}

			protected internal override ZString GetAccountFullName()
			{
				return Parent.GetAccountFullNameCore();
			}

			protected internal override ZString GetPostedBy()
			{
				return Parent.GetPostedByCore();
			}

			protected internal override ZDateTime GetDueDate()
			{
				return Parent.DueDate;
			}

			protected internal override ZDateTime GetPostDate()
			{
				return Parent.PostDate;
			}

			protected internal override ZString GetOSTaxDisplayHeading()
			{
				return Parent.GetOSTaxDisplayHeadingCore();
			}

			protected internal override ZBool GetIsTaxed()
			{
				return Parent.GetIsTaxedCore();
			}

			protected internal override ZBool GetShowInvoiceTaxDate()
			{
				return Parent.GetShowInvoiceTaxDateCore();
			}

			protected internal override ZBool GetIsTotalOSTaxAmountZero()
			{
				return Parent.GetIsTotalOSTaxAmountZeroCore();
			}

			protected internal override ZBool GetIsTotalOSTaxAmountRawZero()
			{
				return Parent.GetIsTotalOSTaxAmountRawZeroCore();
			}

			protected internal override ZString GetRecipientNameAddress()
			{
				return Parent.GetRecipientNameAddressCore();
			}

			protected internal override ZString GetInvoiceSubTotalFormatted()
			{
				return Parent.GetInvoiceSubTotalFormattedCore();
			}

			protected internal override ZString GetTotalOSTaxAmountFormatted()
			{
				return Parent.GetTotalOSTaxAmountFormattedCore();
			}

			protected internal override ZString GetTotalOSTaxAmountRawFormatted()
			{
				return Parent.GetTotalOSTaxAmountRawFormattedCore();
			}

			protected internal override ZDecimal GetTotalLocalTax_Raw()
			{
				return Parent.GetTotalLocalTax_Raw();
			}

			protected internal override ZString GetOSDocumentTitleForITAutofattura()
			{
				return Parent.GetOSDocumentTitleForITAutofattura();
			}

			protected internal override ZString GetTotalOSIGICAmountFormatted()
			{
				return Parent.GetTotalOSISICAmountFormattedCore();
			}

			protected internal override ZString GetTotalOSSERAmountFormatted()
			{
				return Parent.GetTotalOSSERAmountFormattedCore();
			}

			protected internal override ZString GetTotalOSQSTAmountFormatted()
			{
				return Parent.GetTotalOSQSTAmountFormattedCore();
			}

			protected internal override ZString GetTotalOSSBCAmountFormatted()
			{
				return Parent.GetTotalOSSBCAmountFormattedCore();
			}

			protected internal override ZString GetTotalOSKKCAmountFormatted()
			{
				return Parent.GetTotalOSKKCAmountFormattedCore();
			}

			protected internal override ZString GetTotalOSEDUPrimaryAmountFormatted()
			{
				return Parent.GetTotalOSEDUPrimaryAmountFormattedCore();
			}

			protected internal override ZString GetTotalOSEDUSecondaryAmountFormatted()
			{
				return Parent.GetTotalOSEDUSecondaryAmountFormattedCore();
			}

			protected internal override ZString GetTotalOSRETAmountFormatted()
			{
				return Parent.GetTotalOSRETAmountFormattedCore();
			}

			protected internal override ZString GetTotalOSSPVAmountFormatted()
			{
				return Parent.GetTotalOSSPVAmountFormattedCore();
			}

			protected internal override ZString GetOSTotalFormatted()
			{
				return Parent.GetOSTotalFormattedCore();
			}

			protected internal override ZString GetOSTotalRawFormatted()
			{
				return Parent.GetOSTotalRawFormattedCore();
			}

			protected internal override ZBool GetHasSERLineOnly()
			{
				return Parent.GetHasSERLineOnlyCore();
			}

			protected internal override ZBool GetHasAtLeastOneSERLine()
			{
				return Parent.GetHasAtLeastOneSERLineCore();
			}

			protected internal override ZBool GetHasVATANDIGICLine()
			{
				return Parent.GetHasVATANDIGICLineCore();
			}

			protected internal override ZBool GetHasIGICLineOnly()
			{
				return Parent.GetHasIGICLineOnlyCore();
			}

			protected internal override ZBool GetHasGSTANDQSTLine()
			{
				return Parent.GetHasGSTANDQSTLineCore();
			}

			protected internal override ZBool GetHasGSTANDQCTLine()
			{
				return Parent.GetHasGSTANDQCTLineCore();
			}

			protected internal override ZBool GetHasGSTANDEDULine()
			{
				return Parent.GetHasGSTANDEDULineCore();
			}

			protected internal override ZBool GetHasRETLine()
			{
				return Parent.GetHasRETLineCore();
			}

			protected internal override ZBool GetHasIntegratedGSTLine()
			{
				return Parent.GetHasIntegratedGSTLineCore();
			}

			protected internal override ZBool GetHasStateGSTLine()
			{
				return Parent.GetHasStateGSTLineCore();
			}

			protected internal override ZBool GetHasZeroIGSTAmount()
			{
				return Parent.GetHasZeroIGSTAmountCore();
			}

			protected internal override ZString GetTotalOSIntegratedGSTAmountFormatted()
			{
				return Parent.GetTotalOSIntegratedGSTAmountFormattedCore();
			}

			protected internal override ZString GetTotalOSCentreGSTAmountFormatted()
			{
				return Parent.GetTotalOSCentreGSTAmountFormattedCore();
			}

			protected internal override ZString GetTotalOSStateGSTAmountFormatted()
			{
				return Parent.GetTotalOSStateGSTAmountFormattedCore();
			}

			protected internal override ZDecimal GetGSTAmount()
			{
				return Parent.GSTAmount;
			}

			protected internal override ZDecimal GetOSGSTAmount()
			{
				return Parent.OSGstAmount;
			}

			protected internal override ZBool GetShowLocalGSTAmount()
			{
				return Parent.GetShowLocalGSTAmountCore();
			}

			protected internal override ZString GetOSPrimaryTaxDisplayHeading()
			{
				return Parent.GetOSPrimaryTaxDisplayHeadingCore();
			}

			protected internal override ZString GetInvoiceSubTotalDisplayHeading()
			{
				return Parent.GetInvoiceSubTotalDisplayHeadingCore();
			}

			protected internal override ZString GetOSTaxExtraRateQCTDisPlay()
			{
				return Parent.GetOSTaxExtraRateQCTDisPlayCore();
			}

			protected internal override ZString GetOSTaxExtraRateSBCDisPlay()
			{
				return Parent.GetOSTaxExtraRateSBCDisPlayCore();
			}

			protected internal override ZString GetOSTaxExtraRateKKCDisPlay()
			{
				return Parent.GetOSTaxExtraRateKKCDisPlayCore();
			}

			protected internal override ZString GetOSSPVExtraTaxLabel()
			{
				return Parent.GetOSSPVExtraTaxLabelCore();
			}

			protected internal override ZString GetOSSPVExtraTaxCodeLabel()
			{
				return Parent.GetOSSPVExtraTaxCodeLabelCore();
			}

			protected internal override ZInt GetPostPeriod()
			{
				return Parent.PostPeriod;
			}

			protected internal override ZString GetPeriodDisplay()
			{
				return Parent.GetPeriodDisplayCore();
			}

			protected internal override ZString GetAgePeriodDisplay()
			{
				return Parent.GetAgePeriodDisplayCore();
			}

			protected internal override ZString GetJournalType()
			{
				return Parent.GetJournalTypeCore();
			}

			protected internal override ZDateTime GetInvoiceTaxDate()
			{
				return Parent.GetInvoiceTaxDateCore();
			}

			protected internal override ZString GetInvoiceTaxDateHeading()
			{
				return Parent.GetInvoiceTaxDateHeadingCore();
			}

			protected internal override ZDecimal TotalDebitAmount => Parent.TotalDebitAmount;

			protected internal override ZDecimal TotalCreditAmount => Parent.TotalCreditAmount;

			[CodeStringFinderHint(typeof(JobRevenueJournal), "HumanReadableNameCore")]
			protected internal override ZString GetDocumenTitle()
			{
				return Parent.TransactionHeader.HumanReadableName;
			}

			#region Approval

			protected internal override ZString GetRequestedBy()
			{
				return Parent.GetRequestedByCore();
			}

			protected internal override ZDateTime GetRequestedTime()
			{
				return Parent.GetRequestedTimeCore();
			}

			protected internal override ZString GetApprovedBy()
			{
				return Parent.GetApprovedByCore();
			}

			protected internal override ZDateTime GetApprovedTime()
			{
				return Parent.GetApprovedTimeCore();
			}

			protected internal override ZString GetOriginalRequester()
			{
				return Parent.GetOriginalRequesterCore();
			}

			protected internal override ZString GetOriginalApprover()
			{
				return Parent.GetOriginalApproverCore();
			}

			protected internal override DocBankAccount GetAccount()
			{
				return Parent.GetAccountCore();
			}

			protected internal override DocTransactionHeaderCollection GetCashTransactions()
			{
				return Parent.GetCashTransactionsCore();
			}

			protected internal override DocTransactionHeaderCollection GetCreditCardTransactions()
			{
				return Parent.GetCreditCardTransactionsCore();
			}

			protected internal override DocTransactionHeaderCollection GetChequeTransactions()
			{
				return Parent.GetChequeTransactionsCore();
			}

			protected internal override DocTransactionHeaderCollection GetDirectCreditTransactions()
			{
				return Parent.GetDirectCreditTransactionsCore();
			}

			protected internal override DocTaxTransactionCollection GetTaxTransactions() => Parent.GetTaxTransactionsCore();

			protected internal override ZInt GetTotalChequeCount()
			{
				return Parent.GetTotalChequeCountCore();
			}

			protected internal override ZDecimal GetTotalCashAmount()
			{
				return Parent.GetTotalCashAmountCore();
			}

			protected internal override ZDecimal GetTotalCreditCardAmount()
			{
				return Parent.GetTotalCreditCardAmountCore();
			}

			protected internal override ZDecimal GetTotalChequeAmount()
			{
				return Parent.GetTotalChequeAmountCore();
			}

			protected internal override ZDecimal GetTotalDirectCreditAmount()
			{
				return Parent.GetTotalDirectCreditAmountCore();
			}

			protected internal override ZDecimal GetDepositBatchTotalAmountForBank()
			{
				return Parent.GetDepositBatchTotalAmountCoreForBank();
			}

			protected internal override ZDecimal GetDepositBatchTotalAmount()
			{
				return Parent.GetDepositBatchTotalAmountCore();
			}

			protected internal override ZString GetEInvoicingGovernmentAllocatedNumber()
			{
				return Parent.EInvoicingGovernmentAllocatedNumber;
			}

			protected internal override ZString GetEInvoicingAuthorisationNumber()
			{
				return Parent.EInvoicingAuthorisationNumber;
			}
			#endregion
		}

		public ZBool HasForeignCurrencyLines
		{
			get
			{
				if (!hasForeignCurrencyLines.HasValue)
				{
					hasForeignCurrencyLines = GetHasForeignCurrencyLines();
				}

				return hasForeignCurrencyLines.Value;
			}
		}
		ZBool? hasForeignCurrencyLines;

		protected virtual ZBool GetHasForeignCurrencyLines()
		{
			return ZBool.False;
		}

		protected virtual ZDecimal GetPaymentVoucherOSTotalCore()
		{
			return ZDecimal.Zero;
		}

		protected virtual ZString GetFirstAuthorisationCore()
		{
			return ZString.Empty;
		}

		protected virtual ZString GetSecondAuthorisationCore()
		{
			return ZString.Empty;
		}

		protected virtual ZString GetThirdAuthorisationCore()
		{
			return ZString.Empty;
		}

		protected virtual ZString GetFirstAuthorisationDescriptionCore()
		{
			return ZString.Empty;
		}

		protected virtual ZString GetSecondAuthorisationDescriptionCore()
		{
			return ZString.Empty;
		}

		protected virtual ZString GetThirdAuthorisationDescriptionCore()
		{
			return ZString.Empty;
		}

		protected virtual ZString GetApprovalStatusCore()
		{
			return ZString.Empty;
		}

		protected virtual ZString GetPreparedByCore()
		{
			return ZString.Empty;
		}

		protected virtual ZBool GetShowNewAuthorisationFooterCore()
		{
			return ZBool.False;
		}

		protected virtual ZDecimal GetExchangeRateForPaymentVoucherCore()
		{
			return ZDecimal.Zero;
		}

		protected virtual ZDecimal GetTotalPaidAsForPaymentVoucherCore()
		{
			return ZDecimal.Zero;
		}

		protected virtual ZDecimal GetTotalPaymentForPaymentVoucherCore()
		{
			return ZDecimal.Zero;
		}

		protected virtual ZString GetRemittanceAdviceContactCore()
		{
			return null;
		}

		protected virtual ZDecimal GetOSTotalForRemittanceAdviceCore()
		{
			return ZDecimal.Zero;
		}

		protected virtual ZString GetOrganisationARAgreedPaymentMethodCore()
		{
			return null;
		}

		protected virtual ZString GetOrganisationAPAgreedPaymentMethodCore()
		{
			return null;
		}

		protected virtual ZDecimal GetSummaryTotalForRemittanceAdviceCore()
		{
			return ZDecimal.Zero;
		}

		protected virtual ZString GetPaymentCurrencyCodeCore()
		{
			return null;
		}

		protected virtual ZBool GetShowOriginalAmountCore()
		{
			return false;
		}

		protected virtual ZBool GetShowOriginalAmountForPAYCore()
		{
			return false;
		}

		protected virtual ZBool GetShowOriginalAmountForDPYCore()
		{
			return false;
		}

		protected virtual DocARBatchInvoiceLineCollection GetInvoicesCore()
		{
			return null;
		}

		protected virtual DocTransactionLineCollection GetLinesCore()
		{
			return null;
		}

		protected virtual DocGenericTransactionLineCollection GetPaidLinesCore()
		{
			return null;
		}

		protected virtual DocGenericTransactionLineCollection GetReceiptLinesCore()
		{
			var result = new DocGenericTransactionLineCollection(TransactionHeader.Factory);
			if (TransactionHeader is DirectReceipt)
			{
				DirectReceipt directReceipt = (DirectReceipt)TransactionHeader;
				foreach (DirectReceiptLine line in directReceipt.Lines)
				{
					result.Add(DocGenericTransactionLine.New(line, Factory));
				}
			}
			return result;
		}

		protected virtual DocGenericTransactionLineCollection GetFlattenedInvoicesCore()
		{
			return null;
		}

		protected virtual DocGenericTransactionLineCollection GetFlattenedPaymentsCore()
		{
			return null;
		}

		protected virtual DocGenericTransactionLineCollection GetFlattenedReceiptMatchesCore()
		{
			var result = new DocGenericTransactionLineCollection(ReceiptMatches.Factory);

			foreach (FlattenedLine line in FlattenedReceiptMatches)
			{
				result.Add(DocGenericTransactionLine.New(line, Factory));
			}
			return result;
		}

		protected virtual DocGenericTransactionLineCollection GetFilteredFlattenedReceiptMatchesCore()
		{
			var result = new DocGenericTransactionLineCollection(ReceiptMatches.Factory);

			foreach (FlattenedLine line in FilteredFlattenedReceiptMatches)
			{
				result.Add(DocGenericTransactionLine.New(line, Factory));
			}
			return result;
		}

		protected virtual DocGenericTransactionLineCollection GetCostConfirmationSummaryLinesCore()
		{
			return null;
		}

		protected virtual DocGenericTransactionLineCollection GetLinesForInvoiceCore()
		{
			return null;
		}

		protected virtual DocGenericTransactionLineCollection GetOrganisationCustomAttributesCore()
		{
			return null;
		}

		protected virtual Image GetInvoiceLogoCore()
		{
			return null;
		}

		protected virtual ZString GetCostConfirmationDocumentTitleCore()
		{
			return null;
		}

		protected virtual ZString GetAccountCodeCore()
		{
			return null;
		}

		protected virtual ZString GetSupplierTaxIDNumberCore()
		{
			return null;
		}

		protected virtual ZString GetSupplierTaxIDHeadingCore()
		{
			return null;
		}

		protected virtual ZString GetCostConfirmationHeadingTextCore()
		{
			return null;
		}

		protected virtual ZString GetAccountNameCore()
		{
			return null;
		}

		protected virtual ZString GetAccountFullNameCore()
		{
			return null;
		}

		protected virtual ZString GetPostedByCore()
		{
			return null;
		}

		protected virtual ZString GetOSTaxDisplayHeadingCore()
		{
			return null;
		}

		protected virtual ZBool GetIsTaxedCore()
		{
			return false;
		}

		protected virtual ZBool GetShowInvoiceTaxDateCore()
		{
			return false;
		}

		protected virtual ZBool GetIsTotalOSTaxAmountZeroCore()
		{
			return false;
		}

		protected virtual ZBool GetIsTotalOSTaxAmountRawZeroCore()
		{
			return false;
		}

		protected virtual ZString GetRecipientNameAddressCore()
		{
			return null;
		}

		protected virtual ZString GetInvoiceSubTotalFormattedCore()
		{
			return null;
		}

		protected virtual ZString GetTotalOSTaxAmountFormattedCore()
		{
			return null;
		}

		protected virtual ZString GetTotalOSTaxAmountRawFormattedCore()
		{
			return null;
		}

		protected virtual ZDecimal GetTotalLocalTax_Raw()
		{
			return ZDecimal.Zero;
		}

		protected virtual ZString GetOSDocumentTitleForITAutofattura()
		{
			return null;
		}

		protected virtual ZString GetTotalOSISICAmountFormattedCore()
		{
			return null;
		}

		protected virtual ZString GetTotalOSSERAmountFormattedCore()
		{
			return null;
		}

		protected virtual ZString GetTotalOSQSTAmountFormattedCore()
		{
			return null;
		}

		protected virtual ZString GetTotalOSSBCAmountFormattedCore()
		{
			return null;
		}

		protected virtual ZString GetTotalOSKKCAmountFormattedCore()
		{
			return null;
		}

		protected virtual ZString GetTotalOSEDUPrimaryAmountFormattedCore()
		{
			return null;
		}

		protected virtual ZString GetTotalOSEDUSecondaryAmountFormattedCore()
		{
			return null;
		}

		protected virtual ZString GetTotalOSRETAmountFormattedCore()
		{
			return null;
		}

		protected virtual ZString GetTotalOSSPVAmountFormattedCore()
		{
			return null;
		}

		protected virtual ZString GetOSTotalFormattedCore()
		{
			return null;
		}

		protected virtual ZString GetOSTotalRawFormattedCore()
		{
			return null;
		}

		protected virtual ZBool GetHasSERLineOnlyCore()
		{
			return false;
		}

		protected virtual ZBool GetHasAtLeastOneSERLineCore()
		{
			return false;
		}

		protected virtual ZBool GetHasVATANDIGICLineCore()
		{
			return false;
		}

		protected virtual ZBool GetHasIGICLineOnlyCore()
		{
			return false;
		}

		protected virtual ZBool GetHasGSTANDQSTLineCore()
		{
			return false;
		}

		protected virtual ZBool GetHasGSTANDQCTLineCore()
		{
			return false;
		}

		protected virtual ZBool GetHasGSTANDEDULineCore()
		{
			return false;
		}

		protected virtual ZBool GetHasRETLineCore()
		{
			return false;
		}

		protected virtual ZBool GetHasIntegratedGSTLineCore()
		{
			return false;
		}

		protected virtual ZBool GetHasStateGSTLineCore()
		{
			return false;
		}

		protected virtual ZBool GetHasZeroIGSTAmountCore()
		{
			return false;
		}

		protected virtual ZString GetTotalOSIntegratedGSTAmountFormattedCore()
		{
			return null;
		}

		protected virtual ZString GetTotalOSCentreGSTAmountFormattedCore()
		{
			return null;
		}

		protected virtual ZString GetTotalOSStateGSTAmountFormattedCore()
		{
			return null;
		}

		protected virtual ZDateTime GetInvoiceTaxDateCore()
		{
			return ZDateTime.Empty;
		}

		protected virtual ZString GetInvoiceTaxDateHeadingCore()
		{
			return null;
		}

		protected virtual ZBool GetShowLocalGSTAmountCore()
		{
			return false;
		}

		protected virtual ZString GetOSPrimaryTaxDisplayHeadingCore()
		{
			return null;
		}

		protected virtual ZString GetInvoiceSubTotalDisplayHeadingCore()
		{
			return null;
		}

		protected virtual ZString GetOSTaxExtraRateQCTDisPlayCore()
		{
			return null;
		}

		protected virtual ZString GetOSTaxExtraRateSBCDisPlayCore()
		{
			return null;
		}

		protected virtual ZString GetOSTaxExtraRateKKCDisPlayCore()
		{
			return null;
		}

		protected virtual ZString GetOSSPVExtraTaxLabelCore()
		{
			return null;
		}

		protected virtual ZString GetOSSPVExtraTaxCodeLabelCore()
		{
			return null;
		}

		protected virtual ZString GetPeriodDisplayCore()
		{
			return ZString.Empty;
		}

		protected virtual ZString GetAgePeriodDisplayCore()
		{
			return ZString.Empty;
		}

		protected virtual ZString GetJournalTypeCore()
		{
			return ZString.Empty;
		}

		public ZDecimal TotalCreditAmount
		{
			get
			{
				if (!totalCreditAmount.HasValue)
				{
					totalCreditAmount = GetTotalCreditAmount();
				}

				return totalCreditAmount.Value;
			}
		}
		ZDecimal? totalCreditAmount;

		protected virtual ZDecimal GetTotalCreditAmount()
		{
			return ZDecimal.Zero;
		}

		public ZDecimal TotalDebitAmount
		{
			get
			{
				if (!totalDebitAmount.HasValue)
				{
					totalDebitAmount = GetTotalDebitAmount();
				}

				return totalDebitAmount.Value;
			}
		}
		ZDecimal? totalDebitAmount;

		protected virtual ZDecimal GetTotalDebitAmount()
		{
			return ZDecimal.Zero;
		}

		#region Approval

		protected virtual ZString GetRequestedByCore()
		{
			return null;
		}

		protected virtual ZDateTime GetRequestedTimeCore()
		{
			return ZDateTime.Empty;
		}

		protected virtual ZString GetApprovedByCore()
		{
			return null;
		}

		protected virtual ZDateTime GetApprovedTimeCore()
		{
			return ZDateTime.Empty;
		}

		protected virtual ZString GetOriginalRequesterCore()
		{
			return null;
		}

		protected virtual ZString GetOriginalApproverCore()
		{
			return null;
		}

		protected virtual DocBankAccount GetAccountCore()
		{
			return null;
		}

		protected virtual DocTransactionHeaderCollection GetCashTransactionsCore()
		{
			return null;
		}

		protected virtual DocTransactionHeaderCollection GetCreditCardTransactionsCore()
		{
			return null;
		}

		protected virtual DocTransactionHeaderCollection GetChequeTransactionsCore()
		{
			return null;
		}

		protected virtual DocTransactionHeaderCollection GetDirectCreditTransactionsCore()
		{
			return null;
		}

		protected virtual DocTaxTransactionCollection GetTaxTransactionsCore() => null;

		protected virtual ZInt GetTotalChequeCountCore()
		{
			return ZInt.Zero;
		}

		protected virtual ZDecimal GetTotalCashAmountCore()
		{
			return ZDecimal.Zero;
		}

		protected virtual ZDecimal GetTotalCreditCardAmountCore()
		{
			return ZDecimal.Zero;
		}

		protected virtual ZDecimal GetTotalChequeAmountCore()
		{
			return ZDecimal.Zero;
		}

		protected virtual ZDecimal GetTotalDirectCreditAmountCore()
		{
			return ZDecimal.Zero;
		}

		protected virtual ZDecimal GetDepositBatchTotalAmountCoreForBank()
		{
			return ZDecimal.Zero;
		}

		protected virtual ZDecimal GetDepositBatchTotalAmountCore()
		{
			return ZDecimal.Zero;
		}

		#endregion

		#region Constants

		public ZInt FirstLinePaymentWidth
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.FirstLinePaymentWidth, 0); }
		}

		public ZInt NumberOfTransactionLines
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfTransactionLines, 24); }
		}

		#endregion

		#region Collections

		protected DocTransactionHeaderCollection fReceiptMatches;
		public DocTransactionHeaderCollection ReceiptMatches
		{
			get
			{
				if (fReceiptMatches == null)
				{
					fReceiptMatches = new DocTransactionHeaderCollection(TransactionHeader.Factory);
					if (MatchLinkGroupNumbers != null)
					{
						Hashtable matchNumTransactionPKTable = new Hashtable();
						foreach (ZString aMatchLinkNum in MatchLinkGroupNumbers)
						{
							GetTransactionsForAMatchLink(aMatchLinkNum, fReceiptMatches, matchNumTransactionPKTable);
						}
					}

					fReceiptMatches.SortOnDateAndTransactionNumber();
				}

				return fReceiptMatches;
			}
		}

		public FilteredFlattenedLineCollection FilteredFlattenedReceiptMatches
		{
			get { return FilteredflattenedReceiptMatches ?? (FilteredflattenedReceiptMatches = new FilteredFlattenedLineCollection(ReceiptMatches)); }
		}
		FilteredFlattenedLineCollection FilteredflattenedReceiptMatches;

		public class FilteredFlattenedLineCollection : DocBaseWrapperCollection
		{
			public FilteredFlattenedLineCollection(DocTransactionHeaderCollection parentCollection)
				: base(parentCollection.Factory)
			{
				foreach (DocTransactionHeader header in parentCollection)
				{
					bool isSystemGeneratedClearingJournal = header.TransactionType == ZArchitecture.Core.TransactionTypes.Journal && header.IsTransactionCreatedByMatching && header.TransactionCategory == "CLR";

					if (!isSystemGeneratedClearingJournal)
					{
						if ((header.MatchedLines.Count == 0))
						{
							this.Add(FlattenedLine.New(header, Factory));
						}
						else
						{
							foreach (DocTransactionLine line in header.MatchedLines)
							{
								FlattenedLine newLine = FlattenedLine.New(header, Factory);
								newLine.Line = line;
								this.Add(newLine);
							}
						}
					}
				}
			}
		}

		public FlattenedLineCollection FlattenedReceiptMatches
		{
			get { return flattenedReceiptMatches ?? (flattenedReceiptMatches = new FlattenedLineCollection(ReceiptMatches)); }
		}
		FlattenedLineCollection flattenedReceiptMatches;

		public class FlattenedLineCollection : DocBaseWrapperCollection
		{
			public FlattenedLineCollection(DocTransactionHeaderCollection parentCollection)
				: base(parentCollection.Factory)
			{
				foreach (DocTransactionHeader header in parentCollection)
				{
					if (header.MatchedLines.Count == 0)
					{
						this.Add(FlattenedLine.New(header, Factory));
					}
					else
					{
						foreach (DocTransactionLine line in header.MatchedLines)
						{
							FlattenedLine newLine = FlattenedLine.New(header, Factory);
							newLine.Line = line;
							this.Add(newLine);
						}
					}
				}
			}
		}

		public class FlattenedLine : DocBaseWrapper, IGenericTransactionLinePlugIn
		{
			public static FlattenedLine New(DocTransactionHeader header, BusinessObjectFactory factoryToWrap)
			{
				return new FlattenedLine(header);
			}

			FlattenedLine(DocTransactionHeader header)
				: base(null, header.Factory)
			{
				this.Header = header;
			}

			public DocTransactionHeader Header
			{
				get;
				private set;
			}

			public DocTransactionLine Line
			{
				get;
				set;
			}

			#region IGenericTransactionLinePlugIn members

			GenericTransactionLineSupporter IGenericTransactionLinePlugIn.LineSupporter
			{
				get { return fGenericTransactionSupporter ?? (fGenericTransactionSupporter = new DocFlattenedLineGenericTransactionSupporter(this)); }
			}
			DocFlattenedLineGenericTransactionSupporter fGenericTransactionSupporter;

			#endregion

			class DocFlattenedLineGenericTransactionSupporter : GenericTransactionLineSupporter
			{
				public DocFlattenedLineGenericTransactionSupporter(FlattenedLine parent)
				{
					this.Parent = parent;
				}
				protected readonly FlattenedLine Parent;

				protected internal override DocTransactionHeader GetFlattenedHeaderForTransactionHeader()
				{
					return Parent.Header;
				}

				protected internal override DocTransactionLine GetFlattenedLine()
				{
					return Parent.Line;
				}
			}
		}

		protected DocTransactionLineCollection fMatchedLines;
		public DocTransactionLineCollection MatchedLines
		{
			get
			{
				if (fMatchedLines == null)
				{
					fMatchedLines = new DocTransactionLineCollection(Factory);
					if (TransactionHeader != null && MatchLink != null && MatchLink.WrappedObject != null)
					{
						ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccTransactionLines));
						query.AddToFilter(AccTransactionLinesSchema.AL_AH, TransactionHeader.PK);
						query.AddToFilter(AccTransactionLinesSchema.AL_GC, TransactionHeader.AH_GC);
						ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(AccTransLinePay), AccTransLinePaySchema.A7_AL);
						ZQuery filter = new ZQuery(AccTransLinePaySchema.A7_AP, ((AccTransactionMatchLink)MatchLink.WrappedObject).PK);
						subQuery.AddToFilter(filter);
						query.AddSubQuery(subQuery, JoinCondition.And);

						TransactionLine[] lines = Factory.Load<TransactionLine>(query);
						AccTransLinePay[] linesPay = Factory.Load<AccTransLinePay>(filter);
						DocTransactionLine[] docLines = new DocTransactionLine[lines.Length];
						int index = 0;
						foreach (TransactionLine line in lines)
						{
							ZDecimal tempAmount = 0;
							ZDecimal tempOSAmount = 0;
							foreach (AccTransLinePay linePay in linesPay)
							{
								if (linePay.A7_AL == line.PK)
								{
									tempAmount = linePay.A7_Amount;
									tempOSAmount = linePay.A7_OSAmount;
									break;
								}
							}
							docLines[index] = DocTransactionLine.New(line, Factory);
							docLines[index].MatchedAmount = tempAmount;
							docLines[index].MatchedOSAmount = tempOSAmount;
							index++;
						}
						fMatchedLines.AddRange(docLines);
					}
				}
				return fMatchedLines;
			}
		}

		#endregion

		static ZString ReplaceMICRSpecialCharacters(ZString stringForReplace)
		{
			return stringForReplace.Replace("-", DashSymbol);
		}

		internal static string BuildMICRNumber(ZString chequeOrReference, ZString routingTransitNumber, DocBankAccount bankAccount)
		{
			string fMICRNumber = "";
			ZStringBuilder mICRBuilder = new ZStringBuilder();
			mICRBuilder.Append(OnUSSymbol);
			mICRBuilder.Append(ReplaceMICRSpecialCharacters(chequeOrReference));
			mICRBuilder.Append(OnUSSymbol);
			mICRBuilder.Append(Spacer);

			mICRBuilder.Append(TransitNumberSymbol);
			mICRBuilder.Append(ReplaceMICRSpecialCharacters(routingTransitNumber));
			mICRBuilder.Append(TransitNumberSymbol);
			mICRBuilder.Append(Spacer);

			var bankAccountNumber = bankAccount != null ? ReplaceMICRSpecialCharacters(bankAccount.AccountNum) : ZString.Empty;

			mICRBuilder.Append(bankAccountNumber);
			mICRBuilder.Append(OnUSSymbol);
			fMICRNumber = mICRBuilder.ToString();
			return fMICRNumber;
		}

		#region Remittance Advice And Payment Voucher Fields

		public ZString ReceiptTypeDescription
		{
			get
			{
				return GetReceiptTypeDescriptionFromReceiptType(ReceiptType);
			}
		}

		internal static ZString GetReceiptTypeDescriptionFromReceiptType(ZString receiptType)
		{
			if (receiptType == ReceiptTypes.Cash)
			{
				return Res.GetString("4044e09e-b436-4ba6-ad78-445dc969d6fc", "Cash");
			}

			if (receiptType == ReceiptTypes.Cheque)
			{
				return UseUSSpelling ? Res.GetString("423e392e-7a66-4a7f-b0ec-dad4cba0e353", "Check") : Res.GetString("113283ba-d8b2-4b13-ba00-529d6da10ec3", "Cheque");
			}

			if (receiptType == ReceiptTypes.CreditCard)
			{
				return Res.GetString("666ff34d-eb07-4b6c-9b51-ea711b5affc6", "Credit Card");
			}

			if (receiptType == ReceiptTypes.DirectCredit)
			{
				return Res.GetString("b3479c44-9136-4f5c-9e59-61462ada9828", "Direct Credit");
			}

			if (receiptType == ReceiptTypes.DirectDebit)
			{
				return Res.GetString("f0cc2533-f69a-4057-891b-9b4055256554", "Direct Debit");
			}

			return "";
		}

		static bool UseUSSpelling
		{
			get { return GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates; }
		}

		public ZDecimal ApportionedAmount { get; set; }

		public ZString CurrencyAndOrganisation
		{
			get
			{
				ZString result = ZString.Empty;
				if (Currency != null)
				{
					result += Currency.Code;
				}

				if (Organisation != null)
				{
					result += Organisation.Code;
				}

				return result;
			}
		}

		public ZBool IsTransactionCreatedByMatching
		{
			get { return TransactionHeader.AH_TransactionCreatedByMatching; }
		}

		public ZString PaymentItemJobReference
		{
			get { return TransactionHeader != null && TransactionHeader.JobNumber.IsEmpty ? string.Empty : " - " + Res.GetString("28bb505f-d389-4fbd-bcaf-e27e0e8bae21", "Job {0}", TransactionHeader.JobNumber); }
		}

		public ZString JobNumber
		{
			get { return TransactionHeader != null && TransactionHeader.JobNumber.IsEmpty ? TransactionHeader.TransactionNumberPrefixed : TransactionHeader.JobNumber; }
		}

		public ZString LedgerDescription
		{
			get
			{
				ZString description;

				switch (Ledger)
				{
					case ZArchitecture.Core.LedgerTypes.AccountsReceivable:
						description = GlbCompany.CurrentCompany.GC_Name.ToUpper() + " " + Res.GetString("7b4065d7-5fba-4e78-a47b-b859c0ca07b3", "RECEIVABLE TRANSACTIONS");
						break;
					case ZArchitecture.Core.LedgerTypes.AccountsPayable:
						description = Res.GetString("13f7eaaa-c64d-4f4b-b4ff-f7527a193691", "PAYABLE TRANSACTIONS");
						break;
					case ZArchitecture.Core.LedgerTypes.CashBook:
						description = Res.GetString("eb1f073b-967e-47d0-b649-bf157ac862c7", "CASHBOOK TRANSACTIONS");
						break;
					default:
						description = ZString.Empty;
						break;
				}

				return description;
			}
		}

		#endregion

		#region Receipt Matching Fields

		public DocMatchLink MatchLink { get; set; }

		public DocMatchLinkCollection MatchLinks
		{
			get
			{
				if (fMatchLinks == null)
				{
					fMatchLinks = GetMatchLinks(TransactionHeader.PK);
				}
				return fMatchLinks;
			}
		}
		DocMatchLinkCollection fMatchLinks;

		protected ArrayList MatchLinkGroupNumbers
		{
			get
			{
				ArrayList result = new ArrayList();
				foreach (DocMatchLink docMatchLink in GetMatchLinks(TransactionHeader.PK))
				{
					result.Add(docMatchLink.MatchGroupNum);
				}
				return result;
			}
		}

		public DocDepositBatch DocDepositBatch
		{
			get
			{
				DocDepositBatch result = null;
				if (!ReceiptBatchNo.IsEmpty)
				{
					ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.ReceiptBatch);
					filter.AddToFilter(AccTransactionHeaderSchema.AH_ReceiptBatchNo, ReceiptBatchNo);
					filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, TransactionHeader.AH_GC);
					var batch = Factory.LoadTop1<DepositBatch>(filter);
					if (batch != null)
					{
						result = DocDepositBatch.New(batch, Factory);
					}
				}
				return result;
			}
		}

		protected ZString fIsCurrentTransactionHeader;
		public ZString IsCurrentTransactionHeader
		{
			get { return fIsCurrentTransactionHeader; }
			set { fIsCurrentTransactionHeader = value; }
		}
		public ZDecimal TotalOSAmount
		{
			get { return TransactionType == ZArchitecture.Core.TransactionTypes.ExchangeDifference ? InvoiceAmount : OSTotal; }
		}

		protected DocTransactionLineCollection fDirectReceiptLines;
		public DocTransactionLineCollection DirectReceiptLines
		{
			get
			{
				if (fDirectReceiptLines == null)
				{
					fDirectReceiptLines = new DocTransactionLineCollection(TransactionHeader.Factory);
					if (TransactionHeader is DirectReceipt)
					{
						DirectReceipt directReceipt = (DirectReceipt)TransactionHeader;

						foreach (DirectReceiptLine line in directReceipt.Lines)
						{
							fDirectReceiptLines.Add(DocTransactionLine.New(line, Factory));
						}
					}
				}
				return fDirectReceiptLines;
			}
		}

		public ZBool PrintLocalValues
		{
			get
			{
				foreach (DocTransactionHeader transaction in ReceiptMatches)
				{
					if (transaction.Currency != null && CurrentCompany.Currency != null)
					{
						if (transaction.Currency.Code != CurrentCompany.Currency.Code)
						{
							return ZBool.True;
						}
					}
				}
				return ZBool.False;
			}
		}
		#endregion

		#region Properties

		public override string ToString()
		{
			return TransactionNum;
		}

		public static new ZString TableName
		{
			get { return AccTransactionHeaderSchema.Constants.TableName; }  // wtf?
		}

		public ZBool ShowLocalAmountAndExRateOnInvoice
		{
			get
			{
				ZString invoiceLineDisplayOption = GetInvoiceLineDisplayOption();
				return OrgInvoiceRollupOrGroup.InvoiceLineDisplayOptionIncludesExchangeRate(invoiceLineDisplayOption);
			}
		}

		protected virtual ZString GetInvoiceLineDisplayOption()
		{
			ZString result = ZString.Empty;

			if (GenericInvoicingJob != null && TransactionHeader.Header != null)
			{
				result = new OrgInvoiceRollupOrGroup.Loader(TransactionHeader).GetInvoiceLineDisplayOption(
				GetServiceDirection(GenericInvoicingJob.Origin, GenericInvoicingJob.Destination), GenericInvoicingJob.TransportMode, Mode, GenericInvoicingJob.JobType);
			}

			return result;
		}

		public ZString CostConfirmationRollupSetting
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.CostConfirmationDocumentRollupSettings.Value;
			}
		}

		public ZString TransactionNumber
		{
			get { return TransactionNum; }
		}

		public ZString ReferenceNumber
		{
			get
			{
				if (AccountingConfigurationRegistry.Instance.UseJobNumberBasedInvoiceNumbers.Value && !ConsolidatedInvoiceRef.IsEmpty)
				{
					return ConsolidatedInvoiceRef;
				}
				else
				{
					return TransactionNumberPrefixed;
				}
			}
		}

		public ZString TransactionARPaymentMethod
		{
			get
			{
				var result = string.IsNullOrWhiteSpace(TransactionHeader.AH_AgreedPaymentMethodOverride)
						? string.Empty
						: OrganisationsDataRegistry.Instance.ReceivablesCreditAgreedPaymentMethodsList.Value.GetCodeDescriptionPairList().GetDescriptionFromCode(TransactionHeader.AH_AgreedPaymentMethodOverride);

				if (!string.IsNullOrEmpty(result))
				{
					result = GetPaymentMethodSubGroup(result);
				}

				return result;
			}
		}

		public ZString OrganisationARTermsPaymentMethod
		{
			get
			{
				ZString result = ZString.Empty;
				if (Organisation != null && Organisation.CompanyData != null)
				{
					if (IsDisbursement)
					{
						result = Organisation.CompanyData.GetPaymentMethodFromARTerms(Enterprise.MasterFiles.Business.OrgARTermsLookups.InvoiceTypes.DSB.Code);
					}
					else
					{
						result = Organisation.CompanyData.GetPaymentMethodFromARTerms(Enterprise.MasterFiles.Business.OrgARTermsLookups.InvoiceTypes.All.Code);
					}

					if (result.IsEmpty)
					{
						result = Organisation.CompanyData.OB_ARCreditAgreedPaymentMethod;
					}

					result = GetPaymentMethodSubGroup(result);
				}
				return result;
			}
		}

		ZString GetPaymentMethodSubGroup(ZString paymentMethod)
		{
			var result = paymentMethod;
			if (paymentMethod == OrgDescriptions.CreditAgreedPaymentMethods.CollectionRequest.ToString())
			{
				// further sub catgegory them into with and without collection batch
				if (TransactionHeader.IsUsedByActiveCollectionOrderLine)
				{
					result += Res.GetString("509a9b53-fb57-4cb6-a4cb-254f8e4bfb6d", " (attached to collection batch)");
				}
				else
				{
					result += Res.GetString("e620009a-7e68-4365-b673-9a66ceed9fea", " (not attached to collection batch)");
				}
			}
			return result;
		}

		public ZString StatementDescription
		{
			get { return StatementDescriptionCore; }
		}

		protected JobHeader InvoicingJob
		{
			get
			{
				Job job = Factory.Load<Job>(TransactionHeader.AH_JH);
				if (job != null)
				{
					job.InitializeParentFromGenericJobWithoutSettingDefaults();
				}
				return job;
			}
		}

		protected virtual ZString StatementDescriptionCore
		{
			get
			{
				ZString result = ZString.Empty;

				if (ConsolidatedInvoiceRef.IsEmpty)
				{
					result = Desc;
				}
				else if (!ConsolidatedInvoiceRef.IsEmpty)
				{
					if (IsConsolJob())
					{
						ZString[] consolNumber = ConsolidatedInvoiceRef.Split('/');
						var consolBisObj = Factory.LoadFromNaturalKey<ForwardingConsol>(JobConsolSchema.JK_UniqueConsignRef, consolNumber[0]);
						if (consolBisObj != null && consolBisObj.JK_MasterBillNum != "")
						{
							result = "MASTER: " + consolBisObj.JK_MasterBillNum;
						}
					}
					else if (IsShipmentJob())
					{
						ZString[] shipmentNumber = ConsolidatedInvoiceRef.Split('/');
						var shipmentBisObj = Factory.LoadFromNaturalKey<ForwardingShipment>(JobShipmentSchema.JS_UniqueConsignRef, shipmentNumber[0]);
						if (shipmentBisObj != null && shipmentBisObj.JS_HouseBill != "")
						{
							result = "HOUSE: " + shipmentBisObj.JS_HouseBill;
						}
					}
					else if (IsShippingJob)
					{
						ZString[] shipmentNumber = ConsolidatedInvoiceRef.Split('/');
						var shipmentBisObj = Factory.LoadFromNaturalKey<AgencyShipment>(JobShipmentSchema.JS_UniqueConsignRef, shipmentNumber[0]);
						if (shipmentBisObj != null && shipmentBisObj.JS_HouseBill != "")
						{
							result = (NoResString)"BILL No: " + shipmentBisObj.JS_HouseBill;
						}
					}
					else if (IsDeclarationJob())
					{
						ZString[] brokerageNumber = ConsolidatedInvoiceRef.Split('/');
						BaseJobDeclaration declarationBisObj = BaseJobDeclaration.LoadFirstMatchingInCurrentCompanyIncludingInActive(Factory, brokerageNumber[0]);
						if (declarationBisObj != null && declarationBisObj.JE_HouseBill != "")
						{
							result = "HOUSE: " + declarationBisObj.JE_HouseBill;
						}
					}

					if (AccountingConfigurationRegistry.Instance.UseJobNumberBasedInvoiceNumbers.Value)
					{
						result += (NoResString)" PAYMENT REF: " + TransactionNum;
					}
					else
					{
						result += (NoResString)" JOB: " + ConsolidatedInvoiceRef;
					}
				}

				if (!TransactionReference.IsEmpty)
				{
					if (ComplianceSubType != ZString.Empty)
					{
						result += " " + ComplianceSubTypeLocalDescription + " " + TransactionReference;
					}
					else if (TransactionHeader.IsGovtTaxInvoice)
					{
						result += (NoResString)" GOVT TAX INV: " + TransactionReference.ToString();
					}
				}

				return result;
			}
		}

		bool IsDeclarationJob()
		{
			return InvoicingJob != null &&
				InvoicingJob.JH_ParentTableCode == JobDeclarationSchema.Constants.Prefix;
		}

		bool IsShipmentJob()
		{
			bool fIsShipmentJob = false;
			if (InvoicingJob != null && InvoicingJob.JH_ParentTableCode == JobShipmentSchema.Constants.Prefix)
			{
				ForwardingShipment shipment = Factory.Load(typeof(ForwardingShipment), InvoicingJob.JH_ParentID) as ForwardingShipment;
				fIsShipmentJob = (shipment != null && (!shipment.JS_IsCFSRegistered || shipment.JS_IsForwardRegistered) && !shipment.JS_IsShipping);
			}
			return fIsShipmentJob;
		}

		bool IsShippingJob
		{
			get
			{
				bool result = false;
				if (InvoicingJob != null && InvoicingJob.JH_ParentTableCode == JobShipmentSchema.Constants.Prefix)
				{
					AgencyShipment shipment = Factory.Load(typeof(AgencyShipment), InvoicingJob.JH_ParentID) as AgencyShipment;
					result = (shipment != null && shipment.JS_IsShipping);
				}
				return result;
			}
		}

		bool IsConsolJob()
		{
			return (InvoicingJob == null && !ConsolidatedInvoiceRef.IsEmpty);
		}

		public ZString StatementPortOfLoading
		{
			get
			{
				ZString result = ZString.Empty;

				if (!ConsolidatedInvoiceRef.IsEmpty)
				{
					if (IsConsolJob())
					{
						ZString[] consolNumber = ConsolidatedInvoiceRef.Split('/');
						var consolBisObj = Factory.LoadFromNaturalKey<ForwardingConsol>(JobConsolSchema.JK_UniqueConsignRef, consolNumber[0]);
						if (consolBisObj != null && consolBisObj.LoadPort != null)
						{
							result = consolBisObj.LoadPort.Code;
						}
					}
					else if (IsShipmentJob())
					{
						ZString[] shipmentNumber = ConsolidatedInvoiceRef.Split('/');
						var shipmentBisObj = Factory.LoadFromNaturalKey<ForwardingShipment>(JobShipmentSchema.JS_UniqueConsignRef, shipmentNumber[0]);
						if (shipmentBisObj != null && shipmentBisObj.Origin != null)
						{
							result = shipmentBisObj.Origin.Code;
						}
					}
					else if (IsDeclarationJob())
					{
						ZString[] brokerageNumber = ConsolidatedInvoiceRef.Split('/');
						BaseJobDeclaration declarationBisObj = BaseJobDeclaration.LoadFirstMatchingInCurrentCompanyIncludingInActive(Factory, brokerageNumber[0]);
						if (declarationBisObj != null && declarationBisObj.Origin != null)
						{
							result = declarationBisObj.Origin.Code;
						}
					}
				}

				return result;
			}
		}

		public ZString StatementPortOfDischarge
		{
			get
			{
				ZString result = ZString.Empty;

				if (!ConsolidatedInvoiceRef.IsEmpty)
				{
					if (IsConsolJob())
					{
						ZString[] consolNumber = ConsolidatedInvoiceRef.Split('/');
						var consolBisObj = Factory.LoadFromNaturalKey<ForwardingConsol>(JobConsolSchema.JK_UniqueConsignRef, consolNumber[0]);
						if (consolBisObj != null && consolBisObj.DischargePort != null)
						{
							result = consolBisObj.DischargePort.Code;
						}
					}
					else if (IsShipmentJob())
					{
						ZString[] shipmentNumber = ConsolidatedInvoiceRef.Split('/');
						var shipmentBisObj = Factory.LoadFromNaturalKey<ForwardingShipment>(JobShipmentSchema.JS_UniqueConsignRef, shipmentNumber[0]);
						if (shipmentBisObj != null && shipmentBisObj.Destination != null)
						{
							result = shipmentBisObj.Destination.Code;
						}
					}
					else if (IsDeclarationJob())
					{
						ZString[] brokerageNumber = ConsolidatedInvoiceRef.Split('/');
						BaseJobDeclaration declarationBisObj = BaseJobDeclaration.LoadFirstMatchingInCurrentCompanyIncludingInActive(Factory, brokerageNumber[0]);
						if (declarationBisObj != null && declarationBisObj.FinalDestination != null)
						{
							result = declarationBisObj.FinalDestination.Code;
						}
					}
				}

				return result;
			}
		}

		public ZString Disbursement
		{
			get
			{
				if (IsDisbursement)
				{
					return (NoResString)" (Disbursement)";
				}
				return ZString.Empty;
			}
		}

		public ZString CreatingUser
		{
			get { return TransactionHeader.CreatingUser; }
		}

		public ZString ChequeDrawer
		{
			get { return TransactionHeader.AH_ChequeDrawer; }
		}

		public ZString ChequeOrReference
		{
			get { return TransactionHeader.AH_ChequeOrReference; }
		}

		public ZString ChequeNumber
		{
			get { return ChequeOrReference; }
		}
		public ZString ConsolidatedInvoiceRef
		{
			get { return TransactionHeader.AH_ConsolidatedInvoiceRef; }
		}

		protected virtual ZString GetApprovalRequestID()
		{
			return ZString.Empty;
		}

		public ZString Desc
		{
			get { return TransactionHeader.AH_Desc; }
		}

		public ZString DrawerBank
		{
			get { return TransactionHeader.AH_DrawerBank; }
		}

		public ZString DrawerBranch
		{
			get { return TransactionHeader.AH_DrawerBranch; }
		}

		public ZString InvoiceTerm
		{
			get { return TransactionHeader.AH_InvoiceTerm; }
		}

		public ZString Ledger
		{
			get { return TransactionHeader.AH_Ledger; }
		}

		public ZString ReceiptBatchNo
		{
			get { return TransactionHeader.AH_ReceiptBatchNo; }
		}

		public ZString ReceiptType
		{
			get { return TransactionHeader.AH_ReceiptType; }
		}

		public ZDateTime DepositBatchBatchDate
		{
			get { return TransactionHeader.AH_InvoiceDate; }
		}

		public ZString TransactionNum
		{
			get { return TransactionHeader.AH_TransactionNum; }
		}

		public ZString TransactionNumberPrefixed
		{
			get { return TransactionHeader.TransactionNumberPrefixed; }
		}

		public ZString TransactionReference
		{
			get { return TransactionHeader.AH_TransactionReference; }
		}

		public ZString TransactionType
		{
			get { return TransactionHeader.AH_TransactionType; }
		}

		public ZString ComplianceNumber
		{
			get { return TransactionHeader.AH_TransactionReference; }
		}

		public ZString ComplianceSubType
		{
			get
			{
				return TransactionHeader.AH_ComplianceSubType;
			}
		}

		public ZString ComplianceSubTypeLocalDescription
		{
			get
			{
				return AccComplianceSequence.GetDocumentTitle(ComplianceSubType, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			}
		}

		public ZString ComplianceNumberPrefixOnly => ComplianceSequence != null ? ComplianceSequence.XD_Prefix : ZString.Empty;

		public ZString ComplianceNumberWithoutPrefix
		{
			get
			{
				ZString complianceNumberPrefix = ComplianceNumberPrefixOnly;
				if (!complianceNumberPrefix.IsEmpty && ComplianceNumber.StartsWith(complianceNumberPrefix))
				{
					return ComplianceNumber.Substring(complianceNumberPrefix.Length);
				}
				else
				{
					return ComplianceNumber;
				}
			}
		}

		public DocAccountingVoucher Voucher
		{
			get
			{
				if (voucher == null)
				{
					var supporter = TransactionHeader.DocumentSupporter as TransactionHeader.TransactionHeaderDocumentSupporter;
					if (supporter != null && supporter.ShouldSupportAccountingVoucher())
					{
						var transactionheader = TransactionHeaderHelper.ReplaceTransferToWithTransferFromWhenPrintingAccountingVoucher(new TransactionHeader[] { TransactionHeader })[0];
						voucher = DocAccountingVoucher.New(transactionheader, transactionheader.Factory);
					}
				}

				return voucher;
			}
		}
		DocAccountingVoucher voucher;

		AccComplianceSequence ComplianceSequence
		{
			get
			{
				if (complianceSequence == null)
				{
					try
					{
						complianceSequence = TransactionHeader.ComplianceSequence;
					}
					catch (ComplianceSequenceRelatedException)
					{ }
				}
				return complianceSequence;
			}
		}
		AccComplianceSequence complianceSequence;

		OrgHeader ComplianceBookBranchOrgFallBackToCompanyOrg
		{
			get
			{
				OrgHeader result = null;

				if (ComplianceSequence != null)
				{
					GlbBranch branchOwner = ComplianceSequence.BranchOwner;
					if (branchOwner != null)
					{
						if (branchOwner.OrgProxy != null)
						{
							result = branchOwner.OrgProxy;
						}
						else if (branchOwner.Company != null && branchOwner.Company.OrgProxy != null)
						{
							result = branchOwner.Company.OrgProxy;
						}
					}
				}
				return result;
			}
		}

		public ZString ComplianceBookBranchFullName
		{
			get
			{
				ZString result = ZString.Empty;
				if (ComplianceBookBranchOrgFallBackToCompanyOrg != null)
				{
					result = ComplianceBookBranchOrgFallBackToCompanyOrg.OH_FullName;
				}
				return result;
			}
		}

		public ZString ComplianceBookPrintingAuthorizationNumber => !ComplianceNumber.IsEmpty && ComplianceSequence != null ? ComplianceSequence.XD_PrintingAuthorizationNumber : ZString.Empty;

		public ZString BookPrintingAuthorizationHeading
		{
			get
			{
				var result = Res.GetString("46ecb007-e1ca-45bd-aec4-8bd314730e14", "Authorization Number");
				if (!string.IsNullOrEmpty(ComplianceBookPrintingAuthorizationNumber))
				{
					switch (GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
					{
						case Core.Constants.CountryCodes.Honduras:
							result = "CAI";
							break;
						case Core.Constants.CountryCodes.CostaRica:
						case Core.Constants.CountryCodes.Guatemala:
							result = "RESOLUCION";
							break;
						case Core.Constants.CountryCodes.Panama:
							result = "DGI";
							break;
					}
				}
				return result;
			}
		}

		public ZString TransactionUniqueNumber
		{
			get
			{
				var result = string.Empty;

				var transactionAuthorizationNumber = ObjectFactory.Get<ICountryComplianceFactory>()?.GetITransactionAuthorizationNumber(TransactionHeader.Company.GC_RN_NKCountryCode);
				if (transactionAuthorizationNumber?.IsTransactionAuthorizationNumberEnabled(TransactionHeader.AH_GC, TransactionHeader.AH_InvoiceDate) ?? false)
				{
					result = string.Format(CultureInfo.InvariantCulture, "{0}:{1}", transactionAuthorizationNumber.GetTransactionAuthorizationNumberLabel(), TransactionHeader.AuthorizationNumberReference);
				}

				return result;
			}
		}

		public ZDateTime ComplianceBookExpiryDate => ComplianceSequence != null && ComplianceSequence.XD_ExpiryDate.IsValid ? ComplianceSequence.XD_ExpiryDate : ZDateTime.Empty;

		public ZString ComplianceBookExpiryHeading => ComplianceBookExpiryDate.IsValid ? Res.GetString("a6179e33-2d92-49f6-ba4d-eb54666435c4", "Expiry Date") : string.Empty;

		public ZString ComplianceBookBranchMainAddress
		{
			get
			{
				ZString result = ZString.Empty;
				if (ComplianceBookBranchOrgFallBackToCompanyOrg != null && ComplianceBookBranchOrgFallBackToCompanyOrg.MainAddress != null)
				{
					result = DocAddress.New(ComplianceBookBranchOrgFallBackToCompanyOrg.MainAddress, Factory).PostalAddress;
				}
				return result;
			}
		}

		public ZString ComplianceBookBranchCity
		{
			get
			{
				ZString result = ZString.Empty;
				if (ComplianceBookBranchOrgFallBackToCompanyOrg != null)
				{
					result = ComplianceBookBranchOrgFallBackToCompanyOrg.CityFallback;
				}
				return result;
			}
		}

		public DocTransactionHeader ParentTransaction
		{
			get
			{
				DocTransactionHeader result = null;
				if (!TransactionHeader.AH_TransactionBelongsToGroup.IsEmpty)
				{
					result = DocTransactionHeader.New(Factory, TransactionHeader.AH_TransactionBelongsToGroup);
				}
				return result;
			}
		}

		public ZString CreditTermsLabel
		{
			get
			{
				if (TransactionType == "INV" && Ledger == "AR")
				{
					return "TERMS";
				}
				return ZString.Empty;
			}
		}

		public ZString TransactionCategory
		{
			get { return TransactionHeader.AH_TransactionCategory; }
		}

		public virtual ZString YourReference
		{
			get { return ZString.Empty; }
		}

		public virtual ZString OurReference
		{
			get { return ZString.Empty; }
		}
		public virtual ZDecimal InvoiceAmountWithGST
		{
			get
			{
				ZDecimal result = 0M;

				if (TransactionHeader != null)
				{
					DataRow row = ((INeedRow)TransactionHeader).Row;

					if (row != null)
					{
						result = new ZDecimal(row[AccTransactionHeader.Schema.AH_OSTotal]);
					}
				}
				return result * AmountMultiplierForARCreditNote;
			}
		}

		public virtual ZDecimal AccountMovementBalance
		{
			get
			{
				return ZDecimal.Zero;
			}
		}

		public ZString TransactionTypeAndNum
		{
			get { return TransactionType + " " + TransactionNum; }
		}

		public ZDecimal Balance
		{
			get { return BalanceCore; }
		}

		protected virtual ZDecimal BalanceCore
		{
			get
			{
				ZDecimal result = 0M;

				if (TransactionHeader != null)
				{
					DataRow row = ((INeedRow)TransactionHeader).Row;

					if (row != null)
					{
						ZDecimal localTotal = new ZDecimal(TransactionHeader[AccTransactionHeader.Schema.AH_LocalTotal]);
						ZDecimal oSTotal = new ZDecimal(row[AccTransactionHeader.Schema.AH_OSTotal]);
						ZDecimal outstandingAmount = new ZDecimal(row[AccTransactionHeader.Schema.AH_OutstandingAmount]);
						ZDecimal osOutstandingAmount = new ZDecimal(row[AccTransactionHeader.Schema.AH_OSOutstandingAmount]);

						bool isPreviousPeriod = !fEndOfStatementPeriodForCalculatingMatchedAmount.IsEmpty &&
							fEndOfStatementPeriodForCalculatingMatchedAmount.IsValid &&
							fEndOfStatementPeriodForCalculatingMatchedAmount.Date <= ZDateTime.Today; // Using <= comparison due to EndOfStatementPeriod being 12:00:00 AM of the next day

						if (isPreviousPeriod)
						{
							(ZDecimal localMatchLinkAmount, ZDecimal osMatchLinkAmount) = CalculatedMatchLinkAmountForStatement();
							outstandingAmount = localTotal - localMatchLinkAmount;
							osOutstandingAmount = oSTotal - osMatchLinkAmount;
						}

						ZString transactionCurrency = new ZString(row[AccTransactionHeader.Schema.AH_RX_NKTransactionCurrency]);

						if (localTotal == outstandingAmount)
						{
							result = CalculateBalanceInLocalCurrency ? outstandingAmount : oSTotal;
						}
						else if (outstandingAmount == ZDecimal.Zero)
						{
							result = ZDecimal.Zero;
						}
						else if (CalculateBalanceInLocalCurrency)
						{
							result = outstandingAmount;
						}
						else
						{
							result = TransactionHeaderOSOutstandingAmountProvider.IsFeatureEnabled(TransactionHeader) ?
								osOutstandingAmount :
								TransactionHeaderOSOutstandingAmountProvider.GetHighPrecisionOSOutstandingAmount(localTotal, oSTotal, outstandingAmount, transactionCurrency);
						}
					}
				}

				return result;
			}
		}

		protected virtual bool CalculateBalanceInLocalCurrency
		{
			get { return false; }
		}

		public ZDecimal CalculatedMatchedAmountForStatement
		{
			get
			{
				return CalculatedMatchLinkAmountForStatement().LocalAmount;
			}
		}

		(ZDecimal LocalAmount, ZDecimal OSAmount) CalculatedMatchLinkAmountForStatement()
		{
			var filter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, TransactionHeader.PK);

			if (!fEndOfStatementPeriodForCalculatingMatchedAmount.IsEmpty && fEndOfStatementPeriodForCalculatingMatchedAmount.IsValid)
			{
				filter.AddToFilter(AccTransactionMatchLinkSchema.AP_MatchDate, SQLComparisonOperator.LessThan, fEndOfStatementPeriodForCalculatingMatchedAmount);
			}

			var matchLinks = new AccTransactionMatchLinkCollection(Factory, filter);
			matchLinks.Load();

			ZDecimal localAmount = 0;
			ZDecimal osAmount = 0;

			foreach (AccTransactionMatchLink matchLink in matchLinks)
			{
				localAmount += matchLink.AP_Amount;
				osAmount += matchLink.AP_OSAmount;
			}

			return (localAmount, osAmount);
		}

		public ZDateTime EndOfStatementPeriodForCalculatingMatchedAmount
		{
			get { return fEndOfStatementPeriodForCalculatingMatchedAmount; }
			set { fEndOfStatementPeriodForCalculatingMatchedAmount = value; }
		}

		ZDateTime fEndOfStatementPeriodForCalculatingMatchedAmount;

		protected int AmountMultiplierForARCreditNote
		{
			get
			{
				return TransactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable &&
					TransactionHeader.AH_TransactionType == TransactionTypes.CreditNote &&
					AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty) ? -1 : 1;
			}
		}

		internal int Multiplier => (TransactionHeader as ITransactionHeader).Multiplier * AmountMultiplierForARCreditNote;

		public ZDecimal TotalAllocatedAmount
		{
			get { return TotalOSAmount - OSOutstandingAmount; }
		}

		public ZDecimal OSOutstandingAmount
		{
			get { return TransactionHeader.AH_Calc_OSOutstandingAmount * AmountMultiplierForARCreditNote; }
		}
		public ZDecimal OSGstAmount
		{
			get { return TransactionHeader.AH_OSTaxAmount * AmountMultiplierForARCreditNote; }
		}

		public ZDecimal OSExTaxAmount
		{
			get { return TransactionHeader.AH_OSExTaxAmount * AmountMultiplierForARCreditNote; }
		}

		public ZDecimal TotalLocalInvoiceAmount
		{
			get { return TransactionHeader.AH_LocalTotalAmount * AmountMultiplierForARCreditNote; }
		}

		public ZDecimal CurrentCompanyReciprocal
		{
			get { return GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces; }
		}

		public ZDecimal Debit
		{
			get { return TransactionHeader.Debit; }
		}

		public ZDecimal Credit
		{
			get { return TransactionHeader.Credit; }
		}

		public ZDecimal ExchangeRate
		{
			get { return TransactionHeader.AH_ExchangeRate; }
		}

		public virtual ZDecimal GSTAmount
		{
			get { return TransactionHeader.AH_LocalTaxAmount * AmountMultiplierForARCreditNote; }
		}

		public ZDecimal InvoiceAmount
		{
			get { return TransactionHeader.AH_LocalExTaxAmount * AmountMultiplierForARCreditNote; }
		}

		public ZDecimal OSTotal
		{
			get { return TransactionHeader.AH_OSTotalAmount * AmountMultiplierForARCreditNote; }
		}

		public ZDecimal OutstandingAmount
		{
			get { return TransactionHeader.AH_OutstandingAmount * AmountMultiplierForARCreditNote; }
		}

		public ZDecimal WithholdingTax
		{
			get { return TransactionHeader.AH_WithholdingTax * AmountMultiplierForARCreditNote; }
		}

		public ZDateTime TransactionDueDate
		{
			get
			{
				if (TransactionType != "REC")
				{
					return DueDate;
				}
				return ZDateTime.Empty;
			}
		}

		public ZDateTime CreatedDate
		{
			get { return TransactionHeader.CreatedDate; }
		}

		public ZDateTime DueDate
		{
			get { return TransactionHeader.AH_DueDate; }
		}

		public ZDateTime FullyPaidDate
		{
			get { return TransactionHeader.AH_FullyPaidDate; }
		}

		public ZDateTime InvoiceDate
		{
			get { return TransactionHeader.AH_InvoiceDate; }
		}

		public ZDate ComplianceDocDate
		{
			get { return TransactionHeader.AH_ComplianceDocumentDate; }
		}

		public ZDateTime PostDate
		{
			get { return TransactionHeader.AH_PostDate; }
		}

		public ZInt CalcLineNumber
		{
			get { return fCalcLineNumber; }
			set { fCalcLineNumber = value; }
		}

		public ZInt PostPeriod
		{
			get { return TransactionHeader.PostPeriod; }
		}

		public ZInt AgePeriod
		{
			get { return TransactionHeader.AgePeriod; }
		}

		public DocStaff Creator
		{
			get
			{
				return DocStaff.New(TransactionHeader.Creator, Factory);
			}
		}

		public DocBankAccount BankAccount
		{
			get { return DocBankAccount.New(TransactionHeader.BankAccount, Factory); }
		}

		public ZString RoutingTransitNumber
		{
			get { return BankAccount != null ? BankAccount.BSB : ZString.Empty; }
		}

		public ZString RoutingTransitNumberFractionForm
		{
			get
			{
				return BuildRoutingTransitNumberFractionForm(RoutingTransitNumber);
			}
		}

		internal static ZString BuildRoutingTransitNumberFractionForm(ZString routingTransitNumber)
		{
			ZString firstPart = routingTransitNumber.Length > 4 ? routingTransitNumber.Substring(4, Math.Min(4, routingTransitNumber.Length - 4)) : ZString.Empty;
			ZString secondPart = routingTransitNumber.Length > 0 ? routingTransitNumber.Substring(0, Math.Min(4, routingTransitNumber.Length)) : ZString.Empty;
			return firstPart + "/" + secondPart;
		}

		public DocGLAccount GLAccount
		{
			get { return DocGLAccount.New(TransactionHeader.GLHeader, Factory); }
		}

		public DocBranch Branch
		{
			get { return DocBranch.New(TransactionHeader.Branch, Factory); }
		}

		public DocBranch TaxBranch
		{
			get
			{
				var branch = TransactionHeader.TaxBranch ?? TransactionHeader.Branch;
				return DocBranch.New(branch, Factory);
			}
		}

		public DocDepartment Department
		{
			get { return DocDepartment.New(TransactionHeader.Department, Factory); }
		}

		public override DocJobHeader JobHeader
		{
			get { return DocJobHeader.New(InvoicingJob, Factory); }
		}

		public DocOrganisation Organisation
		{
			get { return DocOrganisation.New(TransactionHeader.Header, Factory); }
		}

		public DocCurrency Currency
		{
			get { return DocCurrency.New(TransactionHeader.TransactionCurrency, Factory); }
		}

		public ZString LocalRX_NK
		{
			get { return TransactionHeader.AH_Calc_LocalRXCode; }
		}

		public ZGuid TransactionBelongsToGroup
		{
			get { return TransactionHeader.AH_TransactionBelongsToGroup; }
		}

		public ZBool CashBasisGSTIndicator
		{
			get { return TransactionHeader.AH_CashBasisGSTIndicator; }
		}

		public ZBool CashBasisGSTRealisedToGL
		{
			get { return TransactionHeader.AH_CashBasisGSTRealisedToGL; }
		}

		public ZBool InvoiceApproved
		{
			get { return TransactionHeader.AH_InvoiceApproved; }
		}

		public ZBool InvoicePrinted
		{
			get { return TransactionHeader.AH_InvoicePrinted; }
		}

		public ZBool IsCancelled
		{
			get { return TransactionHeader.AH_IsCancelled; }
		}

		public ZBool IsClearedInCashbook
		{
			get { return !TransactionHeader.AH_DateClearedInCashbook.IsEmpty; }
		}

		public ZBool IsDisbursement
		{
			get { return TransactionHeader.AH_IsDisbursementCalc; }
		}

		public ZBool NotAllocated
		{
			get { return TransactionHeader.AH_NotAllocated; }
		}

		public ZBool POST1
		{
			get { return TransactionHeader.AH_POST1; }
		}

		public ZBool POST2
		{
			get { return TransactionHeader.AH_POST2; }
		}

		public ZBool POST3
		{
			get { return TransactionHeader.AH_POST3; }
		}

		public ZBool POST4
		{
			get { return TransactionHeader.AH_POST4; }
		}

		public ZBool PostedToEFT
		{
			get { return TransactionHeader.AH_PostedToEFT; }
		}

		public ZByte InvoiceTermDays
		{
			get { return TransactionHeader.AH_InvoiceTermDays; }
		}

		public ZByte TransactionCount
		{
			get { return TransactionHeader.AH_TransactionCount; }
		}

		public ZString AccountsPayableSuppliersReference
		{
			get
			{
				ZString result = "";
				ZQuery query = new ZQuery(OrgCusCodeSchema.OK_OH, TransactionHeader.AH_OH);
				query.AddToFilter(JoinCondition.And, OrgCusCodeSchema.OK_RN_NKCodeCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				query.AddToFilter(JoinCondition.And, OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.AccountsPayableSuppliersReference);
				var orgCusCode = Factory.LoadTop1<OrgCusCode>(query);

				if (orgCusCode != null)
				{
					result = orgCusCode.OK_CustomsRegNo;
				}
				return result;
			}
		}

		public FreightWrapper OperationsJob
		{
			get
			{
				return OpJobHelper.OperationsJob;
			}
		}

		public DocAmountByChargeCodeCollection AmountSplittedByChargeCode
		{
			get
			{
				if (fAmountSplittedByChargeCode == null)
				{
					SplitAmountByChargeCode();
				}
				return fAmountSplittedByChargeCode;
			}
		}
		protected DocAmountByChargeCodeCollection fAmountSplittedByChargeCode;

		public DocAmountByChargeCodeCollectionExcept SumOfAmountExceptTheseChargeCodes
		{
			get
			{
				if (fAmountByChargeCodeCollectionExcept == null)
				{
					fAmountByChargeCodeCollectionExcept = new DocAmountByChargeCodeCollectionExcept(new BusinessObjectFactory());
					if (AmountSplittedByChargeCode != null)
					{
						foreach (DocAmountByChargeCode item in AmountSplittedByChargeCode)
						{
							fAmountByChargeCodeCollectionExcept.Add(item);
						}
					}
				}
				return fAmountByChargeCodeCollectionExcept;
			}
		}
		protected DocAmountByChargeCodeCollectionExcept fAmountByChargeCodeCollectionExcept;

		public ZString ServiceNames
		{
			get
			{
				ZString result = ZString.Empty;
				if (OperationsJob != null && OperationsJob.Services != null)
				{
					var svcNames = new List<ZString>();
					foreach (ServiceWrapper item in OperationsJob.Services)
					{
						svcNames.Add(item.Type.Code);
					}
					result = ZString.Join(", ", svcNames.ToArray());
				}
				return result;
			}
		}

		public ZString NoOfItems
		{
			get
			{
				ZString result = ZString.Empty;
				if (OperationsJob != null && OperationsJob.ShipmentOuterPacksQty != null)
				{
					result = ZString.Format("{0} {1}", FormatNumber(OperationsJob.ShipmentOuterPacksQty.Value, 2), OperationsJob.ShipmentOuterPacksQty.Unit.Code);
				}
				return result;
			}
		}

		public ZString Volume
		{
			get
			{
				ZString result = ZString.Empty;
				if (OperationsJob != null && OperationsJob.Volume != null)
				{
					result = ZString.Format("{0} {1}", FormatNumber(OperationsJob.Volume.Value, 2), OperationsJob.Volume.Unit.Code);
				}
				return result;
			}
		}

		public ZString Weight
		{
			get
			{
				ZString result = ZString.Empty;
				if (OperationsJob != null && OperationsJob.Weight != null)
				{
					result = ZString.Format("{0} {1}", FormatNumber(OperationsJob.Weight.Value, 2), OperationsJob.Weight.Unit.Code);
				}
				return result;
			}
		}

		public ZString ChargeableWeight
		{
			get
			{
				ZString result = ZString.Empty;
				if (OperationsJob != null && OperationsJob.ChargeableWeight != null)
				{
					result = ZString.Format("{0} {1}", FormatNumber(OperationsJob.ChargeableWeight.Value, 2), OperationsJob.ChargeableWeight.Unit.Code);
				}
				return result;
			}
		}

		public ZString PickupAddressCity
		{
			get
			{
				ZString result = ZString.Empty;
				if (OperationsJob != null && OperationsJob.PickupAddress != null)
				{
					result = OperationsJob.PickupAddress.City;
				}
				return result;
			}
		}

		public ZString PickupAddressZone
		{
			get
			{
				ZString result = ZString.Empty;
				if (OperationsJob != null && OperationsJob.PickupAddress != null && OperationsJob.PickupAddress.Factory != null && OperationsJob.PickupAddress.Location != null)
				{
					result = OperationsJob.PickupAddress.Location.UNLOCO;
				}
				return result;
			}
		}

		public ZString DeliveryAddressCity
		{
			get
			{
				ZString result = ZString.Empty;
				if (OperationsJob != null && OperationsJob.DeliveryAddress != null)
				{
					result = OperationsJob.DeliveryAddress.City;
				}
				return result;
			}
		}

		public ZString DeliveryAddressZone
		{
			get
			{
				ZString result = ZString.Empty;
				if (OperationsJob != null && OperationsJob.DeliveryAddress != null && OperationsJob.DeliveryAddress.Factory != null && OperationsJob.DeliveryAddress.Location != null)
				{
					result = OperationsJob.DeliveryAddress.Location.UNLOCO;
				}
				return result;
			}
		}

		#endregion

		#region Barcode

		public ZString Barcode
		{
			get { return BarcodeGenerator.CreateDocumentBarcode(TransactionHeader.DocManagerInfo.DocManagerCode, TransactionHeader.AH_TransactionNum, TransactionHeader.AH_TransactionType, TransactionHeader.Company.GC_Code).TextAs128sFontString; }
		}

		BarcodeGenerator BarcodeGenerator
		{
			get { return barcodeGenerator ?? (barcodeGenerator = new BarcodeGenerator()); }
		}

		BarcodeGenerator barcodeGenerator;

		#endregion

		#region Invoice Batch Header

		public ZGuid TransactionPK
		{
			get { return TransactionHeader.PK; }
		}

		public DocTransactionHeader InvoiceBatchHeader
		{
			get { return DocTransactionHeader.New(Factory, TransactionHeader.AH_AH_InvoiceStatement); }
		}

		public ZGuid InvoiceBatchHeaderPK
		{
			get { return TransactionHeader.AH_AH_InvoiceStatement; }
		}

		#endregion

		#region EInvoicing

		public ZString EInvoicingGovernmentAllocatedNumber
		{
			get
			{
				return TransactionHeader.EInvoicingGovernmentAllocatedNumber;
			}
		}

		public ZString EInvoicingAuthorisationNumber
		{
			get
			{
				return TransactionHeader.EInvoicingAuthorisationNumber;
			}
		}
		#endregion

		#region Implementation

		protected delegate DocTransactionHeader NewDelegate(TransactionHeader transactionHeader, BusinessObjectFactory factoryToWrap);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		protected ZInt fCalcLineNumber;

		public TransactionHeader TransactionHeader
		{
			get { return (TransactionHeader)WrappedObject; }
		}

		protected DocMatchLinkCollection GetMatchLinks(ZGuid transactionHeaderPK)
		{
			DocMatchLinkCollection result = new DocMatchLinkCollection(TransactionHeader.Factory);
			ZQuery filter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, transactionHeaderPK);
			AccTransactionMatchLink[] allMatchLink = Factory.Load<AccTransactionMatchLink>(filter);
			foreach (AccTransactionMatchLink aMatchLink in allMatchLink)
			{
				result.Add(DocMatchLink.New(aMatchLink, Factory));
			}
			return result;
		}

		protected void GetTransactionsForAMatchLink(ZString matchLinkNum, DocTransactionHeaderCollection result, Hashtable matchNumTransactionPKTable)
		{
			ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(AccTransactionMatchLink), AccTransactionMatchLinkSchema.AP_AH);
			subQuery.AddToFilter(AccTransactionMatchLinkSchema.AP_MatchGroupNum, matchLinkNum);
			filter.AddSubQuery(subQuery, JoinCondition.And);

			TransactionHeader[] transactionHeaders = (TransactionHeader[])Factory.Load(typeof(TransactionHeader), filter);

			if (transactionHeaders != null)
			{
				foreach (TransactionHeader aTransactionHeader in transactionHeaders)
				{
					if (aTransactionHeader.Branch != null && aTransactionHeader.Branch.Company.PK == GlbCompany.CurrentCompany.PK)
					{
						DocMatchLinkCollection matchLinkColl = GetMatchLinks(aTransactionHeader.PK);
						if (matchLinkColl != null && matchLinkColl.Count > 0)
						{
							foreach (DocMatchLink link in matchLinkColl)
							{
								if (MatchLinkGroupNumbers.Contains(link.MatchGroupNum))
								{
									if (!matchNumTransactionPKTable.ContainsKey(link.MatchGroupNum + aTransactionHeader.PK.ToString()))
									{
										DocTransactionHeader aDocTransactionHeader = DocTransactionHeader.New(aTransactionHeader, Factory);
										aDocTransactionHeader.MatchLink = link;
										result.Add(aDocTransactionHeader);
										SetReceiptMatchingFlag(aDocTransactionHeader);
										matchNumTransactionPKTable.Add(link.MatchGroupNum + aTransactionHeader.PK.ToString(), link);
									}
								}
							}
						}
					}
				}
				result.Sort(nameof(InvoiceDate), ListSortDirection.Ascending);
			}
		}

		protected void SetReceiptMatchingFlag(DocTransactionHeader aTransactionHeader)
		{
			if (aTransactionHeader != null)
			{
				if (aTransactionHeader.TransactionType == TransactionType && aTransactionHeader.TransactionNum == TransactionNum)
				{
					aTransactionHeader.IsCurrentTransactionHeader = "*";
				}
			}
		}

		OperationsJobHelper OpJobHelper
		{
			get
			{
				if (jobHelper == null)
				{
					jobHelper = new OperationsJobHelper(JobHeader, null, null, Factory);
				}
				return jobHelper;
			}
		}
		OperationsJobHelper jobHelper;

		protected virtual void SplitAmountByChargeCode()
		{
			fAmountSplittedByChargeCode = new DocAmountByChargeCodeCollection(Factory);
			if (TransactionHeader is TransactionHeaderWithLines)
			{
				var lines = (TransactionHeader as TransactionHeaderWithLines).Lines;
				if (lines != null)
				{
					foreach (TransactionLine line in lines)
					{
						if (line.ChargeCode != null)
						{
							var amntChargeCode = new AmountByChargeCode(line.ChargeCode.AC_Code, line.AL_OSAmount, line.AL_OSExTaxAmount);
							fAmountSplittedByChargeCode.Add(DocAmountByChargeCode.New(amntChargeCode, Factory));
						}
					}
				}
			}
		}
		#endregion
	}
}
