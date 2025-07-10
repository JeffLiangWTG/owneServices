using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.ComplianceRisk.Business.Test.ComplianceCommodityDetailCollectionTest;
using static Enterprise.ComplianceRisk.Integration.ComplianceRiskStatusCodeList;

namespace Enterprise.ComplianceRisk.Business.Test
{
	public class CommodityRiskStatusBorderWiseCheckerTest : TestCaseWithFactory
	{
		public void TestCheckAllCommodityRiskStatus_InternationalJob_UseCache()
		{
			var pointPairs = GetPointPairs();
			var shipmentProvider = Factory.New<ShipmentWithProvider>();
			shipmentProvider.JS_RL_NKOrigin = "AUSYD";
			shipmentProvider.JS_RL_NKDestination = "NZAKL";
			shipmentProvider.AssessmentPointPairInfo = new ComplianceAssessmentPointPairInfo(pointPairs);

			var plugIn = new ComplianceRiskPlugInBusinessObject(shipmentProvider);
			var checker = new CommodityRiskStatusBorderWiseChecker(plugIn, CancellationToken.None);
			var collection = plugIn.ComplianceRiskStatus.CommodityDetailCollection;
			var commodity1 = collection.AddNew();
			commodity1.CCD_HarmonizedCode = "123456";

			var commodity2 = collection.AddNew();
			commodity2.CCD_HarmonizedCode = "654321";

			var commodity3 = collection.AddNew();
			commodity3.CCD_HarmonizedCode = "111222";
			commodity3.CCD_RiskStatus = Codes.Released;

			AssertEquals(Codes.NotChecked, commodity1.CCD_RiskStatus);
			AssertEquals(Codes.NotChecked, commodity2.CCD_RiskStatus);
			AssertEquals(Codes.Released, commodity3.CCD_RiskStatus);

			using (BorderWiseApiHelper.SetResponse(GetResponseForMultipleHsCodes(new[] { ("123456", false, false), ("654321", true, false), ("111222", true, false) })))
			{
				checker.CheckAllCommoditiesRiskStatus().Wait();
				AssertEquals("Change status to CLR when no conditions apply", Codes.Clear, commodity1.CCD_RiskStatus);
				AssertEquals("Change status to PSK when conditions apply", Codes.HighRisk, commodity2.CCD_RiskStatus);
				AssertEquals("Status stay REL", Codes.Released, commodity3.CCD_RiskStatus);
			}

			using (BorderWiseApiHelper.SetResponse(GetResponseForMultipleHsCodes(new[] { ("123456", false, false), ("111222", false, false) })))
			{
				commodity2.CCD_RiskStatus = Codes.NotChecked;
				checker.CheckAllCommoditiesRiskStatus().Wait();
				AssertEquals("Change status to PSK when request not change, use cached response", Codes.HighRisk, commodity2.CCD_RiskStatus);

				commodity2.CCD_HarmonizedCode = "111222";
				checker.CheckAllCommoditiesRiskStatus().Wait();
				AssertEquals("Not use cached response when request changed", Codes.Clear, commodity2.CCD_RiskStatus);
			}
		}

		public void TestCheckAllCommodityRiskStatus_NotifyChangesFromCpwSide()
		{
			var pointPairs = GetPointPairs();
			var declarationProvider = Factory.New<DeclarationWithProvider>();
			declarationProvider.JS_RL_NKOrigin = "AUSYD";
			declarationProvider.JS_RL_NKDestination = "NZAKL";
			declarationProvider.AssessmentPointPairInfo = new ComplianceAssessmentPointPairInfo(pointPairs);

			var plugIn = new ComplianceRiskPlugInBusinessObject(declarationProvider);
			var checker = new CommodityRiskStatusBorderWiseChecker(plugIn, CancellationToken.None);
			var collection = plugIn.ComplianceRiskStatus.CommodityDetailCollection;
			var commodity1 = collection.AddNew();
			commodity1.CCD_HarmonizedCode = "123456";

			var newCommodity = new ComplianceCommodity("112233", "WCO", "Source", declarationProvider.PK, "",
				"Commercial Invoice", "", "CLR", "Dummy Notes", ZDateTime.UtcNow);
			collection.LoadWithSuspendChanges(new[] {
				newCommodity
			});
			var commodity2 = collection.Cast<ComplianceCommodityDetail>().Single(u => u.CCD_HarmonizedCode == "112233");

			AssertEquals(Codes.NotChecked, commodity1.CCD_RiskStatus);
			AssertEquals(Codes.NotChecked, commodity2.CCD_RiskStatus);

			using (BorderWiseApiHelper.SetResponse(GetResponseForMultipleHsCodes(new[] { ("123456", false, false), ("112233", false, false) })))
			{
				var helper = (DummyInteractionWithComplianceWiseCommoditiesHelper)declarationProvider.Helper;
				AssertEquals(0, helper.CpwSideCommoditiesChanged.Length);

				checker.CheckAllCommoditiesRiskStatus().Wait();
				AssertEquals(Codes.Clear, commodity1.CCD_RiskStatus);
				AssertEquals(Codes.Clear, commodity2.CCD_RiskStatus);

				AssertEquals(1, helper.CpwSideCommoditiesChanged.Length);
				var commodityChanged = helper.CpwSideCommoditiesChanged[0];
				AssertEquals("112233", commodityChanged.HarmonizedCode);
				AssertEquals(Codes.Clear, commodityChanged.RiskStatus);
			}
		}

		public void TestGetComplianceCheckRequestModel_AuthenticationSection()
		{
			var pointPairs = GetPointPairs();
			var shipmentProvider = Factory.New<ShipmentWithProvider>();
			shipmentProvider.JS_RL_NKOrigin = "AUSYD";
			shipmentProvider.JS_RL_NKDestination = "NZAKL";
			shipmentProvider.AssessmentPointPairInfo = new ComplianceAssessmentPointPairInfo(pointPairs);

			var plugIn = new ComplianceRiskPlugInBusinessObject(shipmentProvider);
			var checker = new CommodityRiskStatusBorderWiseCheckerForTest(plugIn, CancellationToken.None, true);
			var collection = plugIn.ComplianceRiskStatus.CommodityDetailCollection;
			var commodity = collection.AddNew();
			commodity.CCD_HarmonizedCode = "123456";

			var commodities = new[] { commodity };
			var request = ComplianceCheckRequestModelBuilder.GetRequestModel(shipmentProvider, pointPairs, commodities);
			AssertNotNull(request.Authentication);
			var loggedInOrgProxy = shipmentProvider.Factory.Load<OrgHeader>(Env.CurrentCompany.OrganisationPK);
			AssertEquals(loggedInOrgProxy.OH_Code, request.Authentication.OrgCode);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, request.Authentication.StaffCode);
			AssertEquals(ObjectFactory.Get<IProductRegistration>().Key.DatabaseNumber.ToString(CultureInfo.InvariantCulture), request.Authentication.DataBaseNumber);
		}

