using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.AFR.Business
{
	public static class BusinessObjectExtensions
	{
		public static bool IsAFRTransmitMessage(this XmlEDIMessage message)
		{
			var result = false;
			if (message != null && message.IsTransmitMessage && message.EM_MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalShipment)
			{
				var interchange = message.Interchange;
				result = interchange != null && interchange.EI_To == Constants.JapanCustomsReceipientID;
			}
			return result;
		}

		public static ZString GetTargetBillNumber(this ForwardingShipment shipment, Guid companyPK)
		{
			ZString result = ZString.Empty;
			if (shipment != null)
			{
				var sourceBillNumber = shipment.JS_HouseBill;
				var nVOCCIdInRegistry = companyPK == Guid.Empty ? string.Empty : JPAFRRegistry.Instance.AFRNVOCCIDtoAddtoBillDuringSync.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);
				var formatedNVOCCIdInRegistry = nVOCCIdInRegistry.PadRight(4, '-');
				if (!string.IsNullOrEmpty(nVOCCIdInRegistry) && !sourceBillNumber.StartsWith(formatedNVOCCIdInRegistry))
				{
					result = formatedNVOCCIdInRegistry + sourceBillNumber;
				}
				else
				{
					result = sourceBillNumber;
				}
			}
			return result;
		}

		public static ZString GetTargetOceanBillNumber(this ZString sourceBillNumber, Guid companyPK, ZString carrierCode)
		{
			ZString result = ZString.Empty;
			var carrierCodeTrimed = carrierCode.Trim();
			var shouldAddSCACCode = companyPK != Guid.Empty && JPAFRRegistry.Instance.AFRAddSCACtoBillsDuringSync.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);
			shouldAddSCACCode = shouldAddSCACCode && !carrierCodeTrimed.IsEmpty;
			var formatedNVOCCIdInRegistry = carrierCodeTrimed.PadRight(4, '-');
			if (shouldAddSCACCode && !sourceBillNumber.StartsWith(formatedNVOCCIdInRegistry))
			{
				result = formatedNVOCCIdInRegistry + sourceBillNumber;
			}
			else
			{
				result = sourceBillNumber;
			}
			return result;
		}

		public static OrgRelatedParty GetJapanNotificationParty(this OrgHeader orgHeader, GlbCompany company)
		{
			var factory = orgHeader.Factory;
			ZQuery filter = new ZQuery(OrgRelatedPartySchema.PR_PartyType, RelatedPartyTypeList.Codes.JapanNotificationParty);
			filter.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, orgHeader.PK);
			filter.AddToFilter(OrgRelatedPartySchema.PR_Location, string.Empty);
			ZQuery companyFilter = new ZQuery(OrgRelatedPartySchema.PR_GC, company.PK);
			companyFilter.AddToFilter(JoinCondition.Or, OrgRelatedPartySchema.PR_GC, null);
			ZQuery directionFilter = new ZQuery(OrgRelatedPartySchema.PR_FreightDirection, RelatedPartyDirectionList.Codes.Delivery);
			directionFilter.AddToFilter(JoinCondition.Or, OrgRelatedPartySchema.PR_FreightDirection, RelatedPartyDirectionList.Codes.PickupAndDelivery);
			filter.AddToFilter(companyFilter);
			filter.AddToFilter(directionFilter);
			var possibleRealtedParties = factory.Load<OrgRelatedParty>(filter);
			return possibleRealtedParties.OrderByDescending(candidate =>
			{
				var result = 0;
				result += candidate.PR_FreightDirection == RelatedPartyDirectionList.Codes.Delivery ? 1 : 0;
				result += candidate.CompanyLevel == CompanyLevelList.Codes.COM ? 10 : 0;
				return result;
			}).FirstOrDefault();
		}
	}
}
