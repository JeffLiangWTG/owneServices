using CargoWise.Types;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	[TestedType(typeof(DummyDependentBusinessObjectCollection))]
	sealed class DummyDependentBusinessObjectCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new DummyDependentBusinessObjectCollection(Factory.New<DummyBusinessObject>(), Factory);
		}

		#region IFindBoxListProvider

		public void TestDescriptionFromPrimaryKey()
		{
			DummyDependantBusinessObject dummy1 = Factory.New<DummyDependantBusinessObject>();
			dummy1.ZD1_Code = "AAA";
			dummy1.ZD1_NumberUnitCode = "BBB";
			Collection.Add(dummy1);

			DummyDependantBusinessObject dummy2 = Factory.New<DummyDependantBusinessObject>();
			dummy2.ZD1_Code = "CCC";
			dummy2.ZD1_NumberUnitCode = "DDD";
			Collection.Add(dummy2);

			AssertEquals("Matches PK", dummy2.ZD1_NumberUnitCode, ((IFindBoxListProvider)Collection).DescriptionFromPrimaryKey(dummy2.PK));
			AssertNull("Non-existent item", ((IFindBoxListProvider)Collection).DescriptionFromPrimaryKey(ZGuid.NewZGuid()));
			AssertNull("Invalid PK", ((IFindBoxListProvider)Collection).DescriptionFromPrimaryKey(ZGuid.Invalid));
		}

		#endregion
	}
}
