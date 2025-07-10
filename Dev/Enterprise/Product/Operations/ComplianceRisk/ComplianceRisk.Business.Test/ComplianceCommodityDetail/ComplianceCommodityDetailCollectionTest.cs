using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(ComplianceCommodityDetailCollection))]
	public class ComplianceCommodityDetailCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestLoadWithAllCommoditiesFromRelatedJob()
		{
			var shipmentProvider = Factory.New<ShipmentWithProvider>();
			var complianceRiskStatus = GetCommodityRiskStatusDefaultClear(shipmentProvider);
			shipmentProvider.ExposedCommodities = new[] { GetComplianceCommodity("062483", "WCO", "Source2", Guid.NewGuid()), GetComplianceCommodity("679213", "WCO", "Source3", Guid.NewGuid(), "CLR", "Dummy Notes") };
			var collection = new ComplianceCommodityDetailCollection(complianceRiskStatus, shipmentProvider);
			collection.Load();
			AssertEquals(2, collection.Count);
			collection.Reload(false);
			AssertEquals(2, collection.Count);
			collection.Reload(true);
			AssertEquals(2, collection.Count);
			Factory.Save();

			var commodityDetailsInDB = new BusinessObjectFactory().Load<ComplianceCommodityDetail>(new ZQuery(ComplianceCommodityDetailSchema.CCD_HarmonizedCode, new[] { "062483", "679213" }));
			AssertEquals(0, commodityDetailsInDB.Length);

			collection.Load();
			AssertEquals(2, collection.Count);
			AssertCommodityDetails(collection[0], "Source2", "062483", "", readOnly: true, savedByFactory: false, harmonizedCodeReadOnly: true, CommodityType.RelatedJobLink);
			AssertCommodityDetails(collection[1], "Source3", "679213", "", readOnly: true, savedByFactory: false, harmonizedCodeReadOnly: true, CommodityType.RelatedJobLink);
		}

		public void TestLoadWithAllCommoditiesFromRelatedJob_AssignAssessmentNotes()
		{
			var shipmentProvider = Factory.New<ShipmentWithProvider>();
			var relatedJobPK = ZGuid.NewZGuid();
			var complianceRiskStatus = GetCommodityRiskStatusDefaultClear(shipmentProvider);
			shipmentProvider.ExposedCommodities = new[] { GetComplianceCommodity("062483", "WCO", "Source2", relatedJobPK), GetComplianceCommodity("062483", "WCO", "Source2", relatedJobPK, "CLR", "Dummy Notes"), GetComplianceCommodity("987654", "WCO", "Source2", relatedJobPK, "REL", "Dummy Notes 2"), GetComplianceCommodity("123456", "WCO", "Source3", Guid.NewGuid()) };
			var collection = new ComplianceCommodityDetailCollection(complianceRiskStatus, shipmentProvider);
			collection.Load();
			AssertEquals(4, collection.Count);
			AssertCommodityDetails(collection[0], "Source2", "062483", "", readOnly: true, savedByFactory: false, harmonizedCodeReadOnly: true, CommodityType.RelatedJobLink);
			AssertCommodityDetails(collection[1], "Source2", "062483", "", readOnly: true, savedByFactory: false, harmonizedCodeReadOnly: true, CommodityType.RelatedJobLink);
			AssertCommodityDetails(collection[2], "Source2", "987654", "", readOnly: true, savedByFactory: false, harmonizedCodeReadOnly: true, CommodityType.RelatedJobLink);
			AssertCommodityDetails(collection[3], "Source3", "123456", "", readOnly: true, savedByFactory: false, harmonizedCodeReadOnly: true, CommodityType.RelatedJobLink);
			AssertEquals(string.Empty, collection[0].CCD_AssessmentNotes);
			AssertEquals("Dummy Notes", collection[1].CCD_AssessmentNotes);
			AssertEquals("Dummy Notes 2", collection[2].CCD_AssessmentNotes);
			AssertEquals(string.Empty, collection[3].CCD_AssessmentNotes);
		}

		public void TestLoadHasAssignedTemporaryProperties()
		{
			var tariff1 = Factory.NewWithValidTestData<TariffView>();
			tariff1.ZZ1_ZZZ_NKDataGrouping = "WCO";
			tariff1.ZZ1_TariffCode = "123456";
			tariff1.ZZ1_Description = "TariffDescription1";
			tariff1.ZZ1_StartDate = new ZDateTime(1900, 01, 01);
			tariff1.ZZ1_EndDate = new ZDateTime(9999, 12, 31);

			var tariff2 = Factory.NewWithValidTestData<TariffView>();
			tariff2.ZZ1_ZZZ_NKDataGrouping = "WCO";
			tariff2.ZZ1_TariffCode = "654321";
			tariff2.ZZ1_Description = "TariffDescription2";
			tariff2.ZZ1_StartDate = new ZDateTime(1900, 01, 01);
			tariff2.ZZ1_EndDate = new ZDateTime(9999, 12, 31);

			var shipmentProvider = Factory.New<ShipmentWithProvider>();
			var complianceRiskStatus = GetCommodityRiskStatusDefaultClear(shipmentProvider);
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipmentProvider);
			shipmentProvider.ExposedCommodities = new[] { GetComplianceCommodity(tariff1.ZZ1_TariffCode, "WCO", "Test001", shipmentProvider.ParentID, "AU"), GetComplianceCommodity(tariff1.ZZ1_TariffCode, "WCO", "Test001", shipmentProvider.ParentID, "US"), GetComplianceCommodity(tariff1.ZZ1_TariffCode, "WCO", "Test002", Guid.NewGuid()) };
			var collection = complianceRiskStatus.CommodityDetailCollection;
			collection.Load();
			var detail = collection.AddNew();
			detail.CCD_HarmonizedCode = tariff2.ZZ1_TariffCode;
			AssertEquals(4, collection.Count);
			AssertCommodityDetails(collection[0], "Test002", "123456", string.Empty, readOnly: true, savedByFactory: false, harmonizedCodeReadOnly: true, CommodityType.RelatedJobLink);
			AssertCommodityDetails(collection[1], "Test001", "123456", string.Empty, readOnly: false, savedByFactory: true, harmonizedCodeReadOnly: true, CommodityType.FetchDataEntry, "AU");
			AssertCommodityDetails(collection[2], "Test001", "123456", string.Empty, readOnly: false, savedByFactory: true, harmonizedCodeReadOnly: true, CommodityType.FetchDataEntry, "US");
			AssertCommodityDetails(collection[3], "Test001", "654321", string.Empty, readOnly: false, savedByFactory: true, harmonizedCodeReadOnly: false, CommodityType.UserDataEntry);
		}

		public void TestLoadWithAllCommoditiesFromCurrentJob()
		{
			var shipmentProvider = Factory.New<ShipmentWithProvider>();
			var complianceRiskStatus = GetCommodityRiskStatusDefaultClear(shipmentProvider);
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipmentProvider);

			var collection = complianceRiskStatus.CommodityDetailCollection;
			var complianceCommodityDetail = collection.AddNew();
			complianceCommodityDetail.CCD_COR_ComplianceRisk = complianceRiskStatus.PK;
			complianceCommodityDetail.CCD_CountryOrGrouping = "WCO";
			complianceCommodityDetail.CCD_HarmonizedCode = "123";

			collection.Load();
			AssertEquals(1, collection.Count);
			AssertCommodityDetails(collection[0], shipmentProvider.JS_UniqueConsignRef, "123", string.Empty, readOnly: false, savedByFactory: true, harmonizedCodeReadOnly: false, CommodityType.UserDataEntry);

			shipmentProvider.ExposedCommodities = new[] { GetComplianceCommodity("062483", "WCO", shipmentProvider.JS_UniqueConsignRef, shipmentProvider.ParentID) };

			collection.Load();
			AssertEquals(2, collection.Count);
			AssertCommodityDetails(collection[0], shipmentProvider.JS_UniqueConsignRef, "123", string.Empty, readOnly: false, savedByFactory: true, harmonizedCodeReadOnly: false, CommodityType.UserDataEntry);
			AssertCommodityDetails(collection[1], shipmentProvider.JS_UniqueConsignRef, "062483", string.Empty, readOnly: false, savedByFactory: true, harmonizedCodeReadOnly: true, CommodityType.FetchDataEntry);
		}

		public void TestSaveAllCommoditiesFromCurrentJob()
		{
			var shipmentProvider = Factory.New<ShipmentWithProvider>();
			var complianceRiskStatus = GetCommodityRiskStatusDefaultClear(shipmentProvider);
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipmentProvider);
			shipmentProvider.ExposedCommodities = new[] { GetComplianceCommodity("123456", "WCO", shipmentProvider.JS_UniqueConsignRef, shipmentProvider.ParentID), GetComplianceCommodity("123456", "WCO", "Test002", Guid.NewGuid()) };

			var collection = complianceRiskStatus.CommodityDetailCollection;
			var complianceCommodityDetail1 = collection.AddNew();
			complianceCommodityDetail1.CCD_COR_ComplianceRisk = complianceRiskStatus.PK;
			complianceCommodityDetail1.CCD_CountryOrGrouping = "WCO";
			complianceCommodityDetail1.CCD_HarmonizedCode = "654321";

			var complianceCommodityDetail2 = collection.AddNew();
			complianceCommodityDetail2.CCD_COR_ComplianceRisk = complianceRiskStatus.PK;
			complianceCommodityDetail2.CCD_CountryOrGrouping = "WCO";
			complianceCommodityDetail2.CCD_HarmonizedCode = "111222";

			collection.Load();
			AssertEquals(4, collection.Count);
			AssertCommodityDetailsInCollection(true);

			Factory.Save();
			var commodityDetailsInDB = new BusinessObjectFactory().Load<ComplianceCommodityDetail>(new ZQuery(ComplianceCommodityDetailSchema.CCD_HarmonizedCode, new[] { "123456", "654321", "111222" }).AddToFilter(ComplianceCommodityDetailSchema.CCD_COR_ComplianceRisk, complianceRiskStatus.PK));
			AssertEquals(3, commodityDetailsInDB.Length);

			collection.Load();
			AssertEquals(4, collection.Count);
			AssertCommodityDetailsInCollection(false);

			void AssertCommodityDetailsInCollection(bool needToSaveByFactory)
			{
				var collectionData = collection.Cast<ComplianceCommodityDetail>().ToArray();
				AssertCommodityDetails(collectionData.Single(u => u.Source == "Test002"), "Test002", "123456", string.Empty, readOnly: true, savedByFactory: false, harmonizedCodeReadOnly: true, CommodityType.RelatedJobLink);
				AssertCommodityDetails(collectionData.Single(u => u.Source == shipmentProvider.JS_UniqueConsignRef && u.CCD_HarmonizedCode == "123456"), shipmentProvider.JS_UniqueConsignRef, "123456", string.Empty, readOnly: false, savedByFactory: needToSaveByFactory, harmonizedCodeReadOnly: true, CommodityType.FetchDataEntry);
				AssertCommodityDetails(collectionData.Single(u => u.PK == complianceCommodityDetail1.PK), shipmentProvider.JS_UniqueConsignRef, "654321", string.Empty, readOnly: false, savedByFactory: needToSaveByFactory, harmonizedCodeReadOnly: false, CommodityType.UserDataEntry);
				AssertCommodityDetails(collectionData.Single(u => u.PK == complianceCommodityDetail2.PK), shipmentProvider.JS_UniqueConsignRef, "111222", string.Empty, readOnly: false, savedByFactory: needToSaveByFactory, harmonizedCodeReadOnly: false, CommodityType.UserDataEntry);
			}
		}

		public void TestDeleteCommodityFromCollectionWhenOverallRiskIsOverrideClear()
		{
			var shipmentProvider = Factory.New<ShipmentWithProvider>();
			var complianceRiskStatus = GetCommodityRiskStatusDefaultClear(shipmentProvider);
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipmentProvider);
			shipmentProvider.ExposedCommodities = new[] { GetComplianceCommodity("123456", "WCO", shipmentProvider.JS_UniqueConsignRef, shipmentProvider.ParentID), GetComplianceCommodity("123456", "WCO", "Test002", Guid.NewGuid()) };

			var collection = complianceRiskStatus.CommodityDetailCollection;
			var complianceCommodityDetail = collection.AddNew();
			complianceCommodityDetail.CCD_CountryOrGrouping = "WCO";
			complianceCommodityDetail.CCD_HarmonizedCode = "654321";
			collection.Load();

			AssertEquals(3, collection.Count);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Held, complianceRiskStatus.COR_OverallRisk);

			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.OverrideClear;
			Factory.Save();

			collection.RemoveAndDelete(complianceCommodityDetail);
			AssertEquals(2, collection.Count);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, complianceRiskStatus.COR_OverallRisk);
		}

		public void TestMaintainCommodityCollection()
		{
			var shipmentProvider = Factory.New<ShipmentWithProvider>();
			var complianceRiskStatus = GetCommodityRiskStatusDefaultClear(shipmentProvider);
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipmentProvider);
			shipmentProvider.ExposedCommodities = new[] { GetComplianceCommodity("123456", "WCO", shipmentProvider.JS_UniqueConsignRef, shipmentProvider.ParentID), GetComplianceCommodity("123456", "WCO", "Test002", Guid.NewGuid()) };

			var collection = complianceRiskStatus.CommodityDetailCollection;
			var complianceCommodityDetail = collection.AddNew();
			complianceCommodityDetail.CCD_COR_ComplianceRisk = complianceRiskStatus.PK;
			complianceCommodityDetail.CCD_CountryOrGrouping = "WCO";
			complianceCommodityDetail.CCD_HarmonizedCode = "654321";

			collection.Load();
			AssertEquals(3, collection.Count);
			var collectionData = collection.Cast<ComplianceCommodityDetail>();
			AssertCommodityDetails(collectionData.Single(u => u.Source == "Test002"), "Test002", "123456", string.Empty, readOnly: true, savedByFactory: false, harmonizedCodeReadOnly: true, CommodityType.RelatedJobLink);
			AssertCommodityDetails(collectionData.Single(u => u.Source == shipmentProvider.JS_UniqueConsignRef && u.CCD_HarmonizedCode == "123456"), shipmentProvider.JS_UniqueConsignRef, "123456", string.Empty, readOnly: false, savedByFactory: true, harmonizedCodeReadOnly: true, CommodityType.FetchDataEntry);
			AssertCommodityDetails(collectionData.Single(u => u.PK == complianceCommodityDetail.PK), shipmentProvider.JS_UniqueConsignRef, "654321", string.Empty, readOnly: false, savedByFactory: true, harmonizedCodeReadOnly: false, CommodityType.UserDataEntry);

			// Mock commodity 123456 removed from job
			shipmentProvider.ExposedCommodities = new[] { GetComplianceCommodity("123456", "WCO", "Test002", Guid.NewGuid()) };
			collection.Load();
			AssertEquals("123456 is removed from collection", 2, collection.Count);
			AssertCommodityDetails(collectionData.Single(u => u.Source == "Test002"), "Test002", "123456", string.Empty, readOnly: true, savedByFactory: false, harmonizedCodeReadOnly: true, CommodityType.RelatedJobLink);
			AssertCommodityDetails(collectionData.Single(u => u.PK == complianceCommodityDetail.PK), shipmentProvider.JS_UniqueConsignRef, "654321", string.Empty, readOnly: false, savedByFactory: true, harmonizedCodeReadOnly: false, CommodityType.UserDataEntry);

			// Mock commodity 111222 added to job
			shipmentProvider.ExposedCommodities = new[] { GetComplianceCommodity("123456", "WCO", "Test002", Guid.NewGuid()), GetComplianceCommodity("111222", "WCO", shipmentProvider.JS_UniqueConsignRef, shipmentProvider.ParentID) };
			collection.Load();
			AssertEquals("111222 is added to collection", 3, collection.Count);
			AssertCommodityDetails(collectionData.Single(u => u.Source == shipmentProvider.JS_UniqueConsignRef && u.CCD_HarmonizedCode == "111222"), shipmentProvider.JS_UniqueConsignRef, "111222", string.Empty, readOnly: false, savedByFactory: true, harmonizedCodeReadOnly: true, CommodityType.FetchDataEntry);

			// Mock commodity 654321 added to not user eneter commodity, will update existing commodity from CurrentJob_UserEntered to CurrentJob_NotUserEntered
			shipmentProvider.ExposedCommodities = new[] { GetComplianceCommodity("123456", "WCO", "Test002", Guid.NewGuid()), GetComplianceCommodity("111222", "WCO", shipmentProvider.JS_UniqueConsignRef, shipmentProvider.ParentID), GetComplianceCommodity("654321", "WCO", shipmentProvider.JS_UniqueConsignRef, shipmentProvider.ParentID) };
			collection.Load();
			AssertEquals("654321 is update", 3, collection.Count);
			AssertCommodityDetails(collectionData.Single(u => u.PK == complianceCommodityDetail.PK), shipmentProvider.JS_UniqueConsignRef, "654321", string.Empty, readOnly: false, savedByFactory: true, harmonizedCodeReadOnly: true, CommodityType.FetchDataEntry);

			// Mock commodity 654321 updated to 222333, will delete 654321 then add 222333
			shipmentProvider.ExposedCommodities = new[] { GetComplianceCommodity("123456", "WCO", "Test002", Guid.NewGuid()), GetComplianceCommodity("111222", "WCO", shipmentProvider.JS_UniqueConsignRef, shipmentProvider.ParentID), GetComplianceCommodity("222333", "WCO", shipmentProvider.JS_UniqueConsignRef, shipmentProvider.ParentID) };
			collection.Load();
			AssertEquals(3, collection.Count);
			AssertNull("654321 is deleted", collectionData.FirstOrDefault(u => u.PK == complianceCommodityDetail.PK));
			AssertCommodityDetails(collectionData.Single(u => u.CCD_HarmonizedCode == "222333"), shipmentProvider.JS_UniqueConsignRef, "222333", string.Empty, readOnly: false, savedByFactory: true, harmonizedCodeReadOnly: true, CommodityType.FetchDataEntry);

			// Mock user manually enter a new commodity
			var commodity = collection.AddNew();
			commodity.CCD_HarmonizedCode = "985985";
			AssertEquals(CommodityType.UserDataEntry, commodity.CommodityType);

			collection.Load();
			AssertEquals(4, collection.Count);
			AssertCommodityDetails(collectionData.Single(u => u.CCD_HarmonizedCode == "985985"), shipmentProvider.JS_UniqueConsignRef, "985985", string.Empty, readOnly: false, savedByFactory: true, harmonizedCodeReadOnly: false, CommodityType.UserDataEntry);
		}

		public void TestMaintainCommodityCollectionWithOriginAndDescriptionLogic()
		{
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipmentProvider = Factory.New<ShipmentWithProvider>();

				StmComplianceEventHelperTest.CreateAssessmentEvent(shipmentProvider, ComplianceEventList.Codes.AssessmentInitialized);

				var complianceRiskStatus = GetCommodityRiskStatusDefaultClear(shipmentProvider);
				complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipmentProvider);
				shipmentProvider.ExposedCommodities = new[] {
					GetComplianceCommodityWithOriginAndDescription("123456", "WCO", shipmentProvider.JS_UniqueConsignRef, shipmentProvider.ParentID, "AU", "Test"),
					GetComplianceCommodityWithOriginAndDescription("123456", "WCO", shipmentProvider.JS_UniqueConsignRef, shipmentProvider.ParentID, "US", "Test"),
					GetComplianceCommodityWithOriginAndDescription("654321", "WCO", shipmentProvider.JS_UniqueConsignRef, shipmentProvider.ParentID, "US", "ABCDEFG"),
				};

				var collection = complianceRiskStatus.CommodityDetailCollection;
				var collectionData = collection.Cast<ComplianceCommodityDetail>();

				collection.Load();
				AssertEquals(3, collection.Count);

				collection[0].CCD_RiskStatus = "BLK";
				collection[1].CCD_RiskStatus = "CLR";
				collection[2].CCD_RiskStatus = "CLR";

				AssertCommodityDetails(collection[0], shipmentProvider.JS_UniqueConsignRef, "123456", "Test", readOnly: false, savedByFactory: true, hsCodeAndOriginAndDescriptionReadOnly: true, CommodityType.FetchDataEntry, "NCH", "AU");
				AssertCommodityDetails(collection[1], shipmentProvider.JS_UniqueConsignRef, "123456", "Test", readOnly: false, savedByFactory: true, hsCodeAndOriginAndDescriptionReadOnly: true, CommodityType.FetchDataEntry, "NCH", "US");
				AssertCommodityDetails(collection[2], shipmentProvider.JS_UniqueConsignRef, "654321", "ABCDEFG", readOnly: false, savedByFactory: true, hsCodeAndOriginAndDescriptionReadOnly: true, CommodityType.FetchDataEntry, "NCH", "US");

				// Mock user modifies the packing line commodities' origin of goods and goods description
				shipmentProvider.ExposedCommodities = new[] {
					GetComplianceCommodityWithOriginAndDescription("123456", "WCO", shipmentProvider.JS_UniqueConsignRef, shipmentProvider.ParentID, "AU", "Test111"),
					GetComplianceCommodityWithOriginAndDescription("123456", "WCO", shipmentProvider.JS_UniqueConsignRef, shipmentProvider.ParentID, "CN", "Test"),
					GetComplianceCommodityWithOriginAndDescription("654321", "WCO", shipmentProvider.JS_UniqueConsignRef, shipmentProvider.ParentID, "US", "abcdefg"),
				};

				collection.Load();
				AssertEquals("Still 3 FetchDataEntry commodities", 3, collection.Count);
				AssertCommodityDetails(collectionData.Single(u => u.CCD_HarmonizedCode == "123456" && u.CCD_RN_NKOrigin == "AU" && u.CCD_Description == "Test111"), shipmentProvider.JS_UniqueConsignRef, "123456", "Test111", readOnly: false, savedByFactory: true, hsCodeAndOriginAndDescriptionReadOnly: true, CommodityType.FetchDataEntry, "NCH", "AU");
				AssertCommodityDetails(collectionData.Single(u => u.CCD_HarmonizedCode == "123456" && u.CCD_RN_NKOrigin == "CN" && u.CCD_Description == "Test"), shipmentProvider.JS_UniqueConsignRef, "123456", "Test", readOnly: false, savedByFactory: true, hsCodeAndOriginAndDescriptionReadOnly: true, CommodityType.FetchDataEntry, "NCH", "CN");
				AssertCommodityDetails(collectionData.Single(u => u.CCD_HarmonizedCode == "654321" && u.CCD_RN_NKOrigin == "US" && u.CCD_Description == "ABCDEFG"), shipmentProvider.JS_UniqueConsignRef, "654321", "ABCDEFG", readOnly: false, savedByFactory: true, hsCodeAndOriginAndDescriptionReadOnly: true, CommodityType.FetchDataEntry, "NCH", "US");

				// Mock user manually enter a new commodity
				var commodity = collection.AddNew();
				commodity.CCD_HarmonizedCode = "654321";
				commodity.CCD_RN_NKOrigin = "US";
				commodity.CCD_Description = "User manually enter";
				AssertEquals(CommodityType.UserDataEntry, commodity.CommodityType);

				collection.Load();
				AssertEquals(4, collection.Count);
				AssertCommodityDetails(collectionData.Single(u => u.CCD_HarmonizedCode == "654321" && u.CCD_Description == "User manually enter"), shipmentProvider.JS_UniqueConsignRef, "654321", "User manually enter", readOnly: false, savedByFactory: true, hsCodeAndOriginAndDescriptionReadOnly: false, CommodityType.UserDataEntry, "NCH", "US");

				collectionData.Single(u => u.CCD_HarmonizedCode == "654321" && u.CCD_Description == "User manually enter").CCD_RiskStatus = "BLK";
				collectionData.Single(u => u.CCD_HarmonizedCode == "654321" && u.CCD_Description == "User manually enter").CCD_Description = "ABCDEFGabcdefg";
				collection.Load();
				AssertEquals(4, collection.Count);
				AssertCommodityDetails(collectionData.Single(u => u.CCD_HarmonizedCode == "654321" && u.CCD_Description == "ABCDEFGabcdefg"), shipmentProvider.JS_UniqueConsignRef, "654321", "ABCDEFGabcdefg", readOnly: false, savedByFactory: true, hsCodeAndOriginAndDescriptionReadOnly: false, CommodityType.UserDataEntry, "NCH", "US");
			}
		}

		public void TestLoadWhenParentHasNoCodeProperty()
		{
			var risk = Factory.New<ComplianceRiskStatusForTest>();
			var status = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			AssertExceptionThrown<Exception>(() => new ComplianceCommodityDetailCollection(status, risk));
		}

		public void TestLoadWhenParentIsNotBusinessObject()
		{
			var status = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			AssertExceptionThrown<ArgumentNullException>(() => new ComplianceCommodityDetailCollection(status, new NonBusinessObject()));
		}

		public void TestCommodityRiskStatusWithCondition()
		{
			var shipmentProvider = Factory.New<ShipmentWithProvider>();
			var complianceRiskStatus = GetCommodityRiskStatusDefaultClear(shipmentProvider);
			var tariff1 = ComplianceRiskTariffTestDataHelper.CreateTariffWithConditions(Factory, "456285", "Test Conditions");
			var tariff2 = ComplianceRiskTariffTestDataHelper.CreateNewOrLoadTariff(Factory, "145982");

			shipmentProvider.ExposedCommodities = new[]
			{
				GetComplianceCommodity(tariff1.ZZ1_TariffCode, "WCO", "Source", shipmentProvider.ParentID),
				GetComplianceCommodity(tariff2.ZZ1_TariffCode, "WCO", "Source", shipmentProvider.ParentID)
			};
			var collection = new ComplianceCommodityDetailCollection(complianceRiskStatus, shipmentProvider);
			collection.Load();

			CombineAssertions(() =>
			{
				AssertEquals(2, collection.Count);
				AssertEquals("Harmonized found conditions", true, collection.Cast<ComplianceCommodityDetail>().Any(u => u.CCD_HarmonizedCode == tariff1.ZZ1_TariffCode && u.CCD_RiskStatus == ComplianceRiskStatusCodeList.Codes.NotChecked));
				AssertEquals("Harmonized no conditions", true, collection.Cast<ComplianceCommodityDetail>().Any(u => u.CCD_HarmonizedCode == tariff2.ZZ1_TariffCode && u.CCD_RiskStatus == ComplianceRiskStatusCodeList.Codes.NotChecked));
			});
		}

		public void TestCommodityChangeUpdateCommodityRiskAndOverallRiskStatus()
		{
			var shipmentProvider = Factory.New<ShipmentWithProvider>();
			var complianceRiskStatus = GetCommodityRiskStatusDefaultClear(shipmentProvider);
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Clear;

			var tariff = ComplianceRiskTariffTestDataHelper.CreateNewOrLoadTariff(Factory, "110623");

			shipmentProvider.ExposedCommodities = new[] { GetComplianceCommodity(tariff.ZZ1_TariffCode, "WCO", "Source", shipmentProvider.ParentID) };
			var collection = new ComplianceCommodityDetailCollection(complianceRiskStatus, shipmentProvider);
			collection.Load();

			AssertEquals(1, collection.Count);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_CommodityRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);

			collection.RemoveAndDelete(collection[0]);

			AssertEquals(ComplianceRiskStatusCodeList.Codes.Unknown, complianceRiskStatus.COR_CommodityRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Held, complianceRiskStatus.COR_OverallRisk);
		}

		public void TestCommodityChangeUpdateCommodityRiskAndOverallRiskStatus_WithBorderWiseAPIIntegration()
		{
			var shipmentProvider = Factory.New<ShipmentWithProvider>();
			var complianceRiskStatus = GetCommodityRiskStatusDefaultClear(shipmentProvider);
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;

			StmComplianceEventHelperTest.CreateAssessmentEvent(shipmentProvider, ComplianceEventList.Codes.AssessmentInitialized);

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var commodityDetail = complianceRiskStatus.CommodityDetailCollection.AddNew();
				commodityDetail.CCD_HarmonizedCode = "123456";
				commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_CommodityRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);

				commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Clear;
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_CommodityRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);

				commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.NotChecked;
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Unknown, complianceRiskStatus.COR_CommodityRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Held, complianceRiskStatus.COR_OverallRisk);

				commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.PossibleRisk;
				AssertEquals(ComplianceRiskStatusCodeList.Codes.PossibleRisk, complianceRiskStatus.COR_CommodityRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);

				commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.HighRisk;
				AssertEquals(ComplianceRiskStatusCodeList.Codes.HighRisk, complianceRiskStatus.COR_CommodityRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Held, complianceRiskStatus.COR_OverallRisk);

				commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Released;
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_CommodityRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);

				// Support Legacy Status
				commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
				AssertEquals(ComplianceRiskStatusCodeList.Codes.HighRisk, complianceRiskStatus.COR_CommodityRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Held, complianceRiskStatus.COR_OverallRisk);
			}
		}

		public void TestCommodityDetailNomenclatureConditionAndSpecificCondition()
		{
			var commodity = Factory.New<ComplianceCommodityDetail>();
			CombineAssertions("Pre-condition", () =>
			{
				AssertEquals(ZString.Empty, commodity.NomenclatureCondition);
				AssertEquals(ZString.Empty, commodity.SpecificCondition);
			});

			commodity.CCD_NomenclatureCondition = true;
			commodity.CCD_SpecificCondition = true;
			AssertEquals("Yes", commodity.NomenclatureCondition);
			AssertEquals("Yes", commodity.SpecificCondition);
		}

		public void TestUpdateJobSourceNumberIfNeeded()
		{
			var shipmentProvider = Factory.New<ForwardingShipment>();
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			var collection = new ComplianceCommodityDetailCollection(complianceRiskStatus, shipmentProvider);
			var detail = collection.AddNew();
			detail.CCD_HarmonizedCode = "123456";
			collection.Load();
			AssertEquals(ZString.Empty, collection[0].Source);

			shipmentProvider.JS_UniqueConsignRef = "S00000001";
			collection.UpdateJobSourceNumberIfNeeded();
			AssertEquals("S00000001", collection[0].Source);
		}

		public void TestCommodityDetailCommoditySource()
		{
			var shipmentProvider = Factory.New<ShipmentWithProvider>();
			var complianceRiskStatus = GetCommodityRiskStatusDefaultClear(shipmentProvider);
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Clear;

			var tariff1 = ComplianceRiskTariffTestDataHelper.CreateNewOrLoadTariff(Factory, "110623");
			var tariff2 = ComplianceRiskTariffTestDataHelper.CreateNewOrLoadTariff(Factory, "848210");
			var tariff3 = ComplianceRiskTariffTestDataHelper.CreateNewOrLoadTariff(Factory, "848291");

			var sameParentID = Guid.NewGuid();

			var originOfGoods = "AU";
			var goodsDescription = "Test";

			shipmentProvider.ExposedCommodities = new[]
			{
				new ComplianceCommodity(tariff1.ZZ1_TariffCode, "WCO", "Source", Guid.NewGuid(), originOfGoods, "Commercial Invoice", goodsDescription, "CLR", "Dummy Notes", ZDateTime.UtcNow),
				new ComplianceCommodity(tariff2.ZZ1_TariffCode, "WCO", "Source", Guid.NewGuid(), originOfGoods, "Packing", goodsDescription, "CLR", "Dummy Notes", ZDateTime.UtcNow),
				new ComplianceCommodity(tariff3.ZZ1_TariffCode, "WCO", "Source", sameParentID, originOfGoods, "Packing", goodsDescription, "CLR", "Dummy Notes", ZDateTime.UtcNow),
				new ComplianceCommodity(tariff3.ZZ1_TariffCode, "WCO", "Source", sameParentID, originOfGoods, "Commercial Invoice", goodsDescription, "CLR", "Dummy Notes", ZDateTime.UtcNow),
				new ComplianceCommodity(tariff3.ZZ1_TariffCode, "WCO", "Source", Guid.NewGuid(), originOfGoods, "Commercial Invoice", goodsDescription, "CLR", "Dummy Notes", ZDateTime.UtcNow),
				new ComplianceCommodity("123456", "WCO", "Source", Guid.NewGuid(), originOfGoods, string.Empty, goodsDescription, "CLR", "Dummy Notes", ZDateTime.UtcNow),
				new ComplianceCommodity("111111", "WCO", "Source", sameParentID, originOfGoods, string.Empty, goodsDescription, "CLR", "Dummy Notes", ZDateTime.UtcNow),
				new ComplianceCommodity("111111", "WCO", "Source", sameParentID, originOfGoods, "Commercial Invoice", goodsDescription, "CLR", "Dummy Notes", ZDateTime.UtcNow),
			};

			var collection = new ComplianceCommodityDetailCollection(complianceRiskStatus, shipmentProvider);
			var newCommodityDetail = collection.AddNew();
			newCommodityDetail.CCD_HarmonizedCode = "555555";

			collection.Load();
			AssertContainsExactElementsInAnyOrder(new[]
			{
				("Commercial Invoice", "110623"),
				("Packing", "848210"),
				("Commercial Invoice", "848291"),
				("Commercial Invoice, Packing", "848291"),
				("Compliance", "123456"),
				("Commercial Invoice", "111111"),
				("Compliance", "555555")
			}, collection.Select(v => (v.CommoditySource.ToString(), v.CCD_HarmonizedCode.ToString())));
		}

		public void TestExtractAllNumbersFromTariffCode()
		{
			AssertEquals("123456", ComplianceCommodityDetailCollection.ExtractAllNumbersFromTariffCode("12.34 5 6"));
		}

		public void TestResetRiskStatus_WhenCommodityDetailHasChanges()
		{
			var shipmentProvider = Factory.New<ShipmentWithProvider>();

			StmComplianceEventHelperTest.CreateAssessmentEvent(shipmentProvider, ComplianceEventList.Codes.AssessmentInitialized);

			var complianceRiskStatus = GetCommodityRiskStatusDefaultClear(shipmentProvider);
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipmentProvider);

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var commodityDetail = complianceRiskStatus.CommodityDetailCollection.AddNew();
				commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;
				commodityDetail.CCD_HarmonizedCode = "123456";
				AssertEquals("NeedResetStatus change to false when risk status been reset", false, commodityDetail.NeedResetStatus);
				AssertEquals("Reset the status when harmonized code change", ComplianceRiskStatusCodeList.Codes.NotChecked, commodityDetail.CCD_RiskStatus);

				commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Released;
				AssertEquals("Not reset status when harmonized code not change and current status is REL", ComplianceRiskStatusCodeList.Codes.Released, commodityDetail.CCD_RiskStatus);

				commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;
				AssertEquals("Not reset status when harmonized code not change and current status is BLK", ComplianceRiskStatusCodeList.Codes.Blocked, commodityDetail.CCD_RiskStatus);

				complianceRiskStatus.CommodityDetailCollection.Load();
				AssertEquals("Not reset status when load and current status is BLK or REL", ComplianceRiskStatusCodeList.Codes.Blocked, commodityDetail.CCD_RiskStatus);

				commodityDetail.CCD_HarmonizedCode = "654321";
				AssertEquals("Reset the status when harmonized code change", ComplianceRiskStatusCodeList.Codes.NotChecked, commodityDetail.CCD_RiskStatus);
			}
		}

		public void TestResetRiskStatus_WhenCommodityDetailHasChanges_WithBorderWiseAPIIntegration()
		{
			var shipmentProvider = Factory.New<ShipmentWithProvider>();

			StmComplianceEventHelperTest.CreateAssessmentEvent(shipmentProvider, ComplianceEventList.Codes.AssessmentInitialized);

			var complianceRiskStatus = GetCommodityRiskStatusDefaultClear(shipmentProvider);
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipmentProvider);

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var commodityDetail = complianceRiskStatus.CommodityDetailCollection.AddNew();
				commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;
				commodityDetail.CCD_HarmonizedCode = "123456";
				AssertEquals("NeedResetStatus change to false when risk status been reset", false, commodityDetail.NeedResetStatus);
				AssertEquals("Reset the status when harmonized code change", ComplianceRiskStatusCodeList.Codes.NotChecked, commodityDetail.CCD_RiskStatus);

				commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Released;
				AssertEquals("Not reset status when harmonized code not change", ComplianceRiskStatusCodeList.Codes.Released, commodityDetail.CCD_RiskStatus);

				complianceRiskStatus.CommodityDetailCollection.Load();
				AssertEquals("Not reset status when load", ComplianceRiskStatusCodeList.Codes.Released, commodityDetail.CCD_RiskStatus);

				var checker = new Mock<ISupportCheckCommodityRiskStatus>();
				checker.Setup(u => u.CheckCommoditiesRiskStatus(It.IsAny<ComplianceCommodityDetail[]>())).Callback<ComplianceCommodityDetail[]>(u =>
				{
					u.Single().CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.HighRisk;
				});

				var plugInParent = new ComplianceRiskPlugInBusinessObject(shipmentProvider);
				plugInParent.CommodityRiskStatusChecker = checker.Object;
				complianceRiskStatus.PlugInParent = plugInParent;

				commodityDetail.CCD_HarmonizedCode = "654321";
				AssertEquals("Check the status from checker when harmonized code change", ComplianceRiskStatusCodeList.Codes.HighRisk, commodityDetail.CCD_RiskStatus);
			}
		}

		public void TestHsCodeFromFetchDataOnlyKeepNumericCharactersAndUpperCaseLetters()
		{
			var shipmentProvider = Factory.New<ShipmentWithProvider>();
			var complianceRiskStatus = GetCommodityRiskStatusDefaultClear(shipmentProvider);

			var sameParentID = Guid.NewGuid();

			var originOfGoods = "AU";
			var goodsDescription = "Test";

			shipmentProvider.ExposedCommodities = new[]
			{
				new ComplianceCommodity("112233", "WCO", "Source", Guid.NewGuid(), originOfGoods, "Commercial Invoice", goodsDescription, "CLR", "Dummy Notes", ZDateTime.UtcNow),
				new ComplianceCommodity("123.456", "WCO", "Source", Guid.NewGuid(), originOfGoods, "Packing", goodsDescription, "CLR", "Dummy Notes", ZDateTime.UtcNow),
				new ComplianceCommodity("2233 44", "WCO", "Source", Guid.NewGuid(), originOfGoods, "Commercial Invoice", goodsDescription, "CLR", "Dummy Notes", ZDateTime.UtcNow),
				new ComplianceCommodity("1234abc", "WCO", "Source", Guid.NewGuid(), originOfGoods, "Packing", goodsDescription, "CLR", "Dummy Notes", ZDateTime.UtcNow),
				new ComplianceCommodity("111abc@$%", "WCO", "Source", Guid.NewGuid(), originOfGoods, "Commercial Invoice", goodsDescription, "CLR", "Dummy Notes", ZDateTime.UtcNow),
				new ComplianceCommodity("12ab cd", "WCO", "Source", Guid.NewGuid(), originOfGoods, "Packing", goodsDescription, "CLR", "Dummy Notes", ZDateTime.UtcNow),
			};

			var collection = new ComplianceCommodityDetailCollection(complianceRiskStatus, shipmentProvider);

			collection.Load();
			AssertContainsExactElementsInAnyOrder(new[]
			{
				("112233","Commercial Invoice"),
				("123456","Packing"),
				("223344","Commercial Invoice"),
				("1234ABC","Packing"),
				("111ABC","Commercial Invoice"),
				("12ABCD","Packing"),
			}, collection.Select(v => (v.CCD_HarmonizedCode.ToString(), v.CommoditySource.ToString())));
		}

		public void TestLoadWithSuspendChanges_WithCommodities()
		{
			var declarationProvider = Factory.New<DeclarationWithProvider>();
			var complianceRiskStatus = GetCommodityRiskStatusDefaultClear(declarationProvider);
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(declarationProvider);
			complianceRiskStatus.InitializeAssessmentWorkflow();

			var collection = complianceRiskStatus.CommodityDetailCollection;
			var complianceCommodityDetail = collection.AddNew();
			complianceCommodityDetail.CCD_COR_ComplianceRisk = complianceRiskStatus.PK;
			complianceCommodityDetail.CCD_CountryOrGrouping = "WCO";
			complianceCommodityDetail.CCD_HarmonizedCode = "123";

			collection.Load();
			AssertEquals(1, collection.Count);

			var newCommodity = new ComplianceCommodity("112233", "WCO", "Source", declarationProvider.PK, "",
				"Commercial Invoice", "", "CLR", "Dummy Notes", ZDateTime.UtcNow);
			collection.LoadWithSuspendChanges(new [] {
				newCommodity
			});
			AssertEquals(2, collection.Count);

			var key = ComplianceRiskHelper.GetUnionKeyFromCommodity("112233", "WCO", "", "");
			AssertEquals(true, collection.CommoditiesLinkedToCurrentJob.ContainsKey(key));

			var commodity = collection.Cast<ComplianceCommodityDetail>().FirstOrDefault(u => u.CCD_HarmonizedCode == "112233");
			AssertNotNull(commodity);

			var helper = (DummyInteractionWithComplianceWiseCommoditiesHelper)declarationProvider.Helper;
			AssertEquals(0, helper.CpwSideCommoditiesChanged.Length);

			commodity.ImportAlertsForExportJobDescription = "HSK";
			commodity.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

			AssertEquals(1, helper.CpwSideCommoditiesChanged.Length);
			AssertEquals("112233", helper.CpwSideCommoditiesChanged[0].HarmonizedCode);
			AssertEquals(ZString.Empty, helper.CpwSideCommoditiesChanged[0].ImportAlertStatus);
		}

		#region Implementation

		ComplianceCommodity GetComplianceCommodity(ZString harmonizedCode, ZString groupingOrCountry, ZString source, ZGuid parentJobID, ZString origin = default)
		{
			return new ComplianceCommodity(harmonizedCode, groupingOrCountry, source, parentJobID, origin, string.Empty, string.Empty);
		}

		ComplianceCommodity GetComplianceCommodity(ZString harmonizedCode, ZString groupingOrCountry, ZString source, ZGuid parentJobID, ZString riskStatus, ZString assessmentNotes)
		{
			return new ComplianceCommodity(harmonizedCode, groupingOrCountry, source, parentJobID, string.Empty, string.Empty, "Test", riskStatus, assessmentNotes, ZDateTime.UtcNow);
		}

		ComplianceCommodity GetComplianceCommodityWithOriginAndDescription(ZString harmonizedCode, ZString groupingOrCountry, ZString source, ZGuid parentJobID, ZString originOfGoods, ZString goodsDescription)
		{
			return new ComplianceCommodity(harmonizedCode, groupingOrCountry, source, parentJobID, originOfGoods, string.Empty, goodsDescription);
		}

		ComplianceRiskStatus GetCommodityRiskStatusDefaultClear(ShipmentWithProvider shipmentWithProvider)
		{
			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			complianceRiskStatus.COR_ParentID = shipmentWithProvider.PK;
			complianceRiskStatus.COR_PartyRisk = "CLR";
			complianceRiskStatus.COR_LocationRisk = "CLR";
			complianceRiskStatus.COR_OverallRisk = "CLR";
			return complianceRiskStatus;
		}

		void AssertCommodityDetails(ComplianceCommodityDetail complianceCommodityDetail, string source, string harmonizedCode, string description, bool readOnly, bool savedByFactory, bool harmonizedCodeReadOnly, CommodityType commodityType, ZString originOfGoods = default)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Source", source, complianceCommodityDetail.Source);
				AssertEquals("HarmonizedCode", harmonizedCode, complianceCommodityDetail.CCD_HarmonizedCode);
				AssertEquals("CountryOrGrouping", "WCO", complianceCommodityDetail.CCD_CountryOrGrouping);
				AssertEquals("Description", description, complianceCommodityDetail.Description);
				AssertEquals("ReadOnly", readOnly, complianceCommodityDetail.ReadOnly);
				AssertEquals("IsSavedByFactory", savedByFactory, complianceCommodityDetail.IsSavedByFactory);
				AssertEquals("HarmonizedCode ReadOnly", harmonizedCodeReadOnly, complianceCommodityDetail.CCD_HarmonizedCodeInfo.ReadOnly);
				AssertEquals("CommodityType", commodityType, complianceCommodityDetail.CommodityType);
				AssertEquals("OriginOfGoods", originOfGoods, complianceCommodityDetail.OriginOfGoods);
			});
		}

		void AssertCommodityDetails(ComplianceCommodityDetail complianceCommodityDetail, string source, string harmonizedCode, string description, bool readOnly, bool savedByFactory, bool hsCodeAndOriginAndDescriptionReadOnly, CommodityType commodityType, ZString riskStatus, ZString originOfGoods)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Source", source, complianceCommodityDetail.Source);
				AssertEquals("HarmonizedCode", harmonizedCode, complianceCommodityDetail.CCD_HarmonizedCode);
				AssertEquals("CountryOrGrouping", "WCO", complianceCommodityDetail.CCD_CountryOrGrouping);
				AssertEquals("Description", description, complianceCommodityDetail.CCD_Description);
				AssertEquals("ReadOnly", readOnly, complianceCommodityDetail.ReadOnly);
				AssertEquals("IsSavedByFactory", savedByFactory, complianceCommodityDetail.IsSavedByFactory);
				AssertEquals("HarmonizedCode ReadOnly", hsCodeAndOriginAndDescriptionReadOnly, complianceCommodityDetail.CCD_HarmonizedCodeInfo.ReadOnly);
				AssertEquals("CCD_DescriptionInfo ReadOnly", hsCodeAndOriginAndDescriptionReadOnly, complianceCommodityDetail.CCD_DescriptionInfo.ReadOnly);
				AssertEquals("CCD_RN_NKOriginInfo ReadOnly", hsCodeAndOriginAndDescriptionReadOnly, complianceCommodityDetail.CCD_RN_NKOriginInfo.ReadOnly);
				AssertEquals("CommodityType", commodityType, complianceCommodityDetail.CommodityType);
				AssertEquals("RiskStatus", riskStatus, complianceCommodityDetail.CCD_RiskStatus);
				AssertEquals("OriginOfGoods", originOfGoods, complianceCommodityDetail.CCD_RN_NKOrigin);
			});
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new DummyBizObjThatImplementPartyAndLocationAndCommodityProvider(Factory).ComplianceRiskPlugInBusinessObjectForTest.ComplianceRiskStatus.CommodityDetailCollection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<ComplianceCommodityDetail>();
		}

		public class ShipmentWithoutCommodityProvider : CommonShipment, ICompliancePartyRiskStatusProvider, IComplianceLocationRiskStatusProvider
		{
			public ShipmentWithoutCommodityProvider(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			IEnumerable<IScreeningParty> ICompliancePartyRiskStatusProvider.Parties => Array.Empty<ScreeningParty>();

			IEnumerable<IComplianceLocation> IComplianceLocationRiskStatusProvider.Locations => Array.Empty<ScreeningParty>();

			IEnumerable<IComplianceItemRiskStatusProvider> IComplianceItemRiskStatusProvider.SubComplianceRiskStatusProviders => throw new NotImplementedException();

			IEnumerable<IComplianceItemRiskStatusProvider> IComplianceItemRiskStatusProvider.ParentComplianceRiskStatusProviders => throw new NotImplementedException();

			public ZGuid ParentID => PK;

			public ZString ParentTableCode => "JS";

			public override ZString JS_UniqueConsignRef => "Test001";

			ComplianceRiskSupport IComplianceItemRiskStatusProvider.ComplianceRiskSupport => ComplianceRiskSupport.SupportInitialization;

			Func<DocumentDeliveryResultForComplianceWorkflow> IComplianceItemRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			public ComplianceAssessmentPointPairInfo AssessmentPointPairInfo { get; set; } = new();

			public (ZBool IsCurrent, ZDateTime JobEndDate) JobTime => (true, ZDateTime.BrettsBirthday);

			public ZBool IsEnabledComplianceWise => true;

			SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideOverallRiskStatusSecurity => Env.Security.ShipmentsComplianceAllowOverrideOverallRiskStatus;

			SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowResynchronizeRiskStatusSecurity => Env.Security.ShipmentsComplianceAllowResynchronizeRiskStatus;

			SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideFreightMovementRestrictionsSecurity => Env.Security.ShipmentsComplianceAllowOverrideFreightMovementRestrictions;
		}

		public class ShipmentWithProvider : ShipmentWithoutCommodityProvider, IComplianceCommodityRiskStatusProvider
		{
			public ShipmentWithProvider(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			IEnumerable<IComplianceCommodity> IComplianceCommodityRiskStatusProvider.Commodities => ExposedCommodities;

			public IEnumerable<ComplianceCommodity> ExposedCommodities { get; set; } = Enumerable.Empty<ComplianceCommodity>();

			ZDateTime IComplianceCommodityRiskStatusProvider.EffectiveDate => new(2022, 10, 20);

			public ZBool IsEditingCommoditySupported => ZBool.True;

			CommodityRiskCalculateFactor IComplianceCommodityRiskStatusProvider.RiskCalculateFactor => CommodityRiskCalculateFactor.All;

			SecurityCheckpoint IComplianceCommodityRiskStatusProvider.EditHarmonizedCodeSecurity => Env.Security.ShipmentsComplianceEditHarmonizedCode;

			SecurityCheckpoint IComplianceCommodityRiskStatusProvider.EditComplianceAssessmentSecurity => Env.Security.ShipmentsComplianceEditComplianceAssessment;

			SecurityCheckpoint IComplianceCommodityRiskStatusProvider.AllowComplianceAssessmentSecurity => Env.Security.ShipmentsComplianceAllowComplianceAssessment;

			SecurityCheckpoint IComplianceCommodityRiskStatusProvider.DeclineComplianceAssessmentSecurity => Env.Security.ShipmentsComplianceDeclineComplianceAssessment;
		}

		public class DeclarationWithProvider : ShipmentWithProvider, ISupportInteractionWithComplianceWiseCommodities, IComplianceCommodityRiskStatusProvider
		{
			protected override AutologState AutoLoggingState => AutologState.NotLogged;

			public DeclarationWithProvider(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
				Enabled = true;
				Helper = new DummyInteractionWithComplianceWiseCommoditiesHelper();
			}
			public bool Enabled { get; }
			public IInteractionWithComplianceWiseCommoditiesHelper Helper { get; set; }

			CommodityRiskCalculateFactor IComplianceCommodityRiskStatusProvider.RiskCalculateFactor
			{
				get
				{
					return SetRiskCalculateFactor;
				}
			}

			public CommodityRiskCalculateFactor SetRiskCalculateFactor { get; set; } = CommodityRiskCalculateFactor.Export;
		}

		public class ShipmentOnlyWithCommodityProvider : CommonShipment, IComplianceCommodityRiskStatusProvider
		{
			public ShipmentOnlyWithCommodityProvider(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public IEnumerable<ComplianceCommodity> CommoditiesExposed { get; set; } = Enumerable.Empty<ComplianceCommodity>();

			ZDateTime IComplianceCommodityRiskStatusProvider.EffectiveDate => new(2022, 10, 20);

			IEnumerable<IComplianceCommodity> IComplianceCommodityRiskStatusProvider.Commodities => CommoditiesExposed;

			IEnumerable<IComplianceItemRiskStatusProvider> IComplianceItemRiskStatusProvider.SubComplianceRiskStatusProviders => throw new NotImplementedException();

			IEnumerable<IComplianceItemRiskStatusProvider> IComplianceItemRiskStatusProvider.ParentComplianceRiskStatusProviders => throw new NotImplementedException();

			public ZGuid ParentID => PK;

			public ZString ParentTableCode => "JS";

			public override ZString JS_UniqueConsignRef => "Test001";

			ComplianceRiskSupport IComplianceItemRiskStatusProvider.ComplianceRiskSupport => ComplianceRiskSupport.SupportInitialization;

			Func<DocumentDeliveryResultForComplianceWorkflow> IComplianceItemRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			public ComplianceAssessmentPointPairInfo AssessmentPointPairInfo { get; set; } = new();

			public (ZBool IsCurrent, ZDateTime JobEndDate) JobTime => (true, ZDateTime.BrettsBirthday);

			public ZBool IsEnabledComplianceWise => true;

			public ZBool IsEditingCommoditySupported => ZBool.True;

			CommodityRiskCalculateFactor IComplianceCommodityRiskStatusProvider.RiskCalculateFactor => CommodityRiskCalculateFactor.All;

			SecurityCheckpoint IComplianceCommodityRiskStatusProvider.EditHarmonizedCodeSecurity => Env.Security.ShipmentsComplianceEditHarmonizedCode;

			SecurityCheckpoint IComplianceCommodityRiskStatusProvider.EditComplianceAssessmentSecurity => Env.Security.ShipmentsComplianceEditComplianceAssessment;

			SecurityCheckpoint IComplianceCommodityRiskStatusProvider.AllowComplianceAssessmentSecurity => Env.Security.ShipmentsComplianceAllowComplianceAssessment;

			SecurityCheckpoint IComplianceCommodityRiskStatusProvider.DeclineComplianceAssessmentSecurity => Env.Security.ShipmentsComplianceDeclineComplianceAssessment;

			SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideOverallRiskStatusSecurity => Env.Security.ShipmentsComplianceAllowOverrideOverallRiskStatus;

			SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowResynchronizeRiskStatusSecurity => Env.Security.ShipmentsComplianceAllowResynchronizeRiskStatus;

			SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideFreightMovementRestrictionsSecurity => Env.Security.ShipmentsComplianceAllowOverrideFreightMovementRestrictions;
		}

		class ComplianceRiskStatusForTest : ComplianceRiskStatus, ICompliancePartyRiskStatusProvider, IComplianceLocationRiskStatusProvider, IComplianceCommodityRiskStatusProvider
		{
			public ComplianceRiskStatusForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			IEnumerable<IComplianceCommodity> IComplianceCommodityRiskStatusProvider.Commodities => Enumerable.Empty<ComplianceCommodity>();

			ZDateTime IComplianceCommodityRiskStatusProvider.EffectiveDate => ZDateTime.Empty;

			IEnumerable<IScreeningParty> ICompliancePartyRiskStatusProvider.Parties => throw new NotImplementedException();

			IEnumerable<IComplianceItemRiskStatusProvider> IComplianceItemRiskStatusProvider.ParentComplianceRiskStatusProviders => throw new NotImplementedException();

			IEnumerable<IComplianceLocation> IComplianceLocationRiskStatusProvider.Locations => throw new NotImplementedException();

			IEnumerable<IComplianceItemRiskStatusProvider> IComplianceItemRiskStatusProvider.SubComplianceRiskStatusProviders => throw new NotImplementedException();

			public ZGuid ParentID => PK;

			public ZString ParentTableCode => "JS";

			ComplianceRiskSupport IComplianceItemRiskStatusProvider.ComplianceRiskSupport => throw new NotImplementedException();

			Func<DocumentDeliveryResultForComplianceWorkflow> IComplianceItemRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			public ComplianceAssessmentPointPairInfo AssessmentPointPairInfo => throw new NotImplementedException();

			public (ZBool IsCurrent, ZDateTime JobEndDate) JobTime => (true, ZDateTime.BrettsBirthday);

			public ZBool IsEnabledComplianceWise => true;

			public ZBool IsEditingCommoditySupported => ZBool.True;

			CommodityRiskCalculateFactor IComplianceCommodityRiskStatusProvider.RiskCalculateFactor => CommodityRiskCalculateFactor.All;

			SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideOverallRiskStatusSecurity => throw new NotImplementedException();

			SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowResynchronizeRiskStatusSecurity => throw new NotImplementedException();

			SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideFreightMovementRestrictionsSecurity => throw new NotImplementedException();

			SecurityCheckpoint IComplianceCommodityRiskStatusProvider.EditHarmonizedCodeSecurity => throw new NotImplementedException();

			SecurityCheckpoint IComplianceCommodityRiskStatusProvider.EditComplianceAssessmentSecurity => throw new NotImplementedException();

			SecurityCheckpoint IComplianceCommodityRiskStatusProvider.AllowComplianceAssessmentSecurity => throw new NotImplementedException();

			SecurityCheckpoint IComplianceCommodityRiskStatusProvider.DeclineComplianceAssessmentSecurity => throw new NotImplementedException();
		}

		class NonBusinessObject : ICompliancePartyRiskStatusProvider, IComplianceLocationRiskStatusProvider, IComplianceCommodityRiskStatusProvider
		{
			IEnumerable<IComplianceCommodity> IComplianceCommodityRiskStatusProvider.Commodities => throw new NotImplementedException();

			ZDateTime IComplianceCommodityRiskStatusProvider.EffectiveDate => ZDateTime.Empty;

			IEnumerable<IScreeningParty> ICompliancePartyRiskStatusProvider.Parties => throw new NotImplementedException();

			IEnumerable<IComplianceLocation> IComplianceLocationRiskStatusProvider.Locations => throw new NotImplementedException();

			IEnumerable<IComplianceItemRiskStatusProvider> IComplianceItemRiskStatusProvider.SubComplianceRiskStatusProviders => throw new NotImplementedException();

			IEnumerable<IComplianceItemRiskStatusProvider> IComplianceItemRiskStatusProvider.ParentComplianceRiskStatusProviders => throw new NotImplementedException();

			public ZGuid ParentID => Guid.NewGuid();

			public ZString ParentTableCode => "JS";

			public BusinessObjectFactory Factory => throw new NotImplementedException();

			ComplianceRiskSupport IComplianceItemRiskStatusProvider.ComplianceRiskSupport => throw new NotImplementedException();

			public (ZBool IsCurrent, ZDateTime JobEndDate) JobTime => (true, ZDateTime.BrettsBirthday);

			Func<DocumentDeliveryResultForComplianceWorkflow> IComplianceItemRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			public ComplianceAssessmentPointPairInfo AssessmentPointPairInfo => throw new NotImplementedException();

			public ZBool IsEnabledComplianceWise => true;

			public ZBool IsEditingCommoditySupported => ZBool.True;

			CommodityRiskCalculateFactor IComplianceCommodityRiskStatusProvider.RiskCalculateFactor => CommodityRiskCalculateFactor.All;

			SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideOverallRiskStatusSecurity => throw new NotImplementedException();

			SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowResynchronizeRiskStatusSecurity => throw new NotImplementedException();

			SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideFreightMovementRestrictionsSecurity => throw new NotImplementedException();

			SecurityCheckpoint IComplianceCommodityRiskStatusProvider.EditHarmonizedCodeSecurity => throw new NotImplementedException();

			SecurityCheckpoint IComplianceCommodityRiskStatusProvider.EditComplianceAssessmentSecurity => throw new NotImplementedException();

			SecurityCheckpoint IComplianceCommodityRiskStatusProvider.AllowComplianceAssessmentSecurity => throw new NotImplementedException();

			SecurityCheckpoint IComplianceCommodityRiskStatusProvider.DeclineComplianceAssessmentSecurity => throw new NotImplementedException();
		}

		IDisposable setAllowComplianceCommodityRiskAssessmentToTrue;
		protected override void SetUp()
		{
			base.SetUp();
			setAllowComplianceCommodityRiskAssessmentToTrue = OrganisationsDataRegistry.Instance.AllowComplianceCommodityRiskAssessment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			setAllowComplianceCommodityRiskAssessmentToTrue.Dispose();
		}

		#endregion
	}
}
