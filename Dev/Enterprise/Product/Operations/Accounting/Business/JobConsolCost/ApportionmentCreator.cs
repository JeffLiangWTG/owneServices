using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	static class ApportionmentCreator
	{
		const decimal Epsilon = 0.00001m;

		internal static void Apportion(this IApportionedChargesHeader header, SchemaColumn apportionColumn, ZDecimal totalAmount, SchemaColumn agentApportionColumn, ZDecimal agentTotalAmount)
		{
			header.Apportion(apportionColumn != null ? apportionColumn.Name : string.Empty,
				totalAmount,
				agentApportionColumn != null ? agentApportionColumn.Name : string.Empty,
				agentTotalAmount);
		}

		internal static void Apportion(this IApportionedChargesHeader header, ZString apportionColumnName, ZDecimal totalAmount, ZString agentApportionColumnName, ZDecimal agentTotalAmount)
		{
			if (apportionColumnName.IsEmpty)
			{
				throw new ArgumentException(Res.GetString("653e4e45-864b-4654-8a60-cfae3813a7cd", "Column name to apportion amount should not be empty"));
			}

			var allCharges = header.Charges;
			var totalMeasures = GetTotalMeasures(header, allCharges, apportionColumnName);
			var totalMeasuresForAgent = GetTotalMeasuresForAgent(header, allCharges, totalMeasures, agentApportionColumnName);
			var freeSpaceByCharge = header.ApportionmentMethod == AllocationMethod.FreeSpaceContribution
				? GetFreeSpaceContributionToConsolChargeable(allCharges, header.FreeSpace)
				: null;

			foreach (IApportionedCharge charge in allCharges.Where(c => c != null))
			{
				using ((charge as Charge)?.SuspendSplittingApportionAmountChangeIsUsedForApportionment())
				using ((charge as ApportionSplitCharge)?.SuspendSplittingApportionAmountChangeIsUsedForApportionment())
				{
					if (header.Currency != null && header.Currency.RX_Code != charge.CurrencyCode)
					{
						charge.CurrencyCode = header.Currency.RX_Code;
					}

					var apportionedValue = 0m;
					var apportionedValueForAgent = 0m;
					if (charge.IsUsedForApportionment)
					{
						var freeSpace = freeSpaceByCharge?[charge] ?? 0m;
						apportionedValue = GetApportionedValue(header, charge, apportionColumnName, totalAmount, totalMeasures, freeSpace);

						if (!agentApportionColumnName.IsEmpty)
						{
							apportionedValueForAgent = GetApportionedValue(header, charge, agentApportionColumnName, agentTotalAmount, totalMeasuresForAgent, freeSpace);
						}
					}
					var businessObject = (BusinessObject)charge;
					businessObject[apportionColumnName] = new ZDecimal(apportionedValue);
					if (!agentApportionColumnName.IsEmpty)
					{
						businessObject[agentApportionColumnName] = apportionedValueForAgent;
					}
				}
			}

			header.PushUnApportionedAmountBasedOnRepresentation(totalAmount, apportionColumnName);
		}

		#region Is Apportionment Method Per Chargeable Unit

		internal static bool IsApportionmentMethodPerChargeableUnit(ZString? apportionmentMethod)
		{
			var allocationPerChargeableUnit = new[]
			{
					AllocationMethod.ChargeableUnits,
					AllocationMethod.GrossWeight,
					AllocationMethod.GrossVolume,
					AllocationMethod.CapacityPerContainer,
					AllocationMethod.FreeSpaceContribution
			};

			return allocationPerChargeableUnit.Contains((string)apportionmentMethod);
		}

		#endregion

		#region Get Total for Apportionment Method

		[SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		static ZDecimal GetTotalMeasures(IApportionedChargesHeader header, IApportionedCharge[] allCharges, ZString apportionColumnName)
		{
			switch (header.ApportionmentMethod)
			{
				case AllocationMethod.Shipment:
					return allCharges.Count(charge => charge != null && charge.IsUsedForApportionment);

				case AllocationMethod.Revenue:
					return allCharges.GetTotal(charge => charge.InvoicingJob.GetRevenueForChargeCode(header.ChargeCode));

				case AllocationMethod.ChargeableUnits:
					return allCharges.GetTotal(charge => charge.ChargeableUnits);

				case AllocationMethod.GrossWeight:
					return allCharges.GetTotal(charge => charge.GrossWeight);

				case AllocationMethod.GrossVolume:
					return allCharges.GetTotal(charge => charge.GrossVolume);

				case AllocationMethod.ContainerCount:
					return allCharges.GetTotal(charge => (ZDecimal)charge.ContainerCount);

				case AllocationMethod.OuterPackTotal:
					return allCharges.GetTotal(charge => (ZDecimal)charge.OuterPackTotal);

				case AllocationMethod.TwentyFootEquivalentUnit:
					return allCharges.GetTotal(charge => charge.TEUCount);

				case AllocationMethod.Manual:
					return apportionColumnName.IsEmpty
						? 0
						: allCharges.GetTotal(charge => (ZDecimal)((BusinessObject)charge)[apportionColumnName]);

				default:
					return 0;
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		static ZDecimal GetAmountToApportionBy(IApportionedChargesHeader header, IApportionedCharge charge, ZString apportionColumnName)
		{
			var chargeBusinessObject = charge as BusinessObject;

			if (apportionColumnName != JobChargeSchema.Constants.JR_OSCostAmt && charge is JobCharge)
			{
				// Balancing for everything else is done based on the OS Cost
				return (ZDecimal)chargeBusinessObject[JobChargeSchema.Constants.JR_OSCostAmt];
			}

			switch (header.ApportionmentMethod)
			{
				case AllocationMethod.Shipment:
					return charge.IsUsedForApportionment ? 1 : 0;

				case AllocationMethod.Revenue:
					return charge.InvoicingJob.GetRevenueForChargeCode(header.ChargeCode);

				case AllocationMethod.ChargeableUnits:
					return charge.ChargeableUnits;

				case AllocationMethod.GrossWeight:
					return charge.GrossWeight;

				case AllocationMethod.GrossVolume:
					return charge.GrossVolume;

				case AllocationMethod.ContainerCount:
					return (ZDecimal)charge.ContainerCount;

				case AllocationMethod.OuterPackTotal:
					return (ZDecimal)charge.OuterPackTotal;

				case AllocationMethod.TwentyFootEquivalentUnit:
					return charge.TEUCount;

				case AllocationMethod.Manual:
					if (apportionColumnName.IsEmpty)
					{
						return 0;
					}
					else
					{
						return (ZDecimal)chargeBusinessObject[apportionColumnName];
					}

				default:
					return 0;
			}
		}

		static ZDecimal GetTotal(this IApportionedCharge[] allCharges, Func<IApportionedCharge, ZDecimal> getAmount)
		{
			ZDecimal total = 0;
			foreach (var charge in allCharges.Where(charge => charge != null && charge.IsUsedForApportionment))
			{
				total += getAmount(charge);
			}

			return total;
		}

		#endregion

		#region Get Total for Apportionment Method For Agent

		static ZDecimal GetTotalMeasuresForAgent(IApportionedChargesHeader header, IApportionedCharge[] allCharges, ZDecimal totalOfApportionments, ZString agentApportionColumnName)
		{
			switch (header.ApportionmentMethod)
			{
				case AllocationMethod.Manual:
					return agentApportionColumnName.IsEmpty
						? 0
						: allCharges.GetTotal(charge => (ZDecimal)((BusinessObject)charge)[agentApportionColumnName]);

				default:
					return totalOfApportionments;
			}
		}

		#endregion

		#region Get Apportioned Value

		[SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		static ZDecimal GetApportionedValue(IApportionedChargesHeader header, IApportionedCharge charge, ZString apportionColumnName, ZDecimal totalAmount, ZDecimal totalMeasures, ZDecimal freeSpace)
		{
			ZDecimal result = 0;

			switch (header.ApportionmentMethod)
			{
				case AllocationMethod.Shipment:
					if (totalMeasures > 0)
					{
						result = totalAmount / totalMeasures;
					}
					break;

				case AllocationMethod.Revenue:
					if (totalMeasures != 0)
					{
						result = charge.InvoicingJob.GetRevenueForChargeCode(header.ChargeCode) * totalAmount / totalMeasures;
					}
					break;

				case AllocationMethod.ChargeableUnits:
					if (totalMeasures != 0)
					{
						result = totalAmount * (charge.ChargeableUnits / totalMeasures);
					}
					break;

				case AllocationMethod.GrossWeight:
					if (totalMeasures != 0)
					{
						result = totalAmount * (charge.GrossWeight / totalMeasures);
					}
					break;

				case AllocationMethod.GrossVolume:
					if (totalMeasures != 0)
					{
						result = totalAmount * (charge.GrossVolume / totalMeasures);
					}
					break;

				case AllocationMethod.ContainerCount:
					if (totalMeasures != 0)
					{
						result = totalAmount * (charge.ContainerCount / totalMeasures);
					}
					break;

				case AllocationMethod.OuterPackTotal:
					if (totalMeasures != 0)
					{
						result = totalAmount * (charge.OuterPackTotal / totalMeasures);
					}
					break;

				case AllocationMethod.TwentyFootEquivalentUnit:
					if (totalMeasures != 0)
					{
						result = totalAmount * (charge.TEUCount / totalMeasures);
					}
					break;

				case AllocationMethod.CapacityPerContainer:
					result = totalAmount * charge.GetContainersCostShare();
					break;

				case AllocationMethod.FreeSpaceContribution:
					result = totalAmount * freeSpace;
					break;

				case AllocationMethod.Manual:
					if (totalMeasures != 0)
					{
						result = totalAmount * ((ZDecimal)((JobCharge)charge)[apportionColumnName] / totalMeasures);
					}
					break;
			}

			return AccountingUtils.Round(result, header.Currency);
		}

		#endregion

		#region Push Unapportioned Amount Based On Representation

		internal static void PushUnApportionedAmountBasedOnRepresentation(this IApportionedChargesHeader header, ZDecimal totalAmount, SchemaColumn apportionColumn, IApportionedCharge[] chargesToApportion)
		{
			header.PushUnApportionedAmountBasedOnRepresentation(totalAmount, apportionColumn.Name, chargesToApportion);
		}

		internal static void PushUnApportionedAmountBasedOnRepresentation(this IApportionedChargesHeader header, ZDecimal totalAmount, SchemaColumn apportionColumn)
		{
			header.PushUnApportionedAmountBasedOnRepresentation(totalAmount, apportionColumn.Name);
		}

		internal static bool HasUnApportionedAmount(this IApportionedChargesHeader header, ZDecimal totalAmount, ZString apportionColumnName)
		{
			var result = false;

			if (header != null && header.Charges.Any() && header.Currency != null)
			{
				var charges = from charge in header.Charges where charge != null select charge;
				result = (LeftToApportionAmount(charges, totalAmount, apportionColumnName) != ZDecimal.Zero);
			}

			return result;
		}

		internal static void PushUnApportionedAmountBasedOnRepresentation(this IApportionedChargesHeader header, ZDecimal totalAmount, ZString apportionColumnName, IApportionedCharge[] chargesToApportion = null)
		{
			var charges = chargesToApportion ??
				(header != null && header.Currency != null ?
					header.Charges.Where(x => x != null && x.IsUsedForApportionment).ToArray()
					: Array.Empty<IApportionedCharge>());
			if (charges.Any())
			{
				// Work out the total amount we still need to apportion
				var leftToApportion = LeftToApportionAmount(charges, totalAmount, apportionColumnName); //totalAmount - totalApportioned;

				if (leftToApportion != ZDecimal.Zero)
				{
					var totalApportioned = totalAmount - leftToApportion;

					// Work out how much to add/subtract from each charge
					// This should always be the minimum unit of currency, either positive or negative
					var minAmount = header.GetMinCurrencyAmount(apportionColumnName);
					var amountToChangeBy = minAmount * Math.Sign(leftToApportion);

					// If the initial apportioning did nothing (which can happen if the things to apportion by are all 0) then split it as evenly as possible
					if (totalAmount != 0m && totalApportioned == 0m)
					{
						header.SplitUnApportionedAmountEvenly(charges, leftToApportion, apportionColumnName);

						totalApportioned = charges.Sum(x => (ZDecimal)((BusinessObject)x)[apportionColumnName]);
						leftToApportion = totalAmount - totalApportioned;
						amountToChangeBy = minAmount * Math.Sign(leftToApportion);
					}

					// work out what totals to use when working out representation amounts
					// Normally we use get total measures, the only difference is if the apportioning is manual and the tax is being apportioned
					ZDecimal totalToApportionBy;
					if (apportionColumnName == JobChargeSchema.Constants.JR_OSCostAmt || !charges.All(x => x is JobCharge))
					{
						totalToApportionBy = GetTotalMeasures(header, header.Charges, apportionColumnName);
					}
					else
					{
						// We want to apportion based on the base cost
						totalToApportionBy = header.Charges.GetTotal(charge => (ZDecimal)((BusinessObject)charge)[JobChargeSchema.Constants.JR_OSCostAmt]);
					}

					// Work out the amount of representation error if we added/subtracted a single unit from each charge
					var chargesWithRepresentation = header.CalculateRepresentationErrorForAllCharges(charges, apportionColumnName, totalToApportionBy, totalAmount, amountToChangeBy);

					// Order by the least total representation error
					var sortedCharges = from chargeWithRepresentation in chargesWithRepresentation orderby Math.Abs(chargeWithRepresentation.Value) ascending select (BusinessObject)chargeWithRepresentation.Key;

					foreach (var charge in sortedCharges)
					{
						if (leftToApportion == ZDecimal.Zero)
						{
							// We've apportioned everything
							break;
						}

						// Add/Subtract one unit from each charge in turn until there's nothing left to apportion
						var currentAmount = (ZDecimal)charge[apportionColumnName];
						charge[apportionColumnName] = new ZDecimal(currentAmount + amountToChangeBy);
						leftToApportion -= amountToChangeBy;
					}
				}
			}
		}

		static ZDecimal LeftToApportionAmount(IEnumerable<IApportionedCharge> charges, ZDecimal totalAmount, ZString apportionColumnName)
		{
			var totalApportioned = charges.Sum(x => (ZDecimal)((BusinessObject)x)[apportionColumnName]);

			// Work out the total amount we still need to apportion
			var leftToApportion = totalAmount - totalApportioned;

			return leftToApportion;
		}

		static void SplitUnApportionedAmountEvenly(this IApportionedChargesHeader header, IEnumerable<IApportionedCharge> charges, ZDecimal amountToApportion, ZString apportionColumnName)
		{
			if (!charges.Any())
			{
				return;
			}

			var amountForEachCharge = AccountingUtils.Round(amountToApportion / charges.Count(), header.Currency);

			foreach (BusinessObject charge in charges)
			{
				var currentValue = (ZDecimal)charge[apportionColumnName];
				charge[apportionColumnName] = new ZDecimal(currentValue + amountForEachCharge);
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		static ZDecimal GetMinCurrencyAmount(this IApportionedChargesHeader header, ZString apportionColumnName)
		{
			switch (apportionColumnName)
			{
				case JobChargeSchema.Constants.JR_DeclaredOSCostAmt:
				case JobChargeSchema.Constants.JR_OSCostAmt:
				case JobChargeSchema.Constants.JR_OSCostGSTAmt:
				case JobCharge.Schema.JR_OSCostGSTAmt_Calc:
				case JobChargeSchema.Constants.JR_OSCostWHTAmt:
				case JobChargeSchema.Constants.JR_OSSellAmt:
				case JobChargeSchema.Constants.JR_OSSellWHTAmt:
				case InvoicingLineBase.Schema.GSTInclusiveAmount:
					return header.Currency.CurrencyMinAmount();

				case JobChargeSchema.Constants.JR_LocalCostAmt:
				case JobChargeSchema.Constants.JR_LocalSellAmt:
					return GlbCompany.CurrentCompany.LocalCurrency.CurrencyMinAmount();

#if DEBUG
				case DummyBizoSchema.Constants.Z0_AnotherDecimal:
				case DummyBizoSchema.Constants.Z0_Decimal:
					if (Globals.IsTest)
					{
						return GlbCompany.CurrentCompany.LocalCurrency.CurrencyMinAmount();
					}
					else
					{
						throw new ArgumentException(string.Format(Culture.Current, "{0} can only be used in testing", apportionColumnName));
					}
#endif
				default:
					throw new ArgumentException(string.Format(Culture.Current, "Apportioning must be of local or overseas amount, column name: '{0}'", apportionColumnName));
			}
		}

		static List<KeyValuePair<IApportionedCharge, ZDecimal>> CalculateRepresentationErrorForAllCharges(this IApportionedChargesHeader header, IEnumerable<IApportionedCharge> charges,
																											ZString apportionColumnName, ZDecimal apportionByTotal, ZDecimal toApportionTotal, ZDecimal amountToChangeBy)
		{
			var chargesWithRepresentation = new List<KeyValuePair<IApportionedCharge, ZDecimal>>();

			foreach (var charge in charges)
			{
				var apportionedAmount = (ZDecimal)((BusinessObject)charge)[apportionColumnName] + amountToChangeBy;

				var apportionByAmount = GetAmountToApportionBy(header, charge, apportionColumnName);

				var representationError = CalculateRepresentationError(apportionByAmount, apportionedAmount, apportionByTotal, toApportionTotal);

				var chargeWithRepresentation = new KeyValuePair<IApportionedCharge, ZDecimal>(charge, representationError);

				chargesWithRepresentation.Add(chargeWithRepresentation);
			}

			return chargesWithRepresentation;
		}

		static ZDecimal CalculateRepresentationError(ZDecimal apportionByAmount, ZDecimal apportionedAmount, ZDecimal apportionByTotal, ZDecimal toApportionTotal)
		{
			if (toApportionTotal == ZDecimal.Zero || apportionByTotal == ZDecimal.Zero)
			{
				return ZDecimal.Zero;
			}
			else
			{
				// Multiply each value's amount by the other's total so that we avoid any divison precision problems
				return (apportionedAmount / toApportionTotal) - (apportionByAmount / apportionByTotal);
			}
		}

		internal static ZDecimal CurrencyMinAmount(this RefCurrency currency)
		{
			Argument.NotNull(currency, "currency");

			ZDecimal result = 1m;
			for (int i = 0; i < currency.Decimals; i++)
			{
				result = result / 10m;
			}

			return result;
		}

		#endregion

		#region Get Free Space Contribution to Consol Chargeable

		static Dictionary<IApportionedCharge, decimal> GetFreeSpaceContributionToConsolChargeable(IApportionedCharge[] allCharges, decimal headerFreeSpace)
		{
			var result = new Dictionary<IApportionedCharge, decimal>(allCharges.Length);

			var totalExcessActualVolumeWeight = 0m;
			var totalExcessChargeableVolumeWeight = 0m;
			foreach (var charge1 in allCharges)
			{
				totalExcessActualVolumeWeight += charge1.ExcessActualVolumeWeight;
				totalExcessChargeableVolumeWeight += charge1.ExcessChargeableVolumeWeight;
			}

			var totalChargeableLessFreeSpace = 0m;
			foreach (var charge2 in allCharges)
			{
				var contributionToExcess = 0m;
				if (totalExcessActualVolumeWeight != 0)
				{
					contributionToExcess += charge2.ExcessActualVolumeWeight / totalExcessActualVolumeWeight;
				}
				if (totalExcessChargeableVolumeWeight != 0)
				{
					contributionToExcess += charge2.ExcessChargeableVolumeWeight / totalExcessChargeableVolumeWeight;
				}
				contributionToExcess /= 2;

				var freeSpaceContribution = headerFreeSpace * contributionToExcess;
				var chargeableLessFreeSpace = charge2.ChargeableUnits - freeSpaceContribution;

				if (charge2.IsUsedForApportionment)
				{
					totalChargeableLessFreeSpace += chargeableLessFreeSpace;
				}

				result[charge2] = chargeableLessFreeSpace;
			}

			if (Math.Abs(totalChargeableLessFreeSpace) <= Epsilon)
			{
				foreach (var charge3 in allCharges)
				{
					result[charge3] = 0;
				}
			}
			else
			{
				foreach (var charge4 in allCharges)
				{
					result[charge4] /= totalChargeableLessFreeSpace;
				}
			}

			return result;
		}

		#endregion
	}
}
