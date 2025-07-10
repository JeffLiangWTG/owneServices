using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class ChangeOwnerReferenceSender : TemporaryStorageSender
	{
		public ChangeOwnerReferenceSender(CusTempStorageDec storageDec)
			: base(storageDec, TemporaryStorageMessageBuilderLoader.ChangeOwnerReference, new CHGSPOCusTempStorageDecProvider(storageDec))
		{
		}

		protected override string RegistrationNumber => ((ICHGSPOTempStorageDec)dataProvider).OwnerReferenceNumber;

		protected override string MessageSubType => TemporaryStorageMessageSubTypeList.Codes.ChangeOwnerReference;
	}
}
