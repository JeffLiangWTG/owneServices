using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch
{
	public class eNettPaymentBatchManager
	{
		public eNettPaymentBatchManager(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		public bool AddToExistingBatchOrCreateNewBatch(Payment payment, ZDateTime batchDate)
		{
			return AddToExistingBatchOrCreateNewBatch(payment, batchDate, true);
		}

		public bool AddToExistingBatchOrCreateNewBatch(Payment payment, ZDateTime batchDate, bool saveFactory)
		{
			bool result = false;

			if (payment.AH_ReceiptBatchNo.IsEmpty && payment.AH_ReceiptType == ReceiptTypes.eNettDirectDebit)
			{
				Factory.SetContext(BusinessContext.ExcludeFromDirectDebitBatchCriticalValidation);

				if (batchDate.IsValid && batchDate.Date >= ZDateTime.Today)
				{
					result = AddToExistingBatch(payment, batchDate);
				}

				if (!result)
				{
					result = CreateNewBatch(payment, batchDate);
				}

				if (saveFactory && result)
				{
					try
					{
						Factory.Save();
					}
					finally
					{
						Factory.RemoveContext(BusinessContext.ExcludeFromDirectDebitBatchCriticalValidation);
					}
				}
			}

			return result;
		}

		bool CreateNewBatch(Payment payment, ZDateTime batchDate)
		{
			bool result = false;

			if (payment.AH_ReceiptBatchNo.IsEmpty && payment.AH_ReceiptType == ReceiptTypes.eNettDirectDebit)
			{
				var batchDateForPosting = (batchDate.IsValid && batchDate.Date >= ZDateTime.Today) ? batchDate.Date : ZDateTime.Today;

				var ddrBatch = Factory.New<DirectDebitBatchHeader>();
				ddrBatch.AH_GC = payment.AH_GC;
				ddrBatch.AH_InvoiceDate = batchDateForPosting;
				ddrBatch.AH_PostDate = batchDateForPosting;
				ddrBatch.AH_AB = payment.AH_AB;
				ddrBatch.AH_ReceiptType = GetBatchReceiptType(payment.AH_RX_NKTransactionCurrency, payment.AH_GC);
				ddrBatch.AH_Desc = GetDescriptionForCompayBatchDate(batchDate);
				ddrBatch.AH_GB = payment.AH_GB;
				ddrBatch.AH_GE = payment.AH_GE;

				foreach (IDirectDebitBatchTransaction otherPayment in ddrBatch.Lines)
				{
					otherPayment.IncludeInTheBatch = false;
				}

				AddPaymentToBatchHeader(ddrBatch, payment);
				result = true;
			}

			return result;
		}

		bool AddToExistingBatch(Payment payment, ZDateTime batchDate)
		{
			bool result = false;

			if (batchDate.IsValid && batchDate.Date >= ZDateTime.Today &&
				payment.AH_ReceiptBatchNo.IsEmpty && payment.AH_ReceiptType == ReceiptTypes.eNettDirectDebit)
			{
				var batchHeader = FindUnclearedDirectDebitBatch(payment.BankAccount, payment.AH_RX_NKTransactionCurrency, batchDate);

				if (batchHeader != null)
				{
					if (!batchHeader.IsInDatabase)
					{
						AddPaymentToBatchHeader(batchHeader, payment);
					}
					else
					{
						payment.AH_ReceiptBatchNo = batchHeader.AH_ReceiptBatchNo;
						batchHeader.AH_InvoiceAmount += payment.AH_InvoiceAmount;
						batchHeader.AH_OSTotal += payment.AH_OSTotal;
					}

					result = true;
				}
			}

			return result;
		}

		void AddPaymentToBatchHeader(DirectDebitBatchHeader directDebitBatch, Payment payment)
		{
			directDebitBatch.Lines.Add(payment);
			payment.SetDDRCollection(directDebitBatch.Lines);
			payment.IncludeInTheBatch = true;
		}

		DirectDebitBatchHeader FindUnclearedDirectDebitBatch(AccBankAccount bankAccount, ZString currencyCode, ZDateTime batchDate)
		{
			DirectDebitBatchHeader result = null;

			if (bankAccount != null && !batchDate.IsEmpty)
			{
				var findBatchHeaderQuery = new ZQuery();
				findBatchHeaderQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.DDRBatch);
				findBatchHeaderQuery.AddToFilter(AccTransactionHeaderSchema.AH_InvoiceDate, SQLComparisonOperator.EqualToDatePartOnly, batchDate);
				findBatchHeaderQuery.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, SQLComparisonOperator.EqualToDatePartOnly, batchDate);
				findBatchHeaderQuery.AddToFilter(AccTransactionHeaderSchema.AH_DateClearedInCashbook, null);
				findBatchHeaderQuery.AddToFilter(AccTransactionHeaderSchema.AH_AB, bankAccount.PK);
				findBatchHeaderQuery.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, ZBool.False);
				findBatchHeaderQuery.AddToFilter(AccTransactionHeaderSchema.AH_Desc, GetDescriptionForCompayBatchDate(batchDate));
				findBatchHeaderQuery.AddToFilter(AccTransactionHeaderSchema.AH_InvoiceAmount, SQLComparisonOperator.GreaterThan, 0);
				findBatchHeaderQuery.AddToFilter(AccTransactionHeaderSchema.AH_ReceiptType, GetBatchReceiptType(currencyCode, bankAccount.AB_GC));
				findBatchHeaderQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, bankAccount.AB_GC);
				findBatchHeaderQuery.OrderBy = AccTransactionHeaderSchema.Constants.AH_TransactionNum + " DESC ";

				result = Factory.LoadTop1<DirectDebitBatchHeader>(findBatchHeaderQuery);
			}

			return result;
		}

		ZString GetBatchReceiptType(ZString currencyCode, ZGuid companyPK)
		{
			var company = Factory.Load<GlbCompany>(companyPK);
			var receiptType = string.Empty;
			if (company != null)
			{
				receiptType = (currencyCode == company.GC_RX_NKLocalCurrency) ? ReceiptTypes.eNettDirectDebit : ReceiptTypes.eNettDirectDebitForeignCurrency;
			}
			return receiptType;
		}

		readonly BusinessObjectFactory Factory;

		static string GetDescriptionForCompayBatchDate(ZDateTime batchDate)
		{
			string dayOfTheMonth = (batchDate.IsValid && !batchDate.IsEmpty) ? batchDate.Day.ToString("00") : "  ";
			string comPayRegistrationNumber = AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode.Left(50);
			string date = (batchDate.IsValid && !batchDate.IsEmpty) ? batchDate.ToString("ddMMyyyy") : "";
			return string.Format((NoResString)"ENI{0}-{1} CREDIT-{2}", dayOfTheMonth, comPayRegistrationNumber, date);
		}
	}
}
