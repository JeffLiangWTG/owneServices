using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(TreatmentActiveIngredientCollection))]
	sealed class TreatmentActiveIngredientCollectionTest : CusCodeDataCollectionTest<TreatmentActiveIngredient>
	{
		public void TestSetDefaultsForNewChild_Order()
		{
			var collection = (TreatmentActiveIngredientCollection)Collection;
			var obj1 = collection.AddNew();
			var obj2 = collection.AddNew();
			var obj3 = collection.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("Obj1 Order", new ZShort(1), obj1.CY_Order);
				AssertEquals("Obj2 Order", new ZShort(2), obj2.CY_Order);
				AssertEquals("Obj3 Order", new ZShort(3), obj3.CY_Order);
			});
		}

		protected override CusCodeDataCollection<TreatmentActiveIngredient> GetCusCodeDataCollection()
		{
			var process = Factory.New<QuarantineExDocEstablishmentAndTime>();
			return new TreatmentActiveIngredientCollection(process);
		}
	}
}
