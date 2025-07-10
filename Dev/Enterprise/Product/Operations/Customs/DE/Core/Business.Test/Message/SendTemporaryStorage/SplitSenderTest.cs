using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	class SplitSenderTest : TemporaryStorageSenderAbstractTest<SplitSender>
	{
		protected override CusTempStorageDec GetTempStorageDecToTest()
		{
			var result = storageJobHeader.CUSPCSCusTempStorageDecs.AddNew();
			result.CusTempStorageLines.AddNew();
			return result;
		}
		protected override TemporaryStorageSender GetTempStorageSender() => new SplitSender(TempStorageDec);

		protected override ZString ExpectedMessageTypeATLASVersion10_1 => nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.SCPCSH);

		protected override ZString ExpectedMessageSubType => Messaging.TemporaryStorageMessageSubTypeList.Codes.Split;
	}
}
