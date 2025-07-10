
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Netting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers
{
	public class DocNettingStatement : DocBaseWrapper
	{
		protected DocNettingStatement(NettingCentreStatement nettingStatement, BusinessObjectFactory factoryToWrap)
			: base(nettingStatement, factoryToWrap)
		{
			Argument.NotNull(nettingStatement, "NettingStatement");
		}

		protected DocNettingStatement(ParticipantStatement participantStatement, BusinessObjectFactory factoryToWrap)
			: base(participantStatement, factoryToWrap)
		{
			Argument.NotNull(participantStatement, "ParticipantStatement");
			OrgCode = participantStatement.OrgCode;
		}

		public static DocNettingStatement New(NettingCentreStatement nettingStatement, BusinessObjectFactory factoryToWrap)
		{
			return new DocNettingStatement(nettingStatement, factoryToWrap);
		}

		public static DocNettingStatement New(ParticipantStatement nettingStatement, BusinessObjectFactory factoryToWrap)
		{
			return new DocNettingStatement(nettingStatement, factoryToWrap);
		}

		NettingStatement NettingStatement
		{
			get { return (NettingStatement)WrappedObject; }
		}

		public ZString OrgCode { get; private set; }

		public ZString ParticipantCurrency
		{
			get
			{
				var participantStatement = NettingStatement as ParticipantStatement;
				if (participantStatement != null)
				{
					return participantStatement.ReportingCurrency;
				}

				return ZString.Empty;
			}
		}

		public ZString NettingCurrency
		{
			get { return NettingStatement != null ? NettingStatement.NettingCurrency : ZString.Empty; }
		}

		public ZString PeriodReference
		{
			get { return NettingStatement != null ? NettingStatement.Period.NSP_Description : ZString.Empty; }
		}

		public ZString PeriodValueDate
		{
			get { return NettingStatement != null ? NettingStatement.Period.NSP_ValueDate.ToShortDateString() : string.Empty; }
		}

		public DocNettingTransactionCollection ReceivableTransactions
		{
			get
			{
				if (receivableTransactions == null)
				{
					receivableTransactions = DocNettingTransactionCollection.New(Factory);

					foreach (NettingTransaction calculation in NettingStatement.Transactions)
					{
						if (calculation.Ledger == LedgerTypes.AccountsReceivable)
						{
							receivableTransactions.Add(DocNettingTransaction.New(calculation, Factory));
						}
					}
				}

				return receivableTransactions;
			}
		}
		DocNettingTransactionCollection receivableTransactions;

		public DocNettingTransactionCollection PayableTransactions
		{
			get
			{
				if (payableTransactions == null)
				{
					payableTransactions = DocNettingTransactionCollection.New(Factory);

					foreach (NettingTransaction calculation in NettingStatement.Transactions)
					{
						if (calculation.Ledger == LedgerTypes.AccountsPayable)
						{
							payableTransactions.Add(DocNettingTransaction.New(calculation, Factory));
						}
					}
				}

				return payableTransactions;
			}
		}
		DocNettingTransactionCollection payableTransactions;

		public DocNettingTransactionCollection FXOffers
		{
			get
			{
				if (fxOffers == null)
				{
					fxOffers = DocNettingTransactionCollection.New(Factory);

					foreach (NettingTransaction calculation in NettingStatement.FXOffers)
					{
						fxOffers.Add(DocNettingTransaction.New(calculation, Factory));
					}
				}
				return fxOffers;
			}
		}
		DocNettingTransactionCollection fxOffers;

		public DocNettingTransactionCollection FXRequests
		{
			get
			{
				if (fxRequests == null)
				{
					fxRequests = DocNettingTransactionCollection.New(Factory);
					foreach (NettingTransaction calculation in NettingStatement.FXRequests)
					{
						fxRequests.Add(DocNettingTransaction.New(calculation, Factory));
					}
				}

				return fxRequests;
			}
		}
		DocNettingTransactionCollection fxRequests;

		public DocNettingMovementCollection ReceivableNettingMovements
		{
			get
			{
				if (receivableNettingMovements == null)
				{
					receivableNettingMovements = DocNettingMovementCollection.New(Factory);
					foreach (NettingMovement movement in NettingStatement.GetReceivableNettingMovements())
					{
						if (movement.MovementAmount != 0)
						{
							receivableNettingMovements.Add(DocNettingMovement.New(movement, Factory));
						}
					}
				}

				return receivableNettingMovements;
			}
		}
		DocNettingMovementCollection receivableNettingMovements;

		public DocNettingMovementCollection SellMovements
		{
			get
			{
				if (sellMovements == null)
				{
					sellMovements = DocNettingMovementCollection.New(Factory);
					foreach (DocNettingMovement movement in ReceivableNettingMovements)
					{
						sellMovements.Add(movement);
					}
				}

				return sellMovements;
			}
		}
		DocNettingMovementCollection sellMovements;

		public DocNettingMovementCollection PayableNettingMovements
		{
			get
			{
				if (payableNettingMovements == null)
				{
					payableNettingMovements = DocNettingMovementCollection.New(Factory);
					foreach (NettingMovement movement in NettingStatement.GetPayableNettingMovements())
					{
						if (movement.MovementAmount != 0)
						{
							payableNettingMovements.Add(DocNettingMovement.New(movement, Factory));
						}
					}
				}

				return payableNettingMovements;
			}
		}
		DocNettingMovementCollection payableNettingMovements;

		public DocNettingMovementCollection BuyMovements
		{
			get
			{
				if (buyMovements == null)
				{
					buyMovements = DocNettingMovementCollection.New(Factory);
					foreach (DocNettingMovement movement in PayableNettingMovements)
					{
						buyMovements.Add(movement);
					}
				}

				return buyMovements;
			}
		}
		DocNettingMovementCollection buyMovements;

		public DocNettingMovementCollection ClearingCurrencyMovements
		{
			get
			{
				if (clearingCurrencyMovements == null)
				{
					clearingCurrencyMovements = DocNettingMovementCollection.New(Factory);
					foreach (var movement in NettingStatement.GetNettingMovements())
					{
						if (movement.Currency == NettingCurrency)
						{
							clearingCurrencyMovements.Add(DocNettingMovement.New(movement, Factory));
						}
					}
				}

				return clearingCurrencyMovements;
			}
		}
		DocNettingMovementCollection clearingCurrencyMovements;

		public DocNettingCurrencyCollection Currencies
		{
			get
			{
				if (currencies == null)
				{
					currencies = DocNettingCurrencyCollection.New(Factory);
					var calculations = new List<NettingTransaction>();
					calculations.AddRange(NettingStatement.Transactions);

					PopulateCurrencyCollection(calculations, currencies);
				}

				return currencies;
			}
		}
		DocNettingCurrencyCollection currencies;

		public DocNettingCurrencyCollection ExchangeRates
		{
			get
			{
				if (exchangeRates == null)
				{
					exchangeRates = DocNettingCurrencyCollection.New(Factory);
					var calculations = new List<NettingTransaction>();
					calculations.AddRange(NettingStatement.Transactions);
					calculations.AddRange(NettingStatement.FXOffers);
					calculations.AddRange(NettingStatement.FXRequests);
					if (NettingStatement.LocalCurrency != null)
					{
						calculations.Add(NettingStatement.LocalCurrency);
					}

					PopulateCurrencyCollection(calculations, exchangeRates);
				}

				return exchangeRates;
			}
		}
		DocNettingCurrencyCollection exchangeRates;

		void PopulateCurrencyCollection(List<NettingTransaction> calculations, DocNettingCurrencyCollection currencyList)
		{
			var transactionsGroupedByCurrency = from c in calculations
												orderby c.Currency
												group c by new { c.Currency, c.NettingSystemExchangeRate, c.ParticipantCurrency, c.ParticipantExchangeRate } into g
												select new
												{
													g.Key.Currency,
													g.Key.NettingSystemExchangeRate,
													g.Key.ParticipantExchangeRate,
													g.Key.ParticipantCurrency,
													ReceivablesTotal = g.Sum(x => x.ReceivableAmount),
													PayablesTotal = g.Sum(x => x.PayableAmount),
													ParticipantTotal = g.Sum(x => x.ParticipantAmount),
													TransactionCurrencyTotal = g.Sum(x => x.Amount),
													Transactions = g
												};

			var isReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;
			foreach (var item in transactionsGroupedByCurrency)
			{
				var currency = new NettingCurrency(Factory);
				currency.Currency = item.Transactions.First().NettingCurrency;
				currency.TransactionCurrency = item.Currency;
				currency.NettingSystemExchangeRate = item.NettingSystemExchangeRate;
				currency.ParticipantExchangeRate = item.ParticipantExchangeRate != 0 ? item.NettingSystemExchangeRate / item.ParticipantExchangeRate : decimal.Zero;

				var crossExchangeRate = decimal.Zero;
				if (item.Currency == item.ParticipantCurrency)
				{
					crossExchangeRate = 1;
				}
				else if (item.TransactionCurrencyTotal != 0 && item.ParticipantTotal != 0)
				{
					crossExchangeRate = isReciprocal
						? item.ParticipantTotal / item.TransactionCurrencyTotal
						: item.TransactionCurrencyTotal / item.ParticipantTotal;
				}

				currency.CrossExchangeRate = crossExchangeRate;
				currency.ExchangeValue = item.ParticipantTotal;
				currency.ParticipantCurrency = item.ParticipantCurrency;
				currency.ReceivableAmount = item.ReceivablesTotal;
				currency.PayableAmount = item.PayablesTotal;

				var docNettingCurrency = DocNettingCurrency.New(currency, Factory);
				currencyList.Add(docNettingCurrency);
			}
		}

		public DocNettingMatchedInvoiceCollection ReceivableMatchedInvoices
		{
			get
			{
				if (receivableMatchedInvoices == null)
				{
					receivableMatchedInvoices = DocNettingMatchedInvoiceCollection.New(Factory);

					foreach (var invoice in NettingStatement.ReceivableMatchedInvoices)
					{
						receivableMatchedInvoices.Add(DocNettingMatchedInvoice.New(invoice, Factory));
					}
				}

				return receivableMatchedInvoices;
			}
		}
		DocNettingMatchedInvoiceCollection receivableMatchedInvoices;

		public DocNettingMatchedInvoiceCollection PayableMatchedInvoices
		{
			get
			{
				if (payableMatchedInvoices == null)
				{
					payableMatchedInvoices = DocNettingMatchedInvoiceCollection.New(Factory);

					foreach (var invoice in NettingStatement.PayableMatchedInvoices)
					{
						payableMatchedInvoices.Add(DocNettingMatchedInvoice.New(invoice, Factory));
					}
				}

				return payableMatchedInvoices;
			}
		}
		DocNettingMatchedInvoiceCollection payableMatchedInvoices;

		public DocNettingClearingJournalCollection NettingClearingJournals
		{
			get
			{
				if (nettingClearingJournals == null)
				{
					nettingClearingJournals = DocNettingClearingJournalCollection.New(Factory);

					foreach (var clearingJournal in NettingStatement.NettingClearingJournals)
					{
						nettingClearingJournals.Add(DocNettingClearingJournal.New(clearingJournal, Factory));
					}
				}

				return nettingClearingJournals;
			}
		}
		DocNettingClearingJournalCollection nettingClearingJournals;

		public ZDecimal TotalReceivables
		{
			get
			{
				if (totalReceivables == 0)
				{
					foreach (DocNettingTransaction calculation in ReceivableTransactions)
					{
						totalReceivables += calculation.NettingSystemAmount;
					}
				}

				return totalReceivables;
			}
		}
		ZDecimal totalReceivables;

		public ZDecimal TotalReceivablesInParticipantCurrency
		{
			get
			{
				if (totalReceivablesInParticipantCurrency == 0)
				{
					foreach (DocNettingTransaction calculation in ReceivableTransactions)
					{
						totalReceivablesInParticipantCurrency += calculation.ParticipantAmount;
					}
				}

				return totalReceivablesInParticipantCurrency;
			}
		}
		ZDecimal totalReceivablesInParticipantCurrency;

		public ZDecimal TotalPayables
		{
			get
			{
				if (totalPayables == 0)
				{
					foreach (DocNettingTransaction calculation in PayableTransactions)
					{
						totalPayables += calculation.NettingSystemAmount;
					}
				}

				return totalPayables;
			}
		}
		ZDecimal totalPayables;

		public ZDecimal TotalPayablesInParticipantCurrency
		{
			get
			{
				if (totalPayablesInParticipantCurrency == 0)
				{
					foreach (DocNettingTransaction calculation in PayableTransactions)
					{
						totalPayablesInParticipantCurrency += calculation.ParticipantAmount;
					}
				}

				return totalPayablesInParticipantCurrency;
			}
		}
		ZDecimal totalPayablesInParticipantCurrency;

		public ZDecimal TotalFXOffers
		{
			get
			{
				if (totalFXOffers == 0)
				{
					foreach (DocNettingTransaction calculation in FXOffers)
					{
						totalFXOffers += calculation.NettingSystemAmount;
					}
				}

				return totalFXOffers;
			}
		}
		ZDecimal totalFXOffers;

		public ZDecimal TotalFXOffersInParticipantCurrency
		{
			get
			{
				if (totalFXOffersInParticipantCurrency == 0)
				{
					foreach (DocNettingTransaction calculation in FXOffers)
					{
						totalFXOffersInParticipantCurrency += calculation.ParticipantAmount;
					}
				}

				return totalFXOffersInParticipantCurrency;
			}
		}
		ZDecimal totalFXOffersInParticipantCurrency;

		public ZDecimal TotalFXRequests
		{
			get
			{
				if (totalFXRequests == 0)
				{
					foreach (DocNettingTransaction calculation in FXRequests)
					{
						totalFXRequests += calculation.NettingSystemAmount;
					}
				}

				return totalFXRequests;
			}
		}
		ZDecimal totalFXRequests;

		public ZDecimal TotalFXRequestsInParticipantCurrency
		{
			get
			{
				if (totalFXRequestsInParticipantCurrency == 0)
				{
					foreach (DocNettingTransaction calculation in FXRequests)
					{
						totalFXRequestsInParticipantCurrency += calculation.ParticipantAmount;
					}
				}

				return totalFXRequestsInParticipantCurrency;
			}
		}
		ZDecimal totalFXRequestsInParticipantCurrency;

		public ZDecimal NetReceivables
		{
			get { return TotalFXOffers + TotalReceivables; }
		}

		public ZDecimal NetReceivablesInParticipantCurrency
		{
			get { return TotalFXOffersInParticipantCurrency + TotalReceivablesInParticipantCurrency; }
		}

		public ZDecimal NetPayables
		{
			get { return TotalFXRequests + TotalPayables; }
		}

		public ZDecimal NetPayablesInParticipantCurrency
		{
			get { return TotalFXRequestsInParticipantCurrency + TotalPayablesInParticipantCurrency; }
		}

		public ZDecimal NetAmount
		{
			get { return NetReceivables + NetPayables; }
		}

		public ZDecimal NetAmountInParticipantCurrency
		{
			get { return NetReceivablesInParticipantCurrency + NetPayablesInParticipantCurrency; }
		}

		public ZString RecipientNameAddress
		{
			get
			{
				ZStringBuilder stringBuilder = new ZStringBuilder();

				if (OrgHeader != null)
				{
					foreach (OrgAddress orgAddress in OrgHeader.Addresses.OfType<OrgAddress>().Where(x => x.OA_IsActive))
					{
						if (orgAddress.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.Office))
						{
							stringBuilder.AppendLine(OrgHeader.OH_FullName);
							stringBuilder.Append(GetFormattedAddress(orgAddress));
							break;
						}
					}
				}

				return stringBuilder.ToString();
			}
		}

		public ZString NettingType
		{
			get
			{
				ZString result = ZString.Empty;

				if (OrgHeader != null)
				{
					var nettingParticipant = Factory.LoadTop1<NettingOrganisation>(new ZQuery(NettingOrganisationSchema.NSO_OH_Organisation, OrgHeader.PK));
					if (nettingParticipant != null)
					{
						switch (nettingParticipant.NSO_NettingType)
						{
							case "FUL":
								result = "FULL";
								break;
							case "CUR":
								result = "CURRENCY";
								break;
							case "HOM":
								result = "HOME";
								break;
							case "GRS":
								result = "GROSS";
								break;
							default:
								result = "UNDEFINED";
								break;
						}
					}
				}

				return result;
			}
		}

		OrgHeader OrgHeader
		{
			get
			{
				if (!OrgCode.IsEmpty && orgHeader == null)
				{
					orgHeader = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, OrgCode);
				}

				return orgHeader;
			}
		}
		OrgHeader orgHeader;

		public ZString CompanyCode
		{
			get
			{
				if (OrgHeader != null && associatedCompany == null)
				{
					associatedCompany = OrgHeader.CompanyProxies(false)?.FirstOrDefault();
				}

				return associatedCompany != null ? associatedCompany.GC_Code : ZString.Empty;
			}
		}
		GlbCompany associatedCompany;

		ZString GetFormattedAddress(OrgAddress address)
		{
			ZStringBuilder stringBuilder = new ZStringBuilder();
			if (address != null)
			{
				if (address.OA_Address1 != ZString.Empty)
				{
					stringBuilder.AppendLine(address.OA_Address1);
				}

				if (address.OA_Address2 != ZString.Empty)
				{
					stringBuilder.AppendLine(address.OA_Address2);
				}

				if (address.OA_City != ZString.Empty)
				{
					stringBuilder.Append(address.OA_City + " ");
				}

				if (address.OA_State != ZString.Empty)
				{
					stringBuilder.Append(address.OA_State + " ");
				}

				stringBuilder.Append(address.OA_PostCode);
			}

			return stringBuilder.ToString();
		}

		public ZString NettingStartDate { get { return NettingStatement.StartDate.ToShortDateString(); } }
		public ZString NettingEndDate { get { return NettingStatement.EndDate.ToShortDateString(); } }

		public ZString NettingPeriod
		{
			get { return NettingStatement != null ? NettingStatement.Period.NSP_Period : ZString.Empty; }
		}
	}
}
