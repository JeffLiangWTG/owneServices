using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.MasterFiles;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(CusCodeDataWithOrderCollectionForTest))]
	class CusCodeDataWithOrderCollectionBaseOnlyTest : CusCodeDataCollectionTest<CusCodeDataWithOrderForTest>
	{
		[ExpectNoExceptions]
		public void TestAsString()
		{
			var collection = (CusCodeDataWithOrderCollectionForTest)GetCusCodeDataCollection();
			NUnit.Framework.Assert.That(collection.Count, NUnit.Framework.Is.EqualTo(0));
			collection.AsString = "QWER,TYUI,ASDF";
			NUnit.Framework.Assert.That(collection.Count, NUnit.Framework.Is.EqualTo(3));
			collection.AddNew("GHJK");
			NUnit.Framework.Assert.That(collection.AsString, NUnit.Framework.Is.EqualTo("QWER,TYUI,ASDF,GHJK").Using(CustomComparers.TypeComparison));
		}

		public void TestGetAllCodes()
		{
			var collection = (CusCodeDataWithOrderCollectionForTest)GetCusCodeDataCollection();
			NUnit.Framework.Assert.That(collection.GetAllCodes().Count(), NUnit.Framework.Is.EqualTo(0), "When the CusCodeDataWithOrderCollection has no elements, GetAllCodes() Count");

			collection.AddNew("1111");
			collection.AddNew("2222");
			collection.AddNew("");
			collection.AddNew("2222");
			collection.AddNew(" ");
			collection.AddNew("3333");
			AssertContainsExactElementsInAnyOrder("When the CusCodeDataWithOrderCollection has elements, GetAllCodes()", new string[] { "1111", "2222", "2222", "3333" }, collection.GetAllCodes());
		}

		[ExpectNoExceptions]
		public void TestDefaultOfCY_Order()
		{
			var collection = (CusCodeDataWithOrderCollectionForTest)GetCusCodeDataCollection();
			NUnit.Framework.Assert.That(collection.AddNew().CY_Order, NUnit.Framework.Is.EqualTo((short)1).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(collection.AddNew().CY_Order, NUnit.Framework.Is.EqualTo((short)2).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(collection.AddNew().CY_Order, NUnit.Framework.Is.EqualTo((short)3).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestNumberOfCodesAllowed()
		{
			var collection = (CusCodeDataWithOrderCollectionForTest)GetCusCodeDataCollection();
			AssertNumberOfCodes(collection, 8);
			var classification = Factory.New<CusClassification>();
			var newCollection = new CusCodeDataWithOrderCollectionForTest(classification.CC_EcAdditionalSupplementsInfo, 10);
			AssertNumberOfCodes(newCollection, 10);
		}

		[ExpectNoExceptions]
		void AssertNumberOfCodes(CusCodeDataWithOrderCollectionForTest collection, int size)
		{
			for (int i = 0; i < size; i++)
			{
				NUnit.Framework.Assert.That(collection.AllowNew, NUnit.Framework.Is.True, "Can Add " + i);
				collection.AddNew();
			}
			NUnit.Framework.Assert.That(!collection.AllowNew, NUnit.Framework.Is.True, "Cannot add more than " + size + " codes");
		}

		protected override CusCodeDataCollection<CusCodeDataWithOrderForTest> GetCusCodeDataCollection()
		{
			var classification = Factory.New<CusClassification>();
			return new CusCodeDataWithOrderCollectionForTest(classification.CC_EcAdditionalSupplementsInfo, 8);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<CusCodeDataWithOrderForTest>();
			result.CY_Order = 3;
			return result;
		}
	}

	class CusCodeDataWithOrderCollectionForTest : CusCodeDataWithOrderCollection<CusCodeDataWithOrderForTest>
	{
		public CusCodeDataWithOrderCollectionForTest(ZPropertyInfo info, short size, short startOrder = 1) : base(info, "TES", size, startOrder)
		{
		}
	}
}
