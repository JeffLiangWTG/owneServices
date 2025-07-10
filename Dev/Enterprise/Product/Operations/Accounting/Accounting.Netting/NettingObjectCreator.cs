#if DEBUG
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Netting
{
	public class NettingObjectCreator : TestObjectCreator
	{
		public NettingObjectCreator(BusinessObjectFactory factory) : base(factory)
		{
		}

		public INettingTransaction CreateNettingTransaction(ZString type, NettingSystemPeriod period, NettingOrganisation issuer, NettingOrganisation recipient, ZString reference
			, ZString currency, ZDecimal amount, string status = "APP", string transactionType = "INV")
		{
			INettingTransaction transaction = null;

			if (type == "AR")
			{
				transaction = Factory.New<NettingReceivableTransaction>();
			}
			else
			{
				transaction = Factory.New<NettingPayableTransaction>();
			}

			transaction.Date = ZDateTime.Now;
			transaction.DueDate = ZDateTime.Now;
			transaction.NettingSystemPK = period.NettingSystem.PK;
			transaction.NettingPeriodPK = period.PK;
			transaction.IssuerPK = issuer.PK;
			transaction.RecipientPK = recipient.PK;
			transaction.Reference = reference;
			transaction.Currency = currency;
			transaction.Amount = amount;
			transaction.ApprovalStatus = status;
			transaction.TransactionType = transactionType;

			return transaction;
		}

		public INettingTransactionReference AddNettingTransactionReference(INettingTransaction transaction, ZString refType, ZString refValue)
		{
			var transactionRef = transaction.AddNewTransactionReference();
			transactionRef.Type = refType;
			transactionRef.Reference = refValue;

			return transactionRef;
		}

		public INettingTransactionLine CreateNettingTransactionLine(INettingTransaction transaction, ZString jobReference, ZDecimal amount, ZString currency)
		{
			var line = transaction.AddNewLine();
			line.JobReference = jobReference;
			line.TransactionCurrency = currency;
			line.Amount = amount;

			return line;
		}

		public INettingTransactionLineReference AddNettingLineReference(INettingTransactionLine line, ZString refType, ZString refValue)
		{
			var lineRef = line.AddNewLineReference();
			lineRef.Type = refType;
			lineRef.Reference = refValue;

			return lineRef;
		}

		public NettingSystem CreateNettingSystem(string code, string description, GlbCompany company)
		{
			var nettingSystem = Factory.New<NettingSystem>();
			nettingSystem.NS_Code = code;
			nettingSystem.NS_Description = description;
			nettingSystem.NS_GC = company.PK;
			nettingSystem.NS_IsActive = true;

			return nettingSystem;
		}

		public NettingSystemPeriod CreateNettingPeriod(NettingSystem ns, ZString period, ZDateTime earliestInvoiceDate, ZDateTime latestInvoiceDate
			, ZDateTime nettingExecutionDate, ZDateTime latestApprovalDate, ZDateTime latestFxOfferDate, ZDateTime latestUploadDate, ZDate valueDate)
		{
			var nsp = Factory.New<NettingSystemPeriod>();
			nsp.NSP_Period = period;
			nsp.NSP_NS_NettingSystem = ns.PK;
			nsp.NSP_EarliestInvoiceDateUtc = earliestInvoiceDate;
			nsp.NSP_LatestInvoiceDateUtc = latestInvoiceDate;
			nsp.NSP_NettingExecutionDateUtc = nettingExecutionDate;
			nsp.NSP_LatestApprovalDateUtc = latestApprovalDate;

			nsp.NSP_LatestFXOfferDateUtc = latestFxOfferDate;
			nsp.NSP_LatestUploadDateUtc = latestUploadDate;

			nsp.NSP_ValueDate = valueDate;
			nsp.NSP_OfferPrepaymentDate = valueDate;

			return nsp;
		}

		public NettingSystemPeriod CreateNextNettingPeriod(NettingSystemPeriod period, ZString value)
		{
			var nsp = Factory.New<NettingSystemPeriod>();
			nsp.NSP_Period = value;
			nsp.NSP_NS_NettingSystem = period.NettingSystem.PK;
			nsp.NSP_EarliestInvoiceDateUtc = period.NSP_EarliestInvoiceDateUtc.AddDays(30);
			nsp.NSP_LatestInvoiceDateUtc = period.NSP_LatestInvoiceDateUtc.AddDays(30);
			nsp.NSP_NettingExecutionDateUtc = period.NSP_NettingExecutionDateUtc.AddDays(30);
			nsp.NSP_LatestApprovalDateUtc = period.NSP_LatestApprovalDateUtc.AddDays(30);

			nsp.NSP_LatestFXOfferDateUtc = period.NSP_LatestFXOfferDateUtc.AddDays(30);
			nsp.NSP_LatestUploadDateUtc = period.NSP_LatestUploadDateUtc.AddDays(30);

			nsp.NSP_ValueDate = period.NSP_ValueDate.AddDays(30);
			nsp.NSP_OfferPrepaymentDate = period.NSP_OfferPrepaymentDate.AddDays(30);

			return nsp;
		}

		public NettingOrganisation CreateNettingOrganisation(NettingSystem nettingSystem, OrgHeader organisation, string nettingType)
		{
			var nettingOrg = Factory.New<NettingOrganisation>();

			nettingOrg.NSO_NS_NettingSystem = nettingSystem.PK;
			nettingOrg.NSO_OH_Organisation = organisation.PK;
			nettingOrg.NSO_NettingType = nettingType;
			return nettingOrg;
		}

		public NettingSystemExchangeRate CreateNettingExchangeRate(NettingSystem system, ZString currency, ZString rateType, ZDecimal rate, NettingSystemPeriod period)
		{
			var exRate = Factory.New<NettingSystemExchangeRate>();
			exRate.NER_NS_NettingSystem = system.PK;
			exRate.NER_RX_NKCurrency = currency;
			exRate.NER_RateType = rateType;
			exRate.NER_Rate = rate;
			exRate.NER_NSP_Period = period.PK;
			return exRate;
		}

		public NettingMatchPivot CreateFullMatchingNettingTransaction(NettingSystemPeriod period, NettingOrganisation issuer, NettingOrganisation recipient, ZString reference, ZString currency, ZDecimal amount)
		{
			NettingReceivableTransaction receivableTransaction;
			NettingPayableTransaction payableTransaction;
			CreateNettingTransactionPairWithoutMatching(period, issuer, recipient, reference, currency, amount, amount, out receivableTransaction, out payableTransaction);

			return CreateNettingMatchingRecord(period, receivableTransaction, payableTransaction);
		}

		[SuppressMessage("Microsoft.Design", "CA1021: Avoid out parameters")]
		public void CreateNettingTransactionPairWithoutMatching(NettingSystemPeriod period, NettingOrganisation issuer, NettingOrganisation recipient, ZString reference, ZString currency, ZDecimal arAmount, ZDecimal apAmount, out NettingReceivableTransaction receivableTransaction, out NettingPayableTransaction payableTransaction)
		{
			receivableTransaction = Factory.New<NettingReceivableTransaction>();
			payableTransaction = Factory.New<NettingPayableTransaction>();

			foreach (INettingTransaction transaction in new INettingTransaction[] { receivableTransaction, payableTransaction })
			{
				transaction.Date = ZDateTime.Now;
				transaction.DueDate = ZDateTime.Now;
				transaction.NettingSystemPK = period.NettingSystem.PK;
				transaction.NettingPeriodPK = period.PK;
				transaction.IssuerPK = issuer.PK;
				transaction.RecipientPK = recipient.PK;
				transaction.Reference = reference;
				transaction.Currency = currency;
				transaction.TransactionType = "INV";

				if (transaction is NettingReceivableTransaction)
				{
					transaction.Amount = arAmount;
				}
				else
				{
					transaction.Amount = apAmount;
				}
				transaction.ApprovalStatus = NettingTransactionApprovalStatus.Approved;
			}
		}

		public NettingMatchPivot CreateNettingMatchingRecord(NettingSystemPeriod period, NettingReceivableTransaction receivableTransaction, NettingPayableTransaction payableTransaction)
		{
			if (receivableTransaction.ApprovalStatus != NettingTransactionApprovalStatus.Matched)
			{
				receivableTransaction.ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			}
			if (payableTransaction.ApprovalStatus != NettingTransactionApprovalStatus.Matched)
			{
				payableTransaction.ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			}

			var pivot = Factory.New<NettingMatchPivot>();
			pivot.NMP_NSP_Period = period.PK;
			pivot.NMP_NRT_ReceivableTransaction = receivableTransaction.PK;
			pivot.NMP_NPT_PayableTransaction = payableTransaction.PK;

			return pivot;
		}

		public void NettingCopyIndicativeRatesToExecutionRates(NettingSystemPeriod period)
		{
			var indicativeRatesQuery = new ZQuery(NettingSystemExchangeRateSchema.NER_NSP_Period, period.PK);
			indicativeRatesQuery.AddToFilter(NettingSystemExchangeRateSchema.NER_RateType, NettingExchangeRateType.Indicative);

			var indicativeRates = Factory.Load<NettingSystemExchangeRate>(indicativeRatesQuery);
			foreach (var indicativeRate in indicativeRates)
			{
				var executionRate = Factory.New<NettingSystemExchangeRate>();
				executionRate.NER_NSP_Period = period.PK;
				executionRate.NER_NS_NettingSystem = indicativeRate.NER_NS_NettingSystem;
				executionRate.NER_RX_NKCurrency = indicativeRate.NER_RX_NKCurrency;
				executionRate.NER_RateType = NettingExchangeRateType.Execution;
				executionRate.NER_Rate = indicativeRate.NER_Rate;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Direct call to a stored proc")]
		public void GenerateCalculationRecords(Guid nettingPeriodPK)
		{
			using (var cmd = Db.Connection.Command("CreateNettingCalculationRecords"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@NettingPeriod", SqlDbType.UniqueIdentifier, nettingPeriodPK);
				cmd.AddParameter("@CurrentDate", SqlDbType.SmallDateTime, ZDateTime.UtcNow.ToDateTime());
				var result = cmd.ExecuteProcedureWithReturnValue();
			}
		}
	}
}

#endif
