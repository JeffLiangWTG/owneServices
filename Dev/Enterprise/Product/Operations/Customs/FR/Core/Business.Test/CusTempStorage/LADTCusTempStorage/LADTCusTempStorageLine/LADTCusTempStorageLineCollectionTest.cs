using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	[TestedType(typeof(LADTCusTempStorageLineCollection))]
	class LADTCusTempStorageLineCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<LADTCusTempStorageLine>();

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var storageDec = Factory.New<LADTCusTempStorageDec>();
			return storageDec.CusTempStorageLines;
		}

		public void TestDefaultTSL_GrossWeightUQ()
		{
			var line = ((LADTCusTempStorageLineCollection)GetCollectionToTest()).AddNew();
			AssertEquals("The default value of TSL_GrossWeightUQ is Kilograms.", Core.Constants.Weight.Kilograms, line.TSL_GrossWeightUQ);
		}
	}
}
