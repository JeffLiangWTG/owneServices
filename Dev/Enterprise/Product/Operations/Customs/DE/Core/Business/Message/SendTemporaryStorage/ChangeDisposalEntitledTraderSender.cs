using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class ChangeDisposalEntitledTraderSender : TemporaryStorageSender
	{
		public ChangeDisposalEntitledTraderSender(CusTempStorageDec storageDec)
			: base(storageDec, TemporaryStorageMessageBuilderLoader.ChangeDisposalEntitledTrader, new CHGOFFCusTempStorageDecProvider(storageDec))
		{
		}

		protected override string RegistrationNumber => IdentificationIdicatorIsREG ? ((ICHGOFFTempStorageDec)dataProvider).OwnerReferenceNumber : string.Empty;

		protected override string MessageSubType => TemporaryStorageMessageSubTypeList.Codes.ChangeCustodianEntitledTrader;
	}
}
