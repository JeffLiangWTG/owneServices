using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ITransitWarehouseSyncEventParent
	{
		ZString CustomsStatus { get; }

		bool IsInDatabase { get; }

		bool IsDeleted { get; }

		BusinessObjectFactory Factory { get; }
	}

	public interface ITransitWarehouseEventSyncService : IService
	{
		void Register(ITransitWarehouseSyncEventParent parent, ZString masterBillNumber, ZString houseBillNumber);
	}

	sealed class TransitWarehouseEventSyncServiceProvider
	{
		public static ITransitWarehouseEventSyncService GetService(BusinessObjectFactory factory)
		{
			var container = factory.ServiceContainer;
			return container.GetService<ITransitWarehouseEventSyncService>() ?? container.AddService<ITransitWarehouseEventSyncService>(new TransitWarehouseEventSyncService(factory));
		}

		sealed class TransitWarehouseEventSyncService : ITransitWarehouseEventSyncService
		{
			public TransitWarehouseEventSyncService(BusinessObjectFactory factory)
			{
				parentsForSend = new Dictionary<ITransitWarehouseSyncEventParent, (ZString MasterBillNumber, ZString HouseBillNumber)>();

				factory.Saved -= SendUniversalEventAfterSaved;
				factory.Saved += SendUniversalEventAfterSaved;
			}

			readonly Dictionary<ITransitWarehouseSyncEventParent, (ZString MasterBillNumber, ZString HouseBillNumber)> parentsForSend;

			void ITransitWarehouseEventSyncService.Register(ITransitWarehouseSyncEventParent parent, ZString masterBillNumber, ZString houseBillNumber)
			{
				parentsForSend[parent] = (masterBillNumber, houseBillNumber);
			}

			void SendUniversalEventAfterSaved(BusinessObjectFactory factory, bool savedSuccessfully)
			{
				if (savedSuccessfully)
				{
					try
					{
						var filterParentsForSend = parentsForSend
							.Where(c => IsValidParentForPublish(c.Key))
							.ToArray();

						if (filterParentsForSend.Any())
						{
							// Use separate containers to keep clean data
							var factoryForPublishEvent = new BusinessObjectFactory() { NameForDebugging = nameof(TransitWarehouseEventSyncService), RefreshEnabled = false };

							using (factoryForPublishEvent.AddDisposableService())
							{
								var recipientOrganisations = new[] { GlbCompany.CurrentCompany.OrgProxy };
								var recipientRoleDetails = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWR } };

								foreach (var parent in filterParentsForSend)
								{
									var obj = factoryForPublishEvent.ImportFromAnotherFactory(parent.Key as BusinessObject);

									var publishUniversalXmlResult = SendUniversalEvent((ITransitWarehouseSyncEventParent)obj, parent.Value.MasterBillNumber, parent.Value.HouseBillNumber, recipientRoleDetails);
									HandPublishToUniversalResults((ITransitWarehouseSyncEventParent)obj, publishUniversalXmlResult);
								}

								ZExceptionReporting.ProcessWithSaveExceptionHandling(factoryForPublishEvent.Save, null);
								factoryForPublishEvent.CleanUp();
							}
						}
					}
					finally
					{
						parentsForSend.Clear();

						factory.Saved -= SendUniversalEventAfterSaved;
						factory.ServiceContainer.RemoveService<ITransitWarehouseEventSyncService>();
					}
				}
			}

			bool IsValidParentForPublish(ITransitWarehouseSyncEventParent parent)
			{
				return parent != null && !parent.IsDeleted && parent.IsInDatabase;
			}

			PublishUniversalXmlResult SendUniversalEvent(ITransitWarehouseSyncEventParent parent, ZString masterBillNumber, ZString houseBillNumber, RecipientRoleDetail[] recipientRoleDetails)
			{
				var obj = parent as BusinessObject;

				var manager = obj.GetUniversalDataContextManager() as IEventDataContextManager;
				var dataObject = GenerateEvent(parent, masterBillNumber, houseBillNumber, manager, recipientRoleDetails);

				return UniversalXmlWorkflowProcessor.PublishUniversalXMLInternally(obj.Factory, obj, dataObject, EDIMessageSubTypeList.Codes.XmlUniversalEvent, dataContextManager: manager);
			}

			UniversalEvent GenerateEvent(ITransitWarehouseSyncEventParent parent, ZString masterBillNumber, ZString houseBillNumber, IEventDataContextManager manager, RecipientRoleDetail[] recipientRoleDetails)
			{
				var obj = parent as BusinessObject;

				var result = new UniversalEvent();
				result.SetWriterStrategy(DefaultDataObjectWriterStrategy.Instance);

				result.DataContext = DataContextFactory.New();
				result.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				result.DataContext.AddDataSource(manager);
				result.DataContext.AddDataTarget(DataContextType.TransitReceive, null);

				var actionInfo = new ActionInfo(recipientRoleDetails, obj) { ActionType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML };
				new DataContextDataObjectWriter().PopulateDataObject(actionInfo, obj, result.DataContext);

				var values = new List<KeyValuePair<TypeWithDescription, IZType>>();
				values.AddIfNotEmpty(UniversalEvent.ContextTypes.MAWBNumber, masterBillNumber);
				values.AddIfNotEmpty(UniversalEvent.ContextTypes.HAWBNumber, houseBillNumber);

				if (values.Any())
				{
					result.SetContextCollection(() =>
					{
						return values
							.Select(c => new Context() { Type = new ContextType() { Type = c.Key.Type, Description = c.Key.Description }, Value = SimpleTypeFormatter.FormatForMultiTypedStringElementTargetInDataObject(c.Value) })
							.ToList();
					});
				}

				result.EventType = Events.CustomsEntryStatusCode;
				result.EventTime = ZDateTimeOffset.Now;
				result.CreatedTime = ZDateTimeOffset.Now;
				result.EventReference = StmALog.GenerateEventReference(string.Empty, new[] { new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, parent.CustomsStatus) });
				result.IsEstimate = false;

				return result;
			}

			void HandPublishToUniversalResults(ITransitWarehouseSyncEventParent parent, PublishUniversalXmlResult result)
			{
				KeyValuePair<string, string>[] BuildParameters(PublishToUniversalResult publishToUniversalResult)
				{
					return new KeyValuePair<string, string>[]
					{
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, AutoEvents.CustomsEntryStatusCode),
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Status, parent.CustomsStatus),
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, publishToUniversalResult.ErrorMessage)
					};
				}

				result.AddDataExportFailureLogIfNeeded(parent as IStmALogParent, DataContextType.TransitReceive, BuildParameters);
			}
		}
	}
}
