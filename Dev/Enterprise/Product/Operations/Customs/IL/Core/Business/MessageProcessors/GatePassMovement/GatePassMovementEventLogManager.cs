using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.GPM.RES_135.GP_NG_1035_MSG2_GatepassFeedbackMessage;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Customs.IL.Business.Constants;

namespace Enterprise.Customs.IL.Business.MessageProcessors
{
	class GatePassMovementEventLogManager : ElectronicFormEventLogManager<GpNg1035Msg2GatepassFeedbackMessage>
	{
		protected override string MessageType => ILMessageEventParameter.GatePassMovementDocumentName;

		protected override Event DetermineEventType(GpNg1035Msg2GatepassFeedbackMessage response, EnterpriseBusinessObject enterpriseBusinessObject)
		{
			var isWithdrawResponse = IsWithdrawResponse(enterpriseBusinessObject);
			var gatePassFeedbackMsg = response.GatepassFeedbackMessage?.FirstOrDefault();
			if (gatePassFeedbackMsg == null)
			{
				return null;
			}

			if (!isWithdrawResponse)
			{
				switch (gatePassFeedbackMsg.GatepassStatus)
				{
					case GatePassMovement.MessageResponseStatus.Rejected:
						return Events.MessageRejected;
					case GatePassMovement.MessageResponseStatus.Accepted:
						return Events.MessageAccepted;
				}
			}

			if (isWithdrawResponse && gatePassFeedbackMsg.GatepassReturnCode == GatePassMovement.MessageReturnCode.WithdrawCancelAccepted)
			{
				return Events.MessageWithdrawCancelAccepted;
			}

			return null;
		}

		protected override string GetReferenceNumberEventParameter(EnterpriseBusinessObject enterpriseBusinessObject)
		{
			if (enterpriseBusinessObject is ForwardingConsol consol)
			{
				return consol.JK_GMN;
			}

			return ((ForwardingShipment)enterpriseBusinessObject).JS_GMN;
		}

		protected override string GetResponseEventParameter(Event eventType, GpNg1035Msg2GatepassFeedbackMessage response)
		{
			if (eventType == Events.MessageRejected
				&& response.GatepassFeedbackMessage?.FirstOrDefault() is GpNg1035Msg2GatepassFeedbackMessageGatepassFeedbackMessage gatePassFeedbackMsg
				&& gatePassFeedbackMsg != null)
			{
				switch (gatePassFeedbackMsg?.GatepassReturnCode)
				{
					case GatePassMovement.MessageReturnCode.RejectedByCustoms:
						return ILMessageEventParameter.GatePassResponseRejectedByCustomsReason;
					case GatePassMovement.MessageReturnCode.RejectedByOriginSite:
						return ILMessageEventParameter.GatePassResponseRejectedByOriginSiteReason;
				}
			}

			return null;
		}
	}
}
