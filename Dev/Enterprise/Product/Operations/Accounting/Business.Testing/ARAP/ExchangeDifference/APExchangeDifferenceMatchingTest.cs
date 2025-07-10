using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	[TestedType(typeof(APExchangeDifference))]
	public class APExchangeDifferenceMatchingTest : ExchangeDifferenceMatchingTest
	{
		protected override ExchangeDifference GetNewExchangeDifference()
		{
			return Factory.New<APExchangeDifference>();
		}
	}
}
