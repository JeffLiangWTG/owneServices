using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CusAuthorisationHeaderLookups))]
	class CusAuthorisationHeaderLookupsTest : Customs.Business.Testing.CusAuthorisationHeaderLookupsAbstractTest<CusAuthorisationHeaderLookups>
	{
		protected override CusAuthorisationHeaderLookups CusAuthorisationHeaderLookupsForTesting() => new CusAuthorisationHeaderLookups(Factory.New<CusAuthorisationHeader>());
	}
}
