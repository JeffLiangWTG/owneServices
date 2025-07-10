using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class CASSCostComponentAmount
	{
		public CASSCostComponentAmount(CASSCostLine costLine, ZString componentName)
		{
			this.componentName = componentName;
			this.costLine = costLine;
		}
		readonly protected CASSCostLine costLine;
		readonly ZString componentName;

		internal ZString ComponentName
		{
			get { return componentName; }
		}

		internal ZDecimal Amount
		{
			get { return amount; }
			set { amount = value; }
		}
		ZDecimal amount;

		internal ZDecimal AdjustedAmount
		{
			get { return adjustedAmount; }
			set { adjustedAmount = value; }
		}
		ZDecimal adjustedAmount;

		internal ZDecimal AmountWithSign
		{
			get { return Amount * costLine.GetComponentAmountMultipler(componentName); }
		}

		internal ZDecimal AdjustedAmountWithSign
		{
			get { return AdjustedAmount * costLine.GetComponentAmountMultipler(componentName); }
		}

		internal virtual void AddAmount(CASSCostLine cstLine)
		{
			var amnt = cstLine.GetComponentAmount(cstLine.IsAdjustmentRecord, componentName);
			if (cstLine.IsAdjustmentRecord)
			{
				AdjustedAmount += amnt;
			}
			else
			{
				Amount += amnt;
			}
		}
	}
}
