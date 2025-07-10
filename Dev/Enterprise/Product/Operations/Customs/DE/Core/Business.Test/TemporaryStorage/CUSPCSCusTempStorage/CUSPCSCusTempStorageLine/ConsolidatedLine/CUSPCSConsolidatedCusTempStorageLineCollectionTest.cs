using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CUSPCSConsolidatedCusTempStorageLineCollection))]
	class CUSPCSConsolidatedCusTempStorageLineCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<CUSPCSConsolidatedCusTempStorageLine>();

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var storageJobHeader = Factory.New<CusTempStorageJobHeader>();
			var storageDec = storageJobHeader.CUSPCSCusTempStorageDecs.AddNew();
			return new CUSPCSConsolidatedCusTempStorageLineCollection(storageDec);
		}
	}
}
