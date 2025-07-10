using CargoWise.Common;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class MXInboundMessageCreator : IInboundMessageCreator
	{
		public void CreateMessagesForInterchange(EDIInterchange interchange)
		{
			var bodyText = interchange.EI_InterchangeType == MXMessageConstants.XER ? interchange.EI_HeaderText : interchange.EI_BodyText;
			if (bodyText.IsEmpty || (interchange.EI_InterchangeType != MXMessageConstants.XER && !XmlUtils.IsValidXml(bodyText)))
			{
				interchange.EI_Status = EDIInterchange.Status.Error; 
				interchange.Logs.AddNew(Events.ErrorReport, "NO MX CUSTOMS DATA");
			}
			else
			{
				var newEDIMessage = interchange.ContainedMessages.AddNew();
				newEDIMessage.EM_MessageNum = interchange.EI_InterchangeNum.Right(EDIMessage.Schema.EM_MessageNumMaxLength);
				newEDIMessage.EM_MessageText = bodyText;
				newEDIMessage.EM_MessageType = interchange.EI_InterchangeType;
				newEDIMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				newEDIMessage.EM_Status = EDIMessage.Status.Queued;

				if (MessageTypes.ShouldLinkRequestMessageByTrackingId(newEDIMessage.EM_MessageType))
				{
					var factory = newEDIMessage.Factory;
					var originalInterchange = MXInboundMessageCreatorHelper.GetSentInterchangeWithTrackingId(interchange.EI_SessionGUID, factory);
					if (originalInterchange != null)
					{
						var originalMessage = MXInboundMessageCreatorHelper.GetOriginalMessage(originalInterchange.PK, factory);
						if (originalMessage != null)
						{
							newEDIMessage.EM_LinkUniqueID = originalMessage.EM_LinkUniqueID;
							newEDIMessage.EM_LinkTable = originalMessage.EM_LinkTable;
						}
					}

					var messageAttachee = newEDIMessage.EM_LinkedObject as IMessageAttachee;
					if (messageAttachee != null)
					{
						newEDIMessage.EM_GB = messageAttachee.GlobalBranchPK;
					}
				}
				else
				{
					var messageAttachee = newEDIMessage.EM_MessageType == MessageTypes.Codes.MXA ? MXMessageHelper.FindRelevantBusinessObjectFirstSeaResponse(bodyText, interchange) : MXMessageHelper.FindRelevantBusinessObjectFinalSeaResponse(bodyText, interchange);

					if (messageAttachee != null)
					{
						newEDIMessage.EM_LinkUniqueID = messageAttachee.PK;
						newEDIMessage.EM_LinkTable = messageAttachee.TableName;
						newEDIMessage.EM_GB = messageAttachee.GlobalBranchPK;
					}
				}
			}
		}
	}
}
