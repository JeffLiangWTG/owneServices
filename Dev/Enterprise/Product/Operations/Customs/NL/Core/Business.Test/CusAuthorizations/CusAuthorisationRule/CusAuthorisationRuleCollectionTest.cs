using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(CusAuthorisationRuleCollection))]
sealed class CusAuthorisationRuleCollectionTest : ActiveBusinessObjectCollectionTestCase<CusAuthorisationRuleCollection>
{
	protected override CusAuthorisationRuleCollection GetCollectionToTest() => new CusAuthorisationRuleCollection(Factory.NewWithValidTestData<CusAuthorisationHeader>());
}
