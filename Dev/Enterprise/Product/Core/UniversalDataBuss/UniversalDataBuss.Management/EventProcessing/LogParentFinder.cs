using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Event = Enterprise.ZArchitecture.Business.Event;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.UniversalDataBuss.Management.EventProcessing
{
	static class LogParentFinder
	{
		internal static bool FindInterestedLogParents(UniversalObjectFactory factory, UniversalEvent eventDataObject, Visitor logParentFinderVisitor, XmlSessionTracker logger, IEDIMessage message, Event eventType)
		{
			// Feel free to replace this with some more useful return value (as opposed to mutating even more inputs)
			var anyLogFound = false;

			var contextManagers = DataContextManagersFactory.All.OfType<IEventDataContextManager>().Where(c => c.ManagesEvents).ToList();
			var groupedContextCoordinator = contextManagers.OfType<IDataContextCoordinator>()
				.GroupBy(c => c.GetUniqueContextIdentifier(eventDataObject))
				.Where(c => !string.IsNullOrEmpty(c.Key));
			foreach (var contextManager in contextManagers.Except(groupedContextCoordinator.SelectMany(c => c.OfType<IEventDataContextManager>())))
			{
				anyLogFound |= FindLog(contextManager, message, eventDataObject, factory, logger, eventType, logParentFinderVisitor);
			}
			foreach (var contextManagerGroup in groupedContextCoordinator)
			{
				var contextManager = FindContextManagerWithSingleLogParent(contextManagerGroup.OfType<IEventDataContextManager>(), factory, eventDataObject, logger, contextManagerGroup.Key);
				if (contextManager != null)
				{
					anyLogFound |= FindLog(contextManager, message, eventDataObject, factory, logger, eventType, logParentFinderVisitor);
				}
			}

			return anyLogFound;
		}

		static IEventDataContextManager FindContextManagerWithSingleLogParent(IEnumerable<IEventDataContextManager> eventDataContextManagers, UniversalObjectFactory factory, UniversalEvent eventDataObject, XmlSessionTracker logger, string identifier)
		{
			IEventDataContextManager lastContextManagerWithLogsFound = null;
			var findContextManagerLog = new XmlSessionTracker(new SimpleLogger());

			foreach (var contextManager in eventDataContextManagers)
			{
				var logParents = contextManager.GetLogParentsForEvent(eventDataObject, factory.BOFactory, findContextManagerLog);

				if (logParents != null && logParents.Any())
				{
					if (lastContextManagerWithLogsFound != null)
					{
						logger.Log(LogType.Warning, "Multiple records found. Message import failed.");
						return null;
					}

					lastContextManagerWithLogsFound = contextManager;
				}
			}

			if (lastContextManagerWithLogsFound == null)
			{
				logger.Log(LogType.Information, findContextManagerLog.ToString());
			}
			return lastContextManagerWithLogsFound;
		}

		static bool FindLog(IEventDataContextManager contextManager, IEDIMessage message, UniversalEvent eventDataObject,
			UniversalObjectFactory factory, XmlSessionTracker logger, Event eventType, Visitor logParentFinderVisitor)
		{
			var logsFound = false;
			try
			{
				logger.IndividualImportBegin(contextManager.DataContextType);
				logsFound = logParentFinderVisitor.FindLogs(message, contextManager, eventDataObject, factory, logger, eventType);
				logger.IndividualImportEnd(reportLogAsAResult: logsFound);
			}
			catch (DataObjectReadFailureException exception)
			{
				logger.LogBoth(LogType.Error, exception.Message);
				logger.IndividualImportEnd();
			}
			catch (MessageProcessingBusinessFailureException businessException)
			{
				logger.Log(LogType.Error, businessException.Message);
				logger.IndividualImportEnd();
			}
			return logsFound;
		}

		internal abstract class Visitor
		{
			internal abstract bool FindLogs(IEDIMessage message, IEventDataContextManager contextManager, UniversalEvent eventDataObject, UniversalObjectFactory factory, XmlSessionTracker logger, Event eventType);
		}
	}
}
