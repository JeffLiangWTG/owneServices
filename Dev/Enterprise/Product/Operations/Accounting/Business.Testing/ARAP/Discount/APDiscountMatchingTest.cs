using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	[TestedType(typeof(APDiscount))]
	public class APDiscountMatchingTest : DiscountMatchingTest
	{
		protected override Discount GetNewDiscount()
		{
			return Factory.New<APDiscount>();
		}
	}
}
