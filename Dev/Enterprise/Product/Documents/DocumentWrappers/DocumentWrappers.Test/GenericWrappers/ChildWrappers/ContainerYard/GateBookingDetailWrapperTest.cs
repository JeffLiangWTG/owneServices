using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Freight.ContainerYard.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(GateBookingDetailWrapper))]
	sealed class GateBookingDetailWrapperTest : GenericWrapperTest
	{
		public void TestWrapperMappings()
		{
			var wrapper = (GateBookingDetailWrapper)GetSetupWrapperForDefaultFormatting();
			CombineAssertions(() =>
			{
				AssertEquals("wrapper.BookingParty.CompanyNameAndAddress", "OWNER TEST\nLINE 1\nLINE 2\nERITREA", wrapper.BookingParty.CompanyNameAndAddress);
				AssertEquals("wrapper.CommodityCode.CodeAndDescription", "ABCD (Test Commodity Code)", wrapper.CommodityCode.CodeAndDescription);
				AssertEquals("wrapper.IsDangerousGoods", true, wrapper.IsDangerousGoods);
				AssertEquals("wrapper.IsContainer", true, wrapper.IsContainer);
				AssertEquals("wrapper.BookingReference", "REF00002", wrapper.BookingReference);
			});
		}

		public override void TestWrapperMappingsEmpty()
		{
			var wrapper = (GateBookingDetailWrapper)GetNewDocumentWrapper();
			CombineAssertions(() =>
			{
				AssertEquals("wrapper.BookingParty.CompanyNameAndAddress", "", wrapper.BookingParty.CompanyNameAndAddress);
				AssertEquals("wrapper.CommodityCode.CodeAndDescription", " ()", wrapper.CommodityCode.CodeAndDescription);
				AssertEquals("wrapper.IsDangerousGoods", false, wrapper.IsDangerousGoods);
				AssertEquals("wrapper.IsContainer", false, wrapper.IsContainer);
				AssertEquals("wrapper.BookingReference", "", wrapper.BookingReference);
			});
		}

		protected override ZString ExpectedDefaultFormatting => @"
BookingParty : OWNER TEST\nLINE 1\nLINE 2\nERITREA
CommodityCode : ABCD (Test Commodity Code)
Registry : (No Default Field Value Available on Registry)
YardUnit : (No Default Field Value Available on GateYardUnit)";

		protected override string ExpectedFieldMap => @"
GateBookingDetail
======================================================================
Name                                    Type
----------------------------------------------------------------------
CommodityCode                           Commodity
YardUnit                                GateYardUnit
BookingParty                            Organisation
BookingReference                        String
IsContainer                             Bool
IsDangerousGoods                        Bool";

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new GateBookingDetailWrapper(null, Factory);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var gateBookingDetail = GetNewGateBookingDetail();
			return new GateBookingDetailWrapper(gateBookingDetail, Factory);
		}

		GateBookingDetail GetNewGateBookingDetail()
		{
			var gateBookingDetail = Factory.New<GateBookingDetail>();

			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.OH_FullName = "OWNER TEST";
			var bookingAddr = bookingParty.Addresses.AddNew();
			bookingAddr.Address1 = "Line 1";
			bookingAddr.Address2 = "Line 2";
			gateBookingDetail.GTD_OA_BookingPartyAddress = bookingAddr.PK;

			var yardUnit = Factory.New<YardUnit>();
			gateBookingDetail.GTD_GTY_YardUnit = yardUnit.PK;

			var commodityCode = Factory.New<RefCommodityCode>();
			commodityCode.RH_Code = "ABCD";
			commodityCode.RH_Description = "Test Commodity Code";
			gateBookingDetail.GTD_RH_NKCommodityCode = commodityCode.RH_Code;

			gateBookingDetail.GTD_IsDangerousGoods = true;
			gateBookingDetail.GTD_IsContainer = true;
			gateBookingDetail.GTD_BookingReference = "REF00002";

			return gateBookingDetail;
		}
	}
}
