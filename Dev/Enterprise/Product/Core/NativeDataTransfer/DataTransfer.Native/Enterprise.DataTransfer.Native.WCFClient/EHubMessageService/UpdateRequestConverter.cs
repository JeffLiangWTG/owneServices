using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Integration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DataTransfer.Native.WCFClient.EHubMessageService
{
	[CodeAlive("The class used by EHubMessageRoutingServiceClient through spring defined in EnterpriseApplicationConfiguration.xml")]
	public class UpdateRequestConverter : IConverter<IRequestMessage, MessageSendRequest>
	{
		public MessageSendRequest Convert(IRequestMessage input)
		{
			var message = new MessageSendRequest
			{
				MessageAction = "UpdateData",
				Message = input.Message,
				MessageID = input.MessageID,
				MessageEntity = input.MessageEntity,
				RecipientID = input.RecipientID,
				RecipientType = input.RecipientType,
				SenderID = input.SenderID,
				SenderType = input.SenderType,
				SenderUsername = input.SenderUsername
			};
			return message;
		}
	}
}
