using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(InlandTransportCollection))]
	class InlandTransportCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestMaxCountValidation()
		{
			var collection = (ISupportMaxCountValidation)GetCollectionToTest();
			AssertEquals(999, collection.MaxCountValidator.MaxCount);
		}

		public void TestCY_Type()
		{
			var collection = (InlandTransportCollection)GetCollectionToTest();
			AssertEquals(NctsConstants.CusCodeDataTypes.TransportInland, collection.CY_Type);
		}

		public void TestDataAndCodeList()
		{
			var collection = (InlandTransportCollection)GetCollectionToTest();
			collection.AddNew("DE", "DE001");
			collection.AddNew("FR", "FR002");
			AssertEquals("Count is 2", 2, collection.Count);
			AssertEquals("ContainsCode 'DE'", "DE001", collection.DataAndCodeList.Single(x => x.Item1 == "DE").Item2);
			AssertEquals("ContainsCode 'FR'", "FR002", collection.DataAndCodeList.Single(x => x.Item1 == "FR").Item2);

			var list = new List<(ZString, ZString)>();
			list.Add(("ES", "ES003"));
			collection.DataAndCodeList = list.AsReadOnly();
			AssertEquals("Count is 1", 1, collection.Count);
			AssertEquals("ContainsCode 'ES'", "ES003", collection.DataAndCodeList.Single(x => x.Item1 == "ES").Item2);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var parent = Factory.New<NctsDepartureMovementHeader>();
			return new InlandTransportCollection(parent);
		}
	}
}
