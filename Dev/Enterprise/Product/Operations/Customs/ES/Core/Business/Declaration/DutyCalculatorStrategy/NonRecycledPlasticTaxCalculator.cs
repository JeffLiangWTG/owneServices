using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.ES.Business.Declaration
{
	sealed class NonRecycledPlasticTaxCalculator : IExtraFeeCalculator
	{
		public NonRecycledPlasticTaxCalculator(CusEntryLine entryLine)
		{
			this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
		}

		readonly CusEntryLine entryLine;

		public IEnumerable<IDutyCalculationIntermediateResult> CalculateExtraFees()
		{
			var fees = new List<IDutyCalculationIntermediateResult>();

			if (!HasNonRecycledPlastic)
			{
				return fees;
			}

			var baseAmount = GetBaseAmount();

			var nonRecycledPlasticFee = new DutyCalculationIntermediateResult(baseAmount * rate, rate, baseAmount, ESConstants.UOM.PK);
			fees.Add(nonRecycledPlasticFee);
			return fees;
		}

		public ZString RateCode => UniversalReferenceConstants.RefCusRateCode.NonRecycledPlasticFee;

		#region Implementation

		decimal GetBaseAmount()
		{
			return InvoiceLinesWithNonRecycledPlastic.Sum(GetApplicableAmountForInvoiceLine);
		}

		decimal GetApplicableAmountForInvoiceLine(JobComInvoiceLine invoiceLine)
		{
			var allAmountsAndUnits = new[]
			{
				(invoiceLine.JI_CustomsQuantity, invoiceLine.JI_CustomsUnitQty),
				(invoiceLine.JI_CustomsSecondQuantity, invoiceLine.JI_CustomsSecondUnitQty),
				(invoiceLine.JI_CustomsThirdQuantity, invoiceLine.JI_CustomsThirdUnitQty),
				(invoiceLine.JI_CustomsFourthQuantity, invoiceLine.JI_CustomsFourthUnitQty),
			};

			foreach (var (amount, unit) in allAmountsAndUnits)
			{
				if (unit == ESConstants.UOM.PK && !amount.IsEmpty)
				{
					return amount;
				}
			}

			return default;
		}

		bool HasNonRecycledPlastic => InvoiceLinesWithNonRecycledPlastic.Count > 0;

		IReadOnlyCollection<JobComInvoiceLine> InvoiceLinesWithNonRecycledPlastic => invoiceLinesWithNonRecycledPlastic ?? (invoiceLinesWithNonRecycledPlastic = GetInvoiceLinesWithNonRecycledPlastic());
		IReadOnlyCollection<JobComInvoiceLine> invoiceLinesWithNonRecycledPlastic;

		IReadOnlyCollection<JobComInvoiceLine> GetInvoiceLinesWithNonRecycledPlastic()
		{
			return entryLine.InvoiceLines
				.Cast<JobComInvoiceLine>()
				.Where(i => i.ZG_HasNonRecycledPlastics)
				.ToArray();
		}

		const decimal rate = 0.45m;

		#endregion
	}
}
