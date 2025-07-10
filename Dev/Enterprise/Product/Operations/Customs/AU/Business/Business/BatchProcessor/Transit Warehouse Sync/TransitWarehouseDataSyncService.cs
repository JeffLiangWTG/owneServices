using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ITransitWarehouseSyncDataParent : IWorkflowProvider
	{
		bool HasChanges { get; }

		BusinessObjectFactory Factory { get; }
	}

	public interface ITransitWarehouseDataSyncService : IService
	{
		void Register(ITransitWarehouseSyncDataParent parent);
	}

	sealed class TransitWarehouseDataSyncServiceProvider
	{
		public static ITransitWarehouseDataSyncService GetService(BusinessObjectFactory factory)
		{
			var container = factory.ServiceContainer;
			return container.GetService<ITransitWarehouseDataSyncService>() ?? container.AddService<ITransitWarehouseDataSyncService>(new TransitWarehouseDataSyncService(factory));
		}

		sealed class TransitWarehouseDataSyncService : ITransitWarehouseDataSyncService
		{
			public TransitWarehouseDataSyncService(BusinessObjectFactory factory)
			{
				parentsForSend = new HashSet<ITransitWarehouseSyncDataParent>();

				factory.Saved -= SendUniversalShipmentAfterSaved;
				factory.Saved += SendUniversalShipmentAfterSaved;
			}

			readonly HashSet<ITransitWarehouseSyncDataParent> parentsForSend;

			void ITransitWarehouseDataSyncService.Register(ITransitWarehouseSyncDataParent parent)
			{
				if (!parentsForSend.Contains(parent))
				{
					parentsForSend.Add(parent);
				}
			}

			void SendUniversalShipmentAfterSaved(BusinessObjectFactory factory, bool savedSuccessfully)
			{
				if (savedSuccessfully)
				{
					try
					{
						var filterParentsForSend = parentsForSend.Where(IsValidParentForPublish).ToArray();

						if (filterParentsForSend.Any())
						{
							// Use separate containers to keep clean data
							var factoryForPublishShipment = new BusinessObjectFactory() { NameForDebugging = nameof(TransitWarehouseDataSyncService), RefreshEnabled = false };

							using (factoryForPublishShipment.AddDisposableService())
							{
								var proxy = GlbCompany.CurrentCompany.OrgProxy;
								var recipientRoleDetails = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWR } };

								foreach (var parent in filterParentsForSend)
								{
									var obj = parent as BusinessObject;

									factoryForPublishShipment.ImportFromAnotherFactory(obj);

									var result = UniversalXmlWorkflowProcessor.PublishUniversalShipment(factoryForPublishShipment, proxy, recipientRoleDetails, parent);
									HandPublishToUniversalResults(parent, result);
								}

								ZExceptionReporting.ProcessWithSaveExceptionHandling(factoryForPublishShipment.Save, null);
								factoryForPublishShipment.CleanUp();
							}
						}
					}
					finally
					{
						parentsForSend.Clear();

						factory.Saved -= SendUniversalShipmentAfterSaved;
						factory.ServiceContainer.RemoveService<ITransitWarehouseDataSyncService>();
					}
				}
			}

			void HandPublishToUniversalResults(ITransitWarehouseSyncDataParent parent, PublishUniversalXmlResult result)
			{
				KeyValuePair<string, string>[] BuildParameters(PublishToUniversalResult publishToUniversalResult)
				{
					return new[] { new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, publishToUniversalResult.ErrorMessage) };
				}

				result.AddDataExportFailureLogIfNeeded(parent as IStmALogParent, DataContextType.TransitReceive, BuildParameters);
			}

			bool IsValidParentForPublish(ITransitWarehouseSyncDataParent parent)
			{
				var obj = parent as BusinessObject;
				return obj != null && !obj.IsDeleted && obj.IsInDatabase;
			}
		}
	}
}
