using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration.Rating;
using Enterprise.Integration.ServiceManager;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using WTG.Foundation.Http;
using static Enterprise.ComplianceRisk.Business.BorderWiseApiHelper;

namespace Enterprise.ComplianceRisk.ServiceTasks.Test;

[TestedType(typeof(ComplianceRiskAssessmentServiceTask))]
public class ComplianceRiskAssessmentServiceTaskTest : ServiceTaskTestCase<ComplianceRiskAssessmentServiceTask>
{
	protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

	public void TestHostedServiceAttributes()
	{
		var serviceAttribute = GetHostedServiceAttributes().Single();
		AssertEquals("AllowsMultipleInstances", false, serviceAttribute.AllowsMultipleInstances);
		AssertEquals("IsMandatory", false, serviceAttribute.IsMandatory);
		AssertEquals("ActiveByDefault", true, serviceAttribute.ActiveByDefault);
		AssertEquals("MinimumPeriod", "1minute", serviceAttribute.MinimumPeriod);
		AssertEquals("ServiceTaskCode", "CRA", serviceAttribute.Code);
		AssertEquals("Description", "Compliance Risk Assessment Service Task", serviceAttribute.Description);
		AssertEquals("Category", "CPW", serviceAttribute.Category);
		AssertEquals("Type", typeof(ComplianceRiskAssessmentServiceTask), serviceAttribute.Type);
		AssertEquals("CanRunInAnyBranch", true, serviceAttribute.CanRunInAnyBranch);
		AssertEquals("DefaultScheduleRunEvery", "10minutes", serviceAttribute.DefaultScheduleRunEvery);
	}

	/// <summary>
	/// Test empty message queue doesn't call BW API
	/// </summary>
	public void TestEmptyMessageQueueDoesNotCallBw()
	{
		using var setReturnDummyResponseForTestToFalse = new SetReturnDummyResponseForTestToFalse();
		var handlerMock = ComplianceServiceTaskTestHelper.MockHttpMessageHandler(("123123", "job123"));
		var httpFactoryMock = new Mock<IHttpClientFactory>();
		httpFactoryMock.Setup(factory => factory.CreateNew(It.IsAny<HttpMessageHandler>(), It.IsAny<TimeSpan>())).Returns(new HttpClient(handlerMock.Object));

		var scheduledTaskPk = CreateStmTaskScheduleEntryIfNotExistsAndGetPk();

		using (ObjectFactory.Substitute(() => httpFactoryMock.Object))
		{
			var serviceTask = new ComplianceRiskAssessmentServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask(CancellationToken.None);

			handlerMock.Protected().Verify("SendAsync", Times.Never(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
			AssertionCount++;
		}
	}

	/// <summary>
	/// Test Single Queued message calls BW API, updates commodities, recalculates shipment status, marks message as processed
	/// </summary>
	public void TestMessageShipment()
	{
		var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
		shipment.JS_RL_NKOrigin = "AUSYD";
		shipment.JS_RL_NKDestination = "USLAX";

		var complianceRiskStatus = ComplianceServiceTaskTestHelper.CreateNewComplianceRiskStatus(Factory, shipment);
		complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;
		complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;

		var commodity = complianceRiskStatus.CommodityDetailCollection.AddNew();
		commodity.CCD_HarmonizedCode = "123456";
		commodity.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.NotChecked;

		var message = ComplianceServiceTaskTestHelper.CreateNewEdiMessage(Factory, shipment);

		CreateAssessmentEvent(shipment, ComplianceEventList.Codes.AssessmentInitialized);

		Factory.Save();

		var bwResponse = ComplianceServiceTaskTestHelper.MockCleanBwResponse(("123456", shipment.JobNumber));
		using (BorderWiseApiHelper.SetResponse(bwResponse))
		{
			var serviceTask = new ComplianceRiskAssessmentServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask(CancellationToken.None);
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, commodity.CCD_RiskStatus);
			complianceRiskStatus.ReloadSafe();
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);
		}
	}

