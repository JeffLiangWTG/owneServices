using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class AQISSingleValueCollectionTest<T> : NonPersistentBusinessObjectCollectionTestCase<T> where T : AQISSingleValueCollection
	{
		public void TestLoadingAQISElements()
		{
			AQISSingleValueCollection collection = CollectionToTestWith;
			collection.SplitAndAddAQISElements(AddInfoProperty.Value.ToString());
			string[] splitValues = AddInfoProperty.Value.ToString().Split(',');

			AssertEquals("Collection count", splitValues.Length, collection.Count);

			for (int i = 0; i < splitValues.Length; i++)
			{
				AssertEquals("Values should be the same", true, splitValues[i] == collection[i].Code);
			}
		}

		public void TestLoadingAQISElementWithALongCode()
		{
			AQISSingleValueCollection collection = CollectionToTestWith;
			ZString valueToAssign = new ZString("1").PadLeft(AddInfoProperty.MaxLength + 1, '0');
			collection.SplitAndAddAQISElements(valueToAssign);
			AssertEquals("No elements in Collection", 0, collection.Count);
		}

		public void TestGetNewAQISElementsString()
		{
			AQISSingleValueCollection collection = CollectionToTestWith;
			collection.RemoveAndDeleteAll();

			collection.Add(FirstBizObjToAdd);
			AddInfoProperty.Value = collection.ReBuildAQISElements();
			AssertEquals("GetNewAQISConcernTypeString", FirstBizObjToAdd.Code, AddInfoProperty.Value);

			collection.Add(SecondBizObjToAdd);
			AddInfoProperty.Value = collection.ReBuildAQISElements();
			AssertEquals("GetNewAQISConcernTypeString", true, AddInfoProperty.Value.ToString().IndexOf(FirstBizObjToAdd.Code) > -1);
			AssertEquals("GetNewAQISConcernTypeString", true, AddInfoProperty.Value.ToString().IndexOf(SecondBizObjToAdd.Code) > -1);
			AssertEquals("GetNewAQISConcernTypeString", false, AddInfoProperty.Value.ToString().EndsWith(","));
		}

		public void TestSortByUniqueCode()
		{
			AQISSingleValueCollection collection = CollectionToTestWith;
			collection.RemoveAndDeleteAll();

			IAQISUniqueCodeForSort firstItem = collection.AddNew();
			foreach (ZPropertyInfo info in firstItem.CodeInfosToSortByForTestingOnly)
			{
				info.Value = new ZString("ZZZ");
			}

			IAQISUniqueCodeForSort secondItem = collection.AddNew();
			foreach (ZPropertyInfo info in firstItem.CodeInfosToSortByForTestingOnly)
			{
				info.Value = new ZString("AAA");
			}

			collection.SortByUniqueCode();
			AssertEquals("First item after sorted", secondItem, ((System.Collections.IList)collection)[0]);
			AssertEquals("Second item after sorted", firstItem, ((System.Collections.IList)collection)[1]);
		}

		public abstract AQISSingleValueCollection CollectionToTestWith { get; }

		public abstract AQISSingleValueBusinessObject FirstBizObjToAdd { get; }

		public abstract AQISSingleValueBusinessObject SecondBizObjToAdd { get; }

		public abstract ZPropertyInfo AddInfoProperty { get; }
	}
}
