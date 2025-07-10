using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.ComplianceRisk.Business.Test
{
	public class ComplianceRiskStatusSupporterTest : TestCaseWithFactory
	{
		public void TestGetStatus()
		{
			AssertGetStatus("PSK", "PSK", "PSK", "INC");
			AssertGetStatus("CLR", "CLR", "CLR", "CLR");
			AssertGetStatus("OVR", "PSK", "PSK", "PSK");
		}

		public void TestGetDefaultStatus()
		{
			var status = Supporter.GetStatus(new DummyBizOForFreightMovementRestrictionTest(Factory));
			CombineAssertions(() =>
			{
				AssertEquals("BLK", status.JobRisk);
				AssertEquals("HSK", status.PartyRisk);
				AssertEquals("BLK", status.LocationRisk);
				AssertEquals("INC", status.CommodityRisk);
			});
		}

		public void TestIsDPSFreightMovementRestrictedTrueWhenPotentialRisk()
		{
			AssertIsDPSFreightMovementRestricted("PSK", "PSK", "CLR", "CLR", DPSFreightMovementRestrictionsOptions.Codes.All, expectedResult: true);
		}

		public void TestIsDPSFreightMovementRestrictedFalseWhenOverrideClear()
		{
			AssertIsDPSFreightMovementRestricted("OVR", "CLR", "CLR", "INC", DPSFreightMovementRestrictionsOptions.Codes.All, expectedResult: false);
		}

		public void TestIsDPSFreightMovementRestrictedFalseWhenAllSinglePartsClear()
		{
			AssertIsDPSFreightMovementRestricted("CLR", "CLR", "CLR", "CLR", DPSFreightMovementRestrictionsOptions.Codes.All, expectedResult: false);
		}

		public void TestIsDPSFreightMovementRestrictedFalseWhenJobTypesExcluded()
		{
			AssertIsDPSFreightMovementRestricted("CLR", "CLR", "CLR", "CLR", DPSFreightMovementRestrictionsOptions.Codes.No, expectedResult: false);
		}

		public void TestComplianceRiskStatusNotSetAndRegistryRestrictionNo_ExpectedFreightMovementRestrictedFalse()
		{
			AssertFreightMovementRestrictedAndServiceDirection("PSK", "PSK", "CLR", "INC", DPSFreightMovementRestrictionsOptions.Codes.No, isCreateRiskStatus: false,
				isExport: true, isImport: false, isCrossTrade: false, isDomestic: false, isRestricted: false);
		}

		public void TestComplianceRiskStatusNotSetAndRegistryRestrictionAll_ExpectedFreightMovementRestrictedTrue()
		{
			AssertFreightMovementRestrictedAndServiceDirection("PSK", "PSK", "CLR", "CLR", DPSFreightMovementRestrictionsOptions.Codes.All, isCreateRiskStatus: false,
				isExport: true, isImport: false, isCrossTrade: false, isDomestic: false, isRestricted: true);
		}

		public void TestComplianceRiskStatusNotSetAndRegistryRestrictionExportWhenServiceDirectionExport_ExpectedFreightMovementRestrictedTrue()
		{
			AssertFreightMovementRestrictedAndServiceDirection("PSK", "PSK", "CLR", "INC", DPSFreightMovementRestrictionsOptions.Codes.Exp, isCreateRiskStatus: false,
				isExport: true, isImport: false, isCrossTrade: false, isDomestic: false, isRestricted: true);
		}

		public void TestComplianceRiskStatusNotSetAndRegistryRestrictionInternationalWhenServiceDirectionExport_ExpectedFreightMovementRestrictedTrue()
		{
			AssertFreightMovementRestrictedAndServiceDirection("PSK", "PSK", "CLR", "INC", DPSFreightMovementRestrictionsOptions.Codes.Int, isCreateRiskStatus: false,
				isExport: true, isImport: false, isCrossTrade: false, isDomestic: false, isRestricted: true);
		}

		public void TestComplianceRiskStatusNotSetAndRegistryRestrictionInternationalWhenServiceDirectionImport_ExpectedFreightMovementRestrictedTrue()
		{
			AssertFreightMovementRestrictedAndServiceDirection("PSK", "PSK", "CLR", "INC", DPSFreightMovementRestrictionsOptions.Codes.Int, isCreateRiskStatus: false,
				isExport: false, isImport: true, isCrossTrade: false, isDomestic: false, isRestricted: true);
		}

		public void TestComplianceRiskStatusNotSetAndRegistryRestrictionExportWhenServiceDirectionImport_ExpectedFreightMovementRestrictedFalse()
		{
			AssertFreightMovementRestrictedAndServiceDirection("PSK", "CLR", "PSK", "CLR", DPSFreightMovementRestrictionsOptions.Codes.Exp, isCreateRiskStatus: false,
				isExport: false, isImport: true, isCrossTrade: false, isDomestic: false, isRestricted: false);
		}

		public void TestServiceDirectionExportAndRegistryRestrictionExportWhenCompliancePotentialRisk_ExpectedFreightMovementRestrictedTrue()
		{
			AssertFreightMovementRestrictedAndServiceDirection("PSK", "PSK", "CLR", "INC", DPSFreightMovementRestrictionsOptions.Codes.Exp, isCreateRiskStatus: true,
				isExport: true, isImport: false, isCrossTrade: false, isDomestic: false, isRestricted: true);
		}

		public void TestServiceDirectionExportAndRegistryRestrictionNoWhenCompliancePotentialRisk_ExpectedFreightMovementRestrictedFalse()
		{
			AssertFreightMovementRestrictedAndServiceDirection("PSK", "PSK", "CLR", "CLR", DPSFreightMovementRestrictionsOptions.Codes.No, isCreateRiskStatus: true,
				isExport: true, isImport: false, isCrossTrade: false, isDomestic: false, isRestricted: false);
		}

		public void TestServiceDirectionImportAndRegistryRestrictionInternationalWhenCompliancePotentialRisk_ExpectedFreightMovementRestrictedTrue()
		{
			AssertFreightMovementRestrictedAndServiceDirection("PSK", "CLR", "PSK", "CLR", DPSFreightMovementRestrictionsOptions.Codes.Int, isCreateRiskStatus: true,
				isExport: false, isImport: true, isCrossTrade: false, isDomestic: false, isRestricted: true);
		}

		public void TestServiceDirectionImportAndRegistryRestrictionNoWhenCompliancePotentialRisk_ExpectedFreightMovementRestrictedFalse()
		{
			AssertFreightMovementRestrictedAndServiceDirection("PSK", "CLR", "PSK", "CLR", DPSFreightMovementRestrictionsOptions.Codes.No, isCreateRiskStatus: true,
				isExport: false, isImport: true, isCrossTrade: false, isDomestic: false, isRestricted: false);
		}

		public void TestServiceDirectionCrossTradeAndRegistryRestrictionAllWhenCompliancePotentialRisk_ExpectedFreightMovementRestrictedTrue()
		{
			AssertFreightMovementRestrictedAndServiceDirection("PSK", "CLR", "PSK", "CLR", DPSFreightMovementRestrictionsOptions.Codes.All, isCreateRiskStatus: true,
				isExport: false, isImport: false, isCrossTrade: true, isDomestic: false, isRestricted: true);
		}

		public void TestServiceDirectionCrossTradeAndRegistryRestrictionAllWhenComplianceOverallClear_ExpetedFreightMovementRestrictedFalse()
		{
			AssertFreightMovementRestrictedAndServiceDirection("OVR", "CLR", "CLR", "CLR", DPSFreightMovementRestrictionsOptions.Codes.All, isCreateRiskStatus: true,
				isExport: false, isImport: false, isCrossTrade: true, isDomestic: false, isRestricted: false);
		}

		public void TestServiceDirectionCrossTradeAndRegisrtyRestrictionAllWhenComplianceClear_ExpectedFreightMovementRestrictedFalse()
		{
			AssertFreightMovementRestrictedAndServiceDirection("CLR", "CLR", "CLR", "CLR", DPSFreightMovementRestrictionsOptions.Codes.All, isCreateRiskStatus: true,
				isExport: false, isImport: false, isCrossTrade: true, isDomestic: false, isRestricted: false);
		}

		public void TestServiceDirectionDomesticAndRegistryRestrictionAllWhenCompliancePotentialRisk_ExpectedFreightMovementRestrictedTrue()
		{
			AssertFreightMovementRestrictedAndServiceDirection("PSK", "CLR", "PSK", "CLR", DPSFreightMovementRestrictionsOptions.Codes.All, isCreateRiskStatus: true,
				isExport: false, isImport: false, isCrossTrade: false, isDomestic: true, isRestricted: true);
		}

		public void TestServiceDirectionDomesticAndRegistryRestrictionAllWhenComplianceOverallClear_ExpectedFreightMovementRestrictedFalse()
		{
			AssertFreightMovementRestrictedAndServiceDirection("OVR", "CLR", "CLR", "INC", DPSFreightMovementRestrictionsOptions.Codes.All, isCreateRiskStatus: true,
				isExport: false, isImport: false, isCrossTrade: false, isDomestic: true, isRestricted: false);
		}

		public void TestServiceDirectionDomesticAndRegistryRestrictionAllWhenComplianceClear_ExpectedFreightMovementRestrictedFalse()
		{
			AssertFreightMovementRestrictedAndServiceDirection("CLR", "CLR", "CLR", "CLR", DPSFreightMovementRestrictionsOptions.Codes.All, isCreateRiskStatus: true,
				isExport: false, isImport: false, isCrossTrade: false, isDomestic: true, isRestricted: false);
		}

		public void TestIsDPSFreightMovementRestrictedTrueWhenWarehouseJobDirectionIsNull()
		{
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var header = Factory.NewWithValidTestData<OrgHeader>();
				var warehouse = WhsTransactionTestHelperCreator.GetNewHelper(Factory).CreateWarehouse("WHS", "A");
				warehouse[WhsWarehouseSchema.WW_OA_WarehouseAddress] = header.MainAddress.PK;

				var warehouseOrder = (BusinessObject)Factory.New<IWhsOrder>();
				warehouseOrder[WhsDocketSchema.WD_OH_Client] = header.PK;
				warehouseOrder[WhsDocketSchema.WD_WW_Whs] = warehouse.PK;

				Factory.Save();

				AssertEquals(false, Supporter.IsDPSFreightMovementRestricted(ScreeningStatusesList.Codes.Matched, null, warehouseOrder));
			}
		}

		public void TestGetStatusForQuoteBooking()
		{
			var ratingHeader = Factory.NewWithValidTestData<RatingHeader>();
			ratingHeader.TH_RateType = "QTE";
			ratingHeader.TH_QuoteDate = ZDateTime.UtcNow.Date;

			var quotedShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			quotedShipment.JS_RL_NKOrigin = "AUSYD";
			quotedShipment.JS_RL_NKDestination = "USLAX";
			quotedShipment.JS_IsBooking = true;
			quotedShipment.JS_IsForwardRegistered = false;
			quotedShipment.JS_TH_OneTimeQuote = ratingHeader.PK;

			var forwardingShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			forwardingShipment.JS_RL_NKOrigin = "AUSYD";
			forwardingShipment.JS_RL_NKDestination = "USLAX";
			forwardingShipment.JS_IsBooking = true;
			forwardingShipment.JS_IsForwardRegistered = true;
			forwardingShipment.JS_TH_OneTimeQuote = Guid.Empty;

			var viewQuotedBooking = Factory.NewWithValidTestData<ViewQuotedBooking>();
			viewQuotedBooking.VB_JS = quotedShipment.PK;
			viewQuotedBooking.VB_TH = ratingHeader.PK;

			var bookingWithQuote = (IQuotedBooking)ObjectFactory.GetType<IQuotedBooking>().InvokeMember(
				"New",
				System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
				null,
				null,
				new object[] { Freight.Integration.QuoteBookingType.BookingWithQuote, Factory });

			var quickBooking = (IQuotedBooking)ObjectFactory.GetType<IQuotedBooking>().InvokeMember(
				"New",
				System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
				null,
				null,
				new object[] { Freight.Integration.QuoteBookingType.QuickBooking, Factory });

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = ratingHeader.PK;
			complianceRiskStatus.COR_ParentTableCode = ratingHeader.TablePrefix;
			complianceRiskStatus.COR_OverallRisk = "CLR";
			complianceRiskStatus.COR_PartyRisk = "CLR";
			complianceRiskStatus.COR_LocationRisk = "CLR";
			complianceRiskStatus.COR_CommodityRisk = "CLR";

			var complianceRiskStatus1 = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus1.COR_ParentID = forwardingShipment.PK;
			complianceRiskStatus1.COR_ParentTableCode = forwardingShipment.TablePrefix;
			complianceRiskStatus1.COR_OverallRisk = "CLR";
			complianceRiskStatus1.COR_PartyRisk = "PSK";
			complianceRiskStatus1.COR_LocationRisk = "CLR";
			complianceRiskStatus1.COR_CommodityRisk = "PSK";

			var complianceRiskStatus2 = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus2.COR_ParentID = ((ForwardingShipment)bookingWithQuote.ForwardingShipment).JS_TH_OneTimeQuote;
			complianceRiskStatus2.COR_ParentTableCode = bookingWithQuote.ForwardingShipment.TablePrefix;
			complianceRiskStatus2.COR_OverallRisk = "PSK";
			complianceRiskStatus2.COR_PartyRisk = "CLR";
			complianceRiskStatus2.COR_LocationRisk = "CLR";
			complianceRiskStatus2.COR_CommodityRisk = "CLR";

			var complianceRiskStatus3 = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus3.COR_ParentID = ((ForwardingShipment)quickBooking.ForwardingShipment).PK;
			complianceRiskStatus3.COR_ParentTableCode = quickBooking.ForwardingShipment.TablePrefix;
			complianceRiskStatus3.COR_OverallRisk = "CLR";
			complianceRiskStatus3.COR_PartyRisk = "PSK";
			complianceRiskStatus3.COR_LocationRisk = "PSK";
			complianceRiskStatus3.COR_CommodityRisk = "PSK";

			Factory.Save();

			var status = Supporter.GetStatus(quotedShipment);

			CombineAssertions(() =>
			{
				AssertEquals(complianceRiskStatus.COR_OverallRisk, status.JobRisk);
				AssertEquals(complianceRiskStatus.COR_PartyRisk, status.PartyRisk);
				AssertEquals(complianceRiskStatus.COR_LocationRisk, status.LocationRisk);
				AssertEquals(complianceRiskStatus.COR_CommodityRisk, status.CommodityRisk);
			});

			status = Supporter.GetStatus(forwardingShipment);

			CombineAssertions(() =>
			{
				AssertEquals(complianceRiskStatus1.COR_OverallRisk, status.JobRisk);
				AssertEquals(complianceRiskStatus1.COR_PartyRisk, status.PartyRisk);
				AssertEquals(complianceRiskStatus1.COR_LocationRisk, status.LocationRisk);
				AssertEquals(complianceRiskStatus1.COR_CommodityRisk, status.CommodityRisk);
			});

			status = Supporter.GetStatus(bookingWithQuote.ForwardingShipment as ForwardingShipment);

			CombineAssertions(() =>
			{
				AssertEquals(complianceRiskStatus2.COR_OverallRisk, status.JobRisk);
				AssertEquals(complianceRiskStatus2.COR_PartyRisk, status.PartyRisk);
				AssertEquals(complianceRiskStatus2.COR_LocationRisk, status.LocationRisk);
				AssertEquals(complianceRiskStatus2.COR_CommodityRisk, status.CommodityRisk);
			});

			status = Supporter.GetStatus(quickBooking.ForwardingShipment as ForwardingShipment);

			CombineAssertions(() =>
			{
				AssertEquals(complianceRiskStatus3.COR_OverallRisk, status.JobRisk);
				AssertEquals(complianceRiskStatus3.COR_PartyRisk, status.PartyRisk);
				AssertEquals(complianceRiskStatus3.COR_LocationRisk, status.LocationRisk);
				AssertEquals(complianceRiskStatus3.COR_CommodityRisk, status.CommodityRisk);
			});
		}

		public void TestCopyAssessmentDetailsFromBookingToShipment()
		{
			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_OverallRisk = "CLR";
			complianceRiskStatus.COR_PartyRisk = "CLR";
			complianceRiskStatus.COR_LocationRisk = "CLR";
			complianceRiskStatus.COR_CommodityRisk = "CLR";

			var bookingWithQuote = (IQuotedBooking)ObjectFactory.GetType<IQuotedBooking>().InvokeMember(
				"New",
				System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
				null,
				null,
				new object[] { Freight.Integration.QuoteBookingType.BookingWithQuote, Factory });

			var commodityDetail = Factory.New<ComplianceCommodityDetail>();
			commodityDetail.CCD_COR_ComplianceRisk = complianceRiskStatus.PK;
			commodityDetail.CCD_HarmonizedCode = "123456";
			commodityDetail.CCD_RN_NKOrigin = "AU";
			commodityDetail.CCD_Description = "Test";
			complianceRiskStatus.COR_ParentID = bookingWithQuote.ViewPK;
			complianceRiskStatus.COR_ParentTableCode = "TH";

			var shipment = CreateNewShipment(isInternationalJob: true);

			Supporter.CopyAssessmentDetailsFromBookingToShipment(bookingWithQuote.ViewPK, shipment.PK, Factory);

			var query = new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, shipment.PK);
			query.FetchOnlyFromLocalCache = true;
			var shipmentComplianceRiskStatus = Factory.LoadTop1<ComplianceRiskStatus>(query);
			AssertNotNull("Compliance Risk Status has been copied.", shipmentComplianceRiskStatus);
			AssertEquals(complianceRiskStatus.COR_OverallRisk, shipmentComplianceRiskStatus.COR_OverallRisk);
			AssertEquals(complianceRiskStatus.COR_PartyRisk, shipmentComplianceRiskStatus.COR_PartyRisk);
			AssertEquals(complianceRiskStatus.COR_LocationRisk, shipmentComplianceRiskStatus.COR_LocationRisk);
			Assert(shipmentComplianceRiskStatus.CopyFromBooking);
			Assert(shipmentComplianceRiskStatus.CopyFromBookingFirstLoaded);

			var commodityInShipment = Supporter.FetchCommodityDetailsInDB((IComplianceItemRiskStatusProvider)shipment);

			AssertContainsExactElementsInAnyOrder(new[] {
				("123456", "WCO", shipment.PK, shipment.JS_UniqueConsignRef, "NCH", string.Empty, "AU", "Test"),
			}, commodityInShipment.Select(u => ((string)u.HarmonizedCode, (string)u.GroupingOrCountry, u.ParentJobID, u.Source, (string)u.RiskStatus, (string)u.AssessmentNotes, (string)u.Origin, (string)u.GoodsDescription)));
		}

		public void TestDeleteNotSavedAssessmentDetailsCopiedFromBooking()
		{
			// case 1
			var shipment1 = CreateNewShipment(isInternationalJob: true);
			var complianceRiskStatus1 = CreateNewComplianceRiskStatusForShipment(shipment1);
			complianceRiskStatus1.CopyFromBooking = true;
			Factory.Save();

			Supporter.DeleteNotSavedAssessmentDetailsCopiedFromBooking(shipment1.PK, Factory);

			var query1 = new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, shipment1.PK);
			query1.FetchOnlyFromLocalCache = true;
			var riskStatus1 = Factory.LoadTop1<ComplianceRiskStatus>(query1);
			AssertNotNull("Compliance Risk remained.", riskStatus1);

			// case 2
			var shipment2 = CreateNewShipment(isInternationalJob: true);
			var complianceRiskStatus2 = CreateNewComplianceRiskStatusForShipment(shipment2);
			complianceRiskStatus2.CopyFromBooking = false;

			Supporter.DeleteNotSavedAssessmentDetailsCopiedFromBooking(shipment2.PK, Factory);

			var query2 = new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, shipment2.PK);
			query2.FetchOnlyFromLocalCache = true;
			var riskStatus2 = Factory.LoadTop1<ComplianceRiskStatus>(query2);
			AssertNotNull("Compliance Risk remained.", riskStatus2);

			// case 3
			var shipment3 = CreateNewShipment(isInternationalJob: true);
			var complianceRiskStatus3 = CreateNewComplianceRiskStatusForShipment(shipment3);
			complianceRiskStatus3.CopyFromBooking = true;
			var complianceCommodityDetail = complianceRiskStatus3.CommodityDetailCollection.AddNew();
			complianceCommodityDetail.CCD_COR_ComplianceRisk = complianceRiskStatus3.PK;
			complianceCommodityDetail.CCD_CountryOrGrouping = "WCO";
			complianceCommodityDetail.CCD_HarmonizedCode = "321";

			Supporter.DeleteNotSavedAssessmentDetailsCopiedFromBooking(shipment3.PK, Factory);

			var query3 = new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, shipment3.PK);
			query3.FetchOnlyFromLocalCache = true;
			var riskStatus3 = Factory.LoadTop1<ComplianceRiskStatus>(query3);
			AssertNull("Compliance Risk has been removed from cache.", riskStatus3);

			var query4 = new ZQuery(ComplianceCommodityDetailSchema.CCD_COR_ComplianceRisk, complianceRiskStatus3.PK);
			query4.FetchOnlyFromLocalCache = true;
			var commodity = Factory.LoadTop1<ComplianceCommodityDetail>(query4);
			AssertNull("Commodity has been removed from cache.", commodity);
		}

		public void TestAddComplianceDocumentHoldStatusEventLog()
		{
			var shipment = CreateNewShipment(isInternationalJob: true);
			var complianceRiskStatus = CreateNewComplianceRiskStatusForShipment(shipment);
			Supporter.AddComplianceDocumentHoldStatusEventLog((IComplianceItemRiskStatusProvider)shipment, "TST", "Document Name");

			Factory.Save();

			var eventLog = complianceRiskStatus.GetEventLogs().Single(u => u.SCE_EventType == AutoEvents.HoldStatusOverrideCode && u.SCE_EventSubType == ComplianceEventList.Codes.Override);
			AssertEquals("Document Hold Status Overridden|Document Name", eventLog.SCE_EventReference);
			AssertEquals("TST", eventLog.SCE_SystemCreateUser);
			AssertNotEquals("TST", eventLog.SCE_SystemLastEditUser);
		}

		public void TestFetchCommodityDetailsInDB()
		{
			var shipment = CreateNewShipment(isInternationalJob: true);
			shipment.JS_UniqueConsignRef = "S00001111";
			var complianceRiskStatus = CreateNewComplianceRiskStatusForShipment(shipment);
			CreateComplianceCommodityDetail("123456", "CLR", string.Empty, "AU", "Test111", new DateTime(2024, 1, 2, 3, 4, 5));
			CreateComplianceCommodityDetail("654321", "REL", "Dummy Reason", "AU", "Test222", new DateTime(2024, 5, 5, 5, 5, 5));
			CreateComplianceCommodityDetail("112233", "BLK", "Dummy Notes", "AU", "Test333", new DateTime(2024, 5, 4, 3, 2, 1));

			var commodities = Supporter.FetchCommodityDetailsInDB((IComplianceItemRiskStatusProvider)shipment);
			AssertContainsExactElementsInAnyOrder(new[] {
				("123456", "WCO", shipment.PK, shipment.JS_UniqueConsignRef, "CLR", string.Empty, "AU", "Test111", new DateTime(2024, 1, 2, 3, 4, 5)),
				("654321", "WCO", shipment.PK, shipment.JS_UniqueConsignRef, "REL", "Dummy Reason", "AU", "Test222", new DateTime(2024, 5, 5, 5, 5, 5)),
				("112233", "WCO", shipment.PK, shipment.JS_UniqueConsignRef, "BLK", "Dummy Notes", "AU", "Test333", new DateTime(2024, 5, 4, 3, 2, 1)),
			}, commodities.Select(u => ((string)u.HarmonizedCode, (string)u.GroupingOrCountry, u.ParentJobID, u.Source, (string)u.RiskStatus, (string)u.AssessmentNotes, (string)u.Origin, (string)u.GoodsDescription, u.DateAddedUtc.ToDateTime())));

			void CreateComplianceCommodityDetail(string code, string riskStatus, string notes, string originOfGoods, string goodsDescription, DateTime dateTime)
			{
				var commodityDetail = Factory.New<ComplianceCommodityDetail>();
				commodityDetail.CCD_HarmonizedCode = code;
				commodityDetail.CCD_AssessmentNotes = notes;
				commodityDetail.CCD_RN_NKOrigin = originOfGoods;
				commodityDetail.CCD_Description = goodsDescription;
				commodityDetail.CCD_RiskStatus = riskStatus;
				commodityDetail.CCD_COR_ComplianceRisk = complianceRiskStatus.PK;
				commodityDetail.CCD_SystemCreateTimeUtc = dateTime;
			}
		}

		[TestDate(2024, 1, 1)]
		public void TestBorderWiseIntegration_LegalBookLinkClickedLog()
		{
			var shipment = CreateNewShipment(isInternationalJob: true);
			var complianceRiskStatus = CreateNewComplianceRiskStatusForShipment(shipment);
			var complianceCommodityDetail = complianceRiskStatus.CommodityDetailCollection.AddNew();
			complianceCommodityDetail.CCD_COR_ComplianceRisk = complianceRiskStatus.PK;
			complianceCommodityDetail.CCD_CountryOrGrouping = "WCO";
			complianceCommodityDetail.CCD_HarmonizedCode = "321";

			var query = new ZQuery(StmComplianceEventSchema.SCE_ParentID, shipment.PK);
			query.AddToFilter(StmComplianceEventSchema.SCE_EventType, ComplianceEventList.EventType.BorderWiseIntegration);
			query.AddToFilter(StmComplianceEventSchema.SCE_EventSubType, ComplianceEventList.Codes.LegalBooksViewed);
			query.AddToFilter(StmComplianceEventSchema.SCE_EventReference, complianceCommodityDetail.CCD_HarmonizedCode);

			new ComplianceRiskStatusSupporter().AddBorderWiseIntegrationLegalBooksViewedEventLog(complianceCommodityDetail);

			var complianceLog = Factory.Load<StmComplianceEvent>(query);
			AssertEquals(0, complianceLog.Length);

			Factory.Save();
			new ComplianceRiskStatusSupporter().AddBorderWiseIntegrationLegalBooksViewedEventLog(complianceCommodityDetail);
			complianceLog = Factory.Load<StmComplianceEvent>(query);

			AssertEquals(1, complianceLog.Length);
			AssertEquals(shipment.PK, complianceLog[0].SCE_ParentID);
			AssertEquals(JobShipmentSchema.Constants.Prefix, complianceLog[0].SCE_ParentTableCode);
			AssertEquals(ComplianceEventList.EventType.BorderWiseIntegration, complianceLog[0].SCE_EventType);
			AssertEquals(ComplianceEventList.Codes.LegalBooksViewed, complianceLog[0].SCE_EventSubType);
			AssertEquals(complianceCommodityDetail.CCD_HarmonizedCode, complianceLog[0].SCE_EventReference);
			AssertEquals(ZDateTimeOffset.Now, complianceLog[0].SCE_EventTimeOffset);
			AssertEquals(Env.CurrentUser.Initials, complianceLog[0].SCE_SystemCreateUser);
		}

		[TestDate(2024, 1, 1)]
		public void TestBorderWiseIntegration_LegalBookLinkClickedWithOriginOfGoodsLog()
		{
			var shipment = CreateNewShipment(isInternationalJob: true);
			var complianceRiskStatus = CreateNewComplianceRiskStatusForShipment(shipment);
			var complianceCommodityDetail = complianceRiskStatus.CommodityDetailCollection.AddNew();
			complianceCommodityDetail.CCD_COR_ComplianceRisk = complianceRiskStatus.PK;
			complianceCommodityDetail.CCD_CountryOrGrouping = "WCO";
			complianceCommodityDetail.CCD_HarmonizedCode = "321";
			complianceCommodityDetail.CCD_RN_NKOrigin = "US";
			Factory.Save();
			new ComplianceRiskStatusSupporter().AddBorderWiseIntegrationLegalBooksViewedEventLog(complianceCommodityDetail);

			var query = new ZQuery(StmComplianceEventSchema.SCE_ParentID, shipment.PK);
			query.AddToFilter(StmComplianceEventSchema.SCE_EventType, ComplianceEventList.EventType.BorderWiseIntegration);
			query.AddToFilter(StmComplianceEventSchema.SCE_EventSubType, ComplianceEventList.Codes.LegalBooksViewed);
			var complianceLog = Factory.Load<StmComplianceEvent>(query);

			AssertEquals(1, complianceLog.Length);
			AssertEquals(complianceCommodityDetail.CCD_HarmonizedCode + "|ORG=US", complianceLog[0].SCE_EventReference);
		}

		IForwardingShipment CreateNewShipment(bool isInternationalJob)
		{
			var shipment = Factory.New<IForwardingShipment>();

			if (isInternationalJob)
			{
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";
			}

			return shipment;
		}

		ComplianceRiskStatus CreateNewComplianceRiskStatusForShipment(IForwardingShipment shipment)
		{
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = (shipment as CommonShipment).TablePrefix;

			return complianceRiskStatus;
		}

		void AssertFreightMovementRestrictedAndServiceDirection(
			string overallRisk,
			string partyRisk,
			string locationRisk,
			string commodityRisk,
			string freightMovementRestrictionOption,
			bool isCreateRiskStatus,
			bool isExport,
			bool isImport,
			bool isCrossTrade,
			bool isDomestic,
			bool isRestricted)
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			if (isExport)
			{
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";
			}
			else if (isImport)
			{
				shipment.JS_RL_NKOrigin = "USLAX";
				shipment.JS_RL_NKDestination = "AUSYD";
			}
			else if (isCrossTrade)
			{
				shipment.JS_RL_NKOrigin = "INBOM";
				shipment.JS_RL_NKDestination = "USLAX";
			}
			else if (isDomestic)
			{
				shipment.JS_RL_NKOrigin = "USLAX";
				shipment.JS_RL_NKDestination = "USLAX";
			}

			if (isCreateRiskStatus)
			{
				var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
				complianceRiskStatus.COR_ParentID = shipment.PK;
				complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
				complianceRiskStatus.COR_OverallRisk = overallRisk;
				complianceRiskStatus.COR_PartyRisk = partyRisk;
				complianceRiskStatus.COR_LocationRisk = locationRisk;
				complianceRiskStatus.COR_CommodityRisk = commodityRisk;
			}

			Factory.Save();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.ComplianceRiskFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, freightMovementRestrictionOption))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				AssertEquals("Freight movement service direction Export", isExport, shipment.IsExport());
				AssertEquals("Freight movement service direction Import", isImport, shipment.IsImport());
				AssertEquals("Freight movement service direction Cross Trade", isCrossTrade, shipment.IsCrossTrade());
				AssertEquals("Freight movement service direction Domestic", isDomestic, shipment.IsDomestic());
				AssertEquals("Freight movement restriction result", isRestricted, Supporter.IsDPSFreightMovementRestricted(ScreeningStatusesList.Codes.Matched, shipment, shipment));
			}
		}

		public void AssertGetStatus(string overallRisk, string partyRisk, string locationRisk, string commodityRisk)
		{
			var status = Supporter.GetStatus(new DummyBizOForFreightMovementRestrictionTest(Factory, overallRisk, partyRisk, locationRisk, commodityRisk));
			CombineAssertions(() =>
			{
				AssertEquals(overallRisk, status.JobRisk);
				AssertEquals(partyRisk, status.PartyRisk);
				AssertEquals(locationRisk, status.LocationRisk);
				AssertEquals(commodityRisk, status.CommodityRisk);
			});
		}

		void AssertIsDPSFreightMovementRestricted(
			string overallRisk,
			string partyRisk,
			string locationRisk,
			string commodityRisk,
			string dpsFreightMovementRestrictionsOptions,
			bool expectedResult)
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.COR_OverallRisk = overallRisk;
			complianceRiskStatus.COR_PartyRisk = partyRisk;
			complianceRiskStatus.COR_LocationRisk = locationRisk;
			complianceRiskStatus.COR_CommodityRisk = commodityRisk;

			Factory.Save();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.ComplianceRiskFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dpsFreightMovementRestrictionsOptions))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				AssertEquals(expectedResult, Supporter.IsDPSFreightMovementRestricted("MAT", shipment, shipment));
			}
		}

		ComplianceRiskStatusSupporter supporter;
		ComplianceRiskStatusSupporter Supporter => supporter ?? (supporter = new ComplianceRiskStatusSupporter());
	}
}
