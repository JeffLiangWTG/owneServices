using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing
{
	[TestedType(typeof(GuaranteesFilterLookups))]
	sealed class GuaranteesFilterLookupsTest : TestCaseWithFactory
	{
		public void TestGuaranteeRuleCodeList() =>
			AssertCodeDescriptionPairList(new GuaranteesFilterStripBusinessObject().Lookups.GuaranteeRuleCodeList,
				(PermitRuleCodeList.Codes.TSP, PermitRuleCodeList.Descriptions.TSP)
			);
	}
}