		public void TestCheckAllCommodityRiskStatus_InternationalJob_ResponseIsValidToProcess()
		{
			var pointPairs = GetPointPairs();
			var shipmentProvider = Factory.New<ShipmentWithProvider>();
			shipmentProvider.JS_RL_NKOrigin = "AUSYD";
			shipmentProvider.JS_RL_NKDestination = "NZAKL";
			shipmentProvider.AssessmentPointPairInfo = new ComplianceAssessmentPointPairInfo(pointPairs);

			var plugIn = new ComplianceRiskPlugInBusinessObject(shipmentProvider);
			var checker = new CommodityRiskStatusBorderWiseCheckerForTest(plugIn, CancellationToken.None, true);
			var collection = plugIn.ComplianceRiskStatus.CommodityDetailCollection;
			var commodity = collection.AddNew();
			commodity.CCD_HarmonizedCode = "123456";

			using (BorderWiseApiHelper.SetResponse(GetResponseForMultipleHsCodes(new[] { ("123456", false, false) })))
			{
				checker.CheckAllCommoditiesRiskStatus().Wait();
				AssertNotEquals("Not change status to CLR when response not valid", Codes.Clear, commodity.CCD_RiskStatus);
			}

			var commodities = new[] { commodity };
			var request = ComplianceCheckRequestModelBuilder.GetRequestModel(shipmentProvider, pointPairs, commodities);
			AssertEquals(true, checker.ResponseIsValidToProcessExposed(true, null, null));
			AssertEquals(true, checker.ResponseIsValidToProcessExposed(false, request, commodities));

			commodity.CCD_HarmonizedCode = "654321";
			AssertEquals(false, checker.ResponseIsValidToProcessExposed(false, request, commodities));
		}

		public void TestCheckAllCommodityRiskStatus_OnlyCheckApplicableCommodities()
		{
			var pointPairs = GetPointPairs();
			var shipmentProvider = Factory.New<ShipmentWithProvider>();
			shipmentProvider.JS_RL_NKOrigin = "AUSYD";
			shipmentProvider.JS_RL_NKDestination = "NZAKL";
			shipmentProvider.AssessmentPointPairInfo = new ComplianceAssessmentPointPairInfo(pointPairs);
			shipmentProvider.ExposedCommodities = new[] { GetComplianceCommodity("123456", "WCO", "Source2", shipmentProvider.PK), GetComplianceCommodity("654321", "WCO", "Source3", Guid.NewGuid()) };

			var plugIn = new ComplianceRiskPlugInBusinessObject(shipmentProvider);
			var checker = new CommodityRiskStatusBorderWiseChecker(plugIn, CancellationToken.None);

			using (BorderWiseApiHelper.SetResponse(GetResponseForMultipleHsCodes(new[] { ("123456", false, false), ("654321", false, false), (string.Empty, false, false) })))
			{
				plugIn.ComplianceRiskStatus.CommodityDetailCollection.Load();

				var emptyHsCodeCommodity = plugIn.ComplianceRiskStatus.CommodityDetailCollection.AddNew();
				var commodityFromFetchData = plugIn.ComplianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>().Single(u => u.CCD_HarmonizedCode == "123456");
				var commodityFromRelatedJob = plugIn.ComplianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>().Single(u => u.CCD_HarmonizedCode == "654321");

				checker.CheckAllCommoditiesRiskStatus().Wait();
				AssertNotEquals("Status not change when harmonized code is empty", Codes.Clear, commodityFromRelatedJob.CCD_RiskStatus);
				AssertEquals("Change status to CLR when no conditions apply", Codes.Clear, commodityFromFetchData.CCD_RiskStatus);
				AssertNotEquals("Status not change when commodity coming from related job", Codes.Clear, commodityFromRelatedJob.CCD_RiskStatus);
			}
		}

		public void TestCheckAllCommodityRiskStatus_NotSupportAssessmentByBorderWise()
		{
			var shipmentProvider = Factory.New<ShipmentWithProvider>();
			shipmentProvider.JS_RL_NKOrigin = "AUSYD";
			shipmentProvider.JS_RL_NKDestination = "NZAKL";
			shipmentProvider.AssessmentPointPairInfo = new ComplianceAssessmentPointPairInfo();

			var plugIn = new ComplianceRiskPlugInBusinessObject(shipmentProvider);

			AssertEquals(false, shipmentProvider.AssessmentPointPairInfo.SupportAssessmentByBorderWise);

			var checker = new CommodityRiskStatusBorderWiseChecker(plugIn, CancellationToken.None);
			var collection = plugIn.ComplianceRiskStatus.CommodityDetailCollection;
			var commodity = collection.AddNew();
			commodity.CCD_HarmonizedCode = "123456";

			AssertEquals(Codes.NotChecked, commodity.CCD_RiskStatus);

			using (BorderWiseApiHelper.SetResponse(GetResponse("123456", false, false, Factory)))
			{
				checker.CheckAllCommoditiesRiskStatus().Wait();
				AssertEquals("Not check status when job not support Assessment By BorderWise", Codes.NotChecked, commodity.CCD_RiskStatus);
			}
		}

		public void TestCheckAllCommodityRiskStatus_IgnoreInvalidRequest()
		{
			var invalidPointPairs = new List<ComplianceCheckRequestPointPair>() { new ComplianceCheckRequestPointPair() };
			var shipmentProvider = Factory.New<ShipmentWithProvider>();
			shipmentProvider.JS_RL_NKOrigin = "AUSYD";
			shipmentProvider.JS_RL_NKDestination = "NZAKL";
			shipmentProvider.AssessmentPointPairInfo = new ComplianceAssessmentPointPairInfo(invalidPointPairs);

			var plugIn = new ComplianceRiskPlugInBusinessObject(shipmentProvider);

			AssertEquals(true, shipmentProvider.AssessmentPointPairInfo.SupportAssessmentByBorderWise);

			var checker = new CommodityRiskStatusBorderWiseChecker(plugIn, CancellationToken.None);
			var collection = plugIn.ComplianceRiskStatus.CommodityDetailCollection;
			var commodity = collection.AddNew();
			commodity.CCD_HarmonizedCode = "123456";
			AssertEquals(Codes.NotChecked, commodity.CCD_RiskStatus);

			using (BorderWiseApiHelper.SetResponse(GetResponse("123456", false, false, Factory)))
			{
				checker.CheckAllCommoditiesRiskStatus().Wait();
				AssertEquals("Not check status when request is not valid", Codes.NotChecked, commodity.CCD_RiskStatus);
			}
		}

