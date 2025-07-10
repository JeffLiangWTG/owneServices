using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Freight.ContainerYard.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(GateTransportCFSDetailWrapper))]
	sealed class GateTransportCFSDetailWrapperTest : GenericWrapperTest
	{
		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new GateTransportCFSDetailWrapper(null, Factory);
		}

		public void TestWrapperMappings()
		{
			var wrapper = (GateTransportCFSDetailWrapper)GetSetupWrapperForDefaultFormatting();

			AssertEquals("wrapper.JobNumber", "GTC10001", wrapper.JobNumber);
			AssertEquals("wrapper.IsPickup", ZBool.True, wrapper.IsPickup);
			AssertEquals("wrapper.TrailerRegistration", "REG5555", wrapper.TrailerRegistration);
			AssertEquals("wrapper.Owner", "MR YARD OWNER\n123 MY YARD STREET\nERITREA", wrapper.Owner.CompanyNameAndAddress);
			AssertEquals("wrapper.Purpose", "PUR", wrapper.Purpose);
		}

		public override void TestWrapperMappingsEmpty()
		{
			var wrapperEmpty = (GateTransportCFSDetailWrapper)GetNewDocumentWrapper();

			AssertEquals("wrapper.JobNumber", "", wrapperEmpty.JobNumber);
			AssertEquals("wrapper.IsPickup", ZBool.False, wrapperEmpty.IsPickup);
			AssertEquals("wrapper.TrailerRegistration", "", wrapperEmpty.TrailerRegistration);
			AssertEquals("wrapper.Owner", "", wrapperEmpty.Owner.CompanyNameAndAddress);
			AssertEquals("wrapper.Purpose", "", wrapperEmpty.Purpose);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get { return @"
GateBookingDetail : (No Default Field Value Available on GateBookingDetail)
GateTransport : GTT20002
Owner : MR YARD OWNER\n123 MY YARD STREET\nERITREA
Registry : (No Default Field Value Available on Registry)
"; }
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var gateTransportCFSDetail = GetNewGateTransportCFSDetail();
			return new GateTransportCFSDetailWrapper(gateTransportCFSDetail, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
GateTransportCFSDetail                      (Default Field: JobNumber)
======================================================================
Name                                    Type
----------------------------------------------------------------------
GateTransport                           Freight
GateBookingDetail                       GateBookingDetail
Owner                                   Organisation
IsPickup                                Bool
JobNumber                               String
Purpose                                 String
TrailerRegistration                     String
";
			}
		}

		GateTransportCFSDetail GetNewGateTransportCFSDetail()
		{
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseName = "Warehouse";

			var gateTransport = Factory.NewWithValidTestData<GateTransport>();
			gateTransport.GTT_JobNumber = "GTT20002";
			gateTransport.GTT_GB_Branch = warehouse.WW_GB_RelatedCompanyBranch;

			var gateTransportCFSDetail = Factory.New<GateTransportCFSDetail>();
			gateTransportCFSDetail.GTF_JobNumber = "GTC10001";
			gateTransportCFSDetail.GTF_TrailerRegistration = "REG5555";
			gateTransportCFSDetail.GTF_IsPickup = true;
			gateTransportCFSDetail.GTF_GTT = gateTransport.PK;
			gateTransportCFSDetail.GTF_Purpose = "PUR";

			var gateTransportOwner = Factory.New<OrgHeader>();
			gateTransportOwner.OH_FullName = "Mr Gate Transport Owner";
			var bookingAddr = gateTransportOwner.Addresses.AddNew();
			bookingAddr.Address1 = "Line 1";
			bookingAddr.Address2 = "Line 2";
			gateTransportCFSDetail.GTF_OH_Owner = gateTransportOwner.PK;

			var yardUnitOwner = Factory.New<OrgHeader>();
			yardUnitOwner.OH_FullName = "Mr Yard Owner";
			var yardUnitOwnerAddress = yardUnitOwner.Addresses.AddNew();
			yardUnitOwnerAddress.Address1 = "123 My Yard Street";

			var gateBookingDetail = Factory.New<GateBookingDetail>();
			gateBookingDetail.GTD_BookingReference = "REF00001";
			gateTransportCFSDetail.GTF_GTD_GateBookingDetail = gateBookingDetail.PK;

			var yardUnit = Factory.New<YardUnit>();
			yardUnit.GTY_OA_UnitOwnerAddress = yardUnitOwnerAddress.PK;
			gateBookingDetail.GTD_GTY_YardUnit = yardUnit.PK;

			return gateTransportCFSDetail;
		}
	}
}
