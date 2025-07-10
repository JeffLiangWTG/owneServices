using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(RiskManagementCollection))]
	class RiskManagementCollectionTest : CusSupportingInfoCollectionTest<RiskManagement>
	{
		protected override CusSupportingInfoCollection<RiskManagement> GetCusSupportingInfoCollection() => new RiskManagementCollection(Factory.New<CusEntryInstruction>());
	}
}
