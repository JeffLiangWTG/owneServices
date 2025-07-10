using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Overpayment.Testing
{
	[TestedType(typeof(AROverpayment))]
	public class AROverpaymentMatchingTest : OverpaymentMatchingTest
	{
		protected override Overpayment GetNewOverpayment()
		{
			return Factory.New<AROverpayment>();
		}
	}
}
