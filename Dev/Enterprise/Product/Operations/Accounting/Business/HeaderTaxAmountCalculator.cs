using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public class HeaderTaxAmountCalculator
	{
		public HeaderTaxAmountCalculator(List<IReceivablesTaxAmountCalculation> charges, int osRoundingPrecision)
		{
			OSRoundingPrecision = osRoundingPrecision;
			Charges = charges.Where(x => !x.ShouldExclude).ToList();
			isPostingReceivableChargesForTaxCalculation = (Charges.FirstOrDefault() as BusinessObject)?.Factory.HasContext(BusinessContext.PostingReceivableChargesForTaxCalculation) ?? false;
		}

		public HeaderTaxAmountCalculator(List<IReceivablesTaxAmountCalculation> charges, RefCurrency currency)
			: this(charges, (currency == null ? -1 : currency.Decimals))
		{
		}

		readonly ZBool isPostingReceivableChargesForTaxCalculation;

		public void AdjustAgainstLargestCharge(ZGuid companyPK)
		{
			if (!CanAdjustCharges())
			{
				return;
			}

			var totalsByTaxRate = GetTotalExTaxAndTaxAmountsByTaxRate();
			if (totalsByTaxRate == null)
			{
				return;
			}

			var useLocalExTaxAmountToCalculateLocalTax = TaxAmountCalculator.UseLocalExTaxAmountToCalculateLocalTax(companyPK);
			foreach (var totalByTaxRate in totalsByTaxRate)
			{
				var largestLine = GetLineWithLargestExTaxAmount(totalByTaxRate.TaxId);

				if ((isPostingReceivableChargesForTaxCalculation || totalByTaxRate.ShouldAdjustExtraTaxAmount) && largestLine == null)
				{
					largestLine = GetLineWithLargestExTaxAmount(totalByTaxRate.TaxId, true);
				}

				if (largestLine != null)
				{
					var osTaxAdjustment = totalByTaxRate.OsTaxCalculation.GetAdjustmentTaxAmount();
					largestLine.AdjustOsTaxAmount(osTaxAdjustment);
					if (totalByTaxRate.ShouldAdjustExtraTaxAmount)
					{
						var osExtraTaxAdjustment = totalByTaxRate.OsTaxCalculation.GetAdjustmentExtraTaxAmount();
						largestLine.AdjustOsExtraTaxAmount(osExtraTaxAdjustment);
					}

					if (useLocalExTaxAmountToCalculateLocalTax)
					{
						var localTaxAdjustment = totalByTaxRate.LocalTaxCalculation.GetAdjustmentTaxAmount();
						largestLine.AdjustLocalTaxAmount(localTaxAdjustment);
						if (totalByTaxRate.ShouldAdjustExtraTaxAmount)
						{
							var localExtraTaxAdjustment = totalByTaxRate.LocalTaxCalculation.GetAdjustmentExtraTaxAmount();
							largestLine.AdjustLocalExtraTaxAmount(localExtraTaxAdjustment);
						}
					}
				}

				if (isPostingReceivableChargesForTaxCalculation)
				{
					ResetOSTaxAmountIfHasZeroLocalTaxAmount(largestLine);
				}
			}

			if (isPostingReceivableChargesForTaxCalculation)
			{
				totalsByTaxRate = GetTotalExTaxAndTaxAmountsByTaxRate(false);
				foreach (var totalByTaxRate in totalsByTaxRate)
				{
					var osTaxAdjustment = totalByTaxRate.OsTaxCalculation.GetAdjustmentTaxAmount();
					var largestLine = GetLineWithLargestExTaxAmount(totalByTaxRate.TaxId) ?? GetLineWithLargestExTaxAmount(totalByTaxRate.TaxId, true);
					if (!largestLine.LocalTaxAmount.IsEmpty)
					{
						largestLine.AdjustOsTaxAmount(osTaxAdjustment, true); // We pass true here because we should only change OSTaxAmount at last adjustment step.
					}
				}
			}
		}

		bool CanAdjustCharges() => OSRoundingPrecision > -1 && Charges != null && Charges.Count > 0;

		void ResetOSTaxAmountIfHasZeroLocalTaxAmount(IReceivablesTaxAmountCalculation line)
		{
			if (!line.OsTaxAmount.IsEmpty && line.LocalTaxAmount.IsEmpty)
			{
				line.AdjustOsTaxAmount(-line.OsTaxAmount);
			}
		}

		public void AdjustAgainstAllCharges(ZGuid companyPK)
		{
			if (!CanAdjustCharges())
			{
				return;
			}

			var totalsByTaxRate = GetTotalExTaxAndTaxAmountsByTaxRate();

			var useLocalExTaxAmountToCalculateLocalTax = TaxAmountCalculator.UseLocalExTaxAmountToCalculateLocalTax(companyPK);
			foreach (var totalByTaxRate in totalsByTaxRate)
			{
				DistributeOsAdjustmentToAllCharges(totalByTaxRate);
				if (useLocalExTaxAmountToCalculateLocalTax)
				{
					DistributeLocalAdjustmentToAllCharges(totalByTaxRate);
				}
			}

			if (isPostingReceivableChargesForTaxCalculation)
			{
				foreach (var line in Charges)
				{
					ResetOSTaxAmountIfHasZeroLocalTaxAmount(line);
				}

				totalsByTaxRate = GetTotalExTaxAndTaxAmountsByTaxRate(false);
				foreach (var totalByTaxRate in totalsByTaxRate)
				{
					DistributeOsAdjustmentToAllCharges(totalByTaxRate);
				}
			}
		}

		void DistributeLocalAdjustmentToAllCharges(TaxRateGroupingWithTotalAmounts totalByTaxRate)
		{
			if (totalByTaxRate.TaxId.IsIndiaStateTax)
			{
				DistributeGstAndExtraTaxAdjustmentToAllCharges(totalByTaxRate.LocalTaxCalculation);
			}
			else
			{
				DistributeTaxAdjustmentToAllCharges(totalByTaxRate.LocalTaxCalculation);
			}
		}

		void DistributeOsAdjustmentToAllCharges(TaxRateGroupingWithTotalAmounts totalByTaxRate)
		{
			if (totalByTaxRate.TaxId.IsIndiaStateTax)
			{
				DistributeGstAndExtraTaxAdjustmentToAllCharges(totalByTaxRate.OsTaxCalculation);
			}
			else
			{
				DistributeTaxAdjustmentToAllCharges(totalByTaxRate.OsTaxCalculation);

				if (totalByTaxRate.ShouldAdjustExtraTaxAmount)
				{
					var largestLine = GetLineWithLargestExTaxAmount(totalByTaxRate.TaxId, true);
					var extraTaxAdjustment = totalByTaxRate.OsTaxCalculation.GetAdjustmentExtraTaxAmount();
					largestLine.AdjustOsExtraTaxAmount(extraTaxAdjustment);
				}
			}
		}

		void DistributeTaxAdjustmentToAllCharges(TotalTaxCalculation totalTaxCalculation)
		{
			var taxRate = totalTaxCalculation.TaxId;
			var totalTaxAdjustment = totalTaxCalculation.GetAdjustmentTaxAmount();

			if (totalTaxAdjustment.IsEmpty)
			{
				return;
			}

			var lines = GetLinesForTaxRate(taxRate);
			var taxCalculationAdaptor = totalTaxCalculation.TaxCalculationAdaptor;
			var totalTaxAmount = lines.Sum(l => taxCalculationAdaptor.GetTaxAmount(l));

			if (totalTaxAmount != 0m)
			{
				var remainingAdjustmentAmount = totalTaxAdjustment;

				foreach (var line in lines)
				{
					var taxAmount = taxCalculationAdaptor.GetTaxAmount(line);
					var proportionOfTotal = taxAmount / totalTaxAmount;
					var adjustmentForLine = Utilities.Round(totalTaxAdjustment * proportionOfTotal, totalTaxCalculation.RoundingPrecision);

					taxCalculationAdaptor.AdjustTaxAmount(line, adjustmentForLine);

					remainingAdjustmentAmount -= adjustmentForLine;
				}

				if (remainingAdjustmentAmount != 0m)
				{
					var largestLine = GetLineWithLargestExTaxAmount(taxRate);
					taxCalculationAdaptor.AdjustTaxAmount(largestLine, remainingAdjustmentAmount);
				}
			}
			else if (isPostingReceivableChargesForTaxCalculation)
			{
				var largestLine = GetLineWithLargestExTaxAmount(taxRate, totalTaxCalculation.IsRecalculation);
				if (largestLine != null)
				{
					taxCalculationAdaptor.AdjustTaxAmount(largestLine, totalTaxAdjustment);
				}
			}
		}

		void DistributeGstAndExtraTaxAdjustmentToAllCharges(TotalTaxCalculation totalTaxCalculation)
		{
			var taxRate = totalTaxCalculation.TaxId;
			var totalGstAdjustment = totalTaxCalculation.GetAdjustmentGSTTaxAmount();
			var totalExtraTaxAdjustment = totalTaxCalculation.GetAdjustmentExtraTaxAmount();

			if (totalGstAdjustment.IsEmpty && totalExtraTaxAdjustment.IsEmpty)
			{
				return;
			}

			var lines = GetLinesForTaxRate(taxRate);
			var taxCalculationAdaptor = totalTaxCalculation.TaxCalculationAdaptor;
			var totalGstAmount = lines.Sum(l => taxCalculationAdaptor.GetGSTAmount(l));
			var totalExtraTaxAmount = lines.Sum(l => taxCalculationAdaptor.GetExtraTaxAmount(l));

			if (totalGstAmount != 0m || totalExtraTaxAmount != 0)
			{
				var remainingGstAdjustment = totalGstAdjustment;
				var remainingExtraTaxAmountAdjustment = totalExtraTaxAdjustment;

				foreach (var line in lines)
				{
					var gstAmount = remainingGstAdjustment == 0m ? ZDecimal.Zero : taxCalculationAdaptor.GetGSTAmount(line);
					var proportionOfTotalGst = gstAmount / totalGstAmount;
					var gstAdjustmentForLine = Utilities.Round(totalGstAdjustment * proportionOfTotalGst, totalTaxCalculation.RoundingPrecision);

					var extraTaxAmount = remainingExtraTaxAmountAdjustment == 0m ? ZDecimal.Zero : taxCalculationAdaptor.GetExtraTaxAmount(line);
					var proportionOfTotalExtraTax = extraTaxAmount / totalExtraTaxAmount;
					var extraTaxAdjustmentForLine = Utilities.Round(totalExtraTaxAdjustment * proportionOfTotalExtraTax, totalTaxCalculation.RoundingPrecision);

					taxCalculationAdaptor.AdjustTaxAmount(line, gstAdjustmentForLine + extraTaxAdjustmentForLine);

					remainingGstAdjustment -= gstAdjustmentForLine;
					remainingExtraTaxAmountAdjustment -= extraTaxAdjustmentForLine;
				}

				if (remainingGstAdjustment != 0m || remainingExtraTaxAmountAdjustment != 0m)
				{
					var largestLine = GetLineWithLargestExTaxAmount(taxRate);
					taxCalculationAdaptor.AdjustTaxAmount(largestLine, remainingGstAdjustment + remainingExtraTaxAmountAdjustment);
				}
			}
			else if (isPostingReceivableChargesForTaxCalculation)
			{
				var largestLine = GetLineWithLargestExTaxAmount(taxRate, totalTaxCalculation.IsRecalculation);
				if (largestLine != null)
				{
					taxCalculationAdaptor.AdjustTaxAmount(largestLine, totalGstAdjustment + totalExtraTaxAdjustment);
				}
			}
		}

		#region Implementation

		IEnumerable<TaxRateGroupingWithTotalAmounts> GetTotalExTaxAndTaxAmountsByTaxRate(bool shouldRecalculateLines = true)
		{
			if (shouldRecalculateLines)
			{
				foreach (var line in Charges)
				{
					line.RecalculateOsTaxAmount();
					if (isPostingReceivableChargesForTaxCalculation)
					{
						ResetOSTaxAmountIfHasZeroLocalTaxAmount(line);
					}
				}
			}

			var taxRateGrouping = from n in Charges.Where(x => x.GSTRate != null)
				group n by new { TaxId = n.GSTRate, Rate = n.Rate, EffectiveExtraRate = n.EffectiveExtraRate }
				into g
				select new TaxRateGroupingWithTotalAmounts
				{
					TaxId = g.Key.TaxId,
					OsTaxCalculation = new TotalTaxCalculation(g.Key.TaxId, g.Key.Rate, g.Key.EffectiveExtraRate,
						OSRoundingPrecision, shouldRecalculateLines, g.ToList(), new OsTaxCalculationAdaptor(!shouldRecalculateLines)),
					LocalTaxCalculation = new TotalTaxCalculation(g.Key.TaxId, g.Key.Rate, g.Key.EffectiveExtraRate,
						GlbCompany.CurrentCompany.LocalCurrency.Decimals, shouldRecalculateLines, g.ToList(), new LocalTaxCalculationAdaptor())
				};

			return taxRateGrouping;
		}

		List<IReceivablesTaxAmountCalculation> GetLinesForTaxRate(AccTaxRate taxRate)
		{
			ZGuid taxRatePK = taxRate != null ? taxRate.PK : ZGuid.Empty;
			List<IReceivablesTaxAmountCalculation> result = new List<IReceivablesTaxAmountCalculation>();

			foreach (IReceivablesTaxAmountCalculation charge in Charges)
			{
				ZGuid chargeGstRatePK = charge.GSTRate != null ? charge.GSTRate.PK : ZGuid.Empty;

				if (chargeGstRatePK == taxRatePK)
				{
					result.Add(charge);
				}
			}

			return result;
		}

		IReceivablesTaxAmountCalculation GetLineWithLargestExTaxAmount(AccTaxRate taxRate, bool ignoreEmptyOSTaxAmount = false)
		{
			List<IReceivablesTaxAmountCalculation> lines = GetLinesForTaxRate(taxRate);
			return GetLineWithLargestExTaxAmount(lines, ignoreEmptyOSTaxAmount);
		}

		IReceivablesTaxAmountCalculation GetLineWithLargestExTaxAmount(List<IReceivablesTaxAmountCalculation> lines, bool ignoreEmptyOSTaxAmount = false)
		{
			IReceivablesTaxAmountCalculation result = null;
			ZDecimal largestOSExTaxAmount = lines.Count > 0 ? (ZDecimal)Math.Abs(lines[0].OsExTaxAmount) : (ZDecimal)0;

			foreach (IReceivablesTaxAmountCalculation charge in lines)
			{
				ZDecimal chargeOSExTaxAmount = (ZDecimal)Math.Abs(charge.OsExTaxAmount);

				if (chargeOSExTaxAmount >= largestOSExTaxAmount && !(charge.OsTaxAmount.IsEmpty && !ignoreEmptyOSTaxAmount))
				{
					result = charge;
					largestOSExTaxAmount = chargeOSExTaxAmount;
				}
			}

			return result;
		}

		readonly int OSRoundingPrecision;

		readonly List<IReceivablesTaxAmountCalculation> Charges;

		class TaxRateGroupingWithTotalAmounts
		{
			public AccTaxRate TaxId { get; set; }
			public TotalTaxCalculation OsTaxCalculation { get; set; }
			public TotalTaxCalculation LocalTaxCalculation { get; set; }

			internal bool ShouldAdjustExtraTaxAmount => OsTaxCalculation.ShouldAdjustExtraTaxAmount ||
				LocalTaxCalculation.ShouldAdjustExtraTaxAmount;
		}

		class TotalTaxCalculation
		{
			internal TotalTaxCalculation(AccTaxRate taxId, ZDecimal rate, ZDecimal extraRate, int roundingPrecision, bool isRecalculation,
				IReadOnlyCollection<IReceivablesTaxAmountCalculation> taxCalculations, ITaxCalculationAdaptor taxCalculationAdaptor)
			{
				TaxId = taxId;
				Rate = rate;
				ExtraRate = extraRate;
				RoundingPrecision = roundingPrecision;
				IsRecalculation = isRecalculation;
				TaxCalculationAdaptor = taxCalculationAdaptor;
				ExTaxAmount = taxCalculations.Sum(t => taxCalculationAdaptor.GetExTaxAmount(t));
				TaxAmount = taxCalculations.Sum(t => taxCalculationAdaptor.GetTaxAmount(t));
				GstAmount = taxCalculations.Sum(t => taxCalculationAdaptor.GetGSTAmount(t));
				ExtraTaxAmount = taxCalculations.Sum(t => taxCalculationAdaptor.GetExtraTaxAmount(t));
			}

			internal AccTaxRate TaxId { get; }
			internal int RoundingPrecision { get; }
			internal bool IsRecalculation { get; }
			internal ITaxCalculationAdaptor TaxCalculationAdaptor { get; }
			internal ZDecimal Rate { get; }
			internal ZDecimal ExtraRate { get; }
			internal ZDecimal ExTaxAmount { get; }
			internal ZDecimal TaxAmount { get; }
			internal ZDecimal GstAmount { get; }
			internal ZDecimal ExtraTaxAmount { get; }
			// We only want to adjust extra tax if it is a special Tax that isn't calculated from tax amount (i.e. when tax amount is 0, Italy SPV tax is one example of such tax)
			internal bool ShouldAdjustExtraTaxAmount => TaxAmount.IsEmpty && !ExtraTaxAmount.IsEmpty;

			internal ZDecimal GetAdjustmentTaxAmount()
				=> GetAdjustment(TaxAmount, TaxAmountCalculator.GetTaxAmountFromExTaxAmount(ExTaxAmount, TaxId, Rate, ExtraRate, RoundingPrecision));

			internal ZDecimal GetAdjustmentGSTTaxAmount()
				=> GetAdjustment(GstAmount, TaxAmountCalculator.GetGSTAmount(ExTaxAmount, Rate));

			internal ZDecimal GetAdjustmentExtraTaxAmount()
				=> GetAdjustment(ExtraTaxAmount, TaxAmountCalculator.GetExtraTaxAmountFromExTaxAmount(ExTaxAmount, ExtraRate));

			ZDecimal GetAdjustment(ZDecimal originalAmount, ZDecimal adjustedAmount)
				=> Utilities.Round(adjustedAmount, RoundingPrecision) - originalAmount;
		}

		interface ITaxCalculationAdaptor
		{
			ZDecimal GetExTaxAmount(IReceivablesTaxAmountCalculation calculation);
			ZDecimal GetTaxAmount(IReceivablesTaxAmountCalculation calculation);
			ZDecimal GetGSTAmount(IReceivablesTaxAmountCalculation calculation);
			ZDecimal GetExtraTaxAmount(IReceivablesTaxAmountCalculation calculation);
			void AdjustTaxAmount(IReceivablesTaxAmountCalculation calculation, ZDecimal adjustmentAmount);
		}

		class OsTaxCalculationAdaptor : ITaxCalculationAdaptor
		{
			readonly bool shouldAdjustOsTaxOnly;

			public OsTaxCalculationAdaptor(bool shouldAdjustOsTaxOnly = false)
			{
				this.shouldAdjustOsTaxOnly = shouldAdjustOsTaxOnly;
			}

			public ZDecimal GetExTaxAmount(IReceivablesTaxAmountCalculation calculation) => calculation.OsExTaxAmount;
			public ZDecimal GetTaxAmount(IReceivablesTaxAmountCalculation calculation) => calculation.OsTaxAmount;
			public ZDecimal GetGSTAmount(IReceivablesTaxAmountCalculation calculation) => calculation.OsGSTAmount;
			public ZDecimal GetExtraTaxAmount(IReceivablesTaxAmountCalculation calculation) => calculation.OsExtraTaxAmount;
			public void AdjustTaxAmount(IReceivablesTaxAmountCalculation calculation, ZDecimal adjustmentAmount)
				=> calculation.AdjustOsTaxAmount(adjustmentAmount, shouldAdjustOsTaxOnly);
		}

		class LocalTaxCalculationAdaptor : ITaxCalculationAdaptor
		{
			public ZDecimal GetExTaxAmount(IReceivablesTaxAmountCalculation calculation) => calculation.LocalExTaxAmount;
			public ZDecimal GetTaxAmount(IReceivablesTaxAmountCalculation calculation) => calculation.LocalTaxAmount;
			public ZDecimal GetGSTAmount(IReceivablesTaxAmountCalculation calculation) => calculation.LocalGSTAmount;
			public ZDecimal GetExtraTaxAmount(IReceivablesTaxAmountCalculation calculation) => calculation.LocalExtraTaxAmount;
			public void AdjustTaxAmount(IReceivablesTaxAmountCalculation calculation, ZDecimal adjustmentAmount)
				=> calculation.AdjustLocalTaxAmount(adjustmentAmount);
		}

		#endregion
	}
}
