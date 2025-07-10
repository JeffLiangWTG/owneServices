using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance
{
	public class TransactionWithCashAdvanceRequestValidationVisitor : ICashAdvanceRequestProcessingByInvoiceVisitor
	{
		public TransactionWithCashAdvanceRequestValidationVisitor(Dictionary<string, List<string>> errorMessageBuilder)
		{
			ErrorMessageBuilder = errorMessageBuilder;
		}

		Dictionary<string, List<string>> ErrorMessageBuilder { get; }

		#region Invoice

		public void Visit(ARInvoice arInvoice) => VisitInternal(arInvoice, JobChargeSchema.JR_CAL_ARLine, (c) => c.JR_CAL_ARLine, (c) => (c.SellAccount, c.JR_RX_NKSellCurrency, c.JR_OSSellAmt));

		public void Visit(APInvoice apInvoice) => VisitInternal(apInvoice, JobChargeSchema.JR_CAL_APLine, (c) => c.JR_CAL_APLine, (c) => (c.CostAccount, c.JR_RX_NKCostCurrency, c.JR_OSCostAmt));

		public void Visit(APInvoice apInvoice, IEnumerable<Charge> jobChargesToImport)
		{
			var jobChargesWithMismatchedCashAdvanceAndInvoiceCurrency = jobChargesToImport.Where(jr => jr.APCashAdvanceRequirement.IsPaid && jr.APCashAdvanceRequirement.OSCurrency != apInvoice.AH_RX_NKTransactionCurrency);
			foreach (Charge charge in jobChargesWithMismatchedCashAdvanceAndInvoiceCurrency)
			{
				var errorDetailsFromJobCharge = FormattableString.Invariant($"Invoice currency is {apInvoice.AH_RX_NKTransactionCurrency} | Advance Payment currency of {charge.ChargeCode.AC_Code} | {charge.ChargeCode.AC_DescMultilingual} is {charge.APCashAdvanceRequirement.OSCurrency}");
				AddError(GetInvoiceAndCashAdvanceCurrencyMismatchErrorMessageHeader(), errorDetailsFromJobCharge);
			}
		}

		void VisitInternal(Invoice invoice, SchemaColumn cashAdvanceRequuestLineFKColumn, Func<JobCharge, ZGuid> getLinkedCashAdvanceRequestLinePk, Func<JobCharge, (OrgHeader Org, ZString Currency, ZDecimal Amount)> getInfoFromJobCharge)
		{
			if (invoice != null && invoice.Lines.Any())
			{
				var carInfoByInvoice = new CashAdvanceRequestInfoByInvoice(invoice);
				var carRequirementLines = carInfoByInvoice.CashAdvanceRequirements;

				if (carRequirementLines?.Any() ?? false)
				{
					var linePKsThatHaveMismatchedCurrency = carRequirementLines.Where(l => l.IsPaid && l.OSCurrency != invoice.AH_RX_NKTransactionCurrency)
																			.Select(l => (l.CashAdvanceRequestLinePK, l.CashAdvanceRequest?.CAH_RequestReferenceNumber ?? ZString.Empty))
																			.ToList();

					if (linePKsThatHaveMismatchedCurrency?.Any() ?? false)
					{
						invoice.IsContainCashAdvanceCurrencyMismatch = true;
						AddError(GetCurrencyMismatchErrorMessageHeader(), BuildErrorDetailsFromJobCharge(linePKsThatHaveMismatchedCurrency));
					}

					var carHeaders = carInfoByInvoice.CashAdvanceRequestHeaders;
					if (carHeaders != null)
					{
						foreach (var carHeader in carHeaders.Where(h => h.IsRequested))
						{
							var requestedLinesInInvoice = carRequirementLines.Where(l => l.CashAdvanceRequest.PK == carHeader.PK && l.IsRequested)
																			.Select(l => l.CashAdvanceRequestLinePK);
							var requestedLinesInCashAdvanceHeader = carHeader.Lines.Where(l => l.IsRequested)
																				.Select(l => l.PK);
							var missingLines = requestedLinesInCashAdvanceHeader.Except(requestedLinesInInvoice);

							if (missingLines.Any())
							{
								var errorMessage = string.Empty;
								if (invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
								{
									errorMessage = GetAllRequestedLinesAreNotPostedErrorMessageHeaderForAR();
								}
								else if (invoice.AH_Ledger == LedgerTypes.AccountsPayable)
								{
									errorMessage = GetAllRequestedLinesAreNotPostedErrorMessageHeaderForAP();
								}
								AddError(errorMessage, BuildErrorDetailsFromMissingJobCharge(missingLines, carHeader.CAH_RequestReferenceNumber));
							}
						}
					}
				}

				string BuildErrorDetailsFromJobCharge(IEnumerable<(ZGuid calPK, ZString cahNumber)> clines)
				{
					var message = new ZStringBuilder();
					foreach (var lineInfo in clines)
					{
						var jobCharge = carInfoByInvoice.Charges.FirstOrDefault(c => getLinkedCashAdvanceRequestLinePk(c) == lineInfo.calPK);
						if (jobCharge != null)
						{
							var (org, currenyCode, amount) = getInfoFromJobCharge(jobCharge);
							message.Append($"{invoice.Header.OH_Code}-{invoice.Header.OH_FullName} | {jobCharge.ChargeCode.AC_Code} | {jobCharge.ChargeCode.AC_DescMultilingual} | {currenyCode} | {amount} | {lineInfo.cahNumber}");
						}
					}
					return message.ToStringWithNewLineBetweenAppends();
				}

				string BuildErrorDetailsFromMissingJobCharge(IEnumerable<ZGuid> clinePKs, ZString cahNumber)
				{
					var message = new ZStringBuilder();
					foreach (var linePK in clinePKs)
					{
						var jobCharge = invoice.Factory.LoadTop1<JobCharge>(new ZQuery(cashAdvanceRequuestLineFKColumn, linePK));
						if (jobCharge != null)
						{
							var (org, currenyCode, amount) = getInfoFromJobCharge(jobCharge);
							message.Append($"{org?.OH_Code}-{org?.OH_FullName} | {jobCharge.ChargeCode.AC_Code} | {jobCharge.ChargeCode.AC_DescMultilingual} | {currenyCode} | {amount} | {cahNumber}");
						}
					}
					return message.ToStringWithNewLineBetweenAppends();
				}
			}
		}

		#endregion

		#region Receipt and Payment

		public void Visit(ARReceipt receipt)
		{
			var cahsInfo = new CashAdvanceRequestInfoByPaymentOrReceipt(receipt);
			VisitInternal(cahsInfo);
		}

		public void Visit(APPayment payment)
		{
			var cahsInfo = new CashAdvanceRequestInfoByPaymentOrReceipt(payment);
			VisitInternal(cahsInfo);
		}

		void VisitInternal(CashAdvanceRequestInfoByPaymentOrReceipt cahsInfo)
		{
			if (cahsInfo?.CashAdvanceRequestHeaders?.Any() ?? false)
			{
				var invoicedCAHs = cahsInfo?.CashAdvanceRequestHeaders.Where(cah => cah.Lines.OfType<AccCashAdvanceRequestLine>().Any(l => l.IsInvoiced));
				if (invoicedCAHs.Any())
				{
					foreach (var cah in invoicedCAHs)
					{
						AddError("c0403a47-baba-4bd7-8b71-dd752c22a5d2", GetInvoicedCashAdvanceLineExistsErrorMessage(cah));
					}
				}
			}
		}

		#endregion

		void AddError(string key, string errorMessage)
		{
			if (!ErrorMessageBuilder.ContainsKey(key))
			{
				ErrorMessageBuilder.Add(key, new List<string>());
			}
			ErrorMessageBuilder[key].Add(errorMessage);
		}

		internal static string[] GetMessageHeadersOfAllErrorsCheckedOnInvoicePosting()
		{
			return new string[]
			{
				GetCurrencyMismatchErrorMessageHeader(),
				GetAllRequestedLinesAreNotPostedErrorMessageHeaderForAR(),
				GetAllRequestedLinesAreNotPostedErrorMessageHeaderForAP(),
				GetInvoiceAndCashAdvanceCurrencyMismatchErrorMessageHeader(),
			};
		}

		static string GetCurrencyMismatchErrorMessageHeader()
		{
			var errMsg = Res.GetString("750ed198-1f41-4a87-a403-2191ce9cc5e8", @"One or more charges have a paid Advance Payment in a different currency to the currency of the invoice being posted. Unable to apply Advance Payment to the invoice.

If the Advance Payment was incorrectly created with the wrong currency, please cancel and recreate the Advance Payment in the correct currency.

***Important:
If the currency entered on this invoice is incorrect, the following steps must be taken to post the invoice:
1. Change the currency on this invoice.
2. Go to Actions and Save as Incomplete.
3. Close the invoice and go to the Payables > Incomplete Invoices module.
4. Open the invoice and post.

Note: If you do not change the invoice currency before saving as incomplete, the invoice cannot be posted from the Incomplete Invoices module. It will need to be canceled, and the invoice re-entered.");
			return errMsg;
		}

		static string GetAllRequestedLinesAreNotPostedErrorMessageHeaderForAR()
		{
			return Res.GetString("9a78544c-1482-4c73-b626-21961d5456dc", "One or more charges have an unpaid Advance Payment. However, not all charges linked to the same Advance Payment are being posted. Please post all charges that relate to a single Advance Payment. Otherwise, cancel the request in order to post this charge.");
		}

		static string GetAllRequestedLinesAreNotPostedErrorMessageHeaderForAP()
		{
			return Res.GetString("1bee22ca-7f04-41a1-b55e-ccfcb31c9d65", @"One or more accruals have an unpaid AP Advance Payment Request, however not all accruals linked to the same Advance Payment Request are being posted.  
If all costs that relate to a single Advance Payment Request are not included in the invoice, the Advance Payment Request must be canceled before the invoice can be posted.");
		}

		static string GetInvoiceAndCashAdvanceCurrencyMismatchErrorMessageHeader()
		{
			return Res.GetString("17a681d9-f197-4a83-8fda-55fa7769dd23", "The related job charge has a paid Advance Payment in a different currency to the currency of the invoice being posted. Unable to apply Advance Payment to the invoice. Please correct the currency of the Invoice, or cancel and re-create the Advance Payment to post this charge.");
		}

		static string GetInvoicedCashAdvanceLineExistsErrorMessage(CashAdvanceRequestHeader cah)
		{
			if (cah.CAH_Ledger == LedgerTypes.AccountsPayable)
			{
				return Res.GetString("34de3ac3-24d0-4d7a-9445-bc545efc1f77", "Payment cannot be reversed as it is linked to Advance Payment Request [{0}] on Job #[{1}] which has costs posted for one or more of the Advance Payment lines. Please reverse the invoice before reversing the payment", cah.CAH_RequestReferenceNumber, cah.Job.JH_JobNum);
			}
			else if (cah.CAH_Ledger == LedgerTypes.AccountsReceivable)
			{
				return Res.GetString("6e5b9b9b-79d1-41e4-8d75-c3d09ae68793", "Receipt cannot be reversed as it is linked to Advance Payment Request [{0}] on Job #[{1}] which has invoiced charge lines. Please reverse the invoice before reversing the receipt", cah.CAH_RequestReferenceNumber, cah.Job.JH_JobNum);
			}
			return string.Empty;
		}
	}
}
