using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	class PreliminarySumASenderTest : TemporaryStorageSenderAbstractTest<PreliminarySumASender>
	{
		protected override CusTempStorageDec GetTempStorageDecToTest()
		{
			var storageDec = CUSPRLCusTempStorageDec.LoadOrCreate(storageJobHeader);
			storageDec.CusEntryNumber.CE_EntryNum = "ATB150002110520195881";
			storageDec.CusTempStorageLines.AddNew();
			return storageDec;
		}

		protected override TemporaryStorageSender GetTempStorageSender() => new PreliminarySumASender(TempStorageDec);

		protected override ZString ExpectedMessageTypeATLASVersion10_1 => nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.SCPRLK);

		protected override ZString ExpectedMessageSubType => Messaging.TemporaryStorageMessageSubTypeList.Codes.PreliminarySummaryDeclaration;
	}
}
