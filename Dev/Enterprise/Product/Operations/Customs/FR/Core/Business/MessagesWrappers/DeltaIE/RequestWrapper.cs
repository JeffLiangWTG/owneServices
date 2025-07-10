using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.FR.Business.MessageSending;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class RequestWrapper : IRequest
	{
		RequestWrapper(DeltaIEJobDeclarationMessageSendingObject sendingObject)
		{
			this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
		}

		readonly DeltaIEJobDeclarationMessageSendingObject sendingObject;

		public static RequestWrapper New(DeltaIEJobDeclarationMessageSendingObject sendingObject) => sendingObject == null ? null : new RequestWrapper(sendingObject);

		public string OperatorRequestReference => sendingObject.OperatorRequestReference;

		public string AmendmentRequestDateAndTime => sendingObject.DateTime.ToString("yyyy-MM-ddTHH:mm:ss");

		public string AmendmentReason => sendingObject.VOCReason;

		public string AmendmentMotivation => sendingObject.ChangeAcknowledgementIndicator;
	}
}
