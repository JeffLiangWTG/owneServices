using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Netting
{
	public abstract class NettingStatement : NonPersistentBusinessObject, IObsoleteValidation, IDocumentSupportable
	{
		protected NettingStatement(BusinessObjectFactory factory, ZGuid nettingPeriod, StatementType statementType)
			: base(factory)
		{
			NettingPeriod = nettingPeriod;
			StatementType = statementType;
		}

		StatementType StatementType { get; }

		[List("NettingPeriodList")]
		public ZGuid NettingPeriod { get; set; }

		public NettingSystemPeriodCollection NettingPeriodList => nettingPeriodList ?? (nettingPeriodList = new NettingSystemPeriodCollection(Factory));
		NettingSystemPeriodCollection nettingPeriodList;

		public ZString NettingCurrency => GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

		public ZDateTime StartDate => Period != null ? Period.NSP_EarliestInvoiceDateUtc : ZDateTime.UtcToday;

		public ZDateTime EndDate => Period != null ? Period.NSP_LatestInvoiceDateUtc : ZDateTime.UtcToday;

		public NettingSystemPeriod Period => !NettingPeriod.IsEmpty ? Factory.Load<NettingSystemPeriod>(NettingPeriod) : null;

		public virtual IEnumerable<NettingTransaction> Transactions
		{
			get
			{
				if (transactions == null)
				{
					transactions = new List<NettingTransaction>();

					using (var command = GetStatementCommand())
					{
						using (var reader = command.ExecuteReader()) // Custom call to DB necessary since business object not present in the system
						{
							while (reader.Read())
							{
								PopulateNettingTransaction(reader);
							}
						}
					}
				}

				return transactions;
			}
		}

		void PopulateNettingTransaction(IDataReader reader)
		{
			var calculation = new NettingTransaction(this);
			calculation.MatchingTransactionReference = !reader.IsDBNull(0) ? reader.GetString(0) : string.Empty;
			calculation.OrgCode = !reader.IsDBNull(1) ? reader.GetString(1) : string.Empty;
			calculation.Currency = !reader.IsDBNull(2) ? reader.GetString(2) : string.Empty;
			calculation.Amount = !reader.IsDBNull(3) ? reader.GetDecimal(3) : 0M;
			calculation.NettingCurrency = !reader.IsDBNull(4) ? reader.GetString(4) : string.Empty;
			calculation.NettingSystemAmount = !reader.IsDBNull(5) ? reader.GetDecimal(5) : 0M;
			calculation.NettingSystemExchangeRate = !reader.IsDBNull(6) ? reader.GetDecimal(6) : 0M;
			calculation.ParticipatingOrgCode = !reader.IsDBNull(7) ? reader.GetString(7) : string.Empty;
			var issuerAmount = !reader.IsDBNull(8) ? reader.GetDecimal(8) : 0M;
			var issuerCurrency = !reader.IsDBNull(9) ? reader.GetString(9) : string.Empty;

			var recipientAmount = !reader.IsDBNull(11) ? reader.GetDecimal(11) : 0M;
			var recipientCurrency = !reader.IsDBNull(12) ? reader.GetString(12) : string.Empty;

			calculation.ParticipantExchangeRate = !reader.IsDBNull(10) ? reader.GetDecimal(10) : !reader.IsDBNull(13) ? reader.GetDecimal(13) : 0M;
			calculation.TransactionPK = !reader.IsDBNull(14) ? reader.GetGuid(14) : Guid.Empty;
			calculation.CompanyCode = !reader.IsDBNull(15) ? reader.GetString(15) : string.Empty;
			calculation.ParticipantCompanyCode = !reader.IsDBNull(16) ? reader.GetString(16) : string.Empty;
			calculation.ParticipantCompanyName = !reader.IsDBNull(17) ? reader.GetString(17) : string.Empty;
			calculation.ParticipantCurrency = !issuerCurrency.IsNullOrEmpty() ? issuerCurrency : recipientCurrency;
			calculation.ParticipantAmount = issuerAmount != 0 ? issuerAmount : recipientAmount;
			calculation.Ledger = !issuerCurrency.IsNullOrEmpty() ? LedgerTypes.AccountsReceivable : LedgerTypes.AccountsPayable;

			if (calculation.Ledger == LedgerTypes.AccountsReceivable)
			{
				calculation.ReceivableAmount = calculation.Amount;
			}
			else
			{
				calculation.PayableAmount = calculation.Amount;
			}

			transactions.Add(calculation);
		}

		List<NettingTransaction> transactions;

		internal void InstantiateTransactionReferencesForNettingClearingJournals()
		{
			foreach (var clearingJournal in NettingClearingJournals)
			{
				((ISupportTransactionReference)clearingJournal).TransactionRef = new NettingTransactionReference(this, NettingClearingJournals);
			}
		}

		internal void InstantiateTransactionLineReferencesForNettingClearingJournals()
		{
			foreach (var clearingJournal in NettingClearingJournals)
			{
				((ISupportTransactionReference)clearingJournal).TransactionLineRef = new NettingTransactionLineReference(this, NettingClearingJournals);
			}
		}

		internal void InstantiateTransactionReferencesForNettingTransactions()
		{
			foreach (var transaction in Transactions)
			{
				((ISupportTransactionReference)transaction).TransactionRef = new NettingTransactionReference(this, Transactions);
			}
		}

		internal void InstantiateTransactionLineReferencesForNettingTransacitons()
		{
			foreach (var transaction in Transactions)
			{
				((ISupportTransactionReference)transaction).TransactionLineRef = new NettingTransactionLineReference(this, Transactions);
			}
		}

		internal void PopulateTransactionLineReferenceField(IEnumerable<ISupportTransactionReference> collection, Func<NettingTransactionLineReference, ZPropertyInfo> propertyInfoGetter)
		{
			var transactionLineReferences = collection.ToList();
			if (!transactionLineReferences.Any())
			{
				return;
			}

			var propertyName = propertyInfoGetter(transactionLineReferences.First().TransactionLineRef).Name;
			var commaSeperatedpks = string.Join("','", transactionLineReferences.Select(x => x.TransactionPK.ToString()).Distinct());
			var sql = string.Format(CultureInfo.InvariantCulture, @"SELECT DISTINCT TransactionPK, {0} FROM dbo.vw_NettingTransactionLineReference WHERE TransactionPK IN ('{1}') AND {0} IS NOT NULL ORDER BY {0}", propertyName, commaSeperatedpks);

			var dynamicObjects = new DynamicBusinessObjectCollection(Factory);
			dynamicObjects.Load(sql);

			foreach (var item in transactionLineReferences)
			{
				var value = ZString.Empty;
				var references = dynamicObjects.Where(x => (ZGuid)x["TransactionPK"] == item.TransactionPK);
				if (references != null)
				{
					var sb = new ZStringBuilder();
					foreach (var reference in references)
					{
						sb.Append((ZString)reference[propertyName]);
					}
					value = sb.ToStringWithDelimiterBetweenAppends(", ");
				}

				item.TransactionLineRef.SetValue(propertyName, value);
			}
		}

		internal void PopulateTransactionReferenceField(IEnumerable<ISupportTransactionReference> collection, Func<NettingTransactionReference, ZPropertyInfo> propertyInfoGetter)
		{
			var transactionReferences = collection.ToList();
			if (!transactionReferences.Any())
			{
				return;
			}

			var propertyName = propertyInfoGetter(transactionReferences.First().TransactionRef).Name;
			var commaSeperatedpks = string.Join("','", transactionReferences.Select(x => x.TransactionPK.ToString()).Distinct());
			var sql = string.Format(CultureInfo.InvariantCulture, @"SELECT DISTINCT TransactionPK, {0} FROM dbo.vw_NettingTransactionReference WHERE TransactionPK IN ('{1}') AND {0} IS NOT NULL ORDER BY {0}", propertyName, commaSeperatedpks);

			var dynamicObjects = new DynamicBusinessObjectCollection(Factory);
			dynamicObjects.Load(sql);

			foreach (var item in transactionReferences)
			{
				var value = ZString.Empty;
				var reference = dynamicObjects.FirstOrDefault(x => (ZGuid)x["TransactionPK"] == item.TransactionPK);
				if (reference != null)
				{
					value = (ZString)reference[propertyName];
				}

				item.TransactionRef.SetValue(propertyName, value);
			}
		}

		public virtual NettingTransaction LocalCurrency => null;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected DbCommand GetStatementCommand()
		{
			if (StatementType == StatementType.Final)
			{
				var sql = @"
							SELECT 
								TransactionReference,
								OH_Code, 
								TransactionCurrency, 
								TransactionAmount, 
								NettingSystemCurrency, 
								NettingSystemAmount, 
								NettingSystemRate, 
								ParticipantOrgCode, 
								IssuerAmount, 
								IssuerCurrency, 
								IssuerRate, 
								RecipientAmount, 
								RecipientCurrency,
								RecipientRate,
								NRT_PK,
								CompanyCode,
								ParticipantCompanyCode,
								ParticipantCompanyName
							FROM fn_NettingGetTransactionsForStatementFromNettingCalculations(@NettingPeriod)";

				var command = Db.Connection.Command(sql);
				command.AddParameter("@NettingPeriod", SqlDbType.UniqueIdentifier, Period != null ? Period.PK.ToGuid() : Guid.Empty);

				return command;
			}
			else
			{
				var sql = @"
							SELECT 
								TransactionReference, 
								OH_Code, 
								TransactionCurrency, 
								TransactionAmount, 
								NettingSystemCurrency, 
								NettingSystemAmount, 
								NettingSystemRate, 
								ParticipantOrgCode, 
								IssuerAmount, 
								IssuerCurrency, 
								IssuerRate, 
								RecipientAmount, 
								RecipientCurrency, 
								RecipientRate,
								NRT_PK,
								CompanyCode,
								ParticipantCompanyCode,
								ParticipantCompanyName
							FROM fn_NettingGetTransactionsForStatement(@NettingPeriod, @ExchangeRateType, @CurrentDate)";

				var command = Db.Connection.Command(sql);
				command.AddParameter("@NettingPeriod", SqlDbType.UniqueIdentifier, Period != null ? Period.PK.ToGuid() : Guid.Empty);
				command.AddParameter("@ExchangeRateType", SqlDbType.VarChar, ExchangeRateType);
				command.AddParameter("@CurrentDate", SqlDbType.DateTime, ZDateTime.UtcToday.ToDateTime());

				return command;
			}
		}

		class FXOfferRequest
		{
			internal ZString Currency { get; set; }
			internal NettingOrganisation Participant { get; set; }
			internal ZDecimal Amount { get; set; }
		}

		public virtual IEnumerable<NettingTransaction> FXOffers
		{
			get
			{
				if (fxOffers == null)
				{
					fxOffers = new List<NettingTransaction>();

					var offers = Factory.Load<NettingFXOffer>(GetFXOffersOrRequestQuery("OFF")); //<ToDo>: source from enum

					if (offers != null && offers.Any())
					{
						var groupedByCurrency = from fx in offers
												group fx by new { fx.Participant, fx.NFO_RX_NKCurrency } into g
												select new FXOfferRequest() { Currency = g.Key.NFO_RX_NKCurrency, Participant = g.Key.Participant, Amount = g.Sum(x => x.NFO_Value) };

						if (exchangeRates == null || !exchangeRates.Any())
						{
							exchangeRates = NettingHelper.ReadExchangeRatesFromDatabase(Period, ExchangeRateType, new BusinessObjectFactory());
						}

						foreach (var item in groupedByCurrency)
						{
							fxOffers.Add(GetCalculationForFX(item, multiplier: 1));
						}
					}
				}

				return fxOffers;
			}
		}
		List<NettingTransaction> fxOffers;

		public virtual IEnumerable<NettingTransaction> FXRequests
		{
			get
			{
				if (fxRequests == null)
				{
					fxRequests = new List<NettingTransaction>();

					var requests = Factory.Load<NettingFXOffer>(GetFXOffersOrRequestQuery("REQ")); //<ToDo>: source from enum

					if (requests != null && requests.Any())
					{
						var groupedByCurrency = from fx in requests
												group fx by new { fx.Participant, fx.NFO_RX_NKCurrency } into g
												select new FXOfferRequest() { Currency = g.Key.NFO_RX_NKCurrency, Participant = g.Key.Participant, Amount = g.Sum(x => x.NFO_Value) };

						if (exchangeRates == null || !exchangeRates.Any())
						{
							exchangeRates = NettingHelper.ReadExchangeRatesFromDatabase(Period, ExchangeRateType, new BusinessObjectFactory());
						}

						foreach (var item in groupedByCurrency)
						{
							fxRequests.Add(GetCalculationForFX(item, multiplier: -1)); //FX Request is deemed as payable
						}
					}
				}

				return fxRequests;
			}
		}
		List<NettingTransaction> fxRequests;

		NettingTransaction GetCalculationForFX(FXOfferRequest item, int multiplier)
		{
			var nettingSystemExRate = GetExchangeRate(item.Currency);
			var participantCurrency = item.Participant.NSO_RX_NKReportingCurrency;
			var participantExRate = GetExchangeRate(participantCurrency);

			var calculation = new NettingTransaction(this);
			calculation.Amount = item.Amount * multiplier;
			calculation.Currency = item.Currency;

			calculation.NettingCurrency = NettingCurrency;
			calculation.NettingSystemExchangeRate = nettingSystemExRate;
			var nettingSystemAmountWithoutRounding = Env.CurrentCompany.ExchangeRate.ForeignToLocalWithoutRounding(calculation.Amount, nettingSystemExRate);
			calculation.NettingSystemAmount = Utilities.Round(nettingSystemAmountWithoutRounding, GlbCompany.CurrentCompany.LocalCurrency.Decimals);

			if (item.Participant.Organisation != null)
			{
				calculation.OrgCode = item.Participant.Organisation.OH_Code;

				var company = item.Participant.Organisation.CompanyProxies(false)?.FirstOrDefault();
				calculation.CompanyCode = company != null ? company.GC_Code : ZString.Empty;
			}
			calculation.ParticipantCurrency = participantCurrency;
			calculation.ParticipantExchangeRate = participantExRate;
			calculation.ParticipantAmount = Env.CurrentCompany.ExchangeRate.LocalToForeign(nettingSystemAmountWithoutRounding, participantExRate, participantCurrency);

			if (calculation.Amount >= 0)
			{
				calculation.ReceivableAmount = calculation.Amount;
			}
			else
			{
				calculation.PayableAmount = calculation.Amount;
			}
			return calculation;
		}

		protected string ExchangeRateType => IsFinalOrIntermediateStatement ? NettingExchangeRateType.Execution : NettingExchangeRateType.Indicative;

		public bool IsFinalOrIntermediateStatement
		{
			get { return StatementType == StatementType.Final || StatementType == StatementType.Intermediate; }
		}

		ZQuery GetFXOffersOrRequestQuery(ZString type)
		{
			var query = new ZDBOnlyQuery(typeof(NettingFXOffer));
			query.AddToFilter(NettingFXOfferSchema.NFO_NSP_Period, Period != null ? Period.PK.ToGuid() : Guid.Empty);
			query.AddToFilter(NettingFXOfferSchema.NFO_Type, type);
			query.AddToFilter(NettingFXOfferSchema.NFO_ApprovalStatus, "APP"); //<ToDo>: source from enum

			return query;
		}

		public virtual IEnumerable<NettingMovement> GetNettingMovements()
		{
			if (nettingMovements == null)
			{
				nettingMovements = NettingHelper.GetNettingMovements(Factory, Period, IsFinalOrIntermediateStatement);
			}
			return nettingMovements;
		}
		IEnumerable<NettingMovement> nettingMovements;

		public abstract IEnumerable<NettingMovement> GetReceivableNettingMovements();
		public abstract IEnumerable<NettingMovement> GetPayableNettingMovements();

		public IEnumerable<ZString> GetAllParticipants()
		{
			var transactionsForGrouping = GetAllNettingTransactionIncludingOffersAndRequests();

			var groupedByParticipantCompany = from n in transactionsForGrouping.Cast<NettingTransaction>()
											  group n by n.CompanyCode into g
											  select g.Key;

			return groupedByParticipantCompany;
		}

		IEnumerable<NettingTransaction> GetAllNettingTransactionIncludingOffersAndRequests()
		{
			var nettingTransactions = new List<NettingTransaction>();
			nettingTransactions.AddRange(Transactions);
			nettingTransactions.AddRange(FXOffers);
			nettingTransactions.AddRange(FXRequests);

			return nettingTransactions;
		}

		public IEnumerable<ZString> GetCurrenciesWithOutExchangeRate()
		{
			var transactionsIncludingOfferesAndRequests = GetAllNettingTransactionIncludingOffersAndRequests().ToList();

			var transactionCurrenciesThatDoNotHaveExRate = (from n in transactionsIncludingOfferesAndRequests
															where n.NettingSystemExchangeRate == 0
															select n.Currency).Distinct();

			var participantCurrenciesThatDoNotHaveExRate = (from n in transactionsIncludingOfferesAndRequests
															where n.ParticipantExchangeRate == 0
															select n.ParticipantCurrency).Distinct();

			return transactionCurrenciesThatDoNotHaveExRate.Union(participantCurrenciesThatDoNotHaveExRate).Distinct();
		}

		public string GetCurrencyStringWithOutExchangeRate(IEnumerable<ZString> currencies)
		{
			var result = new ZStringBuilder();
			foreach (var currency in currencies)
			{
				result.Append(currency);
			}

			return result.ToStringWithDelimiterBetweenAppends(", ").TrimEnd(',');
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected IEnumerable<NettingMatchedInvoice> GetSplitMatchedInvoices()
		{
			if (matchedInvoices == null)
			{
				var sqlQuery = @"
SELECT 
	Issuer,
	Recipient,
	ARInvoiceReference,
	APInvoiceReference
FROM 
	fn_NettingGetMatchedInvoices(@NettingPeriod)";

				matchedInvoices = new List<NettingMatchedInvoice>();
				using (var cmd = Db.Connection.Command(sqlQuery))
				{
					cmd.AddParameter("@NettingPeriod", SqlDbType.UniqueIdentifier, Period != null ? Period.PK.ToGuid() : Guid.Empty);
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							matchedInvoices.Add(new NettingMatchedInvoice(Factory)
							{
								Issuer = reader.GetString(0),
								Recipient = reader.GetString(1),
								ARInvoiceReference = reader.GetString(2),
								APInvoiceReference = reader.GetString(3)
							});
						}
					}
				}
			}

			return matchedInvoices;
		}
		List<NettingMatchedInvoice> matchedInvoices;
		public virtual IEnumerable<NettingMatchedInvoice> ReceivableMatchedInvoices => new List<NettingMatchedInvoice>();

		public virtual IEnumerable<NettingMatchedInvoice> PayableMatchedInvoices => new List<NettingMatchedInvoice>();

		public virtual IEnumerable<NettingClearingJournal> NettingClearingJournals
		{
			get
			{
				if (nettingClearingJournals == null)
				{
					nettingClearingJournals = NettingHelper.GetAllTransactionsForParticipantJournalCreation(NettingPeriod, Db.Connection, this);
				}
				return nettingClearingJournals;
			}
		}
		IEnumerable<NettingClearingJournal> nettingClearingJournals;

		public virtual DocumentSupporter DocumentSupporter => new NettingStatementDocumentSupporter(this);

		protected Dictionary<ZString, ZDecimal> exchangeRates;

		protected ZDecimal GetExchangeRate(ZString currencyCode)
		{
			return exchangeRates.ContainsKey(currencyCode) ? exchangeRates[currencyCode] : 0;
		}
	}

	public enum StatementType
	{
		Trial,
		Final,
		Intermediate
	}

	public class NettingStatementDocumentSupporter : DocumentSupporter
	{
		public NettingStatementDocumentSupporter(NettingStatement participantStatement)
			: base(participantStatement)
		{ }

		NettingStatement NettingStatement
		{
			get { return (NettingStatement)BusinessObject; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return DocumentWrapperFactory.GenerateGenericWrappers(dataContext, NettingStatement);
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.ParticipantStmnt; }
		}

		public override ZArchitecture.Modules.ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return null;
		}
	}
}
