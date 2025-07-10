using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	class ReExportSenderTest : TemporaryStorageSenderAbstractTest<ReExportSender>
	{
		protected override CusTempStorageDec GetTempStorageDecToTest()
		{
			var storageDec = REXDISCusTempStorageDec.LoadOrCreate(storageJobHeader);
			storageDec.CusTempStorageLines.AddNew();
			return storageDec;
		}

		protected override TemporaryStorageSender GetTempStorageSender() => new ReExportSender(TempStorageDec);

		protected override ZString ExpectedMessageTypeATLASVersion10_1 => nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.SREXDJ);

		protected override ZString ExpectedMessageSubType => Messaging.TemporaryStorageMessageSubTypeList.Codes.ReExport;
	}
}
