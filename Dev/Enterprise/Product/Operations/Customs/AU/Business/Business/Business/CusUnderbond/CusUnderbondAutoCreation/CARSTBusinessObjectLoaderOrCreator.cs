using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CARSTBusinessObjectLoaderOrCreator : CusUnderbondFactory
	{
		public BusinessObject[] LoadOrCreateRecordForMessage(CMRCARSTMessage message)
		{
			BusinessObject[] results = null;
			if (IsInterestedInCARST(message))
			{
				results = LoadOrCreateRecordForMessageCore(message);
				if (results != null)
				{
					CusEntryHeader entry = null;
					PackingGroup messagePackingGroup = null;
					foreach (BusinessObject result in results)
					{
						if (result != null)
						{
							CMRCARSTMessage linkedMessage = LinkMessageOrCloneOfMessageToBusinessObject(result, message);
							if (result is CusEntryHeader)
							{
								entry = (CusEntryHeader)result;
							}

							if (result is PackingGroup)
							{
								messagePackingGroup = (PackingGroup)result;
							}

							AuditLogCargoStatusResponse(result, linkedMessage);
						}
					}
					if (entry != null && entry.Declaration != null)
					{
						foreach (Package package in entry.Declaration.Packages)
						{
							var packingGroup = package.PackingGroup;
							if (packingGroup != null)
							{
								// the following caches values and then frees memory allocated to the EDIFACT CUSRES 
								var mostRecentMessage = messagePackingGroup != null && packingGroup.PK == messagePackingGroup.PK ? packingGroup.ResetCacheAndGetMostRecentCARSTorDSAMessage() : packingGroup.MostRecentCARSTorDSAMessage;
								if (mostRecentMessage != null)
								{
									var abbreviatedCargoStatusDescriptionForTransportLine = packingGroup.GetAbbreviatedStatusDescriptionForTransportLine();
									var cargoStatusForTransportLine = packingGroup.GetCargoStatusFromLatestMessage();
									mostRecentMessage.ResetCUSRESCache();
								}
							}
						}
					}
				}
			}
			return results;
		}

		protected virtual internal CMRCARSTMessage LinkMessageOrCloneOfMessageToBusinessObject(BusinessObject businessObject, CMRCARSTMessage message)
		{
			return (CMRCARSTMessage)message.LinkOrCloneMessage(businessObject);
		}

		void AuditLogCargoStatusResponse(BusinessObject result, CMRCARSTMessage message)
		{
			if (EnableAuditLogCargoStatusResponse)
			{
				BusinessObject linkedObject = result;
				BusinessObjectWrapper linkedWrapper = result as BusinessObjectWrapper;
				if (linkedWrapper != null)
				{
					linkedObject = linkedWrapper.WrappedBusinessObject;
				}
				if (linkedObject != null)
				{
					linkedObject.GetLogs().AddNew(Events.StatusUpdated, message.GetStatusDescription().SubstringSafe(0, StmALogSchema.SL_Reference.MaxLength));
				}
			}
		}

		protected virtual bool EnableAuditLogCargoStatusResponse { get { return true; } }

		protected internal abstract bool IsInterestedInCARST(CMRCARSTMessage message);
		protected internal abstract BusinessObject[] LoadOrCreateRecordForMessageCore(CMRCARSTMessage message);

		protected bool StatusAdviceRelatedToCTO(CMRCARSTMessage message)
		{
			bool result = !message.PremiseID.IsEmpty && CompanyHasCTOWithPremiseID(message.PremiseID);
			return result;
		}

		protected bool StatusAdviceRelatedToDepot(CMRCARSTMessage message)
		{
			bool result = !message.PremiseID.IsEmpty && CompanyHasDepotWithPremiseID(GlbCompany.GetCurrentCompany(message.Factory), message.PremiseID);
			return result;
		}
	}
}
