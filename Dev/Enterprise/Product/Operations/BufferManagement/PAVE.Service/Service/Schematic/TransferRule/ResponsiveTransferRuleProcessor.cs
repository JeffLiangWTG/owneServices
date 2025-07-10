using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.BufferManagement.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Service
{
	public class ResponsiveTransferRuleProcessor
	{
		internal ResponsiveTransferRuleProcessor(ILogger logger)
		{
			Logger = logger;
		}

		#region Logger

		ILogger Logger { get; }

		void LogInformation(string message)
		{
			Logger?.Information(FormattableString.Invariant($"{message} using {nameof(ResponsiveTransferRuleProcessor)}")); // Service Task Logging
		}

		#endregion

		#region Process

		ResponsiveTransferRuleRunner GetNewResponsiveTransferRuleRunner(BMSystem system, ILogger logger, IReadOnlyCollection<Guid> transferablesPKs)
		{
			var responsiveLogger = new ResponsiveTransferRuleRunnerLogger(logger, system);
			var dataAccessor = new ResponsiveTransferRuleRunnerDataAccessor(responsiveLogger, transferablesPKs);

			return GetNewResponsiveTransferRuleRunnerCore(system, dataAccessor, responsiveLogger, transferablesPKs);
		}

		protected virtual ResponsiveTransferRuleRunner GetNewResponsiveTransferRuleRunnerCore(
			IPAVESystem system,
			ITransferRuleRunnerDataAccessor dataAccessor,
			ITransferRuleRunnerLogger logger,
			IReadOnlyCollection<Guid> transferablesPKs)
		{
			return new ResponsiveTransferRuleRunner(system, dataAccessor, logger, transferablesPKs);
		}

		internal void ProcessTransferables(IReadOnlyCollection<Guid> transferablePKs)
		{
			LogInformation((NoResString)"Started processing transfer rules"); // Service Task Logging

			try
			{
				using (ProcessTask.Loader.SuppressTemplateApplication())
				{
					ProcessTransferablesCore(transferablePKs);
				}
			}
			finally
			{
				LogInformation((NoResString)"Finished processing transfer rules"); // Service Task Logging
			}
		}

		protected virtual void ProcessTransferablesCore(IReadOnlyCollection<Guid> transferablePKs)
		{
			var systemsToProcess = GetLiveSystems(transferablePKs);

			foreach (var system in systemsToProcess)
			{
				GetNewResponsiveTransferRuleRunner(system, Logger, transferablePKs).Process(new CancellationToken());
			}
		}

		#endregion

		#region Repository Helpers

		static ReadOnlyBusinessObjectFactory GetNewFactory(string callerName)
		{
			return new ReadOnlyBusinessObjectFactory { NameForDebugging = nameof(ResponsiveTransferRuleProcessor) + " GetNewFactory for: " + callerName, RefreshEnabled = false };
		}

		static IReadOnlyCollection<BMSystem> GetLiveSystems(IReadOnlyCollection<Guid> transferablePKs)
		{
			var systemQuery = new ZDBOnlyQuery(typeof(BMSystem));
			systemQuery.AddToFilter(BMSystemSchema.FS_IsLive, true);

			var componentSubquery = new ZDBOnlySubQuery(typeof(BMComponent), BMComponentSchema.FC_FS_System);
			componentSubquery.AddToFilter(BMComponentSchema.FC_IsActive, true);

			var processHeaderSubquery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.FH_FC_CurrentComponent);
			processHeaderSubquery.AddToFilter(ProcessHeaderSchema.FH_IsActive, true);
			processHeaderSubquery.AddToFilter(ProcessHeaderSchema.FH_FH_ParentHeader, SQLComparisonOperator.NotEqual, null);

			var processHeaderPKsQueryFilter = new ZQuery(ProcessHeaderSchema.PK, transferablePKs) { AllowTableValuedParameters = true };
			processHeaderSubquery.AddToFilter(processHeaderPKsQueryFilter);

			componentSubquery.AddSubQuery(BMComponentSchema.PK, processHeaderSubquery, JoinCondition.And);
			systemQuery.AddSubQuery(BMSystemSchema.PK, componentSubquery, JoinCondition.And);

			systemQuery.ReLoadExistingRows = true; // ignores Uber Factory cache

			var factory = GetNewFactory(nameof(GetLiveSystems));
			return factory.Load<BMSystem>(systemQuery);
		}

		#endregion
	}
}
