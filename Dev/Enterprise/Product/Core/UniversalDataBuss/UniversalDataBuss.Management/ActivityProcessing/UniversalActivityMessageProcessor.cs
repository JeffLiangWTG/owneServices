using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.UniversalDataBuss.Management.ActivityProcessing
{
	class UniversalActivityMessageProcessor : ITopLevelDataObjectProcessor
	{
		public MessageStatus ProcessDataObject(IEDIMessage message, ITopLevelDataObject dataObject, IUniversalObjectFactory factory, IXmlSessionTracker logger)
		{
			var result = MessageStatus.Discarded;
			var xmlSessionTracker = (XmlSessionTracker)logger;
			var activity = (Activity)dataObject;
			var logAdder = new MessageLinkedLogAdder(message, Events.DataImport, null, null, (UniversalObjectFactory)factory, logger);

			if (activity.DataContext?.DataTargetCollection != null && activity.DataContext.DataTargetCollection.Any())
			{
				using (xmlSessionTracker.RegisterForBusinessObjectImported(logAdder.AddNewLogToParent))
				{
					foreach (var baseContextManager in DataContextManagersFactory.All)
					{
						if (baseContextManager is IActivityDataContextManager manager && manager.ManagesActivities && DoesManagerManageAnyDataTargets(manager, activity))
						{
							try
							{
								xmlSessionTracker.IndividualImportBegin(manager.DataContextType);
								var reader = manager.GetActivityDataObjectReader(activity, logger, factory);

								var businessObject = reader.ReadIntoTopLevelBusinessObject();
								((UniversalObjectFactory)factory).SaveAtEndOfImport(logger);

								logger.LogTopLevelDataContextKey(() => businessObject.GetUniversalDataContextManager().DataContextKey);
								result = MessageStatus.Processed;
							}
							catch (DataObjectReadFailureException exception)
							{
								result = MessageStatus.Rejected;
								logger.LogBoth(LogType.Error, exception.Message);
								logger.LogBoth(LogType.Information, Res.GetString("9d6a4fde-58e6-442d-b934-12f6f17f23a8", "No changes were made due to the above errors. Please fix the errors and try again."));
							}
							finally
							{
								xmlSessionTracker.IndividualImportEnd();
							}
						}
					}
				}
			}

			if (result == MessageStatus.Discarded)
			{
				xmlSessionTracker.LogWasNotUsedByModule(LogType.Information, Res.GetString("af8316cc-80c5-4bc6-b315-2965e5ce3e58", "No Module used this Universal Activity data."));
			}
			else
			{
				xmlSessionTracker.CheckAndRemoveUnnecessaryUnknownAddressTypeWarnings();
			}

			return result;
		}

		bool DoesManagerManageAnyDataTargets(IActivityDataContextManager manager, Activity activity)
		{
			foreach (var target in activity.DataContext.DataTargetCollection)
			{
				if (target.Type.HasValue && Enum.TryParse<DataContextType>(target.Type.Value, out var dataContextType))
				{
					if (manager.DoesManageDataContextType(dataContextType))
					{
						return true;
					}
				}
			}

			return false;
		}

		public MessageKeyProviderResult GetKeys(IEDIMessage message, ITopLevelDataObject dataObject, IUniversalObjectFactory factory, IXmlSessionTracker logger)
		{
			var keys = GetKeysForThisActivityIncludingRelatedActivities((Activity)dataObject);

			return new MessageKeyProviderResult(keys.Distinct());
		}

		public void ValidateDataObject(ITopLevelDataObject dataObject, IXmlSessionTracker logger)
		{
		}

		static IEnumerable<(string KeyValue, string KeySource)> GetKeysForThisActivityIncludingRelatedActivities(Activity activity)
		{
			foreach (var key in GetAllDataTargetKeys(activity))
			{
				yield return key;
			}

			foreach (var key in GetKeysForRelatedActivities(activity))
			{
				yield return key;
			}
		}

		static IEnumerable<(string KeyValue, string KeySource)> GetAllDataTargetKeys(Activity activity)
		{
			if (activity.DataContext?.DataTargetCollection != null)
			{
				foreach (var target in activity.DataContext.DataTargetCollection)
				{
					if (target.Key.HasValue)
					{
						yield return (target.Key.Value, "activity.DataContext.DataTargetCollection");
					}
				}
			}
		}

		static IEnumerable<(string, string)> GetKeysForRelatedActivities(Activity activity)
		{
			if (activity.RelatedActivityCollection != null)
			{
				foreach (var relatedActivity in activity.RelatedActivityCollection)
				{
					foreach (var key in GetKeysForThisActivityIncludingRelatedActivities(relatedActivity))
					{
						yield return key;
					}
				}
			}
		}
	}
}
