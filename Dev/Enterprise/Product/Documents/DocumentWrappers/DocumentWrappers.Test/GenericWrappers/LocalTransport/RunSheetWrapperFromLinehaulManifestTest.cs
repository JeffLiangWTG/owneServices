using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(RunSheetWrapperFromLinehaulManifest))]
	sealed class RunSheetWrapperFromLinehaulManifestTest : RunSheetWrapperTest
	{
		#region TestProperties

		public void TestProperties()
		{
			var today = ZDateTime.Now;
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "Bob";
			staff.GS_FullName = "Bobby";
			var truck = Helper.CreateVehicle("VHCL");
			truck.RQ_Registration = "Rego123";
			var transportCompany = Helper.CreateOrganisation("DEMO");
			transportCompany.OH_FullName = "Demo Company";
			var depot1 = Factory.NewWithValidTestData<OrgAddress>();
			depot1.OA_RL_NKRelatedPortCode = "AUSYD";
			var depot2 = Factory.NewWithValidTestData<OrgAddress>();
			depot2.OA_RL_NKRelatedPortCode = "NZAKL";

			LinehaulManifest.LHM_GS_NKDriver1 = staff.GS_Code;
			LinehaulManifest.TransportCompanyPK = transportCompany.PK;
			LinehaulManifest.LHM_StartDateTimeUtc = today;
			//LinehaulManifest.LHM_RQ_PrimaryEquipment = truck.PK;
			LinehaulManifest.LHM_OA_OriginDepot = depot1.PK;
			LinehaulManifest.LHM_OA_DestinationDepot = depot2.PK;

			var wrapper = new RunSheetWrapperFromLinehaulManifest(LinehaulManifest, Factory);
			AssertEquals("DriversName", "Bobby", wrapper.DriversName);
			AssertEquals("TransportCompanyName", "DEMO", wrapper.TransportCompanyName);
			AssertEquals("StartDate", today, wrapper.StartDate);
			AssertEquals("EndDate", today, wrapper.EndDate);
		}

		#endregion

		#region Overrides

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
RunSheetFromLinehaulManifest
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
			return LinehaulGenericWrapper;
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return LinehaulGenericWrapper;
		}

		public override void TestWrapperMappingsEmpty()
		{
			var wrapperEmpty = new RunSheetWrapperFromLinehaulManifest(null, Factory);
			AssertEquals("DriversName", "", wrapperEmpty.DriversName);
			AssertEquals("VehicleRegistration", "", wrapperEmpty.VehicleRegistration);
			AssertEquals("TransportCompanyName", "", wrapperEmpty.TransportCompanyName);
		}

		#endregion

		#region Implementation

		DtbLinehaulManifest LinehaulManifest
		{
			get { return linehaulManifest ?? (linehaulManifest = Factory.New<DtbLinehaulManifest>()); }
		}
		DtbLinehaulManifest linehaulManifest;

		RunSheetWrapperFromLinehaulManifest LinehaulGenericWrapper
		{
			get { return linehaulGenericWrapper ?? (linehaulGenericWrapper = new RunSheetWrapperFromLinehaulManifest(LinehaulManifest, Factory)); }
		}
		RunSheetWrapperFromLinehaulManifest linehaulGenericWrapper;

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
