using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.UniversalDataBuss.Management
{
	public abstract class EventParentFinder
	{
		protected EventParentFinder(BusinessObjectFactory factory, IEventDataContextManager manager, IXmlImportLogger logger)
		{
			this.factory = Argument.NotNull(factory, "BusinessObjectFactory factory");
			this.manager = Argument.NotNull(manager, "IEventDataContextManager manager");
			this.logger = Argument.NotNull(logger, "IXmlImportLogger logger");
		}
		protected readonly BusinessObjectFactory factory;
		protected readonly IEventDataContextManager manager;
		protected readonly IXmlImportLogger logger;

		internal IKeysResult GetLogParentKeysForEvent(IXmlEventValueObject xmlEvent) => GetLogParentKeysForEventCore(xmlEvent);

		protected virtual IKeysResult GetLogParentKeysForEventCore(IXmlEventValueObject xmlEvent)
		{
			var result = GetLogParentsForEvent(xmlEvent);
			const int TooManyMatches = 100;
			if (result == null)
			{
				return KeysResult.Match(Enumerable.Empty<(string KeyValue, string KeySource)>());
			}
			else if (result.Length > TooManyMatches)
			{
				return KeysResult.NoMatch();
			}
			else
			{
				return KeysResult.Match(
					result.Where(s => s != null)
					.Select(s => (Value: s.PK.ToStringKey(), TypeName: s.GetType().Name))
					.Where(pair => !string.IsNullOrEmpty(pair.Value))
					.Select(pair => (pair.Value, $"{pair.TypeName}/PK")));
			}
		}

		public BusinessObject[] GetLogParentsForEvent(IXmlEventValueObject xmlEvent)
		{
			var universalEvent = (UniversalEvent)xmlEvent;
			var dataTarget = universalEvent.GetMatchingDataTarget(manager.DataContextType);
			if (dataTarget != null
				|| universalEvent.DataContext == null
				|| universalEvent.DataContext.DataTargetCollection == null
				|| !universalEvent.DataContext.DataTargetCollection.Any())
			{
				return GetLogParentsFromJobNumberAndLogIfFound(universalEvent, dataTarget)
					?? GetLogParentsForEventUsingContextAndLogIfFound(universalEvent);
			}
			return null;
		}

		public BusinessObject[] GetLogParentsFromJobNumberAndLogIfFound(UniversalEvent topLevelDataObject, IDataTargetDataObject dataTarget)
		{
			if (dataTarget != null)
			{
				var logParents = manager.LoadBusinessObjectsFromDataTarget(topLevelDataObject, dataTarget, factory, logger);
				if (logParents != null && logParents.Length > 0)
				{
					return GetChildrenIfSpecifiedInContext(logParents, topLevelDataObject);
				}
			}

			return null;
		}

		protected virtual BusinessObject[] GetChildrenIfSpecifiedInContext(BusinessObject[] logParents, UniversalEvent eventData)
		{
			return logParents;
		}

		BusinessObject[] GetLogParentsForEventUsingContextAndLogIfFound(UniversalEvent xmlEvent)
		{
			var result = GetLogParentsForEventUsingContext(xmlEvent);
			if (result != null)
			{
				if (result.Any(obj => obj == null))
				{
					ErrorReporter.ReportOnce(FormattableString.Invariant($"The EventParentFinder {GetType().FullName} returned an enumerable containing null. Please don't return enumerables containing null."));
					result = result.Where(obj => obj != null).ToArray();
				}

				if (result.Length == 1)
				{
					logger.LogVerboseOnly(LogType.Information, Res.GetString("8a70d37c-afe8-4643-971f-ca4b40db11cf", "Found 1 match using Context values.", result.Length.ToString()));
				}
				else if (result.Length > 1)
				{
					logger.LogVerboseOnly(LogType.Information, Res.GetString("a7af4b27-b184-4777-a7e5-d3e63eb9b5d8", "Found {0} matches using Context values.", result.Length.ToString()));
				}
			}
			return result;
		}

		protected abstract BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent);
	}
}
