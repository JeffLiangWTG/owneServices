using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.Accounting.DocRollUpSort;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public interface IGenericTransactionLinePlugIn
	{
		GenericTransactionLineSupporter LineSupporter { get; }
	}

	public abstract class GenericTransactionLineSupporter
	{
		protected internal virtual DocGLAccount GetGLAccount()
		{
			return null;
		}

		protected internal virtual DocBranch GetBranch()
		{
			return null;
		}

		protected internal virtual DocDepartment GetDepartment()
		{
			return null;
		}

		protected internal virtual ZString GetDescription()
		{
			return null;
		}

		protected internal virtual DocCurrency GetCurrency()
		{
			return null;
		}

		protected internal virtual ZDecimal GetOverseasTotal()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZDecimal GetExchangeRate()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZString GetStatus()
		{
			return null;
		}

		protected internal virtual ZDecimal GetLocalAmountAndTax()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZDecimal GetLineAmount()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZDecimal GetLocalTotalAmount()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZDecimal GetLocalPaidAmount()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZDecimal GetOSAmount()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZDecimal GetOSPaidAmount()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZDecimal GetOSExTaxAmount()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZDecimal GetOSTaxAmount_Raw()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZString GetOSTaxDisplay()
		{
			return null;
		}

		protected internal virtual ZString GetOSTaxAmountDisplay()
		{
			return null;
		}

		protected internal virtual ZString GetOSTaxDisplayNoAsterisksWithRegistryRule()
		{
			return null;
		}

		protected internal virtual ZString GetTaxRateAsterisksAsNumbers()
		{
			return null;
		}

		protected internal virtual ZBool GetIsSpacerLine()
		{
			return false;
		}

		protected internal virtual DocChargeCode GetChargeCode()
		{
			return null;
		}

		protected internal virtual ZString GetGenericChargeCode()
		{
			return null;
		}

		protected internal virtual ZString GetGenericChargeDescription()
		{
			return null;
		}

		protected internal virtual ZString GetGenericChargeType()
		{
			return null;
		}

		protected internal virtual DocJobHeader GetJobHeader()
		{
			return null;
		}

		protected internal virtual DocShipment GetShipment()
		{
			return null;
		}

		protected internal virtual ZString GetDescriptionOne()
		{
			return null;
		}

		protected internal virtual ZString GetDescriptionTwo()
		{
			return null;
		}

		protected internal virtual ZInt GetCount()
		{
			return ZInt.Zero;
		}

		protected internal virtual ZString GetOrganisationCode()
		{
			return null;
		}

		protected internal virtual ZString GetOrganisationName()
		{
			return null;
		}

		protected internal virtual ZString GetTotalOverdueByCurrencyFormatted()
		{
			return null;
		}

		protected internal virtual DocPaymentApprovalItem GetFlattenedHeaderForPaymentApprovalItem()
		{
			return null;
		}

		protected internal virtual DocTransactionHeader GetFlattenedHeaderForTransactionHeader()
		{
			return null;
		}

		protected internal virtual DocTransactionLine GetFlattenedLine()
		{
			return null;
		}

		protected internal virtual ZString GetDebitAmount()
		{
			return ZString.Empty;
		}

		protected internal virtual ZString GetCreditAmount()
		{
			return ZString.Empty;
		}

		protected internal virtual ZString GetForeignCurrencyEquivalentAmount()
		{
			return ZString.Empty;
		}

		protected internal virtual ZDecimal GetDebitAmountDecimal()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZDecimal GetCreditAmountDecimal()
		{
			return ZDecimal.Zero;
		}

		protected internal virtual ZString GetDebitCreditSign()
		{
			return ZString.Empty;
		}

		protected internal virtual ZString GetOSUnsignedAmount()
		{
			return ZString.Empty;
		}

		protected internal virtual ZString GetMultiSubAccountTypeCode()
		{
			return ZString.Empty;
		}

		protected internal virtual ZString GetRevenueRecognitionType()
		{
			return ZString.Empty;
		}

		protected internal virtual ZString GetTaxBasis()
		{
			return ZString.Empty;
		}

		protected internal virtual ZDateTime GetPostDate()
		{
			return ZDateTime.Empty;
		}

		protected internal virtual ZString GetPostPeriod()
		{
			return ZString.Empty;
		}

		protected internal virtual ZBool GetIsMissingPeriodForReportingBook()
		{
			return false;
		}

		protected internal virtual ZString GetGovernmentReportingCode()
		{
			return ZString.Empty;
		}

		protected internal virtual ZString GetGovernmentReportingCodeHeading()
		{
			return ZString.Empty;
		}

		internal ZString GetOSTaxAmount_RawDisplay()
		{
			return ZString.Empty;
		}

		protected internal virtual ZString GetAlternateGLAccount()
		{
			return ZString.Empty;
		}

		protected internal virtual ZString GetParentAccountNum()
		{
			return ZString.Empty;
		}

		protected internal virtual ZString GetJournalEntriesNumber()
		{
			return ZString.Empty;
		}

		protected internal virtual ZBool GetGenerateAndStoreJournalEntriesForPostedAccountingTransactionsOptions()
		{
			return false;
		}
	}

	public class DocGenericTransactionLineCollection : DocumentWrapperCollection<DocGenericTransactionLine>
	{
		public DocGenericTransactionLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}

	public class DocGenericTransactionLine : DocBaseWrapper
	{
		protected DocGenericTransactionLine(BusinessObject businessObject, BusinessObjectFactory factoryToWrap)
			: base(businessObject, factoryToWrap)
		{
			this.BusinessObject = businessObject;
			this.LinePlugIn = GetLinePlugInForBusinessObject(businessObject, factoryToWrap);
		}

		public static DocGenericTransactionLine New(BusinessObject businessObject, BusinessObjectFactory factoryToWrap)
		{
			DocGenericTransactionLine result = null;
			if (businessObject != null)
			{
				result = new DocGenericTransactionLine(businessObject, factoryToWrap);
			}
			return result;
		}

		static IGenericTransactionLinePlugIn GetLinePlugInForBusinessObject(BusinessObject businessObject, BusinessObjectFactory factoryToWrap)
		{
			IGenericTransactionLinePlugIn plugin = null;
			if (businessObject is TransactionLine)
			{
				return DocTransactionLine.New((TransactionLine)businessObject, factoryToWrap);
			}
			else if (businessObject is AccCashAdvanceRequestLine)
			{
				return DocCashAdvanceRequestLine.New((AccCashAdvanceRequestLine)businessObject, factoryToWrap);
			}
			else if (businessObject is DocTransactionHeader.FlattenedLine)
			{
				return (DocTransactionHeader.FlattenedLine)businessObject;
			}
			else if (businessObject is DocPaymentApproval.FlattenedLine)
			{
				return (DocPaymentApproval.FlattenedLine)businessObject;
			}
			else if (businessObject is DocTransactionHeader.FlattenedLine)
			{
				return (DocTransactionHeader.FlattenedLine)businessObject;
			}
			else if (businessObject is DocStatementSummaryLine)
			{
				return (DocStatementSummaryLine)businessObject;
			}
			else if ((plugin = businessObject as DocAccountingJournalLine) != null)
			{
				return plugin;
			}
			else if (businessObject is DocARInvoiceLine)
			{
				return (DocARInvoiceLine)businessObject;
			}
			else if (businessObject is DocARInvoiceLineForRollUp)
			{
				return (DocARInvoiceLineForRollUp)businessObject;
			}
			else
			{
				throw new Exception("No suitable DocWrapper can be found!");
			}
		}

		public readonly BusinessObject BusinessObject;
		public readonly IGenericTransactionLinePlugIn LinePlugIn;

		public ZString OSTaxDisplay
		{
			get { return LinePlugIn.LineSupporter.GetOSTaxDisplay(); }
		}

		public ZString OSTaxAmountDisplay
		{
			get { return LinePlugIn.LineSupporter.GetOSTaxAmountDisplay(); }
		}

		public ZDecimal OSTaxAmount_Raw
		{
			get { return LinePlugIn.LineSupporter.GetOSTaxAmount_Raw(); }
		}

		public ZString OSTaxDisplayNoAsterisksWithRegistryRule
		{
			get { return LinePlugIn.LineSupporter.GetOSTaxDisplayNoAsterisksWithRegistryRule(); }
		}

		public ZString TaxRateAsterisksAsNumbers
		{
			get { return LinePlugIn.LineSupporter.GetTaxRateAsterisksAsNumbers(); }
		}

		public ZBool IsSpacerLine
		{
			get { return LinePlugIn.LineSupporter.GetIsSpacerLine(); }
		}

		public DocChargeCode ChargeCode
		{
			get { return LinePlugIn.LineSupporter.GetChargeCode(); }
		}

		public ZString GenericChargeCode
		{
			get { return LinePlugIn.LineSupporter.GetGenericChargeCode(); }
		}

		public ZString GenericChargeDescription
		{
			get { return LinePlugIn.LineSupporter.GetGenericChargeDescription(); }
		}

		public ZString GenericChargeType
		{
			get { return LinePlugIn.LineSupporter.GetGenericChargeType(); }
		}

		public DocJobHeader JobHeader
		{
			get { return LinePlugIn.LineSupporter.GetJobHeader(); }
		}

		public DocShipment Shipment
		{
			get { return LinePlugIn.LineSupporter.GetShipment(); }
		}

		public ZString DescriptionOne
		{
			get { return LinePlugIn.LineSupporter.GetDescriptionOne(); }
		}

		public ZString DescriptionTwo
		{
			get { return LinePlugIn.LineSupporter.GetDescriptionTwo(); }
		}

		public ZInt Count
		{
			get { return LinePlugIn.LineSupporter.GetCount(); }
		}

		public ZString OrganisationCode
		{
			get { return LinePlugIn.LineSupporter.GetOrganisationCode(); }
		}

		public ZString OrganisationName
		{
			get { return LinePlugIn.LineSupporter.GetOrganisationName(); }
		}

		public ZString TotalOverdueByCurrencyFormatted
		{
			get { return LinePlugIn.LineSupporter.GetTotalOverdueByCurrencyFormatted(); }
		}

		public DocGLAccount GLAccount
		{
			get { return LinePlugIn.LineSupporter.GetGLAccount(); }
		}

		public DocBranch Branch
		{
			get { return LinePlugIn.LineSupporter.GetBranch(); }
		}

		public DocDepartment Department
		{
			get { return LinePlugIn.LineSupporter.GetDepartment(); }
		}

		public ZString Description
		{
			get { return LinePlugIn.LineSupporter.GetDescription(); }
		}

		public ZDecimal OverseasTotal
		{
			get { return LinePlugIn.LineSupporter.GetOverseasTotal(); }
		}

		public ZDecimal ExchangeRate
		{
			get { return LinePlugIn.LineSupporter.GetExchangeRate(); }
		}

		public DocCurrency Currency
		{
			get { return LinePlugIn.LineSupporter.GetCurrency(); }
		}

		public ZDecimal LocalAmountAndTax
		{
			get { return LinePlugIn.LineSupporter.GetLocalAmountAndTax(); }
		}

		public ZDecimal OsAmount
		{
			get { return LinePlugIn.LineSupporter.GetOSAmount(); }
		}

		public ZDecimal OsPaidAmount
		{
			get { return LinePlugIn.LineSupporter.GetOSPaidAmount(); }
		}

		public ZDecimal OsExTaxAmount
		{
			get { return LinePlugIn.LineSupporter.GetOSExTaxAmount(); }
		}

		public ZString OSTaxAmount_RawDisplay
		{
			get { return LinePlugIn.LineSupporter.GetOSTaxAmount_RawDisplay(); }
		}

		public ZDecimal LineAmount
		{
			get { return LinePlugIn.LineSupporter.GetLineAmount(); }
		}

		public ZDecimal LocalPaidAmount
		{
			get { return LinePlugIn.LineSupporter.GetLocalPaidAmount(); }
		}

		public ZDecimal LocalTotalAmount
		{
			get { return LinePlugIn.LineSupporter.GetLocalTotalAmount(); }
		}

		public ZString DebitAmount
		{
			get { return LinePlugIn.LineSupporter.GetDebitAmount(); }
		}

		public ZString CreditAmount
		{
			get { return LinePlugIn.LineSupporter.GetCreditAmount(); }
		}

		public ZString MultiSubAccountTypeCode
		{
			get { return LinePlugIn.LineSupporter.GetMultiSubAccountTypeCode(); }
		}

		public ZString ForeignCurrencyEquivalent
		{
			get { return LinePlugIn.LineSupporter.GetForeignCurrencyEquivalentAmount(); }
		}

		public ZString Status
		{
			get { return LinePlugIn.LineSupporter.GetStatus(); }
		}

		public ZDecimal DebitAmountDecimal
		{
			get { return LinePlugIn.LineSupporter.GetDebitAmountDecimal(); }
		}

		public ZDecimal CreditAmountDecimal
		{
			get { return LinePlugIn.LineSupporter.GetCreditAmountDecimal(); }
		}

		public ZString DebitCreditSign
		{
			get { return LinePlugIn.LineSupporter.GetDebitCreditSign(); }
		}

		public ZString OSUnsignedAmount
		{
			get { return LinePlugIn.LineSupporter.GetOSUnsignedAmount(); }
		}

		public DocPaymentApprovalItem HeaderForApproval
		{
			get { return LinePlugIn.LineSupporter.GetFlattenedHeaderForPaymentApprovalItem(); }
		}

		public DocTransactionHeader HeaderForTransaction
		{
			get { return LinePlugIn.LineSupporter.GetFlattenedHeaderForTransactionHeader(); }
		}

		public DocTransactionLine Line
		{
			get { return LinePlugIn.LineSupporter.GetFlattenedLine(); }
		}

		public ZDateTime PostDate
		{
			get { return LinePlugIn.LineSupporter.GetPostDate(); }
		}

		public ZString PostPeriod
		{
			get { return LinePlugIn.LineSupporter.GetPostPeriod(); }
		}

		public ZBool IsMissingPeriodForReportingBook
		{
			get { return LinePlugIn.LineSupporter.GetIsMissingPeriodForReportingBook(); }
		}

		public ZString RevenueRecognitionType
		{
			get { return LinePlugIn.LineSupporter.GetRevenueRecognitionType(); }
		}

		public ZString TaxBasis
		{
			get { return LinePlugIn.LineSupporter.GetTaxBasis(); }
		}

		public ZString GovernmentReportingCode
		{
			get { return LinePlugIn.LineSupporter.GetGovernmentReportingCode(); }
		}

		public ZString GovernmentReportingCodeHeading
		{
			get { return LinePlugIn.LineSupporter.GetGovernmentReportingCodeHeading(); }
		}

		public ZString AlternateGLAccount
		{
			get { return LinePlugIn.LineSupporter.GetAlternateGLAccount(); }
		}

		public ZString ParentAccountNum
		{
			get { return LinePlugIn.LineSupporter.GetParentAccountNum(); }
		}

		public ZString JournalEntriesNumber
		{
			get { return LinePlugIn.LineSupporter.GetJournalEntriesNumber(); }
		}

		public ZBool GenerateAndStoreJournalEntriesForPostedAccountingTransactionsOptions
		{
			get { return LinePlugIn.LineSupporter.GetGenerateAndStoreJournalEntriesForPostedAccountingTransactionsOptions(); }
		}
	}
}
