using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GenericConsol.Testing
{
	[TestedType(typeof(GenericConsol))]
	public class GenericConsolTest : BusinessObjectBaseTestCase
	{
		#region GetConsolPKByPrimaryCode

		public void TestGetConsolPKByPrimaryCode()
		{
			var forwardingConsol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			var runSheet = Factory.NewWithValidTestData<DtbConsignmentRunSheet>();
			var manifest = Factory.NewWithValidTestData<DtbLinehaulManifest>();
			var transportBookingConsol = (BusinessObject)Factory.New<IDtbBookingConsolidation>();
			transportBookingConsol.FillWithValidTestData();
			transportBookingConsol[DtbBookingConsolidationSchema.KB_JobType] = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;
			var forwardingConsol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			var transportBookingSingleJobConsolParent = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			transportBookingSingleJobConsolParent[DtbBookingConsolidationSchema.KB_JobType] = TransportConsolidationJobTypes.Codes.Booking;
			var transportBooking = transportBookingSingleJobConsolParent.Bookings.AddNew();
			transportBooking.FillWithValidTestData();
			transportBooking.KM_JobType = TransportConsolidationJobTypes.Codes.Booking;
			transportBookingSingleJobConsolParent[DtbBookingConsolidationSchema.KB_ParentID] = forwardingConsol2.PK;
			transportBookingSingleJobConsolParent[DtbBookingConsolidationSchema.KB_ParentTableCode] = JobConsolSchema.Constants.Prefix;
			var masterTransportBookingConsolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			masterTransportBookingConsolidation[DtbBookingConsolidationSchema.KB_JobType] = TransportConsolidationJobTypes.Codes.Booking;
			masterTransportBookingConsolidation[DtbBookingConsolidationSchema.KB_IsMaster] = true;
			var masterTransportBooking = masterTransportBookingConsolidation.Bookings.AddNew();
			masterTransportBooking.FillWithValidTestData();
			masterTransportBooking[DtbBookingSchema.KM_IsMaster] = true;
			var workSheet = Factory.NewWithValidTestData<CommonWorkSheet>();
			var rtu = (BusinessObject)Factory.New<IWhsItemReceiveTransportationUnit>();
			rtu.FillWithValidTestData();
			var dtu = (BusinessObject)Factory.New<IWhsItemDispatchTransportationUnit>();
			dtu.FillWithValidTestData();
			var dll = (BusinessObject)Factory.New<IWhsItemDispatchLoadList>();
			dll.FillWithValidTestData();

			Factory.Save();

			AssertEquals(forwardingConsol1.PK, GenericConsol.GetConsolPKByPrimaryCode(Factory, forwardingConsol1.JK_UniqueConsignRef, JobConsolSchema.Constants.Prefix));
			AssertEquals(forwardingConsol1.PK, GenericConsol.GetConsolPKByPrimaryCode(Factory, forwardingConsol1.JK_UniqueConsignRef));

			AssertEquals(runSheet.PK, GenericConsol.GetConsolPKByPrimaryCode(Factory, ((IJobCostingPlugIn)runSheet).JK_UniqueConsignRef, DtbConsignmentRunSheetSchema.Constants.Prefix));
			AssertEquals(runSheet.PK, GenericConsol.GetConsolPKByPrimaryCode(Factory, ((IJobCostingPlugIn)runSheet).JK_UniqueConsignRef));

			AssertEquals(transportBookingConsol.PK, GenericConsol.GetConsolPKByPrimaryCode(Factory, ((IJobCostingPlugIn)transportBookingConsol).JK_UniqueConsignRef, DtbBookingConsolidationSchema.Constants.Prefix));
			AssertEquals(transportBookingConsol.PK, GenericConsol.GetConsolPKByPrimaryCode(Factory, ((IJobCostingPlugIn)transportBookingConsol).JK_UniqueConsignRef));

			AssertEquals(transportBooking.PK, GenericConsol.GetConsolPKByPrimaryCode(Factory, ((IJobCostingPlugIn)transportBooking).JK_UniqueConsignRef, DtbBookingSchema.Constants.Prefix));
			AssertEquals(transportBooking.PK, GenericConsol.GetConsolPKByPrimaryCode(Factory, ((IJobCostingPlugIn)transportBooking).JK_UniqueConsignRef));

			AssertEquals("Precondition: No booking parent", ZGuid.Empty, masterTransportBookingConsolidation.KB_ParentID);
			AssertEquals("Precondition: No booking parent", string.Empty, masterTransportBookingConsolidation.KB_ParentTableCode);
			AssertEquals(masterTransportBooking.PK, GenericConsol.GetConsolPKByPrimaryCode(Factory, ((IJobCostingPlugIn)masterTransportBooking).JK_UniqueConsignRef, DtbBookingSchema.Constants.Prefix));
			AssertEquals(masterTransportBooking.PK, GenericConsol.GetConsolPKByPrimaryCode(Factory, ((IJobCostingPlugIn)masterTransportBooking).JK_UniqueConsignRef));

			AssertEquals(manifest.PK, GenericConsol.GetConsolPKByPrimaryCode(Factory, ((IJobCostingPlugIn)manifest).JK_UniqueConsignRef, DtbLinehaulManifestSchema.Constants.Prefix));
			AssertEquals(manifest.PK, GenericConsol.GetConsolPKByPrimaryCode(Factory, ((IJobCostingPlugIn)manifest).JK_UniqueConsignRef));

			AssertEquals(workSheet.PK, GenericConsol.GetConsolPKByPrimaryCode(Factory, ((IJobCostingPlugIn)workSheet).JK_UniqueConsignRef, JobCartageRunSheetSchema.Constants.Prefix));
			AssertEquals(workSheet.PK, GenericConsol.GetConsolPKByPrimaryCode(Factory, ((IJobCostingPlugIn)workSheet).JK_UniqueConsignRef));

			AssertEquals(rtu.PK, GenericConsol.GetConsolPKByPrimaryCode(Factory, ((IJobCostingPlugIn)rtu).JK_UniqueConsignRef, WhsItemReceiveTransportationUnitSchema.Constants.Prefix));
			AssertEquals(rtu.PK, GenericConsol.GetConsolPKByPrimaryCode(Factory, ((IJobCostingPlugIn)rtu).JK_UniqueConsignRef));

			AssertEquals(dtu.PK, GenericConsol.GetConsolPKByPrimaryCode(Factory, ((IJobCostingPlugIn)dtu).JK_UniqueConsignRef, WhsItemDispatchTransportationUnitSchema.Constants.Prefix));
			AssertEquals(dtu.PK, GenericConsol.GetConsolPKByPrimaryCode(Factory, ((IJobCostingPlugIn)dtu).JK_UniqueConsignRef));

			AssertEquals(dll.PK, GenericConsol.GetConsolPKByPrimaryCode(Factory, ((IJobCostingPlugIn)dll).JK_UniqueConsignRef, WhsItemDispatchLoadListSchema.Constants.Prefix));
			AssertEquals(dll.PK, GenericConsol.GetConsolPKByPrimaryCode(Factory, ((IJobCostingPlugIn)dll).JK_UniqueConsignRef));
		}

		public void TestGetConsolPKByPrimaryCode_NoRowForNonMasterBooking()
		{
			var transportBookingSingleJobConsolParent = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			transportBookingSingleJobConsolParent[DtbBookingConsolidationSchema.KB_JobType] = TransportConsolidationJobTypes.Codes.Booking;
			var transportBooking = transportBookingSingleJobConsolParent.Bookings.AddNew();
			transportBooking.FillWithValidTestData();
			transportBooking.KM_JobType = TransportConsolidationJobTypes.Codes.Booking;
			AssertEquals("Precondition: No booking parent", ZGuid.Empty, transportBookingSingleJobConsolParent.KB_ParentID);
			AssertEquals("Precondition: No booking parent", string.Empty, transportBookingSingleJobConsolParent.KB_ParentTableCode);
			AssertEquals("Precondition: Not master booking", false, transportBooking.KM_IsMaster);

			Factory.Save();

			AssertEquals("Should return empty guid, as ViewGenericConsol should not have a row when there is no forwarding consol", ZGuid.Empty, GenericConsol.GetConsolPKByPrimaryCode(Factory, ((IJobCostingPlugIn)transportBooking).JK_UniqueConsignRef, DtbBookingSchema.Constants.Prefix));
			AssertEquals("Should return empty guid, as ViewGenericConsol should not have a row when there is no forwarding consol", ZGuid.Empty, GenericConsol.GetConsolPKByPrimaryCode(Factory, ((IJobCostingPlugIn)transportBooking).JK_UniqueConsignRef));
		}

		#endregion

		#region GetIJobCostingPlugInByPrimaryCode

		public void TestGetIJobCostingPlugInByPrimaryCode()
		{
			var forwardingConsol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			var runSheet = Factory.NewWithValidTestData<DtbConsignmentRunSheet>();
			var manifest = Factory.NewWithValidTestData<DtbLinehaulManifest>();
			var transportBookingConsol = (BusinessObject)Factory.New<IDtbBookingConsolidation>();
			transportBookingConsol.FillWithValidTestData();
			transportBookingConsol[DtbBookingConsolidationSchema.KB_JobType] = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;
			var forwardingConsol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			var transportBookingSingleJobConsolParent = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			transportBookingSingleJobConsolParent[DtbBookingConsolidationSchema.KB_JobType] = TransportConsolidationJobTypes.Codes.Booking;
			var transportBooking = transportBookingSingleJobConsolParent.Bookings.AddNew();
			transportBooking.FillWithValidTestData();
			transportBooking.KM_JobType = TransportConsolidationJobTypes.Codes.Booking;
			transportBookingSingleJobConsolParent[DtbBookingConsolidationSchema.KB_ParentID] = forwardingConsol2.PK;
			transportBookingSingleJobConsolParent[DtbBookingConsolidationSchema.KB_ParentTableCode] = JobConsolSchema.Constants.Prefix;
			var masterTransportBookingConsolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			masterTransportBookingConsolidation[DtbBookingConsolidationSchema.KB_JobType] = TransportConsolidationJobTypes.Codes.Booking;
			masterTransportBookingConsolidation[DtbBookingConsolidationSchema.KB_IsMaster] = true;
			var masterTransportBooking = masterTransportBookingConsolidation.Bookings.AddNew();
			masterTransportBooking.FillWithValidTestData();
			masterTransportBooking[DtbBookingSchema.KM_IsMaster] = true;
			var workSheet = Factory.NewWithValidTestData<CommonWorkSheet>();
			var rtu = (BusinessObject)Factory.New<IWhsItemReceiveTransportationUnit>();
			rtu.FillWithValidTestData();
			var dtu = (BusinessObject)Factory.New<IWhsItemDispatchTransportationUnit>();
			dtu.FillWithValidTestData();
			var dll = (BusinessObject)Factory.New<IWhsItemDispatchLoadList>();
			dll.FillWithValidTestData();

			Factory.Save();

			AssertEquals(forwardingConsol1, GenericConsol.GetIJobCostingPlugInByPrimaryCode(Factory, forwardingConsol1.JK_UniqueConsignRef, JobConsolSchema.Constants.Prefix));
			AssertEquals(forwardingConsol1, GenericConsol.GetIJobCostingPlugInByPrimaryCode(Factory, forwardingConsol1.JK_UniqueConsignRef));

			AssertEquals(runSheet, GenericConsol.GetIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)runSheet).JK_UniqueConsignRef, DtbConsignmentRunSheetSchema.Constants.Prefix));
			AssertEquals(runSheet, GenericConsol.GetIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)runSheet).JK_UniqueConsignRef));

			AssertEquals(transportBookingConsol, GenericConsol.GetIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)transportBookingConsol).JK_UniqueConsignRef, DtbBookingConsolidationSchema.Constants.Prefix));
			AssertEquals(transportBookingConsol, GenericConsol.GetIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)transportBookingConsol).JK_UniqueConsignRef));

			AssertEquals(transportBooking, GenericConsol.GetIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)transportBooking).JK_UniqueConsignRef, DtbBookingSchema.Constants.Prefix));
			AssertEquals(transportBooking, GenericConsol.GetIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)transportBooking).JK_UniqueConsignRef));

			AssertEquals("Precondition: No booking parent", ZGuid.Empty, masterTransportBookingConsolidation.KB_ParentID);
			AssertEquals("Precondition: No booking parent", string.Empty, masterTransportBookingConsolidation.KB_ParentTableCode);
			AssertEquals(masterTransportBooking.PK, GenericConsol.GetConsolPKByPrimaryCode(Factory, ((IJobCostingPlugIn)masterTransportBooking).JK_UniqueConsignRef, DtbBookingSchema.Constants.Prefix));
			AssertEquals(masterTransportBooking.PK, GenericConsol.GetConsolPKByPrimaryCode(Factory, ((IJobCostingPlugIn)masterTransportBooking).JK_UniqueConsignRef));

			AssertEquals(manifest, GenericConsol.GetIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)manifest).JK_UniqueConsignRef, DtbLinehaulManifestSchema.Constants.Prefix));
			AssertEquals(manifest, GenericConsol.GetIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)manifest).JK_UniqueConsignRef));

			AssertEquals(workSheet, GenericConsol.GetIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)workSheet).JK_UniqueConsignRef, JobCartageRunSheetSchema.Constants.Prefix));
			AssertEquals(workSheet, GenericConsol.GetIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)workSheet).JK_UniqueConsignRef));

			AssertEquals(rtu, GenericConsol.GetIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)rtu).JK_UniqueConsignRef, WhsItemReceiveTransportationUnitSchema.Constants.Prefix));
			AssertEquals(rtu, GenericConsol.GetIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)rtu).JK_UniqueConsignRef));

			AssertEquals(dtu, GenericConsol.GetIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)dtu).JK_UniqueConsignRef, WhsItemDispatchTransportationUnitSchema.Constants.Prefix));
			AssertEquals(dtu, GenericConsol.GetIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)dtu).JK_UniqueConsignRef));

			AssertEquals(dll, GenericConsol.GetIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)dll).JK_UniqueConsignRef, WhsItemDispatchLoadListSchema.Constants.Prefix));
			AssertEquals(dll, GenericConsol.GetIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)dll).JK_UniqueConsignRef));
		}

		#endregion

		#region GetIJobCostingPlugInBySecondaryCode

		public void TestGetIJobCostingPlugInBySecondaryCode()
		{
			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			forwardingConsol.JK_MasterBillNum = "123";

			Factory.Save();

			AssertEquals(forwardingConsol, GenericConsol.GetIJobCostingPlugInBySecondaryCode(Factory, forwardingConsol.JK_MasterBillNum, JobConsolSchema.Constants.Prefix));
			AssertEquals(forwardingConsol, GenericConsol.GetIJobCostingPlugInBySecondaryCode(Factory, forwardingConsol.JK_MasterBillNum));
		}

		#endregion

		#region GetCountofIJobCostingPlugInByPrimaryCode

		public void TestGetCountofIJobCostingPlugInByPrimaryCode()
		{
			var forwardingConsol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			var runSheet = Factory.NewWithValidTestData<DtbConsignmentRunSheet>();
			var manifest = Factory.NewWithValidTestData<DtbLinehaulManifest>();
			var transportBookingConsol = (BusinessObject)Factory.New<IDtbBookingConsolidation>();
			transportBookingConsol.FillWithValidTestData();
			transportBookingConsol[DtbBookingConsolidationSchema.KB_JobType] = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;
			var forwardingConsol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			var transportBookingSingleJobConsolParent = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			transportBookingSingleJobConsolParent[DtbBookingConsolidationSchema.KB_JobType] = TransportConsolidationJobTypes.Codes.Booking;
			var transportBooking = transportBookingSingleJobConsolParent.Bookings.AddNew();
			transportBooking.FillWithValidTestData();
			transportBooking.KM_JobType = TransportConsolidationJobTypes.Codes.Booking;
			transportBookingSingleJobConsolParent[DtbBookingConsolidationSchema.KB_ParentID] = forwardingConsol2.PK;
			transportBookingSingleJobConsolParent[DtbBookingConsolidationSchema.KB_ParentTableCode] = JobConsolSchema.Constants.Prefix;
			var masterTransportBookingConsolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			masterTransportBookingConsolidation[DtbBookingConsolidationSchema.KB_JobType] = TransportConsolidationJobTypes.Codes.Booking;
			masterTransportBookingConsolidation[DtbBookingConsolidationSchema.KB_IsMaster] = true;
			var masterTransportBooking = masterTransportBookingConsolidation.Bookings.AddNew();
			masterTransportBooking.FillWithValidTestData();
			masterTransportBooking[DtbBookingSchema.KM_IsMaster] = true;
			var workSheet = Factory.NewWithValidTestData<CommonWorkSheet>();
			var rtu = (BusinessObject)Factory.New<IWhsItemReceiveTransportationUnit>();
			rtu.FillWithValidTestData();
			var dtu = (BusinessObject)Factory.New<IWhsItemDispatchTransportationUnit>();
			dtu.FillWithValidTestData();
			var dll = (BusinessObject)Factory.New<IWhsItemDispatchLoadList>();
			dll.FillWithValidTestData();

			Factory.Save();

			AssertEquals(1, GenericConsol.GetCountofIJobCostingPlugInByPrimaryCode(Factory, forwardingConsol1.JK_UniqueConsignRef, JobConsolSchema.Constants.Prefix));
			AssertEquals(1, GenericConsol.GetCountofIJobCostingPlugInByPrimaryCode(Factory, forwardingConsol1.JK_UniqueConsignRef));

			AssertEquals(1, GenericConsol.GetCountofIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)runSheet).JK_UniqueConsignRef, DtbConsignmentRunSheetSchema.Constants.Prefix));
			AssertEquals(1, GenericConsol.GetCountofIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)runSheet).JK_UniqueConsignRef));

			AssertEquals(1, GenericConsol.GetCountofIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)transportBookingConsol).JK_UniqueConsignRef, DtbBookingConsolidationSchema.Constants.Prefix));
			AssertEquals(1, GenericConsol.GetCountofIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)transportBookingConsol).JK_UniqueConsignRef));

			AssertEquals(1, GenericConsol.GetCountofIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)transportBooking).JK_UniqueConsignRef, DtbBookingSchema.Constants.Prefix));
			AssertEquals(1, GenericConsol.GetCountofIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)transportBooking).JK_UniqueConsignRef));

			AssertEquals("Precondition: No booking parent", ZGuid.Empty, masterTransportBookingConsolidation.KB_ParentID);
			AssertEquals("Precondition: No booking parent", string.Empty, masterTransportBookingConsolidation.KB_ParentTableCode);
			AssertEquals(masterTransportBooking.PK, GenericConsol.GetConsolPKByPrimaryCode(Factory, ((IJobCostingPlugIn)masterTransportBooking).JK_UniqueConsignRef, DtbBookingSchema.Constants.Prefix));
			AssertEquals(masterTransportBooking.PK, GenericConsol.GetConsolPKByPrimaryCode(Factory, ((IJobCostingPlugIn)masterTransportBooking).JK_UniqueConsignRef));

			AssertEquals(1, GenericConsol.GetCountofIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)manifest).JK_UniqueConsignRef, DtbLinehaulManifestSchema.Constants.Prefix));
			AssertEquals(1, GenericConsol.GetCountofIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)manifest).JK_UniqueConsignRef));

			AssertEquals(1, GenericConsol.GetCountofIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)workSheet).JK_UniqueConsignRef, JobCartageRunSheetSchema.Constants.Prefix));
			AssertEquals(1, GenericConsol.GetCountofIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)workSheet).JK_UniqueConsignRef));

			AssertEquals(1, GenericConsol.GetCountofIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)rtu).JK_UniqueConsignRef, WhsItemReceiveTransportationUnitSchema.Constants.Prefix));
			AssertEquals(1, GenericConsol.GetCountofIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)rtu).JK_UniqueConsignRef));

			AssertEquals(1, GenericConsol.GetCountofIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)dtu).JK_UniqueConsignRef, WhsItemDispatchTransportationUnitSchema.Constants.Prefix));
			AssertEquals(1, GenericConsol.GetCountofIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)dtu).JK_UniqueConsignRef));

			AssertEquals(1, GenericConsol.GetCountofIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)dll).JK_UniqueConsignRef, WhsItemDispatchLoadListSchema.Constants.Prefix));
			AssertEquals(1, GenericConsol.GetCountofIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)dll).JK_UniqueConsignRef));
		}

		public void TestGetCountofIJobCostingPlugInByPrimaryCode_NoDuplicationForTransportBooking()
		{
			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			var transportBookingSingleJobConsolParent = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			transportBookingSingleJobConsolParent[DtbBookingConsolidationSchema.KB_JobType] = TransportConsolidationJobTypes.Codes.Booking;
			transportBookingSingleJobConsolParent[DtbBookingConsolidationSchema.KB_IsMaster] = true;
			var transportBooking = transportBookingSingleJobConsolParent.Bookings.AddNew();
			transportBooking.FillWithValidTestData();
			transportBooking.KM_JobType = TransportConsolidationJobTypes.Codes.Booking;
			transportBooking.KM_IsMaster = true;
			transportBookingSingleJobConsolParent[DtbBookingConsolidationSchema.KB_ParentID] = forwardingConsol.PK;
			transportBookingSingleJobConsolParent[DtbBookingConsolidationSchema.KB_ParentTableCode] = JobConsolSchema.Constants.Prefix;

			Factory.Save();

			AssertEquals("Should not have duplicate rows because a booking somehow is master and has a forwarding consol", 1, GenericConsol.GetCountofIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)transportBooking).JK_UniqueConsignRef, DtbBookingSchema.Constants.Prefix));
			AssertEquals("Should not have duplicate rows because a booking somehow is master and has a forwarding consol", 1, GenericConsol.GetCountofIJobCostingPlugInByPrimaryCode(Factory, ((IJobCostingPlugIn)transportBooking).JK_UniqueConsignRef));
		}

		#endregion

		#region GetCountofIJobCostingPlugInBySecondaryCode

		public void TestGetCountofIJobCostingPlugInBySecondaryCode()
		{
			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			forwardingConsol.JK_MasterBillNum = "123";

			Factory.Save();

			AssertEquals(1, GenericConsol.GetCountofIJobCostingPlugInBySecondaryCode(Factory, forwardingConsol.JK_MasterBillNum, JobConsolSchema.Constants.Prefix));
			AssertEquals(1, GenericConsol.GetCountofIJobCostingPlugInBySecondaryCode(Factory, forwardingConsol.JK_MasterBillNum));
		}

		#endregion

		#region GetIJobCostingPlugInByPK

		public void TestGetIJobCostingPlugInByPK()
		{
			var forwardingConsol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			var runSheet = Factory.NewWithValidTestData<DtbConsignmentRunSheet>();
			var manifest = Factory.NewWithValidTestData<DtbLinehaulManifest>();
			var transportBookingConsol = (BusinessObject)Factory.New<IDtbBookingConsolidation>();
			transportBookingConsol.FillWithValidTestData();
			transportBookingConsol[DtbBookingConsolidationSchema.KB_JobType] = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;
			var forwardingConsol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			var transportBookingSingleJobConsolParent = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			transportBookingSingleJobConsolParent[DtbBookingConsolidationSchema.KB_JobType] = TransportConsolidationJobTypes.Codes.Booking;
			var transportBooking = transportBookingSingleJobConsolParent.Bookings.AddNew();
			transportBooking.FillWithValidTestData();
			transportBooking.KM_JobType = TransportConsolidationJobTypes.Codes.Booking;
			transportBookingSingleJobConsolParent[DtbBookingConsolidationSchema.KB_ParentID] = forwardingConsol2.PK;
			transportBookingSingleJobConsolParent[DtbBookingConsolidationSchema.KB_ParentTableCode] = JobConsolSchema.Constants.Prefix;
			var masterTransportBookingConsolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			masterTransportBookingConsolidation[DtbBookingConsolidationSchema.KB_JobType] = TransportConsolidationJobTypes.Codes.Booking;
			masterTransportBookingConsolidation[DtbBookingConsolidationSchema.KB_IsMaster] = true;
			var masterTransportBooking = masterTransportBookingConsolidation.Bookings.AddNew();
			masterTransportBooking.FillWithValidTestData();
			masterTransportBooking[DtbBookingSchema.KM_IsMaster] = true;
			var workSheet = Factory.NewWithValidTestData<CommonWorkSheet>();
			var rtu = (BusinessObject)Factory.New<IWhsItemReceiveTransportationUnit>();
			rtu.FillWithValidTestData();
			var dtu = (BusinessObject)Factory.New<IWhsItemDispatchTransportationUnit>();
			dtu.FillWithValidTestData();
			var dll = (BusinessObject)Factory.New<IWhsItemDispatchLoadList>();
			dll.FillWithValidTestData();

			Factory.Save();

			AssertEquals(forwardingConsol1, GenericConsol.GetIJobCostingPlugInByPK(Factory, forwardingConsol1.PK, JobConsolSchema.Constants.Prefix));
			AssertEquals(forwardingConsol1, GenericConsol.GetIJobCostingPlugInByPK(Factory, forwardingConsol1.PK));

			AssertEquals(runSheet, GenericConsol.GetIJobCostingPlugInByPK(Factory, runSheet.PK, DtbConsignmentRunSheetSchema.Constants.Prefix));
			AssertEquals(runSheet, GenericConsol.GetIJobCostingPlugInByPK(Factory, runSheet.PK));

			AssertEquals(transportBookingConsol, GenericConsol.GetIJobCostingPlugInByPK(Factory, transportBookingConsol.PK, DtbBookingConsolidationSchema.Constants.Prefix));
			AssertEquals(transportBookingConsol, GenericConsol.GetIJobCostingPlugInByPK(Factory, transportBookingConsol.PK));

			AssertEquals(transportBooking, GenericConsol.GetIJobCostingPlugInByPK(Factory, transportBooking.PK, DtbBookingSchema.Constants.Prefix));
			AssertEquals(transportBooking, GenericConsol.GetIJobCostingPlugInByPK(Factory, transportBooking.PK));

			AssertEquals("Precondition: No booking parent", ZGuid.Empty, masterTransportBookingConsolidation.KB_ParentID);
			AssertEquals("Precondition: No booking parent", string.Empty, masterTransportBookingConsolidation.KB_ParentTableCode);
			AssertEquals(masterTransportBooking.PK, GenericConsol.GetConsolPKByPrimaryCode(Factory, ((IJobCostingPlugIn)masterTransportBooking).JK_UniqueConsignRef, DtbBookingSchema.Constants.Prefix));
			AssertEquals(masterTransportBooking.PK, GenericConsol.GetConsolPKByPrimaryCode(Factory, ((IJobCostingPlugIn)masterTransportBooking).JK_UniqueConsignRef));

			AssertEquals(manifest, GenericConsol.GetIJobCostingPlugInByPK(Factory, manifest.PK, DtbLinehaulManifestSchema.Constants.Prefix));
			AssertEquals(manifest, GenericConsol.GetIJobCostingPlugInByPK(Factory, manifest.PK));

			AssertEquals(workSheet, GenericConsol.GetIJobCostingPlugInByPK(Factory, workSheet.PK, JobCartageRunSheetSchema.Constants.Prefix));
			AssertEquals(workSheet, GenericConsol.GetIJobCostingPlugInByPK(Factory, workSheet.PK));

			AssertEquals(rtu, GenericConsol.GetIJobCostingPlugInByPK(Factory, rtu.PK, WhsItemReceiveTransportationUnitSchema.Constants.Prefix));
			AssertEquals(rtu, GenericConsol.GetIJobCostingPlugInByPK(Factory, rtu.PK));

			AssertEquals(dtu, GenericConsol.GetIJobCostingPlugInByPK(Factory, dtu.PK, WhsItemDispatchTransportationUnitSchema.Constants.Prefix));
			AssertEquals(dtu, GenericConsol.GetIJobCostingPlugInByPK(Factory, dtu.PK));

			AssertEquals(dll, GenericConsol.GetIJobCostingPlugInByPK(Factory, dll.PK, WhsItemDispatchLoadListSchema.Constants.Prefix));
			AssertEquals(dll, GenericConsol.GetIJobCostingPlugInByPK(Factory, dll.PK));
		}

		#endregion

		#region TestGetParentTableCodeFromParentId

		public void TestGetParentTableCodeFromParentIdForwardingConsol()
		{
			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			CoreTestGetParentTableCodeFromParentId(forwardingConsol, "Forwarding Consolidation");
		}

		public void TestGetParentTableCodeFromParentIdDtbConsignmentRunSheet()
		{
			var runSheet = Factory.NewWithValidTestData<DtbConsignmentRunSheet>();
			CoreTestGetParentTableCodeFromParentId(runSheet, "Transport Consignment Run Sheet");
		}

		public void TestGetParentTableCodeFromParentIdDtbLinehaulManifest()
		{
			var manifest = Factory.NewWithValidTestData<DtbLinehaulManifest>();
			CoreTestGetParentTableCodeFromParentId(manifest, "Linehaul Manifest");
		}

		public void TestGetParentTableCodeFromParentIdDtbBookingConsolidation()
		{
			var transportBookingConsol = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			transportBookingConsol[DtbBookingConsolidationSchema.KB_JobType] = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;
			CoreTestGetParentTableCodeFromParentId(transportBookingConsol, "Transport Booking Consolidation");
		}

		public void TestGetParentTableCodeFromParentIdDtbBooking()
		{
			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			var transportBookingSingleJobConsolParent = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			transportBookingSingleJobConsolParent[DtbBookingConsolidationSchema.KB_JobType] = TransportConsolidationJobTypes.Codes.Booking;
			var transportBooking = transportBookingSingleJobConsolParent.Bookings.AddNew();
			transportBooking.FillWithValidTestData();
			transportBooking.KM_JobType = TransportConsolidationJobTypes.Codes.Booking;
			transportBookingSingleJobConsolParent[DtbBookingConsolidationSchema.KB_ParentID] = forwardingConsol.PK;
			transportBookingSingleJobConsolParent[DtbBookingConsolidationSchema.KB_ParentTableCode] = JobConsolSchema.Constants.Prefix;
			CoreTestGetParentTableCodeFromParentId(transportBooking, "Transport Booking");
		}

		public void TestGetParentTableCodeFromParentIdDtbBooking_MasterBooking()
		{
			var masterTransportBookingConsolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			masterTransportBookingConsolidation[DtbBookingConsolidationSchema.KB_JobType] = TransportConsolidationJobTypes.Codes.Booking;
			masterTransportBookingConsolidation[DtbBookingConsolidationSchema.KB_IsMaster] = true;
			var masterTransportBooking = masterTransportBookingConsolidation.Bookings.AddNew();
			masterTransportBooking.FillWithValidTestData();
			masterTransportBooking[DtbBookingSchema.KM_IsMaster] = true;
			AssertEquals("Precondition: No booking parent", ZGuid.Empty, masterTransportBookingConsolidation.KB_ParentID);
			AssertEquals("Precondition: No booking parent", string.Empty, masterTransportBookingConsolidation.KB_ParentTableCode);

			CoreTestGetParentTableCodeFromParentId(masterTransportBooking, "Master Transport Booking");
		}

		public void TestGetParentTableCodeFromParentIdJobCartageRunSheet()
		{
			var workSheet = Factory.NewWithValidTestData<CommonWorkSheet>();
			CoreTestGetParentTableCodeFromParentId(workSheet, "Job Cartage Run Sheet");
		}

		public void TestGetParentTableCodeFromParentIdWhsItemReceiveTransportationUnit()
		{
			var rtu = (BusinessObject)Factory.New<IWhsItemReceiveTransportationUnit>();
			rtu.FillWithValidTestData();
			CoreTestGetParentTableCodeFromParentId(rtu, "Receive Transportation Unit");
		}

		public void TestGetParentTableCodeFromParentIdWhsItemDispatchTransportationUnit()
		{
			var dtu = (BusinessObject)Factory.New<IWhsItemDispatchTransportationUnit>();
			dtu.FillWithValidTestData();
			CoreTestGetParentTableCodeFromParentId(dtu, "Dispatch Transportation Unit");
		}

		public void TestGetParentTableCodeFromParentIdWhsItemDispatchLoadList()
		{
			var dll = (BusinessObject)Factory.New<IWhsItemDispatchLoadList>();
			dll.FillWithValidTestData();
			CoreTestGetParentTableCodeFromParentId(dll, "Dispatch Load List");
		}

		void CoreTestGetParentTableCodeFromParentId(BusinessObject businessObjectForGenericConsol, string businessObjectDescription)
		{
			Factory.Save();
			AssertEquals(FormattableString.Invariant($"Generic Consol GetParentTableCodeFromParentId() should match table code {businessObjectForGenericConsol.TablePrefix} on {businessObjectDescription}"), businessObjectForGenericConsol.TablePrefix, GenericConsol.GetParentTableCodeFromParentId(Factory, businessObjectForGenericConsol.PK));
		}

		#endregion

		#region TestLoadConsolBOFromParentIdAndCode

		public void TestLoadConsolBOFromParentIdAndCodeForwardingConsol()
		{
			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			CoreTestLoadConsolBOFromParentIdAndCode(forwardingConsol, "Forwarding Consolidation");
		}

		public void TestLoadConsolBOFromParentIdAndCodeDtbConsignmentRunSheet()
		{
			var runSheet = Factory.NewWithValidTestData<DtbConsignmentRunSheet>();
			CoreTestLoadConsolBOFromParentIdAndCode(runSheet, "Transport Consignment Run Sheet");
		}

		public void TestLoadConsolBOFromParentIdAndCodeLinehaulManifest()
		{
			var manifest = Factory.NewWithValidTestData<DtbLinehaulManifest>();
			CoreTestLoadConsolBOFromParentIdAndCode(manifest, "Linehaul Manifest");
		}

		public void TestLoadConsolBOFromParentIdAndCodeDtbBookingConsolidation()
		{
			var transportBookingConsol = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			CoreTestLoadConsolBOFromParentIdAndCode(transportBookingConsol, "Transport Booking Consolidation");
		}

		public void TestLoadConsolBOFromParentIdAndCodeDtbBooking()
		{
			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			var transportBookingSingleJobConsolParent = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			transportBookingSingleJobConsolParent[DtbBookingConsolidationSchema.KB_JobType] = TransportConsolidationJobTypes.Codes.Booking;
			var transportBooking = transportBookingSingleJobConsolParent.Bookings.AddNew();
			transportBookingSingleJobConsolParent[DtbBookingConsolidationSchema.KB_ParentID] = forwardingConsol.PK;
			transportBookingSingleJobConsolParent[DtbBookingConsolidationSchema.KB_ParentTableCode] = JobConsolSchema.Constants.Prefix;
			transportBooking.FillWithValidTestData();
			transportBooking.KM_JobType = TransportConsolidationJobTypes.Codes.Booking;
			CoreTestLoadConsolBOFromParentIdAndCode(transportBooking, "Transport Booking");
		}

		public void TestLoadConsolBOFromParentIdAndCodeDtbBooking_MasterBooking()
		{
			var masterTransportBookingConsolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			masterTransportBookingConsolidation[DtbBookingConsolidationSchema.KB_JobType] = TransportConsolidationJobTypes.Codes.Booking;
			masterTransportBookingConsolidation[DtbBookingConsolidationSchema.KB_IsMaster] = true;
			var masterTransportBooking = masterTransportBookingConsolidation.Bookings.AddNew();
			masterTransportBooking.FillWithValidTestData();
			masterTransportBooking[DtbBookingSchema.KM_IsMaster] = true;
			AssertEquals("Precondition: No booking parent", ZGuid.Empty, masterTransportBookingConsolidation.KB_ParentID);
			AssertEquals("Precondition: No booking parent", string.Empty, masterTransportBookingConsolidation.KB_ParentTableCode);
			CoreTestLoadConsolBOFromParentIdAndCode(masterTransportBooking, "Master Transport Booking");
		}

		public void TestLoadConsolBOFromParentIdAndCodeJobCartageRunSheet()
		{
			var workSheet = Factory.NewWithValidTestData<CommonWorkSheet>();
			CoreTestLoadConsolBOFromParentIdAndCode(workSheet, "Job Cartage Run Sheet");
		}

		public void TestLoadConsolBOFromParentIdAndCodeWhsItemReceiveTransportationUnit()
		{
			var rtu = (BusinessObject)Factory.New<IWhsItemReceiveTransportationUnit>();
			rtu.FillWithValidTestData();
			CoreTestLoadConsolBOFromParentIdAndCode(rtu, "Receive Transportation Unit");
		}

		public void TestLoadConsolBOFromParentIdAndCodeWhsItemDispatchTransportationUnit()
		{
			var dtu = (BusinessObject)Factory.New<IWhsItemDispatchTransportationUnit>();
			dtu.FillWithValidTestData();
			CoreTestLoadConsolBOFromParentIdAndCode(dtu, "Dispatch Transportation Unit");
		}

		void CoreTestLoadConsolBOFromParentIdAndCode(BusinessObject businessObjectForGenericConsol, string businessObjectDescription)
		{
			Factory.Save();
			AssertEquals("Load works for GenericConsol by PK and Table prefix for " + businessObjectDescription, businessObjectForGenericConsol, GenericConsol.LoadConsolBOFromParentIdAndCode(Factory, businessObjectForGenericConsol.PK, businessObjectForGenericConsol.TablePrefix));
		}

		#endregion

		#region TestLoadConsolBOFromParentIdAndCode_WithInvalidParentTableCode

		[ExpectExceptionMessage(typeof(ApplicationException), "Fail to find IJobCostingPlugIn due to VX_ParentTableCode is missing or invalid")]
		public void TestLoadConsolBOFromParentIdAndCode_WithInvalidParentTableCode()
		{
			GenericConsol.LoadConsolBOFromParentIdAndCode(Factory, ZGuid.Empty, "ZZ");
		}

		#endregion

		#region TestLoadConsolBOFromGenericConsol

		public void TestLoadConsolBOFromGenericConsolForwardingConsol()
		{
			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			CoreTestLoadConsolBOFromGenericConsol(forwardingConsol, "Forwarding Consolidation");
		}

		public void TestLoadConsolBOFromGenericConsolDtbConsignmentRunSheet()
		{
			var runSheet = Factory.NewWithValidTestData<DtbConsignmentRunSheet>();
			CoreTestLoadConsolBOFromGenericConsol(runSheet, "Transport Consignment Run Sheet");
		}

		public void TestLoadConsolBOFromGenericConsolDtbLinehaulManifest()
		{
			var manifest = Factory.NewWithValidTestData<DtbLinehaulManifest>();
			CoreTestLoadConsolBOFromGenericConsol(manifest, "Linehaul Manifest");
		}

		public void TestLoadConsolBOFromGenericConsolDtbBookingConsolidation()
		{
			var transportBookingConsol = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			transportBookingConsol[DtbBookingConsolidationSchema.KB_JobType] = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;
			CoreTestLoadConsolBOFromGenericConsol(transportBookingConsol, "Transport Booking Consolidation");
		}

		public void TestLoadConsolBOFromGenericConsolDtbBooking()
		{
			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			var transportBookingSingleJobConsolParent = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			transportBookingSingleJobConsolParent[DtbBookingConsolidationSchema.KB_JobType] = TransportConsolidationJobTypes.Codes.Booking;
			var transportBooking = transportBookingSingleJobConsolParent.Bookings.AddNew();
			transportBooking.FillWithValidTestData();
			transportBooking.KM_JobType = TransportConsolidationJobTypes.Codes.Booking;
			transportBookingSingleJobConsolParent[DtbBookingConsolidationSchema.KB_ParentID] = forwardingConsol.PK;
			transportBookingSingleJobConsolParent[DtbBookingConsolidationSchema.KB_ParentTableCode] = JobConsolSchema.Constants.Prefix;
			CoreTestLoadConsolBOFromGenericConsol(transportBooking, "Transport Booking");
		}

		public void TestLoadConsolBOFromGenericConsolDtbBooking_MasterBooking()
		{
			var masterTransportBookingConsolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			masterTransportBookingConsolidation[DtbBookingConsolidationSchema.KB_JobType] = TransportConsolidationJobTypes.Codes.Booking;
			masterTransportBookingConsolidation[DtbBookingConsolidationSchema.KB_IsMaster] = true;
			var masterTransportBooking = masterTransportBookingConsolidation.Bookings.AddNew();
			masterTransportBooking.FillWithValidTestData();
			masterTransportBooking[DtbBookingSchema.KM_IsMaster] = true;
			AssertEquals("Precondition: No booking parent", ZGuid.Empty, masterTransportBookingConsolidation.KB_ParentID);
			AssertEquals("Precondition: No booking parent", string.Empty, masterTransportBookingConsolidation.KB_ParentTableCode);
			CoreTestLoadConsolBOFromGenericConsol(masterTransportBooking, "Master Transport Booking");
		}

		public void TestLoadConsolBOFromGenericConsolJobCartageRunSheet()
		{
			var workSheet = Factory.NewWithValidTestData<CommonWorkSheet>();
			CoreTestLoadConsolBOFromGenericConsol(workSheet, "Job Cartage Run Sheet");
		}

		public void TestLoadConsolBOFromGenericConsolWhsItemReceiveTransportationUnit()
		{
			var rtu = (BusinessObject)Factory.New<IWhsItemReceiveTransportationUnit>();
			rtu.FillWithValidTestData();
			CoreTestLoadConsolBOFromGenericConsol(rtu, "Receive Transportation Unit");
		}

		public void TestLoadConsolBOFromGenericConsolWhsItemDispatchTransportationUnit()
		{
			var dtu = (BusinessObject)Factory.New<IWhsItemDispatchTransportationUnit>();
			dtu.FillWithValidTestData();
			CoreTestLoadConsolBOFromGenericConsol(dtu, "Dispatch Transportation Unit");
		}

		void CoreTestLoadConsolBOFromGenericConsol(BusinessObject businessObjectForGenericConsol, string businessObjectDescription)
		{
			Factory.Save();
			var genericConsol = Factory.Load<GenericConsol>(businessObjectForGenericConsol.PK);
			AssertEquals("Generic Consol LoadConsolBOFromGenericConsol should work for " + businessObjectDescription, businessObjectForGenericConsol, GenericConsol.LoadConsolBOFromGenericConsol(Factory, genericConsol));
		}

		#endregion

		#region TestLoadConsolBOFromGenericConsol_InvalidParentTableCode

		[ExpectExceptionMessage(typeof(ApplicationException), "Fail to find IJobCostingPlugIn due to VX_ParentTableCode is missing or invalid")]
		public void TestLoadConsolBOFromGenericConsol_InvalidParentTableCode()
		{
			var genericConsol = Factory.New<GenericConsol>();
			genericConsol.VX_ParentTableCode = "ZZ";

			GenericConsol.LoadConsolBOFromGenericConsol(Factory, genericConsol);
		}

		#endregion

		#region TestGetDataContextType

		public void TestGetDataContextTypeJobConsol()
		{
			CoreTestGetDataContextTypeNoException(JobConsolSchema.Constants.Prefix, DataContextType.ForwardingConsol);
		}

		public void TestGetDataContextTypeDtbBookingConsolidation()
		{
			CoreTestGetDataContextTypeNoException(DtbBookingConsolidationSchema.Constants.Prefix, DataContextType.TransportBookingConsolidation);
		}

		public void TestGetDataContextTypeDtbBooking()
		{
			CoreTestGetDataContextTypeNoException(DtbBookingSchema.Constants.Prefix, DataContextType.TransportBooking);
		}

		public void TestGetDataContextTypeDtbConsignmentRunSheet()
		{
			CoreTestGetDataContextTypeNoException(DtbConsignmentRunSheetSchema.Constants.Prefix, DataContextType.TransportConsignmentRunSheet);
		}

		public void TestGetDataContextTypeJobCartageRunSheet()
		{
			CoreTestGetDataContextTypeNoException(JobCartageRunSheetSchema.Constants.Prefix, DataContextType.LocalTransportRunSheet);
		}

		public void TestGetDataContextTypeWhsItemReceiveTransportationUnit()
		{
			CoreTestGetDataContextTypeNoException(WhsItemReceiveTransportationUnitSchema.Constants.Prefix, DataContextType.TransitReceiveHeader);
		}

		public void TestGetDataContextTypeWhsItemDispatchTransportationUnit()
		{
			CoreTestGetDataContextTypeNoException(WhsItemDispatchTransportationUnitSchema.Constants.Prefix, DataContextType.TransitDispatchHeader);
		}

		public void TestGetDataContextTypeWhsItemDispatchLoadList()
		{
			CoreTestGetDataContextTypeNoException(WhsItemDispatchLoadListSchema.Constants.Prefix, DataContextType.TransitDispatchLoadList);
		}

		public void TestGetDataContextTypeIfInvalidThrowsException()
		{
			var genericConsol = Factory.NewWithValidTestData<GenericConsol>();
			genericConsol.VX_ParentTableCode = "Boo";

			var exceptions = ErrorReporter.LastExceptionsReported();
			AssertEquals("No existing exception", 0, exceptions.Count);

			AssertExceptionThrown<NotSupportedException>(() => genericConsol.GetDataContextType.ToString());

			ErrorReporter.Clear();
		}

		void CoreTestGetDataContextTypeNoException(string parentTableCode, DataContextType expectedDataContextType)
		{
			var genericConsol = Factory.NewWithValidTestData<GenericConsol>();
			genericConsol.VX_ParentTableCode = parentTableCode;
			AssertEquals("DataContextType should be " + expectedDataContextType.ToString(), expectedDataContextType, genericConsol.GetDataContextType);
		}

		#endregion

		public void TestGetControllerIDJobConsol()
		{
			CoreTestGetControllerID(JobConsolSchema.Constants.Prefix, ControllerIDs.JobConsol);
		}

		public void TestGetControllerIDDtbBookingConsolidation()
		{
			CoreTestGetControllerID(DtbBookingConsolidationSchema.Constants.Prefix, ControllerIDs.DtbBookingConsolidation);
		}

		public void TestGetControllerIDDtbBooking()
		{
			CoreTestGetControllerID(DtbBookingSchema.Constants.Prefix, ControllerIDs.DtbBooking);
		}

		public void TestGetControllerIDDtbConsignmentRunSheet()
		{
			CoreTestGetControllerID(DtbConsignmentRunSheetSchema.Constants.Prefix, ControllerIDs.DtbConsignmentRunSheet);
		}

		public void TestGetControllerIDJobCartageRunSheet()
		{
			CoreTestGetControllerID(JobCartageRunSheetSchema.Constants.Prefix, ControllerIDs.CartageWorkSheet);
		}

		public void TestGetControllerIDWhsItemReceiveTransportationUnit()
		{
			CoreTestGetControllerID(WhsItemReceiveTransportationUnitSchema.Constants.Prefix, ControllerIDs.WhsItemReceiveTransportationUnit);
		}

		public void TestGetControllerIDWhsItemDispatchTransportationUnit()
		{
			CoreTestGetControllerID(WhsItemDispatchTransportationUnitSchema.Constants.Prefix, ControllerIDs.WhsItemDispatchTransportationUnit);
		}

		public void TestGetControllerIDAnythingElse()
		{
			CoreTestGetControllerID("XXX", ControllerIDs.JobConsol);
		}

		void CoreTestGetControllerID(string parentTableCode, ControllerID expectedControllerID)
		{
			var genericConsol = Factory.NewWithValidTestData<GenericConsol>();
			genericConsol.VX_ParentTableCode = parentTableCode;
			AssertEquals("GenericConsol should have correct ControllerID", expectedControllerID, genericConsol.GetParentConsolController());
		}

		#region TestETDandETA

		public void TestETDandETA()
		{
			SailingsForTestClasses sailingsHelper = new SailingsForTestClasses(Factory);

			OrgHeader shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_IsShippingProvider = true;
			shippingLine.OH_IsShippingLine = true;
			shippingLine.OH_FullName = "ShippingLine";
			shippingLine.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			JobSailing exportSailing = sailingsHelper.SydLaxSailing;
			exportSailing.Voyage.JV_OH_Line = shippingLine.PK;
			BuildConsolHelper consolHelper = new BuildConsolHelper();
			CommonConsol newConsol = Factory.New<CommonConsol>();
			consolHelper.BuildConsolFromSailing(newConsol, exportSailing);

			Factory.Save();
			GenericConsol consol = Factory.LoadTop1<GenericConsol>(new ZQuery(ViewGenericConsolSchema.PK, newConsol.PK));
			AssertEquals(newConsol.JK_JX_JA_E_DEP, consol.VX_ETD);
			AssertEquals(newConsol.JK_JX_JB_E_ARV, consol.VX_ETA);
		}

		#endregion

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			BusinessObject result = Factory.New(GetExpectedBusinessObjectType());
			return result;
		}

		#endregion
	}
}
