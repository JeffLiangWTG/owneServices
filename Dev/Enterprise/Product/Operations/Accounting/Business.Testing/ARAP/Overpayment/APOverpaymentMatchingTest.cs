using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Overpayment.Testing
{
	[TestedType(typeof(APOverpayment))]
	public class APOverpaymentMatchingTest : OverpaymentMatchingTest
	{
		protected override Overpayment GetNewOverpayment()
		{
			return Factory.New<APOverpayment>();
		}
	}
}
