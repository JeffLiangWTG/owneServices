using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.EPayment;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP
{
	//This class is used to keep E-Payment code that is being used by both PaymentApprovalForm and PaymentApprovalWithAuthorisationHelperForm
	public static class PaymentApprovalEPaymentHelper
	{
		#region Create Deal

		internal static bool TryToCreateDeal(PaymentApprovalBase paymentApproval, Action actionToRequestNewQuote)
		{
			bool isDealCreated = false;
			if (!paymentApproval.HasChanges)
			{
				var (status, quote, userMessage) = EPaymentDealCreator.FindAcceptedQuote(paymentApproval);
				switch (status)
				{
					case EPaymentDealCreator.QuoteAcceptingStatus.ValidationErrors:
						Globals.Message.ShowError(userMessage);
						break;
					case EPaymentDealCreator.QuoteAcceptingStatus.QuoteAlreadyAccepted:
						isDealCreated = CreateDeal(paymentApproval, quote, userMessage);
						break;
					case EPaymentDealCreator.QuoteAcceptingStatus.QuoteAlreadyAcceptedButQuoteIsInvalid:
						Globals.Message.ShowError(userMessage);
						break;
					case EPaymentDealCreator.QuoteAcceptingStatus.NotAcceptedOrReceivedQuoteFound:
						RequestNewQuote(userMessage, actionToRequestNewQuote);
						break;
					case EPaymentDealCreator.QuoteAcceptingStatus.QuoteInReceivedStatusNeedUserAcceptance:
						isDealCreated = PromptUserToAcceptQuote(paymentApproval, quote, userMessage);
						break;
					case EPaymentDealCreator.QuoteAcceptingStatus.UserNotAuthorized:
						PromptUserToAuthorize(paymentApproval, userMessage);
						break;
					default:
						throw new DeveloperNotificationException(FormattableString.Invariant($"Unexpected QuoteAcceptingStatus value '{status}' found. Please implement processing logic for it here."));
				}
			}
			else
			{
				SavePaymentApprovalMessage();
			}
			return isDealCreated;
		}

		internal static void PromptUserToAuthorize(PaymentApprovalBase paymentApproval, string message)
		{
			var caption = Res.GetString("24174bc8-663b-49c4-b85b-e62c1978dd46", "Unauthorized staff profile");
			var dialogResult = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (dialogResult == DialogResult.Yes)
			{
				var bankAccount = paymentApproval.BankAccount;
				var staffToken = bankAccount.EPaymentStaffTokenCollection.Cast<AccEPaymentStaffToken>().FirstOrDefault(
							t => t.TK_GS_NKStaffCode == Env.CurrentUser.Initials &&
							t.TK_GC == Env.CurrentCompanyPK);
				if (staffToken != null)
				{
					var authorizeURL = staffToken.PrepareOAuthURL();
					try
					{
						staffToken.ResetToPendingStatus();
						staffToken.Factory.Save();
						WebUrlLauncher.Launch(authorizeURL);
					}
					catch (ZCannotSaveException ex)
					{
						Globals.Message.ShowError(ex.Message);
					}
				}
			}
		}

		static bool PromptUserToAcceptQuote(PaymentApprovalBase paymentApproval, EPaymentQuote quote, string message)
		{
			var dialogResult = Globals.Message.Show(message, Res.GetString("b7a9e967-6401-4542-a241-b3a35bf8403a", "Accept E-Quote for Payment Approval"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (dialogResult == DialogResult.Yes)
			{
				var (isQuoteAccepted, quoteCreationMessage) = EPaymentQuoteAcceptor.AcceptEPaymentQuote(quote, paymentApproval);
				if (isQuoteAccepted)
				{
					Globals.Message.ShowInformation(quoteCreationMessage);
					var (isDealCreated, dealCreationMessage) = EPaymentDealCreator.TryToCreateEPaymentDeal(paymentApproval, quote);
					if (isDealCreated)
					{
						Globals.Message.ShowInformation(dealCreationMessage);
					}
					else
					{
						Globals.Message.ShowError(dealCreationMessage);
					}
					return isDealCreated;
				}
				else
				{
					Globals.Message.ShowError(quoteCreationMessage);
				}
			}
			return false;
		}

		static void RequestNewQuote(string message, Action actionToRequestNewQuote)
		{
			var dialogResult = Globals.Message.Show(message, Res.GetString("1db342f5-5708-42e6-bf5d-a757f767ca79", "Request E-Quote for Payment Approval"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (dialogResult == DialogResult.Yes)
			{
				actionToRequestNewQuote.Invoke();
			}
		}

		static bool CreateDeal(PaymentApprovalBase paymentApproval, EPaymentQuote quote, string message)
		{
			var dialogResult = Globals.Message.Show(message, Res.GetString("ba028142-8484-4c8c-bd9c-7f97e26dfd11", "Create Deal for Payment Approval"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (dialogResult == DialogResult.Yes)
			{
				var (isDealCreated, userMessage) = EPaymentDealCreator.TryToCreateEPaymentDeal(paymentApproval, quote);
				if (isDealCreated)
				{
					Globals.Message.ShowInformation(userMessage);
				}
				else
				{
					Globals.Message.ShowError(userMessage);
				}
				return isDealCreated;
			}
			return false;
		}

		#endregion

		#region Accept Quote

		internal static bool TryToAcceptQuote(PaymentApprovalBase paymentApproval, BusinessObject[] selectedQuotes)
		{
			if (!paymentApproval.HasChanges)
			{
				var selectedQuoteCount = selectedQuotes.Length;
				if (selectedQuoteCount == 0)
				{
					Globals.Message.ShowInformation(Res.GetString("d06e1671-1b6d-4a2f-9bdf-b5ecdad50994", "Please select a Quote."));
				}
				else if (selectedQuoteCount > 1)
				{
					Globals.Message.ShowInformation(Res.GetString("2e3a8ae1-3653-4f41-a947-3b4d595d99dd", "Please select one Quote only."));
				}
				else
				{
					var (isQuoteAccepted, userMessage) = EPaymentQuoteAcceptor.AcceptEPaymentQuote(selectedQuotes[0] as EPaymentQuote, paymentApproval);
					if (isQuoteAccepted)
					{
						Globals.Message.ShowInformation(userMessage);
						return true;
					}
					else
					{
						Globals.Message.ShowError(userMessage);
					}
				}
			}
			else
			{
				SavePaymentApprovalMessage();
			}
			return false;
		}

		#endregion

		internal static void SavePaymentApprovalMessage() => Globals.Message.ShowInformation(Res.GetString("2e7bc081-cfbd-4c07-8b46-b7f143a39b45", "Please save your payment approval first."));

		internal static void ActiveDealErrorMessage() => Globals.Message.ShowError(PaymentApprovalBase.ActiveDealErrorTextForChangingPaymentDetails);

		internal static string GetDisclaimerMessage(ZString paymentProviderCode) => Res.GetString("0d2b11c1-4c6b-40f3-8861-1f893954740a", @"FX quotes are provided by and the FX transaction is executed by {0}, a third party service provider.
CargoWise provides the messaging and information exchange only.", paymentProviderCode);
	}
}
