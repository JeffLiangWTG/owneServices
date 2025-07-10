using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	class ConsolidationSenderTest : TemporaryStorageSenderAbstractTest<ConsolidationSender>
	{
		protected override CusTempStorageDec GetTempStorageDecToTest() => storageJobHeader.PRLCONCusTempStorageDecs.AddNew();

		protected override TemporaryStorageSender GetTempStorageSender() => new ConsolidationSender(TempStorageDec);

		protected override ZString ExpectedMessageTypeATLASVersion10_1 => nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.SPCONH);

		protected override ZString ExpectedMessageSubType => Messaging.TemporaryStorageMessageSubTypeList.Codes.Consolidation;
	}
}
