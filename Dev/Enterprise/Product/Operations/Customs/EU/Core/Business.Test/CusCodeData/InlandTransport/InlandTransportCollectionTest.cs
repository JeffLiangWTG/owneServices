using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(InlandTransportCollection))]
	sealed class InlandTransportCollectionTest : BusinessObjectCollectionTestCase
	{
		[ExpectNoExceptions]
		public void TestDataAndCodeList()
		{
			var collection = (InlandTransportCollection)GetCollectionToTest();
			var item1 = collection.AddNew();
			item1.Nationality = "DE";
			item1.CY_Data = "DE001";
			var item2 = collection.AddNew();
			item2.Nationality = "FR";
			item2.CY_Data = "FR002";
			NUnit.Framework.Assert.That(collection.Count, NUnit.Framework.Is.EqualTo(2), "Count is 2");
			NUnit.Framework.Assert.That(collection.DataAndCodeList.Single(x => x.Item1 == "DE").Item2, NUnit.Framework.Is.EqualTo("DE001").Using(CustomComparers.TypeComparison), "ContainsCode 'DE'");
			NUnit.Framework.Assert.That(collection.DataAndCodeList.Single(x => x.Item1 == "FR").Item2, NUnit.Framework.Is.EqualTo("FR002").Using(CustomComparers.TypeComparison), "ContainsCode 'FR'");

			var list = new List<(ZString, ZString)>();
			list.Add(("ES", "ES003"));
			collection.DataAndCodeList = list.AsReadOnly();
			NUnit.Framework.Assert.That(collection.Count, NUnit.Framework.Is.EqualTo(1), "Count is 1");
			NUnit.Framework.Assert.That(collection.DataAndCodeList.Single(x => x.Item1 == "ES").Item2, NUnit.Framework.Is.EqualTo("ES003").Using(CustomComparers.TypeComparison), "ContainsCode 'ES'");
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var parent = Factory.New<JobDeclaration>();
			return new InlandTransportCollection(parent);
		}
	}
}
