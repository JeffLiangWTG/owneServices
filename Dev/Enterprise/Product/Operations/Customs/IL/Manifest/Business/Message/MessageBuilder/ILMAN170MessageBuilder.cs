using CargoWise.Customs.IL.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.Business.Message.MessageBuilder;
using Enterprise.Messaging.Business.MessageBuilders;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class ILMAN170MessageBuilder : ILMessageBuilderBase
	{
		public ILMAN170MessageBuilder(AsycudaManifestHeader header, AsycudaManifestMessageSendingObject messageSendingObject) : base(header)
		{
			this.header = Argument.NotNull(header, nameof(header));
			this.messageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
		}

		protected override void AfterFullSuccess(IBuilderResult builderResult)
		{
			var manifestHeader = (AsycudaManifestHeader)builderResult.Owner;
			manifestHeader.AMA_MessageStatus = ASYCUDA.Business.MessageStatusCodeList.Codes.Awaiting;
		}

		protected override string GetMessageText()
		{
			var messageBuilder = new ManifestMessageBuilder(MessageManifestWrapper.NewOrNull(header)) as IXmlMessageBuilder;
			return messageBuilder?.GenerateXmlMessage().GetSerializedString() ?? ZString.Empty;
		}

		protected override ILEDIMessage GetMessage() => header.Factory.New<ILMAN170RequestMessage>();

		protected override IBusinessObjectCollection GetMessageOwnerCollection()
			=> header.Messages;

		protected override bool IsAwaitingResponse
			=> messageSendingObject.Header.IsAwaitingResponse();

		readonly AsycudaManifestHeader header;
		readonly AsycudaManifestMessageSendingObject messageSendingObject;
	}
}

