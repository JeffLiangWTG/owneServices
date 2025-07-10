using CargoWise.Common;
using CargoWise.Customs.IL.MessageContracts.MAN;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.Business.Message.MessageBuilder;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class ILMAN820MessageBuilder : ILMessageBuilderBase
	{
		public ILMAN820MessageBuilder(AsycudaManifestHeader header, AsycudaManifestQueryMessageSendingObject manifestQueryMessageToSend) : base(header)
		{
			this.header = Argument.NotNull(header, nameof(header));
			this.messageSendingObject = Argument.NotNull(manifestQueryMessageToSend, nameof(manifestQueryMessageToSend));
		}

		protected override ILEDIMessage GetMessage()
			=> header.Factory.New<ILMAN820RequestMessage>();

		protected override IBusinessObjectCollection GetMessageOwnerCollection()
			=> header.Messages;

		protected override string GetMessageText()
		{
			var messageBuilder = new ManifestQueryMessageBuilder(MessageManifestQueryWrapper.NewOrNull(messageSendingObject)) as CargoWise.Customs.Shared.MessageContracts.IXmlMessageBuilder;
			return messageBuilder?.GenerateXmlMessage().GetSerializedString() ?? ZString.Empty;
		}

		readonly AsycudaManifestHeader header;
		readonly AsycudaManifestQueryMessageSendingObject messageSendingObject;
	}
}
