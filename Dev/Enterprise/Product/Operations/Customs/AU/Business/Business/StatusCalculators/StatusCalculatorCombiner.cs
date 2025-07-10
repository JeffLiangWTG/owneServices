using System.Collections.Generic;
using System.Text;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class StatusCalculatorCombiner : ICalculatedCusStatusCalculator
	{
		public StatusCalculatorCombiner(ICalculatedCusStatusCalculator[] calculators)
		{
			calculatorList.AddRange(calculators);
		}

		public void DeriveStatusIfEmptyWithMessages()
		{
			calculatorList.ForEach(delegate(ICalculatedCusStatusCalculator calculator)
			{ calculator.DeriveStatusIfEmptyWithMessages(); });
		}

		public void DeriveStatusNow()
		{
			calculatorList.ForEach(delegate(ICalculatedCusStatusCalculator calculator)
			{ calculator.DeriveStatusNow(); });
		}

		public ZString UserFriendlyStatusText
		{
			get
			{
				StringBuilder result = new StringBuilder();
				foreach (ICalculatedCusStatusCalculator calculator in calculatorList)
				{
					result.AppendLine(calculator.UserFriendlyStatusText);
				}
				return result.ToString();
			}
		}

		public void ResetToOriginal()
		{
			calculatorList.ForEach(delegate(ICalculatedCusStatusCalculator calculator)
			{ calculator.ResetToOriginal(); });
		}

		readonly List<ICalculatedCusStatusCalculator> calculatorList = new List<ICalculatedCusStatusCalculator>();
	}
}
