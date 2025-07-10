using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	[TestedType(typeof(ARDiscount))]
	public class ARDiscountMatchingTest : DiscountMatchingTest
	{
		protected override Discount GetNewDiscount()
		{
			return Factory.New<ARDiscount>();
		}
	}
}
