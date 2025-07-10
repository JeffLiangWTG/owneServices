using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(RunSheetWrapperFromCommonRunSheet))]
	sealed class RunSheetWrapperFromCommonRunSheetTest : RunSheetWrapperTest
	{
		#region TestProperties

		public void TestProperties()
		{
			var runSheetBizO = Factory.New<CommonWorkSheet>();
			runSheetBizO.EY_DriversName = "Bob";
			runSheetBizO.EY_TruckRegistration = "Veh";
			runSheetBizO.EY_TransportCoName = "Bobs Co";
			runSheetBizO.EY_StartTime = new ZDateTime(2010, 1, 2);
			runSheetBizO.EY_EndTime = new ZDateTime(2010, 1, 3);

			var wrapper1 = new RunSheetWrapperFromCommonRunSheet(runSheetBizO, Factory);
			AssertEquals("DriversName", "Bob", wrapper1.DriversName);
			AssertEquals("VehicleRegistration", "Veh", wrapper1.VehicleRegistration);
			AssertEquals("TransportCompanyName", "Bobs Co", wrapper1.TransportCompanyName);
			AssertEquals("StartDate", new ZDateTime(2010, 1, 2), wrapper1.StartDate);
			AssertEquals("EndDate", new ZDateTime(2010, 1, 3), wrapper1.EndDate);

			var transportCo = Factory.New<OrgHeader>();
			transportCo.OH_FullName = "Bobs Trucking Co";
			runSheetBizO.EY_OH_TransportCo = transportCo.PK;
			var wrapper2 = new RunSheetWrapperFromCommonRunSheet(runSheetBizO, Factory);
			AssertEquals("TransportCompanyName", "Bobs Trucking Co", wrapper2.TransportCompanyName);
		}

		#endregion

		#region Overrides

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
RunSheetFromCommonRunSheet
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
			var wrapperEmpty = new RunSheetWrapperFromCommonRunSheet(null, Factory);
			AssertEquals("DriversName", "", wrapperEmpty.DriversName);
			AssertEquals("VehicleRegistration", "", wrapperEmpty.VehicleRegistration);
			AssertEquals("TransportCompanyName", "", wrapperEmpty.TransportCompanyName);
			AssertEquals("StartDate", ZDateTime.Empty, wrapperEmpty.StartDate);
			AssertEquals("EndDate", ZDateTime.Empty, wrapperEmpty.EndDate);
		}

		#endregion

		#region Implementation

		CommonWorkSheet RunSheet
		{
			get { return runSheet ?? (runSheet = Factory.New<CommonWorkSheet>()); }
		}
		CommonWorkSheet runSheet;

		RunSheetWrapperFromCommonRunSheet RunSheetGenericWrapper
		{
			get { return runSheetGenericWrapper ?? (runSheetGenericWrapper = new RunSheetWrapperFromCommonRunSheet(RunSheet, Factory)); }
		}
		RunSheetWrapperFromCommonRunSheet runSheetGenericWrapper;

		#endregion
	}
}
