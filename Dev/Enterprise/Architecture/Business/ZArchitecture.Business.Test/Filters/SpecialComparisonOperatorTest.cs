using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class SpecialComparisonOperatorTest : TestCase
	{
		public void TestIsBlankType()
		{
			AssertEquals(SpecialComparisonOperator.IsBlank.GetType(), typeof(IsBlankComparisonOperator));
		}

		public void TestIsNotBlankType()
		{
			AssertEquals(SpecialComparisonOperator.IsNotBlank.GetType(), typeof(IsNotBlankComparisonOperator));
		}
	}
}
