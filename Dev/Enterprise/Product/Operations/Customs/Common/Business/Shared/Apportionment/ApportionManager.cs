using System.Collections;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Common
{
	public sealed class ApportionManager
	{
		public ApportionManager(IApportionInvoiceHolder jobDeclaration, IApportionStrategy apportionStrategy)
		{
			this.jobDeclaration = jobDeclaration;
			this.apportionStrategy = apportionStrategy;
		}

		public void ApportionAll()
		{
			ClearAllApportionedCharges();
			ApportionCharges();
			AggregateAmountsFromLinesToInvoices();
			DeleteEmptyApportionedCharges();
		}

		#region ClearAllApportionedCharges

		void ClearAllApportionedCharges()
		{
			foreach (IChargeHolder chargeHolder in jobDeclaration.ChargeHolders)
			{
				foreach (IChargeApportionee chargeApportionee in chargeHolder.AllApportionees)
				{
					chargeApportionee.ApportionedCharges.ClearApportionedCharges();
				}
			}
		}

		#endregion

		#region Apportion Charges

		void ApportionCharges()
		{
			foreach (IChargeHolder chargeHolder in jobDeclaration.ChargeHolders)
			{
				List<ApportionChargeKey> chargeKeysApportioned = new List<ApportionChargeKey>();

				ArrayList charges = new ArrayList();//enumerationcockup error solution
				chargeHolder.Charges.CopyToList(charges);

				if (jobDeclaration.ChargeComparer != null) //this is to allow a precedence to be determined e.g. in the case of SG Customs where insurance is calculated as a % of C&F value
				{
					charges.Sort(jobDeclaration.ChargeComparer);
				}

				foreach (JobComInvCharge charge in charges)
				{
					ApportionChargeKey chargeKey = charge.ApportionChargeKey;
					if (!chargeKeysApportioned.Contains(chargeKey))
					{
						chargeKeysApportioned.Add(chargeKey);

						IChargeApportionee[] apportionees = GetApportionees(chargeKey, chargeHolder);

						if (apportionees.Length > 0)
						{
							if (charge.J7_Percentage > 0m)
							{
								DefaultPercentageAndCalculateAmounts(charge, apportionees);
							}
							else
							{
								ApportionCharge(chargeKey, chargeHolder, apportionees);
							}
						}
					}
				}
			}
		}

		void ApportionCharge(ApportionChargeKey apportionChargeKey, IChargeHolder chargeHolder, IChargeApportionee[] apportionees)
		{
			Money amountToApportion = GetAmountToApportion(chargeHolder, apportionChargeKey);

			if (amountToApportion.Currency != null && amountToApportion.Amount >= 0)
			{
				string distributeBy = chargeHolder.Charges.GetDistributeBy(apportionChargeKey);
				ApportionConsideringRoundingError(apportionees, amountToApportion, apportionChargeKey, distributeBy, chargeHolder);
			}
		}

		Money GetAmountToApportion(IChargeHolder chargeHolder, ApportionChargeKey apportionChargeKey)
		{
			Money result = chargeHolder.Charges.GetCharge(apportionChargeKey);
			if (!apportionChargeKey.IsFullApportionment)
			{
				Money overridenAmounts = GetOverridenAmountsFromChildren(chargeHolder, apportionChargeKey);

				if (overridenAmounts.Amount > 0m && overridenAmounts.Currency != null)
				{
					result = chargeHolder.CurrencyConverter.Subtract(result, overridenAmounts);
				}
			}
			return result;
		}

		Money GetOverridenAmountsFromChildren(IChargeHolder chargeHolder, ApportionChargeKey chargeKey)
		{
			Money result = Money.Empty;
			foreach (IChargeHolder childChargeHolder in chargeHolder.ImmediateChargeHolderChildren)
			{
				Money amountFromChildren = Money.Empty;
				if (DoesThisChargeHolderHaveThisCharge(childChargeHolder, chargeKey))
				{
					amountFromChildren = childChargeHolder.Charges.GetCharge(chargeKey);
				}
				else
				{
					amountFromChildren = GetOverridenAmountsFromChildren(childChargeHolder, chargeKey);
				}
				result = chargeHolder.CurrencyConverter.Add(result, amountFromChildren);
			}
			return result;
		}

		void ApportionConsideringRoundingError(IChargeApportionee[] apportionees, Money amountToApportion, ApportionChargeKey apportionChargeKey, string distributeBy, IChargeHolder chargeHolder)
		{
			ZDecimal totalBaseValue = GetTotalBaseValue(apportionees, distributeBy, chargeHolder.CurrencyConverter);

			if (totalBaseValue > 0)
			{
				ZDecimal sumOfApportionedCharge = 0m;
				var all = new List<BizOApportionedCharge>();

				foreach (IChargeApportionee apportionee in apportionees)
				{
					var baseValueOfApportionee = GetBaseValue(apportionee, distributeBy, chargeHolder.CurrencyConverter);
					var apportionedCharge = amountToApportion.Amount;
					if (totalBaseValue != baseValueOfApportionee)
					{
						apportionedCharge = amountToApportion.Amount / totalBaseValue * baseValueOfApportionee;
					}
					var roundedResult = apportionStrategy.Round(apportionedCharge);
					sumOfApportionedCharge += roundedResult;

					var apportionedResult = new BizOApportionedCharge(apportionee, roundedResult);
					all.Add(apportionedResult);
				}

				var difference = amountToApportion.Amount - sumOfApportionedCharge;
				all.Sort((x, y) => y.ApportionedCharge.CompareTo(x.ApportionedCharge));

				foreach (BizOApportionedCharge one in all)
				{
					var amountToAdjust = 0m;
					if (apportionStrategy.ShouldBackApportion(apportionChargeKey))
					{
						amountToAdjust = difference == 0m ? 0m : (difference > 0m ? apportionStrategy.UnitOfAmountToBackApportion : apportionStrategy.UnitOfAmountToBackApportion * -1);//because we round to 2 decimals above.
						difference -= amountToAdjust;
					}

					ZDecimal value = one.ApportionedCharge + amountToAdjust;

					Money amount = new Money(value, amountToApportion.Currency);
					AssignValueToApportionedCharge(apportionChargeKey, amount, one.BizO, chargeHolder);
				}
			}
		}

		void AssignValueToApportionedCharge(ApportionChargeKey apportionChargeKey, Money amount, IChargeApportionee apportionee, IChargeHolder chargeHolder)
		{
			JobComInvCharge[] charges = apportionee.ApportionedCharges.Find(apportionChargeKey);
			JobComInvCharge charge = null;

			foreach (JobComInvCharge apportionedCharge in charges)
			{
				if (!apportionedCharge.J7_IsSystem)
				{
					charge = apportionedCharge;
					break;
				}
			}

			if (charge == null)
			{
				charge = apportionee.ApportionedCharges.AddNew();
				SetApportionedChargeKeyValuesToApportionedCharge(charge, apportionChargeKey);
			}

			CalculateAndSetIsIncludedInLinesAndInvoices(charge, apportionChargeKey, apportionee);

			using (charge.GetValidationSuspender())
			{
				if (apportionChargeKey.IsFullApportionment || apportionee.ImmediateChargeHolderChildren.Length > 0)//invoice line charges are aggregated into an invoice
				{
					if (amount.Currency != null)
					{
						Money added = chargeHolder.CurrencyConverter.Add(charge.Money, amount);
						charge.J7_Amount = added.Amount;
						charge.J7_RX_NKCurrency = added.Currency.Code;
					}
				}
				else
				{
					charge.J7_Amount = amount.Amount;
					charge.J7_RX_NKCurrency = amount.Currency.Code;
				}

				var chargesApportioned = chargeHolder.Charges.Find(apportionChargeKey.ChargeKey);
				if (chargesApportioned.Length > 0)
				{
					charge.J7_PrepaidCollect = chargesApportioned[0].J7_PrepaidCollect;
				}
			}

			jobDeclaration.UpdateApportionmentProgress();
		}

		IChargeApportionee[] GetApportionees(ApportionChargeKey chargeKey, IChargeHolder chargeHolder)
		{
			var result = new List<IChargeApportionee>();
			foreach (IChargeApportionee apportionee in chargeHolder.AllApportionees)
			{
				if (ShouldThisChargeBeApportionedIntoThis(apportionee, chargeHolder, chargeKey))
				{
					result.Add(apportionee);
				}
			}
			return result.ToArray();
		}

		bool ShouldThisChargeBeApportionedIntoThis(IChargeApportionee destination, IChargeHolder origin, ApportionChargeKey chargeKey)
		{
			bool result = false;

			if (destination.IsValidToApportionTo && destination.AllApportionees.Length == 0)
			{
				result = chargeKey.IsFullApportionment || !DoesDestinationOrParentWhichIsNotOriginHaveThisCharge(destination, origin, chargeKey);
				if (result && origin.IsGroupInvoice)
				{
					var charge = jobDeclaration.IncoTermAndChargeFactory.GetCharge(chargeKey.ChargeKey.ChargeCode);

					if (charge != null && charge.ConsiderIncotermWhenAppoorting)
					{
						result = destination.CanThisChargeBeApportionedBasedOnIncoterm(chargeKey);
					}
				}
			}

			return result;
		}

		bool DoesDestinationOrParentWhichIsNotOriginHaveThisCharge(IChargeApportionee destination, IChargeHolder origin, ApportionChargeKey chargeKey)
		{
			bool result = DoesThisChargeHolderHaveThisCharge(destination, chargeKey);
			if (!result)
			{
				IChargeHolder parent = destination.ImmediateChargeHolderParent;

				while (parent != null && parent.PK != origin.PK)
				{
					result = DoesThisChargeHolderHaveThisCharge(parent, chargeKey);
					parent = parent.ImmediateChargeHolderParent;
					if (result)
					{
						break;
					}
				}
			}
			return result;
		}

		bool DoesThisChargeHolderHaveThisCharge(IChargeHolder chargeHolder, ApportionChargeKey chargeKey)
		{
			return chargeHolder != null && chargeHolder.Charges.HasChargeWithThisKey(chargeKey);
		}

		#endregion

		#region DefaultPercentageAndCalculateAmounts

		void DefaultPercentageAndCalculateAmounts(JobComInvCharge chargeWithPercentage, IChargeApportionee[] apportionees)
		{
			foreach (IChargeApportionee apportionee in apportionees)
			{
				SetPercentageValues(apportionee, chargeWithPercentage);
			}
		}

		void SetPercentageValues(IChargeApportionee apportionee, JobComInvCharge chargeWithPercentage)
		{
			ApportionChargeKey apportionChargeKey = chargeWithPercentage.ApportionChargeKey;
			JobComInvCharge[] charges = apportionee.ApportionedCharges.Find(apportionChargeKey);
			JobComInvCharge charge = null;

			if (charges.Length > 0)
			{
				charge = charges[0];
			}

			if (charge == null)
			{
				charge = apportionee.ApportionedCharges.AddNew();
				SetApportionedChargeKeyValuesToApportionedCharge(charge, apportionChargeKey);
				charge.J7_ChargeDescription = chargeWithPercentage.J7_ChargeDescription;
			}

			apportionee.CalculateAmountBasedOnPercentage(charge);
			charge.J7_Amount = ZArchitecture.Core.Utilities.Round(charge.J7_Amount, 2);
			charge.J7_PrepaidCollect = chargeWithPercentage.J7_PrepaidCollect;
			CalculateAndSetIsIncludedInLinesAndInvoices(charge, apportionChargeKey, apportionee);
		}

		void SetApportionedChargeKeyValuesToApportionedCharge(JobComInvCharge charge, ApportionChargeKey apportionChargeKey)
		{
			using (charge.GetValidationSuspender())
			{
				charge.J7_ChargeType = apportionChargeKey.ChargeKey.ChargeCode;
				charge.J7_DistributeBy = apportionChargeKey.DistributeBy;
				charge.J7_IsDutiable = apportionChargeKey.ChargeKey.IsDutiable;
				charge.J7_IsGSTApplicable = apportionChargeKey.ChargeKey.IsVATible;
				charge.J7_IsStatisticalValueApplicable = apportionChargeKey.ChargeKey.IsStatisticalValueApplicable;
				charge.J7_FullOrPartialApportionment = apportionChargeKey.ApportionType;
				charge.J7_Percentage = apportionChargeKey.Percentage;
				charge.J7_AdjustedCharge = apportionChargeKey.IsAdjustedCharge;
				charge.J7_ChargeDescription = apportionChargeKey.ChargeDesc;
				charge.J7_IsSystem = apportionChargeKey.IsSystem;
			}
		}

		void CalculateAndSetIsIncludedInLinesAndInvoices(JobComInvCharge charge, ApportionChargeKey apportionChargeKey, IChargeApportionee apportionee)
		{
			using (charge.GetValidationSuspender())
			{
				if (apportionChargeKey.IsIncludedInITOT != GroupIsIncludedInLinesOptionList.Codes.NotApplicable)
				{
					charge.J7_IsIncludedInITOT = apportionChargeKey.IsIncludedInITOT == GroupIsIncludedInLinesOptionList.Codes.Yes;
				}
				else
				{
					charge.J7_IsIncludedInITOT = apportionee.CanThisChargeBeApportionedBasedOnIncoterm(apportionChargeKey);
				}

				if (apportionChargeKey.IsIncludedInInvoice != GroupIsIncludedInLinesOptionList.Codes.NotApplicable)
				{
					charge.J7_IsNotIncludedInInvoice = apportionChargeKey.IsIncludedInInvoice == GroupIsIncludedInLinesOptionList.Codes.No;
				}
				else
				{
					charge.J7_IsNotIncludedInInvoice = !apportionee.CanThisChargeBeApportionedBasedOnIncoterm(apportionChargeKey);
				}
			}
		}

		#endregion

		#region AggregateAmountsFromLinesToInvoices

		void AggregateAmountsFromLinesToInvoices()
		{
			foreach (IChargeApportionee invoice in jobDeclaration.Invoices)
			{
				Dictionary<ApportionChargeKey, Money> aggregated = new LineChargesAggregator().GetTotal(invoice, jobDeclaration.LineChargeApportioneeComparer);

				foreach (ApportionChargeKey chargeKey in aggregated.Keys)
				{
					Money amount;

					if (aggregated.TryGetValue(chargeKey, out amount))
					{
						if (invoice.Charges.HasChargeWithPercentage(chargeKey))
						{
							AssignValueToChargeWithPercentage(chargeKey, amount, invoice);
						}
						else if (!DoesThisChargeHolderHaveThisCharge(invoice, chargeKey))
						{
							AssignValueToApportionedCharge(chargeKey, amount, invoice, invoice);
						}
					}
				}
			}
		}

		void AssignValueToChargeWithPercentage(ApportionChargeKey chargeKey, Money amount, IChargeHolder invoice)
		{
			if (amount.Currency != null)
			{
				JobComInvCharge[] charges = invoice.Charges.Find(chargeKey);
				JobComInvCharge result = null;
				if (charges.Length > 0)
				{
					result = charges[0];
				}

				if (result != null)
				{
					result.J7_Amount = amount.Amount;
					result.J7_RX_NKCurrency = amount.Currency.Code;
				}
			}
		}

		#endregion

		#region DeleteEmptyApportionedCharges

		void DeleteEmptyApportionedCharges()
		{
			foreach (IChargeHolder chargeHolder in jobDeclaration.ChargeHolders)
			{
				foreach (IChargeApportionee chargeApportionee in chargeHolder.AllApportionees)
				{
					chargeApportionee.ApportionedCharges.DeleteEmptyApportionedCharges();
				}
			}
		}

		#endregion

		#region Get ratios
		ZDecimal GetTotalBaseValue(IChargeApportionee[] apportionees, string distributeBy, CurrencyConverter currencyConverter)
		{
			ZDecimal result = 0m;
			foreach (IChargeApportionee apportionee in apportionees)
			{
				result += GetBaseValue(apportionee, distributeBy, currencyConverter);
			}
			return result;
		}

		ZDecimal GetBaseValue(IChargeApportionee apportionee, string distributeBy, CurrencyConverter currencyConverter)
		{
			return apportionee.GetBaseValueToApportionOn(currencyConverter, distributeBy);
		}

		#endregion

		#region Implementation

		#region BizOApportionedCharge
		class BizOApportionedCharge
		{
			public BizOApportionedCharge(IChargeApportionee bizO, ZDecimal apportionedCharge)
			{
				this.BizO = bizO;
				this.ApportionedCharge = apportionedCharge;
			}
			public readonly IChargeApportionee BizO;
			public readonly ZDecimal ApportionedCharge;
		}
		#endregion

		readonly IApportionInvoiceHolder jobDeclaration;
		readonly IApportionStrategy apportionStrategy;
		#endregion
	}
}
