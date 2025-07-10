using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CHGTSTCusTempStorageLineCollection))]
	class CHGTSTCusTempStorageLineCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultsForNewChild()
		{
			CombineAssertions(() =>
			{
				var storageDec = Factory.New<CHGTSTCusTempStorageDec>();
				storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
				var storageLine1 = storageDec.CusTempStorageLines.AddNew();
				AssertEquals("Multiple Types to select from for Dec AWB", ZString.Empty, storageLine1.TSL_OwnerReferenceType);
				storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
				AssertEquals("Default to REG for Dec type REG", OwnerReferenceTypeList.Codes.REG, storageLine1.TSL_OwnerReferenceType);
				var storageLine2 = storageDec.CusTempStorageLines.AddNew();
				AssertEquals("Added lines will also be REG", OwnerReferenceTypeList.Codes.REG, storageLine2.TSL_OwnerReferenceType);
				storageDec.STH_IdentificationIndicator = "XYZ";
				var storageLine3 = storageDec.CusTempStorageLines.AddNew();
				AssertEquals("Line 1 not changed", OwnerReferenceTypeList.Codes.REG, storageLine1.TSL_OwnerReferenceType);
				AssertEquals("Line 2 not changed", OwnerReferenceTypeList.Codes.REG, storageLine2.TSL_OwnerReferenceType);
				AssertEquals("Line 3 not defaulted as Dec is invalid type", ZString.Empty, storageLine3.TSL_OwnerReferenceType);
			});
		}

		public void TestTypedSingleParameterAddNew()
		{
			var collection = (CHGTSTCusTempStorageLineCollection)GetCollectionToTest();
			var collectionType = collection.GetType();
			var method = collectionType.GetMethod("AddNew", new Type[] { typeof(Type) });
			var bizO = (BusinessObject)method.Invoke(collection, new object[] { typeof(CHGTSTCusTempStorageLine) });
			AssertNotNull(bizO);
			AssertType<CHGTSTCusTempStorageLine>(bizO);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<CHGTSTCusTempStorageLine>();

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var storageDec = Factory.New<CHGTSTCusTempStorageDec>();
			return storageDec.CusTempStorageLines;
		}
	}
}
