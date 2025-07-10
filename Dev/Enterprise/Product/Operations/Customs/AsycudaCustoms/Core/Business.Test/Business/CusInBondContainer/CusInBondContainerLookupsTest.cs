using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	public class CusInBondContainerLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCusContainers()
		{
			var dec = Factory.New<JobDeclaration>();
			var container1 = dec.CusContainers.AddNew();
			container1.CO_ContainerNumber = "C01";
			var container2 = dec.CusContainers.AddNew();
			container2.CO_ContainerNumber = "C02";

			var moveDetail = dec.CusEntryInstruction.CusInBondPermitsHeaders.AddNew().FirstMoveDetail;
			var inBondContianer = moveDetail.Containers.AddNew();
			var lookupCusContainers = inBondContianer.Lookups.CusContainers;
			AssertEquals(2, lookupCusContainers.Count);
			AssertType(typeof(CodeDescriptionPairList), lookupCusContainers);
			AssertEquals("C01", lookupCusContainers[0].Code);
			AssertEquals("C02", lookupCusContainers[1].Code);
		}
	}
}
