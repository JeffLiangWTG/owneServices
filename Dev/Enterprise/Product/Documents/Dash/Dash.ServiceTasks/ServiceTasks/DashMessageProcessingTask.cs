using System.Threading;
using Enterprise.Dash.ServiceTasks.MessageProcessors;
using Enterprise.DocumentScanning.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Dash.ServiceTasks.ServiceTasks
{
	public abstract class DashMessageProcessingTask : ServiceProviderImpl
	{
#pragma warning disable CA1725 // Parameter names should match base declaration
		public override void RunTask(CancellationToken token)
#pragma warning restore CA1725 // Parameter names should match base declaration
		{
			if (!EDocsParsingHelper.IsDocumentParsingEnabled())
			{
				ServiceLogger.Warning((NoResString)"Failed to run service task. Please check if registry for at least one of the parsing types is enabled at System -> DocManager -> Document Ingestion -> Parse Types.");
				return;
			}

			ServiceLogger.Information((NoResString)"Task was started successfully.");

			DashMessageProcessor.ProcessMessages(token);

			ServiceLogger.Information((NoResString)"Task completed");
		}

		protected abstract DashMessageProcessor DashMessageProcessor { get; }
	}
}
