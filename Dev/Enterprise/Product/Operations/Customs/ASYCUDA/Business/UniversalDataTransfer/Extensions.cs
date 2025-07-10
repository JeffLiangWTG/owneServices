using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	public static class Extensions
	{
		public static BaseJobDeclaration CreateCustomsDeclaration(this AsycudaBill billToCreateFrom)
		{
			BaseJobDeclaration result = null;
			if (billToCreateFrom != null)
			{
				var factory = new BusinessObjectFactory() { NameForDebugging = "Create Customs Declaration" }; // Debug Factory Name
				using (factory.AddDisposableService())
				{
					var bill = factory.Load<AsycudaBill>(billToCreateFrom.PK);
					if (bill != null)
					{
						var header = bill.Header;
						if (header != null)
						{
							var dataContext = DataContextFactory.New();
							dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
							dataContext.AddDataTarget(DataContextType.CustomsDeclaration, bill.CustomsJobNumber);
							var writer = header.ApplicationBusinessProvider.GetCustomsDeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, bill)), header);
							var shipment = writer.GetDataObject(bill);
							shipment.DataContext = dataContext;

							var events = UniversalXmlWorkflowProcessor.PublishUniversalXMLInternally(factory, bill, shipment, EDIMessageSubTypeList.Codes.XmlUniversalShipment);
							if (events != null && events.Length > 0)
							{
								var universalResult = PublishToUniversalResult.New(events, DataContextType.CustomsDeclaration, ZString.Empty);
								result = universalResult.FindJobIfExists() as BaseJobDeclaration;
								if (result != null)
								{
									var declaration = factory.Load<BaseJobDeclaration>(result.PK);
									declaration.Logs.AddNew(Events.TransferFromManifestToCustoms, bill.Header.AMA_JobReference);
									bill.CustomsJobNumber = result.JE_DeclarationReference;
									try
									{
										// This is the wrong place to be caling Save. When you get your next issue manager issue from this area, come talk to me about it.
										ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, () => { });
										billToCreateFrom.CustomsJobNumberInfo.RefreshBinding();
									}
									catch (ZSaveException ex)
									{
										ZExceptionReporting.HandleSaveException(ex);
									}
								}
							}
						}
					}
				}
			}
			return result;
		}

		public static bool IsSGAccessMessageEvent(this UniversalDataBuss.DataObjects.Universal.Event universalEvent)
		{
			return universalEvent.DataContext.DataProviderForCodeMapping == Constants.DataProviders.SGAccess
					&& (universalEvent.EventType.GetValueOrDefault() == AutoEvents.MessageReceivedCode
						|| universalEvent.EventType.GetValueOrDefault() == AutoEvents.MessageRejectedCode
						|| universalEvent.EventType.GetValueOrDefault() == AutoEvents.MessageDeliveredCode);
		}
	}
}
