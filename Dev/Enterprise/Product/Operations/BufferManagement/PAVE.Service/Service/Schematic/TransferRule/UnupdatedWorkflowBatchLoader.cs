using System;
using System.Globalization;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Service
{
	internal class UnupdatedWorkflowBatchLoader : WorkflowBatchLoader<ResponsiveTransferRuleRunnerDataAccessor>
	{
		public UnupdatedWorkflowBatchLoader(ResponsiveTransferRuleRunnerDataAccessor dataAccessor, ILogger logger, Func<(ZQuery query, string queryName)> queryProvider)
			: base(dataAccessor, queryProvider, shouldUseSecondaryServerIfAllowed: false)
		{
			this.logger = logger;
		}

		readonly ILogger logger;

		protected override BatchLogger GetBatchLoggerCore()
		{
			return new BatchLogger(logger, (NoResString)"Resetting Dedicated Buffer for workflows no longer eligible for release"); // This is used in service task logging
		}

		protected override int GetWorkflowBatchSizeCore() => FactoryProvider.GetWorkflowBatchSize();

		protected override string GetEmailIntroForNotificationGroupAboutSQLException()
		{
			var emailIntro = Res.GetString("DF82DF9C-417B-4661-A58A-8C508637E251", @"A SQL exception has occurred when trying to reset Dedicated Buffer for workflows no longer eligible for release");
			return emailIntro;
		}

		protected override string GetLogHeaderForNotificationGroupAboutSQLException()
		{
			var logHeader = string.Format(CultureInfo.InvariantCulture, (NoResString)@"A SQL exception has occurred when trying to reset Dedicated Buffer for workflows no longer eligible for release"); // Logs should not be translated
			return logHeader;
		}
	}
}
