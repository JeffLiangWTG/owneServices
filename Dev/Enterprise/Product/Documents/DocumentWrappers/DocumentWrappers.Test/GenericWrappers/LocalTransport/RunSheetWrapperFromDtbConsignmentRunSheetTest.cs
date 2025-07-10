using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(RunSheetWrapperFromDtbConsignmentRunSheet))]
	sealed class RunSheetWrapperFromDtbConsignmentRunSheetTest : RunSheetWrapperTest
	{
		#region TestProperties

		public void TestProperties()
		{
			var todayDTO = ZDateTimeOffset.Now;
			var today = todayDTO.ToLocalZDateTime();
			var truck = Helper.CreateVehicle("VHCL");
			truck.RQ_Registration = "Reg123";
			RunSheet.KG_AdHocDriversName = "Bob";
			RunSheet.KG_AdHocTruckRegistration = "Veh";
			RunSheet.KG_AdHocTransportCoName = "Bobs Co";
			RunSheet.KG_StartTime = todayDTO;
			RunSheet.KG_EndTime = todayDTO.AddDays(1);
			RunSheet.KG_RQ_Truck = truck.PK;

			var wrapper = new RunSheetWrapperFromDtbConsignmentRunSheet(runSheet, Factory);
			AssertEquals("DriversName", "Bob", wrapper.DriversName);
			AssertEquals("VehicleRegistration", "Reg123", wrapper.VehicleRegistration);
			AssertEquals("TransportCompanyName", "Bobs Co", wrapper.TransportCompanyName);
			AssertEquals("StartDate", today, wrapper.StartDate);
			AssertEquals("EndDate", today.AddDays(1), wrapper.EndDate);
			AssertEquals("Vehicle", "Reg123", wrapper.Vehicle.Registration);
			AssertEquals("Duration", "24:00", wrapper.Duration);
		}

		#endregion

		#region Overrides

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
RunSheetFromDtbConsignmentRunSheet
======================================================================
Name                                    Type
----------------------------------------------------------------------
Vehicle                                 Equipment
DriversName                             String
Duration                                String
EndDate                                 DateTime
StartDate                               DateTime
TransportCompanyName                    String
VehicleRegistration                     String
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"Registry : (No Default Field Value Available on Registry)
Vehicle :  is null";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return RunSheetGenericWrapper;
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return RunSheetGenericWrapper;
		}

		public override void TestWrapperMappingsEmpty()
		{
			var wrapperEmpty = new RunSheetWrapperFromDtbConsignmentRunSheet(null, Factory);
			AssertEquals("DriversName", "", wrapperEmpty.DriversName);
			AssertEquals("VehicleRegistration", "", wrapperEmpty.VehicleRegistration);
			AssertEquals("TransportCompanyName", "", wrapperEmpty.TransportCompanyName);
		}

		#endregion

		#region Implementation

		DtbConsignmentRunSheet RunSheet
		{
			get { return runSheet ?? (runSheet = Factory.New<DtbConsignmentRunSheet>()); }
		}
		DtbConsignmentRunSheet runSheet;

		RunSheetWrapperFromDtbConsignmentRunSheet RunSheetGenericWrapper
		{
			get { return runSheetGenericWrapper ?? (runSheetGenericWrapper = new RunSheetWrapperFromDtbConsignmentRunSheet(RunSheet, Factory)); }
		}
		RunSheetWrapperFromDtbConsignmentRunSheet runSheetGenericWrapper;

		#endregion

		#region Helper

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helper;

		#endregion
	}
}
