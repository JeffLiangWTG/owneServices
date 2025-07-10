using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(REXDISCusTempStorageSumALineCollection))]
	class REXDISCusTempStorageSumALineCollectionTest : EU.Business.CusTempStorage.Testing.CusTempStorageLineCollectionToTest
	{
		public void TestSetDefaults()
		{
			var cusTempStorageDec = Factory.New<REXDISCusTempStorageDec>();
			var line = Factory.New<REXDISCusTempStorageReExportLine>();
			line.TSL_STH = cusTempStorageDec.PK;
			cusTempStorageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;

			var collection = new REXDISCusTempStorageSumALineCollection(line);
			var newLine = collection.AddNew();

			AssertEquals(cusTempStorageDec.PK, newLine.TSL_STH);
			AssertEquals(TemporaryStorageIdentificationIndicatorList.Codes.REG, newLine.TSL_OwnerReferenceType);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var cusTempStorageDec = Factory.New<REXDISCusTempStorageDec>();
			var reExportLine = cusTempStorageDec.CusTempStorageLines.AddNew();
			return reExportLine.CusTempStorageSumALines;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<REXDISCusTempStorageSumALine>();
	}
}
