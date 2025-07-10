using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public class CLInboundMessageCreator : IInboundMessageCreator
	{
		public void CreateMessagesForInterchange(EDIInterchange interchange)
		{
			var bodyText = interchange.EI_BodyText;
			if (interchange.EI_HeaderText.IsEmpty || bodyText.IsEmpty || !XmlUtils.IsValidXml(bodyText))
			{
				interchange.EI_Status = EDIInterchange.Status.Error;
				interchange.Logs.AddNew(Events.ErrorReport, "NO CL CUSTOMS DATA");
			}
			else
			{
				var newEDIMessage = interchange.ContainedMessages.AddNew();
				newEDIMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				newEDIMessage.EM_MessageNum = interchange.EI_InterchangeNum.Right(EDIMessage.Schema.EM_MessageNumMaxLength);
				newEDIMessage.EM_Status = EDIMessage.Status.Queued;
				newEDIMessage.EM_MessageText = bodyText;
				FillMessageType(newEDIMessage, interchange.EI_HeaderText);
			}
		}

		void FillMessageType(EDIMessage message, ZString headerText)
		{
			var headerData = JsonSerializer.Deserialize<Dictionary<string, string>>(headerText, new JsonSerializerOptions
			{
				AllowTrailingCommas = true
			});
			var fileName = ZString.Empty;
			foreach (var dataKey in headerData.Keys)
			{
				if (dataKey == "custom.FileName")
				{
					fileName = headerData[dataKey];
				}
			}

			if (!fileName.IsEmpty)
			{
				var messageNum = fileName.Split('-').Last().Replace(".xml","");
				var original = CLMessageHelper.GetEDIMessageRequest(messageNum, message.Factory);
				if (original != null)
				{
					message.EM_MessageType = original.EM_MessageType;
					message.EM_LinkedObject = original.EM_LinkedObject;
				}
			}
		}
	}
}
