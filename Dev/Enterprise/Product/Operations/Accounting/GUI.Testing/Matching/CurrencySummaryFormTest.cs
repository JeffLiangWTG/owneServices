using System.Windows.Forms;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(CurrencySummaryForm))]
	public class CurrencySummaryFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new CurrencySummaryForm(new CurrencySummary(new IMatchingCollection(Factory)));
		}
	}
}
