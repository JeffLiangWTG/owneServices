using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(Bill))]
class BillTest : Customs.Business.Testing.BaseHouseBillTest<Bill, JobDeclaration>
{
}
