using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class CusPersonLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRelationshipCodeList()
		{
			var person = Factory.New<CusPerson>();
			var lookup = new CusPersonLookups(person);
			AssertEquals(lookup.GetType(), person.Lookups.GetType());

			var list = lookup.RelationshipCodeList;
			AssertEquals(11, list.Count);
			AssertEquals("00", list[0].Code);
			AssertEquals("본인", list[0].Description);

			AssertEquals("01", list[1].Code);
			AssertEquals("아버지", list[1].Description);

			AssertEquals("02", list[2].Code);
			AssertEquals("어머니", list[2].Description);

			AssertEquals("03", list[3].Code);
			AssertEquals("자녀", list[3].Description);

			AssertEquals("04", list[4].Code);
			AssertEquals("손자, 손녀", list[4].Description);

			AssertEquals("05", list[5].Code);
			AssertEquals("조부모", list[5].Description);

			AssertEquals("06", list[6].Code);
			AssertEquals("형제", list[6].Description);

			AssertEquals("07", list[7].Code);
			AssertEquals("숙모", list[7].Description);

			AssertEquals("08", list[8].Code);
			AssertEquals("삼촌", list[8].Description);

			AssertEquals("09", list[9].Code);
			AssertEquals("사촌", list[9].Description);

			AssertEquals("10", list[10].Code);
			AssertEquals("조카", list[10].Description);
		}

		public void TestRelationShipCodeListAndAddRegistry()
		{
			var collection = FamilyRelationCollection.GetDefaultFamilyRelationCollection();
			var newValue = collection.AddNew();
			newValue.Code = "11";
			newValue.Description = "당숙";

			KRCustomsRegistry.Instance.FamilyRelations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var person = Factory.New<CusPerson>();
			var list = new CusPersonLookups(person).RelationshipCodeList;
			AssertEquals(12, list.Count);
			AssertEquals("11", list[11].Code);
			AssertEquals("당숙", list[11].Description);
		}

		public void TestImmigrantJobCodeList()
		{
			var person = Factory.New<CusPerson>();
			var lookup = new CusPersonLookups(person);
			CombineAssertions(() =>
			{
				AssertEquals(20, lookup.ImmigrantJobCodeList.Count);
				AssertEquals("01, 02, 03, 04, 05, 06, 07, 11, 12, 13, 19, 20, 21, 22, 23, 24, 25, 26, 27, 99", lookup.ImmigrantJobCodeList.CodesAsString);
			});
		}

		public void TestImmigrantEntryStatusList()
		{
			var person = Factory.New<CusPerson>();
			var lookup = new CusPersonLookups(person);
			CombineAssertions(() =>
			{
				AssertEquals(2, lookup.ImmigrantEntryStatusList.Count);
				AssertEquals("N, Y", lookup.ImmigrantEntryStatusList.CodesAsString);
			});
		}
	}
}
