using System;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Integration;

namespace Enterprise.Client.EDI.ServiceTasks.EventLogs
{
	class EventLogProcessor
	{
		public EventLogProcessor(ILogger logger)
		{
			this.logger = logger;
		}

		public void Process(XElement element, string hostname, EventLogDataTransmissionHandler eventLogDataTransmissionHandler)
		{
			Argument.NotNull(element, "element");
			Argument.NotNullOrEmpty(hostname, "hostname");

			var errorLogs = CreateNewScalableHelpErrorLogCollection();

			var issueCount = 0;
			var invalidEventCount = 0;
			var skippedIssueCount = 0;

			foreach (var inner in element.Elements())
			{
				if (inner.ExtractData(eventLogDataTransmissionHandler))
				{
					eventLogDataTransmissionHandler.UpdateListProvidersAndTheirCapacityRegistryItemCache(inner.Element(XmlConverter.XmlPath + "System"));
					string formattedAsNormalIssueXml = XmlConverter.ConvertToIssueXmlFormat(inner);
					var log = ProcessExceptionXml(formattedAsNormalIssueXml, errorLogs);
					if (log != null)
					{
						log.AfterIssueNumberSet(() => logger.Log(LogType.Information, FormattableString.Invariant($"Issue {log.HE_IssueNumber} logged from machine {hostname}")));
						issueCount++;
					}
				}
			}

			errorLogs.Save();
			// Update the lastest Water mark cache which is the first element of list. (This cache is not related with unwanted-collecting data). Need to call sync at the end of the process.
			eventLogDataTransmissionHandler.UpdateEventLogCache(element.Elements().First(), hostname);

			logger.Log(LogType.Information, FormattableString.Invariant($"{issueCount} issues, {invalidEventCount} invalid events, {skippedIssueCount} skipped from machine {hostname}"));
		}

		internal virtual IHelpErrorLogCollection CreateNewScalableHelpErrorLogCollection()
		{
			var factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;
			return new ScalableHelpErrorLogCollection(factory);
		}

		internal virtual EdiHelpErrorLog ProcessExceptionXml(string formattedAsNormalIssueXml, IHelpErrorLogCollection errorLogs)
		{
			return new ExceptionXml(formattedAsNormalIssueXml).Process(errorLogs, false);
		}

		readonly ILogger logger;
	}
}
