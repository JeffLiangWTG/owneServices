using System;
using System.Collections;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocAPPayment : DocTransactionHeader, IPayment, IDocPaymentVoucherWithAuthorisationSupport
	{
		#region Construction

		protected DocAPPayment(Payment payment, BusinessObjectFactory factoryToWrap)
			: base(payment, factoryToWrap)
		{
		}

		public static DocAPPayment New(Payment payment, BusinessObjectFactory factoryToWrap)
		{
			DocAPPayment result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(payment, factoryToWrap);
			}
			else if (payment != null)
			{
				result = new DocAPPayment(payment, factoryToWrap);
			}

			return result;
		}

		protected new delegate DocAPPayment NewDelegate(Payment payment, BusinessObjectFactory factoryToWrap);
		protected new static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		protected Payment Payment
		{
			get { return (Payment)WrappedObject; }
		}

		#endregion

		public virtual ZBool ShowTransactionLines
		{
			get
			{
				return (Invoices.Count <= NumberOfTransactionLinesThatCanFitOnThePage);
			}
		}

		#region Remittance Advice, Payment Voucher and Cheque Fields

		protected override ZDecimal GetPaymentVoucherOSTotalCore()
		{
			return OSTotalForRemittanceAdvice;
		}

		protected override ZString GetFirstAuthorisationCore()
		{
			return FirstAuthorisation;
		}

		protected override ZString GetSecondAuthorisationCore()
		{
			return SecondAuthorisation;
		}

		protected override ZString GetThirdAuthorisationCore()
		{
			return ThirdAuthorisation;
		}

		protected override ZString GetFirstAuthorisationDescriptionCore()
		{
			return FirstAuthorisationDescription;
		}

		protected override ZString GetSecondAuthorisationDescriptionCore()
		{
			return SecondAuthorisationDescription;
		}

		protected override ZString GetThirdAuthorisationDescriptionCore()
		{
			return ThirdAuthorisationDescription;
		}

		protected override ZString GetApprovalStatusCore()
		{
			return ApprovalStatus;
		}

		protected override ZString GetPreparedByCore()
		{
			return PreparedBy;
		}

		protected override ZBool GetShowNewAuthorisationFooterCore()
		{
			return ShowNewAuthorisationFooter;
		}

		protected override ZDecimal GetExchangeRateForPaymentVoucherCore()
		{
			return RemittanceExchangeRate;
		}

		protected override ZDecimal GetTotalPaidAsForPaymentVoucherCore()
		{
			return OSTotalForRemittanceAdvice;
		}

		protected override ZDecimal GetTotalPaymentForPaymentVoucherCore()
		{
			return InvoiceAmountForPaymentVoucher;
		}

		protected override ZString GetRemittanceAdviceContactCore()
		{
			return RemittanceAdviceContact;
		}

		protected override ZDecimal GetOSTotalForRemittanceAdviceCore()
		{
			return OSTotalForRemittanceAdvice;
		}

		protected override ZDecimal GetSummaryTotalForRemittanceAdviceCore()
		{
			return OSTotalForRemittanceAdvice;
		}

		protected override ZString GetPaymentCurrencyCodeCore()
		{
			return PaymentCurrency == null ? ZString.Empty : PaymentCurrency.Code;
		}

		protected override ZBool GetShowOriginalAmountForPAYCore()
		{
			return ShowOriginalAmountForPAY;
		}

		protected override ZBool GetShowOriginalAmountCore()
		{
			return ShowOriginalAmount;
		}

		protected override DocGenericTransactionLineCollection GetFlattenedInvoicesCore()
		{
			var result = new DocGenericTransactionLineCollection(Payment.Factory);

			foreach (FlattenedLine line in FlattenedInvoices)
			{
				result.Add(DocGenericTransactionLine.New(line, Factory));
			}

			return result;
		}

		protected override DocGenericTransactionLineCollection GetFlattenedPaymentsCore()
		{
			var result = new DocGenericTransactionLineCollection(Payment.Factory);

			foreach (FlattenedLine line in FlattenedPayments)
			{
				result.Add(DocGenericTransactionLine.New(line, Factory));
			}
			return result;
		}

		protected DocTransactionHeaderCollection fPayments;
		public DocTransactionHeaderCollection Payments
		{
			get
			{
				if (fPayments == null)
				{
					fPayments = new DocTransactionHeaderCollection(Factory);
					foreach (DocTransactionHeader docTransaction in ReceiptMatches)
					{
						if (docTransaction.TransactionPK != TransactionPK)
						{
							fPayments.Add(docTransaction);
						}
					}
				}
				return fPayments;
			}
		}

		public FlattenedLineCollection FlattenedPayments
		{
			get { return flattenedPayments ?? (flattenedPayments = new FlattenedLineCollection(Payments)); }
		}
		FlattenedLineCollection flattenedPayments;

		protected DocTransactionHeaderCollection fInvoices;
		public DocTransactionHeaderCollection Invoices
		{
			get
			{
				if (fInvoices == null)
				{
					fInvoices = new DocTransactionHeaderCollection(Factory);
					foreach (DocTransactionHeader docTransaction in Payments)
					{
						bool isExchangeDifference = docTransaction.TransactionType == ZArchitecture.Core.TransactionTypes.ExchangeDifference;
						bool isSystemGeneratedContra = docTransaction.TransactionType == ZArchitecture.Core.TransactionTypes.Contra && docTransaction.IsTransactionCreatedByMatching;
						bool isSystemGeneratedTransfer = docTransaction.TransactionType == ZArchitecture.Core.TransactionTypes.Transfer && docTransaction.IsTransactionCreatedByMatching;
						bool isSystemGeneratedJournal = docTransaction.TransactionType == ZArchitecture.Core.TransactionTypes.Journal && docTransaction.IsTransactionCreatedByMatching;

						if (!isExchangeDifference && !isSystemGeneratedTransfer && !isSystemGeneratedContra && !isSystemGeneratedJournal)
						{
							fInvoices.Add(docTransaction);
						}
					}

					foreach (DocTransactionHeader docTransaction in fInvoices)
					{
						if (Currency != null && docTransaction.MatchLink != null)
						{
							ApportionAmount(docTransaction, docTransaction.MatchLink.MatchAmount);
						}
					}

					if (fInvoices.Count > 0)
					{
						AdjustApportionedAmountToBalanceRemittanceAdvice();
					}
				}
				return fInvoices;
			}
		}

		public FlattenedLineCollection FlattenedInvoices
		{
			get { return flattenedInvoices ?? (flattenedInvoices = new FlattenedLineCollection(Invoices)); }
		}
		FlattenedLineCollection flattenedInvoices;

		public ZBool ShowOriginalAmountForPAY
		{
			get
			{
				ZBool result = ZBool.False;
				if (Currency != null)
				{
					foreach (DocTransactionHeader invoice in Invoices)
					{
						if (invoice.Currency != null && invoice.Currency.Code != Currency.Code)
						{
							result = ZBool.True;
							break;
						}
					}
				}
				return result;
			}
		}

		public virtual ZString ChequeAmountAsString
		{
			get { return Cheque.ChequeAmount.ToString(2); }
		}

		public virtual ZString RemittanceAdviceDetailsHeadings
		{
			get
			{
				return AlignTextToTheLeft(Res.GetString("c413f5f3-ad59-4896-aa55-7bcf091e3bb9", "DATE"), DateWidth)
					+ new ZString(' ', SpacerWidth)
					+ AlignTextToTheLeft(Res.GetString("97efe63b-0bbe-4cad-82bc-c69ec2894ecf", "CHEQUE"), ChequeWidth)
					+ new ZString(' ', SpacerWidth)
					+ AlignTextToTheLeft(Res.GetString("a56d4a5b-8f37-444a-b61c-9b16e84c09e3", "REFERENCE"), ReferenceWidth)
					+ new ZString(' ', SpacerWidth)
					+ AlignTextToTheLeft(Res.GetString("465d14fd-8698-435e-a768-f99410474037", "DETAILS"), DetailsWidth)
					+ new ZString(' ', SpacerWidth)
					+ AlignTextToTheRight(Res.GetString("fe375c1f-e9ee-4dbb-9716-86805c209d99", "TRANSACTION AMOUNTS"), TransactionAmountsWidth);
			}
		}

		public virtual ZString RemittanceAdviceDetails
		{
			get
			{
				ZString result = ZString.Empty;

				if (Invoices.Count > 0 && Invoices.Count <= NumberOfTransactionLinesThatCanFitOnThePage)
				{
					foreach (DocTransactionHeader item in Invoices)
					{
						result += AlignTextToTheLeft(item.InvoiceDate.ToShortDateString(), DateWidth)
									+ new ZString(' ', SpacerWidth)
									+ AlignTextToTheLeft(ChequeNumber, ChequeWidth)
									+ new ZString(' ', SpacerWidth)
									+ AlignTextToTheLeft(item.TransactionType + " " + item.TransactionNumber, ReferenceWidth)
									+ new ZString(' ', SpacerWidth)
									+ AlignTextToTheLeft(item.Desc, DetailsWidth)
									+ new ZString(' ', SpacerWidth)
									+ AlignTextToTheRight(item.ApportionedAmount.ToString(2), TransactionAmountsWidth - item.Currency.Code.Length - SpacerWidth)
									+ new ZString(' ', SpacerWidth)
									+ item.Currency.Code + System.Environment.NewLine;
					}
					ZInt lengthOfColumn = DateWidth + SpacerWidth + ChequeWidth + SpacerWidth + ReferenceWidth + SpacerWidth + DetailsWidth + SpacerWidth + TransactionAmountsWidth;
					result += AlignTextToTheRight("-------------", lengthOfColumn) + System.Environment.NewLine;

					ZString totalAmountAsString = AlignTextToTheRight(ChequeAmountAsString, TransactionAmountsWidth - Currency.Code.Length - SpacerWidth) + new ZString(' ', SpacerWidth) + Currency.Code;
					result += AlignTextToTheRight(totalAmountAsString, lengthOfColumn);
				}

				return result;
			}
		}

		#region Authorisation Footer

		public ZBool ShowNewAuthorisationFooter
		{
			get { return ApprovalWrapper != null; }
		}

		PaymentApprovalWithAuthorisation Approval
		{
			get
			{
				if (fApproval == null)
				{
					ZQuery approvalQuery = new ZQuery(AccPaymentApprovalSchema.AV_AH, TransactionHeader.PK);
					fApproval = Factory.LoadTop1<PaymentApprovalWithAuthorisation>(approvalQuery);
				}

				return fApproval;
			}
		}

		PaymentApprovalWithAuthorisation fApproval;

		DocPaymentApproval ApprovalWrapper
		{
			get
			{
				if (fApprovalWrapper == null)
				{
					fApprovalWrapper = DocPaymentApproval.New(Approval, Factory);
				}

				return fApprovalWrapper;
			}
		}

		DocPaymentApproval fApprovalWrapper;

		public ZString PreparedBy
		{
			get { return ApprovalWrapper != null ? ApprovalWrapper.PreparedBy : ZString.Empty; }
		}

		public ZString ApprovalStatus
		{
			get { return ApprovalWrapper != null ? ApprovalWrapper.ApprovalStatus : ZString.Empty; }
		}

		public ZString FirstAuthorisationDescription
		{
			get { return ApprovalWrapper != null ? ApprovalWrapper.FirstAuthorisationDescription : ZString.Empty; }
		}

		public ZString SecondAuthorisationDescription
		{
			get { return ApprovalWrapper != null ? ApprovalWrapper.SecondAuthorisationDescription : ZString.Empty; }
		}

		public ZString ThirdAuthorisationDescription
		{
			get { return ApprovalWrapper != null ? ApprovalWrapper.ThirdAuthorisationDescription : ZString.Empty; }
		}

		public ZString FirstAuthorisation
		{
			get { return ApprovalWrapper != null ? ApprovalWrapper.FirstAuthorisation : ZString.Empty; }
		}

		public ZString SecondAuthorisation
		{
			get { return ApprovalWrapper != null ? ApprovalWrapper.SecondAuthorisation : ZString.Empty; }
		}

		public ZString ThirdAuthorisation
		{
			get { return ApprovalWrapper != null ? ApprovalWrapper.ThirdAuthorisation : ZString.Empty; }
		}

		#endregion

		#region Apportion Amount Calculations

		ZDecimal SumOfAllApportionedAmounts;

		ZDecimal SumOfAllTransactionsInPaymentCurrency
		{
			get
			{
				if (!sumOfAllTransactionsInPaymentCurrency.HasValue)
				{
					var result = 0m;
					foreach (DocTransactionHeader invoice in Invoices)
					{
						if (invoice.Currency != null && Currency != null && invoice.MatchLink != null)
						{
							if (invoice.Currency.Code == Currency.Code)
							{
								result += ConvertLocalAmountToPaymentCurrencyAmount(invoice.MatchLink.MatchAmount, invoice.ExchangeRate);
							}
							else
							{
								result += ConvertLocalAmountToPaymentCurrencyAmount(invoice.MatchLink.MatchAmount, ExchangeRate);
							}
						}
					}
					result += OSTotal;
					sumOfAllTransactionsInPaymentCurrency = result;
				}
				return sumOfAllTransactionsInPaymentCurrency.Value;
			}
		}
		ZDecimal? sumOfAllTransactionsInPaymentCurrency;

		ZDecimal SumOfEligibleTransactionsInPaymentCurrency
		{
			get
			{
				if (!sumOfEligibleTransactionsInPaymentCurrency.HasValue)
				{
					var result = 0m;
					foreach (DocTransactionHeader invoice in Invoices)
					{
						if (invoice.MatchLink != null && IsEligibleForApportionAmountAdjustment(invoice))
						{
							result += ConvertLocalAmountToPaymentCurrencyAmount(invoice.MatchLink.MatchAmount, ExchangeRate);
						}
					}
					sumOfEligibleTransactionsInPaymentCurrency = result;
				}
				return sumOfEligibleTransactionsInPaymentCurrency.Value;
			}
		}
		ZDecimal? sumOfEligibleTransactionsInPaymentCurrency;

		ZDecimal ApportionWeight
		{
			get
			{
				if (!apportionWeight.HasValue)
				{
					apportionWeight = (SumOfEligibleTransactionsInPaymentCurrency != 0m) ? (SumOfAllTransactionsInPaymentCurrency / SumOfEligibleTransactionsInPaymentCurrency) : 0m;
				}
				return apportionWeight.Value;
			}
		}
		ZDecimal? apportionWeight;

		void ApportionAmount(DocTransactionHeader invoice, ZDecimal portion)
		{
			ZDecimal apportionAmount = 0m;
			if (invoice.Currency != null)
			{
				var invoiceAmountInPaymentCurrency = (invoice.Currency.Code != Currency.Code) ? ConvertLocalAmountToPaymentCurrencyAmount(portion, ExchangeRate) : invoice.MatchLink.OSMatchAmount;

				if (IsEligibleForApportionAmountAdjustment(invoice))
				{
					apportionAmount = -invoiceAmountInPaymentCurrency * (1m - ApportionWeight);
				}
				else
				{
					apportionAmount = -invoiceAmountInPaymentCurrency;
				}
				apportionAmount = apportionAmount.Round(Currency.Decimals);
			}
			invoice.ApportionedAmount = apportionAmount;
			SumOfAllApportionedAmounts += apportionAmount;
		}

		void AdjustApportionedAmountToBalanceRemittanceAdvice()
		{
			var selectedPaymentAmount = -OSTotal.Round(Currency.Decimals);
			var difference = SumOfAllApportionedAmounts + selectedPaymentAmount; // This must be zero to balance out remittance advice
			if (difference != 0)
			{
				var transactions = Invoices.Cast<DocTransactionHeader>();
				DocTransactionHeader transactionToAdjust = null;
				if (NumberOfDifferentCurrencies > 1)
				{
					//payment currency transaction amounts should not be selected for adjustments
					//select a transaction with currency other than payment currency
					transactionToAdjust = transactions.Where(x => x.Currency != Currency).MaxBy(x => x.ApportionedAmount);
				}
				else
				{
					//all transactions has same currency
					transactionToAdjust = transactions.MaxBy(x => x.ApportionedAmount);
				}
				transactionToAdjust.ApportionedAmount -= difference;
			}
		}

		ZDecimal ConvertLocalAmountToPaymentCurrencyAmount(ZDecimal localAmount, ZDecimal exchangeRate)
		{
			var foreignCurrencyEquivalent = 0m;
			var oSCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Currency.Code);
			if (oSCurrency != null)
			{
				foreignCurrencyEquivalent = Env.CurrentCompany.ExchangeRate.LocalToForeignWithoutRounding(localAmount, exchangeRate, oSCurrency.RX_Code);
			}
			return foreignCurrencyEquivalent;
		}

		bool IsEligibleForApportionAmountAdjustment(DocTransactionHeader invoice)
		{
			return invoice.Currency.Code != Currency.Code && invoice.TransactionType != TransactionTypes.Payment;
		}

		#endregion

		#region Text Alignment

		protected ZString AlignTextToTheLeft(ZString text, ZInt width)
		{
			return (text.Length > width) ? text.Left(width) : text.PadRight(width, ' ');
		}

		protected ZString AlignTextToTheRight(ZString text, ZInt width)
		{
			return (text.Length > width) ? text.Left(width) : text.PadLeft(width, ' ');
		}

		#endregion

		#region IPayment Members

		public ZDecimal OSTotalForRemittanceAdvice
		{
			get { return OSTotal; }
		}

		public ZDecimal InvoiceAmountForPaymentVoucher
		{
			get { return InvoiceAmount; }
		}

		public ZDecimal RemittanceExchangeRate
		{
			get { return ExchangeRate; }
		}

		public ZInt NumberOfDifferentCurrencies
		{
			get
			{
				ZInt result = 0;
				ArrayList currencyTable = new ArrayList();
				foreach (DocTransactionHeader invoice in Invoices)
				{
					if (invoice.Currency != null)
					{
						if (!currencyTable.Contains(invoice.Currency.Code))
						{
							result++;
							currencyTable.Add(invoice.Currency.Code);
						}
					}
				}
				return result;
			}
		}

		public ZInt NumberOfTotalsDisplayedOnRemittance
		{
			get
			{
				ZInt result = 0;
				ArrayList totalsTable = new ArrayList();
				foreach (DocTransactionHeader invoice in Invoices)
				{
					if (!totalsTable.Contains(invoice.CurrencyAndOrganisation + invoice.Ledger))
					{
						result++;
						totalsTable.Add(invoice.CurrencyAndOrganisation + invoice.Ledger);
					}
				}

				return result;
			}
		}

		public ZString RemittanceAdviceContact
		{
			get
			{
				ZString result = ZString.Empty;

				if (Payment.Header != null)
				{
					OrgContact contact = new DefaultContactFinder(Payment.Header, true).DefaultContact(ContactType.Payables);

					if (contact != null)
					{
						result = DocContacts.New(contact, Factory).ContactName.ToUpper() + System.Environment.NewLine;
					}

					result += ChequePayToWithAddress;
				}

				return result;
			}
		}

		public ZString ChequePayToWithAddress
		{
			get
			{
				ZString result = ZString.Empty;

				if (Organisation != null && Organisation.Addresses.Count > 0)
				{
					foreach (DocAddress docAddress in Organisation.Addresses)
					{
						if (docAddress.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.Payables) && docAddress.Country != null && docAddress.Country.Code == GlbCompany.CurrentCompany.Country.Code)
						{
							result = docAddress.PostalAddressExcludeCountryIfSame;
							break;
						}
					}

					if (result.IsEmpty)
					{
						foreach (DocAddress docAddress in Organisation.Addresses)
						{
							if (docAddress.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.Payables))
							{
								result = docAddress.PostalAddressExcludeCountryIfSame;
								break;
							}
						}
					}

					if (result.IsEmpty)
					{
						result = Organisation.PostalAddressExcludeCountryIfSame;
					}
				}

				return result;
			}
		}

		public ZBool IsReversal
		{
			get { return (IsCancelled && MatchLinks.TotalMatchAmount < 0); }
		}

		public ZBool ShowOriginalAmount
		{
			get { return ShowOriginalAmountForPAY; }
		}

		public ZDecimal NumberOfTransactionsAndTotalsOnCheque
		{
			get
			{
				return Invoices.Count + NumberOfTotalAndSpacerLines;
			}
		}

		public ZBool PrintRemittanceOnCheque
		{
			get
			{
				ZBool result = ZBool.True;
				if (!IsReversal)
				{
					// Count number of transactions plus the total lines in template and the group by after them
					if (Invoices.Count > NumberOfTransactionLines ||
							(Invoices.Count + NumberOfTotalAndSpacerLines) > NumberOfTransactionLines)
					{
						result = ZBool.False;
					}
				}
				return result;
			}
		}

		double NumberOfTotalAndSpacerLines
		{
			// For each total Line (rowheight: 15px) there is a spacer (rowheight: 5px) that is 30% of a normal line size
			// Therefore Total Lines (1) + Spacer Lines for each Total (0.3) gives you a multiplication factor of 1.3
			get { return Math.Ceiling(NumberOfTotalsDisplayedOnRemittance * 1.3); }
		}

		public DocCurrency PaymentCurrency
		{
			get { return Currency; }
		}

		protected DocCheque fCheque;
		public DocCheque Cheque
		{
			get
			{
				if (fCheque == null)
				{
					fCheque = new DocCheque(ChequePayTo, OSTotalForRemittanceAdvice, PaymentCurrency, FirstLinePaymentWidth);
				}
				return fCheque;
			}
		}

		public ZString ChequePayTo
		{
			get { return Organisation != null ? Organisation.Name : ZString.Empty; }
		}

		public ZString PostDateSplit
		{
			get
			{
				ZString result = "";

				ZString date = PostDate.ToString("dd");
				ZString month = PostDate.ToString("MM");
				ZString year = PostDate.ToString("yy");

				result += date[0] + "   " + date[1] + "   ";
				result += month[0] + "   " + month[1] + "   ";
				result += year[0] + "   " + year[1];

				return result;
			}
		}

		public DocTransactionHeaderCollection FirstPageInvoices
		{
			get
			{
				if (fFirstPageInvoices == null)
				{
					fFirstPageInvoices = new DocTransactionHeaderCollection(Factory);
					ZInt count = 1;
					foreach (DocTransactionHeader transaction in Invoices)
					{
						bool isSystemGeneratedJournal = transaction.TransactionType == ZArchitecture.Core.TransactionTypes.Journal && transaction.IsTransactionCreatedByMatching;

						if ((count + NumberOfTotalAndSpacerLines <= NumberOfTransactionLines) && !isSystemGeneratedJournal)
						{
							fFirstPageInvoices.Add(transaction);
							count++;
						}
					}
				}
				return fFirstPageInvoices;
			}
		}

		public ZInt FirstPageInvoicesRowCount
		{
			get { return FirstPageInvoices.Count; }
		}

		ZString fPaymentTransactionSummary;
		public ZString PaymentTransactionSummary
		{
			get
			{
				BuildPaymentTransactionSummary();
				return fPaymentTransactionSummary;
			}
		}

		const uint MaxDisplayablePaymentTransactions = 10;
		const uint MaxWidthForDisplayedTransactionLines = 38;
		static string RemittanceReferral
		{
			get { return Res.GetString("1b5e7e23-771d-4142-b7dc-ce8c8c191a0e", "PLEASE REFER TO REMITTANCE ADVICE FOR DETAILED LIST OF TRANSACTIONS"); }
		}
		void BuildPaymentTransactionSummary()
		{
			fPaymentTransactionSummary = "";

			if (Payments.Count > MaxDisplayablePaymentTransactions)
			{
				fPaymentTransactionSummary = RemittanceReferral;
			}
			else if (Payments.Count > 0)
			{
				string paddedTransactionNumber;
				string paddedAmount = string.Empty;
				int longestTransactionNumber = 0;
				int longestAmount = 0;

				foreach (DocTransactionHeader item in Payments)
				{
					longestTransactionNumber = Math.Max(longestTransactionNumber, item.TransactionNumber.Length);
					if (item.MatchLink != null)
					{
						longestAmount = Math.Max(longestAmount, item.MatchLink.InvertedOSAmount.ToString("N2").Length);
					}
				}

				if (longestTransactionNumber + longestAmount + 9 > MaxWidthForDisplayedTransactionLines)
				{
					fPaymentTransactionSummary = RemittanceReferral;
				}
				else
				{
					foreach (DocTransactionHeader item in Payments)
					{
						paddedTransactionNumber = item.TransactionNumber.PadRight(longestTransactionNumber, ' ');
						if (item.MatchLink != null)
						{
							paddedAmount = item.MatchLink.InvertedOSAmount.ToString("N2").PadLeft(longestAmount, ' ');
						}

						fPaymentTransactionSummary += paddedTransactionNumber + "   " + paddedAmount + "   " + item.Currency.Code + System.Environment.NewLine;
					}
				}
			}
		}

		DocTransactionHeaderCollection fFirstPageInvoices;

		public ZString MICRNumber
		{
			get
			{
				return BuildMICRNumber(Payment.AH_ChequeOrReference, RoutingTransitNumber, BankAccount);
			}
		}

		#endregion

		#endregion

		#region IDocPaymentVoucher Members

		DocumentWrapperCollection IDocPaymentVoucherWithAuthorisationSupport.Payments
		{
			get { return Payments; }
		}

		#endregion

		#region Template Constants

		protected ZInt DateWidth
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.DateWidth, 10); }
		}

		protected ZInt ChequeWidth
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.ChequeWidth, 10); }
		}

		protected ZInt ReferenceWidth
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.ReferenceWidth, 15); }
		}

		protected ZInt DetailsWidth
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.DetailsWidth, 20); }
		}

		protected ZInt TransactionAmountsWidth
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.TransactionAmountsWidth, 20); }
		}

		protected ZInt SpacerWidth
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.SpacerWidth, 1); }
		}

		protected ZInt NumberOfTransactionLinesThatCanFitOnThePage
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfTransactionLinesThatCanFitOnThePage, 22); }
		}

		#endregion
	}
}
