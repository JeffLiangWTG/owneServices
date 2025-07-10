using CargoWise.Common;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AR.Manifest.Business
{
	public class ARInboundMessageCreator : IInboundMessageCreator
	{
		public void CreateMessagesForInterchange(EDIInterchange interchange)
		{
			var bodyText = interchange.EI_BodyText;
			if (bodyText.IsEmpty || !XmlUtils.IsValidXml(bodyText))
			{
				interchange.EI_Status = EDIInterchange.Status.Error;
				interchange.Logs.AddNew(Events.ErrorReport, "NO AR CUSTOMS DATA");
			}
			else
			{
				var newEDIMessage = interchange.ContainedMessages.AddNew();
				newEDIMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				newEDIMessage.EM_MessageNum = interchange.EI_InterchangeNum.Right(EDIMessage.Schema.EM_MessageNumMaxLength);
				newEDIMessage.EM_MessageType = interchange.EI_InterchangeType;
				newEDIMessage.EM_Status = EDIMessage.Status.Queued;
				newEDIMessage.EM_MessageText = bodyText;
			}
		}
	}
}
