using System;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class EnRouteEventSealWrapperTest : Customs.Business.Testing.DataProviderTestCase<EnRouteEventSealWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new EnRouteEventSealWrapper(null));
		}

		public void TestSealCount()
		{
			AssertEquals("1", wrapper.SealCount);
		}

		public void TestContainerSeals()
		{
			sealContainer.BC_Seal1 = "0000000001";
			var sealContainer2 = enRouteSeal.SealContainers.AddNew();
			sealContainer2.BC_Seal1 = "0000000001";
			var sealContainer3 = enRouteSeal.SealContainers.AddNew();
			sealContainer3.BC_Seal1 = "0000000002";

			var containersSeals = wrapper.ContainerSeals;
			AssertContainsExactElementsInAnyOrder(new ZString[] { "0000000001", "0000000002" }, containersSeals.Select(x => x.SealIdentity));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			enRouteSeal = header.EnRouteSeals.AddNew();
			sealContainer = enRouteSeal.SealContainers.AddNew();
			wrapper = new EnRouteEventSealWrapper(enRouteSeal);
		}
		EnRouteSeal enRouteSeal;
		EnRouteEventSealWrapper wrapper;
		SealContainer sealContainer;

		protected override EnRouteEventSealWrapper GetProvider() => wrapper;
	}
}
