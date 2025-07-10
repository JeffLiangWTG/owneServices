using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class IsBlankComparisonOperatorTest : TestCase
	{
		public void TestCorrectInheritance()
		{
			Assert(typeof(EqualComparisonOperator).IsAssignableFrom(typeof(IsBlankComparisonOperator)));
		}
	}
}
