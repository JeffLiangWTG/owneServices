using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(Bill))]
class HouseBillTest : Customs.Business.Testing.BaseHouseBillTest<Bill, JobDeclaration>
{
}
