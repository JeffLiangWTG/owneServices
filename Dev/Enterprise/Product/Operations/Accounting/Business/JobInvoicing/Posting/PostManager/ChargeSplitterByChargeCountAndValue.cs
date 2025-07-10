using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting
{
	/// <summary>
	/// Splits a collection of charges based on a maximum number of charges allowed in a single collection (ie, invoice).
	/// This is primarily for China Invoices where the maximum number of charges per invoice is 8.
	/// </summary>
	public static partial class ChargeSplitterByChargeCountAndValue
	{
		public static PostingChargeCollection GetSplitCharges(PostingChargeCollection postingCharges)
		{
			var newPostingCharges = new PostingChargeCollection();

			foreach (IReceivablesPostingChargeCollection charges in postingCharges)
			{
				if (ShouldChargesBeSplit(charges))
				{
					var maxLineNo = GetMaxLineNo();
					var maxValue = GetMaxValue();

					if (charges.Any(x => maxValue > 0 && x.LocalSellAmount > maxValue))
					{
						throw new CriticalPostingErrorException(GetErrorMessageWhenASingleChargeExceedsMaxValue());
					}

					int splitInvoiceCount = 0;
					while (charges.Count > 0)
					{
						var newCharges = new IReceivablesPostingChargeCollection();
						int chargeCountToTransfer = (maxLineNo > 0 && charges.Count > maxLineNo) ? maxLineNo : charges.Count;

						for (int i = 0; i < chargeCountToTransfer; i++)
						{
							var firstCharge = i % 2 == 1
								? charges.Where(x => maxValue == 0 || x.LocalSellAmount + newCharges.TotalExclTaxValueInLocalCurrency <= maxValue)
									.OrderByDescending(x => x.LocalSellAmount).FirstOrDefault()

								: charges.Where(x => maxValue == 0 || x.LocalSellAmount + newCharges.TotalExclTaxValueInLocalCurrency <= maxValue)
									.OrderBy(x => x.LocalSellAmount).FirstOrDefault();

							if (firstCharge == null)
							{
								break;
							}

							newCharges.Add(firstCharge);
							charges.Remove(firstCharge);
							if (charges.Count <= 0)
							{
								break;
							}
						}

						var newKey = new PostingChargeKey(charges.Key.Org, charges.Key.InvoiceType, charges.Key.OrgAddress, charges.Key.OrgContact, charges.Key.TaxRatePostingGroupId, charges.Key.Branch, charges.Key.PlaceOfSupply, charges.Key.TaxBranch);
						newKey.SplitInvoiceCount = splitInvoiceCount;
						splitInvoiceCount++;

						newPostingCharges.SetCharges(newKey, newCharges);
					}
				}
				else
				{
					newPostingCharges.SetCharges(charges.Key, charges);
				}
			}

			return newPostingCharges;
		}

		public static bool ShouldThisChargesBeSplitted(BaseCharge charge)
		{
			return ShouldSplit(1, charge.JR_LocalSellAmt, () => charge.DisplaySellInvoiceAddressBO);
		}

		public static string GetErrorMessageWhenASingleChargeExceedsMaxValue()
		{
			var regPath = ((IMultilingualRegistryItem)AccountingConfigurationRegistry.Instance.JobInvoiceMaximumValueOfSplittingRules).LocationMultilingual;
			return Res.GetString("70592AFB-4A0B-4C55-8EA8-073EC2B62E83", "The Charge has exceeded the maximum value ({0}) as defined in the following registry {1}. Please split the charge to multiple lines with value less than the maximum value.", AccountingConfigurationRegistry.Instance.JobInvoiceMaximumValueOfSplittingRules.Value, regPath);
		}

		static bool ShouldChargesBeSplit(IReceivablesPostingChargeCollection charges)
		{
			if (charges == null || charges.Count == 0)
			{
				return false;
			}

			return ShouldSplit(charges.Count, charges.TotalExclTaxValueInLocalCurrency, () => charges.DebtorAddressBizO);
		}

		static bool ShouldSplit(int lineCount, ZDecimal amount, Func<OrgAddress> getDebtorAddress) // A delegate is used to defer the loading of Address BizO. BizO will be loaded only if MaxLineNo and MaxValue conditions are met.
		{
			var maxLineNo = GetMaxLineNo();
			var maxValue = GetMaxValue();
			return ((maxLineNo > 0 && lineCount > maxLineNo) || (maxValue > 0 && amount > maxValue)) && AccountingConfigurationRegistry.Instance.JobInvoiceAddressCountry.CanSplitRuleBeAppliedOnDebtor(getDebtorAddress());
		}

		static int GetMaxLineNo()
		{
			var jobInvoiceMaximumNumberOfChargesOfSplittingRules = AccountingConfigurationRegistry.Instance.JobInvoiceMaximumNumberOfChargesOfSplittingRules.Value;
			return jobInvoiceMaximumNumberOfChargesOfSplittingRules < 0 ? 0 : jobInvoiceMaximumNumberOfChargesOfSplittingRules;
		}

		static decimal GetMaxValue()
		{
			var jobInvoiceMaximumValueOfSplittingRules = AccountingConfigurationRegistry.Instance.JobInvoiceMaximumValueOfSplittingRules.Value;
			return jobInvoiceMaximumValueOfSplittingRules < 0 ? 0 : jobInvoiceMaximumValueOfSplittingRules;
		}/*Todo-Review*/
	}
}
