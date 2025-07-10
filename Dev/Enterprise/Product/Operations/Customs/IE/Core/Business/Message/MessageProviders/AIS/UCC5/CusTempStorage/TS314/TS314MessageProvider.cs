using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage
{
	class TS314MessageProvider : MessageProvider, ITS314Header
	{
		public TS314MessageProvider(TemporaryStorageMessageSendingObject sendingObject)
		{
			messageSendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
		}
		protected readonly TemporaryStorageMessageSendingObject messageSendingObject;

		public ITS314DeclarationType Declaration => CachedValueHelper.GetValue(ref declarationCached, () => new TS314DeclarationTypeProvider(messageSendingObject));
		CachedValue<ITS314DeclarationType> declarationCached;
	}
}
