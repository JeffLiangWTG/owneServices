using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class CASSVATComponentAmount : CASSCostComponentAmount
	{
		public CASSVATComponentAmount(CASSCostLine costLine, ZString componentName)
			: base(costLine, componentName)
		{
		}

		internal Dictionary<ZString, decimal> ApportionedVATAmountByCostComponets
		{
			get
			{
				if (costLine.LineType != CASSCostLineType.Aggregated)
				{
					apportionedVATAmountByCostComponets = costLine.GetApportionedVATByCostComponent(false, ComponentName);
				}
				if (apportionedVATAmountByCostComponets == null)
				{
					apportionedVATAmountByCostComponets = new Dictionary<ZString, decimal>();
				}
				return apportionedVATAmountByCostComponets;
			}
		}
		Dictionary<ZString, decimal> apportionedVATAmountByCostComponets;

		internal Dictionary<ZString, decimal> AdjustedApportionedVATAmountByCostComponets
		{
			get
			{
				if (costLine.LineType != CASSCostLineType.Aggregated)
				{
					adjustedApportionedVATAmountByCostComponets = costLine.GetApportionedVATByCostComponent(true, ComponentName);
				}
				if (adjustedApportionedVATAmountByCostComponets == null)
				{
					adjustedApportionedVATAmountByCostComponets = new Dictionary<ZString, decimal>();
				}
				return adjustedApportionedVATAmountByCostComponets;
			}
		}
		Dictionary<ZString, decimal> adjustedApportionedVATAmountByCostComponets;

		internal override void AddAmount(CASSCostLine costLine)
		{
			base.AddAmount(costLine);

			var apportionedVATAmount = costLine.IsAdjustmentRecord ? AdjustedApportionedVATAmountByCostComponets : ApportionedVATAmountByCostComponets;
			var vatDistribution = costLine.GetVATApportionedToCostComponents(costLine.IsAdjustmentRecord, ComponentName);

			foreach (KeyValuePair<ZString, decimal> item in vatDistribution)
			{
				if (!apportionedVATAmount.ContainsKey(item.Key))
				{
					apportionedVATAmount.Add(item.Key, item.Value);
				}
				else
				{
					apportionedVATAmount[item.Key] += item.Value;
				}
			}
		}
	}
}
