using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Accounting.Business.AccountingConstants;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public partial class MatchingValidation : AccTransactionHeaderValidation
	{
		public MatchingValidation(TransactionHeader parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		readonly new TransactionHeader Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateOSPartialPaymentAmount();
			ValidateOSOutstandingAmount();
			ValidateMatchStatus();
			ValidateMatchStatusReasonCode();
		}

		public void ValidateOSPartialPaymentAmount()
		{
			if (Parent is IMatching)
			{
				ValidateCalculatedProperty(((IMatching)Parent).OSPartialPaymentAmountInfo);
			}
		}

		public void ValidateOSOutstandingAmount()
		{
			if (Parent is IMatching)
			{
				ValidateCalculatedProperty(((IMatching)Parent).OSOutstandingAmountInfo);
			}
		}

		public void ValidateMatchStatus()
		{
			if (Parent is IMatching)
			{
				ValidateCalculatedProperty(((IMatching)Parent).MatchStatusInfo);
			}
		}

		public void ValidateMatchStatusReasonCode()
		{
			if (Parent is IMatching)
			{
				ValidateCalculatedProperty(((IMatching)Parent).MatchStatusReasonCodeInfo);
			}
		}

		protected virtual void CheckOSOutstandingAmount()
		{
			IMatching thisIMatching = Parent as IMatching;

			if (thisIMatching != null)
			{
				if (thisIMatching.OSOutstandingAmount.IsEmpty && IsUnpostedUnapprovedItemsExist())
				{
					thisIMatching.OSOutstandingAmountInfo.AddError(GetErrorMessageForOSOutstandingAmount());
				}

				if ((Parent is ARInvoice || Parent is ARCreditNote || Parent is ARAdjustmentNote) && Parent.IsUsedByActiveCollectionOrderLine)
				{
					thisIMatching.OSOutstandingAmountInfo.AddError(Res.GetString("55b51c76-c32c-4dc8-98ad-93380f45819a", "This transaction has been attached to an active collection batch, thus it can not be matched here."));
				}
			}
		}

		ZBool IsUnpostedUnapprovedItemsExist()
		{
			foreach (PaymentApprovalItem item in Parent.ExistingPaymentApprovalItems)
			{
				if (item.IsInDatabase && item.Approval != null &&
							item.Approval.IsInDatabase && !item.Approval.IsPosted)
				{
					return true;
				}
			}
			return false;
		}

		ZString GetErrorMessageForOSOutstandingAmount()
		{
			ZString errorMessage = Res.GetString("daa90fa2-c65b-4d3f-b1e5-6a1f56e7611e", "This transaction is fully paid by the following Unapproved payment(s):\r\nPay. Date   Bank Account   Check Book   Check/Reference   Amount");
			foreach (PaymentApprovalItem item in Parent.ExistingPaymentApprovalItems)
			{
				if (item.IsInDatabase && item.Approval != null &&
							item.Approval.IsInDatabase && !item.Approval.IsPosted)
				{
					string checkBook = item.Approval.ChequeBook != null ? item.Approval.ChequeBook.AK_Code.ToString() : "     " + Res.GetString("54f2e506-02ce-4b6e-bd7f-7f6e69eda2ee", "N/A") + "         ";
					string checkNumber = !item.Approval.AV_ChequeOrReference.IsEmpty ? item.Approval.AV_ChequeOrReference.ToString() : Res.GetString("54f2e506-02ce-4b6e-bd7f-7f6e69eda2ee", "N/A") + "     ";
					errorMessage += System.Environment.NewLine + item.Approval.AV_PaymentDate.ToShortDateString() + "  " + item.Approval.BankAccount.AB_Code + "          " + checkBook + "       "
						+ checkNumber + "     " + item.OSAmountPaidThisRun + " " + Parent.AH_RX_NKTransactionCurrency + System.Environment.NewLine;
				}
			}
			return errorMessage.TrimEnd('\r', '\n');
		}

		protected virtual void CheckOSPartialPaymentAmount()
		{
			var thisIMatching = Parent as IMatching;

			if (thisIMatching != null)
			{
				var parentAsISupportMatchingOfMyLines = Parent as ISupportMatchingOfMyLines;

				if (parentAsISupportMatchingOfMyLines != null)
				{
					if (parentAsISupportMatchingOfMyLines.IsPaidAmountApportionedToLines
						&& thisIMatching.OSOutstandingAmount != thisIMatching.OSPartialPaymentAmount
						&& !parentAsISupportMatchingOfMyLines.LineTotalPaidAmountPosted.IsEmpty)
					{
						thisIMatching.OSPartialPaymentAmountInfo.AddError(Res.GetString("166192f8-1c42-4673-a51b-be0ee9b9c229", @"This transaction has previously been partially paid on the line level.
You can only fully pay the transaction without matching lines now.
Alternatively, use the 'Match Transaction Lines' option if you wish to partially pay this transaction's lines again."));
					}
					else if (!thisIMatching.OSPartialPaymentAmountInfo.HasErrors()
							 && parentAsISupportMatchingOfMyLines.LineTotalPaidAmount.IsEmpty
							 && parentAsISupportMatchingOfMyLines.LineTotalPaidAmountPosted.IsEmpty
							 && Math.Abs(thisIMatching.OSOutstandingAmount) > Math.Abs(thisIMatching.OSPartialPaymentAmount))
					{
						thisIMatching.OSPartialPaymentAmountInfo.AddWarning(Res.GetString("294c2a66-0ff9-4a4b-8688-3109e846ec5c", @"You are part paying this transaction without matching lines. 
You will not be able to match this transaction at the line level in the future if you proceed.
Alternatively, use the 'Match Transaction Lines' option by right clicking now to allow matching at the line level in the future."));
					}
					else if (!thisIMatching.OSPartialPaymentAmountInfo.HasErrors() && !parentAsISupportMatchingOfMyLines.LineTotalPaidAmount.IsEmpty && parentAsISupportMatchingOfMyLines.LineTotalPaidAmountPosted.IsEmpty)
					{
						thisIMatching.OSPartialPaymentAmountInfo.AddWarning(Res.GetString("d7cdc3bc-fb97-4909-97f4-5683629402ad", "Pay amount was set on the by the line basis. To edit, right click and select 'Pay Lines'."));
					}
				}

				if (!thisIMatching.OSPartialPaymentAmountInfo.HasErrors() && thisIMatching.OSOutstandingAmount != 0
					&& (Math.Abs(thisIMatching.OSOutstandingAmount) < Math.Abs(thisIMatching.OSPartialPaymentAmount) || Math.Sign(thisIMatching.OSOutstandingAmount) != Math.Sign(thisIMatching.OSPartialPaymentAmount)))
				{
					if (thisIMatching.OSOutstandingAmount > 0)
					{
						thisIMatching.OSPartialPaymentAmountInfo.AddError(Res.GetString("539e6bf2-e381-45aa-8c48-ca697049b758", "Pay Amount must be between 0 and {0}", thisIMatching.OSOutstandingAmount.ToString(Parent.TransactionCurrency.Decimals)));
					}
					else
					{
						thisIMatching.OSPartialPaymentAmountInfo.AddError(Res.GetString("f8c4be11-e04e-4d1e-8b77-88a17bfc60ff", "Pay Amount must be between {0} and 0", thisIMatching.OSOutstandingAmount.ToString(Parent.TransactionCurrency.Decimals)));
					}
				}

				var parentAsInvoicingBase = Parent as InvoicingBase;
				if (!thisIMatching.OSPartialPaymentAmountInfo.HasErrors() && parentAsInvoicingBase != null)
				{
					var warningMessage = DynamicTransactionCreatorClearingJournal.IsInvoiceCanBePartlyPaid(parentAsInvoicingBase);
					if (!string.IsNullOrEmpty(warningMessage))
					{
						thisIMatching.OSPartialPaymentAmountInfo.AddWarning(warningMessage);
					}

					if (IsMatchingForeignCurrencyENettPayment)
					{
						var oSPartialPaymentAmount = ZDecimal.Zero;

						if (IsPaymentCurrencyEqualToInvoiceCurrency)
						{
							oSPartialPaymentAmount = Math.Abs(thisIMatching.OSPartialPaymentAmount);
						}
						else
						{
							var weightedInvoiceExchangeRate = InvoiceContainsMatchedLinesInPaymentCurrency ?
								parentAsInvoicingBase.GetWeightedExchangeRateOfMatchedLines(Parent.PaymentApprovalCurrentlyBeingMatched.AV_RX_NKPaymentCurrency) :
								parentAsInvoicingBase.GetWeightedExchangeRate(Parent.PaymentApprovalCurrentlyBeingMatched.AV_RX_NKPaymentCurrency);
							oSPartialPaymentAmount = Env.CurrentCompany.ExchangeRate.LocalToForeign(thisIMatching.OSPartialPaymentAmount, weightedInvoiceExchangeRate, Parent.PaymentApprovalCurrentlyBeingMatched.AV_RX_NKPaymentCurrency);
						}
						if ((Math.Abs(parentAsInvoicingBase.GetAmountOfLinesWithSpecificCurrency(Parent.PaymentApprovalCurrentlyBeingMatched.AV_RX_NKPaymentCurrency)) < Math.Abs(oSPartialPaymentAmount))
							|| (parentAsInvoicingBase.GetAmountOfLinesWithSpecificCurrency(Parent.PaymentApprovalCurrentlyBeingMatched.AV_RX_NKPaymentCurrency) == 0m))
						{
							thisIMatching.OSPartialPaymentAmountInfo.AddError(Res.GetString("128131ed-cf62-4040-9d93-0708ded96372", @"When matching a ComPay payment, you can only pay an invoice up to a total of its lines that match the payment currency."));
						}
					}
				}
				if (!thisIMatching.OSPartialPaymentAmountInfo.HasErrors())
				{
					var actualOutstandingAmount = Parent.AH_OutstandingAmount;
					ZDecimal amountMatchedToUnpostedPayments = actualOutstandingAmount - thisIMatching.OutstandingAmount;
					if (amountMatchedToUnpostedPayments != 0M)
					{
						var unpostedMatchedPaymentApprovalItems = new List<PaymentApprovalItem>();
						foreach (PaymentApprovalItem item in Parent.ExistingPaymentApprovalItems)
						{
							if (Parent.IsPaymentApprovalItemNotPostedAndNotCurrentlyMatched(item))
							{
								unpostedMatchedPaymentApprovalItems.Add(item);
							}
						}
						if (unpostedMatchedPaymentApprovalItems.Count > 0)
						{
							var warningMessage = new StringBuilder();
							var roundedActualOutstandingAmount = actualOutstandingAmount.ToString(GlbCompany.CurrentCompany.LocalCurrency.Decimals);
							var roundedAmountMatchedToUnpostedPayments = amountMatchedToUnpostedPayments.ToString(GlbCompany.CurrentCompany.LocalCurrency.Decimals);
							var roundedOutstandingAmount = thisIMatching.OutstandingAmount.ToString(GlbCompany.CurrentCompany.LocalCurrency.Decimals);
							warningMessage.AppendLine(unpostedMatchedPaymentApprovalItems.Count == 1 ?
								Res.GetString("1e190c3e-29b9-46b1-b2d0-396e7933e321", "This transaction has an outstanding amount of {0} {1}. There is an unposted Payment matched to this transaction to the value of {0} {2}, leaving {0} {3} available to match in this session. This can be found in the relevant Ledger's 'Payment Processing' Module. The details of the unposted payment are:",
									GlbCompany.CurrentCompany.LocalCurrency.RX_Code, roundedActualOutstandingAmount, roundedAmountMatchedToUnpostedPayments, roundedOutstandingAmount) :
								Res.GetString("ae5f29d3-ef4d-4fda-9c3a-b1a525667b31", "This transaction has an outstanding amount of {0} {1}. There are {4} unposted Payments matched to this transaction to the value of {0} {2}, leaving {0} {3} available to match in this session. This can be found in the relevant Ledger's 'Payment Processing' Module. The details of the unposted payments are:",
									GlbCompany.CurrentCompany.LocalCurrency.RX_Code, roundedActualOutstandingAmount, roundedAmountMatchedToUnpostedPayments, roundedOutstandingAmount, unpostedMatchedPaymentApprovalItems.Count));

							var paymentNumber = 0;
							foreach (var item in unpostedMatchedPaymentApprovalItems)
							{
								if (unpostedMatchedPaymentApprovalItems.Count > 1)
								{
									warningMessage.AppendLine(Res.GetString("808c22f5-bf9e-447b-8c82-5dcf71c30b62", "Payment {0}:", ++paymentNumber));
								}
								warningMessage.AppendLine(Res.GetString("f26584c1-7a03-452b-ae57-cfa1fe979ac3", "- Ledger: {0}", item.PaymentApproval.AV_Ledger));
								warningMessage.AppendLine(Res.GetString("9f6ebfeb-f47f-428e-babf-a1dd73fba1ac", "- Status: {0} ({1})",
									item.PaymentApproval.Lookups.AV_StatusList.GetDescriptionFromCode(item.PaymentApproval.AV_Status), item.PaymentApproval.AV_Status));
								warningMessage.AppendLine(Res.GetString("fe3305b7-794f-4bc9-88fd-85ecccc8e1b0", "- Payment Date: {0}", item.PaymentApproval.AV_PaymentDate.ToShortDateString()));
								warningMessage.AppendLine(Res.GetString("937496e7-17e1-4c18-a730-aa877faa6ab0", "- Check / Reference: {0}", item.PaymentApproval.AV_ChequeOrReference));
								warningMessage.AppendLine(Res.GetString("0ad74306-6051-43a1-a63a-373a3c395553", "- Matched Amount: {0} {1}", GlbCompany.CurrentCompany.LocalCurrency.RX_Code, item.A2_PaymentThisRun.ToString(GlbCompany.CurrentCompany.LocalCurrency.Decimals)));
								var paymentCurrencyDecimals = item.PaymentApproval.PaymentCurrency == null ? GlbCompany.CurrentCompany.LocalCurrency.Decimals : item.PaymentApproval.PaymentCurrency.Decimals;
								warningMessage.AppendLine(Res.GetString("09dc69f3-8f95-4fa7-8127-bb9b3de8a101", "- Payment Amount: {0} {1}", item.PaymentApproval.AV_RX_NKPaymentCurrency, item.PaymentApproval.AV_Amount.ToString(paymentCurrencyDecimals)));

								if (paymentNumber == 3)
								{
									warningMessage.AppendLine(Res.GetString("ceb394e3-d5a9-40a3-8c02-5aa8e3b22850", "..."));
									warningMessage.AppendLine(Res.GetString("44b73b0f-8c27-46dc-af45-90d54aa253db", "To see more unposted payments use Payment Processing module."));
									break;
								}
							}
							thisIMatching.OSPartialPaymentAmountInfo.AddWarning(warningMessage.ToString());
						}
					}
				}
			}
		}

		protected void CheckMatchStatus()
		{
			var thisIMatching = Parent as IMatching;

			if (thisIMatching != null)
			{
				ListValidation.ErrorIfInvalidCode(thisIMatching.MatchStatusInfo, ResString.GetMultilingualString("b0e5d32a-4ed6-4982-94e3-68bf4bb4b174", "Match Status Code"));
			}
		}

		protected void CheckMatchStatusReasonCode()
		{
			var thisIMatching = Parent as IMatching;

			if (thisIMatching != null)
			{
				var matchStatusReasonCodeInfo = thisIMatching.MatchStatusReasonCodeInfo;
				ListValidation.ErrorIfInvalidCode(matchStatusReasonCodeInfo, ResString.GetMultilingualString("fbd14bcc-83d2-45df-8de8-b20710ec79de", "Match Status Reason Code"));

				if (!matchStatusReasonCodeInfo.HasErrors())
				{
					if (!thisIMatching.MatchStatus.IsEmpty)
					{
						MandatoryValidation.CheckEntered(matchStatusReasonCodeInfo);
					}
					else if (!thisIMatching.MatchStatusReasonCode.IsEmpty)
					{
						matchStatusReasonCodeInfo.AddError(AccountingMatchStatusReasonCodeErrorMessage.MatchStatusReasonCodeShouldNotSpecified);
					}
				}
			}
		}

		protected override void CheckAH_TransactionType()
		{
			base.CheckAH_TransactionType();

			if (!Parent.AH_TransactionTypeInfo.HasErrors()
				&& !(Parent is APInvoice || Parent is APExchangeDifference || Parent is APPayment)
				&& IsMatchingENettPayment)
			{
				if (!(Parent is APCreditNote && IsMatchingPaymentSentToENett))
				{
					Parent.AH_TransactionTypeInfo.AddError(Res.GetString("58f180a4-db2d-4b1d-ab77-32e72d449262", "A ComPay payment can only be matched to AP invoices."));
				}
			}
		}

		bool IsMatchingPaymentSentToENett
		{
			get
			{
				var result = false;
				Payment payment = Parent.PaymentCurrentlyBeingMatched;
				if (payment != null &&
					payment.PaymentType == ReceiptTypes.eNettDirectDebit)
				{
					result = eNettHelper.ENettMessageHasBeenSucessfullySent(payment);
				}
				return result;
			}
		}

		protected override void CheckAH_RX_NKTransactionCurrency()
		{
			base.CheckAH_RX_NKTransactionCurrency();

			if (!Parent.AH_RX_NKTransactionCurrencyInfo.HasErrors() && IsMatchingForeignCurrencyENettPayment)
			{
				APInvoice invoice = Parent as APInvoice;

				if (invoice != null
					&& !(invoice.AH_RX_NKTransactionCurrency == Parent.PaymentApprovalCurrentlyBeingMatched.AV_RX_NKPaymentCurrency
						|| invoice.EnettAllowMultiCurrencyPaymentPropertyValue && invoice.AH_RX_NKTransactionCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency && !invoice.GetAmountOfLinesWithSpecificCurrency(Parent.PaymentApprovalCurrentlyBeingMatched.AV_RX_NKPaymentCurrency).IsEmpty)
					)
				{
					Parent.AH_RX_NKTransactionCurrencyInfo.AddError(Res.GetString("ffacf994-c6e0-4566-a3f4-b11e6e5f02d9", "Foreign currency ComPay payment can only be matched with either invoice/s in the same currency as the payment, or with local currency invoice/s having at least one line in the payment currency, provided this is permitted by the issuing party."));
				}
			}
		}

		protected override bool ShouldValidateBranchDepartmentCombinationForParentInDatabase
		{
			get
			{
				return true;
			}
		}

		protected override void CheckAH_DueDateIsValidZDateTimeRange()
		{
			//No need to be checked for Matching
		}

		protected override void CheckAH_InvoiceDateIsValidZDateTimeRange()
		{
			//No need to be checked for Matching
		}

		protected override void CheckAH_PostDateIsValidZDateTimeRange()
		{
			//No need to be checked for Matching
		}

		protected override void CheckAH_InvoicePaymentReferenceCode()
		{
			base.CheckAH_InvoicePaymentReferenceCode();

			var paymentReferenceCodeCollection = AccountingMasterFilesRegistry.Instance.InvoiceRemittanceConfiguration.GetValueWithoutFallback(Parent.AH_GC.ToGuid(), Guid.Empty, Guid.Empty);
			if (paymentReferenceCodeCollection != null && paymentReferenceCodeCollection.Count > 0)
			{
				var matchInvoiceRemittanceConfigurations = paymentReferenceCodeCollection.GetMatchInvoiceRemittanceConfiguration(Parent.Header).ToList();
				if (!string.IsNullOrEmpty(Parent.AH_InvoicePaymentReferenceCode) && !matchInvoiceRemittanceConfigurations.Any(x => x.Code == Parent.AH_InvoicePaymentReferenceCode))
				{
					Parent.AH_InvoicePaymentReferenceCodeInfo.AddError(Res.GetString("3CBBFE2E-3508-47A2-ACB8-4FD75946FC64", "The invoice remittance type '{0}' recorded against transaction number(s) {1} is invalid.\r\nPlease check the invoice remittance configuration under registry 'Accounting > Receivables Defaults > Default Settings > Invoice Remittance Configuration' and update the invoice remittance type via Receivables Transactions > Actions > Override Invoice Remittance Type before matching.", Parent.AH_InvoicePaymentReferenceCode, Parent.AH_TransactionNum));
				}
			}
		}

		bool IsMatchingENettPayment
		{
			get
			{
				bool isMatchingPayment = false;

				PaymentApprovalBase paymentApproval = Parent.PaymentApprovalCurrentlyBeingMatched;
				if (paymentApproval != null)
				{
					isMatchingPayment = paymentApproval.AV_PaymentType == ReceiptTypes.eNettDirectDebit;
				}

				Payment payment = Parent.PaymentCurrentlyBeingMatched;
				if (!isMatchingPayment && payment != null)
				{
					isMatchingPayment = payment.PaymentType == ReceiptTypes.eNettDirectDebit;
				}

				return isMatchingPayment;
			}
		}

		bool IsMatchingForeignCurrencyENettPayment
		{
			get
			{
				return Parent.PaymentApprovalCurrentlyBeingMatched != null
					   && Parent.PaymentApprovalCurrentlyBeingMatched.AV_PaymentType == ReceiptTypes.eNettDirectDebit
					   && Parent.PaymentApprovalCurrentlyBeingMatched.AV_RX_NKPaymentCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			}
		}

		bool IsPaymentCurrencyEqualToInvoiceCurrency
		{
			get
			{
				InvoicingBase invoice = Parent as InvoicingBase;
				return Parent != null
					&& Parent.PaymentApprovalCurrentlyBeingMatched != null
					&& invoice != null
					&& Parent.PaymentApprovalCurrentlyBeingMatched.AV_RX_NKPaymentCurrency == invoice.AH_RX_NKTransactionCurrency;
			}
		}

		bool InvoiceContainsMatchedLinesInPaymentCurrency
		{
			get
			{
				InvoicingBase invoice = Parent as InvoicingBase;

				if (invoice != null && Parent.PaymentApprovalCurrentlyBeingMatched != null)
				{
					return invoice.ContainsMatchedLinesInSpecificCurrency(Parent.PaymentApprovalCurrentlyBeingMatched.AV_RX_NKPaymentCurrency);
				}

				return false;
			}
		}
	}
}
