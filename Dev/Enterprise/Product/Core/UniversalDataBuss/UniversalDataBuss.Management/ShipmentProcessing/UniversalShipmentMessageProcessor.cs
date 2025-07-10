using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.UniversalDataBuss.Management.ShipmentProcessing
{
	class UniversalShipmentMessageProcessor : ITopLevelDataObjectProcessor
	{
		internal UniversalShipmentMessageProcessor() { }

		public MessageStatus ProcessDataObject(IEDIMessage message, ITopLevelDataObject dataObject, IUniversalObjectFactory factory, IXmlSessionTracker logger)
		{
			return ProcessDataObjectCore(UseIncomingShipmentData_ImportShipment, message, (UniversalShipment)dataObject, (UniversalObjectFactory)factory, logger, true);
		}

		MessageStatus ProcessDataObjectCore(InvokeContextManager invoke, IEDIMessage message, UniversalShipment dataObject, UniversalObjectFactory factory, IXmlSessionTracker logger, bool saveAtEndOfImport)
		{
			void AddDataImportLog(BusinessObject bizObj)
			{
				if (bizObj is IStmALogParent logParent)
				{
					var eventToAdd = dataObject.GetImportAction() == ImportAction.LinkOnly
						? Events.DataLinked
						: Events.DataImport;

					var stmALog = logParent.Logs.AddNew(new EventValue(eventToAdd));
					message.AddUniversalDataLink(stmALog);
				}
			}

			var result = ImportShipment(
					invoke,
					dataObject,
					AddDataImportLog,
					factory,
					logger,
					saveAtEndOfImport);

			if (result == MessageStatus.Discarded)
			{
				if (dataObject.DataContext == null)
				{
					logger.LogBoth(LogType.Information, Res.GetString("e6cfdc4e-9718-42f1-8f28-94eea4eb021c", "Hint: Adding a {0} element with an element in the {1} will make the specified Module import where no existing data matches.", "DataContext", "DataTargetCollection"));
				}
				else if ((dataObject.DataContext.DataTargetCollection == null || !dataObject.DataContext.DataTargetCollection.Any()) && !logger.HasIgnoredModule)
				{
					logger.LogBoth(LogType.Information, Res.GetString("0092317a-8927-467e-8b79-3f17aa3833f2", "Hint: Adding an element in the {0} will make the specified Module import where no existing data matches.", "DataTargetCollection"));
				}
			}

			return result;
		}

		public MessageKeyProviderResult GetKeys(IEDIMessage message, ITopLevelDataObject dataObject, IUniversalObjectFactory factory, IXmlSessionTracker logger)
		{
			var keys = new HashSet<(string KeyValue, string KeySource)>();
			var result = ProcessDataObjectCore(UseIncomingShipmentData_GetKeys(keys), message, (UniversalShipment)dataObject, (UniversalObjectFactory)factory, logger, false);

			if (result == MessageStatus.Processed
				|| result == MessageStatus.Linked)
			{
				return new MessageKeyProviderResult(keys);
			}
			else
			{
				return new MessageKeyProviderResult(result);
			}
		}

		delegate bool InvokeContextManager(UniversalShipment dataObject, UniversalObjectFactory factory, IShipmentDataContextManager contextManager, IXmlSessionTracker logger);

		bool UseIncomingShipmentData_ImportShipment(UniversalShipment dataObject, UniversalObjectFactory factory, IShipmentDataContextManager contextManager, IXmlSessionTracker logger)
		{
			return contextManager.UseIncomingShipmentData(dataObject, logger, factory);
		}

		InvokeContextManager UseIncomingShipmentData_GetKeys(HashSet<(string KeyValue, string KeySource)> keysInfo)
		{
			return (dataObject, factory, contextManager, sessionTracker) =>
			{
				var result = contextManager.GetKeysForBlockingParallelImport(dataObject, sessionTracker, factory);
				if (result.IsMatch)
				{
					foreach (var keyInfo in result.KeysInfo.Where(k => !string.IsNullOrEmpty(k.KeyValue)))
					{
						keysInfo.Add(keyInfo);
					}
				}
				return result.IsMatch;
			};
		}

		MessageStatus ImportShipment(InvokeContextManager invoke, UniversalShipment dataObject, DataImportedToBusinessObjectAction dataImportedAction, UniversalObjectFactory factory, IXmlSessionTracker logger, bool saveAtEndOfImport)
		{
			var result = MessageStatus.Discarded;
			var xmlSessionTracker = (XmlSessionTracker)logger;
			DefaultDataTargetFromRecipientRoleIfNoDataTargetPresent(dataObject, logger);

			using (xmlSessionTracker.RegisterForBusinessObjectImported(dataImportedAction))
			{
				foreach (var baseContextManager in DataContextManagersFactory.All)
				{
					var contextManager = baseContextManager as IShipmentDataContextManager;
					if (contextManager != null && contextManager.ManagesShipments)
					{
						try
						{
							xmlSessionTracker.IndividualImportBegin(contextManager.DataContextType);
							bool usedIncomingShipmentData = invoke(dataObject, factory, contextManager, logger);
							if (usedIncomingShipmentData)
							{
								if (saveAtEndOfImport)
								{
									using (new DisposableAction(factory.FireCleanupAfterSaving))
									{
										factory.SaveAtEndOfImport(logger); // TODO: Delete this extra save.
									}
								}

								result = dataObject.GetImportAction() == ImportAction.LinkOnly
									? MessageStatus.Linked
									: MessageStatus.Processed;
							}
							ClearSavedJobs(factory);
							xmlSessionTracker.IndividualImportEnd(reportLogAsAResult: usedIncomingShipmentData);
						}
						catch (DataObjectReadFailureException exception)
						{
							DeleteUnsavedJobs(factory);
							if (!string.IsNullOrEmpty(exception.Message))
							{
								logger.LogBoth(LogType.Error, exception.Message);
							}
							logger.LogBoth(LogType.Information, Res.GetString("9d6a4fde-58e6-442d-b934-12f6f17f23a8", "No changes were made due to the above errors. Please fix the errors and try again."));
							xmlSessionTracker.IndividualImportEnd();
						}
					}
				}
			}

			if (result == MessageStatus.Discarded)
			{
				xmlSessionTracker.LogWasNotUsedByModule(LogType.Information, Res.GetString("12baff53-75cb-42a7-9443-075a3fc5ddb6", "No Module used this Universal Shipment data."));
			}
			else
			{
				xmlSessionTracker.CheckAndRemoveUnnecessaryUnknownAddressTypeWarnings();
			}

			return result;
		}

		void DeleteUnsavedJobs(UniversalObjectFactory factory)
		{
			var service = factory.BOFactory.ServiceContainer.GetService<JobTrackerService>();
			if (service != null)
			{
				service.DeleteNewJobs();
			}
		}

		void ClearSavedJobs(UniversalObjectFactory factory)
		{
			var service = factory.BOFactory.ServiceContainer.GetService<JobTrackerService>();
			if (service != null)
			{
				service.Clear();
			}
		}

		static void DefaultDataTargetFromRecipientRoleIfNoDataTargetPresent(UniversalShipment dataObject, IXmlSessionTracker importSessionLogger)
		{
			if (dataObject.DataContext != null && dataObject.DataContext.DataTargetCollection == null && dataObject.DataContext.RecipientRoleCollection != null && dataObject.DataContext.RecipientRoleCollection.Any())
			{
				foreach (var baseContextManager in DataContextManagersFactory.All)
				{
					var contextManager = baseContextManager as IShipmentDataContextManager;
					if (contextManager != null && contextManager.ManagesShipments)
					{
						contextManager.DefaultDataTargetFromRecipientRole(dataObject.DataContext, importSessionLogger);
					}
				}
			}
		}

		public void ValidateDataObject(ITopLevelDataObject dataObject, IXmlSessionTracker importSessionLogger)
		{
		}
	}
}
