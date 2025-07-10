using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.ComplianceRisk.Integration.ComplianceRiskStatusCodeList;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(ComplianceRiskPlugInBusinessObject))]
	public class ComplianceRiskPlugInBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetComplianceRiskPlugInBusinessObjectsThrowArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => ComplianceRiskPlugInBusinessObject.GetComplianceRiskPlugInBusinessObjects(null));
			AssertExceptionThrown<ArgumentException>(() => ComplianceRiskPlugInBusinessObject.GetComplianceRiskPlugInBusinessObjects(Array.Empty<ICompliancePartyRiskStatusProvider>()));
			AssertExceptionThrown<ArgumentException>(() => ComplianceRiskPlugInBusinessObject.GetComplianceRiskPlugInBusinessObjects(new ICompliancePartyRiskStatusProvider[] { null }));
		}

		public void TestGetComplianceRiskPlugInBusinessObjects()
		{
			var bizo1 = GetNewPartyAndLocationAndCommodityRiskStatusBizO();
			bizo1.ComplianceRiskStatus.COR_ParentID = ZGuid.Empty;

			var bizo2 = GetNewPartyAndLocationAndCommodityRiskStatusBizO();
			bizo2.ComplianceRiskStatus.COR_OverallRisk = "XX";

			var bizos = ComplianceRiskPlugInBusinessObject.GetComplianceRiskPlugInBusinessObjects(new[] { bizo1, bizo2 });

			AssertEquals(2, bizos.Length);
			Assert(bizos.All(x => x.ComplianceRiskStatus != null));
			AssertEquals(bizo2, bizos.Single(x => x.ComplianceRiskStatus.COR_OverallRisk == "XX").HostBusinessEntity);
		}

		public void TestCreateComplianceRiskStatus()
		{
			var bizo = GetNewPartyAndLocationAndCommodityRiskStatusBizO();
			bizo.ComplianceRiskStatus.COR_ParentID = ZGuid.Empty;

			var plugInBizO = new ComplianceRiskPlugInBusinessObject(bizo);

			AssertNotNull(plugInBizO.ComplianceRiskStatus);
			AssertEquals(bizo.ParentID, plugInBizO.ComplianceRiskStatus.COR_ParentID);
			AssertEquals(bizo.ParentTableCode, plugInBizO.ComplianceRiskStatus.COR_ParentTableCode);
		}

		public void TestRegisterEditableChildObjectForComplianceRiskStatus()
		{
			var bizo = GetNewPartyAndLocationAndCommodityRiskStatusBizO();
			var plugInBizO = new ComplianceRiskPlugInBusinessObject(bizo);

			AssertNotNull(plugInBizO.ComplianceRiskStatus);
			AssertEquals(true, bizo.HasChanges);

			Factory.Save();
			AssertEquals(false, bizo.HasChanges);

			plugInBizO.ComplianceRiskStatus.COR_OverallRisk = Codes.Blocked;
			AssertEquals("bizo is RegisterEditableChildObject of the ComplianceRiskStatus", true, bizo.HasChanges);

			bizo = GetNewPartyAndLocationAndCommodityRiskStatusBizO();
			bizo.HasChanges = true;
			plugInBizO = new ComplianceRiskPlugInBusinessObject(bizo);
			AssertNotNull(plugInBizO.ComplianceRiskStatus);
			AssertEquals(true, bizo.HasChanges);
		}

		public void TestLoadComplianceRiskStatus()
		{
			var bizo = GetNewPartyAndLocationAndCommodityRiskStatusBizO();
			var riskStatus = bizo.ComplianceRiskPlugInBusinessObjectForTest.ComplianceRiskStatus;
			var pluginBizO = new ComplianceRiskPlugInBusinessObject(bizo);

			AssertEquals(riskStatus, pluginBizO.ComplianceRiskStatus);
		}

		public void TestPartyRiskDescription()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment);
			pluginBizO.ComplianceRiskStatus.COR_PartyRisk = Codes.Clear;
			AssertEquals(Descriptions.Clear, pluginBizO.PartyRiskDescription);

			pluginBizO.ComplianceRiskStatus.COR_PartyRisk = Codes.PotentialRisk;
			AssertEquals(Descriptions.PotentialRisk, pluginBizO.PartyRiskDescription);

			pluginBizO.ComplianceRiskStatus.COR_PartyRisk = Codes.Blocked;
			AssertEquals(Descriptions.Blocked, pluginBizO.PartyRiskDescription);

			pluginBizO.ComplianceRiskStatus.COR_PartyRisk = Codes.HighRisk;
			AssertEquals(Descriptions.HighRisk, pluginBizO.PartyRiskDescription);
		}

		public void TestLocationRiskDescription()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment);
			pluginBizO.ComplianceRiskStatus.COR_LocationRisk = Codes.Clear;
			AssertEquals(Descriptions.Clear, pluginBizO.LocationRiskDescription);

			pluginBizO.ComplianceRiskStatus.COR_LocationRisk = Codes.PotentialRisk;
			AssertEquals(Descriptions.PotentialRisk, pluginBizO.LocationRiskDescription);

			pluginBizO.ComplianceRiskStatus.COR_LocationRisk = Codes.Blocked;
			AssertEquals(Descriptions.Blocked, pluginBizO.LocationRiskDescription);
		}

		public void TestCommodityRiskDescription()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment);
			pluginBizO.ComplianceRiskStatus.COR_CommodityRisk = Codes.Incomplete;
			AssertEquals(Descriptions.Incomplete, pluginBizO.CommodityRiskDescription);

			pluginBizO.ComplianceRiskStatus.COR_CommodityRisk = Codes.Clear;
			AssertEquals(Descriptions.Clear, pluginBizO.CommodityRiskDescription);

			pluginBizO.ComplianceRiskStatus.COR_CommodityRisk = Codes.PotentialRisk;
			AssertEquals(Descriptions.PotentialRisk, pluginBizO.CommodityRiskDescription);

			pluginBizO.ComplianceRiskStatus.COR_CommodityRisk = Codes.NotApplicable;
			AssertEquals(Descriptions.NotApplicable, pluginBizO.CommodityRiskDescription);

			pluginBizO.ComplianceRiskStatus.COR_CommodityRisk = Codes.Blocked;
			AssertEquals(Descriptions.Blocked, pluginBizO.CommodityRiskDescription);

			pluginBizO.ComplianceRiskStatus.COR_CommodityRisk = Codes.PossibleRisk;
			AssertEquals(Descriptions.PossibleRisk, pluginBizO.CommodityRiskDescription);

			pluginBizO.ComplianceRiskStatus.COR_CommodityRisk = Codes.HighRisk;
			AssertEquals(Descriptions.HighRisk, pluginBizO.CommodityRiskDescription);
		}

		public void TestOverallRiskDescription()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment);

			pluginBizO.ComplianceRiskStatus.COR_OverallRisk = Codes.Clear;
			AssertEquals(Descriptions.Clear, pluginBizO.OverallRiskDescription);

			pluginBizO.ComplianceRiskStatus.COR_OverallRisk = Codes.PotentialRisk;
			AssertEquals(Descriptions.PotentialRisk, pluginBizO.OverallRiskDescription);

			pluginBizO.ComplianceRiskStatus.COR_OverallRisk = Codes.OverrideClear;
			AssertEquals(Descriptions.OverrideClear, pluginBizO.OverallRiskDescription);

			pluginBizO.ComplianceRiskStatus.COR_OverallRisk = Codes.Held;
			AssertEquals(Descriptions.Held, pluginBizO.OverallRiskDescription);

			pluginBizO.ComplianceRiskStatus.COR_OverallRisk = Codes.Blocked;
			AssertEquals(Descriptions.Blocked, pluginBizO.OverallRiskDescription);
		}

		public void TestRefreshData()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_OH_ExportBroker = org1.PK;
			shipment.JS_OH_ImportBroker = org2.PK;

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var plugInBizO = new ComplianceRiskPlugInBusinessObject(shipment);
			plugInBizO.RefreshData();
			CombineAssertions(() =>
			{
				AssertEquals(2, plugInBizO.Parties.Count);
				AssertEquals(true, plugInBizO.Parties.Any(p => ((ComplianceRiskPartyWrapper)p).OrgCode == org1.OH_Code));
				AssertEquals(true, plugInBizO.Parties.Any(p => ((ComplianceRiskPartyWrapper)p).OrgCode == org2.OH_Code));
				AssertEquals(2, plugInBizO.Locations.Count);
				AssertEquals(true, plugInBizO.Locations.Any(l => ((ComplianceRiskLocationWrapper)l).Location == "AU"));
				AssertEquals(true, plugInBizO.Locations.Any(l => ((ComplianceRiskLocationWrapper)l).Location == "NZ"));
			});
		}

		public void TestRefreshDataMergePartiesAndLocations()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_OH_ExportBroker = org1.PK;
			shipment.JS_OH_ImportBroker = org1.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUMEL";

			var plugInBizO = new ComplianceRiskPlugInBusinessObject(shipment);
			plugInBizO.RefreshData();
			CombineAssertions(() =>
			{
				AssertEquals(1, plugInBizO.Parties.Count);
				AssertEquals(org1.OH_Code, plugInBizO.Parties[0].OrgCode);
				AssertEquals(shipment.JS_UniqueConsignRef + ": " + "Export Broker", plugInBizO.Parties[0].ParentsDescription);
				AssertEquals(1, plugInBizO.Locations.Count);
				AssertEquals("AU", plugInBizO.Locations[0].Location);
				AssertEquals(shipment.JS_UniqueConsignRef + ": " + "Origin Country", plugInBizO.Locations[0].Description);
			});
		}

		public void TestRefreshDataDoesNotAddNullScreeningEntityToPartiesOrLocations()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var partyAndLocationAndCommodityBizO = GetNewPartyAndLocationAndCommodityRiskStatusBizO();

			partyAndLocationAndCommodityBizO.AddPartiesForTest(new ScreeningParty(org1, "party", (OrgHeader)null), new ScreeningParty(org2, "party", org2));

			var parties = partyAndLocationAndCommodityBizO.Parties as List<ScreeningParty>;
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(2, parties.Count);
				AssertEquals(string.Empty, parties[0].OrgCode);
				AssertNull(parties[0].ScreeningEntity);
				AssertEquals(org2.OH_Code, parties[1].OrgCode);
				AssertNotNull(parties[1].ScreeningEntity);
			});

			var country1 = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU"));
			var country2 = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "GB"));
			partyAndLocationAndCommodityBizO.AddCountriesForTest(new ScreeningParty(country1, "party", (RefCountry)null), new ScreeningParty(country2, "party", country2));

			var countries = partyAndLocationAndCommodityBizO.Locations as List<ScreeningParty>;
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(2, countries.Count);
				AssertEquals(null, countries[0].Country);
				AssertNull(countries[0].ScreeningEntity);
				AssertEquals(country2, countries[1].Country);
				AssertNotNull(countries[1].ScreeningEntity);
			});

			var plugInBizO = new ComplianceRiskPlugInBusinessObject(partyAndLocationAndCommodityBizO);
			plugInBizO.RefreshData();

			CombineAssertions(() =>
			{
				AssertEquals(1, plugInBizO.Parties.Count);
				AssertEquals(org2.OH_Code, plugInBizO.Parties[0].OrgCode);
				AssertEquals(1, plugInBizO.Locations.Count);
				AssertEquals("GB", plugInBizO.Locations[0].Location);
			});

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestCanNotIncludeCountryInParties()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var country1 = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU"));
			var bizO = GetNewPartyAndLocationAndCommodityRiskStatusBizO();
			bizO.AddPartiesForTest(new ScreeningParty(org1, "party", country1));

			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(1, bizO.Parties.Count());
				AssertNotNull(bizO.Parties.First().Party);
				AssertEquals(country1, (bizO.Parties.First() as ScreeningParty).Country);
				AssertEquals(false, bizO.Locations.Any());
			});

			var plugInBizO = new ComplianceRiskPlugInBusinessObject(bizO);
			AssertEquals(@"When implementing ICompliancePartyRiskStatusProvider, every element of Parties should be instance of following supported types:
Enterprise.MasterFiles.Business.OrgHeader
Enterprise.MasterFiles.Business.IScreeningPartyForVessel
Enterprise.MasterFiles.Business.JobDocAddress
Enterprise.MasterFiles.Business.RefVessel
", ExceptionReporterTestListener.Instance[0].Message);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestRefreshDataDoesNotAddLocationWithoutCountry()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var country1 = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU"));
			var bizO = GetNewPartyAndLocationAndCommodityRiskStatusBizO();
			bizO.AddCountriesForTest(new ScreeningParty(country1, "party", org1));

			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(false, bizO.Parties.Any());
				AssertEquals(1, bizO.Locations.Count());
				var screeningEntity = bizO.Locations.First() as ScreeningParty;
				AssertNotNull(screeningEntity.ScreeningEntity);
				AssertNull(screeningEntity.Country);
			});

			var plugInBizO = new ComplianceRiskPlugInBusinessObject(bizO);
			plugInBizO.RefreshData();

			CombineAssertions(() =>
			{
				AssertEquals(false, plugInBizO.Parties.Any());
				AssertEquals(false, plugInBizO.Locations.Any());
			});

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestShowOverAllRegistryInfoWhenOverrideToClear()
		{
			var bizo = new DummyBizOForFreightMovementRestrictionTest(Factory, "OVR", "CLR", "CLR", "CLR");
			var plugInBizO = new ComplianceRiskPlugInBusinessObject(bizo);

			using (OrganisationsDataRegistry.Instance.ComplianceRiskFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All))
			{
				AssertEquals("Document delivery is not blocked", plugInBizO.OverallRiskRegistryInfo);
			}
		}

		public void TestShowOverAllRegistryInfoWhenNotRestricted()
		{
			var bizo = new DummyBizOForFreightMovementRestrictionTest(Factory, "PSK", "CLR", "PSK", "INC");
			var plugInBizO = new ComplianceRiskPlugInBusinessObject(bizo);

			using (OrganisationsDataRegistry.Instance.ComplianceRiskFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All))
			{
				AssertEquals("Document delivery is not blocked", plugInBizO.OverallRiskRegistryInfo);
			}
		}

		public void TestDoNotShowOverAllRegistryInfoWhenRestrictedAndSomeSinglePartsRiskIncluded()
		{
			var bizo = new DummyBizOForFreightMovementRestrictionTest(Factory, "PSK", "PSK", "PSK", "PSK") { IsDPSFreightMovementRestricted = true };
			var plugInBizO = new ComplianceRiskPlugInBusinessObject(bizo);

			using (OrganisationsDataRegistry.Instance.ComplianceRiskFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All))
			{
				AssertEquals(string.Empty, plugInBizO.OverallRiskRegistryInfo);
			}
		}

		public void TestDoNotShowOverAllRegistryInfoWhenClear()
		{
			var bizo = new DummyBizOForFreightMovementRestrictionTest(Factory, "CLR", "PSK", "PSK", "PSK");
			var plugInBizO = new ComplianceRiskPlugInBusinessObject(bizo);

			using (OrganisationsDataRegistry.Instance.ComplianceRiskFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All))
			{
				AssertEquals(string.Empty, plugInBizO.OverallRiskRegistryInfo);
			}
		}

		public void TestCreateHelperRegisterInteractEventIfNeeded()
		{
			var declarationProvider = Factory.New<ComplianceCommodityDetailCollectionTest.DeclarationWithProvider>();
			var plugInBizO = new ComplianceRiskPlugInBusinessObject(declarationProvider);
			var dummyAssessmentHelper = new DummyAssessmentHelper();
			plugInBizO.CreateHelperRegisterInteractEventIfNeeded(dummyAssessmentHelper);

			var helper = declarationProvider.Helper;
			AssertNotNull(helper);
			AssertNotNull(helper.SourceSideCommodities.InitializeAssessment);
			AssertNotNull(helper.SourceSideCommodities.AssessmentInitialized);
			AssertNotNull(helper.SourceSideCommodities.CommoditiesAssessmentChanged);
			AssertNotNull(helper.SourceSideCommodities.GetCommoditiesStatusFromCpw);
			AssertNotNull(helper.SourceSideCommodities.GetCommodityStatusFromCpw);
			AssertNotNull(helper.SourceSideCommodities.ViewBorderWisePortalIfAvailable);
			AssertNotNull(helper.SourceSideCommodities.CommoditiesChanged);
		}

		public void TestInteractionWithComplianceWiseCommoditiesHelper_GetCommoditiesStatusFromCpw()
		{
			var plugInBizO = GetPlugInBusinessObjectForTest(true);

			var helper = ((ComplianceCommodityDetailCollectionTest.DeclarationWithProvider)plugInBizO.HostBusinessEntity).Helper;
			AssertNotNull(helper);
			AssertNotNull(helper.SourceSideCommodities.GetCommoditiesStatusFromCpw);
			var commodities = helper.SourceSideCommodities.GetCommoditiesStatusFromCpw.Invoke();
			AssertNotNull(commodities);
			AssertEquals(1, commodities.Length);
			AssertEquals("520620", commodities[0].HarmonizedCode);
			AssertEquals("WCO", commodities[0].GroupingOrCountry);
			AssertEquals("NCH", commodities[0].RiskStatus);
			AssertEquals(false, commodities[0].LinkVisible);
			AssertEquals(true, commodities[0].AssessmentInitialized);
			AssertEquals("Clear", commodities[0].ImportAlertStatus);
		}

		public void TestInteractionWithComplianceWiseCommoditiesHelper_CommoditiesChanged()
		{
			var plugInBizO = GetPlugInBusinessObjectForTest(true);
			var commodityRiskStatusChecker = new Mock<ISupportCheckCommodityRiskStatus>();
			plugInBizO.CommodityRiskStatusChecker = commodityRiskStatusChecker.Object;

			var helper = ((ComplianceCommodityDetailCollectionTest.DeclarationWithProvider)plugInBizO.HostBusinessEntity).Helper;
			AssertNotNull(helper);
			AssertNotNull(helper.SourceSideCommodities.CommoditiesChanged);

			var cpwSideCommoditiesChanged = Array.Empty<ComplianceResultFromCpw>();
			helper.CpwSideCommodities.CommoditiesChanged = CommoditiesChanged;

			var commodityChanged = new ComplianceCommodity("112233", "WCO", "Source",
				((ComplianceCommodityDetailCollectionTest.DeclarationWithProvider)plugInBizO.HostBusinessEntity).PK, "",
				"Commercial Invoice", "", "CLR", "Dummy Notes", ZDateTime.UtcNow);

			helper.SourceSideCommodities.CommoditiesChanged.Invoke(new[] {
				commodityChanged
			});

			AssertEquals(1, cpwSideCommoditiesChanged.Length);
			AssertEquals("112233", cpwSideCommoditiesChanged[0].HarmonizedCode);
			AssertEquals("WCO", cpwSideCommoditiesChanged[0].GroupingOrCountry);
			AssertEquals("NCH", cpwSideCommoditiesChanged[0].RiskStatus);
			AssertEquals(false, cpwSideCommoditiesChanged[0].LinkVisible);
			AssertEquals(true, cpwSideCommoditiesChanged[0].AssessmentInitialized);
			AssertEquals(string.Empty, cpwSideCommoditiesChanged[0].ImportAlertStatus);

			commodityRiskStatusChecker.Verify(checker => checker.CheckCommoditiesRiskStatus(It.IsAny<ComplianceCommodityDetail>()), Times.Once);

			void CommoditiesChanged(ComplianceResultFromCpw[] commodities)
			{
				cpwSideCommoditiesChanged = commodities;
			}
		}

		public void TestInteractionWithComplianceWiseCommoditiesHelper_GetCommodityStatusFromCpw()
		{
			var plugInBizO = GetPlugInBusinessObjectForTest(true);

			var helper = ((ComplianceCommodityDetailCollectionTest.DeclarationWithProvider)plugInBizO.HostBusinessEntity).Helper;
			AssertNotNull(helper);
			AssertNotNull(helper.SourceSideCommodities.GetCommodityStatusFromCpw);
			var commodity = new ComplianceCommodityFromSource
			{
				HarmonizedCode = "520620",
				GroupingOrCountry = "WCO",
				GoodsDescription = "",
				OriginOfGoods = ""
			};
			var result = helper.SourceSideCommodities.GetCommodityStatusFromCpw.Invoke(commodity);
			AssertNotNull(result);
			var resultCommodity = result.Value;
			AssertEquals("520620", resultCommodity.HarmonizedCode);
			AssertEquals("WCO", resultCommodity.GroupingOrCountry);
			AssertEquals("NCH", resultCommodity.RiskStatus);
			AssertEquals(false, resultCommodity.LinkVisible);
			AssertEquals(true, resultCommodity.AssessmentInitialized);
			AssertEquals("Clear", resultCommodity.ImportAlertStatus);
		}

		public void TestInteractionWithComplianceWiseCommoditiesHelper_CommoditiesAssessmentChanged()
		{
			var plugInBizO = GetPlugInBusinessObjectForTest(true);

			var newCommodity = new ComplianceCommodity("112233", "WCO", "Source", ((ComplianceCommodityDetailCollectionTest.DeclarationWithProvider)plugInBizO.HostBusinessEntity).PK, "",
				"Commercial Invoice", "", "CLR", "Dummy Notes", ZDateTime.UtcNow);
			plugInBizO.ComplianceRiskStatus.CommodityDetailCollection.LoadWithSuspendChanges(new[] {
				newCommodity
			});

			var helper = ((ComplianceCommodityDetailCollectionTest.DeclarationWithProvider)plugInBizO.HostBusinessEntity).Helper;
			AssertNotNull(helper);
			AssertNotNull(helper.SourceSideCommodities.CommoditiesAssessmentChanged);

			var commodityChanged = new ComplianceCommodityFromSource
			{
				HarmonizedCode = "112233",
				GroupingOrCountry = "WCO",
				GoodsDescription = "",
				OriginOfGoods = "",
				RiskStatus = "BLK",
				RiskNotes = "Test Notes"
			};
			helper.SourceSideCommodities.CommoditiesAssessmentChanged.Invoke(new[] { commodityChanged });

			var commodityInCollection = plugInBizO.ComplianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>()
				.SingleOrDefault(c => c.CCD_HarmonizedCode == "112233");
			AssertNotNull(commodityInCollection);
			AssertEquals("NCH", commodityInCollection.CCD_RiskStatus);
			AssertEquals("Test Notes", commodityInCollection.CCD_AssessmentNotes);
		}

		public void TestInteractionWithComplianceWiseCommoditiesHelper_ViewBorderWisePortalIfAvailable()
		{
			var plugInBizO = GetPlugInBusinessObjectForTest(true);
			var commodityRiskStatusChecker = new Mock<ISupportCheckCommodityRiskStatus>();
			plugInBizO.CommodityRiskStatusChecker = commodityRiskStatusChecker.Object;

			var helper = ((ComplianceCommodityDetailCollectionTest.DeclarationWithProvider)plugInBizO.HostBusinessEntity).Helper;
			AssertNotNull(helper);
			AssertNotNull(helper.SourceSideCommodities.ViewBorderWisePortalIfAvailable);

			var commodityInCollection = plugInBizO.ComplianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>()
				.SingleOrDefault(c => c.CCD_HarmonizedCode == "520620");

			commodityInCollection.BorderWiseCheckStatus = BorderWiseCheckStatus.Viewable;
			AssertEquals(true, commodityInCollection.ShowLegalBookLink);

			var commodity = new ComplianceCommodityFromSource
			{
				HarmonizedCode = "520620",
				GroupingOrCountry = "WCO",
				GoodsDescription = "",
				OriginOfGoods = ""
			};
			helper.SourceSideCommodities.ViewBorderWisePortalIfAvailable.Invoke(commodity);
			commodityRiskStatusChecker.Verify(checker => checker.ViewBorderWisePortal(It.IsAny<ComplianceCommodityDetail>()), Times.Once);
		}

		public void TestInteractionWithComplianceWiseCommoditiesHelper_InitializeAssessment()
		{
			var plugInBizO = GetPlugInBusinessObjectForTest(true);

			var helper = ((ComplianceCommodityDetailCollectionTest.DeclarationWithProvider)plugInBizO.HostBusinessEntity).Helper;
			AssertNotNull(helper);
			AssertNotNull(helper.SourceSideCommodities.InitializeAssessment);

			AssertEquals(0, ((DummyAssessmentHelper)plugInBizO.AssessmentHelper).InitializeAssessmentCount);
			helper.SourceSideCommodities.InitializeAssessment.Invoke();
			AssertEquals(1, ((DummyAssessmentHelper)plugInBizO.AssessmentHelper).InitializeAssessmentCount);
		}

		public void TestInteractionWithComplianceWiseCommoditiesHelper_AssessmentInitialized()
		{
			var plugInBizO = GetPlugInBusinessObjectForTest(true);

			var helper = ((ComplianceCommodityDetailCollectionTest.DeclarationWithProvider)plugInBizO.HostBusinessEntity).Helper;
			AssertNotNull(helper);
			AssertNotNull(helper.SourceSideCommodities.AssessmentInitialized);
			AssertEquals(true, helper.SourceSideCommodities.AssessmentInitialized.Invoke());
		}

		ComplianceRiskPlugInBusinessObject GetPlugInBusinessObjectForTest(bool initialized)
		{
			var declarationProvider = Factory.New<ComplianceCommodityDetailCollectionTest.DeclarationWithProvider>();
			declarationProvider.JS_RL_NKOrigin = "AUSYD";
			declarationProvider.JS_RL_NKDestination = "NZAKL";
			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentTableCode = "JE";
			complianceRiskStatus.COR_ParentID = declarationProvider.PK;
			var plugInBizO = new ComplianceRiskPlugInBusinessObject(declarationProvider);

			var collection = complianceRiskStatus.CommodityDetailCollection;
			var complianceCommodityDetail = collection.AddNew();
			complianceCommodityDetail.CCD_COR_ComplianceRisk = complianceRiskStatus.PK;
			complianceCommodityDetail.CCD_CountryOrGrouping = "WCO";
			complianceCommodityDetail.CCD_HarmonizedCode = "520620";
			complianceCommodityDetail.ImportAlertsForExportJobDescription = "CLR";

			collection.Load();

			var dummyAssessmentHelper = new DummyAssessmentHelper();
			plugInBizO.CreateHelperRegisterInteractEventIfNeeded(dummyAssessmentHelper);

			if (initialized)
			{
				complianceRiskStatus.InitializeAssessmentWorkflow();
			}

			return plugInBizO;
		}

		class DummyAssessmentHelper : IAssessmentHelper
		{
			public int InitializeAssessmentCount { get; set; }
			public Task InitializeAssessment()
			{
				InitializeAssessmentCount++;
				return Task.CompletedTask;
			}
		}

		protected override BusinessObject GetNewBusinessObject() => new ComplianceRiskPlugInBusinessObject(GetNewPartyAndLocationAndCommodityRiskStatusBizO());

		DummyBizObjThatImplementPartyAndLocationAndCommodityProvider GetNewPartyAndLocationAndCommodityRiskStatusBizO() => new(Factory);
	}
}