		public void TestCheckCommodityRiskStatus_InternationalJob_IgnoreCache()
		{
			var pointPairs = GetPointPairs();
			var shipmentProvider = Factory.New<ShipmentWithProvider>();
			shipmentProvider.JS_RL_NKOrigin = "AUSYD";
			shipmentProvider.JS_RL_NKDestination = "NZAKL";
			shipmentProvider.AssessmentPointPairInfo = new ComplianceAssessmentPointPairInfo(pointPairs);

			var plugIn = new ComplianceRiskPlugInBusinessObject(shipmentProvider);
			var checker = new CommodityRiskStatusBorderWiseChecker(plugIn, CancellationToken.None);
			var collection = plugIn.ComplianceRiskStatus.CommodityDetailCollection;
			var commodity = collection.AddNew();
			commodity.CCD_HarmonizedCode = "123456";
			using (BorderWiseApiHelper.SetResponse(null))
			{
				checker.CheckCommoditiesRiskStatus(commodity).Wait();
				AssertEquals("Not change status when response is null", Codes.NotChecked, commodity.CCD_RiskStatus);
			}

			using (BorderWiseApiHelper.SetResponse(GetResponse("123456", false, true, Factory)))
			{
				checker.CheckCommoditiesRiskStatus(commodity).Wait();
				AssertEquals("Change status to CLR when no Commodity Specific Conditions apply", Codes.PossibleRisk, commodity.CCD_RiskStatus);
			}

			using (BorderWiseApiHelper.SetResponse(GetResponse("123456", true, true, Factory)))
			{
				checker.CheckCommoditiesRiskStatus(commodity).Wait();
				AssertEquals("Change status to PSK when Commodity Specific Conditions apply", Codes.HighRisk, commodity.CCD_RiskStatus);

				commodity.CCD_RiskStatus = Codes.Blocked;
				checker.CheckCommoditiesRiskStatus(commodity).Wait();
				AssertEquals("Not change the status when current status is BLK or REL", Codes.Blocked, commodity.CCD_RiskStatus);

				commodity.CCD_RiskStatus = Codes.Released;
				checker.CheckCommoditiesRiskStatus(commodity).Wait();
				AssertEquals("Not change the status when current status is BLK or RE", Codes.Released, commodity.CCD_RiskStatus);
			}
		}

		public void TestCheckCommodityRiskStatus_NotInternationalJob()
		{
			var pointPairs = GetPointPairs();
			var shipmentProvider = Factory.New<ShipmentWithProvider>();
			shipmentProvider.JS_RL_NKOrigin = "AUSYD";
			shipmentProvider.JS_RL_NKDestination = "AUSYD";
			shipmentProvider.AssessmentPointPairInfo = new ComplianceAssessmentPointPairInfo(pointPairs);

			var plugIn = new ComplianceRiskPlugInBusinessObject(shipmentProvider);
			var checker = new CommodityRiskStatusBorderWiseChecker(plugIn, CancellationToken.None);
			var collection = plugIn.ComplianceRiskStatus.CommodityDetailCollection;
			var commodity = collection.AddNew();
			commodity.CCD_HarmonizedCode = "123456";
			checker.CheckCommoditiesRiskStatus(commodity).Wait();
			AssertEquals("Not change status when response is null", Codes.NotChecked, commodity.CCD_RiskStatus);

			using (BorderWiseApiHelper.SetResponse(GetResponse("123456", false, false, Factory)))
			{
				checker.CheckCommoditiesRiskStatus(commodity).Wait();
				AssertEquals("Not change status when job is not international job", Codes.NotChecked, commodity.CCD_RiskStatus);
			}
		}

		public void TestGetRequestId_InternationalJob_UseCache()
		{
			var pointPairs = GetPointPairs();
			var shipmentProvider = Factory.New<ShipmentWithProvider>();
			shipmentProvider.JS_RL_NKOrigin = "AUSYD";
			shipmentProvider.JS_RL_NKDestination = "NZAKL";
			shipmentProvider.AssessmentPointPairInfo = new ComplianceAssessmentPointPairInfo(pointPairs);

			var plugIn = new ComplianceRiskPlugInBusinessObject(shipmentProvider);
			var checker = new CommodityRiskStatusBorderWiseChecker(plugIn, CancellationToken.None);
			var collection = plugIn.ComplianceRiskStatus.CommodityDetailCollection;
			var commodity = collection.AddNew();
			commodity.CCD_HarmonizedCode = "123456";

			var response = GetResponse("123456", false, false, Factory);
			using (BorderWiseApiHelper.SetResponse(response))
			{
				var requstId = checker.GetRequestId().Result;
				AssertEquals("Return request id when job is not international", response.RequestId, requstId.Value);
				AssertEquals("Change status to CLR when no conditions apply", Codes.Clear, commodity.CCD_RiskStatus);
			}

			using (BorderWiseApiHelper.SetResponse(GetResponse("123456", false, true, Factory)))
			{
				var requstId = checker.GetRequestId().Result;
				AssertEquals("Return cached response request Id", response.RequestId, requstId.Value);
				AssertEquals("Not change status when request not change", Codes.Clear, commodity.CCD_RiskStatus);
			}
		}

		public void TestCheckCommodityRiskStatusCore_WithoutComplianceCommodityProvider()
		{
			var pointPairs = GetPointPairs();
			var shipmentProvider = Factory.New<ShipmentWithoutCommodityProvider>();
			shipmentProvider.JS_RL_NKOrigin = "AUSYD";
			shipmentProvider.JS_RL_NKDestination = "NZAKL";
			shipmentProvider.AssessmentPointPairInfo = new ComplianceAssessmentPointPairInfo(pointPairs);

			var plugIn = new ComplianceRiskPlugInBusinessObject(shipmentProvider);
			var checker = new CommodityRiskStatusBorderWiseChecker(plugIn, CancellationToken.None);
			var collection = plugIn.ComplianceRiskStatus.CommodityDetailCollection;
			var commodity = collection.AddNew();
			commodity.CCD_HarmonizedCode = "123456";

			var requestId = checker.GetRequestId().Result;
			AssertEquals(false, requestId.HasValue);
		}

