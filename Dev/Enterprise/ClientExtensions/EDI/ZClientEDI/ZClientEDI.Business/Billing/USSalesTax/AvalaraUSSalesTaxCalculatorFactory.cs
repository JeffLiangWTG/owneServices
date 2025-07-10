using Enterprise.Accounting.Business.ARAP.Invoicing.USSalesTax;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Client.EDI.Billing.Business.USSalesTax
{
	[Immutable]
	public class AvalaraUSSalesTaxCalculatorFactory : IUSSalesTaxCalculatorFactory
	{
		public IUSSalesTaxCalculator Get() => new AvalaraUSSalesTaxCalculator();
	}
}
