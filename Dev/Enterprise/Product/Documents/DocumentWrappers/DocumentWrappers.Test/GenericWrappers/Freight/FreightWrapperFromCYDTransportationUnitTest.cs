using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Yard.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromCYDTransportationUnit))]
	sealed class FreightWrapperFromCYDTransportationUnitTest : FreightWrapperTest
	{
		public void TestJobNumber()
		{
			var transportationUnit = Factory.New<CYDTransportationUnit>();
			transportationUnit.YTU_TransportationUnitID = "TPU1234";
			var wrapper = new FreightWrapperFromCYDTransportationUnit(transportationUnit, Factory);
			AssertEquals("JobNumber", "TPU1234", wrapper.JobNumber);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<CYDTransportationUnit>();
		}

		public void TestDates()
		{
			var transportationUnit = Factory.New<CYDTransportationUnit>();
			transportationUnit.YTU_TransportationUnitID = "TPU1234";
			var wrapper = new FreightWrapperFromCYDTransportationUnit(transportationUnit, Factory);
			AssertEquals("wrapper.GateInTime", ZDateTimeOffset.Empty, wrapper.GateInTime);
			AssertEquals("wrapper.GateOutTime", ZDateTimeOffset.Empty, wrapper.GateOutTime);
		} 

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var transportationUnit = Factory.New<CYDTransportationUnit>();
			return new FreightWrapperFromCYDTransportationUnit(transportationUnit, Factory);
		}
	}
}
