using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;

namespace Enterprise.Accounting.Business
{
	public static class ConsolCostSelector
	{
		/// <summary>
		/// Returns the best matched consol cost from a collection of consol costs. Machting is done by Creditor - Currency -Amount.
		/// </summary>
		/// <param name="costToMatchWith">Cost against which the matching will be done</param>
		/// <param name="costs">Collection in which search will be done to find the best match. Pleae note that this function doesn't check whether the collection contains any invalid consol cost.</param>
		/// <returns></returns>
		public static JobConsolCost GetBestMatch(JobConsolCost costToMatchWith, IEnumerable<JobConsolCost> costs, int amountMultiplier = 1)
		{
			JobConsolCost result = null;

			if (costs.Any())
			{
				var creditorExpressionGroup = new Func<JobConsolCost, bool>[]
					{ x => x.E6_OH_Creditor == costToMatchWith.E6_OH_Creditor, x => x.E6_OH_Creditor.IsEmpty, x => !TransactionLineJobChargeTransformer.IsBringForwardAgainstCreditorEnabled }; //Creditor

				var currencyExpressionGroup = new Func<JobConsolCost, bool>[]
						{ x => x.E6_RX_NKCurrency == costToMatchWith.E6_RX_NKCurrency, x => true };//Currency

				var apportionChargeMatchExpression = new Func<JobConsolCost, bool>(
						 x => x.ApportionmentCharges.Count == costToMatchWith.ApportionmentCharges.Count &&
							!x.ApportionmentCharges.Cast<ApportionSplitCharge>().Select(ac => ac.JR_JH)
								.Except(costToMatchWith.ApportionmentCharges.Cast<ApportionSplitCharge>().Select(acm => acm.JR_JH))
								.Any());//apportion charge match

				result = GetBestMatchedCost(costToMatchWith, costs, amountMultiplier, creditorExpressionGroup, currencyExpressionGroup, apportionChargeMatchExpression);
			}

			return result;
		}

		static JobConsolCost GetBestMatchedCost(JobConsolCost costToMatchWith, IEnumerable<JobConsolCost> costs, int amountMultiplier,
			Func<JobConsolCost, bool>[] creditorExpressionGroup, Func<JobConsolCost, bool>[] currencyExpressionGroup, Func<JobConsolCost, bool> apportionChargeMatchExpression)
		{
			JobConsolCost result = null;
			var allCosts = costs.ToArray();
			JobConsolCost[] selectedCosts = null;

			foreach (Func<JobConsolCost, bool> expression in creditorExpressionGroup)
			{
				selectedCosts = allCosts.Where(c => expression(c)).ToArray();
				if (!selectedCosts.IsNullOrEmpty())
				{
					break;
				}
			}

			allCosts = selectedCosts;

			foreach (Func<JobConsolCost, bool> expression in currencyExpressionGroup)
			{
				selectedCosts = allCosts.Where(c => expression(c)).ToArray();
				if (!selectedCosts.IsNullOrEmpty())
				{
					break;
				}
			}

			selectedCosts = selectedCosts.Where(c => apportionChargeMatchExpression(c)).ToArray();

			if (selectedCosts.Any())
			{
				var amountToMatchWith = costToMatchWith.E6_LocalCostAmount * amountMultiplier;
				var signToMatchWith = Math.Sign(amountToMatchWith);

				result = selectedCosts
								.OrderBy(x => Math.Abs(x.E6_LocalCostAmount - amountToMatchWith))
								.ThenBy(x => Math.Abs(Math.Sign(x.E6_LocalCostAmount) - signToMatchWith))
								.FirstOrDefault(); //Amount
			}
			return result;
		}

		public static ApportionMethodSelectionInfo GetBestMatchedApportionMethod(IEnumerable<ApportionMethodSelectionInfo> apportionMethodSelectionInfo, ZGuid creditorToMatchWith, ZString currencyToMatchWith, ZDecimal amountToMatchWith)
		{
			ApportionMethodSelectionInfo result = null;

			if (apportionMethodSelectionInfo.Any())
			{
				var creditorExpressionGroup = new Func<ApportionMethodSelectionInfo, bool>[]
						{ x => x.CreditorPK == creditorToMatchWith, x => x.CreditorPK.IsEmpty, x => !TransactionLineJobChargeTransformer.IsBringForwardAgainstCreditorEnabled }; //Creditor

				var currencyExpressionGroup = new Func<ApportionMethodSelectionInfo, bool>[]
						{ x => x.Currency == currencyToMatchWith, x => true };

				var allApportionMethodSelectionInfo = apportionMethodSelectionInfo.ToArray();
				ApportionMethodSelectionInfo[] selectedApportionMethodSelectionInfo = null;

				foreach (var expression in creditorExpressionGroup)
				{
					selectedApportionMethodSelectionInfo = allApportionMethodSelectionInfo.Where(c => expression(c)).ToArray();
					if (!selectedApportionMethodSelectionInfo.IsNullOrEmpty())
					{
						break;
					}
				}

				allApportionMethodSelectionInfo = selectedApportionMethodSelectionInfo;

				foreach (var expression in currencyExpressionGroup)
				{
					selectedApportionMethodSelectionInfo = allApportionMethodSelectionInfo.Where(c => expression(c)).ToArray();
					if (!selectedApportionMethodSelectionInfo.IsNullOrEmpty())
					{
						break;
					}
				}

				if (selectedApportionMethodSelectionInfo.Any())
				{
					result = selectedApportionMethodSelectionInfo
								.OrderBy(x => Math.Abs(x.Amount - amountToMatchWith))
								.ThenBy(x => Math.Abs(Math.Sign(x.Amount) - Math.Sign(amountToMatchWith)))
								.FirstOrDefault(); //Amount
				}
			}

			return result;
		}

		public class ApportionMethodSelectionInfo
		{
			public ApportionMethodSelectionInfo()
			{
			}

			public ZGuid CreditorPK { get; set; }
			public ZString Currency { get; set; }
			public ZDecimal Amount { get; set; }
			public ZString ApportionMethod { get; set; }
			public ZGuid TaxRatePK { get; set; }
			public ZGuid InvTaxMsgPK { get; set; }
		}
	}
}
