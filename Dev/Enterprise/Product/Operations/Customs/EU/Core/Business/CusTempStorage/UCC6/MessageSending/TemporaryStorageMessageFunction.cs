using CargoWise.Types;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public abstract class TemporaryStorageMessageFunction
	{
		public ZString MessageType => MessageTypeCore;
		protected abstract ZString MessageTypeCore { get; }

		public ZString SentCustomsStatus => SentCustomsStatusCore;
		protected virtual ZString SentCustomsStatusCore => ZString.Empty;

		public ZString SentMessageStatus => SentMessageStatusCore;
		protected virtual ZString SentMessageStatusCore => PNTSMessageStatusList.Codes.Sent;
	}
}
