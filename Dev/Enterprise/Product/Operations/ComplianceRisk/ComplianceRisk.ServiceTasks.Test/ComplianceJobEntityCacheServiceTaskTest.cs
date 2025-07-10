using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Business.Test;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ComplianceRisk.ServiceTasks.Test
{
	[TestedType(typeof(ComplianceJobEntityCacheServiceTask))]
	public class ComplianceJobEntityCacheServiceTaskTest : ServiceTaskTestCase<ComplianceJobEntityCacheServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		public void TestHostedServiceAttributes()
		{
			var serviceAttribute = GetHostedServiceAttributes().SingleOrDefault();
			CombineAssertions("Service task properties", () =>
			{
				AssertEquals("AllowsMultipleInstances", false, serviceAttribute.AllowsMultipleInstances);
				AssertEquals("IsMandatory", false, serviceAttribute.IsMandatory);
				AssertEquals("MinimumPeriod", "1minute", serviceAttribute.MinimumPeriod);
				AssertEquals("ServiceTaskCode", "JEC", serviceAttribute.Code);
				AssertEquals("Description", "Compliance Job Entity Cache Transformation", serviceAttribute.Description);
				AssertEquals("Category", "CPW", serviceAttribute.Category);
				AssertEquals("Type", typeof(ComplianceJobEntityCacheServiceTask), serviceAttribute.Type);
				AssertEquals("CanRunInAnyBranch", true, serviceAttribute.CanRunInAnyBranch);
				AssertEquals("DefaultScheduleRunEvery", "10minutes", serviceAttribute.DefaultScheduleRunEvery);
				Assert("ActiveByDefault", serviceAttribute.ActiveByDefault);
			});
		}

		public void TestShipmentEntityCache()
		{
			var (orgConsignor, orgConsignee) = Factory.CreateNewOrgConsignorAndConsignee();
			var (complianceRisk, shipment) = Factory.CreateNewComplianceRiskWithShipment();
			shipment.ConsigneePK = orgConsignee.PK;
			shipment.ConsignorPK = orgConsignor.PK;

			var refVessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			var transport = shipment.AddTransportLeg("AUSYD", "SGSIN", refVessel.RV_Code);
			transport = shipment.AddTransportLeg("AUSYD", "AUMEL", "Free Text Vessel");

			var complianceBizO = new ComplianceRiskBusinessObject(shipment, complianceRisk);
			complianceRisk.COR_JobEndDate = complianceBizO.ComplianceItemRiskStatusProvider.JobTime.JobEndDate.UtcToDateTimeOffset();

			Factory.Save();

			var governorMock = new Mock<IServiceManagerGovernor>();
			using (ObjectFactory.Substitute(governorMock.Object))
			using (Env.Instance.TemporaryServiceTaskContext(ComplianceJobEntityCacheServiceTask.Code, canRunInAnyBranch: true))
			{
				var serviceTask = new ComplianceJobEntityCacheServiceTask();
				serviceTask.ServiceLogger = new TestServiceLogger();
				serviceTask.RunTask(CancellationToken.None);

				var entityCache = Factory.LoadComplianceJobEntityCache(complianceRisk);

				CombineAssertions("The entity cache is saved in the database", () =>
				{
					AssertEquals("No Errors", string.Empty, ErrorReporter.LastMessageReported);
					AssertEquals("Entity cache count 4", 4, entityCache.Length);
					AssertEquals(@$"Consignee: {orgConsignee.PK}", true, Factory.IsEntityCacheInDatabase(entityCache, orgConsignee));
					AssertEquals(@$"Consignor: {orgConsignor.PK}", true, Factory.IsEntityCacheInDatabase(entityCache, orgConsignor));
					AssertEquals(@$"Transport Free Text Vessel: {transport.PK}", true, Factory.IsEntityCacheInDatabase(entityCache, transport));
					AssertEquals(@$"Transport RefVessel: {refVessel.PK}", true, Factory.IsEntityCacheInDatabase(entityCache, refVessel));
					AssertDeactivateServiceTask(governorMock);
				});
			}
		}

		public void TestConsolidationEntityCache()
		{
			var (orgSendingForwarder, orgReceivingForwarder) = Factory.CreateNewOrgConsignorAndConsignee();
			var (complianceRisk, consol, transport) = Factory.CreateNewComplianceRiskWithConsol();
			consol.JK_OA_SendingForwarderAddress = orgSendingForwarder.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = orgReceivingForwarder.MainAddress.PK;

			var complianceBizO = new ComplianceRiskBusinessObject(consol, complianceRisk);
			complianceRisk.COR_JobEndDate = complianceBizO.ComplianceItemRiskStatusProvider.JobTime.JobEndDate.UtcToDateTimeOffset();

			Factory.Save();

			var governorMock = new Mock<IServiceManagerGovernor>();
			using (ObjectFactory.Substitute(governorMock.Object))
			using (Env.Instance.TemporaryServiceTaskContext(ComplianceJobEntityCacheServiceTask.Code, canRunInAnyBranch: true))
			{
				var serviceTask = new ComplianceJobEntityCacheServiceTask();
				serviceTask.ServiceLogger = new TestServiceLogger();
				serviceTask.RunTask(CancellationToken.None);

				var entityCache = Factory.LoadComplianceJobEntityCache(complianceRisk);

				CombineAssertions("The entity cache is saved in the database", () =>
				{
					AssertEquals("No Errors", string.Empty, ErrorReporter.LastMessageReported);
					AssertEquals("Entity cache count 2", 2, entityCache.Length);
					AssertEquals(@$"Sending Forwarder: {orgSendingForwarder.PK}", true, Factory.IsEntityCacheInDatabase(entityCache, orgSendingForwarder));
					AssertEquals(@$"Receiving Forwarder: {orgReceivingForwarder.PK}", true, Factory.IsEntityCacheInDatabase(entityCache, orgReceivingForwarder));
					AssertDeactivateServiceTask(governorMock);
				});
			}
		}

		public void TestShipmentWithDeclarationEntityCache()
		{
			var refVessel = Factory.LoadTop1<RefVessel>(new ZQuery());

			var (orgConsignor, orgForwarder) = Factory.CreateNewOrgConsignorAndConsignee();
			orgForwarder.OH_IsForwarder = true;

			var (complianceRisk, shipment) = Factory.CreateNewComplianceRiskWithShipment();
			shipment.ConsignorPK = orgConsignor.PK;

			var transport = shipment.AddTransportLeg("AUSYD", "SGSIN", "FREE TEXT VESSEL");

			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_JS] = shipment.PK;
			declaration[JobDeclarationSchema.JE_RL_NKOrigin] = shipment.JS_RL_NKOrigin;
			declaration[JobDeclarationSchema.JE_RL_NKFinalDestination] = shipment.JS_RL_NKDestination;
			declaration[JobDeclarationSchema.JE_TransportMode] = shipment.JS_TransportMode;
			declaration[JobDeclarationSchema.JE_VesselName] = refVessel.RV_Code;

			var complianceBizO = new ComplianceRiskBusinessObject(shipment, complianceRisk);
			complianceRisk.COR_JobEndDate = complianceBizO.ComplianceItemRiskStatusProvider.JobTime.JobEndDate.UtcToDateTimeOffset();

			Factory.Save();

			var governorMock = new Mock<IServiceManagerGovernor>();
			using (ObjectFactory.Substitute(governorMock.Object))
			using (Env.Instance.TemporaryServiceTaskContext(ComplianceJobEntityCacheServiceTask.Code, canRunInAnyBranch: true))
			{
				var serviceTask = new ComplianceJobEntityCacheServiceTask();
				serviceTask.ServiceLogger = new TestServiceLogger();
				serviceTask.RunTask(CancellationToken.None);

				var entityCache = Factory.LoadComplianceJobEntityCache(complianceRisk);

				CombineAssertions("The entity cache is saved in the database", () =>
				{
					AssertEquals("No Errors", string.Empty, ErrorReporter.LastMessageReported);
					AssertEquals("Entity cache count 3", 3, entityCache.Length);
					Assert(@$"Free Text Vessel: {transport.PK}", Factory.IsEntityCacheInDatabase(entityCache, transport));
					Assert(@$"Consignor: {orgConsignor.PK}", Factory.IsEntityCacheInDatabase(entityCache, orgConsignor));
					Assert(@$"Vessel: {refVessel.PK}", Factory.IsEntityCacheInDatabase(entityCache, refVessel));
					AssertDeactivateServiceTask(governorMock);
				});
			}
		}

		public void TestBookingWithQuoteEntityCache()
		{
			var (orgConsignor, orgConsignee) = Factory.CreateNewOrgConsignorAndConsignee();
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);

			var shipment = quotedBooking.Booking;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.ConsigneePK = orgConsignee.PK;
			shipment.ConsignorPK = orgConsignor.PK;

			var complianceRisk = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRisk.COR_ParentID = shipment.JS_TH_OneTimeQuote;
			complianceRisk.COR_ParentTableCode = RatingHeaderSchema.Constants.Prefix;

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("IsBooking", true, shipment.JS_IsBooking);
				AssertEquals("IsForwardRegistered", false, shipment.JS_IsForwardRegistered);
			});

			var complianceBizO = new ComplianceRiskBusinessObject(quotedBooking, complianceRisk);
			complianceRisk.COR_JobEndDate = complianceBizO.ComplianceItemRiskStatusProvider.JobTime.JobEndDate.UtcToDateTimeOffset();

			Factory.Save();

			var governorMock = new Mock<IServiceManagerGovernor>();
			using (ObjectFactory.Substitute(governorMock.Object))
			using (Env.Instance.TemporaryServiceTaskContext(ComplianceJobEntityCacheServiceTask.Code, canRunInAnyBranch: true))
			{
				var serviceTask = new ComplianceJobEntityCacheServiceTask();
				serviceTask.ServiceLogger = new TestServiceLogger();
				serviceTask.RunTask(CancellationToken.None);

				var entityCache = Factory.LoadComplianceJobEntityCache(complianceRisk);

				CombineAssertions("The entity cache is saved in the database", () =>
				{
					AssertEquals("No Errors", string.Empty, ErrorReporter.LastMessageReported);
					AssertEquals("Entity cache count 2", 2, entityCache.Length);
					AssertEquals(@$"Consignor: {orgConsignor.PK}", true, Factory.IsEntityCacheInDatabase(entityCache, orgConsignor));
					AssertEquals(@$"Consignee: {orgConsignee.PK}", true, Factory.IsEntityCacheInDatabase(entityCache, orgConsignee));
					AssertDeactivateServiceTask(governorMock);
				});
			}
		}

		public void TestHostedServiceNudgedIsApplied()
		{
			using (MockProductRegistration())
			{
				AssertEquals(true, ComplianceJobEntityCacheServiceTask.CheckIsNudged());
			}

			using (MockProductRegistration())
			using (OrganisationsDataRegistry.Instance.ComplianceWiseFeatureDevelopment
				.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.GetJobEntitiesCaching(false)))
			{
				AssertEquals(false, ComplianceJobEntityCacheServiceTask.CheckIsNudged());
			}

			static IDisposable MockProductRegistration()
			{
				var prodKeyMock = new Mock<IProductRegistrationKey>();
				var productRegistrationMock = new Mock<IProductRegistration>();
				productRegistrationMock.Setup(m => m.IsWiseTechGlobalInternalSystem()).Returns(true);
				productRegistrationMock.Setup(m => m.Key).Returns(prodKeyMock.Object);
				prodKeyMock.Setup(m => m.DatabaseType).Returns(DatabaseTypes.Codes.Training);
				prodKeyMock.Setup(m => m.HostedLocation).Returns("SYD");

				return ObjectFactory.Substitute(productRegistrationMock.Object);
			}
		}

		void AssertDeactivateServiceTask(Mock<IServiceManagerGovernor> governorMock)
		{
			AssertNoExceptionThrown(() => governorMock.Verify(g => g.SetServiceTaskIsActive("JEC", false), Times.Once));
		}

		IDisposable complianceWiseFeatureDevelopment;
		protected override void SetUpCore()
		{
			base.SetUpCore();
			complianceWiseFeatureDevelopment = OrganisationsDataRegistry.Instance.ComplianceWiseFeatureDevelopment
				.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.GetJobEntitiesCaching(true));
		}

		protected override void TearDownCore()
		{
			base.TearDownCore();
			complianceWiseFeatureDevelopment?.Dispose();
		}
	}
}
