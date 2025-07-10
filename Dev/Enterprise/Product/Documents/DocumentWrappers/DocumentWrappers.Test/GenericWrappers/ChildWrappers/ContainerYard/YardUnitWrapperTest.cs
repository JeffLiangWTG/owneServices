using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Warehouse.Yard.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(YardUnitWrapper))]
	sealed class YardUnitWrapperTest : GenericWrapperTest
	{
		public void TestWrapperMappings()
		{
			var wrapper = (YardUnitWrapper)GetSetupWrapperForDefaultFormatting();
			CombineAssertions(() =>
			{
				AssertEquals("wrapper.UnitNumber", "TEST123", wrapper.UnitNumber);
				AssertZDatesWithin5Minutes("wrapper.LoadTime", ZDateTimeOffset.Now, wrapper.LoadTime.Value);
				AssertZDatesWithin5Minutes("wrapper.UnloadTime", ZDateTimeOffset.Now, wrapper.UnloadTime.Value);
			});
		}

		public override void TestWrapperMappingsEmpty()
		{
			var wrapper = (YardUnitWrapper)GetNewDocumentWrapper();
			CombineAssertions(() =>
			{
				AssertEquals("wrapper.UnitNumber", "", wrapper.UnitNumber);
				AssertNotEquals("wrapper.LoadTime", ZDateTimeOffset.Now, wrapper.LoadTime.Value);
				AssertNotEquals("wrapper.UnloadTime", ZDateTimeOffset.Now, wrapper.UnloadTime.Value);
			});
		}

		protected override ZString ExpectedDefaultFormatting => @"
Delivery : (No Default Field Value Available on CYDDelivery)
DispatchTransportationUnit : (No Default Field Value Available on CYDTransportationUnit)
Pickup : (No Default Field Value Available on CYDPickup)
ReceiveLine : (No Default Field Value Available on CYDReceiveAdviceLine)
ReceiveTransportationUnit : (No Default Field Value Available on CYDTransportationUnit)
Registry : (No Default Field Value Available on Registry)
ReleaseLine : (No Default Field Value Available on CYDReleaseAdviceLine)
Yard : ";

		protected override string ExpectedFieldMap => @"
YardUnit
======================================================================
Name                                    Type
----------------------------------------------------------------------
Delivery                                CYDDelivery
Pickup                                  CYDPickup
ReceiveLine                             CYDReceiveAdviceLine
ReleaseLine                             CYDReleaseAdviceLine
DispatchTransportationUnit              CYDTransportationUnit
ReceiveTransportationUnit               CYDTransportationUnit
Yard                                    WarehouseBO
UnitNumber                              String";

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new YardUnitWrapper(null, Factory);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var yardUnitState = GetNewYardUnitState();
			return new YardUnitWrapper(yardUnitState, Factory);
		}

		CYDYardUnitState GetNewYardUnitState()
		{
			var yardUnit = Factory.New<CYDYardUnitState>();
			yardUnit.YUS_UnitID = "TEST123";
			yardUnit.YUS_YRL_ReceiveLine = ZGuid.NewZGuid();
			yardUnit.YUS_YEL_ReleaseLine = ZGuid.NewZGuid();
			yardUnit.YUS_WW_CurrentYard = ZGuid.NewZGuid();
			yardUnit.YUS_YDL_Delivery = ZGuid.NewZGuid();
			yardUnit.YUS_YPL_Pickup = ZGuid.NewZGuid();
			yardUnit.YUS_YTU_DispatchTransportationUnit = ZGuid.NewZGuid();
			yardUnit.YUS_YTU_ReceiveTransportationUnit = ZGuid.NewZGuid();
			yardUnit.YUS_GS_NKLoadUser = "ABC";
			yardUnit.YUS_GS_NKUnloadUser = "ABC";
			yardUnit.YUS_LoadTime = ZDateTimeOffset.Now;
			yardUnit.YUS_UnloadTime = ZDateTimeOffset.Now;
			return yardUnit;
		}
	}
}
