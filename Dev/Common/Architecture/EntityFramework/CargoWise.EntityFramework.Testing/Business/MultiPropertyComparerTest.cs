using System.Collections;
using System.ComponentModel;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class MultiPropertyComparerTest : TestCaseWithFactory
	{
		#region Equals / GetHashCode

		public void TestEquals()
		{
			MultiPropertyComparer comparer1 = new MultiPropertyComparer(GetPropertyComparer, new ListSortDescriptionCollection(new ListSortDescription[] { SortDescription1 }));
			MultiPropertyComparer comparer2 = new MultiPropertyComparer(GetPropertyComparer, new ListSortDescriptionCollection(new ListSortDescription[] { SortDescription1, SortDescription2 }));
			MultiPropertyComparer comparer3 = new MultiPropertyComparer(GetPropertyComparer, new ListSortDescriptionCollection(new ListSortDescription[] { SortDescription1, SortDescription2 }));
			MultiPropertyComparer comparer4 = new MultiPropertyComparer(GetPropertyComparer, new ListSortDescriptionCollection(new ListSortDescription[] { SortDescription2, SortDescription1 }));
			AssertEquals(false, comparer1.Equals(comparer2));
			AssertEquals(true, comparer2.Equals(comparer3));
			AssertEquals(false, comparer3.Equals(comparer4));
			AssertEquals(true, comparer4.Equals(comparer4));
		}

		#endregion

		#region IComparer Members

		public void TestCompare()
		{
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy3 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy4 = Factory.New<DummyBusinessObject>();

			dummy1.Z0_Code = "1";
			dummy2.Z0_Code = "1";
			dummy3.Z0_Code = "2";
			dummy4.Z0_Code = "2";
			dummy1.Z0_Description = "2";
			dummy2.Z0_Description = "1";
			dummy3.Z0_Description = "2";
			dummy4.Z0_Description = "1";

			ArrayList list = new ArrayList();
			list.Add(dummy4);
			list.Add(dummy2);
			list.Add(dummy1);
			list.Add(dummy3);

			MultiPropertyComparer comparer = new MultiPropertyComparer(GetPropertyComparer, new ListSortDescriptionCollection(new ListSortDescription[] { SortDescription1, SortDescription2 }));
			list.Sort(comparer);
			AssertEquals(dummy1, list[0]);
			AssertEquals(dummy2, list[1]);
			AssertEquals(dummy3, list[2]);
			AssertEquals(dummy4, list[3]);
		}

		#endregion

		#region Implementation

		static IComparer GetPropertyComparer(PropertyDescriptor property, ListSortDirection direction)
		{
			return new PropertyComparer(property, direction);
		}

		ListSortDescription SortDescription1
		{
			get { return sortDescription1 ?? (sortDescription1 = new ListSortDescription(CodeProperty, ListSortDirection.Ascending)); }
		}
		ListSortDescription sortDescription1;

		ListSortDescription SortDescription2
		{
			get { return sortDescription2 ?? (sortDescription2 = new ListSortDescription(DescriptionProperty, ListSortDirection.Descending)); }
		}
		ListSortDescription sortDescription2;

		PropertyDescriptor CodeProperty
		{
			get { return codeProperty ?? (codeProperty = ZCustomTypeDescriptor.GetProperties(typeof(DummyBusinessObject))[DummyBizoSchema.Z0_Code.Name]); }
		}
		PropertyDescriptor codeProperty;

		PropertyDescriptor DescriptionProperty
		{
			get { return descriptionProperty ?? (descriptionProperty = ZCustomTypeDescriptor.GetProperties(typeof(DummyBusinessObject))[DummyBizoSchema.Z0_Description.Name]); }
		}
		PropertyDescriptor descriptionProperty;

		#endregion
	}
}