		public void TestResponseIsValidToProcess_WithoutComplianceCommodityProvider()
		{
			var pointPairs = GetPointPairs();
			var shipmentProvider = Factory.New<ShipmentWithoutCommodityProvider>();
			shipmentProvider.JS_RL_NKOrigin = "AUSYD";
			shipmentProvider.JS_RL_NKDestination = "NZAKL";
			shipmentProvider.AssessmentPointPairInfo = new ComplianceAssessmentPointPairInfo(pointPairs);

			var plugIn = new ComplianceRiskPlugInBusinessObject(shipmentProvider);
			var checker = new CommodityRiskStatusBorderWiseCheckerForTest(plugIn, CancellationToken.None, true);
			var collection = plugIn.ComplianceRiskStatus.CommodityDetailCollection;
			var commodity = collection.AddNew();
			commodity.CCD_HarmonizedCode = "123456";

			using (BorderWiseApiHelper.SetResponse(GetResponseForMultipleHsCodes(new[] { ("123456", false, false) })))
			{
				checker.CheckAllCommoditiesRiskStatus().Wait();
				AssertNotEquals("Not change status to CLR when response not valid", Codes.Clear, commodity.CCD_RiskStatus);
			}

			var commodities = new[] { commodity };
			var request = ComplianceCheckRequestModelBuilder.GetRequestModel(shipmentProvider, plugIn.ComplianceCommodityRiskStatusProvider?.AssessmentPointPairInfo.PointPairs, commodities);
			AssertEquals(true, checker.ResponseIsValidToProcessExposed(false, request, commodities));
		}

		public void TestGetRequestId_NotInternationalJob()
		{
			var pointPairs = GetPointPairs();
			var shipmentProvider = Factory.New<ShipmentWithProvider>();
			shipmentProvider.JS_RL_NKOrigin = "AUSYD";
			shipmentProvider.JS_RL_NKDestination = "AUSYD";
			shipmentProvider.AssessmentPointPairInfo = new ComplianceAssessmentPointPairInfo(pointPairs);

			var plugIn = new ComplianceRiskPlugInBusinessObject(shipmentProvider);

			var checker = new CommodityRiskStatusBorderWiseChecker(plugIn, CancellationToken.None);
			var collection = plugIn.ComplianceRiskStatus.CommodityDetailCollection;
			var commodity = collection.AddNew();
			commodity.CCD_HarmonizedCode = "123456";

			using (BorderWiseApiHelper.SetResponse(GetResponse("123456", false, false, Factory)))
			{
				var requestId = checker.GetRequestId().Result;
				AssertNull("Return null when job is not international", requestId);
			}
		}

		public void TestTriggerValidationDependOnFormLoadDuringAndAfterBorderWiseStatusCheck()
		{
			var pointPairs = GetPointPairs();
			var shipmentProvider = Factory.New<ShipmentWithProvider>();
			shipmentProvider.JS_RL_NKOrigin = "AUSYD";
			shipmentProvider.JS_RL_NKDestination = "NZAKL";
			shipmentProvider.AssessmentPointPairInfo = new ComplianceAssessmentPointPairInfo(pointPairs);

			var plugIn = new ComplianceRiskPlugInBusinessObject(shipmentProvider);
			var eventLog = Factory.NewWithValidTestData<StmComplianceEvent>();
			eventLog.SCE_EventType = AutoEvents.ComplianceRiskInteractionCode;
			eventLog.SCE_EventSubType = ComplianceEventList.Codes.AssessmentInitialized;
			eventLog.SCE_ParentID = shipmentProvider.PK;

			var checker = new CommodityRiskStatusBorderWiseChecker(plugIn, CancellationToken.None);
			var collection = plugIn.ComplianceRiskStatus.CommodityDetailCollection;
			var commodity1 = collection.AddNew();
			commodity1.CCD_HarmonizedCode = "123456";

			var commodity2 = collection.AddNew();
			commodity2.CCD_HarmonizedCode = "654321";

			var commodity3 = collection.AddNew();
			commodity3.CCD_HarmonizedCode = "111222";

			var commodity4 = collection.AddNew();
			commodity4.CCD_HarmonizedCode = "123321";

			var commodity5 = collection.AddNew();
			commodity5.CCD_HarmonizedCode = "222111";

			var point = new ComplianceCheckResponsePointPairLocationModel
			{
				NomenclatureWideConditionsApply = BorderWiseApiHelper.ConditionApply,
				CommoditySpecificConditionsApply = BorderWiseApiHelper.ConditionApply,
			};

			var response = new ComplianceCheckResponseModel
			{
				RequestId = Guid.NewGuid(),
				Commodities = new[]
				{
					new ComplianceCheckResponseCommodityModel
					{
						HsCode = "123456",
						CommoditySpecificConditionsApply = BorderWiseApiHelper.ConditionApply,
						NomenclatureWideConditionsApply = BorderWiseApiHelper.ConditionNotApply,
						PointPairs = new[]
						{
							new ComplianceCheckResponsePointPairModel
							{
								OriginPoint = point,
								DestinationPoint = point
							}
						}
					},
					new ComplianceCheckResponseCommodityModel
					{
						HsCode = "654321",
						CommoditySpecificConditionsApply = BorderWiseApiHelper.ConditionUncertain,
						NomenclatureWideConditionsApply = BorderWiseApiHelper.ConditionUncertain,
						PointPairs = new[]
						{
							new ComplianceCheckResponsePointPairModel
							{
								OriginPoint = point,
								DestinationPoint = point
							}
						}
					},
					new ComplianceCheckResponseCommodityModel
					{
						HsCode = "123321",
						CommoditySpecificConditionsApply = BorderWiseApiHelper.ConditionApply,
						NomenclatureWideConditionsApply = BorderWiseApiHelper.ConditionNotApply,
						PointPairs = new[]
						{
							new ComplianceCheckResponsePointPairModel
							{
								OriginPoint = point,
								DestinationPoint = point
							}
						}
					},
					new ComplianceCheckResponseCommodityModel
					{
						HsCode = "222111",
						CommoditySpecificConditionsApply = BorderWiseApiHelper.ConditionNotApply,
						NomenclatureWideConditionsApply = BorderWiseApiHelper.ConditionApply,
						PointPairs = new[]
						{
							new ComplianceCheckResponsePointPairModel
							{
								OriginPoint = point,
								DestinationPoint = point
							}
						}
					}
				}
			};

			using (BorderWiseApiHelper.SetResponse(response))
			{
				checker.CheckAllCommoditiesRiskStatus(false).Wait();

				CombineAssertions(() =>
				{
					AssertNoRowWarnings(commodity1);
					AssertNoRowWarnings(commodity2);
					AssertNoRowWarnings(commodity3);
					AssertNoRowWarnings(commodity4);
					AssertNoRowWarnings(commodity5);
				});

				checker.CheckAllCommoditiesRiskStatus().Wait();

				CombineAssertions(() =>
				{
					AssertHasRowWarning(commodity1, ComplianceCommodityDetailValidationReal.GetMessages.UnsupportedHarmonizedMessage);
					AssertHasRowWarningContaining("All Conditions Are Uncertain", commodity2, ComplianceCommodityDetailValidationReal.GetMessages.UnsupportedHarmonizedMessage);
					AssertHasRowWarningContaining("Mock BorderWise not finished yet", commodity3, ComplianceCommodityDetailValidationReal.GetMessages.RiskCheckInProgressMessage);
					AssertHasRowWarningContaining("One or more Commodity Specific Conditions has been found for commodity", commodity4, ComplianceCommodityDetailValidationReal.GetMessages.ReviewComplianceAlertMessage);
					AssertNoRowWarningContaining("No Commodity Specific Conditions has been found for commodity", commodity5, ComplianceCommodityDetailValidationReal.GetMessages.ReviewComplianceAlertMessage);
				});
			}
		}

