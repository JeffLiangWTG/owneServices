
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ExportsOtherMessagesStatusCalculator : CMRStatusCalculator<JobDeclaration>
	{
		public ExportsOtherMessagesStatusCalculator(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected internal override ZString[] InterestedMessageTypes
		{
			get
			{
				return new ZString[]
				{
					CMRMessage.CMRMessageTypes.WARREL,
					CMRMessage.CMRMessageTypes.WARRET,
					CMRMessage.CMRMessageTypes.DEPREC,
					CMRMessage.CMRMessageTypes.DEPREL,
					CMRMessage.CMRMessageTypes.DRWBCK,
					CMRMessage.CMRMessageTypes.SAM
				};
			}
		}

		protected internal override ZPropertyInfo StatusInfo => Parent.JE_MessageStatusInfo;

		internal bool IsWARRELMessage(EDIMessage outgoingMessage) => outgoingMessage.EM_MessageType == CMRMessage.CMRMessageTypes.WARREL;

		internal bool IsWARRETMessage(EDIMessage outgoingMessage) => outgoingMessage.EM_MessageType == CMRMessage.CMRMessageTypes.WARRET;

		internal bool IsDEPRECMessage(EDIMessage outgoingMessage) => outgoingMessage.EM_MessageType == CMRMessage.CMRMessageTypes.DEPREC;

		internal bool IsDEPRELMessage(EDIMessage outgoingMessage) => outgoingMessage.EM_MessageType == CMRMessage.CMRMessageTypes.DEPREL;

		bool IsDRWBCKMessage(EDIMessage outgoingMessage) => outgoingMessage.EM_MessageType == CMRMessage.CMRMessageTypes.DRWBCK;

		internal bool IsOriginal(EDIMessage outgoingMessage)
		{
			var message = outgoingMessage as CMRMessage;
			return message != null && message.EM_MessageSubType == CMRMessage.MessageSubTypes.Original;
		}

		internal bool IsAmendment(EDIMessage outgoingMessage)
		{
			var message = outgoingMessage as CMRMessage;
			return message != null && message.EM_MessageSubType == CMRMessage.MessageSubTypes.Amendment;
		}

		internal bool IsWithdrawl(EDIMessage outgoingMessage)
		{
			var message = outgoingMessage as CMRMessage;
			return message != null && message.EM_MessageSubType == CMRMessage.MessageSubTypes.Withdraw;
		}

		protected internal override ZString GetAcceptedResponseStatus(EDIMessage outgoingMessage)
		{
			ZString result = "?";
			if (IsWARRELMessage(outgoingMessage))
			{
				if (IsOriginal(outgoingMessage))
				{
					result = CustomsEntryStatus.ClearWARRELOriginal.Code;
				}
				else if (IsAmendment(outgoingMessage))
				{
					result = CustomsEntryStatus.ClearWARRELReplacement.Code;
				}
				else if (IsWithdrawl(outgoingMessage))
				{
					result = CustomsEntryStatus.ClearWARRELWithdrawal.Code;
				}
			}
			else if (IsWARRETMessage(outgoingMessage))
			{
				if (IsOriginal(outgoingMessage))
				{
					result = CustomsEntryStatus.ClearWARRETOriginal.Code;
				}
				else if (IsAmendment(outgoingMessage))
				{
					result = CustomsEntryStatus.ClearWARRETReplacement.Code;
				}
			}
			else if (IsDEPRECMessage(outgoingMessage))
			{
				if (IsOriginal(outgoingMessage))
				{
					result = CustomsEntryStatus.ClearDEPRECOriginal.Code;
				}
				else if (IsAmendment(outgoingMessage))
				{
					result = CustomsEntryStatus.ClearDEPRECReplacement.Code;
				}
				else if (IsWithdrawl(outgoingMessage))
				{
					result = CustomsEntryStatus.ClearDEPRECWithdrawal.Code;
				}
			}
			else if (IsDEPRELMessage(outgoingMessage))
			{
				if (IsOriginal(outgoingMessage))
				{
					result = CustomsEntryStatus.ClearDEPRELOriginal.Code;
				}
				else if (IsAmendment(outgoingMessage))
				{
					result = CustomsEntryStatus.ClearDEPRELReplacement.Code;
				}
				else if (IsWithdrawl(outgoingMessage))
				{
					result = CustomsEntryStatus.ClearDEPRELWithdrawal.Code;
				}
			}
			else if (IsDRWBCKMessage(outgoingMessage))
			{
				if (IsOriginal(outgoingMessage))
				{
					result = CustomsEntryStatus.ClearOriginal.Code;
				}
				else if (IsAmendment(outgoingMessage))
				{
					result = CustomsEntryStatus.ClearAmendment.Code;
				}
				else if (IsWithdrawl(outgoingMessage))
				{
					result = CustomsEntryStatus.ClearWithdrawal.Code;
				}
			}
			return result;
		}

		protected internal override ZString GetAwaitingResponseStatus(EDIMessage outgoingMessage)
		{
			ZString result = "?";

			if (IsWARRELMessage(outgoingMessage))
			{
				if (IsOriginal(outgoingMessage))
				{
					result = CustomsEntryStatus.AwaitingWARRELOriginal.Code;
				}
				else if (IsAmendment(outgoingMessage))
				{
					result = CustomsEntryStatus.AwaitingWARRELReplacement.Code;
				}
				else if (IsWithdrawl(outgoingMessage))
				{
					result = CustomsEntryStatus.AwaitingWARRELWithdrawal.Code;
				}
			}
			else if (IsWARRETMessage(outgoingMessage))
			{
				if (IsOriginal(outgoingMessage))
				{
					result = CustomsEntryStatus.AwaitingWARRETOriginal.Code;
				}
				else if (IsAmendment(outgoingMessage))
				{
					result = CustomsEntryStatus.AwaitingWARRETReplacement.Code;
				}
			}
			else if (IsDEPRECMessage(outgoingMessage))
			{
				if (IsOriginal(outgoingMessage))
				{
					result = CustomsEntryStatus.AwaitingDEPRECOriginal.Code;
				}
				else if (IsAmendment(outgoingMessage))
				{
					result = CustomsEntryStatus.AwaitingDEPRECReplacement.Code;
				}
				else if (IsWithdrawl(outgoingMessage))
				{
					result = CustomsEntryStatus.AwaitingDEPRECWithdrawal.Code;
				}
			}
			else if (IsDEPRELMessage(outgoingMessage))
			{
				if (IsOriginal(outgoingMessage))
				{
					result = CustomsEntryStatus.AwaitingDEPRELOriginal.Code;
				}
				else if (IsAmendment(outgoingMessage))
				{
					result = CustomsEntryStatus.AwaitingDEPRELReplacement.Code;
				}
				else if (IsWithdrawl(outgoingMessage))
				{
					result = CustomsEntryStatus.AwaitingDEPRELWithdrawal.Code;
				}
			}
			else if (IsDRWBCKMessage(outgoingMessage))
			{
				if (IsOriginal(outgoingMessage))
				{
					result = CustomsEntryStatus.AwaitingOriginal.Code;
				}
				else if (IsAmendment(outgoingMessage))
				{
					result = CustomsEntryStatus.AwaitingAmendment.Code;
				}
				else if (IsWithdrawl(outgoingMessage))
				{
					result = CustomsEntryStatus.AwaitingWithdrawal.Code;
				}
			}
			return result;
		}

		protected internal override ZString GetRejectedResponseStatus(EDIMessage outgoingMessage, EDIMessage incomingMessage)
		{
			ZString result = "?";

			if (IsWARRELMessage(outgoingMessage))
			{
				if (IsOriginal(outgoingMessage))
				{
					result = CustomsEntryStatus.FailWARRELOriginal.Code;
				}
				else if (IsAmendment(outgoingMessage))
				{
					result = CustomsEntryStatus.FailWARRELReplacement.Code;
				}
				else if (IsWithdrawl(outgoingMessage))
				{
					result = CustomsEntryStatus.FailWARRELWithdrawal.Code;
				}
			}
			else if (IsWARRETMessage(outgoingMessage))
			{
				if (IsOriginal(outgoingMessage))
				{
					result = CustomsEntryStatus.FailWARRETOriginal.Code;
				}
				else if (IsAmendment(outgoingMessage))
				{
					result = CustomsEntryStatus.FailWARRETReplacement.Code;
				}
			}
			else if (IsDEPRECMessage(outgoingMessage))
			{
				if (IsOriginal(outgoingMessage))
				{
					result = CustomsEntryStatus.FailDEPRECOriginal.Code;
				}
				else if (IsAmendment(outgoingMessage))
				{
					result = CustomsEntryStatus.FailDEPRECReplacement.Code;
				}
				else if (IsWithdrawl(outgoingMessage))
				{
					result = CustomsEntryStatus.FailDEPRECWithdrawal.Code;
				}
			}
			else if (IsDEPRELMessage(outgoingMessage))
			{
				if (IsOriginal(outgoingMessage))
				{
					result = CustomsEntryStatus.FailDEPRELOriginal.Code;
				}
				else if (IsAmendment(outgoingMessage))
				{
					result = CustomsEntryStatus.FailDEPRELReplacement.Code;
				}
				else if (IsWithdrawl(outgoingMessage))
				{
					result = CustomsEntryStatus.FailDEPRELWithdrawal.Code;
				}
			}
			else if (IsDRWBCKMessage(outgoingMessage))
			{
				if (IsOriginal(outgoingMessage))
				{
					result = CustomsEntryStatus.FailOriginal.Code;
				}
				else if (IsAmendment(outgoingMessage))
				{
					result = CustomsEntryStatus.FailAmendment.Code;
				}
				else if (IsWithdrawl(outgoingMessage))
				{
					result = CustomsEntryStatus.FailWithdrawal.Code;
				}
			}
			return result;
		}

		protected override bool AcceptedWithErrorsSupported => true;

		protected internal override ZString GetAcceptedWithErrorsResponseStatus(EDIMessage outgoingMessage, EDIMessage incomingMessage)
		{
			ZString result = "?";

			if (IsWARRELMessage(outgoingMessage))
			{
				result = GetAcceptedResponseStatus(outgoingMessage);
			}
			else if (IsWARRETMessage(outgoingMessage))
			{
				result = GetAcceptedResponseStatus(outgoingMessage);
			}
			else if (IsDEPRECMessage(outgoingMessage))
			{
				if (IsOriginal(outgoingMessage))
				{
					result = CustomsEntryStatus.ErrorDEPRECOriginal.Code;
				}
				else if (IsAmendment(outgoingMessage))
				{
					result = CustomsEntryStatus.ErrorDEPRECReplacement.Code;
				}
				else if (IsWithdrawl(outgoingMessage))
				{
					result = CustomsEntryStatus.ErrorDEPRECWithdrawal.Code;
				}
			}
			else if (IsDEPRELMessage(outgoingMessage))
			{
				if (IsOriginal(outgoingMessage))
				{
					result = CustomsEntryStatus.ErrorDEPRELOriginal.Code;
				}
				else if (IsAmendment(outgoingMessage))
				{
					result = CustomsEntryStatus.ErrorDEPRELReplacement.Code;
				}
				else if (IsWithdrawl(outgoingMessage))
				{
					result = CustomsEntryStatus.ErrorDEPRELWithdrawal.Code;
				}
			}
			else if (IsDRWBCKMessage(outgoingMessage))
			{
				result = GetAcceptedResponseStatus(outgoingMessage);
			}
			return result;
		}

		protected override CodeDescriptionPairList StatusList => Factory.GetCachedValue<CodeDescriptionPairList>();

		protected override ZString GetDefaultStatus() => ZString.Empty;
	}
}
