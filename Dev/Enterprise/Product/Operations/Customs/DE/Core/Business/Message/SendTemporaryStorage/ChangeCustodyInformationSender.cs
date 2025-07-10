using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class ChangeCustodyInformationSender : TemporaryStorageSender
	{
		public ChangeCustodyInformationSender(CusTempStorageDec storageDec)
			: base(storageDec, TemporaryStorageMessageBuilderLoader.ChangeCustodyInformation, new CHGTSTCusTempStorageDecProvider(storageDec))
		{
		}

		protected override string RegistrationNumber => IdentificationIdicatorIsREG ? ((ICHGTSTTempStorageDec)dataProvider).OwnerReferenceNumber : string.Empty;

		protected override string MessageSubType => TemporaryStorageMessageSubTypeList.Codes.ChangeCustodianEntitledTrader;
	}
}
