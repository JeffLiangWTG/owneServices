using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	public class IIDMessageWrapper : ICAEDIFACTMessageAttachee
	{
		public IIDMessageWrapper(CusEntryHeader entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
		}

		readonly CusEntryHeader entryHeader;
		readonly JobDeclaration declaration;

		public ZString AmendmentReason => declaration.CA_AmendReasonCode;

		public ZBool IsMessageValidationPassed
		{
			get
			{
				var cusEntryNum = CusEntryNumber.Load(declaration, CusEntryNumber.EntryType.CATransactionNumber, Core.Constants.CountryCodes.Canada, true);
				return cusEntryNum != null && cusEntryNum.CE_EntryStatus == MessageProcessors.MessageValidationPassedMessageProcessor.IsOnFile;
			}
		}

		public IIDUniversalShipmentMessage AddNew()
		{
			var message = (((IEDIFACTMessageAttachee)this).Factory).New<IIDUniversalShipmentMessage>();
			((IEDIFACTMessageAttachee)this).AddMessage(message);
			return message;
		}

		public void PopulateEntrySubmittedDateIfRequired()
		{
			this.entryHeader.PopulateEntrySubmittedDateIfRequired();
		}

		#region ICAEDIFACTMessageAttachee Members

		bool ICAEDIFACTMessageAttachee.IsCancelled
		{
			get { return declaration.IsCancelled; }
		}

		#endregion

		#region IEDIFACTMessageAttachee

		bool IEDIFACTMessageAttachee.HasChanges
		{
			get { return declaration.HasChanges; }
		}

		ZString IEDIFACTMessageAttachee.JobIdentification
		{
			get { return declaration.JE_DeclarationReference; }
		}

		ZString IEDIFACTMessageAttachee.JobStatus
		{
			get { return entryHeader.CH_EntryStatus; }
			set { entryHeader.CH_EntryStatus = value; }
		}

		ZString IEDIFACTMessageAttachee.MessageStatus
		{
			get { return entryHeader.CH_Status; }
			set { entryHeader.CH_Status = value; }
		}

		BusinessObject IEDIFACTMessageAttachee.TopLevelBusinessObject
		{
			get { return declaration; }
		}

		void IEDIFACTMessageAttachee.AddMessage(Enterprise.Messaging.Business.EDIMessage message)
		{
			entryHeader.Messages.Add(message);
		}

		bool IEDIFACTMessageAttachee.RefreshValidationBeforeSendMessage
		{
			get { return declaration.RefreshValidationBeforeSendMessage; }
		}

		#endregion

		#region IEDIMessageCollectionProvider

		BusinessObjectFactory IEDIMessageCollectionProvider.Factory
		{
			get { return entryHeader.Factory; }
		}

		Enterprise.Messaging.Business.EDIMessageCollection IEDIMessageCollectionProvider.Messages
		{
			get { return entryHeader.Messages; }
		}

		public EDIMessage LatestSentAcceptedMessage
		{
			get
			{
				EDIMessage result = null;
				var acceptedMsg = entryHeader.MessagesForDisplay.Where(m => m.EM_ReceiveTransmit == EDIMessage.Direction.Receive && ((m as UniversalEventMessage)?.EventType ?? string.Empty) == AutoEvents.MessageAcceptedCode).OrderByDescending(y => y.EM_SystemCreateTimeUtc).FirstOrDefault();

				if (acceptedMsg != null)
				{
					result = GetLatestSentMessage(acceptedMsg.EM_SystemCreateTimeUtc);
				}
				return result;
			}
		}

		public bool HasAcceptedWithdrawMessage
		{
			get
			{
				bool result = false;
				var acceptedMsg = entryHeader.MessagesForDisplay.Where(m => m.EM_ReceiveTransmit == EDIMessage.Direction.Receive && ((m as UniversalEventMessage)?.EventType ?? string.Empty) == AutoEvents.MessageAcceptedCode);

				foreach (var msg in acceptedMsg)
				{
					var sentMsg = GetLatestSentMessage(msg.EM_SystemCreateTimeUtc);
					result = sentMsg != null && sentMsg.EM_MessageSubType == IIDMessageSubTypeList.Codes.Cancellation;
				}
				return result;
			}
		}

		protected EDIMessage GetLatestSentMessage(ZDateTime acceptedRcvMsgTimeUtc)
		{
			return entryHeader.Messages.OfType<EDIMessage>().Where(x => x.EM_SystemCreateTimeUtc <= acceptedRcvMsgTimeUtc
																		&& x.EM_ApplicationCode == Enterprise.Messaging.Integration.ApplicationCodeList.Codes.CAIMP && x.IsTransmitMessage && x.EM_Status == MessageStatusList.Codes.Sent
																		&& x.EM_MessageType == UniversalEventMessageTypes.Codes.IIDResponses).OrderByDescending(y => y.EM_SystemCreateTimeUtc).FirstOrDefault();
		}

		#endregion

	}
}
