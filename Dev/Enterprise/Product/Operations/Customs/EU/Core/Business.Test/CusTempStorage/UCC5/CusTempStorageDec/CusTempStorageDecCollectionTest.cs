using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageDecCollection<CusTempStorageDec, CusTempStorageJobHeader>))]
	public class CusTempStorageDecCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var storageHeader = Factory.New<CusTempStorageJobHeader>();
			return new CusTempStorageDecCollection<CusTempStorageDec, CusTempStorageJobHeader>(storageHeader);
		}
	}
}
