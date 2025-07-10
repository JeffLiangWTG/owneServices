using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Freight.ContainerYard.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(GateTransportWrapper))]
	sealed class GateTransportWrapperTest : GenericWrapperTest
	{
		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new GateTransportWrapper(null, null, Factory);
		}

		public void TestWrapperMappings()
		{
			var today = ZDateTime.Today;
			var wrapper = (GateTransportWrapper)GetSetupWrapperForDefaultFormatting();

			AssertEquals("wrapper.DriverLicense", "LIC0123456789", wrapper.DriverLicense);
			AssertEquals("wrapper.DriverName", "JAMES BOND", wrapper.DriverName);
			AssertEquals("wrapper.Branch", "TEST BRANCH", wrapper.Branch.BranchName);
			AssertEquals("wrapper.GateInDate", today.AddDays(-1), wrapper.GateInDate);
			AssertEquals("wrapper.GateOutDate", today.AddDays(1), wrapper.GateOutDate);
			AssertEquals("wrapper.JobNumber", "JOB888888", wrapper.JobNumber);
			AssertEquals("wrapper.VehicleRegistration", "CAR123", wrapper.VehicleRegistration);
			AssertEquals("wrapper.SystemCreateUserName", "Tommy Fan", wrapper.SystemCreateUserName);
			AssertEquals("wrapper.TransportCompany.CompanyName", "TRANSPORT COMPANY", wrapper.TransportCompany.CompanyName);
		}

		public override void TestWrapperMappingsEmpty()
		{
			var wrapperEmpty = (GateTransportWrapper)GetNewDocumentWrapper();

			AssertEquals("wrapperEmpty.DriverLicense", "", wrapperEmpty.DriverLicense);
			AssertEquals("wrapperEmpty.DriverName", "", wrapperEmpty.DriverName);
			AssertEquals("wrapperEmpty.Branch", null, wrapperEmpty.Branch);
			AssertEquals("wrapperEmpty.GateInDate", ZDateTime.Empty, wrapperEmpty.GateInDate);
			AssertEquals("wrapperEmpty.GateOutDate", ZDateTime.Empty, wrapperEmpty.GateOutDate);
			AssertEquals("wrapperEmpty.JobNumber", "", wrapperEmpty.JobNumber);
			AssertEquals("wrapperEmpty.VehicleRegistration", "", wrapperEmpty.VehicleRegistration);
			AssertEquals("wrapperEmpty.SystemCreateUserName", "", wrapperEmpty.SystemCreateUserName);
			AssertEquals("wrapperEmpty.TransportCompany.CompanyName", "", wrapperEmpty.TransportCompany.CompanyName);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get { return @"
Registry : (No Default Field Value Available on Registry)
TransportCompany : TRANSPORT COMPANY\nAUSTRALIA
"; }
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var gateTransport = GetNewGateTransportWithCYDetail();
			return GateTransportWrapper.NewWithCYDetail(gateTransport, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
GateTransport                               (Default Field: JobNumber)
======================================================================
Name                                    Type
----------------------------------------------------------------------
TransportCompany                        Organisation
DriverLicense                           String
DriverName                              String
GateInDate                              DateTime
GateOutDate                             DateTime
JobNumber                               String
SystemCreateUserName                    String
VehicleRegistration                     String

GateTransportCFSDetails                 GateTransportCFSDetail Collection
";
			}
		}

		public void TestGateTransportCFSDetails()
		{
			var gateTransport = Factory.New<GateTransport>();
			gateTransport.GateTransportCFSDetails.AddNew();
			gateTransport.GateTransportCFSDetails.AddNew();
			AssertEquals(2, gateTransport.GateTransportCFSDetails.Count);

			var wrapper = new GateTransportWrapper(null, null, Factory);
			AssertEquals(0, wrapper.GateTransportCFSDetails.Count);

			wrapper = GateTransportWrapper.NewWithCFSDetail(gateTransport, gateTransport.GateTransportCFSDetails[0], Factory);
			AssertEquals(1, wrapper.GateTransportCFSDetails.Count);

			wrapper = GateTransportWrapper.NewWithCFSDetail(gateTransport, null, Factory);
			AssertEquals(2, wrapper.GateTransportCFSDetails.Count);
		}

		#region Implementation

		GateTransport GetNewGateTransportWithCYDetail()
		{
			var gateTransport = Factory.New<GateTransport>();
			var today = ZDateTime.Today;

			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseName = "Warehouse";

			var orgTransport = Factory.New<JobDocAddress>();
			orgTransport.E2_AddressOverride = true;
			orgTransport.E2_CompanyName = "Transport Company";
			orgTransport.E2_AddressType = DocAddressTypes.Codes.TransportCompanyDocumentaryAddress;
			orgTransport.E2_RN_NKCountryCode = "AU";
			orgTransport.E2_ParentID = gateTransport.PK;
			orgTransport.E2_ParentTableCode = gateTransport.TablePrefix;

			gateTransport.GTT_DriverName = "JAMES BOND";
			gateTransport.GTT_DriverLicence = "LIC0123456789";
			gateTransport.GTT_TimeIn = today.AddDays(-1);
			gateTransport.GTT_TimeOut = today.AddDays(1);
			gateTransport.GTT_VehicleRegistration = "CAR123";
			gateTransport.GTT_JobNumber = "JOB888888";

			var branch = Factory.New<GlbBranch>();
			branch.GB_BranchName = "TEST BRANCH";
			gateTransport.GTT_GB_Branch = branch.PK;

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TFN";
			staff.GS_FullName = "Tommy Fan";
			gateTransport.GTT_SystemCreateUser = staff.GS_Code;

			var gateTransportDetail = Factory.NewWithValidTestData<GateTransportCYDetail>();
			gateTransportDetail.GTC_JobNumber = "GTCJOB11111";
			gateTransportDetail.GTC_IsPickup = ZBool.True;
			gateTransportDetail.GTC_TrailerRegistration = "TRL588";
			gateTransportDetail.GTC_GTT = gateTransport.PK;

			return gateTransport;
		}

		#endregion
	}
}
