using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	class ChangeCustodyInformationSenderTest : TemporaryStorageSenderAbstractTest<ChangeCustodyInformationSender>
	{
		protected override CusTempStorageDec GetTempStorageDecToTest()
		{
			var result = storageJobHeader.CHGTSTCusTempStorageDecs.AddNew();
			result.STH_OwnerReferenceNumber = LogbookRegNum;
			result.CusTempStorageLines.AddNew();
			return result;
		}

		protected override TemporaryStorageSender GetTempStorageSender() => new ChangeCustodyInformationSender(TempStorageDec);

		protected override ZString ExpectedMessageTypeATLASVersion10_1 => nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.SCHTSG);

		protected override ZString ExpectedMessageSubType => Messaging.TemporaryStorageMessageSubTypeList.Codes.ChangeCustodianEntitledTrader;

		protected override ZString ExpectedLogbookRegNumForIndicatorREG => LogbookRegNum;

		const string LogbookRegNum = "ATB150002110520195876";
	}
}

