using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting
{
	class ChargeAmountsRounder
	{
		public void RoundChargeAmounts(PostingChargeCollection distributedCharges, string roundingRule)
		{
			if (roundingRule != Constants.RoundingRules.Codes.None)
			{
				List<ZGuid> unEligibleJobs = new List<ZGuid>();

				foreach (IReceivablesPostingChargeCollection distributedChargeCollection in distributedCharges)
				{
					Dictionary<ZGuid, List<Charge>> eligibleJobs = new Dictionary<ZGuid, List<Charge>>();

					foreach (Charge charge in distributedChargeCollection)
					{
						if (charge.Job != null && !unEligibleJobs.Contains(charge.Job.PK) && charge.SellAccount == charge.Job.LocalCharges
							&& charge.ChargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Freight
							&& RoundingRuleAdditionalCondition(roundingRule, charge))
						{
							if (eligibleJobs.ContainsKey(charge.Job.PK))
							{
								List<Charge> charges = null;
								eligibleJobs.TryGetValue(charge.Job.PK, out charges);
								if (charges != null)
								{
									charges.Add(charge);
								}
							}
							else
							{
								bool isJobEligible = false;

								if (charge.Job.Parent != null)
								{
									ForwardingShipment shipment = charge.Job.Parent as ForwardingShipment;
									if (shipment != null && shipment.IsAir && shipment.IsImport())
									{
										isJobEligible = true;
									}
								}

								if (isJobEligible)
								{
									eligibleJobs.Add(charge.Job.PK, new List<Charge> { charge });
								}
								else
								{
									unEligibleJobs.Add(charge.Job.PK);
								}
							}
						}
					}

					foreach (List<Charge> value in eligibleJobs.Values)
					{
						Round(roundingRule, distributedChargeCollection, value);
					}
				}
			}
		}

		bool RoundingRuleAdditionalCondition(string roundingRule, Charge charge)
		{
			bool result = false;
			switch (roundingRule)
			{
				case Constants.RoundingRules.Codes.JapanYen:
					{
						result = charge.BillInLocalCurrency || charge.JR_RX_NKSellCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
					}
					break;
				case Constants.RoundingRules.Codes.JapanYenWithCharge:
					{
						result = charge.BillInLocalCurrency &&
							(charge.JR_RX_NKSellCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency
							|| charge.JR_AC == AccountingConfigurationRegistry.Instance.RoundingChargeCode.Value
							|| charge.JR_AC == AccountingConfigurationRegistry.Instance.IncludeInRoundingChargeCode.Value);
					}
					break;
			}
			return result;
		}

		void Round(string roundingRule, IReceivablesPostingChargeCollection distributedChargeCollection, List<Charge> chargesForRounding)
		{
			if (chargesForRounding != null && chargesForRounding.Count > 0)
			{
				switch (roundingRule)
				{
					case Constants.RoundingRules.Codes.JapanYen:
						{
							ZDecimal roundingAmount = GetRoundingAmount(GetLocalTotal(chargesForRounding), 10);
							Charge charge = GetChargeWithMaxAmount(chargesForRounding);
							if (roundingAmount > 0 && charge != null)
							{
								IDisposable calculationSuspender = null;
								if (charge.JR_RX_NKSellCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
								{
									calculationSuspender = ((IPostingCharge)charge).SuspendAutoCalculations();
								}

								try
								{
									charge.JR_LocalSellAmt += roundingAmount;
								}

								finally
								{
									calculationSuspender?.Dispose();
								}
							}
						} break;
					case Constants.RoundingRules.Codes.JapanYenWithCharge:
						{
							ZDecimal roundingAmount = GetRoundingAmount(GetLocalTotal(chargesForRounding), 10);
							Charge charge = GetRealChargeForRounding(chargesForRounding);
							if (roundingAmount > 0 && charge != null && charge.InvoicingJob != null)
							{
								Charge roundingCharge = charge.InvoicingJob.Charges.AddNew();
								roundingCharge.SetContext(NewChargeLoadActionOnPosting.AddChargeWhenPostingCancelled);
								roundingCharge.JR_AC = AccountingConfigurationRegistry.Instance.RoundingChargeCode.Value;
								roundingCharge.JR_OH_SellAccount = charge.JR_OH_SellAccount;
								roundingCharge.JR_OA_SellInvoiceAddress = charge.JR_OA_SellInvoiceAddress;
								roundingCharge.JR_OC_SellInvoiceContact = charge.JR_OC_SellInvoiceContact;
								roundingCharge.JR_AT_SellGSTRate = AccountingConfigurationRegistry.Instance.MainNotReportableTaxID.Value;
								roundingCharge.JR_InvoiceType = charge.JR_InvoiceType;
								roundingCharge.JR_LocalSellAmt = roundingAmount;

								using (new DisposableAction(() => roundingCharge.IgnoreValidationSuspended = true, () => roundingCharge.IgnoreValidationSuspended = false))
								{
									roundingCharge.Validation.ValidateAll();
								}

								distributedChargeCollection.Add(roundingCharge);
							}
						} break;
				}
			}
		}

		ZDecimal GetLocalTotal(List<Charge> chargesForRounding)
		{
			ZDecimal result = 0;
			foreach (Charge charge in chargesForRounding)
			{
				result += charge.JR_LocalSellAmt;
			}
			return result;
		}

		Charge GetChargeWithMaxAmount(List<Charge> chargesForRounding)
		{
			Charge result = null;
			if (chargesForRounding.Count > 0)
			{
				result = chargesForRounding[0];
				foreach (Charge charge in chargesForRounding)
				{
					if (result.JR_LocalSellAmt < charge.JR_LocalSellAmt)
					{
						result = charge;
					}
				}
			}
			return result;
		}

		Charge GetRealChargeForRounding(List<Charge> chargesForRounding)
		{
			Charge result = null;

			foreach (Charge charge in chargesForRounding)
			{
				if (charge.JR_AC != AccountingConfigurationRegistry.Instance.RoundingChargeCode.Value
							&& charge.JR_AC != AccountingConfigurationRegistry.Instance.IncludeInRoundingChargeCode.Value)
				{
					result = charge;
					break;
				}
			}

			return result;
		}

		ZDecimal GetRoundingAmount(ZDecimal totalAmount, ZInt roundingNumber)
		{
			ZDecimal roundingAmount = totalAmount > 0 ? roundingNumber - (totalAmount % roundingNumber) : System.Math.Abs(totalAmount % roundingNumber);
			return roundingAmount == roundingNumber ? 0 : roundingAmount;
		}
	}
}