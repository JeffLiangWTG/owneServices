using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.UniversalDataBuss.Management.Testing
{
	class LoadJobHelperTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestGetLoadedJobFromImportResults_HandlesEmptyContextCollection()
		{
			var universalEvent = new UniversalEvent
			{
				DataContext = DataContextFactory.New(),
				EventType = Events.DataImportFailureCode
			};

			universalEvent.DataContext.AddDataSource(DataContextType.ForwardingShipment, "");

			var result = PublishToUniversalResult.New(new[] { universalEvent }, DataContextType.ForwardingShipment, "");

			AssertNull(result.FindJobIfExists());
			AssertEquals("", result.ErrorMessage);
			AssertEquals(UniversalResult.External, result.ResultType);
		}

		public void TestGetLoadedJobFromImportResults_DoesNotThrowsExceptionIfCannotLoadASuccessfulImport()
		{
			var universalEvent = new UniversalEvent
			{
				DataContext = DataContextFactory.New(),
				EventType = Events.DataImportCode
			};

			universalEvent.DataContext.AddDataSource(DataContextType.ForwardingShipment, "");
			universalEvent.DataContext.SetCompanyAndDataProviderDetails(NewFactory().Load<IGlbCompany>(Env.CurrentCompany.PK));
			var result = PublishToUniversalResult.New(new[] { universalEvent }, DataContextType.ForwardingShipment, "");
			AssertNull(result.FindJobIfExists());
		}

		public void TestGetLoadedJobFromImportResults_WithSuccessfulExport()
		{
			var universalEvent = new UniversalEvent
			{
				EventType = Events.DataExportCode,
				ContextCollection = new List<Context>() { new Context { Type = nameof(UniversalEvent.ContextTypes.FailureReason), Value = "Successfully Exported" } }
			};

			var result = PublishToUniversalResult.New(new[] { universalEvent }, DataContextType.ForwardingConsol, "");
			AssertNull(result.FindJobIfExists());
			AssertEquals("", result.ErrorMessage);
			AssertEquals(UniversalResult.External, result.ResultType);
		}

		public void TestGetLoadedJobFromImportResults_WithFailedExport()
		{
			var universalEvent = new UniversalEvent
			{
				EventType = Events.DataExportFailureCode,
				ContextCollection = new List<Context>() { new Context { Type = nameof(UniversalEvent.ContextTypes.FailureReason), Value = "Could Not Export" } }
			};

			var result = PublishToUniversalResult.New(new[] { universalEvent }, DataContextType.ForwardingConsol, "");
			AssertNull(result.FindJobIfExists());
			AssertEquals("Could Not Export", result.ErrorMessage);
			AssertEquals(UniversalResult.HadErrors, result.ResultType);
		}

		public void TestGetLoadedJobFromImportResults_WithSuccessfulImport()
		{
			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataTarget(DataContextType.ForwardingShipment, null);

			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			var message = GetQueuedUniversalShipmentMessage(string.Empty);
			var sessionTracker = manager.Process(message, universalShipment);
			var attemptedImports = sessionTracker.ImportResults;

			var universalEvents = attemptedImports.Select(GetNewUniversalEvent).ToArray();
			AssertNull(PublishToUniversalResult.New(universalEvents, DataContextType.ForwardingConsol, ""));

			var shipment = (BusinessObject)NewFactory().LoadTop1<Forwarding.IForwardingShipment>(new ZQuery());
			var resultWithLoadedJob = PublishToUniversalResult.New(universalEvents, DataContextType.ForwardingShipment, "");
			var job = resultWithLoadedJob.FindJobIfExists();
			AssertNotNull(job);
			AssertEquals(shipment.PK, job.PK);
			AssertEquals("", resultWithLoadedJob.ErrorMessage);
			AssertEquals(UniversalResult.Internal, resultWithLoadedJob.ResultType);
		}

		public void TestGetLoadedJobFromImportResults_WithFailedImport()
		{
			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.WarehouseOrder, null);
			universalShipment.DataContext.SetWorkflowInfo(new WorkflowInfo()
			{
				ActionPurpose = null,
				EventBranch = null,
				EventDepartment = null,
				EventType = null,
				EventUser = null,
				RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.FOR } },
				TriggerCount = 0,
				TriggerDate = ZDateTimeOffset.Empty,
				TriggerDescription = "",
				TriggerReference = "",
				TriggerType = TriggerType.Manual
			});
			universalShipment.DataContext.CodesMappedToTarget = true;

			var localClient = NewFactory().Load<IOrgHeader>(Env.CurrentCompany.OrganisationPK);
			universalShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() { new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{ OrganizationCode = localClient.OH_Code, Address1 = localClient.Address1, Address2 = localClient.Address2, AddressType = "LocalClient" } });

			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			var message = GetQueuedUniversalShipmentMessage(string.Empty);
			var sessionTracker = manager.Process(message, universalShipment);
			var attemptedImports = sessionTracker.ImportResults;
			var universalEvents = new[]
			{
				new UniversalEvent
				{
					EventType = Events.DataImportFailureCode,
					ContextCollection = new List<Context>() { new Context { Type = nameof(UniversalEvent.ContextTypes.FailureReason), Value = "Could Not Import" } }
				},
				new UniversalEvent
				{
					EventType = Events.DataExportCode,
					ContextCollection = new List<Context>() { new Context { Type = nameof(UniversalEvent.ContextTypes.FailureReason), Value = "Export Successful" } }
				},
				new UniversalEvent
				{
					EventType = Events.DataExportFailureCode,
					ContextCollection = new List<Context>() { new Context { Type = nameof(UniversalEvent.ContextTypes.FailureReason), Value = "Coule Not Export" } }
				},
			}.Concat(attemptedImports.Select(GetNewUniversalEvent)).ToArray();

			var result = PublishToUniversalResult.New(universalEvents, DataContextType.ForwardingShipment, "");
			AssertNull(result.FindJobIfExists());

			AssertMultilineASCIIEquals("result.ErrorLog", @"Could Not Import".Trim(), result.ErrorMessage);
			AssertEquals(UniversalResult.HadErrors, result.ResultType);
		}

		public void TestFailureResultShowErrorOnly()
		{
			var logger = new ServiceTaskLogForTesting();
			logger.Log("Information - Could Not Import");
			logger.LogError("What's this?");
			logger.LogWarning("Warning Warning Warning");
			logger.LogError("Why error again?");
			logger.Log("Extra detail");
			logger.Log("Information - again");
			var contextCollection = new List<Context>()
			{
				Context.GetContextFromLogs(nameof(UniversalEvent.ContextTypes.FailureReason), logger.Logs)
			};

			var universalEvents = new[]
			{
				new UniversalEvent
				{
					EventType = Events.DataImportFailureCode,
					ContextCollection = contextCollection
				}
			};

			var result = PublishToUniversalResult.New(universalEvents, DataContextType.ForwardingShipment, "");
			AssertMultilineASCIIEquals("result.ErrorMessage", @"Error - What's this?
Error - Why error again?", result.ErrorMessage);

			result = PublishToUniversalResult.New(universalEvents, DataContextType.ForwardingShipment, "", false);
			AssertMultilineASCIIEquals("result.ErrorMessage", @"Information - Could Not Import
Error - What's this?
Warning - Warning Warning Warning
Error - Why error again?
Extra detail
Information - again", result.ErrorMessage);
		}

		UniversalEvent GetNewUniversalEvent(IImportResult importResult)
		{
			var universalEvent = new UniversalEvent
			{
				EventType = importResult.WasSuccessful ? Events.DataImportCode : Events.DataImportFailureCode,
				DataContext = DataContextFactory.New(),
				ContextCollection = new List<Context>()
				{
					new Context
					{
						Type = nameof(UniversalEvent.ContextTypes.FailureReason),
						Value = string.Join("\r\n", importResult.Logs.Select(log => log.Type == LogType.Information ?
							log.Message :
							string.Join("\r\n", log.Message.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(messageLine => log.Type.ToString() + " - " + messageLine)
							)))
					}
				}
			};

			universalEvent.DataContext.SetCompanyAndDataProviderDetails(NewFactory().Load<IGlbCompany>(Env.CurrentCompany.PK));
			try
			{
				universalEvent.DataContext.AddDataSource(importResult.DataContextType, importResult.DataContextKey);
			}
			catch (InvalidOperationException)
			{
			}

			return universalEvent;
		}
	}
}
