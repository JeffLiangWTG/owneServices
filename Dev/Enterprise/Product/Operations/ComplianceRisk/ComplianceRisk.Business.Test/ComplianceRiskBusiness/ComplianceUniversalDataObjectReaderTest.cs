using System;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;

namespace Enterprise.ComplianceRisk.Business.Test
{
	public class ComplianceUniversalDataObjectReaderTest : TestCaseWithFactory
	{
		public void TestInitializeComplianceMaterialChangesSnapshotIfNeeded()
		{
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";

				var dataObjectReader = new ComplianceUniversalDataObjectReader(shipment);
				dataObjectReader.InitializeComplianceMaterialChangesSnapshotIfNeeded();

				AssertNotNull(dataObjectReader.ComplianceRiskBusinessObject.ComplianceMaterialChangesSnapshot);
			}
		}

		public void TestNoExceptionThrownWhenComplianceSupportedCountriesDbCacheExists()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			var commodity = Factory.New<ComplianceCommodityDetail>();
			commodity.CCD_HarmonizedCode = "123";
			commodity.CCD_COR_ComplianceRisk = complianceRiskStatus.PK;

			const string countryiesKey = "ComplianceSupportedCountriesDbCache";
			var query = new ZQuery(StmDataSchema.SD_Name, countryiesKey);
			var stmRow = Factory.LoadTop1<StmData>(query);

			if (stmRow == null)
			{
				stmRow = Factory.New<StmData>();
				stmRow.SD_Name = countryiesKey;
			}

			var supportedCountries = new SupportedCountriesCheckResponseModelWithReportedFlag
			{
				ErrorReported = false,
				LastModifyUtcTime = ZDateTime.UtcNow.ToDateTime(),
				SupportedCountries = new SupportedCountriesCheckResponseModel
				{
					CommodityLevel = new CommodityLevelModel
					{
						Export = new[] { "AU" },
						Import = new[] { "US" },
						OriginOfGoods = new[] { "US" }
					},
					LocationLevel = new LocationLevelModel
					{
						Transshipment = new[] { "AU" },
						Location = new[] { "AU" }
					}
				}
			};

			stmRow.SD_BinaryValue = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(supportedCountries));

			Factory.Save();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var dataObjectReader = new ComplianceUniversalDataObjectReader(shipment);
				AssertNoExceptionThrown(() => dataObjectReader.SynchronizeComplianceRiskStatusIfNeeded());
			}
		}

		public void TestAssessmentDeclinedResetCommodityRiskStatusPossibleRisk()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			var commodity = Factory.New<ComplianceCommodityDetail>();
			commodity.CCD_HarmonizedCode = "123";
			commodity.CCD_COR_ComplianceRisk = complianceRiskStatus.PK;

			StmComplianceEventHelperTest.CreateAssessmentEvent(shipment, ComplianceEventList.Codes.AssessmentDeclined);

			Factory.Save();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var dataObjectReader = new ComplianceUniversalDataObjectReader(shipment);
				dataObjectReader.SynchronizeComplianceRiskStatusIfNeeded();

				AssertEquals("Commodity Aggregate risk status PRS - Possible Risk", ComplianceRiskStatusCodeList.Codes.PossibleRisk, complianceRiskStatus.COR_CommodityRisk);
				AssertEquals("Commodity Line risk status NCH - Possible Risk", ComplianceRiskStatusCodeList.Codes.PossibleRisk, commodity.CCD_RiskStatus);
			}
		}
	}
}
