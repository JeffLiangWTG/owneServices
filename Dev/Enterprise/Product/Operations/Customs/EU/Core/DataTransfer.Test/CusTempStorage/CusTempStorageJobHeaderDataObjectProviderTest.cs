using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.EU.DataTransfer.CusTempStorage.Testing
{
	class CusTempStorageJobHeaderDataObjectProviderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestGetTempStorageJobHeaderDataObjectWriter()
		{
			var header = Factory.New<CusTempStorageJobHeader>();
			var provider = new CusTempStorageJobHeaderDataObjectProvider();
			AssertType<CusTempStorageJobHeaderDataObjectWriter>(provider.GetTempStorageJobHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(null, header))));
		}
	}
}
