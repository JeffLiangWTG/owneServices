using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.Business.Utilities.Testing
{
	public class WebCollectionSorterTest : TestCaseWithFactory
	{
		public void TestEquals()
		{
			WebCollectionSorter comparer1 = new WebCollectionSorter("comp1", ListSortDirection.Ascending);
			WebCollectionSorter comparer2 = new WebCollectionSorter("comp2", ListSortDirection.Ascending);
			WebCollectionSorter comparer3 = new WebCollectionSorter("comp1", ListSortDirection.Ascending);
			AssertEquals("When equal", true, comparer1.Equals(comparer1));
			AssertEquals("When different objects are equal", true, comparer1.Equals(comparer3));
			AssertEquals("When not equal", false, comparer2.Equals(comparer3));
			AssertEquals("When not equal to non-comparer type", false, comparer2.Equals("IncompatibleType"));
		}

		public void TestSorting()
		{
			DummyBusinessObjectWithRelatedDummyCollection collection = new DummyBusinessObjectWithRelatedDummyCollection(Factory);
			PopulateCollection(collection);
			AssertEquals("Collection should contain 5 bizobjects", 5, collection.Count);

			collection.Sort(GetCollectionSorter(DummyBizoSchema.Z0_Description.Name, ListSortDirection.Ascending));

			AssertEquals("Collection not sorted by Z0_Description Ascending", "Australia", collection[0].Z0_Description);
			AssertEquals("Collection not sorted by Z0_Description Ascending", "New Zealand", collection[1].Z0_Description);
			AssertEquals("Collection not sorted by Z0_Description Ascending", "Qatar", collection[2].Z0_Description);
			AssertEquals("Collection not sorted by Z0_Description Ascending", "Spain", collection[3].Z0_Description);
			AssertEquals("Collection not sorted by Z0_Description Ascending", "Sudan", collection[4].Z0_Description);

			collection.Sort(new WebCollectionSorter(DummyBizoSchema.Z0_Description.Name, ListSortDirection.Descending));

			AssertEquals("Collection not sorted by Z0_Description Descending", "Sudan", collection[0].Z0_Description);
			AssertEquals("Collection not sorted by Z0_Description Descending", "Spain", collection[1].Z0_Description);
			AssertEquals("Collection not sorted by Z0_Description Descending", "Qatar", collection[2].Z0_Description);
			AssertEquals("Collection not sorted by Z0_Description Descending", "New Zealand", collection[3].Z0_Description);
			AssertEquals("Collection not sorted by Z0_Description Descending", "Australia", collection[4].Z0_Description);

			collection.Sort(new WebCollectionSorter("RelatedDummy.Z0_VarCharMax", ListSortDirection.Ascending));

			AssertEquals("Collection not sorted by RelatedDummy.Z0_VarCharMax Ascending", "Canberra", collection[0].RelatedDummy.Z0_VarCharMax);
			AssertEquals("Collection not sorted by RelatedDummy.Z0_VarCharMax Ascending", "Doha", collection[1].RelatedDummy.Z0_VarCharMax);
			AssertEquals("Collection not sorted by RelatedDummy.Z0_VarCharMax Ascending", "Khartoum", collection[2].RelatedDummy.Z0_VarCharMax);
			AssertEquals("Collection not sorted by RelatedDummy.Z0_VarCharMax Ascending", "Madrid", collection[3].RelatedDummy.Z0_VarCharMax);
			AssertEquals("Collection not sorted by RelatedDummy.Z0_VarCharMax Ascending", "Wellington", collection[4].RelatedDummy.Z0_VarCharMax);

			collection.Sort(new WebCollectionSorter("RelatedDummy.Z0_VarCharMax", ListSortDirection.Descending));

			AssertEquals("Collection not sorted by RelatedDummy.Z0_VarCharMax Descending", "Wellington", collection[0].RelatedDummy.Z0_VarCharMax);
			AssertEquals("Collection not sorted by RelatedDummy.Z0_VarCharMax Descending", "Madrid", collection[1].RelatedDummy.Z0_VarCharMax);
			AssertEquals("Collection not sorted by RelatedDummy.Z0_VarCharMax Descending", "Khartoum", collection[2].RelatedDummy.Z0_VarCharMax);
			AssertEquals("Collection not sorted by RelatedDummy.Z0_VarCharMax Descending", "Doha", collection[3].RelatedDummy.Z0_VarCharMax);
			AssertEquals("Collection not sorted by RelatedDummy.Z0_VarCharMax Descending", "Canberra", collection[4].RelatedDummy.Z0_VarCharMax);
		}

		public void TestSortingWithNullValues()
		{
			DummyBusinessObjectWithNullPropertyCollection collection = new DummyBusinessObjectWithNullPropertyCollection(Factory);
			PopulateCollection(collection);
			AssertEquals("Collection should contain 4 bizobjects", 4, collection.Count);

			AssertNotNull("First element's property should be not null", collection[0].Z0_NullWhenZ0_VarCharMaxIsEmpty);
			AssertNotNull("Second element's property should be not null", collection[1].Z0_NullWhenZ0_VarCharMaxIsEmpty);
			AssertNull("Third element's property should be null", collection[2].Z0_NullWhenZ0_VarCharMaxIsEmpty);
			AssertNull("Fourth element's property should be null", collection[3].Z0_NullWhenZ0_VarCharMaxIsEmpty);

			collection.Sort(GetCollectionSorter("Z0_NullWhenZ0_VarCharMaxIsEmpty", ListSortDirection.Ascending));

			AssertNull("First element's property should be null", collection[0].Z0_NullWhenZ0_VarCharMaxIsEmpty);
			AssertNull("Second element's property should be null", collection[1].Z0_NullWhenZ0_VarCharMaxIsEmpty);
			AssertEquals("Third element's property should be Moscow", "Moscow", collection[2].Z0_NullWhenZ0_VarCharMaxIsEmpty);
			AssertEquals("Fourth element's property should be Sydney", "Sydney", collection[3].Z0_NullWhenZ0_VarCharMaxIsEmpty);

			collection.Sort(GetCollectionSorter("Z0_NullWhenZ0_VarCharMaxIsEmpty", ListSortDirection.Descending));

			AssertEquals("First element's property should be Sydney", "Sydney", collection[0].Z0_NullWhenZ0_VarCharMaxIsEmpty);
			AssertEquals("Second element's property should be Moscow", "Moscow", collection[1].Z0_NullWhenZ0_VarCharMaxIsEmpty);
			AssertNull("Third element's property should be null", collection[2].Z0_NullWhenZ0_VarCharMaxIsEmpty);
			AssertNull("Fourth element's property should be null", collection[3].Z0_NullWhenZ0_VarCharMaxIsEmpty);
		}

		#region Implementation

		void PopulateCollection(DummyBusinessObjectWithRelatedDummyCollection collection)
		{
			DummyBusinessObjectWithRelatedDummy dummy1 = collection.AddNew();
			dummy1.Z0_Description = "New Zealand";
			DummyBusinessObject relatedDummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Guid = relatedDummy1.PK;
			relatedDummy1.Z0_VarCharMax = "Wellington";

			DummyBusinessObjectWithRelatedDummy dummy2 = collection.AddNew();
			dummy2.Z0_Description = "Australia";
			DummyBusinessObject relatedDummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Guid = relatedDummy2.PK;
			relatedDummy2.Z0_VarCharMax = "Canberra";

			DummyBusinessObjectWithRelatedDummy dummy3 = collection.AddNew();
			dummy3.Z0_Description = "Spain";
			DummyBusinessObject relatedDummy3 = Factory.New<DummyBusinessObject>();
			dummy3.Z0_Guid = relatedDummy3.PK;
			relatedDummy3.Z0_VarCharMax = "Madrid";

			DummyBusinessObjectWithRelatedDummy dummy4 = collection.AddNew();
			dummy4.Z0_Description = "Sudan";
			DummyBusinessObject relatedDummy4 = Factory.New<DummyBusinessObject>();
			dummy4.Z0_Guid = relatedDummy4.PK;
			relatedDummy4.Z0_VarCharMax = "Khartoum";

			DummyBusinessObjectWithRelatedDummy dummy5 = collection.AddNew();
			dummy5.Z0_Description = "Qatar";
			DummyBusinessObject relatedDummy5 = Factory.New<DummyBusinessObject>();
			dummy5.Z0_Guid = relatedDummy5.PK;
			relatedDummy5.Z0_VarCharMax = "Doha";
		}

		void PopulateCollection(DummyBusinessObjectWithNullPropertyCollection collection)
		{
			DummyBusinessObjectWithNullProperty dummy1 = collection.AddNew();
			dummy1.Z0_VarCharMax = "Moscow";

			DummyBusinessObjectWithNullProperty dummy2 = collection.AddNew();
			dummy2.Z0_VarCharMax = "Sydney";

			DummyBusinessObjectWithNullProperty dummyNull1 = collection.AddNew();
			DummyBusinessObjectWithNullProperty dummyNull2 = collection.AddNew();
		}

		protected virtual WebCollectionSorter GetCollectionSorter(string sortExpression, ListSortDirection direction)
		{
			return new WebCollectionSorter(sortExpression, direction);
		}

		#region Dummy Classes and Collections

		public class DummyBusinessObjectWithRelatedDummy : DummyBusinessObject
		{
			public DummyBusinessObjectWithRelatedDummy(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override DummyBusinessObject RelatedDummy
			{
				get { return Factory.Load<DummyBusinessObject>(Z0_Guid); }
			}
		}

		public class DummyBusinessObjectWithRelatedDummyCollection : BusinessObjectCollection<DummyBusinessObjectWithRelatedDummy>
		{
			public DummyBusinessObjectWithRelatedDummyCollection(BusinessObjectFactory factory) : base(factory)
			{
			}
		}

		public class DummyBusinessObjectWithNullProperty : DummyBusinessObject
		{
			public DummyBusinessObjectWithNullProperty(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public string Z0_NullWhenZ0_VarCharMaxIsEmpty
			{
				get { return Z0_VarCharMax.IsEmpty ? null : Z0_VarCharMax.ToString(); }
			}
		}

		public class DummyBusinessObjectWithNullPropertyCollection : BusinessObjectCollection<DummyBusinessObjectWithNullProperty>
		{
			public DummyBusinessObjectWithNullPropertyCollection(BusinessObjectFactory factory) : base(factory)
			{
			}
		}

		#endregion

		#endregion
	}
}
