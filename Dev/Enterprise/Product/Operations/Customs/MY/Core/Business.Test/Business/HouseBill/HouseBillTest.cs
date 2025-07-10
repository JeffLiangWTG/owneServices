using NUnit.Framework;

namespace Enterprise.Customs.MY.Business.Testing
{
	[TestedType(typeof(Bill))]
	class HouseBillTest : Customs.Business.Testing.BaseHouseBillTest<Bill, JobDeclaration>
	{
	}
}
