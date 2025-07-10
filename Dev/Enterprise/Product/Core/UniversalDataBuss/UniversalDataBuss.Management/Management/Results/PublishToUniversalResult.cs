using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.UniversalDataBuss.Management
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public class PublishToUniversalResult
	{
		/// <summary>
		/// Successful export.
		/// </summary>
		PublishToUniversalResult()
			: this(null, null, null, "")
		{ }

		/// <summary>
		/// Successful import.
		/// </summary>
		PublishToUniversalResult(IDataContextDataObject dataContext, Func<BusinessObject> findJobIfExists, Func<IEnumerable<BusinessObject>> findChildJobsIfExists, ZString errorPrefix)
			: this(dataContext, findJobIfExists, findChildJobsIfExists, errorPrefix, "")
		{ }

		/// <summary>
		/// Failure to export/import.
		/// </summary>
		PublishToUniversalResult(ZString errorPrefix, ZString error)
			: this(null, null, null, errorPrefix, error)
		{ }

		PublishToUniversalResult(IDataContextDataObject dataContext, Func<BusinessObject> findJobIfExists, Func<IEnumerable<BusinessObject>> findChildJobsIfExists, ZString errorPrefix, ZString error)
		{
			this.findJobIfExists = findJobIfExists;
			this.findChildJobsIfExists = findChildJobsIfExists;
			ErrorPrefix = errorPrefix;
			Error = error;
			ResultType = CalculateResultType(dataContext, error);
		}

		public BusinessObject FindJobIfExists()
		{
			return findJobIfExists == null ? null : findJobIfExists();
		}
		readonly Func<BusinessObject> findJobIfExists;

		public IEnumerable<BusinessObject> FindChildJobsIfExists()
		{
			return findChildJobsIfExists == null ? Array.Empty<BusinessObject>() : findChildJobsIfExists();
		}
		readonly Func<IEnumerable<BusinessObject>> findChildJobsIfExists;

		public readonly UniversalResult ResultType;
		readonly ZString Error;
		readonly ZString ErrorPrefix;

		#region Static New

		public static PublishToUniversalResult New(IEnumerable<UniversalEvent> publishResultEvents, DataContextType topLevelDataContextType, ZString errorPrefix, bool showOnlyError = true, params DataContextType[] childDataContextTypes)
		{
			PublishToUniversalResult result = null;
			// import data

			var topLevelImportDataEvent = publishResultEvents.FirstOrDefault(e => IsEventType(e, Events.DataImportCode) && e.GetMatchingDataSource(topLevelDataContextType) != null);
			if (topLevelImportDataEvent != null)
			{
				var factory = new BusinessObjectFactory() { NameForDebugging = "PublishToUniversalResult_New" };
				Func<BusinessObject> findJobIfExists = () =>
				{
					return LoadJobFromImportDataEvent(factory, topLevelImportDataEvent, topLevelDataContextType);
				};
				Func<IEnumerable<BusinessObject>> findChildJobsIfExists = null;
				if (childDataContextTypes != null)
				{
					findChildJobsIfExists = new ChildJobsFinder(factory, publishResultEvents, childDataContextTypes, Events.DataImportCode).FindChildJobs;
				}

				result = new PublishToUniversalResult(topLevelImportDataEvent.DataContext, findJobIfExists, findChildJobsIfExists, errorPrefix);
			}

			// import data failed
			var importDataFailureEvent = publishResultEvents.FirstOrDefault(e => IsEventType(e, Events.DataImportFailureCode));
			if (result == null && importDataFailureEvent != null)
			{
				result = new PublishToUniversalResult(errorPrefix, GetErrorLogFromEvent(importDataFailureEvent, showOnlyError));
			}

			// export data
			if (result == null && publishResultEvents.Any(e => IsEventType(e, Events.DataExportCode)))
			{
				result = new PublishToUniversalResult();
			}

			// export data failed
			if (result == null)
			{
				var exportFailureLogs = string.Join("\r\n", publishResultEvents.Where(e => IsEventType(e, Events.DataExportFailureCode)).Select((IXmlEventValueObject resultEvent) => GetErrorLogFromEvent(resultEvent, showOnlyError)));
				result = string.IsNullOrEmpty(exportFailureLogs) ? null : new PublishToUniversalResult(errorPrefix, exportFailureLogs);
			}

			return result;
		}

		class ChildJobsFinder
		{
			public ChildJobsFinder(BusinessObjectFactory factory, IEnumerable<UniversalEvent> publishResultEvents, DataContextType[] childDataContextTypes, ZString eventType)
			{
				this.factory = factory;
				this.publishResultEvents = publishResultEvents;
				this.childDataContextTypes = childDataContextTypes;
				this.eventType = eventType;
			}
			readonly BusinessObjectFactory factory;
			readonly IEnumerable<UniversalEvent> publishResultEvents;
			readonly DataContextType[] childDataContextTypes;
			readonly ZString eventType;

			public IEnumerable<BusinessObject> FindChildJobs()
			{
				foreach (var childDataContextType in childDataContextTypes)
				{
					foreach (var childImportDataEvent in publishResultEvents.Where(e => IsEventType(e, eventType) && e.GetMatchingDataSource(childDataContextType) != null))
					{
						yield return PublishToUniversalResult.LoadJobFromImportDataEvent(factory, childImportDataEvent, childDataContextType);
					}
				}
			}
		}

		#endregion

		#region Static New - Error

		public static PublishToUniversalResult Empty
		{
			get { return empty ?? (empty = new PublishToUniversalResult(ZString.Empty, ZString.Empty)); }
		}
		[ThreadStatic]
		static PublishToUniversalResult empty;

		public static PublishToUniversalResult New(ZString errorPrefix, ZString error)
		{
			if (error.IsEmpty)
			{
				error = Res.GetString("PublishToUniversalResult|UnknownError", "Unknown Error");
			}

			return new PublishToUniversalResult(errorPrefix, error);
		}

		#endregion

		#region LoadJobFromEvent

		static BusinessObject LoadJobFromImportDataEvent(BusinessObjectFactory factory, UniversalEvent resultEvent, DataContextType dataContextType)
		{
			resultEvent.SetCodesMappedToTarget();
			var dataContextManager = dataContextType.GetUniversalDataContextManager();
			var dataSource = resultEvent.GetMatchingDataSource(dataContextType);
			var loadedJob = dataContextManager.LoadBusinessObjectFromDataSource(resultEvent, dataSource, factory, new DummyLogger());
			return loadedJob;
		}

		#endregion

		#region ErrorMessage

		public ZString ErrorMessage
		{
			get { return !Error.IsEmpty ? !ErrorPrefix.IsEmpty ? ZString.Format("{0}{1}{2}", ErrorPrefix, System.Environment.NewLine, Error) : Error : ZString.Empty; }
		}

		#endregion

		#region Flags

		static bool IsEventType(UniversalEvent resultEvent, ZString eventType)
		{
			return resultEvent.EventType.GetValueOrDefault() == eventType;
		}

		#endregion

		#region GetErrorLogFromEvent

		static string GetErrorLogFromEvent(IXmlEventValueObject resultEvent, bool showOnlyError)
		{
			return resultEvent.Context != null ? (showOnlyError ? resultEvent.Context.GetErrorLog() : resultEvent.Context.FailureReason) : null;
		}

		#endregion

		#region CalculateResultType

		static UniversalResult CalculateResultType(IDataContextDataObject dataContext, string error)
		{
			UniversalResult resultType;
			if (!string.IsNullOrEmpty(error))
			{
				resultType = UniversalResult.HadErrors;
			}
			else if (dataContext.IsFromSameSystem())
			{
				resultType = UniversalResult.Internal;
			}
			else
			{
				resultType = UniversalResult.External;
			}

			return resultType;
		}

		#endregion
	}

	public enum UniversalResult
	{
		HadErrors,
		External,
		Internal
	}
}
