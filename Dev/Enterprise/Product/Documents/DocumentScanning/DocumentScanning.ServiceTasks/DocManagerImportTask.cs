using System;
using System.Diagnostics;
using System.Threading;
using System.Xml;
using Enterprise.DocumentScanning.Business;
using Enterprise.Integration;
using Enterprise.MailManager;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService("DMI", "DocManager Import", "DOC", typeof(Enterprise.DocumentScanning.ServiceTasks.DocManagerImportTask),
	IsMandatory = true,
	MinimumPeriod = "1minute",
	AllowsMultipleInstances = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding("DMI", MailDBItemsSchema.Constants.TableName,
	new string[] {
		MailDBItemsSchema.Constants.MI_Direction + "=" + MailDirection.Receive,
		MailDBItemsSchema.Constants.MI_Status + "=" + MailStatus.Queued,
		MailDBItemsSchema.Constants.MI_Subject + " like [ediDocManager%"
	}, null)]

[assembly: HostedServiceBusinessObjectBinding("DMI", MailDBItemsSchema.Constants.TableName,
	new string[] {
		MailDBItemsSchema.Constants.MI_Direction + "=" + MailDirection.Receive,
		MailDBItemsSchema.Constants.MI_Status + "=" + MailStatus.Queued,
		MailDBItemsSchema.Constants.MI_Subject + " like [DocManager%"
	}, null)]
namespace Enterprise.DocumentScanning.ServiceTasks
{
#if DEBUG
	public
#endif
	class DocManagerImportTask : ServiceProviderImpl
	{
		[HostedServiceRequirement]
		public static string CheckBatchProcessorImportsAreAllowed()
		{
			return HostedServiceRequirementAttribute.CheckValueIsNotEqualTo(SystemDataRegistry.Instance.AllowDocManagerBatchProcessorImports, false);
		}

		public override void RunTask(CancellationToken token)
		{
			var importManager = GetNewBatchImportManager();
			importManager.LogProgress += args => ServiceLogger.Log(GetLogTypeFromTraceEventType(args.EventType), args.Message);
			var directoryPath = string.Empty;

			try
			{
				directoryPath = SystemDataRegistry.Instance.DocManagerBatchProcessorImportOptions.Value.DirectoryPath;
			}
			catch (InvalidOperationException ex) when (ex.InnerException is XmlException)
			{
				throw new HostedServiceException(
					$"Import directory is invalid, please check Directory Path in {SystemDataRegistry.Instance.DocManagerBatchProcessorImportOptions.GetLocation()}",
					ex.InnerException);
			}

			importManager.ImportFilesAndEmails(directoryPath, token);
		}

		protected virtual BatchImportManager GetNewBatchImportManager()
		{
			return new BatchImportManager();
		}

		static LogType GetLogTypeFromTraceEventType(TraceEventType eventType)
		{
			LogType logType;
			switch (eventType)
			{
				case TraceEventType.Critical:
				case TraceEventType.Error:
					logType = LogType.Error;
					break;

				case TraceEventType.Warning:
					logType = LogType.Warning;
					break;

				case TraceEventType.Verbose:
					logType = LogType.Debug;
					break;

				default:
					logType = LogType.Information;
					break;
			}

			return logType;
		}
	}
}
