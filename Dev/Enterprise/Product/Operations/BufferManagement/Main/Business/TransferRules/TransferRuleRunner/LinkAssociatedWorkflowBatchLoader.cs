using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class LinkAssociatedWorkflowBatchLoader : WorkflowBatchLoader<TransferRuleRunnerDataAccessor>
	{
		public LinkAssociatedWorkflowBatchLoader(TransferRuleRunnerDataAccessor dataAccessor,
			IComponentLink componentLink,
			ILogger logger,
			Func<(ZQuery query, string queryName)> queryProvider,
			bool shouldUseSecondaryServerIfAllowed
			)
			: base(dataAccessor, queryProvider, shouldUseSecondaryServerIfAllowed)
		{
			Argument.NotNull(componentLink, nameof(componentLink));
			Argument.NotNull(logger, nameof(logger));

			this.componentLink = componentLink;
			Logger = logger;
		}

		readonly IComponentLink componentLink;
		protected ILogger Logger { get; }

		#region Data Access

		protected override FilteredBusinessObjectReaderWithLogger GetWorkflowReader()
		{
			try
			{
				return base.GetWorkflowReader();
			}
			catch (InvalidFilterConfigurationException ex)
			{
				throw new TransferRulesAreInvalidException("Invalid transfer rule configuration", ex);
			}
		}

		#endregion

		#region Batch Size

		protected override int GetWorkflowBatchSizeCore() => FactoryProvider.GetWorkflowBatchSize();

		#endregion

		#region Batch Logging

		protected override BatchLogger GetBatchLoggerCore()
		{
			return new BatchLogger(Logger, string.Format(CultureInfo.InvariantCulture, (NoResString)"Component link [{0}]", componentLink.DisplayText)); // This is used in service task logging
		}

		#endregion

		#region SQL Exception Processing

		protected override string GetEmailIntroForNotificationGroupAboutSQLException()
		{
			var componentLinkDescription = Res.GetString("76796c03-d3c8-4680-81ca-53f994e403e7", "Component link [{0} -> {1}]", componentLink.GetComponentFromName(), componentLink.GetComponentToName());
			var emailIntro = Res.GetString("4efaf82a-6013-4565-afbf-ade9e7fc1864", @"A SQL exception has occurred when trying to process {0}", componentLinkDescription);
			return emailIntro;
		}

		protected override string GetLogHeaderForNotificationGroupAboutSQLException()
		{
			var componentLinkDescriptionEnglish = string.Format(CultureInfo.InvariantCulture, (NoResString)"Component link [{0} -> {1}]", componentLink.GetComponentFromName(), componentLink.GetComponentToName()); // Logs should not be translated
			var logHeader = string.Format(CultureInfo.InvariantCulture, (NoResString)@"A SQL exception has occurred when trying to process {0}", componentLinkDescriptionEnglish); // Logs should not be translated
			return logHeader;
		}

		#endregion

		#region For Tests
#if DEBUG

		protected override void NotifyBatchLoaded_ForTest(ref ITransferrableProcessHeader[] workflowBatch, ref ITransferrableProcessHeader lastProcessHeaderRead)
		{
			FactoryProvider.NotifyBatchLoaded_ForTest(ref workflowBatch, ref lastProcessHeaderRead);
		}

#endif
		#endregion

	}
}
