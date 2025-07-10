using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ColumnValueRankerTestForCollection : ColumnValueRankerBaseTest
	{
		public void TestEmptyRanker()
		{
			AssertEquals(null, new ColumnValueRanker().GetBestMatch(collection));
		}

		public void TestCollectionRanking()
		{
			var ranker = GetRanker(SetupDummyBizO(Factory.NewWithValidTestData<DummyBaseBusinessObject>(), SpecificCode, SpecificDescription, SpecificDate, true, 1));
			var items = ranker.GetBestMatch(collection).ToArray();
			AssertEquals("Number of matched Items", 1, items.Length);
			AssertEquals("Z0_Number of Matched Items", 1, items[0].Z0_Number);

			var itemfromDB = ranker.GetBestMatch<DummyBaseBusinessObject>(Factory, new ZQuery());
			AssertNotNull(itemfromDB);
			AssertEquals("Z0_Number of Matched Items", 1, itemfromDB.Z0_Number);

			ranker = GetRanker(SetupDummyBizO(Factory.NewWithValidTestData<DummyBaseBusinessObject>(), SpecificCode, "A Description", SpecificDate, true, 1));
			items = ranker.GetBestMatch(collection).ToArray();
			AssertEquals("Number of matched Items", 1, items.Length);
			AssertEquals("Z0_Number of Matched Items", 3, items[0].Z0_Number);

			itemfromDB = ranker.GetBestMatch<DummyBaseBusinessObject>(Factory, new ZQuery());
			AssertNotNull(itemfromDB);
			AssertEquals("Z0_Number of Matched Items", 3, itemfromDB.Z0_Number);

			ranker = GetRanker(SetupDummyBizO(Factory.NewWithValidTestData<DummyBaseBusinessObject>(), SpecificCode, SpecificDescription, ZDateTime.Today.AddDays(-2), true, 1));
			items = ranker.GetBestMatch(collection).ToArray();
			AssertEquals("Number of matched Items", 1, items.Length);
			AssertEquals("Z0_Number of Matched Items", 4, items[0].Z0_Number);

			itemfromDB = ranker.GetBestMatch<DummyBaseBusinessObject>(Factory, new ZQuery());
			AssertNotNull(itemfromDB);
			AssertEquals("Z0_Number of Matched Items", 4, itemfromDB.Z0_Number);

			ranker = GetRanker(SetupDummyBizO(Factory.NewWithValidTestData<DummyBaseBusinessObject>(), "COD99", SpecificDescription, SpecificDate, true, 2));
			items = ranker.GetBestMatch(collection).ToArray();
			AssertEquals("Number of matched Items", 1, items.Length);
			AssertEquals("Row Number", 2, items[0].Z0_Number);

			itemfromDB = ranker.GetBestMatch<DummyBaseBusinessObject>(Factory, new ZQuery());
			AssertNotNull(itemfromDB);
			AssertEquals("Z0_Number of Matched Items", 2, itemfromDB.Z0_Number);

			ranker = GetRanker(SetupDummyBizO(Factory.NewWithValidTestData<DummyBaseBusinessObject>(), "COD99", "Long Description", SpecificDate, true, 2));
			items = ranker.GetBestMatch(collection).ToArray();
			AssertEquals("Number of matched Items", 1, items.Length);
			AssertEquals("Row Number", 3, items[0].Z0_Number);

			itemfromDB = ranker.GetBestMatch<DummyBaseBusinessObject>(Factory, new ZQuery());
			AssertNotNull(itemfromDB);
			AssertEquals("Z0_Number of Matched Items", 3, itemfromDB.Z0_Number);

			collection.Add(SetupDummyBizO(Factory.NewWithValidTestData<DummyBaseBusinessObject>(), "ALL", "Long Description", SpecificDate, false, 8));

			ranker = GetRanker(SetupDummyBizO(Factory.NewWithValidTestData<DummyBaseBusinessObject>(), "COD99", "Long Description", SpecificDate, true, 2));
			items = ranker.GetBestMatch(collection).ToArray();
			AssertEquals("Number of matched Items", 1, items.Length);
			AssertEquals("Row Number", 8, items[0].Z0_Number);

			itemfromDB = ranker.GetBestMatch<DummyBaseBusinessObject>(Factory, new ZQuery());
			AssertNotNull(itemfromDB);
			AssertEquals("Z0_Number of Matched Items. mismatch in result due to the fact that new item hasn't been save in DB yet.", 3, itemfromDB.Z0_Number);
		}

		public void TestCollectionRanking_NoMatch()
		{
			var ranker = new ColumnValueRanker();
			ranker.Add(DummyBizoSchema.Z0_Code, new ZString("COD"));
			ranker.Add(DummyBizoSchema.Z0_Description, new ZString("Long Description"));
			ranker.Add(DummyBizoSchema.Z0_Date, ZDateTime.BrettsBirthday);

			var items = ranker.GetBestMatch(collection);
			AssertNull(items);
		}

		protected override void SetUp()
		{
			base.SetUp();

			collection = new List<DummyBaseBusinessObject>();
			collection.Add(SetupDummyBizO(Factory.NewWithValidTestData<DummyBaseBusinessObject>(), SpecificCode, SpecificDescription, SpecificDate, true, 1));
			collection.Add(SetupDummyBizO(Factory.NewWithValidTestData<DummyBaseBusinessObject>(), "ALL", SpecificDescription, SpecificDate, true, 2));
			collection.Add(SetupDummyBizO(Factory.NewWithValidTestData<DummyBaseBusinessObject>(), "ALL", "Any Description", SpecificDate, false, 3));
			collection.Add(SetupDummyBizO(Factory.NewWithValidTestData<DummyBaseBusinessObject>(), "ALL", "Any Description", ZDateTime.BrettsBirthday, false, 4));
			collection.Add(SetupDummyBizO(Factory.NewWithValidTestData<DummyBaseBusinessObject>(), "", "Any Description", ZDateTime.BrettsBirthday, false, 5));
			collection.Add(SetupDummyBizO(Factory.NewWithValidTestData<DummyBaseBusinessObject>(), "COD2", SpecificDescription, ZDateTime.Today.AddDays(-2), true, 6));
			collection.Add(SetupDummyBizO(Factory.NewWithValidTestData<DummyBaseBusinessObject>(), "COD2", ZString.Empty, ZDateTime.Empty, true, 7));

			Factory.Save();
		}

		DummyBaseBusinessObject SetupDummyBizO(DummyBaseBusinessObject bizo, ZString z0_Code, ZString z0_Description, ZDateTime z0_Date, ZBool z0_Bool, ZInt z0_Number)
		{
			bizo.Z0_Code = z0_Code;
			bizo.Z0_Description = z0_Description;
			bizo.Z0_Date = z0_Date;
			bizo.Z0_Bool = z0_Bool;
			bizo.Z0_Number = z0_Number;
			return bizo;
		}

		ColumnValueRanker GetRanker(DummyBaseBusinessObject bizo)
		{
			var ranker = new ColumnValueRanker();
			ranker.Add(DummyBizoSchema.Z0_Code, bizo.Z0_Code, new ZString("COD"), new ZString("ALL"), ZString.Empty);
			ranker.Add(DummyBizoSchema.Z0_Description, bizo.Z0_Description, new ZString("Long Description"), new ZString("Any Description"), ZString.Empty);
			ranker.Add(DummyBizoSchema.Z0_Date, bizo.Z0_Date, ZDateTime.BrettsBirthday, ZDateTime.Empty);
			return ranker;
		}

		List<DummyBaseBusinessObject> collection;
	}
}
