using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public static class EMCSHelper
	{
		public static EMCSJobDeclaration GetDeclarationFromCustomsBusinessResponse(BusinessObjectFactory factory, EMCSCustomsBusinessResponse response)
		{
			return GetDeclarationFromEHubTrackingID(factory, response.EHubTrackingID) ??
				GetDeclarationFromJobNumber(factory, response.JobNumber) ??
				GetDeclarationFromInboundMessage(factory, response.ServiceReference);
		}

		public static EMCSJobDeclaration GetDeclarationFromInboundMessage(EMCSInboundEDIMessage message)
		{
			return GetDeclarationFromInboundMessage(message.Factory, message.EM_ApplicationReference);
		}

		public static EMCSJobDeclaration GetDeclarationFromInboundMessage(BusinessObjectFactory factory, string applicationReference)
		{
			return GetDeclarationFromReference(factory, applicationReference) ?? GetDeclarationFromJobNumber(factory, applicationReference);
		}

		public static EMCSJobDeclaration GetDeclarationFromEADNumber(EMCSInboundEDIMessage message, ZString eadNumber, ZString sequenceNumber)
		{
			EMCSJobDeclaration result = null;
			if (!eadNumber.IsEmpty)
			{
				result = EMCSJobDeclaration.Loader.LoadFromEadAndSequenceNumber<EMCSJobDeclaration>(message.Factory, eadNumber, sequenceNumber, message.Company).FirstOrDefault();
			}
			return result;
		}

		public static ZString GetApplicationReferenceFromLastOutgoingMessage(EDIMessageCollection messages)
		{
			return messages.OfType<EDIMessage>().Where(m => m.IsTransmitMessage
				&& !m.EM_ApplicationReference.IsEmpty
				&& m.EM_ApplicationCode == EDIMessage.ApplicationCodes.GbCustomsEMCS)
				.OrderByDescending(m => m.EM_SystemCreateTimeUtc).FirstOrDefault()?.EM_ApplicationReference ?? ZString.Empty;
		}

		static EMCSJobDeclaration GetDeclarationFromEHubTrackingID(BusinessObjectFactory factory, ZString eHubTrackingID)
		{
			EMCSJobDeclaration result = null;
			if (ZGuid.TryParse(eHubTrackingID, out var trackingID))
			{
				var message = GetOriginalMessage(factory, trackingID);
				result = message?.EM_LinkedObject as EMCSJobDeclaration;
			}
			return result;
		}

		public static EDIMessage GetOriginalMessage(BusinessObjectFactory factory, ZGuid eHubTrackingID)
		{
			var interchange = GetOutboundInterchange(factory, eHubTrackingID);
			return interchange != null ? GetMessageFromInterchange(factory, interchange.PK) : null;
		}

		static EDIInterchange GetOutboundInterchange(BusinessObjectFactory factory, ZGuid eHubTrackingID)
		{
			var query = new ZQuery(EDIInterchangeSchema.EI_SessionGUID, eHubTrackingID)
				.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
			return factory.LoadTop1<EDIInterchange>(query);
		}

		static EDIMessage GetMessageFromInterchange(BusinessObjectFactory factory, ZGuid interchangePK)
		{
			var query = new ZQuery(EDIMessageSchema.EM_EI, interchangePK);
			return factory.LoadTop1<EDIMessage>(query);
		}

		static EMCSJobDeclaration GetDeclarationFromJobNumber(BusinessObjectFactory factory, ZString jobNumber)
		{
			EMCSJobDeclaration result = null;
			if (!jobNumber.IsEmpty)
			{
				var reference = jobNumber.Contains("/") ? jobNumber.Split("/")[0] : jobNumber;
				result = factory.LoadTop1<EMCSJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, reference));
			}
			return result;
		}

		static EMCSJobDeclaration GetDeclarationFromReference(BusinessObjectFactory factory, ZString applicationReference)
		{
			return GetOriginalMessage(factory, applicationReference)?.EM_LinkedObject as EMCSJobDeclaration;
		}

		static EDIMessage GetOriginalMessage(BusinessObjectFactory factory, ZString applicationReference, string status = EDIMessage.Status.Acknowledged)
		{
			EDIMessage result = null;
			if (!applicationReference.IsEmpty)
			{
				var query = new ZQuery(EDIMessageSchema.EM_ApplicationReference, applicationReference)
					.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.GbCustomsEMCS)
					.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit)
					.AddToFilter(EDIMessageSchema.EM_Status, status);
				_ = query.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + OrderByClause.Descending;
				result = factory.LoadTop1<EDIMessage>(query);
			}
			return result;
		}
	}
}
