using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class NctsMessageSendingAction
	{
		public NctsMessageSendingAction(EU.NCTS.Business.NctsHeaderMessageSendingObject sendingObject)
		{
			this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
			NctsHeader = (NctsHeader)sendingObject.NctsHeader;
			MessageType = sendingObject.MessageType;
			ReleaseRequest = sendingObject.ReleaseRequest;
			Justification = sendingObject.Justification;
		}

		public readonly NctsHeader NctsHeader;

		public ZString MessageType { get; set; }

		public ZString ReleaseRequest { get; set; }

		public ZString Justification { get; set; }

		public void AddMessage(GbEDIMessage message)
		{
			NctsHeader.LinkedMessages.Add(message);
		}

		public string MessageCreated(string messageText) => sendingObject.MessageCreated(messageText);

		readonly EU.NCTS.Business.NctsHeaderMessageSendingObject sendingObject;
	}
}
