using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	[TestedType(typeof(ISTCusTempStorageLineCollection))]
	class ISTCusTempStorageLineCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<ISTCusTempStorageLine>();

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var storageDec = Factory.New<ISTCusTempStorageDec>();
			return storageDec.CusTempStorageLines;
		}

		public void TestDefaultTSL_GrossWeightUQ()
		{
			var line = ((ISTCusTempStorageLineCollection)GetCollectionToTest()).AddNew();
			AssertEquals("The default value of TSL_GrossWeightUQ is Kilograms.", Core.Constants.Weight.Kilograms, line.TSL_GrossWeightUQ);
		}
	}
}
