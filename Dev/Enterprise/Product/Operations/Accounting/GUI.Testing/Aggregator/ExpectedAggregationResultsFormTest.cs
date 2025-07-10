using System.Windows.Forms;
using Enterprise.Accounting.Business.Aggregator;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Aggregator.Testing
{
	[TestedType(typeof(ExpectedAggregationResultsForm))]
	public class ExpectedAggregationResultsFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			AggregationDiscrepanciesCalculator reAggregator = new AggregationDiscrepanciesCalculator(Factory, "");
			ExpectedAggregationResultsForm result = new ExpectedAggregationResultsForm(reAggregator);
			return result;
		}
	}
}
