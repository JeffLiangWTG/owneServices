using System;
using System.Collections.Generic;
using System.Xml.Schema;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Filters;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.eNett_Integration
{
	internal class eNettInboundPaymentDataAdapter : BaseAccountingDataAdapter<ReceiptPaymentBase, TxnHeader>
	{
		class InvoiceGrouper
		{
			public class InvoiceRecord
			{
				public InvoiceRecord(string currencyCode)
				{
					this.CurrencyCode = currencyCode;
				}

				public string CurrencyCode { get; set; }
				public decimal LocalAmount { get; set; }
				public decimal OSAmount { get; set; }
			}

			public void AddInvoiceData(InvoicingBase invoice, TxnHeader header)
			{
				List<ZGuid> linePKs = new List<ZGuid>();

				foreach (InvoicingLineBase line in invoice.Lines)
				{
					linePKs.Add(line.PK);
				}

				ZQuery query = new ZQuery();
				query.AddToFilter(JobChargeSchema.JR_AL_ARLine, linePKs.ToArray());
				BusinessObjectFactory factory = new BusinessObjectFactory();
				JobCharge[] jobCharges = factory.Load<JobCharge>(query);

				foreach (JobCharge charge in jobCharges)
				{
					InvoiceRecord invoiceRecord = FindOrCreateRecord(invoice, charge.JR_RX_NKSellCurrency);
					invoiceRecord.LocalAmount += charge.JR_LocalSellAmt;
					invoiceRecord.OSAmount += charge.JR_OSSellAmt;
				}

				if (!TxnHeadersDictionary.ContainsKey(invoice))
				{
					TxnHeadersDictionary.Add(invoice, header);
				}
			}

			public bool IsStateValid(string paymentCurrencyCode)
			{
				bool result = true;

				if (paymentCurrencyCode != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
				{
					foreach (InvoicingBase invoice in InvoiceRecords.Keys)
					{
						bool isValid = false;

						foreach (InvoiceRecord record in InvoiceRecords[invoice])
						{
							if (record.CurrencyCode == paymentCurrencyCode)
							{
								isValid = true;
								break;
							}
						}

						result &= isValid;

						if (!result)
						{
							break;
						}
					}
				}

				return result;
			}

			public decimal GetOSTotalAmount(string currencyCode)
			{
				decimal result = 0;

				foreach (InvoicingBase invoice in InvoiceRecords.Keys)
				{
					result += GetOSTotalAmount(invoice, currencyCode);
				}

				return result;
			}

			public decimal GetOSTotalAmount(InvoicingBase invoice, string currencyCode)
			{
				decimal result = 0;

				foreach (InvoiceRecord record in InvoiceRecords[invoice])
				{
					if (record.CurrencyCode == currencyCode)
					{
						result += record.OSAmount;
					}
				}

				return result;
			}

			public decimal GetLocalTotalAmount(string currencyCode)
			{
				decimal result = 0;

				foreach (InvoicingBase invoice in InvoiceRecords.Keys)
				{
					foreach (InvoiceRecord record in InvoiceRecords[invoice])
					{
						if (record.CurrencyCode == currencyCode)
						{
							result += record.LocalAmount;
						}
					}
				}

				return result;
			}

			public decimal GetLocalTotalAmount(InvoicingBase invoice, string currencyCode)
			{
				decimal result = 0;

				foreach (InvoiceRecord record in InvoiceRecords[invoice])
				{
					if (record.CurrencyCode == currencyCode)
					{
						result += record.LocalAmount;
					}
				}

				return result;
			}

			#region Implementation

			Dictionary<InvoicingBase, TxnHeader> fTxnHeadersDictionary;

			public Dictionary<InvoicingBase, TxnHeader> TxnHeadersDictionary
			{
				get { return fTxnHeadersDictionary ?? (fTxnHeadersDictionary = new Dictionary<InvoicingBase, TxnHeader>()); }
			}

			Dictionary<InvoicingBase, List<InvoiceRecord>> fInvoiceRecords;

			public Dictionary<InvoicingBase, List<InvoiceRecord>> InvoiceRecords
			{
				get { return fInvoiceRecords ?? (fInvoiceRecords = new Dictionary<InvoicingBase, List<InvoiceRecord>>()); }
			}

			InvoiceRecord FindOrCreateRecord(InvoicingBase invoice, string currencyCode)
			{
				InvoiceRecord result = null;

				if (!InvoiceRecords.ContainsKey(invoice))
				{
					InvoiceRecords.Add(invoice, new List<InvoiceRecord>());
				}

				foreach (InvoiceRecord invoiceRecord in InvoiceRecords[invoice])
				{
					if (invoiceRecord.CurrencyCode == currencyCode)
					{
						result = invoiceRecord;
						break;
					}
				}

				if (result == null)
				{
					result = new InvoiceRecord(currencyCode);
					InvoiceRecords[invoice].Add(result);
				}

				return result;
			}

			#endregion
		}

		#region Overrides

		protected override void ExportToValueObjectCore(ReceiptPaymentBase bizObj, TxnHeader constructedValueObject, IValueObjectExportContext context)
		{
			throw new NotSupportedException();
		}

		protected override void ImportFromValueObjectCore(ReceiptPaymentBase receipt, TxnHeader value, IValueObjectImportContext context)
		{
			BusinessObjectFactory factory = receipt.Factory;
			notificationManager = new NotificationManager(context);
			string errorContext = Res.GetString("292e209b-0ed4-4648-ae9b-cb8a6199b9a2", "Transaction {0} {1} {2}:", value.Ledger.ToString(), value.TxnType.ToString(), value.TxnNumber) + " ";

			TransformXsdForCrossLedgerImport(value);
			ProcessTransaction(receipt, value, context, errorContext);

			InvoiceGrouper grouper = new InvoiceGrouper();

			if (value.PaidTransactions.Count > 0 && !receipt.IsDeleted)
			{
				MatchingBase matching = new ARMatchingBase(factory, receipt);
				bool settlementGroupChanged = false;
				foreach (TxnHeader paidTxnHeader in value.PaidTransactions)
				{
					ZString transactionNumberWithPrefix = paidTxnHeader.TxnNumber;
					ZString prefix = AccountingConfigurationRegistry.Instance.InvoiceTransactionNumberPrefix.Value;
					if (!prefix.IsEmpty && paidTxnHeader.TxnNumber.StartsWith(prefix))
					{
						transactionNumberWithPrefix = transactionNumberWithPrefix.SubstringSafe(prefix.Length);
					}
					ZGuid orgPK = GetOrganisationPKFromInvoiceNumber(transactionNumberWithPrefix);
					if (orgPK.IsValid)
					{
						if (!matching.MatchingFilterBizO.SettlementOrgInfos.ContainsOrgPK(orgPK))
						{
							OrgLedgerFilter filter = matching.MatchingFilterBizO.SettlementOrgInfos.AddNew();
							filter.OrganisationBizO = factory.Load<OrgHeader>(orgPK);
							settlementGroupChanged = true;
						}
					}
				}

				if (settlementGroupChanged)
				{
					matching.ReloadSettlementOrgTransactions();
				}

				bool reloadForEveryTransaction = matching.FoundMoreThanMaxResultRows;

				try
				{
					if (reloadForEveryTransaction)
					{
						matching.MatchingFilterBizO.SetLedgerTransactionTypeFilter(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
					}

					foreach (TxnHeader header in value.PaidTransactions)
					{
						if (reloadForEveryTransaction)
						{
							matching.MatchingFilterBizO.SetAllNumbersFilter(header.TxnNumber);
							matching.ReloadSettlementOrgTransactions();
						}

						IMatching transaction = null;

						foreach (IMatching matchTransaction in matching.UnmatchedTransactions)
						{
							ZString transactionNumberWithPrefix = AccountingConfigurationRegistry.Instance.InvoiceTransactionNumberPrefix.Value + matchTransaction.TransactionNumber;
							if (matchTransaction.TransactionType == TransactionTypes.Invoice &&
								 (transactionNumberWithPrefix == header.TxnNumber ||
								 matchTransaction.TransactionNumber == header.TxnNumber ||
								 matchTransaction.ConsolidatedRef == header.TxnNumber) &&
								 matchTransaction.Ledger == LedgerTypes.AccountsReceivable)
							{
								transaction = matchTransaction;
								break;
							}
						}

						if (transaction != null && transaction is InvoicingBase)
						{
							grouper.AddInvoiceData((InvoicingBase)transaction, header);
						}
						else
						{
							notificationManager.AddWarningToNotifications(Res.GetString("eb141d62-c300-4ddd-8a7e-9afb856756d4", "Matching failed for Invoice {0}.", header.TxnNumber));
						}
					}
				}
				finally
				{
					if (reloadForEveryTransaction)
					{
						matching.MatchingFilterBizO.ClearFilters();
					}
				}

				bool isLocalCurrencyReceipt = receipt.AH_RX_NKTransactionCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

				if (!isLocalCurrencyReceipt && AccountingConfigurationRegistry.Instance.ENettUseInvoiceExchangeRateForForeignCurrencyReceipts.Value)
				{
					receipt.AH_ExchangeRate = CalculateExchangeRate(receipt.AH_RX_NKTransactionCurrency, grouper);
				}
				else if (!isLocalCurrencyReceipt)
				{
					receipt.AH_ExchangeRate = Env.CurrentCompany.ExchangeRate.TodaysRate(receipt.AH_RX_NKTransactionCurrency, ExchangeRateType.Sell);
				}

				if (grouper.IsStateValid(receipt.AH_RX_NKTransactionCurrency))
				{
					decimal totalLocalAmountToPay = 0;
					bool shouldMatch = true;
					bool shouldCreateExchangeDifference = true;

					foreach (InvoicingBase invoice in grouper.InvoiceRecords.Keys)
					{
						bool isLocalCurrencyInvoice = invoice.AH_RX_NKTransactionCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
						TxnHeader header = grouper.TxnHeadersDictionary[invoice];
						decimal amountToPay = Math.Abs(header.AmountPaidThisPayment.Value);
						decimal amountToPayLocal = amountToPay;
						decimal amountToPayOS = 0m;

						if (grouper.InvoiceRecords[invoice].Count == 1) // single currency invoice
						{
							InvoiceGrouper.InvoiceRecord record = grouper.InvoiceRecords[invoice][0];

							if (!isLocalCurrencyReceipt && !isLocalCurrencyInvoice)
							{
								amountToPayOS = amountToPay = Math.Abs(header.AmountPaidThisPayment.Value); //OS
								amountToPayLocal = Env.CurrentCompany.ExchangeRate.ForeignToLocal(amountToPayOS, CalculateExchangeRate(receipt.AH_RX_NKTransactionCurrency, grouper));
							}
							else if (!isLocalCurrencyReceipt && isLocalCurrencyInvoice)
							{
								amountToPayOS = Math.Abs(header.AmountPaidThisPayment.Value);   //OS
								amountToPayLocal = Env.CurrentCompany.ExchangeRate.ForeignToLocal(amountToPayOS, CalculateExchangeRate(receipt.AH_RX_NKTransactionCurrency, grouper));
								amountToPayLocal = Math.Min(amountToPayLocal, Math.Abs(invoice.AH_OutstandingAmount));
								amountToPay = amountToPayLocal;
							}
							/*else if (!isLocalCurrencyReceipt && Receipt.AH_RX_NKTransactionCurrency == record.CurrencyCode)
							{
								if (header.AmountPaidThisPayment.Value != invoice.AH_OSTotal)
								{
									amountToPayOS = Math.Abs(header.AmountPaidThisPayment.Value);
									amountToPayLocal = amountToPay = Math.Abs(Env.CurrentCompany.ExchangeRate.ForeignToLocal(amountToPayOS, CalculateExchangeRate(Receipt.AH_RX_NKTransactionCurrency, grouper)));	//local
								}
							}*/
							else if (isLocalCurrencyReceipt && !isLocalCurrencyInvoice)
							{
								amountToPayLocal = Math.Abs(header.AmountPaidThisPayment.Value);
								amountToPay = Math.Abs(Env.CurrentCompany.ExchangeRate.LocalToForeign(amountToPayLocal, CalculateExchangeRate(invoice.AH_RX_NKTransactionCurrency, grouper), invoice.AH_RX_NKTransactionCurrency)); //OS
								shouldCreateExchangeDifference = false;
							}

							totalLocalAmountToPay += amountToPayLocal;
							MatchTransaction(matching, invoice, amountToPay);
						}
						else // multiple currency invoice
						{
							decimal osAmountOfReceiptCurrencyComponents = grouper.GetOSTotalAmount(receipt.AH_RX_NKTransactionCurrency);

							if (!isLocalCurrencyReceipt)
							{
								if (osAmountOfReceiptCurrencyComponents == 0 || Math.Abs(receipt.AH_OSTotal) > osAmountOfReceiptCurrencyComponents)
								{
									shouldCreateExchangeDifference = false;
									shouldMatch = false;
								}
							}
							else
							{
								if (header.AmountPaidThisPayment.Value > osAmountOfReceiptCurrencyComponents)
								{
									shouldCreateExchangeDifference = false;
									shouldMatch = false;
								}
								if (!isLocalCurrencyInvoice)
								{
									shouldCreateExchangeDifference = false;
								}
							}

							amountToPayOS = header.AmountPaidThisPayment.Value;
							amountToPayLocal = amountToPay = Math.Abs(Env.CurrentCompany.ExchangeRate.ForeignToLocal(amountToPayOS, CalculateExchangeRate(receipt.AH_RX_NKTransactionCurrency, grouper)));

							totalLocalAmountToPay += amountToPayLocal;
							MatchTransaction(matching, invoice, amountToPay);
						}
					}

					if (shouldCreateExchangeDifference && !isLocalCurrencyReceipt)
					{
						decimal localAmountOfReceiptCurrencyComponents = grouper.GetLocalTotalAmount(receipt.AH_RX_NKTransactionCurrency);

						if (receipt.AH_LocalTotalAmount != localAmountOfReceiptCurrencyComponents)
						{
							//CalcedPaymentLocalAmount = PaymentOsAmount / InvoiceTotalOsAmount(USD) * InvoiceTotalLocalAmount(USD)
							//ExchangeDifferenceAmount = PaymentLocalAmount - CalcedPaymentLocalAmount
							ZDecimal actualPaymentLocalAmount = Math.Abs(receipt.AH_OSTotal / grouper.GetOSTotalAmount(receipt.AH_RX_NKTransactionCurrency) * localAmountOfReceiptCurrencyComponents);
							actualPaymentLocalAmount = Math.Min(actualPaymentLocalAmount, totalLocalAmountToPay);
							RefCurrency currency = RefCurrency.LoadFromCurrencyCode(factory, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
							ZDecimal exchangeDifferenceAmount;
							if (currency != null)
							{
								exchangeDifferenceAmount = Enterprise.ZArchitecture.Core.Utilities.Round(receipt.AH_LocalTotalAmount - actualPaymentLocalAmount, currency.Decimals);
							}
							else
							{
								exchangeDifferenceAmount = receipt.AH_LocalTotalAmount - actualPaymentLocalAmount;
							}
							if (exchangeDifferenceAmount != 0)
							{
								ARExchangeDifference exchangeDifference = (ARExchangeDifference)matching.GetMiscellaneousTransaction(TransactionTypes.ExchangeDifference);
								exchangeDifference.BindableInvoiceAmount = exchangeDifferenceAmount;
								matching.MatchedTransactions.Add(exchangeDifference);
							}
						}
					}
					else if (!shouldCreateExchangeDifference && isLocalCurrencyReceipt)
					{
						decimal receiptAmount = Math.Abs(receipt.AH_LocalTotalAmount);
						decimal invoiceAmount = Math.Abs(totalLocalAmountToPay);

						if (receiptAmount > invoiceAmount)
						{
							foreach (IMatching matchTransaction in matching.MatchedTransactions)
							{
								if (matchTransaction == receipt)
								{
									matchTransaction.OSPartialPaymentAmount += Math.Abs(receipt.AH_OSTotalAmount) - invoiceAmount;
									break;
								}
							}
						}
						else if (invoiceAmount > receiptAmount)
						{
							foreach (IMatching matchTransaction in matching.MatchedTransactions)
							{
								InvoicingBase invoice = matchTransaction as InvoicingBase;
								TxnHeader header = grouper.TxnHeadersDictionary[invoice];
								if (invoice != null && header != null)
								{
									if (invoice.AH_LocalTotalAmount != 0)
									{
										decimal percentage = header.AmountPaidThisPayment.Value / invoice.AH_LocalTotalAmount;
										decimal oSdifference = Env.CurrentCompany.ExchangeRate.LocalToForeign((invoiceAmount - receiptAmount) * percentage, CalculateExchangeRate(invoice, invoice.AH_RX_NKTransactionCurrency, grouper), invoice.AH_RX_NKTransactionCurrency);
										matchTransaction.OSPartialPaymentAmount -= oSdifference;
										break;
									}
								}
							}
						}
					}

					if (shouldMatch && matching.MatchedTransactions.Count > 1)
					{
						/*foreach (IMatching matchTransaction in matching.MatchedTransactions)
						{
							if (matchTransaction == Receipt)
							{
								matchTransaction.OSPartialPaymentAmount = -totalAmountToPay;
							}
						}*/

						if (matching.Balance == 0m)
						{
							matching.MatchAndClearTransactions();
						}
						else
						{
							notificationManager.AddWarningToNotifications(Res.GetString("84963005-ee98-46d3-883f-983ee9c2ad04", "Matching failed for Receipt {0}.", value.TxnNumber));
						}
					}
				}
				else
				{
					notificationManager.AddWarningToNotifications(Res.GetString("84963005-ee98-46d3-883f-983ee9c2ad04", "Matching failed for Receipt {0}.", value.TxnNumber));
				}
			}
		}

		ZGuid GetOrganisationPKFromInvoiceNumber(ZString transactionNumber)
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			ZQuery query = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);
			query.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, transactionNumber);
			ARInvoice invoice = newFactory.LoadTop1<ARInvoice>(query);
			return invoice != null ? invoice.AH_OH : ZGuid.Empty;
		}

		public override string RootElementName
		{
			get { return "FinancialInvoice"; }
		}

		public override XmlSchema Schema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.SingleFinancialInvoiceSchema; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.FinancialInvoicesSchema; }
		}

		public override string RootCollectionElementName
		{
			get { return "FinancialTransactions"; }
		}

		#endregion

		#region Implementation

		void MatchTransaction(MatchingBase matching, InvoicingBase invoice, decimal amount)
		{
			matching.MoveFromUnmatchToMatch(new[] { invoice });
			((IMatching)invoice).OSPartialPaymentAmount = amount;
		}

		decimal CalculateExchangeRate(string currencyCode, InvoiceGrouper grouper)
		{
			return Env.CurrentCompany.ExchangeRate.GetRate(grouper.GetLocalTotalAmount(currencyCode), grouper.GetOSTotalAmount(currencyCode));
		}

		decimal CalculateExchangeRate(InvoicingBase invoice, string currencyCode, InvoiceGrouper grouper)
		{
			return Env.CurrentCompany.ExchangeRate.GetRate(grouper.GetLocalTotalAmount(invoice, currencyCode), grouper.GetOSTotalAmount(invoice, currencyCode));
		}

		void TransformXsdForCrossLedgerImport(TxnHeader xmlHeader)
		{
			xmlHeader.Ledger = TxnLedgerType.AR;

			xmlHeader.LocalInvoiceAmtInclTax = TxnHeaderMapper.SwapXmlFinancialValueSign(xmlHeader.LocalInvoiceAmtInclTax);
			xmlHeader.LocalInvoiceAmtExclTax = TxnHeaderMapper.SwapXmlFinancialValueSign(xmlHeader.LocalInvoiceAmtExclTax);
			xmlHeader.LocalTaxAmount = TxnHeaderMapper.SwapXmlFinancialValueSign(xmlHeader.LocalTaxAmount);
			xmlHeader.LocalWHTAmount = TxnHeaderMapper.SwapXmlFinancialValueSign(xmlHeader.LocalWHTAmount);

			xmlHeader.OsInvoiceAmtInclTax = TxnHeaderMapper.SwapXmlFinancialValueSign(xmlHeader.OsInvoiceAmtInclTax);
			xmlHeader.OsInvoiceAmtExclTax = TxnHeaderMapper.SwapXmlFinancialValueSign(xmlHeader.OsInvoiceAmtExclTax);
			xmlHeader.OsTaxAmount = TxnHeaderMapper.SwapXmlFinancialValueSign(xmlHeader.OsTaxAmount);
			xmlHeader.OsWHTAmount = TxnHeaderMapper.SwapXmlFinancialValueSign(xmlHeader.OsWHTAmount);
		}

		void ProcessTransaction(ReceiptPaymentBase transactionHeader, TxnHeader xmlTransactionHeader, IValueObjectImportContext context, string errorContext)
		{
			TransactionBuilderConfig config = new TransactionBuilderConfig();
			config.SetBranch = false;
			config.SetDepartment = false;
			config.SetOrganisation = false;
			config.SetReceiptPaymentType = false;
			TransactionHeaderBuilder builder = new TransactionHeaderBuilder(notificationManager, config);
			builder.SetValuesOnReceiptBusinessObject(transactionHeader, xmlTransactionHeader, context, errorContext);

			transactionHeader.AH_PostDate = ZDateTime.Now;
			transactionHeader.AH_Desc = AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(LedgerTypes.AccountsReceivable + TransactionTypes.Receipt,
								LedgerTypes.AccountsReceivable + " " +
								new CodeDescriptionPairList(OLookUpEditType.TransactionTypes).GetDescriptionFromCode(TransactionTypes.Receipt)); // Standard value "AR RECEIPT"

			builder.RunValidationAndReportErrors(transactionHeader);

			if (notificationManager.ErrorsHaveBeenReported)
			{
				transactionHeader.Delete();
				notificationManager.AddInfoNotification("  " + Res.GetString("52ec4589-fc12-4910-a06a-addd13604c5d", "This transaction has errors and was not imported"));
			}
			else
			{
				notificationManager.AddInfoNotification("  " + Res.GetString("f36298a3-f536-4e67-a725-ce075cb42d50", "Completed Processing Transaction"));
			}
		}

		NotificationManager notificationManager { get; set; }

		#endregion
	}
}