		public void TestGetComplianceCheckRequest()
		{
			var pointPairs = new List<ComplianceCheckRequestPointPair> {
				new ()
				{
					OriginPoint = new ComplianceCheckRequestPointPairLocation
					{
						Country = "AU",
						UNLOCO = "AUSYD",
						MovementDescription = "Origin"
					},
					DestinationPoint = new ComplianceCheckRequestPointPairLocation
					{
						Country = "US",
						UNLOCO = "USORD",
						MovementDescription = "Destination"
					},
					EstimatedTimeOfArrival = new ZDateTime(2024, 3, 1),
					EstimatedTimeOfDeparture = new ZDateTime(2024, 2, 1),
					Mode = "SEA"
				},
				new ()
				{
					OriginPoint = new ComplianceCheckRequestPointPairLocation
					{
						Country = "AU",
						UNLOCO = "AUSYD",
						MovementDescription = "Origin"
					},
					DestinationPoint = new ComplianceCheckRequestPointPairLocation
					{
						Country = null,
						UNLOCO = null,
						MovementDescription = "Destination"
					},
					EstimatedTimeOfArrival = ZDateTime.Invalid,
					EstimatedTimeOfDeparture = new ZDateTime(2024, 2, 1),
					Mode = "SEA"
				},
				new ()
				{
					OriginPoint = null,
					DestinationPoint = new ComplianceCheckRequestPointPairLocation
					{
						Country = "US",
						UNLOCO = "USORD",
						MovementDescription = "Destination"
					},
					EstimatedTimeOfArrival = new ZDateTime(2024, 4, 1),
					EstimatedTimeOfDeparture = ZDateTime.Invalid,
					Mode = "AIR"
				},
				new ()
				{
					OriginPoint = null,
					DestinationPoint = null,
					EstimatedTimeOfArrival = new ZDateTime(2024, 4, 1),
					EstimatedTimeOfDeparture = new ZDateTime(2024, 2, 1),
					Mode = "AIR"
				}
			};

			var shipmentProvider = Factory.New<ShipmentWithProvider>();
			shipmentProvider.JS_RL_NKOrigin = "AUSYD";
			shipmentProvider.JS_RL_NKDestination = "AUSYD";
			shipmentProvider.AssessmentPointPairInfo = new ComplianceAssessmentPointPairInfo(pointPairs);

			var plugIn = new ComplianceRiskPlugInBusinessObject(shipmentProvider);
			var collection = plugIn.ComplianceRiskStatus.CommodityDetailCollection;
			var commodity1 = collection.AddNew();
			commodity1.CCD_HarmonizedCode = "123456";
			commodity1.CCD_RN_NKOrigin = "AU";
			commodity1.Description = "Dummy Desc";

			var commodity2 = collection.AddNew();
			commodity2.CCD_HarmonizedCode = "123456";
			commodity2.Description = "Dummy Desc";

			var commodity3 = collection.AddNew();
			commodity3.CCD_HarmonizedCode = "123456";
			commodity3.CCD_RN_NKOrigin = "AU";
			commodity3.Description = "Dummy Desc";
			commodity3.CCD_Description = "Dummy Goods Desc";

			var checker = new CommodityRiskStatusBorderWiseCheckerForTest(plugIn, CancellationToken.None);
			var request = ComplianceCheckRequestModelBuilder.GetRequestModel(shipmentProvider, pointPairs, new[] { commodity1, commodity2, commodity3 });
			AssertEquals(shipmentProvider.JobNumber, request.JobNumber);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				("AU", "AUSYD", "Origin", "export", "US", "USORD", "Destination", "import", new ZDateTime(2024, 3, 1).ToOffset().ToDateTimeOffset(),  new ZDateTime(2024, 2, 1).ToOffset().ToDateTimeOffset(), "SEA"),
				("AU", "AUSYD", "Origin", "export", null, null, null, null, DateTimeOffset.MinValue, new ZDateTime(2024, 2, 1).ToOffset().ToDateTimeOffset(), "SEA"),
				(null, null, null, null, "US", "USORD", "Destination", "import", new ZDateTime(2024, 4, 1).ToOffset().ToDateTimeOffset(), DateTimeOffset.MinValue, "AIR"),
			}, request.PointPairs.Select(u => (u.OriginPoint?.Country, u.OriginPoint?.Unloco, u.OriginPoint?.MovementDescription, u.OriginPoint?.MovementType, u.DestinationPoint?.Country, u.DestinationPoint?.Unloco, u.DestinationPoint?.MovementDescription, u.DestinationPoint?.MovementType, u.EstimatedTimeOfArrival, u.EstimatedTimeOfDeparture, u.Mode)));

			AssertContainsExactElementsInAnyOrder(new[]
			{
				("123456", "Dummy Desc", string.Empty, "AU"),
				("123456", "Dummy Desc", string.Empty, null),
				("123456", "Dummy Desc", "Dummy Goods Desc", "AU"),
			}, request.Commodities.Select(u => (u.HsCode, u.HsCodeDescription, u.GoodsDescription, u.Origin.SingleOrDefault())));
		}

		public void TestCheckCommodityRiskStatus_UpdateComplianceMaterialChangesSnapshot()
		{
			var pointPairs = GetPointPairs();
			var shipmentProvider = Factory.New<ShipmentWithProvider>();
			shipmentProvider.JS_RL_NKOrigin = "AUSYD";
			shipmentProvider.JS_RL_NKDestination = "NZAKL";
			shipmentProvider.AssessmentPointPairInfo = new ComplianceAssessmentPointPairInfo(pointPairs);

			var plugIn = new ComplianceRiskPlugInBusinessObject(shipmentProvider);
			var checker = new CommodityRiskStatusBorderWiseChecker(plugIn, CancellationToken.None);
			var collection = plugIn.ComplianceRiskStatus.CommodityDetailCollection;
			var commodity1 = collection.AddNew();
			commodity1.CCD_HarmonizedCode = "123456";

			AssertNull(plugIn.ComplianceMaterialChangesSnapshot);
			using (BorderWiseApiHelper.SetResponse(GetResponseForMultipleHsCodes(new[] { ("123456", false, false) })))
			{
				checker.CheckAllCommoditiesRiskStatus().Wait();
				AssertNotNull("Snapshot updated", plugIn.ComplianceMaterialChangesSnapshot);

				commodity1.CCD_HarmonizedCode = "654321";
				checker.CheckCommoditiesRiskStatus(commodity1).Wait();
				AssertNotNull("New Commodity Added To Snapshot", plugIn.ComplianceMaterialChangesSnapshot.Commodities.SingleOrDefault(u => u.HsCode == "654321"));
			}
		}

		public void TestGetComplianceBorderWiseRequestModel()
		{
			var pointPairs = GetPointPairs();
			var shipmentProvider = Factory.New<ShipmentWithProvider>();
			shipmentProvider.JS_RL_NKOrigin = "AUSYD";
			shipmentProvider.JS_RL_NKDestination = "NZAKL";
			shipmentProvider.AssessmentPointPairInfo = new ComplianceAssessmentPointPairInfo(pointPairs);
			shipmentProvider.ExposedCommodities = new[] { GetComplianceCommodity("123456", "WCO", "Source2", shipmentProvider.PK), GetComplianceCommodity("654321", "WCO", "Source3", Guid.NewGuid()) };

			var plugIn = new ComplianceRiskPlugInBusinessObject(shipmentProvider);
			var (requestModel, commodities) = ComplianceCheckRequestModelBuilder.GetRequestModel(plugIn, shipmentProvider.AssessmentPointPairInfo);
			AssertContainsExactElementsInAnyOrder("Return commodities belong to current job", new[] { "123456" }, requestModel.Commodities.Select(u => u.HsCode));
			AssertContainsExactElementsInAnyOrder("Return commodities belong to current job", new[] { "123456" }, commodities.Select(u => u.CCD_HarmonizedCode.ToString()));
		}

		public void TestLegalBookLinkNotShow_HsCodeNotFoundFromBorderWise()
		{
			var pointPairs = GetPointPairs();
			var shipmentProvider = Factory.New<ShipmentWithProvider>();
			shipmentProvider.JS_RL_NKOrigin = "AUSYD";
			shipmentProvider.JS_RL_NKDestination = "NZAKL";
			shipmentProvider.AssessmentPointPairInfo = new ComplianceAssessmentPointPairInfo(pointPairs);

			var plugIn = new ComplianceRiskPlugInBusinessObject(shipmentProvider);
			var checker = new CommodityRiskStatusBorderWiseChecker(plugIn, CancellationToken.None);
			var collection = plugIn.ComplianceRiskStatus.CommodityDetailCollection;
			var commodity1 = collection.AddNew();
			commodity1.CCD_HarmonizedCode = "123456";

			var commodity2 = collection.AddNew();
			commodity2.CCD_HarmonizedCode = "654321";

			var commodity3 = collection.AddNew();
			commodity3.CCD_HarmonizedCode = "111111";

			var point1 = new ComplianceCheckResponsePointPairLocationModel
			{
				NomenclatureWideConditionsApply = BorderWiseApiHelper.ConditionApply,
				CommoditySpecificConditionsApply = BorderWiseApiHelper.ConditionApply,
				IsValidHsCode = true,
			};

			var point2 = new ComplianceCheckResponsePointPairLocationModel
			{
				NomenclatureWideConditionsApply = BorderWiseApiHelper.ConditionApply,
				CommoditySpecificConditionsApply = BorderWiseApiHelper.ConditionApply,
				IsValidHsCode = false,
			};

			var point3 = new ComplianceCheckResponsePointPairLocationModel
			{
				NomenclatureWideConditionsApply = BorderWiseApiHelper.ConditionApply,
				CommoditySpecificConditionsApply = BorderWiseApiHelper.ConditionApply,
				IsValidHsCode = true,
				MatchedHsCode = "1111"
			};

			var response = new ComplianceCheckResponseModel
			{
				RequestId = Guid.NewGuid(),
				Commodities = new[]
				{
					new ComplianceCheckResponseCommodityModel
					{
						HsCode = "123456",
						CommoditySpecificConditionsApply = BorderWiseApiHelper.ConditionApply,
						NomenclatureWideConditionsApply = BorderWiseApiHelper.ConditionApply,
						PointPairs = new[]
						{
							new ComplianceCheckResponsePointPairModel
							{
								OriginPoint = point1,
								DestinationPoint = point1,
							}
						}
					},
					new ComplianceCheckResponseCommodityModel
					{
						HsCode = "654321",
						CommoditySpecificConditionsApply = BorderWiseApiHelper.ConditionUncertain,
						NomenclatureWideConditionsApply = BorderWiseApiHelper.ConditionUncertain,
						PointPairs = new[]
						{
							new ComplianceCheckResponsePointPairModel
							{
								OriginPoint = point2,
								DestinationPoint = point2
							}
						}
					},
					new ComplianceCheckResponseCommodityModel
					{
						HsCode = "111111",
						CommoditySpecificConditionsApply = BorderWiseApiHelper.ConditionUncertain,
						NomenclatureWideConditionsApply = BorderWiseApiHelper.ConditionUncertain,
						PointPairs = new[]
						{
							new ComplianceCheckResponsePointPairModel
							{
								OriginPoint = point3,
								DestinationPoint = point3
							}
						}
					}
				}
			};

			using (BorderWiseApiHelper.SetResponse(response))
			{
				checker.CheckAllCommoditiesRiskStatus().Wait();

				CombineAssertions(() =>
				{
					AssertEquals(ZString.Empty, commodity1.LegalBookLink);
					AssertEquals(ZString.Empty, commodity1.HarmonizedBorderWiseTextual);
					AssertEquals(BorderWiseCheckStatus.NotViewable, commodity1.BorderWiseCheckStatus);

					AssertEquals(ZString.Empty, commodity2.LegalBookLink);
					AssertEquals(ZString.Empty, commodity2.HarmonizedBorderWiseTextual);
					AssertEquals(BorderWiseCheckStatus.NotViewable, commodity2.BorderWiseCheckStatus);

					AssertNotNullOrEmpty(commodity3.LegalBookLink);
					AssertNotNullOrEmpty(commodity3.HarmonizedBorderWiseTextual);
					AssertEquals(BorderWiseCheckStatus.Viewable, commodity3.BorderWiseCheckStatus);
				});
			}
		}

		public void TestLegalBookLinkNotShow_OriginPointCountryAndDestinationPointCountryUnsupportedFromBorderWise()
		{
			var pointPairs = GetPointPairs();
			var shipmentProvider = Factory.New<ShipmentWithProvider>();
			shipmentProvider.JS_RL_NKOrigin = "AUSYD";
			shipmentProvider.JS_RL_NKDestination = "NZAKL";
			shipmentProvider.AssessmentPointPairInfo = new ComplianceAssessmentPointPairInfo(pointPairs);

			var plugIn = new ComplianceRiskPlugInBusinessObject(shipmentProvider);
			var checker = new CommodityRiskStatusBorderWiseChecker(plugIn, CancellationToken.None);
			var collection = plugIn.ComplianceRiskStatus.CommodityDetailCollection;
			var commodity1 = collection.AddNew();
			commodity1.CCD_HarmonizedCode = "123456";

			var commodity2 = collection.AddNew();
			commodity2.CCD_HarmonizedCode = "654321";

			var commodity3 = collection.AddNew();
			commodity3.CCD_HarmonizedCode = "234567";

			var applyPoint = new ComplianceCheckResponsePointPairLocationModel
			{
				NomenclatureWideConditionsApply = BorderWiseApiHelper.ConditionApply,
				CommoditySpecificConditionsApply = BorderWiseApiHelper.ConditionApply,
				MatchedHsCode = "123456"
			};

			var uncertainPoint = new ComplianceCheckResponsePointPairLocationModel
			{
				NomenclatureWideConditionsApply = BorderWiseApiHelper.ConditionUncertain,
				CommoditySpecificConditionsApply = BorderWiseApiHelper.ConditionUncertain,
			};

			var response = new ComplianceCheckResponseModel
			{
				RequestId = Guid.NewGuid(),
				Commodities = new[]
				{
					new ComplianceCheckResponseCommodityModel
					{
						HsCode = "123456",
						CommoditySpecificConditionsApply = BorderWiseApiHelper.ConditionApply,
						NomenclatureWideConditionsApply = BorderWiseApiHelper.ConditionApply,
						PointPairs = new[]
						{
							new ComplianceCheckResponsePointPairModel
							{
								OriginPoint = applyPoint,
								DestinationPoint = applyPoint
							}
						}
					},

					new ComplianceCheckResponseCommodityModel
					{
						HsCode = "654321",
						CommoditySpecificConditionsApply = BorderWiseApiHelper.ConditionApply,
						NomenclatureWideConditionsApply = BorderWiseApiHelper.ConditionApply,
						PointPairs = new[]
						{
							new ComplianceCheckResponsePointPairModel
							{
								OriginPoint = uncertainPoint,
								DestinationPoint = uncertainPoint
							}
						}
					},

					new ComplianceCheckResponseCommodityModel
					{
						HsCode = "234567",
						CommoditySpecificConditionsApply = BorderWiseApiHelper.ConditionApply,
						NomenclatureWideConditionsApply = BorderWiseApiHelper.ConditionApply,
						PointPairs = new[]
						{
							new ComplianceCheckResponsePointPairModel
							{
								OriginPoint = null,
								DestinationPoint = uncertainPoint
							},
							new ComplianceCheckResponsePointPairModel
							{
								OriginPoint = uncertainPoint,
								DestinationPoint = null
							}
						}
					}
				}
			};

			using (BorderWiseApiHelper.SetResponse(response))
			{
				checker.CheckAllCommoditiesRiskStatus().Wait();

				CombineAssertions(() =>
				{
					AssertNotNullOrEmpty(commodity1.LegalBookLink);
					AssertNotNullOrEmpty(commodity1.HarmonizedBorderWiseTextual);
					AssertEquals(BorderWiseCheckStatus.Viewable, commodity1.BorderWiseCheckStatus);

					AssertEquals(commodity2.LegalBookLink, ZString.Empty);
					AssertEquals(commodity2.HarmonizedBorderWiseTextual, ZString.Empty);
					AssertEquals(BorderWiseCheckStatus.NotViewable, commodity2.BorderWiseCheckStatus);

					AssertEquals(commodity3.LegalBookLink, ZString.Empty);
					AssertEquals(commodity3.HarmonizedBorderWiseTextual, ZString.Empty);
					AssertEquals(BorderWiseCheckStatus.NotViewable, commodity3.BorderWiseCheckStatus);
				});
			}
		}

		public void TestGetSupportedCountriesAndAssignStatusIfNeeded()
		{
			var pointPairs = GetPointPairs();
			var shipmentProvider = Factory.New<ShipmentWithProvider>();
			shipmentProvider.JS_RL_NKOrigin = "AUSYD";
			shipmentProvider.JS_RL_NKDestination = "NZAKL";
			shipmentProvider.AssessmentPointPairInfo = new ComplianceAssessmentPointPairInfo(pointPairs);

			var plugIn = new ComplianceRiskPlugInBusinessObject(shipmentProvider);
			var checker = new CommodityRiskStatusBorderWiseChecker(plugIn, CancellationToken.None);
			var collection = plugIn.ComplianceRiskStatus.CommodityDetailCollection;
			var commodity1 = collection.AddNew();
			commodity1.CCD_RN_NKOrigin = "AU";
			commodity1.CCD_HarmonizedCode = "123456";
			AssertEquals(Codes.NotChecked, commodity1.CCD_RiskStatus);
			AssertNull("SupportedCountriesCheckResponseModel is null before call GetSupportedCountriesAndAssignStatusIfNeeded", plugIn.ComplianceRiskStatus.SupportedCountriesCheckResponseModel);

			using (BorderWiseApiHelper.SetSupportedCountriesResponse(new SupportedCountriesCheckResponseModel
			{
				CommodityLevel = new CommodityLevelModel
				{
					Export = new[] { "US" },
					Import = new[] { "US" },
					OriginOfGoods = new[] { "US" }
				}
			}))
			{
				checker.GetSupportedCountriesAndAssignStatusIfNeeded().Wait();
				AssertNotNull("SupportedCountriesCheckResponseModel is not null when call GetSupportedCountriesAndAssignStatusIfNeeded", plugIn.ComplianceRiskStatus.SupportedCountriesCheckResponseModel);
				AssertEquals(Codes.PossibleRisk, commodity1.CCD_RiskStatus);
			}
		}

		#region Implementation

		ComplianceCommodity GetComplianceCommodity(ZString harmonizedCode, ZString groupingOrCountry, ZString source, ZGuid parentJobID)
		{
			return new ComplianceCommodity(harmonizedCode, groupingOrCountry, source, parentJobID, string.Empty, string.Empty, string.Empty);
		}

		static ComplianceCheckResponseModel GetResponseForMultipleHsCodes((string hsCode, bool commoditySpecificConditionsApply, bool nomenclatureWideConditionsApply)[] responseInfo)
		{
			return GetResponseForMultipleHsCodes(responseInfo.Select(u => (u.hsCode, u.commoditySpecificConditionsApply, u.nomenclatureWideConditionsApply, (string)null, (string)null, true)).ToArray());
		}

		static ComplianceCheckResponseModel GetResponseForMultipleHsCodes((string hsCode, bool commoditySpecificConditionsApply, bool nomenclatureWideConditionsApply, string origin, string description, bool hasMatchedHsCode)[] responseInfo)
		{
			return new ComplianceCheckResponseModel
			{
				RequestId = Guid.NewGuid(),
				Commodities = responseInfo.Select(hsCode => new ComplianceCheckResponseCommodityModel
				{
					HsCode = hsCode.hsCode,
					Origin = new[] { hsCode.origin }.Where(u => !string.IsNullOrEmpty(u)).ToArray(),
					GoodsDescription = hsCode.description,
					CommoditySpecificConditionsApply = hsCode.commoditySpecificConditionsApply ? "Yes" : BorderWiseApiHelper.ConditionNotApply,
					NomenclatureWideConditionsApply = hsCode.nomenclatureWideConditionsApply ? "Yes" : BorderWiseApiHelper.ConditionNotApply,
					PointPairs = new[]
					{
						new ComplianceCheckResponsePointPairModel
						{
							OriginPoint = new ComplianceCheckResponsePointPairLocationModel
							{
								MatchedHsCode = hsCode.hasMatchedHsCode ? hsCode.hsCode : string.Empty,
								Country = hsCode.origin
							},
							DestinationPoint = new ComplianceCheckResponsePointPairLocationModel
							{
								MatchedHsCode = hsCode.hasMatchedHsCode ? hsCode.hsCode : string.Empty,
								Country = "NZ",
								ComplianceCodes = (hsCode.commoditySpecificConditionsApply | hsCode.nomenclatureWideConditionsApply ) ? new[] { "TEST" } : Array.Empty<string>()
							}
						}
					}
				}).ToArray(),
				Locations = new[]
				{
					new ComplianceCheckResponseLocationModel
					{
						Country = "AU",
						IsSupportedCountry = true
					},
					new ComplianceCheckResponseLocationModel
					{
						Country = "NZ",
						IsSupportedCountry = true
					}
				}
			};
		}

		public static ComplianceCheckResponseModel GetResponse(string hsCode, bool commoditySpecificConditionsApply, bool nomenclatureWideConditionsApply, BusinessObjectFactory factory, string origin = null, string goodsDescription = null, bool hasMatchedHsCode = true)
		{
			var duplicateZQuery = new ZQuery(RefComplianceCommodityAlertSchema.RCR_AlertCode, "TEST");
			duplicateZQuery.AddToFilter(RefComplianceCommodityAlertSchema.RCR_CountryRegion, "NZ");

			if (factory.Exists(typeof(RefComplianceCommodityAlert), duplicateZQuery))
			{
				var alert = factory.LoadTop1<RefComplianceCommodityAlert>(duplicateZQuery);
				alert.Delete();
			}

			if (commoditySpecificConditionsApply)
			{
				var alert = factory.New<RefComplianceCommodityAlert>();
				alert.RCR_AlertCode = "TEST";
				alert.RCR_AlertType = "COM";
				alert.RCR_AlertName = "DUMMY NAME";
				alert.RCR_TradeDirection = "IMP";
				alert.RCR_CountryRegion = "NZ";
				alert.RCR_CommodityRiskStatus = "HSK";
			}
			else if (nomenclatureWideConditionsApply)
			{
				var alert = factory.New<RefComplianceCommodityAlert>();
				alert.RCR_AlertCode = "TEST";
				alert.RCR_AlertType = "NOM";
				alert.RCR_AlertName = "DUMMY NAME";
				alert.RCR_TradeDirection = "IMP";
				alert.RCR_CountryRegion = "NZ";
				alert.RCR_CommodityRiskStatus = "PRS";
			}

			return GetResponseForMultipleHsCodes(new[] { (hsCode, commoditySpecificConditionsApply, nomenclatureWideConditionsApply, origin, goodsDescription, hasMatchedHsCode) });
		}

		public static IEnumerable<ComplianceCheckRequestPointPair> GetPointPairs(string originCountry = "AU", string originUnloco = "AUSYD", string destinationCountry = "NZ", string destinationUnloco = "NZAKL")
		{
			return new List<ComplianceCheckRequestPointPair> {
			new ()
			{
				OriginPoint = new ComplianceCheckRequestPointPairLocation
				{
					Country = originCountry,
					UNLOCO = originUnloco,
					MovementDescription = "Origin"
				},
				DestinationPoint = new ComplianceCheckRequestPointPairLocation
				{
					Country = destinationCountry,
					UNLOCO = destinationUnloco,
					MovementDescription = "Destination"
				},
				EstimatedTimeOfArrival = new ZDateTime(2024, 1, 1),
				EstimatedTimeOfDeparture = new ZDateTime(2024, 2, 1),
				Mode = "SEA"
				}
			};
		}

		bool rawEnableComplianceRisk;
		EnableComplianceWiseRegistryBusinessObject rawComplianceWiseRegistryBusinessObject;

		protected override void SetUp()
		{
			base.SetUp();
			rawEnableComplianceRisk = RawDataRegistry.Instance.EnableComplianceRisk.Value;
			rawComplianceWiseRegistryBusinessObject = FreightDataRegistry.Instance.FreightEnableComplianceWise.DefaultValue;

			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
			ComplianceWiseRegistryHelper.SetValue(true));
		}

		protected override void TearDown()
		{
			base.TearDown();
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawEnableComplianceRisk);
			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawComplianceWiseRegistryBusinessObject);
		}

		class CommodityRiskStatusBorderWiseCheckerForTest : CommodityRiskStatusBorderWiseChecker
		{
			public CommodityRiskStatusBorderWiseCheckerForTest(ComplianceRiskPlugInBusinessObject pluginBizO, CancellationToken cancellationToken, bool overrideResponseIsValidToFalseOnce = false) : base(pluginBizO, cancellationToken)
			{
				OverrideResponseIsValidOnce = overrideResponseIsValidToFalseOnce;
			}

			bool OverrideResponseIsValidOnce { get; set; }

			public bool ResponseIsValidToProcessExposed(bool useCachedResponse, ComplianceCheckRequestModel request, ComplianceCommodityDetail[] commodities) => base.ResponseIsValidToProcess(useCachedResponse, request, commodities);

			protected internal override bool ResponseIsValidToProcess(bool useCachedResponse, ComplianceCheckRequestModel request, ComplianceCommodityDetail[] commodities)
			{
				if (OverrideResponseIsValidOnce)
				{
					OverrideResponseIsValidOnce = false;
					return false;
				}

				return base.ResponseIsValidToProcess(useCachedResponse, request, commodities);
			}
		}

		#endregion
	}
}
