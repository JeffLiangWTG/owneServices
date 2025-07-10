using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageLineCollection))]
	class CusTempStorageLineCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<CusTempStorageLine>();

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var storageDec = Factory.New<CusTempStorageDec>();
			return storageDec.CusTempStorageLines;
		}
	}
}
