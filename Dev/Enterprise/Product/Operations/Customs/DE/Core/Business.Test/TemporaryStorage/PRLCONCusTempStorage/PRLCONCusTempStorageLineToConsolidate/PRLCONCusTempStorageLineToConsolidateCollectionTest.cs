using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(PRLCONCusTempStorageLineToConsolidateCollection))]
	public class PRLCONCusTempStorageLineToConsolidateCollectionTest : EU.Business.CusTempStorage.Testing.CusTempStorageLineCollectionToTest
	{
		public void TestSetDefaultsForNew()
		{
			var storageJobHeader = Factory.New<CusTempStorageJobHeader>();
			var storageDec = storageJobHeader.PRLCONCusTempStorageDecs.AddNew();

			var consolidatedLine = storageDec.ConsolidatedLine;

			var lineToConsolidated1 = storageDec.CusTempStorageLines.AddNew();
			AssertEquals(consolidatedLine.PK, lineToConsolidated1.Pivot.ToLine.PK);

			var lineToConsolidated2 = storageDec.CusTempStorageLines.AddNew();
			AssertEquals(consolidatedLine.PK, lineToConsolidated2.Pivot.ToLine.PK);
			AssertNotEquals(lineToConsolidated1.Pivot.PK, lineToConsolidated2.Pivot.PK);
		}

		public void TestRecalculatePackageQuantity()
		{
			var storageJobHeader = Factory.New<CusTempStorageJobHeader>();
			var storageDec = storageJobHeader.PRLCONCusTempStorageDecs.AddNew();
			var line1 = storageDec.CusTempStorageLines.AddNew();
			line1.TSL_PackageQty = 10;
			var line2 = storageDec.CusTempStorageLines.AddNew();
			line2.TSL_PackageQty = 20;

			AssertEquals(30, storageDec.ConsolidatedLine.TSL_PackageQty);

			storageDec.CusTempStorageLines.RemoveAndDelete(line2);
			AssertEquals(10, storageDec.ConsolidatedLine.TSL_PackageQty);
		}

		public void TestTypedSingleParameterAddNew()
		{
			var collection = (PRLCONCusTempStorageLineToConsolidateCollection)GetCollectionToTest();
			var collectionType = collection.GetType();
			var method = collectionType.GetMethod("AddNew", new[] { typeof(Type) });
			var bizO = (BusinessObject)method.Invoke(collection, new object[] { typeof(PRLCONCusTempStorageLineToConsolidate) });
			AssertNotNull(bizO);
			AssertType<PRLCONCusTempStorageLineToConsolidate>(bizO);
		}

		public void TestOnRemovingLineNumberGenerator()
		{
			var collection = (PRLCONCusTempStorageLineToConsolidateCollection)GetCollectionToTest();
			declaration.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			var line1 = collection.AddNew();
			var line2 = collection.AddNew();
			var line3 = collection.AddNew();
			AssertEquals("1|2|3", string.Join("|", line1.TSL_LineNo, line2.TSL_LineNo, line3.TSL_LineNo));

			collection.RemoveAndDelete(line2);
			AssertEquals("1|2", string.Join("|", line1.TSL_LineNo, line3.TSL_LineNo));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<PRLCONCusTempStorageLineToConsolidate>();

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var storageJobHeader = Factory.New<CusTempStorageJobHeader>();
			declaration = storageJobHeader.PRLCONCusTempStorageDecs.AddNew();

			return declaration.CusTempStorageLines;
		}
		PRLCONCusTempStorageDec declaration;
	}
}
