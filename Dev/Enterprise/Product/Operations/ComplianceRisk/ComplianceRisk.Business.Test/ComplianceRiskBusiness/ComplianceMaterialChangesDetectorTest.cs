using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Newtonsoft.Json;
using NUnit.Framework;
using static Enterprise.ComplianceRisk.Business.Test.ComplianceCommodityDetailCollectionTest;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestClass]
	public class ComplianceMaterialChangesDetectorTest : TestCaseWithFactory
	{
		public void TestJobMaterialChangesWhenAssessmentInitialized_ShouldResetAllCommodityRiskStatus()
		{
			var shipment = Factory.New<ShipmentWithProvider>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			StmComplianceEventHelperTest.CreateAssessmentEvent(shipment, ComplianceEventList.Codes.AssessmentInitialized);

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			var commodity1 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity1.CCD_HarmonizedCode = "12345";
			commodity1.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.HighRisk;

			var commodity2 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity2.CCD_HarmonizedCode = "23456";
			commodity2.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Released;

			var commodity3 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity3.CCD_HarmonizedCode = "34567";
			commodity3.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

			var commodity4 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity4.CCD_HarmonizedCode = "456789";
			commodity4.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Clear;

			complianceRiskStatus.PlugInParent.
				ComplianceMaterialChangesSnapshot = ComplianceCheckRequestModelBuilder.GetRequestModel(complianceRiskStatus.PlugInParent, GetComplianceAssessmentPointPairInfo("AU", "US")).RequestModel;

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				shipment.AssessmentPointPairInfo = GetComplianceAssessmentPointPairInfo("NZ", "US");
				complianceRiskStatus.PlugInParent.ResetCommodityRiskStatusHasJobOrCommodityMaterialChanges();

				AssertPointPairs("Job Material Change - Origin Country: AU to NZ");

				complianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>().ForEach(c => c.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.HighRisk);
				shipment.AssessmentPointPairInfo = GetComplianceAssessmentPointPairInfo("NZ", "AU");
				complianceRiskStatus.PlugInParent.ResetCommodityRiskStatusHasJobOrCommodityMaterialChanges();

				AssertPointPairs("Job Material Change - Destination Country: US to AU");

				complianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>().ForEach(c => c.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.HighRisk);
				shipment.AssessmentPointPairInfo = GetComplianceAssessmentPointPairInfo(null, "NZ");
				complianceRiskStatus.PlugInParent.ResetCommodityRiskStatusHasJobOrCommodityMaterialChanges();

				AssertPointPairs("Job Material Change - Origin Country null");

				complianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>().ForEach(c => c.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.HighRisk);
				shipment.AssessmentPointPairInfo = GetComplianceAssessmentPointPairInfo("NZ", null);
				complianceRiskStatus.PlugInParent.ResetCommodityRiskStatusHasJobOrCommodityMaterialChanges();

				AssertPointPairs("Job Material Change - Destination Country null");

				complianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>().ForEach(c => c.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.HighRisk);
				shipment.AssessmentPointPairInfo = GetComplianceAssessmentPointPairInfo("NZ", "NZ");
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "NZAKL";
				complianceRiskStatus.PlugInParent.ResetCommodityRiskStatusHasJobOrCommodityMaterialChanges();

				AssertEquals("Job Material Change - Ignore Non-International job", 4, complianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>().Count(c => c.CCD_RiskStatus == ComplianceRiskStatusCodeList.Codes.HighRisk));
			}

			void AssertPointPairs(string message)
			{
				CombineAssertions(message, () =>
				{
					AssertEquals(4, complianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>().Count(c => c.CCD_RiskStatus == ComplianceRiskStatusCodeList.Codes.NotChecked));
					AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
					AssertContainsExactElementsInAnyOrder("Snapshot updated", complianceRiskStatus.PlugInParent.ComplianceMaterialChangesSnapshot.PointPairs.Select(u => (u.OriginPoint?.Country, u.DestinationPoint?.Country)), shipment.AssessmentPointPairInfo.PointPairs.Select(u => (u.OriginPoint?.Country, u.DestinationPoint?.Country)));
				});
			}
		}

		public void TestResetCommodityRiskStatusHasJobOrCommodityMaterialChanges_WithoutCommodityProvider()
		{
			var shipment = Factory.New<ShipmentWithoutCommodityProvider>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.AssessmentPointPairInfo = GetComplianceAssessmentPointPairInfo("NZ", "US");

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			var commodity1 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity1.CCD_HarmonizedCode = "12345";
			commodity1.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				complianceRiskStatus.PlugInParent.ResetCommodityRiskStatusHasJobOrCommodityMaterialChanges();
				AssertNull(complianceRiskStatus.PlugInParent.ComplianceMaterialChangesSnapshot);
			}
		}

		public void TestInitializeComplianceMaterialChangesSnapshotForUniversalDataTransfer_WithoutCommodityProvider()
		{
			var shipment = Factory.New<ShipmentWithoutCommodityProvider>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				complianceRiskStatus.PlugInParent.InitializeComplianceMaterialChangesSnapshotForUniversalDataTransfer();
				AssertNull(complianceRiskStatus.PlugInParent.ComplianceMaterialChangesSnapshot);
			}
		}

		public void TestJobMaterialChangesAssessmentInitialized_IgnoreRelatedJobCommodity()
		{
			var subShipment = Factory.New<ShipmentWithProvider>();
			var subComplianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			subComplianceRiskStatus.COR_ParentID = subShipment.PK;
			subComplianceRiskStatus.COR_ParentTableCode = subShipment.TablePrefix;

			StmComplianceEventHelperTest.CreateAssessmentEvent(subShipment, ComplianceEventList.Codes.AssessmentInitialized);

			var subCommodity = subComplianceRiskStatus.CommodityDetailCollection.AddNew();
			subCommodity.CCD_HarmonizedCode = "112233";
			subCommodity.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

			var shipment = Factory.New<ShipmentWithProvider>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.ExposedCommodities = new[] { new ComplianceCommodity("112233", "AU", "Source", subShipment.PK, "AU", "Compliance", "Goods Description", ComplianceRiskStatusCodeList.Codes.Blocked, string.Empty, ZDateTime.UtcNow) };

			StmComplianceEventHelperTest.CreateAssessmentEvent(shipment, ComplianceEventList.Codes.AssessmentInitialized);

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			var commodity = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity.CCD_HarmonizedCode = "12345";
			commodity.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

			complianceRiskStatus.PlugInParent.
				ComplianceMaterialChangesSnapshot = ComplianceCheckRequestModelBuilder.GetRequestModel(complianceRiskStatus.PlugInParent, GetComplianceAssessmentPointPairInfo("AU", "US")).RequestModel;

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				shipment.AssessmentPointPairInfo = GetComplianceAssessmentPointPairInfo("NZ", "US");
				complianceRiskStatus.PlugInParent.ResetCommodityRiskStatusHasJobOrCommodityMaterialChanges();

				AssertContainsExactElementsInAnyOrder("Only reset current job's commodity risk to Not Checked",
					new[]
					{
						(ComplianceRiskStatusCodeList.Codes.NotChecked, CommodityType.UserDataEntry),
						(ComplianceRiskStatusCodeList.Codes.Blocked, CommodityType.RelatedJobLink)
					},
					complianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>().Select(u => (u.CCD_RiskStatus.ToString(), u.CommodityType)));
			}
		}

		public void TestCommodityMaterialChangesAssessmentInitialized_ShouldResetCommodityRiskStatus()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			StmComplianceEventHelperTest.CreateAssessmentEvent(shipment, ComplianceEventList.Codes.AssessmentInitialized);

			var commodity1 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity1.CCD_HarmonizedCode = "12345";
			commodity1.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

			var commodity2 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity2.CCD_HarmonizedCode = "23456";
			commodity2.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Released;

			complianceRiskStatus.PlugInParent.
				ComplianceMaterialChangesSnapshot = ComplianceCheckRequestModelBuilder.GetRequestModel(complianceRiskStatus.PlugInParent, GetComplianceAssessmentPointPairInfo("AU", "US")).RequestModel;
			complianceRiskStatus.PlugInParent.ComplianceMaterialChangesSnapshot.Commodities = complianceRiskStatus.PlugInParent.ComplianceMaterialChangesSnapshot.Commodities
				.Select(u => new ComplianceCheckRequestCommodityModel { Origin = new[] { "US" }, HsCode = u.HsCode, HsCodeDescription = u.HsCodeDescription })
				.ToArray();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				complianceRiskStatus.PlugInParent.ResetCommodityRiskStatusHasJobOrCommodityMaterialChanges();

				CombineAssertions("Commodity Material Change Origin", () =>
				{
					AssertEquals(2, complianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>().Count(c => c.CCD_RiskStatus == ComplianceRiskStatusCodeList.Codes.NotChecked));
					AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
				});
			}

			commodity1.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;
			commodity2.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Released;

			complianceRiskStatus.PlugInParent.ComplianceMaterialChangesSnapshot.Commodities = complianceRiskStatus.PlugInParent.ComplianceMaterialChangesSnapshot.Commodities
				.Select(u => new ComplianceCheckRequestCommodityModel { Origin = u.Origin, HsCode = "111222", HsCodeDescription = u.HsCodeDescription })
				.ToArray();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				complianceRiskStatus.PlugInParent.ResetCommodityRiskStatusHasJobOrCommodityMaterialChanges();

				CombineAssertions("Commodity Material Change HS Code", () =>
				{
					AssertEquals(2, complianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>().Count(c => c.CCD_RiskStatus == ComplianceRiskStatusCodeList.Codes.NotChecked));
					AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
				});
			}

			commodity1.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;
			commodity2.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Released;

			complianceRiskStatus.PlugInParent.ComplianceMaterialChangesSnapshot.Commodities = complianceRiskStatus.PlugInParent.ComplianceMaterialChangesSnapshot.Commodities
				.Select(u => new ComplianceCheckRequestCommodityModel { Origin = u.Origin, HsCode = u.HsCode, HsCodeDescription = "Test123456" })
				.ToArray();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				complianceRiskStatus.PlugInParent.ResetCommodityRiskStatusHasJobOrCommodityMaterialChanges();

				CombineAssertions("Commodity Material Change Description When BLK or REL", () =>
				{
					AssertEquals(2, complianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>().Count(c => c.CCD_RiskStatus == ComplianceRiskStatusCodeList.Codes.NotChecked));
					AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
				});
			}

			commodity1.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.HighRisk;
			commodity2.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Clear;

			complianceRiskStatus.PlugInParent.ComplianceMaterialChangesSnapshot.Commodities = complianceRiskStatus.PlugInParent.ComplianceMaterialChangesSnapshot.Commodities
				.Select(u => new ComplianceCheckRequestCommodityModel { Origin = u.Origin, HsCode = u.HsCode, HsCodeDescription = "Test123456" })
				.ToArray();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				complianceRiskStatus.PlugInParent.ResetCommodityRiskStatusHasJobOrCommodityMaterialChanges();

				CombineAssertions("Commodity Material Change Description When HSK or CLR", () =>
				{
					AssertEquals(0, complianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>().Count(c => c.CCD_RiskStatus == ComplianceRiskStatusCodeList.Codes.NotChecked));
					AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
				});
			}
		}

		public void TestUpdateComplianceMaterialChangesSnapshot()
		{
			var shipment = Factory.NewWithValidTestData<ShipmentWithProvider>();
			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			var requestModel = ComplianceCheckRequestModelBuilder.GetRequestModel(complianceRiskStatus.PlugInParent, GetComplianceAssessmentPointPairInfo("AU", "US")).RequestModel;

			CombineAssertions("Update ComplianceMaterialChangesSnapshot", () =>
			{
				ComplianceMaterialChangesDetector.UpdateComplianceMaterialChangeSnapshot(complianceRiskStatus.PlugInParent, requestModel, updateSnapshot: true);

				AssertEquals(JsonConvert.SerializeObject(requestModel), JsonConvert.SerializeObject(complianceRiskStatus.PlugInParent.ComplianceMaterialChangesSnapshot));
				AssertNotNull(complianceRiskStatus.PlugInParent.ComplianceMaterialChangesSnapshot);

				requestModel = ComplianceCheckRequestModelBuilder.GetRequestModel(complianceRiskStatus.PlugInParent, GetComplianceAssessmentPointPairInfo("AU", "US")).RequestModel;
				requestModel.Commodities = new[] {
					new ComplianceCheckRequestCommodityModel
					{
						HsCode = "123456",
						Origin = new[] { "AU" },
						GoodsDescription = "Dummy Goods Description"
					}
				};

				ComplianceMaterialChangesDetector.UpdateComplianceMaterialChangeSnapshot(complianceRiskStatus.PlugInParent, requestModel, updateSnapshot: false);
				AssertContainsExactElementsInAnyOrder(new[]
				{
					("123456", "AU", "Dummy Goods Description"),
				},
				complianceRiskStatus
				.PlugInParent
				.ComplianceMaterialChangesSnapshot
				.Commodities
				.Select(u => (u.HsCode, u.Origin?.SingleOrDefault(), u.GoodsDescription)));

				requestModel = ComplianceCheckRequestModelBuilder.GetRequestModel(complianceRiskStatus.PlugInParent, GetComplianceAssessmentPointPairInfo("AU", "US")).RequestModel;
				requestModel.Commodities = new[] {
					new ComplianceCheckRequestCommodityModel
					{
						HsCode = "123456",
						Origin = new[] { "US" },
						GoodsDescription = "Dummy Goods Description"
					},
					new ComplianceCheckRequestCommodityModel
					{
						HsCode = "123456",
						Origin = null,
						GoodsDescription = "Dummy Goods Description"
					},
					new ComplianceCheckRequestCommodityModel
					{
						HsCode = "123456",
						Origin = new[] { "AU" },
						GoodsDescription = "DUMMY GOODS DESCRIPTION"
					}
				};

				ComplianceMaterialChangesDetector.UpdateComplianceMaterialChangeSnapshot(complianceRiskStatus.PlugInParent, requestModel, updateSnapshot: false);
				AssertContainsExactElementsInAnyOrder(new[]
				{
					("123456", "AU", "Dummy Goods Description"),
					("123456", "US", "Dummy Goods Description"),
					("123456", null, "Dummy Goods Description"),
				},
				complianceRiskStatus
				.PlugInParent
				.ComplianceMaterialChangesSnapshot
				.Commodities
				.Select(u => (u.HsCode, u.Origin?.SingleOrDefault(), u.GoodsDescription)));
			});
		}

		ComplianceAssessmentPointPairInfo GetComplianceAssessmentPointPairInfo(string orignCountry, string destinationCountry)
		{
			return new ComplianceAssessmentPointPairInfo(new List<ComplianceCheckRequestPointPair>
			{
				new ()
				{
					OriginPoint = orignCountry is null ? null : new ComplianceCheckRequestPointPairLocation
					{
						Country = orignCountry,
						UNLOCO = orignCountry + "ORN",
						MovementDescription = "Origin"
					},
					DestinationPoint = destinationCountry is null ? null : new ComplianceCheckRequestPointPairLocation
					{
						Country = destinationCountry,
						UNLOCO = destinationCountry + "DES",
						MovementDescription = "Destination"
					},
					EstimatedTimeOfArrival = new ZDateTime(2024, 1, 1),
					EstimatedTimeOfDeparture = new ZDateTime(2024, 2, 1),
					Mode = "SEA"
				}
			});
		}
	}
}
