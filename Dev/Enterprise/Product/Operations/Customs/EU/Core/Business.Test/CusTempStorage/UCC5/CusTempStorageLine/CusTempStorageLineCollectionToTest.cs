using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageLineCollectionTo<CusTempStorageLine, CusTempStorageLine>))]
	public class CusTempStorageLineCollectionToTest : BusinessObjectCollectionTestCase
	{
		public void TestLoadElements()
		{
			var cusTempStorageDec = Factory.New<CusTempStorageDec>();
			var line1 = Factory.New<CusTempStorageLine>();
			line1.TSL_STH = cusTempStorageDec.PK;

			var line2 = Factory.New<CusTempStorageLine>();
			line2.TSL_STH = cusTempStorageDec.PK;

			var divot = Factory.New<CusTempStorageLinePivot>();
			divot.SLR_TSL_FromLine = line1.PK;
			divot.SLR_TSL_ToLine = line2.PK;

			var collection = new CusTempStorageLineCollectionTo<CusTempStorageLine, CusTempStorageLine>(line1);
			collection.Load();

			AssertContainsExactElementsInAnyOrder(new[] { line2.PK }, collection.Select(x => x.PK));
		}

		public void TestSetDefaultsForNewChild()
		{
			var cusTempStorageDec = Factory.New<CusTempStorageDec>();
			var line = Factory.New<CusTempStorageLine>();
			line.TSL_STH = cusTempStorageDec.PK;

			var collection = new CusTempStorageLineCollectionTo<CusTempStorageLine, CusTempStorageLine>(line);

			var newLine = collection.AddNew();

			AssertEquals(cusTempStorageDec.PK, newLine.TSL_STH);
			Assert(!cusTempStorageDec.HasChanges);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var cusTempStorageDec = Factory.New<CusTempStorageDec>();
			var line = Factory.New<CusTempStorageLine>();
			line.TSL_STH = cusTempStorageDec.PK;

			return new CusTempStorageLineCollectionTo<CusTempStorageLine, CusTempStorageLine>(line);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CusTempStorageLine>();
		}

		public override void TestRemoveFromRelationship()
		{
			// Element will not be removed directly
			Assert(true);
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			// Element will not be added to collection via Binding.
			Assert(true);
		}
	}
}
