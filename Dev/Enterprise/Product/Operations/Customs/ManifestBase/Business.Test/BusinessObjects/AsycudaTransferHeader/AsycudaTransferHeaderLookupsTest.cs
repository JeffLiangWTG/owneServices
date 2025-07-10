using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ManifestBase.Testing
{
	public class AsycudaTransferHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTransferTypeList()
		{
			var transferHeader = Factory.New<AsycudaTransferHeader>();
			var lookups = transferHeader.Lookups;
			var transferTypeList = lookups.TransferTypeList;
			AssertType<TransferTypeList>(transferTypeList);
			AssertSame("Is Cached", transferTypeList, lookups.TransferTypeList);
		}
	}
}
