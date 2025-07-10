using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestsSubclassesOf(typeof(CusTempStorageLine))]
	public abstract class CusTempStorageLineTest<T> : CusTempStorageLineTestCase<T>
		where T : CusTempStorageLine
	{
		public void TestCanDelete()
		{
			var storageLine = GetNewCusTempStorageLine();
			storageLine.Dec.STH_MessageStatus = Common.Shared.MessageStatusList.Codes.Sent;
			Assert("storageLine.CanDelete", !storageLine.CanDelete);
			AssertEquals("storageLine.ReasonForNotAbleToDelete", "Line cannot be deleted as customs messaging has occurred", storageLine.ReasonForNotAbleToDelete);
			storageLine.Dec.STH_MessageStatus = ZString.Empty;
			Assert("storageLine.CanDelete", storageLine.CanDelete);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewCusTempStorageLine();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewCusTempStorageLine();

		protected override BusinessObject GetNewBusinessObject() => GetNewCusTempStorageLine();
	}
}
