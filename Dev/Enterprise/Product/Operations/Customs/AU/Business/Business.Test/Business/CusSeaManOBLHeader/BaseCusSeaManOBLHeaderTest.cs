using Enterprise.Customs.Common.AU.CMR;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public abstract class BaseCusSeaManOBLHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestShipmentStatus()
		{
			var header = (BaseCusSeaManOBLHeader)GetNewBusinessObject();
			header.ShipmentStatus.Code = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;

			AssertEquals(CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl, header.ShipmentStatus.Code);
			AssertEquals(CMRConsolidatedCargoStatuses.Descriptions.HeldCargoIsHeldUnderCustomsControl, header.ShipmentStatus.Description);
		}

		public void TestShipmentCalculator()
		{
			var header = (BaseCusSeaManOBLHeader)GetNewBusinessObject();
			AssertNotNull("Calculator", header.ShipmentCalculator);
			AssertEquals("type", typeof(CusSeaManOBLHeaderShipmentStatusCalculator), header.ShipmentCalculator.GetType());
		}
	}
}
