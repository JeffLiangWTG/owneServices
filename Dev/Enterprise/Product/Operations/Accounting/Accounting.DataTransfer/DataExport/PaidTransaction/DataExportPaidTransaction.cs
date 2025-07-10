using System.Diagnostics.CodeAnalysis;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.DataExport
{
	public class DataExportPaidTransaction : AutoDataExportPaidTransaction
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public DataExportPaidTransaction(BusinessObjectFactory factory, TransactionHeader transactionHeader, DataExportPayment payment)
			: base(factory)
		{
			TransactionHeader = transactionHeader;
			Payment = payment;
			AH_Desc = transactionHeader.AH_Desc;
			AH_DueDate = transactionHeader.AH_DueDate;
			AH_ExchangeRate = transactionHeader.AH_ExchangeRate;
			AH_InvoiceDate = transactionHeader.AH_InvoiceDate;
			AH_OH = transactionHeader.AH_OH;
			AH_OSExTaxAmount = transactionHeader.AH_OSExTaxAmount;
			AH_OSTaxAmount = transactionHeader.AH_OSTaxAmount;
			AH_OSTotalAmount = transactionHeader.AH_OSTotalAmount;
			AH_PostDate = transactionHeader.AH_PostDate;
			AH_RX_NKTransactionCurrency = transactionHeader.AH_RX_NKTransactionCurrency;
			AH_TransactionNum = transactionHeader.AH_TransactionNum;
			LocalMatchedAmount = transactionHeader.LocalMatchedAmount;
			OSMatchedAmount = transactionHeader.MatchedAmount;
			LocalAmountInPaymentCurrency = transactionHeader.AH_RX_NKTransactionCurrency == payment.AH_RX_NKTransactionCurrency
				? transactionHeader.MatchedAmount
				: TransactionHeaderOSOutstandingAmountProvider.GetHighPrecisionOSAmount(
					payment.TransactionHeader.AH_LocalTotal,
					payment.TransactionHeader.AH_OSTotalAmount,
					transactionHeader.LocalMatchedAmount,
					payment.TransactionHeader.AH_RX_NKTransactionCurrency);
			InvoiceRemittanceReference = transactionHeader.InvoiceRemittanceReference;
		}

		public DataExportDirectDebitBatchHeader DDRHeader { get; set; }

		public DataExportPayment Payment { get; }

		public TransactionHeader TransactionHeader { get; }

		#region AH_OH

		[List("Organisations")]
		public override ZGuid AH_OH
		{
			get { return base.AH_OH; }
			set { base.AH_OH = value; }
		}

		OrgHeaderCollection fOrganisations;

		public OrgHeaderCollection Organisations
		{
			get
			{
				if (fOrganisations == null)
				{
					fOrganisations = new OrgHeaderCollection(Factory);
				}
				return fOrganisations;
			}
		}

		#endregion

		#region Currency

		public RefCurrency Currency
		{
			get
			{
				return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, AH_RX_NKTransactionCurrency);
			}
		}

		#endregion
	}
}
