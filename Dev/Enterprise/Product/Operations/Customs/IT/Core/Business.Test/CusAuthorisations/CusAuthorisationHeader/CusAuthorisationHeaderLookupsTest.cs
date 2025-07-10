using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(CusAuthorisationHeaderLookups))]
sealed class CusAuthorisationHeaderLookupsTest : Customs.Business.Testing.CusAuthorisationHeaderLookupsAbstractTest<CusAuthorisationHeaderLookups>
{
	protected override CusAuthorisationHeaderLookups CusAuthorisationHeaderLookupsForTesting() => new CusAuthorisationHeaderLookups(Factory.New<CusAuthorisationHeader>());
}
