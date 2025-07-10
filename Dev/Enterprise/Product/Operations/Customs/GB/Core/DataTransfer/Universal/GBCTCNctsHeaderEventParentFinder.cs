using System;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.GB.DataTransfer.Universal
{
	public class GBCTCNctsHeaderEventParentFinder : EU.NCTS.DataTransfer.NctsEventParentFinder
	{
		public GBCTCNctsHeaderEventParentFinder(BusinessObjectFactory factory, IEventDataContextManager manager, IXmlImportLogger logger) : base(factory, manager, logger) { }

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
		{
			var header = FindHeaderAndProcess(xmlEvent);

			return header == null ? null : new BusinessObject[] { header };
		}

		protected override BusinessObject[] GetChildrenIfSpecifiedInContext(BusinessObject[] logParents, UniversalEvent eventData)
		{
			FindHeaderAndProcess(eventData);
			return base.GetChildrenIfSpecifiedInContext(logParents, eventData);
		}

		protected NctsHeader FindHeaderAndProcess(UniversalEvent xmlEvent)
		{
			var (header, originalMessage, arrivalOrDepartureContext) = FindNctsHeaderAndOriginalMessage(xmlEvent);

			var eventType = ((IXmlEventValueObject)xmlEvent).EventType;
			if (eventType == AutoEvents.MessageSentCode && header != null)
			{
				ProcessEvent(header, originalMessage, arrivalOrDepartureContext);
			}
			else
			{
				ProcessErrorEvent(xmlEvent, originalMessage, arrivalOrDepartureContext);
			}

			return header;
		}

		(NctsHeader, EDIMessage, Context) FindNctsHeaderAndOriginalMessage(UniversalEvent xmlEvent)
		{
			NctsHeader header = null;
			EDIMessage originalMessage = null;
			Context arrivalOrDepartureContext = GetArrivalOrDepartureContext(xmlEvent);

			var expectedProviders = new ZString[] { Constants.CTCGB, Constants.CTCNI };

			if (ZGuid.TryParse(((IXmlEventValueObject)xmlEvent).Context.EHubTrackingID, out var eHubTrackingID) &&
				expectedProviders.Contains((((IXmlEventValueObject)xmlEvent).DataContext?.DataProviderForCodeMapping ?? ZString.Empty))
				)
			{
				var originalInterchange = CDS.Helpers.EventParentFinderHelper.GetOutboundInterchange(eHubTrackingID, factory);
				if (originalInterchange != null)
				{
					originalMessage = CDS.Helpers.EventParentFinderHelper.GetOriginalMessage(originalInterchange.PK, factory);
					if (originalMessage != null)
					{
						header = GetHeader(originalMessage);
					}
				}
			}

			return (header, originalMessage, arrivalOrDepartureContext);
		}

		Context GetArrivalOrDepartureContext(UniversalEvent xmlEvent)
		{
			if (xmlEvent.ContextCollection != null)
			{
				var expectedContextTypes = new ZString[] { Constants.Contexts.ArrivalId, Constants.Contexts.DepartureId };
				foreach (var ctx in xmlEvent.ContextCollection)
				{
					if (expectedContextTypes.Contains(ctx.Type.Type ?? ZString.Empty))
					{
						return ctx;
					}
				}
			}
			return null;
		}

		void ProcessEvent(NctsHeader header, EDIMessage originalMessage, Context arrivalOrDepartureContext)
		{
			originalMessage.EM_ApplicationReference = arrivalOrDepartureContext.Value ?? ZString.Empty;
			originalMessage.EM_Status = EDIMessage.Status.Acknowledged;
			header.BH_MessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.Ok;
		}

		void ProcessErrorEvent(UniversalEvent eventDataObject, EDIMessage originalMessage, Context arrivalOrDepartureContext)
		{
			var errorMessage = ZString.Empty;
			var errorResponse = ((IXmlEventValueObject)eventDataObject).Context.ResponseText;
			var errorType = ((IXmlEventValueObject)eventDataObject).Context.ErrorSummary;
			var errorDetail = ((IXmlEventValueObject)eventDataObject).Context.ErrorDescription;

			if (errorResponse.HasValue && !errorResponse.Value.IsEmpty)
			{
				var data = Convert.FromBase64String(errorResponse.Value);
				errorMessage = Encoding.UTF8.GetString(data);
			}
			else if (errorDetail.HasValue && !errorDetail.Value.IsEmpty)
			{
				errorMessage = CDS.Helpers.EventParentFinderHelper.GenericErrorWrapper(errorType, errorDetail.Value);
			}
			ProcessError(originalMessage, errorMessage, arrivalOrDepartureContext);
		}

		void ProcessError(EDIMessage originalMessage, ZString errorMessage, Context arrivalOrDepartureContext)
		{
			if (originalMessage != null)
			{
				var header = GetHeader(originalMessage);
				if (header != null)
				{
					if (arrivalOrDepartureContext != null)
					{
						originalMessage.EM_Status = EDIMessage.Status.Rejected;
						header.BH_MessageStatus = arrivalOrDepartureContext.Type.Type.ToString() == Constants.Contexts.DepartureId ? EU.NCTS.Business.NctsMessageStatusList.Codes.Rejected : EU.NCTS.Business.NctsMessageStatusList.Codes.ArrivalNotificationRejected;
					}
					else
					{
						originalMessage.EM_Status = EDIMessage.Status.Failed;
						header.BH_MessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.MessageSyntaxOrBusinessRuleErrors;
						var receivedMessage = header.Messages.AddNew(typeof(GbEDIMessage));
						receivedMessage.EM_Status = EDIMessage.Status.ProcessedOK;
						receivedMessage.EM_MessageText = errorMessage;
						receivedMessage.EM_MessageNum = originalMessage.EM_MessageNum.Left(originalMessage.EM_MessageNumInfo.MaxLength - 1) + "R";
						receivedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
					}
				}
			}
		}

		NctsHeader GetHeader(EDIMessage message)
		{
			var linkedObject = message.EM_LinkedObject;
			return (linkedObject as NctsHeader) ?? ((linkedObject as NctsDepartureMovementHeader)?.Header as NctsHeader);
		}
	}
}
