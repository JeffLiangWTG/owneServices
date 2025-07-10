
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CMRStatusCalculator<ParentT> : CMRStatusCalculatorBase<ParentT> where ParentT : BusinessObject, Customs.Business.IStatusNeedsRecalculationProvider
	{
		public CMRStatusCalculator(ParentT parent)
			: base(parent)
		{
		}

		protected override void DeriveStatusCore()
		{
			base.DeriveStatusCore();
			if (this is ICMRCargoReportEventsLogger)
			{
				PostCargoReportEventsIfRequired();
			}
		}

		void PostCargoReportEventsIfRequired()
		{
			IParentForCargoReporter parentForCargoReportingEvents = ((ICMRCargoReportEventsLogger)this).ParentForCargoReportingEvents;
			if (Parent.IsInDatabase && parentForCargoReportingEvents != null)
			{
				if (CMRUtilities.MessageStatusChangedToWaiting(StatusInfo))
				{
					AddCargoReportEvent(parentForCargoReportingEvents, parentForCargoReportingEvents.CargoReportSentEvent, false);
				}
				else if (CMRUtilities.MessageStatusChangedToAccepted(StatusInfo))
				{
					AddCargoReportEvent(parentForCargoReportingEvents, parentForCargoReportingEvents.CargoReportAcceptedEvent, true);
				}
				else if (CMRUtilities.MessageStatusChangedToRejected(StatusInfo))
				{
					AddCargoReportEvent(parentForCargoReportingEvents, parentForCargoReportingEvents.CargoReportRejectedEvent, true);
				}
				else if (CMRUtilities.MessageStatusChangedToWithdrawWaiting(StatusInfo))
				{
					AddCargoReportEvent(parentForCargoReportingEvents, parentForCargoReportingEvents.CargoReportWithdrawEvent, true);
				}
			}
		}

		void AddCargoReportEvent(IParentForCargoReporter parentForCargoReportingEvents, Event cargoReportEvent, bool incomingStatus)
		{
			var eventTime = ZDateTimeOffset.Now;
			var lastMessage = incomingStatus ? Parent.Messages.LastIncomingMessage : Parent.Messages.LastOutgoingMessage;
			if (lastMessage != null)
			{
				eventTime = (incomingStatus ? lastMessage.EM_DateTimeInterchangeSent : lastMessage.EM_SystemCreateTimeUtc).ToOffset();
				if (!eventTime.IsValid)
				{
					eventTime = ZDateTimeOffset.Now;
				}
			}
			var reference = (ZString)StatusInfo.Value + " " + parentForCargoReportingEvents.CargoReporterReferenceText;
			parentForCargoReportingEvents.Logs.AddNew(cargoReportEvent, reference, eventTime);
		}

		#region Implementation

		protected internal override ZString GetAwaitingResponseStatus(EDIMessage message)
		{
			ZString result = ZString.Empty;
			switch (message.EM_MessageSubType)
			{
				case CMRMessage.MessageSubTypes.Original:
				case CMRMessage.MessageSubTypes.Request:
					result = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
					break;
				case CMRMessage.MessageSubTypes.Amendment:
				case CMRMessage.MessageSubTypes.Change:
				case CMRMessage.MessageSubTypes.ReplaceHeader:
					result = CMRBaseStatuses.Codes.AwaitingResponseToAmendment;
					break;
				case CMRMessage.MessageSubTypes.Withdraw:
					result = CMRBaseStatuses.Codes.AwaitingResponseToWithdrawal;
					break;
			}
			return result;
		}

		protected internal override ZString GetRejectedResponseStatus(EDIMessage outgoingMessage, EDIMessage incomingMessage)
		{
			ZString result = ZString.Empty;
			switch (outgoingMessage.EM_MessageSubType)
			{
				case CMRMessage.MessageSubTypes.Original:
				case CMRMessage.MessageSubTypes.Request:
					result = CMRBaseStatuses.Codes.OriginalRejected;
					break;
				case CMRMessage.MessageSubTypes.Amendment:
				case CMRMessage.MessageSubTypes.Change:
				case CMRMessage.MessageSubTypes.ReplaceHeader:
					result = CMRBaseStatuses.Codes.AmendmentRejected;
					break;
				case CMRMessage.MessageSubTypes.Withdraw:
					result = CMRBaseStatuses.Codes.WithdrawalRejected;
					break;
			}
			return result;
		}

		protected internal override ZString GetAcceptedResponseStatus(EDIMessage outgoingMessage)
		{
			ZString result = ZString.Empty;
			switch (outgoingMessage.EM_MessageSubType)
			{
				case CMRMessage.MessageSubTypes.Original:
				case CMRMessage.MessageSubTypes.Request:
					result = CMRBaseStatuses.Codes.OriginalAccepted;
					break;
				case CMRMessage.MessageSubTypes.Amendment:
				case CMRMessage.MessageSubTypes.Change:
				case CMRMessage.MessageSubTypes.ReplaceHeader:
					result = CMRBaseStatuses.Codes.AmendmentAccepted;
					break;
				case CMRMessage.MessageSubTypes.Withdraw:
					result = CMRBaseStatuses.Codes.WithdrawalAccepted;
					break;
			}
			return result;
		}

		protected override CodeDescriptionPairList StatusList
		{
			get { return Factory.GetCachedValue<CMRConsolidatedCargoStatuses>(); }
		}

		#endregion
	}
}
