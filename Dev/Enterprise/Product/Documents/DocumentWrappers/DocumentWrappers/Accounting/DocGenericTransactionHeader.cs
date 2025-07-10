using System;
using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentWrappers.Accounting.TaxFramework;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public interface IGenericTransactionHeaderPlugIn
	{
		GenericTransactionHeaderSupporter HeaderSupporter { get; }
	}

	public abstract class GenericTransactionHeaderSupporter
	{
		protected internal virtual ZString GetReferenceNumber()
		{
			return null;
		}

		protected internal virtual ZString GetStatementDescription()
		{
			return null;
		}

		protected internal virtual ZDateTime GetInvoiceTaxDate()
		{
			return ZDateTime.Empty;
		}

		protected internal virtual ZString GetInvoiceTaxDateHeading()
		{
			return null;
		}

		protected internal virtual ZString GetDisbursement()
		{
			return null;
		}

		protected internal virtual ZDateTime GetTransactionDueDate()
		{
			return ZDateTime.Empty;
		}

		protected internal virtual ZDecimal GetInvoiceAmountWithGST()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZDecimal GetBalance()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual DocOrganisation GetOrganisation()
		{
			return null;
		}

		protected internal abstract ZString GetTransactionType();

		protected internal virtual ZBool GetIsCancelled()
		{
			return false;
		}

		protected internal virtual ZDateTime GetInvoiceDate()
		{
			return ZDateTime.Empty;
		}

		protected internal virtual ZDate GetComplianceDocDate()
		{
			return ZDate.Empty;
		}

		protected internal virtual ZString GetBankAccountCode()
		{
			return null;
		}

		protected internal virtual ZString GetCheckBookCode()
		{
			return null;
		}

		protected internal virtual ZString GetOrganisationECRCode()
		{
			return null;
		}

		protected internal virtual ZString GetTransactionARPaymentMethod()
		{
			return null;
		}

		protected internal virtual ZString GetTransactionAPPaymentMethod()
		{
			return null;
		}

		protected internal virtual ZString GetOrganisationARTermsPaymentMethod()
		{
			return null;
		}

		protected internal virtual ZString GetOrganisationARAgreedPaymentMethod()
		{
			return null;
		}

		protected internal virtual ZString GetOrganisationAPAgreedPaymentMethod()
		{
			return null;
		}

		protected internal virtual ZString GetOrganisationAPAccountName()
		{
			return null;
		}

		protected internal virtual ZString GetOrganisationAPAccountNumber()
		{
			return null;
		}

		protected internal virtual ZString GetOrganisationAPBankDetail()
		{
			return null;
		}

		protected internal virtual ZString GetOrganisationCode()
		{
			return null;
		}

		protected internal virtual ZString GetOrganisationName()
		{
			return null;
		}

		protected internal virtual ZString GetOrganisationPostalAddress()
		{
			return null;
		}

		protected internal virtual ZGuid GetOrganisationARAddressOrgAddressPK()
		{
			return ZGuid.Empty;
		}

		protected internal virtual ZBool GetDisbursementInvoicesOnly()
		{
			return false;
		}

		protected internal virtual ZDateTime GetDisplayDate()
		{
			return ZDateTime.Empty;
		}

		protected internal virtual ZString GetDocumenTitle()
		{
			return null;
		}

		protected internal virtual ZString GetTaxId()
		{
			return null;
		}

		protected internal virtual ZBool GetIsStrongHeading()
		{
			return false;
		}

		protected internal virtual ZBool GetIsStatement()
		{
			return false;
		}

		protected internal virtual ZBool GetIsAccountMovementListing()
		{
			return false;
		}

		protected internal virtual ZString GetAccountMovementListingGroupBy()
		{
			return ZString.Empty;
		}

		protected internal virtual ZDecimal GetAccountMovementOpeningBalance()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZDecimal GetAccountMovementClosingBalance()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZDateTime GetAccountMovementOpeningBalanceDate()
		{
			return ZDateTime.Empty;
		}

		protected internal virtual ZDateTime GetAccountMovementClosingBalanceDate()
		{
			return ZDateTime.Empty;
		}

		protected internal virtual ZString GetOpeningText()
		{
			return null;
		}

		protected internal virtual ZBool GetIssueBySettlementGroup()
		{
			return false;
		}

		protected internal virtual ZString GetCreditBalanceMessage()
		{
			return null;
		}

		protected internal virtual ZBool GetHasCreditBalance()
		{
			return false;
		}

		protected internal virtual ZString GetTotalCurrent()
		{
			return null;
		}

		protected internal virtual ZString GetTotalOverdue()
		{
			return null;
		}

		protected internal virtual ZBool GetDisplayNotYetOutstandingAmount()
		{
			return false;
		}

		protected internal virtual ZString GetReceiptsNotMatched()
		{
			return null;
		}

		protected internal virtual ZString GetMessage()
		{
			return null;
		}

		protected internal virtual ZString GetStatementBalanceFormatted()
		{
			return null;
		}

		protected internal virtual ZString GetPaymentRequestText()
		{
			return null;
		}

		protected internal virtual ZString GetReceiptBankAccountBSB()
		{
			return null;
		}

		protected internal virtual ZString GetReceiptBankAccountSWIFT()
		{
			return null;
		}

		protected internal virtual ZString GetReceiptBankAccountAccountNum()
		{
			return null;
		}

		protected internal virtual ZString GetReceiptBankAccountBankName()
		{
			return null;
		}

		protected internal virtual ZString GetReceiptBankAccountBankAddress()
		{
			return null;
		}

		protected internal virtual ZString GetMailToAddressWithCountry()
		{
			return null;
		}

		protected internal virtual ZString GetSTDTerms()
		{
			return null;
		}

		protected internal virtual ZString GetDSBTerms()
		{
			return null;
		}

		protected internal virtual ZBool GetShouldHideCompanyName()
		{
			return false;
		}

		protected internal virtual ZString GetReceiptTypeDescription()
		{
			return null;
		}

		protected internal virtual ZString GetReceiptType()
		{
			return null;
		}

		protected internal virtual ZString GetChequeOrReference()
		{
			return null;
		}

		protected internal virtual ZString GetStatus()
		{
			return null;
		}

		protected internal virtual ZString GetTransactionReference()
		{
			return null;
		}

		protected internal virtual ZString GetTransactionNumber()
		{
			return null;
		}

		protected internal virtual ZString GetComplianceSubType()
		{
			return null;
		}

		protected internal virtual ZString GetTransactionNumberPrefixed()
		{
			return null;
		}

		protected internal virtual ZString GetRemittanceAdviceContact()
		{
			return null;
		}

		protected internal virtual ZDecimal GetOSTotalForRemittanceAdvice()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZString GetPaymentCurrencyCode()
		{
			return null;
		}

		protected internal virtual ZString GetAccountsPayableSuppliersReference()
		{
			return null;
		}

		protected internal virtual ZBool GetShowOriginalAmount()
		{
			return false;
		}

		protected internal virtual ZBool GetShowOriginalAmountForDPY()
		{
			return false;
		}

		protected internal virtual ZBool GetShowOriginalAmountForPAY()
		{
			return false;
		}

		protected internal virtual ZString GetCurrencyCode()
		{
			return null;
		}

		protected internal virtual ZString GetCurrentCompanyCurrencyCode()
		{
			return null;
		}

		protected internal virtual ZInt GetCurrentCompanyCurrencyDecimalPlaces()
		{
			return GlbCompany.CurrentCompany.GetLocalDecimals();
		}

		protected internal virtual ZDecimal GetExchangeRate()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZDecimal GetSummaryTotalForRemittanceAdvice()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZString GetDesc()
		{
			return null;
		}

		protected internal virtual ZString GetReportingBookCode()
		{
			return null;
		}

		protected internal virtual ZString GetReportingBookDescription()
		{
			return null;
		}

		protected internal virtual ZString GetChartCode()
		{
			return null;
		}

		protected internal virtual ZString GetChartDescription()
		{
			return null;
		}

		protected internal virtual ZBool GetDisplayParentAccount()
		{
			return ZBool.False;
		}

		protected internal virtual ZDecimal GetMatchLinkInvertedOSAmount()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZDecimal GetMatchLinkAmount()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZDateTime GetCreatedDate()
		{
			return ZDateTime.Empty;
		}

		protected internal virtual ZDateTime GetPostDate()
		{
			return ZDateTime.Empty;
		}

		protected internal virtual ZString GetCreatingUser()
		{
			return ZString.Empty;
		}

		protected internal virtual ZDecimal GetInvoiceAmount()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual DocARBatchInvoiceLineCollection GetInvoices()
		{
			return null;
		}

		protected internal virtual ZDecimal GetOSTotal()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZString GetSecondaryTransactionType()
		{
			return null;
		}

		protected internal virtual ZString GetSecondaryTransactionNumber()
		{
			return null;
		}

		protected internal virtual ZString GetSecondaryBankAccountCode()
		{
			return null;
		}

		protected internal virtual ZDecimal GetSecondaryInvoiceAmount()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZDecimal GetSecondaryOSTotal()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZDecimal GetSecondaryExchangeRate()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZString GetSecondaryCurrencyCode()
		{
			return null;
		}

		protected internal virtual ZString GetChargesTransactionType()
		{
			return null;
		}

		protected internal virtual ZString GetChargesTransactionNumber()
		{
			return null;
		}

		protected internal virtual ZString GetChargesBankAccountCode()
		{
			return null;
		}

		protected internal virtual ZDecimal GetChargesInvoiceAmountAndTax()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZDecimal GetChargesOSTotal()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZString GetChargesDesc()
		{
			return null;
		}

		protected internal virtual ZString GetChargesCurrencyCode()
		{
			return null;
		}

		protected internal virtual ZDecimal GetChargesExchangeRate()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZString GetBarcode()
		{
			return null;
		}

		protected internal virtual ZDecimal GetTotalPaymentForPaymentVoucher()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZDecimal GetTotalPaidAsForPaymentVoucher()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZDecimal GetExchangeRateForPaymentVoucher()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual DocTransactionHeaderCollection GetTransactions()
		{
			return null;
		}

		protected internal virtual DocGenericTransactionHeaderCollection GetGenericTransactions()
		{
			return null;
		}

		protected internal virtual DocAccountingJournalCollection GetRelatedJournals()
		{
			return null;
		}

		protected internal virtual DocGenericTransactionLineCollection GetFlattenedInvoices()
		{
			return null;
		}

		protected internal virtual DocGenericTransactionLineCollection GetFlattenedPayments()
		{
			return null;
		}

		protected internal virtual DocGenericTransactionLineCollection GetFlattenedReceiptMatches()
		{
			return null;
		}

		protected internal virtual DocGenericTransactionLineCollection GetFilteredFlattenedReceiptMatches()
		{
			return null;
		}

		protected internal virtual DocTransactionLineCollection GetGLLines()
		{
			return null;
		}

		protected internal virtual DocGenericTransactionLineCollection GetAJLines()
		{
			return null;
		}

		protected internal virtual DocAccountingJournalTaxDetailCollection GetTaxDetails()
		{
			return null;
		}

		protected internal virtual ZBool HasForeignCurrencyLines => ZBool.False;

		protected internal virtual DocGenericTransactionLineCollection GetPaidLines()
		{
			return null;
		}

		protected internal virtual DocGenericTransactionLineCollection GetReceiptLines()
		{
			return null;
		}

		protected internal virtual DocGenericTransactionLineCollection GetStatementSummaryLines()
		{
			return null;
		}

		protected internal virtual DocGenericTransactionLineCollection GetCostConfirmationSummaryLines()
		{
			return null;
		}

		protected internal virtual DocGenericTransactionLineCollection GetOrganisationCustomAttributes()
		{
			return null;
		}

		protected internal virtual DocGenericTransactionLineCollection GetLinesForInvoice()
		{
			return null;
		}

		protected internal virtual DocGenericTransactionLineCollection GetLinesForCashAdvanceRequestHeader()
		{
			return null;
		}

		protected internal virtual ZString GetCostConfirmationRollupSetting()
		{
			return null;
		}

		protected internal virtual ZBool GetShowNewAuthorisationFooter()
		{
			return ZBool.False;
		}

		protected internal virtual ZString GetPreparedBy()
		{
			return null;
		}

		protected internal virtual ZString GetApprovalStatus()
		{
			return null;
		}

		protected internal virtual ZString GetFirstAuthorisationDescription()
		{
			return null;
		}

		protected internal virtual ZString GetSecondAuthorisationDescription()
		{
			return null;
		}

		protected internal virtual ZString GetThirdAuthorisationDescription()
		{
			return null;
		}

		protected internal virtual ZString GetFirstAuthorisation()
		{
			return null;
		}

		protected internal virtual ZString GetSecondAuthorisation()
		{
			return null;
		}

		protected internal virtual ZString GetThirdAuthorisation()
		{
			return null;
		}

		protected internal virtual ZDecimal GetPaymentVoucherOSTotal()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZString GetChequeDrawer()
		{
			return null;
		}

		protected internal virtual ZString GetDrawerBank()
		{
			return null;
		}

		protected internal virtual ZString GetDrawerBranch()
		{
			return null;
		}

		protected internal virtual ZDecimal GetTotalOSAmount()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZDecimal GetTotalOSPaidAmount()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZDecimal GetTotalLocalAmount()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZDecimal GetTotalLocalPaidAmount()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZString GetConsolidatedInvoiceRef()
		{
			return null;
		}

		protected internal virtual ZString GetApprovalRequestID()
		{
			return null;
		}

		protected internal virtual ZDateTime GetDepositBatchBatchDate()
		{
			return ZDateTime.Empty;
		}

		protected internal virtual ZString GetReceiptBatchNo()
		{
			return null;
		}

		protected internal virtual ZString GetLedger()
		{
			return null;
		}

		protected internal virtual ZBool GetPrintLocalValues()
		{
			return ZBool.False;
		}

		protected internal virtual ZDateTime GetFullyPaidDate()
		{
			return ZDateTime.Empty;
		}

		protected internal virtual ZDecimal GetTotalAllocatedAmount()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZDecimal GetOSOutstandingAmount()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZDecimal GetTotalLocalInvoiceAmount()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual Image GetStatementLogo()
		{
			return null;
		}

		protected internal virtual Image GetInvoiceLogo()
		{
			return null;
		}

		protected internal virtual ZString GetCostConfirmationDocumentTitle()
		{
			return null;
		}

		protected internal virtual ZString GetAccountCode()
		{
			return null;
		}

		protected internal virtual ZString GetSupplierTaxIDNumber()
		{
			return null;
		}

		protected internal virtual ZString GetSupplierTaxIDHeading()
		{
			return null;
		}

		protected internal virtual ZString GetCostConfirmationHeadingText()
		{
			return null;
		}

		protected internal virtual ZString GetAccountName()
		{
			return null;
		}

		protected internal virtual ZString GetAccountFullName()
		{
			return null;
		}

		protected internal virtual ZDateTime GetDueDate()
		{
			return ZDateTime.Empty;
		}

		protected internal virtual ZString GetPostedBy()
		{
			return null;
		}

		protected internal virtual ZString GetOSTaxDisplayHeading()
		{
			return null;
		}

		protected internal virtual ZBool GetIsTaxed()
		{
			return false;
		}

		protected internal virtual ZBool GetShowInvoiceTaxDate()
		{
			return false;
		}

		protected internal virtual ZBool GetIsTotalOSTaxAmountZero()
		{
			return false;
		}

		protected internal virtual ZBool GetIsTotalOSTaxAmountRawZero()
		{
			return false;
		}

		protected internal virtual ZString GetRecipientNameAddress()
		{
			return null;
		}

		protected internal virtual ZString GetInvoiceSubTotalFormatted()
		{
			return null;
		}

		protected internal virtual ZString GetTotalOSTaxAmountFormatted()
		{
			return null;
		}

		protected internal virtual ZString GetOSDocumentTitleForITAutofattura()
		{
			return null;
		}

		protected internal virtual ZString GetTotalOSTaxAmountRawFormatted()
		{
			return null;
		}

		protected internal virtual ZDecimal GetTotalLocalTax_Raw()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZString GetTotalOSIGICAmountFormatted()
		{
			return null;
		}

		protected internal virtual ZString GetTotalOSSERAmountFormatted()
		{
			return null;
		}

		protected internal virtual ZString GetTotalOSQSTAmountFormatted()
		{
			return null;
		}

		protected internal virtual ZString GetTotalOSSBCAmountFormatted()
		{
			return null;
		}

		protected internal virtual ZString GetTotalOSKKCAmountFormatted()
		{
			return null;
		}

		protected internal virtual ZString GetTotalOSEDUPrimaryAmountFormatted()
		{
			return null;
		}

		protected internal virtual ZString GetTotalOSEDUSecondaryAmountFormatted()
		{
			return null;
		}

		protected internal virtual ZString GetTotalOSRETAmountFormatted()
		{
			return null;
		}

		protected internal virtual ZString GetTotalOSSPVAmountFormatted()
		{
			return null;
		}

		protected internal virtual ZString GetOSTotalFormatted()
		{
			return null;
		}

		protected internal virtual ZString GetOSTotalRawFormatted()
		{
			return null;
		}

		protected internal virtual ZBool GetHasSERLineOnly()
		{
			return false;
		}

		protected internal virtual ZBool GetHasAtLeastOneSERLine()
		{
			return false;
		}

		protected internal virtual ZBool GetHasVATANDIGICLine()
		{
			return false;
		}

		protected internal virtual ZBool GetHasIGICLineOnly()
		{
			return false;
		}

		protected internal virtual ZBool GetHasGSTANDQSTLine()
		{
			return false;
		}

		protected internal virtual ZBool GetHasGSTANDQCTLine()
		{
			return false;
		}

		protected internal virtual ZBool GetHasGSTANDEDULine()
		{
			return false;
		}

		protected internal virtual ZBool GetHasRETLine()
		{
			return false;
		}

		protected internal virtual ZBool GetHasIntegratedGSTLine()
		{
			return false;
		}

		protected internal virtual ZBool GetHasStateGSTLine()
		{
			return false;
		}

		protected internal virtual ZBool GetHasZeroIGSTAmount()
		{
			return false;
		}

		protected internal virtual ZString GetTotalOSIntegratedGSTAmountFormatted()
		{
			return null;
		}

		protected internal virtual ZString GetTotalOSCentreGSTAmountFormatted()
		{
			return null;
		}

		protected internal virtual ZString GetTotalOSStateGSTAmountFormatted()
		{
			return null;
		}

		protected internal virtual ZDecimal GetGSTAmount()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZDecimal GetOSGSTAmount()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZBool GetShowLocalGSTAmount()
		{
			return false;
		}

		protected internal virtual ZString GetOSPrimaryTaxDisplayHeading()
		{
			return null;
		}

		protected internal virtual ZString GetInvoiceSubTotalDisplayHeading()
		{
			return null;
		}

		protected internal virtual ZString GetOSTaxExtraRateQCTDisPlay()
		{
			return null;
		}

		protected internal virtual ZString GetOSTaxExtraRateSBCDisPlay()
		{
			return null;
		}

		protected internal virtual ZString GetOSTaxExtraRateKKCDisPlay()
		{
			return null;
		}

		protected internal virtual ZString GetOSSPVExtraTaxLabel()
		{
			return null;
		}

		protected internal virtual ZString GetOSSPVExtraTaxCodeLabel()
		{
			return null;
		}

		protected internal virtual ZInt GetPostPeriod()
		{
			return ZInt.Zero;
		}

		protected internal virtual ZString GetAgePeriodDisplay()
		{
			return ZString.Empty;
		}

		protected internal virtual ZString GetDebitAmount()
		{
			return ZString.Empty;
		}

		protected internal virtual ZString GetPeriodDisplay()
		{
			return ZString.Empty;
		}

		protected internal virtual ZString GetJournalType()
		{
			return ZString.Empty;
		}

		protected internal virtual ZString GetJobNumber()
		{
			return ZString.Empty;
		}

		protected internal virtual ZDecimal TotalDebitAmount => ZDecimal.Zero;

		protected internal virtual ZDecimal TotalCreditAmount => ZDecimal.Zero;

		protected internal virtual ZDecimal GetCurrentCompanyReciprocal()
		{
			return GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces;
		}

		protected internal virtual ZString GetStatementTotalOverdueAmount()
		{
			return null;
		}

		protected internal virtual ZString GetStatementTotalDueAmount()
		{
			return null;
		}

		protected internal virtual ZString GetStatementTotalCurrentAmount()
		{
			return null;
		}

		protected internal virtual ZString GetStatementTotalStatementAmount()
		{
			return null;
		}

		protected internal virtual ZString GetTotal30DaysOverdue()
		{
			return null;
		}

		protected internal virtual ZString GetTotal60DaysOverdue()
		{
			return null;
		}

		protected internal virtual ZString GetTotal90DaysOverdue()
		{
			return null;
		}

		protected internal virtual ZString GetTotal90PlusDaysOverdue()
		{
			return null;
		}

		protected internal virtual ZString GetTotalDueBetween0To29thDay()
		{
			return null;
		}

		protected internal virtual ZString GetTotalDueBetween30To59thDay()
		{
			return null;
		}

		protected internal virtual ZString GetTotalDueBetween60To89thDay()
		{
			return null;
		}

		protected internal virtual ZString GetTotalDueBetween90To119thDay()
		{
			return null;
		}

		protected internal virtual ZString GetTotalDueBetween120To149thDay()
		{
			return null;
		}

		protected internal virtual ZString GetTotalDue150PlusDay()
		{
			return null;
		}

		protected internal virtual ZString GetTotal30DaysDue()
		{
			return null;
		}

		protected internal virtual ZString GetTotal30PlusDaysOverDue()
		{
			return null;
		}

		protected internal virtual ZString GetAJOptionField1Caption()
		{
			return ZString.Empty;
		}

		protected internal virtual ZString GetAJOptionField1Value()
		{
			return ZString.Empty;
		}

		protected internal virtual ZString GetAJOptionField2Caption()
		{
			return ZString.Empty;
		}

		protected internal virtual ZString GetAJOptionField2Value()
		{
			return ZString.Empty;
		}

		protected internal virtual ZString GetAJOptionField3Caption()
		{
			return ZString.Empty;
		}

		protected internal virtual ZString GetAJOptionField3Value()
		{
			return ZString.Empty;
		}

		protected internal virtual ZString GetReceiptBankAccountIBAN()
		{
			return ZString.Empty;
		}

		protected internal virtual ZString GetEInvoicingGovernmentAllocatedNumber()
		{
			return ZString.Empty;
		}
		protected internal virtual ZString GetEInvoicingAuthorisationNumber()
		{
			return ZString.Empty;
		}

		#region Approval

		protected internal virtual ZString GetRequestedBy()
		{
			return null;
		}

		protected internal virtual ZDateTime GetRequestedTime()
		{
			return ZDateTime.Empty;
		}

		protected internal virtual ZString GetApprovedBy()
		{
			return null;
		}

		protected internal virtual ZDateTime GetApprovedTime()
		{
			return ZDateTime.Empty;
		}

		protected internal virtual ZString GetOriginalRequester()
		{
			return null;
		}

		protected internal virtual ZString GetOriginalApprover()
		{
			return null;
		}

		protected internal virtual ZString GetJournalEntriesNumber()
		{
			return null;
		}

		protected internal virtual DocBankAccount GetAccount()
		{
			return null;
		}

		protected internal virtual DocAccountingVoucher GetVoucher()
		{
			return null;
		}

		protected internal virtual DocTransactionHeaderCollection GetCashTransactions()
		{
			return null;
		}

		protected internal virtual DocTransactionHeaderCollection GetCreditCardTransactions()
		{
			return null;
		}

		protected internal virtual DocTransactionHeaderCollection GetChequeTransactions()
		{
			return null;
		}

		protected internal virtual DocTransactionHeaderCollection GetDirectCreditTransactions()
		{
			return null;
		}

		protected internal virtual DocTaxTransactionCollection GetTaxTransactions()
		{
			return null;
		}

		protected internal virtual ZInt GetTotalChequeCount()
		{
			return ZInt.Zero;
		}

		protected internal virtual ZDecimal GetTotalCashAmount()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZDecimal GetTotalCreditCardAmount()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZDecimal GetTotalChequeAmount()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZDecimal GetTotalDirectCreditAmount()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZDecimal GetDepositBatchTotalAmountForBank()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZDecimal GetDepositBatchTotalAmount()
		{
			return ZDecimal.Zero;
		}

		#endregion
	}

	public class DocGenericTransactionHeader : DocBaseWrapper
	{
		public static DocGenericTransactionHeader New(BusinessObject businessObject, BusinessObjectFactory factoryToWrap, FreightWrapper freightWrapper)
		{
			DocGenericTransactionHeader result = New(businessObject, factoryToWrap);
			if (result != null)
			{
				result.OperationalJob = freightWrapper;
			}
			return result;
		}

		public static DocGenericTransactionHeader New(BusinessObject businessObject, BusinessObjectFactory factoryToWrap)
		{
			DocGenericTransactionHeader result = null;
			if (businessObject != null)
			{
				result = new DocGenericTransactionHeader(businessObject, factoryToWrap);
			}
			return result;
		}

		public FreightWrapper OperationalJob
		{
			get;
			set;
		}

		protected DocGenericTransactionHeader(BusinessObject businessObject, BusinessObjectFactory factoryToWrap)
			: base(businessObject, factoryToWrap)
		{
			this.BusinessObject = businessObject;
			this.HeaderPlugIn = GetHeaderPlugInForBusinessObject(businessObject, factoryToWrap);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		static IGenericTransactionHeaderPlugIn GetHeaderPlugInForBusinessObject(BusinessObject businessObject, BusinessObjectFactory factoryToWrap)
		{
			if (businessObject is DirectPayment)
			{
				return DocDirectPayment.New((DirectPayment)businessObject, factoryToWrap);
			}
			else if (businessObject is GLJournal)
			{
				return DocGLJournal.New((GLJournal)businessObject, factoryToWrap);
			}
			else if (businessObject is AccountingJournal)
			{
				return DocAccountingJournal.New((AccountingJournal)businessObject, factoryToWrap);
			}
			else if (businessObject is JobRevenueJournal)
			{
				return DocJobRevenueJournal.New((JobRevenueJournal)businessObject, factoryToWrap);
			}
			else if (businessObject is Payment)
			{
				return DocAPPayment.New((Payment)businessObject, factoryToWrap);
			}
			else if (businessObject is PaymentApprovalWithAuthorisation)
			{
				return DocPaymentApproval.New((PaymentApprovalWithAuthorisation)businessObject, factoryToWrap);
			}
			else if (businessObject is AccCashAdvanceRequestHeader cashAdvanceRequest)
			{
				return DocCashAdvanceRequestHeader.New(cashAdvanceRequest, factoryToWrap);
			}
			else if (businessObject is AccPaymentBatch accPaymentBatch)
			{
				return DocPaymentBatch.New(accPaymentBatch, factoryToWrap);
			}
			else if (businessObject is BankTransferRow)
			{
				return DocMasterTransferRecord.New((BankTransferRow)businessObject, factoryToWrap);
			}
			else if (businessObject is APInvoice)
			{
				return DocAPInvoice.New((APInvoice)businessObject, factoryToWrap);
			}
			else if (businessObject is APCreditNote)
			{
				return DocAPInvoice.New((AccTransactionHeader)businessObject, factoryToWrap);
			}
			else if (businessObject is APAdjustmentNote)
			{
				return DocAPInvoice.New((AccTransactionHeader)businessObject, factoryToWrap);
			}
			else if (businessObject is InvoiceBatchHeader)
			{
				return DocARBatchInvoice.New((InvoiceBatchHeader)businessObject, factoryToWrap, false);
			}
			else if (businessObject is DepositBatch)
			{
				return DocDepositBatch.New((DepositBatch)businessObject, factoryToWrap);
			}
			else if (businessObject is TransactionHeader)
			{
				if (businessObject is AccountMovement)
				{
					return DocAccountMovement.New((AccountMovement)businessObject, factoryToWrap);
				}
				else
				{
					return DocTransactionHeader.New((TransactionHeader)businessObject, factoryToWrap);
				}
			}
			else if (businessObject is PrintStatement)
			{
				return DocStatement.New((PrintStatement)businessObject, factoryToWrap);
			}
			else if (businessObject is PrintSummary)
			{
				return DocStatementSummary.New((PrintSummary)businessObject, factoryToWrap);
			}
			else if (businessObject is VoucherProvider)
			{
				return DocAccountingVoucher.New((VoucherProvider)businessObject, factoryToWrap);
			}
			else
			{
				throw new Exception("No suitable DocWrapper can be found!");
			}
		}

		public readonly BusinessObject BusinessObject;
		public readonly IGenericTransactionHeaderPlugIn HeaderPlugIn;

		public ZString ReferenceNumber
		{
			get { return HeaderPlugIn.HeaderSupporter.GetReferenceNumber(); }
		}

		public ZString StatementDescription
		{
			get { return HeaderPlugIn.HeaderSupporter.GetStatementDescription(); }
		}

		public ZString Disbursement
		{
			get { return HeaderPlugIn.HeaderSupporter.GetDisbursement(); }
		}

		public ZDateTime TransactionDueDate
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTransactionDueDate(); }
		}

		public ZDecimal InvoiceAmountWithGST
		{
			get { return HeaderPlugIn.HeaderSupporter.GetInvoiceAmountWithGST(); }
		}

		public ZDecimal Balance
		{
			get { return HeaderPlugIn.HeaderSupporter.GetBalance(); }
		}

		public DocOrganisation Organisation
		{
			get { return HeaderPlugIn.HeaderSupporter.GetOrganisation(); }
		}

		public ZString TransactionType
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTransactionType(); }
		}

		public ZBool IsCancelled
		{
			get { return HeaderPlugIn.HeaderSupporter.GetIsCancelled(); }
		}

		public ZDateTime InvoiceDate
		{
			get { return HeaderPlugIn.HeaderSupporter.GetInvoiceDate(); }
		}

		public ZDate ComplianceDocDate
		{
			get { return HeaderPlugIn.HeaderSupporter.GetComplianceDocDate(); }
		}

		public ZString BankAccountCode
		{
			get { return HeaderPlugIn.HeaderSupporter.GetBankAccountCode(); }
		}

		public ZString OrganisationECRCode
		{
			get { return HeaderPlugIn.HeaderSupporter.GetOrganisationECRCode(); }
		}

		public ZString TransactionARPaymentMethod
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTransactionARPaymentMethod(); }
		}

		public ZString TransactionAPPaymentMethod
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTransactionAPPaymentMethod(); }
		}

		public ZString CheckBookCode
		{
			get { return HeaderPlugIn.HeaderSupporter.GetCheckBookCode(); }
		}

		public ZString OrganisationARTermsPaymentMethod
		{
			get { return HeaderPlugIn.HeaderSupporter.GetOrganisationARTermsPaymentMethod(); }
		}

		public ZString OrganisationARAgreedPaymentMethod
		{
			get { return HeaderPlugIn.HeaderSupporter.GetOrganisationARAgreedPaymentMethod(); }
		}

		public ZString OrganisationAPAgreedPaymentMethod
		{
			get { return HeaderPlugIn.HeaderSupporter.GetOrganisationAPAgreedPaymentMethod(); }
		}

		public ZString OrganisationAPAccountName
		{
			get { return HeaderPlugIn.HeaderSupporter.GetOrganisationAPAccountName(); }
		}

		public ZString OrganisationAPAccountNumber
		{
			get { return HeaderPlugIn.HeaderSupporter.GetOrganisationAPAccountNumber(); }
		}

		public ZString OrganisationAPBankDetail
		{
			get { return HeaderPlugIn.HeaderSupporter.GetOrganisationAPBankDetail(); }
		}

		public ZString OrganisationCode
		{
			get { return HeaderPlugIn.HeaderSupporter.GetOrganisationCode(); }
		}

		public ZString OrganisationName
		{
			get { return HeaderPlugIn.HeaderSupporter.GetOrganisationName(); }
		}

		public ZString OrganisationPostalAddress
		{
			get { return HeaderPlugIn.HeaderSupporter.GetOrganisationPostalAddress(); }
		}

		public ZString ReceiptTypeDescription
		{
			get { return HeaderPlugIn.HeaderSupporter.GetReceiptTypeDescription(); }
		}

		public ZString STDTerms
		{
			get { return HeaderPlugIn.HeaderSupporter.GetSTDTerms(); }
		}

		public ZString DSBTerms
		{
			get { return HeaderPlugIn.HeaderSupporter.GetDSBTerms(); }
		}

		public ZGuid OrganisationARAddressOrgAddressPK
		{
			get { return HeaderPlugIn.HeaderSupporter.GetOrganisationARAddressOrgAddressPK(); }
		}

		public ZBool DisbursementInvoicesOnly
		{
			get { return HeaderPlugIn.HeaderSupporter.GetDisbursementInvoicesOnly(); }
		}

		public ZDateTime DisplayDate
		{
			get { return HeaderPlugIn.HeaderSupporter.GetDisplayDate(); }
		}

		public ZString DocumentTitle
		{
			get { return HeaderPlugIn.HeaderSupporter.GetDocumenTitle(); }
		}

		public ZString TaxId
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTaxId(); }
		}

		public ZBool IsStrongHeading
		{
			get { return HeaderPlugIn.HeaderSupporter.GetIsStrongHeading(); }
		}

		public ZBool IsStatement
		{
			get { return HeaderPlugIn.HeaderSupporter.GetIsStatement(); }
		}

		public ZBool IsAccountMovementListing
		{
			get { return HeaderPlugIn.HeaderSupporter.GetIsAccountMovementListing(); }
		}

		public ZString AccountMovementListingGroupBy
		{
			get { return HeaderPlugIn.HeaderSupporter.GetAccountMovementListingGroupBy(); }
		}

		public ZDecimal AccountMovementOpeningBalance
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetAccountMovementOpeningBalance();
			}
		}

		public ZDecimal AccountMovementClosingBalance
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetAccountMovementClosingBalance();
			}
		}

		public ZDateTime AccountMovementOpeningBalanceDate
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetAccountMovementOpeningBalanceDate();
			}
		}

		public ZDateTime AccountMovementClosingBalanceDate
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetAccountMovementClosingBalanceDate();
			}
		}

		public ZString OpeningText
		{
			get { return HeaderPlugIn.HeaderSupporter.GetOpeningText(); }
		}

		public ZBool IssueBySettlementGroup
		{
			get { return HeaderPlugIn.HeaderSupporter.GetIssueBySettlementGroup(); }
		}

		public ZString CreditBalanceMessage
		{
			get { return HeaderPlugIn.HeaderSupporter.GetCreditBalanceMessage(); }
		}

		public ZBool HasCreditBalance
		{
			get { return HeaderPlugIn.HeaderSupporter.GetHasCreditBalance(); }
		}

		public ZString TotalCurrent
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTotalCurrent(); }
		}

		public ZString TotalOverdue
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTotalOverdue(); }
		}

		public ZBool DisplayNotYetOutstandingAmount
		{
			get { return HeaderPlugIn.HeaderSupporter.GetDisplayNotYetOutstandingAmount(); }
		}

		public ZString ReceiptsNotMatched
		{
			get { return HeaderPlugIn.HeaderSupporter.GetReceiptsNotMatched(); }
		}

		public ZString Message
		{
			get { return HeaderPlugIn.HeaderSupporter.GetMessage(); }
		}

		public ZString StatementBalanceFormatted
		{
			get { return HeaderPlugIn.HeaderSupporter.GetStatementBalanceFormatted(); }
		}

		public ZString PaymentRequestText
		{
			get { return HeaderPlugIn.HeaderSupporter.GetPaymentRequestText(); }
		}

		public ZString ReceiptBankAccountBSB
		{
			get { return HeaderPlugIn.HeaderSupporter.GetReceiptBankAccountBSB(); }
		}

		public ZString ReceiptBankAccountSWIFT
		{
			get { return HeaderPlugIn.HeaderSupporter.GetReceiptBankAccountSWIFT(); }
		}

		public ZString ReceiptBankAccountAccountNum
		{
			get { return HeaderPlugIn.HeaderSupporter.GetReceiptBankAccountAccountNum(); }
		}

		public ZString ReceiptBankAccountBankName
		{
			get { return HeaderPlugIn.HeaderSupporter.GetReceiptBankAccountBankName(); }
		}

		public ZString ReceiptBankAccountBankAddress
		{
			get { return HeaderPlugIn.HeaderSupporter.GetReceiptBankAccountBankAddress(); }
		}

		public ZString MailToAddressWithCountry
		{
			get { return HeaderPlugIn.HeaderSupporter.GetMailToAddressWithCountry(); }
		}

		public ZBool ShouldHideCompanyName
		{
			get { return HeaderPlugIn.HeaderSupporter.GetShouldHideCompanyName(); }
		}

		public ZString ReceiptType
		{
			get { return HeaderPlugIn.HeaderSupporter.GetReceiptType(); }
		}

		public ZString Status
		{
			get { return HeaderPlugIn.HeaderSupporter.GetStatus(); }
		}

		public ZString ChequeOrReference
		{
			get { return HeaderPlugIn.HeaderSupporter.GetChequeOrReference(); }
		}

		public ZString TransactionReference
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTransactionReference(); }
		}

		public ZString TransactionNumber
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTransactionNumber(); }
		}

		public ZString ComplianceSubType
		{
			get { return HeaderPlugIn.HeaderSupporter.GetComplianceSubType(); }
		}

		public ZString TransactionNumberPrefixed
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTransactionNumberPrefixed(); }
		}

		public ZString RemittanceAdviceContact
		{
			get { return HeaderPlugIn.HeaderSupporter.GetRemittanceAdviceContact(); }
		}

		public ZDecimal OSTotalForRemittanceAdvice
		{
			get { return HeaderPlugIn.HeaderSupporter.GetOSTotalForRemittanceAdvice(); }
		}

		public ZDecimal SummaryTotalForRemittanceAdvice
		{
			get { return HeaderPlugIn.HeaderSupporter.GetSummaryTotalForRemittanceAdvice(); }
		}

		public ZString PaymentCurrencyCode
		{
			get { return HeaderPlugIn.HeaderSupporter.GetPaymentCurrencyCode(); }
		}

		public ZString AccountsPayableSuppliersReference
		{
			get { return HeaderPlugIn.HeaderSupporter.GetAccountsPayableSuppliersReference(); }
		}

		public ZBool ShowOriginalAmount
		{
			get { return HeaderPlugIn.HeaderSupporter.GetShowOriginalAmount(); }
		}

		public ZString CurrencyCode
		{
			get { return HeaderPlugIn.HeaderSupporter.GetCurrencyCode(); }
		}

		public ZString CurrentCompanyCurrencyCode
		{
			get { return HeaderPlugIn.HeaderSupporter.GetCurrentCompanyCurrencyCode(); }
		}

		public ZInt CurrentCompanyCurrencyDecimalPlaces
		{
			get { return HeaderPlugIn.HeaderSupporter.GetCurrentCompanyCurrencyDecimalPlaces(); }
		}

		public ZDecimal CurrentCompanyReciprocal
		{
			get { return HeaderPlugIn.HeaderSupporter.GetCurrentCompanyReciprocal(); }
		}

		public ZBool ShowOriginalAmountForPAY
		{
			get { return HeaderPlugIn.HeaderSupporter.GetShowOriginalAmountForPAY(); }
		}

		public ZBool ShowOriginalAmountForDPY
		{
			get { return HeaderPlugIn.HeaderSupporter.GetShowOriginalAmountForDPY(); }
		}

		public DocGenericTransactionLineCollection PaidLines
		{
			get { return HeaderPlugIn.HeaderSupporter.GetPaidLines(); }
		}

		public DocTransactionLineCollection GLLines
		{
			get { return HeaderPlugIn.HeaderSupporter.GetGLLines(); }
		}

		public DocGenericTransactionLineCollection AJLines
		{
			get { return HeaderPlugIn.HeaderSupporter.GetAJLines(); }
		}

		public DocAccountingJournalTaxDetailCollection TaxDetails
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTaxDetails(); }
		}

		public ZBool HasForeignCurrencyLines => HeaderPlugIn.HeaderSupporter.HasForeignCurrencyLines;

		public DocGenericTransactionLineCollection ReceiptLines
		{
			get { return HeaderPlugIn.HeaderSupporter.GetReceiptLines(); }
		}

		public DocGenericTransactionLineCollection FlattenedInvoices
		{
			get { return HeaderPlugIn.HeaderSupporter.GetFlattenedInvoices(); }
		}

		public DocGenericTransactionLineCollection FlattenedPayments
		{
			get { return HeaderPlugIn.HeaderSupporter.GetFlattenedPayments(); }
		}

		public DocGenericTransactionLineCollection FlattenedReceiptMatches
		{
			get { return HeaderPlugIn.HeaderSupporter.GetFlattenedReceiptMatches(); }
		}

		public DocGenericTransactionLineCollection FilteredFlattenedReceiptMatches
		{
			get { return HeaderPlugIn.HeaderSupporter.GetFilteredFlattenedReceiptMatches(); }
		}

		public DocGenericTransactionLineCollection StatementSummaryLines
		{
			get { return HeaderPlugIn.HeaderSupporter.GetStatementSummaryLines(); }
		}

		public DocGenericTransactionLineCollection CostConfirmationSummaryLines
		{
			get { return HeaderPlugIn.HeaderSupporter.GetCostConfirmationSummaryLines(); }
		}

		public DocGenericTransactionLineCollection OrganisationCustomAttributes
		{
			get { return HeaderPlugIn.HeaderSupporter.GetOrganisationCustomAttributes(); }
		}

		public DocGenericTransactionLineCollection LinesForInvoice
		{
			get { return HeaderPlugIn.HeaderSupporter.GetLinesForInvoice(); }
		}

		public DocGenericTransactionLineCollection LinesForCashAdvanceRequest
		{
			get { return HeaderPlugIn.HeaderSupporter.GetLinesForCashAdvanceRequestHeader(); }
		}

		public ZString CostConfirmationRollupSetting
		{
			get { return HeaderPlugIn.HeaderSupporter.GetCostConfirmationRollupSetting(); }
		}

		public DocTransactionHeaderCollection Transactions
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTransactions(); }
		}

		public DocGenericTransactionHeaderCollection GenericTransactions
		{
			get { return HeaderPlugIn.HeaderSupporter.GetGenericTransactions(); }
		}

		public DocAccountingJournalCollection RelatedJournals
		{
			get { return HeaderPlugIn.HeaderSupporter.GetRelatedJournals(); }
		}

		public ZString Desc
		{
			get { return HeaderPlugIn.HeaderSupporter.GetDesc(); }
		}

		public ZString ReportingBookCode
		{
			get { return HeaderPlugIn.HeaderSupporter.GetReportingBookCode(); }
		}

		public ZString ReportingBookDescription
		{
			get { return HeaderPlugIn.HeaderSupporter.GetReportingBookDescription(); }
		}

		public ZString ChartCode
		{
			get { return HeaderPlugIn.HeaderSupporter.GetChartCode(); }
		}

		public ZString ChartDescription
		{
			get { return HeaderPlugIn.HeaderSupporter.GetChartDescription(); }
		}

		public ZBool DisplayParentAccount
		{
			get { return HeaderPlugIn.HeaderSupporter.GetDisplayParentAccount(); }
		}

		public ZDecimal MatchLinkInvertedOSAmount
		{
			get { return HeaderPlugIn.HeaderSupporter.GetMatchLinkInvertedOSAmount(); }
		}

		public ZDecimal MatchLinkAmount
		{
			get { return HeaderPlugIn.HeaderSupporter.GetMatchLinkAmount(); }
		}

		public ZDateTime CreatedDate
		{
			get { return HeaderPlugIn.HeaderSupporter.GetCreatedDate(); }
		}

		public ZDateTime PostDate
		{
			get { return HeaderPlugIn.HeaderSupporter.GetPostDate(); }
		}

		public ZString CreatingUser
		{
			get { return HeaderPlugIn.HeaderSupporter.GetCreatingUser(); }
		}

		public ZDecimal InvoiceAmount
		{
			get { return HeaderPlugIn.HeaderSupporter.GetInvoiceAmount(); }
		}

		public DocARBatchInvoiceLineCollection Invoices
		{
			get { return HeaderPlugIn.HeaderSupporter.GetInvoices(); }
		}

		public ZDecimal OSTotal
		{
			get { return HeaderPlugIn.HeaderSupporter.GetOSTotal(); }
		}

		public ZDecimal ExchangeRate
		{
			get { return HeaderPlugIn.HeaderSupporter.GetExchangeRate(); }
		}

		public ZString SecondaryTransactionType
		{
			get { return HeaderPlugIn.HeaderSupporter.GetSecondaryTransactionType(); }
		}

		public ZString SecondaryTransactionNumber
		{
			get { return HeaderPlugIn.HeaderSupporter.GetSecondaryTransactionNumber(); }
		}

		public ZString SecondaryBankAccountCode
		{
			get { return HeaderPlugIn.HeaderSupporter.GetSecondaryBankAccountCode(); }
		}

		public ZDecimal SecondaryInvoiceAmount
		{
			get { return HeaderPlugIn.HeaderSupporter.GetSecondaryInvoiceAmount(); }
		}

		public ZDecimal SecondaryExchangeRate
		{
			get { return HeaderPlugIn.HeaderSupporter.GetSecondaryExchangeRate(); }
		}

		public ZDecimal SecondaryOSTotal
		{
			get { return HeaderPlugIn.HeaderSupporter.GetSecondaryOSTotal(); }
		}

		public ZString SecondaryCurrencyCode
		{
			get { return HeaderPlugIn.HeaderSupporter.GetSecondaryCurrencyCode(); }
		}

		public ZString ChargesTransactionType
		{
			get { return HeaderPlugIn.HeaderSupporter.GetChargesTransactionType(); }
		}

		public ZString ChargesTransactionNumber
		{
			get { return HeaderPlugIn.HeaderSupporter.GetChargesTransactionNumber(); }
		}

		public ZString ChargesBankAccountCode
		{
			get { return HeaderPlugIn.HeaderSupporter.GetChargesBankAccountCode(); }
		}

		public ZDecimal ChargesInvoiceAmountAndTax
		{
			get { return HeaderPlugIn.HeaderSupporter.GetChargesInvoiceAmountAndTax(); }
		}

		public ZString ChargesDesc
		{
			get { return HeaderPlugIn.HeaderSupporter.GetChargesDesc(); }
		}

		public ZDecimal ChargesOSTotal
		{
			get { return HeaderPlugIn.HeaderSupporter.GetChargesOSTotal(); }
		}

		public ZString ChargesCurrencyCode
		{
			get { return HeaderPlugIn.HeaderSupporter.GetChargesCurrencyCode(); }
		}

		public ZDecimal ChargesExchangeRate
		{
			get { return HeaderPlugIn.HeaderSupporter.GetChargesExchangeRate(); }
		}

		public ZString BarCode
		{
			get { return HeaderPlugIn.HeaderSupporter.GetBarcode(); }
		}

		public ZDecimal TotalPaymentForPaymentVoucher
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTotalPaymentForPaymentVoucher(); }
		}

		public ZDecimal TotalPaidAsForPaymentVoucher
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTotalPaidAsForPaymentVoucher(); }
		}

		public ZDecimal ExchangeRateForPaymentVoucher
		{
			get { return HeaderPlugIn.HeaderSupporter.GetExchangeRateForPaymentVoucher(); }
		}

		public ZBool ShowNewAuthorisationFooter
		{
			get { return HeaderPlugIn.HeaderSupporter.GetShowNewAuthorisationFooter(); }
		}

		public ZString PreparedBy
		{
			get { return HeaderPlugIn.HeaderSupporter.GetPreparedBy(); }
		}

		public ZString ApprovalStatus
		{
			get { return HeaderPlugIn.HeaderSupporter.GetApprovalStatus(); }
		}

		public ZString FirstAuthorisationDescription
		{
			get { return HeaderPlugIn.HeaderSupporter.GetFirstAuthorisationDescription(); }
		}

		public ZString SecondAuthorisationDescription
		{
			get { return HeaderPlugIn.HeaderSupporter.GetSecondAuthorisationDescription(); }
		}

		public ZString ThirdAuthorisationDescription
		{
			get { return HeaderPlugIn.HeaderSupporter.GetThirdAuthorisationDescription(); }
		}

		public ZString FirstAuthorisation
		{
			get { return HeaderPlugIn.HeaderSupporter.GetFirstAuthorisation(); }
		}

		public ZString SecondAuthorisation
		{
			get { return HeaderPlugIn.HeaderSupporter.GetSecondAuthorisation(); }
		}

		public ZString ThirdAuthorisation
		{
			get { return HeaderPlugIn.HeaderSupporter.GetThirdAuthorisation(); }
		}

		public ZDecimal PaymentVoucherOSTotal
		{
			get { return HeaderPlugIn.HeaderSupporter.GetPaymentVoucherOSTotal(); }
		}

		public ZString ChequeDrawer
		{
			get { return HeaderPlugIn.HeaderSupporter.GetChequeDrawer(); }
		}

		public ZString DrawerBank
		{
			get { return HeaderPlugIn.HeaderSupporter.GetDrawerBank(); }
		}

		public ZString DrawerBranch
		{
			get { return HeaderPlugIn.HeaderSupporter.GetDrawerBranch(); }
		}

		public ZDecimal TotalOSAmount
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTotalOSAmount(); }
		}

		public ZDecimal TotalOSPaidAmount
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTotalOSPaidAmount(); }
		}

		public ZDecimal TotalLocalAmount
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTotalLocalAmount(); }
		}

		public ZDecimal TotalLocalPaidAmount
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTotalLocalPaidAmount(); }
		}

		public ZString ConsolidatedInvoiceRef
		{
			get { return HeaderPlugIn.HeaderSupporter.GetConsolidatedInvoiceRef(); }
		}

		public ZString ApprovalRequestID
		{
			get { return HeaderPlugIn.HeaderSupporter.GetApprovalRequestID(); }
		}

		public ZDateTime DepositBatchBatchDate
		{
			get { return HeaderPlugIn.HeaderSupporter.GetDepositBatchBatchDate(); }
		}

		public ZString ReceiptBatchNo
		{
			get { return HeaderPlugIn.HeaderSupporter.GetReceiptBatchNo(); }
		}

		public ZString Ledger
		{
			get { return HeaderPlugIn.HeaderSupporter.GetLedger(); }
		}

		public ZBool PrintLocalValues
		{
			get { return HeaderPlugIn.HeaderSupporter.GetPrintLocalValues(); }
		}

		public ZDateTime FullyPaidDate
		{
			get { return HeaderPlugIn.HeaderSupporter.GetFullyPaidDate(); }
		}

		public ZDecimal TotalAllocatedAmount
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTotalAllocatedAmount(); }
		}

		public ZDecimal OSOutstandingAmount
		{
			get { return HeaderPlugIn.HeaderSupporter.GetOSOutstandingAmount(); }
		}

		public ZDecimal TotalLocalInvoiceAmount
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTotalLocalInvoiceAmount(); }
		}

		public Image StatementLogo
		{
			get { return HeaderPlugIn.HeaderSupporter.GetStatementLogo(); }
		}

		public Image InvoiceLogo
		{
			get { return HeaderPlugIn.HeaderSupporter.GetInvoiceLogo(); }
		}

		public ZString CostConfirmationDocumentTitle
		{
			get { return HeaderPlugIn.HeaderSupporter.GetCostConfirmationDocumentTitle(); }
		}

		public ZString CostConfirmationHeadingText
		{
			get { return HeaderPlugIn.HeaderSupporter.GetCostConfirmationHeadingText(); }
		}

		public ZString AccountCode
		{
			get { return HeaderPlugIn.HeaderSupporter.GetAccountCode(); }
		}

		public ZString SupplierTaxIDNumber
		{
			get { return HeaderPlugIn.HeaderSupporter.GetSupplierTaxIDNumber(); }
		}

		public ZString SupplierTaxIDHeading
		{
			get { return HeaderPlugIn.HeaderSupporter.GetSupplierTaxIDHeading(); }
		}

		public ZString AccountName
		{
			get { return HeaderPlugIn.HeaderSupporter.GetAccountName(); }
		}

		public ZString AccountFullName
		{
			get { return HeaderPlugIn.HeaderSupporter.GetAccountFullName(); }
		}

		public ZDateTime DueDate
		{
			get { return HeaderPlugIn.HeaderSupporter.GetDueDate(); }
		}

		public ZString PostedBy
		{
			get { return HeaderPlugIn.HeaderSupporter.GetPostedBy(); }
		}

		public ZString OSTaxDisplayHeading
		{
			get { return HeaderPlugIn.HeaderSupporter.GetOSTaxDisplayHeading(); }
		}

		public ZBool IsTaxed
		{
			get { return HeaderPlugIn.HeaderSupporter.GetIsTaxed(); }
		}

		public ZBool ShowInvoiceTaxDate
		{
			get { return HeaderPlugIn.HeaderSupporter.GetShowInvoiceTaxDate(); }
		}

		public ZString RecipientNameAddress
		{
			get { return HeaderPlugIn.HeaderSupporter.GetRecipientNameAddress(); }
		}

		public ZBool IsTotalOSTaxAmountZero
		{
			get { return HeaderPlugIn.HeaderSupporter.GetIsTotalOSTaxAmountZero(); }
		}

		public ZBool IsTotalOSTaxAmountRawZero
		{
			get { return HeaderPlugIn.HeaderSupporter.GetIsTotalOSTaxAmountRawZero(); }
		}

		public ZString InvoiceSubTotalFormatted
		{
			get { return HeaderPlugIn.HeaderSupporter.GetInvoiceSubTotalFormatted(); }
		}

		public ZString TotalOSTaxAmountFormatted
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTotalOSTaxAmountFormatted(); }
		}

		public ZString OSDocumentTitleForITAutofattura
		{
			get { return HeaderPlugIn.HeaderSupporter.GetOSDocumentTitleForITAutofattura(); }
		}

		public ZString TotalOSTaxAmountRawFormatted
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTotalOSTaxAmountRawFormatted(); }
		}

		public ZDecimal TotalLocalTax_Raw
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTotalLocalTax_Raw(); }
		}

		public ZString TotalOSIGICAmountFormatted
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTotalOSIGICAmountFormatted(); }
		}

		public ZString TotalOSSERAmountFormatted
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTotalOSSERAmountFormatted(); }
		}

		public ZString TotalOSQSTAmountFormatted
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTotalOSQSTAmountFormatted(); }
		}

		public ZString TotalOSSBCAmountFormatted
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTotalOSSBCAmountFormatted(); }
		}

		public ZString TotalOSKKCAmountFormatted
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTotalOSKKCAmountFormatted(); }
		}

		public ZString TotalOSEDUPrimaryAmountFormatted
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTotalOSEDUPrimaryAmountFormatted(); }
		}

		public ZString TotalOSEDUSecondaryAmountFormatted
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTotalOSEDUSecondaryAmountFormatted(); }
		}

		public ZString TotalOSRETAmountFormatted
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTotalOSRETAmountFormatted(); }
		}

		public ZString TotalOSSPVAmountFormatted
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTotalOSSPVAmountFormatted(); }
		}

		public ZString OSTotalFormatted
		{
			get { return HeaderPlugIn.HeaderSupporter.GetOSTotalFormatted(); }
		}

		public ZString OSTotalRawFormatted
		{
			get { return HeaderPlugIn.HeaderSupporter.GetOSTotalRawFormatted(); }
		}

		public ZBool HasSERLineOnly
		{
			get { return HeaderPlugIn.HeaderSupporter.GetHasSERLineOnly(); }
		}

		public ZBool HasAtLeastOneSERLine
		{
			get { return HeaderPlugIn.HeaderSupporter.GetHasAtLeastOneSERLine(); }
		}

		public ZBool HasVATANDIGICLine
		{
			get { return HeaderPlugIn.HeaderSupporter.GetHasVATANDIGICLine(); }
		}

		public ZBool HasIGICLineOnly
		{
			get { return HeaderPlugIn.HeaderSupporter.GetHasIGICLineOnly(); }
		}

		public ZBool HasGSTANDQSTLine
		{
			get { return HeaderPlugIn.HeaderSupporter.GetHasGSTANDQSTLine(); }
		}

		public ZBool HasGSTANDQCTLine
		{
			get { return HeaderPlugIn.HeaderSupporter.GetHasGSTANDQCTLine(); }
		}

		public ZBool HasGSTANDEDULine
		{
			get { return HeaderPlugIn.HeaderSupporter.GetHasGSTANDEDULine(); }
		}

		public ZBool HasRETLine
		{
			get { return HeaderPlugIn.HeaderSupporter.GetHasRETLine(); }
		}

		public ZBool HasIntegratedGSTLine
		{
			get { return HeaderPlugIn.HeaderSupporter.GetHasIntegratedGSTLine(); }
		}

		public ZBool HasStateGSTLine
		{
			get { return HeaderPlugIn.HeaderSupporter.GetHasStateGSTLine(); }
		}

		public ZBool HasZeroIGSTAmount
		{
			get { return HeaderPlugIn.HeaderSupporter.GetHasZeroIGSTAmount(); }
		}

		public ZString TotalOSIntegratedGSTAmountFormatted
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTotalOSIntegratedGSTAmountFormatted(); }
		}

		public ZString TotalOSCentreGSTAmountFormatted
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTotalOSCentreGSTAmountFormatted(); }
		}

		public ZString TotalOSStateGSTAmountFormatted
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTotalOSStateGSTAmountFormatted(); }
		}

		public ZDecimal GSTAmount
		{
			get { return HeaderPlugIn.HeaderSupporter.GetGSTAmount(); }
		}

		public ZDecimal OSGSTAmount
		{
			get { return HeaderPlugIn.HeaderSupporter.GetOSGSTAmount(); }
		}

		public ZBool ShowLocalGSTAmount
		{
			get { return HeaderPlugIn.HeaderSupporter.GetShowLocalGSTAmount(); }
		}

		public ZString OSPrimaryTaxDisplayHeading
		{
			get { return HeaderPlugIn.HeaderSupporter.GetOSPrimaryTaxDisplayHeading(); }
		}

		public ZString InvoiceSubTotalDisplayHeading
		{
			get { return HeaderPlugIn.HeaderSupporter.GetInvoiceSubTotalDisplayHeading(); }
		}

		public ZString OSTaxExtraRateQCTDisPlay
		{
			get { return HeaderPlugIn.HeaderSupporter.GetOSTaxExtraRateQCTDisPlay(); }
		}

		public ZString OSTaxExtraRateSBCDisPlay
		{
			get { return HeaderPlugIn.HeaderSupporter.GetOSTaxExtraRateSBCDisPlay(); }
		}

		public ZString OSTaxExtraRateKKCDisPlay
		{
			get { return HeaderPlugIn.HeaderSupporter.GetOSTaxExtraRateKKCDisPlay(); }
		}

		public ZString OSSPVExtraTaxLabel => HeaderPlugIn.HeaderSupporter.GetOSSPVExtraTaxLabel();

		public ZString OSSPVExtraTaxCodeLabel => HeaderPlugIn.HeaderSupporter.GetOSSPVExtraTaxCodeLabel();

		public ZDateTime InvoiceTaxDate => HeaderPlugIn.HeaderSupporter.GetInvoiceTaxDate();

		public ZString InvoiceTaxDateHeading => HeaderPlugIn.HeaderSupporter.GetInvoiceTaxDateHeading();

		public ZInt PostPeriod
		{
			get { return HeaderPlugIn.HeaderSupporter.GetPostPeriod(); }
		}

		public ZString PeriodDisplay
		{
			get { return HeaderPlugIn.HeaderSupporter.GetPeriodDisplay(); }
		}

		public ZString AgePeriodDisplay
		{
			get { return HeaderPlugIn.HeaderSupporter.GetAgePeriodDisplay(); }
		}

		public ZString JournalType
		{
			get { return HeaderPlugIn.HeaderSupporter.GetJournalType(); }
		}

		public ZString JobNumber
		{
			get { return HeaderPlugIn.HeaderSupporter.GetJobNumber(); }
		}

		public ZDecimal TotalDebitAmount => HeaderPlugIn.HeaderSupporter.TotalDebitAmount;

		public ZDecimal TotalCreditAmount => HeaderPlugIn.HeaderSupporter.TotalCreditAmount;

		public ZString StatementTotalOverdueAmount
		{
			get { return HeaderPlugIn.HeaderSupporter.GetStatementTotalOverdueAmount(); }
		}
		public ZString StatementTotalDueAmount
		{
			get { return HeaderPlugIn.HeaderSupporter.GetStatementTotalDueAmount(); }
		}
		public ZString StatementTotalStatementAmount
		{
			get { return HeaderPlugIn.HeaderSupporter.GetStatementTotalStatementAmount(); }
		}
		public ZString StatementTotalCurrentAmount
		{
			get { return HeaderPlugIn.HeaderSupporter.GetStatementTotalCurrentAmount(); }
		}

		public ZString Total30DaysOverdue
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTotal30DaysOverdue(); }
		}
		public ZString Total60DaysOverdue
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTotal60DaysOverdue(); }
		}
		public ZString Total90DaysOverdue
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTotal90DaysOverdue(); }
		}
		public ZString Total90PlusDaysOverdue
		{
			get { return HeaderPlugIn.HeaderSupporter.GetTotal90PlusDaysOverdue(); }
		}

		public ZString TotalDueBetween0To29thDay
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetTotalDueBetween0To29thDay();
			}
		}
		public ZString TotalDueBetween30To59thDay
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetTotalDueBetween30To59thDay();
			}
		}
		public ZString TotalDueBetween60To89thDay
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetTotalDueBetween60To89thDay();
			}
		}
		public ZString TotalDueBetween90To119thDay
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetTotalDueBetween90To119thDay();
			}
		}
		public ZString TotalDueBetween120To149thDay
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetTotalDueBetween120To149thDay();
			}
		}
		public ZString TotalDue150PlusDay
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetTotalDue150PlusDay();
			}
		}

		public ZString Total30DaysDue
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetTotal30DaysDue();
			}
		}
		public ZString Total30PlusDaysOverDue
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetTotal30PlusDaysOverDue();
			}
		}

		public ZString RequestedBy
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetRequestedBy();
			}
		}

		public ZDateTime RequestedTime
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetRequestedTime();
			}
		}

		public ZString ApprovedBy
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetApprovedBy();
			}
		}

		public ZDateTime ApprovedTime
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetApprovedTime();
			}
		}

		public ZString OriginalRequester
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetOriginalRequester();
			}
		}

		public ZString OriginalApprover
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetOriginalApprover();
			}
		}

		public DocBankAccount Account
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetAccount();
			}
		}

		public DocTransactionHeaderCollection CashTransactions
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetCashTransactions();
			}
		}

		public DocTransactionHeaderCollection CreditCardTransactions
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetCreditCardTransactions();
			}
		}

		public DocTransactionHeaderCollection ChequeTransactions
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetChequeTransactions();
			}
		}

		public DocTransactionHeaderCollection DirectCreditTransactions
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetDirectCreditTransactions();
			}
		}

		public DocTaxTransactionCollection TaxTransactions => HeaderPlugIn.HeaderSupporter.GetTaxTransactions();

		public ZInt TotalChequeCount
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetTotalChequeCount();
			}
		}

		public ZDecimal TotalCashAmount
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetTotalCashAmount();
			}
		}

		public ZDecimal TotalCreditCardAmount
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetTotalCreditCardAmount();
			}
		}

		public ZDecimal TotalChequeAmount
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetTotalChequeAmount();
			}
		}

		public ZDecimal TotalDirectCreditAmount
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetTotalDirectCreditAmount();
			}
		}

		public ZDecimal DepositBatchTotalAmountForBank
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetDepositBatchTotalAmountForBank();
			}
		}

		public ZDecimal DepositBatchTotalAmount
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetDepositBatchTotalAmount();
			}
		}

		public ZString AJOptionField1Caption
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetAJOptionField1Caption();
			}
		}

		public ZString AJOptionField1Value
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetAJOptionField1Value();
			}
		}

		public ZString AJOptionField2Caption
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetAJOptionField2Caption();
			}
		}
		public ZString AJOptionField2Value
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetAJOptionField2Value();
			}
		}

		public ZString AJOptionField3Caption
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetAJOptionField3Caption();
			}
		}

		public ZString AJOptionField3Value
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetAJOptionField3Value();
			}
		}

		public ZString ReceiptBankAccountIBAN
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetReceiptBankAccountIBAN();
			}
		}

		public DocAccountingVoucher Voucher
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetVoucher();
			}
		}

		public ZString EInvoicingGovernmentAllocatedNumber
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetEInvoicingGovernmentAllocatedNumber();
			}
		}
		public ZString EInvoicingAuthorisationNumber
		{
			get
			{
				return HeaderPlugIn.HeaderSupporter.GetEInvoicingAuthorisationNumber();
			}
		}
	}
}
