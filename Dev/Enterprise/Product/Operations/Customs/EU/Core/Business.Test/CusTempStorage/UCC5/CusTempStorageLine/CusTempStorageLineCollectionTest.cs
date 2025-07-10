using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageLineCollection<CusTempStorageLine, CusTempStorageDec>))]
	public class CusTempStorageLineCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var cusTempStorageDec = Factory.New<CusTempStorageDec>();
			return new CusTempStorageLineCollection<CusTempStorageLine, CusTempStorageDec>(cusTempStorageDec);
		}
	}
}
