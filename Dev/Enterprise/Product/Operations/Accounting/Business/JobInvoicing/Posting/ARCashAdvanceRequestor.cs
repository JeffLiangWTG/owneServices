using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting
{
	public class ARCashAdvanceRequestor
	{
		public ARCashAdvanceRequestor(Job job)
		{
			this.Job = job;
		}

		public BusinessObjectFactory Factory => Job.Factory;

		public (ZString, List<CashAdvanceRequestHeader>) GenerateRequests()
		{
			var result = ZString.Empty;
			List<CashAdvanceRequestHeader> requestHeaders = null;

			try
			{
				var eligibleCharges = GetEligibleCharges();
				var chargesGroupedByDebtorAndCurrency = DistributeCharges(eligibleCharges);
				requestHeaders = CreateCashAdvanceRequests(chargesGroupedByDebtorAndCurrency);
				Factory.Save();
				result = PrepareSummary(requestHeaders);
			}
			catch (ZCannotSaveException ex)
			{
				result = Res.GetString("b423e2de-7cef-451f-bcfa-ffc9892c8ef7", "Error during save Advance Payment requests: {0}", ex.Message);
			}
			return (result, requestHeaders);
		}

		ZString PrepareSummary(List<CashAdvanceRequestHeader> requestHeaders)
		{
			ZString result;
			if (requestHeaders.Any())
			{
				var stringBuilder = new ZStringBuilder(Res.GetString("BBF1F139-CF0C-4258-8B39-72A6A5310B0F", "Debtor/Request ID/Currency/Total Amount"));
				foreach (var x in requestHeaders)
				{
					stringBuilder.AppendFormat("{0}, {1}, {2}, {3}", x.Organization.OH_Code, x.CAH_RequestReferenceNumber, x.TransactionCurrency.RX_Code, x.CAH_OSAmount.ToString(x.TransactionCurrency.Decimals));
				}
				string contentText = stringBuilder.ToStringWithDelimiterBetweenAppends(System.Environment.NewLine);
				result = Res.GetString("0e91190e-0ca8-4b43-892c-1e300e59d451", @"The following Advance Payment requests have been generated:
{0}", contentText);
			}
			else
			{
				result = Res.GetString("e060a3b4-72d0-4e75-afb2-efd5ec90fba1", "No Advance Payment requests have been generated.");
			}

			return result;
		}

		public string GetValidErrorsBeforeGenerateRequest()
		{
			if (Job.HasChanges)
			{
				return Res.GetString("0b237c6d-6954-4d9e-b173-daa87bcb5e62", "Please save job {0} before creating Advance Payment request.", Job.JH_JobNum);
			}

			if (!Job.Charges.OfType<BaseCharge>().Any(c => !c.IsRevenuePosted && c.JR_IsARCashAdvance && c.JR_CAL_ARLine.IsEmpty))
			{
				return Res.GetString("eaba071f-c68f-4e79-9186-5692580c798a", "No pending charges requiring a Advance Payment were found.");
			}

			var charges = GetEligibleCharges();
			if (charges.Sum(x => x.JR_Sell_LocalSellAmount) == 0 && charges.Sum(x => x.JR_Calc_OSSellAmtWithGST) == 0)
			{
				return Res.GetString("89d30483-7f0c-482f-9a0b-7b159589a5ef", "Cannot request Advance Payment, charge amounts are missing.");
			}

			return string.Empty;
		}

		public string GetValidWarningsBeforeGenerateRequest()
		{
			ZString result = ZString.Empty;
			var allCharges = Job.Charges.OfType<Charge>();
			var ineligibleCharges = allCharges.Where(c => !c.IsRevenuePosted
									&& c.JR_IsARCashAdvance && c.JR_CAL_ARLine.IsEmpty
									&& c.JR_InvoiceType == InvoiceTypesList.Codes.DoNotPost);
			if (ineligibleCharges.Any())
			{
				var stringBuilder = new ZStringBuilder();
				foreach (var x in ineligibleCharges)
				{
					stringBuilder.AppendFormat("{0}, {1}, {2}, {3}, {4}", x.ChargeCode.AC_Code, x.JR_Calc_OSSellAmtWithGST.ToString(x.SellCurrency.Decimals), x.JR_OSSellCurrencyCode, x.SellAccount.OH_Code, x.SellAccount.OH_FullName);
				}
				string contentText = stringBuilder.ToStringWithDelimiterBetweenAppends(System.Environment.NewLine);
				result = Res.GetString("a37c489e-11a9-418a-9eea-cebfd64bac6a", @"No Advance Payment Request was generated for the following charges, because the Invoice Type is 'NON - Not for Invoicing'.
{0}", contentText);
			}
			return result;
		}

		protected internal Job Job;

		//Finds all eligible charges for cash advance request on the Job
		IEnumerable<Charge> GetEligibleCharges()
		{
			var result = Job.Charges.OfType<Charge>().Where(c => !c.IsRevenuePosted
																	&& c.JR_IsARCashAdvance && c.JR_CAL_ARLine.IsEmpty
																	&& c.JR_InvoiceType != InvoiceTypesList.Codes.DoNotPost);
			return result;
		}

		Dictionary<RequestHeaderKey, List<Charge>> DistributeCharges(IEnumerable<Charge> chargeCollection)
		{
			var result = new Dictionary<RequestHeaderKey, List<Charge>>();
			//Group charges on the job by Debtor and Currency, with the same job header and ledger "AR"
			// the dictionary's key contains the RequestHeaderKey, the value contains the job charges collection

			foreach (var charge in chargeCollection)
			{
				var key = CreateKey(charge);
				var entry = result.FirstOrDefault(x => x.Key.CompareTo(key) == 0);
				if (entry.Value == null)
				{
					result[key] = new List<Charge>(new Charge[] { charge });
				}
				else
				{
					entry.Value.Add(charge);
				}
			}

			return result;
		}

		List<CashAdvanceRequestHeader> CreateCashAdvanceRequests(Dictionary<RequestHeaderKey, List<Charge>> chargesGroupedByDebtorAndCurrency)
		{
			var result = new List<CashAdvanceRequestHeader>();
			foreach (var group in chargesGroupedByDebtorAndCurrency)
			{
				var header = CreateRequestHeader(group);
				CreateRequestLines(header, group.Value);
				result.Add(header);
			}
			return result;
		}

		CashAdvanceRequestHeader CreateRequestHeader(KeyValuePair<RequestHeaderKey, List<Charge>> item)
		{
			var headerKey = item.Key;
			var header = Factory.New<CashAdvanceRequestHeader>();
			header.CAH_Ledger = LedgerTypes.AccountsReceivable;
			header.CAH_JH_Job = headerKey.JobPK;
			header.CAH_OH_Organization = headerKey.OrgPK;
			header.CAH_RX_NKTransactionCurrency = headerKey.Currency;
			header.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Requested;
			header.CAH_GC_Company = GlbCompany.CurrentCompany.PK;
			var charges = item.Value;
			header.CAH_LocalAmount = charges.Sum(c => c.JR_Sell_LocalSellAmount);
			if (header.CAH_RX_NKTransactionCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
			{
				header.CAH_OSAmount = charges.Sum(c => c.JR_Calc_OSSellAmtWithGST);
			}
			else
			{
				header.CAH_OSAmount = header.CAH_LocalAmount;
			}
			header.CAH_OSPaidAmount = 0m;
			header.CAH_LocalPaidAmount = 0m;
			return header;
		}

		void CreateRequestLines(CashAdvanceRequestHeader requestHeader, List<Charge> charges)
		{
			foreach (var charge in charges)
			{
				var line = requestHeader.Lines.AddNew();
				line.CAL_CAH_RequestHeader = requestHeader.PK;
				line.CAL_Status = CashAdvanceStatusCodes.RequestHeader.Requested;
				line.CAL_LocalAmount = charge.JR_Sell_LocalSellAmount;
				if (requestHeader.CAH_RX_NKTransactionCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
				{
					line.CAL_OSAmount = charge.JR_Calc_OSSellAmtWithGST;
				}
				else
				{
					line.CAL_OSAmount = line.CAL_LocalAmount;
				}
				line.CAL_OSPaidAmount = 0m;
				line.CAL_LocalPaidAmount = 0m;
				line.CAL_GC_Company = GlbCompany.CurrentCompany.PK;

				charge.JR_CAL_ARLine = line.PK;
			}
		}

		RequestHeaderKey CreateKey(Charge charge)
		{
			var keyCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			switch (charge.JR_InvoiceType)
			{
				case InvoiceTypesList.Codes.FreightInvoice:
				case InvoiceTypesList.Codes.FreightInvoice_Batching:
				case InvoiceTypesList.Codes.ForeignCurrencyInvoice:
				case InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching:
				case InvoiceTypesList.Codes.DisbursementInForeignCurrency:
				case InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching:
				case AgencyInvoiceTypesList.Codes.ForeignCollect:
				case AgencyInvoiceTypesList.Codes.ForeignCollect_Batching:
				case AgencyInvoiceTypesList.Codes.ForeignPrePaid:
				case AgencyInvoiceTypesList.Codes.ForeignPrePaid_Batching:
				case InvoiceTypesList.Codes.SelfBillingInvoice:
					if (charge.SellCurrency != null)
					{
						keyCurrency = charge.SellCurrency.RX_Code;
					}
					break;
				case InvoiceTypesList.Codes.InvoicePerTaxCode:
				case InvoiceTypesList.Codes.InvoicePerTaxCode_Batching:
				case InvoiceTypesList.Codes.FinalInvoice:
				case InvoiceTypesList.Codes.FinalInvoice_Batching:
				case InvoiceTypesList.Codes.DisbursementInvoice:
				case InvoiceTypesList.Codes.DisbursementInvoice_Batching:
				case InvoiceTypesList.Codes.DestinationChargesInvoice:
				case InvoiceTypesList.Codes.DestinationChargesInvoice_Batching:
				case AgencyInvoiceTypesList.Codes.LocalCollect:
				case AgencyInvoiceTypesList.Codes.LocalCollect_Batching:
				case AgencyInvoiceTypesList.Codes.LocalPrePaid:
				case AgencyInvoiceTypesList.Codes.LocalPrePaid_Batching:
				case AgencyInvoiceTypesList.Codes.Misc:
				case AgencyInvoiceTypesList.Codes.Misc_Batching:
					if (!charge.BillInLocalCurrency && charge.SellCurrency != null && charge.SellCurrency.RX_Code != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
					{
						keyCurrency = charge.SellCurrency.RX_Code;
					}
					break;
			}
			return new RequestHeaderKey(charge.JR_JH, LedgerTypes.AccountsReceivable, charge.JR_OH_SellAccount, keyCurrency);
		}
	}

	public class RequestHeaderKey : IComparable
	{
		public RequestHeaderKey(ZGuid jobPK, ZString ledger, ZGuid orgPK, ZString currency)
		{
			this.JobPK = jobPK;
			this.Ledger = ledger;
			this.OrgPK = orgPK;
			this.Currency = currency;
		}

		public readonly ZGuid JobPK;
		public readonly ZString Ledger;
		public readonly ZGuid OrgPK;
		public readonly ZString Currency;

		public int CompareTo(object obj)
		{
			var requestKey = (RequestHeaderKey)obj;

			if (requestKey.JobPK == JobPK &&
				requestKey.Ledger == Ledger &&
				requestKey.OrgPK == OrgPK &&
				requestKey.Currency == Currency)
			{
				return 0;
			}
			else
			{
				return -1;
			}
		}
	}
}
