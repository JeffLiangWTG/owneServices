using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRInterchange : EDIInterchange, Integration.Customs.AU.ICMRInterchange
	{
		public CMRInterchange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override string GetMessageNum(string messageText, int messageNumberSequece)
		{
			return new ZString(EI_InterchangeNum + messageNumberSequece.ToString().PadLeft(6, '0')).Left(20);
		}

		protected override Type GetMessageTypeToCreate(ZString messageText)
		{
			SegmentGroup message = new Edifact.D99B.EdifactD99BMessageFactory().GetMessage(new UNOCCMRCharacterSet(), messageText);
			if (message is CUSRESMessage)
			{
				return GetCUSRESMessageTypeToCreate(message as CUSRESMessage);
			}
			else if (message is Edifact.D99B.Messages.CONTRL.CONTRLMessage)
			{
				return typeof(CMRCONTRLMessage);
			}
			else
			{
				return base.GetMessageTypeToCreate(messageText);
			}
		}

		protected Type GetCUSRESMessageTypeToCreate(CUSRESMessage cUSRES)
		{
			ZString documentName = GetDocumentName(cUSRES);
			MessageTypeConverter converter = Factory.GetCachedValue("MessageTypeConverter", delegate
			{ return new MessageTypeConverter(); });
			if (documentName != Enterprise.Customs.AU.Declaration.Business.CMRMessage.CMRMessageTypes.ERM)
			{
				return converter.GetTypeForMessageCode(documentName);
			}
			else
			{
				return converter.GetTypeForERMMessageCode(cUSRES.GetRelatedDocumentType());
			}
		}

		protected ZString GetDocumentName(CUSRESMessage cUSRES)
		{
			return cUSRES.BGM[0].DocumentMessageName.DocumentName;
		}

		protected override bool ShouldSendViaEHubCore => true;

		public LogsForNominatedEvent InterchangeInProgressLogs
		{
			get { return interchangeInProgressLogs ?? (interchangeInProgressLogs = new LogsForNominatedEvent(Logs, Events.InterchangeInProgress)); }
		}
		protected LogsForNominatedEvent interchangeInProgressLogs;

		protected override bool EI_NeedsAcknowledgementCore
		{
			get
			{
				return EI_BodyText.IndexOf("+CUSRES:D:99B", StringComparison.InvariantCulture) != -1 &&
									(eHubID.IsEmpty || !eHubMessagingRegistry.Instance.AUSuppressCONTRLAcknowledgements.Value);
			}
		}

		protected override ZString NewInterchangeStatusCore(ZString oldInterchangeStatus)
		{
			return !eHubMessagingRegistry.Instance.AUSuppressResends.Value && oldInterchangeStatus == EDIInterchange.Status.Sent ?
								new ZString(EDIInterchange.Status.SendPending) : base.NewInterchangeStatusCore(oldInterchangeStatus);
		}

		public void MarkAsFailedToBeSent(ZString reference)
		{
			EI_Status = EDIInterchange.Status.Failed;
			Logs.AddNew(Events.InterchangeFailedToBeSent, reference);
			foreach (EDIMessage message in ContainedMessages)
			{
				message.EM_Status = EDIMessage.Status.Failed;
				message.Logs.AddNew(Events.InterchangeFailedToBeSent);
			}
		}

		protected override void ResetToQueuedStatusCore()
		{
			base.ResetToQueuedStatusCore();

			if (EI_ReceiveTransmit.Equals(Direction.Transmit) && EI_Status == EDIInterchange.Status.Failed)
			{
				EI_Status = EDIInterchange.Status.SendPending;
				EI_RetryCount = 0;

				foreach (EDIMessage message in ContainedMessages)
				{
					message.EM_Status = EDIMessage.Status.Sent;
				}
			}
		}
	}
}
