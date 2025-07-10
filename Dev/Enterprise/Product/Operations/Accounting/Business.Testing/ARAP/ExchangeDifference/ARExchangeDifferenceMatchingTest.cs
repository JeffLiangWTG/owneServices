using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	[TestedType(typeof(ARExchangeDifference))]
	public class ARExchangeDifferenceMatchingTest : ExchangeDifferenceMatchingTest
	{
		protected override ExchangeDifference GetNewExchangeDifference()
		{
			return Factory.New<ARExchangeDifference>();
		}
	}
}
