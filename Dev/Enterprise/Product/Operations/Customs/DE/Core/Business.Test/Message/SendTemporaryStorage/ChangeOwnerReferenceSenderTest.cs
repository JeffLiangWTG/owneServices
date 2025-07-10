using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	class ChangeOwnerReferenceSenderTest : TemporaryStorageSenderAbstractTest<ChangeOwnerReferenceSender>
	{
		protected override CusTempStorageDec GetTempStorageDecToTest()
		{
			var storageDec = storageJobHeader.CHGSPOCusTempStorageDecs.AddNew();
			storageDec.STH_OwnerReferenceNumber = LogbookRegNum;
			return storageDec;
		}

		protected override TemporaryStorageSender GetTempStorageSender() => new ChangeOwnerReferenceSender(TempStorageDec);

		protected override ZString ExpectedMessageTypeATLASVersion10_1 => nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.SCHSPE);

		protected override ZString ExpectedMessageSubType => Messaging.TemporaryStorageMessageSubTypeList.Codes.ChangeOwnerReference;

		protected override ZString ExpectedLogbookRegNumForIndicatorREG => LogbookRegNum;

		protected override ZString ExpectedLogbookRegNumForIndicatorNotREG => LogbookRegNum;

		const string LogbookRegNum = "ATB150002110520195878";
	}
}
