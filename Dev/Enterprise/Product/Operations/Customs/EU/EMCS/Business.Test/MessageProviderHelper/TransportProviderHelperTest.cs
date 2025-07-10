using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	sealed class TransportProviderHelperTest : TestCaseWithFactory
	{
		public void TestUnitCode()
		{
			container.ZG_UnitCode = EMCSTransportUnitCodeList.Codes.Tractor;
			AssertEquals(EMCSTransportUnitCodeList.Codes.Tractor, helper.UnitCode);
		}

		public void TestIdentityOfUnit()
		{
			container.CO_ContainerNumber = "PONU2864065";
			AssertEquals("PONU2864065", helper.IdentityOfUnit);
		}

		public void TestCommercialSealIdentification()
		{
			container.CO_Seal = "4419151";
			AssertEquals("4419151", helper.CommercialSealIdentification);
		}

		protected override void SetUp()
		{
			base.SetUp();
			container = Factory.New<EMCSCusContainer>();
			helper = new TransportProviderHelper(container);
		}
		EMCSCusContainer container;
		TransportProviderHelper helper;
	}
}
