using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	class FinalSumAWithoutPreliminarySenderTest : TemporaryStorageSenderAbstractTest<FinalSumAWithoutPreliminarySender>
	{
		protected override CusTempStorageDec GetTempStorageDecToTest()
		{
			var storageDec = CUSPRLCusTempStorageDec.LoadOrCreate(storageJobHeader);
			storageDec.CusEntryNumber.CE_EntryNum = "ATB150002110520195880";
			storageDec.CusTempStorageLines.AddNew();
			return storageDec;
		}

		protected override TemporaryStorageSender GetTempStorageSender() => new FinalSumAWithoutPreliminarySender(TempStorageDec);

		protected override ZString ExpectedMessageTypeATLASVersion10_1 => nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.SCPRLK);

		protected override ZString ExpectedMessageSubType => Messaging.TemporaryStorageMessageSubTypeList.Codes.SummaryDeclarationAfterPresentation;
	}
}
