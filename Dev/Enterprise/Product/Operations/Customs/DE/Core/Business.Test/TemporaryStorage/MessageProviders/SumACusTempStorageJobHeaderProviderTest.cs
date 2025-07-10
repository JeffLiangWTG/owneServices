using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	class SumACusTempStorageJobHeaderProviderTest : CusTempStorageJobHeaderProviderAbstractTest<SumACusTempStorageJobHeaderProvider>
	{
		protected override ITempStorageHeader GetTempStorageHeaderWrapped(CusTempStorageJobHeader tempStorageHeader) => new SumACusTempStorageJobHeaderProvider(tempStorageHeader);
	}
}
