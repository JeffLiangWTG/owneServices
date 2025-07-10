using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.GlobalCommercialInvoice.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ComplianceRisk.Business.Test
{
	public class RelatedJobComplianceRiskStatusUpdater2Test : TestCaseWithFactory
	{
		public void TestRelatedShipmentComplianceRiskStatusUpdateForOrg()
		{
			var job = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobShipmentSchema.JS_OH_ExportBroker] = org.PK;

			AssertUpdateRelatedJobComplianceRiskStatusForOrgWithDifferentParty(job as IComplianceItemRiskStatusProvider, org, "UpdateRelatedShipmentsComplianceRiskStatusForOrg2");
		}

		public void TestRelatedShipmentComplianceRiskStatusUpdateForOrg_ByCreditor()
		{
			var job = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobShipmentSchema.JS_OH_Creditor] = org.PK;
			AssertUpdateRelatedJobComplianceRiskStatusForOrgWithDifferentParty(job as IComplianceItemRiskStatusProvider, org, "UpdateRelatedShipmentsComplianceRiskStatusForOrg2");
		}

		public void TestRelatedShipmentComplianceRiskStatusUpdateForOrg_ByServiceProvider()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "CZPRG";
			shipment.Services.AddNew();
			shipment.Services[0].ServiceProviderPK = org.PK;
			AssertUpdateRelatedJobComplianceRiskStatusForOrgWithDifferentParty(shipment, org, "UpdateRelatedShipmentsComplianceRiskStatusForOrg2");
		}

		public void TestRelatedShipmentComplianceRiskStatusUpdateForOrgWithChildShipments()
		{
			var job = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			var subJob = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			subJob[JobShipmentSchema.JS_JS_ColoadMasterShipment] = job.PK;
			subJob[JobShipmentSchema.JS_OH_ExportBroker] = org.PK;

			AssertUpdateRelatedJobComplianceRiskStatusForOrgWithDifferentParty(job as IComplianceItemRiskStatusProvider, org, "UpdateRelatedShipmentsComplianceRiskStatusForOrg2");
		}

		public void TestRelatedShipmentComplianceRiskStatusUpdateForOrgWithChildShipments_ByCreditor()
		{
			var job = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			var subJob = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			subJob[JobShipmentSchema.JS_JS_ColoadMasterShipment] = job.PK;
			subJob[JobShipmentSchema.JS_OH_Creditor] = org.PK;

			AssertUpdateRelatedJobComplianceRiskStatusForOrgWithDifferentParty(job as IComplianceItemRiskStatusProvider, org, "UpdateRelatedShipmentsComplianceRiskStatusForOrg2");
		}

		public void TestRelatedShipmentComplianceRiskStatusUpdateForOrgWithChildShipments_ByServiceProvider()
		{
			var job = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			var subJob = Factory.NewWithValidTestData<ForwardingShipment>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "CZPRG";
			subJob.Services.AddNew();
			subJob.Services[0].ServiceProviderPK = org.PK;
			subJob.JS_JS_ColoadMasterShipment = job.PK;

			AssertUpdateRelatedJobComplianceRiskStatusForOrgWithDifferentParty(job as IComplianceItemRiskStatusProvider, org, "UpdateRelatedShipmentsComplianceRiskStatusForOrg2");
		}

		public void TestRelatedShipmentComplianceRiskStatusUpdateForOrgWithGlobalCommercialInvoiceHeaders()
		{
			var job = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			var header = Factory.New<GlobalCommercialInvoiceHeader>();
			header.GIH_ParentID = job.PK;
			header.GIH_ParentTableCode = job.TablePrefix;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			header.GIH_OH_Supplier = org.PK;

			var shipment = job as IComplianceItemRiskStatusProvider;
			var complianceRiskStatus = SetupComplianceRiskStatus(shipment, org, null);

			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);
			new RelatedJobComplianceRiskStatusUpdater("UpdateRelatedShipmentsComplianceRiskStatusForOrg2", null).UpdateForOrg(org.PK.ToGuid(), new Guid(), null);

			complianceRiskStatus = new BusinessObjectFactory().Load<ComplianceRiskStatus>(complianceRiskStatus.PK);
			AssertEquals(ScreeningStatusesList.Codes.Matched, org.OH_ScreeningStatus);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
		}

		public void TestRelatedQuickBookingsComplianceRiskStatusUpdateForOrgWithGlobalCommercialInvoiceHeaders()
		{
			var job = (IQuotedBooking)ObjectFactory.GetType<IQuotedBooking>().InvokeMember(
				"New",
				System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
				null,
				null,
				new object[] { Freight.Integration.QuoteBookingType.QuickBooking, Factory });
			var newShipment = (BusinessObject)Factory.Load<Forwarding.IForwardingShipment>(job.ViewPK);
			var header = Factory.New<GlobalCommercialInvoiceHeader>();
			header.GIH_ParentID = newShipment.PK;
			header.GIH_ParentTableCode = newShipment.TablePrefix;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			header.GIH_OH_Supplier = org.PK;

			var quotedBooking = job as IComplianceItemRiskStatusProvider;
			var complianceRiskStatus = SetupComplianceRiskStatus(quotedBooking, org, null);

			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);
			new RelatedJobComplianceRiskStatusUpdater("UpdateRelatedQuotedBookingsComplianceRiskStatusForOrg2", null).UpdateForOrg(org.PK.ToGuid(), new Guid(), null);

			complianceRiskStatus = new BusinessObjectFactory().Load<ComplianceRiskStatus>(complianceRiskStatus.PK);
			AssertEquals(ScreeningStatusesList.Codes.Matched, org.OH_ScreeningStatus);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
		}

		public void TestRelatedQuotedBookingsComplianceRiskStatusUpdateForOrgWithGlobalCommercialInvoiceHeaders()
		{
			var ratingHeader = Factory.NewWithValidTestData<RatingHeader>();
			ratingHeader.TH_RateType = "QTE";
			ratingHeader.TH_QuoteDate = ZDateTime.UtcNow.Date;
			ratingHeader.TH_OneTimeQuote = true;

			var quotedShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			quotedShipment.JS_RL_NKOrigin = "AUSYD";
			quotedShipment.JS_RL_NKDestination = "USLAX";
			quotedShipment.JS_IsBooking = true;
			quotedShipment.JS_IsForwardRegistered = false;
			quotedShipment.JS_TH_OneTimeQuote = ratingHeader.PK;

			var viewQuotedBooking = Factory.NewWithValidTestData<ViewQuotedBooking>();
			viewQuotedBooking.VB_JS = quotedShipment.PK;
			viewQuotedBooking.VB_TH = ratingHeader.PK;

			var bookingWithQuote = (IQuotedBooking)ObjectFactory.GetType<IQuotedBooking>().InvokeMember(
				"New",
				System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
				null,
				null,
				new object[] { Freight.Integration.QuoteBookingType.BookingWithQuote, Factory });

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			var invoiceheader = Factory.New<GlobalCommercialInvoiceHeader>();
			invoiceheader.GIH_ParentID = ratingHeader.PK;
			invoiceheader.GIH_ParentTableCode = ratingHeader.TablePrefix;
			invoiceheader.GIH_OH_Importer = orgHeader.PK;

			var quotedBooking = bookingWithQuote as IComplianceItemRiskStatusProvider;

			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = ratingHeader.PK;
			complianceRiskStatus.COR_ParentTableCode = ratingHeader.TablePrefix;
			complianceRiskStatus.COR_PartyRisk = "CLR";
			complianceRiskStatus.COR_LocationRisk = "CLR";
			complianceRiskStatus.COR_CommodityRisk = "CLR";
			complianceRiskStatus.COR_OverallRisk = "CLR";
			SetComplianceRiskStatusJobEndDate(complianceRiskStatus);

			Factory.Save();

			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);
			new RelatedJobComplianceRiskStatusUpdater("UpdateRelatedQuotedBookingsComplianceRiskStatusForOrg2", null).UpdateForOrg(orgHeader.PK.ToGuid(), new Guid(), null);

			complianceRiskStatus = new BusinessObjectFactory().Load<ComplianceRiskStatus>(complianceRiskStatus.PK);
			AssertEquals(ScreeningStatusesList.Codes.Matched, orgHeader.OH_ScreeningStatus);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
		}

		public void TestRelatedConsolsComplianceRiskStatusUpdateForOrgWithGlobalCommercialInvoiceHeaders()
		{
			var consol = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();

			var shipment = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			var conShipLink = (BusinessObject)Factory.New<Freight.Integration.IJobConShipLink>();

			conShipLink[JobConShipLinkSchema.JN_JK] = consol.PK;
			conShipLink[JobConShipLinkSchema.JN_JS] = shipment.PK;

			var header = Factory.New<GlobalCommercialInvoiceHeader>();
			header.GIH_ParentID = shipment.PK;
			header.GIH_ParentTableCode = shipment.TablePrefix;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			header.GIH_OH_Supplier = org.PK;

			var complianceJob = consol as IComplianceItemRiskStatusProvider;
			var complianceRiskStatus = SetupComplianceRiskStatus(complianceJob, org, null);

			AssertEquals(ScreeningStatusesList.Codes.Matched, org.OH_ScreeningStatus);

			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);
			new RelatedJobComplianceRiskStatusUpdater("UpdateRelatedConsolsComplianceRiskStatusForOrg2", null).UpdateForOrg(org.PK.ToGuid(), new Guid(), null);

			complianceRiskStatus = new BusinessObjectFactory().Load<ComplianceRiskStatus>(complianceRiskStatus.PK);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
		}

		public void TestRelatedShipmentComplianceRiskStatusUpdateForOrg_ConsiderLocationAndCommodityRisk()
		{
			var job = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobShipmentSchema.JS_OH_ExportBroker] = org.PK;

			var shipment = job as IComplianceItemRiskStatusProvider;
			var complianceRiskStatus = CreateComplianceRiskStatus(shipment, commodityRisk: ComplianceRiskStatusCodeList.Codes.Incomplete);
			Factory.Save();

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			AssertConsiderLocationAndCommodityRisk("UpdateRelatedShipmentsComplianceRiskStatusForOrg2", complianceRiskStatus, shipment, org, null);
		}

		public void TestRelatedShipmentComplianceRiskStatusUpdateForVessel()
		{
			var job = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "SHP";
			transport[JobConsolTransportSchema.JW_Vessel] = vessel.RV_Code;

			var shipment = job as IComplianceItemRiskStatusProvider;
			var complianceRiskStatus = SetupComplianceRiskStatus(shipment, null, vessel);

			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);
			new RelatedJobComplianceRiskStatusUpdater(null, "UpdateRelatedShipmentsComplianceRiskStatusForVessel2").UpdateForVessel(vessel.PK.ToGuid(), new Guid(), null);
			complianceRiskStatus = new BusinessObjectFactory().Load<ComplianceRiskStatus>(complianceRiskStatus.PK);
			AssertEquals(ScreeningStatusesList.Codes.Matched, vessel.RV_ScreeningStatus);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
		}

		public void TestRelatedShipmentComplianceRiskStatusUpdateForVesselWithChildShipments()
		{
			var job = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			var subJob = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = subJob.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = subJob.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "SHP";
			transport[JobConsolTransportSchema.JW_Vessel] = vessel.RV_Code;

			var shipment = job as IComplianceItemRiskStatusProvider;
			subJob[JobShipmentSchema.JS_JS_ColoadMasterShipment] = job.PK;

			var complianceRiskStatus = SetupComplianceRiskStatus(shipment, null, vessel);

			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);
			new RelatedJobComplianceRiskStatusUpdater(null, "UpdateRelatedShipmentsComplianceRiskStatusForVessel2").UpdateForVessel(vessel.PK.ToGuid(), new Guid(), null);
			complianceRiskStatus = new BusinessObjectFactory().Load<ComplianceRiskStatus>(complianceRiskStatus.PK);
			AssertEquals(ScreeningStatusesList.Codes.Matched, vessel.RV_ScreeningStatus);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
		}

		public void TestRelatedShipmentComplianceRiskStatusUpdateForVessel_ConsiderLocationAndCommodityRisk()
		{
			var job = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "SHP";
			transport[JobConsolTransportSchema.JW_Vessel] = vessel.RV_Code;

			var shipment = job as IComplianceItemRiskStatusProvider;
			var complianceRiskStatus = CreateComplianceRiskStatus(shipment, commodityRisk: ComplianceRiskStatusCodeList.Codes.Incomplete);
			Factory.Save();

			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			AssertConsiderLocationAndCommodityRisk("UpdateRelatedShipmentsComplianceRiskStatusForVessel2", complianceRiskStatus, shipment, null, vessel);
		}

		public void TestRelatedConsolComplianceRiskStatusUpdateForOrg()
		{
			var job = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobConsolSchema.JK_OA_CreditorAddress] = org.MainAddress.PK;

			AssertUpdateRelatedJobComplianceRiskStatusForOrgWithDifferentParty(job as IComplianceItemRiskStatusProvider, org, "UpdateRelatedConsolsComplianceRiskStatusForOrg2");
		}

		public void TestRelatedConsolComplianceRiskStatusUpdateForOrgWithChildShipments()
		{
			var consol = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();
			var shipment = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			var subShipment = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			var conShipLink = (BusinessObject)Factory.New<Freight.Integration.IJobConShipLink>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			subShipment[JobShipmentSchema.JS_OH_ExportBroker] = org.PK;
			subShipment[JobShipmentSchema.JS_JS_ColoadMasterShipment] = shipment.PK;
			conShipLink[JobConShipLinkSchema.JN_JK] = consol.PK;
			conShipLink[JobConShipLinkSchema.JN_JS] = shipment.PK;

			AssertUpdateRelatedJobComplianceRiskStatusForOrgWithDifferentParty(consol as IComplianceItemRiskStatusProvider, org, "UpdateRelatedConsolsComplianceRiskStatusForOrg2");
		}

		public void TestRelatedConsolComplianceRiskStatusUpdateForOrgWithChildShipments_ByCreditor()
		{
			var consol = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();
			var shipment = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			var subShipment = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			var conShipLink = (BusinessObject)Factory.New<Freight.Integration.IJobConShipLink>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			subShipment[JobShipmentSchema.JS_OH_Creditor] = org.PK;
			subShipment[JobShipmentSchema.JS_JS_ColoadMasterShipment] = shipment.PK;
			conShipLink[JobConShipLinkSchema.JN_JK] = consol.PK;
			conShipLink[JobConShipLinkSchema.JN_JS] = shipment.PK;

			AssertUpdateRelatedJobComplianceRiskStatusForOrgWithDifferentParty(consol as IComplianceItemRiskStatusProvider, org, "UpdateRelatedConsolsComplianceRiskStatusForOrg2");
		}

		public void TestRelatedConsolComplianceRiskStatusUpdateForOrgWithChildShipments_ByServiceProvider()
		{
			var consol = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();
			var shipment = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			var subShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var conShipLink = (BusinessObject)Factory.New<Freight.Integration.IJobConShipLink>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "CZPRG";
			subShipment.Services.AddNew();
			subShipment.Services[0].ServiceProviderPK = org.PK;
			subShipment.JS_JS_ColoadMasterShipment = shipment.PK;
			conShipLink[JobConShipLinkSchema.JN_JK] = consol.PK;
			conShipLink[JobConShipLinkSchema.JN_JS] = shipment.PK;

			AssertUpdateRelatedJobComplianceRiskStatusForOrgWithDifferentParty(consol as IComplianceItemRiskStatusProvider, org, "UpdateRelatedConsolsComplianceRiskStatusForOrg2");
		}

		public void TestRelatedConsolComplianceRiskStatusUpdateForOrg_ConsiderLocationAndCommodityRisk()
		{
			var job = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobConsolSchema.JK_OA_CreditorAddress] = org.MainAddress.PK;

			var consol = job as IComplianceItemRiskStatusProvider;
			var complianceRiskStatus = CreateComplianceRiskStatus(consol, commodityRisk: ComplianceRiskStatusCodeList.Codes.Incomplete);
			Factory.Save();

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			AssertConsiderLocationAndCommodityRisk("UpdateRelatedConsolsComplianceRiskStatusForOrg2", complianceRiskStatus, consol, org, null);
		}

		public void TestRelatedConsolComplianceRiskStatusUpdateForOrgWithTimeFilter()
		{
			var consol = Factory.New<Forwarding.IForwardingConsol>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			((BusinessObject)consol)[JobConsolSchema.JK_OA_CreditorAddress] = org.MainAddress.PK;

			var transport = consol.Transports_Get(0);
			transport.JW_ETA = ZDateTime.Now.AddDays(-9);

			var complianceJob = consol as IComplianceItemRiskStatusProvider;
			var complianceRiskStatus = SetupComplianceRiskStatus(complianceJob, org, null, transport.JW_ETA.ToDateTime());
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);

			new RelatedJobComplianceRiskStatusUpdater("UpdateRelatedConsolsComplianceRiskStatusForOrg2", null).UpdateForOrg(org.PK.ToGuid(), new Guid(), null);
			complianceRiskStatus.Reload();

			AssertEquals("Not update consol when consol is not a current job", ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);

			transport.JW_ETA = ZDateTime.Now.AddDays(-2);
			SetComplianceRiskStatusJobEndDate(complianceRiskStatus, transport.JW_ETA.ToDateTime());
			Factory.Save();

			new RelatedJobComplianceRiskStatusUpdater("UpdateRelatedConsolsComplianceRiskStatusForOrg2", null).UpdateForOrg(org.PK.ToGuid(), new Guid(), null);
			complianceRiskStatus.Reload();

			AssertEquals(ScreeningStatusesList.Codes.Matched, org.OH_ScreeningStatus);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
		}

		public void TestRelatedConsolComplianceRiskStatusUpdateForOrgWithNotClearVessel()
		{
			var job = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobConsolSchema.JK_OA_CreditorAddress] = org.MainAddress.PK;

			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "CON";
			transport[JobConsolTransportSchema.JW_Vessel] = vessel.RV_Code;

			var consol = job as IComplianceItemRiskStatusProvider;
			var complianceRiskStatus = CreateComplianceRiskStatus(consol);

			Factory.Save();

			org.OH_ScreeningStatus = "CLR";
			Factory.Save();

			AssertEquals("CLR", org.OH_ScreeningStatus);
			AssertEquals("NOT", vessel.RV_ScreeningStatus);

			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);
			new RelatedJobComplianceRiskStatusUpdater("UpdateRelatedConsolsComplianceRiskStatusForOrg2", null).UpdateForOrg(org.PK.ToGuid(), new Guid(), null);
			var logs = GetComplianceLogs(consol);

			complianceRiskStatus = new BusinessObjectFactory().Load<ComplianceRiskStatus>(complianceRiskStatus.PK);
			AssertEquals("HLD", complianceRiskStatus.COR_OverallRisk);
		}

		public void TestRelatedConsolComplianceRiskStatusUpdateForVessel()
		{
			var job = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "CON";
			transport[JobConsolTransportSchema.JW_Vessel] = vessel.RV_Code;

			var consol = job as IComplianceItemRiskStatusProvider;
			var complianceRiskStatus = SetupComplianceRiskStatus(consol, null, vessel);

			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);
			new RelatedJobComplianceRiskStatusUpdater(null, "UpdateRelatedConsolsComplianceRiskStatusForVessel2").UpdateForVessel(vessel.PK.ToGuid(), new Guid(), null);
			complianceRiskStatus = new BusinessObjectFactory().Load<ComplianceRiskStatus>(complianceRiskStatus.PK);
			AssertEquals(ScreeningStatusesList.Codes.Matched, vessel.RV_ScreeningStatus);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
		}

		public void TestRelatedConsolComplianceRiskStatusUpdateForVessel_ConsiderLocationAndCommodityRisk()
		{
			var job = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "CON";
			transport[JobConsolTransportSchema.JW_Vessel] = vessel.RV_Code;

			var consol = job as IComplianceItemRiskStatusProvider;
			var complianceRiskStatus = CreateComplianceRiskStatus(consol, commodityRisk: ComplianceRiskStatusCodeList.Codes.Incomplete);
			Factory.Save();

			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			AssertConsiderLocationAndCommodityRisk("UpdateRelatedConsolsComplianceRiskStatusForVessel2", complianceRiskStatus, consol, null, vessel);
		}

		public void TestRelatedConsolComplianceRiskStatusUpdateForVesselWithChildShipments()
		{
			var consol = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();
			var shipment = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			var subShipment = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = subShipment.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = subShipment.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "SHP";
			transport[JobConsolTransportSchema.JW_Vessel] = vessel.RV_Code;

			subShipment[JobShipmentSchema.JS_JS_ColoadMasterShipment] = shipment.PK;

			var shipLink = Factory.New<Freight.Integration.IJobConShipLink>();
			shipLink.JN_JK = consol.PK;
			shipLink.JN_JS = shipment.PK;

			var complianceJob = consol as IComplianceItemRiskStatusProvider;
			var complianceRiskStatus = SetupComplianceRiskStatus(complianceJob, null, vessel);

			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);
			new RelatedJobComplianceRiskStatusUpdater(null, "UpdateRelatedConsolsComplianceRiskStatusForVessel2").UpdateForVessel(vessel.PK.ToGuid(), new Guid(), null);
			complianceRiskStatus = new BusinessObjectFactory().Load<ComplianceRiskStatus>(complianceRiskStatus.PK);
			AssertEquals(ScreeningStatusesList.Codes.Matched, vessel.RV_ScreeningStatus);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
		}

		public void TestRelatedConsolComplianceRiskStatusUpdateForVesselWithTimeFilter()
		{
			var job = Factory.New<Forwarding.IForwardingConsol>();
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var transport = job.Transports_Get(0);
			transport.ParentType = job.GetType();
			transport.JW_ParentGUID = job.PK;
			transport.JW_Vessel = vessel.RV_Code;
			transport.JW_ETA = ZDateTime.Now.AddDays(-9);

			var consol = job as IComplianceItemRiskStatusProvider;
			var complianceRiskStatus = SetupComplianceRiskStatus(consol, null, vessel, transport.JW_ETA.ToDateTime());
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);

			new RelatedJobComplianceRiskStatusUpdater(null, "UpdateRelatedConsolsComplianceRiskStatusForVessel2").UpdateForVessel(vessel.PK.ToGuid(), new Guid(), null);
			complianceRiskStatus.Reload();

			AssertEquals("Not update consol when consol is not a current job", ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);

			transport.JW_ETA = ZDateTime.Now.AddDays(-2);
			SetComplianceRiskStatusJobEndDate(complianceRiskStatus, transport.JW_ETA.ToDateTime());
			Factory.Save();

			new RelatedJobComplianceRiskStatusUpdater(null, "UpdateRelatedConsolsComplianceRiskStatusForVessel2").UpdateForVessel(vessel.PK.ToGuid(), new Guid(), null);
			complianceRiskStatus.Reload();

			AssertEquals(ScreeningStatusesList.Codes.Matched, vessel.RV_ScreeningStatus);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
		}

		public void TestRelatedQuotedBookingComplianceRiskStatusUpdateForOrg()
		{
			var job = (IQuotedBooking)ObjectFactory.GetType<IQuotedBooking>().InvokeMember(
				"New",
				System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
				null,
				null,
				new object[] { Freight.Integration.QuoteBookingType.QuickBooking, Factory });

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var newShipment = (BusinessObject)Factory.Load<Forwarding.IForwardingShipment>(job.ViewPK);
			newShipment[JobShipmentSchema.JS_OH_ExportBroker] = org.PK;

			AssertUpdateRelatedJobComplianceRiskStatusForOrgWithDifferentParty(job as IComplianceItemRiskStatusProvider, org, "UpdateRelatedQuotedBookingsComplianceRiskStatusForOrg2");
		}

		public void TestRelatedQuotedBookingComplianceRiskStatusUpdateForOrg_ByCreditor()
		{
			var job = (IQuotedBooking)ObjectFactory.GetType<IQuotedBooking>().InvokeMember(
				"New",
				System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
				null,
				null,
				new object[] { Freight.Integration.QuoteBookingType.QuickBooking, Factory });

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var newShipment = (BusinessObject)Factory.Load<Forwarding.IForwardingShipment>(job.ViewPK);
			newShipment[JobShipmentSchema.JS_OH_Creditor] = org.PK;

			AssertUpdateRelatedJobComplianceRiskStatusForOrgWithDifferentParty(job as IComplianceItemRiskStatusProvider, org, "UpdateRelatedQuotedBookingsComplianceRiskStatusForOrg2");
		}

		public void TestRelatedQuotedBookingComplianceRiskStatusUpdateForOrg_ByServiceProvider()
		{
			var job = (IQuotedBooking)ObjectFactory.GetType<IQuotedBooking>().InvokeMember(
				"New",
				System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
				null,
				null,
				new object[] { Freight.Integration.QuoteBookingType.QuickBooking, Factory });

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "CZPRG";
			var newShipment = Factory.Load<ForwardingShipment>(job.ViewPK);
			newShipment.Services.AddNew();
			newShipment.Services[0].ServiceProviderPK = org.PK;

			AssertUpdateRelatedJobComplianceRiskStatusForOrgWithDifferentParty(job as IComplianceItemRiskStatusProvider, org, "UpdateRelatedQuotedBookingsComplianceRiskStatusForOrg2");
		}

		public void TestRelatedQuotedBookingComplianceRiskStatusUpdateForOrg_ConsiderLocationAndCommodityRisk()
		{
			var job = (IQuotedBooking)ObjectFactory.GetType<IQuotedBooking>().InvokeMember(
				"New",
				System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
				null,
				null,
				new object[] { Freight.Integration.QuoteBookingType.QuickBooking, Factory });

			var quotedBooking = job as IComplianceItemRiskStatusProvider;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var newShipment = (BusinessObject)Factory.Load<Forwarding.IForwardingShipment>(job.ViewPK);
			newShipment[JobShipmentSchema.JS_OH_ExportBroker] = org.PK;

			var complianceRiskStatus = CreateComplianceRiskStatus(quotedBooking, commodityRisk: ComplianceRiskStatusCodeList.Codes.Incomplete);
			Factory.Save();

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			AssertConsiderLocationAndCommodityRisk("UpdateRelatedQuotedBookingsComplianceRiskStatusForOrg2", complianceRiskStatus, quotedBooking, org, null);
		}

		public void TestRelatedQuotedBookingComplianceRiskStatusUpdateForVessel()
		{
			var job = (IQuotedBooking)ObjectFactory.GetType<IQuotedBooking>().InvokeMember(
				"New",
				System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
				null,
				null,
				new object[] { Freight.Integration.QuoteBookingType.QuickBooking, Factory });

			var quotedBooking = job as IComplianceItemRiskStatusProvider;
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var newShipment = (BusinessObject)Factory.Load<Forwarding.IForwardingShipment>(job.ViewPK);
			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = newShipment.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = newShipment.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "SHP";
			transport[JobConsolTransportSchema.JW_Vessel] = vessel.RV_Code;

			var complianceRiskStatus = SetupComplianceRiskStatus(quotedBooking, null, vessel);

			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);
			new RelatedJobComplianceRiskStatusUpdater(null, "UpdateRelatedQuotedBookingsComplianceRiskStatusForVessel2").UpdateForVessel(vessel.PK.ToGuid(), new Guid(), null);
			complianceRiskStatus = new BusinessObjectFactory().Load<ComplianceRiskStatus>(complianceRiskStatus.PK);

			AssertEquals(ScreeningStatusesList.Codes.Matched, vessel.RV_ScreeningStatus);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
		}

		public void TestRelatedQuotedBookingComplianceRiskStatusUpdateForVessel_ConsiderLocationAndCommodityRisk()
		{
			var job = (IQuotedBooking)ObjectFactory.GetType<IQuotedBooking>().InvokeMember(
				"New",
				System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
				null,
				null,
				new object[] { Freight.Integration.QuoteBookingType.QuickBooking, Factory });

			var quotedBooking = job as IComplianceItemRiskStatusProvider;
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var newShipment = (BusinessObject)Factory.Load<Forwarding.IForwardingShipment>(job.ViewPK);
			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = newShipment.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = newShipment.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "SHP";
			transport[JobConsolTransportSchema.JW_Vessel] = vessel.RV_Code;

			var complianceRiskStatus = CreateComplianceRiskStatus(quotedBooking, commodityRisk: ComplianceRiskStatusCodeList.Codes.Incomplete);
			Factory.Save();

			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			AssertConsiderLocationAndCommodityRisk("UpdateRelatedQuotedBookingsComplianceRiskStatusForVessel2", complianceRiskStatus, quotedBooking, null, vessel);
		}

		public void TestUpdateForOrg()
		{
			var updater = new DummyJobComplianceRiskStatusUpdater();
			var companyPK = Guid.NewGuid();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "OLDCODE";
			Factory.Save();

			updater.UpdateForOrg(orgHeader.PK.ToGuid(), companyPK, null);
			orgHeader.Reload();
			AssertEquals("NEWCODE0", orgHeader.OH_Code);
			AssertEquals(companyPK, updater.CompanyPK);

			var useActionToExcuteCommand = false;
			updater.UpdateForOrg(orgHeader.PK.ToGuid(), companyPK, (cmd, connection) =>
			{
				useActionToExcuteCommand = true;
			});

			orgHeader.Reload();
			AssertEquals(true, useActionToExcuteCommand);
			AssertEquals("NEWCODE0", orgHeader.OH_Code);
		}

		public void TestUpdateForVessel()
		{
			var updater = new DummyJobComplianceRiskStatusUpdater();
			var companyPK = Guid.NewGuid();
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "OLDCODE";
			Factory.Save();

			updater.UpdateForVessel(vessel.PK.ToGuid(), companyPK, null);
			vessel.Reload();
			AssertEquals("NEWCODE0", vessel.RV_Code);
			AssertEquals(companyPK, updater.CompanyPK);

			var useActionToExcuteCommand = false;
			updater.UpdateForVessel(vessel.PK.ToGuid(), companyPK, (cmd, connection) =>
			{
				useActionToExcuteCommand = true;
			});

			vessel.Reload();
			AssertEquals(true, useActionToExcuteCommand);
			AssertEquals("NEWCODE0", vessel.RV_Code);
		}

		public void TestUpdateRelatedShipmentsComplianceRiskStatusForOrg_AddComplianceRiskSTULogs()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			org1.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			org2.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			var job = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			job[JobShipmentSchema.JS_OH_ExportBroker] = org1.PK;
			job[JobShipmentSchema.JS_OH_ImportBroker] = org2.PK;

			AssertComplianceRiskLogsResults("UpdateRelatedShipmentsComplianceRiskStatusForOrg2", org1, org2, job, "JobShipment");
		}

		public void TestUpdateRelatedShipmentsComplianceRiskStatusForVessel_AddComplianceRiskSTULogs()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			org2.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			var job = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();

			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "SHP";
			transport[JobConsolTransportSchema.JW_Vessel] = vessel.RV_Code;

			job[JobShipmentSchema.JS_OH_ImportBroker] = org2.PK;

			AssertComplianceRiskLogsResults("UpdateRelatedShipmentsComplianceRiskStatusForVessel2", vessel, org2, job, "JobShipment");
		}

		public void TestUpdateRelatedConsolsComplianceRiskStatusForOrg_AddComplianceRiskSTULogs()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			org1.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			org2.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			var job = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();
			job[JobConsolSchema.JK_OA_CreditorAddress] = org1.MainAddress.PK;
			job[JobConsolSchema.JK_OA_ShippingLineAddress] = org2.MainAddress.PK;

			AssertComplianceRiskLogsResults("UpdateRelatedConsolsComplianceRiskStatusForOrg2", org1, org2, job, "JobConsol");
		}

		public void TestUpdateRelatedConsolsComplianceRiskStatusForVessel_AddComplianceRiskSTULogs()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			org2.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			var job = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();

			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "CON";
			transport[JobConsolTransportSchema.JW_Vessel] = vessel.RV_Code;

			job[JobConsolSchema.JK_OA_CreditorAddress] = org2.MainAddress.PK;

			AssertComplianceRiskLogsResults("UpdateRelatedConsolsComplianceRiskStatusForVessel2", vessel, org2, job, "JobConsol");
		}

		public void TestUpdateRelatedQuotedBookingsComplianceRiskStatusForOrg_AddComplianceRiskSTULogs()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			org1.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			org2.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			var job = (IQuotedBooking)ObjectFactory.GetType<IQuotedBooking>().InvokeMember(
				"New",
				System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
				null,
				null,
				new object[] { Freight.Integration.QuoteBookingType.BookingWithQuote, Factory });

			job.ForwardingShipment[JobShipmentSchema.JS_OH_ExportBroker] = org1.PK;
			job.ForwardingShipment[JobShipmentSchema.JS_OH_ImportBroker] = org2.PK;

			AssertEquals(RatingHeaderSchema.Constants.Prefix, (job as IComplianceItemRiskStatusProvider).ParentTableCode);
			AssertComplianceRiskLogsResults("UpdateRelatedQuotedBookingsComplianceRiskStatusForOrg2", org1, org2, job as BusinessObject, "RatingHeader");
		}

		public void TestUpdateRelatedQuotedBookingsComplianceRiskStatusForVessel_AddComplianceRiskSTULogs()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			org2.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			var job = (IQuotedBooking)ObjectFactory.GetType<IQuotedBooking>().InvokeMember(
				"New",
				System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
				null,
				null,
				new object[] { Freight.Integration.QuoteBookingType.BookingWithQuote, Factory });

			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.ForwardingShipment.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.ForwardingShipment.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "SHP";
			transport[JobConsolTransportSchema.JW_Vessel] = vessel.RV_Code;

			job.ForwardingShipment[JobShipmentSchema.JS_OH_ImportBroker] = org2.PK;

			AssertEquals(RatingHeaderSchema.Constants.Prefix, (job as IComplianceItemRiskStatusProvider).ParentTableCode);
			AssertComplianceRiskLogsResults("UpdateRelatedQuotedBookingsComplianceRiskStatusForVessel2", vessel, org2, job as BusinessObject, "RatingHeader");
		}

		#region Implementation

		ComplianceRiskStatus SetupComplianceRiskStatus(IComplianceItemRiskStatusProvider provider, OrgHeader org, RefVessel vessel, DateTime? jobEndDate = null)
		{
			var complianceRiskStatus = CreateComplianceRiskStatus(provider, jobEndDate: jobEndDate);

			if (org != null)
			{
				org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			}

			if (vessel != null)
			{
				vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			}

			Factory.Save();

			return complianceRiskStatus;
		}

		ComplianceRiskStatus CreateComplianceRiskStatus(IComplianceItemRiskStatusProvider provider, string partyRisk = "CLR", string locationRisk = "CLR", string overallRisk = "CLR", string commodityRisk = "CLR", DateTime? jobEndDate = null)
		{
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = provider.ParentID;
			complianceRiskStatus.COR_ParentTableCode = provider.ParentTableCode;
			complianceRiskStatus.COR_PartyRisk = partyRisk;
			complianceRiskStatus.COR_LocationRisk = locationRisk;
			complianceRiskStatus.COR_CommodityRisk = commodityRisk;
			complianceRiskStatus.COR_OverallRisk = overallRisk;
			SetComplianceRiskStatusJobEndDate(complianceRiskStatus, jobEndDate);

			return complianceRiskStatus;
		}

		/// <summary>
		/// Here we read the value of the JobEndDate feature flag.
		/// If this flag is set to false, it means that the feature is not yet enabled, i.e. released.
		/// If the feature is not enabled, the app logic won't set the dbo.ComplianceRiskStatus.COR_JobEndDate value and leave it NULL.
		/// In this case, the stored procedures will fail to updated the status because the 'COR_JobEndDate > @earliestDT'
		/// condition will always be false.
		/// Taking into account the mentioned above, we set the COR_JobEndDate value manually below to simulate the app
		/// behavior when the feature flag is not enabled.
		/// This logic can be removed if the feature flag is permanently enabled.
		/// </summary>
		/// <param name="complianceRiskStatus"></param>
		void SetComplianceRiskStatusJobEndDate(ComplianceRiskStatus complianceRiskStatus, DateTime? jobEndDate = null)
		{
				complianceRiskStatus.COR_JobEndDate = jobEndDate ?? DateTime.UtcNow.AddDays(10);
		}

		StmALog[] GetComplianceLogs(IComplianceItemRiskStatusProvider provider)
		{
			var query = new ZDBOnlyQuery(typeof(StmALog));
			query.IgnoreDbQueryCache = true;
			query.AddToFilter(StmALogSchema.SL_Parent, provider.ParentID);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.StatusUpdated.Code);
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "|MST=Compliance Risk");
			return Factory.Load<StmALog>(query);
		}

		StmComplianceEvent[] GetComplianceEventLogs(IComplianceItemRiskStatusProvider provider)
		{
			var query = new ZDBOnlyQuery(typeof(StmComplianceEvent));
			query.IgnoreDbQueryCache = true;
			query.AddToFilter(StmComplianceEventSchema.SCE_ParentID, provider.ParentID);
			query.AddToFilter(StmComplianceEventSchema.SCE_EventType, AutoEvents.StatusUpdated.Code);
			return Factory.Load<StmComplianceEvent>(query);
		}

		void AssertComplianceRiskLogsResults(string storedProcedureName, IScreeningStatusProvider entity, OrgHeader organization, BusinessObject job, string tableName)
		{
			var updater = new RelatedJobComplianceRiskStatusUpdater(storedProcedureName, storedProcedureName);
			var complianceRiskStatusProvider = job as IComplianceItemRiskStatusProvider;
			var complianceRiskStatus = CreateComplianceRiskStatus(complianceRiskStatusProvider);

			Factory.Save();

			using (OrganisationsDataRegistry.Instance.ComplianceWiseFeatureDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.GetGlobalCommercialInvoice(true)))
			{
				// Clear the initialised logs.
				complianceRiskStatus.GetEventLogs().ForEach(e => e.Delete());

				var complianceLogs1 = GetComplianceLogs(complianceRiskStatusProvider);
				AssertEquals("New job should not create event log STU", 0, complianceLogs1.Length);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);

				var eventLogs1 = GetComplianceEventLogs(complianceRiskStatusProvider);

				// Entity status update to MAT
				entity.ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				Factory.Save();
				UpdateForEntity();
				complianceRiskStatus.Reload();

				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_PartyRisk);

				var complianceLogs2 = GetComplianceLogs(complianceRiskStatusProvider);
				var newLogs = complianceLogs2.Where(u => complianceLogs1.All(v => v.PK != u.PK)).ToArray();
				AssertEquals(2, complianceLogs2.Length);
				AssertLogsInformation("Create logs from CLR to BLK", new[]
				{
					"|MST=Compliance Risk|NEW=Blocked|OLD=Clear|TYP=Party risk status",
					"|MST=Compliance Risk|NEW=Blocked|OLD=Clear|TYP=Job compliance status"
				}, newLogs);

				var eventLogs2 = GetComplianceEventLogs(complianceRiskStatusProvider);
				var newEventLogs = eventLogs2.Where(u => eventLogs1.All(v => v.PK != u.PK)).ToArray();
				AssertEquals(2, eventLogs2.Length);
				AssertEventLogsInformation("Create logs from CLR to BLK", new[]
				{
					"NEW=BLK|OLD=CLR|Party Compliance Risk status",
					"NEW=BLK|OLD=CLR|Job Compliance status"
				}, newEventLogs);

				UpdateComplianceRiskStatusBySQL(complianceRiskStatus, ComplianceRiskStatusCodeList.Codes.Blocked, ComplianceRiskStatusCodeList.Codes.OverrideClear);

				UpdateForEntity();
				complianceRiskStatus.Reload();
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_PartyRisk);

				var complianceLogs3 = GetComplianceLogs(complianceRiskStatusProvider);
				newLogs = complianceLogs3.Where(u => complianceLogs2.All(v => v.PK != u.PK)).ToArray();
				AssertEquals(3, complianceLogs3.Length);
				AssertLogsInformation("Create logs from OVR to BLK", new[]
				{
					"|MST=Compliance Risk|NEW=Blocked|OLD=Override Clear|TYP=Job compliance status"
				}, newLogs);

				var eventLogs3 = GetComplianceEventLogs(complianceRiskStatusProvider);
				newEventLogs = eventLogs3.Where(u => eventLogs2.All(v => v.PK != u.PK)).ToArray();
				AssertEquals(3, eventLogs3.Length);
				AssertEventLogsInformation("Create logs from OVR to BLK", new[]
				{
					"NEW=BLK|OLD=OVR|Job Compliance status"
				}, newEventLogs);

				UpdateForEntity();
				complianceRiskStatus.Reload();
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_PartyRisk);

				var complianceLogs4 = GetComplianceLogs(complianceRiskStatusProvider);
				var eventLogs4 = GetComplianceEventLogs(complianceRiskStatusProvider);
				AssertEquals("No new logs created when risk status not changed", 3, complianceLogs4.Length);
				AssertEquals("No new logs created when risk status not changed", 3, eventLogs4.Length);

				// Entity status update to UNK
				entity.ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
				UpdateComplianceRiskStatusBySQL(complianceRiskStatus, ComplianceRiskStatusCodeList.Codes.Clear, ComplianceRiskStatusCodeList.Codes.Clear, ComplianceRiskStatusCodeList.Codes.Clear, "PRS");
				Factory.Save();

				UpdateForEntity();
				complianceRiskStatus.Reload();
				AssertEquals("HLD", complianceRiskStatus.COR_OverallRisk);
				AssertEquals("HSK", complianceRiskStatus.COR_PartyRisk);

				var complianceLogs5 = GetComplianceLogs(complianceRiskStatusProvider);
				newLogs = complianceLogs5.Where(u => complianceLogs4.All(v => v.PK != u.PK)).ToArray();
				AssertEquals(5, complianceLogs5.Length);
				AssertLogsInformation("Create logs from CLR to HLD", new[]
				{
					"|MST=Compliance Risk|NEW=High Risk|OLD=Clear|TYP=Party risk status",
					"|MST=Compliance Risk|NEW=Held|OLD=Clear|TYP=Job compliance status"
				}, newLogs);

				var eventLogs5 = GetComplianceEventLogs(complianceRiskStatusProvider);
				newEventLogs = eventLogs5.Where(u => eventLogs4.All(v => v.PK != u.PK)).ToArray();
				AssertEquals(5, eventLogs5.Length);
				AssertEventLogsInformation("Create logs from CLR to HLD", new[]
				{
					"NEW=HSK|OLD=CLR|Party Compliance Risk status",
					"NEW=HLD|OLD=CLR|Job Compliance status"
				}, newEventLogs);

				UpdateForEntity();
				var complianceLogs6 = GetComplianceLogs(complianceRiskStatusProvider);
				var eventLogs6 = GetComplianceEventLogs(complianceRiskStatusProvider);
				AssertEquals("No new logs created when risk status not changed", 5, complianceLogs6.Length);
				AssertEquals("No new logs created when risk status not changed", 5, eventLogs6.Length);

				// Entity status update to REQ
				entity.ScreeningStatus = ScreeningStatusesList.Codes.RequiresReview;
				UpdateComplianceRiskStatusBySQL(complianceRiskStatus);
				Factory.Save();

				complianceRiskStatus.Reload();
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_PartyRisk);

				UpdateForEntity();
				var complianceLogs7 = GetComplianceLogs(complianceRiskStatusProvider);
				newLogs = complianceLogs7.Where(u => complianceLogs6.All(v => v.PK != u.PK)).ToArray();
				AssertEquals(7, complianceLogs7.Length);
				AssertLogsInformation("Create overall risk logs from CLR to HLD", new[]
				{
					"|MST=Compliance Risk|NEW=High Risk|OLD=Clear|TYP=Party risk status",
					"|MST=Compliance Risk|NEW=Held|OLD=Clear|TYP=Job compliance status"
				}, newLogs);

				var eventLogs7 = GetComplianceEventLogs(complianceRiskStatusProvider);
				newEventLogs = eventLogs7.Where(u => eventLogs6.All(v => v.PK != u.PK)).ToArray();
				AssertEquals(7, eventLogs7.Length);
				AssertEventLogsInformation("Create overall risk logs from CLR to HLD", new[]
				{
					"NEW=HSK|OLD=CLR|Party Compliance Risk status",
					"NEW=HLD|OLD=CLR|Job Compliance status"
				}, newEventLogs);

				// Entity status update to NOT
				entity.ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
				UpdateComplianceRiskStatusBySQL(complianceRiskStatus, ComplianceRiskStatusCodeList.Codes.Clear, ComplianceRiskStatusCodeList.Codes.OverrideClear);
				Factory.Save();

				UpdateForEntity();
				complianceRiskStatus.Reload();
				AssertEquals("HLD", complianceRiskStatus.COR_OverallRisk);
				AssertEquals("HSK", complianceRiskStatus.COR_PartyRisk);

				var complianceLogs8 = GetComplianceLogs(complianceRiskStatusProvider);
				newLogs = complianceLogs8.Where(u => complianceLogs7.All(v => v.PK != u.PK)).ToArray();
				AssertEquals(9, complianceLogs8.Length);
				AssertLogsInformation("Create party risk logs from CLR to HSK, overall risk logs from OVR to HLD", new[]
				{
					"|MST=Compliance Risk|NEW=High Risk|OLD=Clear|TYP=Party risk status",
					"|MST=Compliance Risk|NEW=Held|OLD=Override Clear|TYP=Job compliance status"
				}, newLogs);

				var eventLogs8 = GetComplianceEventLogs(complianceRiskStatusProvider);
				newEventLogs = eventLogs8.Where(u => eventLogs7.All(v => v.PK != u.PK)).ToArray();
				AssertEquals(9, eventLogs8.Length);
				AssertEventLogsInformation("Create party risk logs from CLR to HSK, overall risk logs from OVR to HLD", new[]
				{
					"NEW=HSK|OLD=CLR|Party Compliance Risk status",
					"NEW=HLD|OLD=OVR|Job Compliance status"
				}, newEventLogs);

				// Entity status update to CLP
				entity.ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
				UpdateComplianceRiskStatusBySQL(complianceRiskStatus, ComplianceRiskStatusCodeList.Codes.Blocked, ComplianceRiskStatusCodeList.Codes.Blocked);
				Factory.Save();

				UpdateForEntity();

				var complianceLogs9 = GetComplianceLogs(complianceRiskStatusProvider);
				newLogs = complianceLogs9.Where(u => complianceLogs8.All(v => v.PK != u.PK)).ToArray();
				AssertEquals(11, complianceLogs9.Length);
				AssertLogsInformation("Create overall risk logs from BLK to CLR", new[]
				{
					"|MST=Compliance Risk|NEW=Clear|OLD=Blocked|TYP=Party risk status",
					"|MST=Compliance Risk|NEW=Clear|OLD=Blocked|TYP=Job compliance status"
				}, newLogs);

				var eventLogs9 = GetComplianceEventLogs(complianceRiskStatusProvider);
				newEventLogs = eventLogs9.Where(u => eventLogs8.All(v => v.PK != u.PK)).ToArray();
				AssertEquals(11, eventLogs9.Length);
				AssertEventLogsInformation("Create overall risk logs from BLK to CLR", new[]
				{
					"NEW=CLR|OLD=BLK|Party Compliance Risk status",
					"NEW=CLR|OLD=BLK|Job Compliance status"
				}, newEventLogs);

				// Entity status update to CLR
				entity.ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				Factory.Save();
				UpdateComplianceRiskStatusBySQL(complianceRiskStatus, "HSK", "HLD");

				UpdateForEntity();
				complianceRiskStatus.Reload();
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_PartyRisk);

				var complianceLogs10 = GetComplianceLogs(complianceRiskStatusProvider);
				newLogs = complianceLogs10.Where(u => complianceLogs9.All(v => v.PK != u.PK)).ToArray();
				AssertEquals(13, complianceLogs10.Length);
				AssertLogsInformation("Create overall risk logs from HLD to CLR", new[]
				{
					"|MST=Compliance Risk|NEW=Clear|OLD=High Risk|TYP=Party risk status",
					"|MST=Compliance Risk|NEW=Clear|OLD=Held|TYP=Job compliance status"
				}, newLogs);

				var eventLogs10 = GetComplianceEventLogs(complianceRiskStatusProvider);
				newEventLogs = eventLogs10.Where(u => eventLogs9.All(v => v.PK != u.PK)).ToArray();
				AssertEquals(13, eventLogs10.Length);
				AssertEventLogsInformation("Create overall risk logs from HLD to CLR", new[]
				{
					"NEW=CLR|OLD=HSK|Party Compliance Risk status",
					"NEW=CLR|OLD=HLD|Job Compliance status"
				}, newEventLogs);

				//Two Entity status are updated to BLK
				entity.ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				organization.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				Factory.Save();
				UpdateForEntity();

				var complianceLogs11 = GetComplianceLogs(complianceRiskStatusProvider);
				newLogs = complianceLogs11.Where(u => complianceLogs10.All(v => v.PK != u.PK)).ToArray();
				AssertEquals(15, complianceLogs11.Length);
				AssertLogsInformation("Create overall risk logs from CLR to BLK", new[]
				{
					"|MST=Compliance Risk|NEW=Blocked|OLD=Clear|TYP=Party risk status",
					"|MST=Compliance Risk|NEW=Blocked|OLD=Clear|TYP=Job compliance status"
				}, newLogs);

				var eventLogs11 = GetComplianceEventLogs(complianceRiskStatusProvider);
				newEventLogs = eventLogs11.Where(u => eventLogs10.All(v => v.PK != u.PK)).ToArray();
				AssertEquals(15, eventLogs11.Length);
				AssertEventLogsInformation("Create overall risk logs from CLR to BLK", new[]
				{
					"NEW=BLK|OLD=CLR|Party Compliance Risk status",
					"NEW=BLK|OLD=CLR|Job Compliance status"
				}, newEventLogs);

				//Two Entity status, one is still BLK, another is updated to UNK
				entity.ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				organization.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
				Factory.Save();
				UpdateForEntity();

				var complianceLogs12 = GetComplianceLogs(complianceRiskStatusProvider);
				newLogs = complianceLogs12.Where(u => complianceLogs11.All(v => v.PK != u.PK)).ToArray();
				AssertEquals("No new logs should be created", 0, newLogs.Length);

				var eventLogs12 = GetComplianceEventLogs(complianceRiskStatusProvider);
				newEventLogs = eventLogs12.Where(u => eventLogs11.All(v => v.PK != u.PK)).ToArray();
				AssertEquals("No new event logs should be created", 0, newEventLogs.Length);

				complianceRiskStatus.Reload();
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_PartyRisk);

				//Two Entity status, one is still BLK, another is updated to CLR
				entity.ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				organization.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				Factory.Save();
				UpdateForEntity();

				var complianceLogs13 = GetComplianceLogs(complianceRiskStatusProvider);
				newLogs = complianceLogs13.Where(u => complianceLogs12.All(v => v.PK != u.PK)).ToArray();
				AssertEquals("No new logs should be created", 0, newLogs.Length);

				var eventLogs13 = GetComplianceEventLogs(complianceRiskStatusProvider);
				newEventLogs = eventLogs13.Where(u => eventLogs12.All(v => v.PK != u.PK)).ToArray();
				AssertEquals("No new event logs should be created", 0, newEventLogs.Length);

				complianceRiskStatus.Reload();
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_PartyRisk);
			}

			void UpdateComplianceRiskStatusBySQL(ComplianceRiskStatus complianceRiskStatus, string partyRisk = "CLR", string overallRisk = "CLR", string locationRisk = "CLR", string commodityRisk = "CLR")
			{
				Db.Connection.ExecuteNonQuery($@"
UPDATE
	dbo.ComplianceRiskStatus
SET
	COR_PartyRisk = '{partyRisk}',
	COR_OverallRisk = '{overallRisk}',
	COR_LocationRisk = '{locationRisk}',
	COR_CommodityRisk = '{commodityRisk}'
WHERE
	COR_PK = '{complianceRiskStatus.PK}'");
			}

			void UpdateForEntity()
			{
				if (entity is OrgHeader)
				{
					updater.UpdateForOrg((entity as BusinessObject).PK.ToGuid(), new Guid(), null);
				}
				else
				{
					updater.UpdateForVessel((entity as BusinessObject).PK.ToGuid(), new Guid(), null);
				}
			}

			void AssertLogsInformation(string message, string[] expected, StmALog[] stmALogs)
			{
				Assert($"User should be {Environment.Env.CurrentUser.Initials}, table should be {tableName}", stmALogs.All(u => u.SL_GS_NKUser == Environment.Env.CurrentUser.Initials && u.SL_Table == tableName && !u.SL_IsEstimate && !u.IsCancelled));
				AssertContainsExactElementsInAnyOrder(message, expected, stmALogs.Select(u => u.SL_Reference));
			}

			void AssertEventLogsInformation(string message, string[] expected, StmComplianceEvent[] eventLogs)
			{
				AssertContainsExactElementsInAnyOrder(message, expected, eventLogs.Select(u => ZString.Format("NEW={0}|OLD={1}|{2}", u.SCE_NewValue, u.SCE_OldValue, u.SCE_EventReference)));
			}
		}

		void AssertConsiderLocationAndCommodityRisk(string storedProcedureName, ComplianceRiskStatus complianceRiskStatus, IComplianceItemRiskStatusProvider complianceRiskStatusProvider, OrgHeader org, RefVessel vessel)
		{
			var updater = new RelatedJobComplianceRiskStatusUpdater(storedProcedureName, storedProcedureName);

			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);

			// Clear the initialised logs.
			complianceRiskStatus.GetEventLogs().ForEach(log => log.Delete());

			UpdateForOrgOrVessel();

			// When Commodity Risk is INC, should create new log with status Held.
			var logs = GetComplianceLogs(complianceRiskStatusProvider);
			complianceRiskStatus.Reload();
			AssertEquals("HLD", complianceRiskStatus.COR_OverallRisk);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"|MST=Compliance Risk|NEW=Held|OLD=Clear|TYP=Job compliance status"
			}, logs.Select(u => u.SL_Reference));

			var eventLogs = GetComplianceEventLogs(complianceRiskStatusProvider);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"NEW=HLD|OLD=CLR|Job Compliance status"
			}, eventLogs.Select(u => ZString.Format("NEW={0}|OLD={1}|{2}", u.SCE_NewValue, u.SCE_OldValue, u.SCE_EventReference)));

			// Update Commodity Risk To HSK, should not create new log.
			Db.Connection.ExecuteNonQuery($"UPDATE dbo.ComplianceRiskStatus SET COR_CommodityRisk = 'HSK' WHERE COR_PK = '{complianceRiskStatus.PK}'");

			UpdateForOrgOrVessel();

			logs = GetComplianceLogs(complianceRiskStatusProvider);
			complianceRiskStatus.Reload();
			AssertEquals("HLD", complianceRiskStatus.COR_OverallRisk);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"|MST=Compliance Risk|NEW=Held|OLD=Clear|TYP=Job compliance status"
			}, logs.Select(u => u.SL_Reference));

			eventLogs = GetComplianceEventLogs(complianceRiskStatusProvider);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"NEW=HLD|OLD=CLR|Job Compliance status"
			}, eventLogs.Select(u => ZString.Format("NEW={0}|OLD={1}|{2}", u.SCE_NewValue, u.SCE_OldValue, u.SCE_EventReference)));

			// Update Commodity Risk To Clear, should create new logs.
			Db.Connection.ExecuteNonQuery($"UPDATE dbo.ComplianceRiskStatus SET COR_CommodityRisk = '{ComplianceRiskStatusCodeList.Codes.Clear}' WHERE COR_PK = '{complianceRiskStatus.PK}'");

			UpdateForOrgOrVessel();

			logs = GetComplianceLogs(complianceRiskStatusProvider);
			complianceRiskStatus.Reload();
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"|MST=Compliance Risk|NEW=Held|OLD=Clear|TYP=Job compliance status",
				"|MST=Compliance Risk|NEW=Clear|OLD=Held|TYP=Job compliance status"
			}, logs.Select(u => u.SL_Reference));

			eventLogs = GetComplianceEventLogs(complianceRiskStatusProvider);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"NEW=HLD|OLD=CLR|Job Compliance status",
				"NEW=CLR|OLD=HLD|Job Compliance status"
			}, eventLogs.Select(u => ZString.Format("NEW={0}|OLD={1}|{2}", u.SCE_NewValue, u.SCE_OldValue, u.SCE_EventReference)));

			// Update Commodity Risk To Not Applicable, should create new logs.
			Db.Connection.ExecuteNonQuery($"UPDATE dbo.ComplianceRiskStatus SET COR_CommodityRisk = '{ComplianceRiskStatusCodeList.Codes.NotApplicable}', COR_OverallRisk = 'HLD' WHERE COR_PK = '{complianceRiskStatus.PK}'");

			UpdateForOrgOrVessel();

			logs = GetComplianceLogs(complianceRiskStatusProvider);
			complianceRiskStatus.Reload();
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"|MST=Compliance Risk|NEW=Held|OLD=Clear|TYP=Job compliance status",
				"|MST=Compliance Risk|NEW=Clear|OLD=Held|TYP=Job compliance status",
				"|MST=Compliance Risk|NEW=Clear|OLD=Held|TYP=Job compliance status"
			}, logs.Select(u => u.SL_Reference));

			eventLogs = GetComplianceEventLogs(complianceRiskStatusProvider);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"NEW=HLD|OLD=CLR|Job Compliance status",
				"NEW=CLR|OLD=HLD|Job Compliance status",
				"NEW=CLR|OLD=HLD|Job Compliance status"
			}, eventLogs.Select(u => ZString.Format("NEW={0}|OLD={1}|{2}", u.SCE_NewValue, u.SCE_OldValue, u.SCE_EventReference)));

			// Update Commodity Risk To Blocked, should create new logs.
			Db.Connection.ExecuteNonQuery($"UPDATE dbo.ComplianceRiskStatus SET COR_CommodityRisk = 'BLK' WHERE COR_PK = '{complianceRiskStatus.PK}'");

			UpdateForOrgOrVessel();

			logs = GetComplianceLogs(complianceRiskStatusProvider);
			complianceRiskStatus.Reload();
			AssertEquals("BLK", complianceRiskStatus.COR_OverallRisk);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"|MST=Compliance Risk|NEW=Held|OLD=Clear|TYP=Job compliance status",
				"|MST=Compliance Risk|NEW=Clear|OLD=Held|TYP=Job compliance status",
				"|MST=Compliance Risk|NEW=Clear|OLD=Held|TYP=Job compliance status",
				"|MST=Compliance Risk|NEW=Blocked|OLD=Clear|TYP=Job compliance status"
			}, logs.Select(u => u.SL_Reference));

			eventLogs = GetComplianceEventLogs(complianceRiskStatusProvider);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"NEW=HLD|OLD=CLR|Job Compliance status",
				"NEW=CLR|OLD=HLD|Job Compliance status",
				"NEW=CLR|OLD=HLD|Job Compliance status",
				"NEW=BLK|OLD=CLR|Job Compliance status"
			}, eventLogs.Select(u => ZString.Format("NEW={0}|OLD={1}|{2}", u.SCE_NewValue, u.SCE_OldValue, u.SCE_EventReference)));

			// Update Location Risk To Blocked, should create new logs.
			Db.Connection.ExecuteNonQuery($"UPDATE dbo.ComplianceRiskStatus SET COR_CommodityRisk = 'HSK',COR_LocationRisk = 'BLK', COR_OverallRisk = 'CLR' WHERE COR_PK = '{complianceRiskStatus.PK}'");

			UpdateForOrgOrVessel();

			logs = GetComplianceLogs(complianceRiskStatusProvider);
			complianceRiskStatus.Reload();
			AssertEquals("BLK", complianceRiskStatus.COR_OverallRisk);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"|MST=Compliance Risk|NEW=Held|OLD=Clear|TYP=Job compliance status",
				"|MST=Compliance Risk|NEW=Clear|OLD=Held|TYP=Job compliance status",
				"|MST=Compliance Risk|NEW=Clear|OLD=Held|TYP=Job compliance status",
				"|MST=Compliance Risk|NEW=Blocked|OLD=Clear|TYP=Job compliance status",
				"|MST=Compliance Risk|NEW=Blocked|OLD=Clear|TYP=Job compliance status"
			}, logs.Select(u => u.SL_Reference));

			eventLogs = GetComplianceEventLogs(complianceRiskStatusProvider);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"NEW=HLD|OLD=CLR|Job Compliance status",
				"NEW=CLR|OLD=HLD|Job Compliance status",
				"NEW=CLR|OLD=HLD|Job Compliance status",
				"NEW=BLK|OLD=CLR|Job Compliance status",
				"NEW=BLK|OLD=CLR|Job Compliance status"
			}, eventLogs.Select(u => ZString.Format("NEW={0}|OLD={1}|{2}", u.SCE_NewValue, u.SCE_OldValue, u.SCE_EventReference)));

			void UpdateForOrgOrVessel()
			{
				if (org != null)
				{
					updater.UpdateForOrg(org.PK.ToGuid(), new Guid(), null);
				}

				if (vessel != null)
				{
					updater.UpdateForVessel(vessel.PK.ToGuid(), new Guid(), null);
				}
			}
		}

		void AssertUpdateRelatedJobComplianceRiskStatusForOrgWithDifferentParty(IComplianceItemRiskStatusProvider job, OrgHeader org, string orgSpScriptName)
		{
			var complianceRiskStatus = SetupComplianceRiskStatus(job, org, null);

			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_PartyRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);
			new RelatedJobComplianceRiskStatusUpdater(orgSpScriptName, null).UpdateForOrg(org.PK.ToGuid(), new Guid(), null);
			complianceRiskStatus = new BusinessObjectFactory().Load<ComplianceRiskStatus>(complianceRiskStatus.PK);

			AssertEquals(ScreeningStatusesList.Codes.Matched, org.OH_ScreeningStatus);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_PartyRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
		}

		#endregion
	}
}
