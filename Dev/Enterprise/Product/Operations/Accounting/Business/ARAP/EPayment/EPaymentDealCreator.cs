using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.EPayment;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using QuoteStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Quote;

namespace Enterprise.Accounting.Business
{
	public static class EPaymentDealCreator
	{
		public enum QuoteAcceptingStatus
		{
			ValidationErrors,
			QuoteAlreadyAccepted,
			QuoteAlreadyAcceptedButQuoteIsInvalid,
			NotAcceptedOrReceivedQuoteFound,
			QuoteInReceivedStatusNeedUserAcceptance,
			UserNotAuthorized,
			NoErrors,
			QuoteRequested
		}

		public static (QuoteAcceptingStatus, EPaymentQuote, string) FindAcceptedQuote(PaymentApprovalBase paymentApproval)
		{
			Argument.NotNull(paymentApproval, nameof(paymentApproval));
			var (quoteAcceptingStatus, userMessage) = RunPreconditionChecks(paymentApproval);
			if (quoteAcceptingStatus == QuoteAcceptingStatus.NoErrors)
			{
				return FindAcceptedQuoteCore(paymentApproval);
			}
			else
			{
				return (quoteAcceptingStatus, null, userMessage);
			}
		}

		public static (QuoteAcceptingStatus, string) ValidateExRate(PaymentApprovalBase paymentApproval)
		{
			Argument.NotNull(paymentApproval, nameof(paymentApproval));
			var bankAccount = paymentApproval.BankAccount;
			if (!CheckUserRegistration(bankAccount))
			{
				var message = Res.GetString("a4da019f-bc30-41a9-b5e9-04035f547406", "Your staff profile is not currently registered in the list of users who can use this Bank Account. To manage users, edit the Bank Account and select “Manage Users”.");
				return (QuoteAcceptingStatus.ValidationErrors, message);
			}
			else if (!CheckUserAuthorization(bankAccount))
			{
				var message = Res.GetString("dd3d4963-c247-479c-acf4-3d6c5feaf312", @"You need to authorize your OFX user account before proceeding.
Please note that the authorization process can take up to several minutes.
Would you like to authorize your account now?");
				return (QuoteAcceptingStatus.UserNotAuthorized, message);
			}
			return (QuoteAcceptingStatus.NoErrors, null);
		}

		public static (bool isDealCreated, string message) TryToCreateEPaymentDeal(PaymentApprovalBase paymentApproval, EPaymentQuote quote, bool saveFactory = true)
		{
			if (paymentApproval == null)
			{
				throw new ArgumentNullException(nameof(paymentApproval));
			}

			if (quote == null)
			{
				throw new ArgumentNullException(nameof(quote));
			}

			var deal = paymentApproval.Factory.New<EPaymentDeal>();
			deal.AED_GC_Company = paymentApproval.AV_GC;
			deal.AED_QU_Quote = quote.PK;
			deal.AED_ProviderCode = quote.QU_ProviderCode;
			if (saveFactory)
			{
				try
				{
					paymentApproval.Factory.Save();
				}
				catch (ZCannotSaveException ex)
				{
					return (false, ex.Message);
				}
			}
			return (true, Res.GetString("1c01b67b-7c83-42fd-bb55-e2246fcc006d", "E-Payment Deal request generated to service provider {0}. Response may take from a few moments up to several minutes to be received.", deal.AED_ProviderCode));
		}

		static (QuoteAcceptingStatus, string) RunPreconditionChecks(PaymentApprovalBase paymentApproval)
		{
			var userMessage = string.Empty;
			if (paymentApproval.ProcessEPaymentSecurityCheckPoint.IsAllowed)
			{
				if (paymentApproval.IsFullyApproved)
				{
					var bankAccountAndRegistrationMessage = CheckBankAccountAndUserRegistration(paymentApproval);
					if (bankAccountAndRegistrationMessage.IsNullOrEmpty())
					{
						if (!CheckUserAuthorization(paymentApproval.BankAccount))
						{
							return (QuoteAcceptingStatus.UserNotAuthorized, Res.GetString("dd3d4963-c247-479c-acf4-3d6c5feaf312", @"You need to authorize your OFX user account before proceeding.
Please note that the authorization process can take up to several minutes.
Would you like to authorize your account now?"));
						}
						var activeDealMessage = CheckActiveDealExists(paymentApproval);
						if (!activeDealMessage.IsNullOrEmpty())
						{
							userMessage = activeDealMessage;
							return (QuoteAcceptingStatus.ValidationErrors, userMessage);
						}
					}
					else
					{
						userMessage = bankAccountAndRegistrationMessage;
						return (QuoteAcceptingStatus.ValidationErrors, userMessage);
					}
				}
				else
				{
					userMessage = Res.GetString("b1c7ec6e-77eb-4d00-9113-f2a80dfe7a5e", "Only payments in Approved status can be submitted for processing.");
					return (QuoteAcceptingStatus.ValidationErrors, userMessage);
				}
			}
			else
			{
				userMessage = paymentApproval.ProcessEPaymentSecurityCheckPoint.ErrorMessageForNotAllowed;
				return (QuoteAcceptingStatus.ValidationErrors, userMessage);
			}
			return (QuoteAcceptingStatus.NoErrors, userMessage);
		}

		static bool CheckUserAuthorization(AccBankAccount bankAccount)
		{
			if (bankAccount.AB_AccountType == AccountTypeCodeDescriptionPairList.Codes.EPA)
			{
				var staffToken = bankAccount.EPaymentStaffTokenCollection.Cast<AccEPaymentStaffToken>().FirstOrDefault(
						t => t.TK_GS_NKStaffCode == Env.CurrentUser.Initials &&
						t.TK_GC == Env.CurrentCompanyPK &&
						t.TK_Status == AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
				var hour = new TimeSpan(1, 0, 0).TotalMilliseconds;
				return staffToken != null && staffToken.TK_ExpiryUtc.IsInTheFutureUtc(hour);
			}
			return true;
		}

		static bool CheckUserRegistration(AccBankAccount bankAccount)
		{
			if (bankAccount.AB_AccountType == AccountTypeCodeDescriptionPairList.Codes.EPA)
			{
				var staffToken = bankAccount.EPaymentStaffTokenCollection.Cast<AccEPaymentStaffToken>().FirstOrDefault(
						t => t.TK_GS_NKStaffCode == Env.CurrentUser.Initials &&
						t.TK_GC == Env.CurrentCompanyPK);
				return staffToken != null;
			}
			return true;
		}

		static string CheckBankAccountAndUserRegistration(PaymentApprovalBase paymentApproval)
		{
			var message = string.Empty;
			if (paymentApproval.BankAccount != null)
			{
				var bankAccount = paymentApproval.BankAccount;
				if (bankAccount.IsEPaymentAccount)
				{
					if (CheckUserRegistration(bankAccount))
					{
						if (paymentApproval.PayeeOrganisation != null)
						{
							var accountDetails = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
							var matchingAccountDetail = accountDetails.GetAccountDetails(EPaymentMethods.EPaymentViaOFX, paymentApproval.AV_RX_NKPaymentCurrency);
							if (matchingAccountDetail != null)
							{
								if (matchingAccountDetail.EPaymentBeneficiary == null)
								{
									message = Res.GetString("0f3bef5d-66c4-4f41-a0ca-834b68d4abfa", "Payee Bank Account is not configured for E-Payment Processing. Please configure the Account Details in A/P tab of the Payee organization.");
								}
							}
							else
							{
								message = Res.GetString("e15a3ebb-7b8b-4df3-b88e-8eebf99efcca", "An AP Bank Account could not be found with currency {0} and payment type EPO for the payee {1}.", paymentApproval.AV_RX_NKPaymentCurrency, paymentApproval.PayeeOrganisation.OH_Code);
							}
						}
						else
						{
							message = Res.GetString("4a3d92ee-cc20-4f7e-b57d-901b71214985", "Payment must have an Organization recorded against it, in order to submit payment for electronic processing.");
						}
					}
					else
					{
						message = Res.GetString("a4da019f-bc30-41a9-b5e9-04035f547406", "Your staff profile is not currently registered in the list of users who can use this Bank Account. To manage users, edit the Bank Account and select “Manage Users”.");
					}
				}
				else
				{
					message = Res.GetString("5a9524dd-9ec9-4b09-9b67-7d0301ee2599", "Payment Bank Account must be an E-Payment Account in order to submit payment for electronic processing.");
				}
			}
			else
			{
				message = Res.GetString("e2076817-0f04-4e16-9067-f53007c85ea9", "Payment must have a Bank Account recorded against it, in order to submit payment for electronic processing.");
			}
			return message;
		}

		static string CheckActiveDealExists(PaymentApprovalBase paymentApproval)
		{
			return paymentApproval.HasActiveDeal ? Res.GetString("c3e9b26e-a01d-4b7c-8074-0ba3941af0ae", "This payment has already been submitted for processing.") : string.Empty;
		}

		static bool CheckPaymentDetailsAreSameAsQuoteDetails(PaymentApprovalBase paymentApproval, EPaymentQuote acceptedQuote)
		{
			if (acceptedQuote.QU_ToAmount != paymentApproval.AV_Amount ||
				acceptedQuote.QU_RX_NKToCurrency != paymentApproval.AV_RX_NKPaymentCurrency)
			{
				return false;
			}

			var fundingCurrency = EPaymentFundingInfoProviderFactory.CreateProvider(paymentApproval).GetFundingCurrency();

			if (acceptedQuote.QU_RX_NKFromCurrency != fundingCurrency)
			{
				return false;
			}

			if (acceptedQuote.QU_RX_NKFromCurrency == paymentApproval.AV_Calc_LocalCurrency &&
				acceptedQuote.QU_FromAmount != paymentApproval.AV_Calc_LocalAmount)
			{
				return false;
			}

			return true;
		}

		static (QuoteAcceptingStatus, EPaymentQuote, string) FindAcceptedQuoteCore(PaymentApprovalBase paymentApproval)
		{
			var quotes = paymentApproval.PaymentQuotes.Cast<EPaymentQuote>();
			var acceptedQuote = quotes.FirstOrDefault(q => q.QU_Status == QuoteStatusCodes.Accepted);
			var receivedQuote = quotes.FirstOrDefault(q => q.QU_Status == QuoteStatusCodes.Received);

			if (acceptedQuote != null)
			{
				if (CheckPaymentDetailsAreSameAsQuoteDetails(paymentApproval, acceptedQuote))
				{
					var userMessage = GetUserMessageForAcceptedQuote(paymentApproval, acceptedQuote);
					return (QuoteAcceptingStatus.QuoteAlreadyAccepted, acceptedQuote, userMessage);
				}
				else
				{
					var userMessage = Res.GetString("f60d64d3-c13e-4bae-a93f-437391dbfc95", "Payment has an accepted E-Quote, but its details don't match the payment. Please review payment details in order to continue.");
					return (QuoteAcceptingStatus.QuoteAlreadyAcceptedButQuoteIsInvalid, null, userMessage);
				}
			}
			else if (receivedQuote != null)
			{
				var userMessage = GetUserMessageForReceivedQuote(paymentApproval, receivedQuote);
				return (QuoteAcceptingStatus.QuoteInReceivedStatusNeedUserAcceptance, receivedQuote, userMessage);
			}
			else
			{
				var userMessage = Res.GetString("d3fabf7c-689f-4ad1-96c0-a7bf9159cc73", "Payment must have an accepted E-Quote in order to continue. Would you like to request an E-Quote now?");
				return (QuoteAcceptingStatus.NotAcceptedOrReceivedQuoteFound, null, userMessage);
			}
		}

		static string GetUserMessageForReceivedQuote(PaymentApprovalBase paymentApproval, EPaymentQuote receivedQuote)
		{
			var availableQuoteMsg = Res.GetString("e5d57f8e-158a-4514-87de-6d53da6c12e1", "Payment must have an accepted E-Quote in order to continue. The following E-Quote is available for this Payment and will be automatically accepted if you proceed to book the deal.");
			return GetUserMessage(paymentApproval, receivedQuote, availableQuoteMsg + "\r\n");
		}

		static string GetUserMessageForAcceptedQuote(PaymentApprovalBase paymentApproval, EPaymentQuote acceptedQuote)
		{
			return GetUserMessage(paymentApproval, acceptedQuote, string.Empty);
		}

		static string GetUserMessage(PaymentApprovalBase paymentApproval, EPaymentQuote acceptedQuote, string availableQuoteMessage)
		{
			return Res.GetString("a4b93ceb-aed9-4ea1-8360-5c0d95834bf6", @"This FX transaction will be executed by a third party provider, {1}. CargoWise provides the messaging and information exchange only.
Review the details of the transaction to ensure they are correct.
{11}
Provider: {1}
Payment To: {0}
OS Amount: {2} {3}
Exchange Rate: {4} ({9})
Funding Currency Amount: {5} {6}
Processing Fee: {7} {8}
Funding Currency Total Cost: {10} {6}

The OS Amount is the amount your recipient will receive* and the Total Cost is the amount you will be required to pay to {1}.
*Occasionally third-party intermediary banks may deduct a fee from your transfer before paying your recipient. This fee may vary, and {1} receives no portion of it.

By clicking “Yes”, this FX transaction with {1} becomes legally binding if accepted by {1}. Would you like to continue?

Please check the status of the transaction after clicking ""Yes"".
If the {1} quote has expired, {1} cannot process the FX transaction and CargoWise will display the status as ""DEC - Provider Declined"".
If you have any issues or questions about the transaction, please contact {1} directly.",
			paymentApproval.OrganisationFullName,
			acceptedQuote.QU_ProviderCode,
			acceptedQuote.QU_ToAmount.Round(acceptedQuote.ToRXDecimals),
			acceptedQuote.QU_RX_NKToCurrency,
			acceptedQuote.QU_ExchangeRate.Round(acceptedQuote.ExchangeRateDecimals),
			acceptedQuote.QU_FromAmount.Round(acceptedQuote.FromRXDecimals),
			acceptedQuote.QU_RX_NKFromCurrency,
			acceptedQuote.QU_FeeAmount.Round(acceptedQuote.FeeRXDecimals),
			acceptedQuote.QU_RX_NKFeeCurrency,
			acceptedQuote.QU_ExchangeRateInverted.Round(acceptedQuote.ExchangeRateDecimals),
			new ZDecimal(acceptedQuote.QU_FromAmount + acceptedQuote.QU_FeeAmount).Round(acceptedQuote.FeeRXDecimals),
			availableQuoteMessage
			);
		}
	}
}
