using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal.RefDataGrouping;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(ComplianceRiskStatus))]
	public class ComplianceRiskStatusTest : EnterpriseBusinessObjectTestCase
	{
		public void TestUserInputCommoditiesHasRecordAfterAddNew()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Codes.WorldCustomsOrganisationWCO);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Australia, "Australia", parentDataGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Codes.WorldCustomsOrganisationWCO, Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariffView = helper.LoadOrCreateNewTariff(Codes.WorldCustomsOrganisationWCO, tariffType.PK, "123456", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			var commodityCollection = new ComplianceCommodityDetailCollection(complianceRiskStatus, (IComplianceCommodityRiskStatusProvider)shipment);
			var commodity = commodityCollection.AddNew();
			commodity.FillWithValidTestData();
			commodity.CCD_HarmonizedCode = tariffView.ZZ1_TariffCode;
			commodity.CCD_CountryOrGrouping = tariffView.ZZ1_ZZZ_NKDataGrouping;

			Factory.Save();

			AssertEquals(1, commodityCollection.Count);
			AssertEquals(complianceRiskStatus.PK, ((ComplianceCommodityDetail)commodityCollection.First()).CCD_COR_ComplianceRisk);
			AssertEquals(tariffView.ZZ1_TariffCode, ((ComplianceCommodityDetail)commodityCollection.First()).CCD_HarmonizedCode);
			AssertEquals(Codes.WorldCustomsOrganisationWCO, ((ComplianceCommodityDetail)commodityCollection.First()).CCD_CountryOrGrouping);
		}

		public void TestSetDefaultValues()
		{
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.HighRisk, complianceRiskStatus.COR_PartyRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_LocationRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Unknown, complianceRiskStatus.COR_CommodityRisk);
		}

		public void TestJobEndDateValidation()
		{
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_JobEndDate = ZDateTimeOffset.Now.AddYears(-11);
			AssertNoErrors(complianceRiskStatus.COR_JobEndDateInfo);
		}

		public void TestAssessmentStatus_WhenInitialized()
		{
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			Factory.Save();

			CombineAssertions("Before assessment initialize:", () =>
			{
				AssertEquals(false, complianceRiskStatus.IsAssessmentInitialized);
				AssertEquals(false, complianceRiskStatus.IsAssessmentDeclined);
			});

			complianceRiskStatus.InitializeAssessmentWorkflow();
			CombineAssertions("After assessment initialize:", () =>
			{
				AssertEquals(true, complianceRiskStatus.IsAssessmentInitialized);
				AssertEquals(false, complianceRiskStatus.IsAssessmentDeclined);
			});
		}

		public void TestAssessmentStatus_WhenDeclined()
		{
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			Factory.Save();

			CombineAssertions("Before assessment decline:", () =>
			{
				AssertEquals(false, complianceRiskStatus.IsAssessmentInitialized);
				AssertEquals(false, complianceRiskStatus.IsAssessmentDeclined);
			});

			complianceRiskStatus.DeclinedAssessmentWorkflow();
			CombineAssertions("After assessment decline:", () =>
			{
				AssertEquals(false, complianceRiskStatus.IsAssessmentInitialized);
				AssertEquals(true, complianceRiskStatus.IsAssessmentDeclined);
			});

			complianceRiskStatus.InitializeAssessmentWorkflow();
			CombineAssertions("After assessment initialize:", () =>
			{
				AssertEquals(true, complianceRiskStatus.IsAssessmentInitialized);
				AssertEquals(false, complianceRiskStatus.IsAssessmentDeclined);
			});
		}

		public void TestViewBorderWisePortalIfAvailable()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment);
			var commodityRiskStatusChecker = new Mock<ISupportCheckCommodityRiskStatus>();
			pluginBizO.CommodityRiskStatusChecker = commodityRiskStatusChecker.Object;

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.PlugInParent = pluginBizO;

			var commodityDetail = Factory.NewWithValidTestData<ComplianceCommodityDetail>();
			AssertEquals(false, commodityDetail.ShowLegalBookLink);

			complianceRiskStatus.ViewBorderWisePortalIfAvailable(commodityDetail).Wait();
			commodityRiskStatusChecker.Verify(checker => checker.ViewBorderWisePortal(It.IsAny<ComplianceCommodityDetail>()), Times.Never);

			commodityDetail.CCD_HarmonizedCode = "123456";
			commodityDetail.BorderWiseCheckStatus = BorderWiseCheckStatus.Viewable;
			AssertEquals(true, commodityDetail.ShowLegalBookLink);
			complianceRiskStatus.ViewBorderWisePortalIfAvailable(commodityDetail).Wait();
			commodityRiskStatusChecker.Verify(checker => checker.ViewBorderWisePortal(It.IsAny<ComplianceCommodityDetail>()), Times.Once);
		}

		public void TestIsProceedWithoutCommodityRiskAssessmentCheck()
		{
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			var commodityCollection = new ComplianceCommodityDetailCollection(complianceRiskStatus, (IComplianceCommodityRiskStatusProvider)shipment);
			var commodity = commodityCollection.AddNew();
			commodity.CCD_HarmonizedCode = "123456";
			commodity.CCD_RN_NKOrigin = "US";

			var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment);
			complianceRiskStatus.PlugInParent = pluginBizO;

			Factory.Save();

			complianceRiskStatus.SupportedCountriesCheckResponseModel = new SupportedCountriesCheckResponseModel
			{
				CommodityLevel = new CommodityLevelModel
				{
					Export = new[] { "US" },
					Import = new[] { "US" },
					OriginOfGoods = new[] { "US" }
				}
			};

			using (OrganisationsDataRegistry.Instance.AllowComplianceCommodityRiskAssessment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(false, complianceRiskStatus.IsAssessmentInitialized);
				AssertEquals(false, complianceRiskStatus.IsAssessmentDeclined);
				AssertEquals(true, complianceRiskStatus.IsProceedWithoutCommodityRiskAssessmentCheck);

				commodity.CCD_RN_NKOrigin = "AU";
				AssertEquals(false, complianceRiskStatus.IsProceedWithoutCommodityRiskAssessmentCheck);
			}
		}

		public void TestIsProceedWithoutCommodityRiskAssessmentCheck_WithSubRelatedJobCommodity()
		{
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			var commodityCollection = new ComplianceCommodityDetailCollection(complianceRiskStatus, (IComplianceCommodityRiskStatusProvider)shipment);
			var commodity = commodityCollection.AddNew();
			commodity.CCD_HarmonizedCode = "123456";
			commodity.CCD_RN_NKOrigin = "AU";

			var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment);
			complianceRiskStatus.PlugInParent = pluginBizO;

			Factory.Save();

			complianceRiskStatus.SupportedCountriesCheckResponseModel = new SupportedCountriesCheckResponseModel
			{
				CommodityLevel = new CommodityLevelModel
				{
					Export = new[] { "US" },
					Import = new[] { "US" },
					OriginOfGoods = new[] { "US" }
				}
			};

			complianceRiskStatus.UpdateCommodityRiskStatusBySupportedCountriesModelOrDeclined();
			AssertEquals(1, complianceRiskStatus.CommodityDetailCollection.Count);

			var commodityFromRelatedJob = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodityFromRelatedJob.CCD_HarmonizedCode = "654321";
			commodityFromRelatedJob.CCD_RN_NKOrigin = "AU";
			commodityFromRelatedJob.CommodityType = CommodityType.RelatedJobLink;
			commodityFromRelatedJob.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.NotChecked;
			using (OrganisationsDataRegistry.Instance.AllowComplianceCommodityRiskAssessment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(true, complianceRiskStatus.IsProceedWithoutCommodityRiskAssessmentCheck);
			}
		}

		public void TestShouldDoAssessmentByBorderWiseAndCommodityDetailsShouldBeNotChecked()
		{
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			var commodityCollection = new ComplianceCommodityDetailCollection(complianceRiskStatus, (IComplianceCommodityRiskStatusProvider)shipment);
			var commodity = commodityCollection.AddNew();

			commodity.CCD_HarmonizedCode = "123456";
			commodity.CCD_RN_NKOrigin = "AU";

			var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment);
			var commodityRiskStatusChecker = new Mock<ISupportCheckCommodityRiskStatus>();
			complianceRiskStatus.PlugInParent = pluginBizO;

			Factory.Save();

			CombineAssertions("Before assessment initialize:", () =>
			{
				AssertEquals(false, complianceRiskStatus.IsAssessmentInitialized);
				AssertEquals(false, complianceRiskStatus.ShouldDoAssessmentByBorderWise);
				AssertEquals(true, complianceRiskStatus.CommodityDetailsShouldBeNotChecked);
			});

			complianceRiskStatus.SupportedCountriesCheckResponseModel = new SupportedCountriesCheckResponseModel
			{
				CommodityLevel = new CommodityLevelModel
				{
					Export = new[] { "US" },
					Import = new[] { "US" },
					OriginOfGoods = new[] { "US" }
				}
			};
			AssertEquals(false, complianceRiskStatus.CommodityDetailsShouldBeNotChecked);

			complianceRiskStatus.InitializeAssessmentWorkflow();
			CombineAssertions("After assessment initialize:", () =>
			{
				AssertEquals(true, complianceRiskStatus.IsAssessmentInitialized);
				AssertEquals(true, complianceRiskStatus.ShouldDoAssessmentByBorderWise);
				AssertEquals(true, complianceRiskStatus.CommodityDetailsShouldBeNotChecked);
			});
		}

		public void TestGetSupportedCountriesCheckResponseModelFromDb()
		{
			var supportedCountriesResponse = @"
			{
				""commodityLevel"": {
					""import"": [
						""AU""
					],
					""export"": [
						""AU""
					],
					""originOfGoods"": [
						""US""
					]
				},
				""locationLevel"": {
					""transshipment"": [],
					""location"": []
				}
			}";

			var response = JsonConvert.DeserializeObject<SupportedCountriesCheckResponseModel>(supportedCountriesResponse);
			var responseWithFlag = new SupportedCountriesCheckResponseModelWithReportedFlag()
			{
				SupportedCountries = response,
				ErrorReported = false,
				LastModifyUtcTime = DateTime.UtcNow
			};
			var helper = new DummySupportedCountriesHelper();
			helper.DummySetSupportedCountries(responseWithFlag);

			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			AssertNotNull(complianceRiskStatus.SupportedCountriesCheckResponseModel);
		}

		class DummySupportedCountriesHelper : SupportedCountriesHelper
		{
			public DummySupportedCountriesHelper() : base(null)
			{
			}

			public void DummySetSupportedCountries(SupportedCountriesCheckResponseModelWithReportedFlag supportedCountries)
			{
				SetSupportedCountries(supportedCountries);
			}
		}

		public void TestCommodityRiskStatusSetToNotAssessed()
		{
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.NotAssessed;
			AssertNoExceptionThrown(() => Factory.Save());

			AssertEquals(ComplianceRiskStatusCodeList.Codes.NotAssessed, complianceRiskStatus.COR_CommodityRisk);
		}

		public void TestJobComplainceStatusSetClear_WhenProceedWithoutCommodityRiskCheckIsEnabled()
		{
			using (OrganisationsDataRegistry.Instance.AllowComplianceCommodityRiskAssessment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,false))
			{
				var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
				var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
				complianceRiskStatus.COR_ParentID = shipment.PK;
				complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

				complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;
				complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
				complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.NotAssessed;

				Factory.Save();

				complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);
				complianceRiskStatus.PlugInParent.HostBusinessEntity.HasChanges = true;
				ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

				AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);
			}
		}

		public void TestJobComplianceStatusSetClear_WhenProceedConsolWithoutShipmentAttached()
		{
		 var consolBusinessObject = (BusinessObject)Factory.New<IForwardingConsol>();
		 var consol = (ForwardingConsol)consolBusinessObject;
		 consol[JobConsolSchema.JK_RL_NKLoadPort] = "AUBNE";
		 consol[JobConsolSchema.JK_RL_NKDischargePort] = "USLAX";

		 var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
		 complianceRiskStatus.COR_ParentID = consolBusinessObject.PK;
		 complianceRiskStatus.COR_ParentTableCode = consolBusinessObject.TablePrefix;

		 complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;
		 complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
		 complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Unknown;

		 Factory.Save();

		 complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(consolBusinessObject);
		 complianceRiskStatus.PlugInParent.HostBusinessEntity.HasChanges = true;
		 ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

		 AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);
		}

		public void TestJobComplianceStatusSetHeld_WhenProceedShipmentWithoutCommodities()
		{
		 var shipmentBusinessObject = (BusinessObject)Factory.New<IForwardingShipment>();
		 var shipment = (ForwardingShipment)shipmentBusinessObject;
		 shipment[JobShipmentSchema.JS_RL_NKOrigin] = "AUBNE";
		 shipment[JobShipmentSchema.JS_RL_NKDestination] = "USLAX";

		 var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
		 complianceRiskStatus.COR_ParentID = shipmentBusinessObject.PK;
		 complianceRiskStatus.COR_ParentTableCode = shipmentBusinessObject.TablePrefix;

		 complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;
		 complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
		 complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;

		 Factory.Save();

		 complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipmentBusinessObject);
		 complianceRiskStatus.PlugInParent.HostBusinessEntity.HasChanges = true;
		 ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

		 AssertEquals(ComplianceRiskStatusCodeList.Codes.Held, complianceRiskStatus.COR_OverallRisk);
		}

		public void TestCommodityRiskStatusSetNotAssessed_ThenProcessInitiateAssessment()
		{
			using (OrganisationsDataRegistry.Instance.AllowComplianceCommodityRiskAssessment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var consignee = Factory.NewWithValidTestData<OrgHeader>();
				consignee.OH_IsConsignor = true;
				consignee.OH_Code = "Consignee";
				consignee.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.ConsigneePK = consignee.PK;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USORD";

				var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
				complianceRiskStatus.COR_ParentID = shipment.PK;
				complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

				var commodity = complianceRiskStatus.CommodityDetailCollection.AddNew();
				commodity.CCD_HarmonizedCode = "369852";

				complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);
				AssertNull(complianceRiskStatus.PlugInParent.SnapShot);

				ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

				AssertEquals(ComplianceRiskStatusCodeList.Codes.NotAssessed, complianceRiskStatus.COR_CommodityRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);

				StmComplianceEventHelperTest.CreateAssessmentEvent(shipment, ComplianceEventList.Codes.AssessmentInitialized);
				ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

				AssertEquals(ComplianceRiskStatusCodeList.Codes.Unknown, complianceRiskStatus.COR_CommodityRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Held, complianceRiskStatus.COR_OverallRisk);
			}
		}

		#region Add Status Updated Events

		public void TestNewShipmentWithComplianceRiskStatus_ShouldNotCreateEventLogSTU()
		{
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			Factory.Save();

			var eventLog = shipment.GetLogs().GetAllLogs().Cast<StmALog>();
			AssertEquals("New Shipment should not create event log STU", false, eventLog.Any(u => u.SL_SE_NKEvent == AutoEvents.StatusUpdated.Code && u.SL_Reference.Contains("|MST=Compliance Risk")));
		}

		public void TestNewConsolidationWithComplianceRiskStatus_ShouldNotCreateEventLogSTU()
		{
			var consol = (BusinessObject)Factory.New<IForwardingConsol>();
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = consol.PK;
			complianceRiskStatus.COR_ParentTableCode = consol.TablePrefix;

			Factory.Save();

			var eventLog = consol.GetLogs().GetAllLogs().Cast<StmALog>();
			AssertEquals("New Consolidation should not create event log STU", false, eventLog.Any(u => u.SL_SE_NKEvent == AutoEvents.StatusUpdated.Code && u.SL_Reference.Contains("|MST=Compliance Risk")));
		}

		public void TestNewQuotedBookingWithComplianceRiskStatus_ShouldNotCreateEventLogSTU()
		{
			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.QuickBooking, Factory);
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = quotedBooking.ForwardingShipment.PK;
			complianceRiskStatus.COR_ParentTableCode = quotedBooking.ForwardingShipment.TablePrefix;

			Factory.Save();

			var eventLog = quotedBooking.ForwardingShipment.GetLogs().GetAllLogs().Cast<StmALog>();
			AssertEquals("New Quoted Booking should not create event log STU", false, eventLog.Any(u => u.SL_SE_NKEvent == AutoEvents.StatusUpdated.Code && u.SL_Reference.Contains("|MST=Compliance Risk")));
		}

		public void TestShipmentWithComplianceRiskParty_ShouldCreateEventLogSTU()
		{
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Blocked;
			Factory.Save();
			complianceRiskStatus.GetEventLogs().ForEach(e => e.Delete());

			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			Factory.Save();

			CombineAssertions("StmALog:", () =>
			{
				var eventLog = shipment.GetLogs().GetAllLogs().Cast<StmALog>();
				var assessmentLog = eventLog.Where(u => u.SL_SE_NKEvent == AutoEvents.StatusUpdated.Code && u.SL_Reference.Contains("|MST=Compliance Risk"));
				AssertEquals(1, assessmentLog.Count());
				AssertEquals("|MST=Compliance Risk|NEW=Clear|OLD=Blocked|TYP=Party risk status", assessmentLog.Single().SL_Reference);
				AssertEquals("Status Updated from Compliance Risk: Party risk status from Blocked to Clear", assessmentLog.Single().DisplayEventReference);
			});

			CombineAssertions("StmComplianceEvent:", () =>
			{
				var eventLog = complianceRiskStatus.GetEventLogs();
				AssertEquals(1, eventLog.Count());
				AssertEquals("Party Compliance Risk status",
					eventLog.Single(u => u.SCE_EventType == AutoEvents.StatusUpdatedCode && u.SCE_EventSubType == ComplianceEventList.Codes.PartyRisk).SCE_EventReference);
			});
		}

		public void TestShipmentWithComplianceRiskOverall_ShouldCreateEventLogSTU()
		{
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Blocked;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Blocked;
			Factory.Save();

			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.OverrideClear;
			Factory.Save();

			CombineAssertions("StmALog:", () =>
			{
				var eventLog = shipment.GetLogs().GetAllLogs().Cast<StmALog>();
				var assessmentLog = eventLog.Where(u => u.SL_SE_NKEvent == AutoEvents.StatusUpdated.Code && u.SL_Reference.Contains("|MST=Compliance Risk"));
				AssertEquals(1, assessmentLog.Count());
				AssertEquals("|MST=Compliance Risk|NEW=Override Clear|OLD=Blocked|TYP=Job compliance status", assessmentLog.Single().SL_Reference);
				AssertEquals("Status Updated from Compliance Risk: Job compliance status from Blocked to Override Clear", assessmentLog.Single().DisplayEventReference);
			});

			CombineAssertions("StmComplianceEvent:", () =>
			{
				var eventLog = complianceRiskStatus.GetEventLogs().FirstOrDefault(e => e.SCE_NewValue == ComplianceRiskStatusCodeList.Codes.OverrideClear);
				AssertNotNull(eventLog);
				AssertEquals("Job Compliance status", eventLog.SCE_EventReference);
			});
		}

		public void TestShipmentWithComplianceRiskPartyLocationAssessmentCommodity_ShouldCreateEventLogSTU()
		{
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Blocked;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Blocked;
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Blocked;
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Blocked;
			Factory.Save();
			complianceRiskStatus.GetEventLogs().ForEach(e => e.Delete());

			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			Factory.Save();

			CombineAssertions("StmALog:", () =>
			{
				var eventLog = shipment.GetLogs().GetAllLogs().Cast<StmALog>().Where(u => u.SL_SE_NKEvent == AutoEvents.StatusUpdated.Code && u.SL_Reference.Contains("|MST=Compliance Risk"));
				var eventLogsReference = new List<string>()
				{
					"|MST=Compliance Risk|NEW=Clear|OLD=Blocked|TYP=Party risk status",
					"|MST=Compliance Risk|NEW=Clear|OLD=Blocked|TYP=Location risk status",
					"|MST=Compliance Risk|NEW=Clear|OLD=Blocked|TYP=Commodity risk status",
					"|MST=Compliance Risk|NEW=Clear|OLD=Blocked|TYP=Job compliance status"
				};
				var eventLogsDetail = new List<string>()
				{
					"Status Updated from Compliance Risk: Party risk status from Blocked to Clear",
					"Status Updated from Compliance Risk: Location risk status from Blocked to Clear",
					"Status Updated from Compliance Risk: Commodity risk status from Blocked to Clear",
					"Status Updated from Compliance Risk: Job compliance status from Blocked to Clear"
				};

				AssertEquals("Should create 4 STU event logs", 4, eventLog.Count());
				AssertContainsExactElementsInAnyOrder("Should create event logs reference", eventLogsReference, eventLog.Select(u => u.SL_Reference));
				AssertContainsExactElementsInAnyOrder("Should create event logs detail", eventLogsDetail, eventLog.Select(u => u.DisplayEventReference));
			});

			CombineAssertions("StmComplianceEvent:", () =>
			{
				var eventLog = complianceRiskStatus.GetEventLogs();
				var eventLogsReference = new List<string>()
				{
					"Party Compliance Risk status",
					"Location Compliance Risk status",
					"Commodities Compliance Risk status",
					"Job Compliance status",
				};

				AssertEquals("Should create 4 event logs", 4, eventLog.Count());
				AssertContainsExactElementsInAnyOrder("Should create event logs reference", eventLogsReference, eventLog.Select(u => u.SCE_EventReference));
			});
		}

		public void TestSynchronizeFirst_OverallRiskOverrideClear()
		{
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			shipment[JobShipmentSchema.JS_RL_NKOrigin] = "AUBNE";
			shipment[JobShipmentSchema.JS_RL_NKDestination] = "USLAX";

			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Blocked;

			Factory.Save();

			var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment);
			AssertNull("SnapShot is empty", pluginBizO.SnapShot);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);

			complianceRiskStatus.TakeSnapshotAndSetStatusToOverrideClear(pluginBizO, ("OTH", "Dummy", "Dummy Reason"));
			AssertNotNull("SnapShot is updated", pluginBizO.SnapShot);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, complianceRiskStatus.COR_OverallRisk);
		}

		public void TestCreateStatusUpdatedEvent_NotCreateWhenParentIsNull()
		{
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = ZGuid.NewZGuid();
			complianceRiskStatus.COR_ParentTableCode = "JS";

			AssertNoExceptionThrown(() => Factory.Save());
			Assert("No Status Updated events created to compliance risk status", complianceRiskStatus.GetLogs().GetAllLogs().Cast<StmALog>().All(u => u.SL_SE_NKEvent != AutoEvents.StatusUpdated.Code));
		}

		public void TestComplianceRiskStatusInitialization_NotCreateStmALog_ButCreateStmComplianceEventLog()
		{
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentTableCode = "JS";
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Held;
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.HighRisk;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			Factory.Save();

			var eventLogs = complianceRiskStatus.GetEventLogs();
			AssertContainsExactElementsInAnyOrder(new[]
			{
				(ComplianceRiskStatusCodeList.Codes.Held, AutoEvents.StatusUpdatedCode, ComplianceEventList.Codes.OverallRisk),
				(ComplianceRiskStatusCodeList.Codes.HighRisk, AutoEvents.StatusUpdatedCode, ComplianceEventList.Codes.CommodityRisk),
				(ComplianceRiskStatusCodeList.Codes.Clear, AutoEvents.StatusUpdatedCode, ComplianceEventList.Codes.PartyRisk),
				(ComplianceRiskStatusCodeList.Codes.Clear, AutoEvents.StatusUpdatedCode, ComplianceEventList.Codes.LocationRisk),
			}, eventLogs.Select(u => ((string)u.SCE_NewValue, (string)u.SCE_EventType, (string)u.SCE_EventSubType)));

			Assert("No Status Updated events created to compliance risk status", complianceRiskStatus.GetLogs().GetAllLogs().Cast<StmALog>().All(u => u.SL_SE_NKEvent != AutoEvents.StatusUpdated.Code));
		}

		public void TestTakeSnapshotAndSetStatusToOverrideClear()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S123";
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentTableCode = "JS";
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Blocked;
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Blocked;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "ORG 1";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "ORG 2";

			shipment.JS_OH_ExportBroker = org1.PK;
			shipment.JS_OH_ImportBroker = org2.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var plugInBizO = new ComplianceRiskPlugInBusinessObject(shipment);
			complianceRiskStatus.PlugInParent = plugInBizO;

			plugInBizO.RefreshData();

			var complianceCommodityDetail = plugInBizO.ComplianceRiskStatus.CommodityDetailCollection.AddNew();
			complianceCommodityDetail.CCD_COR_ComplianceRisk = plugInBizO.ComplianceRiskStatus.PK;
			complianceCommodityDetail.CCD_CountryOrGrouping = "WCO";
			complianceCommodityDetail.CCD_HarmonizedCode = "123";
			complianceCommodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.NotChecked;
			complianceCommodityDetail.CommoditySource = "Commercial Invoice, Packing";
			complianceCommodityDetail.CCD_AssessmentNotes = "NOTES";
			complianceCommodityDetail.CCD_RN_NKOrigin = "AU";
			complianceCommodityDetail.CCD_Description = "Test";
			complianceCommodityDetail.CCD_SystemCreateTimeUtc = new DateTime(2024, 9, 6, 1, 2, 3);

			var screeningLog_Org1 = Factory.New<IStmEntityScreeningLog>();
			screeningLog_Org1.PJ_ParentID = org1.PK;
			screeningLog_Org1.PJ_ParentTableCode = "OH";
			screeningLog_Org1.PJ_SourceID = shipment.PK;
			screeningLog_Org1.PJ_SourceTableCode = "JS";
			screeningLog_Org1.PJ_Status = DeniedPartyConstants.LogsScreeningStatus.ScreenedCanceled;

			var screeningLog_Org2 = Factory.New<IStmEntityScreeningLog>();
			screeningLog_Org2.PJ_ParentID = org2.PK;
			screeningLog_Org2.PJ_ParentTableCode = "OH";
			screeningLog_Org2.PJ_SourceID = shipment.PK;
			screeningLog_Org2.PJ_SourceTableCode = "JS";
			screeningLog_Org2.PJ_Status = DeniedPartyConstants.LogsScreeningStatus.InvalidatedByLocalDataChanges;

			var screeningLog_CountryAU = Factory.New<IStmEntityScreeningLog>();
			screeningLog_CountryAU.PJ_ParentID = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU").PK;
			screeningLog_CountryAU.PJ_ParentTableCode = "RN";
			screeningLog_CountryAU.PJ_SourceID = shipment.PK;
			screeningLog_CountryAU.PJ_SourceTableCode = "JS";
			screeningLog_CountryAU.PJ_Status = DeniedPartyConstants.LogsScreeningStatus.UserDecisionsRemoveSanctions;

			var screeningLog_CountryNZ = Factory.New<IStmEntityScreeningLog>();
			screeningLog_CountryNZ.PJ_ParentID = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "NZ").PK;
			screeningLog_CountryNZ.PJ_ParentTableCode = "RN";
			screeningLog_CountryNZ.PJ_SourceID = shipment.PK;
			screeningLog_CountryNZ.PJ_SourceTableCode = "JS";
			screeningLog_CountryNZ.PJ_Status = DeniedPartyConstants.LogsScreeningStatus.NoScreeningPerformed;
			Factory.Save();

			complianceRiskStatus.TakeSnapshotAndSetStatusToOverrideClear(plugInBizO, ("NOT", "Not a non-compliant data", "I am authorized to do so"));

			var log = Factory.LoadTop1<StmComplianceEvent>(new ZQuery(StmComplianceEventSchema.SCE_ParentID, shipment.PK).AddToFilter(StmComplianceEventSchema.SCE_NewValue, ComplianceRiskStatusCodeList.Codes.OverrideClear));
			var expectedSnapshot = new ComplianceAuditSnapshot();
			expectedSnapshot.Parties.Add(new Party() { PK = org1.PK.ToGuid(), Code = "ORG 1", Description = "S123: Export Broker", TableCode = "OH", Status = "NOT", LogPK = Guid.Empty });
			expectedSnapshot.Parties.Add(new Party() { PK = org2.PK.ToGuid(), Code = "ORG 2", Description = "S123: Import Broker", TableCode = "OH", Status = "NOT", LogPK = screeningLog_Org2.PK.ToGuid() });
			expectedSnapshot.Countries.Add(new Country() { Code = "AU", Name = "Australia", Description = "S123: Origin Country", IsSanctioned = false, LogPK = screeningLog_CountryAU.PK.ToGuid() });
			expectedSnapshot.Countries.Add(new Country() { Code = "NZ", Name = "New Zealand", Description = "S123: Destination Country", IsSanctioned = false, LogPK = screeningLog_CountryNZ.PK.ToGuid() });
			expectedSnapshot.Commodities.Add(new Commodity() { Code = "123", Conditions = string.Empty, HsCodeDescription = string.Empty, RiskStatus = "NCH", NomenclatureCondition = string.Empty, SpecificCondition = string.Empty, Source = "S123", Notes = "NOTES", CommoditySource = "Commercial Invoice, Packing", OriginOfGoods = "AU", GoodsDescription = "Test", DateAddedUtc = DateTime.SpecifyKind(new DateTime(2024, 9, 6, 1, 2, 3), DateTimeKind.Utc) });
			expectedSnapshot.ComplianceJobDirection.IsInternational = true;
			expectedSnapshot.ComplianceJobDirection.Direction = "Export";
			expectedSnapshot.OverrideDecision.Code = "NOT";
			expectedSnapshot.OverrideDecision.Description = "Not a non-compliant data";
			expectedSnapshot.OverrideDecision.Reason = "I am authorized to do so";
			expectedSnapshot.PartyRisk = "HSK";
			expectedSnapshot.LocationRisk = "CLR";
			expectedSnapshot.CommodityRisk = "UNK";

			AssertEquals(JsonConvert.SerializeObject(expectedSnapshot), log.SCE_Snapshot);
			AssertEquals(complianceRiskStatus.COR_ParentID, log.SCE_ParentID);
			AssertEquals(complianceRiskStatus.COR_ParentTableCode, log.SCE_ParentTableCode);
		}

		public void TestTakeSnapshotSetTimeToUtcNowIfNull()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S123";
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentTableCode = "JS";
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Blocked;
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Blocked;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "ORG 1";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "ORG 2";

			shipment.JS_OH_ExportBroker = org1.PK;
			shipment.JS_OH_ImportBroker = org2.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var plugInBizO = new ComplianceRiskPlugInBusinessObject(shipment);
			complianceRiskStatus.PlugInParent = plugInBizO;

			plugInBizO.RefreshData();

			var complianceCommodityDetail = plugInBizO.ComplianceRiskStatus.CommodityDetailCollection.AddNew();
			complianceCommodityDetail.CCD_COR_ComplianceRisk = plugInBizO.ComplianceRiskStatus.PK;
			complianceCommodityDetail.CCD_CountryOrGrouping = "WCO";
			complianceCommodityDetail.CCD_HarmonizedCode = "123";
			complianceCommodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;
			complianceCommodityDetail.CommoditySource = "Commercial Invoice, Packing";
			complianceCommodityDetail.CCD_AssessmentNotes = "NOTES";
			complianceCommodityDetail.CCD_RN_NKOrigin = "AU";
			complianceCommodityDetail.CCD_Description = "Test";

			complianceRiskStatus.TakeSnapshotAndSetStatusToOverrideClear(plugInBizO, ("NOT", "Not a non-compliant data", "I am authorized to do so"));

			Assert(!complianceCommodityDetail.CCD_SystemCreateTimeUtc.IsEmpty);
		}

		public void TestGetCommodityAggregateRiskStatusWithAssessmentCheckWhenNoCommodities()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.COR_ParentID = shipment.PK;

			CombineAssertions("Commodity Aggregate risk status with assessment check when no commodities", () =>
			{
				AssertEquals("Assessment Not Performed", "UNK", ComplianceRiskStatusCodeList.Codes.Unknown);

				StmComplianceEventHelperTest.CreateAssessmentEvent(shipment, ComplianceEventList.Codes.AssessmentDeclined);

				AssertEquals("Assessment Declined", "PRS", ComplianceRiskStatusCodeList.Codes.PossibleRisk);

				StmComplianceEventHelperTest.CreateAssessmentEvent(shipment, ComplianceEventList.Codes.AssessmentInitialized);

				AssertEquals("Assessment Initialized", "INC", ComplianceRiskStatusCodeList.Codes.Incomplete);
			});
		}

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			return complianceRiskStatus;
		}

		public override void TestFetchForLoad()
		{
			var complianceRiskStatus = GetNewBusinessObject();
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertNotNull(newFactory.Load<ComplianceRiskStatus>(complianceRiskStatus.PK));
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			var complianceRiskStatus = GetNewBusinessObject();
			Factory.Save();

			complianceRiskStatus.Delete();
			Factory.Save();

			AssertNull(Factory.Load<ComplianceRiskStatus>(complianceRiskStatus.PK));
		}

		public override void TestCalcPropertiesWithDbHitsUseFetchHints()
		{
			var complianceRiskStatus = GetNewBusinessObject();
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertNotNull(newFactory.Load<ComplianceRiskStatus>(complianceRiskStatus.PK));
			AssertEquals("Fetch hints should be used", 0, newFactory.ActiveTableFetchHints);
		}

		public void TestTwoUsersSaveDuplicateComplianceRiskStatus_UniqueIndexFailureHandler()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var complianceRiskStatusExists = newFactory.New<ComplianceRiskStatus>();
			complianceRiskStatusExists.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatusExists.COR_ParentID = shipment.PK;
			newFactory.Save();

			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			var notifier = new MockNotificationHandler();
			ZExceptionReporting.ProcessWithSaveExceptionHandling(() => Factory.Save(), () => { }, notifier: notifier);
			AssertEquals($"While you have been working with this form, another user has made changes.\r\n\r\nThe system will now try to combine your changes with those of the other user.\r\nAfter you click 'OK', the form will merge your changes with changes made by other user.\r\n\r\nHowever, the fields will have warning messages explaining the other user's changes.\r\nPlease review the form carefully before clicking the 'Save' button again.\r\n\r\nThe following objects have changes and will be merged:\r\nCompliance Risk Status", notifier.LastMessage);
			AssertEquals("Duplicate Compliance Risk", notifier.LastCaption);
			AssertNoExceptionThrown(Factory.Save);

			var complianceRiskStatusInCurrentFactory = Factory.Load<ComplianceRiskStatus>(complianceRiskStatusExists.PK);
			AssertEquals(complianceRiskStatus.PlugInParent, complianceRiskStatusInCurrentFactory.PlugInParent);
			AssertEquals(true, complianceRiskStatus.IsDeleted);
		}

		class MockNotificationHandler : INotificationHandler
		{
			public string LastMessage;
			public string LastCaption;

			public void ReportError(string message, string caption, string errorContext = null, Exception exception = null)
			{
				throw new NotSupportedException();
			}

			public void ReportInformation(string message, string caption)
			{
				LastMessage = message;
				LastCaption = caption;
			}
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
	}
}
