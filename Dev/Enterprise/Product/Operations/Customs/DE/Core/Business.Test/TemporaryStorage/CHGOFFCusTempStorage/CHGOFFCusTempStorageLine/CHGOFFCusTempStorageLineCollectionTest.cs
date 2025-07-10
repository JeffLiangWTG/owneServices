using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CHGOFFCusTempStorageLineCollection))]
	class CHGOFFCusTempStorageLineCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestTypedSingleParameterAddNew()
		{
			var collection = (CHGOFFCusTempStorageLineCollection)GetCollectionToTest();
			var collectionType = collection.GetType();
			var method = collectionType.GetMethod("AddNew", new Type[] { typeof(Type) });
			var bizO = (BusinessObject)method.Invoke(collection, new object[] { typeof(CHGOFFCusTempStorageLine) });
			AssertNotNull(bizO);
			AssertType<CHGOFFCusTempStorageLine>(bizO);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<CHGOFFCusTempStorageLine>();

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var storageDec = Factory.New<CHGOFFCusTempStorageDec>();
			return storageDec.CusTempStorageLines;
		}
	}
}