	public void TestMessageViewQuotedBookingQuick()
	{
		AssertViewQuotedBookingQuick(shipmentForwardRegistered: false);
		AssertViewQuotedBookingQuick(shipmentForwardRegistered: true);

		void AssertViewQuotedBookingQuick(bool shipmentForwardRegistered)
		{
			var quotedBooking = ComplianceServiceTaskTestHelper.CreateNewQuotedBooking(Factory, QuoteBookingType.QuickBooking);
			var viewQuotedBooking = quotedBooking.Factory.Load<IViewQuotedBooking>((quotedBooking as BusinessObject).PK);
			var shipment = (CommonShipment)quotedBooking.ForwardingShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_IsForwardRegistered = shipmentForwardRegistered;

			CombineAssertions("Pre-condition", () =>
			{
				AssertEquals("VB_JS", shipment.PK, viewQuotedBooking.VB_JS);
				AssertEquals("VB_TH", ZGuid.Empty, viewQuotedBooking.VB_TH);
			});

			var complianceRiskStatus = ComplianceServiceTaskTestHelper.CreateNewComplianceRiskStatus(Factory, shipment);
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;

			var commodity = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity.CCD_HarmonizedCode = "123456";
			commodity.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.NotChecked;

			var message = ComplianceServiceTaskTestHelper.CreateNewEdiMessage(Factory, (BusinessObject)quotedBooking);

			CreateAssessmentEvent((BusinessObject)quotedBooking, ComplianceEventList.Codes.AssessmentInitialized);

			Factory.Save();

			var bwResponse = ComplianceServiceTaskTestHelper.MockCleanBwResponse(("123456", shipment.JobNumber));
			using (BorderWiseApiHelper.SetResponse(bwResponse))
			{
				var serviceTask = new ComplianceRiskAssessmentServiceTask { ServiceLogger = new TestServiceLogger() };
				serviceTask.RunTask(CancellationToken.None);
				complianceRiskStatus.ReloadSafe();

				CombineAssertions("Quoted Booking Type: " + nameof(QuoteBookingType.QuickBooking), () =>
				{
					AssertEquals(ViewQuotedBookingSchema.Constants.TableName, message.EM_LinkTable);

					if (shipmentForwardRegistered)
					{
						AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);
						AssertEquals(ComplianceRiskStatusCodeList.Codes.NotChecked, commodity.CCD_RiskStatus);
						AssertEquals(ComplianceRiskStatusCodeList.Codes.Held, complianceRiskStatus.COR_OverallRisk);
					}
					else
					{
						AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
						AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, commodity.CCD_RiskStatus);
						AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);
					}
				});
			}
		}
	}

	public void TestMessageViewQuotedBookingQuote()
	{
		var voyage = ComplianceServiceTaskTestHelper.CreateNewSailingVoyage(Factory, "VesselForTest");
		var sailing = ComplianceServiceTaskTestHelper.GetSailingFromPortPair(voyage, "AUSYD", "USLAX");
		sailing.Destination.JB_A_ARV = sailing.JX_JB_E_ARV;
		sailing.Origin.JA_A_DEP = sailing.JX_JA_E_DEP;

		var quotedBooking = ComplianceServiceTaskTestHelper.CreateNewQuotedBooking(Factory, QuoteBookingType.BookingWithQuote);
		var viewQuotedBooking = quotedBooking.Factory.Load<IViewQuotedBooking>((quotedBooking as BusinessObject).PK);
		var shipment = (CommonShipment)quotedBooking.ForwardingShipment;
		shipment.JS_RL_NKOrigin = "AUSYD";
		shipment.JS_RL_NKDestination = "USLAX";
		shipment.JS_JX = sailing.PK;

		CombineAssertions("Pre-condition", () =>
		{
			AssertEquals("VB_JS", shipment.PK, viewQuotedBooking.VB_JS);
			AssertEquals("VB_TH", quotedBooking.Quote.PK, viewQuotedBooking.VB_TH);
		});

		var complianceRiskStatus = ComplianceServiceTaskTestHelper.CreateNewComplianceRiskStatus(Factory, quotedBooking.Quote);
		complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;
		complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
		complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Blocked;

		var commodity = complianceRiskStatus.CommodityDetailCollection.AddNew();
		commodity.CCD_HarmonizedCode = "123456";
		commodity.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.NotChecked;

		var message = ComplianceServiceTaskTestHelper.CreateNewEdiMessage(Factory, (BusinessObject)quotedBooking);

		CreateAssessmentEvent((BusinessObject)quotedBooking, ComplianceEventList.Codes.AssessmentInitialized);

		Factory.Save();

		var bwResponse = ComplianceServiceTaskTestHelper.MockCleanBwResponse(("123456", (quotedBooking.Quote as IRatingHeader).TH_QuoteNumber));
		using (BorderWiseApiHelper.SetResponse(bwResponse))
		{
			var serviceTask = new ComplianceRiskAssessmentServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask(CancellationToken.None);
			complianceRiskStatus.ReloadSafe();

			CombineAssertions("Quoted Booking Type: " + nameof(QuoteBookingType.BookingWithQuote), () =>
			{
				AssertEquals(ViewQuotedBookingSchema.Constants.TableName, message.EM_LinkTable);
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, commodity.CCD_RiskStatus);
				AssertEquals("Parties Risk: ", ComplianceRiskStatusCodeList.Codes.HighRisk, complianceRiskStatus.COR_PartyRisk);
				AssertEquals("Locations Risk:", ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_LocationRisk);
				AssertEquals("Commodities Risk: ", ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_CommodityRisk);
				AssertEquals("Overall Risk:", ComplianceRiskStatusCodeList.Codes.Held, complianceRiskStatus.COR_OverallRisk);
			});
		}
	}

	/// <summary>
	/// Test BatchSize+1 (11 by default) Queued messages calls BW API twice
	/// </summary>
	public void TestBatchCallsToBwService()
	{
		using (OrganisationsDataRegistry.Instance.ComplianceRiskAssessmentBatchSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 10))
		{
			var hsCodes = new[] { "123001", "123002", "123003", "123004", "123005", "123006", "123007", "123008", "123009", "123010", "123011" };
			var shipments = hsCodes.Select(CreateShipmentWithMessage).ToArray();

			Factory.Save();

			using var setReturnDummyResponseForTestToFalse = new SetReturnDummyResponseForTestToFalse();
			var handlerMock = ComplianceServiceTaskTestHelper.MockHttpMessageHandler(hsCodes.Zip(shipments, (code, shipment) => (code, shipment.JobNumber)).ToArray());
			var httpFactoryMock = new Mock<IHttpClientFactory>();
			httpFactoryMock.Setup(factory => factory.CreateNew(It.IsAny<HttpMessageHandler>(), It.IsAny<TimeSpan>()))
				.Returns(() => new HttpClient(handlerMock.Object, false));

			using (ObjectFactory.Substitute(() => httpFactoryMock.Object))
			{
				var serviceTask = new ComplianceRiskAssessmentServiceTask { ServiceLogger = new TestServiceLogger() };
				serviceTask.RunTask(CancellationToken.None);

				handlerMock.Protected().Verify("SendAsync", Times.Exactly(2), ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>());
				AssertionCount++;
			}
		}

		ForwardingShipment CreateShipmentWithMessage(string hsCode)
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var complianceRiskStatus = ComplianceServiceTaskTestHelper.CreateNewComplianceRiskStatus(Factory, shipment);
			var commodity = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity.CCD_HarmonizedCode = hsCode;
			commodity.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.NotChecked;

			_ = ComplianceServiceTaskTestHelper.CreateNewEdiMessage(Factory, shipment);

			return shipment;
		}
	}

	/// <summary>
	/// Test Queued messages for non-international shipment
	/// </summary>
	public void TestMessageDiscarded()
	{
		var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
		shipment.JS_RL_NKOrigin = "AUSYD";
		shipment.JS_RL_NKDestination = "AUSYD";

		var message = ComplianceServiceTaskTestHelper.CreateNewEdiMessage(Factory, shipment);
		message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;

		Factory.Save();

		using (BorderWiseApiHelper.SetResponse(Array.Empty<ComplianceCheckResponseModel>()))
		{
			var serviceTask = new ComplianceRiskAssessmentServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask(CancellationToken.None);

			AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);
		}
	}

	/// <summary>
	/// Test Multiple Queued messages for the same shipment calls BW API once and cancels the rest of messages but does not cancel
	/// messages arrived after the task started processing shipment
	/// </summary>
	public void TestMessageCancelled()
	{
		var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
		shipment.JS_RL_NKOrigin = "AUSYD";
		shipment.JS_RL_NKDestination = "USLAX";
		var complianceRiskStatus = ComplianceServiceTaskTestHelper.CreateNewComplianceRiskStatus(Factory, shipment);
		var commodity = complianceRiskStatus.CommodityDetailCollection.AddNew();
		commodity.CCD_HarmonizedCode = "123456";
		commodity.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.NotChecked;

		var message1 = ComplianceServiceTaskTestHelper.CreateNewEdiMessage(Factory, shipment);
		message1.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(-10);

		var message2 = ComplianceServiceTaskTestHelper.CreateNewEdiMessage(Factory, shipment);
		message2.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(-9);

		Factory.Save();

		CreateStmTaskScheduleEntryIfNotExistsAndGetPk();

		var bwResponse = ComplianceServiceTaskTestHelper.MockCleanBwResponse(("123456", shipment.JobNumber));
		using (BorderWiseApiHelper.SetResponse(bwResponse))
		{
			var serviceTask = new ComplianceRiskAssessmentServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask(CancellationToken.None);

			message1.ReloadSafe();
			message2.ReloadSafe();
			AssertEquals(EDIMessageStatusList.Codes.Cancelled, message1.EM_Status);
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message2.EM_Status);
		}
	}

	/// <summary>
	/// Test BW API returns 500, message is left in Queued status
	/// </summary>
	public void TestMessageErrorWithServiceReturns500()
	{
		var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
		shipment.JS_RL_NKOrigin = "AUSYD";
		shipment.JS_RL_NKDestination = "USLAX";

		var complianceRiskStatus = ComplianceServiceTaskTestHelper.CreateNewComplianceRiskStatus(Factory, shipment);
		var commodity = complianceRiskStatus.CommodityDetailCollection.AddNew();
		commodity.CCD_HarmonizedCode = "123456";
		commodity.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.NotChecked;

		var message = ComplianceServiceTaskTestHelper.CreateNewEdiMessage(Factory, shipment);

		Factory.Save();

		using var setReturnDummyResponseForTestToFalse = new SetReturnDummyResponseForTestToFalse();

		var exceptionReporter = ExceptionReporterTestListener.Instance;
		var handlerMock = ComplianceServiceTaskTestHelper.MockHttpMessageHandlerWithEmptyResponse(HttpStatusCode.InternalServerError);
		var httpFactoryMock = new Mock<IHttpClientFactory>();
		httpFactoryMock.Setup(factory => factory.CreateNew(It.IsAny<HttpMessageHandler>(), It.IsAny<TimeSpan>()))
			.Returns(() => new HttpClient(handlerMock.Object));

		using (ObjectFactory.Substitute(() => httpFactoryMock.Object))
		{
			var serviceTask = new ComplianceRiskAssessmentServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask(CancellationToken.None);

			message.ReloadSafe();
			AssertEquals(EDIMessageStatusList.Codes.Error, message.EM_Status);
			AssertEquals(1, exceptionReporter.Count);
			AssertContains("Should have error reported.", "Error occurred when Processing NCH Commodities in CRA service task", exceptionReporter.Last().Message);
			exceptionReporter.Clear();
		}
	}

	Guid CreateStmTaskScheduleEntryIfNotExistsAndGetPk()
	{
		var stQuery = new ZQuery(StmScheduleTaskSchema.S5_ScheduleType, "CRA").AddToFilter(new ZQuery(StmScheduleTaskSchema.S5_TypeOfDocument, "CPW"));
		var scheduledTask = Factory.LoadTop1(ObjectFactory.GetType<IStmScheduleTask>(), stQuery);
		if (scheduledTask == null)
		{
			scheduledTask = Factory.NewWithValidTestData(ObjectFactory.GetType<IStmScheduleTask>());
			Factory.Save();
			_ = TestConnection.ExecuteNonQuery(@"
UPDATE
	dbo.StmScheduleTask
SET
	S5_TypeOfDocument='CPW',
	S5_ScheduleType='CRA'
WHERE
	S5_PK=@Pk",
	parameters => parameters.AddParameter("@Pk", System.Data.SqlDbType.UniqueIdentifier, scheduledTask.PK.ToGuid()));
		}

		return scheduledTask.PK.ToGuid();
	}

	public static StmComplianceEvent CreateAssessmentEvent(BusinessObject bizO, string assessmentType)
	{
		var eventLog = bizO.Factory.NewWithValidTestData<StmComplianceEvent>();
		eventLog.SCE_EventType = AutoEvents.ComplianceRiskInteractionCode;
		eventLog.SCE_EventSubType = assessmentType;
		eventLog.SCE_ParentID = bizO.PK;
		return eventLog;
	}

	IDisposable setAllowComplianceCommodityRiskAssessmentToTrue;
	protected override void SetUpCore()
	{
		base.SetUpCore();
		setAllowComplianceCommodityRiskAssessmentToTrue = OrganisationsDataRegistry.Instance.AllowComplianceCommodityRiskAssessment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
	}

	protected override void TearDownCore()
	{
		base.TearDownCore();
		setAllowComplianceCommodityRiskAssessmentToTrue.Dispose();
	}
}
