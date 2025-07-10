using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class IsNotBlankComparisonOperatorTest : TestCase
	{
		public void TestCorrectInheritance()
		{
			Assert(typeof(NotEqualComparisonOperator).IsAssignableFrom(typeof(IsNotBlankComparisonOperator)));
		}
	}
}
