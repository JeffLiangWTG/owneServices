using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Accounting.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public enum PaymentPostingResult { Successful, Partially, Failed }

	public class CustomsPaymentCreator : ICustomsPaymentCreator
	{
		public ICustomsPaymentCreationResult CreatePayment(ICustomsPaymentDataProvider paymentDataProvider, IEnumerable<IAPPaymentGroup> dataProviders)
		{
			Argument.NotNull(dataProviders, "dataProviders");
			Argument.GreaterThanZero(dataProviders.Count(), "dataProviders.Length");

			foreach (var data in dataProviders)
			{
				Argument.GreaterThanZero(data.InvoiceDataProviders.Count(), Res.GetString("335E53F6-C02A-408B-9248-352734B986E4", "AP Payment Group -> Invoice Data Providers Length"));
			}

			Argument.NotNull(paymentDataProvider, "paymentDataProvider");
			Argument.NotNull(paymentDataProvider.Company, "paymentDataProvider.Company");

			if (paymentDataProvider.Company.PK != GlbCompany.CurrentCompany.PK)
			{
				throw new CustomsInvoiceRaiseException(Res.GetString("D7B34226-55EB-441C-A102-6BF44DEFBE8F", "Auto-Billing is attempted in the company '{0}' for {1} that belongs to the company {2}.", GlbCompany.CurrentCompany.GC_Code, paymentDataProvider.BankAccountUniqueID, paymentDataProvider.Company.GC_Code));
			}

			List<ZString> errors = new List<ZString>();

			ZString enterpriseHyperlinkText = paymentDataProvider.HyperLinkText;
			ZGuid emailGroup = paymentDataProvider.EmailGroupPK;

			BusinessObjectFactory factory = GetBusinessObjectFactory(dataProviders);
			OrgHeader creditor = paymentDataProvider.Creditor;

			ZStringBuilder message = new ZStringBuilder();

			var creditorError = CustomsDSBChargePostValidator.GetCustomsDisbursementCreditorValidationError(creditor == null ? Guid.Empty : creditor.PK.ToGuid(), paymentDataProvider.Company);
			if (!string.IsNullOrEmpty(creditorError))
			{
				errors.Add(creditorError);
			}

			if (creditor != null)
			{
				CheckForCustomsJobsAndItsCompany(dataProviders, errors, creditor);
			}

			var listOfPostingResults = new List<PaymentPostingResult>();
			var finalPostingResult = PaymentPostingResult.Failed;

			if (errors.Count == 0)
			{
				foreach (var data in dataProviders)
				{
					listOfPostingResults.Add(GenerateAPPaymentRecord(factory, data, creditor, paymentDataProvider, errors));
				}

				finalPostingResult = listOfPostingResults.All(x => x == PaymentPostingResult.Successful) ? PaymentPostingResult.Successful :
					(listOfPostingResults.All(x => x == PaymentPostingResult.Failed) ? PaymentPostingResult.Failed : PaymentPostingResult.Partially);
			}

			foreach (ZString error in errors)
			{
				message.Append(error);
			}

			var paymentCreationResult = new CustomsPaymentCreationResult(finalPostingResult, message, enterpriseHyperlinkText, dataProviders.Count() > 1);

			if (paymentDataProvider.SendEmail)
			{
				var emailSender = new CustomsPaymentCreationEmail();
				emailSender.Send(emailGroup, factory, paymentCreationResult, paymentDataProvider.BankAccountUniqueID);
			}
			return paymentCreationResult;
		}

		PaymentPostingResult GenerateAPPaymentRecord(BusinessObjectFactory factory, IAPPaymentGroup data, OrgHeader creditor, ICustomsPaymentDataProvider paymentDataProvider, List<ZString> errors)
		{
			ZString apPaymentNumber = data.APPaymentNumber;
			Argument.NotNullOrEmpty("apInvoiceNumber", apPaymentNumber);

			var result = PaymentPostingResult.Failed;
			if (creditor != null)
			{
				var errorsForPaymentGroup = new List<ZString>();

				if (PaymentAlreadyExists(factory, creditor, apPaymentNumber))
				{
					errorsForPaymentGroup.Add(Res.GetString("cfeafed6-5c81-4622-ba3b-abe5fb937b1f", "Payment {0} already exists", apPaymentNumber));
				}
				else
				{
					var statementNumberText = apPaymentNumber != paymentDataProvider.BankAccountUniqueID ?
								Res.GetString("96D211DE-00EC-46FF-97C0-D9E330B3AFD8", "Daily Statement ") + apPaymentNumber + ":" : "";

					List<DataProviderWithNotifications> invoicesToPay = GetInvoicesToPay(factory, data.InvoiceDataProviders.ToArray(), creditor, statementNumberText);
					errorsForPaymentGroup.AddRange(CheckPreRequisitesForCreatingPayment(paymentDataProvider, invoicesToPay, creditor));

					APInvoice firstAPInvoiceForBranchDepartment = null;
					foreach (DataProviderWithNotifications dataProvider in invoicesToPay)
					{
						errorsForPaymentGroup.AddRange(dataProvider.Errors);
						if (firstAPInvoiceForBranchDepartment == null)
						{
							firstAPInvoiceForBranchDepartment = dataProvider.Invoices.OfType<APInvoice>().FirstOrDefault();
						}
					}

					if (errorsForPaymentGroup.Count == 0)
					{
						APPayment payment = factory.New<APPayment>();
						payment.AH_OH = creditor.PK;
						payment.AH_AB = paymentDataProvider.BankAccountPK;
						payment.AH_Desc = Res.GetString("4f80d206-6224-4599-bd02-c0c7ab662a5d", "Statement: {0}", apPaymentNumber);

						payment.AH_ReceiptType = ReceiptTypes.EFT;
						payment.AH_ChequeOrReference = apPaymentNumber;
						payment.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

						var paymentDate = paymentDataProvider.PaymentDate;
						if (paymentDate.IsValid)
						{
							payment.AH_InvoiceDate = paymentDate;
						}

						if (firstAPInvoiceForBranchDepartment != null)
						{
							payment.AH_GB = firstAPInvoiceForBranchDepartment.AH_GB;
							payment.AH_GE = firstAPInvoiceForBranchDepartment.AH_GE;
						}

						MatchingBase matchingObject = payment.MatchingBaseObject;
						PayInvoices(payment, invoicesToPay, matchingObject);

						if (matchingObject.Balance != 0)
						{
							payment.Delete();
							errorsForPaymentGroup.Add(statementNumberText + Res.GetString("93d97139-2193-4d56-af7b-d893651deacf", "Matching Balance does not balance to zero (Balance: {0})", matchingObject.Balance));
						}
						else if (payment.AH_InvoiceAmount == 0)
						{
							payment.Delete();
							errorsForPaymentGroup.Add(statementNumberText + Res.GetString("01a745f1-6799-49e2-804b-f92ed0b9bfe4", "Nothing to pay. No payment has been created."));
						}
						else
						{
							result = PaymentPostingResult.Successful;
							matchingObject.MatchAndClearTransactions();
						}
					}
				}

				errors.AddRange(errorsForPaymentGroup);
			}
			return result;
		}

		void CheckForCustomsJobsAndItsCompany(IEnumerable<IAPPaymentGroup> dataProviders, List<ZString> errors, OrgHeader creditor)
		{
			foreach (var data in dataProviders)
			{
				foreach (IAccInvoiceDataProvider dataProvider in data.InvoiceDataProviders)
				{
					if (dataProvider.CustomsJob == null)
					{
						errors.Add(Res.GetString("e4af707f-9f25-44a9-b258-db48c39b5ba4", "No customs declaration job exists in the system for {0}.", dataProvider.UniqueNumber));
					}
					else
					{
						if (dataProvider.CustomsJob.CreditorPK != creditor.PK)
						{
							OrgHeader dataProviderCreditor = dataProvider.Factory.Load<OrgHeader>(dataProvider.CustomsJob.CreditorPK);
							errors.Add(Res.GetString("0dd8841c-69a2-4e4e-9e8f-433f3de651c6", "This payment is for this creditor {0}, however this entry, {1} has a different creditor {2}.", creditor.OH_Code, dataProvider.UniqueNumber, dataProviderCreditor != null ? dataProviderCreditor.OH_Code : new ZString(Res.GetString("ccf8da7b-e4a4-4509-adbb-ab5b07cc92c2", "No creditor is specified"))));
						}

						if (dataProvider.CustomsJob.Branch.Company.PK != GlbCompany.CurrentCompany.PK)
						{
							throw new ArgumentException("dataProvider does not belong to the current company.");
						}
					}
				}
			}
		}

		BusinessObjectFactory GetBusinessObjectFactory(IEnumerable<IAPPaymentGroup> dataProviders)
		{
			foreach (var data in dataProviders)
			{
				foreach (IAccInvoiceDataProvider dataProvider in data.InvoiceDataProviders)
				{
					if (dataProvider.Factory != null)
					{
						return dataProvider.Factory;
					}
				}
			}

			throw new ArgumentException("unable to get BusinessObjectFactory from dataProviders");
		}

		List<DataProviderWithNotifications> GetInvoicesToPay(BusinessObjectFactory factory, IAccInvoiceDataProvider[] dataProviders, OrgHeader creditor, ZString referenceText)
		{
			List<DataProviderWithNotifications> result = new List<DataProviderWithNotifications>();
			var invoices = new InvoicingBaseCollection(factory);
			invoices.AddRange(GetAPInvoices(factory, dataProviders, creditor));
			try
			{
				foreach (IAccInvoiceDataProvider dataProvider in dataProviders)
				{
					var apInvoiceNumToMatch = dataProvider.GetAPInvoiceNumberToMatch(factory, creditor.PK);

					var filter = GetAPInvoiceQuery(creditor.PK, apInvoiceNumToMatch);
					var apInvoices = invoices.Find(filter).OfType<InvoicingBase>().ToArray();
					var providerWithNotifications = new DataProviderWithNotifications(dataProvider, apInvoices, referenceText);
					result.Add(providerWithNotifications);
				}
			}
			finally
			{
				invoices.RemoveAll();
			}

			return result;
		}

		IEnumerable<InvoicingBase> GetAPInvoices(BusinessObjectFactory factory, IAccInvoiceDataProvider[] dataProviders, OrgHeader creditor)
		{
			List<ZString> apInvoiceNumbers = new List<ZString>();

			foreach (IAccInvoiceDataProvider dataProvider in dataProviders)
			{
				ZString apInvoiceNumberToFindWith = dataProvider.GetAPInvoiceNumberToMatch(factory, creditor.PK);

				apInvoiceNumbers.Add(apInvoiceNumberToFindWith);
			}

			var result = Array.Empty<InvoicingBase>();

			if (apInvoiceNumbers.Count > 0)
			{
				ZQuery filter = GetAPInvoiceQuery(creditor.PK, apInvoiceNumbers.ToArray());
				result = factory.Load<InvoicingBase>(new InvoicingBaseCollection(factory, filter).CompleteFilter);
			}

			return result;
		}

		ZQuery GetAPInvoiceQuery(ZGuid creditorPK, params ZString[] apInvoiceNumbers)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, new[] { TransactionTypes.Invoice, TransactionTypes.CreditNote });
			filter.AddToFilter(AccTransactionHeaderSchema.AH_OH, creditorPK);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, SQLComparisonOperator.StartsWith, apInvoiceNumbers);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_OutstandingAmount, SQLComparisonOperator.NotEqual, 0);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_FullyPaidDate, null);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, false);
			return filter;
		}

		List<ZString> CheckPreRequisitesForCreatingPayment(ICustomsPaymentDataProvider paymentDataProvider, List<DataProviderWithNotifications> invoicesToPay, OrgHeader creditor)
		{
			List<ZString> errors = new List<ZString>();

			if (creditor == null)
			{
				errors.Add(Res.GetString("4f1cddf4-d82e-49ae-9bdd-471627df8b6f", "There is no Customs Disbursement Creditor defined in the registry: Registry > AutoRating > Charge Codes > Customs > Disbursement Creditor"));
			}
			else
			{
				if (!creditor.OH_IsCreditor)
				{
					errors.Add(Res.GetString("5b592ae9-3b0d-4075-b314-e019f3928709", "The Customs Disbursement Creditor ({0} - {1}) is not ticked as a 'Payables' Organization", creditor.OH_Code, creditor.OH_FullNameTruncated));
				}

				AccBankAccount bankAccount = creditor.Factory.Load<AccBankAccount>(paymentDataProvider.BankAccountPK);
				if (bankAccount == null)
				{
					errors.Add(Res.GetString("672c917e-4515-45b4-a47c-d43fcf9ab774", "There is no bank account against which payment can be made for {0}.", paymentDataProvider.BankAccountUniqueID));
				}
				else if (bankAccount.AB_RX_NKAccountCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
				{
					errors.Add(Res.GetString("24acf7b0-0412-484b-a8e4-52d685da5e3d", "The bank account ({0}) is not in local currency", creditor.CompanyData.APDefaultBankAccount.AB_Code));
				}
			}

			if (invoicesToPay.Count == 0) // Remove ?
			{
				errors.Add(Res.GetString("27ebf134-6a14-45df-ac6f-3e6b51dd359d", "There are no invoices to pay"));
			}

			return errors;
		}

		void PayInvoices(APPayment payment, List<DataProviderWithNotifications> invoicesToPay, MatchingBase matchingObject)
		{
			ZDecimal paymentAmount = 0m;

			foreach (DataProviderWithNotifications dataProvider in invoicesToPay)
			{
				if (dataProvider.Invoices != null && dataProvider.Invoices.Any())
				{
					paymentAmount -= dataProvider.Invoices.Sum(invoice => invoice.OutstandingAmountMatching);
				}
			}

			payment.AH_OSExTaxAmount = paymentAmount;
			payment.OSPartialPaymentAmount = paymentAmount;

			foreach (DataProviderWithNotifications dataProvider in invoicesToPay)
			{
				if (dataProvider.Invoices != null && dataProvider.Invoices.Any())
				{
					matchingObject.MoveFromUnmatchToMatch(dataProvider.Invoices);

					foreach (var invoice in dataProvider.Invoices)
					{
						((IMatching)invoice).OSPartialPaymentAmount = invoice.OutstandingAmountMatching;
					}
				}
			}
		}

		bool PaymentAlreadyExists(BusinessObjectFactory factory, OrgHeader creditor, ZString jobNumber)
		{
			bool result = false;

			if (factory != null && !jobNumber.IsEmpty)
			{
				ZQuery apInvoiceQuery = new ZQuery();
				apInvoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
				apInvoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment);
				apInvoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_OH, creditor.PK);
				apInvoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_ChequeOrReference, jobNumber);
				apInvoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
				apInvoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, false);

				if (factory.LoadTop1<APPayment>(apInvoiceQuery) != null)
				{
					result = true;
				}
			}

			return result;
		}

		#region CustomsPaymentCreationResult

		public class CustomsPaymentCreationResult : ICustomsPaymentCreationResult
		{
			public CustomsPaymentCreationResult(PaymentPostingResult postingResult, ZStringBuilder message, ZString enterpriseHyperlinkText, bool multipleDataProviders)
			{
				this.PostingResult = postingResult;
				this.ErrorMessage = message.ToStringWithNewLineBetweenAppends();
				this.EmailPresentationMessage = message.ToStringWithDelimiterBetweenAppends((NoResString)@"</br></br>");
				this.DataProvidersWithNotifications = new List<DataProviderWithNotifications>();
				this.enterpriseHyperlinkText = enterpriseHyperlinkText;
				this.multipleDataProviders = multipleDataProviders;
			}

			public readonly ZString ErrorMessage;
			public readonly ZString EmailPresentationMessage;
			public readonly PaymentPostingResult PostingResult;
			public readonly List<DataProviderWithNotifications> DataProvidersWithNotifications;
			public readonly ZString enterpriseHyperlinkText;
			readonly bool multipleDataProviders;

			public string NotificationText
			{
				get
				{
					var result = ZString.Empty;

					if (PostingResult == PaymentPostingResult.Successful)
					{
						result = multipleDataProviders ? Res.GetString("B42FCB4F-1AAA-4474-A79C-FF8E2A44796F", "Payment records are created successfully for {0}", enterpriseHyperlinkText) :
							Res.GetString("D3FF2633-5524-406F-9D56-615822D41076", "A payment was created successfully for {0}", enterpriseHyperlinkText);
					}
					else if (PostingResult == PaymentPostingResult.Partially)
					{
						result = Res.GetString("F93F0BC6-7086-4A41-882C-A57291943F32", "Payment records are created for some periodic daily statements in a monthly statement {0}. This is a list of periodic daily statements and their problems.</br>", enterpriseHyperlinkText);
					}
					else if (PostingResult == PaymentPostingResult.Failed)
					{
						if (multipleDataProviders)
						{
							result = Res.GetString("7103CCA1-3553-4A3A-9B5F-288AABEB6551", "No payment records are created for a monthly statement {0}, This is a list of daily statements and their problems.</br>", enterpriseHyperlinkText);
						}
						else
						{
							result = Res.GetString("22066fd9-e0e9-4640-803b-5b6f130b808d", "The following errors were encountered while trying to automatically create a payment for {0}</br>", enterpriseHyperlinkText);
						}
					}
					return result;
				}
			}

			#region ICustomsPaymentCreationResult Members

			IDataProviderWithNotifications[] ICustomsPaymentCreationResult.DataProvidersWithNotifications
			{
				get { return DataProvidersWithNotifications.ToArray(); }
			}

			ZString ICustomsPaymentCreationResult.ErrorMessage
			{
				get { return ErrorMessage; }
			}

			ZBool ICustomsPaymentCreationResult.WasSuccessful
			{
				get { return PostingResult == PaymentPostingResult.Successful || PostingResult == PaymentPostingResult.Partially; }
			}

			#endregion
		}

		#endregion

		#region DataProviderWithNotifications

		public class DataProviderWithNotifications : IDataProviderWithNotifications
		{
			public DataProviderWithNotifications(IAccInvoiceDataProvider dataProvider, InvoicingBase[] apInvoices, ZString referenceNumber)
			{
				this.DataProvider = dataProvider;
				this.Errors = new List<ZString>();
				this.apInvoices = apInvoices;
				CalculateAmounts();
				Validate(referenceNumber);
			}

			void CalculateAmounts()
			{
				CustomsAmount = 0m;
				CustomsTaxAmount = 0m;

				foreach (ICustomsCharges customsCharges in DataProvider.CustomsCharges)
				{
					if (customsCharges.IsActive)
					{
						foreach (CustomsCharge charge in customsCharges.GetCustomsCharges(null))
						{
							if (charge.IsPaidByBroker)
							{
								CustomsAmount += charge.Amount;
								CustomsTaxAmount += charge.GST;
							}
						}
					}
				}

				InvoiceAmount = apInvoices != null ? apInvoices.Sum(invoice => ((ITransactionHeader)invoice).Multiplier * invoice.AH_OSExTaxAmount * -1) : decimal.Zero;
				InvoiceTaxAmount = apInvoices != null ? apInvoices.Sum(invoice => ((ITransactionHeader)invoice).Multiplier * invoice.AH_OSTaxAmount * -1) : decimal.Zero;
			}

			[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
			void Validate(ZString referenceNumber)
			{
				var hasAPInvoices = apInvoices != null && apInvoices.Any();
				if (!hasAPInvoices && CustomsAmount > 0)
				{
					Errors.Add(referenceNumber + Res.GetString("e9a02439-60b5-4a66-8240-1a485a90513d", "Unable to find unpaid AP Invoice {0}", DataProvider.UniqueNumber));
				}

				if (hasAPInvoices)
				{
					var invoicesNotInLocalCurrenty = apInvoices.Where(invoice => invoice.AH_RX_NKTransactionCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency).Select(x => x.AH_TransactionNum).Distinct();
					if (invoicesNotInLocalCurrenty.Any())
					{
						Errors.Add(referenceNumber + Res.GetString("717b50df-8aeb-40a0-b939-2e1d0445c37f", "AP Invoice {0} is not in local currency", string.Join(", ", invoicesNotInLocalCurrenty)));
					}
					else
					{
						var invoiceAlreadyFullyPaid = apInvoices.Where(invoice => invoice.AH_Calc_OSOutstandingAmount.IsEmpty).Select(x => x.AH_TransactionNum).Distinct();
						if (invoiceAlreadyFullyPaid.Any())
						{
							Errors.Add(referenceNumber + Res.GetString("26d9b8a5-9ceb-45be-9b8f-563c992a48dd", "AP Invoice {0} is already paid", string.Join(", ", invoiceAlreadyFullyPaid)));
						}
						else
						{
							var invoiceAlreadyPartiallyPaid = apInvoices.Where(invoice => invoice.AH_OSTotalAmount != invoice.AH_Calc_OSOutstandingAmount).Select(x => x.AH_TransactionNum).Distinct();
							if (invoiceAlreadyPartiallyPaid.Any())
							{
								Errors.Add(referenceNumber + Res.GetString("ab2941b4-1a0e-49cc-9490-11cdf79fdbdb", "AP Invoice {0} is already partially paid", string.Join(", ", invoiceAlreadyPartiallyPaid)));
							}
							else if (InvoiceAmount != CustomsAmount)
							{
								Errors.Add(referenceNumber + Res.GetString("9dcee281-645e-4a67-9bba-159f1944ab8c", "AP Invoice {0} Amount ({1}) is different to Customs Amount ({2})", string.Join(", ", apInvoices.Select(invoice => invoice.AH_TransactionNum).Distinct()), InvoiceAmount, CustomsAmount));
							}
							else if (InvoiceTaxAmount != CustomsTaxAmount)
							{
								Errors.Add(referenceNumber + Res.GetString("9093ac28-5cea-4530-9202-5117091dd88b", "AP Invoice {0} Tax amount ({1}) is different to Customs Tax Amount ({2})", string.Join(", ", apInvoices.Select(invoice => invoice.AH_TransactionNum).Distinct()), InvoiceTaxAmount, CustomsTaxAmount));
							}
						}
					}
				}
			}

			readonly IAccInvoiceDataProvider DataProvider;
			public readonly List<ZString> Errors;
			readonly InvoicingBase[] apInvoices;

			public ZBool HasDiscrepancyForAmount
			{
				get { return InvoiceAmount != CustomsAmount; }
			}

			public ZBool HasDiscrepancyForTaxAmount
			{
				get { return InvoiceTaxAmount != CustomsTaxAmount; }
			}

			public ZDecimal CustomsAmount
			{
				get;
				set;
			}

			public ZDecimal CustomsTaxAmount
			{
				get;
				set;
			}

			public ZDecimal InvoiceAmount
			{
				get;
				set;
			}

			public ZDecimal InvoiceTaxAmount
			{
				get;
				set;
			}

			[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
			public InvoicingBase[] Invoices
			{
				get { return apInvoices; }
			}

			#region IDataProviderWithNotifications Members

			IAccInvoiceDataProvider IDataProviderWithNotifications.DataProvider
			{
				get { return DataProvider; }
			}

			ZString[] IDataProviderWithNotifications.Errors
			{
				get { return Errors.ToArray(); }
			}

			ZString[] IDataProviderWithNotifications.Warnings
			{
				get { return Array.Empty<ZString>(); }
			}

			#endregion
		}

		#endregion
	}
}

#region Test
#if DEBUG

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	class MockAPPaymentGroup : IAPPaymentGroup
	{
		public MockAPPaymentGroup(IAccInvoiceDataProvider[] chargesProviders)
		{
			this.chargesProviders = chargesProviders;
		}

		readonly IAccInvoiceDataProvider[] chargesProviders;

		public ZString APPaymentNumber
		{
			get;
			set;
		}

		public IEnumerable<IAccInvoiceDataProvider> InvoiceDataProviders
		{
			get { return chargesProviders; }
		}
	}
}

#endif
#endregion
