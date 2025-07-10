using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.TriggerActionWithOptionalFactorySave
{
	[Serializable]
	public class TriggerActionWithOptionalFactorySaveLogSubscriber : LogSubscriberWithOptionalFactorySave
	{
		public override string Name => "AccTriggerWithOptSaveLogSubscriber";

		public override string[] EventTypes => new[] { Events.MiscellaneousEventCode };

		public override string[] TableNames => new[] { StmEventSchema.Constants.TableName }; //This is just a table that will never be parent of a log. Empty list is not allowed and we do not process any particular table. We assign this subscriber directly to StmJobQueue record we create.

		public override PrettyPrinter GetPrettyPrinter()
		{
			var displayName = Name;

			if (!string.IsNullOrEmpty(CachedTriggerAction))
			{
				displayName += $" ({CachedTriggerAction})";
			}

			return new PrettyPrinter(displayName);
		}

		class LogBatcher : LogBatcher<ZGuid>
		{
			protected override void AddGroupingFetchHints(IEnumerable<IQueuedLog> enumberable) { }
			protected override ZGuid GetGroupLogKey(IQueuedLog log) => log.PK;
			protected override LogsGroupContext SetContextForLogsGroup(ZGuid groupKey, IEnumerable<IQueuedLog> queuedLogs)
			{
				var queuedLog = queuedLogs.First();
				var user = queuedLog.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, queuedLog.SJ_GS_NKUser);
				var branch = queuedLog.Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, queuedLog.SJ_GB_NKBranch);
				var department = queuedLog.Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, queuedLog.SJ_GE_NKDepartment);
				if (user != null && branch != null && department != null)
				{
					return new LogsGroupContext(false, Env.SetTemporaryUserContext(user.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid()));
				}
				else
				{
					var errorMessage = $"Invalid user, branch or department values. User: {user?.GS_Code ?? "null"}, log user: {queuedLog.SJ_GS_NKUser}. Branch: {branch?.GB_Code ?? "null"}, log branch {queuedLog.SJ_GB_NKBranch}. Department: {department?.GE_Code ?? "null"}, log department {queuedLog.SJ_GE_NKDepartment}.";
					ErrorReporter.ReportOnce(errorMessage);
					throw new InvalidOperationException(errorMessage);
				}
			}
		}

		protected override ILogBatcher GetLogBatcher() => new LogBatcher();

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			var notifications = GetINotificationsWrapperAroundILogger();
			var queuedLog = queuedLogs.First();
			var referenceMap = StmALog.GetParametersFromReference(queuedLog.SJ_Reference);
			referenceMap.TryGetValue(JobQueueReferenceParameters.TriggerAction, out var triggerAction);
			CachedTriggerAction = triggerAction;
			if (string.IsNullOrEmpty(triggerAction))
			{
				throw new InvalidOperationException($"{nameof(triggerAction)} is invalid.");
			}

			var provider = QueuedLogReferenceProviderFactory.GetReferenceProvider(triggerAction, DefaultLogger);
			var queuedLogParameters = provider.GetParametersFromReferenceMap(referenceMap, queuedLog);

			var processor = GetProcessorInstance(triggerAction, queuedLogParameters);
			processor?.Process(notifications, CancellationToken.None);
		}

		static IProcessor GetProcessorInstance(string triggerAction, QueuedLogParameters parameters)
		{
			switch (triggerAction)
			{
				case WorkflowTriggerActionTypeConstants.Codes.CreateProfitShareCharges:
					return ObjectFactory.Get<IProfitShareChargeWorkflowProcessorCreator>().CreateProfitShareChargeWorkflowProcessor(parameters.WorkflowProvider);

				case WorkflowTriggerActionTypeConstants.Codes.PostAllCosts:
					return ObjectFactory.Get<ICostPosterCreator>().CreateCostPoster(parameters.WorkflowProvider);

				case WorkflowTriggerActionTypeConstants.Codes.PostAllRevenue:
					return ObjectFactory.Get<IRevenuePosterCreator>().CreateRevenuePoster(parameters.WorkflowProvider);

				case WorkflowTriggerActionTypeConstants.Codes.PostConsolCostOnly:
					return ObjectFactory.Get<IConsolCostOnlyPosterCreator>().CreateConsolCostOnlyPoster(parameters.WorkflowProvider);

				case WorkflowTriggerActionTypeConstants.Codes.PostLocalSisterCompanyChargesOnly:
					return ObjectFactory.Get<ILocalSisterCompanyChargePosterCreator>().CreateLocalSisterCompanyChargePoster(parameters.WorkflowProvider);

				case WorkflowTriggerActionTypeConstants.Codes.RecognizeRevenue:
					return ObjectFactory.Get<IRevenueRecognizerCreator>().CreateRevenueRecognizer(parameters.WorkflowProvider);

				case WorkflowTriggerActionTypeConstants.Codes.PostOverseasAgentCharges:
					return ObjectFactory.Get<IPostOverseasAgentChargesProcessorCreator>().CreateOverseasAgentChargesPoster(parameters.WorkflowProvider);

				case WorkflowTriggerActionTypeConstants.Codes.CreateJobInvoiceHeader:
					return ObjectFactory.Get<IJobInvoiceHeaderProcessorCreator>().CreateJobInvoiceHeaderProcessor(parameters.WorkflowProvider);

				case WorkflowTriggerActionTypeConstants.Codes.PostAllSisterCompanyCharges:
					return ObjectFactory.Get<ISisterCompanyChargePosterCreator>().CreateSisterCompanyChargePoster(parameters.WorkflowProvider);

				case WorkflowTriggerActionTypeConstants.Codes.IncludeChargeInProfitShare:
					return ObjectFactory.Get<IIncludeChargeInProfitShareProcessorCreator>().CreateIncludeChargeInProfitShareProcessor(parameters.WorkflowProvider);

				case WorkflowTriggerActionTypeConstants.Codes.TransactionAllocationAndPost:
					return ObjectFactory.Get<ITransactionAllocationAndPosterCreator>().CreateTransactionAllocationAndPoster(parameters.WorkflowProvider);

				case WorkflowTriggerActionTypeConstants.Codes.ImportAPInvoicesFromOtherCompanies:
					return ObjectFactory.Get<IOtherCompanyAPInvoiceImportCreator>().CreateOtherCompanyAPInvoiceImport(parameters.WorkflowProvider, parameters.CompanyPK);

				default:
					return null;
			}
		}

		string CachedTriggerAction;

#if DEBUG
		public static IProcessor GetProcessorInstance_ForTestOnly(string triggerAction, QueuedLogParameters parameters) => GetProcessorInstance(triggerAction, parameters);
#endif
	}
}
