using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(REXDISCusTempStorageReExportLineCollection))]
	class REXDISCusTempStorageReExportLineCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestOnRemoving()
		{
			var dec = Factory.New<REXDISCusTempStorageDec>();
			var collection = dec.CusTempStorageLines;
			AssertType<REXDISCusTempStorageReExportLineCollection>(collection);
			var line1 = collection.AddNew();
			AssertEquals("Pre-req", true, line1.SequenceNumberEnabled);

			var line2 = collection.AddNew();
			var line3 = collection.AddNew();
			AssertEquals("1 2 3", string.Join(" ", line1.TSL_LineNo, line2.TSL_LineNo, line3.TSL_LineNo));

			collection.RemoveAndDelete(line2);
			AssertEquals("1 2", string.Join(" ", line1.TSL_LineNo, line3.TSL_LineNo));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var cusTempStorageDec = Factory.New<REXDISCusTempStorageDec>();
			return new REXDISCusTempStorageReExportLineCollection(cusTempStorageDec);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<REXDISCusTempStorageReExportLine>();
	}
}
