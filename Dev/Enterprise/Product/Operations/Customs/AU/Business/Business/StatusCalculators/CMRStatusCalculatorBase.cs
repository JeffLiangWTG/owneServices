using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CMRStatusCalculatorBase<ParentT> : StatusCalculator<ParentT>, ICalculatedCusStatusCalculator where ParentT : BusinessObject, IStatusNeedsRecalculationProvider
	{
		public CMRStatusCalculatorBase(ParentT parent)
			: base(parent)
		{
		}

		public void ResetToOriginal()
		{
			foreach (EDIMessage message in MessagesWeAreInterestedIn)
			{
				message.EM_Status = EDIMessage.Status.Discarded;
			}
			DeriveStatus();
		}

		protected sealed override void DeriveStatus()
		{
			if (!CMRStatusRecalculationSuspender.IsStatusRecalculationSuspended(Factory))
			{
				OnDerivingStatus?.Invoke(this, EventArgs.Empty);
				DeriveStatusCore();
			}
		}

		public event EventHandler OnDerivingStatus;

		protected virtual void DeriveStatusCore()
		{
			bool outgoingMessageRejected = false;
			bool outgoingMessageAccepted = false;
			bool outgoingMessageAcceptedWithErrors = false;
			EDIMessage[] sortedMessages = GetSortedMessages(MessagesWeAreInterestedIn);
			var furtherSplitMessagesToProcess = AreFurtherSplitMessagesPending(sortedMessages);
			EDIMessage currentIncomingMessage = null;
			ZString status = furtherSplitMessagesToProcess ? ZString.Empty : GetDefaultStatus();

			if (!furtherSplitMessagesToProcess)
			{
				for (int i = 0; i < sortedMessages.Length; i++)
				{
					if (sortedMessages[i].EM_ReceiveTransmit == EDIMessage.Direction.Transmit)
					{
						if (outgoingMessageAcceptedWithErrors)
						{
							status = GetAcceptedWithErrorsResponseStatus(sortedMessages[i], currentIncomingMessage);
						}
						else if (outgoingMessageAccepted)
						{
							status = GetAcceptedResponseStatus(sortedMessages[i]);
						}
						else if (outgoingMessageRejected || sortedMessages[i].EM_Status == EDIMessage.Status.Rejected)
						{
							status = GetRejectedResponseStatus(sortedMessages[i], currentIncomingMessage);
						}
						else
						{
							status = GetAwaitingResponseStatus(sortedMessages[i]);
						}

						break;
					}
					else if (sortedMessages[i].EM_ReceiveTransmit == EDIMessage.Direction.Receive)
					{
						if (!outgoingMessageRejected && !outgoingMessageAcceptedWithErrors)
						{
							currentIncomingMessage = sortedMessages[i];
							if (sortedMessages[i].EM_MessageSubType == CMRMessage.ManifestResponseSubTypes.Rejected)
							{
								outgoingMessageRejected = true;
							}
							else if (AcceptedWithErrorsSupported && sortedMessages[i].EM_MessageSubType == CMRMessage.ManifestResponseSubTypes.Error)
							{
								outgoingMessageAcceptedWithErrors = true;
							}
							else
							{
								status = GetStatusFromInboundMessage(sortedMessages[i]);
								if (status.IsEmpty)
								{
									outgoingMessageAccepted = true;
								}
								else
								{
									break;
								}
							}
						}
					}
				}
			}

			if (!status.IsEmpty && !StatusInfo.BizObj.IsDeleted)
			{
				if (StatusInfo.Value.ToString() != status && StatusInfo.BizObj.IsInDatabase && ShouldLogStatusChange(status))
				{
					LogStatusChangeInMessageBranchTimeZone(currentIncomingMessage, status);
				}
				StatusInfo.Value = status;
			}
		}

		protected virtual bool ShouldLogStatusChange(ZString status) { return true; }

		void LogStatusChangeInMessageBranchTimeZone(EDIMessage incomingMessage, ZString status)
		{
			var branchPk = incomingMessage is CMRCARSTMessage ? incomingMessage.EM_GB : ZGuid.Empty;

			using (branchPk.IsValid && branchPk != GlbBranch.CurrentBranch.PK ? DisposableEnvironment.ForBranch(branchPk.ToGuid()) : null)
			{
				StatusInfo.BizObj.GetLogs().AddNew(Events.StatusChange, StatusChangedEventLogPrefix + status);
			}
		}

		bool AreFurtherSplitMessagesPending(EDIMessage[] messages)
		{
			var result = false;

			if (messages.Length > 0)
			{
				if ((messages[0].EM_MessageType == CMRMessage.CMRMessageTypes.AIROUT || messages[0].EM_MessageType == CMRMessage.CMRMessageTypes.SEAOUT)
					&& messages[0].EM_ReceiveTransmit == EDIMessageStatusList.Codes.Received)
				{
					foreach (EDIMessage message in messages)
					{
						if (message.EM_Status == EDIMessage.Status.Pending || message.EM_Status == EDIMessage.Status.Queued)
						{
							result = true;
							break;
						}
					}
				}
			}

			return result;
		}

		protected virtual ZString GetDefaultStatus() => CMRBaseStatuses.Codes.NotSent;

		protected internal virtual ZString StatusChangedEventLogPrefix => ZString.Empty;

		protected virtual IEnumerable<EDIMessage> Messages => StatusNeedsRecalculationProvider.Messages.Cast<EDIMessage>();

		protected EDIMessage[] MessagesWeAreInterestedIn
		{
			get
			{
				return Messages.Where(bizO =>
								bizO.EM_ApplicationCode == EDIInterchange.ApplicationCodes.CMR
							&& InterestedMessageTypes.Contains(bizO.EM_MessageType)
							&& (InterestedInPendingMessages || bizO.EM_Status != EDIMessage.Status.Pending)
							&& bizO.EM_Status != EDIMessage.Status.Discarded
							&& bizO.EM_Status != EDIMessage.Status.Cancelled
					).ToArray();
			}
		}

#if DEBUG
		protected virtual
#endif
 EDIMessage[] GetSortedMessages(EDIMessage[] messages)
		{
			Array.Sort(messages, new AUEDIMessageComparer(ListSortDirection.Descending));
			return messages;
		}

		protected internal virtual ZString GetStatusFromInboundMessage(EDIMessage message)
		{
			ZString result = ZString.Empty;
			CUSRESMessage cUSRES = message.GetAutoEdifactMessageUsingNamedFactory(AuEdifactMessageFactory.AUCMessageFactory, new Edifact.UNOCCMRCharacterSet()) as CUSRESMessage;
			if (cUSRES != null)
			{
				foreach (FTXSegment fTX in cUSRES.FTX)
				{
					if (fTX.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.StatusDetails)
					{
						result = GetCargoStatusCodeFromDescription(fTX.TextLiteral.FreeTextValue2);
						break;
					}
				}
			}
			return result;
		}

		protected override void OnStatusCalculated()
		{
			base.OnStatusCalculated();

			foreach (EDIMessage message in MessagesWeAreInterestedIn)
			{
				var cusResMessage = message as CMRCUSRESMessage;
				if (cusResMessage != null)
				{
					cusResMessage.ResetCUSRESCache();
				}
			}
		}
		protected virtual bool AcceptedWithErrorsSupported => false;

		protected internal virtual ZString GetAcceptedWithErrorsResponseStatus(EDIMessage outgoingMessage, EDIMessage incomingMessage) => ZString.Empty;

		protected ZString GetCargoStatusCodeFromDescription(ZString statusDescription)
		{
			foreach (CodeDescriptionPair pair in StatusList)
			{
				if (pair.Description.StartsWith(statusDescription))
				{
					return pair.Code;
				}
			}
			return ZString.Empty;
		}

		ZString ICalculatedCusStatusCalculator.UserFriendlyStatusText
		{
			get
			{
				ZString result = ZString.Empty;
				EDIMessage[] sortedMessages = GetSortedMessages(MessagesWeAreInterestedIn);
				for (int i = 0; i < sortedMessages.Length; i++)
				{
					if (sortedMessages[i].EM_ReceiveTransmit == EDIMessage.Direction.Receive)
					{
						result = GetAllCustomsStatusesFromInboundMessage(sortedMessages[i]);
						break;
					}
				}
				return result;
			}
		}

		internal ZString GetAllCustomsStatusesFromInboundMessage(EDIMessage message)
		{
			ZStringBuilder result = new ZStringBuilder();
			CUSRESMessage cUSRES = message.GetAutoEdifactMessageUsingNamedFactory(AuEdifactMessageFactory.AUCMessageFactory, new Edifact.UNOCCMRCharacterSet()) as CUSRESMessage;
			if (cUSRES != null)
			{
				ZBool cargoNotConsolidation = false;
				foreach (FTXSegment fTX in cUSRES.FTX)
				{
					if (fTX.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.StatusDetails)
					{
						if (fTX.TextLiteral.FreeTextValue1.IndexOf("CARGO NOT A CONSOLIDATION") != -1)
						{
							cargoNotConsolidation = true;
						}
						else if (fTX.TextLiteral.FreeTextValue2.IndexOf("YES") == -1)
						{
							result.Append(fTX.TextLiteral.FreeTextValue1 + " : " + fTX.TextLiteral.FreeTextValue2);
							result.Append("\r\n");
						}
					}
				}

				if (cargoNotConsolidation)
				{
					result.Append("\r\n========================================\r\nWarning: Cargo is not a consolidation.\r\n========================================");
				}
			}
			return result.ToString();
		}

		protected abstract CodeDescriptionPairList StatusList { get; }

		protected internal abstract ZString GetRejectedResponseStatus(EDIMessage outgoingMessage, EDIMessage incomingMessage);
		protected internal abstract ZString GetAwaitingResponseStatus(EDIMessage outgoingMessage);
		protected internal abstract ZString GetAcceptedResponseStatus(EDIMessage outgoingMessage);

		protected internal abstract ZPropertyInfo StatusInfo { get; }
		protected internal abstract ZString[] InterestedMessageTypes { get; }
		protected internal virtual bool InterestedInPendingMessages => false;

		protected IStatusNeedsRecalculationProvider StatusNeedsRecalculationProvider => Parent;

		#region ICalculatedCusStatusCalculator Members

		void ICalculatedCusStatusCalculator.DeriveStatusIfEmptyWithMessages()
		{
			if (StatusInfo.Value.IsEmpty && StatusNeedsRecalculationProvider.Messages.Count > 0)
			{
				DeriveStatus();
			}
		}

		#endregion
	}
}
