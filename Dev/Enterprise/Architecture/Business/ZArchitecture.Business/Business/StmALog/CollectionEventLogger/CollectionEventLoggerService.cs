using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public delegate T NewCollectionEventLoggerDelegate<T>();

	public class CollectionEventLoggerService : IService
	{
		public static T LogEventOnParentOnSave<T>(BusinessObject parent, NewCollectionEventLoggerDelegate<T> constructor) where T : CollectionEventLogger
		{
			T result = null;
			if (parent != null)
			{
				var service = GetService(parent.Factory);
				result = service.LogEventOnParentOnSaveCore(parent, constructor);
			}
			return result;
		}

		static CollectionEventLoggerService GetService(BusinessObjectFactory factory)
		{
			var service = factory.ServiceContainer.GetService<CollectionEventLoggerService>();
			if (service == null)
			{
				service = new CollectionEventLoggerService();
				factory.Saving += new BusinessObjectFactory.SavingEventHandler(service.factory_Saving);
				factory.ServiceContainer.AddService(service);
			}
			return service;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "for developer only session id: 6494dccc-e054-4386-9f79-b80d1f2adff5")]
		void factory_Saving(BusinessObjectFactory factory)
		{
			var debugMessage = "Original loggers:\r\n";
			debugMessage += GetLogsDebugInfo();
			try
			{
				foreach (var logger in loggers.Values)
				{
					logger.CreateOrCancelEventsOnParentForCollectionIfRequired();
				}
			}
			catch (InvalidOperationException ex) when (ex.Message.Contains("Collection was modified; enumeration operation may not execute."))
			{
				debugMessage += "Catched Loggers:\r\n";
				debugMessage += GetLogsDebugInfo();
				ErrorReporter.ReportOnce("LoggersCollectionModifiedWhenEached", debugMessage, ex);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "for developer only session id: 6494dccc-e054-4386-9f79-b80d1f2adff5")]
		string GetLogsDebugInfo()
		{
			var logsMessage = new StringBuilder(string.Format(CultureInfo.InvariantCulture, "Loggers Count:{0}\r\n", loggers.Count));
			foreach (var item in loggers)
			{
				logsMessage.Append(string.Format(CultureInfo.InvariantCulture, "CashKey hash code:{0}; ", item.Key.GetHashCode()));
				logsMessage.Append(string.Format(CultureInfo.InvariantCulture, "CollectionEventLogger Type:{0}, hash code:{1}; Parent type:{2}, hashcode:{3}\r\n", item.Value.GetType().FullName, item.Value.GetHashCode(), item.Value.Parent?.GetType().FullName, item.Value.Parent?.GetHashCode()));
			}
			return logsMessage.ToString();
		}

		#region Implementation

		readonly Dictionary<CacheKey, CollectionEventLogger> loggers = new Dictionary<CacheKey, CollectionEventLogger>();

		internal int LoggersCount
		{
			get
			{
				return loggers.Count;
			}
		}

		T LogEventOnParentOnSaveCore<T>(BusinessObject parent, NewCollectionEventLoggerDelegate<T> constructor) where T : CollectionEventLogger
		{
			CacheKey key = new CacheKey(parent, typeof(T));
			CollectionEventLogger result = null;
			if (!loggers.TryGetValue(key, out result))
			{
				result = constructor();
				loggers[key] = result;
			}
			return (T)result;
		}

		class CacheKey
		{
			public CacheKey(BusinessObject parent, Type eventManagerType)
			{
				this.parent = parent;
				this.eventManagerType = eventManagerType;
			}

			public override bool Equals(object obj)
			{
				CacheKey rhs = obj as CacheKey;
				return rhs != null && parent == rhs.parent && eventManagerType == rhs.eventManagerType;
			}

			public override int GetHashCode()
			{
				return parent.GetHashCode() ^ eventManagerType.GetHashCode();
			}

			readonly BusinessObject parent;
			readonly Type eventManagerType;
		}

		#endregion
	}
}
