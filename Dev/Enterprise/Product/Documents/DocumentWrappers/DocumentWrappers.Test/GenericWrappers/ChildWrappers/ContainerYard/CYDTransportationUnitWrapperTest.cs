using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Warehouse.Yard.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(CYDTransportationUnitWrapper))]
	sealed class CYDTransportationUnitWrapperTest : GenericWrapperTest
	{
		public void TestWrapperMappings()
		{
			var wrapper = (CYDTransportationUnitWrapper)GetSetupWrapperForDefaultFormatting();
			CombineAssertions(() =>
			{
				AssertEquals("wrapper.TransportationReference", "REF123", wrapper.TransportationReference);
				AssertEquals("wrapper.TransportationUnitID", "TEST123", wrapper.TransportationUnitID);
				AssertZDatesWithin5Minutes("wrapper.GateInTime", ZDateTimeOffset.Now, wrapper.GateInTime.Value);
				AssertZDatesWithin5Minutes("wrapper.GateOutTime", ZDateTimeOffset.Now, wrapper.GateOutTime.Value);
			});
		}

		public override void TestWrapperMappingsEmpty()
		{
			var wrapper = (CYDTransportationUnitWrapper)GetNewDocumentWrapper();
			CombineAssertions(() =>
			{
				AssertEquals("wrapper.TransportationReference", "", wrapper.TransportationReference);
				AssertEquals("wrapper.TransportationUnitID", "", wrapper.TransportationUnitID);
				AssertNotEquals("wrapper.GateInTime", ZDateTimeOffset.Now, wrapper.GateInTime.Value);
				AssertNotEquals("wrapper.GateOutTime", ZDateTimeOffset.Now, wrapper.GateOutTime.Value);
			});
		}

		protected override ZString ExpectedDefaultFormatting => @"
Registry : (No Default Field Value Available on Registry)";

		protected override string ExpectedFieldMap => @"
CYDTransportationUnit
======================================================================
Name                                    Type
----------------------------------------------------------------------
TransportationReference                 String
TransportationUnitID                    String";

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new CYDTransportationUnitWrapper(null, Factory);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var transportationUnit = GetNewTransportationUnit();
			return new CYDTransportationUnitWrapper(transportationUnit, Factory);
		}

		CYDTransportationUnit GetNewTransportationUnit()
		{
			var transportationUnit = Factory.New<CYDTransportationUnit>();
			transportationUnit.YTU_TransportationUnitID = "TEST123";
			transportationUnit.YTU_TransportationReference = "REF123";
			transportationUnit.YTU_GateInTime = ZDateTimeOffset.Now;
			transportationUnit.YTU_GateOutTime = ZDateTimeOffset.Now;
			return transportationUnit;
		}
	}
}
